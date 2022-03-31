using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Etl.Core.Events;
using Etl.Core.Transformation;
using Etl.Core.Load;
using Etl.Core.Scanner;
using System.IO;
using Etl.Core.Settings;
using System.Linq;

namespace Etl.Core
{
    public class Workflow
    {
        private readonly int _maxExtractorThread;
        private readonly int _maxBatchBuffer;
        private readonly IEtlContext _context;
        private readonly IEtlFactory _etlDefFactory;
        private readonly ILoaderFactory _loaderFactory;

        private ICompilerEvent _events;

        private int _scanBatch;
        private int _flushBatch;
        private List<(ILoader loader, LoaderDef args)> _loaders = new();
        private Etl _etl;
        private SequenceFlushBuffer _sequenceFlushBuffer;

        private int _totalRecords;
        private int _totalValidRecords;
        private int _totalErrors;
        private TransformResult _transformResult;
        private DateTime _start = DateTime.Now;

        public Workflow(EtlSetting setting, IEtlContext eltContext, IEtlFactory etlDefFactory, ILoaderFactory loaderFactory)
        {
            _maxExtractorThread = setting?.Extraction?.MaxThread ?? 2;
            _maxBatchBuffer = setting?.Extraction?.MaxBatchBuffer ?? 100;
            _context = eltContext;
            _etlDefFactory = etlDefFactory;
            _loaderFactory = loaderFactory;
        }

        public Workflow Subcribe(Action<CompilerEvent> subscribe)
        {
            var e = new CompilerEvent();
            subscribe(e);

            _events = e;
            return this;
        }

        public Workflow SetConfig(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
                return this;

            var (definition, executor) = _etlDefFactory.DirectlyLoad(configFilePath);
            return SetConfig(definition, executor);
        }

        public Workflow SetConfig(EtlDef definition, Etl executor = null)
        {
            if (definition != null)
            {
                _scanBatch = definition.ScanBatch;
                _flushBatch = definition.FlushBatch;
                _loaders = _loaderFactory.Get(definition.Loaders) ?? new();
                _transformResult = new TransformResult(definition.FlushBatch);
                _etl = executor ?? new Etl(definition);
            }

            return this;
        }

        public Workflow AddLoaders(params LoaderDef[] args)
        {
            var items = _loaderFactory.Get(args);
            if (items != null)
                _loaders.AddRange(items);

            return this;
        }

        public Workflow AddLoaders(params ILoader[] loaders)
        {
            if (loaders != null)
                _loaders.AddRange(loaders.Select(e => (e, (LoaderDef)default)));
            return this;
        }

        public Workflow AddLoaders(params (ILoader loader, LoaderDef args)[] loaders)
        {
            if (loaders != null)
                _loaders.AddRange(loaders);
            return this;
        }

        public void Start(string dataFilePath, int? take = null, int? skip = null)
        {
            if (!File.Exists(dataFilePath))
                throw new Exception($"Not existed data file '{dataFilePath}'.");

            if (_etl == null)
            {
                var (definition, executor) = _etlDefFactory.Load(dataFilePath);
                SetConfig(definition, executor);
            }

            bool isFirst = true;
            _sequenceFlushBuffer = new SequenceFlushBuffer((result, isLast) =>
            {
                OnTransformed(result, isFirst, isLast, dataFilePath);
                isFirst = false;
            });

            _start = DateTime.Now;
            List<List<TextLine>> scannedBatch = new();
            List<Task> extractTasks = new(_maxExtractorThread);

            using (var scanner = _etl.CreateScanner(
                () => new StreamReader(dataFilePath),
                textLines => scannedBatch = OnScanned(textLines, scannedBatch, extractTasks)))
                scanner.Start(take, skip);

            Task.WaitAll(extractTasks.ToArray());

            OnExtractAndTransform(scannedBatch, true);

            _sequenceFlushBuffer.WaitFlush();
            _loaders.ForEach(e => e.loader.WaitToComplete());
        }

        private List<List<TextLine>> OnScanned(List<TextLine> textLines, List<List<TextLine>> batch, List<Task> extractionTasks)
        {
            if (textLines != null)
            {
                batch.Add(textLines);
                if (batch.Count >= _scanBatch)
                {
                    var temp = batch;
                    batch = new List<List<TextLine>>();

                    PushExtractorPool(extractionTasks, () => OnExtractAndTransform(temp, false));
                }
            }

            return batch;
        }

        private void OnExtractAndTransform(List<List<TextLine>> scannedBatch, bool isLast)
        {
            var batch = new TransformResult();

            foreach (var textLines in scannedBatch)
            {
                try
                {
                    _events?.OnScanned?.Invoke(textLines);

                    var record = _etl.Extract(textLines, _events);
                    _events?.OnExtracted?.Invoke(record);

                    var result = _etl.Transform(record, _context);
                    _events?.OnTransformed?.Invoke(result);

                    batch.Append(result);
                }
                catch (Exception ex)
                {
                    _events?.OnError?.Invoke($"{textLines}\n", ex);
                }
            }

            TransformResult temp = null;
            lock (this)
            {
                _transformResult.Append(batch);
                if (isLast || _transformResult.Items.Count >= _flushBatch)
                {
                    temp = _transformResult;
                    _transformResult = new TransformResult(_flushBatch);
                }
            }

            if (temp != null)
                _sequenceFlushBuffer.Push(temp, isLast);
        }

        private void OnTransformed(TransformResult result, bool isFirst, bool isLast, string dataFile)
        {
            if (isFirst)
            {
                isFirst = false;
                _loaders.ForEach(e => e.loader.Initalize(e.args, dataFile, _etl.AllFields));
            }

            var batch = _etl.ApplyMassage(result.Items);
            var batchResult = new BatchResult
            {
                Batch = batch,
                Errors = result.Errors,
                TotalValidRecords = _totalValidRecords += batch.Count,
                TotalErrors = _totalErrors += result.TotalErrors,
                TotalRecords = _totalRecords += result.TotalRecords,
                StartAt = _start,
                IsLast = isLast
            };

            _loaders.ForEach(e => e.loader.ProcessBatch(batchResult));
            _events?.OnTransformedBatch?.Invoke(batchResult);
        }

        private void PushExtractorPool(List<Task> extractTasks, Action action)
        {
            if (_maxBatchBuffer > 0)
            {
                int count = 0;
                while (_sequenceFlushBuffer.Count > _maxBatchBuffer)
                {
                    if ((count % 1000) == 0)
                    {
                        count = 0;
                        Console.WriteLine("Buffer is full, Loaders are processing...");
                    }
                    count += 200;
                    Thread.Sleep(200);
                }
            }

            Task t = null;
            lock (extractTasks)
            {
                if (extractTasks.Count < _maxExtractorThread)
                {
                    extractTasks.Add(t = new(() =>
                    {
                        try { action(); }
                        finally { lock (extractTasks) { extractTasks.Remove(t); } }
                    }));
                }
            }

            if (t != null)
                t.Start();
            else
                action();
        }
    }
}

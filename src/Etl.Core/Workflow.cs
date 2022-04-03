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
using Etl.Core.Extraction;

namespace Etl.Core
{
    public class Workflow
    {
        private readonly string _dataFilePath;
        private readonly int _maxExtractorThread;
        private readonly int _maxBatchBuffer;
        private readonly int _scanBatch;
        private readonly int _flushBatch;
        private readonly ICompilerEvent _events;
        private readonly IServiceProvider _sp;
        private readonly EtlInst _etl;
        private readonly List<Loader> _extraLoaders;

        private int _totalRecords;
        private int _totalValidRecords;
        private int _totalErrors;
        private TransformResult _transformResult;
        private DateTime _start = DateTime.Now;
        private SequenceFlushBuffer _sequenceFlushBuffer;
        private Func<ExtractedRecord, TransformResult> _transformInstance;
        private List<ILoaderInst> _loaderInstances;

        public Workflow(string dataFilePath, EtlSetting etlSetting, Etl etl, EtlInst etlInst, List<Loader> extraLoaders,  ICompilerEvent events, IServiceProvider sp)
        {
            _dataFilePath = dataFilePath;
            _maxExtractorThread = etlSetting?.Extraction?.MaxThread ?? 2;
            _maxBatchBuffer = etlSetting?.Extraction?.MaxBatchBuffer ?? 100;
            _scanBatch = etl.ScanBatch;
            _flushBatch = etl.FlushBatch;
            _etl = etlInst;
            _extraLoaders = extraLoaders;
            _events = events;
            _sp = sp;
            _transformResult = new TransformResult(_flushBatch);
        }

        public void Start(int? take = null, int? skip = null)
        {
            _start = DateTime.Now;
            List<List<TextLine>> scannedBatch = new();
            List<Task> extractTasks = new(_maxExtractorThread);

            var (scanner, transformInstance, loaderInstances) = _etl.Start(
                _dataFilePath,
                () => new StreamReader(_dataFilePath),
                _sp,
                _extraLoaders,
                textLines => scannedBatch = OnScanned(textLines, scannedBatch, extractTasks));

            _transformInstance = transformInstance;
            _loaderInstances = loaderInstances;
            _sequenceFlushBuffer = new SequenceFlushBuffer(OnTransformed);

            scanner.Start(take, skip);
            scanner.Dispose();

            Task.WaitAll(extractTasks.ToArray());

            OnExtractAndTransform(scannedBatch, true);

            _sequenceFlushBuffer.WaitFlush();
            _loaderInstances.ForEach(e => e.WaitToComplete());
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

                    var result = _transformInstance(record);
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

        private void OnTransformed(TransformResult result, bool isLast)
        {
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

            _loaderInstances.ForEach(e => e.ProcessBatch(batchResult));
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

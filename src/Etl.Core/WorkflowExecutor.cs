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
    public class WorkflowExecutor
    {
        private readonly int _maxExtractorThread;
        private readonly int _maxBatchBuffer;
        private readonly int _scanBatch;
        private readonly int _flushBatch;
        private readonly ICompilerEvent _events;
        private readonly List<(LoaderDef definition, ILoader instance)> _loaders = new();
        private readonly Etl _etl;

        private int _totalRecords;
        private int _totalValidRecords;
        private int _totalErrors;
        private TransformResult _transformResult;
        private DateTime _start = DateTime.Now;
        private SequenceFlushBuffer _sequenceFlushBuffer;
        private Func<ExtractedRecord, TransformResult> _transformInstance;

        public WorkflowExecutor(EtlSetting etlSetting, EtlDef etlDef, Etl etl,
            List<(LoaderDef definition, ILoader instance)> loaders,
            ICompilerEvent events)
        {
            _maxExtractorThread = etlSetting?.Extraction?.MaxThread ?? 2;
            _maxBatchBuffer = etlSetting?.Extraction?.MaxBatchBuffer ?? 100;
            _scanBatch = etlDef.ScanBatch;
            _flushBatch = etlDef.FlushBatch;
            _etl = etl;
            _loaders = loaders;
            _events = events;
            _transformResult = new TransformResult(_flushBatch);
        }

        public void Start(string dataFilePath, IServiceProvider sp, int? take = null, int? skip = null)
        {
            bool isFirst = true;
            _sequenceFlushBuffer = new SequenceFlushBuffer((result, isLast) =>
            {
                OnTransformed(result, isFirst, isLast, dataFilePath);
                isFirst = false;
            });

            _start = DateTime.Now;
            List<List<TextLine>> scannedBatch = new();
            List<Task> extractTasks = new(_maxExtractorThread);

            var (scanner, transformInstance) = _etl.Start(
                () => new StreamReader(dataFilePath),
                sp,
                textLines => scannedBatch = OnScanned(textLines, scannedBatch, extractTasks));

            _transformInstance = transformInstance;
            scanner.Start();
            scanner.Dispose();

            Task.WaitAll(extractTasks.ToArray());

            OnExtractAndTransform(scannedBatch, true);

            _sequenceFlushBuffer.WaitFlush();
            _loaders.ForEach(e => e.instance.WaitToComplete());
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

        private void OnTransformed(TransformResult result, bool isFirst, bool isLast, string dataFile)
        {
            if (isFirst)
            {
                isFirst = false;
                _loaders.ForEach(e => e.instance.Initalize(e.definition, dataFile, _etl.AllFields));
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

            _loaders.ForEach(e => e.instance.ProcessBatch(batchResult));
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

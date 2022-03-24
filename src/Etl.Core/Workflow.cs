using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Etl.Core.Events;
using Etl.Core.Transformation;
using Etl.Core.Load;
using Etl.Core.Scanner;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Etl.Core
{
    public class Workflow
    {
        private readonly int _maxCompilerThread;
        private readonly int _maxBatchBuffer;
        private readonly Executor _compiler;
        private readonly Func<StreamReader> _getStreamReader;
        private readonly SequenceFlushBuffer _sequenceFlushBuffer;
        private readonly int _scanBatch;
        private readonly int _flushBatch;
        private readonly List<Loader> _loaders = new();

        private ICompilerEvent _events;
        private List<IDictionary<string, object>> _flushBuffer = new();

        private int _totalScannedRecords;
        private int _totalRecords;
        private int _totalValidRecords;
        private int _totalErrors;
        private DateTime _start = DateTime.Now;

        public Workflow(IConfiguration appSetting, EtlDef etlDef, string dataFile, Func<StreamReader> getStreamReader = null)
        {
            _maxCompilerThread = appSetting.GetSection("Compiler:MaxThread")?.Get<int>() ?? 2;
            _maxBatchBuffer = appSetting.GetSection("Compiler:MaxBatchBuffer")?.Get<int>() ?? 100;
            _getStreamReader = getStreamReader ?? (() => new StreamReader(dataFile));
            _scanBatch = etlDef.ScanBatch;
            _flushBatch = etlDef.FlushBatch;
            _flushBuffer = new List<IDictionary<string, object>>(etlDef.FlushBatch);
            _compiler = etlDef.GetCompiler();
            _loaders = etlDef.Loaders ?? new();

            bool isFirst = true;
            _sequenceFlushBuffer = new SequenceFlushBuffer(result =>
            {
                _compiler.ApplyMassage(result);
                result.TotalValidRecords = _totalValidRecords += result.Batch.Count;

                if (isFirst)
                {
                    _loaders.ForEach(e => e.Initialize(appSetting, dataFile, _compiler.AllFields));
                    isFirst = false;
                }

                _loaders.ForEach(e => e.ProcessBatch(result));
                _events?.OnTransformedBatch?.Invoke(result);
            });
        }

        public Workflow Subcribe(Action<CompilerEvent> subscribe)
        {
            var e = new CompilerEvent();
            subscribe(e);

            _events = e;
            return this;
        }

        public Workflow AddLoaders(params Loader[] loaders)
        {
            _loaders.AddRange(loaders);
            return this;
        }

        public void Start(Context context = null, int? take = null, int? skip = null)
        {
            _start = DateTime.Now;
            List<List<TextLine>> scannedBatch = new();
            List<Task> compilerTasks = new(_maxCompilerThread);

            using (var scanner = _compiler.CreateScanner(_getStreamReader,
                textLines =>
                    {
                        if (textLines == null)
                            return;

                        scannedBatch.Add(textLines);
                        if (scannedBatch.Count >= _scanBatch)
                        {
                            var batch = scannedBatch;
                            scannedBatch = new List<List<TextLine>>();

                            PushCompilerPool(compilerTasks, () => Compile(context, batch, false));
                        }
                    }))
                scanner.Start(take, skip);

            Task.WaitAll(compilerTasks.ToArray());

            Compile(context, scannedBatch, true);

            _sequenceFlushBuffer.WaitFlush();
            _loaders.ForEach(e => e.WaitToComplete());
        }

        private void Compile(Context context, List<List<TextLine>> scannedBatch, bool isLast)
        {
            var records = new List<IDictionary<string, object>>();

            foreach (var textLines in scannedBatch)
            {
                Interlocked.Increment(ref _totalScannedRecords);
                var recordOrder = _totalScannedRecords;
                try
                {
                    _compiler.RemoveComments(textLines);

                    _events?.OnStart?.Invoke(recordOrder);
                    _events?.OnScanned?.Invoke(textLines);

                    var record = _compiler.Extract(textLines, _events);
                    _events?.OnExtracted?.Invoke(record);

                    var result = _compiler.Transform(record, context);
                    _events?.OnTransformed?.Invoke(result);

                    if (result is List<IDictionary<string, object>> many)
                        records.AddRange(many);
                    else if (result is IDictionary<string, object> one)
                        records.Add(one);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _totalErrors);
                    _events?.OnError?.Invoke($"Scanned Record: {recordOrder}\n", ex);
                }
            }

            List<IDictionary<string, object>> temp = null;
            lock (this)
            {
                _flushBuffer.AddRange(records);
                if (isLast || _flushBuffer.Count >= _flushBatch)
                {
                    temp = _flushBuffer;
                    _totalRecords += temp.Count;
                    _flushBuffer = new List<IDictionary<string, object>>(_flushBatch);
                }
            }

            if (temp != null)
            {
                _sequenceFlushBuffer.Push(new BatchResult
                {
                    Batch = temp,
                    StartAt = _start,
                    TotalScannedRecords = _totalScannedRecords,
                    TotalRecords = _totalRecords,
                    TotalErrors = _totalErrors,
                    IsLast = isLast
                });
            }
        }

        private void PushCompilerPool(List<Task> parserTasks, Action action)
        {
            if (_maxBatchBuffer > 0)
            {
                int count = 0;
                while (_sequenceFlushBuffer.Count > _maxBatchBuffer)
                {
                    if ((count % 1000) == 0)
                    {
                        count = 0;
                        Console.WriteLine("Sleep...");
                    }
                    count += 200;
                    Thread.Sleep(200);
                }
            }

            Task t = null;
            lock (parserTasks)
            {
                if (parserTasks.Count < _maxCompilerThread)
                {
                    parserTasks.Add(t = new(() =>
                    {
                        try { action(); }
                        finally { lock (parserTasks) { parserTasks.Remove(t); } }
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

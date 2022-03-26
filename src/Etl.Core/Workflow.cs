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
        private TransformResult _flushBuffer;

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
            _flushBuffer = new TransformResult(etlDef.FlushBatch);
            _compiler = etlDef.GetCompiler();
            _loaders = etlDef.Loaders ?? new();

            bool isFirst = true;
            _sequenceFlushBuffer = new SequenceFlushBuffer((result, isLast) =>
            {
                if (isFirst)
                {
                    isFirst = false;
                    _loaders.ForEach(e => e.Initialize(appSetting, dataFile, _compiler.AllFields));
                }

                var batch = _compiler.ApplyMassage(result.Items);
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

                _loaders.ForEach(e => e.ProcessBatch(batchResult));
                _events?.OnTransformedBatch?.Invoke(batchResult);
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
            var batch = new TransformResult();

            foreach (var textLines in scannedBatch)
            {
                try
                {
                    _compiler.RemoveComments(textLines);

                    _events?.OnScanned?.Invoke(textLines);

                    var record = _compiler.Extract(textLines, _events);
                    _events?.OnExtracted?.Invoke(record);

                    var result = _compiler.Transform(record, context);
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
                _flushBuffer.Append(batch);
                if (isLast || _flushBuffer.Items.Count >= _flushBatch)
                {
                    temp = _flushBuffer;
                    _flushBuffer = new TransformResult(_flushBatch);
                }
            }

            if (temp != null)
                _sequenceFlushBuffer.Push(temp, isLast);
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

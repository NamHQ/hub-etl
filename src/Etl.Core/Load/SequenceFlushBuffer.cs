using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etl.Core.Load
{
    public class SequenceFlushBuffer
    {
        private readonly Queue<BatchResult> _buffer = new();
        private readonly Action<BatchResult> _flushHandler;
        private bool _isProcessing = false;
        private Task _executor;

        public int Count
            => _buffer.Count;

        public SequenceFlushBuffer(Action<BatchResult> flushHandler)
        {
            _flushHandler = flushHandler;
        }

        public void Push(BatchResult parseResult)
        {
            if (!parseResult.IsLast && (parseResult.Batch == null || parseResult.Batch.Count == 0))
                return;

            lock (this)
            {
                _buffer.Enqueue(parseResult);
                if (_isProcessing)
                    return;

                _isProcessing = true;
            }

            _executor = Task.Run(() => Flush());
        }

        private void Flush()
        {
            BatchResult item = default;
            while (true)
            {
                lock (this)
                {
                    if (_buffer.Count == 0)
                    {
                        _isProcessing = false;
                        return;
                    }

                    item = _buffer.Dequeue();
                }

                _flushHandler(item);
            }
        }

        public void WaitFlush()
        {
            _executor?.Wait();
        }
    }
}

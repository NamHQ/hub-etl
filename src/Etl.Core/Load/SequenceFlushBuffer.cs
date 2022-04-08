using Etl.Core.Transformation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etl.Core.Load
{
    public class SequenceFlushBuffer
    {
        private readonly Queue<(TransformResult result, bool isLast)> _buffer = new();
        private readonly Action<TransformResult, bool> _onFlush;
        private readonly Action<TransformResult, (string message, Exception ex)> _onCompleted;
        private bool _isProcessing = false;
        private Task _executor;

        public int Count
            => _buffer.Count;

        public SequenceFlushBuffer(Action<TransformResult, bool> onFlush, Action<TransformResult, (string message, Exception ex)> onCompleted)
        {
            _onFlush = onFlush;
            _onCompleted = onCompleted;
        }

        public void Push(TransformResult result, bool isLast)
        {
            lock (this)
            {
                _buffer.Enqueue((result, isLast));
                if (_isProcessing)
                    return;

                _isProcessing = true;
            }

            _executor = Task.Run(() => Flush());
        }

        private void Flush()
        {
            (TransformResult result, bool isLast) item = default;
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

                try
                {
                    _onFlush(item.result, item.isLast);
                    _onCompleted(item.result, default);
                }
                catch (Exception ex)
                {
                    _onCompleted(item.result, ($"Loader error: {(ex.InnerException ?? ex).Message}", ex));
                }
            }
        }

        public void WaitFlush()
        {
            _executor?.Wait();
        }
    }
}

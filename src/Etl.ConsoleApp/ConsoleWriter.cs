using Etl.Core.Events;
using System;

namespace Etl.ConsoleApp
{
    class ConsoleWriter : ILogWriter
    {
        public void Debug(string message)
           => Console.WriteLine(message);

        public void Error(string message, Exception exception = null)
           => Console.Error.WriteLine(message, exception);
    }

    static class LogWriterHelper
    {
        public static void ConsoleLog(
            this CompilerEvent events,
            bool onScanned = true, 
            bool onExtracting = true, bool onExtracted = true, 
            bool onTransformed = true, bool onTransformedBatch = true,
            bool onError = true)
        {
            var isDebug = onScanned || onExtracting || onExtracted || onTransformed || onTransformedBatch;

            var writer = new ConsoleWriter();
            LoggerHelper.ApplyLog(events, writer, new LogOptions
            {
                OnScanned = isDebug && onScanned,
                OnExtracting = isDebug && onExtracting,
                OnExtracted = isDebug && onExtracted,
                OnTransformed = isDebug && onTransformed,
                OnTransformedBatch = isDebug && onTransformedBatch,
                OnError = onError
            });
        }
    }
}

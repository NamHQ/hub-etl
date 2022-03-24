using Etl.Core.Events;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;

namespace Etl.ConsoleApp
{
    class LogWriter : ILogWriter
    {
        private readonly NLog.Logger _logger;

        public LogWriter(bool isDebug, string filePath, string layout)
        {
            var fileInfo = new FileInfo(filePath);

            _logger = CreateNLog(fileInfo, layout, isDebug);
        }

        public void Debug(string message)
           => _logger.Debug(message);

        public void Error(string message, Exception exception = null)
           => _logger.Error(exception, message);

        private static NLog.Logger CreateNLog(FileInfo fileInfo, string layout = null, bool isDebug = false)
        {
            var factory = new LogFactory();
            var config = new LoggingConfiguration();

            var name = fileInfo.Name;
            var fileName = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length);

            if (isDebug)
            {
                var target = new ColoredConsoleTarget
                {
                    Layout = "${message} ${exception:format=ToString}"
                };
                var rule = new LoggingRule("*", NLog.LogLevel.Trace, target);
                config.AddTarget(name, target);
                config.LoggingRules.Add(rule);
            }
            else
            {
                var target = new FileTarget
                {
                    Name = fileInfo.Name,
                    FileName = $"{Path.GetFileNameWithoutExtension(fileName)}.error",
                    Layout = layout ?? "${message} ${exception:format=ToString}"
                };
                var rule = new LoggingRule("*", NLog.LogLevel.Error, new AsyncTargetWrapper(target));
                config.AddTarget(name, target);
                config.LoggingRules.Add(rule);
            }

            factory.Configuration = config;
            return factory.GetCurrentClassLogger();
        }
    }

    static class LogWriterHelper
    {
        public static void ApplyLog(
            this CompilerEvent events,
            string filePath, string layout = null, bool onStart = true, bool onScanned = true, 
            bool onExtracting = true, bool onExtracted = true, 
            bool onTransformed = true, bool onTransformedBatch = true,
            bool onError = true)
        {
            var isDebug = onStart || onScanned || onExtracting || onExtracted || onTransformed || onTransformedBatch;

            var writer = new LogWriter(isDebug, filePath, layout);
            LoggerHelper.ApplyLog(events, writer, new LogOptions
            {
                OnStart = isDebug && onStart,
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

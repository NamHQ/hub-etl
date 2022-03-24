using System;

namespace Etl.Core.Events
{
    public interface ILogWriter
    {
        void Debug(string message);
        void Error(string message, Exception exception = null);
    }
}

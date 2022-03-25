using Etl.Core.Transformation.Fields;
using System;

namespace Etl.Core.Transformation
{
    public class TransformException : Exception
    {
        public readonly object RawData;

        public TransformException(string message, object data) : base(message)
        {
            RawData = data;
        }
    }
}

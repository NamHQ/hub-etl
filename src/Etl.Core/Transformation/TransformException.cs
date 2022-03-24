using Etl.Core.Transformation.Fields;
using System;

namespace Etl.Core.Transformation
{
    public class TransformException : Exception
    {
        public readonly FieldBase Field;

        public TransformException(FieldBase field, string message) : base(message)
        {
            this.Field = field;
        }
    }
}

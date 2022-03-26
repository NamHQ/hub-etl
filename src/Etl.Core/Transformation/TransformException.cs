using Etl.Core.Transformation.Fields;
using System;

namespace Etl.Core.Transformation
{
    public class TransformException : Exception
    {
        private readonly FieldBase _field;
        public readonly object _rawData;

        public TransformException(FieldBase field, string reason, object data) : base(reason)
        {
            _field = field;
            _rawData = data;
        }

        public override string ToString()
        {
            return $"{_field.GetType().Name}:'{_field.LazyDbField.Value}', Invalid: '{Message}', Data: '{_rawData??"NULL"}'";
        }
    }
}

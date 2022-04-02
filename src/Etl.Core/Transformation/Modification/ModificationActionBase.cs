using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Modification
{
    [XmlInclude(typeof(CSharpAction))]
    [XmlInclude(typeof(SubStringAction))]
    public abstract class ModificationActionBase
    {
        public abstract string Execute(TransformField field, string rawValue, IDictionary<string, object> record);
    }
}

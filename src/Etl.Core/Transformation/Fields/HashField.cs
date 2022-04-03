using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class HashField : PipeLineField
    {
        public override TransformAction DefaultAction => new HashAction();
    }
}

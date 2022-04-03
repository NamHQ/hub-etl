using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class FloatField : PipeLineField
    {
        public override TransformAction DefaultAction => new FloatAction();
    }
}

using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class IntegerField : PipeLineField
    {
        public override TransformAction DefaultAction => new IntegerAction();
    }
}

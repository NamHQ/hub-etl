using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class BooleanField : PipeLineField
    {
        public override TransformAction DefaultAction => new BooleanAction();
    }
}

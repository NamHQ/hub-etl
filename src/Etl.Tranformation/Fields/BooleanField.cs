using Etl.Core.Transformation.Actions;
using Etl.Core.Transformation.Fields;
using Etl.Tranformation.Actions;

namespace Etl.Tranformation.Fields
{
    public class BooleanField : PipeLineField
    {
        public override TransformAction DefaultAction => new BooleanAction();
    }
}

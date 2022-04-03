using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class DateField : PipeLineField
    {
        public override TransformAction DefaultAction => new DateAction();
    }
}

using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class EncryptField : PipeLineField
    {
        public override TransformAction DefaultAction => new EncryptAction();
    }
}


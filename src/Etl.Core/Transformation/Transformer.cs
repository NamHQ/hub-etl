using Etl.Core.Transformation.Fields;
using Etl.Core.Load;
using System.Collections.Generic;

namespace Etl.Core.Transformation
{
    public class Transformer 
    {
        public List<TransformField> Fields { get; set; } = new();

        public MassageDataCSharpCode Massage { get; set; } = new();
    }
}

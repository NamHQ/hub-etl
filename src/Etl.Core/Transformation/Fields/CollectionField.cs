using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class CollectionField : GroupField
    {
        public HashSet<string> IgnoreParserFields = new();
    }
}

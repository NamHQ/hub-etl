using Etl.Core.Transformation.Fields;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    //IMPORTANT: ILoaderArgs is singleton, it is loaded from xml so it must be IMMUTABLE.

    public class LoaderArgs
    {
        public readonly string InputFile;
        public readonly IReadOnlyCollection<TransformField> Fields;

        public LoaderArgs(string inputFile, IReadOnlyCollection<TransformField> fields)
        {
            InputFile = inputFile;
            Fields = fields;
        }
    }
}

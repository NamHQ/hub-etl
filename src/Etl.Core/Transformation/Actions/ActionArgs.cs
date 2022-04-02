using Etl.Core.Extraction;

namespace Etl.Core.Transformation.Actions
{
    public class ActionArgs
    {
        public readonly ExtractedRecord Record;

        public ActionArgs(ExtractedRecord record)
        {
            Record = record;
        }
    }
}

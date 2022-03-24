using System;

namespace Etl.Core.Load
{
    public class ConsoleLoader : Loader
    {
        public override void ProcessBatch(BatchResult result)
        {
            Console.WriteLine(result);
        }
    }
}
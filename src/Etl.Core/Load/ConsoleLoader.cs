using System;

namespace Etl.Core.Load
{
    public class ConsoleLoader : Loader<ConsoleLoaderInst>
    {
    }

    public class ConsoleLoaderInst : LoaderInst<ConsoleLoader>
    {
        public override void ProcessBatch(BatchResult batch)
            => Console.WriteLine(batch);
    }
}
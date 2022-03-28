using System;

namespace Etl.Core.Load
{
    public class ConsoleLoaderArgs : LoaderDef<ConsoleLoader, ConsoleLoaderArgs>
    {
    }

    public class ConsoleLoader : Loader<ConsoleLoader, ConsoleLoaderArgs>
    {
        protected override void ProcessBatch(BatchResult batch)
            => Console.WriteLine(batch);
    }
}
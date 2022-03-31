using System;

namespace Etl.Core.Load
{
    public class ConsoleLoaderDef : LoaderDef<ConsoleLoader, ConsoleLoaderDef>
    {
    }

    public class ConsoleLoader : Loader<ConsoleLoader, ConsoleLoaderDef>
    {
        protected override void ProcessBatch(BatchResult batch)
            => Console.WriteLine(batch);
    }
}
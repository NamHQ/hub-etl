using System;

namespace Etl.Core.Load
{
    public class ConsoleLoader : Loader<ConsoleLoaderInst, ConsoleLoader>
    {
    }

    public class ConsoleLoaderInst : LoaderInst<ConsoleLoaderInst, ConsoleLoader>
    {
        protected override void ProcessBatch(BatchResult batch)
            => Console.WriteLine(batch);
    }
}
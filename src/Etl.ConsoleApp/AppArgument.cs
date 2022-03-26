using Etl.Core;
using Etl.Core.Transformation;
using System.IO;

namespace Etl.ConsoleApp
{
    partial class Program
    {
        public class AppArgument
        {
            public FileInfo DataFile;
            public EtlDef Config;
            public Context Context;
            public int? Take;
            public int? Skip;
            public bool OnScanned;
            public bool OnExtracting;
            public bool OnExtracted;
            public bool OnTransformed;
            public bool OnTransformedBatch;
        }
    }
}

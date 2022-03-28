using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.IO;

namespace Etl.Storage
{
    public abstract class FileLoader<TLoader, TDef> : Loader<TLoader, TDef>
        where TLoader : FileLoader<TLoader, TDef>
        where TDef : FileLoaderDef<TLoader, TDef>
    {
        protected StreamWriter _stream;

        protected override void Initalize(TDef args, string inputFile, IReadOnlyCollection<FieldBase> fields)
        {
            var file = new FileInfo(inputFile);
            var path = args.OutPath.Replace("$path", file.DirectoryName)
                .Replace("$name", file.Name);

            _stream = new StreamWriter(path);
        }

        protected sealed override void ProcessBatch(BatchResult result)
        {
            OnProcessBatch(result);

            if (result.IsLast)
                OnCompleted();
        }

        protected abstract void OnProcessBatch(BatchResult result);


        protected virtual void OnCompleted()
            => _stream?.Dispose();
    }
}

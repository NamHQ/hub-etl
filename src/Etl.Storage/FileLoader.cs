using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public abstract class FileLoader : Loader
    {
        [XmlAttribute]
        public string OutPath { get; set; } = "$path/$name.result";

        protected StreamWriter _stream;

        public override void Initialize(IConfiguration appSetting, string inputFile, IReadOnlyCollection<FieldBase> fields)
        {
            var file = new FileInfo(inputFile);
            var path = OutPath.Replace("$path", file.DirectoryName)
                .Replace("$name", file.Name);

            _stream = new StreamWriter(path);
        }

        public sealed override void ProcessBatch(BatchResult result)
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

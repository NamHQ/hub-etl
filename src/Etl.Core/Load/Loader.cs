using Etl.Core.Transformation.Fields;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    public abstract class Loader
    {
        public virtual void Initialize(IConfiguration appSetting, string inputFile, IReadOnlyCollection<FieldBase> fields) { }

        public abstract void ProcessBatch(BatchResult parseResult);

        public virtual void WaitToComplete() { }
    }
}

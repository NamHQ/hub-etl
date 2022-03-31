using Etl.Core.Transformation.Fields;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    //IMPORTANT: ILoaderArgs is transient, it is created with workflow.

    public interface ILoader
    {
        void Initalize(object args, string inputFile, IReadOnlyCollection<TransformField> fields);
        void ProcessBatch(BatchResult batch);
        void WaitToComplete();
    }

    public abstract class Loader<TLoader, TDef> : ILoader
        where TLoader : Loader<TLoader, TDef>
        where TDef : LoaderDef<TLoader, TDef>
    {
        protected virtual void Initalize(TDef args, string inputFile, IReadOnlyCollection<TransformField> fields) { }
        void ILoader.Initalize(object config, string inputFile, IReadOnlyCollection<TransformField> fields)
            => Initalize(config as TDef, inputFile, fields);

        protected abstract void ProcessBatch(BatchResult parseResult);
        void ILoader.ProcessBatch(BatchResult parseResult)
            => ProcessBatch(parseResult);

        protected virtual void WaitToComplete() { }
        void ILoader.WaitToComplete()
            => WaitToComplete();
    }
}

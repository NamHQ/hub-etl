using Etl.Core.Transformation.Fields;
using System;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    //IMPORTANT: ILoaderArgs is singleton, it is loaded from xml so it must be IMMUTABLE.

    public abstract class Loader
    {
        protected internal abstract Type LoaderType { get; }
    }

    public abstract class Loader<TInst, TDef> : Loader
        where TInst : LoaderInst<TInst, TDef>
        where TDef : Loader<TInst, TDef>
    {
        protected internal override Type LoaderType => typeof(TInst);
    }

    //IMPORTANT: ILoaderArgs is transient, it is created with workflow.

    public interface ILoaderInst
    {
        void Initalize(object args, string inputFile, IReadOnlyCollection<TransformField> fields);
        void ProcessBatch(BatchResult batch);
        void WaitToComplete();
    }

    public abstract class LoaderInst<TInst, TDef> : ILoaderInst
        where TInst : LoaderInst<TInst, TDef>
        where TDef : Loader<TInst, TDef>
    {
        protected virtual void Initalize(TDef args, string inputFile, IReadOnlyCollection<TransformField> fields) { }
        void ILoaderInst.Initalize(object config, string inputFile, IReadOnlyCollection<TransformField> fields)
            => Initalize(config as TDef, inputFile, fields);

        protected abstract void ProcessBatch(BatchResult parseResult);
        void ILoaderInst.ProcessBatch(BatchResult parseResult)
            => ProcessBatch(parseResult);

        protected virtual void WaitToComplete() { }
        void ILoaderInst.WaitToComplete()
            => WaitToComplete();
    }
}

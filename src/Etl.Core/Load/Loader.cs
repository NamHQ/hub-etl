using System;

namespace Etl.Core.Load
{
    public abstract class Loader
    {
        protected internal abstract Type InstanceType { get; }
    }

    public abstract class Loader<TInst> : Loader
        where TInst : ILoaderInst
    {
        protected internal override Type InstanceType => typeof(TInst);
    }

    //IMPORTANT: ILoaderArgs is transient, it is created with workflow.

    public interface ILoaderInst
    {
        void Initalize(object definition, LoaderArgs args);
        void ProcessBatch(BatchResult batch);
        void WaitToComplete();
    }

    public abstract class LoaderInst<TDef> : ILoaderInst
        where TDef : Loader
    {
        protected virtual void Initalize(TDef definition, LoaderArgs args) { }
        void ILoaderInst.Initalize(object definition, LoaderArgs args)
            => Initalize((TDef)definition, args);

        public abstract void ProcessBatch(BatchResult parseResult);

        public virtual void WaitToComplete() { }
    }
}

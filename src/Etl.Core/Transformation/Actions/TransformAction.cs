using System;

namespace Etl.Core.Transformation.Actions
{
    public abstract class TransformAction
    {
        public int Order { get; set; }

        protected internal abstract Type ActionType { get; }
    }

    public abstract class TransformAction<TInst> : TransformAction
        where TInst : ITransformActionInst
    {
        override sealed internal protected Type ActionType  => typeof(TInst);
    }

    public interface ITransformActionInst
    {
        object Execute(object input, ActionArgs args);
    }

    public abstract class TransformActionInst<TOutput> : ITransformActionInst
    {
        protected abstract TOutput Execute(object input, ActionArgs args);
        object ITransformActionInst.Execute(object input, ActionArgs args)
            => Execute(input, args);
    }

    public abstract class TransformActionInst<TDef, TOutput> : TransformActionInst<TOutput>, IInitialization
        where TDef : TransformAction
    {
        protected abstract void Initialize(TDef definition, IServiceProvider sp);
        void IInitialization.Initialize(object args, IServiceProvider sp)
            => Initialize((TDef)args, sp);
    }
}

using System;

namespace Etl.Core.Transformation.Actions
{
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

    public abstract class ValidateActionInst : ITransformActionInst
    {
        protected abstract void Execute(object input, ActionArgs args);
        object ITransformActionInst.Execute(object input, ActionArgs args)
        {
            Execute(input, args);
            return input;
        }
    }

    public abstract class ValidateActionInst<TDef> : ValidateActionInst, IInitialization
        where TDef : TransformAction
    {
        protected abstract void Initialize(TDef definition, IServiceProvider sp);
        void IInitialization.Initialize(object args, IServiceProvider sp)
            => Initialize((TDef)args, sp);
    }
}

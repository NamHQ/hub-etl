namespace Etl.Core.Transformation.Actions
{
    public interface ITransformActionInst
    {
        object Execute(object input, ActionArgs args);
    }

    public abstract class TransformActionInst<TOutput> : ITransformActionInst
    {
        object ITransformActionInst.Execute(object input, ActionArgs args)
            => Execute(input, args);
        protected abstract TOutput Execute(object input, ActionArgs args);
    }
}

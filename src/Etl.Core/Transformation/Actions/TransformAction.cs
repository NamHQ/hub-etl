namespace Etl.Core.Transformation.Actions
{
    public interface ITransformAction
    {
        object Execute(object input, ActionArgs args);
    }

    public abstract class TransformAction<TOutput> : ITransformAction
    {
        object ITransformAction.Execute(object input, ActionArgs args)
            => Execute(input, args);
        protected abstract TOutput Execute(object input, ActionArgs args);
    }
}

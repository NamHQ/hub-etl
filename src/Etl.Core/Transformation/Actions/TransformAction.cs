using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etl.Core.Transformation.Actions
{
    public abstract class TransformAction
    {
        public int Order { get; set; }

        public abstract ITransformActionInst CreateInstance(IServiceProvider sp);
    }

    public class TransformAction<TInstance> : TransformAction where TInstance : ITransformActionInst
    {
        public override ITransformActionInst CreateInstance(IServiceProvider sp)
            => (ITransformActionInst) sp.GetRequiredService(typeof(TInstance));
    }
}

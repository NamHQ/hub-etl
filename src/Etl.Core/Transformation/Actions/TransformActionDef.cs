using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etl.Core.Transformation.Actions
{
    public abstract class TransformActionDef
    {
        public int Order { get; set; }

        public abstract ITransformAction CreateInstance(IServiceProvider sp);
    }

    public class TransformActionDef<TInstance> : TransformActionDef where TInstance : ITransformAction
    {
        public override ITransformAction CreateInstance(IServiceProvider sp)
            => (ITransformAction) sp.GetRequiredService(typeof(TInstance));
    }
}

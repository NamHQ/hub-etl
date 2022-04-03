using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Actions
{
    public abstract class TransformAction
    {
        [XmlAttribute]
        public int Order { get; set; }

        protected internal abstract Type ActionType { get; }
    }

    public abstract class TransformAction<TInst> : TransformAction
        where TInst : ITransformActionInst
    {
        override sealed internal protected Type ActionType  => typeof(TInst);
    }
}

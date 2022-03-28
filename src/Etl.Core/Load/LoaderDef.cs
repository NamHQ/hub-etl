using System;

namespace Etl.Core.Load
{
    //IMPORTANT: ILoaderArgs is singleton, it is loaded from xml so it must be IMMUTABLE.

    public abstract class LoaderDef
    {
        protected internal abstract Type LoaderType { get; }
    }

    public abstract class LoaderDef<TLoader, TDef> : LoaderDef
        where TLoader : Loader<TLoader, TDef>
        where TDef : LoaderDef<TLoader, TDef>
    {
        protected internal override Type LoaderType => typeof(TLoader);
    }
}

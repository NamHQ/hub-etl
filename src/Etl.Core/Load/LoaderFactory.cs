using System;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    public interface ILoaderFactory
    {
        List<(LoaderDef definition, ILoader instance)> Get(IEnumerable<LoaderDef> args);
    }

    public class LoaderFactory : ILoaderFactory
    {
        private readonly IServiceProvider _sp;

        public LoaderFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public List<(LoaderDef definition, ILoader instance)> Get(IEnumerable<LoaderDef> args)
        {
            var loaders = new List<(LoaderDef, ILoader)>();
            if (args != null)
                foreach (var e in args)
                {
                    var type = e.GetType();
                    do
                    {
                        type = type.BaseType;
                        var generic = type.GetGenericArguments();
                        if (generic.Length == 2)
                        {
                            var loader = (ILoader)_sp.GetService(generic[0]);
                            if (loader == null)
                                throw new Exception($"Cannot instantiate implementation type '{generic[0].FullName}'");
                            loaders.Add((e, loader));
                            break;
                        }
                    }
                    while (true);
                }


            return loaders;
        }
    }
}

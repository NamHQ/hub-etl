using Microsoft.Extensions.DependencyInjection;
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
                    loaders.Add((e, (ILoader) _sp.GetRequiredService(e.LoaderType)));


            return loaders;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    public interface ILoaderFactory
    {
        List<(Loader definition, ILoaderInst instance)> Get(IEnumerable<Loader> args);
    }

    public class LoaderFactory : ILoaderFactory
    {
        private readonly IServiceProvider _sp;

        public LoaderFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public List<(Loader definition, ILoaderInst instance)> Get(IEnumerable<Loader> args)
        {
            var loaders = new List<(Loader, ILoaderInst)>();
            if (args != null)
                foreach (var e in args)
                    loaders.Add((e, (ILoaderInst) _sp.GetRequiredService(e.LoaderType)));


            return loaders;
        }
    }
}

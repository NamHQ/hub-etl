using Etl.Core.Load;
using Etl.Core.Settings;
using Etl.Core.Transformation;
using Etl.Core.Transformation.Fields;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Etl.Core
{
    public static class Setup
    {
        public static IServiceCollection AddEtl(this IServiceCollection services, 
            IConfiguration configuration, 
            params Assembly[] loaderAssemblies)
        {
            var etlSetting = configuration.GetSection("Etl").Get<EtlSetting>();
            services.AddSingleton(etlSetting);

            var loaderDefs = services.AddLoaders(loaderAssemblies, etlSetting.References);

            var etlFactory = new EtlFactory(etlSetting, loaderDefs);
            services.AddSingleton(etlFactory);
            services.AddSingleton<IEtlFactory>(etlFactory);

            var cryptorInfo = new CryptorInfo(etlSetting);
            services.AddSingleton(cryptorInfo);
            services.AddTransient<ICryptorInfo>(sp => cryptorInfo);
            
            services.AddTransient<Workflow>();

            services.AddSingleton<IntegerActionInst>();
            services.AddSingleton<StringActionInst>();

            return services;
        }

        private static List<Type> AddLoaders(this IServiceCollection services,
            IEnumerable<Assembly> loaderAssemblies,
            IEnumerable<string> refDlls)
        {
            services.AddSingleton<ILoaderFactory, LoaderFactory>();

            var assemblies = loaderAssemblies == null ? new List<Assembly>() : new List<Assembly>(loaderAssemblies);
            if (refDlls != null)
                assemblies.AddRange(refDlls.Select(e =>
                {
                    var fileInfo = new FileInfo(e);
                    if (!fileInfo.Exists)
                        throw new Exception($"File not found {e} to refer.");
                    return Assembly.LoadFrom(fileInfo.FullName);
                }));

            var types = new List<Type> { typeof(ConsoleLoaderInst) };
            types.AddRange(assemblies
                   .SelectMany(e => e.GetTypes())
                   .Where(e => typeof(ILoaderInst).IsAssignableFrom(e) && !e.IsAbstract));

            foreach (var type in types)
                services.AddTransient(type);

            types =  new List<Type> { typeof(ConsoleLoader) };
            types.AddRange(assemblies
                   .SelectMany(e => e.GetTypes())
                   .Where(e => typeof(Loader).IsAssignableFrom(e) && !e.IsAbstract));

            return types;
        }
    }
}

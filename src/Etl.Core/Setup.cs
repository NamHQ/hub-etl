using Etl.Core.Load;
using Etl.Core.Settings;
using Etl.Core.Transformation;
using Etl.Core.Transformation.Actions;
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
            List<Assembly> transformerAssemblies,
            List<Assembly> loaderAssemblies)
        {
            var etlSetting = configuration.GetSection("Etl").Get<EtlSetting>();
            services.AddSingleton(etlSetting);

            var refAssemblies = etlSetting.References.GetRefAssemblies();

            var (fieldDefs, actionDefs) = services.AddTransformer(transformerAssemblies, refAssemblies);

            var loaderDefs = services.AddLoaders(loaderAssemblies, refAssemblies);

            var etlFactory = new EtlFactory(etlSetting, fieldDefs, actionDefs, loaderDefs);
            services.AddSingleton(etlFactory);
            services.AddSingleton<IEtlFactory>(etlFactory);

            var cryptorInfo = new CryptorInfo(etlSetting);
            services.AddSingleton(cryptorInfo);
            services.AddTransient<ICryptorInfo>(sp => cryptorInfo);

            services.AddTransient<Workflow>();

            return services;
        }

        private static (List<Type> fieldDefs, List<Type> actionDefs) AddTransformer(this IServiceCollection services,
            IEnumerable<Assembly> transformerAssemblies,
            IEnumerable<Assembly> refAssemblies)
        {
            var assemblies = new List<Assembly>(refAssemblies);
            if (transformerAssemblies != null)
                assemblies.AddRange(transformerAssemblies);

            var types = assemblies.SelectMany(e => e.GetTypes().Where(x => !x.IsAbstract));

            var fieldInstType = typeof(ITransformFieldInst);
            var fieldDefType = typeof(TransformField);
            var fieldDefs = new List<Type>();

            var actionInstType = typeof(ITransformActionInst);
            var actionDefType = typeof(TransformAction);
            var actionDefs = new List<Type>();

            foreach (var e in types)
            {
                if (fieldInstType.IsAssignableFrom(e) || actionInstType.IsAssignableFrom(e))
                    services.AddTransient(e);

                if (fieldDefType.IsAssignableFrom(e))
                    fieldDefs.Add(e);

                if (actionDefType.IsAssignableFrom(e))
                    actionDefs.Add(e);
            }
            
            return (fieldDefs, actionDefs);
        }

        private static List<Type> AddLoaders(this IServiceCollection services,
            IEnumerable<Assembly> loaderAssemblies,
            IEnumerable<Assembly> refAssemblies)
        {
            services.AddSingleton<ILoaderFactory, LoaderFactory>();

            var assemblies = new List<Assembly>(refAssemblies);
            if (loaderAssemblies != null)
                assemblies.AddRange(loaderAssemblies);

            var types = assemblies.SelectMany(e => e.GetTypes().Where(x => !x.IsAbstract));

            var loaderInstType = typeof(ILoaderInst);
            var loaderDefType = typeof(Loader);
            var loaderDefs = new List<Type>();

            foreach (var e in types)
            {
                if (loaderInstType.IsAssignableFrom(e))
                    services.AddTransient(e);

                if (loaderDefType.IsAssignableFrom(e))
                    loaderDefs.Add(e);
            }

            services.AddSingleton<ConsoleLoaderInst>();
            loaderDefs.Add(typeof(ConsoleLoader));

            return loaderDefs;
        }

        private static IEnumerable<Assembly> GetRefAssemblies(this IEnumerable<string> refDlls)
            => refDlls.Select(e =>
            {
                var fileInfo = new FileInfo(e);
                if (!fileInfo.Exists)
                    throw new Exception($"File not found {e} to refer.");
                return Assembly.LoadFrom(fileInfo.FullName);
            });
    }
}

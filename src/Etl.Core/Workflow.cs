using System;
using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Load;
using System.IO;
using Etl.Core.Settings;
using System.Linq;

namespace Etl.Core
{
    public class Workflow
    {
        private readonly IServiceProvider _sp;
        private readonly EtlSetting _etlSetting;
        private readonly IEtlContext _context;
        private readonly IEtlFactory _etlFactory;
        private readonly ILoaderFactory _loaderFactory;
        
        private ICompilerEvent _events;
        private List<(LoaderDef definition, ILoader instance)> _loaders = new();
        private (EtlDef definition, Etl instance) _etl;

        public Workflow(EtlSetting setting, IEtlContext eltContext, IEtlFactory etlFactory, ILoaderFactory loaderFactory, IServiceProvider sp)
        {
            _sp = sp;

            _etlSetting = setting;
            _context = eltContext;
            _etlFactory = etlFactory;
            _loaderFactory = loaderFactory;
        }

        public Workflow Subcribe(Action<CompilerEvent> subscribe)
        {
            var e = new CompilerEvent();
            subscribe(e);

            _events = e;
            return this;
        }

        public Workflow SetConfig(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
                return this;

            var (definition, instance) = _etlFactory.GetFrom(configFilePath);
            return SetConfig(definition, instance);
        }

        public Workflow SetConfig(EtlDef definition, Etl instance = null)
        {
            if (definition != null)
            {
                _loaders = _loaderFactory.Get(definition.Loaders) ?? new();
                _etl = (definition, instance ?? new Etl(definition));
            }

            return this;
        }

        public Workflow AddLoaders(params LoaderDef[] args)
        {
            var items = _loaderFactory.Get(args);
            if (items != null)
                _loaders.AddRange(items);

            return this;
        }

        public Workflow AddLoaders(params ILoader[] loaders)
        {
            if (loaders != null)
                _loaders.AddRange(loaders.Select(e => ((LoaderDef)default, e)));
            return this;
        }

        public Workflow AddLoaders(params (LoaderDef def, ILoader instance)[] loaders)
        {
            if (loaders != null)
                _loaders.AddRange(loaders);
            return this;
        }

        public void Start(string dataFilePath, int? take = null, int? skip = null)
        {
            if (!File.Exists(dataFilePath))
                throw new Exception($"Not existed data file '{dataFilePath}'.");

            if (_etl.instance == null)
            {
                var (definition, instance) = _etlFactory.Get(dataFilePath);
                SetConfig(definition, instance);
            }

            new WorkflowExecutor(
                _etlSetting, _etl.definition, _etl.instance, 
                _context, 
                _loaders,
                _events)
                .Start(dataFilePath, _sp, take, skip);
        }
    }
}

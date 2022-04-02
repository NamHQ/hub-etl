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
        private readonly IEtlFactory _etlFactory;
        private readonly ILoaderFactory _loaderFactory;
        
        private ICompilerEvent _events;
        private List<(Loader definition, ILoaderInst instance)> _loaders = new();
        private (Etl definition, EtlInst instance) _etl;

        public Workflow(EtlSetting setting, IEtlFactory etlFactory, ILoaderFactory loaderFactory, IServiceProvider sp)
        {
            _sp = sp;

            _etlSetting = setting;
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

        public Workflow SetConfig(Etl definition, EtlInst instance = null)
        {
            if (definition != null)
            {
                _loaders = _loaderFactory.Get(definition.Loaders) ?? new();
                _etl = (definition, instance ?? new EtlInst(definition));
            }

            return this;
        }

        public Workflow AddLoaders(params Loader[] args)
        {
            var items = _loaderFactory.Get(args);
            if (items != null)
                _loaders.AddRange(items);

            return this;
        }

        public Workflow AddLoaders(params ILoaderInst[] loaders)
        {
            if (loaders != null)
                _loaders.AddRange(loaders.Select(e => ((Loader)default, e)));
            return this;
        }

        public Workflow AddLoaders(params (Loader def, ILoaderInst instance)[] loaders)
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

            new WorkflowInst(
                _etlSetting, _etl.definition, _etl.instance, 
                _loaders,
                _events)
                .Start(dataFilePath, _sp, take, skip);
        }
    }
}

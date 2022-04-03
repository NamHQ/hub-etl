using System;
using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Load;
using System.IO;
using Etl.Core.Settings;

namespace Etl.Core
{
    public class WorkflowBuilder
    {
        private readonly IServiceProvider _sp;
        private readonly EtlSetting _etlSetting;
        private readonly IEtlFactory _etlFactory;
        
        private ICompilerEvent _events;
        private (Etl definition, EtlInst instance) _etl;
        private List<Loader> _extraLoaders = new();

        public WorkflowBuilder(EtlSetting setting, IEtlFactory etlFactory, IServiceProvider sp)
        {
            _sp = sp;

            _etlSetting = setting;
            _etlFactory = etlFactory;
        }

        public WorkflowBuilder Subcribe(Action<CompilerEvent> subscribe)
        {
            var e = new CompilerEvent();
            subscribe(e);

            _events = e;
            return this;
        }

        public WorkflowBuilder SetConfig(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
                return this;

            var (definition, instance) = _etlFactory.GetFrom(configFilePath);
            return SetConfig(definition, instance);
        }

        public WorkflowBuilder SetConfig(Etl definition, EtlInst instance = null)
        {
            if (definition != null)
                _etl = (definition, instance ?? new EtlInst(definition));

            return this;
        }

        public WorkflowBuilder AddLoaders(params Loader[] loaders)
        {
            _extraLoaders.AddRange(loaders);

            return this;
        }

        public Workflow Build(string dataFilePath)
        {
            if (!File.Exists(dataFilePath))
                throw new Exception($"Not existed data file '{dataFilePath}'.");

            if (_etl.instance == null)
            {
                var (definition, instance) = _etlFactory.Get(dataFilePath);
                SetConfig(definition, instance);
            }

            return new Workflow(dataFilePath, _etlSetting, _etl.definition, _etl.instance, _extraLoaders, _events, _sp);
        }
    }
}

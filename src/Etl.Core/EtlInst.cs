using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Scanner;
using Etl.Core.Extraction;
using Etl.Core.Transformation;
using System;
using Etl.Core.Transformation.Fields;
using System.IO;
using Etl.Core.Load;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Etl.Core
{
    public class EtlInst
    {
        private readonly ExtractorInst _extractor;
        private readonly TransformerInst _transformer;
        private readonly List<Loader> _loaders;
        public IReadOnlyCollection<TransformField> AllFields => _transformer.AllFields;

        public EtlInst(Etl etl)
        {
            _extractor = new ExtractorInst(etl.Extraction);
            _transformer = new TransformerInst(etl.Transformation, etl.Extraction.Layout);
            _loaders = etl.Loaders;
        }

        public (
            Scanner.Scanner scaner,
            Func<ExtractedRecord, TransformResult> transformInstance,
            List<ILoaderInst> loaders)
            Start(string dataFile, Func<StreamReader> getStreamReader, IServiceProvider sp, List<Loader> extraLoaders, Action<List<TextLine>, float> flush)
        {
            

            return (
                _extractor.CreateScanner(getStreamReader, flush),
                _transformer.CreateInstance(sp),
                CreateLoaderInstances(dataFile, sp, extraLoaders));
        }

        public ExtractedRecord Extract(List<TextLine> textLines, IEtlEvent events)
            => _extractor.Execute(textLines, events);

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _transformer.ApplyMassage(batch);

        private List<ILoaderInst> CreateLoaderInstances(string dataFile, IServiceProvider sp, List<Loader> extraLoaders)
        {
            var allLoaders = new List<Loader>(_loaders);
            if (extraLoaders != null)
                allLoaders.AddRange(extraLoaders);

            var loaderArgs = new LoaderArgs(dataFile, _transformer.AllFields);
            return allLoaders.Select(e =>
            {
                var inst = (ILoaderInst)sp.GetRequiredService(e.InstanceType);
                inst.Initalize(e, loaderArgs);
                return inst;
            }).ToList();
        }
    }
}

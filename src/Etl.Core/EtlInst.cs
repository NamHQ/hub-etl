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
        private readonly Func<string, IServiceProvider, List<ILoaderInst>> _createLoaders;
        public IReadOnlyCollection<TransformField> AllFields => _transformer.AllFields;

        public EtlInst(Etl etl)
        {
            _extractor = new ExtractorInst(etl.Extraction);
            _transformer = new TransformerInst(etl.Transformation, etl.Extraction.Layout);

            _createLoaders = (dataFile, sp) =>
            {
                var loaderArgs = new LoaderArgs(dataFile, _transformer.AllFields);
                return etl.Loaders.Select(e =>
                {
                    var inst = (ILoaderInst)sp.GetRequiredService(e.InstanceType);
                    inst.Initalize(e, loaderArgs);
                    return inst;
                }).ToList();
            };
        }

        public (
            Scanner.Scanner scaner,
            Func<ExtractedRecord, TransformResult> transformInstance,
            List<ILoaderInst> loaders)
            Start(string dataFile, Func<StreamReader> getStreamReader, IServiceProvider sp, Action<List<TextLine>> flush)
        {
            return (
                _extractor.CreateScanner(getStreamReader, flush),
                _transformer.CreateInstance(sp),
                _createLoaders(dataFile, sp));
        }

        public ExtractedRecord Extract(List<TextLine> textLines, ICompilerEvent events)
            => _extractor.Execute(textLines, events);

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _transformer.ApplyMassage(batch);
    }
}

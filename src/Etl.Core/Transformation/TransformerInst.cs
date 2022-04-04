using Etl.Core.Transformation.Fields;
using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Etl.Core.Transformation
{
    public class TransformerInst
    {
        private const string NAMESPACE = "tempNameSpace";
        private const string CLASS = "tempClass";
        private const string METHOD = "Execute";
        private readonly Assembly _massageAssembly;
        private readonly RecordField _recordField;

        private Func<List<IDictionary<string, object>>, List<IDictionary<string, object>>> _applyMassage;
        public IReadOnlyCollection<TransformField> AllFields { get; }

        public TransformerInst(Transformer transformDef, Layout layoutDef)
        {
            _massageAssembly = CompileCSharpCode(transformDef.Massage);
            _recordField = new RecordField { Fields = transformDef.Fields };

            AllFields = MakeSureMergeParserFields(_recordField, layoutDef);
        }

        public Func<ExtractedRecord, TransformResult> CreateInstance(IServiceProvider sp)
        {
            if (_massageAssembly != null)
            {
                var instance = _massageAssembly.CreateInstance($"{NAMESPACE}.{CLASS}");
                var method = instance.GetType().GetMethod(METHOD);
                _applyMassage = batch => method.Invoke(instance, new object[] { batch }) as List<IDictionary<string, object>>;
            }

            var recordFieldInst = new RecordFieldInst();
            recordFieldInst.Initialize(_recordField, sp);

            return record => recordFieldInst.Transform(record);
        }

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _applyMassage?.Invoke(batch) ?? batch;

        private static List<TransformField> MakeSureMergeParserFields(RecordField recordField, Layout layout)
        {
            List<TransformField> extractedFields = new();
            ToExtractedFields(layout, extractedFields);

            if (recordField.Fields.Count == 0)
            {
                recordField.Fields.AddRange(extractedFields);
                return new List<TransformField>(recordField.Fields);
            }

            return MergeFields(recordField.Fields, extractedFields, recordField.IgnoreParserFields);
        }

        private static void ToExtractedFields(Layout layout, List<TransformField> outFields)
        {
            if (layout == null)
                return;

            if (!string.IsNullOrWhiteSpace(layout.DataField))
            {
                if (layout.Repeat)
                {
                    ArrayField arr = new() { DataField = layout.DataField };
                    outFields.Add(arr);
                    outFields = arr.Fields;
                }
                else
                    outFields.Add(new StringField { DataField = layout.DataField });
            }

            if (layout.Children != null && layout.Children.Count > 0)
                layout.Children.ForEach(e => ToExtractedFields(e, outFields));
        }

        private static List<TransformField> MergeFields(List<TransformField> allTransformFields, List<TransformField> allExtractedFields, HashSet<string> ignoreParserFields)
        {
            var items = new List<TransformField> ();

            var dictionary = allTransformFields.ToDictionary(e => e.DataField);

            foreach (var extractedField in allExtractedFields.Where(x => !ignoreParserFields.Contains(x.DataField)))
            {
                if (!dictionary.TryGetValue(extractedField.DataField, out TransformField transformField))
                {
                    allTransformFields.Add(extractedField);
                    items.Add(extractedField);
                }
                else if (extractedField is ArrayField extractedArray)
                {
                    if (transformField is not ArrayField transfromArray)
                        throw new Exception($"Extract array field {extractedArray.DataField} does not match defined field {transformField.DataField} {transformField.GetType().Name}");
                    
                    var nestedItems = MergeFields(transfromArray.Fields, extractedArray.Fields, transfromArray.IgnoreParserFields);

                    if (transfromArray.Flat)
                        items.AddRange(nestedItems);
                    else
                        items.Add(transfromArray);
                }
                else
                    items.Add(transformField);
            }

            return items;
        }

        private static Assembly CompileCSharpCode(MassageDataCSharpCode transform)
        {
            if (transform == null || string.IsNullOrWhiteSpace(transform.Code))
                return null;

            var sb = new StringBuilder($"namespace {NAMESPACE} {{ class {CLASS} {{");
            if (!string.IsNullOrWhiteSpace(transform.GlobalVariables))
                sb.Append($"public {transform.GlobalVariables}");
            sb.Append($"public List<IDictionary<string, object>> {METHOD}(List<IDictionary<string, object>> B) {{ {transform.Code};");
            sb.Append("}}}");

            return CShapCompiler.Compile(sb.ToString());
        }
    }
}

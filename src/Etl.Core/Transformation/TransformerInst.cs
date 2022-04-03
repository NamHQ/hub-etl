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
        private readonly GroupField _gropuField;

        private Func<List<IDictionary<string, object>>, List<IDictionary<string, object>>> _applyMassage;
        public IReadOnlyCollection<TransformField> AllFields { get; }

        public TransformerInst(Transformer transformDef, Layout layoutDef)
        {
            _massageAssembly = CompileCSharpCode(transformDef.Massage);
            _gropuField = new GroupField { Fields = transformDef.Fields };

            MakeSureMergeParserFields(_gropuField, layoutDef);

            AllFields = _gropuField.Fields;
        }

        public Func<ExtractedRecord, TransformResult> CreateInstance(IServiceProvider sp)
        {
            if (_massageAssembly != null)
            {
                var instance = _massageAssembly.CreateInstance($"{NAMESPACE}.{CLASS}");
                var method = instance.GetType().GetMethod(METHOD);
                _applyMassage = batch => method.Invoke(instance, new object[] { batch }) as List<IDictionary<string, object>>;
            }

            var groupFieldInst = new GroupFieldInst();
            groupFieldInst.Initialize(_gropuField, sp);

            return record => groupFieldInst.Transform(record);
        }

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _applyMassage?.Invoke(batch) ?? batch;

        private static void MakeSureMergeParserFields(GroupField mapFields, Layout layout)
        {
            List<TransformField> parseFields = new();
            GetParserFieldsRecursive(layout, parseFields);

            if (mapFields.Fields.Count == 0)
                mapFields.Fields.AddRange(parseFields);
            else
                MergeFieldsRecursive(mapFields.Fields, parseFields, mapFields.IgnoreParserFields);
        }

        private static void GetParserFieldsRecursive(Layout layout, List<TransformField> parserFields)
        {
            if (layout == null)
                return;

            if (!string.IsNullOrWhiteSpace(layout.DataField))
            {
                if (layout.Repeat)
                {
                    ArrayField arr = new() { ParserField = layout.DataField };
                    parserFields.Add(arr);
                    parserFields = arr.Fields;
                }
                else
                    parserFields.Add(new StringField { ParserField = layout.DataField });
            }

            if (layout.Children != null && layout.Children.Count > 0)
                layout.Children.ForEach(e => GetParserFieldsRecursive(e, parserFields));
        }

        private static void MergeFieldsRecursive(List<TransformField> mapFields, List<TransformField> parserFields, HashSet<string> ignoreParserFields)
        {
            var dictionary = mapFields.ToDictionary(e =>
            {
                var dataField = GetFirstNotEmpty(e.Field, e.ParserField);
                if (string.IsNullOrEmpty(dataField))
                    throw new Exception($"Fields in {nameof(Transformer)} expect {nameof(e.Field)} or {nameof(e.ParserField)}");

                return dataField;
            });

            foreach (var field in parserFields.Where(x => !ignoreParserFields.Contains(x.ParserField)))
            {
                var parserField = GetFirstNotEmpty(field.ParserField, field.Field);
                if (!dictionary.TryGetValue(parserField, out TransformField mapField))
                {
                    mapFields.Add(field);
                }
                else if (field is ArrayField parserArray)
                {
                    if (mapField is not ArrayField mapArray)
                    {
                        var mapParserField = GetFirstNotEmpty(mapField.ParserField, mapField.Field);
                        throw new Exception($"Parser array field {parserField} does not match defined field {mapParserField} {mapField.GetType().Name}");
                    }
                    MergeFieldsRecursive(mapArray.Fields, parserArray.Fields, mapArray.IgnoreParserFields);
                }
            }
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

        private static string GetFirstNotEmpty(params string[] values)
        {
            foreach(var e in values)
                if (!string.IsNullOrWhiteSpace(e))
                    return e;

            return null;
        }
    }
}

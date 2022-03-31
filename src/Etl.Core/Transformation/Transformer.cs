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
    public class Transformer
    {
        private const string NAMESPACE = "tempNameSpace";
        private const string CLASS = "tempClass";
        private const string METHOD = "Execute";
        private readonly Assembly _massageAssembly;
        private readonly GroupField _collectionField;

        private Func<List<IDictionary<string, object>>, List<IDictionary<string, object>>> _applyMassage;
        public IReadOnlyCollection<TransformField> AllFields => _collectionField.Fields;

        public Transformer(TransformDef transformDef, LayoutDef layoutDef)
        {
            _massageAssembly = CompileCSharpCode(transformDef.Massage);
            _collectionField = new GroupField { Fields = transformDef.Fields };

            MakeSureMergeParserFields(_collectionField, layoutDef);
        }

        public void Initialize(IServiceProvider sp)
        {
            _collectionField.Initialize(sp);

            if (_massageAssembly != null)
            {
                var instance = _massageAssembly.CreateInstance($"{NAMESPACE}.{CLASS}");
                var method = instance.GetType().GetMethod(METHOD);
                _applyMassage = batch => method.Invoke(instance, new object[] { batch }) as List<IDictionary<string, object>>;
            }
        }

        public TransformResult Execute(IDictionary<string, object> record, IEtlContext context)
            => _collectionField.Transform(record, context) as TransformResult;

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _applyMassage?.Invoke(batch) ?? batch;

        private static void MakeSureMergeParserFields(GroupField mapFields, LayoutDef layout)
        {
            List<TransformField> parseFields = new();
            GetParserFieldsRecursive(layout, parseFields);

            if (mapFields.Fields.Count == 0)
                mapFields.Fields.AddRange(parseFields);
            else
                MergeFieldsRecursive(mapFields.Fields, parseFields, mapFields.IgnoreParserFields);
        }

        private static void GetParserFieldsRecursive(LayoutDef layout, List<TransformField> parserFields)
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
                if (string.IsNullOrEmpty(e.LazyDbField.Value))
                    throw new Exception($"Fields in {nameof(TransformDef)} expect {nameof(e.Field)} or {nameof(e.ParserField)}");

                return e.LazyDbField.Value;
            });

            foreach (var field in parserFields.Where(x => !ignoreParserFields.Contains(x.LazyParserField.Value)))
                if (!dictionary.TryGetValue(field.LazyParserField.Value, out TransformField mapField))
                {
                    mapFields.Add(field);
                }
                else if (field is ArrayField parserArray)
                {
                    if (mapField is not ArrayField mapArray)
                        throw new Exception($"Parser array field {field.LazyParserField.Value} does not match defined field {mapField.LazyParserField.Value} {mapField.GetType().Name}");

                    MergeFieldsRecursive(mapArray.Fields, parserArray.Fields, mapArray.IgnoreParserFields);
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
    }
}

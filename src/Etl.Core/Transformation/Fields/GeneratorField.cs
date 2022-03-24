//using Etl.Core.Parser;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Etl.Core.Generator.Fields
//{
//    public class DataGenerator : CollectionField
//    {
//        private bool _isMerged;

//        public object Execute(IDictionary<string, object> record, Config config)
//        {
//            MergeParserField(config.Layout);

//            return GetValue(record);
//        }

//        public void MergeParserField(Layout layout)
//        {
//            if (!_isMerged)
//                lock (this)
//                {
//                    if (!_isMerged)
//                    {
//                        var parseFields = new List<FieldBase>();
//                        GetParserFieldsRecursive(layout, parseFields);

//                        if (Fields.Count == 0)
//                            Fields = parseFields;
//                        else
//                            MergeFieldsRecursive(Fields, parseFields, IgnoreParserFields);
//                    }

//                    _isMerged = true;
//                }
//        }

//        private static void GetParserFieldsRecursive(Layout layout, List<FieldBase> parserFields)
//        {
//            if (layout == null)
//                return;

//            if (!string.IsNullOrWhiteSpace(layout.DataField))
//            {
//                if (layout.Repeat)
//                {
//                    var arr = new ArrayField { ParserField = layout.DataField };
//                    parserFields.Add(arr);
//                    parserFields = arr.Fields;
//                }
//                else
//                    parserFields.Add(new StringField { ParserField = layout.DataField });
//            }

//            if (layout.Children != null && layout.Children.Count > 0)
//                layout.Children.ForEach(e => GetParserFieldsRecursive(e, parserFields));
//        }

//        private void MergeFieldsRecursive(List<FieldBase> mapFields, List<FieldBase> parserFields, HashSet<string> ignoreParserFields)
//        {
//            var dictionary = mapFields.ToDictionary(e =>
//            {
//                if (string.IsNullOrEmpty(e.LazyDbField.Value))
//                    throw new Exception($"Fields in {nameof(DataGenerator)} expect {nameof(e.DbField)} or {nameof(e.ParserField)}");

//                return e.LazyDbField.Value;
//            });

//            foreach (var field in parserFields.Where(x => !ignoreParserFields.Contains(x.LazyParserField.Value)))
//                if (!dictionary.TryGetValue(field.LazyParserField.Value, out FieldBase mapField))
//                {
//                    mapFields.Add(field);
//                }
//                else if (field is ArrayField parserArray)
//                {
//                    if (mapField is not ArrayField mapArray)
//                        throw new Exception($"Parser array field {field.LazyParserField.Value} does not match defined field {mapField.LazyParserField.Value} {mapField.GetType().Name}");

//                    MergeFieldsRecursive(mapArray.Fields, parserArray.Fields, mapArray.IgnoreParserFields);
//                }
//        }
//    }
//}

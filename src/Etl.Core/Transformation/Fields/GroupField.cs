﻿using Etl.Core.Extraction;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class GroupField : TransformField
    {
        [XmlArrayItem("Integer", typeof(IntegerField))]
        //[XmlArrayItem("Float", typeof(FLoatField))]
        //[XmlArrayItem("Boolean", typeof(BooleanField))]
        //[XmlArrayItem("Date", typeof(DateField))]
        //[XmlArrayItem("String", typeof(StringField))]
        //[XmlArrayItem("Hash", typeof(HashField))]
        //[XmlArrayItem("Encrypt", typeof(EncryptField))]
        //[XmlArrayItem("Group", typeof(GroupField))]
        //[XmlArrayItem("Array", typeof(ArrayField))]
        public virtual List<TransformField> Fields { get; set; } = new();

        public HashSet<string> IgnoreParserFields { get; set; } = new();

        protected override ITransformFieldInst OnCreateInstance(IServiceProvider sp)
        {
            var result = new GroupFieldInst();
            foreach (var e in Fields)
            {
                var item = e.CreateInstance(sp);
                if (e is ArrayField array && array.Flat)
                {
                    if (result.FlatArray != null)
                        throw new Exception($"Not except multiple flat {nameof(ArrayField)} in the same hierarchy.");

                    result.FlatArray = (ArrayFieldInst)item;
                }
                else
                    result.Fields.Add(item);
            }

            return result;
        }
    }

    public class GroupFieldInst : TransformFieldInst<TransformResult>
    {
        public ArrayFieldInst FlatArray;
        public List<ITransformFieldInst> Fields = new();

        protected override TransformResult Transform(ExtractedRecord record)
        {
            IDictionary<string, object> newRecord = null;
            var result = FlatArray?.Transform(record) ?? new TransformResult();
            if (result.Items.Count == 0)
                result.Items.Add(newRecord = new Dictionary<string, object>());

            try
            {
                foreach (var field in Fields)
                {
                    var val = field.Transform(record);
                    if (val == null)
                        continue;

                    if (newRecord != null)
                        newRecord[field.DataField] = val;
                    else
                        result.Items.ForEach(e => e[field.DataField] = val);
                }
            }
            catch (Exception ex)
            {
                return new TransformResult { TotalErrors = Math.Max(result.TotalRecords, 1) }
                    .AddErorr(ex.Message);
            }

            return result;
        }
    }

}
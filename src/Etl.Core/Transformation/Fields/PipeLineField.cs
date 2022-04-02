using Etl.Core.Extraction;
using Etl.Core.Scanner;
using Etl.Core.Transformation.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class PipeLineFieldDef : TransformFieldDef
    {
        [XmlElement("Actions")]
        public List<TransformActionDef> ActionDefs { get; set; } = new();

        protected abstract TransformActionDef MainActionDef { get; }

        protected override ITransformField OnCreateInstance(IServiceProvider sp)
        {
            var instance = new PipeLineField
            {
                PipeLine = new List<TransformActionDef>(ActionDefs) { MainActionDef }
                    .OrderBy(e => e.Order)
                    .Select(e => e.CreateInstance(sp))
                    .ToList()
            };

            return instance;
        }
    }

    public class PipeLineField : TransformField
    {
        public List<ITransformAction> PipeLine { get; set; }

        public override object Transform(ExtractedRecord record)
        {
            record.TryGetValue(ParserField, out IExtractedInfo info);
            string rawValue = info == null ? null : record.Block.GetValue(info);

            object value = rawValue;
            var args = new ActionArgs();
            ITransformAction currentAction = default;

            foreach (var action in PipeLine)
                try
                {
                    currentAction = action;
                    value = action.Execute(value, args);
                }
                catch
                {
                    throw Stop(record.Block, info, $"{currentAction.GetType().Name}({value ?? "NULL"})", rawValue);
                }

            if (Required && value == null)
                throw Stop(record.Block, info, "Required", rawValue);

            
            return value;
        }

        protected Exception Stop(TextBlock block, IExtractedInfo info, string reason, string rawValue)
        {
            var sb = new StringBuilder();
            sb.Append($"  Field: '{DataField}'");
            sb.Append($", Invalid {reason}");

            if (info != null)
            {
                var startIndex = Math.Min(block.Count - 1, info.From.row);
                var endIndex = Math.Min(block.Count - 1, info.To.row);

                sb.Append($", Raw-Data: '{rawValue ?? "EMPTY"}'");
                sb.Append($", From(R.{block[startIndex].Row}, {WriteColumn(info.From)})");
                sb.Append($"-To(R.{block[endIndex].Row}, {WriteColumn(info.To)})");
            }
            else
                sb.Append(", Raw-Data: [NOT FOUND]");

            sb.Append($", Text: {block[0].Text}");

            return new(sb.ToString());
        }

        private static string WriteColumn((int row, int column) pos)
                => pos.column == int.MaxValue ? "~" : pos.column.ToString();
    }
}

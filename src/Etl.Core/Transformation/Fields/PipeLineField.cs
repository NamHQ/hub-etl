using Etl.Core.Extraction;
using Etl.Core.Scanner;
using Etl.Core.Transformation.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etl.Core.Transformation.Fields
{
    public class PipeLineField : TransformField<PipeLineFieldInst>
    {
        public List<TransformAction> Actions { get; set; } = new();

        public virtual TransformAction DefaultAction { get; } = null;
    }

    public class PipeLineFieldInst : TransformFieldInst<PipeLineField, object>
    {
        private List<ITransformActionInst> _pipeLine;

        public override void Initialize(PipeLineField definition, IServiceProvider sp)
        {
            var actions = new List<TransformAction>(definition.Actions);
            if (definition.DefaultAction != null)
                actions.Add(definition.DefaultAction);

            _pipeLine = actions
                     .OrderBy(e => e.Order)
                     .Select(e =>
                        {
                            var action = (ITransformActionInst) sp.GetRequiredService(e.ActionType);
                            (action as IInitialization)?.Initialize(e, sp);
                            return action;
                         })
                     .ToList();
        }

        public override object Transform(ExtractedRecord record)
        {
            record.TryGetValue(ParserField, out IExtractedInfo info);
            string text = info == null ? null : record.Block.GetValue(info);

            object value = text;
            var args = new ActionArgs(record);
            ITransformActionInst currentAction = default;

            foreach (var action in _pipeLine)
                try
                {
                    currentAction = action;
                    value = action.Execute(value, args);
                }
                catch
                {
                    throw Stop(record.Block, info, $"{currentAction.GetType().Name.Replace("ActionInst", "")}({value ?? "NULL"})", text);
                }

            if (Required && value == null)
                throw Stop(record.Block, info, "Required", text);

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

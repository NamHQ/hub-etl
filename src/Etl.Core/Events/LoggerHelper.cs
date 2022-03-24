﻿using Etl.Core.Transformation;
using Etl.Core.Extraction;
using Etl.Core.Scanner;
using System;
using System.Collections.Generic;
using System.Text;
using Etl.Core.Load;

namespace Etl.Core.Events
{
    public static class LoggerHelper
    {
        public static void ApplyLog(CompilerEvent events, ILogWriter writer, LogOptions options)
        {
            if (options.OnStart)
                events.OnStart = e => OnStart(writer.Debug, e);

            if (options.OnScanned)
                events.OnScanned = e => OnScanned(writer.Debug, e);

            if (options.OnExtracting)
                events.OnParsing = (layout, hierarchy, block, from, start, end, dataField, value) => OnParsing(writer.Debug, layout, hierarchy, block, from, start, end, dataField, value);

            if (options.OnExtracted)
                events.OnExtracted = e => OnExtracted(writer.Debug, e);

            if (options.OnTransformed)
                events.OnTransformed = e => OnTransformed(writer.Debug, e);

            if (options.OnTransformedBatch)
                events.OnTransformedBatch = e => OnTransformedBatch(writer.Debug, e);

            if (options.OnError)
                events.OnError = (message, e) => OnError(writer.Error, message, e);
        }

        public static void OnStart(Action<string> writer, int count)
            => writer($"==========   SCANNED RECORD {count}   ============");

        public static void OnScanned(Action<string> writer, IReadOnlyCollection<TextLine> textLines)
        {
            if (textLines != null)
                foreach (var e in textLines)
                    writer(e.ToString());
        }

        public static void OnParsing(
            Action<string> writer,
            LayoutDef layout,
            int hierachy,
            TextBlock block,
            (int row, int column)? start, (int row, int column)? from, (int row, int column)? to,
            string dataField = null, string value = null)
        {
            if (block.Count == 0)
                return;

            var sb = new StringBuilder(" ".PadLeft(hierachy * 4));

            if (start != null)
            {
                sb.Append($"STR {layout.Direction}");

                if (!string.IsNullOrEmpty(layout.Start))
                    sb.Append($", Regex={layout.Start}");

                if (layout.StartOffset != 0)
                    sb.Append($", Offset={layout.StartOffset}");

                sb.Append($", Current(R.{block[Math.Min(block.Count - 1, start.Value.row)].Row},{WriteColumn(start)})");

                if (to != null)
                    sb.Append($", To(R.{block[Math.Min(block.Count - 1, to.Value.row)].Row},{WriteColumn(to)})");

                writer(sb.ToString());
            }
            else if (from != null)
            {
                sb.Append($"    From(R.{block[Math.Min(block.Count - 1, from.Value.row)].Row},{WriteColumn(from)})");

                if (to != null)
                    sb.Append($", To(R.{block[Math.Min(block.Count - 1, to.Value.row)].Row},{WriteColumn(to)})");

                if (!string.IsNullOrEmpty(dataField))
                    sb.Append($" ===> Field({dataField},{value})");

                writer(sb.ToString());
            }
            else if (to != null)
            {
                sb.Append($"END {layout.Direction}");

                if (!string.IsNullOrEmpty(layout.End))
                    sb.Append($", Regex={layout.End}");

                if (layout.EndOffset != 0)
                    sb.Append($", Offset={layout.EndOffset}");

                sb.Append($", EndPos(R.{block[Math.Min(block.Count - 1, to.Value.row)].Row},{WriteColumn(to)})");
                writer(sb.ToString());
            }

            static string WriteColumn((int row, int column)? pos)
                => pos == null || pos.Value.column == int.MaxValue ? "~" : pos.Value.column.ToString();
        }

        public static void OnExtracted(Action<string> writer, IDictionary<string, object> record)
            => writer(BuildMessage("\nPARSE RESULT:\n____________\n", record));

        public static void OnTransformed(Action<string> writer, object result)
        {
            if (result is IDictionary<string, object> one)
                writer(BuildMessage("\nGENERATE RESULT:\n________________\n", one));
            else if (result is List<IDictionary<string, object>> many)
            {
                foreach (var e in many)
                    writer(BuildMessage("---------------\n", e));
            }
        }

        public static void OnTransformedBatch(Action<string> writer, BatchResult result)
            => writer($"BATCH RESULT: {result}");

        public static void OnError(Action<string, Exception> writer, string message, Exception exception)
        {
            if (exception is ExtractException)
                writer(message, exception);
            else if (exception is TransformException)
                writer(message, exception);
            else
                writer(message, exception);
        }

        public static string BuildMessage(string message, IDictionary<string, object> record)
        {
            var sb = new StringBuilder(message);
            foreach (var e in record)
                BuildMessage("", e, sb);

            return sb.ToString();
        }

        public static void BuildMessage(string pad, KeyValuePair<string, object> field, StringBuilder sb)
        {
            var space = "    ";
            if (field.Value is IDictionary<string, object> group)
            {
                sb.AppendLine($"{pad}{field.Key} :");
                foreach (var f in group)
                    BuildMessage(pad + space, f, sb);
            }
            else if (field.Value is List<IDictionary<string, object>> array)
            {
                sb.AppendLine($"{pad}{field.Key} :");
                foreach (var record in array)
                    if (record.Count > 0)
                    {
                        sb.AppendLine(pad + space + "---------------");
                        foreach (var f in record)
                            BuildMessage(pad + space, f, sb);
                    }
            }
            else
                sb.AppendLine($"{pad}{field.Key} : {field.Value}");
        }
    }
}

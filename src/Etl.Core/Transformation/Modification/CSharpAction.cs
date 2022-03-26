using Etl.Core.Transformation.Fields;
using Etl.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etl.Core.Transformation.Modification
{
    public class CSharpAction : ModificationActionBase
    {
        private Func<IDictionary<string, object>, string, string> _executor;
        public string Code { get; set; }

        public override string Execute(FieldBase field, string rawValue, IDictionary<string, object> record)
        {
            if (string.IsNullOrWhiteSpace(Code))
                throw new Exception($"{nameof(CSharpAction)} expects {nameof(Code)} not empty.");

            if (_executor == null)
            {
                var namespaceName = "tempNameSpace";
                var className = "tempClass";
                var methodName = "tempMethod";
                var sb = new StringBuilder ($"namespace {namespaceName} {{ class {className} {{");
                sb.Append($"public static string {methodName}(IDictionary<string, object> R, string V) {{ {Code};");
                sb.Append("}}}");

                var (method, instance) = CShapCompiler.BuildExecutor(sb.ToString(), namespaceName, className, methodName);
                _executor = (record, value) => method.Invoke(instance, new object[] { record, value }) as string;
            }

            return _executor(record, rawValue);
        }
    }
}

using Etl.Core.Extraction;
using Etl.Core.Utils;
using System;
using System.Text;

namespace Etl.Core.Transformation.Actions
{
    public class CSharpAction : TransformAction<CSharpActionInst>
    {
        public string Code { get; set; }
        public readonly Func<string, ExtractedRecord, object> Executor;

        public CSharpAction()
        {
            var namespaceName = "tempNameSpace";
            var className = "tempClass";
            var methodName = "tempMethod";
            var sb = new StringBuilder($"namespace {namespaceName} {{ class {className} {{");
            sb.Append($"public static object {methodName}(string V, ExtractedRecord R) {{ {Code};");
            sb.Append("}}}");

            var (method, instance) = CShapCompiler.BuildExecutor(sb.ToString(), namespaceName, className, methodName);
            Executor = (value, record) => method.Invoke(instance, new object[] { record, value });
        }
    }

    public class CSharpActionInst : TransformActionInst<CSharpAction, object>
    {
        public Func<string, ExtractedRecord, object> _executor;

        protected override void Initialize(CSharpAction definition, IServiceProvider sp)
            => _executor = definition.Executor;
        
        protected override object Execute(object input, ActionArgs args)
            => _executor(input as string, args.Record);

        
    }
}

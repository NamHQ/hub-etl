using Etl.Core.Extraction;
using Etl.Core.Utils;
using System;
using System.Text;

namespace Etl.Core.Transformation.Actions
{
    public class CSharpAction : TransformAction
    {
        public string Code { get; set; }
        private readonly Func<string, ExtractedRecord, object> _executor;

        public CSharpAction()
        {
            var namespaceName = "tempNameSpace";
            var className = "tempClass";
            var methodName = "tempMethod";
            var sb = new StringBuilder($"namespace {namespaceName} {{ class {className} {{");
            sb.Append($"public static object {methodName}(string V, ExtractedRecord R) {{ {Code};");
            sb.Append("}}}");

            var (method, instance) = CShapCompiler.BuildExecutor(sb.ToString(), namespaceName, className, methodName);
            _executor = (value, record) => method.Invoke(instance, new object[] { record, value });
        }


        public override ITransformActionInst CreateInstance(IServiceProvider sp)
        {
            var inst = new CSharpActionInst
            {
                Executor = _executor
            };
            return inst;
        }
    }

    public class CSharpActionInst : TransformActionInst<object>
    {
        public Func<string, ExtractedRecord, object> Executor;

        protected override object Execute(object input, ActionArgs args)
            => Executor(input as string, args.Record);
        
    }
}

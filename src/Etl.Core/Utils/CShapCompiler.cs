using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Etl.Core.Utils
{
    public static class CShapCompiler
    {
        private static readonly Lazy<(List<MetadataReference> refMeta, string refUsing)> REFERENCES = new(() => BuildRef());

        private static (List<MetadataReference> refMeta, string refUsing) BuildRef()
        {
            var refType = new HashSet<Type> { typeof(object), typeof(StringBuilder), typeof(List<object>), typeof(Console) };

            var locations = new HashSet<string>();
            var namespaces = new HashSet<string>();

            var refMeta = new List<MetadataReference>();
            var refUsing = new StringBuilder($"using {typeof(IDictionaryExtension).Namespace};");

            foreach (var e in refType)
            {
                if (locations.Add(e.Assembly.Location))
                    refMeta.Add(MetadataReference.CreateFromFile(e.Assembly.Location));

                if (namespaces.Add(e.Namespace))
                    refUsing.AppendLine($"using {e.Namespace};");
            }

            Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
                .ForEach(a => refMeta.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            return (refMeta, refUsing.ToString());
        }

        public static (MethodInfo method, object instance) BuildExecutor(string sourceCode,string namespaceName, string className, string methodName)
        {
            var assembly = Compile(sourceCode);
            var instance = assembly.CreateInstance($"{namespaceName}.{className}");
            var method = instance.GetType().GetMethod(methodName);

            return (method, instance);
        }

        public static Assembly Compile(string sourceCode)
        {
            using var codeStream = new MemoryStream();
            var result = GenerateCode(sourceCode).Emit(codeStream);

            if (!result.Success)
            {
                var sb = new StringBuilder();
                var errors = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (var e in errors)
                    sb.AppendLine($"{e.Id}: {e.GetMessage()}");

                throw new Exception($"Compile fail! {sb}");
            }

            codeStream.Seek(0, SeekOrigin.Begin);

            using var asemblyStream = new MemoryStream(codeStream.ToArray());
            return new AssemblyLoadContext(null).LoadFromStream(asemblyStream);
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var sb = new StringBuilder(REFERENCES.Value.refUsing);
            sb.AppendLine(sourceCode);

            var codeString = SourceText.From(sb.ToString());
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            return CSharpCompilation.Create("Dummy.dll",
                new[] { parsedSyntaxTree },
                references: REFERENCES.Value.refMeta,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }

    public static class IDictionaryExtension
    {
        public static string GetText(this IDictionary<string, object> record, string key, string defaultValue = null)
        {
            if (record != null && record.TryGetValue(key, out object raw))
                return raw is string text ? text : raw?.ToString();

            return defaultValue;
        }
    }
}

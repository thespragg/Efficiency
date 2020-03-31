using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using SandboxHelpers;
using server.Helpers;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        [HttpPost]
        public CompilerResponse Post(UserCode userCode)
        {
            var code = AppendHelpers(new StringBuilder(userCode.Code));
            return Compile(code);
        }

        public string AppendHelpers(StringBuilder code) {
            code.Remove(0, 1);
            code.Remove(code.Length - 1, 1);
            code.Insert(0, "using SandboxHelpers;");
            code.Replace("\\\"", @"""");
            var indexOfClass = code.ToString().IndexOf("SandBox") + 1;
            var insertionIndex = code.ToString().IndexOf("{", indexOfClass) + 1;
            code.Insert(insertionIndex, "public OutputHandler Output {get;set;} public SandBox(){Output = new OutputHandler();}");

            return code.ToString();
           // var index = code.IndexOf("{",;
        }

        public static CompilerResponse Compile(string code)
        {
            string assemblyName = "UserCode";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            var refPaths = new[] {
                typeof(Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(OutputHandler).GetTypeInfo().Assembly.Location), "SandboxHelpers.dll"),
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    List<string> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error).Select(x => x.ToString()).ToList();

                    return new CompilerResponse()
                    {
                        Status = "Error",
                        Errors = failures
                    };
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    var loader = new CustomAssemblyLoadContext();
                    Assembly assembly = loader.LoadFromStream(ms);
                    var type = assembly.GetType("CodeEnv.SandBox");
                    var instance = assembly.CreateInstance("CodeEnv.SandBox");
                    var prop = type.GetProperty("Output");
                    var meth = type.GetMember("Run").First() as MethodInfo;
                    meth.Invoke(instance, null);

                    var outputs = prop.GetValue(instance, null);

                    assembly = null;
                    loader.Unload();
                    return new CompilerResponse()
                    {
                        Status = "Success",
                        ConsoleLogs = (outputs as OutputHandler).GetLines()
                    };
                }
            }

        }
    }
}
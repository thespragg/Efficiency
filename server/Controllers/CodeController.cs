using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            userCode.Code = userCode.Code.Substring(1, userCode.Code.Length - 2);
            userCode.Code = "using SandboxHelpers;" + userCode.Code;
            AppendHelpers(userCode.Code);
            return Compile(userCode.Code);
        }

        public static void AppendHelpers(string code) {
            var index = code.IndexOf("{",code.IndexOf("SandBox")) + 1;

        }

        public static CompilerResponse Compile(string code)
        {
            string assemblyName = "UserCode";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            var refPaths = new[] {
                typeof(Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(Output).GetTypeInfo().Assembly.Location), "SandboxHelpers.dll"),
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
                    var type = assembly.GetType("CodeEnv.Test");
                    var instance = assembly.CreateInstance("CodeEnv.Test");
                    var meth = type.GetMember("Run").First() as MethodInfo;
                    meth.Invoke(instance, null);
                    assembly = null;
                    loader.Unload();
                    return new CompilerResponse()
                    {
                        Status = "Success"
                    };
                }
            }

        }
    }
}
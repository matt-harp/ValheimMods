using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using CodeCompiler = DevTools.CSharpCompiler.CodeCompiler;

namespace DevTools
{
    public class ScriptManager
    {
        public static void TryStart(Assembly assembly)
        {
            var typesWithMainMethod = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.Name.Equals("Main") && m.IsStatic))
                .ToList();
            if (typesWithMainMethod.Count != 1)
            {
                Console.instance.Print($"[ERROR] {assembly.FullName} contains no or more than one static entry point and cannot be loaded.");
                return;
            }
        
            var type = typesWithMainMethod.Single();
            var method = type.GetMethod("Main");
            method?.Invoke(null, new object[0]);
        }

        public static void TryStop(Assembly assembly)
        {
            var typesWithExitMethod = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.Name.Equals("Exit") && m.IsStatic))
                .ToList();
            if (typesWithExitMethod.Count > 1)
            {
                Console.instance.Print($"[ERROR] {assembly.FullName} contains more than one Exit method.");
                return;
            }
        
            var type = typesWithExitMethod.Single();
            var method = type.GetMethod("Exit");
            method?.Invoke(null, null);
        }

        private static IEnumerable<string> Crawl(string path)
        {
            var files = Directory.GetFiles(path)
                .Where(file => !file.Contains(@"\obj") && !file.Contains(@"\bin") && file.EndsWith(".cs"))
                .ToList();
            foreach (var directory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                files.AddRange(Crawl(directory));
            }

            return files.ToArray();
        }

        public static Assembly Compile(string[] files)
        {
            var outputName = Mathf.Abs(DateTime.Now.GetHashCode()) + ".dll";
            var options = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                OutputAssembly = outputName
            };

            options.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray());

            var compiler = new CodeCompiler();
            var result = compiler.CompileAssemblyFromFileBatch(options, files);
            if (result.Errors.Count > 0)
            {
                Console.instance.Print("There were compilation errors.");
                foreach (CompilerError resultError in result.Errors)
                {
                    Console.instance.Print($"[{resultError.Line}]: {resultError.ErrorText}");
                }
            }

            return result.CompiledAssembly;
        }
    }
}
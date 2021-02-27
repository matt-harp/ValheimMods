using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using CodeCompiler = DevTools.CSharpCompiler.CodeCompiler;

namespace DevTools
{
    public class DevTools
    {
        public static string LocalPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static ScriptManager ScriptManager;
        public static ILogger Logger;

        public static void Main(string[] args)
        {
            new Thread(() =>
            {
                while (AccessTools.Method(typeof(Console), "Print") == null || Console.instance == null)
                {
                    Thread.Sleep(10);
                }

                Logger = new ValheimLogger();

                Logger.Log("[DevTools] Startup...");
                Logger.Log("[DevTools] Loading libraries...");

                var path = Path.Combine(LocalPath, "lib");
                foreach (var file in Directory.GetFiles(path, "*.dll"))
                {
                    try
                    {
                        Assembly.LoadFile(file);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message);
                        Logger.Log(e.StackTrace);
                    }

                    Logger.Log($" - Loaded {file}");
                }

                Logger.Log("[DevTools] Applying patches...");
                try
                {
                    var harmony = new Harmony(nameof(DevTools));
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                Logger.Log("[DevTools] Loaded!");
            }).Start();
        }

        public static void CSRun(string input)
        {
            var @class = "CSRUN" + Mathf.Abs(DateTime.Now.GetHashCode());
            var code = @"
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Text;
                            using UnityEngine;
                            using HarmonyLib;
                            using System.Reflection;
                            using System.Threading;
                            public class %CLASS%
                            {
                                public static void Run()
                                {
                                    try
                                    {
                                        %CODE%
                                    }
                                    catch (Exception e)
                                    {
                                        Console.instance.Print(e.Message);
                                    }
                                } 
                            }
                        ".Replace("%CLASS%", @class)
                .Replace("%CODE%", input);
            try
            {
                var options = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };

                options.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(a => a.Location)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray());

                var compiler = new CodeCompiler();
                var result = compiler.CompileAssemblyFromSource(options, code);

                if (result.Errors.Count > 0)
                {
                    Logger.Log("There were compilation errors.");
                    foreach (CompilerError resultError in result.Errors)
                    {
                        Logger.Log($"[{resultError.Line}]: {resultError.ErrorText}");
                    }
                }
                else
                {
                    Logger.Log("Compilation success.");
                    try
                    {
                        result.CompiledAssembly.GetType(@class)?.GetMethod("Run")?.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("An exception occured: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(Console), "InputText")]
    static class PatchConsole
    {
        static void Postfix(InputField ___m_input)
        {
            Console.instance.m_input.characterLimit = 4096;
            var text = ___m_input.text;
            if (text.StartsWith("csrun "))
            {
                var input = text.Substring("csrun ".Length - 1);
                DevTools.CSRun(input);
            }

            if (text.StartsWith("load "))
            {
                var path = Path.Combine(DevTools.LocalPath, text.Substring("load ".Length));
                if (!File.Exists(path))
                {
                    Console.instance.Print("That path isnt valid");
                    return;
                }

                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(path));
                    ScriptManager.TryStart(assembly);
                }
                catch (Exception e)
                {
                    Console.instance.Print(e.Message);
                }
            }
        }
    }
}
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using Mono.CSharp;

/*
 * Taken from https://github.com/aeroson/mcs-ICodeCompiler
 */

namespace DevTools.CSharpCompiler
{
    //
    // Summary:
    //     Defines an interface for invoking compilation of source code or a CodeDOM tree
    //     using a specific compiler.
    public class CodeCompiler// : ICodeCompiler
    {
        private static long _assemblyCounter;

        // Summary:
        //     Compiles an assembly from the source code contained within the specified file,
        //     using the specified compiler settings.
        //
        // Parameters:
        //   options:
        //     A System.CodeDom.Compiler.CompilerParameters object that indicates the settings
        //     for compilation.
        //
        //   fileName:
        //     The file name of the file that contains the source code to compile.
        //
        // Returns:
        //     A System.CodeDom.Compiler.CompilerResults object that indicates the results of
        //     compilation.
        public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName, AppDomain domain = null)
        {
            return CompileAssemblyFromFileBatch(options, new[] { fileName }, domain);
        }

        // Summary:
        //     Compiles an assembly from the source code contained within the specified files,
        //     using the specified compiler settings.
        //
        // Parameters:
        //   options:
        //     A System.CodeDom.Compiler.CompilerParameters object that indicates the settings
        //     for compilation.
        //
        //   fileNames:
        //     The file names of the files to compile.
        //
        // Returns:
        //     A System.CodeDom.Compiler.CompilerResults object that indicates the results of
        //     compilation.
        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames, AppDomain domain = null)
        {
            var settings = ParamsToSettings(options);

            foreach (var fileName in fileNames)
            {
                string path = Path.GetFullPath(fileName);
                var unit = new SourceFile(fileName, path, settings.SourceFiles.Count + 1);
                settings.SourceFiles.Add(unit);
            }

            return CompileFromCompilerSettings(settings, options.GenerateInMemory, domain);
        }



        // Summary:
        //     Compiles an assembly from the specified string containing source code, using
        //     the specified compiler settings.
        //
        // Parameters:
        //   options:
        //     A System.CodeDom.Compiler.CompilerParameters object that indicates the settings
        //     for compilation.
        //
        //   source:
        //     The source code to compile.
        //
        // Returns:
        //     A System.CodeDom.Compiler.CompilerResults object that indicates the results of
        //     compilation.
        public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source, AppDomain domain = null)
        {
            return CompileAssemblyFromSourceBatch(options, new[] { source }, domain);
        }

        // Summary:
        //     Compiles an assembly from the specified array of strings containing source code,
        //     using the specified compiler settings.
        //
        // Parameters:
        //   options:
        //     A System.CodeDom.Compiler.CompilerParameters object that indicates the settings
        //     for compilation.
        //
        //   sources:
        //     The source code strings to compile.
        //
        // Returns:
        //     A System.CodeDom.Compiler.CompilerResults object that indicates the results of
        //     compilation.
        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources, AppDomain domain = null)
        {
            var settings = ParamsToSettings(options);

            int i = 0;
            foreach (var source in sources)
            {
                Func<Stream> getStream = () => new MemoryStream(Encoding.UTF8.GetBytes(source ?? ""));
                var fileName = i.ToString();
                var unit = new SourceFile(fileName, fileName, settings.SourceFiles.Count + 1, getStream);
                settings.SourceFiles.Add(unit);
                i++;
            }

            return CompileFromCompilerSettings(settings, options.GenerateInMemory, domain);
        }


        CompilerResults CompileFromCompilerSettings(CompilerSettings settings, bool generateInMemory, AppDomain domain = null)
        {
            domain = domain ?? AppDomain.CurrentDomain;
            var compilerResults = new CompilerResults(new TempFileCollection(Path.GetTempPath()));
            var driver = new CustomDynamicDriver(new CompilerContext(settings, new CustomReportPrinter(compilerResults)));

            AssemblyBuilder outAssembly = null;
            try
            {
                driver.Compile(out outAssembly, domain, generateInMemory);
            }
            catch (Exception e)
            {
                compilerResults.Errors.Add(new CompilerError()
                {
                    IsWarning = false,
                    ErrorText = e.Message,
                });
            }
            compilerResults.CompiledAssembly = outAssembly;

            return compilerResults;
        }


        CompilerSettings ParamsToSettings(CompilerParameters parameters)
        {
            var settings = new CompilerSettings();


            foreach (var assembly in parameters.ReferencedAssemblies) settings.AssemblyReferences.Add(assembly);

            //settings.AssemblyReferencesAliases;
            //settings.BreakOnInternalError
            //settings.Checked;
            //settings.DebugFlags;
            //settings.DocumentationFile;
            settings.Encoding = Encoding.UTF8;
            //settings.EnhancedWarnings;
            //settings.FatalCounter;

            settings.GenerateDebugInfo = parameters.IncludeDebugInformation;
            //settings.GetResourceStrings;
            //settings.LoadDefaultReferences;

            settings.MainClass = parameters.MainClass;
            //settings.Modules;
            //settings.Optimize;


            //settings.ParseOnly;
            settings.Platform = Platform.AnyCPU;
            /*
            settings.ReferencesLookupPaths;
            settings.Resources;
            settings.RuntimeMetadataVersion;
            settings.SdkVersion;
            settings.ShowFullPaths;
            settings.Stacktrace;
            settings.StatementMode;
            settings.StdLib;
            */
            settings.StdLibRuntimeVersion = RuntimeVersion.v4;
            /*
            settings.StrongNameDelaySign;
            settings.StrongNameKeyContainer;
            settings.StrongNameKeyFile;
            settings.TabSize;
            */
            if (parameters.GenerateExecutable)
            {
                settings.Target = Target.Exe;
                settings.TargetExt = ".exe";
            }
            else
            {
                settings.Target = Target.Library;
                settings.TargetExt = ".dll";
            }
            if (parameters.GenerateInMemory) settings.Target = Target.Library;

            if (string.IsNullOrEmpty(parameters.OutputAssembly))
            {
                parameters.OutputAssembly = settings.OutputFile = "DynamicAssembly_" + _assemblyCounter + settings.TargetExt;
                _assemblyCounter++;
            }
            settings.OutputFile = parameters.OutputAssembly; // if it is not being outputted, we use this to set name of the dynamic assembly

            /*
            settings.Timestamps;
            settings.TokenizeOnly;
            settings.Unsafe;
            settings.VerboseParserFlag;
            settings.VerifyClsCompliance;
            */
            settings.Version = LanguageVersion.Default;
            settings.WarningLevel = parameters.WarningLevel;
            settings.WarningsAreErrors = parameters.TreatWarningsAsErrors;
            /*
            settings.Win32IconFile;
            settings.Win32ResourceFile;
            settings.WriteMetadataOnly;
            */

            return settings;
        }
    }
}
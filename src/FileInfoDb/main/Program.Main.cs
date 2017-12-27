using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Config;
using Grynwald.Utilities.Squirrel;
using Grynwald.Utilities.Squirrel.Installation;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace FileInfoDb
{

    partial class Program
    { 
        static int Main(string[] args)
        {            
            LaunchDebugger(args);
            
            InstallerBuilder.CreateConsoleApplicationBuilder()       
                .SaveResourceToFile(Assembly.GetExecutingAssembly(), "FileInfoDb.config.json", SpecialDirectory.ApplicationRootDirectory, Configuration.ConfigFileName, overwriteOnUpdate: false)               
                .Build()
                .HandleInstallationEvents();

            var parser = new Parser(settings =>
            {
                settings.IgnoreUnknownArguments = true;
                settings.HelpWriter = null;
            });                       
            var verbose = parser
                .ParseArguments<BaseArgs>(args)
                .MapResult(
                    (BaseArgs opts) => opts.Verbose,
                    errs => false
                );
                
            var loggerFactory = new LoggerFactory();
            if (verbose)
                loggerFactory.AddConsole(LogLevel.Information);
            
            using (new ExecutionTimeLogger())
            using (var updater = new Updater(Configuration.Current.UpdateOptions, loggerFactory.CreateLogger<Updater>()))
            {
                updater.Start();
                var program = new Program(loggerFactory.CreateLogger<Program>());
                return program.Run(args);
            }
        }

        [Conditional("DEBUG")]
        private static void LaunchDebugger(string[] args)
        {
            if (args.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, "--debug")))
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Debugger.Launch();
                }
            }
        }       
    }
}

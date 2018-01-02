using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Config;
using Grynwald.Utilities.Squirrel;
using Grynwald.Utilities.Squirrel.Installation;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Logging;

namespace FileInfoDb
{

    partial class Program
    { 
        static int Main(string[] args)
        {            
            // launch debugger if --debug option was specified
            LaunchDebugger(args);
            
            // run installer (application is invoked by Squirrel on installation/updates and deinstallation)
            InstallerBuilder.CreateConsoleApplicationBuilder()       
                .SaveResourceToFile(Assembly.GetExecutingAssembly(), "FileInfoDb.config.json", SpecialDirectory.ApplicationRootDirectory, Configuration.ConfigFileName, overwriteOnUpdate: false)               
                .Build()
                .HandleInstallationEvents();

            // determine if verbose option was specfified
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
                
            // set up logger (log to console when verbose option is enabled)
            var loggerFactory = new LoggerFactory();
            if (verbose)
            {
                loggerFactory.AddConsole(LogLevel.Information);
            }

            // load configuration
            var configuration = Configuration.Load(loggerFactory);

            // run application while checking for updates in the background
            using (verbose ? (IDisposable) new ExecutionTimeLogger() : NullDisposable.Instance)
            using (var updater = new Updater(configuration.UpdateOptions, loggerFactory.CreateLogger<Updater>()))
            {
                updater.Start();
                var program = new Program(loggerFactory.CreateLogger<Program>(), loggerFactory, configuration);
                return program.Run(args);
            }
        }

        /// <summary>
        /// Launches the debugger if the --debug swicth was specified on commandline.
        /// If a debugger is already attached, a breakpoint is signaled
        /// </summary>
        /// <remarks>
        /// Mathod is only called in debug builds
        /// </remarks>
        [Conditional("DEBUG")]
        static void LaunchDebugger(string[] args)
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

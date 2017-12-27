using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;
using Grynwald.Utilities.Squirrel;
using Grynwald.Utilities.Squirrel.Installation;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Logging.Abstractions;
using FileInfoDb.Config;
using FileInfoDb.Core.Hashing.Cache;

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

            using (new ExecutionTimeLogger())
            using (var updater = new Updater(Configuration.Current.UpdateOptions))
            {
                updater.Start();
                var program = new Program();
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

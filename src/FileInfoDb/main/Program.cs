using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;
using FileInfoDb.Options;
using Grynwald.Utilities.Squirrel;
using Grynwald.Utilities.Squirrel.Installation;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileInfoDb
{
    
    class Program
    {
        static IHashProvider s_HashProvider;

        static int Main(string[] args)
        {            
            LaunchDebugger(args);
            
            InstallerBuilder.CreateConsoleApplicationBuilder()       
                .SaveResourceToFile(Assembly.GetExecutingAssembly(), "FileInfoDb.config.json", SpecialDirectory.ApplicationRootDirectory, ConfigFileNames.ApplicationConfigFile, overwriteOnUpdate: false)               
                .Build()
                .HandleInstallationEvents();

            using (new ExecutionTimeLogger())
            using (var updater = new Updater(new UpdateOptions()))
            {
                updater.Start();
                return Run(args);                
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

        static int Run(string[] args) 
        {

            // Setup
            //var cacheDb = new Database("cache.db");
            //var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
            //IHashProvider hashProvider = new CachingHashProvider(cache, new SHA256HashProvider());
            s_HashProvider = new SHA256HashProvider();


            // Run Command
            var parsed = Parser.Default.ParseArguments<InitOptions, SetPropertyOptions, GetPropertyOptions>(args);
            return parsed.MapResult(
                (Func<InitOptions, int>)Init,
                (Func<SetPropertyOptions, int>)SetProperty,
                (Func<GetPropertyOptions, int>)GetProperty,
                (IEnumerable<Error> errors) =>
                {
                    Console.WriteLine("Invalid arguments.");
                    return -1;
                }
            );

            
        }

        static int Init(InitOptions opts)
        {
            var db = GetDatabase(opts);
            db.Create();
            return 0;
        }

        static int SetProperty(SetPropertyOptions opts)
        {
            var hashedFile = s_HashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            propertyStorage.SetProperty(hashedFile.Hash, new Property(opts.Name, opts.Value));

            return 0;
        }

        static int GetProperty(GetPropertyOptions opts)
        {
            var hashedFile = s_HashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            var properties = propertyStorage.GetProperties(hashedFile.Hash);

            foreach(var property in properties)
            {
                Console.WriteLine(property);
            }

            return 0;
        }


        static PropertiesDatabase GetDatabase(OptionsBase opts) => 
            new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, new Uri(opts.DatabaseUri));
    }
}

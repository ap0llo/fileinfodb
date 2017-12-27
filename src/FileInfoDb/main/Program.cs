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

    class Program
    {
        static IHashProvider s_HashProvider;

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
            s_HashProvider = new SHA256HashProvider();

            var hashingOptions = Configuration.Current.HashingOptions;
            if(hashingOptions.EnableCache)
            {
                var cacheDb = new CacheDatabase(hashingOptions.CachePath);
                var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
                s_HashProvider = new CachingHashProvider(cache, s_HashProvider);
            }

            
            // Run Command
            var parsed = Parser.Default.ParseArguments<InitArgs, SetPropertyArgs, GetPropertyArgs>(args);
            return parsed.MapResult(
                (Func<InitArgs, int>)Init,
                (Func<SetPropertyArgs, int>)SetProperty,
                (Func<GetPropertyArgs, int>)GetProperty,
                (IEnumerable<Error> errors) =>
                {
                    Console.WriteLine("Invalid arguments.");
                    return -1;
                }
            );

            
        }

        static int Init(InitArgs opts)
        {
            var db = GetDatabase(opts);
            db.Create();
            return 0;
        }

        static int SetProperty(SetPropertyArgs opts)
        {
            var hashedFile = s_HashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            propertyStorage.SetProperty(hashedFile.Hash, new Property(opts.Name, opts.Value));

            return 0;
        }

        static int GetProperty(GetPropertyArgs opts)
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


        static PropertiesDatabase GetDatabase(ArgsBase opts) => 
            new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, new Uri(opts.DatabaseUri));
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;
using FileInfoDb.Core.Hashing.Cache;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileInfoDb
{
    
    class Program
    {
        static IHashProvider s_HashProvider;

        static int Main(string[] args)
        {
        #if DEBUG

            if(args.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, "--debug")))
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


            var stopwatch = new Stopwatch();
            stopwatch.Start();
        #endif


            // Setup
            //var cacheDb = new Database("cache.db");
            //var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
            //IHashProvider hashProvider = new CachingHashProvider(cache, new SHA256HashProvider());
            s_HashProvider = new SHA256HashProvider();


            // Run Command
            var parsed = Parser.Default.ParseArguments<InitOptions, SetPropertyOptions, GetPropertyOptions>(args);
            var exitCode = parsed.MapResult(
                (Func<InitOptions, int>)Init,
                (Func<SetPropertyOptions, int>)SetProperty,
                (Func<GetPropertyOptions, int>)GetProperty,
                (IEnumerable<Error> errors) => 
                {                 
                    Console.WriteLine("Invalid arguments.");
                    return -1;
                }
            );
            

        #if DEBUG   
            stopwatch.Stop();
            Console.WriteLine($"Completed, Elapsed time {stopwatch.Elapsed}");

            if(Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        #endif

            return exitCode;
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

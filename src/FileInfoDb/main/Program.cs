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
        IHashProvider m_HashProvider;
        
        public int Run(string[] args) 
        {
            // Setup
            m_HashProvider = new SHA256HashProvider();

            var hashingOptions = Configuration.Current.HashingOptions;
            if(hashingOptions.EnableCache)
            {
                var cacheDb = new CacheDatabase(hashingOptions.CachePath);
                var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
                m_HashProvider = new CachingHashProvider(cache, m_HashProvider);
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

        int Init(InitArgs opts)
        {
            var db = GetDatabase(opts);
            db.Create();
            return 0;
        }

        int SetProperty(SetPropertyArgs opts)
        {
            var hashedFile = m_HashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            propertyStorage.SetProperty(hashedFile.Hash, new Property(opts.Name, opts.Value));

            return 0;
        }

        int GetProperty(GetPropertyArgs opts)
        {
            var hashedFile = m_HashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            var properties = propertyStorage.GetProperties(hashedFile.Hash);

            foreach(var property in properties)
            {
                Console.WriteLine(property);
            }

            return 0;
        }


        PropertiesDatabase GetDatabase(ArgsBase opts) => 
            new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, new Uri(opts.DatabaseUri));
    }
}

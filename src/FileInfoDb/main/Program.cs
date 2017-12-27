using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Config;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;
using FileInfoDb.Core.Hashing.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace FileInfoDb
{

    partial class Program
    {
        readonly ILogger<Program> m_Logger;


        public Program(ILogger<Program> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public int Run(string[] args) =>
            Parser.Default
                .ParseArguments<InitArgs, SetPropertyArgs, GetPropertyArgs>(args)
                .MapResult(
                    (Func<InitArgs, int>)Init,
                    (Func<SetPropertyArgs, int>)SetProperty,
                    (Func<GetPropertyArgs, int>)GetProperty,
                    (IEnumerable<Error> errors) =>
                    {
                        Console.WriteLine("Invalid arguments.");
                        return -1;
                    }
                );


        int Init(InitArgs opts)
        {
            m_Logger.LogInformation($"Running '{CommandNames.Init}' command");

            var db = GetDatabase(opts);
            db.Create();

            return 0;
        }

        int SetProperty(SetPropertyArgs opts)
        {
            m_Logger.LogInformation($"Running '{CommandNames.SetProperty}' command");

            var hashProvider = GetHashProvider();
            var hashedFile = hashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            var property = new Property(opts.Name, opts.Value);
            m_Logger.LogInformation($"Setting property '{property}' for file {hashedFile.Hash}");
            propertyStorage.SetProperty(hashedFile.Hash, property);

            return 0;
        }

        int GetProperty(GetPropertyArgs opts)
        {
            m_Logger.LogInformation($"Running '{CommandNames.GetProperty}' command");

            var hashProvider = GetHashProvider();
            var hashedFile = hashProvider.GetFileHash(opts.FilePath);
            var db = GetDatabase(opts);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            m_Logger.LogInformation($"Loading properties for file {hashedFile.Hash}");
            var properties = propertyStorage.GetProperties(hashedFile.Hash);

            foreach(var property in properties)
            {
                Console.WriteLine(property);
            }

            return 0;
        }

        PropertiesDatabase GetDatabase(DatabaseArgs opts)
        {
            var uri = new Uri(opts.DatabaseUri);
            m_Logger.LogInformation($"Using database '{uri.WithoutCredentials()}'");
            return new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, uri);
        }

        IHashProvider GetHashProvider()
        {
            // Setup
            IHashProvider provider = new SHA256HashProvider();

            var hashingOptions = Configuration.Current.HashingOptions;
            if (hashingOptions.EnableCache)
            {
                m_Logger.LogInformation($"Using hashing cache at '{hashingOptions.CachePath}'");

                var cacheDb = new CacheDatabase(hashingOptions.CachePath);
                var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
                provider = new CachingHashProvider(cache, provider);
            }

            return provider;
        }
    }
}

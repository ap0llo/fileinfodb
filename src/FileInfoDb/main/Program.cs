﻿using CommandLine;
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
        readonly LoggerFactory m_LoggerFactory;
        readonly Configuration m_Configuration;


        public Program(ILogger<Program> logger, LoggerFactory loggerFactory, Configuration configuration)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public int Run(string[] args)
        {
            try
            {
                return Parser.Default
                    .ParseArguments<InitArgs, SetPropertyArgs, GetPropertyArgs>(args)
                    .MapResult(
                        (Func<InitArgs, int>)Init,
                        (Func<SetPropertyArgs, int>)SetProperty,
                        (Func<GetPropertyArgs, int>)GetProperty,
                        (IEnumerable<Error> errors) =>
                        {
                            Console.Error.WriteLine("Invalid arguments.");
                            return -1;
                        });
            }
            catch (MissingConfigurationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
        }

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
            Uri uri;
            if(!String.IsNullOrEmpty(opts.DatabaseUri))
            {
                m_Logger.LogInformation("Using database uri from commandline arguments");
                uri = new Uri(opts.DatabaseUri);
            }
            else if(!String.IsNullOrEmpty(m_Configuration.DatabaseOptions.Uri))
            {
                m_Logger.LogInformation("Using database uri from configuration");
                uri = new Uri(m_Configuration.DatabaseOptions.Uri);
            }
            else
            {
                m_Logger.LogInformation("Could not determine database uri");
                throw new MissingConfigurationException("No database uri is configured and no value was specified as commandline parameter");
            }
            
            m_Logger.LogInformation($"Using database '{uri.WithoutCredentials()}'");
            return new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, uri);
        }

        IHashProvider GetHashProvider()
        {
            // Setup
            IHashProvider provider = new SHA256HashProvider(m_LoggerFactory.CreateLogger<SHA256HashProvider>());

            var hashingOptions = Configuration.Current.HashingOptions;
            if (hashingOptions.EnableCache)
            {
                m_Logger.LogInformation($"Using hashing cache at '{hashingOptions.CachePath}'");

                var cacheDb = new CacheDatabase(hashingOptions.CachePath);
                var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
                provider = new CachingHashProvider(m_LoggerFactory.CreateLogger<CachingHashProvider>(), cache, provider);
            }

            return provider;
        }
    }
}

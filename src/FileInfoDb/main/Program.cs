using CommandLine;
using FileInfoDb.Cli;
using FileInfoDb.Config;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;
using FileInfoDb.Core.Hashing.Cache;
using Grynwald.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    .ParseArguments<InitArgs, ConfigureArgs, SetPropertyArgs, GetPropertyArgs, GetPropertyNameArgs>(args)
                    .MapResult(
                        (Func<InitArgs, int>)Init,
                        (Func<ConfigureArgs, int>)Configure,
                        (Func<SetPropertyArgs, int>)SetProperty,
                        (Func<GetPropertyArgs, int>)GetProperty,
                        (Func<GetPropertyNameArgs, int>)GetPropertyName,
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


        int Init(InitArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.Init}' command");

            var db = GetDatabase(args);
            db.Create();

            return 0;
        }

        int Configure(ConfigureArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.Configure}' command");

            m_Configuration.SetDatabaseUri(args.DatabaseUri);
            return 0;                   
        }

        int SetProperty(SetPropertyArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.SetProperty}' command");

            var hashProvider = GetHashProvider();
            var hashedFile = hashProvider.GetFileHash(args.FilePath);
            var db = GetDatabase(args);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            var property = new Property(args.Name, args.Value);
            m_Logger.LogInformation($"Setting property '{property}' for file {hashedFile.Hash}");
            propertyStorage.SetProperty(hashedFile.Hash, property);

            return 0;
        }

        int GetProperty(GetPropertyArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.GetProperty}' command");

            var hashProvider = GetHashProvider();
            var hashedFile = hashProvider.GetFileHash(args.FilePath);
            var db = GetDatabase(args);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);

            m_Logger.LogInformation($"Loading properties for file {hashedFile.Hash}");
            var properties = propertyStorage.GetProperties(hashedFile.Hash);

            if(!String.IsNullOrEmpty(args.Name))
            {
                m_Logger.LogInformation($"Filtering properties using wildcard pattern '{args.Name}'");
                var wildcard = new Wildcard(args.Name);
                properties = properties.Where(p => wildcard.IsMatch(p.Name));
            }

            foreach(var property in properties)
            {
                Console.WriteLine(property);
            }

            return 0;
        }

        int GetPropertyName(GetPropertyNameArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.GetPropertyName}' command");

            var db = GetDatabase(args);
            var propertyStorage = new DatabaseBackedPropertyStorage(db);
            
            foreach(var name in propertyStorage.GetPropertyNames())
            {
                Console.WriteLine(name);
            }

            return 0;
        }

        PropertiesDatabase GetDatabase(DatabaseArgs args)
        {
            Uri uri;
            if(!String.IsNullOrEmpty(args.DatabaseUri))
            {
                m_Logger.LogInformation("Using database uri from commandline arguments");
                uri = new Uri(args.DatabaseUri);
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

            var hashingOptions = m_Configuration.HashingOptions;
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
    
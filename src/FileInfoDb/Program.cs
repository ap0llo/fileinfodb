using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Grynwald.Utilities;
using Grynwald.Utilities.Squirrel;
using Meziantou.Framework.Win32;
using Microsoft.Extensions.Logging;
using FileInfoDb.Cli;
using FileInfoDb.Config;
using FileInfoDb.Core;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Hashing;
using FileInfoDb.Hashing.Cache;

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
                var parser = new Parser(opts =>
                {
                    opts.CaseInsensitiveEnumValues = true;
                    opts.CaseSensitive = false;
                    opts.HelpWriter = Console.Out;
                });

                return parser
                    .ParseArguments<InitArgs, ConfigureArgs, SetPropertyArgs, GetPropertyArgs, GetPropertyNameArgs, IndexArgs>(args)                    
                    .MapResult(
                        (Func<InitArgs, int>)Init,
                        (Func<ConfigureArgs, int>)Configure,
                        (Func<SetPropertyArgs, int>)SetProperty,
                        (Func<GetPropertyArgs, int>)GetProperty,
                        (Func<GetPropertyNameArgs, int>)GetPropertyName,
                        (Func<IndexArgs, int>)Index,
                        (IEnumerable<Error> errors) =>
                        {
                            if (errors.All(e => e is HelpRequestedError || e is HelpVerbRequestedError || e is VersionRequestedError))
                            {
                                return 0;
                            }
                            else
                            {
                                Console.Error.WriteLine("Invalid arguments");
                                return -1;
                            }
                        });
            }            
            catch (Exception ex) when (ex is ExecutionErrorException || ex is DatabaseAccessDeniedException)
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


            if (!Uri.IsWellFormedUriString(args.DatabaseUri, UriKind.Absolute))
            {
                Console.Error.WriteLine("Specified value is not a valid URI");
                return -1;
            }

            var uri = new Uri(args.DatabaseUri);
            if (!uri.IsValidFileInfoDbMySqlUri(out var error))
            {
                Console.Error.WriteLine(error);
                return -1;
            }

            
            // if prompt-for-credentials switch was specified, read credentials from console
            // and add credentials to uri (this will also overwrite other credentials if they are part of the uri)
            if (args.PromptForCredentials)
            {
                Console.Write("User name: ");
                var userName = Console.ReadLine();

                Console.Write($"Password for user '{userName}': ");
                var password = ConsoleHelper.ReadPassword();
                
                uri = uri.WithCredentials(userName, password);
            }

            // check if uri contains a password
            // - if the uri contains a password, username and password are stored using the Windows Credential Manager,
            //   the uri without username or password is stored in config            
            // - if the uri contains no password, the uri (with a optional username) is stored in config
            //   (Windows Credential Manager (or the wrapper library) cannot store "null"-Passwords)
            if (uri.HasPassword())
            {
                m_Logger.LogInformation("Saving username and password to Windows Credential Manager");
                CredentialManager.WriteCredential(ApplicationInfo.ApplicationName, uri.GetUserName(), uri.GetPassword(), CredentialPersistence.LocalMachine);

                m_Configuration.DatabaseOptions = new DatabaseOptions()
                {
                    Uri = uri.WithoutCredentials().ToString(),
                    CredentialSource = CredentialSource.WindowsCredentialManager
                };
            }
            else
            {
                // delete credential from Credential Manager if it exists
                if (CredentialManager.EnumerateCrendentials().Any(c => c.ApplicationName.Equals(ApplicationInfo.ApplicationName)))
                {
                    m_Logger.LogInformation("Uri does not contain a password, deleting corresponding entry from Windows Credential Manager");
                    CredentialManager.DeleteCredential(ApplicationInfo.ApplicationName);
                }
                
                m_Configuration.DatabaseOptions = new DatabaseOptions()
                {
                    Uri = uri.ToString(),
                    CredentialSource = CredentialSource.None
                };
            }

            return 0;                   
        }

        int SetProperty(SetPropertyArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.SetProperty}' command");

            var hashedFile = GetHashProvider().GetFileHash(args.FilePath);            
            var propertyStorage = new DatabaseBackedPropertyStorage(GetDatabase(args));


            // determine property value (either use value supplied on commandline or load from specified file)
            string value;
            if(String.IsNullOrEmpty(args.Value))
            {
                m_Logger.LogInformation($"Using value from file '{args.ValueFromFile}'");
                value = File.ReadAllText(args.ValueFromFile);
            }
            else
            {
                m_Logger.LogInformation("Using property value from commandline arguments");
                value = args.Value;
            }

            var property = new Property(args.Name, value);
            m_Logger.LogInformation($"Setting property '{property.Name}' for file {hashedFile.Hash}");

            // try to save the property
            try
            {
                propertyStorage.SetProperty(hashedFile.Hash, property, args.Overwrite);
            }
            catch (PropertyAlreadyExistsException)
            {
                // ignore PropertyAlreadyExistsException if the value already in the database
                // equals the value being set
                var existingProperty = propertyStorage.GetProperty(hashedFile.Hash, property.Name);
                if(property.Equals(existingProperty))
                {
                    return 0;
                }

                Console.Error.WriteLine($"Property '{property.Name}' has already been set for the file, use --overwrite to replace");
                return -1;
            }

            return 0;
        }

        int GetProperty(GetPropertyArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.GetProperty}' command");
            
            var hashedFile = GetHashProvider().GetFileHash(args.FilePath);            
            var propertyStorage = new DatabaseBackedPropertyStorage(GetDatabase(args));

            // load properties
            m_Logger.LogInformation($"Loading properties for file {hashedFile.Hash}");
            var properties = propertyStorage.GetProperties(hashedFile.Hash);

            // filter properties if a name was specified
            if(!String.IsNullOrEmpty(args.Name))
            {
                m_Logger.LogInformation($"Filtering properties using wildcard pattern '{args.Name}'");
                var wildcard = new Wildcard(args.Name);
                properties = properties.Where(p => wildcard.IsMatch(p.Name));
            }

            // write properties to console
            foreach(var property in properties)
            {
                Console.WriteLine(property);
            }

            return 0;
        }

        int GetPropertyName(GetPropertyNameArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.GetPropertyName}' command");
            
            // get property names and write to console
            var propertyStorage = new DatabaseBackedPropertyStorage(GetDatabase(args));
            foreach(var name in propertyStorage.GetPropertyNames())
            {
                Console.WriteLine(name);
            }

            return 0;
        }

        int Index(IndexArgs args)
        {
            m_Logger.LogInformation($"Running '{CommandNames.Index}' command");

            // ensure cache is enabled
            var hashingOptions = m_Configuration.HashingOptions;
            if (!hashingOptions.EnableCache)
            {
                var message = "Cannot index directory because caching of hashes is disabled in the configuration file";
                m_Logger.LogError(message);
                throw new ExecutionErrorException(message);
            }

            // ensure directory exists
            if (!Directory.Exists(args.DirectoryPath))
            {
                var message = $"Directory '{args.DirectoryPath}' does not exist";
                m_Logger.LogError(message);
                throw new ExecutionErrorException(message);
            }
            
            

            // determine number of threads to use for indexing (default 1)
            var degreeOfParallelism = 1;
            if(args.Parallel)
            {                
                if(args.ThreadCount != 0)
                {
                    if(args.ThreadCount < 1)
                    {
                        var message = $"Number of threads must be at least 1.";
                        m_Logger.LogError(message);
                        throw new ExecutionErrorException(message);
                    }

                    degreeOfParallelism = args.ThreadCount;
                }
                else
                {
                    degreeOfParallelism = Environment.ProcessorCount;
                }
            }

            m_Logger.LogInformation($"Using {degreeOfParallelism} thread(s) for indexing");

            // index all the files
            var hashProvider = GetHashProvider();
            Directory.EnumerateFiles(args.DirectoryPath, "*.*", args.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .AsParallel()
                .WithDegreeOfParallelism(degreeOfParallelism)
                .ForAll(file =>
                {
                    m_Logger.LogInformation($"Indexing '{file}'");

                    //TODO: Make writing/updating the cache more explicit 
                    // (currently updating the cache is a side-effect of calling GetFileHash)
                    var hash = hashProvider.GetFileHash(file);
                });


            return 0;
        }

        PropertiesDatabase GetDatabase(DatabaseArgs args)
        {
            // if uri was specified as commandline parameter, use this uri, otherwise get uri from settings
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

                // if uri requires authentication, get username and password from Windows Credential Manager
                if (m_Configuration.DatabaseOptions.CredentialSource == CredentialSource.WindowsCredentialManager)
                {
                    m_Logger.LogInformation("Loading credentials from Windows Credentials manager");
                    var cred = CredentialManager.ReadCredential(ApplicationInfo.ApplicationName);
                    if (cred == null)
                    {
                        throw new ExecutionErrorException($"Failed to load credentials for database connection. Use the '{CommandNames.Configure}' command to set up database connection");
                    }

                    uri = uri.WithCredentials(cred.UserName, cred.Password);
                }
            }
            else
            {
                m_Logger.LogInformation("Could not determine database uri");
                throw new ExecutionErrorException("No database uri is configured and no value was specified as commandline parameter");
            }
            
            m_Logger.LogInformation($"Using database '{uri.WithoutCredentials()}'");
            return new MySqlPropertiesDatabase(m_LoggerFactory.CreateLogger<MySqlPropertiesDatabase>(), uri);
        }

        IHashProvider GetHashProvider()
        {
            // initialize hash provider
            IHashProvider provider = new SHA256HashProvider(m_LoggerFactory.CreateLogger<SHA256HashProvider>());

            // if enabled in settings, add cache
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
    
using System;
using System.IO;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileInfoDb.Config
{
    class Configuration
    {
        const string s_UpdateOptionsSectionName = "Update";
        const string s_HashingOptionsSectionName = "Hashing";
        const string s_DatabaseOptionsSectionName = "Database";

        readonly LoggerFactory m_LoggerFactory;
        readonly ILogger m_Logger;
        DatabaseOptions m_DatabaseOptions;


        public const string ConfigFileName = "config.json";

        public const string DebugConfigFileName = "config.Debug.json";

        public UpdateOptions UpdateOptions { get; }

        public HashingOptions HashingOptions { get; }

        public DatabaseOptions DatabaseOptions
        {
            get => m_DatabaseOptions;
            set
            {
                var writer = new ConfigurationWriter(
                    m_LoggerFactory.CreateLogger<ConfigurationWriter>(),
                    Path.Combine(ApplicationDirectories.RoamingAppData, ConfigFileName));

                m_Logger.LogInformation("Saving databse options");
                writer.SetValue($"{s_DatabaseOptionsSectionName}:{nameof(DatabaseOptions.Uri)}", value.Uri);
                writer.SetValue($"{s_DatabaseOptionsSectionName}:{nameof(DatabaseOptions.CredentialSource)}", value.CredentialSource.ToString());

                m_DatabaseOptions = value;
            }
        }


        private Configuration(LoggerFactory loggerFactory, ILogger logger, UpdateOptions updateOptions, HashingOptions hashingOptions, DatabaseOptions databaseOptions)
        {
            m_LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            UpdateOptions = updateOptions ?? throw new ArgumentNullException(nameof(updateOptions));
            HashingOptions = hashingOptions ?? throw new ArgumentNullException(nameof(hashingOptions));
            m_DatabaseOptions = databaseOptions ?? throw new ArgumentNullException(nameof(databaseOptions));
        }

        
        public static Configuration Load(LoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Configuration>();

            logger.LogInformation($"Loading configuration sections '{s_UpdateOptionsSectionName}' and '{s_HashingOptionsSectionName}' from '{ApplicationDirectories.InstallationDirectory}'");
            var installationRoot = new ConfigurationBuilder()                
                .AddJsonFile(Path.Combine(ApplicationDirectories.InstallationDirectory, ConfigFileName), true)
                .AddJsonFile(Path.Combine(ApplicationDirectories.InstallationDirectory, DebugConfigFileName), true)
                .Build();

            var updateOptions = installationRoot.GetSection(s_UpdateOptionsSectionName)?.Get<UpdateOptions>() ?? new UpdateOptions();
            var hashingOptions = installationRoot.GetSection(s_HashingOptionsSectionName)?.Get<HashingOptions>() ?? new HashingOptions();


            logger.LogInformation($"Loading configuration section '{s_DatabaseOptionsSectionName}' from '{ApplicationDirectories.RoamingAppData}'");
            var appDataRoot = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationDirectories.RoamingAppData, ConfigFileName), true)
                .AddJsonFile(Path.Combine(ApplicationDirectories.RoamingAppData, DebugConfigFileName), true)
                .Build();

            var databaseOptions = appDataRoot.GetSection(s_DatabaseOptionsSectionName)?.Get<DatabaseOptions>() ?? new DatabaseOptions();

            return new Configuration(loggerFactory, logger, updateOptions, hashingOptions, databaseOptions);
        }
    }
}

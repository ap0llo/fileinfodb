using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace FileInfoDb.Config
{
    class Configuration
    {
        const string s_UpdateOptionsSectionName = "Update";
        const string s_HashingOptionsSectionName = "Hashing";
        const string s_DatabaseOptionsSectionName = "Database";

        readonly LoggerFactory m_LoggerFactory;


        public const string ConfigFileName = "config.json";

        public const string DebugConfigFileName = "config.Debug.json";

        public UpdateOptions UpdateOptions { get; }

        public HashingOptions HashingOptions { get; }
        
        public DatabaseOptions DatabaseOptions { get; }


        private Configuration(LoggerFactory loggerFactory, UpdateOptions updateOptions, HashingOptions hashingOptions, DatabaseOptions databaseOptions)
        {
            m_LoggerFactory = loggerFactory ?? throw new System.ArgumentNullException(nameof(loggerFactory));
            UpdateOptions = updateOptions ?? throw new System.ArgumentNullException(nameof(updateOptions));
            HashingOptions = hashingOptions ?? throw new System.ArgumentNullException(nameof(hashingOptions));
            DatabaseOptions = databaseOptions ?? throw new System.ArgumentNullException(nameof(databaseOptions));
        }


        public void SetDatabaseUri(string value)
        {
            var writer = new ConfigurationWriter(
                m_LoggerFactory.CreateLogger<ConfigurationWriter>(),
                Path.Combine(ApplicationDirectories.RoamingAppData, ConfigFileName));

            writer.SetValue($"{s_DatabaseOptionsSectionName}:{nameof(DatabaseOptions.Uri)}", value);
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

            return new Configuration(loggerFactory, updateOptions, hashingOptions, databaseOptions);
        }
    }
}

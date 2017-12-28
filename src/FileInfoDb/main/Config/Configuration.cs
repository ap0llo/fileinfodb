using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FileInfoDb.Config
{
    class Configuration
    {
        const string s_UpdateOptionsSectionName = "Update";
        const string s_HashingOptionsSectionName = "Hashing";
        const string s_DatabaseOptionsSectionName = "Database";


        public const string ConfigFileName = "config.json";

        public const string DebugConfigFileName = "config.Debug.json";


        public UpdateOptions UpdateOptions { get; }

        public HashingOptions HashingOptions { get; }
        
        public DatabaseOptions DatabaseOptions { get; }


        private Configuration(UpdateOptions updateOptions, HashingOptions hashingOptions, DatabaseOptions databaseOptions)
        {
            UpdateOptions = updateOptions ?? throw new System.ArgumentNullException(nameof(updateOptions));
            HashingOptions = hashingOptions ?? throw new System.ArgumentNullException(nameof(hashingOptions));
            DatabaseOptions = databaseOptions ?? throw new System.ArgumentNullException(nameof(databaseOptions));
        }


        public static Configuration Current { get; }

        static Configuration()
        {
            var installationRoot = new ConfigurationBuilder()                
                .AddJsonFile(Path.Combine(ApplicationDirectories.InstallationDirectory, ConfigFileName), true)
                .AddJsonFile(Path.Combine(ApplicationDirectories.InstallationDirectory, DebugConfigFileName), true)
                .Build();

            var appDataRoot = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationDirectories.RoamingAppData, ConfigFileName), true)
                .Build();

            var updateOptions = installationRoot.GetSection(s_UpdateOptionsSectionName)?.Get<UpdateOptions>() ?? new UpdateOptions();
            var hashingOptions = installationRoot.GetSection(s_HashingOptionsSectionName)?.Get<HashingOptions>() ?? new HashingOptions();
            var databaseOptions = appDataRoot.GetSection(s_DatabaseOptionsSectionName)?.Get<DatabaseOptions>() ?? new DatabaseOptions();

            Current = new Configuration(updateOptions, hashingOptions, databaseOptions);
        }
    }
}

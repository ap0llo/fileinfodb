using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FileInfoDb.Config
{
    class Configuration
    {
        const string s_UpdateOptionsSectionName = "Update";
        const string s_HashingOptionsSectionName = "Hashing";


        public const string ConfigFileName = "config.json";

        public const string DebugConfigFileName = "config.Debug.json";


        public UpdateOptions UpdateOptions { get; }

        public HashingOptions HashingOptions { get; }


        private Configuration(UpdateOptions updateOptions, HashingOptions hashingOptions)
        {
            UpdateOptions = updateOptions ?? throw new System.ArgumentNullException(nameof(updateOptions));
            HashingOptions = hashingOptions ?? throw new System.ArgumentNullException(nameof(hashingOptions));
        }


        public static Configuration Current { get; }

        static Configuration()
        {
            var root = new ConfigurationBuilder()                
                .AddJsonFile(Path.Combine(ApplicationDirectories.LocalAppData, ConfigFileName), true)
                .AddJsonFile(Path.Combine(ApplicationDirectories.LocalAppData, DebugConfigFileName), true)
                .Build();

            var updateOptions = root.GetSection(s_UpdateOptionsSectionName)?.Get<UpdateOptions>() ?? new UpdateOptions();
            var hashingOptions = root.GetSection(s_HashingOptionsSectionName)?.Get<HashingOptions>() ?? new HashingOptions();
            Current = new Configuration(updateOptions, hashingOptions);
        }
    }
}

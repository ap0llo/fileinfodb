using Grynwald.Utilities.Squirrel;
using Grynwald.Utilities.Squirrel.Updating;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FileInfoDb
{
    class Configuration
    {
        const string s_UpdateOptionsSectionName = "Update";


        public const string ConfigFileName = "config.json";

        public const string DebugConfigFileName = "config.Debug.json";


        public UpdateOptions UpdateOptions { get; }


        private Configuration(UpdateOptions updateOptions)
        {
            UpdateOptions = updateOptions ?? throw new System.ArgumentNullException(nameof(updateOptions));
        }


        public static Configuration Current { get; }

        static Configuration()
        {
            var root = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationInfo.GetDirectoryPath(SpecialDirectory.ApplicationRootDirectory), ConfigFileName), true)
                .AddJsonFile(Path.Combine(ApplicationInfo.GetDirectoryPath(SpecialDirectory.ApplicationRootDirectory), DebugConfigFileName), true)
                .Build();

            var updateOptions = root.GetSection(s_UpdateOptionsSectionName).Get<UpdateOptions>();
            Current = new Configuration(updateOptions);
        }
    }
}

using System;
using System.IO;

namespace FileInfoDb.Config
{
    class HashingOptions
    {
        string m_CachePath;


        public bool EnableCache { get; set; }

        public string CachePath
        {
            get => m_CachePath;
            set => m_CachePath = Environment.ExpandEnvironmentVariables(value);
        }


        public HashingOptions()
        {
            EnableCache = true;
            CachePath = Path.Combine(ApplicationDirectories.RoamingAppData, "hashing.cache.db");
        }
    }
}

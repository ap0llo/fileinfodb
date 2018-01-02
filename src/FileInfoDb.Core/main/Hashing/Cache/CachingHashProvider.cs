using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FileInfoDb.Core.Hashing.Cache
{
    /// <summary>
    /// Implementation of <see cref="IHashProvider"/> that uses a cache to avoid 
    /// calculating a file's hash when possible
    /// </summary>
    public class CachingHashProvider : IHashProvider
    {
        readonly ILogger m_Logger;
        readonly IHashedFileInfoCache m_Cache;
        readonly IHashProvider m_InnerHashProvider;


        public HashAlgorithm Algorithm => m_InnerHashProvider.Algorithm;


        public CachingHashProvider(ILogger logger, IHashedFileInfoCache cache, IHashProvider innerHashProvider)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            m_InnerHashProvider = innerHashProvider ?? throw new ArgumentNullException(nameof(innerHashProvider));
        }
        

        public HashedFileInfo GetFileHash(string path)
        {
            // open and lock file to prevent modifications while determining hash
            var fileInfo = new FileInfo(path);
            using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // refresh file info in case it was changed between creating the FileInfo object and getting the file lock
                fileInfo.Refresh(); 
                var fileProperties = FileProperties.FromFileInfo(fileInfo);

                // try to get the file's hash from the cache
                var (cacheHit, hashedFileInfo) = m_Cache.TryGetHashedFileInfo(fileProperties, m_InnerHashProvider.Algorithm);                
                if(cacheHit)
                {
                    m_Logger.LogInformation($"Hash for file '{fileInfo.FullName}' was present in cache, using cached value");
                    // cache hit => return cached value
                    return hashedFileInfo;
                }
            }

            // cache miss => calculate hash and store value in the cache
            var newHashedFileInfo = m_InnerHashProvider.GetFileHash(path);
            m_Logger.LogInformation($"Saving hash for file '{fileInfo.FullName}' to cache");
            m_Cache.AddOrUpdateHashedFileInfo(newHashedFileInfo);
            return newHashedFileInfo;
        }
    }
}

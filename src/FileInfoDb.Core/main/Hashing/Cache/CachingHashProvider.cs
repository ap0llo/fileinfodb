using System;
using System.IO;
using NodaTime.Extensions;

namespace FileInfoDb.Core.Hashing.Cache
{
    public class CachingHashProvider : IHashProvider
    {
        readonly IHashedFileInfoCache m_Cache;
        readonly IHashProvider m_InnerHashProvider;


        public HashAlgorithm Algorithm => m_InnerHashProvider.Algorithm;


        public CachingHashProvider(IHashedFileInfoCache cache, IHashProvider innerHashProvider)
        {
            m_Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            m_InnerHashProvider = innerHashProvider ?? throw new ArgumentNullException(nameof(innerHashProvider));
        }
        

        public HashedFileInfo GetFileHash(string path)
        {
            // open and lock file to prevent modifications while determining hash
            var fileInfo = new FileInfo(path);

            using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileInfo.Refresh();
                var fileProperties = FileProperties.FromFileInfo(fileInfo);

                var (cacheHit, hashedFileInfo) = m_Cache.TryGetHashedFileInfo(fileProperties, m_InnerHashProvider.Algorithm);
                
                if(cacheHit)
                {
                    return hashedFileInfo;
                }
            }

            var newHashedFileInfo = m_InnerHashProvider.GetFileHash(path);
            m_Cache.AddOrUpdateHashedFileInfo(newHashedFileInfo);

            return newHashedFileInfo;
        }
    }
}

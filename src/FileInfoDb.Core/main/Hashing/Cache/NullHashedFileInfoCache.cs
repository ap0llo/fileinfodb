using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FileInfoDb.Core.Hashing.Cache
{
    public class NullHashedFileInfoCache : IHashedFileInfoCache
    {
        public void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo)
        {
            // nop
        }

        public (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(string path, Instant lastWriteTime, long length, HashAlgorithm algorithm) => (false, null);
    }
}

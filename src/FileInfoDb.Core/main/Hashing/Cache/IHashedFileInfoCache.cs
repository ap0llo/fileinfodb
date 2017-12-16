using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FileInfoDb.Core.Hashing.Cache
{
    public interface IHashedFileInfoCache
    {
        void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo);

        (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(string path, Instant lastWriteTime, long length, HashAlgorithm algorithm);        
    }
}

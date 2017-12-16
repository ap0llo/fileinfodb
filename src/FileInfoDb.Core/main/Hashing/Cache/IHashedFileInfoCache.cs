using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FileInfoDb.Core.Hashing.Cache
{
    public interface IHashedFileInfoCache
    {
        void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo);

        (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(FileProperties file, HashAlgorithm algorithm);        
    }
}

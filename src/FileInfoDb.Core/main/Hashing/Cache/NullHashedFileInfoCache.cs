using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FileInfoDb.Core.Hashing.Cache
{
    /// <summary>
    /// Dummy implementtion of <see cref="IHashedFileInfoCache"/>
    /// </summary>
    public class NullHashedFileInfoCache : IHashedFileInfoCache
    {
        public void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo)
        {
            // nop
        }

        public (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(FileProperties file, HashAlgorithm algorithm) => (false, null);
    }
}

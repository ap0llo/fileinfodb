using System;
using System.Collections.Generic;
using System.Text;

namespace FileInfoDb.Core.Hashing
{
    public interface IHashProvider
    {
        HashAlgorithm Algorithm { get; }

        HashedFileInfo GetFileHash(string path);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FileInfoDb.Core.Hashing
{
    public interface IHashProvider
    {
        /// <summary>
        /// The algorithm the provider uses to calculate file hashes
        /// </summary>
        HashAlgorithm Algorithm { get; }

        /// <summary>
        /// Determines the hash value for the specified file
        /// </summary>
        /// <param name="path">The path of the file to hash</param>        
        HashedFileInfo GetFileHash(string path);
    }
}

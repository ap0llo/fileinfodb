using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileInfoDb.Core.Hashing
{
    public class MD5HashProvider : IHashProvider
    {
        public HashAlgorithm Algorithm => HashAlgorithm.MD5;

        public HashedFileInfo GetFileHash(string path)
        {
            var fileInfo = new FileInfo(path);

            using (var sha256 = MD5.Create())
            using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileInfo.Refresh();

                var hashBytes = sha256.ComputeHash(fileStream);
                var hash = new HashValue(BitConverter.ToString(hashBytes).Replace("-", ""), HashAlgorithm.SHA256);

                return new HashedFileInfo(FileProperties.FromFileInfo(fileInfo), hash);
            }

        }
    }
}

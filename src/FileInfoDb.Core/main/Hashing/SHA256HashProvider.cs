using Microsoft.Extensions.Logging;
using NodaTime.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;

namespace FileInfoDb.Core.Hashing
{
    public class SHA256HashProvider : IHashProvider
    {
        readonly ILogger m_Logger;


        public HashAlgorithm Algorithm => HashAlgorithm.SHA256;


        public SHA256HashProvider(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public HashedFileInfo GetFileHash(string path)
        {
            var fileInfo = new FileInfo(path);
            m_Logger.LogInformation($"Calulating hash for file '{fileInfo.FullName}'");

            using (var sha256 = SHA256.Create())
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

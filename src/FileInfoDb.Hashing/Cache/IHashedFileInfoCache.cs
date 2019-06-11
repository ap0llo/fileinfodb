namespace FileInfoDb.Hashing.Cache
{
    /// <summary>
    /// Interface that provides access to a cache of file-hashes
    /// </summary>
    public interface IHashedFileInfoCache
    {
        /// <summary>
        /// Stores the specified hash in the cache replacing the previously
        /// cached value if it exists
        /// </summary>
        void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo);

        /// <summary>
        /// Tries to get a file-hash computed with the specified algorithm for the specified file
        /// from the cache
        /// </summary>
        (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(FileProperties file, HashAlgorithm algorithm);        
    }
}

using System.Data;
using FileInfoDb.Core.Utilities;

namespace FileInfoDb.Core.Hashing.Cache
{
    /// <summary>
    /// Helper class that defines the schema for the database table holding the cached file hashes
    /// </summary>
    static class HashedFileInfoTable
    {
        public const string Name = "HashedFileInfo";        

        public enum Column
        {
            Path,
            LastWriteTime,
            Length,
            HashAlgorithm,
            Hash
        }        
        
        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Path} TEXT NOT NULL,
                    {Column.LastWriteTime} INTEGER NOT NULL,
                    {Column.Length} INTEGER NOT NULL,
                    {Column.HashAlgorithm} TEXT NOT NULL,
                    {Column.Hash} TEXT NOT NULL,
                    PRIMARY KEY ({Column.Path}, {Column.HashAlgorithm})
                );                
            ");
        }
    }
}
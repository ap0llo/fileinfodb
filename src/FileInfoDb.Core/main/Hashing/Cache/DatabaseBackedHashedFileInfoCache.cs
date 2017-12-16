using System;
using NodaTime;
using Microsoft.Data.Sqlite;
using FileInfoDb.Core.Utilities;
using Dapper;

namespace FileInfoDb.Core.Hashing.Cache
{
    public class DatabaseBackedHashedFileInfoCache : IHashedFileInfoCache
    {
        readonly Database m_Database;


        public DatabaseBackedHashedFileInfoCache(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public void AddOrUpdateHashedFileInfo(HashedFileInfo fileInfo)
        {
            using (var connection = m_Database.OpenConnection())
            {
                connection.ExecuteNonQuery($@"
                    INSERT OR REPLACE INTO {HashedFileInfoTable.Name} 
                    (   
                        {HashedFileInfoTable.Column.Path},
                        {HashedFileInfoTable.Column.LastWriteTime},
                        {HashedFileInfoTable.Column.Length},
                        {HashedFileInfoTable.Column.HashAlgorithm},
                        {HashedFileInfoTable.Column.Hash}
                    )
                    VALUES (@path, @lastWriteTimeTicks, @length, @algorithm, @hash)",

                    ("path" , fileInfo.Path.ToLower()),
                    ("lastWriteTimeTicks" ,fileInfo.LastWriteTime.ToUnixTimeTicks()),
                    ("length" , fileInfo.Length),
                    ("algorithm" , fileInfo.Hash.Algorithm.ToString()),
                    ("hash" ,fileInfo.Hash.Value.ToUpper())
                );
            }
        }

        public (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(string path, Instant lastWriteTime, long length, HashAlgorithm algorithm)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var query = $@"
                    SELECT {HashedFileInfoTable.Column.Hash} 
                    FROM {HashedFileInfoTable.Name}
                    WHERE 
                        {HashedFileInfoTable.Column.Path} = @path AND
                        {HashedFileInfoTable.Column.LastWriteTime} = @lastWriteTimeTicks AND 
                        {HashedFileInfoTable.Column.Length} = @length AND
                        {HashedFileInfoTable.Column.HashAlgorithm} = @algorithm
                ";
                var hash = connection.QuerySingleOrDefault<string>(query, 
                new
                {
                    path = path.ToLower(),
                    lastWriteTimeTicks = lastWriteTime.ToUnixTimeTicks(),
                    length = length,
                    algorithm = algorithm.ToString()
                });

                return hash == null
                    ? (false, null)
                    : (true, new HashedFileInfo(path, lastWriteTime, length, new HashValue(hash, algorithm)));                
            }
        }
    }
}


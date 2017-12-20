using System;
using NodaTime;
using Microsoft.Data.Sqlite;
using Dapper;
using Grynwald.Utilities.Data;

namespace FileInfoDb.Core.Hashing.Cache
{
    /// <summary>
    /// Implementation of <see cref="IHashedFileInfoCache"/> taht is using 
    /// a relatational database to store cache values
    /// </summary>
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

                    ("path" , fileInfo.File.Path.ToLower()),
                    ("lastWriteTimeTicks" ,fileInfo.File.LastWriteTime.ToUnixTimeTicks()),
                    ("length" , fileInfo.File.Length),
                    ("algorithm" , fileInfo.Hash.Algorithm.ToString()),
                    ("hash" ,fileInfo.Hash.Value.ToUpper())
                );
            }
        }

        public (bool success, HashedFileInfo fileinfo) TryGetHashedFileInfo(FileProperties file, HashAlgorithm algorithm)
        {
            // if both Length and LastWriteTime of the file match we value
            // in the database, we assume the hash hasn't changed either

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
                    path = file.Path.ToLower(),
                    lastWriteTimeTicks = file.LastWriteTime.ToUnixTimeTicks(),
                    length = file.Length,
                    algorithm = algorithm.ToString()
                });

                return hash == null
                    ? (false, null)
                    : (true, new HashedFileInfo(file, new HashValue(hash, algorithm)));                
            }
        }
    }
}


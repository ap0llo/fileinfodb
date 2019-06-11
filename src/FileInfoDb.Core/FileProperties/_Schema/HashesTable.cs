using System.Data;
using Grynwald.Utilities.Data;

namespace FileInfoDb.Core.FileProperties
{
    /// <summary>
    /// Helper class that defines the schema for the database table holding the hashes stored in the database
    /// </summary>
    static class HashesTable
    {
        public const string Name = "Hashes";


        public enum Column
        {
            Id,
            Hash,
            Algorithm
        }


        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Hash}           VARCHAR(128) CHARACTER SET utf8 COLLATE utf8_general_ci UNIQUE NOT NULL,
                    {Column.Algorithm}      VARCHAR(10)  CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
                    CONSTRAINT UniqueHash UNIQUE ({Column.Hash}, {Column.Algorithm})
                );
            ");
        }

    }
}

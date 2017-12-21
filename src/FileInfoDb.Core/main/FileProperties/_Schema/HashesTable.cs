using Grynwald.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FileInfoDb.Core.FileProperties
{
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

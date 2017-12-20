using Grynwald.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FileInfoDb.Core.FileProperties
{
    static class PropertiesTable
    {

        public const string Name = "Properties";

        public enum Column
        {
            Id,
            HashId,
            Name,
            Value
        }


        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.HashId}         INTEGER NOT NULL,
                    {Column.Name}           VARCHAR(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
                    {Column.Value}          TEXT NOT NULL,
                    FOREIGN KEY ({Column.HashId})  REFERENCES {HashesTable.Name}({HashesTable.Column.Id}),
                    CONSTRAINT UniqueProperty UNIQUE ({Column.HashId}, {Column.Name})
                );
            ");
        }

    }
}

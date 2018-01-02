using System.Data;
using Grynwald.Utilities.Data;

namespace FileInfoDb.Core.FileProperties
{
    /// <summary>
    /// Helper class that defines the schema for the database table holding the properties associated with files
    /// </summary>
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

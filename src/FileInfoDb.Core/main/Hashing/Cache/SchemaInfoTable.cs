using System.Data;
using FileInfoDb.Core.Utilities;

namespace FileInfoDb.Core.Hashing.Cache
{
    static class SchemaInfoTable
    {
        public const string Name = "SchemaInfo";        

        public enum Column
        {
            Name,
            Version            
        }        
        
        public static void Create(IDbConnection connection)
        {
            // table is supposed to only have a single row
            // for that purpose, the "Name" column will always have 
            // the same value and has a UNIQUE constraint 
            
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Name} VARCHAR(20) UNIQUE NOT NULL,                  
                    {Column.Version} INTEGER UNIQUE NOT NULL                    
                );

                INSERT INTO {Name} ({Column.Name}, {Column.Version}) 
                VALUES ('SchemaInfo', {Database.SchemaVersion});
            ");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Grynwald.Utilities.Data;
using FileInfoDb.Core.Hashing;
using MySql.Data.MySqlClient;

namespace FileInfoDb.Core.FileProperties
{
    public class DatabaseBackedPropertyStorage : IPropertyStorage
    {
        readonly PropertiesDatabase m_Database;


        public DatabaseBackedPropertyStorage(PropertiesDatabase database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public IEnumerable<Property> GetProperties(HashValue fileHash)
        {
            using(var connection = m_Database.OpenConnection())
            {
                var properties = connection.Query<Property>($@"
                    SELECT 
                        {PropertiesTable.Column.Name}, 
                        {PropertiesTable.Column.Value}
                    FROM 
                        {PropertiesTable.Name} JOIN 
                        {HashesTable.Name} ON
                        {HashesTable.Name}.{HashesTable.Column.Id} = {PropertiesTable.Column.HashId}
                    WHERE {HashesTable.Column.Hash} = @hash AND
                          {HashesTable.Column.Algorithm} = @algorithm;
                    ",
                new
                { 
                    hash = fileHash.Value,
                    algorithm = fileHash.Algorithm.ToString()
                });

                return properties.ToList();
            }            
        }

        public IEnumerable<string> GetPropertyNames()
        {
            using (var connection = m_Database.OpenConnection())
            {
                var properties = connection.Query<string>($@"
                    SELECT DISTINCT
                        {PropertiesTable.Column.Name}
                    FROM  {PropertiesTable.Name};
                    ");

                return properties.ToList();
            }
        }

        public void SetProperty(HashValue fileHash, Property property, bool overwrite)
        {
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                connection.ExecuteNonQuery($@"
                        INSERT IGNORE INTO {HashesTable.Name} 
                        (   
                            {HashesTable.Column.Algorithm}, 
                            {HashesTable.Column.Hash}
                        )
                        VALUES 
                        (
                            @algorithm, 
                            @hash
                        )",
                        ("algorithm", fileHash.Algorithm.ToString()),
                        ("hash", fileHash.Value)
                    );

                var hashId = connection.QuerySingle<int>($@"
                    SELECT {HashesTable.Column.Id} 
                    FROM {HashesTable.Name}
                    WHERE {HashesTable.Column.Algorithm} = @algorithm AND
                          {HashesTable.Column.Hash} = @hash",

                    new
                    {
                        hash = fileHash.Value,
                        algorithm = fileHash.Algorithm.ToString()
                    }
                );

                try
                {
                    connection.ExecuteNonQuery($@"
                    {(overwrite ? "REPLACE" : "INSERT")} INTO {PropertiesTable.Name} 
                    (
                        {PropertiesTable.Column.HashId}, 
                        {PropertiesTable.Column.Name}, 
                        {PropertiesTable.Column.Value}
                    )
                    VALUES (@hashId, @name, @value)",
                    ("hashId", hashId),
                    ("name", property.Name),
                    ("value", property.Value)
                    );

                }
                catch (MySqlException ex) when (ex.Number == (int) MySqlErrorCode.DuplicateKeyEntry)
                {
                    transaction.Rollback();
                    throw new PropertyAlreadyExistsException("Property already exists", ex);
                }
                transaction.Commit();
            }
        }
    }
}

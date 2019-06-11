using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Grynwald.Utilities.Data;
using MySql.Data.MySqlClient;
using FileInfoDb.Hashing;

namespace FileInfoDb.Core.FileProperties
{
    /// <summary>
    /// Implementation of <see cref="IPropertyStorage"/> bsed on a relational database
    /// </summary>
    public class DatabaseBackedPropertyStorage : IPropertyStorage
    {
        readonly PropertiesDatabase m_Database;


        public DatabaseBackedPropertyStorage(PropertiesDatabase database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public Property GetProperty(HashValue fileHash, string propertyName)
        {
            using (var connection = m_Database.OpenConnection())
            {
                return connection.QuerySingleOrDefault<Property>($@"
                    SELECT 
                        {PropertiesTable.Column.Name}, 
                        {PropertiesTable.Column.Value}
                    FROM 
                        {PropertiesTable.Name} JOIN 
                        {HashesTable.Name} ON
                        {HashesTable.Name}.{HashesTable.Column.Id} = {PropertiesTable.Column.HashId}
                    WHERE {HashesTable.Column.Hash} = @hash AND
                          {HashesTable.Column.Algorithm} = @algorithm AND
                          {PropertiesTable.Column.Name} = @propertyName;
                    ",
                new
                {
                    hash = fileHash.Value,
                    algorithm = fileHash.Algorithm.ToString(),
                    propertyName = propertyName
                });
            }
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
                    SELECT DISTINCT {PropertiesTable.Column.Name}
                    FROM {PropertiesTable.Name};
                ");

                return properties.ToList();
            }
        }
        

        public void SetProperty(HashValue fileHash, Property property, bool overwrite)
        {
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                // insert hash into database if it does not already exist
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

                // get the id of the hash
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
                    // insert (or overwrite) the property
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
                //TODO: The MySQL specific exception should not be handled here, MySQL is a implementation detail of 'PropertiesDatabase'
                catch (MySqlException ex) when (ex.Number == (int) MySqlErrorCode.DuplicateKeyEntry)
                {                    
                    // abort if the property already exists and overwrite was false
                    transaction.Rollback();
                    throw new PropertyAlreadyExistsException("Property already exists", ex);
                }
                transaction.Commit();
            }
        }
    }
}

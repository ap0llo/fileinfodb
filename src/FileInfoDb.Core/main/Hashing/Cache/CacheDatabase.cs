using System;
using System.Data;
using System.IO;
using Grynwald.Utilities.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileInfoDb.Core.Hashing.Cache
{
    /// <summary>
    /// Helper class for accessing the SQLite-based cache 
    /// </summary>
    public sealed class CacheDatabase
    {
        static readonly object s_Lock = new object();
        readonly string m_ConnectionString;
        readonly ILogger<CacheDatabase> m_Logger;
        bool m_FirstAccess = true;


        public const int SchemaVersion = 1;


        public CacheDatabase(string databaseFilePath) : this(NullLogger<CacheDatabase>.Instance, databaseFilePath)
        {
        }

        public CacheDatabase(ILogger<CacheDatabase> logger, string databaseFilePath)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath));
            
            m_ConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFilePath
            }.ConnectionString;           
        }


        public IDbConnection OpenConnection()
        {
            // on first access, ensure the databse is compatible
            // and the schema was created
            lock (s_Lock)
            {
                if (m_FirstAccess)
                {
                    EnsureDatabaseIsCreated();
                    m_FirstAccess = false;
                }
            }
            
            return DoOpenConnection();
        }

        
        IDbConnection DoOpenConnection()
        {
            var connection = new SqliteConnection(m_ConnectionString);
            connection.Open();

            return connection;
        }

        void EnsureDatabaseIsCreated()
        {
            using (var connection = DoOpenConnection())
            {
                // if the SchemaInfo table exists, we assume all required
                // tables are present in the database
                if (connection.TableExists(SchemaInfoTable.Name))
                {
                    CheckSchema();
                }
                else
                {
                    CreateSchema();
                }
            }
        }

        void CreateSchema()
        {
            m_Logger.LogInformation($"Creating schema in database");

            using (var connection = DoOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                HashedFileInfoTable.Create(connection);
                SchemaInfoTable.Create(connection);

                transaction.Commit();
            }
        }

        void CheckSchema()
        {
            //TODO: Implement upgrade logic here when a new schema version is introduced.
            using (var connection = DoOpenConnection())
            {
                using (m_Logger.BeginScope("SchemaCheck"))
                {
                    m_Logger.LogInformation("Checking schema of database");
                    var schemaVersion = connection.ExecuteScalar<int>($"SELECT {SchemaInfoTable.Column.Version} FROM {SchemaInfoTable.Name}");

                    m_Logger.LogInformation($"Schema version of database is {schemaVersion}, current version is {SchemaVersion}");

                    if (schemaVersion != SchemaVersion)
                    {
                        throw new IncompatibleSchemaException(
                            supportedVersion: SchemaVersion,
                            databaseVersion: schemaVersion);
                    }
                }
            }
        }        
    }
}

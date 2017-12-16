using FileInfoDb.Core.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace FileInfoDb.Core.Hashing.Cache
{
    public sealed class Database
    {
        readonly string m_ConnectionString;
        readonly string m_DatabaseFilePath;
        readonly ILogger<Database> m_Logger;
        static readonly object s_Lock = new object();
        bool m_FirstAccess = true;


        public const int SchemaVersion = 1;


        public Database(string databaseFilePath) : this(NullLogger<Database>.Instance, databaseFilePath)
        {
        }

        public Database(ILogger<Database> logger, string databaseFilePath)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_DatabaseFilePath = databaseFilePath ?? throw new ArgumentNullException(nameof(databaseFilePath));

            m_ConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFilePath
            }.ConnectionString;           
        }


        public IDbConnection OpenConnection()
        {
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
            // create schema
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
                        throw new IncompatibleSchmeaException(
                            supportedVersion: SchemaVersion,
                            databaseVersion: schemaVersion);
                    }
                }
            }
        }
        
    }
}

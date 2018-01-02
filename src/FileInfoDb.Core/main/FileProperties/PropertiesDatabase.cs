using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace FileInfoDb.Core.FileProperties
{
    public abstract class PropertiesDatabase
    {
        public const int SchemaVersion = 1;


        static readonly object s_Lock = new object();
        bool m_FirstAccess = true;
        readonly ILogger m_Logger;

        
        protected PropertiesDatabase(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public IDbConnection OpenConnection()
        {
            lock(s_Lock)
            {
                if(m_FirstAccess)
                {
                    CheckSchema();
                    m_FirstAccess = false;
                }
            }

            return DoOpenConnection();
        }

        public void Create()
        {
            DoCreateDatabase();
            DoCreateSchema();
        }

        public void CheckSchema()
        {
            //TODO: Implement upgrade logic here when a new schema version is introduced. Should schema upgrades be explicit or implicit?
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

        public abstract void Drop();


        protected abstract void DoCreateDatabase();

        protected abstract IDbConnection DoOpenConnection();


        void DoCreateSchema()
        {            
            m_Logger.LogInformation($"Creating schema in database");

            using (var connection = DoOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {

                HashesTable.Create(connection);
                PropertiesTable.Create(connection);       
                SchemaInfoTable.Create(connection);

                transaction.Commit();
            }
        }
    }
}

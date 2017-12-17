using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FileInfoDb.Core.FileProperties;
using FileInfoDb.Core.Hashing;

namespace FileInfoDb.Core.Test.FileProperties
{
    public class DatabaseBackedPropertyStorageTest : IDisposable
    {
        protected readonly Uri m_DatabaseUri;
        protected PropertiesDatabase Database { get; }


        public DatabaseBackedPropertyStorageTest()
        {
            // load database uri from environment variables      
            var mysqlUri = Environment.GetEnvironmentVariable("FILEINFODB_TEST_MYSQLURI");

            // set database name 
            var uriBuilder = new UriBuilder(new Uri(mysqlUri))
            {
                Path = "fileinfodb_test_" + Guid.NewGuid().ToString().Replace("-", "")
            };

            m_DatabaseUri = uriBuilder.Uri;

            // create database
            Database = new MySqlPropertiesDatabase(NullLogger<MySqlPropertiesDatabase>.Instance, m_DatabaseUri);
            Database.Create();
        }


        public virtual void Dispose() => Database.Drop();

        HashValue GetRandomHashValue()
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Guid.NewGuid().ToByteArray());
                return new HashValue(
                            BitConverter.ToString(hashBytes).Replace("-", ""),
                            HashAlgorithm.SHA256);
            }
        }


        [Fact]
        public void GetProperties_is_empty_for_unknown_file()
        {
            var instance = new DatabaseBackedPropertyStorage(Database);
            Assert.Empty(instance.GetProperties(GetRandomHashValue()));            
        }


        [Fact]
        public void SetProperty_saves_new_property()
        {
            var instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("testproperty", Guid.NewGuid().ToString());

            instance.SetProperty(hash, property);

            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(property, readProperties.Single());
        }

        [Fact]
        public void A_file_can_have_multiple_properties()
        {
            // ARRANGE
            var instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();

            var property1 = new Property("property1", Guid.NewGuid().ToString());
            var property2 = new Property("property2", Guid.NewGuid().ToString());

            // ACT
            instance.SetProperty(hash, property1);
            instance.SetProperty(hash, property2);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Equal(2, readProperties.Count());
            Assert.Single(readProperties.Where(x => x.Equals(property1)));
            Assert.Single(readProperties.Where(x => x.Equals(property2)));
        }

        [Fact]
        public void SetProperty_overwrites_existing_property_with_same_name()
        {
            // ARRANGE
            var instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("testproperty", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property);

            // ACT
            var newProperty = new Property("testproperty", Guid.NewGuid().ToString());
            instance.SetProperty(hash, newProperty);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(newProperty, readProperties.Single());
        }

        [Fact]
        public void Property_names_are_treated_case_insensitive()
        {
            // ARRANGE
            var instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("property1", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property);

            // ACT
            var newProperty = new Property("PROPERTY1", Guid.NewGuid().ToString());
            instance.SetProperty(hash, newProperty);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(newProperty, readProperties.Single());
        }        

    }
}

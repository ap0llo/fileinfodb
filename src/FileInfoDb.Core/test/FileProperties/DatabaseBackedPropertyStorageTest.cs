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
        public void GetPropertyNames_returns_empty_enumerable_for_empty_database()
        {
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            Assert.Empty(instance.GetPropertyNames());
        }

        [Fact]
        public void GetPropertyNames_returns_expected_names()
        {
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);

            instance.SetProperty(GetRandomHashValue(), new Property("name1", "Irrelevant"), false);
            instance.SetProperty(GetRandomHashValue(), new Property("name2", "Irrelevant"), false);
            instance.SetProperty(GetRandomHashValue(), new Property("Name1", "Irrelevant"), false);

            var names = instance.GetPropertyNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.Equal(2, names.Count());

            var expectedNames = new[] { "name1", "name2" }.ToHashSet(StringComparer.OrdinalIgnoreCase);
            names.ExceptWith(expectedNames);            
            Assert.Empty(names);
        }

        [Fact]
        public void GetProperties_is_empty_for_unknown_file()
        {
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            Assert.Empty(instance.GetProperties(GetRandomHashValue()));            
        }

        [Fact]
        public void SetProperty_saves_new_property()
        {
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("testproperty", Guid.NewGuid().ToString());

            instance.SetProperty(hash, property, false);

            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(property, readProperties.Single());
        }

        [Fact]
        public void A_file_can_have_multiple_properties()
        {
            // ARRANGE
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();

            var property1 = new Property("property1", Guid.NewGuid().ToString());
            var property2 = new Property("property2", Guid.NewGuid().ToString());

            // ACT
            instance.SetProperty(hash, property1, false);
            instance.SetProperty(hash, property2, false);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Equal(2, readProperties.Count());
            Assert.Single(readProperties.Where(x => x.Equals(property1)));
            Assert.Single(readProperties.Where(x => x.Equals(property2)));
        }

        [Fact]
        public void SetProperty_can_overwrites_a_existing_property_with_same_name()
        {
            // ARRANGE
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("testproperty", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property, false);

            // ACT
            var newProperty = new Property("testproperty", Guid.NewGuid().ToString());
            instance.SetProperty(hash, newProperty, true);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(newProperty, readProperties.Single());
        }

        [Fact]
        public void SetProperty_throws_PropertyAlreadyExistsException_whem_property_already_exists()
        {
            // ARRANGE
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("testproperty", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property, false);

            // ACT / ASSERT
            var newProperty = new Property("testproperty", Guid.NewGuid().ToString());
            Assert.Throws<PropertyAlreadyExistsException>(() => instance.SetProperty(hash, newProperty, false));            
        }

        [Fact]
        public void Property_names_are_treated_case_insensitive_01()
        {
            // ARRANGE
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("property1", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property, false);

            // ACT
            var newProperty = new Property("PROPERTY1", Guid.NewGuid().ToString());
            instance.SetProperty(hash, newProperty, true);

            // ASSERT
            var readProperties = instance.GetProperties(hash);

            Assert.Single(readProperties);
            Assert.Equal(newProperty, readProperties.Single());
        }

        [Fact]
        public void Property_names_are_treated_case_insensitive_02()
        {
            // ARRANGE
            IPropertyStorage instance = new DatabaseBackedPropertyStorage(Database);
            var hash = GetRandomHashValue();
            var property = new Property("property1", Guid.NewGuid().ToString());
            instance.SetProperty(hash, property, false);

            // ACT / ASSERT
            var newProperty = new Property("PROPERTY1", Guid.NewGuid().ToString());
            Assert.Throws<PropertyAlreadyExistsException>(() => instance.SetProperty(hash, newProperty, false));            
        }
    }
}

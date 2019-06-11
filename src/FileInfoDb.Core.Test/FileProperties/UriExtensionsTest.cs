﻿using System;
using Xunit;
using FileInfoDb.Core.FileProperties;

namespace FileInfoDb.Core.Test.FileProperties
{
    public class UriExtensionsTest
    {
        [Theory]
        [InlineData("fileinfodb-mysql://user:password@host:123/databasename", "host", 123, "databasename", "user", "password")]
        [InlineData("fileINFODB-MYSQL://user:password@host:123/databasename", "host", 123, "databasename", "user", "password")]
        [InlineData("fileinfodb-mysql://user:password@host/databasename", "host", 3306, "databasename", "user", "password")]
        [InlineData("fileinfodb-mysql://user:password@host:123", "host", 123, "", "user", "password")]
        [InlineData("fileinfodb-mysql://user:password@host:123/", "host", 123, "", "user", "password")]
        [InlineData("fileinfodb-mysql://user@host:123/databasename", "host", 123, "databasename", "user", "")]
        [InlineData("fileinfodb-mysql://host:123/databasename", "host", 123, "databasename", "", "")]
        public void ToMySqlConnectionStringBuilder_returns_expected_values(string uri, string expectedServer, uint expectedPort, string expectedDatabaseName, string expectedUserId, string expectedPassword)
        {
            var connectionStringBuilder = new Uri(uri).ToMySqlConnectionStringBuilder();

            Assert.Equal(expectedServer, connectionStringBuilder.Server);
            Assert.Equal(expectedPort, connectionStringBuilder.Port);
            Assert.Equal(expectedDatabaseName, connectionStringBuilder.Database);
            Assert.Equal(expectedUserId, connectionStringBuilder.UserID);
            Assert.Equal(expectedPassword, connectionStringBuilder.Password);
        }

        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("file://www.example.com")]
        public void ToMySqlConnectionStringBuilder_throws_InvalidDatabaseUriException_if_scheme_if_unknown(string uri)
        {
            Assert.Throws<InvalidDatabaseUriException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }

        [Theory]
        [InlineData("fileinfodb-mysql://host:123/databasename/anothername")]
        public void ToMySqlConnectionStringBuilder_throws_InvalidDatabaseUriException_if_path_contains_multiple_segments(string uri)
        {
            Assert.Throws<InvalidDatabaseUriException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }

        [Theory]
        [InlineData("fileinfodb-mysql:///databasename")]
        public void ToMySqlConnectionStringBuilder_throws_InvalidDatabaseUriException_if_host_is_null(string uri)
        {
            Assert.Throws<InvalidDatabaseUriException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }
    }
}
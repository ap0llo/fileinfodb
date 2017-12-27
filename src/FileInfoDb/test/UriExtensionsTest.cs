using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FileInfoDb.Test
{
    public class UriExtensionsTest
    {

        [Theory]
        [InlineData("http://example.com/path")]
        [InlineData("http://example.com/path?query=foo")]
        [InlineData("http://example.com:1234/path")]
        [InlineData("http://user@example.com:1234/path")]
        [InlineData("http://user:pw@example.com:1234/path")]
        public void WithoutCredentials_returns_an_equivalent_uri_without_credentials(string uriString)
        {
            var uri = new Uri(uriString);
            var uriWithoutCredentials = uri.WithoutCredentials();

            Assert.True(String.IsNullOrEmpty(uriWithoutCredentials.UserInfo));
            Assert.Equal(uri.Scheme, uriWithoutCredentials.Scheme);
            Assert.Equal(uri.Host, uriWithoutCredentials.Host);
            Assert.Equal(uri.PathAndQuery, uriWithoutCredentials.PathAndQuery);
            Assert.Equal(uri.Port, uriWithoutCredentials.Port);            
        }
    }
}

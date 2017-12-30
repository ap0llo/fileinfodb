using System;
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


        [Theory]
        [InlineData("http://example.com/path", false)]
        [InlineData("http://example.com/path?query=foo", false)]
        [InlineData("http://example.com:1234/path", false)]
        [InlineData("http://user@example.com:1234/path", false)]
        [InlineData("http://user:pw@example.com:1234/path", true)]
        public void HasPassword_returns_expected_value(string uriString, bool hasPassword)
        {
            var uri = new Uri(uriString);
            Assert.Equal(hasPassword, uri.HasPassword());
        }
    }
}

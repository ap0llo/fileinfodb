using System;

namespace FileInfoDb
{
    static class UriExtensions
    {
        public static bool HasPassword(this Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            return !String.IsNullOrEmpty(uriBuilder.Password);
        }        
   
        public static Uri WithCredentials(this Uri uri, string userName, string password)
        {
            var uriBuilder = new UriBuilder(uri)
            {
                UserName = userName,
                Password = password
            };
            return uriBuilder.Uri;
        }

        public static Uri WithoutCredentials(this Uri uri) => uri.WithCredentials(null, null);

        public static string GetUserName(this Uri uri) => new UriBuilder(uri).UserName;

        public static string GetPassword(this Uri uri) => new UriBuilder(uri).Password;
    }
}

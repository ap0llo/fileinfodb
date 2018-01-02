using System;

namespace FileInfoDb
{
    static class UriExtensions
    {
        /// <summary>
        /// Determines if the Uri contains a password
        /// </summary>
        public static bool HasPassword(this Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            return !String.IsNullOrEmpty(uriBuilder.Password);
        }        

        /// <summary>
        /// Replaces the credentials of the uri with the specified user name and password or
        /// adds the credentials if the Uri does not yet contain credentials
        /// </summary>
        /// <returns></returns>
        public static Uri WithCredentials(this Uri uri, string userName, string password)
        {
            var uriBuilder = new UriBuilder(uri)
            {
                UserName = userName,
                Password = password
            };
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Removes user name and password from the Uri if they exist
        /// </summary>
        public static Uri WithoutCredentials(this Uri uri) => uri.WithCredentials(null, null);

        /// <summary>
        /// Gets the user name from the uri
        /// </summary>
        /// <returns>Retunrs the user name or null if the Uri did not contain a user name</returns>
        public static string GetUserName(this Uri uri) => new UriBuilder(uri).UserName;

        /// <summary>
        /// Gets the password from the uri
        /// </summary>
        /// <returns>Retunrs the password or null if the Uri did not contain credentials</returns>        
        public static string GetPassword(this Uri uri) => new UriBuilder(uri).Password;
    }
}

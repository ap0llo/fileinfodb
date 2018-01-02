using System;
using MySql.Data.MySqlClient;

namespace FileInfoDb.Core.FileProperties
{
    public static class UriExtensions
    {
        const string s_Scheme = "fileinfodb-mysql";


        public static string ToMySqlConnectionString(this Uri uri) => uri.ToMySqlConnectionStringBuilder().ConnectionString;        
        
        public static MySqlConnectionStringBuilder ToMySqlConnectionStringBuilder(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if(!uri.IsValidFileInfoDbMySqlUri(out var error))
                throw new InvalidDatabaseUriException(uri, error);

            
            var (user, password) = GetUserInfo(uri);

            var connectionStringBuilder = new MySqlConnectionStringBuilder()            
            {
                Server = uri.Host,     
                Database = uri.Segments.Length == 2 ? uri.Segments[1] : null,
                UserID = user,
                Password = password
            };

            if (uri.Port > 0)
                connectionStringBuilder.Port = (uint) uri.Port;

            return connectionStringBuilder;
        }

        public static bool IsValidFileInfoDbMySqlUri(this Uri uri) => IsValidFileInfoDbMySqlUri(uri, out var _);

        public static bool IsValidFileInfoDbMySqlUri(this Uri uri, out string error)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (uri.Scheme != s_Scheme)
            {
                error = $"Unsupported scheme '{uri.Scheme}'";
                return false;
            }

            if (String.IsNullOrEmpty(uri.Host))
            {
                error = "Host must not be empty";
                return false;
            }

            if (uri.Segments.Length > 2)
            {
                error = "Uri must not contain multiple segments";
                return false;
            }

            error = default;
            return true;
        }


        static (string user, string password) GetUserInfo(Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            return (uriBuilder.UserName, uriBuilder.Password);
        }
    }
}
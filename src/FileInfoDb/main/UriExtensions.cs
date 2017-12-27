using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoDb
{
    static class UriExtensions
    {
        public static Uri WithoutCredentials(this Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.UserName = null;
            uriBuilder.Password = null;
            return uriBuilder.Uri;
        }

    }
}

using Grynwald.Utilities.Squirrel;
using System;
using System.IO;

namespace FileInfoDb
{
    class ApplicationDirectories
    {
        public static string LocalAppData => 
            ApplicationInfo.GetDirectoryPath(SpecialDirectory.ApplicationRootDirectory);

        public static string RoamingAppData =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                         ApplicationInfo.ApplicationName);
    }
}

using System;
using System.IO;
using Grynwald.Utilities.Squirrel;

namespace FileInfoDb
{
    class ApplicationDirectories
    {
        public static string InstallationDirectory => 
            ApplicationInfo.GetDirectoryPath(SpecialDirectory.ApplicationRootDirectory);

        public static string RoamingAppData =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                         ApplicationInfo.ApplicationName);
    }
}
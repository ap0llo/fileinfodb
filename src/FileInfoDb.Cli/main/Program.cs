using FileInfoDb.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace FileInfoDb.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var hashProvider = new SHA256HashProvider();

            var files = Directory.EnumerateFiles(args[0], "*", SearchOption.AllDirectories);

            foreach(var file in files)
            {
                Console.WriteLine(file);

                var hashFileInfo = hashProvider.GetFileHash(file);
                Console.WriteLine($"\t{hashFileInfo.Hash}");
            }

#if DEBUG
            if(Debugger.IsAttached)
            {
                Console.WriteLine("Completed. Press any key to continue...");
                Console.ReadKey();
            }
#endif
        }
    }
}

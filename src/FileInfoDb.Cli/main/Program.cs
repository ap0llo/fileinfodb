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
#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            var hashProvider = new SHA256HashProvider();

            var files = Directory.EnumerateFiles(args[0], "*", SearchOption.AllDirectories);

            foreach(var file in files)
            {
                Console.WriteLine(file);

                var hashFileInfo = hashProvider.GetFileHash(file);
                Console.WriteLine($"\t{hashFileInfo.Hash}");
            }

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine($"Completed, Elapsed time {stopwatch.Elapsed}");

            if(Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
#endif
        }
    }
}

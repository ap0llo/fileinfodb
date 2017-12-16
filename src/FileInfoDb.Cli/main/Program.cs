using System;
using System.Diagnostics;
using System.IO;
using FileInfoDb.Core.Hashing;
using FileInfoDb.Core.Hashing.Cache;

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

            var cacheDb = new Database("cache.db");
            var cache = new DatabaseBackedHashedFileInfoCache(cacheDb);
            IHashProvider hashProvider = new CachingHashProvider(cache, new SHA256HashProvider());

            var files = Directory.EnumerateFiles(args[0], "*.mp3", SearchOption.AllDirectories);

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

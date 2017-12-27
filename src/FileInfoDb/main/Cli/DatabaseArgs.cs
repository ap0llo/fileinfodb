using CommandLine;

namespace FileInfoDb.Cli
{
    class DatabaseArgs : BaseArgs
    {
        
        [Option("uri", Required= true)]
        public string DatabaseUri { get; set; }


    }
}
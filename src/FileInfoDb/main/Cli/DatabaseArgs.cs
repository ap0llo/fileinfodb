using CommandLine;

namespace FileInfoDb.Cli
{
    class DatabaseArgs : BaseArgs
    {
        [Option("uri", Required= false)]
        public string DatabaseUri { get; set; }
    }
}
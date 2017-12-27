using CommandLine;

namespace FileInfoDb.Cli
{
    class ArgsBase
    { 
        [Option("uri", Required= true)]
        public string DatabaseUri { get; set; }
    }
}
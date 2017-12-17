using CommandLine;

namespace FileInfoDb.Cli
{
    class OptionsBase
    { 
        [Option("uri", Required= true)]
        public string DatabaseUri { get; set; }
    }
}
using CommandLine;

namespace FileInfoDb.Cli
{
    class DatabaseArgs : BaseArgs
    {
        [Option("uri", Required= false, HelpText = "The database uri to use. If omitted, the default database uri is used (see 'configure' command)")]
        public string DatabaseUri { get; set; }
    }
}
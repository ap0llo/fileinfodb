using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.Configure)]
    class ConfigureArgs : BaseArgs
    {
        [Option("uri", Required = true)]
        public string DatabaseUri { get; set; }
    }
}

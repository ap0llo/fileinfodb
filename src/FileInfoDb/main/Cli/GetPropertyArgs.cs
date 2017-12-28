using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.GetProperty)]
    class GetPropertyArgs : DatabaseArgs
    {
        [Option("file", Required = true)]
        public string FilePath { get; set; }

        [Option("name", Required = false)]
        public string Name { get; set; }
    }
}

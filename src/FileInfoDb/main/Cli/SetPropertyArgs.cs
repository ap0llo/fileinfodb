using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.SetProperty)]
    class SetPropertyArgs : DatabaseArgs
    {
        [Option("file", Required = true)]
        public string FilePath { get; set; }

        [Option("name", Required = true)]
        public string Name { get; set; }

        [Option("value", Required = true, SetName ="Value")]
        public string Value { get; set; }

        [Option("value-from-file", Required = true, SetName = "ValueFromFile")]
        public string ValueFromFile { get; set; }
    }
}

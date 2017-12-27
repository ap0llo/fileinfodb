using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb("set-property")]
    class SetPropertyArgs : ArgsBase
    {
        [Option("file", Required = true)]
        public string FilePath { get; set; }

        [Option("name", Required = true)]
        public string Name { get; set; }

        [Option("value", Required =true)]
        public string Value { get; set; }
    }
}

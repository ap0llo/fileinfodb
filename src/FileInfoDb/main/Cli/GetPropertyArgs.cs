using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb("get-property")]
    class GetPropertyArgs : ArgsBase
    {
        [Option("file", Required = true)]
        public string FilePath { get; set; }
    }
}

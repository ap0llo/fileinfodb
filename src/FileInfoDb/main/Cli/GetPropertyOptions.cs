using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb("get-property")]
    class GetPropertyOptions : OptionsBase
    {
        [Option("file", Required = true)]
        public string FilePath { get; set; }
    }
}

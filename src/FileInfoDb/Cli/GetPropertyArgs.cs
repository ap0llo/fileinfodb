using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.GetProperty, HelpText = "Query properties for the specified file")]
    class GetPropertyArgs : DatabaseArgs
    {
        [Option("file", Required = true, HelpText ="The file for which to query properties")]
        public string FilePath { get; set; }

        [Option("name", Required = false, HelpText = "The name of the property to retrieve. Supports wildcards. If omitted, all properties for the file are retrieved")]
        public string Name { get; set; }
    }
}

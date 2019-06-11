using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.SetProperty, HelpText = "Sets the specified property for the file")]
    class SetPropertyArgs : DatabaseArgs
    {
        [Option("file", Required = true, HelpText = "The file which's property to set")]
        public string FilePath { get; set; }

        [Option("name", Required = true, HelpText = "The name of the property to set")]
        public string Name { get; set; }

        [Option("value", Required = true, SetName ="Value", HelpText = "The value of the property. Use either this parameter or 'value-from-file'")]
        public string Value { get; set; }

        [Option("value-from-file", Required = true, SetName = "ValueFromFile", HelpText = "The path of the file to read the property value from. use either this parameter or 'value'")]
        public string ValueFromFile { get; set; }

        [Option("overwrite", Required =false, Default = false, HelpText = "Overwrite the existing value if the specified property already exists for the file.")]
        public bool Overwrite { get; set; }
    }
}

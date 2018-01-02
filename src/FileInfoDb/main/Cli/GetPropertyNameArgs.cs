using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.GetPropertyName, HelpText = "Gets the names of all properties associated with any file")]
    class GetPropertyNameArgs : DatabaseArgs
    {        
    }
}

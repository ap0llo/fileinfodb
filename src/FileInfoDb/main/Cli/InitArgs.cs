using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.Init, HelpText = "Initializes a new database and creates the required database tables")]
    class InitArgs : DatabaseArgs
    {
    }
}

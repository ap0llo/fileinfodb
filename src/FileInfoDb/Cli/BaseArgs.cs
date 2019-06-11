using CommandLine;

namespace FileInfoDb.Cli
{
    class BaseArgs
    {
        [Option('v', "verbose", HelpText = "Show detailed progress messages")]
        public bool Verbose { get; set; }

#if DEBUG
        [Option("debug", HelpText = "Automatically attach debugger after launch")]
        public bool AttachDebugger { get; set; }
#endif 
    }
}

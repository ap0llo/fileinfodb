using CommandLine;

namespace FileInfoDb.Cli
{
    class ArgsBase
    {

#if DEBUG
        [Option("debug", HelpText = "Automatically attach debugger after launch")]
        public bool AttachDebugger { get; set; }
#endif 

        [Option("uri", Required= true)]
        public string DatabaseUri { get; set; }
    }
}
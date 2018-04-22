using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.Index, HelpText = "Compute and cache the hashes for files in a directory to speed up later " + 
                                          CommandNames.GetProperty + " and " + CommandNames.SetProperty + 
                                          " operations for these files")]
    class IndexArgs : BaseArgs
    {
        [Option("directory", Required = true, HelpText = "The path of the directory to index")]
        public string DirectoryPath { get; set; }

        [Option('r', "recursive", HelpText = "Index all subdirectories, too")]
        public bool Recursive { get; set; }
    }
}

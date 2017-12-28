using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.Configure)]
    class ConfigureArgs : BaseArgs
    {
        [Option("uri", Required = true)]
        public string DatabaseUri { get; set; }

    }
}

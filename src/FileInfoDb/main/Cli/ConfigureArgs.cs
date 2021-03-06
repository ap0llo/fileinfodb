﻿using CommandLine;

namespace FileInfoDb.Cli
{
    [Verb(CommandNames.Configure, HelpText = "Configure default database uri")]
    class ConfigureArgs : BaseArgs
    {
        [Option("uri", Required = true, HelpText ="The default database uri to save")]
        public string DatabaseUri { get; set; }

        [Option("prompt-for-credentials", HelpText = "Prompt for username and password")]
        public bool PromptForCredentials { get; set; }

    }
}

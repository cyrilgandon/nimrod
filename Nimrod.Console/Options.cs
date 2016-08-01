﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace Nimrod.Console
{
    public class Options
    {
        // -i -import
        [Option('m', "module", Required = false, HelpText = "Module mode, valid values are 'typescript' for typescript namespaces style, 'require' for require style and 'module' for ECMAScript 2015 style imports.")]
        public string Module { get; set; } = "typescript";

        public ModuleType ModuleType
        {
            get
            {
                if (Module.ToLowerInvariant() == "require") return ModuleType.Require;
                if (Module.ToLowerInvariant() == "module") return ModuleType.Module;
                return ModuleType.TypeScript;
            }
        }

        // -v --verbose
        [Option('v', "verbose", Required = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; } = false;

        [Option('h', "help", Required = false, HelpText = "Output the help")]
        public bool Help { get; set; }

        [Option('o', "output", Required = true, HelpText = "Directory where files will be generated.")]
        public string OutputPath { get; set; }

        // Assemblies to retrieve, -f --files
        [OptionList('f', "files", Required = true, Separator = ',', HelpText = "Specify files, separated by a comma. Example --files=bin\\Assembly1.dll,bin\\Assembly2.dll")]
        public IList<string> Files { get; set; }
    }
}

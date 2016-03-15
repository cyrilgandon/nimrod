﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nimrod
{
    public class Generator
    {
        public IFileSystem FileSystem { get; }

        public Generator(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
        }

        public void Generate(IOptions options, string helpText)
        {
            if (options.Help)
            {
                options.WriteLine(helpText);
            }
            else
            {
                var outputFolderPath = options.OutputPath;

                var files = options.Files
                    .Select(filePath =>
                    {
                        options.Write($"Reading file {filePath}...");

                        if (this.FileSystem.File.Exists(filePath))
                        {
                            options.WriteLine($"OK!");
                            return new { File = this.FileSystem.FileInfo.FromFileName(filePath), Success = true }; ;
                        }
                        else
                        {
                            options.WriteLine($"Warning! The specified file {filePath} doesn't exist and will be skipped.");
                            return new { File = null as FileInfoBase, Success = false };
                        }
                    })
                    .Where(file => file.Success)
                    .Select(file => file.File)
                    .ToList();

                options.WriteLine("outputFolderPath = " + outputFolderPath);

                var directories = files.Select(f => f.DirectoryName).Distinct();

                // Load all assemblies in the folder
                AssemblyLocator.Init();
                foreach (var directory in directories)
                {
                    foreach (var assemblyFile in this.FileSystem.Directory.EnumerateFiles(directory, "*.dll"))
                    {
                        options.Write($"Loading assembly {assemblyFile}");
                        Assembly.LoadFile(this.FileSystem.Path.Combine(directory, assemblyFile));
                        options.WriteLine($" Done!");
                    }
                }

                var foldersManager = new FoldersManager(outputFolderPath);
                options.Write($"Recursive deletion of {outputFolderPath}...");
                foldersManager.Recreate();
                options.WriteLine($" Done!");


                var assemblies = files.Select(t => Assembly.LoadFile(t.FullName));

                options.Write($"Discovering types..");
                var types = TypeDiscovery.GetControllerTypes(assemblies, true).ToList();
                options.WriteLine($" Done!");
                // Write all types except the ones in System
                var toWrites = types
                    .Where(t => t.IsSystem() == false)
                    .Select(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t)
                    .Distinct()
                    .ToList();
                options.Write($"Writing static files...");
                WriteStaticFiles(foldersManager, options.ModuleType);
                options.WriteLine($" Done!");

                options.Write($"Writing {toWrites.Count} files...");
                toWrites.ForEach(t =>
                {
                    options.Write($"Writing {t.Name}...");
                    WriteType(foldersManager, t, options.ModuleType);
                    options.WriteLine($" Done!");
                });
                options.WriteLine($"Writing {toWrites.Count} files...Done!");
            }

        }

        private void WriteStaticFiles(FoldersManager foldersManager, ModuleType module)
        {
            var writer = new StaticWriter(module);
            var restApiFilePath = this.FileSystem.Path.Combine(foldersManager.OutputFolderPath, "IRestApi.ts");
            using (var fileWriter = this.FileSystem.File.CreateText(restApiFilePath))
            {
                writer.Write(fileWriter);
            }
        }

        private void WriteType(FoldersManager foldersManager, Type type, ModuleType module)
        {
            BaseWriter writer;
            var filePath = this.FileSystem.Path.Combine(foldersManager.OutputFolderPath, type.GetTypeScriptFilename());
            using (var fileWriter = this.FileSystem.File.CreateText(filePath))
            {
                if (type.IsWebMvcController())
                {
                    writer = new ControllerWriter(fileWriter, module);
                }
                else if (type.IsEnum)
                {
                    writer = new EnumWriter(fileWriter, module);
                }
                else if (type.IsValueType)
                {
                    writer = new SerializableWriter(fileWriter, module);
                }
                else
                {
                    writer = new ModelWriter(fileWriter, module);
                }

                writer.Write(type);
            }
        }
    }
}

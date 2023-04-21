﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using RPGMakerDecrypter.Decrypter;
using RPGMakerDecrypter.Decrypter.Exceptions;

namespace RPGMakerDecrypter.Cli
{
    static class Program
    {
        private static CommandLineOptions _commandLineOptions;

        static void Main(string[] args)
        {
            var parsedResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
            _commandLineOptions = parsedResult.Value;

            if (parsedResult.Errors.Any())
            {
                Environment.Exit(1);
            }

            RPGMakerVersion version = RGSSAD.GetVersion(_commandLineOptions.InputPath);

            if (version == RPGMakerVersion.Invalid)
            {
                Console.WriteLine("Invalid input file.");
                Environment.Exit(1);
            }

            string outputDirectoryPath;

            if (_commandLineOptions.OutputDirectoryPath != null)
            {
                outputDirectoryPath = _commandLineOptions.OutputDirectoryPath;
            }
            else
            {
                FileInfo fi = new FileInfo(_commandLineOptions.InputPath);
                outputDirectoryPath = fi.DirectoryName;
            }

            try
            {
                switch (version)
                {
                    case RPGMakerVersion.Xp:
                    case RPGMakerVersion.Vx:
                        RGSSADv1 rgssadv1 = new RGSSADv1(_commandLineOptions.InputPath);
                        foreach (ArchivedFile archivedFile in rgssadv1.ArchivedFiles)
                        {
                            try
                            {
                                rgssadv1.ExtractFile(archivedFile, outputDirectoryPath);
                            } catch (Exception)
                            {
                                Console.WriteLine("Failed to extract: {0}", archivedFile.Name);
                            }
                        }
                        break;
                    case RPGMakerVersion.VxAce:
                        RGSSADv3 rgssadv2 = new RGSSADv3(_commandLineOptions.InputPath);
                        rgssadv2.ExtractAllFiles(outputDirectoryPath);
                        break;
                }
            }
            catch (InvalidArchiveException)
            {
                Console.WriteLine("Archive is invalid or corrupted. Reading failed.");
                Environment.Exit(1);
            }
            catch (UnsupportedArchiveException)
            {
                Console.WriteLine("Archive is not supported or it is corrupted.");
                Environment.Exit(1);
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong with reading or extraction. Archive is likely invalid or corrupted.");
                Environment.Exit(1);
            }

            if (_commandLineOptions.GenerateProjectFile)
            {
                ProjectGenerator.GenerateProject(version, outputDirectoryPath);
            }
        }
    }
}

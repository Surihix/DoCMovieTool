﻿using DoCMovieTool.CryptoClasses;
using DoCMovieTool.SupportClasses;
using System;
using System.IO;
using static DoCMovieTool.SupportClasses.ToolEnums;
using static DoCMovieTool.SupportClasses.ToolHelpers;

namespace DoCMovieTool
{
    internal class MovieUnpack
    {
        public static void UnpackProcess(string inFile)
        {
            var movieArchiveName = Path.GetFileName(inFile);

            if (!NamesDict.ArchiveNames.ContainsKey(movieArchiveName))
            {
                ExitType.Error.ExitProgram("Specified filename does not match with a valid movie file archive names.\nCheck if the file was renamed.");
            }

            Console.WriteLine("");

            var fileRegion = NamesDict.ArchiveNames[Path.GetFileName(inFile)];
            var cryptoVariables = new CryptoVariables();
            cryptoVariables.KeyArray = fileRegion.DetermineKeyArray();

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    var fileCount = inFileReader.ReadUInt32();
                    var extractDir = Path.Combine(Path.GetDirectoryName(inFile), $"_{movieArchiveName}");

                    Console.WriteLine($"File Count: {fileCount}");
                    Console.WriteLine($"File Region: {fileRegion}");

                    if (Directory.Exists(extractDir))
                    {
                        Directory.Delete(extractDir, true);
                    }
                    Directory.CreateDirectory(extractDir);

                    var tocFile = Path.Combine(extractDir, "TOC");
                    using (var tocFileStream = new FileStream(tocFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        inFileStream.Seek(0, SeekOrigin.Begin);
                        inFileStream.ExCopyTo(tocFileStream, (fileCount * 32) + 4);
                    }

                    Console.WriteLine("");
                    Console.WriteLine("Unpacking movie files....");
                    Console.WriteLine("");
                    Console.WriteLine("");


                    using (var tocFileReader = new BinaryReader(File.Open(tocFile, FileMode.Open, FileAccess.Read)))
                    {
                        var movieVariables = new MovieVariables();
                        long readPos = 8;
                        long unkDataStart = new FileInfo(tocFile).Length;
                        long unkDataSize = 0;
                        var fileCounter = 1;

                        for (int m = 0; m < fileCount; m++)
                        {
                            tocFileReader.BaseStream.Position = readPos;
                            movieVariables.Start = tocFileReader.ReadUInt32() * 2048;
                            movieVariables.Size = tocFileReader.ReadUInt32();

                            // Unpack unk data
                            unkDataSize = movieVariables.Start - unkDataStart;
                            if (unkDataSize > 0)
                            {
                                var unkDataFile = Path.Combine(extractDir, $"UNKDATA_{fileCounter}");
                                using (var paddedDataStream = new FileStream(unkDataFile, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    inFileStream.Seek(unkDataStart, SeekOrigin.Begin);
                                    inFileStream.ExCopyTo(paddedDataStream, unkDataSize);
                                    Console.WriteLine($"Unpacked {Path.GetFileName(unkDataFile)}");
                                }
                            }

                            // Unpack movie data
                            var movieFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}.bin");
                            using (var movieStream = new FileStream(movieFile, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                Console.WriteLine($"Unpacking '{Path.GetFileName(movieFile)}'....");

                                if (movieVariables.Size % 16 != 0)
                                {
                                    inFileStream.Seek(movieVariables.Start, SeekOrigin.Begin);
                                    inFileStream.ExCopyTo(movieStream, movieVariables.Size - 4);

                                    var movieFooterFile = Path.Combine(extractDir, $"MOVIEFOOTER_{fileCounter}.bin");
                                    using (var movieFooterStream = new FileStream(movieFooterFile, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        inFileStream.ExCopyTo(movieFooterStream, 4);
                                    }
                                }
                                else
                                {
                                    inFileStream.Seek(movieVariables.Start, SeekOrigin.Begin);
                                    inFileStream.ExCopyTo(movieStream, movieVariables.Size);
                                }

                                Console.WriteLine("");
                            }

                            readPos += 32;
                            unkDataStart = movieVariables.Start + movieVariables.Size;
                            fileCounter++;
                        }

                        Console.WriteLine("");
                        Decryption.DecryptFiles(fileCount, extractDir, tocFileReader, cryptoVariables);
                    }
                }
            }

            Console.WriteLine("");
            ExitType.Success.ExitProgram($"Finished unpacking file '{Path.GetFileName(inFile)}'");
        }
    }
}
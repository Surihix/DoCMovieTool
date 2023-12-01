using DoCMovieTool.CryptoClasses;
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

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    var fileCount = inFileReader.ReadUInt32();
                    var extractDir = Path.Combine(Path.GetDirectoryName(inFile), $"_{movieArchiveName}");

                    Console.WriteLine("");
                    Console.WriteLine($"File Count: {fileCount}");
                    Console.WriteLine($"File Region: {NamesDict.ArchiveNames[Path.GetFileName(inFile)]}");

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
                        var cryptoVariables = new CryptoVariables();

                        long tocReadPos = 8;
                        long unkDataStart = new FileInfo(tocFile).Length;
                        long unkDataSize = 0;
                        var fileCounter = 1;

                        string unkDataFile;
                        string movieFile;

                        cryptoVariables.KeyArray = NamesDict.ArchiveNames[Path.GetFileName(inFile)].DetermineKeyArray();

                        for (int f = 0; f < fileCount; f++)
                        {
                            tocFileReader.BaseStream.Position = tocReadPos;
                            movieVariables.Start = tocFileReader.ReadUInt32() * 2048;
                            movieVariables.Size = tocFileReader.ReadUInt32();

                            unkDataSize = movieVariables.Start - unkDataStart;
                            if (unkDataSize > 0)
                            {
                                unkDataFile = Path.Combine(extractDir, $"UNKDATA_{fileCounter}");
                                using (var paddedDataStream = new FileStream(unkDataFile, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    inFileStream.Seek(unkDataStart, SeekOrigin.Begin);
                                    inFileStream.ExCopyTo(paddedDataStream, unkDataSize);

                                    Console.WriteLine($"Unpacked {Path.GetFileName(unkDataFile)}");
                                }
                            }

                            movieFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}");
                            using (var movieStream = new FileStream(movieFile, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                Console.WriteLine($"Unpacking MOVIEDATA_{fileCounter}....");

                                inFileStream.Seek(movieVariables.Start, SeekOrigin.Begin);
                                inFileStream.ExCopyTo(movieStream, movieVariables.Size);

                                Console.WriteLine("");
                            }

                            tocReadPos += 32;
                            unkDataStart = movieVariables.Start + movieVariables.Size;
                            fileCounter++;
                        }

                        if (inFileStream.Length > unkDataStart)
                        {
                            unkDataFile = Path.Combine(extractDir, $"UNKDATA_{fileCounter}");
                            using (var lastpaddedDataStream = new FileStream(unkDataFile, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                inFileStream.Seek(unkDataStart, SeekOrigin.Begin);
                                inFileStream.ExCopyTo(lastpaddedDataStream, unkDataSize);
                            }

                            Console.WriteLine($"Unpacked {Path.GetFileName(unkDataFile)}");
                            Console.WriteLine("");
                        }

                        Console.WriteLine("");
                        Console.WriteLine("Decrypting movie files....");
                        Console.WriteLine("");
                        Console.WriteLine("");

                        Decryption.DecryptFiles(fileCount, extractDir, tocFileReader, cryptoVariables);
                    }
                }
            }

            ExitType.Success.ExitProgram($"Finished unpacking file '{Path.GetFileName(inFile)}'");
        }
    }
}
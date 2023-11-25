using DoCMovieTool.CryptoClasses;
using DoCMovieTool.SupportClasses;
using System;
using System.IO;
using static DoCMovieTool.SupportClasses.FileStructs;
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
                ExitType.Error.ExitProgram("Specified filename does not match with valid movie file archive names.\nCheck if the file was renamed from its original name.");
            }

            Console.WriteLine("");

            var fileRegion = NamesDict.ArchiveNames[Path.GetFileName(inFile)];
            var keyArray = fileRegion.DetermineKeyArray();

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
                    Console.WriteLine("Extracted TOC file. unpacking movie files....");
                    Console.WriteLine("");
                    Console.WriteLine("");


                    using (var tocFileReader = new BinaryReader(File.Open(tocFile, FileMode.Open, FileAccess.Read)))
                    {
                        var movieInfo = new MovieInfo();
                        long readPos = 8;
                        long unkDataStart = new FileInfo(tocFile).Length;
                        long unkDataSize = 0;
                        var fileCounter = 1;

                        for (int m = 0; m < fileCount; m++)
                        {
                            tocFileReader.BaseStream.Position = readPos;
                            movieInfo.Start = tocFileReader.ReadUInt32() * 2048;
                            movieInfo.Size = tocFileReader.ReadUInt32();

                            // Unpack unk data
                            unkDataSize = movieInfo.Start - unkDataStart;
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

                                inFileStream.Seek(movieInfo.Start, SeekOrigin.Begin);
                                inFileStream.ExCopyTo(movieStream, movieInfo.Size);
                                Console.WriteLine("");
                            }

                            readPos += 32;
                            unkDataStart = movieInfo.Start + movieInfo.Size;
                            fileCounter++;
                        }

                        Decryption.DecryptFiles(extractDir, tocFileReader, keyArray);
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine($"Finished unpacking file '{Path.GetFileName(inFile)}'");
            Console.ReadLine();
        }
    }
}
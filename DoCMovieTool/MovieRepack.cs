using DoCMovieTool.CryptoClasses;
using DoCMovieTool.SupportClasses;
using System;
using System.IO;
using static DoCMovieTool.SupportClasses.ToolEnums;

namespace DoCMovieTool
{
    internal class MovieRepack
    {
        public static void RepackProcess(string extractedDir)
        {
            var newMovieArchiveName = Path.GetFileName(extractedDir).Replace("_", "");

            if (!NamesDict.ArchiveNames.ContainsKey(newMovieArchiveName))
            {
                ExitType.Error.ExitProgram("Specified foldername does not match with a valid movie file archive names.\nCheck if the extracted folder was renamed.");
            }

            Console.WriteLine("");

            var fileRegion = NamesDict.ArchiveNames[newMovieArchiveName];

            var cryptoVariables = new CryptoVariables();
            cryptoVariables.KeyArray = fileRegion.DetermineKeyArray();

            var tocFile = Path.Combine(extractedDir, "TOC");
            if (!File.Exists(tocFile))
            {
                ExitType.Error.ExitProgram("'TOC' file is missing");
            }

            var tmpTocFile = Path.Combine(extractedDir, "_TMP_TOC");
            tmpTocFile.IfFileExistsDel();
            File.WriteAllBytes(tmpTocFile, File.ReadAllBytes(tocFile));

            uint fileCount;
            using (var tocFileReader = new BinaryReader(File.Open(tocFile, FileMode.Open, FileAccess.Read)))
            {
                tocFileReader.BaseStream.Position = 0;
                fileCount = tocFileReader.ReadUInt32();

                Console.WriteLine($"File Count: {fileCount}");
                Console.WriteLine($"File Region: {fileRegion}");

                Console.WriteLine("");
                Console.WriteLine("Repacking movie files....");
                Console.WriteLine("");
                Console.WriteLine("");

                using (var tocFileWriter = new BinaryWriter(File.Open(tmpTocFile, FileMode.Open, FileAccess.Write)))
                {
                    var tmpArchiveFile = Path.Combine(extractedDir, "_TMP_ARCHIVE");
                    tmpArchiveFile.IfFileExistsDel();

                    using (var tmpArchiveStream = new FileStream(tmpArchiveFile, FileMode.Append, FileAccess.Write))
                    {
                        for (int p = 0; p < fileCount * 32 + 4; p++)
                        {
                            tmpArchiveStream.WriteByte(0);
                        }

                        var movieVariables = new MovieVariables();
                        long readPos = 28;
                        long writePos = 8;
                        var fileCounter = 1;
                        string unkDataFile;
                        string movieDataFile;
                        string tmpMovieDataFile;

                        for (int f = 0; f < fileCount; f++)
                        {
                            tocFileReader.BaseStream.Position = readPos;
                            cryptoVariables.MultiplyKey = tocFileReader.ReadUInt16();
                            cryptoVariables.Key1 = tocFileReader.ReadUInt16();
                            cryptoVariables.Key2 = tocFileReader.ReadUInt16();
                            cryptoVariables.Key3 = tocFileReader.ReadUInt16();

                            unkDataFile = Path.Combine(extractedDir, $"UNKDATA_{fileCounter}");
                            if (File.Exists(unkDataFile))
                            {
                                using (var unkDataStream = new FileStream(unkDataFile, FileMode.Open, FileAccess.Read))
                                {
                                    unkDataStream.Seek(0, SeekOrigin.Begin);
                                    unkDataStream.CopyTo(tmpArchiveStream);
                                }
                                Console.WriteLine($"Repacked '{Path.GetFileName(unkDataFile)}'");
                            }

                            // Pad nulls if start position is 
                            // not divisible by 2048
                            movieVariables.Start = (uint)tmpArchiveStream.Length;
                            if (movieVariables.Start % 2048 != 0)
                            {
                                var remainder = movieVariables.Start % 2048;
                                var increaseBytes = 2048 - remainder;
                                var newPos = movieVariables.Start + increaseBytes;
                                var nullAmount = newPos - movieVariables.Start;

                                movieVariables.Start = newPos;

                                for (int n = 0; n < nullAmount; n++)
                                {
                                    tmpArchiveStream.WriteByte(0);
                                }
                            }
                            movieVariables.Start /= 2048;

                            // Encrypt
                            Console.WriteLine($"Encrypting 'MOVIEDATA_{fileCounter}.bin'....");

                            movieDataFile = Path.Combine(extractedDir, $"MOVIEDATA_{fileCounter}.bin");
                            tmpMovieDataFile = Path.Combine(extractedDir, $"TMP_{Path.GetFileName(movieDataFile)}");
                            tmpMovieDataFile.IfFileExistsDel();

                            if (!File.Exists(movieDataFile))
                            {
                                ExitType.Error.ExitProgram($"Missing 'MOVIEDATA_{fileCounter}.bin' file");
                            }

                            Encryption.EncryptFile(movieDataFile, tmpMovieDataFile, cryptoVariables);

                            // Repack
                            Console.WriteLine($"Repacking 'MOVIEDATA_{fileCounter}.bin'....");

                            using (var tmpEncMovieDataStream = new FileStream(tmpMovieDataFile, FileMode.Open, FileAccess.Read))
                            {
                                movieVariables.Size = (uint)tmpEncMovieDataStream.Length;
                                tmpEncMovieDataStream.Seek(0, SeekOrigin.Begin);
                                tmpEncMovieDataStream.CopyTo(tmpArchiveStream);
                            }

                            var movieFooterFile = Path.Combine(extractedDir, $"MOVIEFOOTER_{fileCounter}.bin");
                            if (File.Exists(movieFooterFile))
                            {
                                using (var movieFooterStream = new FileStream(movieFooterFile, FileMode.Open, FileAccess.Read))
                                {
                                    movieVariables.Size += (uint)movieFooterStream.Length;
                                    movieFooterStream.Seek(0, SeekOrigin.Begin);
                                    movieFooterStream.CopyTo(tmpArchiveStream);
                                }
                            }

                            //
                            tmpMovieDataFile.IfFileExistsDel();

                            tocFileWriter.BaseStream.Position = writePos;
                            tocFileWriter.Write(movieVariables.Start);
                            tocFileWriter.Write(movieVariables.Size);

                            Console.WriteLine("");

                            readPos += 32;
                            writePos += 32;
                            fileCounter++;
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Rebuilding new archive....");

            var newMovieArchiveFile = Path.Combine(Path.GetDirectoryName(extractedDir), newMovieArchiveName);
            newMovieArchiveFile.IfFileExistsDel();

            using (var newArchiveStream = new FileStream(newMovieArchiveFile, FileMode.Append, FileAccess.Write))
            {
                using (var newTOCstream = new FileStream(tmpTocFile, FileMode.Open, FileAccess.Read))
                {
                    newTOCstream.Seek(0, SeekOrigin.Begin);
                    newTOCstream.CopyTo(newArchiveStream);
                }

                tmpTocFile.IfFileExistsDel();

                using (var newTmpArchiveStream = new FileStream(Path.Combine(extractedDir, "_TMP_ARCHIVE"), FileMode.Open, FileAccess.Read))
                {
                    newTmpArchiveStream.Seek(fileCount * 32 + 4, SeekOrigin.Begin);
                    newTmpArchiveStream.CopyTo(newArchiveStream);
                }

                Path.Combine(extractedDir, "_TMP_ARCHIVE").IfFileExistsDel();
            }

            Console.WriteLine("");
            ExitType.Success.ExitProgram($"Finished repacking files to {newMovieArchiveName}");
        }
    }
}
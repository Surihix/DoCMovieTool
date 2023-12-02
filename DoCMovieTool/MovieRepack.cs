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
                ExitType.Error.ExitProgram("Specified foldername does not match with valid movie file archive names.");
            }

            var tocFile = Path.Combine(extractedDir, "TOC");
            if (!File.Exists(tocFile))
            {
                ExitType.Error.ExitProgram("TOC file is missing");
            }

            var tmpTocFile = Path.Combine(extractedDir, "_TMP_TOC");
            tmpTocFile.IfFileExistsDel();

            File.WriteAllBytes(tmpTocFile, File.ReadAllBytes(tocFile));


            uint fileCount;
            var tmpArchiveFile = Path.Combine(extractedDir, "_TMP_ARCHIVE");

            using (var tocFileReader = new BinaryReader(File.Open(tocFile, FileMode.Open, FileAccess.Read)))
            {
                tocFileReader.BaseStream.Position = 0;
                fileCount = tocFileReader.ReadUInt32();

                Console.WriteLine("");
                Console.WriteLine($"File Count: {fileCount}");
                Console.WriteLine($"File Region: {NamesDict.ArchiveNames[newMovieArchiveName]}");

                Console.WriteLine("");
                Console.WriteLine("Repacking movie files....");
                Console.WriteLine("");
                Console.WriteLine("");

                var movieDataFilesArray = new string[fileCount];
                var fileCounter = 1;

                for (int a = 0; a < fileCount; a++)
                {
                    var fileInDir = Directory.GetFiles(extractedDir, $"MOVIEDATA_{fileCounter}.*", SearchOption.TopDirectoryOnly);
                    if (fileInDir.Length > 1)
                    {
                        ExitType.Error.ExitProgram($"Detected one or more 'MOVIEDATA_{fileCounter}' files. remove the secondary files from the extracted folder.");
                    }

                    if (fileInDir.Length == 0)
                    {
                        ExitType.Error.ExitProgram($"Unable to locate a file beginning with the name 'MOVIEDATA_{fileCounter}'. ensure that this file is present in the extraced folder.");
                    }

                    movieDataFilesArray[a] = fileInDir[0];

                    fileCounter++;
                }

                using (var tocFileWriter = new BinaryWriter(File.Open(tmpTocFile, FileMode.Open, FileAccess.Write)))
                {
                    tmpArchiveFile.IfFileExistsDel();

                    using (var tmpArchiveStream = new FileStream(tmpArchiveFile, FileMode.Append, FileAccess.Write))
                    {
                        PadNulls(fileCount * 32 + 4, tmpArchiveStream);


                        var movieVariables = new MovieVariables();
                        var cryptoVariables = new CryptoVariables();

                        long tocReadPos = 28;
                        long tocWritePos = 8;
                        fileCounter = 1;

                        string unkDataFile;
                        string movieDataFile;
                        string tmpMovieDataFile;

                        uint remainder;
                        uint increaseBytes;
                        uint newPos;
                        uint nullAmount;

                        cryptoVariables.KeyArray = NamesDict.ArchiveNames[newMovieArchiveName].DetermineKeyArray();

                        for (int f = 0; f < fileCount; f++)
                        {
                            tocFileReader.BaseStream.Position = tocReadPos;
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
                                Console.WriteLine($"Repacked {Path.GetFileName(unkDataFile)}");
                            }

                            movieVariables.Start = (uint)tmpArchiveStream.Length;
                            if (movieVariables.Start % 2048 != 0)
                            {
                                remainder = movieVariables.Start % 2048;
                                increaseBytes = 2048 - remainder;
                                newPos = movieVariables.Start + increaseBytes;
                                nullAmount = newPos - movieVariables.Start;

                                movieVariables.Start = newPos;

                                PadNulls(nullAmount, tmpArchiveStream);
                            }
                            movieVariables.Start /= 2048;

                            Console.WriteLine($"Encrypting MOVIEDATA_{fileCounter}....");

                            movieDataFile = movieDataFilesArray[f];
                            tmpMovieDataFile = Path.Combine(extractedDir, $"TMP_MOVIEDATA_{fileCounter}");
                            tmpMovieDataFile.IfFileExistsDel();

                            Encryption.EncryptFile(movieDataFile, tmpMovieDataFile, cryptoVariables);

                            Console.WriteLine($"Repacking MOVIEDATA_{fileCounter}....");

                            using (var tmpMovieDataStream = new FileStream(tmpMovieDataFile, FileMode.Open, FileAccess.Read))
                            {
                                movieVariables.Size = (uint)tmpMovieDataStream.Length;
                                tmpMovieDataStream.Seek(0, SeekOrigin.Begin);
                                tmpMovieDataStream.CopyTo(tmpArchiveStream);
                            }

                            Console.WriteLine("");

                            tmpMovieDataFile.IfFileExistsDel();

                            tocFileWriter.BaseStream.Position = tocWritePos;
                            tocFileWriter.Write(movieVariables.Start);
                            tocFileWriter.Write(movieVariables.Size);

                            tocReadPos += 32;
                            tocWritePos += 32;
                            fileCounter++;
                        }

                        if (tmpArchiveStream.Length % 2048 != 0)
                        {
                            unkDataFile = Path.Combine(extractedDir, $"UNKDATA_{fileCounter}");
                            if (File.Exists(unkDataFile))
                            {
                                using (var lastPaddedStream = new FileStream(unkDataFile, FileMode.Open, FileAccess.Read))
                                {
                                    lastPaddedStream.Seek(0, SeekOrigin.Begin);
                                    lastPaddedStream.CopyTo(tmpArchiveStream);
                                }

                                if (tmpArchiveStream.Length % 2048 != 0)
                                {
                                    PrepPadding(tmpArchiveStream.Length, tmpArchiveStream);
                                }

                                Console.WriteLine($"Repacked {Path.GetFileName(unkDataFile)}");
                                Console.WriteLine("");
                            }
                            else
                            {
                                if (tmpArchiveStream.Length % 2048 != 0)
                                {
                                    PrepPadding(tmpArchiveStream.Length, tmpArchiveStream);
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Rebuilding new archive....");
            Console.WriteLine("");

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

                using (var newTmpArchiveStream = new FileStream(tmpArchiveFile, FileMode.Open, FileAccess.Read))
                {
                    newTmpArchiveStream.Seek(fileCount * 32 + 4, SeekOrigin.Begin);
                    newTmpArchiveStream.CopyTo(newArchiveStream);
                }

                tmpArchiveFile.IfFileExistsDel();
            }

            ExitType.Success.ExitProgram($"Finished repacking files to '{newMovieArchiveName}'");
        }


        static void PadNulls(uint padAmount, FileStream streamToPad)
        {
            for (int p = 0; p < padAmount; p++)
            {
                streamToPad.WriteByte(0);
            }
        }

        static void PrepPadding(long startPos, FileStream tmpArchiveStream)
        {
            var remainder = startPos % 2048;
            var increaseBytes = 2048 - remainder;
            var newPos = startPos + increaseBytes;
            var nullAmount = (uint)(newPos - startPos);

            PadNulls(nullAmount, tmpArchiveStream);
        }
    }
}
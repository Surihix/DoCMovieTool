using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace DoCMovieTool.CryptoClasses
{
    internal class Decryption
    {
        static uint EncryptedBytesVal1;

        static uint EncryptedBytesVal2;

        static uint EncryptedBytesVal3;

        static uint EncryptedBytesVal4;

        public static void DecryptFiles(uint fileCount, string extractDir, BinaryReader tocFileReader, CryptoVariables cryptoVariables)
        {
            using (var movieInfoStreamWriter = new StreamWriter(Path.Combine(extractDir, "#VALID_MOVIES.txt"), true))
            {

                long readPos = 28;
                int fileCounter = 1;

                for (int d = 0; d < fileCount; d++)
                {
                    var currentFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}.bin");

                    Console.WriteLine($"Decrypting '{Path.GetFileName(currentFile)}'....");

                    tocFileReader.BaseStream.Position = readPos;
                    cryptoVariables.MultiplyKey = tocFileReader.ReadUInt16();
                    cryptoVariables.Key1 = tocFileReader.ReadUInt16();
                    cryptoVariables.Key2 = tocFileReader.ReadUInt16();
                    cryptoVariables.Key3 = tocFileReader.ReadUInt16();

                    var totalFileLength = new FileInfo(currentFile).Length;
                    var bytesToProcess = totalFileLength;
                    var computeBytes = true;

                    if (File.Exists(Path.Combine(extractDir, $"MOVIEFOOTER_{fileCounter}.bin")))
                    {
                        movieInfoStreamWriter.WriteLine($"MOVIEDATA_{fileCounter}.bin");
                    }

                    if (bytesToProcess % 16 != 0)
                    {
                        var remainder = bytesToProcess % 16;
                        bytesToProcess -= remainder;
                    }

                    if (bytesToProcess < 16)
                    {
                        computeBytes = false;
                    }

                    long fileReadPos = 0;

                    using (var mmf = MemoryMappedFile.CreateFromFile(currentFile, FileMode.Open))
                    {
                        using (var accessor = mmf.CreateViewAccessor())
                        {
                            while (computeBytes)
                            {
                                EncryptedBytesVal1 = accessor.ReadUInt32(fileReadPos);
                                EncryptedBytesVal2 = accessor.ReadUInt32(fileReadPos + 4);
                                EncryptedBytesVal3 = accessor.ReadUInt32(fileReadPos + 8);
                                EncryptedBytesVal4 = accessor.ReadUInt32(fileReadPos + 12);

                                CryptoBase.PrepKeyValues(cryptoVariables);

                                accessor.Write(fileReadPos, EncryptedBytesVal1 ^ cryptoVariables.CombinedKey1);
                                accessor.Write(fileReadPos + 4, EncryptedBytesVal2 ^ cryptoVariables.CombinedKey2);
                                accessor.Write(fileReadPos + 8, EncryptedBytesVal3 ^ cryptoVariables.CombinedKey1);
                                accessor.Write(fileReadPos + 12, EncryptedBytesVal4 ^ cryptoVariables.CombinedKey2);

                                CryptoBase.PrepNextKeyValues(cryptoVariables);

                                fileReadPos += 16;

                                if (fileReadPos == bytesToProcess)
                                {
                                    computeBytes = false;
                                }
                            }
                        }
                    }

                    Console.WriteLine("");

                    readPos += 32;
                    fileCounter++;
                }
            }
        }
    }
}
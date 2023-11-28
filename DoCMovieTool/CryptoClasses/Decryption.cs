using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using static DoCMovieTool.SupportClasses.FileStructs;

namespace DoCMovieTool.CryptoClasses
{
    internal class Decryption
    {
        static uint EncryptedBytesVal1;
        static uint EncryptedBytesVal2;
        static uint EncryptedBytesVal3;
        static uint EncryptedBytesVal4;

        static uint CombinedKey1;
        static uint CombinedKey2;

        static int KeyArrayIndex;
        static uint CurrentKeyArrayVal1;
        static uint CurrentKeyArrayVal2;

        public static void DecryptFiles(uint fileCount, string extractDir, BinaryReader tocFileReader, uint[] keyArray)
        {
            using (var movieInfoStreamWriter = new StreamWriter(Path.Combine(extractDir, "#VALID_MOVIES.txt"), true))
            {

                var keyInfo = new KeyInfo();
                long readPos = 28;
                int fileCounter = 1;

                for (int d = 0; d < fileCount; d++)
                {
                    var currentFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}.bin");

                    Console.WriteLine($"Decrypting '{Path.GetFileName(currentFile)}'....");

                    tocFileReader.BaseStream.Position = readPos;
                    keyInfo.MultiplyKey = tocFileReader.ReadUInt16();
                    keyInfo.Key1 = tocFileReader.ReadUInt16();
                    keyInfo.Key2 = tocFileReader.ReadUInt16();
                    keyInfo.Key3 = tocFileReader.ReadUInt16();

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

                                keyInfo.Key1 *= keyInfo.MultiplyKey;
                                keyInfo.Key2 *= keyInfo.MultiplyKey;
                                keyInfo.Key3 *= keyInfo.MultiplyKey;

                                CombinedKey1 = keyInfo.Key1.UShortsToUInt(keyInfo.Key2);
                                CombinedKey2 = keyInfo.Key2.UShortsToUInt(keyInfo.Key3);

                                accessor.Write(fileReadPos, EncryptedBytesVal1 ^ CombinedKey1);
                                accessor.Write(fileReadPos + 4, EncryptedBytesVal2 ^ CombinedKey2);
                                accessor.Write(fileReadPos + 8, EncryptedBytesVal3 ^ CombinedKey1);
                                accessor.Write(fileReadPos + 12, EncryptedBytesVal4 ^ CombinedKey2);

                                CurrentKeyArrayVal1 = keyArray[KeyArrayIndex / 2];
                                CurrentKeyArrayVal2 = keyArray[(KeyArrayIndex / 2) + 1];

                                keyInfo.MultiplyKey ^= (ushort)CurrentKeyArrayVal1;
                                keyInfo.Key1 ^= (ushort)(CurrentKeyArrayVal1 >> 16);
                                keyInfo.Key2 ^= (ushort)CurrentKeyArrayVal2;
                                keyInfo.Key3 ^= (ushort)(CurrentKeyArrayVal2 >> 16);

                                KeyArrayIndex += 4;
                                KeyArrayIndex &= 255;
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
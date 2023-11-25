using System;
using System.IO;
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
            Console.WriteLine("");

            var keyInfo = new KeyInfo();
            long readPos = 28;
            int fileCounter = 1;

            for (int d = 0; d < fileCount; d++)
            {
                var currentFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}.bin" );
                var decryptedOutFile = Path.Combine(extractDir, $"{Path.GetFileNameWithoutExtension(currentFile)}.dec");

                Console.WriteLine($"Decrypting '{Path.GetFileName(currentFile)}'....");

                tocFileReader.BaseStream.Position = readPos;
                keyInfo.MultiplyKey = tocFileReader.ReadUInt16();
                keyInfo.Key1 = tocFileReader.ReadUInt16();
                keyInfo.Key2 = tocFileReader.ReadUInt16();
                keyInfo.Key3 = tocFileReader.ReadUInt16();

                long fileReadPos = 0;

                using (var encryptedStream = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
                {
                    using (var encrytedStreamBinReader = new BinaryReader(encryptedStream))
                    {
                        using (var decryptedStream = new FileStream(decryptedOutFile, FileMode.Append, FileAccess.Write))
                        {
                            using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                            {

                                var bytesToProcess = encryptedStream.Length;
                                var computeBytes = true;

                                if (encryptedStream.Length % 16 != 0)
                                {
                                    var remainder = bytesToProcess % 16;
                                    bytesToProcess -= remainder;

                                    if (bytesToProcess < 16)
                                    {
                                        computeBytes = false;
                                    }
                                }

                                encrytedStreamBinReader.BaseStream.Position = fileReadPos;
                                
                                while (computeBytes)
                                {
                                    EncryptedBytesVal1 = encrytedStreamBinReader.ReadUInt32();
                                    EncryptedBytesVal2 = encrytedStreamBinReader.ReadUInt32();
                                    EncryptedBytesVal3 = encrytedStreamBinReader.ReadUInt32();
                                    EncryptedBytesVal4 = encrytedStreamBinReader.ReadUInt32();

                                    keyInfo.Key1 *= keyInfo.MultiplyKey;
                                    keyInfo.Key2 *= keyInfo.MultiplyKey;
                                    keyInfo.Key3 *= keyInfo.MultiplyKey;

                                    CombinedKey1 = keyInfo.Key1.UShortsToUInt(keyInfo.Key2);
                                    CombinedKey2 = keyInfo.Key2.UShortsToUInt(keyInfo.Key3);

                                    decryptedStreamBinWriter.Write(EncryptedBytesVal1 ^ CombinedKey1);
                                    decryptedStreamBinWriter.Write(EncryptedBytesVal2 ^ CombinedKey2);
                                    decryptedStreamBinWriter.Write(EncryptedBytesVal3 ^ CombinedKey1);
                                    decryptedStreamBinWriter.Write(EncryptedBytesVal4 ^ CombinedKey2);

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

                                var remainingBytes = encryptedStream.Length - fileReadPos;

                                if (remainingBytes > 0)
                                {
                                    encrytedStreamBinReader.BaseStream.Position = fileReadPos;
                                    encryptedStream.CopyTo(decryptedStream);
                                }
                            }
                        }
                    }
                }

                File.Delete(currentFile);
                File.Move(decryptedOutFile, currentFile);

                Console.WriteLine("");

                readPos += 32;
                fileCounter++;
            }
        }
    }
}
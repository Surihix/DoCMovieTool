using System;
using System.IO;
using static DoCMovieTool.SupportClasses.FileStructs;

namespace DoCMovieTool.CryptoClasses
{
    internal class Decryption
    {
        public static void DecryptFiles(uint fileCount, string extractDir, BinaryReader tocFileReader)
        {
            Console.WriteLine("");

            var keyInfo = new KeyInfo();
            long readPos = 28;
            int fileCounter = 1;

            var extractDirMovies = Directory.GetFiles(extractDir, "MOVIEDATA_*.bin");

            for (int d = 0; d < extractDirMovies.Length; d++)
            {
                Console.WriteLine($"Decrypting '{Path.GetFileName(extractDirMovies[d])}'....");

                tocFileReader.BaseStream.Position = readPos;
                keyInfo.MultiplyKey = tocFileReader.ReadUInt16();
                keyInfo.Key1 = tocFileReader.ReadUInt16();
                keyInfo.Key2 = tocFileReader.ReadUInt16();
                keyInfo.Key3 = tocFileReader.ReadUInt16();

                Console.WriteLine("");

                readPos += 32;
                fileCounter++;
            }
        }
    }
}
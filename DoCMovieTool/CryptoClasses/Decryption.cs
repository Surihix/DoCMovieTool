using System;
using System.IO;

namespace DoCMovieTool.CryptoClasses
{
    internal class Decryption
    {
        public static void DecryptFiles(string validMoviesTxtFile, uint fileCount, string extractDir, BinaryReader tocFileReader, CryptoVariables cryptoVariables)
        {
            using (var movieInfoStreamWriter = new StreamWriter(validMoviesTxtFile, true))
            {

                long tocReadPos = 28;
                int fileCounter = 1;

                string currentFile;

                for (int d = 0; d < fileCount; d++)
                {
                    currentFile = Path.Combine(extractDir, $"MOVIEDATA_{fileCounter}.bin");

                    Console.WriteLine($"Decrypting {Path.GetFileName(currentFile)}....");

                    tocFileReader.BaseStream.Position = tocReadPos;
                    cryptoVariables.MultiplyKey = tocFileReader.ReadUInt16();
                    cryptoVariables.Key1 = tocFileReader.ReadUInt16();
                    cryptoVariables.Key2 = tocFileReader.ReadUInt16();
                    cryptoVariables.Key3 = tocFileReader.ReadUInt16();

                    cryptoVariables.BytesToProcess = new FileInfo(currentFile).Length;
                    cryptoVariables.ComputeBytes = true;

                    cryptoVariables.BytesLeftOut = 0;
                    if (cryptoVariables.BytesToProcess % 16 != 0)
                    {
                        cryptoVariables.BytesLeftOut = cryptoVariables.BytesToProcess % 16;
                        cryptoVariables.BytesToProcess -= cryptoVariables.BytesLeftOut;
                    }

                    if (cryptoVariables.BytesToProcess < 16)
                    {
                        cryptoVariables.ComputeBytes = false;
                    }
                    else
                    {
                        cryptoVariables.ComputeBytes = true;
                    }

                    cryptoVariables.IsMovie = false;
                    CryptoBase.CryptOperation(currentFile, cryptoVariables);

                    if (cryptoVariables.IsMovie)
                    {
                        movieInfoStreamWriter.WriteLine($"MOVIEDATA_{fileCounter}.bin");
                        cryptoVariables.IsMovie = false;
                    }

                    Console.WriteLine("");

                    tocReadPos += 32;
                    fileCounter++;
                }
            }
        }
    }
}
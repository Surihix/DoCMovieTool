using System.IO;
using System.IO.MemoryMappedFiles;

namespace DoCMovieTool.CryptoClasses
{
    internal class Encryption
    {
        static uint DecryptedBytesVal1;

        static uint DecryptedBytesVal2;

        static uint DecryptedBytesVal3;

        static uint DecryptedBytesVal4;

        public static void EncryptFile(string movieDataFile, string tmpMovieDataFile, CryptoVariables cryptoVariables)
        {
            using (var movieDataStream = new FileStream(movieDataFile, FileMode.Open, FileAccess.Read))
            {
                using (var tmpMovieDataStream = new FileStream(tmpMovieDataFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    movieDataStream.Seek(0, SeekOrigin.Begin);
                    movieDataStream.CopyTo(tmpMovieDataStream);
                }
            }

            long totalFileLength = new FileInfo(tmpMovieDataFile).Length;
            var bytesToProcess = totalFileLength;
            bool computeBytes = true;

            if (bytesToProcess % 16 != 0)
            {
                var remainder = bytesToProcess % 16;
                bytesToProcess -= remainder;
            }

            if (bytesToProcess < 16)
            {
                computeBytes = false;
            }


            long fileReadWritePos = 0;

            using (var mmf = MemoryMappedFile.CreateFromFile(tmpMovieDataFile, FileMode.Open))
            {
                using (var accessor = mmf.CreateViewAccessor())
                {
                    while (computeBytes)
                    {
                        DecryptedBytesVal1 = accessor.ReadUInt32(fileReadWritePos);
                        DecryptedBytesVal2 = accessor.ReadUInt32(fileReadWritePos + 4);
                        DecryptedBytesVal3 = accessor.ReadUInt32(fileReadWritePos + 8);
                        DecryptedBytesVal4 = accessor.ReadUInt32(fileReadWritePos + 12);

                        CryptoBase.PrepKeyValues(cryptoVariables);

                        accessor.Write(fileReadWritePos, cryptoVariables.CombinedKey1 ^ DecryptedBytesVal1);
                        accessor.Write(fileReadWritePos + 4, cryptoVariables.CombinedKey2 ^ DecryptedBytesVal2);
                        accessor.Write(fileReadWritePos + 8, cryptoVariables.CombinedKey1 ^ DecryptedBytesVal3);
                        accessor.Write(fileReadWritePos + 12, cryptoVariables.CombinedKey2 ^ DecryptedBytesVal4);

                        CryptoBase.PrepNextKeyValues(cryptoVariables);

                        fileReadWritePos += 16;

                        if (fileReadWritePos == bytesToProcess)
                        {
                            computeBytes = false;
                        }
                    }
                }
            }
        }
    }
}
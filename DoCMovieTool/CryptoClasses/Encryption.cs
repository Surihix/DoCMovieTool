using System.IO;
using System.IO.MemoryMappedFiles;
using static DoCMovieTool.SupportClasses.FileStructs;

namespace DoCMovieTool.CryptoClasses
{
    internal class Encryption
    {
        static uint DecryptedBytesVal1;
        static uint DecryptedBytesVal2;
        static uint DecryptedBytesVal3;
        static uint DecryptedBytesVal4;

        static uint CombinedKey1;
        static uint CombinedKey2;

        static int KeyArrayIndex;
        static uint CurrentKeyArrayVal1;
        static uint CurrentKeyArrayVal2;

        public static void EncryptFile(string extractedDir, string movieDataFile, string tmpMovieDataFile, KeyInfo keyInfo, uint[] keyArray)
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

            //using (var mmf = MemoryMappedFile.CreateFromFile(tmpMovieDataFile, FileMode.Open))
            //{
            //    using (var accessor = mmf.CreateViewAccessor())
            //    {
            //        while (computeBytes)
            //        {
            //            DecryptedBytesVal1 = accessor.ReadUInt32(fileReadWritePos);
            //            DecryptedBytesVal2 = accessor.ReadUInt32(fileReadWritePos + 4);
            //            DecryptedBytesVal3 = accessor.ReadUInt32(fileReadWritePos + 8);
            //            DecryptedBytesVal4 = accessor.ReadUInt32(fileReadWritePos + 12);

            //            CurrentKeyArrayVal1 = keyArray[KeyArrayIndex / 2];
            //            CurrentKeyArrayVal2 = keyArray[(KeyArrayIndex / 2) + 1];





            //            KeyArrayIndex += 4;
            //            KeyArrayIndex &= 255;
            //            fileReadWritePos += 16;
            //        }
            //    }
            //}
        }
    }
}
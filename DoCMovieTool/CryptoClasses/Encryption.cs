using System.IO;

namespace DoCMovieTool.CryptoClasses
{
    internal class Encryption
    {
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

            cryptoVariables.BytesToProcess = new FileInfo(tmpMovieDataFile).Length;
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

            CryptoBase.CryptOperation(tmpMovieDataFile, cryptoVariables);
        }
    }
}
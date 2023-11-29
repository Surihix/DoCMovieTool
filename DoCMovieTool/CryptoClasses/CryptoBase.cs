using System.IO;
using System.IO.MemoryMappedFiles;

namespace DoCMovieTool.CryptoClasses
{
    internal class CryptoBase
    {
        public static void CryptOperation(string fileToProcess, CryptoVariables cryptoVariables)
        {
            cryptoVariables.ReadWritePos = 0;

            using (var mmf = MemoryMappedFile.CreateFromFile(fileToProcess, FileMode.Open))
            {
                using (var accessor = mmf.CreateViewAccessor())
                {
                    while (cryptoVariables.ComputeBytes)
                    {
                        cryptoVariables.ByteVal1 = accessor.ReadUInt32(cryptoVariables.ReadWritePos);
                        cryptoVariables.ByteVal2 = accessor.ReadUInt32(cryptoVariables.ReadWritePos + 4);
                        cryptoVariables.ByteVal3 = accessor.ReadUInt32(cryptoVariables.ReadWritePos + 8);
                        cryptoVariables.ByteVal4 = accessor.ReadUInt32(cryptoVariables.ReadWritePos + 12);

                        PrepKeyValues(cryptoVariables);

                        accessor.Write(cryptoVariables.ReadWritePos, cryptoVariables.ByteVal1 ^ cryptoVariables.CombinedKey1);
                        accessor.Write(cryptoVariables.ReadWritePos + 4, cryptoVariables.ByteVal2 ^ cryptoVariables.CombinedKey2);
                        accessor.Write(cryptoVariables.ReadWritePos + 8, cryptoVariables.ByteVal3 ^ cryptoVariables.CombinedKey1);
                        accessor.Write(cryptoVariables.ReadWritePos + 12, cryptoVariables.ByteVal4 ^ cryptoVariables.CombinedKey2);

                        cryptoVariables.CurrentKeyArrayVal1 = cryptoVariables.KeyArray[cryptoVariables.KeyArrayIndex / 2];
                        cryptoVariables.CurrentKeyArrayVal2 = cryptoVariables.KeyArray[(cryptoVariables.KeyArrayIndex / 2) + 1];

                        cryptoVariables.MultiplyKey ^= (ushort)cryptoVariables.CurrentKeyArrayVal1;
                        cryptoVariables.Key1 ^= (ushort)(cryptoVariables.CurrentKeyArrayVal1 >> 16);
                        cryptoVariables.Key2 ^= (ushort)cryptoVariables.CurrentKeyArrayVal2;
                        cryptoVariables.Key3 ^= (ushort)(cryptoVariables.CurrentKeyArrayVal2 >> 16);

                        cryptoVariables.KeyArrayIndex += 4;
                        cryptoVariables.KeyArrayIndex &= 255;

                        cryptoVariables.ReadWritePos += 16;

                        if (cryptoVariables.ReadWritePos == cryptoVariables.BytesToProcess)
                        {
                            cryptoVariables.ComputeBytes = false;
                        }
                    }

                    if (cryptoVariables.BytesLeftOut == 4)
                    {
                        cryptoVariables.ByteVal1 = accessor.ReadUInt32(cryptoVariables.ReadWritePos);

                        PrepKeyValues(cryptoVariables);

                        accessor.Write(cryptoVariables.ReadWritePos, cryptoVariables.ByteVal1 ^ cryptoVariables.CombinedKey1);

                        cryptoVariables.IsMovie = true;
                    }
                }
            }
        }

        static void PrepKeyValues(CryptoVariables cryptoVariables)
        {
            cryptoVariables.Key1 *= cryptoVariables.MultiplyKey;
            cryptoVariables.Key2 *= cryptoVariables.MultiplyKey;
            cryptoVariables.Key3 *= cryptoVariables.MultiplyKey;

            cryptoVariables.CombinedKey1 = cryptoVariables.Key1.UShortsToUInt(cryptoVariables.Key2);
            cryptoVariables.CombinedKey2 = cryptoVariables.Key2.UShortsToUInt(cryptoVariables.Key3);
        }
    }
}
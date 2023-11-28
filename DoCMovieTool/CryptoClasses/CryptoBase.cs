namespace DoCMovieTool.CryptoClasses
{
    internal class CryptoBase
    {
        public static void PrepKeyValues(CryptoVariables cryptoVariables)
        {
            cryptoVariables.Key1 *= cryptoVariables.MultiplyKey;
            cryptoVariables.Key2 *= cryptoVariables.MultiplyKey;
            cryptoVariables.Key3 *= cryptoVariables.MultiplyKey;

            cryptoVariables.CombinedKey1 = cryptoVariables.Key1.UShortsToUInt(cryptoVariables.Key2);
            cryptoVariables.CombinedKey2 = cryptoVariables.Key2.UShortsToUInt(cryptoVariables.Key3);
        }

        public static void PrepNextKeyValues(CryptoVariables cryptoVariables)
        {
            cryptoVariables.CurrentKeyArrayVal1 = cryptoVariables.KeyArray[cryptoVariables.KeyArrayIndex / 2];
            cryptoVariables.CurrentKeyArrayVal2 = cryptoVariables.KeyArray[(cryptoVariables.KeyArrayIndex / 2) + 1];

            cryptoVariables.MultiplyKey ^= (ushort)cryptoVariables.CurrentKeyArrayVal1;
            cryptoVariables.Key1 ^= (ushort)(cryptoVariables.CurrentKeyArrayVal1 >> 16);
            cryptoVariables.Key2 ^= (ushort)cryptoVariables.CurrentKeyArrayVal2;
            cryptoVariables.Key3 ^= (ushort)(cryptoVariables.CurrentKeyArrayVal2 >> 16);

            cryptoVariables.KeyArrayIndex += 4;
            cryptoVariables.KeyArrayIndex &= 255;
        }
    }
}
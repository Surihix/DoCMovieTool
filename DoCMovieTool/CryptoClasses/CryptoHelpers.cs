using System;
using static DoCMovieTool.SupportClasses.ToolHelpers;

namespace DoCMovieTool.CryptoClasses
{
    internal static class CryptoHelpers
    {
        public static uint[] DetermineKeyArray(this FileRegion fileRegion)
        {
            uint[] keyArray = new uint[] { };

            switch (fileRegion)
            {
                case FileRegion.JORG:
                    keyArray = KeyArrays.KeysJORG;
                    break;

                case FileRegion.JINT:
                    keyArray = KeyArrays.KeysJINT;
                    break;

                case FileRegion.NA:
                    keyArray = KeyArrays.KeysNA;
                    break;

                case FileRegion.EU:
                    keyArray = KeyArrays.KeysEU;
                    break;
            }

            return keyArray;
        }

        public static uint UShortsToUInt(this ushort value1, ushort value2)
        {
            return Convert.ToUInt32(value1.ToString("X4") + "" + value2.ToString("X4"), 16);
        }
    }
}
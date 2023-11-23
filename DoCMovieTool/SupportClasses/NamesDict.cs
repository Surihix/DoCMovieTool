using System.Collections.Generic;
using static DoCMovieTool.SupportClasses.ToolHelpers;

namespace DoCMovieTool.SupportClasses
{
    internal class NamesDict
    {
        public static readonly Dictionary<string, FileRegion> ArchiveNames = new Dictionary<string, FileRegion>
        {
            { "7570F45E.F7", FileRegion.JORG },
            { "D8F7BC60.45", FileRegion.JORG },

            { "5671C560.68", FileRegion.JINT },
            { "B1A04F8B.4C", FileRegion.JINT },

            { "23CFDD41.F7", FileRegion.NA },
            { "B08ED50C.AA", FileRegion.NA },

            { "200ECE2F.5C", FileRegion.EU },
            { "B18C4B8C.FC", FileRegion.EU }
        };
    }
}
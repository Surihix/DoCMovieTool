namespace DoCMovieTool.SupportClasses
{
    internal class FileStructs
    {
        public struct MovieInfo
        {
            public uint Start;
            public uint Size;
        }

        public struct KeyInfo
        {
            public ushort MultiplyKey;
            public ushort Key1;
            public ushort Key2;
            public ushort Key3;
        }
    }
}
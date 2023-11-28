namespace DoCMovieTool.CryptoClasses
{
    internal class CryptoVariables
    {
        public ushort MultiplyKey { get; set; }
        public ushort Key1 { get; set; }
        public ushort Key2 { get; set; }
        public ushort Key3 { get; set; }

        public uint CombinedKey1 { get; set; }
        public uint CombinedKey2 { get; set; }

        public uint[] KeyArray { get; set; }
        public int KeyArrayIndex { get; set; }
        public uint CurrentKeyArrayVal1 { get; set; }
        public uint CurrentKeyArrayVal2 { get; set; }
    }
}
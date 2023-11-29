namespace DoCMovieTool.CryptoClasses
{
    internal class CryptoVariables
    {
        public ushort MultiplyKey { get; set; }
        public ushort Key1 { get; set; }
        public ushort Key2 { get; set; }
        public ushort Key3 { get; set; }

        public long BytesToProcess { get; set; }
        public long BytesLeftOut { get; set; }
        public long ReadWritePos { get; set; }
        public bool ComputeBytes { get; set; }
        public uint ByteVal1 { get; set; }
        public uint ByteVal2 { get; set; }
        public uint ByteVal3 { get; set; }
        public uint ByteVal4 { get; set; }
        public bool IsMovie { get; set; }

        public uint CombinedKey1 { get; set; }
        public uint CombinedKey2 { get; set; }

        public uint[] KeyArray { get; set; }
        public int KeyArrayIndex { get; set; }
        public uint CurrentKeyArrayVal1 { get; set; }
        public uint CurrentKeyArrayVal2 { get; set; }
    }
}
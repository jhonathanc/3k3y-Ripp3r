namespace Ripp3r.Streams
{
    internal class PartialFile
    {
        public string Filename { get; set; }
        public long Length { get; private set; }
        public int Number { get; private set; }
        public long StartPosition { get; private set; }
        public bool IsLast { get; set; }

        public uint Crc { get; set; }

        internal PartialFile(string filename, long length, int number, long startPosition)
        {
            Filename = filename;
            Length = length;
            Number = number;
            StartPosition = startPosition;
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PartialFile && ((PartialFile)obj).Number == Number;
        }
    }
}

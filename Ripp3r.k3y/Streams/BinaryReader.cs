using System.IO;
using System.Text;

namespace Ripp3r.Streams
{
    public class BinaryReaderExt : System.IO.BinaryReader
    {
        private readonly bool _leaveOpen;

        public BinaryReaderExt(Stream input) : base(input)
        {
        }

        public BinaryReaderExt(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding)
        {
            _leaveOpen = leaveOpen;
        }

        protected override void Dispose(bool disposing)
        {
            if (_leaveOpen) return;

            base.Dispose(disposing);
        }
    }
}

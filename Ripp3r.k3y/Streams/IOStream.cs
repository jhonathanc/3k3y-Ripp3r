using System.IO;

namespace Ripp3r.Streams
{
    internal class IOStream : Stream
    {
        private readonly Stream _input;
        private readonly Stream _output;

        public IOStream(Stream input, Stream output)
        {
            _input = input;
            _output = output;
        }

        public override void Flush()
        {
            _input.Flush();
            if (_output != null) _output.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_output != null) _output.Seek(offset, origin);
            return _input.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _input.SetLength(value);
            if (_output != null) _output.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _input.Read(buffer, offset, count);
            if (_output != null) _output.Write(buffer, offset, count);
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _input.Write(buffer, offset, count);
            if (_output != null) _output.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return _input.Length; }
        }

        public override long Position
        {
            get { return _input.Position; }
            set { Seek(value, SeekOrigin.Begin); }
        }
    }
}

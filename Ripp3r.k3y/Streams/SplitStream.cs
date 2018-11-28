using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ripp3r.Streams
{
    class SplitStream : Stream
    {
        private readonly IEnumerable<string> _filenames;
        private FileStream _current;
        private long length;
        private readonly List<PartialFile> Parts;
        private PartialFile _currentPart;
        private long positionInPart;
        private long _position;

        public SplitStream(IEnumerable<string> filenames)
        {
            Parts = new List<PartialFile>();
            _filenames = filenames;
            FindParts();

            OpenPart(Parts.First());
        }

        private void FindParts()
        {
            int part = 1;
            length = 0;

            // Find all parts with the name .zip.<partnum>
            foreach (FileInfo info in _filenames.Select(f => new FileInfo(f)).Where(i => i.Exists))
            {
                Parts.Add(new PartialFile(info.FullName, info.Length, part++, length));
                length += info.Length;
            }
            Parts.Last().IsLast = true;
        }

        private PartialFile FindPartAt(long offsetFromStart)
        {
            return Parts.FirstOrDefault(
                p => p.StartPosition <= offsetFromStart && p.StartPosition + p.Length >= offsetFromStart);
        }

        private bool OpenPart(PartialFile part)
        {
            if (!File.Exists(part.Filename)) return false;

            if (_current != null)
            {
                _current.Close();
                _current.Dispose();
            }

            _current = new FileStream(part.Filename, FileMode.Open, FileAccess.Read);
            _currentPart = part;
            positionInPart = 0;
            return true;
        }

        public override void Close()
        {
            _current.Close();
            _current = null;
            base.Close();
        }

        public override void Flush()
        {
            if (_current != null) _current.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long offsetFromStart = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offsetFromStart = offset;
                    break;
                case SeekOrigin.Current:
                    offsetFromStart = Position + offset;
                    break;
                case SeekOrigin.End:
                    offsetFromStart = length + offset;
                    break;
            }

            // Use this to calculate the correct file
            PartialFile part = FindPartAt(offsetFromStart);
            if (!_currentPart.Equals(part))
            {
                if (!OpenPart(part))
                {
                    positionInPart = _currentPart.Length;
                    _current.Seek(0, SeekOrigin.End);
                    _position = _currentPart.StartPosition + _currentPart.Length;
                    return _position;
                }
            }

            positionInPart = offsetFromStart - _currentPart.StartPosition;
            _current.Seek(positionInPart, SeekOrigin.Begin);
            _position = offsetFromStart;

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            do
            {
                int read = _current.Read(buffer, offset, count);
                bytesRead += read;
                count -= read;
                positionInPart += read;
                _position += read;
                if (count == 0) break;
                if (_currentPart.IsLast) break;
                offset += read;

                if (!OpenPart(Parts.First(p => p.Number == _currentPart.Number + 1)))
                    break;
            } while (count > 0);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
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
            get { return false; }
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }
    }
}

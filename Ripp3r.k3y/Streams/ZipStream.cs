using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ionic.Crc;

namespace Ripp3r.Streams
{
    internal abstract class ZipStream : Stream
    {
        //When opening, open all files at once? Something like that? Will support seeking for sure
        //When writing, will open up the first file (.zip), then when that file is maxed out, will move 
        // it to .z01, .z02, etc., until the last file is written

        protected readonly bool isReading;
        protected readonly string basePath;
        protected CrcCalculatorStream internalStream;
        protected long length;
        public List<PartialFile> Parts { get; private set; }
        protected int amountOfParts;
        protected PartialFile currentPart;
        protected readonly FileMode mode;
        protected readonly FileAccess access;
        protected long positionInPart;
        private long _position;
        protected bool isMultipart;
        protected readonly long partSize = Interaction.Instance.PartSize;

        public static ZipStream Create(string path, FileMode mode, FileAccess access)
        {
            bool read = access == FileAccess.Read;

            if (!read)
            {
                return new SevenZipStream(path, mode, access);
            }

            // Is this WinZip or 7Zip style?
            return new SevenZipStream(path, mode, access);
        }

        protected ZipStream(string path, FileMode mode, FileAccess fileAccess)
        {
            this.mode = mode;
            access = fileAccess;

            isReading = fileAccess == FileAccess.Read;

            // ReSharper disable AssignNullToNotNullAttribute
            basePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            // ReSharper restore AssignNullToNotNullAttribute

            amountOfParts = 1; // At least one

            Initialize();
        }

        private void Initialize()
        {
            Parts = new List<PartialFile>();

            // Check if file .z01 exists
            if (isReading)
            {
                FindParts();
                currentPart = Parts.First();
                string path = currentPart.Filename;
                isMultipart = amountOfParts > 1;
                internalStream = new CrcCalculatorStream(new FileStream(path, mode, access), false);
            }
            else
            {
                isMultipart = Interaction.Instance.MultiPart;
                internalStream = CreatePart(1);
            }
        }

        protected abstract CrcCalculatorStream CreatePart(int partnum);

        protected abstract void FindParts();

        private PartialFile FindPartAt(long offsetFromStart)
        {
            return Parts.FirstOrDefault(
                p => p.StartPosition <= offsetFromStart && p.StartPosition + p.Length >= offsetFromStart);
        }

        private bool OpenPart(PartialFile part)
        {
            if (!File.Exists(part.Filename)) return false;

            currentPart.Crc = (uint) internalStream.Crc;
            internalStream.Close();
            internalStream.Dispose();

            internalStream = new CrcCalculatorStream(new FileStream(part.Filename, mode, access), false);
            currentPart = part;
            positionInPart = 0;
            return true;
        }

        public override void Close()
        {
            if (currentPart != null)
            {
                currentPart.Crc = (uint) internalStream.Crc;
                currentPart.IsLast = true;
            }

            if (internalStream != null)
                internalStream.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (internalStream != null)
            {
                internalStream.Dispose();
                internalStream = null;
            }
            base.Dispose(disposing);
        }

        // Find the first file to open (should be the .z01
        public override void Flush()
        {
            internalStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // Find the correct part
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
            Debug.WriteLine("Seeking to {0} from {1}, found in part {2}, currentpart {3}", offset, origin, part.Number,
                            currentPart.Number);

            if (!currentPart.Equals(part))
            {
                if (!OpenPart(part))
                {
                    positionInPart = currentPart.Length;
                    internalStream.Seek(0, SeekOrigin.End);
                    _position = currentPart.StartPosition + currentPart.Length;
                    return _position;
                }
            }

            positionInPart = offsetFromStart - currentPart.StartPosition;
            internalStream.Seek(positionInPart, SeekOrigin.Begin);
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
                int read = internalStream.Read(buffer, offset, count);
                bytesRead += read;
                count -= read;
                positionInPart += read;
                _position += read;
                if (count == 0) break;
                if (currentPart.IsLast) break;
                offset += read;

                Debug.WriteLine("Could read {0} bytes from the current part, open next part for the rest ({1} bytes)", bytesRead, count);

                if (!OpenPart(Parts.First(p => p.Number == currentPart.Number + 1)))
                    break;
            } while (count > 0);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!isMultipart || partSize > positionInPart + (count - offset))
            {
                // Just write
                internalStream.Write(buffer, offset, count);
                positionInPart += count;
                _position += count;
                return;
            }

            // Else, write what we can
            int sizeLeft = (int) (partSize - positionInPart);
            internalStream.Write(buffer, offset, sizeLeft);
            positionInPart += sizeLeft;
            _position += sizeLeft;

            internalStream = CreatePart(++amountOfParts);
            Write(buffer, offset + sizeLeft, count - sizeLeft);
        }

        public override bool CanRead
        {
            get { return access == FileAccess.Read; }
        }

        public override bool CanSeek
        {
            get { return access == FileAccess.Read; }
        }

        public override bool CanWrite
        {
            get { return access != FileAccess.Read; }
        }

        public override long Length
        {
            get
            {
                if (isReading) return length;
                throw new System.NotSupportedException();
            }
        }

        public override long Position
        {
            get { return _position; }
            set { _position = Seek(value, SeekOrigin.Begin); }
        }
    }
}

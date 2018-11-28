using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Ripp3r.Streams
{
    class FileHash
    {
        public readonly long StartSector;
        public readonly long Length;
        public byte[] Hash;

        public FileHash(long startSector, long length)
        {
            StartSector = startSector;
            Length = length;
        }

        public bool Equals(FileHash fh)
        {
            return fh != null && fh.StartSector == StartSector;
        }

        public override int GetHashCode()
        {
            return StartSector.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is FileHash && Equals((FileHash) obj);
        }
    }

    class FileHashStream : Stream
    {
        private readonly List<FileHash> _fileHashes;
        private long _position;
        private readonly MD5 streamMd5;

        private FileHash fileHash;
        private MD5 fileMd5;

        public FileHashStream(List<FileHash> fileHashes, long startSector)
        {
            _fileHashes = fileHashes;
            _position = startSector*Utilities.SectorSize;
            streamMd5 = MD5.Create();
        }

        public override void Close()
        {
            streamMd5.TransformFinalBlock(new byte[0], 0, 0);
            Hash = streamMd5.Hash;
            CloseCurrentHash();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            streamMd5.Dispose();
            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private FileHash FindCurrentHash()
        {
            // Current sector
            long currentSector = _position/Utilities.SectorSize;
            return
                _fileHashes.FirstOrDefault(
                    f => currentSector >= f.StartSector && currentSector < f.StartSector + f.Length.RoundToSector());
        }

        private void CloseCurrentHash()
        {
            if (fileHash == null) return;

            fileMd5.TransformFinalBlock(new byte[0], 0, 0);

            fileHash.Hash = fileMd5.Hash;
            fileMd5.Dispose();
            fileMd5 = null;
            fileHash = null;
        }

        private void CreateNewHash(FileHash newHash)
        {
            CloseCurrentHash();

            fileHash = newHash;
            fileMd5 = MD5.Create();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count % Utilities.SectorSize != 0)
            {
                throw new NotSupportedException("Only write full sectors, and one sector at a time");
            }

            streamMd5.TransformBlock(buffer, offset, count, null, 0);

            long parts = count/Utilities.SectorSize;
            for (long i = 0; i < parts; i++)
            {
                FileHash currentHash = FindCurrentHash();
                if ((fileHash != null && !fileHash.Equals(currentHash))) // Close the filehash
                    CloseCurrentHash();

                if (currentHash != null)
                {
                    if (fileHash == null) // Different hash!!!
                        CreateNewHash(currentHash);

                    if (fileHash != null)
                    {
                        // Now, find out how many bytes we have to write, only copy the required bytes.
                        // Probably, the rest of the buffer can be thrown away, unless count > Utilities.SectorSize
                        long posInFile = _position - (fileHash.StartSector*Utilities.SectorSize);
                        int amount = (int) Math.Min(Utilities.SectorSize, fileHash.Length - posInFile);

                        fileMd5.TransformBlock(buffer, offset, amount, null, 0);
                    }
                }

                offset += (int) Utilities.SectorSize;
                _position += Utilities.SectorSize;
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position 
        {
            get { return _position; }
            set { throw new NotSupportedException(); } 
        }

        public byte[] Hash { get; private set; }
    }
}

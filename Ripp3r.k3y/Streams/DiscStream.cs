using System;
using System.IO;
using System.Security.Authentication;
using Ionic.Zip;

namespace Ripp3r.Streams
{
    class DiscStream : Stream
    {
        private readonly CDB _cdb;
        private readonly ODD _odd;
        private readonly long _length;
        private long _position;
        private uint _sectorPosition;
        private uint _positionInSector;

        public DiscStream(ODD odd)
        {
            _cdb = ODD.CDB;
            _odd = odd;
            byte[] picData = new byte[0x73];
            byte[] data1 = new byte[0x10];
            byte[] data2 = new byte[0x10];

            if (_cdb.DoReadPICZone(picData) != CDB.IoResult.OK)
                throw new BadReadException("Can't read PIC data");
            PicData = picData;

            // Establish the session with the DVD drive
            if (odd.EstablishSessionKeys(0, ODD.Key1, ODD.Key2) == false)
                throw new AuthenticationException("Can't authenticate with the drive for D1/D2 extraction.");
            if (odd.GetData(data1, data2) == false)
                throw new AuthenticationException("Can't extract D1/D2 values");
            if (odd.EstablishSessionKeys(1, _odd.FixedKey30, _odd.FixedKey31) == false)
                throw new AuthenticationException("Can't authenticate with the drive for data extraction.");

            Data1 = data1;
            Data2 = data2;

            uint NumSectors = odd.GetNumSectors();
            _length = NumSectors*Utilities.SectorSize;

            _cdb.DoSetCDROMSpeed(0xFFFF);
        }

        public override void Flush()
        {
            // Do nothing? It's a read only stream at all times
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos = Position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset;
                    break;
                case SeekOrigin.Current:
                    pos += offset;
                    break;
                case SeekOrigin.End:
                    pos = Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }
            if (pos > Length) pos = Length;

            SetPosition(pos);

            return pos;
        }

        private void SetPosition(long pos)
        {
            _position = pos;
            _sectorPosition = (uint)Math.Floor(pos / (Utilities.SectorSize * 1.0));
            _positionInSector = (uint)(pos % (Utilities.SectorSize));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            uint numSectors = (uint) Utilities.RoundToSector(count);

            // We read by whole sectors only, so check if the count is a multiple of the sector size
            if (buffer == null) throw new ArgumentNullException("buffer");

            // Read as much as required
            // We will read as much sectors as needed, but always up to a whole sector
            byte[] completeBuffer = new byte[numSectors * Utilities.SectorSize];

            // Read the bytes requested
            if (ODD.ReadWithRetry(completeBuffer, _sectorPosition, numSectors) == false)
            {
                // Abort here, this won't work...
                throw new BadReadException("Can't read sectors at position " + _sectorPosition +
                                           ". Please check the disc for errors");
            }
            Array.Copy(completeBuffer, _positionInSector + offset, buffer, offset, count);

            PatchBuffer(buffer, count);

            SetPosition(_position + count);
            return count;
        }

        private void PatchBuffer(byte[] buffer, int count)
        {
            // Now see if we have to patch this buffer with the pic, d1 and d2 data
            // That information is stored in sector 1 and 2 (if sectors starts with 0)
            if (_position <= 0xF70 && _position + count >= 0xFA0)
            {
                // This will always fit, no sector boundary is crossed
                Array.Copy(Utilities.Encrypted3KISO, 0, buffer, 0xF70 - _position, 0x10);
                Array.Copy(Data1, 0, buffer, 0xF80 - _position, 0x10);
                Array.Copy(Data2, 0, buffer, 0xF90 - _position, 0x10);
                
                // This one will cross the sector boundary
                if (_position + count >= 0xFA0 + 0x73)
                    Array.Copy(PicData, 0, buffer, 0xFA0 - _position, 0x73);
                else
                    Array.Copy(PicData, 0, buffer, 0xFA0 - _position, count - (0xFA0 - _position)); // Until sector boundary
            }
            else if (_sectorPosition == 2)
            {
                // When we start at sector 2, only copy the last part of the PicData
                Array.Copy(PicData, 0x60, buffer, 0, 0x13); // The rest of the picdata
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            get { return _length; }
        }

        public override long Position
        {
            get { return _position; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public byte[] PicData { get; private set; }
        public byte[] Data1 { get; private set; }
        public byte[] Data2 { get; private set; }
    }
}

using System;
using System.IO;

namespace Ripp3r.Controls.Hex
{
    internal struct Cache
    {
        private byte[] buffer_;
        private long offset_;
        private long length_;
        private Stream stream_;

        public Stream Stream { get { return stream_; } }

        public int StreamLength
        {
            get { return stream_ != null ? (int)stream_.Length : 0; }
        }

        public Cache(Stream stream, int size)
        {
            buffer_ = new byte[size];
            stream_ = stream;
            offset_ = 0L;
            length_ = stream_.Read(buffer_, 0, size);
        }

        public byte GetByte(long index)
        {
            if (index < offset_ || index >= offset_ + length_)
            {
                stream_.Seek(index, SeekOrigin.Begin);
                length_ = stream_.Read(buffer_, 0, buffer_.Length);
                offset_ = index;
            }
            return buffer_[index - offset_];
        }

        public byte[] Read(int start, int length)
        {
            byte[] buffer = new byte[length];
            if (length > buffer_.Length)
            {
                stream_.Seek(start, SeekOrigin.Begin);
                stream_.Read(buffer, 0, length);
            }
            else
            {
                if (start < offset_ || start + length >= offset_ + length_)
                {
                    stream_.Seek(start, SeekOrigin.Begin);
                    length_ = stream_.Read(buffer_, 0, buffer_.Length);
                    offset_ = start;
                }
                Array.Copy(buffer_, start - offset_, buffer, 0L, length);
            }
            return buffer;
        }

        public bool Valid()
        {
            return stream_ != null;
        }

        public void Clear()
        {
            if (stream_ != null)
                stream_.Dispose();
            Init(new MemoryStream(), 256);
        }

        private void Init(Stream stream, int size)
        {
            stream_ = stream;
            buffer_ = new byte[size];
            length_ = stream_.Read(buffer_, 0, size);
            offset_ = 0L;
        }
    }
}

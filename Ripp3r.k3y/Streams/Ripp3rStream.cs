using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;

namespace Ripp3r.Streams
{
    internal enum StreamType
    {
        Normal,
        Zip
    }

    internal class Ripp3rStream : Stream
    {
        private readonly List<PartialFile> parts;

        private Stream internalStream;
        private long position;
        private MD5 md5;
        private string filename;
        private readonly bool input;
        private bool calculateHash;

        /// <summary>
        /// Opens a stream for reading. This will detect the stream type automatically based on the extension and filetype
        /// </summary>
        /// <param name="path">The path of the inputfile.</param>
        /// <param name="calculateHash">Wether or not to calculate the md5 hash of the stream.</param>
        public Ripp3rStream(string path, bool calculateHash = true)
        {
            if (calculateHash)
                StartCalculateHash();

            input = true;

            switch (Path.GetExtension(path))
            {
                case ".zip":
                case ".001":
                    ZipStream zipStream = ZipStream.Create(path, FileMode.Open, FileAccess.Read);
                    internalStream = FindFile(zipStream);
                    parts = zipStream.Parts;
                    if (internalStream == null)
                        throw new FileNotFoundException("Cannot find an iso or decrypted iso file in the zip file.");
                    StreamType = StreamType.Zip;
                    break;
                case ".iso":
                case ".dec":
                    internalStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamType = StreamType.Normal;
                    break;
                default:
                    throw new FileLoadException("Invalid file type");
            }
            filename = Path.GetFileName(path);
        }

        public Ripp3rStream(StreamType streamType, string filename, bool calculateHash = true)
        {
            StreamType = streamType;
            this.filename = Path.GetFileName(filename);
            string outputFile = filename;
            if (streamType == StreamType.Zip)
            {
                string ext = Path.GetExtension(outputFile);
                outputFile = outputFile.Replace(ext, ".zip");
            }

            if (calculateHash) StartCalculateHash();

            switch (streamType)
            {
                case StreamType.Zip:
                    ZipStream zipStream = ZipStream.Create(outputFile, FileMode.Create, FileAccess.Write);
                    ZipOutputStream outputStream = new ZipOutputStream(zipStream, false)
                        {
                            ParallelDeflateThreshold = 0,
                            EnableZip64 = Zip64Option.AsNecessary
                        };
                    outputStream.PutNextEntry(this.filename);
                    internalStream = outputStream;
                    parts = zipStream.Parts;
                    break;
                default:
                    internalStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                    break;
            }
        }

        private static Stream FindFile(ZipStream zipStream)
        {
            ZipFile zf = ZipFile.Read(zipStream);
            ZipEntry entry = zf.Entries.FirstOrDefault(e => e.FileName.EndsWith(".iso") || e.FileName.EndsWith(".dec"));

            return entry != null ? entry.OpenReader() : null;
        }

        public void StartCalculateHash()
        {
            if (calculateHash) return; // Already running

            if (md5 != null) md5.Dispose();

            calculateHash = true;
            md5 = MD5.Create();
        }

        public void StopCalculateHash()
        {
            if (!calculateHash || md5 == null) return; // Already stopped

            calculateHash = false;
            md5.TransformFinalBlock(new byte[0], 0, 0);
            Hash = md5.Hash;
        }

        public void AddFile(string name, byte[] content)
        {
            if (input || StreamType != StreamType.Zip) throw new NotSupportedException("Cannot add files to a normal stream, only to a zip stream");

            GC.Collect();
            ZipOutputStream zip = (ZipOutputStream)internalStream;
            filename = name;
            zip.PutNextEntry(name);
            Write(content, 0, content.Length);
        }

        public override void Close()
        {
            StopCalculateHash();
            if (internalStream != null)
            {
                if (!input && StreamType == StreamType.Zip && calculateHash)
                {
                    byte[] hashAsString = Encoding.ASCII.GetBytes(md5.Hash.AsString());
                    AddFile(string.Concat(filename, ".md5"), hashAsString);
                }
                internalStream.Close();
            }
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (internalStream != null)
            {
                internalStream.Dispose();
                internalStream = null;
            }
            if (md5 != null)
            {
                md5.Dispose();
                md5 = null;
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            internalStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!calculateHash)
            {
                long offs = internalStream.Seek(offset, origin);
                Position = offs;
                return offs;
            }

            throw new NotSupportedException("Cannot seek or the hash will be corrupted");
        }

        public override void SetLength(long value)
        {
            if (input) throw new NotSupportedException("You can't set the length on a readonly stream.");
            internalStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int val = internalStream.Read(buffer, offset, count);
            position += val;
            if (calculateHash) md5.TransformBlock(buffer, offset, count, null, 0);
            return val;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            internalStream.Write(buffer, offset, count);
            position += count;
            if (calculateHash) md5.TransformBlock(buffer, offset, buffer.Length, null, 0);
        }

        public override bool CanRead
        {
            get { return input; }
        }

        public override bool CanSeek
        {
            get { return !calculateHash; }
        }

        public override bool CanWrite
        {
            get { return !input; }
        }

        public override long Length
        {
            get { return internalStream.Length; }
        }

        public override long Position
        {
            get { return position; }
            set
            {
                if (!calculateHash)
                {
                    internalStream.Position = value;
                    position = internalStream.Position;
                    return;
                }
                throw new NotSupportedException("Cannot set the position, or the hash will be corrupted");
            }
        }

        public long RealPosition
        {
            get { return internalStream.Position; }
        }

        public byte[] Hash { get; private set; }

        public StreamType StreamType { get; private set; }

        public bool IsMultipart
        {
            get { return parts.Count > 1; }
        }

        public string GetSfvContent()
        {
            if (parts == null) throw new InvalidOperationException("Stream is already disposed or not a zipstream");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; SFV file created by 3k3y ripper");
            sb.AppendLine("; The future is here!");
            sb.AppendLine("; More info: http://www.3k3y.com");
            sb.AppendLine();
            foreach (PartialFile p in parts)
            {
                sb.Append(Path.GetFileName(p.Filename)).Append(" ").Append(p.Crc.ToString("x8")).AppendLine();
            }
            return sb.ToString();
        }
    }
}

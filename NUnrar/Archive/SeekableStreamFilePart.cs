using System.IO;
using NUnrar.Common;
using NUnrar.Headers;

namespace NUnrar.Archive
{
    internal class SeekableStreamFilePart : RarFilePart
    {
        internal SeekableStreamFilePart(MarkHeader mh, FileHeader fh, Stream stream, bool streamOwner)
            : base(mh, fh, streamOwner)
        {
            Stream = stream;
        }

        internal Stream Stream
        {
            get;
            private set;
        }

        internal override Stream GetStream()
        {
            Stream.Position = FileHeader.DataStartPosition;
            return Stream;
        }

        internal override string FilePartName
        {
            get
            {
                return "Unknown Stream - File Entry: " + base.FileHeader.FileName;
            }
        }
    }
}

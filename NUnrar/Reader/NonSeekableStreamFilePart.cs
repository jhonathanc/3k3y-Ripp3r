using System.IO;
using NUnrar.Common;
using NUnrar.Headers;

namespace NUnrar.Reader
{
    internal class NonSeekableStreamFilePart : RarFilePart
    {
        internal NonSeekableStreamFilePart(MarkHeader mh, FileHeader fh, bool streamOwner)
            : base(mh, fh, streamOwner)
        {
        }

        internal override Stream GetStream()
        {
            return FileHeader.PackedStream;
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

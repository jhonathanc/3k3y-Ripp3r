using NUnrar.Common;
using NUnrar.Headers;

namespace NUnrar.Reader
{

    public class RarReaderEntry : RarEntry
    {
        internal RarReaderEntry(bool solid, RarFilePart part)
        {
            this.Part = part;
            IsSolid = solid;
        }

        internal bool IsSolid
        {
            get;
            private set;
        }

        internal RarFilePart Part
        {
            get;
            private set;
        }

        internal override FileHeader FileHeader
        {
            get
            {
                return Part.FileHeader;
            }
        }

        /// <summary>
        /// The compressed file size
        /// </summary>
        public override long CompressedSize
        {
            get
            {
                return Part.FileHeader.CompressedSize;
            }
        }

        /// <summary>
        /// The uncompressed file size
        /// </summary>
        public override long Size
        {
            get
            {
                return Part.FileHeader.UncompressedSize;
            }
        }
    }
}

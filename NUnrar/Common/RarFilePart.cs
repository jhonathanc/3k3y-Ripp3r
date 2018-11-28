using System.IO;
using NUnrar.Headers;

namespace NUnrar.Common
{
    /// <summary>
    /// This represents a single file part that exists in a rar volume.  A compressed file is one or many file parts that are spread across one or may rar parts.
    /// </summary>
    internal abstract class RarFilePart
    {
        protected RarFilePart(MarkHeader mh, FileHeader fh, bool streamOwner)
        {
            MarkHeader = mh;
            FileHeader = fh;
            StreamOwner = streamOwner;
        }

        internal MarkHeader MarkHeader
        {
            get;
            private set;
        }

        internal FileHeader FileHeader
        {
            get;
            private set;
        }

        internal bool StreamOwner
        {
            get;
            private set;
        }

        internal abstract string FilePartName
        {
            get;
        }

        internal abstract Stream GetStream();
    }
}

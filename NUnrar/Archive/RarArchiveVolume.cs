using System.IO;
using NUnrar.Common;
using NUnrar.IO;

namespace NUnrar.Archive
{
    public abstract class RarArchiveVolume : RarVolume
    {
        internal RarArchiveVolume(StreamingMode mode, RarOptions options)
            : base(mode, options)
        {
        }

#if !PORTABLE
        /// <summary>
        /// File that backs this volume, if it not stream based
        /// </summary>
        public abstract FileInfo VolumeFile
        {
            get;
        }
#endif
    }
}

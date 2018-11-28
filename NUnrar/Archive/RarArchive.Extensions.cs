using System.Linq;

namespace NUnrar.Archive
{
    public static class RarArchiveExtensions
    {
        /// <summary>
        /// RarArchive is the first volume of a multi-part archive.  If MultipartVolume is true and IsFirstVolume is false then the first volume file must be missing.
        /// </summary>
        public static bool IsFirstVolume(this RarArchive archive)
        {
            return archive.Volumes.First().IsFirstVolume;
        }

        /// <summary>
        /// RarArchive is part of a multi-part archive.
        /// </summary>
        public static bool IsMultipartVolume(this RarArchive archive)
        {
            return archive.Volumes.First().IsMultiVolume;
        }

        /// <summary>
        /// RarArchive is SOLID (this means the Archive saved bytes by reusing information which helps for archives containing many small files).
        /// Currently, NUnrar cannot decompress SOLID archives.
        /// </summary>
        public static bool IsSolidArchive(this RarArchive archive)
        {
            return archive.Volumes.First().IsSolidArchive;
        }
    }
}

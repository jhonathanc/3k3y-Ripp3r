using System;
using NUnrar.Headers;

namespace NUnrar.Common
{
    public abstract class RarEntry
    {

        internal abstract FileHeader FileHeader
        {
            get;
        }

        /// <summary>
        /// The File's 32 bit CRC Hash
        /// </summary>
        public virtual uint Crc
        {
            get
            {
                return FileHeader.FileCRC;
            }
        }

        /// <summary>
        /// The path of the file internal to the Rar Archive.
        /// </summary>
        public string FilePath
        {
            get
            {
                return FileHeader.FileName;
            }
        }

        /// <summary>
        /// The compressed file size
        /// </summary>
        public abstract long CompressedSize
        {
            get;
        }

        /// <summary>
        /// The uncompressed file size
        /// </summary>
        public abstract long Size
        {
            get;
        }

        /// <summary>
        /// The entry last modified time in the archive, if recorded
        /// </summary>
        public DateTime? LastModifiedTime
        {
            get
            {
                return FileHeader.FileLastModifiedTime;
            }
        }

        /// <summary>
        /// The entry create time in the archive, if recorded
        /// </summary>
        public DateTime? CreatedTime
        {
            get
            {
                return FileHeader.FileCreatedTime;
            }
        }

        /// <summary>
        /// The entry last accessed time in the archive, if recorded
        /// </summary>
        public DateTime? LastAccessedTime
        {
            get
            {
                return FileHeader.FileLastAccessedTime;
            }
        }

        /// <summary>
        /// The entry time whend archived, if recorded
        /// </summary>
        public DateTime? ArchivedTime
        {
            get
            {
                return FileHeader.FileArchivedTime;
            }
        }

        /// <summary>
        /// Entry is password protected and encrypted and cannot be extracted.
        /// </summary>
        public bool IsEncrypted
        {
            get
            {
                return FileHeader.FileFlags.HasFlag(FileFlags.PASSWORD);
            }
        }

        /// <summary>
        /// Entry is password protected and encrypted and cannot be extracted.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return FileHeader.FileFlags.HasFlag(FileFlags.DIRECTORY);
            }
        }

        public bool IsSplit
        {
            get
            {
                return FileHeader.FileFlags.HasFlag(FileFlags.SPLIT_AFTER);
            }
        }

        public override string ToString()
        {
            return string.Format("Entry Path: {0} Compressed Size: {1} Uncompressed Size: {2} CRC: {3}",
                FilePath, CompressedSize, Size, Crc);
        }
    }
}

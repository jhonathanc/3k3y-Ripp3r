using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnrar.Common;
using NUnrar.Headers;
using NUnrar.IO;

namespace NUnrar.Archive
{
    public class RarArchive
    {
        private LazyReadOnlyCollection<RarArchiveVolume> lazyVolumes;
        private LazyReadOnlyCollection<RarArchiveEntry> lazyEntries;


#if !PORTABLE
        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        internal RarArchive(FileInfo fileInfo, RarOptions options)
        {
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("File does not exist: " + fileInfo.FullName);
            }
            lazyVolumes = new LazyReadOnlyCollection<RarArchiveVolume>(RarArchiveVolumeFactory.GetParts(fileInfo, options));
            lazyEntries = new LazyReadOnlyCollection<RarArchiveEntry>(RarArchiveEntry.GetEntries(this, Volumes));
        }
#endif

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        internal RarArchive(IEnumerable<Stream> streams, RarOptions options)
        {
            lazyVolumes = new LazyReadOnlyCollection<RarArchiveVolume>(RarArchiveVolumeFactory.GetParts(streams, options));
            lazyEntries = new LazyReadOnlyCollection<RarArchiveEntry>(RarArchiveEntry.GetEntries(this, Volumes));
        }

        /// <summary>
        /// Returns an ReadOnlyCollection of all the RarArchiveEntries across the one or many parts of the RarArchive.
        /// </summary>
        /// <returns></returns>
        public ICollection<RarArchiveEntry> Entries
        {
            get
            {
                return lazyEntries;
            }
        }

        /// <summary>
        /// Returns an ReadOnlyCollection of all the RarArchiveVolumes across the one or many parts of the RarArchive.
        /// </summary>
        /// <returns></returns>
        public ICollection<RarArchiveVolume> Volumes
        {
            get
            {
                return lazyVolumes;
            }
        }

        /// <summary>
        /// The total size of the files compressed in the archive.
        /// </summary>
        public long TotalSize
        {
            get
            {
                return Entries.Aggregate(0L, (total, cf) => total + cf.CompressedSize);
            }
        }

        #region Internal
        internal Stream StreamData
        {
            get;
            private set;
        }
        #endregion

        #region Creation

#if !PORTABLE
        /// <summary>
        /// Constructor expects a filepath to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        public static RarArchive Open(string filePath)
        {
            return Open(filePath, RarOptions.None);
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        public static RarArchive Open(FileInfo fileInfo)
        {
            return Open(fileInfo, RarOptions.None);
        }

        /// <summary>
        /// Constructor expects a filepath to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="options"></param>
        public static RarArchive Open(string filePath, RarOptions options)
        {
            filePath.CheckNotNullOrEmpty("filePath");
            return Open(new FileInfo(filePath), options);
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        public static RarArchive Open(FileInfo fileInfo, RarOptions options)
        {
            fileInfo.CheckNotNull("fileInfo");
            return new RarArchive(fileInfo, options);
        }
#endif
        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        public static RarArchive Open(Stream stream)
        {
            stream.CheckNotNull("stream");
            return Open(stream.AsEnumerable());
        }

        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        public static RarArchive Open(Stream stream, RarOptions options)
        {
            stream.CheckNotNull("stream");
            return Open(stream.AsEnumerable(), options);
        }

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        public static RarArchive Open(IEnumerable<Stream> streams)
        {
            streams.CheckNotNull("streams");
            return new RarArchive(streams, RarOptions.KeepStreamsOpen);
        }

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        public static RarArchive Open(IEnumerable<Stream> streams, RarOptions options)
        {
            streams.CheckNotNull("streams");
            return new RarArchive(streams, options);
        }

#if !PORTABLE
        public static void WriteToDirectory(string sourceArchive, string destinationDirectory, IRarExtractionListener listener,
            CancellationToken cancellation, ExtractOptions options = ExtractOptions.Overwrite)
        {
            RarArchive archive = RarArchive.Open(sourceArchive);
            foreach (RarArchiveEntry entry in archive.Entries)
            {
                entry.WriteToDirectory(destinationDirectory, cancellation, listener, options);
            }
        }

        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteToDirectory(string sourceArchive, string destinationPath, CancellationToken cancellation,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            WriteToDirectory(sourceArchive, destinationPath, new NullRarExtractionListener(), cancellation, options);
        }

        public static bool IsRarFile(string filePath)
        {
            return IsRarFile(new FileInfo(filePath));
        }

        public static bool IsRarFile(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return false;
            }
            using (Stream stream = fileInfo.OpenRead())
            {
                return IsRarFile(stream);
            }
        }
#endif
        public static bool IsRarFile(Stream stream)
        {
            try
            {

                RarHeaderFactory headerFactory = new RarHeaderFactory(StreamingMode.Seekable, RarOptions.CheckForSFX);
                RarHeader header = headerFactory.ReadHeaders(stream).FirstOrDefault();
                if (header == null)
                {
                    return false;
                }
                return Enum.IsDefined(typeof(HeaderType), header.HeaderType);
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnrar.Common;
using NUnrar.IO;
#if PORTABLE || THREEFIVE
using NUnrar.Headers;
#endif

namespace NUnrar.Reader
{
    /// <summary>
    /// This class faciliates Reading a Rar Archive in a non-seekable forward-only manner
    /// </summary>
    public abstract class RarReader : IDisposable
    {
        private readonly IRarExtractionListener listener;
        private readonly RarOptions options;
        private bool completed;
        private IEnumerator<RarReaderEntry> entriesForCurrentReadStream;
        private bool wroteCurrentEntry;

        internal RarReader(RarOptions options, IRarExtractionListener listener)
        {
            this.listener = listener;
            this.options = options;
            listener.CheckNotNull("listener");
        }

        public RarReaderVolume Volume { get; private set; }

        public RarReaderEntry Entry
        {
            get { return entriesForCurrentReadStream.Current; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (entriesForCurrentReadStream != null)
            {
                entriesForCurrentReadStream.Dispose();
            }
            if (Volume.Stream != null && !options.HasFlag(RarOptions.KeepStreamsOpen))
            {
                Volume.Stream.Dispose();
            }
        }

        #endregion

        public bool MoveToNextEntry()
        {
            if (completed)
            {
                return false;
            }
            if (entriesForCurrentReadStream == null)
            {
                LoadStreamForReading(RequestInitialStream());
                return true;
            }
            if (!wroteCurrentEntry)
            {
                SkipEntry();
            }
            wroteCurrentEntry = false;
            if (NextEntryForCurrentStream())
            {
                return true;
            }
            completed = true;
            return false;
        }

        internal virtual bool NextEntryForCurrentStream()
        {
            return entriesForCurrentReadStream.MoveNext();
        }

        internal bool LoadStreamForReading(Stream stream)
        {
            if (entriesForCurrentReadStream != null)
            {
                entriesForCurrentReadStream.Dispose();
            }
            if ((stream == null) || (!stream.CanRead))
            {
                throw new MultipartStreamRequiredException("File is split into multiple archives: '"
                                                           + Entry.FilePath +
                                                           "'. A new readable stream is required.  Use Cancel if it was intended.");
            }
            entriesForCurrentReadStream = GetEntries(stream, options).GetEnumerator();
            if (!entriesForCurrentReadStream.MoveNext())
            {
                return false;
            }
            return true;
        }

        internal abstract IEnumerable<RarFilePart> CreateFilePartEnumerable();
        internal abstract Stream RequestInitialStream();
        internal abstract void ValidateArchive(RarVolume archive);

        private IEnumerable<RarReaderEntry> GetEntries(Stream stream, RarOptions options)
        {
            Volume = new RarReaderVolume(stream, options);
            foreach (RarFilePart fp in Volume.ReadFileParts())
            {
                ValidateArchive(Volume);
                yield return new RarReaderEntry(Volume.IsSolidArchive, fp);
            }
        }

        #region Entry Skip/Write

        public void SkipEntry()
        {
            listener.OnInformation("Skipping Entry");
            if (!Entry.IsDirectory)
            {
                Skip(CreateFilePartEnumerable());
            }
        }

        private void Skip(IEnumerable<RarFilePart> parts)
        {
            var buffer = new byte[4096];
            using (Stream s = new MultiVolumeStream(parts, listener))
            {
                while (s.Read(buffer, 0, buffer.Length) > 0)
                {
                }
            }
        }

        public void WriteEntryTo(Stream writableStream, CancellationToken cancellation)
        {
            if (wroteCurrentEntry)
            {
                throw new RarExtractionException("Already extracted current entry.");
            }
            if ((writableStream == null) || (!writableStream.CanWrite))
            {
                throw new ArgumentNullException(
                    "A writable Stream was required.  Use Cancel if that was intended.");
            }
            listener.OnInformation("Writing Entry to Stream");
            Write(CreateFilePartEnumerable(), writableStream, cancellation);
            wroteCurrentEntry = true;
        }

        private void Write(IEnumerable<RarFilePart> parts, Stream writeStream, CancellationToken cancellation)
        {
            using (Stream input = new MultiVolumeStream(parts, listener))
            {
                var pack = new Unpack.Unpack(Entry.FileHeader, input, writeStream);
                pack.doUnpack(Entry.IsSolid, cancellation);
            }
        }

        #endregion

        #region Open

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with a single volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="listener"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RarReader Open(Stream stream, IRarExtractionListener listener,
                                     RarOptions options = RarOptions.KeepStreamsOpen)
        {
            stream.CheckNotNull("stream");
            return new SingleVolumeRarReader(stream, options, listener);
        }

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with a single volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RarReader Open(Stream stream, RarOptions options = RarOptions.KeepStreamsOpen)
        {
            stream.CheckNotNull("stream");
            return new SingleVolumeRarReader(stream, options, new NullRarExtractionListener());
        }

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with multiple volumes
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="listener"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RarReader Open(IEnumerable<Stream> streams, IRarExtractionListener listener,
                                     RarOptions options = RarOptions.KeepStreamsOpen)
        {
            streams.CheckNotNull("streams");
            return new MultiVolumeRarReader(streams, options, listener);
        }

        /// <summary>
        /// Opens a RarReader for Non-seeking usage with multiple volumes
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RarReader Open(IEnumerable<Stream> streams, RarOptions options = RarOptions.KeepStreamsOpen)
        {
            streams.CheckNotNull("streams");
            return new MultiVolumeRarReader(streams, options, new NullRarExtractionListener());
        }

        #endregion
    }
}
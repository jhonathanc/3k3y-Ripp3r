using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnrar.Common;

namespace NUnrar.Reader
{
    internal class MultiVolumeRarReader : RarReader
    {
        private readonly IEnumerator<Stream> streams;

        internal MultiVolumeRarReader(IEnumerable<Stream> streams, RarOptions options, IRarExtractionListener listener)
            : base(options, listener)
        {
            this.streams = streams.GetEnumerator();
        }

        internal override void ValidateArchive(RarVolume archive)
        {
        }

        internal override Stream RequestInitialStream()
        {
            if (streams.MoveNext())
            {
                return streams.Current;
            }
            throw new RarExtractionException("No stream provided when requested by MultiVolumeRarReader");
        }

        internal override IEnumerable<RarFilePart> CreateFilePartEnumerable()
        {
            return new MultiVolumeStreamEnumerator(this, streams);
        }

        internal override bool NextEntryForCurrentStream()
        {
            if (!base.NextEntryForCurrentStream())
            {
                //if we're got another stream to try to process then do so
                if (streams.MoveNext() && LoadStreamForReading(streams.Current))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        #region Nested type: MultiVolumeStreamEnumerator

        private class MultiVolumeStreamEnumerator : IEnumerable<RarFilePart>, IEnumerator<RarFilePart>
        {
            private readonly IEnumerator<Stream> nextReadableStreams;
            private readonly MultiVolumeRarReader reader;
            private bool isFirst = true;

            internal MultiVolumeStreamEnumerator(MultiVolumeRarReader r, IEnumerator<Stream> nextReadableStreams)
            {
                reader = r;
                this.nextReadableStreams = nextReadableStreams;
            }

            #region IEnumerable<RarFilePart> Members

            public IEnumerator<RarFilePart> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerator<RarFilePart> Members

            public RarFilePart Current { get; private set; }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public bool MoveNext()
            {
                if (isFirst)
                {
                    Current = reader.Entry.Part;
                    isFirst = false; //first stream already to go
                    return true;
                }

                if (!reader.Entry.IsSplit)
                {
                    return false;
                }
                if (!nextReadableStreams.MoveNext())
                {
                    throw new RarExtractionException("No stream provided when requested by MultiVolumeRarReader");
                }
                reader.LoadStreamForReading(nextReadableStreams.Current);

                Current = reader.Entry.Part;
                return true;
            }

            public void Reset()
            {
            }

            #endregion
        }

        #endregion
    }
}
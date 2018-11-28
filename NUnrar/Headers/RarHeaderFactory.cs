using System;
using System.Collections.Generic;
using System.IO;
using NUnrar.Common;
using NUnrar.IO;

namespace NUnrar.Headers
{
    internal class RarHeaderFactory
    {
        private int MAX_SFX_SIZE = 0x80000 - 16; //archive.cpp line 136

        internal RarHeaderFactory(StreamingMode mode, RarOptions options)
        {
            StreamingMode = mode;
            Options = options;
        }

        internal RarOptions Options
        {
            get;
            private set;
        }

        internal StreamingMode StreamingMode
        {
            get;
            private set;
        }

        internal IEnumerable<RarHeader> ReadHeaders(Stream stream)
        {
            bool checkForSFX = Options.HasFlag(RarOptions.CheckForSFX);
            if ((StreamingMode == StreamingMode.Seekable) && checkForSFX)
            {
                BinaryReader reader = new BinaryReader(stream);
                try
                {
                    int count = 0;
                    while (true)
                    {
                        byte firstByte = reader.ReadByte();
                        if (firstByte == 0x52)
                        {
                            byte[] nextThreeBytes = reader.ReadBytes(3);
                            if ((nextThreeBytes[0] == 0x45)
                                && (nextThreeBytes[1] == 0x7E)
                                && (nextThreeBytes[2] == 0x5E))
                            {
                                //old format and isvalid
                                stream.Position = stream.Position - 4;
                                break;
                            }
                            byte[] secondThreeBytes = reader.ReadBytes(3);
                            if ((nextThreeBytes[0] == 0x61)
                                && (nextThreeBytes[1] == 0x72)
                                && (nextThreeBytes[2] == 0x21)
                                && (secondThreeBytes[0] == 0x1A)
                                && (secondThreeBytes[1] == 0x07)
                                && (secondThreeBytes[2] == 0x00))
                            {
                                //new format and isvalid
                                stream.Position = stream.Position - 7;
                                break;
                            }
                        }
                        if (count > MAX_SFX_SIZE)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!Options.HasFlag(RarOptions.KeepStreamsOpen))
                    {
#if THREEFIVE
                        reader.Close();
#else
                        reader.Dispose();
#endif
                    }
                    throw new InvalidRarFormatException("Error trying to read rar signature.", e);
                }
            }
            RarHeader header;
            while ((header = ReadNextHeader(stream)) != null)
            {
                yield return header;
                if (header.HeaderType == HeaderType.EndArchiveHeader)
                {
                    yield break; // the end?
                }
            }
        }

        private RarHeader ReadNextHeader(Stream stream)
        {
            MarkingBinaryReader reader = new MarkingBinaryReader(stream);
            RarHeader header = RarHeader.Create(reader);
            if (header == null)
            {
                return null;
            }
            switch (header.HeaderType)
            {
                case HeaderType.ArchiveHeader:
                    {
                        return header.PromoteHeader<ArchiveHeader>(reader);
                    }
                case HeaderType.MarkHeader:
                    {
                        return header.PromoteHeader<MarkHeader>(reader);
                    }
                case HeaderType.NewSubHeader:
                    {
                        FileHeader fh = header.PromoteHeader<FileHeader>(reader);
                        switch (StreamingMode)
                        {
                            case StreamingMode.Seekable:
                                {
                                    fh.DataStartPosition = reader.BaseStream.Position;
                                    reader.BaseStream.Position += fh.CompressedSize;
                                }
                                break;
                            case StreamingMode.Streaming:
                                {
                                    //skip the data because it's useless?
                                    reader.BaseStream.Skip(fh.CompressedSize);
                                }
                                break;
                            default:
                                {
                                    throw new InvalidRarFormatException("Invalid StreamingMode");
                                }
                        }
                        return fh;
                    }
                case HeaderType.FileHeader:
                    {
                        FileHeader fh = header.PromoteHeader<FileHeader>(reader);
                        switch (StreamingMode)
                        {
                            case StreamingMode.Seekable:
                                {
                                    fh.DataStartPosition = reader.BaseStream.Position;
                                    reader.BaseStream.Position += fh.CompressedSize;
                                }
                                break;
                            case StreamingMode.Streaming:
                                {
                                    ReadOnlySubStream ms
                                        = new ReadOnlySubStream(reader.BaseStream, fh.CompressedSize);
                                    fh.PackedStream = ms;
                                }
                                break;
                            default:
                                {
                                    throw new InvalidRarFormatException("Invalid StreamingMode");
                                }
                        }
                        return fh;
                    }
                case HeaderType.EndArchiveHeader:
                    {
                        return header.PromoteHeader<EndArchiveHeader>(reader);
                    }
                default:
                    {
                        throw new InvalidRarFormatException("Invalid Rar Header: " + header.HeaderType.ToString());
                    }
            }
        }
    }
}

//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System.Linq;

namespace DiscUtils.Iso9660
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Vfs;

    internal class VfsCDReader : VfsReadOnlyFileSystem<ReaderDirEntry, File, ReaderDirectory, IsoContext>
    {
        private static readonly Iso9660Variant[] DefaultVariantsNoJoliet = new[] { Iso9660Variant.RockRidge, Iso9660Variant.Iso9660 };
        private static readonly Iso9660Variant[] DefaultVariantsWithJoliet = new[] { Iso9660Variant.Joliet, Iso9660Variant.RockRidge, Iso9660Variant.Iso9660 };

        private readonly Stream _data;
        private readonly bool _hideVersions;
        private readonly Iso9660Variant _activeVariant;

        /// <summary>
        /// Initializes a new instance of the VfsCDReader class.
        /// </summary>
        /// <param name="data">The stream to read the ISO image from.</param>
        /// <param name="joliet">Whether to read Joliet extensions.</param>
        /// <param name="hideVersions">Hides version numbers (e.g. ";1") from the end of files</param>
        public VfsCDReader(Stream data, bool joliet, bool hideVersions)
            : this(data, joliet ? DefaultVariantsWithJoliet : DefaultVariantsNoJoliet, hideVersions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VfsCDReader class.
        /// </summary>
        /// <param name="data">The stream to read the ISO image from.</param>
        /// <param name="variantPriorities">Which possible file system variants to use, and with which priority</param>
        /// <param name="hideVersions">Hides version numbers (e.g. ";1") from the end of files</param>
        /// <remarks>
        /// <para>
        /// The implementation considers each of the file system variants in <c>variantProperties</c> and selects
        /// the first which is determined to be present.  In this example Joliet, then Rock Ridge, then vanilla
        /// Iso9660 will be considered:
        /// </para>
        /// <code lang="cs">
        /// VfsCDReader(stream, new Iso9660Variant[] {Joliet, RockRidge, Iso9660}, true);
        /// </code>
        /// <para>The Iso9660 variant should normally be specified as the final entry in the list.  Placing it earlier
        /// in the list will effectively mask later items and not including it may prevent some ISOs from being read.</para>
        /// </remarks>
        private VfsCDReader(Stream data, IEnumerable<Iso9660Variant> variantPriorities, bool hideVersions)
        {
            _data = data;
            _hideVersions = hideVersions;

            long vdpos = 0x8000; // Skip lead-in

            byte[] buffer = new byte[IsoUtilities.SectorSize];

            long pvdPos = 0;
            long svdPos = 0;

            BaseVolumeDescriptor bvd;
            do
            {
                data.Position = vdpos;
                int numRead = data.Read(buffer, 0, IsoUtilities.SectorSize);
                if (numRead != IsoUtilities.SectorSize)
                {
                    break;
                }

                bvd = new BaseVolumeDescriptor(buffer, 0);
                switch (bvd.VolumeDescriptorType)
                {
                    case VolumeDescriptorType.Primary: // Primary Vol Descriptor
                        pvdPos = vdpos;
                        break;

                    case VolumeDescriptorType.Supplementary: // Supplementary Vol Descriptor
                        svdPos = vdpos;
                        break;

                    case VolumeDescriptorType.Partition: // Volume Partition Descriptor
                        break;
                    case VolumeDescriptorType.SetTerminator: // Volume Descriptor Set Terminator
                        break;
                }

                vdpos += IsoUtilities.SectorSize;
            }
            while (bvd.VolumeDescriptorType != VolumeDescriptorType.SetTerminator);

            _activeVariant = Iso9660Variant.None;
            foreach (var variant in variantPriorities)
            {
                switch (variant)
                {
                    case Iso9660Variant.Joliet:
                        if (svdPos != 0)
                        {
                            data.Position = svdPos;
                            data.Read(buffer, 0, IsoUtilities.SectorSize);
                            var volDesc = new SupplementaryVolumeDescriptor(buffer, 0);

                            Context = new IsoContext { VolumeDescriptor = volDesc, DataStream = _data };
                            RootDirectory = new ReaderDirectory(Context, new ReaderDirEntry(null, volDesc.RootDirectory));
                            _activeVariant = Iso9660Variant.Iso9660;
                        }

                        break;

                    case Iso9660Variant.RockRidge:
                    case Iso9660Variant.Iso9660:
                        if (pvdPos != 0)
                        {
                            data.Position = pvdPos;
                            data.Read(buffer, 0, IsoUtilities.SectorSize);
                            var volDesc = new PrimaryVolumeDescriptor(buffer, 0);

                            IsoContext context = new IsoContext { VolumeDescriptor = volDesc, DataStream = _data };
                            DirectoryRecord rootSelfRecord = ReadRootSelfRecord(context);

                            if (variant == Iso9660Variant.Iso9660)
                            {
                                Context = context;
                                RootDirectory = new ReaderDirectory(context, new ReaderDirEntry(null, rootSelfRecord));
                                _activeVariant = variant;
                            }
                        }

                        break;
                }

                if (_activeVariant != Iso9660Variant.None)
                {
                    break;
                }
            }

            if (_activeVariant == Iso9660Variant.None)
            {
                throw new IOException("None of the permitted ISO9660 file system variants was detected");
            }
        }

        /// <summary>
        /// Provides the friendly name for the CD filesystem.
        /// </summary>
        public override string FriendlyName
        {
            get { return "ISO 9660 (CD-ROM)"; }
        }

        /// <summary>
        /// Gets the Volume Identifier.
        /// </summary>
        public override string VolumeLabel
        {
            get { return Context.VolumeDescriptor.VolumeIdentifier; }
        }

        public long ClusterSize
        {
            get { return IsoUtilities.SectorSize; }
        }

        public long TotalClusters
        {
            get { return Context.VolumeDescriptor.VolumeSpaceSize; }
        }

        public Iso9660Variant ActiveVariant
        {
            get { return _activeVariant; }
        }

        public long ClusterToOffset(long cluster)
        {
            return cluster * ClusterSize;
        }

        public long OffsetToCluster(long offset)
        {
            return offset / ClusterSize;
        }

        public Range<long, long>[] PathToClusters(string path)
        {
            ReaderDirEntry dirEntry = GetDirectoryEntry(path);
            if (dirEntry == null)
            {
                throw new FileNotFoundException("File not found", path);
            }
            if (dirEntry.IsDirectory)
            {
                if (dirEntry.Record.FileUnitSize != 0 || dirEntry.Record.InterleaveGapSize != 0)
                {
                    throw new NotSupportedException("Non-contiguous extents not supported");
                }

                return new[]
                    {
                        new Range<long, long>(dirEntry.Record.LocationOfExtent, dirEntry.Record.DataLength)
                    };
            }

            // It's a file, check if there are more parts
            int index = path.LastIndexOf('\\'); // Don't use Path.GetDirectoryName, since it won't work with Mono if files use \ instead of /
            string dir = path.Substring(0, index + 1);
            string filename = path.Substring(index + 1);

            dirEntry = GetDirectoryEntry(dir);

            ReaderDirectory rdr = new ReaderDirectory(Context, dirEntry);
            IEnumerable<ReaderDirEntry> dirEntries = rdr.GetEntriesByName(filename);
            // Check the file

            return
                dirEntries.Select(
                    d =>
                    new Range<long, long>(d.Record.LocationOfExtent, d.Record.DataLength)).ToArray();
        }

        public StreamExtent[] PathToExtents(string path)
        {
            ReaderDirEntry entry = GetDirectoryEntry(path);
            if (entry == null)
            {
                throw new FileNotFoundException("File not found", path);
            }

            if (entry.Record.FileUnitSize != 0 || entry.Record.InterleaveGapSize != 0)
            {
                throw new NotSupportedException("Non-contiguous extents not supported");
            }

            return new[] { new StreamExtent(entry.Record.LocationOfExtent * IsoUtilities.SectorSize, entry.Record.DataLength) };
        }

        public ClusterMap BuildClusterMap()
        {
            long totalClusters = TotalClusters;
            ClusterRoles[] clusterToRole = new ClusterRoles[totalClusters];
            object[] clusterToFileId = new object[totalClusters];
            Dictionary<object, string[]> fileIdToPaths = new Dictionary<object, string[]>();

            ForAllDirEntries(
                string.Empty,
                (path, entry) =>
                {
                    string[] paths = null;
                    if (fileIdToPaths.ContainsKey(entry.UniqueCacheId))
                    {
                        paths = fileIdToPaths[entry.UniqueCacheId];
                    }

                    if (paths == null)
                    {
                        fileIdToPaths[entry.UniqueCacheId] = new[] { path };
                    }
                    else
                    {
                        string[] newPaths = new string[paths.Length + 1];
                        Array.Copy(paths, newPaths, paths.Length);
                        newPaths[paths.Length] = path;
                        fileIdToPaths[entry.UniqueCacheId] = newPaths;
                    }

                    if (entry.Record.FileUnitSize != 0 || entry.Record.InterleaveGapSize != 0)
                    {
                        throw new NotSupportedException("Non-contiguous extents not supported");
                    }

                    long clusters = Utilities.Ceil(entry.Record.DataLength, IsoUtilities.SectorSize);
                    for (long i = 0; i < clusters; ++i)
                    {
                        clusterToRole[i + entry.Record.LocationOfExtent] = ClusterRoles.DataFile;
                        clusterToFileId[i + entry.Record.LocationOfExtent] = entry.UniqueCacheId;
                    }
                });

            return new ClusterMap(clusterToRole, clusterToFileId, fileIdToPaths);
        }

        protected override File ConvertDirEntryToFile(ReaderDirEntry dirEntry)
        {
            if (dirEntry.IsDirectory)
            {
                return new ReaderDirectory(Context, dirEntry);
            }
            return new File(Context, dirEntry);
        }

        protected override string FormatFileName(string name)
        {
            if (_hideVersions)
            {
                int pos = name.LastIndexOf(';');
                if (pos > 0)
                {
                    return name.Substring(0, pos);
                }
            }

            return name;
        }

        private static DirectoryRecord ReadRootSelfRecord(IsoContext context)
        {
            context.DataStream.Position = context.VolumeDescriptor.RootDirectory.LocationOfExtent * context.VolumeDescriptor.LogicalBlockSize;
            byte[] firstSector = Utilities.ReadFully(context.DataStream, context.VolumeDescriptor.LogicalBlockSize);

            DirectoryRecord rootSelfRecord;
            DirectoryRecord.ReadFrom(firstSector, 0, context.VolumeDescriptor.CharacterEncoding, out rootSelfRecord);
            return rootSelfRecord;
        }
    }
}

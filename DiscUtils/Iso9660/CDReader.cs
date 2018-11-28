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

namespace DiscUtils.Iso9660
{
    using System.IO;
    using Vfs;

    /// <summary>
    /// Class for reading existing ISO images.
    /// </summary>
    public class CDReader : VfsFileSystemFacade
    {
        /// <summary>
        /// Initializes a new instance of the CDReader class.
        /// </summary>
        /// <param name="data">The stream to read the ISO image from.</param>
        /// <param name="joliet">Whether to read Joliet extensions.</param>
        /// <param name="hideVersions">Hides version numbers (e.g. ";1") from the end of files</param>
        protected CDReader(Stream data, bool joliet, bool hideVersions)
            : base(new VfsCDReader(data, joliet, hideVersions))
        {
        }

        /// <summary>
        /// Gets the size (in bytes) of each cluster.
        /// </summary>
        public long ClusterSize
        {
            get { return GetRealFileSystem<VfsCDReader>().ClusterSize; }
        }

        /// <summary>
        /// Gets the total number of clusters managed by the file system.
        /// </summary>
        public long TotalClusters
        {
            get { return GetRealFileSystem<VfsCDReader>().TotalClusters; }
        }

        /// <summary>
        /// Converts a cluster (index) into an absolute byte position in the underlying stream.
        /// </summary>
        /// <param name="cluster">The cluster to convert</param>
        /// <returns>The corresponding absolute byte position.</returns>
        public long ClusterToOffset(long cluster)
        {
            return GetRealFileSystem<VfsCDReader>().ClusterToOffset(cluster);
        }

        /// <summary>
        /// Converts an absolute byte position in the underlying stream to a cluster (index).
        /// </summary>
        /// <param name="offset">The byte position to convert</param>
        /// <returns>The cluster containing the specified byte</returns>
        public long OffsetToCluster(long offset)
        {
            return GetRealFileSystem<VfsCDReader>().OffsetToCluster(offset);
        }

        /// <summary>
        /// Converts a file name to the list of clusters occupied by the file's data.
        /// </summary>
        /// <param name="path">The path to inspect</param>
        /// <returns>The clusters</returns>
        /// <remarks>Note that in some file systems, small files may not have dedicated
        /// clusters.  Only dedicated clusters will be returned.</remarks>
        public Range<long, long>[] PathToClusters(string path)
        {
            return GetRealFileSystem<VfsCDReader>().PathToClusters(path);
        }

        /// <summary>
        /// Converts a file name to the extents containing its data.
        /// </summary>
        /// <param name="path">The path to inspect</param>
        /// <returns>The file extents, as absolute byte positions in the underlying stream</returns>
        /// <remarks>Use this method with caution - not all file systems will store all bytes
        /// directly in extents.  Files may be compressed, sparse or encrypted.  This method
        /// merely indicates where file data is stored, not what's stored.</remarks>
        public StreamExtent[] PathToExtents(string path)
        {
            return GetRealFileSystem<VfsCDReader>().PathToExtents(path);
        }

        /// <summary>
        /// Gets an object that can convert between clusters and files.
        /// </summary>
        /// <returns>The cluster map</returns>
        public ClusterMap BuildClusterMap()
        {
            return GetRealFileSystem<VfsCDReader>().BuildClusterMap();
        }
    }
}

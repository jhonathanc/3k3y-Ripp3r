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

using System.Collections.Generic;
using System.Linq;

namespace DiscUtils.Iso9660
{
    using System;
    using System.IO;

    public class ReaderDirEntry
    {
        internal ReaderDirEntry(ReaderDirectory readerDirectory, DirectoryRecord dirRecord)
        {
            Parent = readerDirectory;
            Records = new List<DirectoryRecord> {dirRecord};
            FileName = Record.FileIdentifier;

            LastAccessTimeUtc = Record.RecordingDateAndTime;
            LastWriteTimeUtc = Record.RecordingDateAndTime;
            CreationTimeUtc = Record.RecordingDateAndTime;
        }

        public ReaderDirectory Parent { get; private set; }

        public DirectoryRecord Record { get { return Records.FirstOrDefault(); } }
        public List<DirectoryRecord> Records { get; private set; }

        public bool IsDirectory
        {
            get
            {
                return (Record.Flags & FileFlags.Directory) != 0;
            }
        }

        public bool IsSymlink
        {
            get { return false; }
        }

        public string FileName { get; private set; }

        public string Path { get; internal set; }

        public bool HasVfsTimeInfo
        {
            get { return true; }
        }

        public DateTime LastAccessTimeUtc { get; private set; }

        public DateTime LastWriteTimeUtc { get; private set; }

        public DateTime CreationTimeUtc { get; private set; }

        public bool HasVfsFileAttributes
        {
            get { return true; }
        }

        public FileAttributes FileAttributes
        {
            get
            {
                FileAttributes attrs = 0;

                attrs |= FileAttributes.ReadOnly;

                if ((Record.Flags & FileFlags.Directory) != 0)
                {
                    attrs |= FileAttributes.Directory;
                }

                if ((Record.Flags & FileFlags.Hidden) != 0)
                {
                    attrs |= FileAttributes.Hidden;
                }

                return attrs;
            }
        }

        public long UniqueCacheId
        {
            get { return (((long)Record.LocationOfExtent) << 32) | Record.DataLength; }
        }

        /// <summary>
        /// Gets a version of FileName that can be used in wildcard matches.
        /// </summary>
        /// <remarks>
        /// The returned name, must have an extension separator '.', and not have any optional version
        /// information found in some files.  The returned name is matched against a wildcard patterns
        /// such as "*.*".
        /// </remarks>
        public string SearchName
        {
            get { return FileName.IndexOf('.') == -1 ? FileName + "." : FileName; }
        }
    }
}

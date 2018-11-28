using System;
using System.IO;

namespace Ripp3r.Iso9660
{
    public class DirectoryMemberInformation
    {
        public DirectoryMemberInformation(string path, long startSector, long totalSectors, long length, bool isFile)
        {
            Path = ParsePath(path);
            AltPath = ParsePath(path.MakeShortFileName());
            StartSector = startSector;
            TotalSectors = totalSectors;
            Length = length;
            IsFile = isFile;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ StartSector.GetHashCode();
        }

        public bool CheckPath(string path)
        {
            if (Path.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                AltPath.Equals(path.MakeShortFileName(),
                               StringComparison.OrdinalIgnoreCase))
                return true;

            if (Path.EndsWith(".") && Path.Equals(path + ".", StringComparison.OrdinalIgnoreCase))
                return true;
            if (AltPath.EndsWith(".") && AltPath.Equals(path.MakeShortFileName() + ".", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            DirectoryMemberInformation other = obj as DirectoryMemberInformation;
            if (other == null) return false;
            return Path == other.Path && StartSector == other.StartSector;
        }

        private static string ParsePath(string path)
        {
            path = path.Replace("\\", "/");
            if (path.Contains(";"))
            {
                string[] parts = path.Split(';');
                path = parts[0];
            }

            if (path.Length <= 1) return path;

            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);
            if (!path.StartsWith("/"))
                path = "/" + path;

            return path;
        }

        /// <summary>
        /// The full path of the file
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// The path with the shortname according to the ISO9660 specification
        /// </summary>
        public string AltPath { get; private set; }
        /// <summary>
        /// The sector at which this file starts
        /// </summary>
        public long StartSector { get; private set; }
        /// <summary>
        /// The sector at which this file ends
        /// </summary>
        public long TotalSectors { get; private set; }
        /// <summary>
        /// The total length of the file
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// Wether this is a file or directory
        /// </summary>
        public bool IsFile { get; private set; }

        /// <summary>
        /// Wether or not this file is added to the resulting iso
        /// </summary>
        public bool Added { get; set; }

        /// <summary>
        /// If the file is streamed from some location, use this as origin instead of a file
        /// </summary>
        public Stream Stream { get; set; }
    }
}

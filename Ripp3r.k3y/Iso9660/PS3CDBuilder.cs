using System;
using System.Collections.Generic;
using System.Linq;
using DiscUtils;
using DiscUtils.Iso9660;

namespace Ripp3r.Iso9660
{
    class PS3CDBuilder : CDBuilder
    {
        private static string GetPath(FileExtent fe)
        {
            string path = GetPath(fe.FileInfo.Parent, false) + fe.FileInfo.Name;
            if (path.Contains(";"))
            {
                string[] parts = path.Split(';');
                path = parts[0];
            }

            return path;
        }

        private static string GetPath(BuildDirectoryInfo bdi, bool stripSlash = true)
        {
            string path = string.Empty;

            do
            {
                path = (bdi.Name != "\0" ? bdi.Name : string.Empty) + "/" + path;
                bdi = bdi.Parent;
            } while (bdi != bdi.Parent);

            // Stip trailing slash
            if (stripSlash && path != "/") path = path.Substring(0, path.Length - 1);
            if (path[0] != '/') path = "/" + path;

            return path;
        }

        private DirectoryMemberInformation Find(string path)
        {
            return
                ExtentInfo.FirstOrDefault(
                    e => e.Path != null && (
                                               e.Path.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                                               e.AltPath.Equals(path.MakeShortFileName(),
                                                             StringComparison.OrdinalIgnoreCase)));
        }

        protected override List<BuilderExtent> FixExtents(out long totalLength)
        {
            List<BuilderExtent> extent = base.FixExtents(out totalLength);

            bool invalid = false;
            if (ExtentInfo != null)
            {
                PathTable primary = extent.OfType<PathTable>().First();
                PathTable suppl = extent.OfType<PathTable>().Skip(2).First();

                // Delete the pathtable extents with unicode encoding (we're not using the partition table by DiscUtils anyway)
                extent.Remove(extent.OfType<PathTable>().Skip(1).First());
                extent.Remove(extent.OfType<PathTable>().Last());

                // Change every calculated extent to match the information specified in the TOC
                foreach (BuilderExtent builderExtent in extent)
                {
                    DirectoryMemberInformation dmi = null;
                    FileExtent fe = builderExtent as FileExtent;
                    string path = null;
                    if (fe != null)
                    {
                        // Find the file, and fix the offset
                        path = GetPath(fe);
                    }
                    DirectoryExtent de = builderExtent as DirectoryExtent;
                    if (de != null)
                    {
                        // Find the path, and fix the offset
                        path = GetPath(de.DirectoryInfo);
                    }

                    if (!string.IsNullOrEmpty(path)) dmi = Find(path);

                    if (dmi == null)
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            invalid = true;
                            Interaction.Instance.ReportMessage(
                                string.Format("The JB directory contains a file '{0}' which is not part of the ISO.",
                                              path));
                        }
                        continue;
                    }

                    builderExtent.Start = dmi.StartSector*Utilities.SectorSize;
                    if (fe != null)
                    {
                        if (builderExtent.Length.RoundToSector() != dmi.TotalSectors)
                        {
                            if (path != "/PS3_UPDATE/PS3UPDAT.PUP")
                            {
                                invalid = true;
                                Interaction.Instance.ReportMessage(
                                    string.Format(@"The file '{2}' has a different size (expected: {0}, actual: {1})",
                                                  dmi.Length, builderExtent.Length, path));
                            }
                            else
                            {
                                Interaction.Instance.ReportMessage(
                                    string.Format(@"Padding file '{2}' to size {0} (actual: {1})",
                                                  dmi.Length, builderExtent.Length, path));                                
                            }
                        }

                        if (!primary.Locations.ContainsKey(fe.FileInfo)) throw new InvalidFileSystemException(string.Format("The file '{0}' does not exists in the partition table of the iso.", path));
                        primary.Locations[fe.FileInfo] = (uint) builderExtent.Start;
                        suppl.Locations[fe.FileInfo] = (uint)builderExtent.Start;
                    }
                    if (de == null) continue;

                    if (!primary.Locations.ContainsKey(de.DirectoryInfo)) throw new InvalidFileSystemException(string.Format("The file '{0}' does not exists in the partition table of the iso.", path));
                    primary.Locations[de.DirectoryInfo] = (uint)builderExtent.Start;
                    suppl.Locations[de.DirectoryInfo] = (uint)builderExtent.Start;
                }
            }
            totalLength = TotalLength;

            if (invalid)
            {
                throw new PS3BuilderException();
            }

            return extent;
        }

        public ICollection<DirectoryMemberInformation> ExtentInfo { get; set; }

        public long TotalLength { get; set; }
    }

    public class PS3BuilderException : Exception
    {
    }
}

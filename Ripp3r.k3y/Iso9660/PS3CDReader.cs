using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscUtils;
using DiscUtils.Iso9660;

namespace Ripp3r.Iso9660
{
    public class PS3CDReader : CDReader
    {
        private readonly List<DirectoryMemberInformation> directoryMembers;

        public PS3CDReader(Stream data) : base(data, true, true)
        {
            directoryMembers = new List<DirectoryMemberInformation>();
            ParseCd();

            directoryMembers.Sort((m1, m2) => m1.StartSector == m2.StartSector ? 0 : m1.StartSector < m2.StartSector ? -1 : 1);
        }

        private void ParseCd(DiscDirectoryInfo pathDir = null)
        {
            if (pathDir == null)
            {
                pathDir = GetDirectoryInfo("\\");
                Range<long, long> r = PathToClusters("\\").First();
                directoryMembers.Add(new DirectoryMemberInformation(pathDir.FullName, r.Offset, r.Count.RoundToSector(),
                                                                    0, false) {Added = true});
            }

            foreach (DiscDirectoryInfo dir in pathDir.GetDirectories())
            {
                DirectoryRecord rec = dir.Entry.Record;
                long offset = rec.LocationOfExtent;
                long count = rec.DataLength;
                directoryMembers.Add(new DirectoryMemberInformation(dir.FullName, offset, count.RoundToSector(), 0, false));
                ParseCd(dir);
            }
            foreach (DiscFileInfo f in pathDir.GetFiles())
            {
                ReaderDirectory rdr = f.Entry.Parent;
                
                // Fetch all cluster information from the parent
                IEnumerable<ReaderDirEntry> entries = rdr.GetEntriesByName(f.Entry.FileName);

                long offset = f.Entry.Record.LocationOfExtent;
                long count = entries.Sum(e => e.Record.DataLength);

                directoryMembers.Add(new DirectoryMemberInformation(f.FullName, offset, count.RoundToSector(), count, true));
            }
        }

        public ICollection<DirectoryMemberInformation> Members
        {
            get { return directoryMembers; }
        }
    }
}

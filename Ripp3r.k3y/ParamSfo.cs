using System.IO;
using System.Linq;
using System.Text;

namespace Ripp3r
{
    public class ParamSfo
    {
        public class IndexTable
        {
            public ushort KeyTableOffset { get; private set; }
            public ushort ParamFormat { get; private set; }
            public uint ParamLength { get; private set; }
            public uint ParamMaxLength { get; private set; }
            public uint DataTableOffset { get; private set; }

            internal IndexTable(BinaryReader br)
            {
                KeyTableOffset = br.ReadUInt16();
                ParamFormat = br.ReadUInt16();
                ParamLength = br.ReadUInt32();
                ParamMaxLength = br.ReadUInt32();
                DataTableOffset = br.ReadUInt32();
            }

            public string Name { get; internal set; }
            public string StringValue { get; internal set; }
            public int IntValue { get; internal set; }
        }

        public string Magic { get; private set; }
        public uint Version { get; private set; }

        public IndexTable[] IndexTables { get; private set; }

        public static ParamSfo Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Load(fs);
            }
        }

        public static ParamSfo Load(Stream s)
        {
            // Read magic
            using (BinaryReader br = new BinaryReader(s))
            {
                ParamSfo sfo = new ParamSfo();
                sfo.Magic = Encoding.ASCII.GetString(br.ReadBytes(4)).Trim('\0');
                if (sfo.Magic != "PSF")
                    throw new FileLoadException("Invalid SFO file");

                sfo.Version = br.ReadUInt32(); // Version

                uint keyTableOffset = br.ReadUInt32();
                uint dataTableOffset = br.ReadUInt32();
                uint indexTableEntries = br.ReadUInt32();

                sfo.IndexTables = new IndexTable[indexTableEntries];
                for (int i = 0; i < indexTableEntries; i++)
                    sfo.IndexTables[i] = new IndexTable(br);

                for (int i = 0; i < indexTableEntries; i++)
                {
                    s.Position = keyTableOffset + sfo.IndexTables[i].KeyTableOffset;

                    uint size = ((i == indexTableEntries - 1)
                                     ? dataTableOffset - keyTableOffset
                                     : sfo.IndexTables[i + 1].KeyTableOffset) - sfo.IndexTables[i].KeyTableOffset;

                    sfo.IndexTables[i].Name = Encoding.ASCII.GetString(br.ReadBytes((int) size)).Trim('\0');

                    s.Position = dataTableOffset + sfo.IndexTables[i].DataTableOffset;

                    switch (sfo.IndexTables[i].ParamFormat)
                    {
                        case 0x404:
                            // Read integer
                            sfo.IndexTables[i].IntValue = br.ReadInt32();
                            break;
                        default:
                            sfo.IndexTables[i].StringValue =
                                Encoding.UTF8.GetString(br.ReadBytes((int) sfo.IndexTables[i].ParamLength)).Trim('\0');
                            break;
                    }
                }

                return sfo;
            }
        }

        public string GetStringValue(string key)
        {
            IndexTable ix = IndexTables.FirstOrDefault(i => i.Name == key);
            return ix != null ? ix.StringValue : string.Empty;
        }
    }
}

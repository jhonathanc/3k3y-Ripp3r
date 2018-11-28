using System.IO;
using System.Linq;
using Ionic.Crc;

namespace Ripp3r.Streams
{
    internal class SevenZipStream : ZipStream
    {
        public SevenZipStream(string path, FileMode fileMode, FileAccess fileAccess) 
            : base(path, fileMode, fileAccess)
        {
        }

        private string GetPartName(int part)
        {
            return isMultipart ? string.Format("{0}.zip.{1:000}", basePath, part) : string.Concat(basePath, ".zip");
        }

        protected override CrcCalculatorStream CreatePart(int partnum)
        {
            if (partnum > 1 && internalStream != null)
            {
                if (currentPart != null)
                    currentPart.Crc = (uint) internalStream.Crc;

                internalStream.Close();
                internalStream.Dispose();
            }

            string partName = GetPartName(partnum);

            // Create a new part
            PartialFile newPart = new PartialFile(partName, partSize, partnum, Position);
            Parts.Add(newPart);
            currentPart = newPart;

            // Create a new part
            positionInPart = 0;
            return new CrcCalculatorStream(new FileStream(partName, mode, access), false);
        }

        public override void Close()
        {
            base.Close();

            if (!isMultipart || Parts.Count != 1) return;

            // Started as multipart, but it's only one! Let's rename it
            isMultipart = false;
            string newFile = GetPartName(1);
            PartialFile p = Parts.First();

            if (File.Exists(newFile)) File.Delete(newFile);
            File.Move(p.Filename, newFile);
            p.Filename = newFile;
        }

        protected override void FindParts()
        {
            int part = 1;
            FileInfo info;
            length = 0;

            // Find all parts with the name .zip.<partnum>
            while ((info = new FileInfo(GetPartName(part))).Exists)
            {
                Parts.Add(new PartialFile(info.FullName, info.Length, part++, length));
                length += info.Length;

                if (!isMultipart) break;
            }
            Parts.Last().IsLast = true;
            amountOfParts = Parts.Count;
        }
    }
}

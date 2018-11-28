using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ripp3r
{
    public static class CdRom
    {
        public static ODD CheckDrive(string driveletter)
        {
            ODD odd = ODD.getODD(driveletter);
            return odd;
        }

        public static ODD FindDrive()
        {
            ODD odd = null;
            IEnumerable<DriveInfo> cd = DriveInfo.GetDrives().Where(c => c.DriveType == DriveType.CDRom);
            foreach (var driveInfo in cd)
            {
                odd = CheckDrive(driveInfo.RootDirectory.Name.Substring(0, 2));
                if (odd != null) break;
            }
            return odd;
        }
    }
}

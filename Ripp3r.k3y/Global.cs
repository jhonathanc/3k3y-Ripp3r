using System;
using System.Globalization;
using System.IO;

namespace Ripp3r
{
    public delegate void ReportProgressHandler(int progress);

    public static class Static
    {
        public static string DirectorySeperator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

        public static bool IsRunningMono
        {
            get { return Type.GetType("Mono.RunTime") != null; }
        }
    }
}

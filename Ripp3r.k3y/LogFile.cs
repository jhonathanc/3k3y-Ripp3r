using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ripp3r
{
    public class LogFile : IDisposable
    {
        private const string k3yRipper = "3k3y_ripper.{0}.log";
        private StreamWriter streamWriter;

        public LogFile()
        {
            // Create a new logfile, and move a few files

            string path = Path.GetTempPath();

            // Clean old logfiles
            List<string> logfiles = Directory.GetFiles(path, "3k3y_ripper.*.log").ToList();
            logfiles.Sort();
            while (logfiles.Count > 4)
            {
                File.Delete(logfiles[0]);
                logfiles.RemoveAt(0);
            }

            string filename = Path.Combine(path, string.Format(k3yRipper, DateTime.Now.ToString("yyyyMMddHHmmss")));
            streamWriter = new StreamWriter(filename, false);
        }

        public void Log(string log)
        {
            streamWriter.Write(DateTime.Now.ToShortTimeString());
            streamWriter.Write(": ");
            streamWriter.WriteLine(log);
            streamWriter.Flush();
        }

        public void Dispose()
        {
            if (streamWriter == null) return;

            streamWriter.Dispose();
            streamWriter = null;
        }
    }
}

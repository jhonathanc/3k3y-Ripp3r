using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ripp3r
{
    public class Ripp3rUpdate
    {
        private const string VERSION_URL = "http://www.3k3y.com/ripp3r/version.txt";
        private const string SETUP_URL = "http://www.3k3y.com/ripp3r/Setup.msi";

        public static void CheckForUpdate(bool notifyAlways = false)
        {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += OnDownloadComplete;
            wc.DownloadStringAsync(new Uri(VERSION_URL), notifyAlways);
        }

        private static void OnDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            bool notifyAlways = (bool) e.UserState;

            if (e.Error == null)
            {
                string[] version_and_notes = e.Result.Split(new[] {"\n", "\r\n"}, 2, StringSplitOptions.None);
                if (version_and_notes.Length == 2)
                {
                    string version = version_and_notes[0].Trim();
                    string notes = version_and_notes[1].Trim();

                    Version remoteVersion;
                    if (Version.TryParse(version, out remoteVersion))
                    {

                        if (typeof (Ripp3rUpdate).Assembly.GetName().Version < remoteVersion)
                        {
                            if (notifyAlways)
                                Interaction.Instance.ReportMessage(string.Format("Updated version {0} found", remoteVersion.ToString(2)), ReportType.Success);

                            // Update found, report that
                            Interaction.Instance.UpdateFound(remoteVersion, notes);

                            return;
                        }
                    }
                }
            }

            if (notifyAlways)
            {
                Interaction.Instance.ReportMessage("No update found");
            }
        }

        public
            async Task DownloadAndInstallUpdate(CancellationToken token)
        {
            Interaction.Instance.TaskBegin(true);
            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(SETUP_URL);
            try
            {
                HttpWebResponse response = (HttpWebResponse) await webRequest.GetResponseAsync();
                if (token.IsCancellationRequested) return;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Interaction.Instance.ReportMessage("Download of update failed");
                }
                else
                {
                    string tempSetup = Path.Combine(Path.GetTempPath(), "Ripp3r Setup.msi");
                    Interaction.Instance.SetProgressMaximum((int)response.ContentLength);
                    using (Stream s = response.GetResponseStream())
                    {
                        if (s == null) return;
                        using (FileStream fs = new FileStream(tempSetup, FileMode.Create, FileAccess.Write))
                        {
                            // Get the length
                            int bytesRead, totalRead = 0;
                            do
                            {
                                byte[] buffer = new byte[2048];
                                bytesRead = await s.ReadAsync(buffer, 0, buffer.Length);
                                await fs.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;
                                Interaction.Instance.ReportProgress(totalRead);
                                if (token.IsCancellationRequested) break;
                            } while (bytesRead > 0);
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        File.Delete(tempSetup);
                    }
                    else
                    {
                        // Execute the setup and exit this application
                        Process.Start(tempSetup);
                        Interaction.Instance.Terminate();
                    }
                }
            }
            catch (WebException)
            {
            }
            Interaction.Instance.TaskComplete();
        }
    }
}

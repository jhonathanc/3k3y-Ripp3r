using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ripp3r
{
    public class IrdUploader
    {
        private const string URL = "http://www.3k3y.com/packages/ird_files/libraries/ird/upload.php";
        private const string SEARCHURL = "http://www.3k3y.com/packages/ird_files/libraries/ird/search.php";
        private const string BASEURL = "http://www.3k3y.com/ird_files/view_detail/";

        public static async Task Search(string gameid, string appVersion, string gameVersion)
        {
            Encryption enc = new Encryption();
            string queryString = string.Concat("gameid=", gameid, "&app_ver=", appVersion, "&game_ver=", gameVersion);
            string authInfo = string.Format("{0}:{1}", enc.PublicKey.Join().Md5().AsString(),
                                            enc.EncryptQuerystring(queryString).AsString());
            try
            {
                HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(SEARCHURL);
                wr.Method = "POST";
                wr.Headers["Authentication"] = "K3Y " + Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));

                using (HttpWebResponse resp = (HttpWebResponse) (await wr.GetResponseAsync()))
                {
                    string response = GetResponseString(resp);
                    ProcessMessages(response);
                }
            }
            catch (WebException)
            {
                Interaction.Instance.ReportMessage("Unable to find the IRD file for this game.");
            }
        }

        public static async Task Upload(CancellationToken cancellation, string filename = null)
        {
            string[] filenames;

            // Find the IRD file
            if (string.IsNullOrEmpty(filename))
                filenames = await Interaction.Instance.GetIrdFiles();
            else
                filenames = new[] {filename};
            
            if (filenames == null || filenames.Length == 0) return;

            Interaction.Instance.TaskBegin();

            int totalSize = (int) filenames.Sum(f => new FileInfo(f).Length);
            Interaction.Instance.SetProgressMaximum(totalSize);

            int processedSize = 0;
            foreach (var f in filenames)
            {
                string hash;
                try
                {
                    hash = IrdFile.GetHash(f);
                }
                catch (FileLoadException)
                {
                    Interaction.Instance.ReportMessage(
                        "Version of IRD file " + Path.GetFileName(f) + " cannot be handled", ReportType.Fail);
                    continue;
                }

                byte[] content = File.ReadAllBytes(f);

                Encryption enc = new Encryption();
                enc.Encrypt(content, hash);

                try
                {
                    string authInfo = string.Format("{0}:{1}", enc.PublicKey.Join().Md5().AsString(),
                                                    enc.EncryptedKey.AsString());

                    HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(URL);
                    wr.Method = "POST";
                    wr.Headers["Authentication"] = "K3Y " + Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
                    using (Stream s = await wr.GetRequestStreamAsync())
                    {
                        if (cancellation.IsCancellationRequested) return;

                        int bytesToWrite = enc.EncryptedContent.Length;
                        totalSize += bytesToWrite - content.Length;

                        Interaction.Instance.SetProgressMaximum(totalSize);
                        int bytesWritten = 0;
                        while (bytesToWrite > 0)
                        {
                            int l = bytesToWrite > 2048 ? 2048 : bytesToWrite;
                            await s.WriteAsync(enc.EncryptedContent, bytesWritten, l);
                            bytesWritten += l;
                            bytesToWrite -= l;
                            processedSize += l;

                            Interaction.Instance.ReportProgress(processedSize);
                            if (cancellation.IsCancellationRequested) return;
                        }
                    }
                    using (HttpWebResponse response = (HttpWebResponse) await wr.GetResponseAsync())
                    {
                        // Fetch the response
                        string resp = GetResponseString(response);

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Interaction.Instance.ReportMessage(Path.GetFileName(f) + ": IRD file upload failed: " +
                                (string.IsNullOrEmpty(resp) ? response.StatusDescription : resp), ReportType.Fail);
                        }
                        else
                        {
                            Interaction.Instance.ReportMessage(Path.GetFileName(f) + ": IRD file successfully uploaded", ReportType.Success);
                            ProcessMessages(resp);
                        }
                    }
                }
                catch (WebException e)
                {
                    string errorMessage = e.Response != null ? e.Response.Headers["X-Error"] : null;
                    if (!string.IsNullOrEmpty(errorMessage))
                        Interaction.Instance.ReportMessage(Path.GetFileName(f) + ": Upload failed: " + errorMessage, ReportType.Fail);
                    else
                        Interaction.Instance.ReportMessage(Path.GetFileName(f) + ": Upload failed: " + e.Message, ReportType.Fail);
                }
            }

            Interaction.Instance.TaskComplete();
        }

        private static string GetResponseString(WebResponse response)
        {
            string resp = null;
            using (Stream s = response.GetResponseStream())
            {
                if (s != null)
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        resp = sr.ReadToEnd();
                    }
                }
            }
            return resp;
        }

        private static void ProcessMessages(string resp)
        {
            if (string.IsNullOrEmpty(resp)) return;

            string[] messages = resp.Split('|');
            foreach (string message in messages)
            {
                string[] parts = message.Split('=');
                if (parts[0] == "unknown_title")
                {
                    // Add a line asking you to fill out GameTDB
                    Interaction.Instance.ReportMessage(
                        "This game is not yet known in the GameTDB database|http://www.gametdb.com/PS3/" +
                        parts[1], ReportType.Url);
                }
                else if (parts[0] == "ird_file_id")
                {
                    // Add a line pointing you directly to the IRD file
                    Interaction.Instance.ReportMessage(
                        "Click for the correct IRD for this game|" + BASEURL +
                        parts[1], ReportType.Url);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ionic.Zip;
using Ripp3r.k3y.Properties;

namespace Ripp3r
{
    public class GameTDB : IDisposable
    {
        private const string filename = "gametdb_{0}.xml";
        private const string GameTdbUrl = "http://www.gametdb.com/ps3tdb.zip?LANG=";
        private const string CoverHQUrl = "http://art.gametdb.com/ps3/coverHQ/{1}/{0}.jpg"; // {0} = gameid, {1} = language
        private const string CoverMUrl = "http://art.gametdb.com/ps3/coverM/{1}/{0}.jpg"; // {0} = gameid, {1} = language

        private static readonly Dictionary<string, string[]> LanguageForRegion = new Dictionary<string, string[]>
                {
                    {"A", new[] {"ZH"}},
                    {"J", new[] {"JA"}},
                    {"U", new[] {"US"}},
                    {"K", new[] {"KO"}},
                    {"E", new[] {"EN, FR, DE, ES, PT, IT, RU"}} // PAL
                };

        private CancellationTokenSource cancellationTokenSource;
        private Task loadTask;
        private bool isEnabled = Interaction.Instance.GameTDB;
        private string language = Interaction.Instance.GameTDBLanguage;
        private XDocument gameTdb;

        private readonly string path;

        public GameTDB()
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "3K3Y");
            Directory.CreateDirectory(path);
            if (isEnabled) StartLoading();
        }

        private async void RestartLoading()
        {
            StopLoading();
            if (loadTask != null) await loadTask;
            StartLoading();
        }

        private void StartLoading()
        {
            if (cancellationTokenSource != null) return;
            cancellationTokenSource = new CancellationTokenSource();
            loadTask =
                TaskEx.Run(() => LoadGameTDB(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        private void StopLoading()
        {
            if (cancellationTokenSource == null) return;
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value) return;
                if (value) StartLoading();
                else StopLoading();
                isEnabled = value;
            }
        }

        public string Language
        {
            get { return language; }
            set 
            { 
                if (language == value) return;
                language = value;
                TaskEx.Run(() => RestartLoading());
            }
        }

        public void Dispose()
        {
            StopLoading();
        }

        private XElement FindGame(string id)
        {
            return gameTdb == null
                       ? null
                       : gameTdb.Descendants("game").First(g => g.Descendants("id").First().Value == id);
        }

        private static string ParseElement(XElement elm)
        {
            return "<infoitem name=\"" + elm.Name + "\">" + elm.Value + "</infoitem>";
        }

        private static string ParseDate(XElement elm)
        {
            string y = elm.Attribute("year").Value;
            string m = elm.Attribute("month").Value;
            string d = elm.Attribute("day").Value;

            return "<infoitem name=\"Releasedate\">" + y + "/" + m + "/" + d + "</infoitem>";
        }

        private static string ParseWifi(XElement element)
        {
            int players;
            int.TryParse(element.Attribute("players").Value, out players);
            string retval = string.Empty;
            if (players != 0)
            {
                retval += "<infoitem name=\"Online players\">" + players + "</infoitem>";
            }

            string options = string.Join(", ",
                                         element.Descendants("feature")
                                                .Select(e => e.Value)
                                                .Except(new[] {"online"})
                                                .ToArray());
            if (!string.IsNullOrEmpty(options)) retval += "<infoitem name=\"Online features\">" + options + "</infoitem>";
            return retval;
        }

        private static string ParseInput(XElement element)
        {
            int players;
            int.TryParse(element.Attribute("players").Value, out players);
            string retval = string.Empty;
            if (players != 0)
            {
                retval += "<infoitem name=\"Local players\">" + players + "</infoitem>";
            }

            List<string> reqControllers = new List<string>();
            List<string> optControllers = new List<string>();
            foreach (XElement control in element.Descendants("control"))
            {
                if (control.Attribute("required").Value == "false")
                    optControllers.Add(control.Attribute("type").Value);
                else
                    reqControllers.Add(control.Attribute("type").Value);
            }
            if (reqControllers.Count > 0) retval += "<infoitem name=\"Required controllers\">" + string.Join(", ", reqControllers) + "</infoitem>";
            if (optControllers.Count > 0) retval += "<infoitem name=\"Optional controllers\">" + string.Join(", ", optControllers) + "</infoitem>";
            return retval;
        }

        public async Task<XDocument> GetInfo(string id)
        {
            if (loadTask != null) await loadTask;

            // The XML file should be completely read by now
            XElement elm = FindGame(id);
            if (elm == null) return null;

            IEnumerable<XElement> locales = elm.Descendants("locale").ToArray();
            // Find the correct locale
            XElement locale = locales.FirstOrDefault(l => l.Attribute("lang").Value == language) ??
                              locales.FirstOrDefault(l => l.Attribute("lang").Value == "EN");
            if (locale == null) return null; // No locale found, this is impossible afaik

            string xml = "<xml version=\"1.0\"?><gameinfo><title>" + locale.Descendants("title").First().Value + "</title><summary>";
            xml += locale.Descendants("synopsis").First().Value;
            xml += "</summary><info>";

            XName[] tagsToSkip = new XName[] {"id", "type", "locale", "rating", "rom"};
            foreach (XElement node in elm.DescendantNodes().Cast<XElement>().Where(node => !tagsToSkip.Contains(node.Name)))
            {
                // Add the rest as info item
                if (node.Name == "date") xml += ParseDate(node);
                else if (node.Name == "wi-fi") xml += ParseWifi(node);
                else if (node.Name == "input") xml += ParseInput(node);

                xml += ParseElement(node);
            }

            xml += "</info>";

            return XDocument.Parse(xml);
        }

        private async Task<HttpWebResponse> DoRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);
            webRequest.UserAgent = "3K3Y Ripper, version " + GetType().Assembly.GetName().Version;
            return (HttpWebResponse) (await webRequest.GetResponseAsync());
        }

        private async Task<Stream> GetCover(string coverUrl)
        {
            HttpWebResponse response = await DoRequest(coverUrl);
            return response.StatusCode == HttpStatusCode.OK ? response.GetResponseStream() : null;
        }

        public async Task<Image> FindCover(string id)
        {
            // First, try to fetch a cover for the language
            Stream coverStream = await GetCover(string.Format(CoverHQUrl, id, language)) ??
                                 await GetCover(string.Format(CoverMUrl, id, language));

            // No cover found yet?
            if (coverStream == null)
            {
                // Parse the region, and try to find a cover for the region
                string regionCode = id.Substring(2, 1);
                string[] languages = LanguageForRegion.ContainsKey(regionCode)
                                         ? LanguageForRegion[regionCode]
                                         : new string[0];

                foreach (string lang in languages.Except(new[] {language}))
                {
                    coverStream = await GetCover(string.Format(CoverHQUrl, id, lang)) ??
                                  await GetCover(string.Format(CoverMUrl, id, lang));

                    if (coverStream != null) break;
                }
            }

            // Cover not found...too bad
            if (coverStream == null) return null;

            Image img = Image.FromStream(coverStream);
            coverStream.Dispose();

            return img;
        }

        private async Task Update(CancellationToken token)
        {
            HttpWebResponse response = await DoRequest(string.Concat(GameTdbUrl, language));
            if (token.IsCancellationRequested) return;

            using (Stream zipStream = response.GetResponseStream())
            {
                if (zipStream == null) return;

                using (MemoryStream memStream = new MemoryStream())
                {
                    await zipStream.CopyToAsync(memStream);
                    memStream.Position = 0;
                    // Unzip the stream
                    ZipFile zf = ZipFile.Read(memStream);
                    ZipEntry tdbEntry = zf.Entries.FirstOrDefault();
                    if (tdbEntry == null) return;

                    string filePath = Path.Combine(path, string.Format(filename, language));
                    using (
                        FileStream fs = new FileStream(filePath, FileMode.Create,
                                                       FileAccess.Write))
                    {
                        tdbEntry.Extract(fs);
                        Settings.Default.GameTDBLastUpdate = DateTime.Now;
                        Settings.Default.Save();
                    }
                }
            }
        }

        private async void LoadGameTDB(CancellationToken token)
        {
            if (RequiresUpdate())
                await Update(token);

            if (token.IsCancellationRequested) return;

            // Parse the xml file
            string filePath = Path.Combine(path, string.Format(filename, language));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                gameTdb = XDocument.Load(fs);
            }
        }

        private bool RequiresUpdate()
        {
            // Try to open the file, if it exists
            string filePath = Path.Combine(path, string.Format(filename, language));
            return !File.Exists(filePath) ||
                    ((DateTime.Now - Settings.Default.GameTDBLastUpdate).TotalHours >= 24);
        }
    }
}

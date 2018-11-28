using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnrar.Common;
using NUnrar.Reader;
using Ripp3r.Streams;

namespace Ripp3r
{
    internal static class UpdateDownloader
    {
        // {0}: en/us/jp
        // {1}: year
        // {2}: month
        // {3}: day
        // {4}: md5
        // {5}: version

        private const string URL =
            "http://d{0}01.ps3.update.playstation.net/update/ps3/image/{0}/{1:0000}_{2:00}{3:00}_{4}/PS3UPDAT.PUP";

        private const string URL2 = "http://dl.console.se/download/PS3/Firmware/Retail%20-%20OFW/PS3%20Update%20{0}.rar";

        private static bool isRarStream;
        private static int updateSize;

        private static async Task<Stream> OpenUrl(Update update, string gameId, bool retry = false)
        {
            try
            {
                string url = !retry
                                 ? string.Format(URL, GetServer(gameId), update.Year, update.Month, update.Day, update.Md5)
                                 : string.Format(URL2, update.Version);

                isRarStream = retry;
                WebClient wc = new WebClient();
                
                Stream s = await wc.OpenReadTaskAsync(url);
                int.TryParse(wc.ResponseHeaders["Content-Length"], out updateSize);
                return s;
            }
            catch (WebException e)
            {
                if (retry || ((HttpWebResponse) e.Response).StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            return await OpenUrl(update, gameId, true);
        }

        public static async Task<bool> Download(string version, string gameId, string outputFile, CancellationToken cancellation)
        {
            // Check if the version is known
            Update update = Update.Find(version);

            string dir = Path.GetDirectoryName(outputFile);
            if (string.IsNullOrEmpty(dir)) throw new ArgumentException(@"Outputfile must be a full path", outputFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            Interaction.Instance.SetProgressMaximum(updateSize != 0 ? updateSize : update.Size);
            try
            {
                using (Stream s = await OpenUrl(update, gameId))
                {
                    if (cancellation.IsCancellationRequested) return false;

                    string tmpFile = string.Concat(outputFile, ".tmp");
                    using (FileStream fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write))
                    {
                        if (isRarStream)
                        {
                            // Wrap into unrar
                            RarReader reader = RarReader.Open(s, RarOptions.None);
                            if (!reader.MoveToNextEntry())
                            {
                                return false;
                            }
                            reader.WriteEntryTo(new WriteProgressStream(fs), cancellation);
                        }
                        else
                        {
                            int bytesRead = -1, totalBytesRead = 0;
                            byte[] buffer = new byte[2048];
                            while (bytesRead != 0)
                            {
                                if (cancellation.IsCancellationRequested) break;

                                bytesRead = await s.ReadAsync(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                Interaction.Instance.ReportProgress(totalBytesRead);
                            }
                        }
                    }

                    if (cancellation.IsCancellationRequested)
                    {
                        File.Delete(tmpFile);
                        return false;
                    }
                    if (File.Exists(outputFile)) File.Delete(outputFile);
                    File.Move(tmpFile, outputFile);
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private static string GetServer(string gameId)
        {
            switch (gameId[2])
            {
                case 'E':
                    return "eu";
                case 'J':
                case 'K':
                    return "jp";
                default:
                    return "us";
            }
        }
    }

    internal class Update
    {
        public string Version { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }
        public int Year { get; private set; }
        public int Size { get; private set; }
        public string Md5 { get; private set; }

        private Update(string version, int month, int day, int year, int size, string md5)
        {
            Version = version;
            Month = month;
            Day = day;
            Year = year;
            Size = size;
            Md5 = md5;
        }

        private static List<Update> Updates { get; set; }

        public static Update Find(string version)
        {
            return Updates.LastOrDefault(u => u.Version == version);
        }

        // List from http://www.eurasia.nu/wiki/index.php/Ps3OsRels
        static Update()
        {
            Updates = new List<Update>
                {
                    new Update("1.10", 11, 28, 2006, 102984576, "374371771ad1608d0684c4d588440f0b"),
                    new Update("1.11", 12, 01, 2006, 102994816, "8b6b4fb0a5fdd68bf85f02d4f6a0f096"),
                    new Update("1.30", 12, 06, 2006, 102093696, "d2c1aa63e55f5eb74e9846112d843f2b"),
                    new Update("1.31", 12, 13, 2006, 102103936, "602a1373cf1845d56348698d7037ab1b"),
                    new Update("1.32", 12, 21, 2006, 102114176, "c4b08159eaa9fce111e076fb255aa106"),
                    new Update("1.50", 01, 24, 2007, 103768480, "9d7f3830fb308202f303bd0ce2f17c9"),
                    new Update("1.51", 02, 02, 2007, 103768480, "afd01d9c72b3b69c03a0206c45d87dda"),
                    new Update("1.54", 02, 28, 2007, 103758240, "891bf61f56808119c4968ac3a9101540"),
                    new Update("1.60", 03, 22, 2007, 109061008, "3a5f49bcee3948301aa698ed57ecb10e"),
                    new Update("1.70", 04, 19, 2007, 111959400, "468238974943b45c09aeb632853401f2"),
                    new Update("1.80", 05, 24, 2007, 115956904, "fd9887626f734b46b271aec58e827cb2"),
                    new Update("1.81", 06, 15, 2007, 115947032, "b81fe78b163b62ce8983ad990306fe57"),
                    new Update("1.82", 06, 28, 2007, 115947032, "25de800491e8e88104d7e30b7524e9dc"),
                    new Update("1.90", 07, 24, 2007, 125177152, "bcefc8569ae4c2f4b7c09fd52f728c02"),
                    new Update("1.92", 09, 04, 2007, 125279424, "0f99147438936578951dd9d3f514000a"),
                    new Update("1.93", 09, 13, 2007, 125279424, "9c95692d4dd1c1e17c2b0ce5dcaebba1"),
                    new Update("2.00", 11, 08, 2007, 124698576, "f9df10bb57cdeeef8395c39415f54965"),
                    new Update("2.01", 11, 20, 2007, 124699328, "1d8e69249aa1a5593307cf7d8ca8a331"),
                    new Update("2.10", 12, 18, 2007, 125482288, "08393f1bd8e91589c95837142caf0a58"),
                    new Update("2.16", 03, 03, 2008, 268435456, "73fe70e0198b80f01525b6b72774af25"),
                    new Update("2.17", 03, 13, 2008, 125400160, "b859ca0f15fe9516d85fd0d89444ac5b"),
                    new Update("2.20", 03, 25, 2008, 127729779, "d33eb3ef5721bc940ca29b4c80b96bde"),
                    new Update("2.30", 04, 15, 2008, 130530965, "dd2e675fce5d2eb28ca93bc32edb3de6"),
                    new Update("2.35", 05, 15, 2008, 130517344, "db48cbcde359982c065c2f5503204b67"),
                    new Update("2.36", 06, 18, 2008, 131285344, "0274382cb3e21aa90e319eba358588c6"),
                    new Update("2.40", 07, 02, 2008, 136710765, "96e9def601672c5abcbb4604bb2346f1"),
                    new Update("2.41", 07, 08, 2008, 136716817, "67c660325b0b97acdeda6c0913dc1f74"),
                    new Update("2.42", 07, 30, 2008, 136716817, "60a6c36e2dd2000b3fda3205428665dc"),
                    new Update("2.43", 09, 17, 2008, 136716817, "da62cbc7eafde9f6792b8a76a4aef004"),
                    new Update("2.50", 10, 15, 2008, 144074159, "823ab63c96d570510b254a80755df446"),
                    new Update("2.51", 10, 23, 2008, 144067418, "252d34d7cdd598ba95ad0fe5662bb60b"),
                    new Update("2.52", 11, 05, 2008, 144057102, "e9cd7c268667ee695a708bf00c6853d8"),
                    new Update("2.53", 12, 02, 2008, 144179982, "5b0e9d62ff6e9edc796db9a2916824f0"),
                    new Update("2.60", 01, 21, 2009, 145394063, "043067d8624040f9f3f1a8dc2e662bce"),
                    new Update("2.70", 04, 02, 2009, 149369197, "3352adc32e39ec5f7b4cdeb8b861052a"),
                    new Update("2.76", 05, 14, 2009, 149341926, "d6856f234f3066b94dc88ab430a63bfb"),
                    new Update("2.80", 06, 24, 2009, 149597830, "d69362743142953be7cb44a5fd5cc888"),
                    new Update("3.00", 09, 01, 2009, 160980204, "c049de7a6a4b03d53e7f9fde04e0fc08"),
                    new Update("3.01", 09, 14, 2009, 160962601, "6d956116eb5094564359339f60650f56"),
                    new Update("3.10", 11, 19, 2009, 167241919, "70296b36d559e35752ae6efd04f702c0"),
                    new Update("3.15", 12, 10, 2009, 171809237, "54ee80e14e479f8351a988eb9a472072"),
                    new Update("3.21", 04, 01, 2010, 172318525, "3a08ef6164a7770ae3e7d5b9f366437a"),
                    new Update("3.30", 04, 22, 2010, 174497808, "6bdf1b2409d705a0136c40746c62e85d"),
                    new Update("3.40", 06, 29, 2010, 175131444, "88b2f8d458119f666c97d893c17201cd"),
                    new Update("3.41", 07, 27, 2010, 175193760, "00c835be718fc3d5f793e130a2b74217"),
                    new Update("3.41", 08, 02, 2010, 175193760, "e07d2b84c9e9691c261b73e5f1aada20"),
                    new Update("3.42", 09, 07, 2010, 175193760, "6ba866514589155ab094099a9f358ffd"),
                    new Update("3.50", 09, 21, 2010, 177696774, "0215e26d1dadeb950471a9c3397a140a"),
                    new Update("3.55", 12, 07, 2010, 178890320, "ca595ad9f3af8f1491d9c9b6921a8c61"),
                    new Update("3.56", 01, 27, 2011, 184579832, "6e070c96e0464e993aaf9deac3660863"),
                    new Update("3.56", 02, 01, 2011, 184579832, "2a52196399a4b96ea568aafa65d1a27e"),
                    new Update("3.60", 03, 10, 2011, 185548749, "91ee193a2fa921a6fce780fc40236e3b"),
                    new Update("3.61", 04, 28, 2011, 185550328, "f446810aabec0af1340c02d852e4118c"),
                    new Update("3.65", 06, 07, 2011, 186820088, "3001e6becbea7abf30fc35a7819c4478"),
                    new Update("3.66", 06, 23, 2011, 186820088, "f4cbe2651e9a0c6115028043bdc2c5dd"),
                    new Update("3.70", 08, 10, 2011, 194500876, "7ee6b91bbd07dde1e65a0681de66745b"),
                    new Update("3.72", 09, 20, 2011, 194529720, "c7d179d273699c2e5d53e401264828f3"),
                    new Update("3.73", 10, 18, 2011, 194529720, "077a6a0a9abf3622373e3daa53f3ec70"),
                    new Update("4.00", 11, 30, 2011, 189077685, "52419374ba45a3d4a2b2dface2594140"),
                    new Update("4.10", 02, 08, 2012, 198682295, "2c3671d071279b4f4b4d07e7166eaf38"),
                    new Update("4.11", 02, 16, 2012, 198732944, "7e6dbb1708ab1df66f0a0f0e2987e8f4"),
                    new Update("4.20", 06, 26, 2012, 201528247, "10c273d5390aa318986e81c6f3746b27"),
                    new Update("4.21", 07, 05, 2012, 201517104, "31f890bd9e75deed773c29e51e56a4b9"),
                    new Update("4.25", 09, 12, 2012, 202059824, "a35b7e74015408e463d15fd66a245929"),
                    new Update("4.30", 10, 24, 2012, 201652852, "7e99b978582026df83ad7224ffa8c8d0"),
                    new Update("4.31", 10, 30, 2012, 201641671, "70067a427e003e892ef81a0fca04eb1d"),
                    new Update("4.40", 03, 21, 2013, 203607815, "1b6a1cb5a909325a7f5ed949e8cc57cb")
                };
        }
    }
}

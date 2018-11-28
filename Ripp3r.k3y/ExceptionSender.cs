using System;
using System.IO;
using System.Net;
using System.Text;

namespace Ripp3r
{
    public class ExceptionSender
    {
        private const string URL = "http://www.3k3y.com/packages/ird_files/libraries/ird/exception.php";

        private readonly string _content;

        public ExceptionSender(string content)
        {
            _content = content;
        }

        public async void Send()
        {
            Encryption enc = new Encryption();
            enc.Encrypt(Encoding.UTF8.GetBytes(_content), string.Empty);

            string authInfo = string.Format("{0}:{1}", enc.PublicKey.Join().Md5().AsString(),
                                            enc.EncryptedKey.AsString());

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(URL);
            wr.Method = "POST";
            wr.Headers["Authentication"] = "K3Y " + Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
            using (Stream s = await wr.GetRequestStreamAsync())
            {
                await s.WriteAsync(enc.EncryptedContent, 0, enc.EncryptedContent.Length);
            }
            await wr.GetResponseAsync();
        }
    }
}

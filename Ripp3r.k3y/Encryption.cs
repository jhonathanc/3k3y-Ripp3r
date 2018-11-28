using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Ripp3r.k3y.Properties;

namespace Ripp3r
{
    internal class Encryption
    {
        public Encryption()
        {
            PublicKey = GetKey();
        }

        public void Encrypt(byte[] content, string hash)
        {
            // Create a random IV and key
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.GenerateIV();
            aes.GenerateKey();

            EncryptKeys(aes.IV, aes.Key, hash);

            ICryptoTransform encrypt = aes.CreateEncryptor();
            EncryptedContent = encrypt.TransformFinalBlock(content, 0, content.Length);
        }

        private void EncryptKeys(byte[] iv, byte[] key, string hash)
        {
            Version version = GetType().Assembly.GetName().Version;

            string queryString = "iv=" + iv.AsString() + "&key=" + key.AsString() + "&id=" + Utilities.InstallationId +
                                 "&h=" + hash + "&v=" + string.Format("{0:00}.{1:00}", version.Major, version.Minor);
            EncryptedKey = EncryptQuerystring(queryString);
        }

        public byte[] EncryptQuerystring(string querystring)
        {
            byte[] payload = Encoding.ASCII.GetBytes(querystring);
            using (StringReader sr = new StringReader(PublicKey))
            {
                PemReader rdr = new PemReader(sr);
                RsaKeyParameters cert = (RsaKeyParameters)rdr.ReadObject();
                RsaEngine e = new RsaEngine();
                e.Init(true, cert);
                return e.ProcessBlock(payload, 0, payload.Length);
            }
        }

        public byte[] EncryptedContent { get; private set; }
        public byte[] EncryptedKey { get; private set; }
        public string PublicKey { get; private set; }

        private static string GetKey()
        {
            if (!string.IsNullOrEmpty(Settings.Default.PublicKey)) return Settings.Default.PublicKey;

            // Read the embedded resource
            using (Stream s = typeof(IrdUploader).Assembly.GetManifestResourceStream("Ripp3r.public.key"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}

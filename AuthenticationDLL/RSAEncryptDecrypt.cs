using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AuthenticationDLL
{
    public class RSAEncryptDecrypt
    {
        /// <summary>
        /// This method generate Public and Private key
        /// </summary>
        /// <param name="keySize">lentgh of key</param>
        public void InitKeyPare(int keySize)
        {

            var csp = new RSACryptoServiceProvider(keySize);
            var privKey = csp.ExportParameters(true);
            var pubKey = csp.ExportParameters(false);
            System.IO.File.WriteAllText(@"C:\PublicKey.txt", ConvertKeyToStr(pubKey));
            System.IO.File.WriteAllText(@"C:\PrivateKey.txt", ConvertKeyToStr(privKey));

        }


        private string ConvertKeyToStr(RSAParameters rsaKey)
        {
            string strKey;
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, rsaKey);
            strKey = sw.ToString();
            return strKey;

        }


        private RSAParameters ConvertStrToKey(string KeyString)
        {

            var sr = new System.IO.StringReader(KeyString);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            var pubKey = (RSAParameters)xs.Deserialize(sr);
            return pubKey;
        }

        public string encryptRSA(string passwordUser, string publicKeyPath)
        {

            string pubKeystr = System.IO.File.ReadAllText(publicKeyPath);
            var publicKey = ConvertStrToKey(pubKeystr);
            string cypherText = string.Empty;
            using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider())
            {
                csp.ImportParameters(publicKey);
                var plainTextData = passwordUser;
                var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);
                var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                cypherText = Convert.ToBase64String(bytesCypherText);
            }

            return cypherText;

        }

        public string DecryptPassword(string cypherText)
        {

            var bytesCypherText = Convert.FromBase64String(cypherText);
            string privateKeystr = System.IO.File.ReadAllText(@"C:\PrivateKey.txt");
            var privKey = ConvertStrToKey(privateKeystr);
            string plainTextData = string.Empty;

            using (RSACryptoServiceProvider csp2 = new RSACryptoServiceProvider())
            {

                csp2.ImportParameters(privKey);

                var bytesPlainTextData = csp2.Decrypt(bytesCypherText, false);

                plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
            }
            //Console.WriteLine("******DecryptedText: {0}", plainTextData);

            return plainTextData;
        }




    }
}

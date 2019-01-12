using System;
using Windows.Storage.Streams;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RPiVendApp
{
    /// <summary>
    /// Структура данных, отправляемых на сервер
    /// </summary>
    public class ClientRequest
    {
        public string Request;
        public string Signature;
        public string AData;
        public string BData;
    }

    /// <summary>
    /// собирает, шифрует и подписывает данные для отправки на сервер
    /// </summary>
    public static class RequestEncoder
    {
        public static ClientRequest EncodeRequestData(object DataObject)
        {
            ClientRequest res = new ClientRequest();
            try
            {
                var xs = new XmlSerializer(DataObject.GetType());
                var xml = new Utf8StringWriter();
                xs.Serialize(xml, DataObject);
                byte[] xmlbytes = Encoding.UTF8.GetBytes(xml.ToString());
                //инициализируем криптодвижок для подписи
                string RSAProvName = AsymmetricAlgorithmNames.RsaPkcs1;
                string AESProvName = SymmetricAlgorithmNames.AesCbcPkcs7;
                SymmetricKeyAlgorithmProvider AESProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(AESProvName);
                IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(32);
                CryptographicKey AESkey = AESProv.CreateSymmetricKey(keyMaterial);
                IBuffer iv = CryptographicBuffer.GenerateRandom(16);
                byte[] encrypteddatabytes = CryptographicEngine.Encrypt(AESkey, xmlbytes.AsBuffer(), iv).ToArray();
                AsymmetricKeyAlgorithmProvider RSAEncryptProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(RSAProvName);
                CryptographicKey ServerPublicKey = RSAEncryptProv.ImportPublicKey(Convert.FromBase64String(GlobalVars.ServerPublicKey).AsBuffer(), CryptographicPublicKeyBlobType.Capi1PublicKey);
                byte[] encryptedkeybytes = CryptographicEngine.Encrypt(ServerPublicKey, keyMaterial, null).ToArray();
                byte[] encryptedivbytes = CryptographicEngine.Encrypt(ServerPublicKey, iv, null).ToArray();
                string strAlgName = HashAlgorithmNames.Sha512;
                HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
                CryptographicHash objHash = objAlgProv.CreateHash();
                objHash.Append(xmlbytes.AsBuffer());
                IBuffer xmlbyteshash = objHash.GetValueAndReset();
                byte[] tmpsignature = CryptographicEngine.SignHashedData(GlobalVars.ClientKeyPair, xmlbyteshash).ToArray();
                res.Request = Convert.ToBase64String(encrypteddatabytes);
                res.Signature = Convert.ToBase64String(tmpsignature);
                res.AData = Convert.ToBase64String(encryptedkeybytes);
                res.BData = Convert.ToBase64String(encryptedivbytes);
            }
            catch /*(Exception ex)*/
            {
                res = null;
            }
            return res;
        }
    }
}

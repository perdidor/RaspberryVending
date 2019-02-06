using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography;

/// <summary>
/// Сводное описание для CryptoHelper
/// </summary>
public class CryptoHelper
{
    public CryptoHelper()
    {
        CryptoKeys tmpkeys;
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                tmpkeys = dc.CryptoKeys.FirstOrDefault();
                PrivateKey = tmpkeys.PrivateKey;
                PublicKey = tmpkeys.PublicKey;
            }
            catch
            {
                
            }
        }
    }

    public static byte[] PublicKey = null;
    private static byte[] PrivateKey = null;


    public byte[] SignByteArray(byte[] InputArray)
    {
        CspParameters cspParams = new CspParameters();
        cspParams.ProviderType = 24;
        RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
        rsaProvider.ImportCspBlob(PrivateKey);
        SHA512 sha512 = new SHA512Managed();
        byte[] hash = sha512.ComputeHash(InputArray);
        return rsaProvider.SignHash(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }

    public bool CheckSignature(byte[] InputArray, byte[] SignatureArray)
    {
        CspParameters cspParams = new CspParameters();
        cspParams.ProviderType = 1;
        RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
        rsaProvider.ImportCspBlob(PublicKey);
        SHA1Managed sha1 = new SHA1Managed();
        byte[] hash = sha1.ComputeHash(InputArray);
        return rsaProvider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), SignatureArray);
    }

    public bool DecryptVerifyDeviceLog(string encryptedrequest, string signature, string adata, string bdata, out WaterDeviceTelemetry outlogentry)
    {
        
        byte[] encryptedrequestbytes = Convert.FromBase64String(encryptedrequest);
        byte[] signaturebytes = Convert.FromBase64String(signature);
        byte[] encryptedaeskeybytes = Convert.FromBase64String(adata);
        byte[] encryptedivbytes = Convert.FromBase64String(bdata);
        bool signcorrect = false;
        try
        {
            //инициализируем криптодвижок для расшифровки
            CspParameters cspParams = new CspParameters
            {
                ProviderType = 1
            };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            rsaProvider.ImportCspBlob(PrivateKey);
            //расшифровываем симметричный ключ и вектор инициализации
            byte[] AESKeyBytes = rsaProvider.Decrypt(encryptedaeskeybytes, false);
            byte[] AESIVBytes = rsaProvider.Decrypt(encryptedivbytes, false);
            AesCryptoServiceProvider AESProv = new AesCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                Key = AESKeyBytes,
                IV = AESIVBytes
            };
            //расшифровываем запрос
            string plaintext = "";
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(encryptedrequestbytes);
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, AESProv.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    plaintext = new StreamReader(cryptoStream, Encoding.UTF8).ReadToEnd();
                }
            }
            finally
            {
                if (memoryStream != null) memoryStream.Dispose();
            }
            WaterDeviceTelemetry tmplog = Deserialize<WaterDeviceTelemetry>(plaintext);
            //инициализируем криптодвижок для проверки подписи присланных данных
            rsaProvider = new RSACryptoServiceProvider();
            using (VendingModelContainer dc = new VendingModelContainer())
            {
                var devicestable = dc.WaterDevices;
                rsaProvider.ImportCspBlob(devicestable.First(x => x.ID == tmplog.WaterDeviceID).PublicKey);
            }
            signcorrect = rsaProvider.VerifyData(Encoding.UTF8.GetBytes(plaintext), CryptoConfig.MapNameToOID("SHA512"), signaturebytes);
            outlogentry = tmplog;
        }
        catch
        {
            outlogentry = null;
        }
        return signcorrect;
    }

    private static T Deserialize<T>(string xml)
    {
        var xs = new XmlSerializer(typeof(T));
        return (T)xs.Deserialize(new StringReader(xml));
    }

    public byte[] DecryptData(byte[] encrypteddata)
    {
        byte[] tmpres = null;
        try
        {
            CspParameters cspParams = new CspParameters
            {
                ProviderType = 1
            };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            rsaProvider.ImportCspBlob(PrivateKey);
            tmpres = rsaProvider.Decrypt(encrypteddata, false);
        }
        catch
        {

        }
        return tmpres;
    }

    public byte[] SignHashedData(byte[] HashedData)
    {
        byte[] tmpres = null;
        try
        {
            CspParameters cspParams = new CspParameters();
            cspParams.ProviderType = 24;
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            tmpres = rsaProvider.SignHash(HashedData, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
        catch
        {

        }
        return tmpres;
    }
}
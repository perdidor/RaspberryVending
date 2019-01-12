using System.Linq;
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

    public byte[] PrivateKey, PublicKey = null;


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
}
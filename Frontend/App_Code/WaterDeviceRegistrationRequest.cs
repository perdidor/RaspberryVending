using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для WaterDeviceRegistrationRequest
/// </summary>
public class WaterDeviceRegistrationRequest
{
    public WaterDeviceRegistrationRequest()
    {
        //
        // TODO: добавьте логику конструктора
        //
    }

    /// <summary>
    /// Строка авторизации, сгенерированная на клиенте
    /// </summary>
    public byte[] AuthorizationString;
    /// <summary>
    /// подпись авторизации
    /// </summary>
    public byte[] AuthSignature;
    /// <summary>
    /// Открытый ключ клиента
    /// </summary>
    public byte[] PublicKey;
}

public class WaterDeviceRegistrationResponse
{
    public WaterDeviceRegistrationResponse()
    {
        //
        // TODO: добавьте логику конструктора
        //
    }
    public string AuthResponse;
    public long RegID;
    public string ServerEndPoint;
    public string Signature;
}
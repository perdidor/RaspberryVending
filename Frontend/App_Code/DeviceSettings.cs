using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для KKTReceiptSettings
/// </summary>
public class DeviceSettings
{
    public DeviceSettings()
    {
        //
        // TODO: добавьте логику конструктора
        //
    }
    public double Latitude = 0;
    public double Longitude = 0;
    /// <summary>
    /// Высота бака для воды (высота установки датчика уровня)
    /// </summary>
    public int TankHeigthcm = 0;

    public string ProductName = "Вода питьевая";
    /// <summary>
    /// Номер телефона для общения с восторженными покупателями. Будем печатать на чеках.
    /// </summary>
    public string CustomerServiceContactPhone = "";
    /// <summary>
    /// Цена за литр (цена единицы товара в МДЕ - минимальная денежная единица в РФ = 1 копейка)
    /// </summary>
    public int PRICE_PER_ITEM_MDE = 0;
    /// <summary>
    /// используемая система налогообложения (см. документацию к ККТ)
    /// </summary>
    public int TaxSystem = 00;
    public long SettingsVersion = 0;
}
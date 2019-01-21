using System.Collections.Generic;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Linq;
using Windows.Storage.Streams;


namespace RPiVendApp
{
    class GlobalVars
    {

        public static long RegID;

        public static CryptographicKey ClientKeyPair;
        public static string ServerPublicKey = "Paste server public key here";

        /// <summary>
        /// Список строк для печати нефискальной информации на чеке
        /// </summary>
        public static List<string> ReceiptLinesToPrint
            {
                get
                {
                return new List<string>
                    {
                    "Сдача округляется до ближайшего",
                    "целого рубля 0-49коп=0руб 50-99коп=1руб",
                    "телефон для справок " + StartPage.CurrentDeviceSettings.CustomerServiceContactPhone
                    };
                }
            }

        /// <summary>
        /// уникальный аппаратный идентификатор устройства
        /// </summary>
        public static readonly string HardWareID = GetHardwareID();

        /// <summary>
        /// Берем MAC сетевой карты, вычисляем MD5 хеш, надеемся,  что полученное значение уникально
        /// </summary>
        /// <returns></returns>
        private static  string GetHardwareID()
        {
            string result = "";
            var networkProfiles = Windows.Networking.Connectivity.NetworkInformation.GetConnectionProfiles().ToList();
            foreach (var net in networkProfiles)
            {
                result += net.NetworkAdapter.NetworkAdapterId.ToString();
            }
            string strAlgName = HashAlgorithmNames.Md5;
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(result, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            string hex = CryptographicBuffer.EncodeToHexString(buffHash);
            StartPage.AddItemToLogBox("Hardware ID: " + hex);
            return hex;
        }

        //GPIO Pins definitions and other global stuff
        public static int TriggerPin = 17;
        public static int EchoPin = 27;
        public static int WaterFlowSensorPin = 22;
        public static int StartLEDPin = 11;
        public static int StopLEDPin = 9;
        public static int EndLEDPin = 10;

        public static int StartButtonPin = 24;
        public static int StopButtonPin = 25;
        public static int EndButtonPin = 8;

        public static int PumpPin = 26;
        public static int HeaterPin = 21;
        public static int FillPumpPin = 20;
        public static int WaterValvePin = 6;
        public static int MDBPin = 5;
        public static int HeartBeatPin = 23;
        public static int ExternalLightPin = 19;
        public static int ReservedPin1 = 16;
        public static int ReservedPin2 = 13;

        /// <summary>
        /// 0x41 is the Attiny85 soft-coded slave I2c address -- see \PLC_Firmware\Attiny_AmbientSensors_I2C\Attiny_AmbientSensors_I2C.ino
        /// </summary>
        public static int I2cSlaveAddress = 65;

        public static int AmbientTempLowerTreshold = 4;
        public static int AmbientTempHigherTreshold = 8;

        /// <summary>
        /// we will disable accepting cash when reached, to avoid Coin Changer deplete
        /// </summary>
        public static int MaxUserDepositTreshold = 100;


        public static string SERVER_ENDPOINT;

        public static string SYSTEM_STATE_LOG_PATH = "/Backend/WaterDeviceStateLog.aspx";
        public static string RECEIVE_COMMANDS_PATH = "/Backend/GetWaterDeviceCommand.aspx";
        public static string INCASSO_REPORT_PATH = "/Backend/WaterDeviceIncassoReport.aspx";
        public static string SALES_LOG_PATH = "/Backend/WaterDeviceSalesLog.aspx";
        public static string REGISTRATION_PATH = "/Backend/NewWaterDevice.aspx";
        public static string GET_DEVICE_SETTINGS_PATH = "/Backend/GetDeviceSettings.aspx";
    }
}

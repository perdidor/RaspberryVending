using System;
using Windows.Storage.Streams;
using Windows.Devices.Gpio;
using System.Collections.Generic;
using System.IO;
using Windows.UI;
using Windows.UI.Xaml;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading;
using Windows.Web.Http;
using Windows.Storage;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace RPiVendApp
{
    /// <summary>
    /// Информация о сеансе продажи для формирования кассового чека
    /// </summary>
    public class WaterSales
    {
        public WaterSales()
        {
            DateTime cdt = DateTime.Now;
            StartDateTime = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
            StartDateTimeStr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
            UserCash = 0;
            Quantity = 0.00;
            TaxSystemInUse = 0;
            PRICE_PER_ITEM_MDE = StartPage.CurrentDeviceSettings.PRICE_PER_ITEM_MDE;
            ProductName = StartPage.CurrentDeviceSettings.ProductName;
            ActualChangeDispensed = 0;
        }
        /// <summary>
        /// Заканчиваем сеанс продажи, пишем логи
        /// </summary>
        public void CompleteSession()
        {
            Quantity = Math.Round(Quantity, 2);
            if (Quantity > 0)
            {
                DateTime cdt = DateTime.Now;
                EndDateTime = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
                EndDateTimeString = cdt.ToString("dd.MM.yyyy HH:mm:ss");
                List<WaterSales> WaterSalesList = new List<WaterSales> { };
                string watersalesfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".006";
                try
                {
                    string tmpxmlstr = File.ReadAllText(watersalesfilename);
                    WaterSalesList = StartPage.Deserialize<List<WaterSales>>(tmpxmlstr);
                }
                catch
                {

                }
                WaterSalesList.Add(StartPage.CurrentSaleSession);
                try
                {
                    var xs = new XmlSerializer(WaterSalesList.GetType());
                    var xml = new Utf8StringWriter();
                    xs.Serialize(xml, WaterSalesList);
                    File.WriteAllText(watersalesfilename, xml.ToString());
                }
                catch
                {

                }
            }
        }
        public long ID = 0;
        public long WaterDeviceID = GlobalVars.RegID;
        public long StartDateTime;
        public string StartDateTimeStr;
        public double UserCash;
        public double Quantity;
        public int TaxSystemInUse;
        public int PRICE_PER_ITEM_MDE;
        public string ProductName;
        public int ActualChangeDispensed;
        public double ChangeActualDiff;
        public long EndDateTime;
        public string EndDateTimeString;
        public long StageNumber;
        public long ReceiptNumber;
    }
}

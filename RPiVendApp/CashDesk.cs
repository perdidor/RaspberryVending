using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace RPiVendApp
{

    /// <summary>
    /// Режимы ККТ
    /// </summary>
    public enum DeviceStateMode
    {
        /// <summary>
        /// Выбор
        /// </summary>
        Selection = 0x00,
        /// <summary>
        /// Регистрация: Ожидание команды
        /// </summary>
        Registration_WaitingForCommand = 0x01,
        /// <summary>
        /// Регистрация: Ввод пароля
        /// </summary>
        Registration_EnteringPassword = 0x11,
        /// <summary>
        /// Регистрация: Ожидание ввода секции
        /// </summary>
        Registration_WaitingForSection = 0x21,
        /// <summary>
        /// Регистрация: Ожидание сторно по штрихкоду
        /// </summary>
        Registration_WaitingForSTORNObyBarCode = 0x31,
        /// <summary>
        /// Регистрация: Прием платежей
        /// </summary>
        Registration_PaymentInProgress = 0x41,
        /// <summary>
        /// Регистрация: Ожидание печати отложенного документа
        /// </summary>
        Registration_WaitForDelayedDocPrint = 0x51,
        /// <summary>
        /// Регистрация: Печать отложенного документа
        /// </summary>
        Registration_DelayedDocumentPrintInProgress = 0x61,
        /// <summary>
        /// Регистрация: Формирование документа
        /// </summary>
        Registration_DocumentFormInProgress = 0x71,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Ожидание команды
        /// </summary>
        CounterStateReports_WaitingForCommand = 0x02,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Ввод пароля
        /// </summary>
        CounterStateReports_EnteringPassword = 0x12,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Формирование отчета о состоянии счетчиков ККТ
        /// </summary>
        CounterStateReports_CountersStateReportInProgress = 0x22,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Формирование служебного отчета
        /// </summary>
        CounterStateReports_ServiceReportInProgress = 0x32,
        /// <summary>
        /// Отчет о закрытии смены: Ожидание команды
        /// </summary>
        StageReports_WaitingForCommand = 0x03,
        /// <summary>
        /// Отчет о закрытии смены: Ввод пароля
        /// </summary>
        StageReports_EnteringPassword = 0x13,
        /// <summary>
        /// Отчет о закрытии смены: Формирование отчета о закрытии смены
        /// </summary>
        StageReports_StageCloseReportInProgress = 0x23,
        /// <summary>
        /// Отчет о закрытии смены: Подтверждение гашения счетчиков
        /// </summary>
        StageReports_CountersClearConfirmation = 0x33,
        /// <summary>
        /// Отчет о закрытии смены: Ввод даты с клавиатуры
        /// </summary>
        StageReports_ManualDateEntryInProgress = 0x43,
        /// <summary>
        /// Отчет о закрытии смены: Ожидание подтверждения общего гашения счетчиков
        /// </summary>
        StageReports_WaitingForCountersClearConfirmation = 0x53,
        /// <summary>
        /// Отчет о закрытии смены: Идет общее гашение
        /// </summary>
        StageReports_TotalClearingInProgress = 0x63,
        /// <summary>
        /// Программирование: Ожидание команды
        /// </summary>
        Program_WaitingForCommand = 0x04,
        /// <summary>
        /// Программирование: Ввод пароля
        /// </summary>
        Program_EnteringPassword = 0x14,
        /// <summary>
        /// Ввод ЗН: Ожидание команды
        /// </summary>
        EnterMfgNumber_WaitingForCommand = 0x05,
        /// <summary>
        /// Ввод ЗН: Ввод пароля
        /// </summary>
        EnterMfgNumber_EnteringPassword = 0x15,
        /// <summary>
        /// Ввод ЗН: Описание отсутствует
        /// </summary>
        EnterMfgNumber_EMPTY = 0x25,
        /// <summary>
        /// Ввод ЗН: Ввод данных
        /// </summary>
        EnterMfgNumber_EnteringDataInProgress = 0x35,
        /// <summary>
        /// Ввод ЗН: Подтверждение ввода
        /// </summary>
        EnterMfgNumber_ConfirmEnteredData = 0x45,
        /// <summary>
        /// Доступ к ФН: Ожидание команды
        /// </summary>
        FNAccess_WaitingForCommand = 0x06,
        /// <summary>
        /// Доступ к ФН: Формирование отчета
        /// </summary>
        FNAccess_ReportInProgress = 0x26,
        /// <summary>
        /// Дополнительный: Идет обнуление таблиц и гашение операционных регистров
        /// </summary>
        Additional_RegistersClearingInProgress = 0x07,
        /// <summary>
        /// Дополнительный: Выполняется тестовый прогон
        /// </summary>
        Additional_TestPassInProgress = 0x27,
        /// <summary>
        /// Дополнительный: Режим ввода времени с клавиатуры
        /// </summary>
        Additional_ManualTimeEntryInProgress = 0x37,
        /// <summary>
        /// Дополнительный: Режим тестов (для технологической ККТ)
        /// </summary>
        Additional_TestMode = 0x47,
        /// <summary>
        /// Дополнительный: Ввод даты после сбоя часов
        /// </summary>
        Additional_EnteringDateAfterClockFailed = 0x57,
        /// <summary>
        /// Дополнительный: Ввод времени после сбоя часов
        /// </summary>
        Additional_EnteringTimeAfterClockFailed = 0x67,
        /// <summary>
        /// Дополнительный: Начальная инициализация ККТ
        /// </summary>
        Additional_Initialization = 0x77,
        /// <summary>
        /// Дополнительный: Ожидание подтверждения обнуления таблиц
        /// </summary>
        Additional_WaitingForTablesClear = 0x87,
        /// <summary>
        /// Дополнительный: Разные накопители памяти
        /// </summary>
        Additional_DifferentStorageMedia = 0x97,
        /// <summary>
        /// Дополнительный: ККТ не инициализирована
        /// </summary>
        Additional_DeviceNotInitialized = 0xA7,
        /// <summary>
        /// Дополнительный: ККТ заблокирована при вводе даты, меньшей даты последней записи ФН
        /// </summary>
        Additional_DeviceLockedDueDateTampering = 0xB7,
        /// <summary>
        /// Дополнительный: Подтверждение ввода даты
        /// </summary>
        Additional_ConfirmEnteredData = 0xC7,
        /// <summary>
        /// Дополнительный: Оповещение о переводе часов на летнее/зимнее время
        /// </summary>
        Additional_DayLightSavingNotification = 713,
        /// <summary>
        /// Дополнительный: Блокировка при ошибке ФН
        /// </summary>
        Additional_DeviceLockedDueFNError = 0xD7,
    }

    public class StageProperties
    {
        /// <summary>
        /// состояние смены
        /// </summary>
        public enum StageState
        {
            /// <summary>
            /// Смена открыта
            /// </summary>
            Closed,
            /// <summary>
            /// Cмена закрыта
            /// </summary>
            Opened,
            /// <summary>
            /// Длительность смены превысила 24 часа
            /// </summary>
            Over24h,
            undefined
        }

        public StageProperties(byte[] StagePropertiesData)
        {
            if (StagePropertiesData.Length == 10)
            {
                CurrentStageState = (StageState)StagePropertiesData[3];
                string tmpstagecloseddt = BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[4] }).ToString().PadLeft(2,'0') + 
                    BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[5] }).ToString().PadLeft(2, '0') + 
                    BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[6] }).ToString().PadLeft(2, '0') + 
                    BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[7] }).ToString().PadLeft(2, '0') + 
                    BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[8] }).ToString().PadLeft(2, '0') + 
                    BCDHelper.BCDByteToInt(new byte[1] { StagePropertiesData[9] }).ToString().PadLeft(2, '0');
                DateTime.TryParseExact(tmpstagecloseddt, "ddMMyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime tmpdt);
                StageClosedDateTime = tmpdt;
            }
        }

        /// <summary>
        /// состояние текущей смены
        /// </summary>
        public readonly StageState CurrentStageState = StageState.undefined;
        /// <summary>
        /// Дата и время последнего закрытия смены
        /// </summary>
        public readonly DateTime StageClosedDateTime = DateTime.MinValue;
    }

    public class CurrentStageParameters
    {
        public CurrentStageParameters(byte[] CurrentStageParametersData)
        {
            if (CurrentStageParametersData[2] == 0)
            {
                StageOpened = (CurrentStageParametersData[3] == 1);
                StageNumber = CurrentStageParametersData[5] * 255 + CurrentStageParametersData[4];
                LastReceiptNumber = CurrentStageParametersData[7] * 255 + CurrentStageParametersData[6];
            }
        }
        public readonly bool StageOpened = false;
        public readonly int StageNumber = -1;
        public readonly int LastReceiptNumber = -1;
    }

    /// <summary>
    /// вспомогательный класс для перевода чисел в формат BCD
    /// </summary>
    static class BCDHelper
    {
        /// <summary>
        /// Преобразует целое число в масив байт BCD
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] IntToBcd(string IntValue)
        {
            byte[] ret = new byte[IntValue.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)(Convert.ToInt32(IntValue.Substring(i * 2,1)) * 10);
                ret[i] |= (byte)((Convert.ToInt32(IntValue.Substring(i * 2 + 1, 1))) << 4);
            }
            return ret;
        }

        public static byte[] IntToBcd2(bool isLittleEndian, string bcdString)
        {
            bool isValid = true;
            isValid = isValid && !string.IsNullOrEmpty(bcdString);
            // Check that the string is made up of sets of two numbers (e.g. "01" or "3456")
            isValid = isValid && Regex.IsMatch(bcdString, "^([0-9]{2})+$");
            byte[] bytes;
            if (isValid)
            {
                char[] chars = bcdString.ToCharArray();
                int len = chars.Length / 2;
                bytes = new byte[len];
                if (isLittleEndian)
                {
                    for (int i = 0; i < len; i++)
                    {
                        byte highNibble = byte.Parse(chars[2 * (len - 1) - 2 * i].ToString());
                        byte lowNibble = byte.Parse(chars[2 * (len - 1) - 2 * i + 1].ToString());
                        bytes[i] = (byte)((byte)(highNibble << 4) | lowNibble);
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        byte highNibble = byte.Parse(chars[2 * i].ToString());
                        byte lowNibble = byte.Parse(chars[2 * i + 1].ToString());
                        bytes[i] = (byte)((byte)(highNibble << 4) | lowNibble);
                    }
                }
            }
            else
            {
                throw new ArgumentException(string.Format(
                    "Input string ({0}) was invalid.", bcdString));
            }
            return bytes;
        }

        /// <summary>
        /// Преобразует массив байт BCD в целое число
        /// </summary>
        /// <param name="BCDBytes"></param>
        /// <returns></returns>
        public static ulong BCDByteToInt(byte[] BCDBytes)
        {
            ulong res = 0;
            for (int i = 0; i < BCDBytes.Length; i++)
            {
                res *= 100;
                res += (ulong)(10 * (BCDBytes[i] >> 4));
                res += (ulong)(BCDBytes[i] & 0xf);
            }
            return res;
        }
    }

    /// <summary>
    /// вспомогательный класс для вычисления контрольной суммы
    /// </summary>
    public static class CRC8Helper
    {
        public static byte CRC8(byte[] data)
        {
            int crc = 0xFF;
            for (int i = 0; i < data.Length; i++)
            {
                int extract = data[i];
                int k = extract & 0xFF;
                crc ^= k;
                for (int a = 8; a > 0; a--)
                {
                    int j = crc & 0x80;
                    crc <<= 1;
                    if (j != 0)
                    {
                        crc ^= 0x31;
                    }
                }
            }
            return (byte)crc;
        }

    }

    /// <summary>
    /// Структура, описывающая состояние ККТ
    /// </summary>
    public class CashDeskKKTState
    {
        public CashDeskKKTState(byte[] KKTStateBytes)
        {
            KassirID = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[2] });
            KKTNumber = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[3] });
            KKTYear = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[4] }) + 2000;
            KKTMonth = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[5] });
            KKTDay = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[6] });
            KKTHour = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[7] });
            KKTMinute = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[8] });
            KKTSeconds = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[9] });
            KKTFiscalized = (KKTStateBytes[10] & (1 << 0)) != 0;
            StageOpened = (KKTStateBytes[10] & (1 << 1)) != 0;
            PresenterPaperPresent = (KKTStateBytes[10] & (1 << 2)) != 0;
            ReceiptPaperPresent = (KKTStateBytes[10] & (1 << 3)) != 0;
            NotUsedProperty = (KKTStateBytes[10] & (1 << 4)) != 0;
            CapOpened = (KKTStateBytes[10] & (1 << 5)) != 0;
            FNActivated = (KKTStateBytes[10] & (1 << 6)) != 0;
            KKTBatteryPresent = (KKTStateBytes[10] & (1 << 7)) != 0;
            KKTMfgNumber = BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[11], KKTStateBytes[12], KKTStateBytes[13], KKTStateBytes[14] }).ToString().PadLeft(8, '0');
            DeviceType = ((CashDesk.DeviceTypes)KKTStateBytes[15]).ToString();
            ReservedPropertyByte0 = KKTStateBytes[16];
            ReservedPropertyByte1 = KKTStateBytes[17];
            CurrentMode = (DeviceStateMode)KKTStateBytes[18];
            NextReceiptNumber = ((int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[19], KKTStateBytes[20] })).ToString().PadLeft(4, '0');
            StageNumber = ((int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[21], KKTStateBytes[22] })).ToString().PadLeft(4, '0');
            ReceiptState = KKTStateBytes[23];
            ReceiptSum = (int)BCDHelper.BCDByteToInt(new byte[] { KKTStateBytes[24], KKTStateBytes[25], KKTStateBytes[26], KKTStateBytes[27], KKTStateBytes[28] });
            DecimalDigits = KKTStateBytes[29];
            CommPortType = KKTStateBytes[30];
        }
        /// <summary>
        /// Резервное поле байт 0
        /// </summary>
        public readonly int ReservedPropertyByte0;
        /// <summary>
        /// Резервное поле байт 1
        /// </summary>
        public readonly int ReservedPropertyByte1;
        /// <summary>
        /// Номер кассира
        /// </summary>
        public readonly int KassirID;
        /// <summary>
        /// Номер ККТ в зале
        /// </summary>
        public readonly int KKTNumber;
        /// <summary>
        /// Текущий год по показаниям внутренних часов
        /// </summary>
        public readonly int KKTYear;
        /// <summary>
        /// Текущий месяц по показаниям внутренних часов
        /// </summary>
        public readonly int KKTMonth;
        /// <summary>
        /// Текущий день месяца по показаниям внутренних часов
        /// </summary>
        public readonly int KKTDay;
        /// <summary>
        /// Текущий час по показаниям внутренних часов
        /// </summary>
        public readonly int KKTHour;
        /// <summary>
        /// Текущие минуты по показаниям внутренних часов
        /// </summary>
        public readonly int KKTMinute;
        /// <summary>
        /// Текущие секунды по показаниям внутренних часов
        /// </summary>
        public readonly int KKTSeconds;
        /// <summary>
        /// ККТ фискализирована
        /// </summary>
        public readonly bool KKTFiscalized;
        /// <summary>
        /// Смена открыта
        /// </summary>
        public readonly bool StageOpened;
        /// <summary>
        /// Наличие бумаги в презенторе
        /// </summary>
        public readonly bool PresenterPaperPresent;
        /// <summary>
        /// Датчик чековой ленты
        /// </summary>
        public readonly bool ReceiptPaperPresent;
        /// <summary>
        /// бит не используется;
        /// </summary>
        public readonly bool NotUsedProperty;
        /// <summary>
        /// состояние датчика крышки (0 – крышка закрыта, 1 – крышка открыта);
        /// </summary>
        public readonly bool CapOpened;
        /// <summary>
        /// состояние ФН: 0 – не активизирован, 1 – активизирован);
        /// </summary>
        public readonly bool FNActivated;
        /// <summary>
        /// наличие батарейки в ККТ: 0 – установлена, 1 – отсутствует
        /// </summary>
        public readonly bool KKTBatteryPresent;
        /// <summary>
        /// Заводской номер
        /// </summary>
        public readonly string KKTMfgNumber;
        /// <summary>
        /// Модель
        /// </summary>
        public readonly string DeviceType;
        /// <summary>
        /// Поле зарезервировано. В ответ на команду 3Fh всегда возвращает значение 3.0.
        /// </summary>
        public readonly string ReservedProperty = "3.0";
        /// <summary>
        /// Режим работы
        /// </summary>
        public readonly DeviceStateMode CurrentMode;
        /// <summary>
        /// Номер следующего чека
        /// </summary>
        public readonly string NextReceiptNumber;
        /// <summary>
        /// Номер смены – номер последней закрытой смены, а не текущей.
        /// Всегда до фискализации ККТ и до снятия первого суточного отчета с гашением после фискализации ККТ номер последней закрытой смены равен 0000.
        /// </summary>
        public readonly string StageNumber;
        /// <summary>
        /// Имеет смысл только в режиме регистрации.
        /// Битовый формат.Назначение бит:
        /// биты 0 .. 8 – тип чека: 0 – чек закрыт, 1 – чек прихода, 2 – чек возврата прихода, 4 – чек расхода, 
        /// 5 – чек возврата расхода, 7 – чек коррекции: приход, , 9 – чек коррекции: расход.
        /// </summary>
        public readonly int ReceiptState;
        /// <summary>
        /// Сумма текущего чека 000000000 .. 4294967295 мде (в копейках короче говоря).
        /// </summary>
        public readonly int ReceiptSum;
        /// <summary>
        /// 0 .. 3 – положение десятичной точки во всех денежных величинах (кол-во разрядов справа от десятичной точки)
        /// </summary>
        public readonly int DecimalDigits;
        /// <summary>
        /// Данный параметр обозначает тип интерфейса, по которому работает ККТ, и принимает значения:
        /// 1 – RS-232,
        /// 4 – USB,
        /// 5 – Bluetooth,
        /// 6 – Ethernet,
        /// 7 – WiFi
        /// </summary>
        public readonly int CommPortType;
    }


    /// <summary>
    /// Структура, описывающая состояние принтера фискального регистратора
    /// </summary>
    public class KKTPrinterState
    {
        public KKTPrinterState(byte[] DeviceStateBytes)
        {
            int Mode = DeviceStateBytes[2] & 15;
            int SubMode = DeviceStateBytes[2] >> 4;
            CurrentMode = (Mode) + (SubMode * 10);
            PrinterPaperEmpty = (DeviceStateBytes[3] & (1 << 0)) != 0;
            PrinterConnected = (DeviceStateBytes[3] & (1 << 1)) == 0;
            PrinterNonRecoverableError = (DeviceStateBytes[3] & (1 << 2)) != 0;
            PrinterCutterError = (DeviceStateBytes[3] & (1 << 3)) != 0;
            PrinterOverHeated = (DeviceStateBytes[3] & (1 << 4)) != 0;
            PrinterPaperJammed = (DeviceStateBytes[3] & (1 << 5)) != 0;
            PrinterPresenterError = (DeviceStateBytes[3] & (1 << 6)) != 0;
            PrinterPaperEnding = (DeviceStateBytes[3] & (1 << 7)) != 0;
        }
        /// <summary>
        /// Текущий режим
        /// </summary>
        public readonly int CurrentMode;
        /// <summary>
        /// Флаг отсутствия бумаги
        /// </summary>
        public readonly bool PrinterPaperEmpty;
        /// <summary>
        /// Флаг наличия связи с принтером
        /// </summary>
        public readonly bool PrinterConnected;
        /// <summary>
        /// Флаг наличия механической (неисправимой) ошибки
        /// </summary>
        public readonly bool PrinterNonRecoverableError;
        /// <summary>
        /// Флаг наличия ошибки отрезчика чеков
        /// </summary>
        public readonly bool PrinterCutterError;
        /// <summary>
        /// Флаг перегрева головки принтера
        /// </summary>
        public readonly bool PrinterOverHeated;
        /// <summary>
        /// Флаг наличия замятия бумаги
        /// </summary>
        public readonly bool PrinterPaperJammed;
        /// <summary>
        /// не используется
        /// </summary>
        public readonly bool PrinterPresenterError;
        /// <summary>
        /// Флаг срабатывания датчика "кончается бумага"
        /// </summary>
        public readonly bool PrinterPaperEnding;
    }

    /// <summary>
    /// Описание кассового аппарата
    /// </summary>
    static class CashDesk
    {

        private static DataReader CashDeskSerialDataReaderObject = null;

        public static bool Busy = false;

        /// <summary>
        /// Делегат типа оборудования
        /// </summary>
        /// <param name="KKTDeviceTypeString">Строка описания типа оборудования</param>
        public delegate void CashDeskAbortedDelegate();
        public delegate void CashDeskStringDelegate(string KKTAnswerString);
        public delegate void CashDeskBytesDelegate(byte[] KKTAnswerBytes);
        public delegate void KKTStateDelegate(CashDeskKKTState KKTState);
        public delegate void KKTPrinterStateDelegate(KKTPrinterState PrinterState);
        public delegate void CashDeskStagePropsDelegate(StageProperties StageProps);
        public delegate void CashDeskCurrentStageParametersDelegate(CurrentStageParameters StageParameters);
        public static event CashDeskAbortedDelegate AllTasksCancelled;
        public static event CashDeskCurrentStageParametersDelegate CashDeskCurrentStageParametersReceived;
        public static event CashDeskStringDelegate DeviceTypeReceived;
        public static event KKTPrinterStateDelegate KKTPrinterStateReceived;
        public static event KKTStateDelegate KKTStateReceived;
        public static event CashDeskBytesDelegate SerialPortBytesWritten;
        public static event CashDeskStagePropsDelegate StageStateReceived;
        public static event CashDeskStringDelegate CurrentModeChanged;
        public static event CashDeskStringDelegate CloseStageResult;
        public static event CashDeskStringDelegate OpenStageResult;
        public static event CashDeskStringDelegate OpenReceiptResult;
        public static event CashDeskStringDelegate CancelReceiptResult;
        public static event CashDeskStringDelegate StartReceiptEntryResult;
        public static event CashDeskStringDelegate AddEntryDataResult;
        public static event CashDeskStringDelegate ReceiptPaymentResult;
        public static event CashDeskStringDelegate CloseReceiptResult;

        /// <summary>
        /// Кодирует строку в массив байт для посылки в ККТ
        /// </summary>
        /// <param name="KKTString"></param>
        /// <returns></returns>
        private static byte[] EncodeStringToKKTBytes(string KKTString)
        {
            List<byte> res = new List<byte> { };
            foreach (char item in KKTString)
            {
                switch (item)
                {
                    case '!':
                        {
                            res.Add(0x21);
                            break;
                        }
                    case '\"':
                        {
                            res.Add(0x22);
                            break;
                        }
                    case '#':
                        {
                            res.Add(0x23);
                            break;
                        }
                    case '№':
                        {
                            res.Add(0x24);
                            break;
                        }
                    case '%':
                        {
                            res.Add(0x25);
                            break;
                        }
                    case '&':
                        {
                            res.Add(0x26);
                            break;
                        }
                    case '\'':
                        {
                            res.Add(0x27);
                            break;
                        }
                    case '(':
                        {
                            res.Add(0x28);
                            break;
                        }
                    case ')':
                        {
                            res.Add(0x29);
                            break;
                        }
                    case '*':
                        {
                            res.Add(0x2A);
                            break;
                        }
                    case '+':
                        {
                            res.Add(0x2B);
                            break;
                        }
                    case ',':
                        {
                            res.Add(0x2C);
                            break;
                        }
                    case '-':
                        {
                            res.Add(0x2D);
                            break;
                        }
                    case '.':
                        {
                            res.Add(0x2E);
                            break;
                        }
                    case '/':
                        {
                            res.Add(0x2F);
                            break;
                        }
                    case '0':
                        {
                            res.Add(0x30);
                            break;
                        }
                    case '1':
                        {
                            res.Add(0x31);
                            break;
                        }
                    case '2':
                        {
                            res.Add(0x32);
                            break;
                        }
                    case '3':
                        {
                            res.Add(0x33);
                            break;
                        }
                    case '4':
                        {
                            res.Add(0x34);
                            break;
                        }
                    case '5':
                        {
                            res.Add(0x35);
                            break;
                        }
                    case '6':
                        {
                            res.Add(0x36);
                            break;
                        }
                    case '7':
                        {
                            res.Add(0x37);
                            break;
                        }
                    case '8':
                        {
                            res.Add(0x38);
                            break;
                        }
                    case '9':
                        {
                            res.Add(0x39);
                            break;
                        }
                    case ':':
                        {
                            res.Add(0x3A);
                            break;
                        }
                    case ';':
                        {
                            res.Add(0x3B);
                            break;
                        }
                    case '<':
                        {
                            res.Add(0x3C);
                            break;
                        }
                    case '=':
                        {
                            res.Add(0x3D);
                            break;
                        }
                    case '>':
                        {
                            res.Add(0x3E);
                            break;
                        }
                    case '?':
                        {
                            res.Add(0x3F);
                            break;
                        }
                    case '@':
                        {
                            res.Add(0x40);
                            break;
                        }
                    case 'A':
                        {
                            res.Add(0x41);
                            break;
                        }
                    case 'B':
                        {
                            res.Add(0x42);
                            break;
                        }
                    case 'C':
                        {
                            res.Add(0x43);
                            break;
                        }
                    case 'D':
                        {
                            res.Add(0x44);
                            break;
                        }
                    case 'E':
                        {
                            res.Add(0x45);
                            break;
                        }
                    case 'F':
                        {
                            res.Add(0x46);
                            break;
                        }
                    case 'G':
                        {
                            res.Add(0x47);
                            break;
                        }
                    case 'H':
                        {
                            res.Add(0x48);
                            break;
                        }
                    case 'I':
                        {
                            res.Add(0x49);
                            break;
                        }
                    case 'J':
                        {
                            res.Add(0x4A);
                            break;
                        }
                    case 'K':
                        {
                            res.Add(0x4B);
                            break;
                        }
                    case 'L':
                        {
                            res.Add(0x4C);
                            break;
                        }
                    case 'M':
                        {
                            res.Add(0x4D);
                            break;
                        }
                    case 'N':
                        {
                            res.Add(0x4E);
                            break;
                        }
                    case 'O':
                        {
                            res.Add(0x4F);
                            break;
                        }
                    case 'P':
                        {
                            res.Add(0x50);
                            break;
                        }
                    case 'Q':
                        {
                            res.Add(0x51);
                            break;
                        }
                    case 'R':
                        {
                            res.Add(0x52);
                            break;
                        }
                    case 'S':
                        {
                            res.Add(0x53);
                            break;
                        }
                    case 'T':
                        {
                            res.Add(0x54);
                            break;
                        }
                    case 'U':
                        {
                            res.Add(0x55);
                            break;
                        }
                    case 'V':
                        {
                            res.Add(0x56);
                            break;
                        }
                    case 'W':
                        {
                            res.Add(0x57);
                            break;
                        }
                    case 'X':
                        {
                            res.Add(0x58);
                            break;
                        }
                    case 'Y':
                        {
                            res.Add(0x59);
                            break;
                        }
                    case 'Z':
                        {
                            res.Add(0x5A);
                            break;
                        }
                    case '[':
                        {
                            res.Add(0x5B);
                            break;
                        }
                    case '\\':
                        {
                            res.Add(0x5C);
                            break;
                        }
                    case ']':
                        {
                            res.Add(0x5D);
                            break;
                        }
                    case '^':
                        {
                            res.Add(0x5E);
                            break;
                        }
                    case '_':
                        {
                            res.Add(0x5F);
                            break;
                        }
                    case '`':
                        {
                            res.Add(0x60);
                            break;
                        }
                    case 'a':
                        {
                            res.Add(0x61);
                            break;
                        }
                    case 'b':
                        {
                            res.Add(0x62);
                            break;
                        }
                    case 'c':
                        {
                            res.Add(0x63);
                            break;
                        }
                    case 'd':
                        {
                            res.Add(0x64);
                            break;
                        }
                    case 'e':
                        {
                            res.Add(0x65);
                            break;
                        }
                    case 'f':
                        {
                            res.Add(0x66);
                            break;
                        }
                    case 'g':
                        {
                            res.Add(0x67);
                            break;
                        }
                    case 'h':
                        {
                            res.Add(0x68);
                            break;
                        }
                    case 'i':
                        {
                            res.Add(0x69);
                            break;
                        }
                    case 'j':
                        {
                            res.Add(0x6A);
                            break;
                        }
                    case 'k':
                        {
                            res.Add(0x6B);
                            break;
                        }
                    case 'l':
                        {
                            res.Add(0x6C);
                            break;
                        }
                    case 'm':
                        {
                            res.Add(0x6D);
                            break;
                        }
                    case 'n':
                        {
                            res.Add(0x6E);
                            break;
                        }
                    case 'o':
                        {
                            res.Add(0x6F);
                            break;
                        }
                    case 'p':
                        {
                            res.Add(0x70);
                            break;
                        }
                    case 'q':
                        {
                            res.Add(0x71);
                            break;
                        }
                    case 'r':
                        {
                            res.Add(0x72);
                            break;
                        }
                    case 's':
                        {
                            res.Add(0x73);
                            break;
                        }
                    case 't':
                        {
                            res.Add(0x74);
                            break;
                        }
                    case 'u':
                        {
                            res.Add(0x75);
                            break;
                        }
                    case 'v':
                        {
                            res.Add(0x76);
                            break;
                        }
                    case 'w':
                        {
                            res.Add(0x77);
                            break;
                        }
                    case 'x':
                        {
                            res.Add(0x78);
                            break;
                        }
                    case 'y':
                        {
                            res.Add(0x79);
                            break;
                        }
                    case 'z':
                        {
                            res.Add(0x7A);
                            break;
                        }
                    case '{':
                        {
                            res.Add(0x7B);
                            break;
                        }
                    case '|':
                        {
                            res.Add(0x7C);
                            break;
                        }
                    case '}':
                        {
                            res.Add(0x7D);
                            break;
                        }
                    case '~':
                        {
                            res.Add(0x7E);
                            break;
                        }
                    case 'А':
                        {
                            res.Add(0x80);
                            break;
                        }
                    case 'Б':
                        {
                            res.Add(0x81);
                            break;
                        }
                    case 'В':
                        {
                            res.Add(0x82);
                            break;
                        }
                    case 'Г':
                        {
                            res.Add(0x83);
                            break;
                        }
                    case 'Д':
                        {
                            res.Add(0x84);
                            break;
                        }
                    case 'Е':
                        {
                            res.Add(0x85);
                            break;
                        }
                    case 'Ж':
                        {
                            res.Add(0x86);
                            break;
                        }
                    case 'З':
                        {
                            res.Add(0x87);
                            break;
                        }
                    case 'И':
                        {
                            res.Add(0x88);
                            break;
                        }
                    case 'Й':
                        {
                            res.Add(0x89);
                            break;
                        }
                    case 'К':
                        {
                            res.Add(0x8A);
                            break;
                        }
                    case 'Л':
                        {
                            res.Add(0x8B);
                            break;
                        }
                    case 'М':
                        {
                            res.Add(0x8C);
                            break;
                        }
                    case 'Н':
                        {
                            res.Add(0x8D);
                            break;
                        }
                    case 'О':
                        {
                            res.Add(0x8E);
                            break;
                        }
                    case 'П':
                        {
                            res.Add(0x8F);
                            break;
                        }
                    case 'Р':
                        {
                            res.Add(0x90);
                            break;
                        }
                    case 'С':
                        {
                            res.Add(0x91);
                            break;
                        }
                    case 'Т':
                        {
                            res.Add(0x92);
                            break;
                        }
                    case 'У':
                        {
                            res.Add(0x93);
                            break;
                        }
                    case 'Ф':
                        {
                            res.Add(0x94);
                            break;
                        }
                    case 'Х':
                        {
                            res.Add(0x95);
                            break;
                        }
                    case 'Ц':
                        {
                            res.Add(0x96);
                            break;
                        }
                    case 'Ч':
                        {
                            res.Add(0x97);
                            break;
                        }
                    case 'Ш':
                        {
                            res.Add(0x98);
                            break;
                        }
                    case 'Щ':
                        {
                            res.Add(0x99);
                            break;
                        }
                    case 'Ъ':
                        {
                            res.Add(0x9A);
                            break;
                        }
                    case 'Ы':
                        {
                            res.Add(0x9B);
                            break;
                        }
                    case 'Ь':
                        {
                            res.Add(0x9C);
                            break;
                        }
                    case 'Э':
                        {
                            res.Add(0x9D);
                            break;
                        }
                    case 'Ю':
                        {
                            res.Add(0x9E);
                            break;
                        }
                    case 'Я':
                        {
                            res.Add(0x9F);
                            break;
                        }
                    case 'а':
                        {
                            res.Add(0xA0);
                            break;
                        }
                    case 'б':
                        {
                            res.Add(0xA1);
                            break;
                        }
                    case 'в':
                        {
                            res.Add(0xA2);
                            break;
                        }
                    case 'г':
                        {
                            res.Add(0xA3);
                            break;
                        }
                    case 'д':
                        {
                            res.Add(0xA4);
                            break;
                        }
                    case 'е':
                        {
                            res.Add(0xA5);
                            break;
                        }
                    case 'ж':
                        {
                            res.Add(0xA6);
                            break;
                        }
                    case 'з':
                        {
                            res.Add(0xA7);
                            break;
                        }
                    case 'и':
                        {
                            res.Add(0xA8);
                            break;
                        }
                    case 'й':
                        {
                            res.Add(0xA9);
                            break;
                        }
                    case 'к':
                        {
                            res.Add(0xAA);
                            break;
                        }
                    case 'л':
                        {
                            res.Add(0xAB);
                            break;
                        }
                    case 'м':
                        {
                            res.Add(0xAC);
                            break;
                        }
                    case 'н':
                        {
                            res.Add(0xAD);
                            break;
                        }
                    case 'о':
                        {
                            res.Add(0xAE);
                            break;
                        }
                    case 'п':
                        {
                            res.Add(0xAF);
                            break;
                        }
                    case 'р':
                        {
                            res.Add(0xE0);
                            break;
                        }
                    case 'с':
                        {
                            res.Add(0xE1);
                            break;
                        }
                    case 'т':
                        {
                            res.Add(0xE2);
                            break;
                        }
                    case 'у':
                        {
                            res.Add(0xE3);
                            break;
                        }
                    case 'ф':
                        {
                            res.Add(0xE4);
                            break;
                        }
                    case 'х':
                        {
                            res.Add(0xE5);
                            break;
                        }
                    case 'ц':
                        {
                            res.Add(0xE6);
                            break;
                        }
                    case 'ч':
                        {
                            res.Add(0xE7);
                            break;
                        }
                    case 'ш':
                        {
                            res.Add(0xE8);
                            break;
                        }
                    case 'щ':
                        {
                            res.Add(0xE9);
                            break;
                        }
                    case 'ъ':
                        {
                            res.Add(0xEA);
                            break;
                        }
                    case 'ы':
                        {
                            res.Add(0xEB);
                            break;
                        }
                    case 'ь':
                        {
                            res.Add(0xEC);
                            break;
                        }
                    case 'э':
                        {
                            res.Add(0xED);
                            break;
                        }
                    case 'ю':
                        {
                            res.Add(0xEE);
                            break;
                        }
                    case 'я':
                        {
                            res.Add(0xEF);
                            break;
                        }
                    case 'Ё':
                        {
                            res.Add(0xF0);
                            break;
                        }
                    case 'ё':
                        {
                            res.Add(0xF1);
                            break;
                        }
                    case '€':
                        {
                            res.Add(0xF2);
                            break;
                        }
                    case '$':
                        {
                            res.Add(0xFC);
                            break;
                        }
                    default:
                        {
                            res.Add(0x20);
                            break;
                        }
                }
            }
            return res.ToArray();
        }

        /// <summary>
        /// Декодирует массив байт, полученный от ККТ, в строку
        /// </summary>
        /// <param name="StringFromKKT"></param>
        /// <returns></returns>
        private static string DecodeStringFromKKT(byte[] StringFromKKT)
        {
            string res = "";
            foreach (byte item in StringFromKKT)
            {
                string cc = "";
                switch (item)
                {
                    case 0x20:
                        {
                            cc = " ";
                            break;
                        }
                    case 0x21:
                        {
                            cc = "!";
                            break;
                        }
                    case 0x22:
                        {
                            cc = "\"";
                            break;
                        }
                    case 0x23:
                        {
                            cc = "#";
                            break;
                        }
                    case 0x24:
                        {
                            cc = "№";
                            break;
                        }
                    case 0x25:
                        {
                            cc = "%";
                            break;
                        }
                    case 0x26:
                        {
                            cc = "&";
                            break;
                        }
                    case 0x27:
                        {
                            cc = "'";
                            break;
                        }
                    case 0x28:
                        {
                            cc = "(";
                            break;
                        }
                    case 0x29:
                        {
                            cc = ")";
                            break;
                        }
                    case 0x2A:
                        {
                            cc = "*";
                            break;
                        }
                    case 0x2B:
                        {
                            cc = "+";
                            break;
                        }
                    case 0x2C:
                        {
                            cc = ",";
                            break;
                        }
                    case 0x2D:
                        {
                            cc = "-";
                            break;
                        }
                    case 0x2E:
                        {
                            cc = ".";
                            break;
                        }
                    case 0x2F:
                        {
                            cc = "/";
                            break;
                        }
                    case 0x30:
                        {
                            cc = "0";
                            break;
                        }
                    case 0x31:
                        {
                            cc = "1";
                            break;
                        }
                    case 0x32:
                        {
                            cc = "2";
                            break;
                        }
                    case 0x33:
                        {
                            cc = "3";
                            break;
                        }
                    case 0x34:
                        {
                            cc = "4";
                            break;
                        }
                    case 0x35:
                        {
                            cc = "5";
                            break;
                        }
                    case 0x36:
                        {
                            cc = "6";
                            break;
                        }
                    case 0x37:
                        {
                            cc = "7";
                            break;
                        }
                    case 0x38:
                        {
                            cc = "8";
                            break;
                        }
                    case 0x39:
                        {
                            cc = "9";
                            break;
                        }
                    case 0x3A:
                        {
                            cc = ":";
                            break;
                        }
                    case 0x3B:
                        {
                            cc = ";";
                            break;
                        }
                    case 0x3C:
                        {
                            cc = "<";
                            break;
                        }
                    case 0x3D:
                        {
                            cc = "=";
                            break;
                        }
                    case 0x3E:
                        {
                            cc = ">";
                            break;
                        }
                    case 0x3F:
                        {
                            cc = "?";
                            break;
                        }
                    case 0x40:
                        {
                            cc = "@";
                            break;
                        }
                    case 0x41:
                        {
                            cc = "A";
                            break;
                        }
                    case 0x42:
                        {
                            cc = "B";
                            break;
                        }
                    case 0x43:
                        {
                            cc = "C";
                            break;
                        }
                    case 0x44:
                        {
                            cc = "D";
                            break;
                        }
                    case 0x45:
                        {
                            cc = "E";
                            break;
                        }
                    case 0x46:
                        {
                            cc = "F";
                            break;
                        }
                    case 0x47:
                        {
                            cc = "G";
                            break;
                        }
                    case 0x48:
                        {
                            cc = "H";
                            break;
                        }
                    case 0x49:
                        {
                            cc = "I";
                            break;
                        }
                    case 0x4A:
                        {
                            cc = "J";
                            break;
                        }
                    case 0x4B:
                        {
                            cc = "K";
                            break;
                        }
                    case 0x4C:
                        {
                            cc = "L";
                            break;
                        }
                    case 0x4D:
                        {
                            cc = "M";
                            break;
                        }
                    case 0x4E:
                        {
                            cc = "N";
                            break;
                        }
                    case 0x4F:
                        {
                            cc = "O";
                            break;
                        }
                    case 0x50:
                        {
                            cc = "P";
                            break;
                        }
                    case 0x51:
                        {
                            cc = "Q";
                            break;
                        }
                    case 0x52:
                        {
                            cc = "R";
                            break;
                        }
                    case 0x53:
                        {
                            cc = "S";
                            break;
                        }
                    case 0x54:
                        {
                            cc = "T";
                            break;
                        }
                    case 0x55:
                        {
                            cc = "U";
                            break;
                        }
                    case 0x56:
                        {
                            cc = "V";
                            break;
                        }
                    case 0x57:
                        {
                            cc = "W";
                            break;
                        }
                    case 0x58:
                        {
                            cc = "X";
                            break;
                        }
                    case 0x59:
                        {
                            cc = "Y";
                            break;
                        }
                    case 0x5A:
                        {
                            cc = "Z";
                            break;
                        }
                    case 0x5B:
                        {
                            cc = "[";
                            break;
                        }
                    case 0x5C:
                        {
                            cc = "\\";
                            break;
                        }
                    case 0x5D:
                        {
                            cc = "]";
                            break;
                        }
                    case 0x5E:
                        {
                            cc = "^";
                            break;
                        }
                    case 0x5F:
                        {
                            cc = "_";
                            break;
                        }
                    case 0x60:
                        {
                            cc = "`";
                            break;
                        }
                    case 0x61:
                        {
                            cc = "a";
                            break;
                        }
                    case 0x62:
                        {
                            cc = "b";
                            break;
                        }
                    case 0x63:
                        {
                            cc = "c";
                            break;
                        }
                    case 0x64:
                        {
                            cc = "d";
                            break;
                        }
                    case 0x65:
                        {
                            cc = "e";
                            break;
                        }
                    case 0x66:
                        {
                            cc = "f";
                            break;
                        }
                    case 0x67:
                        {
                            cc = "g";
                            break;
                        }
                    case 0x68:
                        {
                            cc = "h";
                            break;
                        }
                    case 0x69:
                        {
                            cc = "i";
                            break;
                        }
                    case 0x6A:
                        {
                            cc = "j";
                            break;
                        }
                    case 0x6B:
                        {
                            cc = "k";
                            break;
                        }
                    case 0x6C:
                        {
                            cc = "l";
                            break;
                        }
                    case 0x6D:
                        {
                            cc = "m";
                            break;
                        }
                    case 0x6E:
                        {
                            cc = "n";
                            break;
                        }
                    case 0x6F:
                        {
                            cc = "o";
                            break;
                        }
                    case 0x70:
                        {
                            cc = "p";
                            break;
                        }
                    case 0x71:
                        {
                            cc = "q";
                            break;
                        }
                    case 0x72:
                        {
                            cc = "r";
                            break;
                        }
                    case 0x73:
                        {
                            cc = "s";
                            break;
                        }
                    case 0x74:
                        {
                            cc = "t";
                            break;
                        }
                    case 0x75:
                        {
                            cc = "u";
                            break;
                        }
                    case 0x76:
                        {
                            cc = "v";
                            break;
                        }
                    case 0x77:
                        {
                            cc = "w";
                            break;
                        }
                    case 0x78:
                        {
                            cc = "x";
                            break;
                        }
                    case 0x79:
                        {
                            cc = "y";
                            break;
                        }
                    case 0x7A:
                        {
                            cc = "z";
                            break;
                        }
                    case 0x7B:
                        {
                            cc = "{";
                            break;
                        }
                    case 0x7C:
                        {
                            cc = "|";
                            break;
                        }
                    case 0x7D:
                        {
                            cc = "}";
                            break;
                        }
                    case 0x7E:
                        {
                            cc = "~";
                            break;
                        }
                    case 0x80:
                        {
                            cc = "А";
                            break;
                        }
                    case 0x81:
                        {
                            cc = "Б";
                            break;
                        }
                    case 0x82:
                        {
                            cc = "В";
                            break;
                        }
                    case 0x83:
                        {
                            cc = "Г";
                            break;
                        }
                    case 0x84:
                        {
                            cc = "Д";
                            break;
                        }
                    case 0x85:
                        {
                            cc = "Е";
                            break;
                        }
                    case 0x86:
                        {
                            cc = "Ж";
                            break;
                        }
                    case 0x87:
                        {
                            cc = "З";
                            break;
                        }
                    case 0x88:
                        {
                            cc = "И";
                            break;
                        }
                    case 0x89:
                        {
                            cc = "Й";
                            break;
                        }
                    case 0x8A:
                        {
                            cc = "К";
                            break;
                        }
                    case 0x8B:
                        {
                            cc = "Л";
                            break;
                        }
                    case 0x8C:
                        {
                            cc = "М";
                            break;
                        }
                    case 0x8D:
                        {
                            cc = "Н";
                            break;
                        }
                    case 0x8E:
                        {
                            cc = "О";
                            break;
                        }
                    case 0x8F:
                        {
                            cc = "П";
                            break;
                        }
                    case 0x90:
                        {
                            cc = "Р";
                            break;
                        }
                    case 0x91:
                        {
                            cc = "С";
                            break;
                        }
                    case 0x92:
                        {
                            cc = "Т";
                            break;
                        }
                    case 0x93:
                        {
                            cc = "У";
                            break;
                        }
                    case 0x94:
                        {
                            cc = "Ф";
                            break;
                        }
                    case 0x95:
                        {
                            cc = "Х";
                            break;
                        }
                    case 0x96:
                        {
                            cc = "Ц";
                            break;
                        }
                    case 0x97:
                        {
                            cc = "Ч";
                            break;
                        }
                    case 0x98:
                        {
                            cc = "Ш";
                            break;
                        }
                    case 0x99:
                        {
                            cc = "Щ";
                            break;
                        }
                    case 0x9A:
                        {
                            cc = "Ъ";
                            break;
                        }
                    case 0x9B:
                        {
                            cc = "Ы";
                            break;
                        }
                    case 0x9C:
                        {
                            cc = "Ь";
                            break;
                        }
                    case 0x9D:
                        {
                            cc = "Э";
                            break;
                        }
                    case 0x9E:
                        {
                            cc = "Ю";
                            break;
                        }
                    case 0x9F:
                        {
                            cc = "Я";
                            break;
                        }
                    case 0xA0:
                        {
                            cc = "а";
                            break;
                        }
                    case 0xA1:
                        {
                            cc = "б";
                            break;
                        }
                    case 0xA2:
                        {
                            cc = "в";
                            break;
                        }
                    case 0xA3:
                        {
                            cc = "г";
                            break;
                        }
                    case 0xA4:
                        {
                            cc = "д";
                            break;
                        }
                    case 0xA5:
                        {
                            cc = "е";
                            break;
                        }
                    case 0xA6:
                        {
                            cc = "ж";
                            break;
                        }
                    case 0xA7:
                        {
                            cc = "з";
                            break;
                        }
                    case 0xA8:
                        {
                            cc = "и";
                            break;
                        }
                    case 0xA9:
                        {
                            cc = "й";
                            break;
                        }
                    case 0xAA:
                        {
                            cc = "к";
                            break;
                        }
                    case 0xAB:
                        {
                            cc = "л";
                            break;
                        }
                    case 0xAC:
                        {
                            cc = "м";
                            break;
                        }
                    case 0xAD:
                        {
                            cc = "н";
                            break;
                        }
                    case 0xAE:
                        {
                            cc = "о";
                            break;
                        }
                    case 0xAF:
                        {
                            cc = "п";
                            break;
                        }
                    case 0xE0:
                        {
                            cc = "р";
                            break;
                        }
                    case 0xE1:
                        {
                            cc = "с";
                            break;
                        }
                    case 0xE2:
                        {
                            cc = "т";
                            break;
                        }
                    case 0xE3:
                        {
                            cc = "у";
                            break;
                        }
                    case 0xE4:
                        {
                            cc = "ф";
                            break;
                        }
                    case 0xE5:
                        {
                            cc = "х";
                            break;
                        }
                    case 0xE6:
                        {
                            cc = "ц";
                            break;
                        }
                    case 0xE7:
                        {
                            cc = "ч";
                            break;
                        }
                    case 0xE8:
                        {
                            cc = "ш";
                            break;
                        }
                    case 0xE9:
                        {
                            cc = "щ";
                            break;
                        }
                    case 0xEA:
                        {
                            cc = "ъ";
                            break;
                        }
                    case 0xEB:
                        {
                            cc = "ы";
                            break;
                        }
                    case 0xEC:
                        {
                            cc = "ь";
                            break;
                        }
                    case 0xED:
                        {
                            cc = "э";
                            break;
                        }
                    case 0xEE:
                        {
                            cc = "ю";
                            break;
                        }
                    case 0xEF:
                        {
                            cc = "я";
                            break;
                        }
                    case 0xF0:
                        {
                            cc = "Ё";
                            break;
                        }
                    case 0xF1:
                        {
                            cc = "ё";
                            break;
                        }
                    case 0xF2:
                        {
                            cc = "€";
                            break;
                        }
                    case 0xFA:
                        {
                            cc = "-";
                            break;
                        }
                    case 0xFC:
                        {
                            cc = "$";
                            break;
                        }
                    default:
                        {
                            cc = " ";
                            break;
                        }
                }
                res = string.Concat(res, cc);
            }
            return res;
        }

        public static List<byte> IncomingKKTData = new List<byte> { };


        /// <summary>
        /// Транспортный уровень
        /// </summary>
        private static class TransportLayerV30Constants
        {
            /// <summary>
            /// Константы транспортного уровня уровня 3
            /// </summary>
            public enum V30Transport
            {
                STX = 0xFE,
                ESC = 0xFD,
                TSTX = 0xEE,
                TESC = 0xED,
            }

            /// <summary>
            /// Статусы заданий ККТ
            /// </summary>
            public enum V30BatchStatus
            {
                Pending = 0xA1,
                InProgress = 0xA2,
                Result = 0xA3,
                Error = 0xA4,
                Stopped = 0xA5,
                AsyncResult = 0xA6,
                AsyncError = 0xA7,
                Waiting = 0xA8,
            }

            /// <summary>
            /// Ошибки
            /// </summary>
            public enum V30Errors
            {
                E_Overflow = 0xB1,
                E_AlreadyExists = 0xB2,
                E_NotFound = 0xB3,
                E_IllegalValue = 0xB4,
            }

            /// <summary>
            /// Команды буфера
            /// </summary>
            public enum V30BufferCommands
            {
                Add = 0xC1,
                Ack = 0xC2,
                Req = 0xC3,
                Abort = 0xC4,
                AckAdd = 0xC5,
            }
        }

        public static SerialDevice CashDeskDeviceSerialPort = null;
        private static SemaphoreSlim CashDeskCommandsListSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Считывает данные с последовательного порта фискального регистратора
        /// </summary>
        public static async void StartCommunication()
        {
            StartPage.UpdateProgress(5);
            while (true)
            {
                try
                {
                    CashDeskSerialDataReaderObject = new DataReader(CashDeskDeviceSerialPort.InputStream)
                    {
                        InputStreamOptions = InputStreamOptions.Partial
                    };
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    //эта задача нужна пока только для тестов, строчку CashDesk.Abort() - НЕ УДАЛЯТЬ
                    Task.Run(() => {
                        Abort();
                        //GetStageState();
                        //RequestKKTState();
                        //RequestPrinterState();
                        //GetCurrentStageParameters();
                        //PrintStrings(new List<string> { "application started"});
                        //CutReceipt();
                        //OpenReceipt();
                        //AddReceiptEntry();
                        //AddReceiptEntryData(17.85, 500, 10000);
                        //ReceiptPayment(10000);
                        //CloseReceipt(100);
                        //CancelReceipt();
                        //ExitCurrentMode();
                        //ChangeMode(DeviceStateMode.Registration_WaitingForCommand);
                        //PrintStrings(GlobalVars.ReceiptLinesToPrint);
                        //CutReceipt();
                        //OpenStage(); 
                        //CloseStage();
                        //GetStageState();
                        //RequestKKTState();
                        //RequestPrinterState();
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    while (true)
                    {
                        Task<uint> loadAsyncTask = CashDeskSerialDataReaderObject.LoadAsync(64).AsTask(StartPage.GlobalCancellationTokenSource.Token);
                        uint bytesRead = 0;
                        bytesRead = await loadAsyncTask;
                        if (bytesRead > 0)
                        {
                            try
                            {
                                byte[] tmpbyte = new byte[bytesRead];
                                CashDeskSerialDataReaderObject.ReadBytes(tmpbyte);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                AddData(tmpbyte);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            }
                            catch
                            {

                            }
                            finally
                            {

                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    CloseCashDeskSerialDevice();
                }
                catch /*(Exception ex)*/
                {
                    //AddItemToLogBox(ex.Message);
                }
                finally
                {
                    if (CashDeskSerialDataReaderObject != null)
                    {
                        CashDeskSerialDataReaderObject.DetachStream();
                        CashDeskSerialDataReaderObject = null;
                    }
                }
            }
        }

        /// <summary>
        /// Закрывает последовательный порт фискального регистратора
        /// </summary>
        private static void CloseCashDeskSerialDevice()
        {
            if (CashDeskDeviceSerialPort != null)
            {
                CashDeskDeviceSerialPort.Dispose();
            }
            CashDeskDeviceSerialPort = null;
        }

        /// <summary>
        /// Пароль доступа к ККТ
        /// </summary>
        public static byte[] KKTAccessCode = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// Внутренние статусы заданий
        /// </summary>
        public enum KKTTaskStatus
        {
            /// <summary>
            /// Новая задача
            /// </summary>
            New = 0,
            /// <summary>
            /// ожидание ответа от ККТ
            /// </summary>
            AwaitingAnswer = 1,
            /// <summary>
            /// Ожидание результата асинхронной задачи
            /// </summary>
            AwaitingAsyncResult = 2,
            /// <summary>
            /// Обработка ответа от ККТ
            /// </summary>
            ProcessingAnswer = 3,
            /// <summary>
            /// Отмена
            /// </summary>
            Abort = 4,
            /// <summary>
            /// Отработано, можно удалять
            /// </summary>
            Completed = 5
        }


        /// <summary>
        /// Постоянно проверяем наличие входящих данных от ККТ
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task IncomingKKTDataWatcher()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            while (true)
            {
                if (IncomingKKTData.Count > 0)
                {
                    int startpos = -1;
                    int endpos = -1;
                    for (int i = 0; i < IncomingKKTData.Count; i++)
                    {
                        if ((IncomingKKTData[i] == 0xFE && startpos >= 0))
                        {
                            endpos = i;
                            break;
                        }
                        if (IncomingKKTData[i] == 0xFE && startpos == -1)
                        {
                            startpos = i;
                        }
                    }
                    if (startpos >= 0)
                    {
                        if (endpos >= 0)
                        {
                            try
                            {
                                Task<UnWrappedV30Data> unwraptask = UnWrap_fromlevelV3(IncomingKKTData.GetRange(startpos, endpos - startpos).ToArray());
                                UnWrappedV30Data data = await unwraptask;
                                if (data.CheckCRC8)
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    Task.Run(() => { ProcessIncomingV3Packet(data); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    IncomingKKTData.RemoveRange(startpos, endpos - startpos);
                                }
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            try
                            {
                                Task<UnWrappedV30Data> unwraptask = UnWrap_fromlevelV3(IncomingKKTData.ToArray());
                                UnWrappedV30Data data = await unwraptask;
                                if (data.CheckCRC8)
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    Task.Run(() => { ProcessIncomingV3Packet(data); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    IncomingKKTData = new List<byte> { };
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                Task.Delay(500).Wait();
            }
        }

        /// <summary>
        /// Отсылаем новые задачи в порт ККТ
        /// </summary>
        /// <returns></returns>
        public static async Task KKTTaskWatcher()
        {
            while (true)
            {
                try
                {
                    CashDeskCommandsListSemaphore.Wait();
                    PendingTasks.RemoveAll(x => x.Expires <= DateTime.Now || x.Status == KKTTaskStatus.Completed);
                    foreach (KKTTask item in PendingTasks)
                    {
                        switch (item.Status)
                        {
                            case KKTTaskStatus.New:
                                {
                                    byte[] V3Data = PrepareV3(item);
                                    if (item.ParentTaskCommandByte != 0xFF)
                                    {
                                        KKTTask parentitem = PendingTasks.Where(x => x.Command.CommandByte == item.ParentTaskCommandByte).First();
                                        item.Command = parentitem.Command;
                                        item.ParentTaskCommandByte = 0xFF;
                                        item.Expires = parentitem.Expires;
                                        parentitem.Status = KKTTaskStatus.Completed;
                                    }
                                    DataWriter CashDeskSerialDataWriteObject = new DataWriter(CashDeskDeviceSerialPort.OutputStream);
                                    if (V3Data.Length != 0)
                                    {
                                        CashDeskSerialDataWriteObject.WriteBytes(V3Data);
                                        await CashDeskSerialDataWriteObject.StoreAsync();
                                        item.Status = KKTTaskStatus.AwaitingAnswer;
                                        SerialPortBytesWritten?.Invoke(V3Data);
                                    }
                                    else
                                    {

                                    }
                                    CashDeskSerialDataWriteObject.DetachStream();
                                    CashDeskSerialDataWriteObject = null;
                                    break;
                                }
                            case KKTTaskStatus.Abort:
                                {
                                    byte[] V3Data = PrepareV3(item);
                                    DataWriter CashDeskSerialDataWriteObject = new DataWriter(CashDeskDeviceSerialPort.OutputStream);
                                    if (V3Data.Length != 0)
                                    {
                                        CashDeskSerialDataWriteObject.WriteBytes(V3Data);
                                        await CashDeskSerialDataWriteObject.StoreAsync();
                                        item.Status = KKTTaskStatus.AwaitingAnswer;
                                        foreach (var item2 in PendingTasks.Where(x => x != item))
                                        {
                                            item2.Status = KKTTaskStatus.Completed;
                                        }
                                        StringBuilder hex = new StringBuilder();
                                        foreach (byte b in V3Data) hex.AppendFormat("{0:x2} ", b);
                                        StartPage.AddItemToLogBox("Запись в порт ККТ: " + hex.ToString());
                                        StartPage.AddItemToLogBox("Таймаут ожидания ответа от ККТ, сек: " + (item.Command.TimeOut / 1000).ToString());
                                    }
                                    else
                                    {

                                    }
                                    CashDeskSerialDataWriteObject.DetachStream();
                                    CashDeskSerialDataWriteObject = null;
                                    break;
                                }
                            default:
                                {
                                    await Task.Delay(100);
                                    break;
                                }
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    CashDeskCommandsListSemaphore.Release();
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// Класс, описывающий задачу для ККТ
        /// </summary>
        public class KKTTask
        {
            public delegate void KKTAnswerDelegate(byte[] KKTAnswerData, int TaskID, int V3PacketID);

            public KKTTask(CashDeskCommand cdcmd, int Tid, byte[] rqpayload)
            {
                Command = cdcmd;
                HighLevelTaskID = Tid;
                AnswerBytes = new byte[] { };
                RequestPayload = rqpayload;
                Status = KKTTaskStatus.New;
            }

            public KKTTask(CashDeskCommand cdcmd, int Tid, byte[] rqpayload, byte ParentTaskCmd)
            {
                Command = cdcmd;
                HighLevelTaskID = Tid;
                ParentTaskCommandByte = ParentTaskCmd;
                AnswerBytes = new byte[] { };
                RequestPayload = rqpayload;
                Status = KKTTaskStatus.New;
            }

            public KKTTaskStatus Status
            {
                get
                {
                    return _status;
                }
                set
                {
                    StartPage.AddItemToLogBox("Задача ККТ ID=" + LevelV30PacketID.ToString() + " TaskID=" + HighLevelTaskID.ToString() + " новый статус=" + value.ToString());
                    switch (value)
                    {
                        case KKTTaskStatus.New:
                            {
                                break;
                            }
                        case KKTTaskStatus.AwaitingAnswer:
                            {
                                break;
                            }
                        case KKTTaskStatus.ProcessingAnswer:
                            {
                                ProcessKKTAnswer?.Invoke(AnswerBytes, HighLevelTaskID, LevelV30PacketID);
                                break;
                            }
                        case KKTTaskStatus.AwaitingAsyncResult:
                            {
                                Task.Run(() => {
                                    Task.Delay(1000).Wait();
                                    RequestTaskResult(HighLevelTaskID);
                                    }
                                );
                                break;
                            }
                        case KKTTaskStatus.Abort:
                            {

                                break;
                            }
                    }
                    _status = value;
                }
            }
            private KKTTaskStatus _status;
            public byte ParentTaskCommandByte = 0xFF;
            public byte[] AnswerBytes;
            public int LevelV30PacketID;
            public DateTime Expires;
            public int HighLevelTaskID;
            public CashDeskCommand Command;
            public event KKTAnswerDelegate ProcessKKTAnswer;
            public readonly byte[] RequestPayload;

        }

        /// <summary>
        /// Класс, описывающий команду ККТ
        /// </summary>
        public class CashDeskCommand
        {
            public CashDeskCommand(byte cmdbyte, byte answerstart, uint timeoutmillisec, byte[] args)
            {
                CommandByte = cmdbyte;
                AnswerStartByte = answerstart;
                TimeOut = timeoutmillisec;
                CommandArgBytes = args;
            }
            public CashDeskCommand(byte cmdbyte, byte answerstart, uint timeoutmillisec)
            {
                CommandByte = cmdbyte;
                AnswerStartByte = answerstart;
                TimeOut = timeoutmillisec;
                CommandArgBytes = null;
            }
            public CashDeskCommand(byte cmdbyte, byte answerstart)
            {
                CommandByte = cmdbyte;
                AnswerStartByte = answerstart;
                TimeOut = 10000;
                CommandArgBytes = null;
            }
            public readonly byte CommandByte;
            public readonly byte AnswerStartByte;
            public readonly uint TimeOut;
            public readonly byte[] CommandArgBytes;
            
        }

        /// <summary>
        /// Байты, соответствующие командам
        /// </summary>
        private enum CommandBytes
        {
            GetKKTState = 0x3F,
            GetPrinterState = 0x45,
            GetDeviceType = 0xA5,
            GetParams = 0xA4,
            Abort = TransportLayerV30Constants.V30BufferCommands.Abort,
            Req = TransportLayerV30Constants.V30BufferCommands.Req,
            ReadRegister = 0x91,
            ExitCurrentMode = 0x48,
            ChangeMode = 0x56,
            CloseStage = 0x5A,
            OpenStage = 0x9A,
            OpenReceipt = 0x92,
            CancelReceipt = 0x59,
            CutReceipt = 0x75,
            StartReceiptEntry = 0xEA,
            AddEntryData = 0xEB,
            ReceiptPayment = 0x99,
            CloseReceipt = 0x4A,
            PrintString = 0x4C,
        }

        /// <summary>
        /// Типы устройств ККТ
        /// </summary>
        public enum DeviceTypes
        {
            NotDefined = 0,
            /// <summary>
            /// ККТ
            /// </summary>
            KKT = 1,
            /// <summary>
            /// Весы
            /// </summary>
            WeighingMachine = 2,
            /// <summary>
            /// Блок Memo Plus™
            /// </summary>
            MemoPlus = 3,
            /// <summary>
            /// Принтер этикеток
            /// </summary>
            TicketPrinter = 4,
            /// <summary>
            /// Терминал сбора данных
            /// </summary>
            MobileTerminal = 5,
            /// <summary>
            /// Дисплей покупателя
            /// </summary>
            CustomerDisplay = 6,
            /// <summary>
            /// Сканер штрихкода, PIN-клавиатура, ресторанная клавиатура
            /// </summary>
            InputDevice = 7,
        }

        /// <summary>
        /// Возвращает структуру текущего состояния ККТ
        /// </summary>
        /// <param name="Response">массив байт ответа</param>
        /// <returns></returns>
        private static void ProcessKKTStateResponse(byte[] Response, int taskID, int v3PacketID)
        {
            try
            {
                string res = "";
                if (Response.Length < 4)
                {
                    res = CheckErrorsAndStatus(Response[0], taskID);
                    //if (res != "") KKTStateReceived?.Invoke(null);
                    return;
                }
                if ((TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult ||
                    (TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    CashDeskKKTState tmp = new CashDeskKKTState(Response);
                    KKTStateReceived?.Invoke(tmp);
                    PendingTasks.First(x => x.HighLevelTaskID == taskID).Status = KKTTaskStatus.Completed;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Возвращает структуру параметров текущей смены
        /// </summary>
        /// <param name="Response">массив байт ответа</param>
        /// <returns></returns>
        private static void ProcessCurrentStageParamsResponse(byte[] Response, int taskID, int V3PacketID)
        {
            try
            {
                string res = "";
                if (Response.Length < 4)
                {
                    res = CheckErrorsAndStatus(Response[0], taskID);
                    //if (res != "") StageStateReceived?.Invoke(null);
                    return;
                }
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.Result || (TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult)
                {
                    CurrentStageParameters tmp = new CurrentStageParameters(Response);
                    CashDeskCurrentStageParametersReceived?.Invoke(tmp);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Возвращает структуру текущего состояния смены
        /// </summary>
        /// <param name="Response">массив байт ответа</param>
        /// <returns></returns>
        private static void ProcessStageStateResponse(byte[] Response, int taskID, int V3PacketID)
        {
            try
            {
                string res = "";
                if (Response.Length < 4)
                {
                    res = CheckErrorsAndStatus(Response[0], taskID);
                    //if (res != "") StageStateReceived?.Invoke(null);
                    return;
                }
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.Result || (TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult)
                {
                    StageProperties tmp = new StageProperties(Response);
                    StageStateReceived?.Invoke(tmp);
                    tmptask.Status = KKTTaskStatus.Completed;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Возвращает структуру текущего состояния устройства
        /// </summary>
        /// <param name="Response">массив байт ответа</param>
        /// <returns></returns>
        private static void ProcessDeviceStateResponse(byte[] Response, int taskID, int V3PacketID)
        {
            try
            {
                string res = "";
                if (Response.Length < 4)
                {
                    res = CheckErrorsAndStatus(Response[0], taskID);
                    //if (res != "") KKTPrinterStateReceived?.Invoke(null);
                    return;
                }
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.Result || (TransportLayerV30Constants.V30BatchStatus)Response[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult)
                {
                    KKTPrinterState tmp = new KKTPrinterState(Response);
                    KKTPrinterStateReceived?.Invoke(tmp);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду расчет по чеку
        /// </summary>
        private static void ProcessReceiptPaymentResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") ReceiptPaymentResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    string notpaidremain = "";
                    string change = "";
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        notpaidremain = (BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[4], KKTAnswerData[5], KKTAnswerData[6], KKTAnswerData[7], KKTAnswerData[8] }) / 0.01).ToString("N2");
                        change = (BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[9], KKTAnswerData[10], KKTAnswerData[11], KKTAnswerData[12], KKTAnswerData[13] }) / 0.01).ToString("N2");
                        res += "неоплаченный остаток = " + notpaidremain + ", сдача = " + change;
                    }
                    ReceiptPaymentResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }
        

        /// <summary>
        /// Обрабатывает ответ на команду по умолчанию
        /// </summary>
        private static void ProcessDefaultResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                //if (res != "") CloseReceiptResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    //CloseReceiptResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду добавить данные о позиции в чеке
        /// </summary>
        private static void ProcessCloseReceiptResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") CloseReceiptResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    CloseReceiptResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду добавить данные о позиции в чеке
        /// </summary>
        private static void ProcessAddEntryDataResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") AddEntryDataResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    AddEntryDataResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду добавить позицию в чек
        /// </summary>
        private static void ProcessStartReceiptEntryResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") StartReceiptEntryResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    StartReceiptEntryResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду отмены чека
        /// </summary>
        private static void ProcessCancelReceiptResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") CancelReceiptResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    CancelReceiptResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду открытия чека
        /// </summary>
        private static void ProcessOpenReceiptResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") OpenReceiptResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    OpenReceiptResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду открытия смены
        /// </summary>
        private static void ProcessOpenStageResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") OpenStageResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    OpenStageResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает ответ на команду закрытия смены
        /// </summary>
        private static void ProcessCloseStageResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") CloseStageResult?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    CloseStageResult?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// Обрабатывает ответ на команду смены режима
        /// </summary>
        private static void ProcessChangeModeModeResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") CurrentModeChanged?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    }
                    else
                    {
                        res += "ОК";
                    }
                    CurrentModeChanged?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обрабатывает сведения об устройстве
        /// </summary>
        private static void ProcessExitCurrentModeResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 2)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") CurrentModeChanged?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                }
                else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    bool tmperr = (KKTAnswerData[3] != 0);
                    if (tmperr)
                    {
                        res += "Код  ошибки: " + KKTAnswerData[3].ToString();
                    } else
                    {
                        res += "ОК";
                    }
                    CurrentModeChanged?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// Обрабатывает сведения об устройстве
        /// </summary>
        /// <param name="Response">массив байт ответа</param>
        /// <returns></returns>
        private static void ProcessDeviceTypeResponse(byte[] KKTAnswerData, int taskID, int V3PacketID)
        {
            string res = "";
            if (KKTAnswerData.Length < 11)
            {
                res = CheckErrorsAndStatus(KKTAnswerData[0], taskID);
                if (res != "") DeviceTypeReceived?.Invoke(res);
                return;
            }
            try
            {
                KKTTask tmptask = null;
                if (taskID != 0xFE)
                {
                    tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
                } else
                {
                    tmptask = PendingTasks.First(x => x.LevelV30PacketID == V3PacketID);
                }
                if ((TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.AsyncResult || 
                    (TransportLayerV30Constants.V30BatchStatus)KKTAnswerData[0] == TransportLayerV30Constants.V30BatchStatus.Result)
                {
                    res += "Код ошибки: " + ((TransportLayerV30Constants.V30Errors)KKTAnswerData[1]).ToString() + ", ";
                    string prot = "";
                    if (KKTAnswerData[2] == 1) prot = "3.0"; else prot = "2.0";
                    res += "версия протокола: " + prot + ", ";
                    string dtype = ((DeviceTypes)(KKTAnswerData[3])).ToString();
                    res += "тип: " + dtype + ", ";
                    string modelname = "";
                    if (KKTAnswerData[4] == 76) modelname = "Казначей ФА"; else modelname = "N/A";
                    res += "модель: " + modelname + ", ";
                    res += "режим: " + (KKTAnswerData[5] + KKTAnswerData[6] * 255).ToString() + ", ";
                    string langtable = "";
                    if (KKTAnswerData[9] == 0) langtable = "русский"; else langtable = "N/A";
                    res += "версия: " + BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[7] }).ToString() + "." + BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[8] }).ToString() + ", код языковой таблицы " + BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[9] }).ToString() + " (" + langtable + "), сборка " + BCDHelper.BCDByteToInt(new byte[] { KKTAnswerData[10], KKTAnswerData[11] }).ToString() + ", ";
                    byte[] devicenamebytes = new byte[KKTAnswerData.Length - 12];
                    Array.Copy(KKTAnswerData, 12, devicenamebytes, 0, devicenamebytes.Length);
                    string devicenametext = DecodeStringFromKKT(devicenamebytes);
                    res += "название: " + devicenametext;
                    DeviceTypeReceived?.Invoke(res);
                    tmptask.Status = KKTTaskStatus.Completed;
                    return;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// при слишком коротком ответе проверяет статус задания
        /// </summary>
        /// <param name="ResByte"></param>
        /// <param name="taskID"></param>
        /// <returns></returns>
        private static string CheckErrorsAndStatus(byte ResByte, int taskID)
        {
            string res = "";
            KKTTask tmptask = PendingTasks.First(x => x.HighLevelTaskID == taskID);
            switch (ResByte)
            {
                case (byte)TransportLayerV30Constants.V30Errors.E_NotFound:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30Errors.E_Overflow:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30Errors.E_IllegalValue:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30Errors.E_AlreadyExists:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30BatchStatus.Error:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30BatchStatus.AsyncError:
                    {
                        res += "Ошибка: " + ((TransportLayerV30Constants.V30Errors)ResByte).ToString();
                        tmptask.Status = KKTTaskStatus.Completed;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30BatchStatus.InProgress:
                    {
                        res += "Задание в процессе исполнения...";
                        //if (tmptask.Status != KKTTaskStatus.AwaitingAnswer)
                        tmptask.Status = KKTTaskStatus.AwaitingAsyncResult;
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30BatchStatus.Stopped:
                    {
                        res += "Задание остановлено...";
                        break;
                    }
                case (byte)TransportLayerV30Constants.V30BatchStatus.Pending:
                    {
                        res += "Задание ожидает исполнения...";
                        break;
                    }
            }
            return res;
        }

        /// <summary>
        /// Структура ответа от фискального регистратора
        /// </summary>
        private class UnWrappedV30Data
        {
            public UnWrappedV30Data(byte[] payload, byte packetid, byte crc8)
            {
                PacketID = packetid;
                CRC8 = crc8;
                if (payload.Length > 1)
                {
                    AnswerForTaskID = payload[2];
                    FirstAnswerByte = payload[1];
                }
                TaskStatus = (TransportLayerV30Constants.V30BatchStatus)payload[0];
                Payload = payload;
                byte[] datatocheckcrc8 = new byte[payload.Length + 1];
                datatocheckcrc8[0] = (byte)PacketID;
                Array.Copy(payload, 0, datatocheckcrc8, 1, payload.Length);
                CheckCRC8 = (CheckCRC8(datatocheckcrc8, (byte)CRC8));
            }
            /// <summary>
            /// Первый байт полезных данных, зависит от запроса на который идет ответ
            /// </summary>
            public readonly int FirstAnswerByte;
            /// <summary>
            /// Идентификатор транспортного пакета
            /// </summary>
            public readonly int PacketID;
            /// <summary>
            /// Статус задания
            /// </summary>
            public readonly TransportLayerV30Constants.V30BatchStatus TaskStatus;
            /// <summary>
            /// Идентификатор задания, на которое пришел ответ
            /// </summary>
            public readonly int AnswerForTaskID;
            /// <summary>
            /// Контрольная сумма CRC8
            /// </summary>
            public readonly int CRC8;
            /// <summary>
            /// Результат проверки контрольной суммы, 1 = контрольная сумма корректна
            /// </summary>
            public readonly bool CheckCRC8;
            /// <summary>
            /// Массив байт полезной нагрузки, последний байт - CRC , побитовый XOR 
            /// </summary>
            public readonly byte[] Payload;
        }


        private static int _packetid = -1;
        public static List<KKTTask> PendingTasks = new List<KKTTask>() { };
        private static int _taskid = -1;
        /// <summary>
        /// Счетчик идентификатора транспортных пакетов, взвращает значение в диапазоне 0x00..0xDF
        /// </summary>
        private static byte Packetid
        {
            get
            {
                _packetid++;
                if (_packetid == 223) _packetid = 0;
                return (byte)_packetid;
            }
        }

        /// <summary>
        /// Счетчик идентификатора заданий, взвращает значение в диапазоне 0x00..0xDF
        /// </summary>
        private static byte Taskid
        {
            get
            {
                _taskid++;
                if (_taskid == 223) _taskid = 0;
                return (byte)_taskid;
            }
        }

        /// <summary>
        /// Вспомогательный класс для вычисления контрольной суммы
        /// </summary>
        private class CRC8Calc
        {
            private byte[] table = new byte[256];
            public byte Checksum(params byte[] val)
            {
                if (val == null)
                    throw new ArgumentNullException("val");
                byte c = 0x00;
                foreach (byte b in val)
                {
                    c = table[c ^ b];
                }
                return c;
            }

            public byte[] Table
            {
                get
                {
                    return table;
                }
                set
                {
                    table = value;
                }
            }

            public byte[] GenerateTable()
            {
                byte[] csTable = new byte[256];
                for (int i = 0; i < 256; ++i)
                {
                    int curr = i;
                    for (int j = 0; j < 8; ++j)
                    {
                        if ((curr & 0x80) != 0)
                        {
                            curr = (curr << 1) ^ 31;
                        }
                        else
                        {
                            curr <<= 1;
                        }
                    }
                    csTable[i] = (byte)curr;
                }
                return csTable;
            }

            public CRC8Calc()
            {
                table = GenerateTable();
            }
        }

        /// <summary>
        /// Формирует транспортный пакет уровня 3, готовый к отправке в последовательный порт
        /// </summary>
        /// <param name="Data">Массив байт полезной нагрузки</param>
        /// <param name="cmd">текущий экземпляр команды</param>
        /// <returns></returns>
        private static byte[] PrepareV3(KKTTask cmd)
        {
            List<byte> res = new List<byte> { };
            res.Add((byte)TransportLayerV30Constants.V30Transport.STX);
            res.Add(0x00);//datalength LSB
            res.Add(0x00);//datalength MSB
            byte current_packet_id = Packetid;
            res.Add(current_packet_id);
            cmd.LevelV30PacketID = current_packet_id;
            res[1] = (byte)((byte)cmd.RequestPayload.Length & 0x7F); //LSB
            res[2] = (byte)((byte)cmd.RequestPayload.Length >> 7);  //MSB
            byte[] crc_data = new byte[cmd.RequestPayload.Length + 1];
            crc_data[0] = current_packet_id;
            Array.Copy(cmd.RequestPayload, 0, crc_data, 1, cmd.RequestPayload.Length);
            byte checksum = CRC8Helper.CRC8(crc_data);
            for (int i = 0; i < cmd.RequestPayload.Length; i++)
            {
                if (cmd.RequestPayload[i] == (byte)TransportLayerV30Constants.V30Transport.STX)
                {
                    res.Add((byte)TransportLayerV30Constants.V30Transport.ESC);
                    res.Add((byte)TransportLayerV30Constants.V30Transport.TSTX);
                }
                else
                if (cmd.RequestPayload[i] == (byte)TransportLayerV30Constants.V30Transport.ESC)
                {
                    res.Add((byte)TransportLayerV30Constants.V30Transport.ESC);
                    res.Add((byte)TransportLayerV30Constants.V30Transport.TESC);
                }
                else
                {
                    res.Add(cmd.RequestPayload[i]);
                }
            }
            if (checksum == (byte)TransportLayerV30Constants.V30Transport.STX)
            {
                res.Add((byte)TransportLayerV30Constants.V30Transport.ESC);
                res.Add((byte)TransportLayerV30Constants.V30Transport.TSTX);
            }
            else
                if (checksum == (byte)TransportLayerV30Constants.V30Transport.ESC)
            {
                res.Add((byte)TransportLayerV30Constants.V30Transport.ESC);
                res.Add((byte)TransportLayerV30Constants.V30Transport.TESC);
            }
            else
            {
                res.Add(checksum);
            }
            return res.ToArray();
        }


        /// <summary>
        /// Проверка контрольной суммы
        /// </summary>
        /// <param name="data"></param>
        /// <param name="crc8"></param>
        /// <returns></returns>
        private static bool CheckCRC8(byte[] data, byte crc8)
        {
            byte checksum = CRC8Helper.CRC8(data);
            return (checksum == crc8);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Возвращает структуру UnWrappedLevelV3Data
        /// </summary>
        /// <param name="LowLevelV3_Data">Транспортный пакет уровня 3</param>
        /// <returns></returns>
        private static async Task<UnWrappedV30Data> UnWrap_fromlevelV3(byte[] LowLevelV3_Data)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            int datalength = LowLevelV3_Data[1];
            datalength += LowLevelV3_Data[2] * 255;//вычисляем длину полезных данных, два байта, LSB
            byte V3packetID = LowLevelV3_Data[3];
            byte[] masked_data = new byte[(LowLevelV3_Data.Length - 4)];//инициализируем массив данных в пакете с учетом маскировки, без стартового байта, 
                                                                        //двух байт длины и идентификатора транспортного пакета
            Array.Copy(LowLevelV3_Data, 4, masked_data, 0, masked_data.Length);//копируем данные в масив
            List<byte> nonmasked_data = new List<byte> { };//инициализируем коллекцию байт данных без маскировки на транспортном уровне
            for (int i = 0; i < masked_data.Length; i++)
            {
                if (masked_data[i] == (byte)TransportLayerV30Constants.V30Transport.ESC)//убираем максировку транспортного уровня
                {
                    if (masked_data[i + 1] == (byte)TransportLayerV30Constants.V30Transport.TSTX)
                    {
                        nonmasked_data.Add((byte)TransportLayerV30Constants.V30Transport.STX);
                    }
                    else
                    if (masked_data[i + 1] == (byte)TransportLayerV30Constants.V30Transport.TESC)
                    {
                        nonmasked_data.Add((byte)TransportLayerV30Constants.V30Transport.ESC);
                    }
                    else
                    {
                        return null;//недопустимая последовательность байт, прерываем обработку и возвращаем пустой объект
                    }
                    i += 2;
                }
                else
                {
                    nonmasked_data.Add(masked_data[i]);//если байт немаскирован, добавляем его в коллекцию
                }
            }
            if (datalength == nonmasked_data.Count - 1)//сравниваем datalength и длину немаскированных данных без учета байта контрольной суммы и номера пакета
            {
                byte[] payload = new byte[nonmasked_data.Count - 1];
                Array.Copy(nonmasked_data.ToArray(), 0, payload, 0, payload.Length);
                byte crc8 = nonmasked_data[nonmasked_data.Count - 1];
                return new UnWrappedV30Data(payload, V3packetID, crc8);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Формирует массив байт данных полезной нагрузки задания для передачи в ККТ
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="NeedResult">настройка передачи результата задания (0 – результат не
        /// передается, 1 – результат передается);</param>
        /// <param name="IgnoreError">настройка работы с ошибками (0 – не игнорировать, 1 –
        /// игнорировать);</param>
        /// <param name="WaitAsyncData">настройка ожидания выполнения задания (0 – сразу выполнять
        /// задание, 1 – ожидать выполнения задания сколь угодно долго, не препятствовать
        /// добавлению нового задания).</param>
        /// <returns></returns>
        private static byte[] PrepareCashDeskTask(CashDeskCommand cmd, byte taskid, bool NeedResult = true, bool IgnoreError = false, bool WaitAsyncData = false)
        {
            List<byte> res = new List<byte>() { };
            res.Add((byte)TransportLayerV30Constants.V30BufferCommands.Add);//код команды добавления задания
            res.Add((byte)(Convert.ToInt32(NeedResult) + Convert.ToInt32(IgnoreError) * 2 + Convert.ToInt32(WaitAsyncData) * 4));//выставляем флаги
            res.Add(taskid);//добавляем идентификатор задания
            res.AddRange(KKTAccessCode);//добавляем пароль доступа ККТ
            if (cmd.CommandByte != 0x00) res.Add(cmd.CommandByte);//добавляем байт команды
            if (cmd.CommandArgBytes != null) res.AddRange(cmd.CommandArgBytes);//добавляем дополнительные аргументы если есть
            return res.ToArray();
        }

        /// <summary>
        /// Формирует массив байт данных прерывания всех заданий ККТ
        /// </summary>
        /// <returns></returns>
        private static byte[] PrepareCashDeskAbort()
        {
            List<byte> res = new List<byte>() { };
            res.Add((byte)TransportLayerV30Constants.V30BufferCommands.Abort);//код команды
            return res.ToArray();
        }

        /// <summary>
        /// Формирует массив байт данных выхода из текущего режима
        /// </summary>
        /// <returns></returns>
        private static byte[] PrepareExitCurrentMode()
        {
            List<byte> res = new List<byte>() { };
            res.Add((byte)CommandBytes.ExitCurrentMode);//код команды
            return res.ToArray();
        }

        /// <summary>
        /// Формирует массив байт данных запроса результата асинхронной задачи
        /// </summary>
        /// <returns></returns>
        private static byte[] PrepareCashDeskTaskStatusRequest(int TaskID)
        {
            List<byte> res = new List<byte>() { };
            res.Add((byte)TransportLayerV30Constants.V30BufferCommands.Req);//код команды
            res.Add((byte)TaskID);
            return res.ToArray();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Обрабатываем входящий транспортный пакет
        /// </summary>
        /// <param name="LowLevelV3_Data">экземпляр структуры UnWrappedV30Data</param>
        private static async void ProcessIncomingV3Packet(UnWrappedV30Data data)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (data.PacketID == -1)
            {
                return;//ошибочные данные, отбрасываем пакет
            }
            KKTTask cmd = null;
            try
            {
                cmd = PendingTasks.Where(x => (x.LevelV30PacketID == data.PacketID && x.Status != KKTTaskStatus.Completed)).First();
                //StartPage.AddItemToLogBox("Получен ответ на задачу TaskID=" + cmd.HighLevelTaskID.ToString() + ", байт: " + data.Payload.Length.ToString());
            }
            catch
            {

            }
            if (cmd != null && (cmd.Status == KKTTaskStatus.AwaitingAnswer || cmd.Status == KKTTaskStatus.AwaitingAsyncResult))
            {
                cmd.AnswerBytes = data.Payload;
                switch (cmd.Command.CommandByte)
                {
                    case (byte)CommandBytes.ReadRegister:
                        {
                            switch (cmd.Command.CommandArgBytes[0])
                            {
                                case 0x12:
                                    {
                                        cmd.ProcessKKTAnswer += ProcessStageStateResponse;
                                        cmd.Status = KKTTaskStatus.ProcessingAnswer;
                                        return;
                                    }
                            }
                            return;
                        }
                    case (byte)CommandBytes.GetParams:
                        {
                            switch (cmd.Command.CommandArgBytes[0])
                            {
                                case 0x10:
                                    {
                                        cmd.ProcessKKTAnswer += ProcessCurrentStageParamsResponse;
                                        cmd.Status = KKTTaskStatus.ProcessingAnswer;
                                        return;
                                    }
                            }
                            return;
                        }
                    case (byte)CommandBytes.CloseReceipt:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessCloseReceiptResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.ReceiptPayment:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessReceiptPaymentResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.AddEntryData:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessAddEntryDataResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.StartReceiptEntry:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessStartReceiptEntryResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.CancelReceipt:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessCancelReceiptResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.OpenReceipt:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessOpenReceiptResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.OpenStage:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessOpenStageResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.CloseStage:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessCloseStageResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.ChangeMode:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessChangeModeModeResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.ExitCurrentMode:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessExitCurrentModeResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.GetDeviceType:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessDeviceTypeResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.GetPrinterState:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessDeviceStateResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.GetKKTState:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessKKTStateResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                    case (byte)CommandBytes.Abort:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            switch (cmd.AnswerBytes[0])
                            {
                                case (byte)TransportLayerV30Constants.V30BatchStatus.Result:
                                    {
                                        AllTasksCancelled?.Invoke();
                                        break;
                                    }
                            }
                            cmd.Status = KKTTaskStatus.Completed;
                            return;
                        }
                    default:
                        {
                            //ProcessDeviceTypeResponse(data.Payload);
                            cmd.ProcessKKTAnswer += ProcessDefaultResponse;
                            cmd.Status = KKTTaskStatus.ProcessingAnswer;
                            return;
                        }
                }
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Команда удаляет все задания, вне зависимости от того, успешно оно или нет и установлен ли
        /// флаг NeedResult.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static async Task AbortCashDeskTasks(CashDeskCommand cmd)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            await CashDeskCommandsListSemaphore.WaitAsync();
            try
            {
                byte currenttaskid = Taskid;
                KKTTask scmd = new KKTTask(cmd, currenttaskid, PrepareCashDeskAbort());
                scmd.Expires = DateTime.Now.AddMilliseconds(scmd.Command.TimeOut);
                scmd.Status = KKTTaskStatus.Abort;
                PendingTasks.Add(scmd);
                //SendCommand(V3Data)q;
            }
            catch
            {

            }
            CashDeskCommandsListSemaphore.Release();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Команда выхода в надрежим текущего режима
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static async Task CurrentModeLevelUp(CashDeskCommand cmd)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            await CashDeskCommandsListSemaphore.WaitAsync();
            try
            {
                byte currenttaskid = Taskid;
                KKTTask scmd = new KKTTask(cmd, currenttaskid, PrepareExitCurrentMode());
                scmd.Expires = DateTime.Now.AddMilliseconds(scmd.Command.TimeOut);
                PendingTasks.Add(scmd);
                //SendCommand(V3Data)q;
            }
            catch
            {

            }
            CashDeskCommandsListSemaphore.Release();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Запрашивает результат выполнения асинхронной задачи
        /// флаг NeedResult.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static async Task RequestCashDeskTaskResult(CashDeskCommand cmd, int TaskID)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            await CashDeskCommandsListSemaphore.WaitAsync();
            try
            {
                KKTTask scmd = new KKTTask(cmd, TaskID, PrepareCashDeskTaskStatusRequest(TaskID),PendingTasks.First(x => x.HighLevelTaskID == TaskID).Command.CommandByte);
                scmd.Expires = DateTime.Now.AddMilliseconds(scmd.Command.TimeOut);
                PendingTasks.Add(scmd);
                //SendCommand(V3Data)q;
            }
            catch
            {

            }
            CashDeskCommandsListSemaphore.Release();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// подготовка задания для ККТ и помещение в список PendingTasks
        /// </summary>
        /// <param name="cmd"></param>
        private static async void AddCashDeskTask(CashDeskCommand[] Commands)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            CashDeskCommandsListSemaphore.Wait();
            try
            {
                foreach (var item in Commands)
                {
                    byte currenttaskid = Taskid;
                    KKTTask scmd = new KKTTask(item, currenttaskid, PrepareCashDeskTask(item, currenttaskid)); ;
                    scmd.Expires = DateTime.Now.AddMilliseconds(scmd.Command.TimeOut);
                    PendingTasks.Add(scmd);
                }
            }
            catch
            {

            }
            CashDeskCommandsListSemaphore.Release();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Добавляет данные из входящего потока в массив для дальнейшей обработки
        /// </summary>
        /// <param name="answerdata"></param>
        /// <returns></returns>
        public static async Task AddData(byte[] answerdata)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Mutex m = new Mutex(true, "CashDeskIncomingV30packetAccessMutex", out bool mutexWasCreated);
            if (!mutexWasCreated)
            {
                try
                {
                    m.WaitOne();
                }
                catch (AbandonedMutexException)
                {

                }
            }
            IncomingKKTData.AddRange(answerdata.ToList());
            m.ReleaseMutex();
            m.Dispose();
        }


        /// <summary>
        /// запрос типа устройства
        /// </summary>
        public static void RequestDeviceType()
        {
            StartPage.AddItemToLogBox("Пробуем прочитать информацию о кассовом аппарате...");
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.GetDeviceType, 0x00);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Запрос информации о состоянии устройства ККТ
        /// </summary>
        public static void RequestPrinterState()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.GetPrinterState, 0x00);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Запрос информации о состоянии ККТ
        /// </summary>
        public static void RequestKKTState()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.GetKKTState, 0x00);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// отмена всех заданий ККТ
        /// </summary>
        public static void Abort()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)TransportLayerV30Constants.V30BufferCommands.Abort, 0x00);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AbortCashDeskTasks(cmd);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Запрос результатата задания
        /// </summary>
        /// <param name="TaskID"></param>
        private static void RequestTaskResult(int TaskID)
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)TransportLayerV30Constants.V30BufferCommands.Req, 0x00,10000, new byte[1] { (byte)TaskID });
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RequestCashDeskTaskResult(cmd, TaskID);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Запрос состояния смены
        /// </summary>
        public static void GetStageState()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.ReadRegister, 0x00, 10000, new byte[3] { 0x12, 0x00, 0x01 });
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Запрос состояния смены
        /// </summary>
        public static void GetCurrentStageParameters()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.GetParams, 0x55, 10000, new byte[1] { 0x10 });
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Выход из текущего режима на уровень выше
        /// </summary>
        public static void ExitCurrentMode()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.ExitCurrentMode, 0x55);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Вход в режим
        /// </summary>
        public static void ChangeMode(DeviceStateMode NewMode)
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.ChangeMode, 0x55, 10000, new byte[] { (byte)NewMode, 0x00, 0x00, 0x00, 0x30 });
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Закрытие смены
        /// </summary>
        public static void CloseStage()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.CloseStage, 0x55, 45000);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Открытие смены
        /// </summary>
        public static void OpenStage(string OpenStageText = "")
        {
            byte[] textbytes;
            if (OpenStageText != "" && OpenStageText.Length < 58)
            {
                textbytes = EncodeStringToKKTBytes(OpenStageText);
            } else
            {
                textbytes = new byte[57];
            }
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.OpenStage, 0x55, 10000, textbytes);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Открытие чека
        /// </summary>
        public static void OpenReceipt()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.OpenReceipt, 0x55, 10000, new byte[] { 0x00, 0x01 });
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Отмена чека
        /// </summary>
        public static void CancelReceipt()
        {
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.CancelReceipt, 0x55);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Формирование товарной позиции в чеке
        /// </summary>
        public static void AddReceiptEntry()
        {
            CashDeskCommand StartEntrycmd = new CashDeskCommand((byte)CommandBytes.StartReceiptEntry, 0x55, 10000, new byte[] { 0x00, 0x01, 0x00 });
            AddCashDeskTask(new CashDeskCommand[] { StartEntrycmd });
        }

        /// <summary>
        /// Формирование товарной позиции в чеке
        /// </summary>
        public static void CutReceipt()
        {
            CashDeskCommand CutReceiptcmd = new CashDeskCommand((byte)CommandBytes.CutReceipt, 0x55, 10000, new byte[] { 0x01 });
            AddCashDeskTask(new CashDeskCommand[] { CutReceiptcmd });
        }

        /// <summary>
        /// печать кассового чека
        /// </summary>
        public static void PrintReceipt(List<string> ReceiptLinesToPrint, double Quantity, int PricePerItem_MDE, int Cash_MDE)
        {
            List<CashDeskCommand> tmpcmdlist = new List<CashDeskCommand> { };
            foreach (string item in ReceiptLinesToPrint)
            {
                tmpcmdlist.Add(new CashDeskCommand((byte)CommandBytes.PrintString, 0x55, 10000, EncodeStringToKKTBytes(item)));
            }
            CashDeskCommand OpenReceiptcmd = new CashDeskCommand((byte)CommandBytes.OpenReceipt, 0x55, 10000, new byte[] { 0x00, 0x01 });
            CashDeskCommand StartEntrycmd = new CashDeskCommand((byte)CommandBytes.StartReceiptEntry, 0x55, 10000, new byte[] { 0x00, 0x01, 0x00 });
            byte[] PriceData = BCDHelper.IntToBcd2(false, PricePerItem_MDE.ToString().PadLeft(14, '0'));
            byte[] QuantityData = BCDHelper.IntToBcd2(false, (Math.Round(Quantity * 1000,MidpointRounding.AwayFromZero)).ToString().PadLeft(10, '0'));
            byte[] PositionCostData = BCDHelper.IntToBcd2(false, (Math.Round(Quantity * PricePerItem_MDE, MidpointRounding.AwayFromZero)).ToString().PadLeft(14, '0'));
            byte[] productnamebytes = EncodeStringToKKTBytes(StartPage.CurrentSaleSession.ProductName);
            byte[] TaxSystemBytes = BCDHelper.IntToBcd2(false, StartPage.CurrentSaleSession.TaxSystemInUse.ToString().PadLeft(2, '0'));
            List<byte> tmplist = new List<byte> { 0x02,
                PriceData[0], PriceData[1], PriceData[2], PriceData[3], PriceData[4], PriceData[5], PriceData[6],
                QuantityData[0], QuantityData[1], QuantityData[2], QuantityData[3], QuantityData[4],
                PositionCostData[0], PositionCostData[1], PositionCostData[2], PositionCostData[3], PositionCostData[4], PositionCostData[5], PositionCostData[6],
                TaxSystemBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00 };
            tmplist.AddRange(productnamebytes.ToList());
            byte[] entrydata = tmplist.ToArray();
            CashDeskCommand AddEntryDatacmd = new CashDeskCommand((byte)CommandBytes.AddEntryData, 0x55, 10000, entrydata);
            byte[] CashData = BCDHelper.IntToBcd2(false, Cash_MDE.ToString().PadLeft(10, '0'));
            byte[] PaymentData = new byte[] { 0x00, 0x01, CashData[0], CashData[1], CashData[2], CashData[3], CashData[4] };
            CashDeskCommand PaymentDatacmd = new CashDeskCommand((byte)CommandBytes.ReceiptPayment, 0x55, 10000, PaymentData);
            byte[] CloseReceiptData = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
            CashDeskCommand CloseReceiptDatacmd = new CashDeskCommand((byte)CommandBytes.CloseReceipt, 0x55, 25000, CloseReceiptData);
            tmpcmdlist.AddRange(new List<CashDeskCommand> { OpenReceiptcmd, StartEntrycmd, AddEntryDatacmd, PaymentDatacmd, CloseReceiptDatacmd });
            AddCashDeskTask(tmpcmdlist.ToArray());
        }


        /// <summary>
        /// Получение информации для отправки на сервер
        /// </summary>
        public static void GetKKTInformation()
        {
            List<CashDeskCommand> tmpcmdlist = new List<CashDeskCommand> { };
            CashDeskCommand GetStageStateCMD = new CashDeskCommand((byte)CommandBytes.ReadRegister, 0x00, 10000, new byte[3] { 0x12, 0x00, 0x01 });
            CashDeskCommand GetKKTStateCMD = new CashDeskCommand((byte)CommandBytes.GetKKTState, 0x00);
            CashDeskCommand GetPrinterStateCMD = new CashDeskCommand((byte)CommandBytes.GetPrinterState, 0x00);
            CashDeskCommand GetCurrentStageParametersCMD = new CashDeskCommand((byte)CommandBytes.GetParams, 0x55, 10000, new byte[1] { 0x10 });
            tmpcmdlist.AddRange(new List<CashDeskCommand> { GetStageStateCMD, GetKKTStateCMD, GetPrinterStateCMD, GetCurrentStageParametersCMD });
            AddCashDeskTask(tmpcmdlist.ToArray());
        }

        /// <summary>
        /// печать строк
        /// </summary>
        public static void PrintStrings(List<string> ReceiptLinesToPrint)
        {
            List<CashDeskCommand> tmpcmdlist = new List<CashDeskCommand> { };
            foreach (string item in ReceiptLinesToPrint)
            {
                tmpcmdlist.Add(new CashDeskCommand((byte)CommandBytes.PrintString, 0x55, 10000, EncodeStringToKKTBytes(item)));
            }
            AddCashDeskTask(tmpcmdlist.ToArray());
        }

        /// <summary>
        /// Расчет по чеку
        /// </summary>
        public static void ReceiptPayment(int cashsum)
        {
            byte[] CashData = BCDHelper.IntToBcd2(false, cashsum.ToString().PadLeft(10, '0'));
            byte[] PaymentData = new byte[] { 0x00, 0x01, CashData[0], CashData[1], CashData[2], CashData[3], CashData[4] };
            CashDeskCommand cmd = new CashDeskCommand((byte)CommandBytes.ReceiptPayment, 0x55, 10000, PaymentData);
            AddCashDeskTask(new CashDeskCommand[] { cmd });
        }

        /// <summary>
        /// Закрыть чек
        /// </summary>
        public static void CloseReceipt(int cashsum)
        {
            byte[] CloseReceiptData = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
            CashDeskCommand CloseReceiptDatacmd = new CashDeskCommand((byte)CommandBytes.CloseReceipt, 0x55, 25000, CloseReceiptData);
            AddCashDeskTask(new CashDeskCommand[] { CloseReceiptDatacmd });
        }
    }

}

namespace RPiVendApp
{
    public class WaterDeviceTelemetry
    {
        public long ID = -1;
        public long DateTime = -1;
        public string DateTimeStr = "";
        public long WaterDeviceID = -1;
        public int CC10RURCount = -1;
        public int CC5RURCount = -1;
        public int CC2RURCount = -1;
        public int CC1RURCount = -1;
        public double CCSum = -1;
        public int CBCount = -1;
        public double CBSum = -1;
        public double IncassoSum = -1;
        public double WaterTempCelsius = -1;
        public double InboxTempCelsius = -1;
        public double AmbientTempCelsius = -1;
        public double AmbientRelativeHumidity = -1;
        public int WaterLevelPercent = -1;
        public bool IsHeaterOn = false;
        public bool IsExternalLightOn = false;
        public bool IsFillPumpSocketActive = false;
        public double TotalLitersDIspensed = -1;
        public double TotalHoursWorked = -1;
        public string KKTMfgNumber = "";
        public bool KKTStageOpened = false;
        public bool KKTStageOver24h = false;
        public string KKTStageNumber = "";
        public bool KKTReceiptOpened = false;
        public int KKTCurrentMode = -1;
        public bool KKTPrinterConnected = false;
        public bool KKTPrinterPaperEmpty = false;
        public bool KKTPrinterNonRecoverableError = false;
        public bool KKTPrinterCutterError = false;
        public bool KKTPrinterOverHeated = false;
        public bool KKTPrinterPaperJammed = false;
        public bool KKTPrinterPaperEnding = false;
        public int BABillsCount = -1;
        public double BASum = -1;
        public long LastStageClosedDateTime  = -1;
        public string LastStageClosedDateTimeStr = "";
        public int KKTReceiptNextNumber = -1;
        public string VMCMode = "";
        public int MDBInitStep = -1;
        public string ProgramVersion = "";
    }
}

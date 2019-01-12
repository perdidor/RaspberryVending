namespace RPiVendApp
{

    public class MDBCommands
    {
        public byte[] ResetBA;
        public byte[] ResetCC;
        public byte[] GetBAStatus;
        public byte[] GetCCStatus;
        public byte[] DisableAcceptCoins;
        public byte[] DisableAcceptBills;
        public byte[] EnableAcceptCoins;
        public byte[] EnableAcceptBills;
        public byte[] PayOutCoins;
        public byte[] ReturnBill;
        public byte[] EnableDispenseCoins;
        public byte[] DispenseCoinsInProgressMessage;
        public byte[] GetDispensedCoinsInfo;
        public int DispensedCoinsInfoDataLength;
        public bool WaitWhileDispenseCoins;
    }

    class MDBCommandSets
    {
        /// <summary>
        /// Currenza C2 Green coin changer and ICT A7/V7 Bill validator
        /// </summary>
        public static MDBCommands CurrenzaC2Blue_ICTA7V7 = new MDBCommands
        {
            DisableAcceptBills = new byte[] { 0x34, 0x00, 0x00, 0x00, 0x00, 0x34 },
            DisableAcceptCoins = new byte[] { 0x0C, 0x00, 0x00, 0x00, 0xFF, 0x0B },
            DispenseCoinsInProgressMessage = new byte[] { 0x08, 0x02 },
            DispensedCoinsInfoDataLength = 47,
            EnableAcceptBills = new byte[] { 0x34, 0x00, 0x06, 0x00, 0x00, 0x3A },
            EnableAcceptCoins = new byte[] { 0x0C, 0x00, 0xFF, 0x00, 0xFF, 0x0A },
            EnableDispenseCoins = new byte[] { 0x0C, 0x00, 0x00, 0x00, 0xFF, 0x0B },
            GetBAStatus = new byte[] { 0x36, 0x36 },
            GetCCStatus = new byte[] { 0x0A, 0x0A },
            GetDispensedCoinsInfo = new byte[] { 0x0F, 0x03, 0x12 },
            PayOutCoins = new byte[] { 0x0F, 0x02 },
            ResetBA = new byte[] { 0x30, 0x30 },
            ResetCC = new byte[] { 0x08, 0x08 },
            ReturnBill = new byte[] { 0x35, 0x00, 0x35 },
            WaitWhileDispenseCoins = true,
        };

        public static byte[] CurrenzaC2Green_PayoutCoins(int PayOutSum)
        {
            byte[] payouttmpcmd = new byte[4] { 0x00,0x00,0x00, 0x00 };
            payouttmpcmd[0] = CurrenzaC2Blue_ICTA7V7.PayOutCoins[0];
            payouttmpcmd[1] = CurrenzaC2Blue_ICTA7V7.PayOutCoins[1];
            payouttmpcmd[2] = (byte)(PayOutSum * 2 & 0xff);
            payouttmpcmd[3] = (byte)((payouttmpcmd[0] + payouttmpcmd[1] + payouttmpcmd[2]) & 0xff);
            return payouttmpcmd;
        }

    }
}

using System;
using System.Drawing;
using System.Web;
using System.Linq;
using System.Text;
using System.Data.EntityClient;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using TwoFactorAuthNet;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.Data.ConnectionUI;

namespace VendingServerInitialSetup
{
    public partial class Form1 : Form
    {
        TwoFactorAuth tfa = null;
        string otpsecret = "";
        bool newsettings = true;
        string sqlconnstr = "";
        string entityconnstr = "";
        string entityMetadata = @"res://*/SetupModel.csdl|res://*/SetupModel.ssdl|res://*/SetupModel.msl";

        public Form1()
        {
            InitializeComponent();
        }

        private bool _smtptestok = false;

        private bool SMTPTestOK
        {
            get { return _smtptestok; }
            set
            {
                wizardPage4.AllowNext = value;
                _smtptestok = value;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SMTPTestOK = false;
            bool connOK = false;
            bool exitapp = false;
            DateTime dt = DateTime.Now;
            long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
            string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
            while (!connOK)
            {
                try
                {
                    EntityConnectionStringBuilder csb = new EntityConnectionStringBuilder();
                    csb.Metadata = entityMetadata;
                    csb.Provider = "System.Data.SqlClient";
                    csb.ProviderConnectionString = sqlconnstr;
                    using (VendingEntities dc = (sqlconnstr == "") ? new VendingEntities() : new VendingEntities(csb.ToString()))
                    {
                        using (var dbContextTransaction = dc.Database.BeginTransaction())
                        {
                            connOK = true;
                            if (sqlconnstr != "")
                            {
                                entityconnstr = csb.ToString();
                                var appconfigFile = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                                var settings = appconfigFile.ConnectionStrings;
                                settings.ConnectionStrings["VendingServerInitialSetup.Properties.Settings.VendingConnectionString"].ConnectionString = sqlconnstr;
                                settings.ConnectionStrings["VendingEntities"].ConnectionString = entityconnstr;
                                appconfigFile.Save(System.Configuration.ConfigurationSaveMode.Modified);
                                System.Configuration.ConfigurationManager.RefreshSection(appconfigFile.AppSettings.SectionInformation.Name);
                                OpenFileDialog ofd = new OpenFileDialog()
                                {
                                    AddExtension = true,
                                    Filter = "config files | web.config",
                                    FilterIndex = 0,
                                    Title = "Укажите файл конфигурации сервера (обычно располагается в корне сайта)",
                                    InitialDirectory = "C:\\inetpub\\wwwroot"
                                };
                                if (ofd.ShowDialog() == DialogResult.OK && ofd.CheckFileExists)
                                {
                                    var config = XDocument.Load(ofd.FileName);
                                    var targetNode = config.Root.Element("connectionStrings").Element("add").Attribute("connectionString");
                                    targetNode.Value = string.Concat("metadata=res://*/App_Code.VendingModel.csdl|res://*/App_Code.VendingModel.ssdl|res://*/App_Code.VendingModel.msl;provider=System.Data.SqlClient;provider connection string=\"", sqlconnstr, ";MultipleActiveResultSets=True;App=EntityFramework\"");
                                    config.Save(ofd.FileName);
                                }
                            }
                            WebSettings extmpws = null;
                            try
                            {
                                extmpws = dc.WebSettings.First();
                            }
                            catch
                            {
                                newsettings = true;
                                break;
                            }
                            Accounts extmpacc = dc.Accounts.First(x => x.UserID == extmpws.AdminEmail);
                            DialogResult tmpres = MessageBox.Show("Редактировать существующие настройки? Ответ \"Нет\" удалит их полностью, \"Отмена\" - выход из приложения", "В базе данных найдены настройки!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                            switch (tmpres)
                            {
                                case DialogResult.Yes:
                                    {
                                        newsettings = false;
                                        adminemailtextbox.Text = extmpws.AdminEmail;
                                        bingmapsapikeytextbox.Text = extmpws.BingMapsAPIKey;
                                        fromemailtextbox.Text = extmpws.MailFromAddress;
                                        sendernametextbox.Text = extmpws.EMailDisplayName;
                                        smtpusernametextbox.Text = extmpws.MailLogin;
                                        smtppasswordtextbox.Text = extmpws.MailPassword;
                                        smtpusesslcheckbox.Checked = extmpws.SMTPUseSSL;
                                        userregistersubjtextbox.Text = extmpws.RegAccountMailSubject;
                                        devregistersubjtextbox.Text = extmpws.RegDeviceMailSubject;
                                        serverendpointtextbox.Text = extmpws.ServerEndPoint;
                                        sitenametextbox.Text = extmpws.SiteName;
                                        smtphosttextbox.Text = extmpws.SMTPHost;
                                        smtpporttextbox.Text = extmpws.SMTPPort.ToString();
                                        maillogincheckbox.Checked = extmpws.MailUseSMTPAuth;
                                        adminemailtextbox.Text = extmpacc.UserID;
                                        otpsecret = extmpacc.TOTPSecret;
                                        break;
                                    }
                                case DialogResult.No:
                                    {
                                        newsettings = true;
                                        dc.Accounts.Remove(extmpacc);
                                        dc.WebSettings.Remove(extmpws);
                                        SystemLog tmplog = new SystemLog()
                                        {
                                            DateTime = cdt,
                                            DateTimeStr = cdtstr,
                                            EventText = "Настройки удалены",
                                            Description = "",
                                            UserID = "Local administrator",
                                            IPAddress = "localhost"
                                        };
                                        dc.SystemLog.Add(tmplog);
                                        dc.SaveChanges();
                                        dbContextTransaction.Commit();
                                        break;
                                    }
                                case DialogResult.Cancel:
                                    {
                                        exitapp = true;
                                        break;
                                    }
                                default: break;
                            }
                        }
                    }
                }
                catch (System.Data.Entity.Core.EntityException)
                {
                    DataConnectionDialog dcd = new DataConnectionDialog();
                    dcd.DataSources.Add(DataSource.SqlDataSource);
                    DialogResult tmpdr = DataConnectionDialog.Show(dcd);
                    if (tmpdr == DialogResult.OK)
                    {
                        sqlconnstr = dcd.ConnectionString;
                    }
                    if (tmpdr == DialogResult.Cancel)
                    {
                        exitapp = true;
                        break;
                    }
                }
            }
            if (exitapp) Application.Exit();
        }

        private void LoadSettings()
        {
            
        }

        private void Page1_TextChanged(object sender, EventArgs e)
        {
            wizardPage1.AllowNext = (adminpassrepeattextbox.Text != "" && adminpasstextbox.TextLength > 5 && adminpasstextbox.Text == adminpassrepeattextbox.Text);
        }

        private string TryGetDataConnectionStringFromUser()
        {
            string res = "";
            try
            {
                DataConnectionDialog dcd = new DataConnectionDialog();
                DataConnectionConfiguration dcs = new DataConnectionConfiguration(null);
                dcs.LoadConfiguration(dcd);
                if (DataConnectionDialog.Show(dcd) == DialogResult.OK)
                {
                    res = dcd.ConnectionString;
                }
            }
            catch
            {
                res = "";
            }
            return res;
        }

        private void wizardPage2_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
        {
            if (!wizardPage2.AllowNext)
            {
                tfa = new TwoFactorAuth("Vending control system");
                if (otpsecret == "") otpsecret = tfa.CreateSecret(160);
                var pic = Convert.FromBase64String(tfa.GetQrCodeImageAsDataUri(adminemailtextbox.Text, otpsecret, 150).Substring(22));
                Image image = Image.FromStream(new MemoryStream(pic));
                otpsecretpicture.Image = image;
            }
        }

        private void checkotpbutton_Click(object sender, EventArgs e)
        {
            if (tfa.VerifyCode(otpsecret, otptextbox.Text))
            {
                MessageBox.Show("Двухфакторная авторизация успешно настроена.", "ОК", MessageBoxButtons.OK, MessageBoxIcon.Information);
                wizardPage2.AllowNext = true;
                checkotpbutton.Enabled = false;
                otpsecretpicture.Image = null;
            } else
            {
                MessageBox.Show("НЕПРАВИЛЬНЫЙ ОДНОРАЗОВЫЙ ПАРОЛЬ!!!", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            otptextbox.Text = "";
        }

        private void Page3_TextChanged(object sender, EventArgs e)
        {
            wizardPage3.AllowNext = (sitenametextbox.Text != "" && serverendpointtextbox.Text != "" && sendernametextbox.Text != "" && bingmapsapikeytextbox.Text != "");
        }

        private void Page4_TextChanged(object sender, EventArgs e)
        {
            SMTPTestOK = false;
            testsmtpsettingsbutton.Enabled = (fromemailtextbox.Text != "" && devregistersubjtextbox.Text != "" && userregistersubjtextbox.Text != "" && smtphosttextbox.Text != "" && smtpporttextbox.Text != "" && (!maillogincheckbox.Checked || (maillogincheckbox.Checked && smtpusernametextbox.Text != "" && smtppasswordtextbox.Text != "")));
        }

        private void FinishWizard(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            using (VendingEntities dc = (sqlconnstr == "") ? new VendingEntities() : new VendingEntities(entityconnstr))
            {
                using (var dbContextTransaction = dc.Database.BeginTransaction())
                {
                    DateTime dt = DateTime.Now;
                    long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                    SHA512 shaM = new SHA512Managed();
                    byte[] HashedPsssword = shaM.ComputeHash(Encoding.UTF8.GetBytes(adminpasstextbox.Text));
                    if (!SetFolderPermission(chartdirtextbox.Text))
                    {
                        MessageBox.Show(@"Невозможно установить разрешения 'FULL ACCESS' для пользователя 'IIS AppPool\DefaultAppPool' на каталог '" + chartdirtextbox.Text + "'. Необходимо сделать это вручную, в противном случае на сайте не будут показываться графики.", "Установка разрешений на каталог", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    try
                    {
                        CspParameters cspParams = new CspParameters
                        {
                            ProviderType = 1,
                            Flags = CspProviderFlags.UseArchivableKey,
                            KeyNumber = (int)KeyNumber.Exchange
                        };
                        RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(2048, cspParams);
                        CryptoKeys tmpkeys = null;
                        try
                        {
                            tmpkeys = dc.CryptoKeys.First();
                            MessageBox.Show("Ключевая пара присутствует в БД, генерация ключей ОТМЕНЕНА! Для генерации новой пары ключей необходимо вручную удалить старые из базы данных.", "Генерация ключей", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch
                        {
                            //we will generate new keys only if no keypair exists in database
                            tmpkeys = new CryptoKeys()
                            {
                                PrivateKey = rsaProvider.ExportCspBlob(true),
                                PublicKey = rsaProvider.ExportCspBlob(false)
                            };
                            dc.CryptoKeys.Add(tmpkeys);
                            dc.SaveChanges();
                            MessageBox.Show("Сгенерированы новые ключи. Необходимо изменить вручную открытый ключ сервера в настройках RpiVendApp. На устройствах со старыми ключами ПО должно быть обновлено для продолжения работы.", "Генерация ключей", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            File.WriteAllText("pubkey.txt", Convert.ToBase64String(tmpkeys.PublicKey));
                            System.Diagnostics.Process.Start("pubkey.txt");
                        }
                        if (newsettings)
                        {
                            Accounts tmpacc = new Accounts()
                            {
                                UserID = adminemailtextbox.Text,
                                Valid = true,
                                TOTPSecret = otpsecret,
                                DefaultContactPhone = "",
                                DeviceCountLimit = 999,
                                LicenseContent = "",
                                PaidTillDateTime = 99999999,
                                PaidTillDateTimeStr = "99999999",
                                RegistrationDateTime = cdt,
                                RegistrationDateTimeStr = cdtstr,
                                PasswordHash = Convert.ToBase64String(HashedPsssword),
                                Suspended = false
                            };
                            dc.Accounts.Add(tmpacc);
                            int tmpport = Convert.ToInt32(smtpporttextbox.Text);
                            WebSettings tmpws = new WebSettings()
                            {
                                AdminEmail = adminemailtextbox.Text,
                                BingMapsAPIKey = bingmapsapikeytextbox.Text,
                                MailFromAddress = fromemailtextbox.Text,
                                LastSavedDateTimeStr = cdtstr,
                                EMailDisplayName = sendernametextbox.Text,
                                MailLogin = smtpusernametextbox.Text,
                                MailPassword = smtppasswordtextbox.Text,
                                MailUseSMTPAuth = smtpusesslcheckbox.Checked,
                                RegAccountMailSubject = userregistersubjtextbox.Text,
                                RegDeviceMailSubject = devregistersubjtextbox.Text,
                                ServerEndPoint = serverendpointtextbox.Text,
                                SiteName = sitenametextbox.Text,
                                SMTPHost = smtphosttextbox.Text,
                                SMTPPort = tmpport,
                                SMTPUseSSL = smtpusesslcheckbox.Checked
                            };
                            SystemLog tmplog = new SystemLog()
                            {
                                DateTime = cdt,
                                DateTimeStr = cdtstr,
                                EventText = "начальная настройка завершена",
                                Description = "",
                                UserID = "Local administrator",
                                IPAddress = "localhost"
                            };
                            dc.SystemLog.Add(tmplog);
                            dc.WebSettings.Add(tmpws);
                            dc.SaveChanges();
                        } else
                        {
                            WebSettings extmpws = dc.WebSettings.First();
                            Accounts extmpacc = dc.Accounts.First(x => x.UserID == extmpws.AdminEmail);
                            extmpacc.UserID = adminemailtextbox.Text;
                            extmpacc.Valid = true;
                            extmpacc.TOTPSecret = otpsecret;
                            extmpacc.RegistrationDateTime = cdt;
                            extmpacc.RegistrationDateTimeStr = cdtstr;
                            extmpacc.PasswordHash = Convert.ToBase64String(HashedPsssword);
                            int tmpport = Convert.ToInt32(smtpporttextbox.Text);
                            extmpws.AdminEmail = adminemailtextbox.Text;
                            extmpws.BingMapsAPIKey = bingmapsapikeytextbox.Text;
                            extmpws.MailFromAddress = fromemailtextbox.Text;
                            extmpws.LastSavedDateTimeStr = cdtstr;
                            extmpws.EMailDisplayName = sendernametextbox.Text;
                            extmpws.MailLogin = smtpusernametextbox.Text;
                            extmpws.MailPassword = smtppasswordtextbox.Text;
                            extmpws.MailUseSMTPAuth = smtpusesslcheckbox.Checked;
                            extmpws.RegAccountMailSubject = userregistersubjtextbox.Text;
                            extmpws.RegDeviceMailSubject = devregistersubjtextbox.Text;
                            extmpws.ServerEndPoint = serverendpointtextbox.Text;
                            extmpws.SiteName = sitenametextbox.Text;
                            extmpws.SMTPHost = smtphosttextbox.Text;
                            extmpws.SMTPPort = tmpport;
                            extmpws.SMTPUseSSL = smtpusesslcheckbox.Checked;
                            dc.SaveChanges();
                            SystemLog tmplog = new SystemLog()
                            {
                                DateTime = cdt,
                                DateTimeStr = cdtstr,
                                EventText = "Настройки изменены и сохранены",
                                Description = "",
                                UserID = "Local administrator",
                                IPAddress = "localhost"
                            };
                            dc.SystemLog.Add(tmplog);
                            dc.SaveChanges();
                        }
                        dbContextTransaction.Commit();
                        MessageBox.Show("Настройки сохранены. Программа будет закрыта.", "ОК", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        MessageBox.Show("Ошибка сохранения настроек. Программа будет закрыта." + Environment.NewLine + "Exception: " + ex.Message + Environment.NewLine + "Inner exception: " + ex.InnerException?.Message, "FAIL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void newotpsecretbutton_Click(object sender, EventArgs e)
        {
            DialogResult tmpres = MessageBox.Show("Существующий секрет двухфакторной авторизации для администратора сайта будет перезаписан. Продолжить?", "Внимание!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (tmpres == DialogResult.Yes)
            {
                tfa = new TwoFactorAuth("Vending control system");
                otpsecret = tfa.CreateSecret(160);
                var pic = Convert.FromBase64String(tfa.GetQrCodeImageAsDataUri(adminemailtextbox.Text, otpsecret, 150).Substring(22));
                Image image = Image.FromStream(new System.IO.MemoryStream(pic));
                otpsecretpicture.Image = image;
                wizardPage2.AllowNext = false;
                checkotpbutton.Enabled = true;
            }
        }

        private void maillogincheckbox_CheckedChanged(object sender, EventArgs e)
        {
            label13.Enabled = maillogincheckbox.Checked;
            label14.Enabled = maillogincheckbox.Checked;
            smtpusernametextbox.Enabled = maillogincheckbox.Checked;
            smtppasswordtextbox.Enabled = maillogincheckbox.Checked;
        }

        public bool SetFolderPermission(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var directoryInfo = new DirectoryInfo(folderPath);
                var directorySecurity = directoryInfo.GetAccessControl();
                //если папка существует, удаляем существующие разрешения для пула IIS
                foreach (FileSystemAccessRule item in directorySecurity.GetAuditRules(true, false, typeof(System.Security.Principal.NTAccount)))
                {
                    if (item.IdentityReference.Value == @"IIS AppPool\DefaultAppPool")
                    {
                        directorySecurity.RemoveAccessRule(item);
                    }
                }
                //создаем новое правило доступа ФС
                var fileSystemRule = new FileSystemAccessRule(@"IIS AppPool\DefaultAppPool",
                                                              FileSystemRights.FullControl,
                                                              InheritanceFlags.ObjectInherit |
                                                              InheritanceFlags.ContainerInherit,
                                                              PropagationFlags.InheritOnly,
                                                              AccessControlType.Allow);

                directorySecurity.AddAccessRule(fileSystemRule);
                directoryInfo.SetAccessControl(directorySecurity);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException?.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return false;
        }

        private async void testsmtpsettingsbutton_Click(object sender, EventArgs e)
        {
            wizardPage4.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            Exception ex = null;
            await Task.Run(() =>
            {
                try
                {
                    MailMessage testmail = new MailMessage();
                    testmail.To.Add(adminemailtextbox.Text);
                    testmail.From = new MailAddress(fromemailtextbox.Text, sendernametextbox.Text, Encoding.UTF8);
                    testmail.Subject = "Тестовое сообщение";
                    testmail.SubjectEncoding = Encoding.UTF8;
                    testmail.Body = "Добрый день, уважаемый\\ая сэр\\мадам.<br> Это тестовое сообщение для проверки корректности настроек SMTP-клиента.";
                    testmail.BodyEncoding = Encoding.UTF8;
                    testmail.IsBodyHtml = true;
                    testmail.Priority = MailPriority.High;
                    SmtpClient client = new SmtpClient
                    {
                        UseDefaultCredentials = !maillogincheckbox.Checked,
                        Port = Convert.ToInt32(smtpporttextbox.Text),
                        Host = smtphosttextbox.Text,
                        EnableSsl = smtpusesslcheckbox.Checked,
                    };
                    if (!client.UseDefaultCredentials) client.Credentials = new System.Net.NetworkCredential(smtpusernametextbox.Text, smtppasswordtextbox.Text);
                    client.Send(testmail);
                    SMTPTestOK = true;
                }
                catch (Exception exx)
                {
                    ex = exx;
                }
            });
            if (SMTPTestOK)
            {
                MessageBox.Show("Проверка настроек SMTP завершена, проверьте входящие письма в ящике  " + adminemailtextbox.Text, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                MessageBox.Show("Проверка SMTP настроек завершена с ошибкой:" + Environment.NewLine + ex?.Message + Environment.NewLine + ex?.InnerException?.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.AppStarting;
            wizardPage4.Enabled = true;
        }

        private void Setchartsdirbutton_Click(object sender, EventArgs e)
        {
            if (chartsdirbrowser.ShowDialog() == DialogResult.OK && chartsdirbrowser.SelectedPath != "")
            {
                chartdirtextbox.Text = chartsdirbrowser.SelectedPath;
            }
        }
    }
}

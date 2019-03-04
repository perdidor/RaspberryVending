using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using TwoFactorAuthNet;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Security.Cryptography;

namespace VendingServerInitialSetup
{
    public partial class Form1 : Form
    {
        TwoFactorAuth tfa = null;
        string otpsecret = "";
        bool newsettings = true;

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
            DateTime dt = DateTime.Now;
            long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
            string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
            using (VendingEntities dc = new VendingEntities())
            {
                using (var dbContextTransaction = dc.Database.BeginTransaction())
                {
                    try
                    {
                        WebSettings extmpws = dc.WebSettings.First();
                        Accounts extmpacc = dc.Accounts.First(x => x.UserID == extmpws.AdminEmail);
                        DialogResult tmpres = MessageBox.Show("Edit existing settings? Answering \"No\" will completely delete them, \"Cancel\" to stop operation and exit", "Settings found in DB!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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
                                        EventText = "Removed existing settings",
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
                                    Application.Exit();
                                    break;
                                }
                            default: break;
                        }
                    }
                    catch
                    {
                        newsettings = true;
                    }
                }
            }
        }

        private void LoadSettings()
        {
            
        }

        private void Page1_TextChanged(object sender, EventArgs e)
        {
            wizardPage1.AllowNext = (adminpassrepeattextbox.Text != "" && adminpasstextbox.TextLength > 5 && adminpasstextbox.Text == adminpassrepeattextbox.Text);
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
                MessageBox.Show("Two-factor authentication setup complete. Proceed to next page.", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                wizardPage2.AllowNext = true;
                checkotpbutton.Enabled = false;
                otpsecretpicture.Image = null;
            } else
            {
                MessageBox.Show("WRONG ONE-TIME CODE!!!", "FAIL", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
            using (VendingEntities dc = new VendingEntities())
            {
                using (var dbContextTransaction = dc.Database.BeginTransaction())
                {
                    DateTime dt = DateTime.Now;
                    long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                    SHA512 shaM = new SHA512Managed();
                    byte[] HashedPsssword = shaM.ComputeHash(Encoding.UTF8.GetBytes(adminpasstextbox.Text));
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
                            MessageBox.Show("KeyPair exists, no new key generation wil performed! You have to manually delete keypair from Database to generate new one.", "Key generation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show("New keypair generated. You have to manually add server's public key to RpiVendApp project's global variables. Devices with old server key must be updated to keep functioning.", "Key generation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            SaveFileDialog sfd = new SaveFileDialog()
                            {
                                AddExtension = true,
                                Filter = "Text File | *.txt",
                                FilterIndex = 0,
                                FileName = "pubkey.txt",
                                OverwritePrompt = true,
                                Title = "Select file to save server's public key",
                                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                            };
                            if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
                            {
                                File.WriteAllText(sfd.FileName, Convert.ToBase64String(tmpkeys.PublicKey));
                            }
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
                                EventText = "Initial setup complete",
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
                                EventText = "System settings changed",
                                Description = "",
                                UserID = "Local administrator",
                                IPAddress = "localhost"
                            };
                            dc.SystemLog.Add(tmplog);
                            dc.SaveChanges();
                        }
                        dbContextTransaction.Commit();
                        MessageBox.Show("Settings updated. Program will be closed.", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        MessageBox.Show("Settings NOT updated. Program will be closed." + Environment.NewLine + "Exception: " + ex.Message + Environment.NewLine + "Inner exception: " + ex.InnerException?.Message, "FAIL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void newotpsecretbutton_Click(object sender, EventArgs e)
        {
            DialogResult tmpres = MessageBox.Show("Existing 2FA secret will be rewritten, you have to scan QR code again. Proceed?", "Warning!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
                MessageBox.Show("SMTP test passed OK, check your inbox at " + adminemailtextbox.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                MessageBox.Show("SMTP test FAILED with error:" + Environment.NewLine + ex?.Message + Environment.NewLine + ex?.InnerException?.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.AppStarting;
            wizardPage4.Enabled = true;
        }
    }
}

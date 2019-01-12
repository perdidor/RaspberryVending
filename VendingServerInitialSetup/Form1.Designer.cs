namespace VendingServerInitialSetup
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.wizardPage1 = new AeroWizard.WizardPage();
            this.adminpassrepeattextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.adminpasstextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.adminemailtextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.wizardPage2 = new AeroWizard.WizardPage();
            this.newotpsecretbutton = new System.Windows.Forms.Button();
            this.checkotpbutton = new System.Windows.Forms.Button();
            this.otptextbox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.otpsecretpicture = new System.Windows.Forms.PictureBox();
            this.wizardPage3 = new AeroWizard.WizardPage();
            this.bingmapsapikeytextbox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sendernametextbox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.serverendpointtextbox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.sitenametextbox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.wizardPage4 = new AeroWizard.WizardPage();
            this.smtppasswordtextbox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.maillogincheckbox = new System.Windows.Forms.CheckBox();
            this.smtpusernametextbox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.smtpusesslcheckbox = new System.Windows.Forms.CheckBox();
            this.smtpporttextbox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.smtphosttextbox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.userregistersubjtextbox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.devregistersubjtextbox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.fromemailtextbox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.wizardPage1.SuspendLayout();
            this.wizardPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.otpsecretpicture)).BeginInit();
            this.wizardPage3.SuspendLayout();
            this.wizardPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.BackColor = System.Drawing.Color.White;
            this.wizardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Pages.Add(this.wizardPage1);
            this.wizardControl1.Pages.Add(this.wizardPage2);
            this.wizardControl1.Pages.Add(this.wizardPage3);
            this.wizardControl1.Pages.Add(this.wizardPage4);
            this.wizardControl1.Size = new System.Drawing.Size(580, 354);
            this.wizardControl1.TabIndex = 7;
            this.wizardControl1.Title = "setup wizard";
            // 
            // wizardPage1
            // 
            this.wizardPage1.AllowNext = false;
            this.wizardPage1.Controls.Add(this.adminpassrepeattextbox);
            this.wizardPage1.Controls.Add(this.label3);
            this.wizardPage1.Controls.Add(this.adminpasstextbox);
            this.wizardPage1.Controls.Add(this.label2);
            this.wizardPage1.Controls.Add(this.adminemailtextbox);
            this.wizardPage1.Controls.Add(this.label1);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(533, 200);
            this.wizardPage1.TabIndex = 0;
            this.wizardPage1.Text = "Step 1 of 4: administrator credentials";
            // 
            // adminpassrepeattextbox
            // 
            this.adminpassrepeattextbox.Location = new System.Drawing.Point(290, 112);
            this.adminpassrepeattextbox.Name = "adminpassrepeattextbox";
            this.adminpassrepeattextbox.Size = new System.Drawing.Size(230, 23);
            this.adminpassrepeattextbox.TabIndex = 5;
            this.adminpassrepeattextbox.UseSystemPasswordChar = true;
            this.adminpassrepeattextbox.TextChanged += new System.EventHandler(this.Page1_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Repeat admin password: ";
            // 
            // adminpasstextbox
            // 
            this.adminpasstextbox.Location = new System.Drawing.Point(290, 73);
            this.adminpasstextbox.Name = "adminpasstextbox";
            this.adminpasstextbox.Size = new System.Drawing.Size(230, 23);
            this.adminpasstextbox.TabIndex = 3;
            this.adminpasstextbox.UseSystemPasswordChar = true;
            this.adminpasstextbox.TextChanged += new System.EventHandler(this.Page1_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(238, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Admin password (database will store hash): ";
            // 
            // adminemailtextbox
            // 
            this.adminemailtextbox.Location = new System.Drawing.Point(290, 34);
            this.adminemailtextbox.Name = "adminemailtextbox";
            this.adminemailtextbox.Size = new System.Drawing.Size(230, 23);
            this.adminemailtextbox.TabIndex = 1;
            this.adminemailtextbox.TextChanged += new System.EventHandler(this.Page1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(253, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Admin e-mail address (will be used as UserID): ";
            // 
            // wizardPage2
            // 
            this.wizardPage2.AllowNext = false;
            this.wizardPage2.Controls.Add(this.newotpsecretbutton);
            this.wizardPage2.Controls.Add(this.checkotpbutton);
            this.wizardPage2.Controls.Add(this.otptextbox);
            this.wizardPage2.Controls.Add(this.textBox1);
            this.wizardPage2.Controls.Add(this.otpsecretpicture);
            this.wizardPage2.Name = "wizardPage2";
            this.wizardPage2.Size = new System.Drawing.Size(533, 200);
            this.wizardPage2.TabIndex = 1;
            this.wizardPage2.Text = "Step 2 of 4: enforced security";
            this.wizardPage2.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.wizardPage2_Initialize);
            // 
            // newotpsecretbutton
            // 
            this.newotpsecretbutton.Location = new System.Drawing.Point(14, 105);
            this.newotpsecretbutton.Name = "newotpsecretbutton";
            this.newotpsecretbutton.Size = new System.Drawing.Size(154, 23);
            this.newotpsecretbutton.TabIndex = 4;
            this.newotpsecretbutton.Text = "Change 2FA Secret";
            this.newotpsecretbutton.UseVisualStyleBackColor = true;
            this.newotpsecretbutton.Click += new System.EventHandler(this.newotpsecretbutton_Click);
            // 
            // checkotpbutton
            // 
            this.checkotpbutton.Location = new System.Drawing.Point(138, 152);
            this.checkotpbutton.Name = "checkotpbutton";
            this.checkotpbutton.Size = new System.Drawing.Size(88, 23);
            this.checkotpbutton.TabIndex = 3;
            this.checkotpbutton.Text = "Check Code";
            this.checkotpbutton.UseVisualStyleBackColor = true;
            this.checkotpbutton.Click += new System.EventHandler(this.checkotpbutton_Click);
            // 
            // otptextbox
            // 
            this.otptextbox.Location = new System.Drawing.Point(14, 152);
            this.otptextbox.Name = "otptextbox";
            this.otptextbox.Size = new System.Drawing.Size(118, 23);
            this.otptextbox.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(11, 14);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(314, 85);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // otpsecretpicture
            // 
            this.otpsecretpicture.Location = new System.Drawing.Point(358, 25);
            this.otpsecretpicture.Name = "otpsecretpicture";
            this.otpsecretpicture.Size = new System.Drawing.Size(150, 150);
            this.otpsecretpicture.TabIndex = 0;
            this.otpsecretpicture.TabStop = false;
            // 
            // wizardPage3
            // 
            this.wizardPage3.AllowNext = false;
            this.wizardPage3.Controls.Add(this.bingmapsapikeytextbox);
            this.wizardPage3.Controls.Add(this.label7);
            this.wizardPage3.Controls.Add(this.sendernametextbox);
            this.wizardPage3.Controls.Add(this.label6);
            this.wizardPage3.Controls.Add(this.serverendpointtextbox);
            this.wizardPage3.Controls.Add(this.label5);
            this.wizardPage3.Controls.Add(this.sitenametextbox);
            this.wizardPage3.Controls.Add(this.label4);
            this.wizardPage3.Name = "wizardPage3";
            this.wizardPage3.Size = new System.Drawing.Size(533, 200);
            this.wizardPage3.TabIndex = 2;
            this.wizardPage3.Text = "Step 3 of 4: system-wide variables";
            // 
            // bingmapsapikeytextbox
            // 
            this.bingmapsapikeytextbox.Location = new System.Drawing.Point(191, 124);
            this.bingmapsapikeytextbox.Name = "bingmapsapikeytextbox";
            this.bingmapsapikeytextbox.Size = new System.Drawing.Size(324, 23);
            this.bingmapsapikeytextbox.TabIndex = 7;
            this.bingmapsapikeytextbox.TextChanged += new System.EventHandler(this.Page3_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 127);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 15);
            this.label7.TabIndex = 6;
            this.label7.Text = "Bing maps API key: ";
            // 
            // sendernametextbox
            // 
            this.sendernametextbox.Location = new System.Drawing.Point(191, 85);
            this.sendernametextbox.Name = "sendernametextbox";
            this.sendernametextbox.Size = new System.Drawing.Size(324, 23);
            this.sendernametextbox.TabIndex = 5;
            this.sendernametextbox.TextChanged += new System.EventHandler(this.Page3_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(158, 15);
            this.label6.TabIndex = 4;
            this.label6.Text = "E-mail sender display name: ";
            // 
            // serverendpointtextbox
            // 
            this.serverendpointtextbox.Location = new System.Drawing.Point(191, 47);
            this.serverendpointtextbox.Name = "serverendpointtextbox";
            this.serverendpointtextbox.Size = new System.Drawing.Size(324, 23);
            this.serverendpointtextbox.TabIndex = 3;
            this.serverendpointtextbox.TextChanged += new System.EventHandler(this.Page3_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(175, 15);
            this.label5.TabIndex = 2;
            this.label5.Text = "Server endpoint (IP or domain): ";
            // 
            // sitenametextbox
            // 
            this.sitenametextbox.Location = new System.Drawing.Point(191, 11);
            this.sitenametextbox.Name = "sitenametextbox";
            this.sitenametextbox.Size = new System.Drawing.Size(324, 23);
            this.sitenametextbox.TabIndex = 1;
            this.sitenametextbox.TextChanged += new System.EventHandler(this.Page3_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(177, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Site name (e.g. \"super system\"): ";
            // 
            // wizardPage4
            // 
            this.wizardPage4.Controls.Add(this.smtppasswordtextbox);
            this.wizardPage4.Controls.Add(this.label14);
            this.wizardPage4.Controls.Add(this.maillogincheckbox);
            this.wizardPage4.Controls.Add(this.smtpusernametextbox);
            this.wizardPage4.Controls.Add(this.label13);
            this.wizardPage4.Controls.Add(this.smtpusesslcheckbox);
            this.wizardPage4.Controls.Add(this.smtpporttextbox);
            this.wizardPage4.Controls.Add(this.label12);
            this.wizardPage4.Controls.Add(this.smtphosttextbox);
            this.wizardPage4.Controls.Add(this.label11);
            this.wizardPage4.Controls.Add(this.userregistersubjtextbox);
            this.wizardPage4.Controls.Add(this.label10);
            this.wizardPage4.Controls.Add(this.devregistersubjtextbox);
            this.wizardPage4.Controls.Add(this.label9);
            this.wizardPage4.Controls.Add(this.fromemailtextbox);
            this.wizardPage4.Controls.Add(this.label8);
            this.wizardPage4.Name = "wizardPage4";
            this.wizardPage4.Size = new System.Drawing.Size(533, 200);
            this.wizardPage4.TabIndex = 3;
            this.wizardPage4.Text = "Step 4 of 4: SMTP settings";
            this.wizardPage4.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.FinishWizard);
            // 
            // smtppasswordtextbox
            // 
            this.smtppasswordtextbox.Location = new System.Drawing.Point(333, 161);
            this.smtppasswordtextbox.Name = "smtppasswordtextbox";
            this.smtppasswordtextbox.Size = new System.Drawing.Size(133, 23);
            this.smtppasswordtextbox.TabIndex = 23;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(264, 164);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 15);
            this.label14.TabIndex = 22;
            this.label14.Text = "Password: ";
            // 
            // maillogincheckbox
            // 
            this.maillogincheckbox.AutoSize = true;
            this.maillogincheckbox.Checked = true;
            this.maillogincheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.maillogincheckbox.Location = new System.Drawing.Point(93, 130);
            this.maillogincheckbox.Name = "maillogincheckbox";
            this.maillogincheckbox.Size = new System.Drawing.Size(92, 19);
            this.maillogincheckbox.TabIndex = 21;
            this.maillogincheckbox.Text = "SMTP AUTH";
            this.maillogincheckbox.UseVisualStyleBackColor = true;
            // 
            // smtpusernametextbox
            // 
            this.smtpusernametextbox.Location = new System.Drawing.Point(85, 163);
            this.smtpusernametextbox.Name = "smtpusernametextbox";
            this.smtpusernametextbox.Size = new System.Drawing.Size(131, 23);
            this.smtpusernametextbox.TabIndex = 20;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(9, 164);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(66, 15);
            this.label13.TabIndex = 19;
            this.label13.Text = "Username: ";
            // 
            // smtpusesslcheckbox
            // 
            this.smtpusesslcheckbox.AutoSize = true;
            this.smtpusesslcheckbox.Checked = true;
            this.smtpusesslcheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.smtpusesslcheckbox.Location = new System.Drawing.Point(11, 130);
            this.smtpusesslcheckbox.Name = "smtpusesslcheckbox";
            this.smtpusesslcheckbox.Size = new System.Drawing.Size(66, 19);
            this.smtpusesslcheckbox.TabIndex = 18;
            this.smtpusesslcheckbox.Text = "Use SSL";
            this.smtpusesslcheckbox.UseVisualStyleBackColor = true;
            // 
            // smtpporttextbox
            // 
            this.smtpporttextbox.Location = new System.Drawing.Point(333, 94);
            this.smtpporttextbox.Name = "smtpporttextbox";
            this.smtpporttextbox.Size = new System.Drawing.Size(133, 23);
            this.smtpporttextbox.TabIndex = 17;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(264, 97);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(69, 15);
            this.label12.TabIndex = 16;
            this.label12.Text = "SMTP port: ";
            // 
            // smtphosttextbox
            // 
            this.smtphosttextbox.Location = new System.Drawing.Point(85, 94);
            this.smtphosttextbox.Name = "smtphosttextbox";
            this.smtphosttextbox.Size = new System.Drawing.Size(131, 23);
            this.smtphosttextbox.TabIndex = 15;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 97);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(70, 15);
            this.label11.TabIndex = 14;
            this.label11.Text = "SMTP host: ";
            // 
            // userregistersubjtextbox
            // 
            this.userregistersubjtextbox.Location = new System.Drawing.Point(234, 65);
            this.userregistersubjtextbox.Name = "userregistersubjtextbox";
            this.userregistersubjtextbox.Size = new System.Drawing.Size(285, 23);
            this.userregistersubjtextbox.TabIndex = 13;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 68);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(207, 15);
            this.label10.TabIndex = 12;
            this.label10.Text = "New user registered message subject: ";
            // 
            // devregistersubjtextbox
            // 
            this.devregistersubjtextbox.Location = new System.Drawing.Point(234, 36);
            this.devregistersubjtextbox.Name = "devregistersubjtextbox";
            this.devregistersubjtextbox.Size = new System.Drawing.Size(285, 23);
            this.devregistersubjtextbox.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(219, 15);
            this.label9.TabIndex = 10;
            this.label9.Text = "New device registered message subject: ";
            // 
            // fromemailtextbox
            // 
            this.fromemailtextbox.Location = new System.Drawing.Point(234, 7);
            this.fromemailtextbox.Name = "fromemailtextbox";
            this.fromemailtextbox.Size = new System.Drawing.Size(285, 23);
            this.fromemailtextbox.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(126, 15);
            this.label8.TabIndex = 8;
            this.label8.Text = "FROM e-mail address: ";
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(580, 354);
            this.Controls.Add(this.wizardControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vending database initial setup";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            this.wizardPage2.ResumeLayout(false);
            this.wizardPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.otpsecretpicture)).EndInit();
            this.wizardPage3.ResumeLayout(false);
            this.wizardPage3.PerformLayout();
            this.wizardPage4.ResumeLayout(false);
            this.wizardPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage wizardPage1;
        private System.Windows.Forms.TextBox adminpasstextbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox adminemailtextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox adminpassrepeattextbox;
        private System.Windows.Forms.Label label3;
        private AeroWizard.WizardPage wizardPage2;
        private System.Windows.Forms.Button checkotpbutton;
        private System.Windows.Forms.TextBox otptextbox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox otpsecretpicture;
        private AeroWizard.WizardPage wizardPage3;
        private System.Windows.Forms.TextBox serverendpointtextbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sitenametextbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox bingmapsapikeytextbox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox sendernametextbox;
        private System.Windows.Forms.Label label6;
        private AeroWizard.WizardPage wizardPage4;
        private System.Windows.Forms.TextBox smtppasswordtextbox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox maillogincheckbox;
        private System.Windows.Forms.TextBox smtpusernametextbox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox smtpusesslcheckbox;
        private System.Windows.Forms.TextBox smtpporttextbox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox smtphosttextbox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox userregistersubjtextbox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox devregistersubjtextbox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox fromemailtextbox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button newotpsecretbutton;
    }
}


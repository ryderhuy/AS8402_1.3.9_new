using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MessageManager;

namespace Cognex.VS.Utility
{
    /// <summary>
    /// Summary description for FormPasswordPrompt.
    /// </summary>
    public class FormPasswordPrompt : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox textBox_Password;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label_Password;
        private Label label1;
        private ComboBox comboBox_Login;
        private Button btnChangePassword;
        private AccessLevel mCurrentAccessLevel = AccessLevel.Operator;
        private PasswordFile mPasswordFile = null;

        public event EventHandler AccessLevelChanged;
        public event EventHandler PasswordRequireChanged;

        public AccessLevel CurrentAccessLevel
        {
            get { return mCurrentAccessLevel; }
            internal set { mCurrentAccessLevel = value; }
        }

        public FormPasswordPrompt(PasswordFile currentPasswordFile, AccessLevel currentAccessLevel)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            comboBox_Login.Items.Add(new AccessLevel_Localized(AccessLevel.Operator, ResourceUtility.GetString("RtOperator")));
            comboBox_Login.Items.Add(new AccessLevel_Localized(AccessLevel.Supervisor, ResourceUtility.GetString("RtSupervisor")));
            comboBox_Login.Items.Add(new AccessLevel_Localized(AccessLevel.Administrator, ResourceUtility.GetString("RtAdministrator")));
            
            mPasswordFile = currentPasswordFile;
            CurrentAccessLevel = currentAccessLevel;
            comboBox_Login.SelectedIndex = (int)currentAccessLevel;
            foreach (AccessLevel_Localized al in comboBox_Login.Items)
                if (al.val == mCurrentAccessLevel)
                    comboBox_Login.SelectedItem = al;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_Password = new System.Windows.Forms.Label();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_Login = new System.Windows.Forms.ComboBox();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Password
            // 
            this.label_Password.AutoSize = true;
            this.label_Password.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Password.Location = new System.Drawing.Point(13, 46);
            this.label_Password.Name = "label_Password";
            this.label_Password.Size = new System.Drawing.Size(68, 16);
            this.label_Password.TabIndex = 0;
            this.label_Password.Text = "Password";
            // 
            // textBox_Password
            // 
            this.textBox_Password.Location = new System.Drawing.Point(104, 44);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.PasswordChar = '●';
            this.textBox_Password.Size = new System.Drawing.Size(175, 22);
            this.textBox_Password.TabIndex = 0;
            this.textBox_Password.Text = "1";
            this.textBox_Password.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_Password_KeyDown);
            // 
            // button_OK
            // 
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(14, 73);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 27);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "OK";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(105, 73);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 27);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "Cancel";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(37, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "User";
            // 
            // comboBox_Login
            // 
            this.comboBox_Login.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Login.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_Login.Location = new System.Drawing.Point(105, 12);
            this.comboBox_Login.Name = "comboBox_Login";
            this.comboBox_Login.Size = new System.Drawing.Size(175, 24);
            this.comboBox_Login.TabIndex = 42;
            this.comboBox_Login.SelectionChangeCommitted += new System.EventHandler(this.comboBox_Login_SelectionChangeCommitted);
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnChangePassword.Location = new System.Drawing.Point(196, 73);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(84, 27);
            this.btnChangePassword.TabIndex = 43;
            this.btnChangePassword.Text = "Change";
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);
            // 
            // FormPasswordPrompt
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(307, 112);
            this.Controls.Add(this.btnChangePassword);
            this.Controls.Add(this.comboBox_Login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.label_Password);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPasswordPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Password";
            this.Load += new System.EventHandler(this.FormPasswordPrompt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            AccessLevel newAccessLevel = ((AccessLevel_Localized)(comboBox_Login.SelectedItem)).val;
            bool isSuccess = true;
            if (newAccessLevel > mCurrentAccessLevel)
            {
                string expected = mPasswordFile.GetPasswordForAccessLevel(newAccessLevel);
                if (expected != "")
                {
                    this.Text = ResourceUtility.FormatString("RtEnterPassword", newAccessLevel.ToString());
                    string enteredPassword = this.textBox_Password.Text;
                    if (enteredPassword != expected)
                    {
                        isSuccess = false;
                        MessageBox.Show(ResourceUtility.GetString("RtInvalidPassword2"), ResourceUtility.GetString("RtInvalidPassword"));
                    }
                }
            }
            if (isSuccess)
            {
                mCurrentAccessLevel = newAccessLevel;
                this.DialogResult = DialogResult.OK;
                this.Close();
                MessageLoggerManager.Log.Info("[Action] Log-in as " + mCurrentAccessLevel.ToString());
            }
            //else
            //{
            //    this.DialogResult = DialogResult.Cancel;
            //}
        }

        private void FormPasswordPrompt_Load(object sender, System.EventArgs e)
        {
            this.label_Password.Text = ResourceUtility.GetString("RtPassword");
            this.button_OK.Text = ResourceUtility.GetString("RtOK");
            this.button_Cancel.Text = ResourceUtility.GetString("RtCancel");
        }

        private void comboBox_Login_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AccessLevel newAccessLevel = ((AccessLevel_Localized)(comboBox_Login.SelectedItem)).val;
            if (newAccessLevel > AccessLevel.Operator)
            {
                textBox_Password.Enabled = true;
                //textBox_Password.Text = "";
            }
        }

        public void SetCurrentAccessLevel(AccessLevel currentAccessLevel)
        {
            // update gui to reflect current accessLevel
            foreach (AccessLevel_Localized al in comboBox_Login.Items)
                if (al.val == currentAccessLevel)
                    comboBox_Login.SelectedItem = al;
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string expectedPassword = mPasswordFile.GetPasswordForAccessLevel(mCurrentAccessLevel);
            using (FormSetPasswords form = new FormSetPasswords(mPasswordFile,mCurrentAccessLevel, expectedPassword))
            {
                form.ShowDialog(this);
            }
        }

        private void textBox_Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_OK_Click(sender, e);
            }
        }
    }
}

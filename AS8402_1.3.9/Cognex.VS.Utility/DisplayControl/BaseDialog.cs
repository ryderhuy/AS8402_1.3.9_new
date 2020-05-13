using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cognex.VS.Utility
{
    public partial class BaseDialog : CenterBaseForm
    {
        public BaseDialog() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Please implement this function in inherited class
        /// </summary>
        protected virtual void BtnReset_Click()
        {

        }

        protected virtual void BtnOk_Click()
        {

        }

        protected virtual void BtnCancel_Click()
        {
        }

        private void mBtnReset_Click(object sender, EventArgs e)
        {
            BtnReset_Click();
        }

        public void SetTitle(string title)
        {
            Text = title;
        }

        private void MBtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            BtnOk_Click();
        }

        private void MBtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            BtnCancel_Click();
        }
    }
}

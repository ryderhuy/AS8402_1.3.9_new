using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cognex.VS.Utility.DisplayControl
{
    public partial class ExitButton : UserControl
    {
        private Image mBackgroundImage;

        public event EventHandler ButtonClick;

        public ExitButton()
        {
            InitializeComponent();

            pictureBox.Click += Control_Click;
            labelCommand.Click += Control_Click;
            splitContainer1.Click += Control_Click;
            this.Click += Control_Click;
        }

        private void Control_Click(object sender, EventArgs e)
        {
            if (ButtonClick != null)
                ButtonClick(this, EventArgs.Empty);
        }

        [Description("ImageButton"),
        Category("CustomProperty")]
        public Image ImageButton
        {
            get { return mBackgroundImage; }
            set
            {
                if (mBackgroundImage != value)
                {
                    mBackgroundImage = value;
                    if (this.pictureBox != null)
                        pictureBox.BackgroundImage = mBackgroundImage;
                }
            }
        }
    }
}

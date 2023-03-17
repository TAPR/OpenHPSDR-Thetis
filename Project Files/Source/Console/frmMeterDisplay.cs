using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    public partial class frmMeterDisplay : Form
    {
        private Console _console;
        private int _rx;
        private string _id;

        public frmMeterDisplay(Console c, int rx)
        {
            InitializeComponent();

            _id = System.Guid.NewGuid().ToString();
            _console = c;
            _rx = rx;

            setTitle();
        }
        private void setTitle()
        {
            // so each meter title is 'unique'. Useful for streaming software such as OBS
            this.Text = "Thetis Meter [" + Common.FiveDigitHash(_id).ToString("00000") + "]";
        }
        public string ID
        {
            get { return _id; }
            set 
            {
                _id = value;

                Common.RestoreForm(this, "MeterDisplay_" + _id, true);
                Common.ForceFormOnScreen(this);

                setTitle();
            }
        }
        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }

            Common.SaveForm(this, "MeterDisplay_" + _id);
        }

        public void TakeOwner(ucMeter m)
        {
            m.Parent = this;
            m.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            m.Location = new Point(0, 0);            
            m.Size = new Size(this.Width, this.Height);
            m.BringToFront();
            m.Show();
        }
    }
}

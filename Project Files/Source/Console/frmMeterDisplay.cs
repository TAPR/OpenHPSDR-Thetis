using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmMeterDisplay : Form
    {
        private Console _console;
        private int _rx;

        public frmMeterDisplay(Console c, int rx)
        {
            InitializeComponent();

            _console = c;
            _rx = rx;

            _console.MoxChangeHandlers += OnMox;

            setTitle(_console.MOX);
        }
        public PictureBox DisplayContainer
        {
            get { return picContainer; }
        }

        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
            {
                _console.MoxChangeHandlers -= OnMox;
            }
        }
        private void OnMox(int rx, bool oldMox, bool newMox)
        {
            setTitle(newMox);
        }
        private void setTitle(bool mox)
        {
            this.Text = (mox ? "TX " : "RX ") + _rx.ToString();
        }
    }
}

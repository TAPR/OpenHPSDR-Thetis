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
    public partial class frmSeqLog : Form
    {
        public delegate void ClearButtonHandler();
        private event ClearButtonHandler clearButtonEvents;

        public frmSeqLog()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            clearButtonEvents();
        }

        public void LogString(string s)
        {
            if(txtLog.Text.Length > 32000) // some arbitary max lengh
            {
                txtLog.Text = txtLog.Text.Substring(0, 32000);
            }
            txtLog.Text = s + System.Environment.NewLine + txtLog.Text;
        }

        private void frmSeqLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public event ClearButtonHandler ClearButtonEvent {
            add {
                clearButtonEvents += value;
            }
            remove {
                clearButtonEvents -= value;
            }
        }
    }
}

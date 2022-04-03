//=================================================================
// MW0LGE 2022
//=================================================================

using System;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmLog : Form
    {
        private const int MAX_ENTRIES = 500;

        private bool _log = false;
        private Object _lock = new Object();

        private string[] _logLines;

        public frmLog()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _logLines = null;
            }
            txtLog.Lines = _logLines;
        }

        private void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            _log = chkLog.Checked;
            if (_log)
            {
                lock (_lock)
                {
                    _logLines = null; // clear on start
                }
                txtLog.Lines = _logLines;
            }
        }

        public void Log(bool bIn, string sMessage)
        {
            if (!_log) return;

            string[] newLines;

            lock (_lock)
            {
                if (_logLines == null) _logLines = new string[0];

                if (_logLines.Length + 1 > MAX_ENTRIES)
                    newLines = new string[MAX_ENTRIES];
                else
                    newLines = new string[_logLines.Length + 1];

                for (int i = 1; i < newLines.Length; i++)
                {
                    newLines[i] = _logLines[i - 1];
                }

                newLines[0] = (bIn ? "< " : "> ") + sMessage;

                _logLines = newLines;
            }

            if (_log)
                txtLog.Lines = _logLines;
        }

        public void ShowWithTitle(string title)
        {
            this.Text = "Log: " + title;
            this.Show();
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}

//=================================================================
// frmIPv4Picker.cs - MW0LGE 2022
//=================================================================

using System;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace Thetis
{
    public partial class frmIPv4Picker : Form
    {
        private string _ip = "";
        private int _port = -1;
        public frmIPv4Picker()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (_port != -1)
                _ip = comboAddresses.Text + ":" + _port.ToString();
            else
                _ip = comboAddresses.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _ip = "";
        }
        public string IP
        {
            get { return _ip; }
        }

        public void Init(string sIPPort, bool addBroadcast = false)
        {
            _port = -1;
            _ip = "";
            comboAddresses.Items.Clear();

            try
            {
                string[] parts = sIPPort.Split(':');
                string sTmp = "";
                string sPort = "";
                IPAddress address;
                bool bEntries = false;

                if (parts.Length == 1) sTmp = sIPPort;
                else if (parts.Length > 1)
                {
                    sTmp = parts[0];
                    sPort = parts[1];
                    bool bOk = int.TryParse(sPort, out _port);
                }

                bool bOK = IPAddress.TryParse(sTmp, out address);
                if (bOK) sTmp = address.ToString();

                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (/*item.NetworkInterfaceType == NetworkInterfaceType. && */item.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                string sIp = ip.Address.ToString();

                                int n = comboAddresses.Items.Add(sIp);

                                if (sIp == sTmp) comboAddresses.SelectedIndex = n;
                                if (sIp == "255.255.255.255") addBroadcast = false;

                                bEntries = true;
                            }
                        }
                    }
                }

                if (addBroadcast) comboAddresses.Items.Add("255.255.255.255");

                btnSelect.Enabled = bEntries;
            }
            catch
            {
                btnSelect.Enabled = false;
            }
        }
    }
}

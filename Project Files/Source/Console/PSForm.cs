using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Thetis
{
    public unsafe partial class PSForm : Form
    {
        #region constructor

        private Console console;

        public PSForm(Console c)
        {
            InitializeComponent();
            Common.RestoreForm(this, "PureSignal", false);
            console = c;
        }

        #endregion

        #region variables

        private int gcolor = (0xFF << 24) | (0xFF << 8);
        private static bool autoON = false;
        private static bool singlecalON = false;
        private static bool restoreON = false;
        private static bool OFF = true;
        private int oldCalCount = 0;
        private int red, green, blue;
        private int aastate = 0;
        private static double PShwpeak;
        private static double GetPSpeakval;

        public static AmpView ampv = null;
        public static Thread ampvThread = null;

        private int oldCalCount2 = 0;
        private int save_autoON = 0;
        private int save_singlecalON = 0;
        private int deltadB = 0;

        private static int cmdstate = 0;
        private static bool topmost = false;
        #endregion

        #region properties

        private bool dismissAmpv = false;
        public bool DismissAmpv
        {
            get { return dismissAmpv; }
            set
            {
                dismissAmpv = value;
            }
        }

        private static bool psenabled = false;
        public bool PSEnabled
        {
            get { return psenabled; }
            set
            {
                psenabled = value;
                if (psenabled)
                {
                    // Set the system to supply feedback.
                    console.UpdateDDCs(console.RX2Enabled);
                    NetworkIO.SetPureSignal(1);
                    console.UpdateRXADCCtrl();
                    console.UpdateAAudioMixerStates();
                    unsafe { cmaster.LoadRouterControlBit((void*)0, 0, 0, 1); }
                    console.radio.GetDSPTX(0).PSRunCal = true;
                }
                else
                {
                    // Set the system to turn-off feedback.
                    console.UpdateDDCs(console.RX2Enabled);
                    NetworkIO.SetPureSignal(0);
                    console.UpdateRXADCCtrl();
                    console.UpdateAAudioMixerStates();
                    unsafe { cmaster.LoadRouterControlBit((void*)0, 0, 0, 0); }
                    console.radio.GetDSPTX(0).PSRunCal = false;
                }
               // console.EnableDup();
                if (console.path_Illustrator != null)
                    console.path_Illustrator.pi_Changed();
            }
        }

        private static bool autocal_enabled = false;
        public bool AutoCalEnabled
        {
            get { return autocal_enabled; }
            set
            {
                autocal_enabled = value;
                if (autocal_enabled)
                {
                    autoON = true;
                    console.PSState = true;
                }
                else
                {
                    OFF = true;
                    console.PSState = false;
                }
            }
        }

        private static bool autoattenuate = true;
        public bool AutoAttenuate
        {
            get { return autoattenuate; }
            set
            {
                autoattenuate = value;
            }
        }

        private static bool ttgenON = false;
        public bool TTgenON
        {
            get { return ttgenON; }
            set
            {
                ttgenON = value;
                if (ttgenON)
                    btnPSTwoToneGen.BackColor = Color.FromArgb(gcolor);
                else
                    btnPSTwoToneGen.BackColor = SystemColors.Control;
            }
        }

        private static int txachannel = WDSP.id(1, 0);
        public int TXAchannel
        {
            get { return txachannel; }
            set { txachannel = value; }
        }

        private static bool mox = false;
        public bool Mox
        {
            get { return mox; }
            set
            {
                mox = value;
                puresignal.SetPSMox(txachannel, value);
            }
        }

        private int ints = 16;
        public int Ints
        {
            get { return ints; }
            set
            {
                ints = value;
            }
        }

        private int spi = 256;
        public int Spi
        {
            get { return spi; }
            set
            {
                spi = value;
            }
        }

        #endregion

        #region event handlers

        private void PSForm_Load(object sender, EventArgs e)
        {
            if (ttgenON == true)
                btnPSTwoToneGen.BackColor = Color.FromArgb(gcolor);
            fixed (double* ptr = &PShwpeak)
                puresignal.GetPSHWPeak(txachannel, ptr);
            PSpeak.Text = PShwpeak.ToString();
            btnPSAdvanced_Click(this, e);
        }

        private void PSForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (ampv != null)
            {
                dismissAmpv = true;
                ampvThread.Join();
                ampv.Close();
                ampv = null;
            }
            advancedON = true;
            btnPSAdvanced_Click(this, e);
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "PureSignal");
        }

        public void RunAmpv()
        {
            ampv = new AmpView(this);
            Application.Run(ampv);
        }

        private void btnPSAmpView_Click(object sender, EventArgs e)
        {
            if (ampv == null || (ampv != null && ampv.IsDisposed))
            {
                dismissAmpv = false;
                ampvThread = new Thread(RunAmpv);
                ampvThread.SetApartmentState(ApartmentState.STA);
                ampvThread.Name = "Ampv Thread";
                ampvThread.Start();
            }
        }

        private void btnPSCalibrate_Click(object sender, EventArgs e)
        {
            console.ForcePureSignalAutoCalDisable();
            singlecalON = true;
            console.PSState = false;
        }

        //-W2PA Adds capability for CAT control via console
        public void SingleCalrun()
        {
            btnPSCalibrate_Click(this, EventArgs.Empty); 
        }

        private void btnPSReset_Click(object sender, EventArgs e)
        {
            console.ForcePureSignalAutoCalDisable();
            if (!OFF) OFF = true;
            console.PSState = false;
        }

        private void udPSMoxDelay_ValueChanged(object sender, EventArgs e)
        {
            puresignal.SetPSMoxDelay(txachannel, (double)udPSMoxDelay.Value);
        }

        private void udPSCalWait_ValueChanged(object sender, EventArgs e)
        {
            puresignal.SetPSLoopDelay(txachannel, (double)udPSCalWait.Value);
        }

        private void udPSPhnum_ValueChanged(object sender, EventArgs e)
        {
            double actual_delay = puresignal.SetPSTXDelay(txachannel, (double)udPSPhnum.Value * 1.0e-09);
        }

        private void btnPSTwoToneGen_Click(object sender, EventArgs e)
        {
            if (ttgenON == false)
            {
                btnPSTwoToneGen.BackColor = Color.FromArgb(gcolor);
                ttgenON = true;
                console.SetupForm.TTgenrun = true;
            }
            else
            {
                btnPSTwoToneGen.BackColor = SystemColors.Control;
                ttgenON = false;
                console.SetupForm.TTgenrun = false;
            }
        }

        private void btnPSSave_Click(object sender, EventArgs e)
        {
            System.IO.Directory.CreateDirectory(console.AppDataPath + "\\PureSignal\\");
            SaveFileDialog savefile1 = new SaveFileDialog();
            savefile1.InitialDirectory = console.AppDataPath + "PureSignal\\";
            savefile1.RestoreDirectory = true;
            if (savefile1.ShowDialog() == DialogResult.OK)
                puresignal.PSSaveCorr(txachannel, savefile1.FileName);
        }

        private void btnPSRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile1 = new OpenFileDialog();
            openfile1.InitialDirectory = console.AppDataPath + "PureSignal\\";
            openfile1.RestoreDirectory = true;
            if (openfile1.ShowDialog() == DialogResult.OK)
            {
                console.ForcePureSignalAutoCalDisable();
                OFF = false;
                puresignal.PSRestoreCorr(txachannel, openfile1.FileName);
                restoreON = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.TopMost != topmost) 
                this.TopMost  = topmost;
            if(autocal_enabled)
                console.TxtLeftForeColor = Color.FromArgb(0, 255, 0);
            puresignal.getinfo(txachannel);
            lblPSInfo0.Text = puresignal.Info[0].ToString();
            lblPSInfo1.Text = puresignal.Info[1].ToString();
            lblPSInfo2.Text = puresignal.Info[2].ToString();
            lblPSInfo3.Text = puresignal.Info[3].ToString();
            lblPSInfo5.Text = puresignal.Info[5].ToString();
            lblPSInfo6.Text = puresignal.Info[6].ToString();
            lblPSInfo13.Text = puresignal.Info[13].ToString();

            if (puresignal.Info[14] == 1)
            {
                if (puresignal.Info[4] > 90)
                    lblPSInfoCO.BackColor = Color.FromArgb(0, 255, 0);
                else
                    lblPSInfoCO.BackColor = Color.FromArgb(255, 255, 0);
                if (autocal_enabled)
                    if (puresignal.Info[4] > 90)
                        console.TxtRightBackColor = Color.FromArgb(0, 255, 0);
                    else
                        console.TxtRightBackColor = Color.FromArgb(255, 255, 0);
                btnPSSave.Enabled = true;                           // enable Save capability
                btnPSSave.BackColor = SystemColors.Control;
            }
            else
            {
                lblPSInfoCO.BackColor = Color.Black;
                if (autocal_enabled)
                    console.TxtRightBackColor = Color.Black;
                btnPSSave.Enabled = false;
                btnPSSave.Enabled = false;
                btnPSSave.BackColor = SystemColors.ButtonShadow;
            }
            lblPSInfo15.Text = puresignal.Info[15].ToString();

            if (puresignal.Info[5] != oldCalCount)
            {
                oldCalCount = puresignal.Info[5];

                if (puresignal.Info[4] > 181)
                {
                    red = 0; green = 0; blue = 255;
                }
                else if (puresignal.Info[4] > 128)
                {
                    red = 000; green = 255; blue = 000;
                }
                else if (puresignal.Info[4] > 90)
                {
                    red = 255; green = 255; blue = 000;
                }
                else
                {
                    red = 255; green = 000; blue = 000;
                }
            }
            red = Math.Max(0, red - 5);
            green = Math.Max(0, green - 5);
            blue = Math.Max(0, blue - 5);
            Color FBColor = Color.FromArgb(red, green, blue);
            lblPSInfoFB.BackColor = FBColor;
            if (autocal_enabled) 
                console.TxtCenterBackColor = FBColor;
            lblPSfb2.Text = puresignal.Info[4].ToString();

            fixed (double* ptr = &GetPSpeakval)
                puresignal.GetPSMaxTX(txachannel, ptr);
            GetPSpeak.Text = GetPSpeakval.ToString();

            // Command State-Machine
            switch (cmdstate)
            {
                case 0:     // OFF
                    puresignal.SetPSControl(txachannel, 1, 0, 0, 0);
                    if (PSEnabled) PSEnabled = false;
                    btnPSCalibrate.BackColor = SystemColors.Control;
                    if (restoreON)
                        cmdstate = 7;
                    else if (autoON)
                        cmdstate = 1;
                    else if (singlecalON)
                        cmdstate = 3;
                    OFF = false;
                    break;
                case 1:     // Turn-ON Auto-Calibrate Mode
                    puresignal.SetPSControl(txachannel, 1, 0, 1, 0);
                    if (!PSEnabled) PSEnabled = true;
                    btnPSCalibrate.BackColor = SystemColors.Control;
                    cmdstate = 2;
                    break;
                case 2:     // Auto-Calibrate Mode
                    if (OFF)
                        cmdstate = 6;
                    else if (restoreON)
                        cmdstate = 7;
                    else if (singlecalON)
                        cmdstate = 3;
                    break;
                case 3:     // Turn-ON Single-Calibrate Mode
                    autoON = false;
                    puresignal.SetPSControl(txachannel, 1, 1, 0, 0);
                    if (!PSEnabled) PSEnabled = true;
                    btnPSCalibrate.BackColor = Color.FromArgb(gcolor);
                    cmdstate = 4;
                    break;
                case 4:     // Single-Calibrate Mode
                    singlecalON = false;
                    if (OFF)
                        cmdstate = 6;
                    else if (restoreON)
                        cmdstate = 7;
                    else if (autoON)
                        cmdstate = 1;
                    else if (puresignal.Info[14] == 1)
                        cmdstate = 5;
                    break;
                case 5:     // Stay-ON
                    if (PSEnabled) PSEnabled = false;
                    btnPSCalibrate.BackColor = SystemColors.Control;
                    if (OFF)
                        cmdstate = 6;
                    else if (restoreON)
                        cmdstate = 7;
                    else if (autoON)
                        cmdstate = 1;
                    else if (singlecalON)
                        cmdstate = 3;
                    break;
                case 6:     // Turn-OFF
                    autoON = false;
                    puresignal.SetPSControl(txachannel, 1, 0, 0, 0);
                    if (!PSEnabled) PSEnabled = true;
                    btnPSCalibrate.BackColor = SystemColors.Control;
                    OFF = false;
                    if (restoreON)
                        cmdstate = 7;
                    else if (autoON)
                        cmdstate = 1;
                    else if (singlecalON)
                        cmdstate = 3;
                    else if (puresignal.Info[14] == 0 && puresignal.Info[15] == 0)
                        cmdstate = 0;
                    break;
                case 7:     // Initiate Restored Correction
                    autoON = false;
                    puresignal.SetPSControl(txachannel, 0, 0, 0, 1);
                    if (!PSEnabled) PSEnabled = true;
                    btnPSCalibrate.BackColor = SystemColors.Control;
                    restoreON = false;
                    if (puresignal.Info[15]==8)
                        cmdstate = 5;
                    break;
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            bool newcal = (puresignal.Info[5] != oldCalCount2);
            oldCalCount2 = puresignal.Info[5];
            if (autoattenuate && !console.ATTOnTX) console.ATTOnTX = true;
            switch (aastate)
            {
                case 0: // monitor
                    if (autoattenuate && newcal
                        && (puresignal.Info[4] > 181 || (puresignal.Info[4] <= 128 && console.SetupForm.ATTOnTX > 0)))
                    {
                        aastate = 1;
                        double ddB;
                        if (puresignal.Info[4] <= 256)
                        {
                            ddB = 20.0 * Math.Log10((double)puresignal.Info[4] / 152.293);
                            if (Double.IsNaN(ddB)) ddB = 31.1;
                            if (ddB < -100.0) ddB = -100.0;
                            if (ddB > +100.0) ddB = +100.0;
                        }
                        else
                            ddB = 31.1;
                        deltadB = Convert.ToInt32(ddB);
                        if (cmdstate == 2) save_autoON = 1;
                        else               save_autoON = 0;
                        if (cmdstate == 4) save_singlecalON = 1;
                        else               save_singlecalON = 0;
                        puresignal.SetPSControl(txachannel, 1, 0, 0, 0);
                    }
                    break;
                case 1: // set new value
                    aastate = 2;
                    if ((console.SetupForm.ATTOnTX + deltadB) > 0)
                        console.SetupForm.ATTOnTX += deltadB;
                    else
                        console.SetupForm.ATTOnTX = 0;
                    break;
                case 2: // restore operation
                    aastate = 0;
                    puresignal.SetPSControl(txachannel, 0, save_singlecalON, save_autoON, 0);
                    break;
            }
        }

        private void PSpeak_TextChanged(object sender, EventArgs e)
        {
            PShwpeak = Convert.ToDouble(PSpeak.Text);
            puresignal.SetPSHWPeak(txachannel, PShwpeak);
        }

        private void chkPSRelaxPtol_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPSRelaxPtol.Checked)
                puresignal.SetPSPtol(txachannel, 0.400);
            else
                puresignal.SetPSPtol(txachannel, 0.800);
        }

        private void chkPSAutoAttenuate_CheckedChanged(object sender, EventArgs e)
        {
            autoattenuate = chkPSAutoAttenuate.Checked;
        }

        private void checkLoopback_CheckedChanged(object sender, EventArgs e)
        {
            cmaster.PSLoopback = checkLoopback.Checked;
        }

        private void chkPSPin_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPSPin.Checked)
                puresignal.SetPSPinMode(txachannel, 1);
            else
                puresignal.SetPSPinMode(txachannel, 0);
        }

        private void chkPSMap_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPSMap.Checked)
                puresignal.SetPSMapMode(txachannel, 1);
            else
                puresignal.SetPSMapMode(txachannel, 0);
        }

        private void chkPSStbl_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPSStbl.Checked)
                puresignal.SetPSStabilize(txachannel, 1);
            else
                puresignal.SetPSStabilize(txachannel, 0);
        }

        private void comboPSTint_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboPSTint.SelectedIndex)
            {
                case 0:
                    puresignal.SetPSIntsAndSpi(txachannel, 16, 256);
                    ints = 16;
                    spi = 256;
                    btnPSSave.Enabled = btnPSRestore.Enabled = true;
                    break;
                case 1:
                    puresignal.SetPSIntsAndSpi(txachannel, 8, 512);
                    ints = 8;
                    spi = 512;
                    btnPSSave.Enabled = btnPSRestore.Enabled = false;
                    break;
                case 2:
                    puresignal.SetPSIntsAndSpi(txachannel, 4, 1024);
                    ints = 4;
                    spi = 1024;
                    btnPSSave.Enabled = btnPSRestore.Enabled = false;
                    break;
                default:
                    puresignal.SetPSIntsAndSpi(txachannel, 16, 256);
                    ints = 16;
                    spi = 256;
                    btnPSSave.Enabled = btnPSRestore.Enabled = true;
                    break;
            }
        }

        private bool advancedON = true;
        private void btnPSAdvanced_Click(object sender, EventArgs e)
        {
            if (advancedON)
            {
                advancedON = false;
                console.psform.ClientSize = new System.Drawing.Size(560, 60);
            }
            else
            {
                advancedON = true;
                console.psform.ClientSize = new System.Drawing.Size(560, 300);
            }
        }

        private void chkPSOnTop_CheckedChanged(object sender, EventArgs e)
        {
            topmost = chkPSOnTop.Checked;
        }

        #endregion

        #region methods

        public void ForcePS()
        {
            EventArgs e = EventArgs.Empty;
            if (!autoON)
                puresignal.SetPSControl(txachannel, 1, 0, 0, 0);
            else
                puresignal.SetPSControl(txachannel, 0, 0, 1, 0);
            if (!ttgenON)
                WDSP.SetTXAPostGenRun(txachannel, 0);
            else
            {
                WDSP.SetTXAPostGenMode(txachannel, 1);
                WDSP.SetTXAPostGenRun(txachannel, 1);
            }
            udPSCalWait_ValueChanged(this, e);
            udPSPhnum_ValueChanged(this, e);
            udPSMoxDelay_ValueChanged(this, e);
            chkPSRelaxPtol_CheckedChanged(this, e);
            chkPSAutoAttenuate_CheckedChanged(this, e);
            chkPSPin_CheckedChanged(this, e);
            chkPSMap_CheckedChanged(this, e);
            chkPSStbl_CheckedChanged(this, e);
            comboPSTint_SelectedIndexChanged(this, e);
            chkPSOnTop_CheckedChanged(this, e);
        }

        #endregion

    }

    unsafe static class puresignal
    {
        #region DllImport - Main

        [DllImport("wdsp.dll", EntryPoint = "SetPSRunCal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSRunCal(int channel, bool run);

        [DllImport("wdsp.dll", EntryPoint = "SetPSMox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSMox(int channel, bool mox);

        [DllImport("wdsp.dll", EntryPoint = "GetPSInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPSInfo(int channel, int* info);

        [DllImport("wdsp.dll", EntryPoint = "SetPSReset", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSReset(int channel, int reset);

        [DllImport("wdsp.dll", EntryPoint = "SetPSMancal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSMancal(int channel, int mancal);

        [DllImport("wdsp.dll", EntryPoint = "SetPSAutomode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSAutomode(int channel, int automode);

        [DllImport("wdsp.dll", EntryPoint = "SetPSTurnon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSTurnon(int channel, int turnon);

        [DllImport("wdsp.dll", EntryPoint = "SetPSControl", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSControl(int channel, int reset, int mancal, int automode, int turnon);

        [DllImport("wdsp.dll", EntryPoint = "SetPSLoopDelay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSLoopDelay(int channel, double delay);

        [DllImport("wdsp.dll", EntryPoint = "SetPSMoxDelay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSMoxDelay(int channel, double delay);

        [DllImport("wdsp.dll", EntryPoint = "SetPSTXDelay", CallingConvention = CallingConvention.Cdecl)]
        public static extern double SetPSTXDelay(int channel, double delay);

        [DllImport("wdsp.dll", EntryPoint = "psccF", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psccF(int channel, int size, float* Itxbuff, float* Qtxbuff, float* Irxbuff, float* Qrxbuff, bool mox, bool solidmox);

        [DllImport("wdsp.dll", EntryPoint = "PSSaveCorr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PSSaveCorr(int channel, string filename);

        [DllImport("wdsp.dll", EntryPoint = "PSRestoreCorr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PSRestoreCorr(int channel, string filename);

        [DllImport("wdsp.dll", EntryPoint = "SetPSHWPeak", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSHWPeak(int channel, double peak);

        [DllImport("wdsp.dll", EntryPoint = "GetPSHWPeak", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPSHWPeak(int channel, double* peak);

        [DllImport("wdsp.dll", EntryPoint = "GetPSMaxTX", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPSMaxTX(int channel, double* maxtx);

        [DllImport("wdsp.dll", EntryPoint = "SetPSPtol", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSPtol(int channel, double ptol);

        [DllImport("wdsp.dll", EntryPoint = "GetPSDisp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPSDisp(int channel, IntPtr x, IntPtr ym, IntPtr yc, IntPtr ys, IntPtr cm, IntPtr cc, IntPtr cs);

        [DllImport("wdsp.dll", EntryPoint = "SetPSFeedbackRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSFeedbackRate(int channel, int rate);

        [DllImport("wdsp.dll", EntryPoint = "SetPSPinMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSPinMode(int channel, int pin);

        [DllImport("wdsp.dll", EntryPoint = "SetPSMapMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSMapMode(int channel, int map);

        [DllImport("wdsp.dll", EntryPoint = "SetPSStabilize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSStabilize(int channel, int stbl);

        [DllImport("wdsp.dll", EntryPoint = "SetPSIntsAndSpi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPSIntsAndSpi(int channel, int ints, int spi);

        #endregion

        #region public methods

        public static int[] Info = new int[16];
        public static void getinfo(int txachannel)
        {
            fixed (int* ptr = &(Info[0]))
                GetPSInfo(txachannel, ptr);
        }

        #endregion
    }
}

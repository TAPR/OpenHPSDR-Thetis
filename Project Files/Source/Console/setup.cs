//=================================================================
// setup.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2018  Doug Wigley
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================

using System.Collections.Generic;
using System.Linq;

namespace Thetis
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Forms;
    using System.Text;
    using System.IO;
    using System.IO.Ports;
    using Midi2Cat;

    public partial class Setup : Form
    {
        #region Variable Declaration

        private Console console;
        private Progress progress;
        private ArrayList KeyList;
        private bool initializing;
        public bool alex_fw_good = false;

        #endregion

        #region Constructor and Destructor

        public Setup(Console c)
        {
            FlexProfilerInstalled = false;
            InitializeComponent();
            console = c;
            openFileDialog1.InitialDirectory = console.AppDataPath;

#if(!DEBUG)
            comboGeneralProcessPriority.Items.Remove("Idle");
            comboGeneralProcessPriority.Items.Remove("Below Normal");
#endif
            initializing = true;

            // GetMixerDevices();
            GetHosts();
            InitAlexAntTables();

            KeyList = new ArrayList();
            SetupKeyMap();

            GetTxProfiles();
            GetTxProfileDefs();

            RefreshCOMPortLists();

            RefreshSkinList();
            InitAudioTab();
            CurrentModel = Model.HERMES;
            comboGeneralProcessPriority.Text = "Normal";
            comboOptFilterWidthMode.Text = "Linear";
            //  comboAudioSampleRate1.SelectedIndex = 0;
            //  comboAudioSampleRateRX2.SelectedIndex = 0;
            comboAudioSampleRate1.Text = "192000";
            comboAudioSampleRateRX2.Text = "192000";
            comboAudioSampleRate2.Text = "48000";
            comboAudioSampleRate3.Text = "48000";
            comboAudioBuffer2.Text = "1024";
            comboAudioBuffer3.Text = "1024";
            //comboAudioChannels1.Text = "6";
            comboDisplayLabelAlign.Text = "Auto";
            comboColorPalette.Text = "enhanced";
            comboRX2ColorPalette.Text = "enhanced";
            comboTXLabelAlign.Text = "Auto";
            comboDisplayDriver.Text = "GDI+";
            comboFRSRegion.Text = "United States";

            comboDSPPhoneRXBuf.Text = "1024";
            comboDSPPhoneTXBuf.Text = "1024";
            comboDSPFMRXBuf.Text = "1024";
            comboDSPFMTXBuf.Text = "1024";
            comboDSPCWRXBuf.Text = "1024";
            comboDSPDigRXBuf.Text = "1024";
            comboDSPDigTXBuf.Text = "1024";

            comboDSPPhoneRXFiltSize.Text = "4096";
            comboDSPPhoneTXFiltSize.Text = "4096";
            comboDSPFMRXFiltSize.Text = "4096";
            comboDSPFMTXFiltSize.Text = "4096";
            comboDSPCWRXFiltSize.Text = "4096";
            comboDSPDigRXFiltSize.Text = "4096";
            comboDSPDigTXFiltSize.Text = "4096";

            comboDSPPhoneRXFiltType.SelectedIndex = 0;
            comboDSPPhoneTXFiltType.SelectedIndex = 0;
            comboDSPFMRXFiltType.SelectedIndex = 0;
            comboDSPFMTXFiltType.SelectedIndex = 0;
            comboDSPCWRXFiltType.SelectedIndex = 0;
            comboDSPDigRXFiltType.SelectedIndex = 0;
            comboDSPDigTXFiltType.SelectedIndex = 0;

            comboDispWinType.Text = "Kaiser";
            comboRX2DispWinType.Text = "Kaiser";
            comboDispPanDetector.Text = "Peak";
            comboDispWFDetector.Text = "Peak";
            comboDispPanAveraging.Text = "Log Recursive";
            comboDispWFAveraging.Text = "Log Recursive";
            comboRX2DispPanDetector.Text = "Peak";
            comboRX2DispPanAveraging.Text = "Log Recursive";
            comboRX2DispWFDetector.Text = "Peak";
            comboRX2DispWFAveraging.Text = "Log Recursive";
            comboTXDispPanDetector.Text = "Peak";
            comboTXDispPanAveraging.Text = "Log Recursive";
            comboTXDispWinType.Text = "Kaiser";
            comboTXDispWFDetector.Text = "Peak";
            comboTXDispWFAveraging.Text = "Log Recursive";
            comboKeyerConnKeyLine.SelectedIndex = 0;
            comboKeyerConnSecondary.SelectedIndex = 0;
            comboKeyerConnPTTLine.SelectedIndex = 0;
            comboKeyerConnPrimary.SelectedIndex = 0;
            comboTXTUNMeter.SelectedIndex = 0;
            comboMeterType.Text = "Edge";
            if (comboCATPort.Items.Count > 0) comboCATPort.SelectedIndex = 0;
            if (comboCATPTTPort.Items.Count > 0) comboCATPTTPort.SelectedIndex = 0;
            comboCATbaud.Text = "1200";
            comboCATparity.Text = "none";
            comboCATdatabits.Text = "8";
            comboCATstopbits.Text = "1";
            comboCATRigType.Text = "TS-2000";
            comboFocusMasterMode.Text = "None";

            if (comboCAT2Port.Items.Count > 0) comboCAT2Port.SelectedIndex = 0;
            // if (comboCATPTTPort.Items.Count > 0) comboCATPTTPort.SelectedIndex = 0;
            comboCAT2baud.Text = "1200";
            comboCAT2parity.Text = "none";
            comboCAT2databits.Text = "8";
            comboCAT2stopbits.Text = "1";

            if (comboCAT3Port.Items.Count > 0) comboCAT3Port.SelectedIndex = 0;
            // if (comboCATPTTPort.Items.Count > 0) comboCATPTTPort.SelectedIndex = 0;
            comboCAT3baud.Text = "1200";
            comboCAT3parity.Text = "none";
            comboCAT3databits.Text = "8";
            comboCAT3stopbits.Text = "1";

            if (comboCAT4Port.Items.Count > 0) comboCAT4Port.SelectedIndex = 0;
            //  if (comboCATPTTPort.Items.Count > 0) comboCATPTTPort.SelectedIndex = 0;
            comboCAT4baud.Text = "1200";
            comboCAT4parity.Text = "none";
            comboCAT4databits.Text = "8";
            comboCAT4stopbits.Text = "1";

            // comboFRSRegion.Text = "United States";

            //fillMetisIPAddrCombo();  /* must happen before GetOptions is called */ 

            GetOptions();

            if (comboDSPPhoneRXBuf.SelectedIndex < 0 || comboDSPPhoneRXBuf.SelectedIndex >= comboDSPPhoneRXBuf.Items.Count)
                comboDSPPhoneRXBuf.SelectedIndex = comboDSPPhoneRXBuf.Items.Count - 1;
            if (comboDSPPhoneTXBuf.SelectedIndex < 0 || comboDSPPhoneTXBuf.SelectedIndex >= comboDSPPhoneTXBuf.Items.Count)
                comboDSPPhoneTXBuf.SelectedIndex = comboDSPPhoneTXBuf.Items.Count - 1;
            if (comboDSPCWRXBuf.SelectedIndex < 0 || comboDSPCWRXBuf.SelectedIndex >= comboDSPCWRXBuf.Items.Count)
                comboDSPCWRXBuf.SelectedIndex = comboDSPCWRXBuf.Items.Count - 1;
            if (comboDSPDigRXBuf.SelectedIndex < 0 || comboDSPDigRXBuf.SelectedIndex >= comboDSPDigRXBuf.Items.Count)
                comboDSPDigRXBuf.SelectedIndex = comboDSPDigRXBuf.Items.Count - 1;
            if (comboDSPDigTXBuf.SelectedIndex < 0 || comboDSPDigTXBuf.SelectedIndex >= comboDSPDigTXBuf.Items.Count)
                comboDSPDigTXBuf.SelectedIndex = comboDSPDigTXBuf.Items.Count - 1;

            if (comboCATPort.SelectedIndex < 0)
            {
                if (comboCATPort.Items.Count > 0)
                    comboCATPort.SelectedIndex = 0;
                else
                {
                    chkCATEnable.Checked = false;
                    chkCATEnable.Enabled = false;
                }
            }

            if (comboCAT2Port.SelectedIndex < 0)
            {
                if (comboCAT2Port.Items.Count > 0)
                    comboCAT2Port.SelectedIndex = 0;
                else
                {
                    chkCAT2Enable.Checked = false;
                    chkCAT2Enable.Enabled = false;
                }
            }

            if (comboCAT3Port.SelectedIndex < 0)
            {
                if (comboCAT3Port.Items.Count > 0)
                    comboCAT3Port.SelectedIndex = 0;
                else
                {
                    chkCAT3Enable.Checked = false;
                    chkCAT3Enable.Enabled = false;
                }
            }

            if (comboCAT4Port.SelectedIndex < 0)
            {
                if (comboCAT4Port.Items.Count > 0)
                    comboCAT4Port.SelectedIndex = 0;
                else
                {
                    chkCAT4Enable.Checked = false;
                    chkCAT4Enable.Enabled = false;
                }
            }

            cmboSigGenRXMode.Text = "Radio";
            cmboSigGenTXMode.Text = "Radio";

            if (comboAudioDriver2.SelectedIndex < 0 &&
                comboAudioDriver2.Items.Count > 0)
                comboAudioDriver2.SelectedIndex = 0;

            if (comboAudioDriver3.SelectedIndex < 0 &&
                comboAudioDriver3.Items.Count > 0)
                comboAudioDriver3.SelectedIndex = 0;

            comboAudioBuffer1_SelectedIndexChanged(this, EventArgs.Empty);

            initializing = false;
            udDisplayScopeTime_ValueChanged(this, EventArgs.Empty);

            if (comboTXProfileName.SelectedIndex < 0 &&
                comboTXProfileName.Items.Count > 0)
                comboTXProfileName.SelectedIndex = 0;
            current_profile = comboTXProfileName.Text;

            if (chkCATEnable.Checked)
            {
                chkCATEnable_CheckedChanged(this, EventArgs.Empty);
            }

            if (chkCAT2Enable.Checked)
            {
                chkCAT2Enable_CheckedChanged(this, EventArgs.Empty);
            }

            if (chkCAT3Enable.Checked)
            {
                chkCAT3Enable_CheckedChanged(this, EventArgs.Empty);
            }

            if (chkCAT4Enable.Checked)
            {
                chkCAT4Enable_CheckedChanged(this, EventArgs.Empty);
            }

            if (chkCATPTTEnabled.Checked)
            {
                chkCATPTTEnabled_CheckedChanged(this, EventArgs.Empty);
            }

            comboKeyerConnSecondary_SelectedIndexChanged(this, EventArgs.Empty);

            //if (radGenModelFLEX5000.Checked && DB.GetVars("Options").Count != 0)
            //    radGenModelFLEX5000_CheckedChanged(this, EventArgs.Empty);

            ForceAllEvents();

            EventArgs e = EventArgs.Empty;
            //comboGeneralLPTAddr_LostFocus(this, e);
            //udDDSCorrection_ValueChanged(this, e);
           // udDSPCWPitch_ValueChanged(this, e);
           // udTXFilterHigh_ValueChanged(this, e);
           // udTXFilterLow_ValueChanged(this, e);
            tbRX1FilterAlpha_Scroll(this, e);
            tbTXFilterAlpha_Scroll(this, e);
            tbBackgroundAlpha_Scroll(this, e);
            tbTXBackgroundAlpha_Scroll(this, e);
            tbGridCourseAlpha_Scroll(this, e);
            tbTXVGridCourseAlpha_Scroll(this, e);
            tbGridFineAlpha_Scroll(this, e);
            tbTXVGridFineAlpha_Scroll(this, e);
            tbHGridColorAlpha_Scroll(this, e);
            tbTXHGridColorAlpha_Scroll(this, e);
            tbMultiRXFilterAlpha_Scroll(this, e);
            tbTXZeroLineAlpha_Scroll(this, e);
            tbTXTextAlpha_Scroll(this, e);
            tbMeterEdgeBackgroundAlpha_Scroll(this, e);
            checkHPSDRDefaults(this, e);
            chkAlexPresent_CheckedChanged(this, e);
            chkApolloPresent_CheckedChanged(this, e);
            chkAlexAntCtrl_CheckedChanged(this, e);

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    console.radio.GetDSPRX(i, j).Update = true;

            comboDSPPhoneRXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPPhoneTXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMRXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMTXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPCWRXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigRXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigTXBuf_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPPhoneRXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPPhoneTXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMRXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMTXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPCWRXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigRXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigTXFiltSize_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPPhoneRXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPPhoneTXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMRXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPFMTXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPCWRXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigRXFiltType_SelectedIndexChanged(this, EventArgs.Empty);
            comboDSPDigTXFiltType_SelectedIndexChanged(this, EventArgs.Empty);

            console.specRX.GetSpecRX(0).Update = true;
            console.specRX.GetSpecRX(1).Update = true;
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).Update = true;

            openFileDialog1.Filter = "Thetis Database Files (*.xml) | *.xml";

            AddHPSDRPages();

            if (chkKWAI.Checked)
                AllowFreqBroadcast = true;
            else
                AllowFreqBroadcast = false;

        }
#if false
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
#endif
        #endregion

        #region Init Routines
        // ======================================================
        // Init Routines
        // ======================================================

        private void InitGeneralTab()
        {
            chkGeneralRXOnly.Checked = console.RXOnly;
            chkGeneralDisablePTT.Checked = console.DisablePTT;
        }

        private void InitAudioTab()
        {
            if (!comboAudioSampleRate1.Items.Contains(96000))
                comboAudioSampleRate1.Items.Add(96000);
            if (!comboAudioSampleRate1.Items.Contains(192000))
                comboAudioSampleRate1.Items.Add(192000);
            if (!comboAudioSampleRate1.Items.Contains(384000))
                comboAudioSampleRate1.Items.Add(384000);
            if (!comboAudioSampleRate1.Items.Contains(768000))
                comboAudioSampleRate1.Items.Add(768000);
            if (!comboAudioSampleRate1.Items.Contains(1536000))
                comboAudioSampleRate1.Items.Add(1536000);
            if (comboAudioSampleRate1.SelectedIndex < 0)
                comboAudioSampleRate1.Text = "192000";

            if (!comboAudioSampleRateRX2.Items.Contains(96000))
                comboAudioSampleRateRX2.Items.Add(96000);
            if (!comboAudioSampleRateRX2.Items.Contains(192000))
                comboAudioSampleRateRX2.Items.Add(192000);
            if (!comboAudioSampleRateRX2.Items.Contains(384000))
                comboAudioSampleRateRX2.Items.Add(384000);
            if (!comboAudioSampleRateRX2.Items.Contains(768000))
                comboAudioSampleRateRX2.Items.Add(768000);
            if (!comboAudioSampleRateRX2.Items.Contains(1536000))
                comboAudioSampleRateRX2.Items.Add(1536000);
            if (comboAudioSampleRateRX2.SelectedIndex < 0)
                comboAudioSampleRateRX2.Text = "192000";
        }

        private void InitDisplayTab()
        {
            udDisplayGridMax.Value = Display.SpectrumGridMax;
            udDisplayGridMin.Value = Display.SpectrumGridMin;
            udDisplayGridStep.Value = Display.SpectrumGridStep;
            udRX2DisplayGridMax.Value = Display.RX2SpectrumGridMax;
            udRX2DisplayGridMin.Value = Display.RX2SpectrumGridMin;
            udRX2DisplayGridStep.Value = Display.RX2SpectrumGridStep;
            udDisplayFPS.Value = console.DisplayFPS;
            clrbtnWaterfallLow.Color = Display.WaterfallLowColor;
            clrbtnRX2WaterfallLow.Color = Display.RX2WaterfallLowColor;
            udDisplayWaterfallLowLevel.Value = (decimal)Display.WaterfallLowThreshold;
            udDisplayWaterfallHighLevel.Value = (decimal)Display.WaterfallHighThreshold;
            udRX2DisplayWaterfallLowLevel.Value = (decimal)Display.RX2WaterfallLowThreshold;
            udRX2DisplayWaterfallHighLevel.Value = (decimal)Display.RX2WaterfallHighThreshold;
            udDisplayMeterDelay.Value = console.MeterDelay;
            udDisplayPeakText.Value = console.PeakTextDelay;
            udDisplayCPUMeter.Value = console.CPUMeterDelay;
            udDisplayPhasePts.Value = Display.PhaseNumPts;
            udDisplayMultiPeakHoldTime.Value = console.MultimeterPeakHoldTime;
            udDisplayMultiTextHoldTime.Value = console.MultimeterTextPeakTime;
            udTXGridMax.Value = Display.TXSpectrumGridMax;
            udTXGridMin.Value = Display.TXSpectrumGridMin;
            udTXGridStep.Value = Display.TXSpectrumGridStep;
        }

        private void InitDSPTab()
        {
            udDSPCWPitch.Value = console.CWPitch;
        }

        private void InitKeyboardTab()
        {
            // set tune keys
            comboKBTuneUp1.Text = KeyToString(console.KeyTuneUp1);
            comboKBTuneUp2.Text = KeyToString(console.KeyTuneUp2);
            comboKBTuneUp3.Text = KeyToString(console.KeyTuneUp3);
            comboKBTuneUp4.Text = KeyToString(console.KeyTuneUp4);
            comboKBTuneUp5.Text = KeyToString(console.KeyTuneUp5);
            comboKBTuneUp6.Text = KeyToString(console.KeyTuneUp6);
            comboKBTuneUp7.Text = KeyToString(console.KeyTuneUp7);
            comboKBTuneDown1.Text = KeyToString(console.KeyTuneDown1);
            comboKBTuneDown2.Text = KeyToString(console.KeyTuneDown2);
            comboKBTuneDown3.Text = KeyToString(console.KeyTuneDown3);
            comboKBTuneDown4.Text = KeyToString(console.KeyTuneDown4);
            comboKBTuneDown5.Text = KeyToString(console.KeyTuneDown5);
            comboKBTuneDown6.Text = KeyToString(console.KeyTuneDown6);
            comboKBTuneDown7.Text = KeyToString(console.KeyTuneDown7);

            // set band keys
            comboKBBandDown.Text = KeyToString(console.KeyBandDown);
            comboKBBandUp.Text = KeyToString(console.KeyBandUp);

            // set filter keys
            comboKBFilterDown.Text = KeyToString(console.KeyFilterDown);
            comboKBFilterUp.Text = KeyToString(console.KeyFilterUp);

            // set mode keys
            comboKBModeDown.Text = KeyToString(console.KeyModeDown);
            comboKBModeUp.Text = KeyToString(console.KeyModeUp);

            // set RIT keys
            comboKBRITDown.Text = KeyToString(console.KeyRITDown);
            comboKBRITUp.Text = KeyToString(console.KeyRITUp);

            // set XIT keys
            comboKBXITDown.Text = KeyToString(console.KeyXITDown);
            comboKBXITUp.Text = KeyToString(console.KeyXITUp);

            // set CW keys
            comboKBCWDot.Text = KeyToString(console.KeyCWDot);
            comboKBCWDash.Text = KeyToString(console.KeyCWDash);

            // set PTT keys
            comboKBPTTTx.Text = KeyToString(console.KeyPTTTx);
            comboKBPTTRx.Text = KeyToString(console.KeyPTTRx);
        }

        private void InitAppearanceTab()
        {
            clrbtnBackground.Color = Display.DisplayBackgroundColor;
            clrbtnGrid.Color = Display.GridColor;
            clrbtnGridFine.Color = Display.GridPenDark;
            clrbtnHGridColor.Color = Display.HGridColor;
            clrbtnZeroLine.Color = Display.GridZeroColor;
            clrbtnText.Color = Display.GridTextColor;
            clrbtnDataLine.Color = Display.DataLineColor;
            clrbtnFilter.Color = Display.DisplayFilterColor;
            clrbtnMeterLeft.Color = console.MeterLeftColor;
            clrbtnMeterRight.Color = console.MeterRightColor;
            clrbtnBtnSel.Color = console.ButtonSelectedColor;
            clrbtnVFODark.Color = console.VFOTextDarkColor;
            clrbtnVFOLight.Color = console.VFOTextLightColor;
            clrbtnBandDark.Color = console.BandTextDarkColor;
            clrbtnBandLight.Color = console.BandTextLightColor;
            clrbtnPeakText.Color = console.PeakTextColor;
            clrbtnOutOfBand.Color = console.OutOfBandColor;
        }

        #endregion

        #region Misc Routines
        // ======================================================
        // Misc Routines
        // ======================================================

        private void RefreshCOMPortLists()
        {
            string[] com_ports = SerialPort.GetPortNames();

            comboKeyerConnPrimary.Items.Clear();
            comboKeyerConnPrimary.Items.Add("Radio");
            comboKeyerConnPrimary.Items.Add("None");
            comboKeyerConnPrimary.Items.AddRange(com_ports);

            comboKeyerConnSecondary.Items.Clear();
            comboKeyerConnSecondary.Items.Add("None");
            comboKeyerConnSecondary.Items.Add("CAT");
            comboKeyerConnSecondary.Items.AddRange(com_ports);

            comboCATPort.Items.Clear();
            comboCATPort.Items.Add("None");
            comboCATPort.Items.AddRange(com_ports);

            comboCAT2Port.Items.Clear();
            comboCAT2Port.Items.Add("None");
            comboCAT2Port.Items.AddRange(com_ports);

            comboCAT3Port.Items.Clear();
            comboCAT3Port.Items.Add("None");
            comboCAT3Port.Items.AddRange(com_ports);

            comboCAT4Port.Items.Clear();
            comboCAT4Port.Items.Add("None");
            comboCAT4Port.Items.AddRange(com_ports);

            comboCATPTTPort.Items.Clear();
            comboCATPTTPort.Items.Add("None");
            comboCATPTTPort.Items.Add("CAT");
            comboCATPTTPort.Items.AddRange(com_ports);
        }

        private void RefreshSkinList()
        {
            string skin = comboAppSkin.Text;
            comboAppSkin.Items.Clear();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\OpenHPSDR\\Skins";

            if (!Directory.Exists(path))
            {
                MessageBox.Show("The console presentation files (skins) were not found.\n" +
                    "Appearance will suffer until this is rectified.\n",
                    "Skins files not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            foreach (string d in Directory.GetDirectories(path))
            {
                string s = d.Substring(d.LastIndexOf("\\") + 1);
                if (!s.StartsWith("."))
                    comboAppSkin.Items.Add(d.Substring(d.LastIndexOf("\\") + 1));
            }

            if (comboAppSkin.Items.Count == 0)
            {
                MessageBox.Show("The console presentation files (skins) were not found.\n" +
                    "Appearance will suffer until this is rectified.\n",
                    "Skins files not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (skin == "")
            {
                //if (comboAppSkin.Items.Contains("Default"))
                //    comboAppSkin.Text = "Default";
                //else
                //    comboAppSkin.Text = "IK3VIG Special"; //"OpenHPSDR-Gray";
                if (comboAppSkin.Items.Contains("IK3VIG Special"))
                    comboAppSkin.Text = "IK3VIG Special";
                else
                    comboAppSkin.Text = "OpenHPSDR-Gray"; //"OpenHPSDR-Gray";
            }
            else if (comboAppSkin.Items.Contains(skin))
                     comboAppSkin.Text = skin;
            else comboAppSkin.Text = "IK3VIG Special";
        }

        private void GetHosts()
        {
            // comboAudioDriver1.Items.Clear();
            comboAudioDriver2.Items.Clear();
            comboAudioDriver3.Items.Clear();
            int host_index = 0;
            foreach (string PAHostName in Audio.GetPAHosts())
            {
                if (Audio.GetPAInputDevices(host_index).Count > 0 ||
                    Audio.GetPAOutputDevices(host_index).Count > 0)
                {
                    //comboAudioDriver1.Items.Add(new PADeviceInfo(PAHostName, host_index));
                    if (PAHostName != "Windows WASAPI")
                    {
                        comboAudioDriver2.Items.Add(new PADeviceInfo(PAHostName, host_index));
                        comboAudioDriver3.Items.Add(new PADeviceInfo(PAHostName, host_index));
                    }
                    // comboAudioDriver1.Items.Add(new PADeviceInfo(PAHostName, host_index));
                    // comboAudioDriver2.Items.Add(new PADeviceInfo(PAHostName, host_index));
                }
                host_index++; //Increment host index
            }
        }

        private void GetDevices2()
        {
            comboAudioInput2.Items.Clear();
            comboAudioOutput2.Items.Clear();
            int host = ((PADeviceInfo)comboAudioDriver2.SelectedItem).Index;
            ArrayList a = Audio.GetPAInputDevices(host);
            foreach (PADeviceInfo p in a)
                comboAudioInput2.Items.Add(p);

            a = Audio.GetPAOutputDevices(host);
            foreach (PADeviceInfo p in a)
                comboAudioOutput2.Items.Add(p);
        }

        private void GetDevices3()
        {
            comboAudioInput3.Items.Clear();
            comboAudioOutput3.Items.Clear();
            int host = ((PADeviceInfo)comboAudioDriver3.SelectedItem).Index;
            ArrayList a = Audio.GetPAInputDevices(host);
            foreach (PADeviceInfo p in a)
                comboAudioInput3.Items.Add(p);

            a = Audio.GetPAOutputDevices(host);
            foreach (PADeviceInfo p in a)
                comboAudioOutput3.Items.Add(p);
        }

        private void ControlList(Control c, ref ArrayList a)
        {
            if (c.Controls.Count > 0)
            {
                foreach (Control c2 in c.Controls)
                    ControlList(c2, ref a);
            }

            if (c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(CheckBox) ||
                c.GetType() == typeof(ComboBoxTS) || c.GetType() == typeof(ComboBox) ||
                c.GetType() == typeof(NumericUpDownTS) || c.GetType() == typeof(NumericUpDown) ||
                c.GetType() == typeof(RadioButtonTS) || c.GetType() == typeof(RadioButton) ||
                c.GetType() == typeof(TextBoxTS) || c.GetType() == typeof(TextBox) ||
                c.GetType() == typeof(TrackBarTS) || c.GetType() == typeof(TrackBar) ||
                c.GetType() == typeof(ColorButton))
                a.Add(c);
        }

        private static bool saving = false;

        public void SaveOptions()
        {
            // Automatically saves all control settings to the database in the tab
            // pages on this form of the following types: CheckBoxTS, ComboBoxTS,
            // NumericUpDownTS, RadioButtonTS, TextBox, and TrackBar (slider)

            saving = true;

            ArrayList a = new ArrayList();
            ArrayList temp = new ArrayList();

            ControlList(this, ref temp);

            foreach (Control c in temp)				// For each control
            {
                if (c.GetType() == typeof(CheckBoxTS))
                    a.Add(c.Name + "/" + ((CheckBoxTS)c).Checked.ToString());
                else if (c.GetType() == typeof(ComboBoxTS))
                {
                    //if(((ComboBoxTS)c).SelectedIndex >= 0)
                    a.Add(c.Name + "/" + ((ComboBoxTS)c).Text);
                }
                else if (c.GetType() == typeof(NumericUpDownTS))
                    a.Add(c.Name + "/" + ((NumericUpDownTS)c).Value.ToString());
                else if (c.GetType() == typeof(RadioButtonTS))
                    a.Add(c.Name + "/" + ((RadioButtonTS)c).Checked.ToString());
                else if (c.GetType() == typeof(TextBoxTS))
                    a.Add(c.Name + "/" + ((TextBoxTS)c).Text);
                else if (c.GetType() == typeof(TrackBarTS))
                    a.Add(c.Name + "/" + ((TrackBarTS)c).Value.ToString());
                else if (c.GetType() == typeof(ColorButton))
                {
                    Color clr = ((ColorButton)c).Color;
                    a.Add(c.Name + "/" + clr.R + "." + clr.G + "." + clr.B + "." + clr.A);
                }
#if(DEBUG)
                else if (c.GetType() == typeof(GroupBox) ||
                    c.GetType() == typeof(CheckBox) ||
                    c.GetType() == typeof(ComboBox) ||
                    c.GetType() == typeof(NumericUpDown) ||
                    c.GetType() == typeof(RadioButton) ||
                    c.GetType() == typeof(TextBox) ||
                    c.GetType() == typeof(TrackBar))
                    Debug.WriteLine(c.Name + " needs to be converted to a Thread Safe control.");
#endif
            }

            DB.SaveVars("Options", ref a);		// save the values to the DB
            saving = false;
        }

        public void GetOptions()
        {
            // Automatically restores all controls from the database in the
            // tab pages on this form of the following types: CheckBoxTS, ComboBoxTS,
            // NumericUpDownTS, RadioButtonTS, TextBox, and TrackBar (slider)

            // get list of live controls
            //ArrayList temp = new ArrayList();
            ArrayList temp = new ArrayList();		// list of all first level controls
            ControlList(this, ref temp);

            ArrayList checkbox_list = new ArrayList();
            ArrayList combobox_list = new ArrayList();
            ArrayList numericupdown_list = new ArrayList();
            ArrayList radiobutton_list = new ArrayList();
            ArrayList textbox_list = new ArrayList();
            ArrayList trackbar_list = new ArrayList();
            ArrayList colorbutton_list = new ArrayList();

            //ArrayList controls = new ArrayList();	// list of controls to restore
            foreach (Control c in temp)
            {

                if (c.GetType() == typeof(CheckBoxTS))			// the control is a CheckBoxTS
                    checkbox_list.Add(c);
                else if (c.GetType() == typeof(ComboBoxTS))		// the control is a ComboBoxTS
                    combobox_list.Add(c);
                else if (c.GetType() == typeof(NumericUpDownTS))	// the control is a NumericUpDownTS
                    numericupdown_list.Add(c);
                else if (c.GetType() == typeof(RadioButtonTS))	// the control is a RadioButtonTS
                    radiobutton_list.Add(c);
                else if (c.GetType() == typeof(TextBoxTS))		// the control is a TextBox
                    textbox_list.Add(c);
                else if (c.GetType() == typeof(TrackBarTS))		// the control is a TrackBar (slider)
                    trackbar_list.Add(c);
                else if (c.GetType() == typeof(ColorButton))
                    colorbutton_list.Add(c);
            }
            temp.Clear();	// now that we have the controls we want, delete first list 

            ArrayList a = DB.GetVars("Options");						// Get the saved list of controls
            a.Sort();
            int num_controls = checkbox_list.Count + combobox_list.Count +
                numericupdown_list.Count + radiobutton_list.Count +
                textbox_list.Count + trackbar_list.Count +
                colorbutton_list.Count;

            if (a.Count < num_controls)		// some control values are not in the database
            {								// so set all of them to the defaults
                InitGeneralTab();
                InitAudioTab();
                InitDSPTab();
                InitDisplayTab();
                InitKeyboardTab();
                InitAppearanceTab();
            }
            //InitAudioTab();
            // restore saved values to the controls
            foreach (string s in a)				// string is in the format "name,value"
            {
                string[] vals = s.Split('/');
                if (vals.Length > 2)
                {
                    for (int i = 2; i < vals.Length; i++)
                        vals[1] += "/" + vals[i];
                }

                string name = vals[0];
                string val = vals[1];

                if (s.StartsWith("chk"))			// control is a CheckBoxTS
                {
                    for (int i = 0; i < checkbox_list.Count; i++)
                    {	// look through each control to find the matching name
                        CheckBoxTS c = (CheckBoxTS)checkbox_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            c.Checked = bool.Parse(val);	// restore value
                            i = checkbox_list.Count + 1;
                        }
                        if (i == checkbox_list.Count - 1)
                            // MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("combo"))	// control is a ComboBox
                {
                    for (int i = 0; i < combobox_list.Count; i++)
                    {	// look through each control to find the matching name
                        ComboBoxTS c = (ComboBoxTS)combobox_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            if (c.Items.Count > 0 && c.Items[0].GetType() == typeof(string))
                            {
                                c.Text = val;
                            }
                            else
                            {
                                foreach (object o in c.Items)
                                {
                                    if (o.ToString() == val)
                                        c.Text = val;	// restore value
                                }
                            }
                            i = combobox_list.Count + 1;
                        }
                        if (i == combobox_list.Count - 1)
                            // MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("ud"))
                {
                    for (int i = 0; i < numericupdown_list.Count; i++)
                    {	// look through each control to find the matching name
                        NumericUpDownTS c = (NumericUpDownTS)numericupdown_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            decimal num = decimal.Parse(val);

                            if (num > c.Maximum) num = c.Maximum;		// check endpoints
                            else if (num < c.Minimum) num = c.Minimum;
                            c.Value = num;			// restore value
                            i = numericupdown_list.Count + 1;
                        }
                        if (i == numericupdown_list.Count - 1)
                            // MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("rad"))
                {	// look through each control to find the matching name

                    for (int i = 0; i < radiobutton_list.Count; i++)
                    {
                        RadioButtonTS c = (RadioButtonTS)radiobutton_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            c.Checked = bool.Parse(val);	// restore value
                            i = radiobutton_list.Count + 1;
                        }
                        if (i == radiobutton_list.Count - 1)
                            //MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("txt"))
                {	// look through each control to find the matching name
                    for (int i = 0; i < textbox_list.Count; i++)
                    {
                        TextBoxTS c = (TextBoxTS)textbox_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            c.Text = val;	// restore value
                            i = textbox_list.Count + 1;
                        }
                        if (i == textbox_list.Count - 1)
                            // MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("tb"))
                {
                    // look through each control to find the matching name
                    for (int i = 0; i < trackbar_list.Count; i++)
                    {
                        TrackBarTS c = (TrackBarTS)trackbar_list[i];
                        if (c.Name.Equals(name))		// name found
                        {
                            c.Value = Int32.Parse(val);
                            i = trackbar_list.Count + 1;
                        }
                        if (i == trackbar_list.Count - 1)
                            //MessageBox.Show("Control not found: " + name);
                            Debug.WriteLine("Control not found: " + name);
                    }
                }
                else if (s.StartsWith("clrbtn"))
                {
                    string[] colors = val.Split('.');
                    if (colors.Length == 4)
                    {
                        int R, G, B, A;
                        R = Int32.Parse(colors[0]);
                        G = Int32.Parse(colors[1]);
                        B = Int32.Parse(colors[2]);
                        A = Int32.Parse(colors[3]);

                        for (int i = 0; i < colorbutton_list.Count; i++)
                        {
                            ColorButton c = (ColorButton)colorbutton_list[i];
                            if (c.Name.Equals(name))		// name found
                            {
                                c.Color = Color.FromArgb(A, R, G, B);
                                i = colorbutton_list.Count + 1;
                            }
                            if (i == colorbutton_list.Count - 1)
                                //MessageBox.Show("Control not found: " + name);
                                Debug.WriteLine("Control not found: " + name);
                        }
                    }
                }
            }

            foreach (ColorButton c in colorbutton_list)
                c.Automatic = "";
        }

        private string KeyToString(Keys k)
        {
            if (!k.ToString().StartsWith("Oem"))
                return k.ToString();

            string s = "";
            switch (k)
            {
                case Keys.OemOpenBrackets:
                    s = "[";
                    break;
                case Keys.OemCloseBrackets:
                    s = "]";
                    break;
                case Keys.OemQuestion:
                    s = "/";
                    break;
                case Keys.OemPeriod:
                    s = ".";
                    break;
                case Keys.OemPipe:
                    s = (k & Keys.Shift) == 0 ? "\\" : "|";
                    break;
            }
            return s;
        }

        private void SetupKeyMap()
        {
            KeyList.Add(Keys.None);
            KeyList.Add(Keys.A);
            KeyList.Add(Keys.B);
            KeyList.Add(Keys.C);
            KeyList.Add(Keys.D);
            KeyList.Add(Keys.E);
            KeyList.Add(Keys.F);
            KeyList.Add(Keys.G);
            KeyList.Add(Keys.H);
            KeyList.Add(Keys.I);
            KeyList.Add(Keys.J);
            KeyList.Add(Keys.K);
            KeyList.Add(Keys.L);
            KeyList.Add(Keys.M);
            KeyList.Add(Keys.N);
            KeyList.Add(Keys.O);
            KeyList.Add(Keys.P);
            KeyList.Add(Keys.Q);
            KeyList.Add(Keys.R);
            KeyList.Add(Keys.S);
            KeyList.Add(Keys.T);
            KeyList.Add(Keys.U);
            KeyList.Add(Keys.V);
            KeyList.Add(Keys.W);
            KeyList.Add(Keys.X);
            KeyList.Add(Keys.Y);
            KeyList.Add(Keys.Z);
            KeyList.Add(Keys.F1);
            KeyList.Add(Keys.F2);
            KeyList.Add(Keys.F3);
            KeyList.Add(Keys.F4);
            KeyList.Add(Keys.F5);
            KeyList.Add(Keys.F6);
            KeyList.Add(Keys.F7);
            KeyList.Add(Keys.F8);
            KeyList.Add(Keys.F9);
            KeyList.Add(Keys.F10);
            KeyList.Add(Keys.Insert);
            KeyList.Add(Keys.Delete);
            KeyList.Add(Keys.Home);
            KeyList.Add(Keys.End);
            KeyList.Add(Keys.PageUp);
            KeyList.Add(Keys.PageDown);
            KeyList.Add(Keys.Up);
            KeyList.Add(Keys.Down);
            KeyList.Add(Keys.Left);
            KeyList.Add(Keys.Right);
            KeyList.Add(Keys.OemOpenBrackets);
            KeyList.Add(Keys.OemCloseBrackets);
            KeyList.Add(Keys.OemPeriod);
            KeyList.Add(Keys.OemQuestion);
            //			KeyList.Add(Keys.OemSemicolon);
            //			KeyList.Add(Keys.OemQuotes);
            //			KeyList.Add(Keys.Oemcomma);
            //			KeyList.Add(Keys.OemPeriod);
            //			KeyList.Add(Keys.OemBackslash);
            //			KeyList.Add(Keys.OemQuestion);
            //          KeyList.Add(Keys.Space);

            foreach (Control c in tpKeyboard.Controls)
            {
                if (c.GetType() == typeof(GroupBoxTS))
                {
                    foreach (Control c2 in c.Controls)
                    {
                        if (c2.GetType() != typeof(ComboBoxTS)) continue;
                        ComboBoxTS combo = (ComboBoxTS)c2;
                        combo.Items.Clear();
                        foreach (Keys k in KeyList)
                        {
                            combo.Items.Add(k.ToString().StartsWith("Oem") ? KeyToString(k) : k.ToString());
                        }
                    }
                }
                else if (c.GetType() == typeof(ComboBoxTS))
                {
                    ComboBoxTS combo = (ComboBoxTS)c;
                    combo.Items.Clear();
                    foreach (Keys k in KeyList)
                        combo.Items.Add(k.ToString());
                }
            }
        }

        public int CollapsedWidth
        {
            get { return int.Parse(txtCollapsedWidth.Text); }
            set
            {
                this.txtCollapsedWidth.Text = value.ToString();
                //				if (!saving)
                //					SaveOptions();
            }
        }

        public int CollapsedHeight
        {
            get { return int.Parse(txtCollapsedHeight.Text); }
            set
            {
                this.txtCollapsedHeight.Text = value.ToString();
                //				if (!saving)
                //					SaveOptions();
            }
        }

        private void ForceAllEvents()
        {
            EventArgs e = EventArgs.Empty;

            // General Tab
            //  chkAlexAntCtrl_CheckedChanged(this, e);
            //comboGeneralLPTAddr_SelectedIndexChanged(this, e);
            udGeneralLPTDelay_ValueChanged(this, e);
            chkGeneralRXOnly_CheckedChanged(this, e);
            //chkGeneralUSBPresent_CheckedChanged(this, e);
            // chkGeneralPAPresent_CheckedChanged(this, e);
            // chkGeneralATUPresent_CheckedChanged(this, e);
            // chkXVTRPresent_CheckedChanged(this, e);
            comboGeneralXVTR_SelectedIndexChanged(this, e);
            // udDDSCorrection_ValueChanged(this, e);
            //udDDSPLLMult_ValueChanged(this, e);
            // udDDSIFFreq_ValueChanged(this, e);
            // chkGeneralSpurRed_CheckedChanged(this, e);
            chkGeneralDisablePTT_CheckedChanged(this, e);
            //chkGeneralSoftwareGainCorr_CheckedChanged(this, e);
            // chkGeneralEnableX2_CheckedChanged(this, e);
            // udGeneralX2Delay_ValueChanged(this, e);
            // chkGeneralCustomFilter_CheckedChanged(this, e);
            comboGeneralProcessPriority_SelectedIndexChanged(this, e);
            chkFullDiscovery_CheckedChanged(this, e);
            btnSetIPAddr_Click(this, e);
            // radMetis_CheckedChanged(this, e);
            radOrionPTTOff_CheckedChanged(this, e);
            radOrionMicTip_CheckedChanged(this, e);
            radOrionBiasOn_CheckedChanged(this, e);
            chkNetworkWDT_CheckedChanged(this, e);

            // Audio Tab
            comboAudioBuffer2_SelectedIndexChanged(this, e);
            comboAudioSampleRate1_SelectedIndexChanged(this, e);
            comboAudioSampleRateRX2_SelectedIndexChanged(this, e);
            comboAudioSampleRate2_SelectedIndexChanged(this, e);
            udAudioLatency2_ValueChanged(this, e);
            udAudioLatency2_Out_ValueChanged(this, e);
            udAudioLatencyPAIn_ValueChanged(this, e);
            udAudioLatencyPAOut_ValueChanged(this, e);
            udVAC2Latency_ValueChanged(this, e);
            udVAC2LatencyOut_ValueChanged(this, e);
            udVAC2LatencyPAIn_ValueChanged(this, e);
            udVAC2LatencyPAOut_ValueChanged(this, e);
            udAudioVACGainRX_ValueChanged(this, e);
            udVAC2GainRX_ValueChanged(this, e);
            udAudioVACGainTX_ValueChanged(this, e);
            udVAC2GainTX_ValueChanged(this, e);
            chkVAC2UseRX2_CheckedChanged(this, e);
            chkAudioRX2toVAC_CheckedChanged(this, e);
            chkAudioLatencyManual2_CheckedChanged(this, e);
            chkAudioLatencyManual2_Out_CheckedChanged(this, e);
            chkAudioLatencyPAInManual_CheckedChanged(this, e);
            chkAudioLatencyPAOutManual_CheckedChanged(this, e);
            chkVAC2LatencyManual_CheckedChanged(this, e);
            chkVAC2LatencyOutManual_CheckedChanged(this, e);
            chkVAC2LatencyPAInManual_CheckedChanged(this, e);
            chkVAC2LatencyPAOutManual_CheckedChanged(this, e);

            // Calibration Tab
            udTXDisplayCalOffset_ValueChanged(this, e);
            udHPSDRFreqCorrectFactor_ValueChanged(this, e);

            // Test Tab
            udTXGenFreq_ValueChanged(this, e);
            udTXGenScale_ValueChanged(this, e);
            udTwoToneLevel_ValueChanged(this, e);

            // Display Tab
            udDisplayGridMax_ValueChanged(this, e);
            udDisplayGridMin_ValueChanged(this, e);
            udDisplayGridStep_ValueChanged(this, e);
            udDisplayFPS_ValueChanged(this, e);
            udTXGridMax_ValueChanged(this, e);
            udTXGridMin_ValueChanged(this, e);
            udTXGridStep_ValueChanged(this, e);
            udDisplayMeterDelay_ValueChanged(this, e);
            udDisplayPeakText_ValueChanged(this, e);
            udDisplayCPUMeter_ValueChanged(this, e);
            udDisplayPhasePts_ValueChanged(this, e);
            udDisplayAVGTime_ValueChanged(this, e);
            udRX2DisplayAVGTime_ValueChanged(this, e);
            udDisplayWaterfallLowLevel_ValueChanged(this, e);
            udDisplayWaterfallHighLevel_ValueChanged(this, e);
            clrbtnWaterfallLow_Changed(this, e);
            udDisplayMultiPeakHoldTime_ValueChanged(this, e);
            udDisplayMultiTextHoldTime_ValueChanged(this, e);
            udRX2DisplayGridMax_ValueChanged(this, e);
            udRX2DisplayGridMin_ValueChanged(this, e);
            udRX2DisplayGridStep_ValueChanged(this, e);
            udRX2DisplayWaterfallLowLevel_ValueChanged(this, e);
            udRX2DisplayWaterfallHighLevel_ValueChanged(this, e);
            clrbtnRX2WaterfallLow_Changed(this, e);
            chkRX1WaterfallAGC_CheckedChanged(this, e);
            chkRX2WaterfallAGC_CheckedChanged(this, e);
            chkANAN8000DLEDisplayVoltsAmps_CheckedChanged(this, e);

            // DSP Tab
            udLMSANF_ValueChanged(this, e);
            udLMSNR_ValueChanged(this, e);
            udLMSANF2_ValueChanged(this, e);
            udLMSNR2_ValueChanged(this, e);
            // udDSPImagePhaseTX_ValueChanged(this, e);
            // udDSPImageGainTX_ValueChanged(this, e);
            udDSPCWPitch_ValueChanged(this, e);
            udDSPNB_ValueChanged(this, e);
            comboDSPNOBmode_SelectedIndexChanged(this, e);
            comboDSPRxWindow_SelectedIndexChanged(this, e);
            comboDSPTxWindow_SelectedIndexChanged(this, e);
            chkDSPKeyerSemiBreakInEnabled_CheckedChanged(this, e);
            udHWKeyDownDelay_ValueChanged(this, e);
            chkCWKeyerRevPdl_CheckedChanged(this, e);
            chkCWKeyerIambic_CheckedChanged(this, e);
            udCWKeyerWeight_ValueChanged(this, e);
            chkStrictCharSpacing_CheckedChanged(this, e);
            chkCWKeyerMode_CheckedChanged(this, e);
            chkDSPKeyerSidetone_CheckedChanged(this, e);
            chkDSPCESSB_CheckedChanged(this, e);
            udRXAMSQMaxTail_ValueChanged(this, e);
            radANFPreAGC_CheckedChanged(this, e);
            radANF2PreAGC_CheckedChanged(this, e);
            chkMNFAutoIncrease_CheckedChanged(this, e);
            //AGC
            udDSPAGCFixedGaindB_ValueChanged(this, e);
            udDSPAGCMaxGaindB_ValueChanged(this, e);
            udDSPAGCSlope_ValueChanged(this, e);
            udDSPAGCRX2Slope_ValueChanged(this, e);
            udDSPAGCDecay_ValueChanged(this, e);
            udDSPAGCHangTime_ValueChanged(this, e);
            tbDSPAGCHangThreshold_Scroll(this, e);
            udDSPAGCRX2FixedGaindB_ValueChanged(this, e);
            udDSPAGCRX2MaxGaindB_ValueChanged(this, e);
            udDSPAGCRX2Slope_ValueChanged(this, e);
            udDSPAGCRX2Decay_ValueChanged(this, e);
            udDSPAGCRX2HangTime_ValueChanged(this, e);
            tbDSPAGCRX2HangThreshold_Scroll(this, e);
            //Leveler
            chkDSPLevelerEnabled_CheckedChanged(this, e);
            udDSPLevelerThreshold_ValueChanged(this, e);
            udDSPLevelerDecay_ValueChanged(this, e);
            //ALC
            udDSPALCMaximumGain_ValueChanged(this, e);
            udDSPALCDecay_ValueChanged(this, e);
            // AM/SAM Tab
            chkLevelFades_CheckedChanged(this, e);
            chkRX2LevelFades_CheckedChanged(this, e);
            radLSBUSB_CheckedChanged(this, e);
            radLSB_CheckedChanged(this, e);
            radUSB_CheckedChanged(this, e);
            radRX2LSBUSB_CheckedChanged(this, e);
            radRX2LSB_CheckedChanged(this, e);
            radRX2USB_CheckedChanged(this, e);
            chkCBlock_CheckedChanged(this, e);
            chkRX2CBlock_CheckedChanged(this, e);
            radTXDSB_CheckedChanged(this, e);
            // FM Tab
            chkEmphPos_CheckedChanged(this, e);
            chkRemoveTone_CheckedChanged(this, e);
            // EER Tab
            chkDSPEERon_CheckedChanged(this, e);
            udDSPEERmgain_ValueChanged(this, e);
            udDSPEERpgain_ValueChanged(this, e);
            chkDSPEERRunDelays_CheckedChanged(this, e);
            udDSPEERmdelay_ValueChanged(this, e);
            udDSPEERpdelay_ValueChanged(this, e);
            chkDSPEERamIQ_CheckedChanged(this, e);
            udDSPEERpwmMax_ValueChanged(this, e);
            udDSPEERpwmMin_ValueChanged(this, e);
            // NR Tab
            radDSPNR2Linear_CheckedChanged(this, e);
            radDSPNR2Log_CheckedChanged(this, e);
            radDSPNR2OSMS_CheckedChanged(this, e);
            radDSPNR2MMSE_CheckedChanged(this, e);
            chkDSPNR2AE_CheckedChanged(this, e);
            radDSPNR2LinearRX2_CheckedChanged(this, e);
            radDSPNR2LogRX2_CheckedChanged(this, e);
            radDSPNR2OSMSRX2_CheckedChanged(this, e);
            radDSPNR2MMSERX2_CheckedChanged(this, e);
            chkDSPNR2AERX2_CheckedChanged(this, e);
            // Transmit Tab
            udTXFilterHigh_ValueChanged(this, e);
            udTXFilterLow_ValueChanged(this, e);
            udTransmitTunePower_ValueChanged(this, e);
            chkTXTunePower_CheckedChanged(this, e);
            udPAGain_ValueChanged(this, e);
            radMicIn_CheckedChanged(this, e);
            radLineIn_CheckedChanged(this, e);
            udMicGainMax_ValueChanged(this, e);
            udMicGainMin_ValueChanged(this, e);
            udLineInBoost_ValueChanged(this, e);
            udTXAMCarrierLevel_ValueChanged(this, e);
            chkLimitExtAmpOnOverload_CheckedChanged(this, e);
            chkBPF2Gnd_CheckedChanged(this, e);
            // Keyboard Tab
            comboKBTuneUp1_SelectedIndexChanged(this, e);
            comboKBTuneUp2_SelectedIndexChanged(this, e);
            comboKBTuneUp3_SelectedIndexChanged(this, e);
            comboKBTuneUp4_SelectedIndexChanged(this, e);
            comboKBTuneUp5_SelectedIndexChanged(this, e);
            comboKBTuneUp6_SelectedIndexChanged(this, e);
            comboKBTuneDown1_SelectedIndexChanged(this, e);
            comboKBTuneDown2_SelectedIndexChanged(this, e);
            comboKBTuneDown3_SelectedIndexChanged(this, e);
            comboKBTuneDown4_SelectedIndexChanged(this, e);
            comboKBTuneDown5_SelectedIndexChanged(this, e);
            comboKBTuneDown6_SelectedIndexChanged(this, e);
            comboKBBandUp_SelectedIndexChanged(this, e);
            comboKBBandDown_SelectedIndexChanged(this, e);
            comboKBFilterUp_SelectedIndexChanged(this, e);
            comboKBFilterDown_SelectedIndexChanged(this, e);
            comboKBModeUp_SelectedIndexChanged(this, e);
            comboKBModeDown_SelectedIndexChanged(this, e);
            comboKBCWDash_SelectedIndexChanged(this, e);
            comboKBCWDot_SelectedIndexChanged(this, e);
            comboKBPTTTx_SelectedIndexChanged(this, e);
            comboKBPTTRx_SelectedIndexChanged(this, e);
            // Appearance Tab
            clrbtnBtnSel_Changed(this, e);
            clrbtnVFODark_Changed(this, e);
            clrbtnVFOLight_Changed(this, e);
            clrbtnBandDark_Changed(this, e);
            clrbtnBandLight_Changed(this, e);
            clrbtnPeakText_Changed(this, e);
            clrbtnBackground_Changed(this, e);
            clrbtnTXBackground_Changed(this, e);
            clrbtnGrid_Changed(this, e);
            clrbtnTXVGrid_Changed(this, e);
            clrbtnGridFine_Changed(this, e);
            clrbtnHGridColor_Changed(this, e);
            clrbtnTXHGridColor_Changed(this, e);
            clrbtnZeroLine_Changed(this, e);
            clrbtnTXZeroLine_Changed(this, e);
            clrbtnFilter_Changed(this, e);
            clrbtnGridTXFilter_Changed(this, e);
            clrbtnText_Changed(this, e);
            clrbtnDataLine_Changed(this, e);
            udDisplayLineWidth_ValueChanged(this, e);
            udTXLineWidth_ValueChanged(this, e);
            clrbtnTXDataLine_Changed(this, e);
            clrbtnMeterLeft_Changed(this, e);
            clrbtnMeterRight_Changed(this, e);
            chkGridControl_CheckedChanged(this, e);
            clrbtnBandEdge_Changed(this, e);
            clrbtnTXBandEdge_Changed(this, e);
            tbDisplayFFTSize_Scroll(this, e);
            tbRX2DisplayFFTSize_Scroll(this, e);
            //radDisplayWindow_CheckedChanged(this, e);
            comboDispPanDetector_SelectedIndexChanged(this, e);
            comboDispWFDetector_SelectedIndexChanged(this, e);
            comboDispPanAveraging_SelectedIndexChanged(this, e);
            comboDispWFAveraging_SelectedIndexChanged(this, e);
            udDisplayAVTimeWF_ValueChanged(this, e);
            comboRX2DispPanDetector_SelectedIndexChanged(this, e);
            comboRX2DispPanAveraging_SelectedIndexChanged(this, e);
            comboRX2DispWFDetector_SelectedIndexChanged(this, e);
            comboRX2DispWFAveraging_SelectedIndexChanged(this, e);
            udRX2DisplayWFAVTime_ValueChanged(this, e);
            chkDispRX2Normalize_CheckedChanged(this, e);
            chkDispNormalize_CheckedChanged(this, e);
            comboTXDispPanDetector_SelectedIndexChanged(this, e);
            comboTXDispPanAveraging_SelectedIndexChanged(this, e);
            udTXDisplayAVGTime_ValueChanged(this, e);
            chkDispTXNormalize_CheckedChanged(this, e);
            tbTXDisplayFFTSize_Scroll(this, e);
            comboTXDispWFDetector_SelectedIndexChanged(this, e);
            comboTXDispWFAveraging_SelectedIndexChanged(this, e);
            udTXDisplayAVTime_ValueChanged(this, e);
            comboTXDispWinType_SelectedIndexChanged(this, e);
            comboDispWinType_SelectedIndexChanged(this, e);
            comboRX2DispWinType_SelectedIndexChanged(this, e);
            udDSPNBTransition_ValueChanged(this, e);
            udDSPNBLead_ValueChanged(this, e);
            udDSPNBLag_ValueChanged(this, e);

            // RX2 tab
            chkRX2AutoMuteTX_CheckedChanged(this, e);
            udMoxDelay_ValueChanged(this, e);
            udCWKeyUpDelay_ValueChanged(this, e);
            console.psform.ForcePS();

            // APF

            chkDSPRX1APFEnable_CheckedChanged(this, e);
            chkDSPRX1subAPFEnable_CheckedChanged(this, e);
            chkDSPRX2APFEnable_CheckedChanged(this, e);
            tbDSPAudRX1APFGain_ValueChanged(this, e);
            tbDSPAudRX1subAPFGain_ValueChanged(this, e);
            tbDSPAudRX2APFGain_ValueChanged(this, e);
            tbRX1APFTune_Scroll(this, e);
            tbRX1subAPFTune_Scroll(this, e);
            tbRX2APFTune_Scroll(this, e);
            tbRX1APFBW_Scroll(this, e);
            tbRX1subAPFBW_Scroll(this, e);
            tbRX2APFBW_Scroll(this, e);
            radDSPRX1APFControls_CheckedChanged(this, e);
            radDSPRX1subAPFControls_CheckedChanged(this, e);
            radDSPRX2APFControls_CheckedChanged(this, e);

            // dolly filter
            chkDSPRX1DollyEnable_CheckedChanged(this, e);
            chkDSPRX1DollySubEnable_CheckedChanged(this, e);
            chkDSPRX2DollyEnable_CheckedChanged(this, e);
            udDSPRX1DollyF0_ValueChanged(this, e);
            udDSPRX1SubDollyF0_ValueChanged(this, e);
            udDSPRX2DollyF0_ValueChanged(this, e);
            udDSPRX1DollyF1_ValueChanged(this, e);
            udDSPRX1SubDollyF1_ValueChanged(this, e);
            udDSPRX2DollyF1_ValueChanged(this, e);
            // CAT
            comboFocusMasterMode_SelectedIndexChanged(this, e);
            // SNB
            udDSPSNBThresh1_ValueChanged(this, e);
            udDSPSNBThresh2_ValueChanged(this, e);
            // MNF
            chkMNFAutoIncrease_CheckedChanged(this, e);

			chkEnableXVTRHF_CheckedChanged(this, e);

            // CFCompressor
            chkCFCEnable_CheckedChanged(this, e);
            setCFCProfile(this, e);
            tbCFCPRECOMP_Scroll(this, e);
            chkCFCPeqEnable_CheckedChanged(this, e);
            tbCFCPEG_Scroll(this, e);

            // Phase Rotator
            chkPHROTEnable_CheckedChanged(this, e);
            udPhRotFreq_ValueChanged(this, e);
            udPHROTStages_ValueChanged(this, e);
        
            // TXEQ
            console.EQForm.setTXEQProfile(this, e);

            //ADC assignment
            radDDCADC_CheckedChanged(this, e);

            chkWheelReverse_CheckedChanged(this, e);
            // IVAC
            chkVAC1_Force_CheckedChanged(this, e);
            chkVAC1_Force2_CheckedChanged(this, e);
        }

        public string[] GetTXProfileStrings()
        {
            string[] s = new string[comboTXProfileName.Items.Count];
            for (int i = 0; i < comboTXProfileName.Items.Count; i++)
                s[i] = (string)comboTXProfileName.Items[i];
            return s;
        }

        public string TXProfile
        {
            get
            {
                return comboTXProfileName != null ? comboTXProfileName.Text : "";
            }
            set { if (comboTXProfileName != null) comboTXProfileName.Text = value; }
        }

        public void GetTxProfiles()
        {
            comboTXProfileName.Items.Clear();
            foreach (DataRow dr in DB.ds.Tables["TxProfile"].Rows)
            {
                if (dr.RowState != DataRowState.Deleted)
                {
                    if (!comboTXProfileName.Items.Contains(dr["Name"]))
                        comboTXProfileName.Items.Add(dr["Name"]);
                }
            }
        }

        public void GetTxProfileDefs()
        {
            lstTXProfileDef.Items.Clear();
            foreach (DataRow dr in DB.ds.Tables["TxProfileDef"].Rows)
            {
                if (dr.RowState != DataRowState.Deleted)
                {
                    if (!lstTXProfileDef.Items.Contains(dr["Name"]))
                        lstTXProfileDef.Items.Add(dr["name"]);
                }
            }
        }

        private bool CheckTXProfileChanged()
        {
            DataRow[] rows = DB.ds.Tables["TxProfile"].Select(
                "'" + current_profile + "' = Name");

            if (rows.Length != 1)
                return false;

            int[] eq = console.EQForm.TXEQ;
            if (eq[0] != (int)rows[0]["TXEQPreamp"])
                return true;

            if (console.EQForm.TXEQEnabled != (bool)rows[0]["TXEQEnabled"])
                return true;

            for (int i = 1; i < 11; i++)
            {
                if (eq[i] != (int)rows[0]["TXEQ" + i])
                    return true;
            }

            if (udTXFilterLow.Value != (int)rows[0]["FilterLow"] ||
                udTXFilterHigh.Value != (int)rows[0]["FilterHigh"] ||
                console.CPDR != (bool)rows[0]["CompanderOn"] ||
                console.CPDRLevel != (int)rows[0]["CompanderLevel"] ||
                console.Mic != (int)rows[0]["MicGain"])
                return true;

            return false;
        }

        public void SaveTXProfileData() //W4TME
        {
            if (profile_deleted == true)
            {
                profile_deleted = false;
                return;
            }

            string name = current_profile;

            DataRow dr = null;

            foreach (DataRow d in DB.ds.Tables["TxProfile"].Rows)
            {
                if ((string)d["Name"] == name)
                {
                    dr = d;
                    break;
                }
            }

            dr["FilterLow"] = (int)udTXFilterLow.Value;
            dr["FilterHigh"] = (int)udTXFilterHigh.Value;
            dr["TXEQEnabled"] = console.EQForm.TXEQEnabled;
            dr["TXEQNumBands"] = console.EQForm.NumBands;
            int[] eq = console.EQForm.TXEQ;
            dr["TXEQPreamp"] = eq[0];
            for (int i = 1; i < 11; i++)
                dr["TXEQ" + i.ToString()] = eq[i];
            for (int i = 11; i < 21; i++)
                dr["TxEqFreq" + (i - 10).ToString()] = eq[i];

            dr["DXOn"] = console.DX;
            dr["DXLevel"] = console.DXLevel;
            dr["CompanderOn"] = console.CPDR;
            dr["CompanderLevel"] = console.CPDRLevel;

            dr["MicGain"] = console.Mic;
            dr["FMMicGain"] = console.FMMic;

            dr["Lev_On"] = chkDSPLevelerEnabled.Checked;
            dr["Lev_Slope"] = (int)udDSPLevelerSlope.Value;
            dr["Lev_MaxGain"] = (int)udDSPLevelerThreshold.Value;
            dr["Lev_Attack"] = (int)udDSPLevelerAttack.Value;
            dr["Lev_Decay"] = (int)udDSPLevelerDecay.Value;
            dr["Lev_Hang"] = (int)udDSPLevelerHangTime.Value;
            dr["Lev_HangThreshold"] = tbDSPLevelerHangThreshold.Value;

            dr["ALC_Slope"] = (int)udDSPALCSlope.Value;
            dr["ALC_MaximumGain"] = (int)udDSPALCMaximumGain.Value;
            dr["ALC_Attack"] = (int)udDSPALCAttack.Value;
            dr["ALC_Decay"] = (int)udDSPALCDecay.Value;
            dr["ALC_Hang"] = (int)udDSPALCHangTime.Value;
            dr["ALC_HangThreshold"] = tbDSPALCHangThreshold.Value;

            dr["Power"] = console.PWR;

            dr["Dexp_On"] = chkTXNoiseGateEnabled.Checked;
            dr["Dexp_Threshold"] = (int)udTXNoiseGate.Value;
            dr["Dexp_Attenuate"] = (int)udTXNoiseGateAttenuate.Value;

            dr["VOX_On"] = chkTXVOXEnabled.Checked;
            dr["VOX_Threshold"] = (int)udTXVOXThreshold.Value;
            dr["VOX_HangTime"] = (int)udTXVOXHangTime.Value;
            dr["Tune_Power"] = (int)udTXTunePower.Value;
            dr["Tune_Meter_Type"] = (string)comboTXTUNMeter.Text;

            // dr["TX_Limit_Slew"] = (bool)chkTXLimitSlew.Checked;

            dr["TX_AF_Level"] = console.TXAF;

            dr["AM_Carrier_Level"] = (int)udTXAMCarrierLevel.Value;

            dr["Show_TX_Filter"] = (bool)console.ShowTXFilter;

            dr["VAC1_On"] = (bool)chkAudioEnableVAC.Checked;
            dr["VAC1_Auto_On"] = (bool)chkAudioVACAutoEnable.Checked;
            dr["VAC1_RX_GAIN"] = (int)udAudioVACGainRX.Value;
            dr["VAC1_TX_GAIN"] = (int)udAudioVACGainTX.Value;
            dr["VAC1_Stereo_On"] = (bool)chkAudio2Stereo.Checked;
            dr["VAC1_Sample_Rate"] = (string)comboAudioSampleRate2.Text;
            dr["VAC1_Buffer_Size"] = (string)comboAudioBuffer2.Text;
            dr["VAC1_IQ_Output"] = (bool)chkAudioIQtoVAC.Checked;
            dr["VAC1_IQ_Correct"] = (bool)chkAudioCorrectIQ.Checked;
            dr["VAC1_PTT_OverRide"] = (bool)chkVACAllowBypass.Checked;
            dr["VAC1_Combine_Input_Channels"] = (bool)chkVACCombine.Checked;
            dr["VAC1_Latency_On"] = true;
            dr["VAC1_Latency_Duration"] = (int)udAudioLatency2.Value;

            dr["VAC2_On"] = (bool)chkVAC2Enable.Checked;
            dr["VAC2_Auto_On"] = (bool)chkVAC2AutoEnable.Checked;
            dr["VAC2_RX_GAIN"] = (int)udVAC2GainRX.Value;
            dr["VAC2_TX_GAIN"] = (int)udVAC2GainTX.Value;
            dr["VAC2_Stereo_On"] = (bool)chkAudioStereo3.Checked;
            dr["VAC2_Sample_Rate"] = (string)comboAudioSampleRate3.Text;
            dr["VAC2_Buffer_Size"] = (string)comboAudioBuffer3.Text;
            dr["VAC2_IQ_Output"] = (bool)chkVAC2DirectIQ.Checked;
            dr["VAC2_IQ_Correct"] = (bool)chkVAC2DirectIQCal.Checked;
            dr["VAC2_Combine_Input_Channels"] = (bool)chkVAC2Combine.Checked;
            dr["VAC2_Latency_On"] = true;
            dr["VAC2_Latency_Duration"] = (int)udVAC2Latency.Value;

            dr["Phone_RX_DSP_Buffer"] = (string)comboDSPPhoneRXBuf.Text;
            dr["Phone_TX_DSP_Buffer"] = (string)comboDSPPhoneTXBuf.Text;
            dr["FM_RX_DSP_Buffer"] = (string)comboDSPFMRXBuf.Text;
            dr["FM_TX_DSP_Buffer"] = (string)comboDSPFMTXBuf.Text;
            dr["Digi_RX_DSP_Buffer"] = (string)comboDSPDigRXBuf.Text;
            dr["Digi_TX_DSP_Buffer"] = (string)comboDSPDigTXBuf.Text;
            dr["CW_RX_DSP_Buffer"] = (string)comboDSPCWRXBuf.Text;

            dr["Phone_RX_DSP_Filter_Size"] = (string)comboDSPPhoneRXFiltSize.Text;
            dr["Phone_TX_DSP_Filter_Size"] = (string)comboDSPPhoneTXFiltSize.Text;
            dr["FM_RX_DSP_Filter_Size"] = (string)comboDSPFMRXFiltSize.Text;
            dr["FM_TX_DSP_Filter_Size"] = (string)comboDSPFMTXFiltSize.Text;
            dr["Digi_RX_DSP_Filter_Size"] = (string)comboDSPDigRXFiltSize.Text;
            dr["Digi_TX_DSP_Filter_Size"] = (string)comboDSPDigTXFiltSize.Text;
            dr["CW_RX_DSP_Filter_Size"] = (string)comboDSPCWRXFiltSize.Text;

            dr["Phone_RX_DSP_Filter_Type"] = (string)comboDSPPhoneRXFiltType.Text;
            dr["Phone_TX_DSP_Filter_Type"] = (string)comboDSPPhoneTXFiltType.Text;
            dr["FM_RX_DSP_Filter_Type"] = (string)comboDSPFMRXFiltType.Text;
            dr["FM_TX_DSP_Filter_Type"] = (string)comboDSPFMTXFiltType.Text;
            dr["Digi_RX_DSP_Filter_Type"] = (string)comboDSPDigRXFiltType.Text;
            dr["Digi_TX_DSP_Filter_Type"] = (string)comboDSPDigTXFiltType.Text;
            dr["CW_RX_DSP_Filter_Type"] = (string)comboDSPCWRXFiltType.Text;

            dr["Mic_Input_On"] = (bool)radMicIn.Checked;
            dr["Mic_Input_Boost"] = (bool)chk20dbMicBoost.Checked;
            dr["Line_Input_On"] = (bool)radLineIn.Checked;
            dr["Line_Input_Level"] = udLineInBoost.Value;

            dr["CESSB_On"] = chkDSPCESSB.Checked;
            dr["Pure_Signal_Enabled"] = console.PureSignalEnabled; 

            //CFC
            dr["CFCEnabled"] = chkCFCEnable.Checked;
            dr["CFCPostEqEnabled"] = chkCFCPeqEnable.Checked;
            dr["CFCPhaseRotatorEnabled"] = chkPHROTEnable.Checked;
            dr["CFCPhaseRotatorFreq"] = (int)udPhRotFreq.Value;
            dr["CFCPhaseRotatorStages"] = (int)udPHROTStages.Value;
            int[] cfceq = CFCCOMPEQ;
            dr["CFCPreComp"] = cfceq[0];
            for (int i = 1; i < 11; i++)
                dr["CFCPreComp" + (i - 1).ToString()] = cfceq[i];
            dr["CFCPostEqGain"] = cfceq[11];
            for (int i = 12; i < 22; i++)
                dr["CFCPostEqGain" + (i - 12).ToString()] = cfceq[i];
            for (int i = 22; i < 32; i++)
                dr["CFCEqFreq" + (i - 22).ToString()] = cfceq[i];

        }

        public void UpdateWaterfallBandInfo()
        {
            if (!initializing)
            {
                switch (console.RX1Band)
                {
                    case Band.B160M: txtWaterFallBandLevel.Text = "160 meters"; break;
                    case Band.B80M: txtWaterFallBandLevel.Text = "80 meters"; break;
                    case Band.B60M: txtWaterFallBandLevel.Text = "60 meters"; break;
                    case Band.B40M: txtWaterFallBandLevel.Text = "40 meters"; break;
                    case Band.B30M: txtWaterFallBandLevel.Text = "30 meters"; break;
                    case Band.B20M: txtWaterFallBandLevel.Text = "20 meters"; break;
                    case Band.B17M: txtWaterFallBandLevel.Text = "17 meters"; break;
                    case Band.B15M: txtWaterFallBandLevel.Text = "15 meters"; break;
                    case Band.B12M: txtWaterFallBandLevel.Text = "12 meters"; break;
                    case Band.B10M: txtWaterFallBandLevel.Text = "10 meters"; break;
                    case Band.B6M: txtWaterFallBandLevel.Text = "6 meters"; break;
                    case Band.WWV: txtWaterFallBandLevel.Text = "WWV"; break;
                    case Band.BLMF: txtWaterFallBandLevel.Text = "LW/MW bands"; break; // ke9ns add
                    case Band.B120M: txtWaterFallBandLevel.Text = "120 meters"; break;
                    case Band.B90M: txtWaterFallBandLevel.Text = "90 meters"; break;
                    case Band.B61M: txtWaterFallBandLevel.Text = "60 meters"; break;
                    case Band.B49M: txtWaterFallBandLevel.Text = "49 meters"; break;
                    case Band.B41M: txtWaterFallBandLevel.Text = "41 meters"; break;
                    case Band.B31M: txtWaterFallBandLevel.Text = "31 meters"; break;
                    case Band.B25M: txtWaterFallBandLevel.Text = "25 meters"; break;
                    case Band.B22M: txtWaterFallBandLevel.Text = "22 meters"; break;
                    case Band.B19M: txtWaterFallBandLevel.Text = "19 meters"; break;
                    case Band.B16M: txtWaterFallBandLevel.Text = "16 meters"; break;
                    case Band.B14M: txtWaterFallBandLevel.Text = "14 meters"; break;
                    case Band.B13M: txtWaterFallBandLevel.Text = "13 meters"; break;
                    case Band.B11M: txtWaterFallBandLevel.Text = "11 meters"; break;

                    case Band.GEN: txtWaterFallBandLevel.Text = "General"; break;
                    default: txtWaterFallBandLevel.Text = "2M & VHF+"; break;
                }
                switch (console.RX2Band)
                {
                    case Band.B160M: txtRX2WaterFallBandLevel.Text = "160 meters"; break;
                    case Band.B80M: txtRX2WaterFallBandLevel.Text = "80 meters"; break;
                    case Band.B60M: txtRX2WaterFallBandLevel.Text = "60 meters"; break;
                    case Band.B40M: txtRX2WaterFallBandLevel.Text = "40 meters"; break;
                    case Band.B30M: txtRX2WaterFallBandLevel.Text = "30 meters"; break;
                    case Band.B20M: txtRX2WaterFallBandLevel.Text = "20 meters"; break;
                    case Band.B17M: txtRX2WaterFallBandLevel.Text = "17 meters"; break;
                    case Band.B15M: txtRX2WaterFallBandLevel.Text = "15 meters"; break;
                    case Band.B12M: txtRX2WaterFallBandLevel.Text = "12 meters"; break;
                    case Band.B10M: txtRX2WaterFallBandLevel.Text = "10 meters"; break;
                    case Band.B6M: txtRX2WaterFallBandLevel.Text = "6 meters"; break;
                    case Band.WWV: txtRX2WaterFallBandLevel.Text = "WWV"; break;
                    case Band.GEN: txtRX2WaterFallBandLevel.Text = "General"; break;
                    default: txtRX2WaterFallBandLevel.Text = "2M & VHF+"; break;
                }
            }
        }

        public void UpdateDisplayGridBandInfo()
        {
            if (!initializing)
            {
                switch (console.RX1Band)
                {
                    case Band.B160M: txtDisplayGridBandLevel.Text = "160 meters"; break;
                    case Band.B80M: txtDisplayGridBandLevel.Text = "80 meters"; break;
                    case Band.B60M: txtDisplayGridBandLevel.Text = "60 meters"; break;
                    case Band.B40M: txtDisplayGridBandLevel.Text = "40 meters"; break;
                    case Band.B30M: txtDisplayGridBandLevel.Text = "30 meters"; break;
                    case Band.B20M: txtDisplayGridBandLevel.Text = "20 meters"; break;
                    case Band.B17M: txtDisplayGridBandLevel.Text = "17 meters"; break;
                    case Band.B15M: txtDisplayGridBandLevel.Text = "15 meters"; break;
                    case Band.B12M: txtDisplayGridBandLevel.Text = "12 meters"; break;
                    case Band.B10M: txtDisplayGridBandLevel.Text = "10 meters"; break;
                    case Band.B6M: txtDisplayGridBandLevel.Text = "6 meters"; break;
                    case Band.WWV: txtDisplayGridBandLevel.Text = "WWV"; break;
                    case Band.GEN: txtDisplayGridBandLevel.Text = "General"; break;
                    default: txtDisplayGridBandLevel.Text = "2M & VHF+"; break;
                }
                switch (console.RX2Band)
                {
                    case Band.B160M: txtRX2DisplayGridBandLevel.Text = "160 meters"; break;
                    case Band.B80M: txtRX2DisplayGridBandLevel.Text = "80 meters"; break;
                    case Band.B60M: txtRX2DisplayGridBandLevel.Text = "60 meters"; break;
                    case Band.B40M: txtRX2DisplayGridBandLevel.Text = "40 meters"; break;
                    case Band.B30M: txtRX2DisplayGridBandLevel.Text = "30 meters"; break;
                    case Band.B20M: txtRX2DisplayGridBandLevel.Text = "20 meters"; break;
                    case Band.B17M: txtRX2DisplayGridBandLevel.Text = "17 meters"; break;
                    case Band.B15M: txtRX2DisplayGridBandLevel.Text = "15 meters"; break;
                    case Band.B12M: txtRX2DisplayGridBandLevel.Text = "12 meters"; break;
                    case Band.B10M: txtRX2DisplayGridBandLevel.Text = "10 meters"; break;
                    case Band.B6M: txtRX2DisplayGridBandLevel.Text = "6 meters"; break;
                    case Band.WWV: txtRX2DisplayGridBandLevel.Text = "WWV"; break;
                    case Band.GEN: txtRX2DisplayGridBandLevel.Text = "General"; break;
                    default: txtRX2DisplayGridBandLevel.Text = "2M & VHF+"; break;
                }
            }
        }
        #endregion

        #region Properties

        public FocusMasterMode FocusMasterMode
        {
            set
            {
                switch (value)
                {
                    case FocusMasterMode.None:
                        comboFocusMasterMode.Text = "None";
                        break;
                    case FocusMasterMode.Logger:
                        comboFocusMasterMode.Text = "N1MM+ Logger";
                        break;
                    case FocusMasterMode.Click:
                        comboFocusMasterMode.Text = "Select by Click";
                        break;
                    case FocusMasterMode.Title:
                        comboFocusMasterMode.Text = "Enter Window Title";
                        break;
                }
            }
        }

        public string FocusMasterTitle
        {
            set
            {
                txtFocusMasterWinTitle.Text = value;
            }
        }

        public bool EnableRX1APFControl
        {
            set
            {
                chkDSPRX1APFEnable.Enabled = value;
                chkDSPRX1subAPFEnable.Enabled = value;
            }
        }

        public bool EnableRX2APFControl
        {
            set
            {
                chkDSPRX2APFEnable.Enabled = value;
            }
        }

        public float RX6mGainOffset
        {
            get
            {
                if (ud6mLNAGainOffset != null) return (float)ud6mLNAGainOffset.Value;
                else return -1;
            }
            set
            {
                if (ud6mLNAGainOffset != null) ud6mLNAGainOffset.Value = (decimal)value;
            }
        }

        public bool AlexTRRelay
        {
            get
            {
                if (chkHFTRRelay != null) return chkHFTRRelay.Checked;
                else return false;
            }
            set
            {
                if (chkHFTRRelay != null)
                    chkHFTRRelay.Checked = value;
            }
        }

        public bool HermesEnableAttenuator
        {
            get
            {
                if (chkHermesStepAttenuator != null) return chkHermesStepAttenuator.Checked;
                else return false;
            }
            set
            {
                if (chkHermesStepAttenuator != null) chkHermesStepAttenuator.Checked = value;
            }
        }

        public int HermesAttenuatorData
        {
            get
            {
                if (udHermesStepAttenuatorData != null) return (int)udHermesStepAttenuatorData.Value;
                else return -1;
            }
            set
            {
                if (udHermesStepAttenuatorData != null) udHermesStepAttenuatorData.Value = value;
            }
        }

        public bool RX2EnableAtt
        {
            get
            {
                if (chkRX2StepAtt != null) return chkRX2StepAtt.Checked;
                else return false;
            }
            set
            {
                if (chkRX2StepAtt != null)
                {
                    chkRX2StepAtt.Checked = value;
                    //chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public int ATTOnTX
        {
            get
            {
                if (udATTOnTX != null) return (int)udATTOnTX.Value;
                else return -1;
            }
            set
            {
                if (udATTOnTX != null)
                {
                    if (value > 31) value = 31;
                    udATTOnTX.Value = value;
                }
            }
        }

        public string SerialNumber
        {
            get { return txtZZSN.Text; }

        }

        public int VACDriver
        {
            get
            {
                return comboAudioDriver2.SelectedIndex;
            }
            set
            {
                if ((comboAudioDriver2.Items.Count - 1) > value)
                {
                    comboAudioDriver2.SelectedIndex = value;
                }
            }
        }

        public int VAC2Driver
        {
            get
            {
                return comboAudioDriver3.SelectedIndex;
            }
            set
            {
                if ((comboAudioDriver3.Items.Count - 1) > value)
                {
                    comboAudioDriver3.SelectedIndex = value;
                }
            }
        }

        public int VACInputCable
        {
            get
            {
                return comboAudioInput2.SelectedIndex;
            }
            set
            {
                if ((comboAudioInput2.Items.Count - 1) > value)
                {
                    comboAudioInput2.SelectedIndex = value;
                }
            }
        }

        public int VAC2InputCable
        {
            get
            {
                return comboAudioInput3.SelectedIndex;
            }
            set
            {
                if ((comboAudioInput3.Items.Count - 1) > value)
                {
                    comboAudioInput3.SelectedIndex = value;
                }
            }
        }

        public int VACOutputCable
        {
            get
            {
                return comboAudioOutput2.SelectedIndex;
            }
            set
            {
                if ((comboAudioOutput2.Items.Count - 1) > value)
                {
                    comboAudioOutput2.SelectedIndex = value;
                }
            }
        }

        public int VAC2OutputCable
        {
            get
            {
                return comboAudioOutput3.SelectedIndex;
            }
            set
            {
                if ((comboAudioOutput3.Items.Count - 1) > value)
                {
                    comboAudioOutput3.SelectedIndex = value;
                }
            }
        }

        public bool VACUseRX2
        {
            get
            {
                if (chkAudioRX2toVAC != null && IQOutToVAC)
                {
                    return chkAudioRX2toVAC.Checked;
                }
                else return false;
            }
            set
            {
                if (chkAudioRX2toVAC != null && IQOutToVAC)
                {
                    chkAudioRX2toVAC.Checked = value;
                }
            }
        }

        public bool VAC2UseRX2
        {
            get
            {
                if (chkVAC2UseRX2 != null && IQOutToVAC)
                {
                    return chkVAC2UseRX2.Checked;
                }
                else return false;
            }
            set
            {
                if (chkVAC2UseRX2 != null && IQOutToVAC)
                {
                    chkVAC2UseRX2.Checked = value;
                }
            }
        }

        public bool CATEnabled
        {
            get
            {
                if (chkCATEnable != null) return chkCATEnable.Checked;
                else return false;
            }
            set
            {
                if (chkCATEnable != null) chkCATEnable.Checked = value;
            }
        }

        public bool CAT2Enabled
        {
            get
            {
                if (chkCAT2Enable != null) return chkCAT2Enable.Checked;
                else return false;
            }
            set
            {
                if (chkCAT2Enable != null) chkCAT2Enable.Checked = value;
            }
        }

        public bool CAT3Enabled
        {
            get
            {
                if (chkCAT3Enable != null) return chkCAT3Enable.Checked;
                else return false;
            }
            set
            {
                if (chkCAT3Enable != null) chkCAT3Enable.Checked = value;
            }
        }

        public bool CAT4Enabled
        {
            get
            {
                if (chkCAT4Enable != null) return chkCAT4Enable.Checked;
                else return false;
            }
            set
            {
                if (chkCAT4Enable != null) chkCAT4Enable.Checked = value;
            }
        }

        public int RXAGCAttack
        {
            get
            {
                if (udDSPAGCAttack != null) return (int)udDSPAGCAttack.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCAttack != null) udDSPAGCAttack.Value = value;
            }
        }

        public int RXAGCHang
        {
            get
            {
                if (udDSPAGCHangTime != null) return (int)udDSPAGCHangTime.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCHangTime != null) udDSPAGCHangTime.Value = value;
            }
        }

        public int RXAGCDecay
        {
            get
            {
                if (udDSPAGCDecay != null) return (int)udDSPAGCDecay.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCDecay != null) udDSPAGCDecay.Value = value;
            }
        }

        public int RX2AGCAttack
        {
            get
            {
                if (udDSPAGCRX2Attack != null) return (int)udDSPAGCRX2Attack.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCRX2Attack != null) udDSPAGCRX2Attack.Value = value;
            }
        }

        public int RX2AGCHang
        {
            get
            {
                if (udDSPAGCRX2HangTime != null) return (int)udDSPAGCRX2HangTime.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCRX2HangTime != null) udDSPAGCRX2HangTime.Value = value;
            }
        }

        public int RX2AGCDecay
        {
            get
            {
                if (udDSPAGCRX2Decay != null) return (int)udDSPAGCRX2Decay.Value;
                else return 0;
            }
            set
            {
                if (udDSPAGCRX2Decay != null) udDSPAGCRX2Decay.Value = value;
            }
        }

        private float display_contrast = 1.0f;
        public float DisplayContrast
        {
            get { return display_contrast; }

            set { display_contrast = value; }
        }

        public int BreakInDelay
        {
            get
            {
                if (udCWBreakInDelay != null) return (int)udCWBreakInDelay.Value;
                else return -1;
            }
            set
            {
                if (udCWBreakInDelay != null)
                {
                    udCWBreakInDelay.Value = value;
                    udCWKeyerSemiBreakInDelay_ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public int CWPitch
        {
            get
            {
                if (udDSPCWPitch != null) return (int)udDSPCWPitch.Value;
                else return -1;
            }
            set
            {
                if (udDSPCWPitch != null) udDSPCWPitch.Value = value;
            }
        }

        public bool CWDisableMonitor
        {
            get
            {
                if (chkDSPKeyerSidetone != null) return chkDSPKeyerSidetone.Checked;
                else return false;
            }
            set
            {
                if (chkDSPKeyerSidetone != null) chkDSPKeyerSidetone.Checked = value;
            }
        }

        public bool CWIambic
        {
            get
            {
                if (chkCWKeyerIambic != null) return chkCWKeyerIambic.Checked;
                else return false;
            }
            set
            {
                if (chkCWKeyerIambic != null) chkCWKeyerIambic.Checked = value;
            }
        }

        public string VACSampleRate
        {
            get
            {
                if (comboAudioSampleRate2 != null) return comboAudioSampleRate2.Text;
                else return "";
            }
            set
            {
                if (comboAudioSampleRate2 != null) comboAudioSampleRate2.Text = value;
            }
        }

        public string VAC2SampleRate
        {
            get
            {
                if (comboAudioSampleRate3 != null) return comboAudioSampleRate3.Text;
                else return "";
            }
            set
            {
                if (comboAudioSampleRate3 != null) comboAudioSampleRate3.Text = value;
            }
        }

        public string VAC1BufferSize
        {
            get
            {
                if (comboAudioBuffer2 != null)
                    return comboAudioBuffer2.Text;
                else return "";
            }
            set
            {
                if (comboAudioBuffer2 != null)
                    comboAudioBuffer2.Text = value;
            }
        }

        public string VAC2BufferSize
        {
            get
            {
                if (comboAudioBuffer3 != null)
                    return comboAudioBuffer3.Text;
                else return "";
            }
            set
            {
                if (comboAudioBuffer3 != null)
                    comboAudioBuffer3.Text = value;
            }
        }

        public bool IQOutToVAC
        {
            get
            {
                if (chkAudioIQtoVAC != null) return chkAudioIQtoVAC.Checked;
                else return false;
            }
            set
            {
                if (chkAudioIQtoVAC != null) chkAudioIQtoVAC.Checked = value;
            }
        }

        public bool VAC2DirectIQ
        {
            get
            {
                if (chkVAC2DirectIQ != null) return chkVAC2DirectIQ.Checked;
                else return false;
            }
            set
            {
                if (chkVAC2DirectIQ != null) chkVAC2DirectIQ.Checked = value;
            }
        }

        public bool VAC1Calibrate
        {
            get
            {
                if (chkAudioCorrectIQ != null) return chkAudioIQtoVAC.Checked;
                else return false;
            }
            set
            {
                if (chkAudioCorrectIQ != null) chkAudioIQtoVAC.Checked = value;
            }
        }

        public bool VAC2Calibrate
        {
            get
            {
                if (chkVAC2DirectIQCal != null) return chkVAC2DirectIQ.Checked;
                else return false;
            }
            set
            {
                if (chkVAC2DirectIQCal != null) chkVAC2DirectIQ.Checked = value;
            }
        }

        public bool VACStereo
        {
            get
            {
                if (chkAudio2Stereo != null) return chkAudio2Stereo.Checked;
                else return false;
            }
            set
            {
                if (chkAudio2Stereo != null) chkAudio2Stereo.Checked = value;
            }
        }

        public bool VAC2Stereo
        {
            get
            {
                if (chkAudioStereo3 != null) return chkAudioStereo3.Checked;
                else return false;
            }
            set
            {
                if (chkAudioStereo3 != null) chkAudioStereo3.Checked = value;
            }
        }

        public int NoiseGate
        {
            get
            {
                if (udTXNoiseGate != null) return (int)udTXNoiseGate.Value;
                else return -1;
            }
            set
            {
                if (udTXNoiseGate != null) udTXNoiseGate.Value = value;
            }
        }

        public int VOXSens
        {
            get
            {
                if (udTXVOXThreshold != null) return (int)udTXVOXThreshold.Value;
                else return -1;
            }
            set
            {
                if (udTXVOXThreshold != null) udTXVOXThreshold.Value = value;
            }
        }

        public bool NoiseGateEnabled
        {
            get
            {
                if (chkTXNoiseGateEnabled != null) return chkTXNoiseGateEnabled.Checked;
                else return false;
            }
            set
            {
                if (chkTXNoiseGateEnabled != null) chkTXNoiseGateEnabled.Checked = value;
            }
        }

        public bool TXLevelerOn
        {
            get
            {
                if (chkDSPLevelerEnabled != null) return chkDSPLevelerEnabled.Checked;
                else return false;
            }
            set
            {
                if (chkDSPLevelerEnabled != null) chkDSPLevelerEnabled.Checked = value;
            }
        }

        public int VACRXGain
        {
            get
            {
                if (udAudioVACGainRX != null) return (int)udAudioVACGainRX.Value;
                else return -99;
            }
            set
            {
                if (udAudioVACGainRX != null) udAudioVACGainRX.Value = value;
            }
        }

        public int VAC2RXGain
        {
            get
            {
                if (udVAC2GainRX != null) return (int)udVAC2GainRX.Value;
                else return -99;
            }
            set
            {
                if (udVAC2GainRX != null) udVAC2GainRX.Value = value;
            }
        }

        public int VACTXGain
        {
            get
            {
                if (udAudioVACGainTX != null) return (int)udAudioVACGainTX.Value;
                else return -99;
            }
            set
            {
                if (udAudioVACGainTX != null) udAudioVACGainTX.Value = value;
            }
        }

        public int VAC2TXGain
        {
            get
            {
                if (udVAC2GainTX != null) return (int)udVAC2GainTX.Value;
                else return -99;
            }
            set
            {
                if (udVAC2GainTX != null) udVAC2GainTX.Value = value;
            }
        }

        public bool BreakInEnabled
        {
            get
            {
                if (chkCWBreakInEnabled != null)
                    return chkCWBreakInEnabled.Checked;
                else return false;
            }
            set
            {
                if (chkCWBreakInEnabled != null)
                    chkCWBreakInEnabled.Checked = value;
            }
        }

        public bool RX1APFEnable
        {
            get
            {
                if (chkDSPRX1APFEnable != null)
                    return chkDSPRX1APFEnable.Checked;
                else return false;
            }
            set
            {
                if (chkDSPRX1APFEnable != null)
                {
                    chkDSPRX1APFEnable.Checked = value;
                    chkDSPRX1APFEnable_CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool RX1subAPFEnable
        {
            get
            {
                if (chkDSPRX1subAPFEnable != null)
                    return chkDSPRX1subAPFEnable.Checked;
                else return false;
            }
            set
            {
                if (chkDSPRX1subAPFEnable != null)
                {
                    chkDSPRX1subAPFEnable.Checked = value;
                    chkDSPRX1subAPFEnable_CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool RX2APFEnable
        {
            get
            {
                if (chkDSPRX2APFEnable != null)
                    return chkDSPRX2APFEnable.Checked;
                else return false;
            }
            set
            {
                if (chkDSPRX2APFEnable != null)
                {
                    chkDSPRX2APFEnable.Checked = value;
                    chkDSPRX2APFEnable_CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public int RX1APFFreq
        {
            get
            {
                if (tbRX1APFTune != null) return (int)tbRX1APFTune.Value;
                else return -1;
            }
            set
            {
                if (tbRX1APFTune != null)
                {
                    tbRX1APFTune.Value = value;
                    tbRX1APFTune_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX1subAPFFreq
        {
            get
            {
                if (tbRX1subAPFTune != null) return (int)tbRX1subAPFTune.Value;
                else return -1;
            }
            set
            {
                if (tbRX1subAPFTune != null)
                {
                    tbRX1subAPFTune.Value = value;
                    tbRX1subAPFTune_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX2APFFreq
        {
            get
            {
                if (tbRX2APFTune != null) return (int)tbRX2APFTune.Value;
                else return -1;
            }
            set
            {
                if (tbRX2APFTune != null)
                {
                    tbRX2APFTune.Value = value;
                    tbRX2APFTune_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX1APFBandwidth
        {
            get
            {
                if (tbRX1APFBW != null) return (int)tbRX1APFBW.Value;
                else return -1;
            }
            set
            {
                if (tbRX1APFBW != null)
                {
                    tbRX1APFBW.Value = value;
                    tbRX1APFBW_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX1subAPFBandwidth
        {
            get
            {
                if (tbRX1subAPFBW != null) return (int)tbRX1subAPFBW.Value;
                else return -1;
            }
            set
            {
                if (tbRX1subAPFBW != null)
                {
                    tbRX1subAPFBW.Value = value;
                    tbRX1subAPFBW_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX2APFBandwidth
        {
            get
            {
                if (tbRX2APFBW != null) return (int)tbRX2APFBW.Value;
                else return -1;
            }
            set
            {
                if (tbRX2APFBW != null)
                {
                    tbRX2APFBW.Value = value;
                    tbRX2APFBW_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX1APFGain
        {
            get
            {
                if (tbDSPAudRX1APFGain != null) return tbDSPAudRX1APFGain.Value;
                else return -1;
            }
            set
            {
                if (tbDSPAudRX1APFGain != null)
                {
                    tbDSPAudRX1APFGain.Value = value;
                }
            }
        }

        public int RX1subAPFGain
        {
            get
            {
                if (tbDSPAudRX1subAPFGain != null) return tbDSPAudRX1subAPFGain.Value;
                else return -1;
            }
            set
            {
                if (tbDSPAudRX1subAPFGain != null)
                {
                    tbDSPAudRX1subAPFGain.Value = value;
                    // tbDSPAudRX1subAPFGain_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public int RX2APFGain
        {
            get
            {
                // if (udDSPAudRX2APFGain != null) return (int)udDSPAudRX2APFGain.Value;
                if (tbDSPAudRX2APFGain != null) return tbDSPAudRX2APFGain.Value;
                else return -1;
            }
            set
            {
                if (tbDSPAudRX2APFGain != null)
                {
                    // udDSPAudRX2APFGain.Value = value;
                    tbDSPAudRX2APFGain.Value = value;
                    // tbDSPAudRX2APFGain_Scroll(this, EventArgs.Empty);
                }
            }
        }

        public bool RX1APFControls
        {
            get
            {
                if (radDSPRX1APFControls != null)
                    return radDSPRX1APFControls.Checked;
                else return false;
            }
        }

        public bool RX1subAPFControls
        {
            get
            {
                if (radDSPRX1subAPFControls != null)
                    return radDSPRX1subAPFControls.Checked;
                else return false;
            }
        }

        public bool RX2APFControls
        {
            get
            {
                if (radDSPRX2APFControls != null)
                    return radDSPRX2APFControls.Checked;
                else return false;
            }
        }


        public bool VOXEnable
        {
            get
            {
                if (chkTXVOXEnabled != null) return chkTXVOXEnabled.Checked;
                else return false;
            }
            set
            {
                if (chkTXVOXEnabled != null) chkTXVOXEnabled.Checked = value;
            }
        }

        public int AGCMaxGain
        {
            get
            {
                if (udDSPAGCMaxGaindB != null) return (int)udDSPAGCMaxGaindB.Value;
                else return -1;
            }
            set
            {
                if (udDSPAGCMaxGaindB != null) udDSPAGCMaxGaindB.Value = value;
            }
        }

        public int AGCFixedGain
        {
            get
            {
                if (udDSPAGCFixedGaindB != null) return (int)udDSPAGCFixedGaindB.Value;
                else return -1;
            }
            set
            {
                if (udDSPAGCFixedGaindB != null) udDSPAGCFixedGaindB.Value = value;
            }
        }

        public int AGCRX2MaxGain
        {
            get
            {
                if (udDSPAGCRX2MaxGaindB != null) return (int)udDSPAGCRX2MaxGaindB.Value;
                else return -1;
            }
            set
            {
                if (udDSPAGCRX2MaxGaindB != null) udDSPAGCRX2MaxGaindB.Value = value;
            }
        }

        public int AGCRX2FixedGain
        {
            get
            {
                if (udDSPAGCRX2FixedGaindB != null) return (int)udDSPAGCRX2FixedGaindB.Value;
                else return -1;
            }
            set
            {
                if (udDSPAGCRX2FixedGaindB != null) udDSPAGCRX2FixedGaindB.Value = value;
            }
        }

        public int AGCHangThreshold
        {
            set
            {
                if (tbDSPAGCHangThreshold != null)
                {
                    tbDSPAGCHangThreshold.Value = value;
                    // tbDSPAGCHangThreshold_Scroll(this, EventArgs.Empty);
                }

            }
        }

        public int SetAGCRX2HangThreshold
        {
            set
            {
                if (tbDSPAGCRX2HangThreshold != null)
                {
                    //tbDSPAGCRX2HangThreshold.Value = value;
                    // tbDSPAGCRX2HangThreshold_Scroll(this, EventArgs.Empty);
                    if (value > tbDSPAGCRX2HangThreshold.Maximum) value = (int)tbDSPAGCRX2HangThreshold.Maximum;
                    if (value < tbDSPAGCRX2HangThreshold.Minimum) value = (int)tbDSPAGCRX2HangThreshold.Minimum;

                    if (tbDSPAGCRX2HangThreshold.Value == 0)
                    {
                        tbDSPAGCRX2HangThreshold.Value = value + 1;
                        tbDSPAGCRX2HangThreshold_Scroll(this, EventArgs.Empty);
                        tbDSPAGCRX2HangThreshold.Value = tbDSPAGCRX2HangThreshold.Value - 1;
                        tbDSPAGCRX2HangThreshold_Scroll(this, EventArgs.Empty);
                    }
                    else
                    {
                        tbDSPAGCRX2HangThreshold.Value = value - 1;
                        tbDSPAGCRX2HangThreshold_Scroll(this, EventArgs.Empty);
                        tbDSPAGCRX2HangThreshold.Value = tbDSPAGCRX2HangThreshold.Value + 1;
                        tbDSPAGCRX2HangThreshold_Scroll(this, EventArgs.Empty);
                    }
                }

            }
        }

        public int SetAGCHangThres
        {
            set
            {
                if (tbDSPAGCHangThreshold != null)
                {
                    if (value > tbDSPAGCHangThreshold.Maximum) value = (int)tbDSPAGCHangThreshold.Maximum;
                    if (value < tbDSPAGCHangThreshold.Minimum) value = (int)tbDSPAGCHangThreshold.Minimum;

                    if (tbDSPAGCHangThreshold.Value == 0)
                    {
                        tbDSPAGCHangThreshold.Value = value + 1;
                        tbDSPAGCHangThreshold_Scroll(this, EventArgs.Empty);
                        tbDSPAGCHangThreshold.Value = tbDSPAGCHangThreshold.Value - 1;
                        tbDSPAGCHangThreshold_Scroll(this, EventArgs.Empty);
                    }
                    else
                    {
                        tbDSPAGCHangThreshold.Value = value - 1;
                        tbDSPAGCHangThreshold_Scroll(this, EventArgs.Empty);
                        tbDSPAGCHangThreshold.Value = tbDSPAGCHangThreshold.Value + 1;
                        tbDSPAGCHangThreshold_Scroll(this, EventArgs.Empty);
                    }
                }
            }
        }

        public int DisplayFFTSize
        {
            get { return (int)tbDisplayFFTSize.Value; }
            set
            {
                if (value > tbDisplayFFTSize.Maximum) value = (int)tbDisplayFFTSize.Maximum;
                if (value < tbDisplayFFTSize.Minimum) value = (int)tbDisplayFFTSize.Minimum;
                tbDisplayFFTSize.Value = value;
                tbDisplayFFTSize_Scroll(this, EventArgs.Empty);
            }
        }

        public int TXFilterHigh
        {
            get { return (int)udTXFilterHigh.Value; }
            set
            {
                if (value > udTXFilterHigh.Maximum) value = (int)udTXFilterHigh.Maximum;
                if (value < udTXFilterHigh.Minimum) value = (int)udTXFilterHigh.Minimum;
                udTXFilterHigh.Value = value;
            }
        }

        public int TXFilterLow
        {
            get { return (int)udTXFilterLow.Value; }
            set
            {
                if (value > udTXFilterLow.Maximum) value = (int)udTXFilterLow.Maximum;
                if (value < udTXFilterLow.Minimum) value = (int)udTXFilterLow.Minimum;
                udTXFilterLow.Value = value;
            }
        }

        public int TXFilterHighSave
        {
            get { return (int)udTXFilterHighSave.Value; }
            set
            {
                if (console.TXBand != Band.B60M)
                {
                    if (value > udTXFilterHighSave.Maximum) value = (int)udTXFilterHighSave.Maximum;
                    if (value < udTXFilterHighSave.Minimum) value = (int)udTXFilterHighSave.Minimum;
                    udTXFilterHighSave.Value = value;
                }
            }
        }

        public int TXFilterLowSave
        {
            get { return (int)udTXFilterLowSave.Value; }
            set
            {
                if (console.TXBand != Band.B60M)
                {
                    if (value > udTXFilterLowSave.Maximum) value = (int)udTXFilterLowSave.Maximum;
                    if (value < udTXFilterLowSave.Minimum) value = (int)udTXFilterLowSave.Minimum;
                    udTXFilterLowSave.Value = value;
                }
            }
        }

        public bool CustomRXAGCEnabled
        {
            set
            {
                udDSPAGCAttack.Enabled = value;
                udDSPAGCDecay.Enabled = value;
                udDSPAGCHangTime.Enabled = value;
                // udDSPAGCRX2Attack.Enabled = value;
                // udDSPAGCRX2Decay.Enabled = value;
                // udDSPAGCRX2HangTime.Enabled = value;

                if (value)
                {
                    console.radio.GetDSPRX(0, 0).Force = true;
                    console.radio.GetDSPRX(0, 1).Force = true;
                    udDSPAGCDecay_ValueChanged(this, EventArgs.Empty);
                    udDSPAGCHangTime_ValueChanged(this, EventArgs.Empty);
                    //  udDSPAGCRX2Attack_ValueChanged(this, EventArgs.Empty);
                    //  udDSPAGCRX2Decay_ValueChanged(this, EventArgs.Empty);
                    //  udDSPAGCRX2HangTime_ValueChanged(this, EventArgs.Empty);
                    console.radio.GetDSPRX(0, 0).Force = false;
                    console.radio.GetDSPRX(0, 1).Force = false;
                }
            }
        }

        public bool CustomRX2AGCEnabled
        {
            set
            {
                // udDSPAGCAttack.Enabled = value;
                // udDSPAGCDecay.Enabled = value;
                // udDSPAGCHangTime.Enabled = value;
                udDSPAGCRX2Attack.Enabled = value;
                udDSPAGCRX2Decay.Enabled = value;
                udDSPAGCRX2HangTime.Enabled = value;

                if (value)
                {
                    console.radio.GetDSPRX(1, 0).Force = true;
                    //   console.radio.GetDSPRX(1, 1).Force = true;
                    // udDSPAGCAttack_ValueChanged(this, EventArgs.Empty);
                    // udDSPAGCDecay_ValueChanged(this, EventArgs.Empty);
                    // udDSPAGCHangTime_ValueChanged(this, EventArgs.Empty);
                    udDSPAGCRX2Decay_ValueChanged(this, EventArgs.Empty);
                    udDSPAGCRX2HangTime_ValueChanged(this, EventArgs.Empty);
                    console.radio.GetDSPRX(1, 0).Force = false;
                    // console.radio.GetDSPRX(1, 1).Force = false;
                }
            }
        }

        public bool VACEnable
        {
            get { return chkAudioEnableVAC.Checked; }
            set { chkAudioEnableVAC.Checked = value; }
        }

        public bool VAC2Enable
        {
            get { return chkVAC2Enable.Checked; }
            set { chkVAC2Enable.Checked = value; }
        }

        private bool force_model = false;
        public Model CurrentModel
        {
            set
            {
                switch (value)
                {
                    case Model.HPSDR:
                        force_model = true;
                        radGenModelHPSDR.Checked = true;
                        break;
                    case Model.HERMES:
                        force_model = true;
                        radGenModelHermes.Checked = true;
                        radGenModelHermes_CheckedChanged(this, EventArgs.Empty);
                        break;
                    case Model.ANAN10:
                        force_model = true;
                        radGenModelANAN10.Checked = true;
                        break;
                    case Model.ANAN10E:
                        force_model = true;
                        radGenModelANAN10E.Checked = true;
                        break;
                    case Model.ANAN100B:
                        force_model = true;
                        radGenModelANAN100B.Checked = true;
                        break;
                    case Model.ANAN100:
                        force_model = true;
                        radGenModelANAN100.Checked = true;
                        break;
                    case Model.ANAN100D:
                        force_model = true;
                        radGenModelANAN100D.Checked = true;
                        break;
                    case Model.ANAN200D:
                        force_model = true;
                        radGenModelANAN200D.Checked = true;
                        break;
                    case Model.ANAN7000D:
                        force_model = true;
                        radGenModelANAN7000D.Checked = true;
                        break;
                    case Model.ANAN8000D:
                        force_model = true;
                        radGenModelANAN8000D.Checked = true;
                        break;

                }
            }
        }

        public bool RXOnly
        {
            get { return chkGeneralRXOnly.Checked; }
            set { chkGeneralRXOnly.Checked = value; }
        }

        private bool mox;
        public bool MOX
        {
            get { return mox; }
            set
            {
                mox = value;
                chkAudioEnableVAC.Enabled = !mox;
                if (chkAudioEnableVAC.Checked)
                {
                    grpAudioDetails2.Enabled = !mox;
                    grpAudioBuffer2.Enabled = !mox;
                    grpAudioLatency2.Enabled = !mox;
                    grpAudioSampleRate2.Enabled = !mox;
                    grpAudio2Stereo.Enabled = !mox;
                }
                else
                {
                    grpAudioDetails2.Enabled = true;
                    grpAudioBuffer2.Enabled = true;
                    grpAudioLatency2.Enabled = true;
                    grpAudioSampleRate2.Enabled = true;
                    grpAudio2Stereo.Enabled = true;
                }
                grpDSPBufferSize.Enabled = !mox;
            }
        }

        public int TXAF
        {
            get { return (int)udTXAF.Value; }
            set { udTXAF.Value = value; }
        }

        public double HPSDRFreqCorrectFactor
        {
            get { return (double)udHPSDRFreqCorrectFactor.Value; }
            set
            {
                udHPSDRFreqCorrectFactor.Value = (decimal)value;
                NetworkIO.FreqCorrectionFactor = (double)value;
            }
        }

        public bool CESSB
        {
            get { return chkDSPCESSB.Checked; }
            set { chkDSPCESSB.Checked = value; }
        }

        public bool AlexPresent
        {
            get { return chkAlexPresent.Checked; }
            set { chkAlexPresent.Checked = value; }
        }

        public bool ExcaliburPresent
        {
            get { return chkExcaliburPresent.Checked; }
            set { chkExcaliburPresent.Checked = value; }
        }

        public bool MercuryPresent
        {
            get { return chkMercuryPresent.Checked; }
            set { chkMercuryPresent.Checked = value; }
        }

        public bool PenelopePresent
        {
            get { return chkPennyPresent.Checked; }
            set { chkPennyPresent.Checked = value; }
        }

        public bool PennyLanePresent
        {
            get { return chkPennyLane.Checked; }
            set { chkPennyLane.Checked = value; }
        }

        //public bool PTTODelayControl
        //{
        //    get { return chkPTTOutDelay.Checked; }
        //    set { chkPTTOutDelay.Checked = value; }
        //}


        public double BPF1_1_5Start
        {
            get { return (double)ud1_5BPF1Start.Value; }
        }

        public double BPF1_1_5End
        {
            get { return (double)ud1_5BPF1End.Value; }
        }

        public double BPF1_6_5Start
        {
            get { return (double)ud6_5BPF1Start.Value; }
        }

        public double BPF1_6_5End
        {
            get { return (double)ud6_5BPF1End.Value; }
        }

        public double BPF1_9_5Start
        {
            get { return (double)ud9_5BPF1Start.Value; }
        }

        public double BPF1_9_5End
        {
            get { return (double)ud9_5BPF1End.Value; }
        }

        public double BPF1_13Start
        {
            get { return (double)ud13BPF1Start.Value; }
        }

        public double BPF1_13End
        {
            get { return (double)ud13BPF1End.Value; }
        }

        public double BPF1_20Start
        {
            get { return (double)ud20BPF1Start.Value; }
        }

        public double BPF1_20End
        {
            get { return (double)ud20BPF1End.Value; }
        }

        public double BPF1_6Start
        {
            get { return (double)ud6BPF1Start.Value; }
        }

        public double BPF1_6End
        {
            get { return (double)ud6BPF1End.Value; }
        }


        public bool BPF1_1_5led
        {
            set { rad1_5BPF1led.Checked = value; }
        }

        public bool BPF1_6_5led
        {
            set { rad6_5BPF1led.Checked = value; }
        }

        public bool BPF1_9_5led
        {
            set { rad9_5BPF1led.Checked = value; }
        }

        public bool BPF1_13led
        {
            set { rad13BPF1led.Checked = value; }
        }

        public bool BPF1_20led
        {
            set { rad20BPF1led.Checked = value; }
        }

        public bool BPF1_6led
        {
            set { rad6BPF1led.Checked = value; }
        }

        public bool BPBPF1led
        {
            set { radBPBPF1led.Checked = value; }
        }

        public bool BPF1BPTXled
        {
            set { radBPF1BPTXled.Checked = value; }
        }


        // PAGain for HPSDR
        public float PAGain160
        {
            get { return (float)udPAGain160.Value; }
            set { udPAGain160.Value = (decimal)value; }
        }

        public float PAGain80
        {
            get { return (float)udPAGain80.Value; }
            set { udPAGain80.Value = (decimal)value; }
        }

        public float PAGain60
        {
            get { return (float)udPAGain60.Value; }
            set { udPAGain60.Value = (decimal)value; }
        }

        public float PAGain40
        {
            get { return (float)udPAGain40.Value; }
            set { udPAGain40.Value = (decimal)value; }
        }

        public float PAGain30
        {
            get { return (float)udPAGain30.Value; }
            set { udPAGain30.Value = (decimal)value; }
        }

        public float PAGain20
        {
            get { return (float)udPAGain20.Value; }
            set { udPAGain20.Value = (decimal)value; }
        }

        public float PAGain17
        {
            get { return (float)udPAGain17.Value; }
            set { udPAGain17.Value = (decimal)value; }
        }

        public float PAGain15
        {
            get { return (float)udPAGain15.Value; }
            set { udPAGain15.Value = (decimal)value; }
        }

        public float PAGain12
        {
            get { return (float)udPAGain12.Value; }
            set { udPAGain12.Value = (decimal)value; }
        }

        public float PAGain10
        {
            get { return (float)udPAGain10.Value; }
            set { udPAGain10.Value = (decimal)value; }
        }

        public float PAGain6
        {
            get { return (float)udPAGain6.Value; }
            set { udPAGain6.Value = (decimal)value; }
        }

        public float PAGainVHF0
        {
            get { return (float)udPAGainVHF0.Value; }
            set { udPAGainVHF0.Value = (decimal)value; }
        }

        public float PAGainVHF1
        {
            get { return (float)udPAGainVHF1.Value; }
            set { udPAGainVHF1.Value = (decimal)value; }
        }

        public float PAGainVHF2
        {
            get { return (float)udPAGainVHF2.Value; }
            set { udPAGainVHF2.Value = (decimal)value; }
        }

        public float PAGainVHF3
        {
            get { return (float)udPAGainVHF3.Value; }
            set { udPAGainVHF3.Value = (decimal)value; }
        }

        public float PAGainVHF4
        {
            get { return (float)udPAGainVHF4.Value; }
            set { udPAGainVHF4.Value = (decimal)value; }
        }

        public float PAGainVHF5
        {
            get { return (float)udPAGainVHF5.Value; }
            set { udPAGainVHF5.Value = (decimal)value; }
        }

        public float PAGainVHF6
        {
            get { return (float)udPAGainVHF6.Value; }
            set { udPAGainVHF6.Value = (decimal)value; }
        }

        public float PAGainVHF7
        {
            get { return (float)udPAGainVHF7.Value; }
            set { udPAGainVHF7.Value = (decimal)value; }
        }

        public float PAGainVHF8
        {
            get { return (float)udPAGainVHF8.Value; }
            set { udPAGainVHF8.Value = (decimal)value; }
        }

        public float PAGainVHF9
        {
            get { return (float)udPAGainVHF9.Value; }
            set { udPAGainVHF9.Value = (decimal)value; }
        }

        public float PAGainVHF10
        {
            get { return (float)udPAGainVHF10.Value; }
            set { udPAGainVHF10.Value = (decimal)value; }
        }
        public float PAGainVHF11
        {
            get { return (float)udPAGainVHF11.Value; }
            set { udPAGainVHF11.Value = (decimal)value; }
        }

        public float PAGainVHF12
        {
            get { return (float)udPAGainVHF12.Value; }
            set { udPAGainVHF12.Value = (decimal)value; }
        }

        public float PAGainVHF13
        {
            get { return (float)udPAGainVHF13.Value; }
            set { udPAGainVHF13.Value = (decimal)value; }
        }

        // PAGain for Hermes
        public float HermesPAGain160
        {
            get { return (float)udHermesPAGain160.Value; }
            set { udHermesPAGain160.Value = (decimal)value; }
        }

        public float HermesPAGain80
        {
            get { return (float)udHermesPAGain80.Value; }
            set { udHermesPAGain80.Value = (decimal)value; }
        }

        public float HermesPAGain60
        {
            get { return (float)udHermesPAGain60.Value; }
            set { udHermesPAGain60.Value = (decimal)value; }
        }

        public float HermesPAGain40
        {
            get { return (float)udHermesPAGain40.Value; }
            set { udHermesPAGain40.Value = (decimal)value; }
        }

        public float HermesPAGain30
        {
            get { return (float)udHermesPAGain30.Value; }
            set { udHermesPAGain30.Value = (decimal)value; }
        }

        public float HermesPAGain20
        {
            get { return (float)udHermesPAGain20.Value; }
            set { udHermesPAGain20.Value = (decimal)value; }
        }

        public float HermesPAGain17
        {
            get { return (float)udHermesPAGain17.Value; }
            set { udHermesPAGain17.Value = (decimal)value; }
        }

        public float HermesPAGain15
        {
            get { return (float)udHermesPAGain15.Value; }
            set { udHermesPAGain15.Value = (decimal)value; }
        }

        public float HermesPAGain12
        {
            get { return (float)udHermesPAGain12.Value; }
            set { udHermesPAGain12.Value = (decimal)value; }
        }

        public float HermesPAGain10
        {
            get { return (float)udHermesPAGain10.Value; }
            set { udHermesPAGain10.Value = (decimal)value; }
        }

        public float HermesPAGain6
        {
            get { return (float)udHermesPAGain6.Value; }
            set { udHermesPAGain6.Value = (decimal)value; }
        }

        public float HermesPAGainVHF0
        {
            get { return (float)udHermesPAGainVHF0.Value; }
            set { udHermesPAGainVHF0.Value = (decimal)value; }
        }

        public float HermesPAGainVHF1
        {
            get { return (float)udHermesPAGainVHF1.Value; }
            set { udHermesPAGainVHF1.Value = (decimal)value; }
        }

        public float HermesPAGainVHF2
        {
            get { return (float)udHermesPAGainVHF2.Value; }
            set { udHermesPAGainVHF2.Value = (decimal)value; }
        }

        public float HermesPAGainVHF3
        {
            get { return (float)udHermesPAGainVHF3.Value; }
            set { udHermesPAGainVHF3.Value = (decimal)value; }
        }

        public float HermesPAGainVHF4
        {
            get { return (float)udHermesPAGainVHF4.Value; }
            set { udHermesPAGainVHF4.Value = (decimal)value; }
        }

        public float HermesPAGainVHF5
        {
            get { return (float)udHermesPAGainVHF5.Value; }
            set { udHermesPAGainVHF5.Value = (decimal)value; }
        }

        public float HermesPAGainVHF6
        {
            get { return (float)udHermesPAGainVHF6.Value; }
            set { udHermesPAGainVHF6.Value = (decimal)value; }
        }

        public float HermesPAGainVHF7
        {
            get { return (float)udHermesPAGainVHF7.Value; }
            set { udHermesPAGainVHF7.Value = (decimal)value; }
        }

        public float HermesPAGainVHF8
        {
            get { return (float)udHermesPAGainVHF8.Value; }
            set { udHermesPAGainVHF8.Value = (decimal)value; }
        }

        public float HermesPAGainVHF9
        {
            get { return (float)udHermesPAGainVHF9.Value; }
            set { udHermesPAGainVHF9.Value = (decimal)value; }
        }

        public float HermesPAGainVHF10
        {
            get { return (float)udHermesPAGainVHF10.Value; }
            set { udHermesPAGainVHF10.Value = (decimal)value; }
        }
        public float HermesPAGainVHF11
        {
            get { return (float)udHermesPAGainVHF11.Value; }
            set { udHermesPAGainVHF11.Value = (decimal)value; }
        }

        public float HermesPAGainVHF12
        {
            get { return (float)udHermesPAGainVHF12.Value; }
            set { udHermesPAGainVHF12.Value = (decimal)value; }
        }

        public float HermesPAGainVHF13
        {
            get { return (float)udHermesPAGainVHF13.Value; }
            set { udHermesPAGainVHF13.Value = (decimal)value; }
        }

        // PAGain ANAN-10
        public float ANAN10PAGain160
        {
            get { return (float)udANAN10PAGain160.Value; }
            set { udANAN10PAGain160.Value = (decimal)value; }
        }

        public float ANAN10PAGain80
        {
            get { return (float)udANAN10PAGain80.Value; }
            set { udANAN10PAGain80.Value = (decimal)value; }
        }

        public float ANAN10PAGain60
        {
            get { return (float)udANAN10PAGain60.Value; }
            set { udANAN10PAGain60.Value = (decimal)value; }
        }

        public float ANAN10PAGain40
        {
            get { return (float)udANAN10PAGain40.Value; }
            set { udANAN10PAGain40.Value = (decimal)value; }
        }

        public float ANAN10PAGain30
        {
            get { return (float)udANAN10PAGain30.Value; }
            set { udANAN10PAGain30.Value = (decimal)value; }
        }

        public float ANAN10PAGain20
        {
            get { return (float)udANAN10PAGain20.Value; }
            set { udANAN10PAGain20.Value = (decimal)value; }
        }

        public float ANAN10PAGain17
        {
            get { return (float)udANAN10PAGain17.Value; }
            set { udANAN10PAGain17.Value = (decimal)value; }
        }

        public float ANAN10PAGain15
        {
            get { return (float)udANAN10PAGain15.Value; }
            set { udANAN10PAGain15.Value = (decimal)value; }
        }

        public float ANAN10PAGain12
        {
            get { return (float)udANAN10PAGain12.Value; }
            set { udANAN10PAGain12.Value = (decimal)value; }
        }

        public float ANAN10PAGain10
        {
            get { return (float)udANAN10PAGain10.Value; }
            set { udANAN10PAGain10.Value = (decimal)value; }
        }

        public float ANAN10PAGain6
        {
            get { return (float)udANAN10PAGain6.Value; }
            set { udANAN10PAGain6.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF0
        {
            get { return (float)udANAN10PAGainVHF0.Value; }
            set { udANAN10PAGainVHF0.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF1
        {
            get { return (float)udANAN10PAGainVHF1.Value; }
            set { udANAN10PAGainVHF1.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF2
        {
            get { return (float)udANAN10PAGainVHF2.Value; }
            set { udANAN10PAGainVHF2.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF3
        {
            get { return (float)udANAN10PAGainVHF3.Value; }
            set { udANAN10PAGainVHF3.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF4
        {
            get { return (float)udANAN10PAGainVHF4.Value; }
            set { udANAN10PAGainVHF4.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF5
        {
            get { return (float)udANAN10PAGainVHF5.Value; }
            set { udANAN10PAGainVHF5.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF6
        {
            get { return (float)udANAN10PAGainVHF6.Value; }
            set { udANAN10PAGainVHF6.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF7
        {
            get { return (float)udANAN10PAGainVHF7.Value; }
            set { udANAN10PAGainVHF7.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF8
        {
            get { return (float)udANAN10PAGainVHF8.Value; }
            set { udANAN10PAGainVHF8.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF9
        {
            get { return (float)udANAN10PAGainVHF9.Value; }
            set { udANAN10PAGainVHF9.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF10
        {
            get { return (float)udANAN10PAGainVHF10.Value; }
            set { udANAN10PAGainVHF10.Value = (decimal)value; }
        }
        public float ANAN10PAGainVHF11
        {
            get { return (float)udANAN10PAGainVHF11.Value; }
            set { udANAN10PAGainVHF11.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF12
        {
            get { return (float)udANAN10PAGainVHF12.Value; }
            set { udANAN10PAGainVHF12.Value = (decimal)value; }
        }

        public float ANAN10PAGainVHF13
        {
            get { return (float)udANAN10PAGainVHF13.Value; }
            set { udANAN10PAGainVHF13.Value = (decimal)value; }
        }

        //PAGain ANAN-100B
        public float ANAN100BPAGain160
        {
            get { return (float)udANAN100BPAGain160.Value; }
            set { udANAN100BPAGain160.Value = (decimal)value; }
        }

        public float ANAN100BPAGain80
        {
            get { return (float)udANAN100BPAGain80.Value; }
            set { udANAN100BPAGain80.Value = (decimal)value; }
        }

        public float ANAN100BPAGain60
        {
            get { return (float)udANAN100BPAGain60.Value; }
            set { udANAN100BPAGain60.Value = (decimal)value; }
        }

        public float ANAN100BPAGain40
        {
            get { return (float)udANAN100BPAGain40.Value; }
            set { udANAN100BPAGain40.Value = (decimal)value; }
        }

        public float ANAN100BPAGain30
        {
            get { return (float)udANAN100BPAGain30.Value; }
            set { udANAN100BPAGain30.Value = (decimal)value; }
        }

        public float ANAN100BPAGain20
        {
            get { return (float)udANAN100BPAGain20.Value; }
            set { udANAN100BPAGain20.Value = (decimal)value; }
        }

        public float ANAN100BPAGain17
        {
            get { return (float)udANAN100BPAGain17.Value; }
            set { udANAN100BPAGain17.Value = (decimal)value; }
        }

        public float ANAN100BPAGain15
        {
            get { return (float)udANAN100BPAGain15.Value; }
            set { udANAN100BPAGain15.Value = (decimal)value; }
        }

        public float ANAN100BPAGain12
        {
            get { return (float)udANAN100BPAGain12.Value; }
            set { udANAN100BPAGain12.Value = (decimal)value; }
        }

        public float ANAN100BPAGain10
        {
            get { return (float)udANAN100BPAGain10.Value; }
            set { udANAN100BPAGain10.Value = (decimal)value; }
        }

        public float ANAN100BPAGain6
        {
            get { return (float)udANAN100BPAGain6.Value; }
            set { udANAN100BPAGain6.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF0
        {
            get { return (float)udANAN100BPAGainVHF0.Value; }
            set { udANAN100BPAGainVHF0.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF1
        {
            get { return (float)udANAN100BPAGainVHF1.Value; }
            set { udANAN100BPAGainVHF1.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF2
        {
            get { return (float)udANAN100BPAGainVHF2.Value; }
            set { udANAN100BPAGainVHF2.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF3
        {
            get { return (float)udANAN100BPAGainVHF3.Value; }
            set { udANAN100BPAGainVHF3.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF4
        {
            get { return (float)udANAN100BPAGainVHF4.Value; }
            set { udANAN100BPAGainVHF4.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF5
        {
            get { return (float)udANAN100BPAGainVHF5.Value; }
            set { udANAN100BPAGainVHF5.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF6
        {
            get { return (float)udANAN100BPAGainVHF6.Value; }
            set { udANAN100BPAGainVHF6.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF7
        {
            get { return (float)udANAN100BPAGainVHF7.Value; }
            set { udANAN100BPAGainVHF7.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF8
        {
            get { return (float)udANAN100BPAGainVHF8.Value; }
            set { udANAN100BPAGainVHF8.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF9
        {
            get { return (float)udANAN100BPAGainVHF9.Value; }
            set { udANAN100BPAGainVHF9.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF10
        {
            get { return (float)udANAN100BPAGainVHF10.Value; }
            set { udANAN100BPAGainVHF10.Value = (decimal)value; }
        }
        public float ANAN100BPAGainVHF11
        {
            get { return (float)udANAN100BPAGainVHF11.Value; }
            set { udANAN100BPAGainVHF11.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF12
        {
            get { return (float)udANAN100BPAGainVHF12.Value; }
            set { udANAN100BPAGainVHF12.Value = (decimal)value; }
        }

        public float ANAN100BPAGainVHF13
        {
            get { return (float)udANAN100BPAGainVHF13.Value; }
            set { udANAN100BPAGainVHF13.Value = (decimal)value; }
        }

        //PAGain ANAN-100
        public float ANAN100PAGain160
        {
            get { return (float)udANAN100PAGain160.Value; }
            set { udANAN100PAGain160.Value = (decimal)value; }
        }

        public float ANAN100PAGain80
        {
            get { return (float)udANAN100PAGain80.Value; }
            set { udANAN100PAGain80.Value = (decimal)value; }
        }

        public float ANAN100PAGain60
        {
            get { return (float)udANAN100PAGain60.Value; }
            set { udANAN100PAGain60.Value = (decimal)value; }
        }

        public float ANAN100PAGain40
        {
            get { return (float)udANAN100PAGain40.Value; }
            set { udANAN100PAGain40.Value = (decimal)value; }
        }

        public float ANAN100PAGain30
        {
            get { return (float)udANAN100PAGain30.Value; }
            set { udANAN100PAGain30.Value = (decimal)value; }
        }

        public float ANAN100PAGain20
        {
            get { return (float)udANAN100PAGain20.Value; }
            set { udANAN100PAGain20.Value = (decimal)value; }
        }

        public float ANAN100PAGain17
        {
            get { return (float)udANAN100PAGain17.Value; }
            set { udANAN100PAGain17.Value = (decimal)value; }
        }

        public float ANAN100PAGain15
        {
            get { return (float)udANAN100PAGain15.Value; }
            set { udANAN100PAGain15.Value = (decimal)value; }
        }

        public float ANAN100PAGain12
        {
            get { return (float)udANAN100PAGain12.Value; }
            set { udANAN100PAGain12.Value = (decimal)value; }
        }

        public float ANAN100PAGain10
        {
            get { return (float)udANAN100PAGain10.Value; }
            set { udANAN100PAGain10.Value = (decimal)value; }
        }

        public float ANAN100PAGain6
        {
            get { return (float)udANAN100PAGain6.Value; }
            set { udANAN100PAGain6.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF0
        {
            get { return (float)udANAN100PAGainVHF0.Value; }
            set { udANAN100PAGainVHF0.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF1
        {
            get { return (float)udANAN100PAGainVHF1.Value; }
            set { udANAN100PAGainVHF1.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF2
        {
            get { return (float)udANAN100PAGainVHF2.Value; }
            set { udANAN100PAGainVHF2.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF3
        {
            get { return (float)udANAN100PAGainVHF3.Value; }
            set { udANAN100PAGainVHF3.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF4
        {
            get { return (float)udANAN100PAGainVHF4.Value; }
            set { udANAN100PAGainVHF4.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF5
        {
            get { return (float)udANAN100PAGainVHF5.Value; }
            set { udANAN100PAGainVHF5.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF6
        {
            get { return (float)udANAN100PAGainVHF6.Value; }
            set { udANAN100PAGainVHF6.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF7
        {
            get { return (float)udANAN100PAGainVHF7.Value; }
            set { udANAN100PAGainVHF7.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF8
        {
            get { return (float)udANAN100PAGainVHF8.Value; }
            set { udANAN100PAGainVHF8.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF9
        {
            get { return (float)udANAN100PAGainVHF9.Value; }
            set { udANAN100PAGainVHF9.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF10
        {
            get { return (float)udANAN100PAGainVHF10.Value; }
            set { udANAN100PAGainVHF10.Value = (decimal)value; }
        }
        public float ANAN100PAGainVHF11
        {
            get { return (float)udANAN100PAGainVHF11.Value; }
            set { udANAN100PAGainVHF11.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF12
        {
            get { return (float)udANAN100PAGainVHF12.Value; }
            set { udANAN100PAGainVHF12.Value = (decimal)value; }
        }

        public float ANAN100PAGainVHF13
        {
            get { return (float)udANAN100PAGainVHF13.Value; }
            set { udANAN100PAGainVHF13.Value = (decimal)value; }
        }

        //PAGain ANAN-100D
        public float ANANPAGain160
        {
            get { return (float)udANANPAGain160.Value; }
            set { udANANPAGain160.Value = (decimal)value; }
        }

        public float ANANPAGain80
        {
            get { return (float)udANANPAGain80.Value; }
            set { udANANPAGain80.Value = (decimal)value; }
        }

        public float ANANPAGain60
        {
            get { return (float)udANANPAGain60.Value; }
            set { udANANPAGain60.Value = (decimal)value; }
        }

        public float ANANPAGain40
        {
            get { return (float)udANANPAGain40.Value; }
            set { udANANPAGain40.Value = (decimal)value; }
        }

        public float ANANPAGain30
        {
            get { return (float)udANANPAGain30.Value; }
            set { udANANPAGain30.Value = (decimal)value; }
        }

        public float ANANPAGain20
        {
            get { return (float)udANANPAGain20.Value; }
            set { udANANPAGain20.Value = (decimal)value; }
        }

        public float ANANPAGain17
        {
            get { return (float)udANANPAGain17.Value; }
            set { udANANPAGain17.Value = (decimal)value; }
        }

        public float ANANPAGain15
        {
            get { return (float)udANANPAGain15.Value; }
            set { udANANPAGain15.Value = (decimal)value; }
        }

        public float ANANPAGain12
        {
            get { return (float)udANANPAGain12.Value; }
            set { udANANPAGain12.Value = (decimal)value; }
        }

        public float ANANPAGain10
        {
            get { return (float)udANANPAGain10.Value; }
            set { udANANPAGain10.Value = (decimal)value; }
        }

        public float ANANPAGain6
        {
            get { return (float)udANANPAGain6.Value; }
            set { udANANPAGain6.Value = (decimal)value; }
        }

        public float ANANPAGainVHF0
        {
            get { return (float)udANANPAGainVHF0.Value; }
            set { udANANPAGainVHF0.Value = (decimal)value; }
        }

        public float ANANPAGainVHF1
        {
            get { return (float)udANANPAGainVHF1.Value; }
            set { udANANPAGainVHF1.Value = (decimal)value; }
        }

        public float ANANPAGainVHF2
        {
            get { return (float)udANANPAGainVHF2.Value; }
            set { udANANPAGainVHF2.Value = (decimal)value; }
        }

        public float ANANPAGainVHF3
        {
            get { return (float)udANANPAGainVHF3.Value; }
            set { udANANPAGainVHF3.Value = (decimal)value; }
        }

        public float ANANPAGainVHF4
        {
            get { return (float)udANANPAGainVHF4.Value; }
            set { udANANPAGainVHF4.Value = (decimal)value; }
        }

        public float ANANPAGainVHF5
        {
            get { return (float)udANANPAGainVHF5.Value; }
            set { udANANPAGainVHF5.Value = (decimal)value; }
        }

        public float ANANPAGainVHF6
        {
            get { return (float)udANANPAGainVHF6.Value; }
            set { udANANPAGainVHF6.Value = (decimal)value; }
        }

        public float ANANPAGainVHF7
        {
            get { return (float)udANANPAGainVHF7.Value; }
            set { udANANPAGainVHF7.Value = (decimal)value; }
        }

        public float ANANPAGainVHF8
        {
            get { return (float)udANANPAGainVHF8.Value; }
            set { udANANPAGainVHF8.Value = (decimal)value; }
        }

        public float ANANPAGainVHF9
        {
            get { return (float)udANANPAGainVHF9.Value; }
            set { udANANPAGainVHF9.Value = (decimal)value; }
        }

        public float ANANPAGainVHF10
        {
            get { return (float)udANANPAGainVHF10.Value; }
            set { udANANPAGainVHF10.Value = (decimal)value; }
        }
        public float ANANPAGainVHF11
        {
            get { return (float)udANANPAGainVHF11.Value; }
            set { udANANPAGainVHF11.Value = (decimal)value; }
        }

        public float ANANPAGainVHF12
        {
            get { return (float)udANANPAGainVHF12.Value; }
            set { udANANPAGainVHF12.Value = (decimal)value; }
        }

        public float ANANPAGainVHF13
        {
            get { return (float)udANANPAGainVHF13.Value; }
            set { udANANPAGainVHF13.Value = (decimal)value; }
        }

        //PAGain ANAN-7000DLE
        public float ANAN7000DPAGain160
        {
            get { return (float)udANAN7000DPAGain160.Value; }
            set { udANAN7000DPAGain160.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain80
        {
            get { return (float)udANAN7000DPAGain80.Value; }
            set { udANAN7000DPAGain80.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain60
        {
            get { return (float)udANAN7000DPAGain60.Value; }
            set { udANAN7000DPAGain60.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain40
        {
            get { return (float)udANAN7000DPAGain40.Value; }
            set { udANAN7000DPAGain40.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain30
        {
            get { return (float)udANAN7000DPAGain30.Value; }
            set { udANAN7000DPAGain30.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain20
        {
            get { return (float)udANAN7000DPAGain20.Value; }
            set { udANAN7000DPAGain20.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain17
        {
            get { return (float)udANAN7000DPAGain17.Value; }
            set { udANAN7000DPAGain17.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain15
        {
            get { return (float)udANAN7000DPAGain15.Value; }
            set { udANAN7000DPAGain15.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain12
        {
            get { return (float)udANAN7000DPAGain12.Value; }
            set { udANAN7000DPAGain12.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain10
        {
            get { return (float)udANAN7000DPAGain10.Value; }
            set { udANAN7000DPAGain10.Value = (decimal)value; }
        }

        public float ANAN7000DPAGain6
        {
            get { return (float)udANAN7000DPAGain6.Value; }
            set { udANAN7000DPAGain6.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF0
        {
            get { return (float)udANAN7000DPAGainVHF0.Value; }
            set { udANAN7000DPAGainVHF0.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF1
        {
            get { return (float)udANAN7000DPAGainVHF1.Value; }
            set { udANAN7000DPAGainVHF1.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF2
        {
            get { return (float)udANAN7000DPAGainVHF2.Value; }
            set { udANAN7000DPAGainVHF2.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF3
        {
            get { return (float)udANAN7000DPAGainVHF3.Value; }
            set { udANAN7000DPAGainVHF3.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF4
        {
            get { return (float)udANAN7000DPAGainVHF4.Value; }
            set { udANAN7000DPAGainVHF4.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF5
        {
            get { return (float)udANAN7000DPAGainVHF5.Value; }
            set { udANAN7000DPAGainVHF5.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF6
        {
            get { return (float)udANAN7000DPAGainVHF6.Value; }
            set { udANAN7000DPAGainVHF6.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF7
        {
            get { return (float)udANAN7000DPAGainVHF7.Value; }
            set { udANAN7000DPAGainVHF7.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF8
        {
            get { return (float)udANAN7000DPAGainVHF8.Value; }
            set { udANAN7000DPAGainVHF8.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF9
        {
            get { return (float)udANAN7000DPAGainVHF9.Value; }
            set { udANAN7000DPAGainVHF9.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF10
        {
            get { return (float)udANAN7000DPAGainVHF10.Value; }
            set { udANAN7000DPAGainVHF10.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF11
        {
            get { return (float)udANAN7000DPAGainVHF11.Value; }
            set { udANAN7000DPAGainVHF11.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF12
        {
            get { return (float)udANAN7000DPAGainVHF12.Value; }
            set { udANAN7000DPAGainVHF12.Value = (decimal)value; }
        }

        public float ANAN7000DPAGainVHF13
        {
            get { return (float)udANAN7000DPAGainVHF13.Value; }
            set { udANAN7000DPAGainVHF13.Value = (decimal)value; }
        }

        //PAGain Orion
        public float OrionPAGain160
        {
            get { return (float)udOrionPAGain160.Value; }
            set { udOrionPAGain160.Value = (decimal)value; }
        }

        public float OrionPAGain80
        {
            get { return (float)udOrionPAGain80.Value; }
            set { udOrionPAGain80.Value = (decimal)value; }
        }

        public float OrionPAGain60
        {
            get { return (float)udOrionPAGain60.Value; }
            set { udOrionPAGain60.Value = (decimal)value; }
        }

        public float OrionPAGain40
        {
            get { return (float)udOrionPAGain40.Value; }
            set { udOrionPAGain40.Value = (decimal)value; }
        }

        public float OrionPAGain30
        {
            get { return (float)udOrionPAGain30.Value; }
            set { udOrionPAGain30.Value = (decimal)value; }
        }

        public float OrionPAGain20
        {
            get { return (float)udOrionPAGain20.Value; }
            set { udOrionPAGain20.Value = (decimal)value; }
        }

        public float OrionPAGain17
        {
            get { return (float)udOrionPAGain17.Value; }
            set { udOrionPAGain17.Value = (decimal)value; }
        }

        public float OrionPAGain15
        {
            get { return (float)udOrionPAGain15.Value; }
            set { udOrionPAGain15.Value = (decimal)value; }
        }

        public float OrionPAGain12
        {
            get { return (float)udOrionPAGain12.Value; }
            set { udOrionPAGain12.Value = (decimal)value; }
        }

        public float OrionPAGain10
        {
            get { return (float)udOrionPAGain10.Value; }
            set { udOrionPAGain10.Value = (decimal)value; }
        }

        public float OrionPAGain6
        {
            get { return (float)udOrionPAGain6.Value; }
            set { udOrionPAGain6.Value = (decimal)value; }
        }

        public float OrionPAGainVHF0
        {
            get { return (float)udOrionPAGainVHF0.Value; }
            set { udOrionPAGainVHF0.Value = (decimal)value; }
        }

        public float OrionPAGainVHF1
        {
            get { return (float)udOrionPAGainVHF1.Value; }
            set { udOrionPAGainVHF1.Value = (decimal)value; }
        }

        public float OrionPAGainVHF2
        {
            get { return (float)udOrionPAGainVHF2.Value; }
            set { udOrionPAGainVHF2.Value = (decimal)value; }
        }

        public float OrionPAGainVHF3
        {
            get { return (float)udOrionPAGainVHF3.Value; }
            set { udOrionPAGainVHF3.Value = (decimal)value; }
        }

        public float OrionPAGainVHF4
        {
            get { return (float)udOrionPAGainVHF4.Value; }
            set { udOrionPAGainVHF4.Value = (decimal)value; }
        }

        public float OrionPAGainVHF5
        {
            get { return (float)udOrionPAGainVHF5.Value; }
            set { udOrionPAGainVHF5.Value = (decimal)value; }
        }

        public float OrionPAGainVHF6
        {
            get { return (float)udOrionPAGainVHF6.Value; }
            set { udOrionPAGainVHF6.Value = (decimal)value; }
        }

        public float OrionPAGainVHF7
        {
            get { return (float)udOrionPAGainVHF7.Value; }
            set { udOrionPAGainVHF7.Value = (decimal)value; }
        }

        public float OrionPAGainVHF8
        {
            get { return (float)udOrionPAGainVHF8.Value; }
            set { udOrionPAGainVHF8.Value = (decimal)value; }
        }

        public float OrionPAGainVHF9
        {
            get { return (float)udOrionPAGainVHF9.Value; }
            set { udOrionPAGainVHF9.Value = (decimal)value; }
        }

        public float OrionPAGainVHF10
        {
            get { return (float)udOrionPAGainVHF10.Value; }
            set { udOrionPAGainVHF10.Value = (decimal)value; }
        }
        public float OrionPAGainVHF11
        {
            get { return (float)udOrionPAGainVHF11.Value; }
            set { udOrionPAGainVHF11.Value = (decimal)value; }
        }

        public float OrionPAGainVHF12
        {
            get { return (float)udOrionPAGainVHF12.Value; }
            set { udOrionPAGainVHF12.Value = (decimal)value; }
        }

        public float OrionPAGainVHF13
        {
            get { return (float)udOrionPAGainVHF13.Value; }
            set { udOrionPAGainVHF13.Value = (decimal)value; }
        }

        //PAGain ORION MKII
        public float ORIONMKIIPAGain160
        {
            get { return (float)udORIONMKIIPAGain160.Value; }
            set { udORIONMKIIPAGain160.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain80
        {
            get { return (float)udORIONMKIIPAGain80.Value; }
            set { udORIONMKIIPAGain80.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain60
        {
            get { return (float)udORIONMKIIPAGain60.Value; }
            set { udORIONMKIIPAGain60.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain40
        {
            get { return (float)udORIONMKIIPAGain40.Value; }
            set { udORIONMKIIPAGain40.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain30
        {
            get { return (float)udORIONMKIIPAGain30.Value; }
            set { udORIONMKIIPAGain30.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain20
        {
            get { return (float)udORIONMKIIPAGain20.Value; }
            set { udORIONMKIIPAGain20.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain17
        {
            get { return (float)udORIONMKIIPAGain17.Value; }
            set { udORIONMKIIPAGain17.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain15
        {
            get { return (float)udORIONMKIIPAGain15.Value; }
            set { udORIONMKIIPAGain15.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain12
        {
            get { return (float)udORIONMKIIPAGain12.Value; }
            set { udORIONMKIIPAGain12.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain10
        {
            get { return (float)udORIONMKIIPAGain10.Value; }
            set { udORIONMKIIPAGain10.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGain6
        {
            get { return (float)udORIONMKIIPAGain6.Value; }
            set { udORIONMKIIPAGain6.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF0
        {
            get { return (float)udORIONMKIIPAGainVHF0.Value; }
            set { udORIONMKIIPAGainVHF0.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF1
        {
            get { return (float)udORIONMKIIPAGainVHF1.Value; }
            set { udORIONMKIIPAGainVHF1.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF2
        {
            get { return (float)udORIONMKIIPAGainVHF2.Value; }
            set { udORIONMKIIPAGainVHF2.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF3
        {
            get { return (float)udORIONMKIIPAGainVHF3.Value; }
            set { udORIONMKIIPAGainVHF3.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF4
        {
            get { return (float)udORIONMKIIPAGainVHF4.Value; }
            set { udORIONMKIIPAGainVHF4.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF5
        {
            get { return (float)udORIONMKIIPAGainVHF5.Value; }
            set { udORIONMKIIPAGainVHF5.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF6
        {
            get { return (float)udORIONMKIIPAGainVHF6.Value; }
            set { udORIONMKIIPAGainVHF6.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF7
        {
            get { return (float)udORIONMKIIPAGainVHF7.Value; }
            set { udORIONMKIIPAGainVHF7.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF8
        {
            get { return (float)udORIONMKIIPAGainVHF8.Value; }
            set { udORIONMKIIPAGainVHF8.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF9
        {
            get { return (float)udORIONMKIIPAGainVHF9.Value; }
            set { udORIONMKIIPAGainVHF9.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF10
        {
            get { return (float)udORIONMKIIPAGainVHF10.Value; }
            set { udORIONMKIIPAGainVHF10.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF11
        {
            get { return (float)udORIONMKIIPAGainVHF11.Value; }
            set { udORIONMKIIPAGainVHF11.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF12
        {
            get { return (float)udORIONMKIIPAGainVHF12.Value; }
            set { udORIONMKIIPAGainVHF12.Value = (decimal)value; }
        }

        public float ORIONMKIIPAGainVHF13
        {
            get { return (float)udORIONMKIIPAGainVHF13.Value; }
            set { udORIONMKIIPAGainVHF13.Value = (decimal)value; }
        }


        //PAGain ANAN-8000DLE
        public float ANAN8000DPAGain160
        {
            get { return (float)udANAN8000DPAGain160.Value; }
            set { udANAN8000DPAGain160.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain80
        {
            get { return (float)udANAN8000DPAGain80.Value; }
            set { udANAN8000DPAGain80.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain60
        {
            get { return (float)udANAN8000DPAGain60.Value; }
            set { udANAN8000DPAGain60.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain40
        {
            get { return (float)udANAN8000DPAGain40.Value; }
            set { udANAN8000DPAGain40.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain30
        {
            get { return (float)udANAN8000DPAGain30.Value; }
            set { udANAN8000DPAGain30.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain20
        {
            get { return (float)udANAN8000DPAGain20.Value; }
            set { udANAN8000DPAGain20.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain17
        {
            get { return (float)udANAN8000DPAGain17.Value; }
            set { udANAN8000DPAGain17.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain15
        {
            get { return (float)udANAN8000DPAGain15.Value; }
            set { udANAN8000DPAGain15.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain12
        {
            get { return (float)udANAN8000DPAGain12.Value; }
            set { udANAN8000DPAGain12.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain10
        {
            get { return (float)udANAN8000DPAGain10.Value; }
            set { udANAN8000DPAGain10.Value = (decimal)value; }
        }

        public float ANAN8000DPAGain6
        {
            get { return (float)udANAN8000DPAGain6.Value; }
            set { udANAN8000DPAGain6.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF0
        {
            get { return (float)udANAN8000DPAGainVHF0.Value; }
            set { udANAN8000DPAGainVHF0.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF1
        {
            get { return (float)udANAN8000DPAGainVHF1.Value; }
            set { udANAN8000DPAGainVHF1.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF2
        {
            get { return (float)udANAN8000DPAGainVHF2.Value; }
            set { udANAN8000DPAGainVHF2.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF3
        {
            get { return (float)udANAN8000DPAGainVHF3.Value; }
            set { udANAN8000DPAGainVHF3.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF4
        {
            get { return (float)udANAN8000DPAGainVHF4.Value; }
            set { udANAN8000DPAGainVHF4.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF5
        {
            get { return (float)udANAN8000DPAGainVHF5.Value; }
            set { udANAN8000DPAGainVHF5.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF6
        {
            get { return (float)udANAN8000DPAGainVHF6.Value; }
            set { udANAN8000DPAGainVHF6.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF7
        {
            get { return (float)udANAN8000DPAGainVHF7.Value; }
            set { udANAN8000DPAGainVHF7.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF8
        {
            get { return (float)udANAN8000DPAGainVHF8.Value; }
            set { udANAN8000DPAGainVHF8.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF9
        {
            get { return (float)udANAN8000DPAGainVHF9.Value; }
            set { udANAN8000DPAGainVHF9.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF10
        {
            get { return (float)udANAN8000DPAGainVHF10.Value; }
            set { udANAN8000DPAGainVHF10.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF11
        {
            get { return (float)udANAN8000DPAGainVHF11.Value; }
            set { udANAN8000DPAGainVHF11.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF12
        {
            get { return (float)udANAN8000DPAGainVHF12.Value; }
            set { udANAN8000DPAGainVHF12.Value = (decimal)value; }
        }

        public float ANAN8000DPAGainVHF13
        {
            get { return (float)udANAN8000DPAGainVHF13.Value; }
            set { udANAN8000DPAGainVHF13.Value = (decimal)value; }
        }


        public int TunePower
        {
            get { return (int)udTXTunePower.Value; }
            set { udTXTunePower.Value = (decimal)value; }
        }

        public bool DigUIsUSB
        {
            get { return chkDigUIsUSB.Checked; }
        }

        public int DigU_CT_Offset
        {
            get { return (int)udOptClickTuneOffsetDIGU.Value; }
            set { udOptClickTuneOffsetDIGU.Value = value; }
        }

        public int DigL_CT_Offset
        {
            get { return (int)udOptClickTuneOffsetDIGL.Value; }
            set { udOptClickTuneOffsetDIGL.Value = value; }
        }

        public float WaterfallLowThreshold
        {
            get { return (float)udDisplayWaterfallLowLevel.Value; }
            set { udDisplayWaterfallLowLevel.Value = (decimal)value; }
        }

        public float WaterfallHighThreshold
        {
            get { return (float)udDisplayWaterfallHighLevel.Value; }
            set { udDisplayWaterfallHighLevel.Value = (decimal)value; }
        }

        public float RX2WaterfallLowThreshold
        {
            get { return (float)udRX2DisplayWaterfallLowLevel.Value; }
            set { udRX2DisplayWaterfallLowLevel.Value = (decimal)value; }
        }

        public float RX2WaterfallHighThreshold
        {
            get { return (float)udRX2DisplayWaterfallHighLevel.Value; }
            set { udRX2DisplayWaterfallHighLevel.Value = (decimal)value; }
        }

        public float DisplayGridMin
        {
            get { return (float)udDisplayGridMin.Value; }
            set { udDisplayGridMin.Value = (decimal)value; }
        }

        public float DisplayGridMax
        {
            get { return (float)udDisplayGridMax.Value; }
            set { udDisplayGridMax.Value = (decimal)value; }
        }

        public float RX2DisplayGridMin
        {
            get { return (float)udRX2DisplayGridMin.Value; }
            set { udRX2DisplayGridMin.Value = (decimal)value; }
        }

        public float RX2DisplayGridMax
        {
            get { return (float)udRX2DisplayGridMax.Value; }
            set { udRX2DisplayGridMax.Value = (decimal)value; }
        }

        public float PA10W
        {
            get
            {
                float rv = (float)ud100PA10W.Value;

                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA1W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA20W.Value;
                        break;
                }
                return rv;

            }
        }

        public float PA20W
        {
            get
            {
                float rv = (float)ud100PA20W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA2W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA40W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA30W
        {
            get
            {
                float rv = (float)ud100PA30W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA3W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA60W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA40W
        {
            get
            {
                float rv = (float)ud100PA40W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA4W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA80W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA50W
        {
            get
            {
                float rv = (float)ud100PA50W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA5W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA100W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA60W
        {
            get
            {
                float rv = (float)ud100PA60W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA6W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA120W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA70W
        {
            get
            {
                float rv = (float)ud100PA70W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA7W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA140W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA80W
        {
            get
            {
                float rv = (float)ud100PA80W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA8W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA160W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA90W
        {
            get
            {
                float rv = (float)ud100PA90W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA9W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA180W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA100W
        {
            get
            {
                float rv = (float)ud100PA100W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA10W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA200W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA110W
        {
            get
            {
                float rv = (float)ud100PA110W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA11W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA220W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA120W
        {
            get
            {
                float rv = (float)ud100PA120W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA12W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA240W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA130W
        {
            get
            {
                float rv = (float)ud100PA130W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA13W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA260W.Value;
                        break;
                }
                return rv;
            }
        }

        public float PA140W
        {
            get
            {
                float rv = (float)ud100PA140W.Value;
                switch (console.CurrentHPSDRModel)
                {
                    case HPSDRModel.ANAN10:
                    case HPSDRModel.ANAN10E:
                        rv = (float)ud10PA14W.Value;
                        break;
                    case HPSDRModel.ANAN8000D:
                        rv = (float)ud200PA280W.Value;
                        break;
                }
                return rv;
            }
        }


        // Added 06/21/05 BT for CAT commands

        public int CATNB1Threshold
        {
            get { return Convert.ToInt32(udDSPNB.Value); }
            set
            {
                value = (int)Math.Max(udDSPNB.Minimum, value);			// lower bound
                value = (int)Math.Min(udDSPNB.Maximum, value);			// upper bound
                udDSPNB.Value = value;
            }
        }

        // Added 06/21/05 BT for CAT commands
        public int CATNB2Threshold
        {
            get { return 0; }
            set
            {
                value = 0;
                //value = (int)Math.Min(udDSPNB2.Maximum, value);
                //udDSPNB2.Value = value;
            }
        }

        // Added 06/21/05 BT for CAT commands
        /*public int CATCompThreshold
        {
            get{return Convert.ToInt32(udTXFFCompression.Value);}
            set
            {
                value = (int)Math.Max(udTXFFCompression.Minimum, value);
                value = (int)Math.Min(udTXFFCompression.Maximum, value);
                udTXFFCompression.Value = value;
            }
        }*/

        // Added 06/30/05 BT for CAT commands
        public int CATCWPitch
        {
            get { return (int)udDSPCWPitch.Value; }
            set
            {
                value = (int)Math.Max(udDSPCWPitch.Minimum, value);
                value = (int)Math.Min(udDSPCWPitch.Maximum, value);
                udDSPCWPitch.Value = value;
            }
        }

        // Added 07/07/05 BT for CAT commands
        public void CATSetRig(string rig)
        {
            comboCATRigType.Text = rig;
        }


        // Added 06/30/05 BT for CAT commands
        //		public int CATTXPreGain
        //		{
        //			get{return (int) udTXPreGain.Value;}
        //			set
        //			{
        //				value = Math.Max(-30, value);
        //				value = Math.Min(70, value);
        //				udTXPreGain.Value = value;
        //			}
        //		}

        //Reads or sets the setup form Spectrum Grid Display Maximum value.
        public int CATSGMax
        {
            get { return (int)udDisplayGridMax.Value; }
            set
            {
                value = (int)Math.Max(udDisplayGridMax.Minimum, value);
                value = (int)Math.Min(udDisplayGridMin.Maximum, value);
                udDisplayGridMax.Value = value;
            }
        }

        //Reads or sets the setup form Spectrum Grid Display Minimum value.
        public int CATSGMin
        {
            get { return (int)udDisplayGridMin.Value; }
            set
            {
                value = (int)Math.Max(udDisplayGridMax.Minimum, value);
                value = (int)Math.Min(udDisplayGridMin.Maximum, value);
                udDisplayGridMin.Value = value;
            }
        }

        //Reads or sets the setup form Spectrum Grid Display Step value.
        public int CATSGStep
        {
            get { return (int)udDisplayGridStep.Value; }
            set
            {
                value = (int)Math.Max(udDisplayGridStep.Minimum, value);
                value = (int)Math.Min(udDisplayGridStep.Maximum, value);
                udDisplayGridStep.Value = value;
            }
        }

        //Reads or sets the setup form Waterfall low level value.
        public int CATWFLo
        {
            get { return (int)udDisplayWaterfallLowLevel.Value; }
            set
            {
                value = (int)Math.Max(udDisplayWaterfallLowLevel.Minimum, value);
                value = (int)Math.Min(udDisplayWaterfallLowLevel.Maximum, value);
                udDisplayWaterfallLowLevel.Value = value;
            }
        }

        //Reads or sets the setup form Waterfall high level value.
        public int CATWFHi
        {
            get { return (int)udDisplayWaterfallHighLevel.Value; }
            set
            {
                value = (int)Math.Max(udDisplayWaterfallHighLevel.Minimum, value);
                value = (int)Math.Min(udDisplayWaterfallHighLevel.Maximum, value);
                udDisplayWaterfallHighLevel.Value = value;
            }
        }

        public int DSPPhoneRXBuffer
        {
            get { return Int32.Parse(comboDSPPhoneRXBuf.Text); }
            set
            {
                string temp = value.ToString();
                if (comboDSPPhoneRXBuf.Items.Contains(temp))
                    comboDSPPhoneRXBuf.SelectedItem = temp;
            }
        }

        public int DSPPhoneTXBuffer
        {
            get { return Int32.Parse(comboDSPPhoneTXBuf.Text); }
            set
            {
                string temp = value.ToString();
                if (comboDSPPhoneTXBuf.Items.Contains(temp))
                    comboDSPPhoneTXBuf.SelectedItem = temp;
            }
        }

        public int DSPCWRXBuffer
        {
            get { return Int32.Parse(comboDSPCWRXBuf.Text); }
            set
            {
                string temp = value.ToString();
                if (comboDSPCWRXBuf.Items.Contains(temp))
                    comboDSPCWRXBuf.SelectedItem = temp;
            }
        }

        public int DSPDigRXBuffer
        {
            get { return Int32.Parse(comboDSPDigRXBuf.Text); }
            set
            {
                string temp = value.ToString();
                if (comboDSPDigRXBuf.Items.Contains(temp))
                    comboDSPDigRXBuf.SelectedItem = temp;
            }
        }

        public int DSPDigTXBuffer
        {
            get { return Int32.Parse(comboDSPDigTXBuf.Text); }
            set
            {
                string temp = value.ToString();
                if (comboDSPDigTXBuf.Items.Contains(temp))
                    comboDSPDigTXBuf.SelectedItem = temp;
            }
        }

        public int AudioBufferSize
        {
            get { return console.BlockSize1; }
            set
            {
                string temp = value.ToString();
                //if (comboAudioBuffer1.Items.Contains(temp))
                //comboAudioBuffer1.SelectedItem = temp;
            }
        }

        private bool flex_profiler_installed = false;
        public bool FlexProfilerInstalled
        {
            get { return flex_profiler_installed; }
            set { flex_profiler_installed = value; }
        }

        private bool allow_freq_broadcast = false;
        public bool AllowFreqBroadcast
        {
            get { return allow_freq_broadcast; }
            set
            {
                allow_freq_broadcast = value;
                console.KWAutoInformation = value;
            }
        }

        public bool AutoMuteRX2onVFOATX
        {
            get { return chkRX2AutoMuteTX.Checked; }
            set { chkRX2AutoMuteTX.Checked = value; }
        }

        public bool AutoMuteRX1onVFOBTX
        {
            get { return chkRX2AutoMuteRX1OnVFOBTX.Checked; }
            set { chkRX2AutoMuteRX1OnVFOBTX.Checked = value; }
        }

        private bool rtty_offset_enabled_a;
        public bool RttyOffsetEnabledA
        {
            get { return rtty_offset_enabled_a; }
            set { chkRTTYOffsetEnableA.Checked = value; }
        }

        private bool rtty_offset_enabled_b;
        public bool RttyOffsetEnabledB
        {
            get { return rtty_offset_enabled_b; }
            set { chkRTTYOffsetEnableB.Checked = value; }
        }

        private int rtty_offset_high = 2125;
        public int RttyOffsetHigh
        {
            get { return rtty_offset_high; }
            set
            {
                value = (int)Math.Max(udRTTYU.Minimum, value);
                value = (int)Math.Min(udRTTYU.Maximum, value);
                udRTTYU.Value = value;
            }
        }

        private int rtty_offset_low = 2125;
        public int RttyOffsetLow
        {
            get { return rtty_offset_low; }
            set
            {
                value = (int)Math.Max(udRTTYL.Minimum, value);
                value = (int)Math.Min(udRTTYL.Maximum, value);
                udRTTYL.Value = value;
            }
        }

        public float MeterOffset
        {
            get
            {
                return float.Parse(txtMeterOffset.Text);
            }
            set
            {
                txtMeterOffset.Text = value.ToString();
            }
        }

        public float DisplayOffset
        {
            get
            {
                return float.Parse(txtDisplayOffset.Text);
            }
            set
            {
                txtDisplayOffset.Text = value.ToString();
            }
        }

        private bool firmware_bypass = false;
        public bool FirmwareBypass
        {
            get
            {
                return firmware_bypass;
            }
            set
            {
                firmware_bypass = value;
            }
        }


        public int[] CFCCOMPEQ
        {
            get
            {
                int[] cfceq = new int[32];
                cfceq[0] = tbCFCPRECOMP.Value;
                cfceq[1] = tbCFC0.Value;
                cfceq[2] = tbCFC1.Value;
                cfceq[3] = tbCFC2.Value;
                cfceq[4] = tbCFC3.Value;
                cfceq[5] = tbCFC4.Value;
                cfceq[6] = tbCFC5.Value;
                cfceq[7] = tbCFC6.Value;
                cfceq[8] = tbCFC7.Value;
                cfceq[9] = tbCFC8.Value;
                cfceq[10] = tbCFC9.Value;

                cfceq[11] = tbCFCPEQGAIN.Value;
                cfceq[12] = tbCFCEQ0.Value;
                cfceq[13] = tbCFCEQ1.Value;
                cfceq[14] = tbCFCEQ2.Value;
                cfceq[15] = tbCFCEQ3.Value;
                cfceq[16] = tbCFCEQ4.Value;
                cfceq[17] = tbCFCEQ5.Value;
                cfceq[18] = tbCFCEQ6.Value;
                cfceq[19] = tbCFCEQ7.Value;
                cfceq[20] = tbCFCEQ8.Value;
                cfceq[21] = tbCFCEQ9.Value;

                cfceq[22] = (int)udCFC0.Value;
                cfceq[23] = (int)udCFC1.Value;
                cfceq[24] = (int)udCFC2.Value;
                cfceq[25] = (int)udCFC3.Value;
                cfceq[26] = (int)udCFC4.Value;
                cfceq[27] = (int)udCFC5.Value;
                cfceq[28] = (int)udCFC6.Value;
                cfceq[29] = (int)udCFC7.Value;
                cfceq[30] = (int)udCFC8.Value;
                cfceq[31] = (int)udCFC9.Value;

                return cfceq;
            }

            set
            {
                if (value.Length < 32)
                {
                    MessageBox.Show("Error setting CFC EQ");
                    return;
                }
                tbCFCPRECOMP.Value = Math.Max(tbCFCPRECOMP.Minimum, Math.Min(tbCFCPRECOMP.Maximum, value[0]));
                tbCFC0.Value = Math.Max(tbCFC0.Minimum, Math.Min(tbCFC0.Maximum, value[1]));
                tbCFC1.Value = Math.Max(tbCFC1.Minimum, Math.Min(tbCFC1.Maximum, value[2]));
                tbCFC2.Value = Math.Max(tbCFC2.Minimum, Math.Min(tbCFC2.Maximum, value[3]));
                tbCFC3.Value = Math.Max(tbCFC3.Minimum, Math.Min(tbCFC3.Maximum, value[4]));
                tbCFC4.Value = Math.Max(tbCFC4.Minimum, Math.Min(tbCFC4.Maximum, value[5]));
                tbCFC5.Value = Math.Max(tbCFC5.Minimum, Math.Min(tbCFC5.Maximum, value[6]));
                tbCFC6.Value = Math.Max(tbCFC6.Minimum, Math.Min(tbCFC6.Maximum, value[7]));
                tbCFC7.Value = Math.Max(tbCFC7.Minimum, Math.Min(tbCFC7.Maximum, value[8]));
                tbCFC8.Value = Math.Max(tbCFC8.Minimum, Math.Min(tbCFC8.Maximum, value[9]));
                tbCFC9.Value = Math.Max(tbCFC9.Minimum, Math.Min(tbCFC9.Maximum, value[10]));
                tbCFCPEQGAIN.Value = Math.Max(tbCFCPEQGAIN.Minimum, Math.Min(tbCFCPEQGAIN.Maximum, value[11]));
                tbCFCEQ0.Value = Math.Max(tbCFCEQ0.Minimum, Math.Min(tbCFCEQ0.Maximum, value[12]));
                tbCFCEQ1.Value = Math.Max(tbCFCEQ1.Minimum, Math.Min(tbCFCEQ1.Maximum, value[13]));
                tbCFCEQ2.Value = Math.Max(tbCFCEQ2.Minimum, Math.Min(tbCFCEQ2.Maximum, value[14]));
                tbCFCEQ3.Value = Math.Max(tbCFCEQ3.Minimum, Math.Min(tbCFCEQ3.Maximum, value[15]));
                tbCFCEQ4.Value = Math.Max(tbCFCEQ4.Minimum, Math.Min(tbCFCEQ4.Maximum, value[16]));
                tbCFCEQ5.Value = Math.Max(tbCFCEQ5.Minimum, Math.Min(tbCFCEQ5.Maximum, value[17]));
                tbCFCEQ6.Value = Math.Max(tbCFCEQ6.Minimum, Math.Min(tbCFCEQ6.Maximum, value[18]));
                tbCFCEQ7.Value = Math.Max(tbCFCEQ7.Minimum, Math.Min(tbCFCEQ7.Maximum, value[19]));
                tbCFCEQ8.Value = Math.Max(tbCFCEQ8.Minimum, Math.Min(tbCFCEQ8.Maximum, value[20]));
                tbCFCEQ9.Value = Math.Max(tbCFCEQ9.Minimum, Math.Min(tbCFCEQ9.Maximum, value[21]));
                udCFC0.Value = Math.Max(udCFC0.Minimum, Math.Min(udCFC0.Maximum, value[22]));
                udCFC1.Value = Math.Max(udCFC1.Minimum, Math.Min(udCFC1.Maximum, value[23]));
                udCFC2.Value = Math.Max(udCFC2.Minimum, Math.Min(udCFC2.Maximum, value[24]));
                udCFC3.Value = Math.Max(udCFC3.Minimum, Math.Min(udCFC3.Maximum, value[25]));
                udCFC4.Value = Math.Max(udCFC4.Minimum, Math.Min(udCFC4.Maximum, value[26]));
                udCFC5.Value = Math.Max(udCFC5.Minimum, Math.Min(udCFC5.Maximum, value[27]));
                udCFC6.Value = Math.Max(udCFC6.Minimum, Math.Min(udCFC6.Maximum, value[28]));
                udCFC7.Value = Math.Max(udCFC7.Minimum, Math.Min(udCFC7.Maximum, value[29]));
                udCFC8.Value = Math.Max(udCFC8.Minimum, Math.Min(udCFC8.Maximum, value[30]));
                udCFC9.Value = Math.Max(udCFC9.Minimum, Math.Min(udCFC9.Maximum, value[31]));

                tbCFCPRECOMP_Scroll(this, EventArgs.Empty);
                tbCFCPEG_Scroll(this, EventArgs.Empty);
                setCFCProfile(this, EventArgs.Empty);
            }
        }



        #endregion

        #region General Tab Event Handlers
        // ======================================================
        // General Tab Event Handlers
        // ======================================================

        //   private void radGenModelFLEX5000_CheckedChanged(object sender, System.EventArgs e)
        //  {
        //if (radGenModelFLEX5000.Checked)
        //  {
        //comboAudioSoundCard.Text = "Unsupported Card";
        ////comboAudioSampleRate1.Text = "96000";

        //foreach (PADeviceInfo p in comboAudioDriver1.Items)
        //{
        //    if (p.Name == "ASIO")
        //    {
        //        comboAudioDriver1.SelectedItem = p;
        //        break;
        //    }
        //}

        //foreach (PADeviceInfo dev in comboAudioInput1.Items)
        //{
        //    if (dev.Name.ToLower().IndexOf("flex") >= 0)
        //    {
        //        comboAudioInput1.Text = dev.Name;
        //        comboAudioOutput1.Text = dev.Name;
        //        break;
        //    }
        //}

        //  comboAudioMixer1.Text = "None";

        // if (comboAudioInput1.Text.ToLower().IndexOf("flex") < 0)
        // {
        /*MessageBox.Show("FLEX-5000 hardware not found.  Please check " +
            "the following:\n" +
            "\t1. Verify that the unit has power and is running (note blue LED).\n" +
            "\t2. Verify FireWire cable is securely plugged in on both ends.\n" +
            "\t3. Verify that the driver is installed properly and the device shows up as FLEX 5000 in the device manager.\n" +
            "Note that after correcting any of these issues, you must restart PowerSDR for the changes to take effect.\n" +
            "For more support, see our website at www.flex-radio.com or email support@flex-radio.com.",
            "Hardware Not Found",
            MessageBoxButtons.OK,
            MessageBoxIcon.Exclamation);*/
        // console.PowerEnabled = false;
        // }
        //  }
        // else console.PowerEnabled = true;

        // bool b = radGenModelFLEX5000.Checked;
        // radPACalAllBands_CheckedChanged(this, EventArgs.Empty);

        // grpAudioDetails1.Visible = !b;
        // grpAudioCard.Visible = !b;
        // grpAudioLineInGain1.Visible = !b;
        // grpAudioMicInGain1.Visible = !b;
        // grpAudioChannels.Visible = !b;

        //  chkCalExpert.Visible = b;
        // chkCalExpert_CheckedChanged(this, EventArgs.Empty);
        // if (!b)
        //{
        //    grpGeneralCalibration.Visible = true;
        //    grpGenCalLevel.Visible = true;
        //    grpGenCalRXImage.Visible = true;
        //}

        //  grpPAGainByBand.Visible = true;
        //  chkPANewCal.Visible = false;
        // grpImpulseTest.Visible = !b;
        //  }

        private void radGenModelANAN10_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN10Present = radGenModelANAN10.Checked;

            if (radGenModelANAN10.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                // console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN10;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Checked = true;
                chkAlexPresent.Enabled = false;
                chkApolloPresent.Checked = false;
                chkApolloPresent.Enabled = false;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                // chkGeneralRXOnly.Checked = false;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                tpAlexControl.Text = "Ant/Filters";
                //string key = comboKeyerConnPrimary.Text;
                //if (comboKeyerConnPrimary.Items.Contains("5000"))
                //    comboKeyerConnPrimary.Items.Remove("5000");
                //if (comboKeyerConnPrimary.Items.Contains("SDR"))
                //    comboKeyerConnPrimary.Items.Remove("SDR");
                //if (!comboKeyerConnPrimary.Items.Contains("Ozy/Hermes"))
                //    comboKeyerConnPrimary.Items.Insert(0, "Ozy/Hermes");
                //comboKeyerConnPrimary.Text = !key.StartsWith("COM") ? "Ozy/Hermes" : key;
                //comboKeyerConnPrimary_SelectedIndexChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                grpANAN10PAGainByBand.BringToFront();
                labelRXAntControl.Text = "  RX1   RX2    XVTR";
                chkEXT2OutOnTx.Visible = false;
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                // console.RX2PreampPresent = false;
                groupBoxHPSDRHW.Visible = false;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = false;
                console.RX2PreampPresent = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(1);
            }
            console.ANAN10Present = radGenModelANAN10.Checked;
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);
            chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
            chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);

            if (radGenModelANAN10.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN10E_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN10EPresent = radGenModelANAN10E.Checked;

            if (radGenModelANAN10E.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                //console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN10E;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Checked = true;
                chkAlexPresent.Enabled = false;
                chkApolloPresent.Checked = false;
                chkApolloPresent.Enabled = false;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                tpAlexControl.Text = "Ant/Filters";
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                grpANAN10PAGainByBand.BringToFront();
                labelRXAntControl.Text = "  RX1   RX2    XVTR";
                chkEXT2OutOnTx.Visible = false;
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                groupBoxHPSDRHW.Visible = false;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = false;
                console.RX2PreampPresent = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(1);
            }

            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);
            chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
            chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);

            if (radGenModelANAN10E.Checked)
            {
                bool power = console.PowerOn;
                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);
                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN100B_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN100BPresent = radGenModelANAN100B.Checked;

            if (radGenModelANAN100B.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                //console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN100B;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkPennyPresent.Visible = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkMercuryPresent.Visible = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                chkPennyLane.Visible = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Checked = true;
                chkAlexPresent.Enabled = false;
                chkApolloPresent.Checked = false;
                chkApolloPresent.Enabled = false;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                grpANAN100BPAGainByBand.BringToFront();
                labelRXAntControl.Text = " EXT2  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = false;
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = true;
                groupBoxHPSDRHW.Visible = false;
                chkDisableRXOut.Visible = true;
                chkBPF2Gnd.Visible = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(1);
            }
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN100B.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN100_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;

            if (radGenModelANAN100.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                // console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN100;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Checked = false;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                grpANAN100PAGainByBand.BringToFront();
                labelRXAntControl.Text = " EXT2  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = false;
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = true;
                groupBoxHPSDRHW.Visible = false;
                chkDisableRXOut.Visible = true;
                chkBPF2Gnd.Visible = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(1);
            }
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN100.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN100D_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN100DPresent = radGenModelANAN100D.Checked;

            if (radGenModelANAN100D.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                // console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN100D;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkPennyPresent.Visible = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkMercuryPresent.Visible = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                chkPennyLane.Visible = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                chkBypassANANPASettings.Visible = true;

                if (!chkBypassANANPASettings.Checked)
                    grpANANPAGainByBand.BringToFront();
                else grpPAGainByBand.BringToFront();

                labelRXAntControl.Text = " EXT2  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = true;
                chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = true;
                groupBoxHPSDRHW.Visible = true;
                chkDisableRXOut.Visible = true;
                chkBPF2Gnd.Visible = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(2);

                radDDC0ADC2.Enabled = false;
                radDDC1ADC2.Enabled = false;
                radDDC2ADC2.Enabled = false;
                radDDC3ADC2.Enabled = false;
                radDDC4ADC2.Enabled = false;
                radDDC5ADC2.Enabled = false;
                radDDC6ADC2.Enabled = false;
                if (radDDC0ADC2.Checked) radDDC0ADC0.Checked = true;
                if (radDDC1ADC2.Checked) radDDC1ADC1.Checked = true;
                if (radDDC2ADC2.Checked) radDDC2ADC0.Checked = true;
                if (radDDC3ADC2.Checked) radDDC3ADC0.Checked = true;
                if (radDDC4ADC2.Checked) radDDC4ADC0.Checked = true;
                if (radDDC5ADC2.Checked) radDDC5ADC0.Checked = true;
                if (radDDC6ADC2.Checked) radDDC6ADC0.Checked = true;
            }
            console.ANAN100DPresent = radGenModelANAN100D.Checked;
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN100D.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN200D_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN200DPresent = radGenModelANAN200D.Checked;

            if (radGenModelANAN200D.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                //console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN200D;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                chkBypassANANPASettings.Visible = true;
                chkDisableRXOut.Visible = true;
                chkBPF2Gnd.Visible = false;
                grpOrionPAGainByBand.BringToFront();

                labelRXAntControl.Text = " EXT2  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = true;
                chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = true;

                chkAlexPresent.Parent = grpGeneralHardwareORION;
                chkAlexPresent.Location = new Point(43, 120);
                chkApolloPresent.Parent = grpGeneralHardwareORION;
                chkApolloPresent.Location = new Point(43, 140);
                cmaster.SetADCSupply(0, 50);
                NetworkIO.SetRxADC(2);

                radDDC0ADC2.Enabled = true;
                radDDC1ADC2.Enabled = true;
                radDDC2ADC2.Enabled = true;
                radDDC3ADC2.Enabled = true;
                radDDC4ADC2.Enabled = true;
                radDDC5ADC2.Enabled = true;
                radDDC6ADC2.Enabled = true;
            }
            else
            {
                chkAlexPresent.Parent = groupBoxHPSDRHW;
                chkAlexPresent.Location = new Point(25, 80);
                chkApolloPresent.Parent = groupBoxHPSDRHW;
                chkApolloPresent.Location = new Point(25, 100);

            }
            console.ANAN200DPresent = radGenModelANAN200D.Checked;
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN200D.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelORIONMKII_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ORIONMKIIPresent = radGenModelORIONMKII.Checked;

            if (radGenModelORIONMKII.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                //console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ORIONMKII;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                chkBypassANANPASettings.Visible = true;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = true;
                grpORIONMKIIPAGainByBand.BringToFront();

                labelRXAntControl.Text = "  BYPS  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = true;
                chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                //chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = false;

                chkAlexPresent.Parent = grpGeneralHardwareORION;
                chkAlexPresent.Location = new Point(43, 120);
                chkApolloPresent.Parent = grpGeneralHardwareORION;
                chkApolloPresent.Location = new Point(43, 140);
                cmaster.SetADCSupply(0, 50);
                NetworkIO.SetRxADC(2);

                radDDC0ADC2.Enabled = true;
                radDDC1ADC2.Enabled = true;
                radDDC2ADC2.Enabled = true;
                radDDC3ADC2.Enabled = true;
                radDDC4ADC2.Enabled = true;
                radDDC5ADC2.Enabled = true;
                radDDC6ADC2.Enabled = true;

                radGenModelORIONMKII.Visible = true;
               // radGenModelANAN8000D.Visible = true;
            }
            else
            {
                chkAlexPresent.Parent = groupBoxHPSDRHW;
                chkAlexPresent.Location = new Point(25, 80);
                chkApolloPresent.Parent = groupBoxHPSDRHW;
                chkApolloPresent.Location = new Point(25, 100);

            }
            console.ORIONMKIIPresent = radGenModelORIONMKII.Checked;
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelORIONMKII.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN7000D_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN7000DPresent = radGenModelANAN7000D.Checked;

            if (radGenModelANAN7000D.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                console.CurrentHPSDRModel = HPSDRModel.ANAN7000D;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                chkBypassANANPASettings.Visible = true;
                grpANAN7000DPAGainByBand.BringToFront();

                labelRXAntControl.Text = "  BYPS  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = true;
                chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                //chkRxOutOnTx.Visible = false;
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                chkEXT2OutOnTx.Text = "Rx BYPASS on Tx";

                chkAlexPresent.Parent = grpGeneralHardwareORION;
                chkAlexPresent.Location = new Point(43, 120);
                chkApolloPresent.Parent = grpGeneralHardwareORION;
                chkApolloPresent.Location = new Point(43, 140);

                panelAlex1HPFControl.Visible = false;
                panelBPFControl.Visible = true;

                chkDisable6mLNAonRX.Parent = panelBPFControl;
                chkDisable6mLNAonRX.Location = new Point(16, 208);
                chkDisable6mLNAonTX.Parent = panelBPFControl;
                chkDisable6mLNAonTX.Location = new Point(63, 208);

                chkAlexHPFBypass.Parent = panelBPFControl;
                chkAlexHPFBypass.Location = new Point(140, 185);
                chkDisableHPFonTX.Parent = panelBPFControl;
                chkDisableHPFonTX.Location = new Point(140, 213);

                cmaster.SetADCSupply(0, 50);
                NetworkIO.SetRxADC(2);

                radDDC0ADC2.Enabled = true;
                radDDC1ADC2.Enabled = true;
                radDDC2ADC2.Enabled = true;
                radDDC3ADC2.Enabled = true;
                radDDC4ADC2.Enabled = true;
                radDDC5ADC2.Enabled = true;
                radDDC6ADC2.Enabled = true;

                //chkDisableRXOut.Visible = false;
                //chkBPF2Gnd.Visible = true;
            }
            else
            {
                chkAlexPresent.Parent = groupBoxHPSDRHW;
                chkAlexPresent.Location = new Point(25, 80);
                chkApolloPresent.Parent = groupBoxHPSDRHW;
                chkApolloPresent.Location = new Point(25, 100);

                panelBPFControl.Visible = false;
                panelAlex1HPFControl.Visible = true;

                chkDisable6mLNAonRX.Parent = panelAlex1HPFControl;
                chkDisable6mLNAonRX.Location = new Point(16, 208);
                chkDisable6mLNAonTX.Parent = panelAlex1HPFControl;
                chkDisable6mLNAonTX.Location = new Point(63, 208);

                chkAlexHPFBypass.Parent = panelAlex1HPFControl;
                chkAlexHPFBypass.Location = new Point(140, 185);
                chkDisableHPFonTX.Parent = panelAlex1HPFControl;
                chkDisableHPFonTX.Location = new Point(140, 213);
                panelAlex1HPFControl.Visible = false;

            }
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN7000D.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelANAN8000D_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;
            console.ANAN8000DPresent = radGenModelANAN8000D.Checked;

            if (radGenModelANAN8000D.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                // console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.ANAN8000D;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "ANAN Options";
                grpMetisAddr.Text = "ANAN Address";
                grpHermesStepAttenuator.Text = "ANAN Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                chkBypassANANPASettings.Visible = true;
                grpANAN8000DPAGainByBand.BringToFront();

                labelRXAntControl.Text = "  BYPS  EXT1  XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = true;
                chkRX2StepAtt_CheckedChanged(this, EventArgs.Empty);
                chkRxOutOnTx.Text = "BYPASS on Tx";
                chkEXT1OutOnTx.Text = "Ext 1 on Tx";
                // chkEXT2OutOnTx.Text = "Ext 2 on Tx";
                chkEXT2OutOnTx.Visible = false;

                chkAlexPresent.Parent = grpGeneralHardwareORION;
                chkAlexPresent.Location = new Point(43, 120);
                chkApolloPresent.Parent = grpGeneralHardwareORION;
                chkApolloPresent.Location = new Point(43, 140);

                panelAlex1HPFControl.Visible = false;
                panelBPFControl.Visible = true;

                chkDisable6mLNAonRX.Parent = panelBPFControl;
                chkDisable6mLNAonRX.Location = new Point(16, 208);
                chkDisable6mLNAonTX.Parent = panelBPFControl;
                chkDisable6mLNAonTX.Location = new Point(63, 208);

                chkAlexHPFBypass.Parent = panelBPFControl;
                chkAlexHPFBypass.Location = new Point(140, 185);
                chkDisableHPFonTX.Parent = panelBPFControl;
                chkDisableHPFonTX.Location = new Point(140, 213);

                cmaster.SetADCSupply(0, 50);
                NetworkIO.SetRxADC(2);

                radDDC0ADC2.Enabled = true;
                radDDC1ADC2.Enabled = true;
                radDDC2ADC2.Enabled = true;
                radDDC3ADC2.Enabled = true;
                radDDC4ADC2.Enabled = true;
                radDDC5ADC2.Enabled = true;
                radDDC6ADC2.Enabled = true;

              //  radGenModelORIONMKII.Visible = true;
              //  radGenModelANAN8000D.Visible = true;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = true;
            }
            else
            {
                chkAlexPresent.Parent = groupBoxHPSDRHW;
                chkAlexPresent.Location = new Point(25, 80);
                chkApolloPresent.Parent = groupBoxHPSDRHW;
                chkApolloPresent.Location = new Point(25, 100);

                panelBPFControl.Visible = false;
                panelAlex1HPFControl.Visible = true;

                chkDisable6mLNAonRX.Parent = panelAlex1HPFControl;
                chkDisable6mLNAonRX.Location = new Point(16, 208);
                chkDisable6mLNAonTX.Parent = panelAlex1HPFControl;
                chkDisable6mLNAonTX.Location = new Point(63, 208);

                chkAlexHPFBypass.Parent = panelAlex1HPFControl;
                chkAlexHPFBypass.Location = new Point(140, 185);
                chkDisableHPFonTX.Parent = panelAlex1HPFControl;
                chkDisableHPFonTX.Location = new Point(140, 213);
                panelAlex1HPFControl.Visible = false;

            }
            console.ANAN8000DPresent = radGenModelANAN8000D.Checked;
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelANAN8000D.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelHermes_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;

            if (radGenModelHermes.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                //console.CurrentModel = Model.HERMES;
                console.CurrentHPSDRModel = HPSDRModel.HERMES;
                chkPennyPresent.Checked = false;
                chkPennyPresent.Enabled = false;
                chkPennyPresent.Visible = false;
                chkMercuryPresent.Checked = true;
                chkMercuryPresent.Enabled = false;
                chkMercuryPresent.Visible = false;
                chkExcaliburPresent.Checked = false;
                chkExcaliburPresent.Enabled = false;
                chkExcaliburPresent.Visible = false;
                chkPennyLane.Checked = true;
                chkPennyLane.Enabled = false;
                chkPennyLane.Visible = false;
                radPenny10MHz.Checked = true;
                rad12288MHzPenny.Checked = true;
                chkAlexPresent.Enabled = true;
                chkApolloPresent.Enabled = true;
                chkApolloPresent.Visible = true;
                groupBox10MhzClock.Visible = false;
                groupBox122MHz.Visible = false;
                groupBoxMicSource.Visible = false;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Enabled = true;
                groupBoxRXOptions.Text = "Hermes Options";
                grpMetisAddr.Text = "Hermes Address";
                grpHermesStepAttenuator.Text = "Hermes Step Attenuator";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkAutoPACalibrate.Checked = false;
                chkAutoPACalibrate.Visible = false;
                grpHermesPAGainByBand.BringToFront();
                labelRXAntControl.Text = "  RX1   RX2    XVTR";
                labelATTOnTX.Visible = true;
                udATTOnTX.Visible = true;
                console.RX2PreampPresent = false;
                chkRxOutOnTx.Text = "RX 1 OUT on Tx";
                chkEXT1OutOnTx.Text = "RX 2 IN on Tx";
                chkEXT2OutOnTx.Text = "RX 1 IN on Tx";
                chkEXT2OutOnTx.Visible = true;
                groupBoxHPSDRHW.Visible = true;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = false;
                cmaster.SetADCSupply(0, 33);
                NetworkIO.SetRxADC(1);
            }
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, true);

            if (radGenModelHermes.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelHPSDR_CheckedChanged(object sender, System.EventArgs e)
        {
            HPSDRModel old_model = console.CurrentHPSDRModel;

            if (radGenModelHPSDR.Checked)
            {
                NetworkIO.fwVersionsChecked = false;
                // console.CurrentModel = Model.HPSDR;
                console.CurrentHPSDRModel = HPSDRModel.HPSDR;
                chkPennyPresent.Enabled = true;
                chkPennyPresent.Visible = true;
                chkPennyLane.Enabled = true;
                chkPennyLane.Visible = true;
                chkMercuryPresent.Enabled = true;
                chkMercuryPresent.Visible = true;
                chkExcaliburPresent.Enabled = true;
                chkExcaliburPresent.Visible = true;
                groupBox10MhzClock.Visible = true;
                groupBox122MHz.Visible = true;
                groupBoxMicSource.Visible = true;
                chkGeneralRXOnly.Visible = true;
                chkHermesStepAttenuator.Checked = false;
                chkHermesStepAttenuator.Enabled = false;
                chkAlexPresent.Enabled = true;
                chkAlexPresent.Visible = true;
                chkApolloPresent.Checked = false;
                chkApolloPresent.Enabled = false;
                chkApolloPresent.Visible = false;
                groupBoxRXOptions.Text = "Mercury Options";
                grpMetisAddr.Text = "Metis Address";
                chkAlexPresent_CheckedChanged(this, EventArgs.Empty);
                chkAlexAntCtrl_CheckedChanged(this, EventArgs.Empty);
                chkBypassANANPASettings.Visible = false;
                chkAutoPACalibrate.Visible = true;
                grpPAGainByBand.BringToFront();
                labelRXAntControl.Text = "  RX1   RX2    XVTR";
                labelATTOnTX.Visible = false;
                udATTOnTX.Visible = false;
                console.RX2PreampPresent = false;
                chkRxOutOnTx.Text = "RX 1 OUT on Tx";
                chkEXT1OutOnTx.Text = "RX 2 IN on Tx";
                chkEXT2OutOnTx.Text = "RX 1 IN on Tx";
                chkEXT2OutOnTx.Visible = true;
                groupBoxHPSDRHW.Visible = true;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = false;
                NetworkIO.SetRxADC(1);
            }
            radGenModelHPSDR_or_Hermes_CheckedChanged(sender, e, false);
            if (chkHermesStepAttenuator.Checked) chkHermesStepAttenuator.Checked = false;
            chkHermesStepAttenuator.Enabled = !radGenModelHPSDR.Checked;
            // if (radMetis.Checked)
            // {
            // console.HPSDRisMetis = true;
            grpMetisAddr.Visible = true;
            grpMetisAddr.Visible = true;
            radMicIn_CheckedChanged(this, EventArgs.Empty);
            radLineIn_CheckedChanged(this, EventArgs.Empty);

            // }

            if (radGenModelHPSDR.Checked)
            {
                bool power = console.PowerOn;

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = false;
                    Thread.Sleep(100);
                }
                cmaster.CMLoadRouterAll(console.CurrentHPSDRModel);

                if (power && (old_model != console.CurrentHPSDRModel))
                {
                    console.PowerOn = true;
                }
            }
        }

        private void radGenModelHPSDR_or_Hermes_CheckedChanged(object sender, System.EventArgs e, bool is_hermes)
        {
            // add or remove setup pages for HPSDR stuff 
            //
            bool b = is_hermes;
            if (!initializing)
                AddHPSDRPages();

            // force setting of audio card 
            // comboAudioSampleRate1.Text = "192000"; 
            // comboAudioSampleRate1.Enabled = false;
            // and enable the gain by band page 
            grpFRSRegion.Visible = true;
            grpPAGainByBand.Visible = true;
            // grpGeneralHardwareSDR1000.Visible = false;
            // grpGeneralDDS.Visible = false;
            // btnWizard.Visible = true;
            //chkGeneralRXOnly.Checked = false;
            //chkGeneralRXOnly.Enabled = true;
            // grpHWSoftRock.Visible = false;
            //  chkGeneralCustomFilter.Visible = false;
            //  grpGenAutoMute.Visible = false;
            grpGenCalRXImage.Visible = false;
            //chkGenTX1Delay.Visible = false;
            //lblGenTX1Delay.Visible = false;
            //udGenTX1Delay.Visible = false;
            //chkGeneralEnableX2.Visible = false;
            //lblGeneralX2Delay.Visible = false;
            //udGeneralX2Delay.Visible = false;
            //  rtxtPACalReq.Visible = false;
            // lblPTTOutDelay.Visible = true;
            // udGenPTTOutDelay.Visible = true;
            lblMoxDelay.Visible = true;
            udMoxDelay.Visible = true;
            udMoxDelay.Enabled = true;
            //chkPTTOutDelay.Visible = true;
            //chkPTTOutDelay.Enabled = true;
            udRFDelay.Visible = true;
            udRFDelay.Enabled = true;
            lblRFDelay.Visible = true;
            grpImpulseTest.Visible = false;
            // chkGeneralPAPresent.Checked = true;

            if (is_hermes)
            {
                if (radGenModelANAN200D.Checked || radGenModelANAN7000D.Checked ||
                    radGenModelORIONMKII.Checked || radGenModelANAN8000D.Checked)
                {
                    groupBoxHPSDRHW.Visible = false;
                    grpGeneralHardwareORION.Visible = true;
                }
                else
                {
                    grpGeneralHardwareORION.Visible = false;
                    // groupBoxHPSDRHW.Visible = true;
                }
                grpOzyType.Visible = true;
                grpOzyType.Enabled = true;
                grpMetisAddr.Visible = true;
            }
            grpGenCalRXImage.Enabled = false;
            chkCalExpert.Enabled = false;
            grpHPSDRFreqCalDbg.Visible = true;

            if (radGenModelORIONMKII.Checked || radGenModelANAN8000D.Checked || radGenModelANAN7000D.Checked)
            {
                chkLPFBypass.Checked = false;
                chkLPFBypass.Visible = false;
                console.MKIIBPFPresent = true;
                chkDisableRXOut.Visible = false;
                chkBPF2Gnd.Visible = true;
                chkEnableXVTRHF.Visible = true;
                toolTip1.SetToolTip(chkEXT2OutOnTx, "Enable Rx BYPASS during transmit.");
            }
            else
            {
                chkLPFBypass.Visible = true;
                console.MKIIBPFPresent = false;
                chkDisableRXOut.Visible = true;
                chkBPF2Gnd.Visible = false;
                chkEnableXVTRHF.Visible = false;
                toolTip1.SetToolTip(chkEXT2OutOnTx, "Enable RX 1 IN on Alex or Ext 2 on ANAN during transmit.");
            }


            if (radGenModelANAN10.Checked || radGenModelANAN10E.Checked)
            {
                chkRxOutOnTx.Checked = false;
                chkRxOutOnTx.Visible = false;
                chkEXT1OutOnTx.Checked = false;
                chkEXT1OutOnTx.Visible = false;
                chkEXT2OutOnTx.Checked = false;
                chkEXT2OutOnTx.Visible = false;
                panelAlex1HPFControl.Visible = false;
                tpAlexFilterControl.Text = "LPF";
                panelAlexRXXVRTControl.Visible = false;
                labelAlexFilterActive.Location = new Point(298, 0);
                grp10WattMeterTrim.BringToFront();
                chkEnableXVTRHF.Visible = false;
            }
            else if (radGenModelORIONMKII.Checked || radGenModelANAN7000D.Checked || radGenModelANAN8000D.Checked)
            {
                if (radGenModelANAN7000D.Checked)
                {
                    chkRxOutOnTx.Visible = false;
                    chkEXT1OutOnTx.Visible = false;
                    chkEXT2OutOnTx.Visible = true;
                    panelAlexRXXVRTControl.Visible = true;
                    grp100WattMeterTrim.BringToFront();
                }
                else
                {
                    chkRxOutOnTx.Visible = false;
                    chkEXT1OutOnTx.Visible = false;
                    chkEXT2OutOnTx.Visible = false;
                    panelAlexRXXVRTControl.Visible = false;
                    grp200WattMeterTrim.BringToFront();
                }

                tpAlexFilterControl.Text = "BPF1/LPF";
                tpAlex2FilterControl.Text = "BPF2";
                labelAlex1FilterHPF.Text = "BPF1";
                chkAlexHPFBypass.Text = "ByPass/55 MHz BPF";
                chkDisableHPFonTX.Text = "BPF ByPass on TX";
                panelAlexRXXVRTControl.Visible = false;
                labelAlexFilterActive.Location = new Point(275, 0);
                ud6mRx2LNAGainOffset.Visible = true;
                lblRx26mLNA.Visible = true;
                chkEnableXVTRHF.Visible = true;
            }
            else
            {
                chkRxOutOnTx.Enabled = true;
                chkRxOutOnTx.Visible = true;
                chkEXT1OutOnTx.Enabled = true;
                chkEXT1OutOnTx.Visible = true;
                chkEXT2OutOnTx.Enabled = true;
                chkEXT2OutOnTx.Visible = true;
                panelAlex1HPFControl.Visible = true;
                tpAlexFilterControl.Text = "HPF/LPF";
                labelAlex1FilterHPF.Text = "HPF";
                chkAlexHPFBypass.Text = "ByPass/55 MHz HPF";
                chkDisableHPFonTX.Text = "HPF ByPass on TX";
                panelAlexRXXVRTControl.Visible = true;
                labelAlexFilterActive.Location = new Point(275, 0);
                ud6mRx2LNAGainOffset.Visible = false;
                lblRx26mLNA.Visible = false;
                grp100WattMeterTrim.BringToFront();
                chkEnableXVTRHF.Visible = false;
            }

            if (radGenModelHermes.Checked || radGenModelHPSDR.Checked)
            {
                tpAlexControl.Text = "Alex";
                chkHFTRRelay.Checked = false;
                chkHFTRRelay.Enabled = false;
                chkHFTRRelay.Visible = false;
            }
            else
            {
                tpAlexControl.Text = "Ant/Filters";
                chkHFTRRelay.Visible = true;
                chkHFTRRelay.Enabled = true;
            }

            if (radGenModelHPSDR.Checked) tpPennyCtrl.Text = "Penny Ctrl";
            else if (radGenModelHermes.Checked) tpPennyCtrl.Text = "Hermes Ctrl";
            // else if (radGenModelOrion.Checked) tpPennyCtrl.Text = "Orion Ctrl";
            else tpPennyCtrl.Text = "OC Control";

            if (!console.RX2PreampPresent &&
                console.diversityForm != null)
                console.diversityForm.Dispose();

            // console.EnableDup();

            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();

            // console.UpdatePAVoltsAmpsDisplay();
        }

        public void UpdateDisplayMeter()
        {
            //txtMeterOffset.Text = console.PreampOffset.ToString();
            //txtDisplayOffset.Text = console.RX1DisplayCalOffset.ToString();
            MeterOffset = console.MultiMeterCalOffset;
            DisplayOffset = console.RX1DisplayCalOffset;
        }

        public void AddHPSDRPages()
        {

            /*if (tcGeneral.TabPages.Contains(tpRX2))
            {
                tcGeneral.TabPages.Remove(tpRX2);
                tcGeneral.SelectedIndex = 0;
            }*/

            /*  if (tcAudio.TabPages.Contains(tpVAC2))
               {
                   tcAudio.TabPages.Remove(tpVAC2);
                   tcAudio.SelectedIndex = 0;
               } */

            /*  if (tcDSP.TabPages.Contains(tpDSPImageReject))
              {
                  tcDSP.TabPages.Remove(tpDSPImageReject);
                  tcDSP.SelectedIndex = 0;
              } */

            /* if (tcDSP.TabPages.Contains(tpDSPEER))
              {
                  tcDSP.TabPages.Remove(tpDSPEER);
                  tcDSP.SelectedIndex = 0;
              } */

            if (tcGeneral.TabPages.Contains(tpInfo)) // Info page
            {
                tcGeneral.TabPages.Remove(tpInfo);
                tcGeneral.SelectedIndex = 0;
            }

            //if (tcCAT.TabPages.Contains(tpCAT2)) // CAT2 page
            //{
            //    tcCAT.TabPages.Remove(tpCAT2);
            //    tcCAT.SelectedIndex = 0;
            //}

            if (!tcGeneral.TabPages.Contains(tpHPSDR))
            {
                Common.TabControlInsert(tcGeneral, tpHPSDR, 1);
            }
            else
            {
                if (tcGeneral.TabPages.IndexOf(tpHPSDR) != 1)
                {
                    tcGeneral.TabPages.Remove(tpHPSDR);
                    Common.TabControlInsert(tcGeneral, tpHPSDR, 1);
                }
            }

            if (radGenModelANAN200D.Checked || radGenModelANAN100D.Checked ||
                radGenModelORIONMKII.Checked || radGenModelANAN8000D.Checked || radGenModelANAN7000D.Checked)
            {
                if (!tcGeneral.TabPages.Contains(tpADC))
                {
                    Common.TabControlInsert(tcGeneral, tpADC, 2);
                }

                else
                {
                    if (tcGeneral.TabPages.IndexOf(tpADC) != 2)
                    {
                        tcGeneral.TabPages.Remove(tpADC);
                        Common.TabControlInsert(tcGeneral, tpADC, 2);
                    }
                }
            }
            else
            {
                if (tcGeneral.TabPages.Contains(tpADC))
                {
                    tcGeneral.TabPages.Remove(tpADC);
                    tcGeneral.SelectedIndex = 0;
                }
            }

            if (!tcGeneral.TabPages.Contains(tpPennyCtrl))
            {
                Common.TabControlInsert(tcGeneral, tpPennyCtrl, 5);
            }
            else
            {
                if (tcGeneral.TabPages.IndexOf(tpPennyCtrl) != 5)
                {
                    tcGeneral.TabPages.Remove(tpPennyCtrl);
                    Common.TabControlInsert(tcGeneral, tpPennyCtrl, 5);
                }
            }

            if (!tcGeneral.TabPages.Contains(tpAlexControl))
            {
                Common.TabControlInsert(tcGeneral, tpAlexControl, 6);
            }
            else
            {
                if (tcGeneral.TabPages.IndexOf(tpAlexControl) != 6)
                {
                    tcGeneral.TabPages.Remove(tpAlexControl);
                    Common.TabControlInsert(tcGeneral, tpAlexControl, 6);
                }
            }

            if (radGenModelANAN200D.Checked || radGenModelHermes.Checked || radGenModelANAN100D.Checked)
            {
                if (!tcGeneral.TabPages.Contains(tpApolloControl))
                {
                    Common.TabControlInsert(tcGeneral, tpApolloControl, 7);
                }

                else
                {
                    if (tcGeneral.TabPages.IndexOf(tpApolloControl) != 7)
                    {
                        tcGeneral.TabPages.Remove(tpApolloControl);
                        Common.TabControlInsert(tcGeneral, tpApolloControl, 7);
                    }
                }
            }
            else // if (!console.RX2PreampPresent)
            {
                if (tcGeneral.TabPages.Contains(tpApolloControl))
                {
                    tcGeneral.TabPages.Remove(tpApolloControl);
                    tcGeneral.SelectedIndex = 0;
                }
            }

            if (radGenModelORIONMKII.Checked || radGenModelANAN8000D.Checked || radGenModelANAN7000D.Checked)
            {
                if (!tcAlexControl.TabPages.Contains(tpAlex2FilterControl))
                {
                    tcAlexControl.TabPages.Add(tpAlex2FilterControl);
                    //Common.TabControlInsert(tcAlexControl, tpAlex2FilterControl, 2);
                }
                //else
                //{
                //    if (tcAlexControl.TabPages.IndexOf(tpAlex2FilterControl) != 2)
                //    {
                //        tcAlexControl.TabPages.Remove(tpAlex2FilterControl);
                //        Common.TabControlInsert(tcAlexControl, tpAlex2FilterControl, 2);
                //    }
                //}
            }
            else
            {
                if (tcAlexControl.TabPages.Contains(tpAlex2FilterControl))
                {
                    tcAlexControl.TabPages.Remove(tpAlex2FilterControl);
                    //tcAlexControl.SelectedIndex = 0;
                }
            }

            // now make sure enablements are correct 
            if (!chkAlexPresent.Checked)
            {
                chkAlexAntCtrl.Enabled = false;
                SetAlexAntEnabled(false);
            }
            if (!(chkPennyPresent.Checked || chkPennyLane.Checked))
            {
                chkPennyExtCtrl.Enabled = false;
                grpPennyExtCtrl.Enabled = false;
            }
            console.MaxFreq = (double)udMaxFreq.Value;
        }

        private void chkGeneralRXOnly_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkGeneralRXOnly.Focused &&
                // comboAudioSoundCard.Text == "Unsupported Card" &&
                !chkGeneralRXOnly.Checked)
            {
                DialogResult dr = MessageBox.Show(
                    "Unchecking Receive Only while in Unsupported Card mode may \n" +
                    "cause damage to your SDR-1000 hardware.  Are you sure you want \n" +
                    "to enable transmit?",
                    "Warning: Enable Transmit?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    chkGeneralRXOnly.Checked = true;
                    return;
                }
            }
            console.RXOnly = chkGeneralRXOnly.Checked;
            tpTransmit.Enabled = !chkGeneralRXOnly.Checked;
            tpPowerAmplifier.Enabled = !chkGeneralRXOnly.Checked;
            grpTestTXIMD.Enabled = !chkGeneralRXOnly.Checked;
        }

        private void btnGeneralCalFreqStart_Click(object sender, System.EventArgs e)
        {
            btnGeneralCalFreqStart.Enabled = false;
            Thread t = new Thread(new ThreadStart(CalibrateFreq));
            t.Name = "Freq Calibration Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        private void btnGeneralCalLevelStart_Click(object sender, System.EventArgs e)
        {
            btnGeneralCalLevelStart.Enabled = false;
            progress = new Progress("Calibrate RX Level");

            Thread t = new Thread(new ThreadStart(CalibrateLevel));
            t.Name = "Level Calibration Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();

            if (console.PowerOn)
                progress.Show();
        }

        private void btnCalLevel_Click(object sender, System.EventArgs e)
        {
            btnCalLevel.Enabled = false;
            progress = new Progress("Calibrate RX2 Level");

            Thread t = new Thread(new ThreadStart(CalibrateRX2Level));
            t.Name = "Level Calibration Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
            t.Start();

            if (console.PowerOn)
                progress.Show();
        }

        private void btnGeneralCalImageStart_Click(object sender, System.EventArgs e)
        {
        }

        private void CalibrateFreq()
        {
            bool done = console.CalibrateFreq((float)udGeneralCalFreq1.Value);
            if (done) MessageBox.Show("Frequency Calibration complete.");
            btnGeneralCalFreqStart.Enabled = true;
        }

        private void CalibrateLevel()
        {
            bool done = console.CalibrateLevel(
                (float)udGeneralCalLevel.Value,
                (float)udGeneralCalFreq2.Value,
                progress,
                false);
            if (done) MessageBox.Show("Level Calibration complete.");
            btnGeneralCalLevelStart.Enabled = true;
            // UpdateDisplayMeter();

        }

        private void CalibrateRX2Level()
        {
            bool done = console.CalibrateRX2Level(
                (float)udGeneralCalRX2Level.Value,
                (float)udGeneralCalRX2Freq2.Value,
                progress,
                false);
            if (done) MessageBox.Show("Level Calibration complete.");
            btnCalLevel.Enabled = true;
            //  UpdateDisplayMeter();

        }

        private void chkGeneralDisablePTT_CheckedChanged(object sender, System.EventArgs e)
        {
            console.DisablePTT = chkGeneralDisablePTT.Checked;
        }

        private void comboGeneralXVTR_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //switch (comboGeneralXVTR.SelectedIndex)
            //{
            //    case (int)XVTRTRMode.NEGATIVE:
            //        if (comboGeneralXVTR.Focused)
            //        {
            //            MessageBox.Show("The default TR Mode for the DEMI144-28FRS sold by FlexRadio Systems is\n" +
            //                "Postive TR Logic.  Please use caution when using other TR modes.", "Warning");
            //        }
            //        break;
            //    case (int)XVTRTRMode.POSITIVE:
            //    case (int)XVTRTRMode.NONE:
            //        break;
            //}

            //console.CurrentXVTRTRMode = (XVTRTRMode)comboGeneralXVTR.SelectedIndex;
        }

        //  private void chkGeneralSoftwareGainCorr_CheckedChanged(object sender, System.EventArgs e)
        //  {
        // console.NoHardwareOffset = chkGeneralSoftwareGainCorr.Checked;
        //  }

        //private void chkGeneralEnableX2_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    console.X2Enabled = chkGeneralEnableX2.Checked;
        //    udGeneralX2Delay.Enabled = chkGeneralEnableX2.Checked;
        //}

        //private void udGeneralX2Delay_ValueChanged(object sender, System.EventArgs e)
        //{
        //    console.X2Delay = (int)udGeneralX2Delay.Value;
        //}

        private void comboGeneralProcessPriority_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Process p = Process.GetCurrentProcess();

            if (comboGeneralProcessPriority.Text == "Real Time" &&
                comboGeneralProcessPriority.Focused)
            {
                DialogResult dr = MessageBox.Show(
                    "Setting the Process Priority to Realtime can cause the system to become unresponsive.\n" +
                    "This setting is not recommended.\n" +
                    "Are you sure you want to change to Realtime?",
                    "Warning: Realtime Not Recommended",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    switch (p.PriorityClass)
                    {
                        case ProcessPriorityClass.Idle:
                            comboGeneralProcessPriority.Text = "Idle";
                            break;
                        case ProcessPriorityClass.BelowNormal:
                            comboGeneralProcessPriority.Text = "Below Normal";
                            break;
                        case ProcessPriorityClass.AboveNormal:
                            comboGeneralProcessPriority.Text = "Above Normal";
                            break;
                        case ProcessPriorityClass.High:
                            comboGeneralProcessPriority.Text = "Highest";
                            break;
                        default:
                            comboGeneralProcessPriority.Text = "Normal";
                            break;
                    }
                    return;
                }
            }

            switch (comboGeneralProcessPriority.Text)
            {
                case "Idle":
                    p.PriorityClass = ProcessPriorityClass.Idle;
                    break;
                case "Below Normal":
                    p.PriorityClass = ProcessPriorityClass.BelowNormal;
                    break;
                case "Normal":
                    p.PriorityClass = ProcessPriorityClass.Normal;
                    break;
                case "Above Normal":
                    p.PriorityClass = ProcessPriorityClass.AboveNormal;
                    break;
                case "High":
                    p.PriorityClass = ProcessPriorityClass.High;
                    break;
                case "Real Time":
                    p.PriorityClass = ProcessPriorityClass.RealTime;
                    break;
            }
        }

        //private void chkGeneralCustomFilter_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    console.EnableLPF0 = chkGeneralCustomFilter.Checked;
        //}

        //private void chkGenAutoMute_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    console.AutoMute = chkGenAutoMute.Checked;
        //}

        private void chkOptQuickQSY_CheckedChanged(object sender, System.EventArgs e)
        {
            console.QuickQSY = chkOptQuickQSY.Checked;
        }

        private void chkOptAlwaysOnTop_CheckedChanged(object sender, System.EventArgs e)
        {
            console.AlwaysOnTop = chkOptAlwaysOnTop.Checked;
        }

        private void udOptClickTuneOffsetDIGL_ValueChanged(object sender, System.EventArgs e)
        {
            console.DIGLClickTuneOffset = (int)udOptClickTuneOffsetDIGL.Value;
        }

        private void udOptClickTuneOffsetDIGU_ValueChanged(object sender, System.EventArgs e)
        {
            console.DIGUClickTuneOffset = (int)udOptClickTuneOffsetDIGU.Value;
        }

        private void udOptMaxFilterWidth_ValueChanged(object sender, System.EventArgs e)
        {
            console.MaxFilterWidth = (int)udOptMaxFilterWidth.Value;
        }

        private void comboOptFilterWidthMode_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboOptFilterWidthMode.Text)
            {
                case "Linear":
                    console.CurrentFilterWidthMode = FilterWidthMode.Linear;
                    break;
                case "Log":
                    console.CurrentFilterWidthMode = FilterWidthMode.Log;
                    break;
                case "Log10":
                    console.CurrentFilterWidthMode = FilterWidthMode.Log10;
                    break;
            }
        }

        private void udOptMaxFilterShift_ValueChanged(object sender, System.EventArgs e)
        {
            console.MaxFilterShift = (int)udOptMaxFilterShift.Value;
        }

        private void chkOptFilterSaveChanges_CheckedChanged(object sender, System.EventArgs e)
        {
            console.SaveFilterChanges = chkOptFilterSaveChanges.Checked;
        }

        private void chkOptEnableKBShortcuts_CheckedChanged(object sender, System.EventArgs e)
        {
            console.EnableKBShortcuts = chkOptEnableKBShortcuts.Checked;
            chkOptQuickQSY.Enabled = chkOptEnableKBShortcuts.Checked;
        }

        private void udFilterDefaultLowCut_ValueChanged(object sender, System.EventArgs e)
        {
            console.DefaultLowCut = (int)udFilterDefaultLowCut.Value;
        }

        private void udRX2FilterDefaultLowCut_ValueChanged(object sender, System.EventArgs e)
        {
            console.DefaultRX2LowCut = (int)udRX2FilterDefaultLowCut.Value;
        }

        #endregion

        #region Audio Tab Event Handlers
        // ======================================================
        // Audio Tab Event Handlers
        // ======================================================

        private void chkAudioEnableVAC_CheckedChanged(object sender, System.EventArgs e)
        {
            bool val = chkAudioEnableVAC.Checked;
            bool old_val = console.VACEnabled;

            if (val)
            {
                if (comboAudioDriver2.SelectedIndex < 0 &&
                    comboAudioDriver2.Items.Count > 0)
                    comboAudioDriver2.SelectedIndex = 0;
            }

            //bool power = console.PowerOn;
            //if (power && val != old_val)
            //{
            //    console.PowerOn = false;
            //    Thread.Sleep(100);
            //}

            console.VACEnabled = val;
            // if (power && val != old_val) console.PowerOn = true;
        }

        private void chkVAC2Enable_CheckedChanged(object sender, System.EventArgs e)
        {
            bool val = chkVAC2Enable.Checked;
            bool old_val = console.VAC2Enabled;

            if (val)
            {
                if (comboAudioDriver3.SelectedIndex < 0 &&
                    comboAudioDriver3.Items.Count > 0)
                    comboAudioDriver3.SelectedIndex = 0;
            }

            //bool power = console.PowerOn;
            //if (power && val != old_val)
            //{
            //    console.PowerOn = false;
            //    Thread.Sleep(100);
            //}

            console.VAC2Enabled = val;
            // if (power && val != old_val) console.PowerOn = true;
        }

        private void comboAudioDriver2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioDriver2.SelectedIndex < 0) return;

            int old_driver = Audio.Host2;
            int new_driver = ((PADeviceInfo)comboAudioDriver2.SelectedItem).Index;
            bool power = console.PowerOn;

            if (power && chkAudioEnableVAC.Checked && old_driver != new_driver)
            {
                // console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            string new_driver_name = ((PADeviceInfo)comboAudioDriver2.SelectedItem).Name;

            console.AudioDriverIndex2 = new_driver;
            Audio.Host2 = new_driver;
            GetDevices2();
            if (comboAudioInput2.Items.Count != 0)
                comboAudioInput2.SelectedIndex = 0;
            if (comboAudioOutput2.Items.Count != 0)
                comboAudioOutput2.SelectedIndex = 0;
            if (power && chkAudioEnableVAC.Checked && old_driver != new_driver)
            {
                // console.PowerOn = true;
                Audio.EnableVAC1(false);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }
        }

        private void comboAudioDriver3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioDriver3.SelectedIndex < 0) return;

            int old_driver = Audio.Host3;
            int new_driver = ((PADeviceInfo)comboAudioDriver3.SelectedItem).Index;
            bool power = console.PowerOn;

            if (power && chkVAC2Enable.Checked && old_driver != new_driver)
            {
                //console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            string new_driver_name = ((PADeviceInfo)comboAudioDriver3.SelectedItem).Name;

            console.AudioDriverIndex3 = new_driver;
            Audio.Host3 = new_driver;
            GetDevices3();
            if (comboAudioInput3.Items.Count != 0)
                comboAudioInput3.SelectedIndex = 0;
            if (comboAudioOutput3.Items.Count != 0)
                comboAudioOutput3.SelectedIndex = 0;

            if (power && chkVAC2Enable.Checked && old_driver != new_driver)
            {
                // console.PowerOn = true;
                Audio.EnableVAC2(false);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }
        }

        private void comboAudioInput2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioInput2.SelectedIndex < 0) return;

            int old_input = Audio.Input2;
            int new_input = ((PADeviceInfo)comboAudioInput2.SelectedItem).Index;
            bool power = console.PowerOn;

            if (power && chkAudioEnableVAC.Checked && old_input != new_input)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            console.AudioInputIndex2 = new_input;
            Audio.Input2 = new_input;

            if (power && chkAudioEnableVAC.Checked && old_input != new_input)
            {
                // console.PowerOn = true;
                Audio.EnableVAC1(false);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }
        }

        private void comboAudioInput3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioInput3.SelectedIndex < 0) return;

            int old_input = Audio.Input3;
            int new_input = ((PADeviceInfo)comboAudioInput3.SelectedItem).Index;
            bool power = console.PowerOn;

            if (power && chkVAC2Enable.Checked && old_input != new_input)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            console.AudioInputIndex3 = new_input;
            Audio.Input3 = new_input;

            if (power && chkVAC2Enable.Checked && old_input != new_input)
            {
                //console.PowerOn = true;
                Audio.EnableVAC2(false);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }
        }

        private void comboAudioOutput2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioOutput2.SelectedIndex < 0) return;

            int old_output = Audio.Output2;
            int new_output = ((PADeviceInfo)comboAudioOutput2.SelectedItem).Index;
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked && old_output != new_output)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            console.AudioOutputIndex2 = new_output;
            Audio.Output2 = new_output;

            if (power && chkAudioEnableVAC.Checked && old_output != new_output)
            {
                // console.PowerOn = true;
                Audio.EnableVAC1(false);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }
        }

        private void comboAudioOutput3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioOutput3.SelectedIndex < 0) return;

            int old_output = Audio.Output3;
            int new_output = ((PADeviceInfo)comboAudioOutput3.SelectedItem).Index;
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked && old_output != new_output)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            console.AudioOutputIndex3 = new_output;
            Audio.Output3 = new_output;

            if (power && chkVAC2Enable.Checked && old_output != new_output)
            {
                // console.PowerOn = true;
                Audio.EnableVAC2(false);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }

        }

        public void forceAudioSampleRate1(String rate)
        {
            comboAudioSampleRate1.Text = rate;
        }

        public void forceAudioSampleRateRX2(String rate)
        {
            comboAudioSampleRateRX2.Text = rate;
        }

        private bool force_reset = false;
        public bool ForceReset
        {
            set
            {
                force_reset = value;
                if (value)
                {
                    comboAudioSampleRate1_SelectedIndexChanged(this, EventArgs.Empty);
                    comboAudioSampleRateRX2_SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        private void comboAudioSampleRate1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioSampleRate1.SelectedIndex < 0) return;

            int old_rate = console.SampleRate1;
            int new_rate = Int32.Parse(comboAudioSampleRate1.Text);
            bool was_enabled = true;
            if (new_rate != old_rate || initializing)
            {
                // turn OFF the DSP channels so they get flushed out (must do while data is flowing to get slew-down and flush)
                WDSP.SetChannelState(WDSP.id(0, 1), 0, 0);
                WDSP.SetChannelState(WDSP.id(0, 0), 0, 1);
                Thread.Sleep(10);

                // remove the RX1 main and sub audio streams from the mix set
                unsafe { cmaster.SetAAudioMixStates((void*)0, 0, 3, 0); }

                // disable VAC
                if (console.VACEnabled) Audio.EnableVAC1(false);

                // turn OFF the DDC(s) 
                NetworkIO.EnableRx(0, 0);
                NetworkIO.EnableRx(1, 0);
                NetworkIO.EnableRx(2, 0);

                //wait for all inflight packets to arrive - need to experiment with this delay value
                Thread.Sleep(20);

                // flush any buffers where these old-rate packets may be stored ... this is done via operations in SetXcmInrate()

                // in the property SampleRate1:
                //    (1) the new samplerate will be sent to the DDC
                //    (2) SetXcmInrate() is called by Audio.SampleRate1 which is called by console.SampleRate1
                console.SampleRate1 = new_rate;

                // set the corresponding new buffer size
                int new_size = cmaster.GetBuffSize(new_rate);
                console.BlockSize1 = new_size;

                // set rate and size for the display too; could move this to SetXcmInrate() in cmaster.c
                console.specRX.GetSpecRX(0).SampleRate = new_rate;
                console.specRX.GetSpecRX(0).BlockSize = new_size;

                if (was_enabled)
                {
                    // turn on the DDC(s)
                    console.UpdateDDCs(true);
                    // wait for samples at the new rate to be received
                    Thread.Sleep(1);
                    // add the audio streams to the mix set
                    unsafe { cmaster.SetAAudioMixStates((void*)0, 0, 3, 3); }
                }

                // enable VAC
                if (console.VACEnabled) Audio.EnableVAC1(true);

                // turn ON the DSP channels
                int w_enable = 0;
                if (was_enabled) w_enable = 1;
                WDSP.SetChannelState(WDSP.id(0, 0), w_enable, 0);
                if (console.radio.GetDSPRX(0, 1).Active) WDSP.SetChannelState(WDSP.id(0, 1), w_enable, 0);

                // calculate and display the new bin_width
                double bin_width = (double)new_rate / (double)console.specRX.GetSpecRX(0).FFTSize;
                lblDisplayBinWidth.Text = bin_width.ToString("N3");
            }

        }

        private void comboAudioSampleRateRX2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioSampleRateRX2.SelectedIndex < 0) return;

            int old_rate = console.SampleRateRX2;
            int new_rate = Int32.Parse(comboAudioSampleRateRX2.Text);
            bool was_enabled = console.RX2Enabled;
            if (new_rate != old_rate || initializing)
            {
                // turn OFF the DSP channel so it gets flushed out (must do while data is flowing to get slew-down and flush)
                WDSP.SetChannelState(WDSP.id(2, 0), 0, 1);
                // remove the RX2 audio stream from the mix set
                unsafe { cmaster.SetAAudioMixState((void*)0, 0, 2, false); }
                // disable VAC
                if (console.VAC2Enabled) Audio.EnableVAC2(false);
                // turn OFF the DDC for RX2; had to add rx2_enabled as a parameter to UpdateReceivers() to do this --- THIS IS S HACK.  BETTER WAY?
                console.UpdateDDCs(false);
                // wait for all inflight packets to arrive
                Thread.Sleep(20);   // need to experiment with this time

                // flush any buffers where these old-rate packets may be stored ... this is done via operations in SetXcmInrate()

                // in the property SampleRateRX2:
                //    (1) the new samplerate will be sent to the DDC
                //    (2) SetXcmInrate() is called by Audio.SampleRateRX2 which is called by console.SampleRateRX2
                console.SampleRateRX2 = new_rate;
                // set the corresponding new buffer size
                int new_size = cmaster.GetBuffSize(new_rate);
                console.BlockSizeRX2 = new_size;
                // set rate and size for the display too; could move this to SetXcmInrate() in cmaster.c
                console.specRX.GetSpecRX(1).SampleRate = new_rate;
                console.specRX.GetSpecRX(1).BlockSize = new_size;
                // turn the DDC for RX2 back ON; had to add rx2_enabled as a parameter to UpdateReceivers() to do this
                if (was_enabled)
                {
                    console.UpdateDDCs(true);
                    // wait for samples at the new rate to be received
                    Thread.Sleep(1);   // need to experiment with this time
                    // add the RX2 audio stream to the mix set
                    unsafe { cmaster.SetAAudioMixState((void*)0, 0, 2, true); }
                }
                // enable VAC2
                if (console.VAC2Enabled) Audio.EnableVAC2(true);
                // turn ON the DSP channel if it was ON before
                int w_enable = 0;
                if (was_enabled) w_enable = 1;
                WDSP.SetChannelState(WDSP.id(2, 0), w_enable, 0);
                // calculate and display the new bin_width
                double bin_width = (double)new_rate / (double)console.specRX.GetSpecRX(1).FFTSize;
                lblRX2DisplayBinWidth.Text = bin_width.ToString("N3");
            }
        }

        private void comboAudioSampleRate2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioSampleRate2.SelectedIndex < 0) return;

            int old_rate = console.SampleRate2;
            int new_rate = Int32.Parse(comboAudioSampleRate2.Text);
            bool poweron = console.PowerOn;

            if (poweron && chkAudioEnableVAC.Checked && new_rate != old_rate)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC1(false);
                // Thread.Sleep(10);
            }

            console.SampleRate2 = new_rate;
            console.VACSampleRate = comboAudioSampleRate2.Text;

            if (poweron && chkAudioEnableVAC.Checked && new_rate != old_rate)
            {
                // console.PowerOn = true;
                // Thread.Sleep(10);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }
        }

        private void comboAudioSampleRate3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioSampleRate3.SelectedIndex < 0) return;

            int old_rate = console.SampleRate3;
            int new_rate = Int32.Parse(comboAudioSampleRate3.Text);
            bool poweron = console.PowerOn;

            if (poweron && chkVAC2Enable.Checked && new_rate != old_rate)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC2(false);
                //Thread.Sleep(10);
            }

            console.SampleRate3 = new_rate;
            console.VAC2SampleRate = comboAudioSampleRate3.Text;

            if (poweron && chkVAC2Enable.Checked && new_rate != old_rate)
            {
                //console.PowerOn = true;
                // Thread.Sleep(10);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }
        }

        private void comboAudioBuffer1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            /*if (comboAudioBuffer1.SelectedIndex < 0) return;

            int old_size = console.BlockSize1;
            int new_size = Int32.Parse(comboAudioBuffer1.Text);
            bool power = console.PowerOn;

            if (power && old_size != new_size)
            {
                console.PowerOn = false;
                Thread.Sleep(100);
            }

            console.specRX.GetSpecRX(0).BlockSize = new_size;
            console.specRX.GetSpecRX(1).BlockSize = new_size;
            console.BlockSize1 = new_size;

            if (power && old_size != new_size) console.PowerOn = true;*/
        }

        private void comboAudioBuffer2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioBuffer2.SelectedIndex < 0) return;

            int old_size = console.BlockSize2;
            int new_size = Int32.Parse(comboAudioBuffer2.Text);
            bool power = console.PowerOn;

            if (power && chkAudioEnableVAC.Checked && old_size != new_size)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            console.BlockSize2 = new_size;

            if (power && chkAudioEnableVAC.Checked && old_size != new_size)
            {
                // console.PowerOn = true;
                //Thread.Sleep(100);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }
        }

        private void comboAudioBuffer3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboAudioBuffer3.SelectedIndex < 0) return;

            int old_size = console.BlockSize3;
            int new_size = Int32.Parse(comboAudioBuffer3.Text);
            bool power = console.PowerOn;

            if (power && chkVAC2Enable.Checked && old_size != new_size)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            console.BlockSize3 = new_size;

            if (power && chkVAC2Enable.Checked && old_size != new_size)
            {
                // console.PowerOn = true;
                //Thread.Sleep(100);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }
        }

        private void udAudioLatency2_ValueChanged(object sender, System.EventArgs e)
        {
            // RingBuffer In
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            Audio.Latency2 = (int)udAudioLatency2.Value;

            if (power && chkAudioEnableVAC.Checked)
                // console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void udAudioLatency2_Out_ValueChanged(object sender, System.EventArgs e)
        {
            // RingBuffer Out
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            Audio.Latency2_Out = (int)udAudioLatency2_Out.Value;

            if (power && chkAudioEnableVAC.Checked)
                //console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }


        private void udAudioLatencyPAIn_ValueChanged(object sender, System.EventArgs e)
        {
            // PortAudio In
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            Audio.LatencyPAIn = (int)udAudioLatencyPAIn.Value;

            if (power && chkAudioEnableVAC.Checked)
                // console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void udAudioLatencyPAOut_ValueChanged(object sender, System.EventArgs e)
        {
            // PortAudio Out
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            Audio.LatencyPAOut = (int)udAudioLatencyPAOut.Value;

            if (power && chkAudioEnableVAC.Checked)
                // console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void udVAC2Latency_ValueChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            Audio.Latency3 = (int)udVAC2Latency.Value;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void udVAC2LatencyOut_ValueChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            Audio.VAC2LatencyOut = (int)udVAC2LatencyOut.Value;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void udVAC2LatencyPAIn_ValueChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            Audio.VAC2LatencyPAIn = (int)udVAC2LatencyPAIn.Value;

            if (power && chkVAC2Enable.Checked)
                //  console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void udVAC2LatencyPAOut_ValueChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            Audio.VAC2LatencyPAOut = (int)udVAC2LatencyPAOut.Value;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void chkAudio2Stereo_CheckedChanged(object sender, System.EventArgs e)
        {
            //bool power = console.PowerOn;
            //if (power && chkAudioEnableVAC.Checked)
            //{
            //    console.PowerOn = false;
            //    Thread.Sleep(100);
            //}

            console.VACSoundCardStereo = chkAudio2Stereo.Checked;
            console.VACStereo = chkAudio2Stereo.Checked;
            chkVACCombine.Enabled = chkAudio2Stereo.Checked;

            //if (power && chkAudioEnableVAC.Checked)
            //    console.PowerOn = true;
        }

        private void chkAudioStereo3_CheckedChanged(object sender, System.EventArgs e)
        {
            //bool power = console.PowerOn;
            //if (power && chkVAC2Enable.Checked)
            //{
            //    console.PowerOn = false;
            //    Thread.Sleep(100);
            //}

            console.VAC2SoundCardStereo = chkAudio2Stereo.Checked;
            console.VAC2Stereo = chkAudioStereo3.Checked;
            chkVAC2Combine.Enabled = chkAudioStereo3.Checked;

            //if (power && chkVAC2Enable.Checked)
            //    console.PowerOn = true;
        }

        private void udAudioVACGainRX_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VACRXScale = Math.Pow(10.0, (int)udAudioVACGainRX.Value / 20.0);
            console.VACRXGain = (int)udAudioVACGainRX.Value;
        }

        private void udVAC2GainRX_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VAC2RXScale = Math.Pow(10.0, (int)udVAC2GainRX.Value / 20.0);
            console.VAC2RXGain = (int)udVAC2GainRX.Value;
        }

        private void udAudioVACGainTX_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VACPreamp = Math.Pow(10.0, (int)udAudioVACGainTX.Value / 20.0);
            console.VACTXGain = (int)udAudioVACGainTX.Value;
        }

        private void udVAC2GainTX_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VAC2TXScale = Math.Pow(10.0, (int)udVAC2GainTX.Value / 20.0);
            console.VAC2TXGain = (int)udVAC2GainTX.Value;
        }

        private void chkAudioVACAutoEnable_CheckedChanged(object sender, System.EventArgs e)
        {
            console.VACAutoEnable = chkAudioVACAutoEnable.Checked;
        }

        private void chkVAC2AutoEnable_CheckedChanged(object sender, System.EventArgs e)
        {
            console.VAC2AutoEnable = chkVAC2AutoEnable.Checked;
        }

        private void chkAudioLatencyManual2_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                //console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(0, 0);
                ivac.StopAudioIVAC(0);
            }

            udAudioLatency2.Enabled = chkAudioLatencyManual2.Checked;
            Audio.VAC1LatencyManual = chkAudioLatencyManual2.Checked;

            //if (!chkAudioLatencyManual2.Checked)
            //    Audio.Latency2 = 120;

            if (power && chkAudioEnableVAC.Checked)
                //console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void chkAudioLatencyManual2_Out_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                //console.PowerOn = false;
                //Thread.Sleep(100);
                ivac.SetIVACrun(0, 0);
                ivac.StopAudioIVAC(0);
            }

            udAudioLatency2_Out.Enabled = chkAudioLatencyManual2_Out.Checked;
            Audio.VAC1LatencyManualOut = chkAudioLatencyManual2_Out.Checked;

            //if (!chkAudioLatencyManual2_Out.Checked)
            //    Audio.Latency2_Out = 120;

            if (power && chkAudioEnableVAC.Checked)
                //console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void chkAudioLatencyPAInManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(0, 0);
                ivac.StopAudioIVAC(0);
            }

            udAudioLatencyPAIn.Enabled = chkAudioLatencyPAInManual.Checked;
            Audio.VAC1LatencyPAInManual = chkAudioLatencyPAInManual.Checked;

            //if (!chkAudioLatencyPAInManual.Checked)
            //    Audio.LatencyPAIn = 120;

            if (power && chkAudioEnableVAC.Checked)
                //console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void chkAudioLatencyPAOutManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(0, 0);
                ivac.StopAudioIVAC(0);
            }

            udAudioLatencyPAOut.Enabled = chkAudioLatencyPAOutManual.Checked;
            Audio.VAC1LatencyPAOutManual = chkAudioLatencyPAOutManual.Checked;

            //if (!chkAudioLatencyPAOutManual.Checked)
            //    Audio.LatencyPAOut = 120;

            if (power && chkAudioEnableVAC.Checked)
                //console.PowerOn = true;
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
        }

        private void chkVAC2LatencyManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(1, 0);
                ivac.StopAudioIVAC(1);
            }

            udVAC2Latency.Enabled = chkVAC2LatencyManual.Checked;
            Audio.VAC2LatencyManual = chkVAC2LatencyManual.Checked;

            //if (!chkVAC2LatencyManual.Checked)
            //    Audio.Latency3 = 120;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void chkVAC2LatencyOutManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(1, 0);
                ivac.StopAudioIVAC(1);
            }

            udVAC2LatencyOut.Enabled = chkVAC2LatencyOutManual.Checked;
            Audio.VAC2LatencyOutManual = chkVAC2LatencyOutManual.Checked;

            //if (!chkVAC2LatencyOutManual.Checked)
            //    Audio.VAC2LatencyOut = 120;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void chkVAC2LatencyPAInManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(1, 0);
                ivac.StopAudioIVAC(1);
            }

            udVAC2LatencyPAIn.Enabled = chkVAC2LatencyPAInManual.Checked;
            Audio.VAC2LatencyPAInManual = chkVAC2LatencyPAInManual.Checked;

            //if (!chkVAC2LatencyPAInManual.Checked)
            //    Audio.VAC2LatencyPAIn = 120;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void chkVAC2LatencyPAOutManual_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                ivac.SetIVACrun(1, 0);
                ivac.StopAudioIVAC(1);
            }

            udVAC2LatencyPAOut.Enabled = chkVAC2LatencyPAOutManual.Checked;
            Audio.VAC2LatencyPAOutManual = chkVAC2LatencyPAOutManual.Checked;

            //if (!chkVAC2LatencyPAOutManual.Checked)
            //    Audio.VAC2LatencyPAOut = 120;

            if (power && chkVAC2Enable.Checked)
                // console.PowerOn = true;
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
        }

        private void chk20dbMicBoost_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chk20dbMicBoost.Checked) udVOXGain_ValueChanged(this, e);
            console.MicBoost = chk20dbMicBoost.Checked;
            udVOXGain.Enabled = chk20dbMicBoost.Checked;
            //console.SetMicGain();
        }

        #endregion

        #region Display Tab Event Handlers
        // ======================================================
        // Display Tab Event Handlers
        // ======================================================

        private void udDisplayGridMax_LostFocus(object sender, System.EventArgs e)
        {
            Display.SpectrumGridMax = (int)udDisplayGridMax.Value;
        }

        private void udTXGridMax_LostFocus(object sender, System.EventArgs e)
        {
            Display.TXSpectrumGridMax = (int)udTXGridMax.Value;
        }

        private void udDisplayGridMax_Click(object sender, System.EventArgs e)
        {
            udDisplayGridMax_LostFocus(sender, e);
        }

        private void udDisplayGridMax_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            udDisplayGridMax_LostFocus(sender, new System.EventArgs());
        }

        private void udDisplayFPS_ValueChanged(object sender, System.EventArgs e)
        {
            console.DisplayFPS = (int)udDisplayFPS.Value;
            console.specRX.GetSpecRX(0).FrameRate = (int)udDisplayFPS.Value;
            console.specRX.GetSpecRX(1).FrameRate = (int)udDisplayFPS.Value;
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).FrameRate = (int)udDisplayFPS.Value;

            udDisplayAVGTime_ValueChanged(this, EventArgs.Empty);
        }

        private void udDisplayGridMax_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udDisplayGridMax.Value <= udDisplayGridMin.Value)
            //   udDisplayGridMax.Value = udDisplayGridMin.Value + 10;
            // Display.SpectrumGridMax = (int)udDisplayGridMax.Value;
            UpdateDisplayGridBandInfo();
            switch (console.RX1Band)
            {
                case Band.B160M:
                    console.DisplayGridMax160m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax160m;
                    break;
                case Band.B80M:
                    console.DisplayGridMax80m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax80m;
                    break;
                case Band.B60M:
                    console.DisplayGridMax60m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax60m;
                    break;
                case Band.B40M:
                    console.DisplayGridMax40m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax40m;
                    break;
                case Band.B30M:
                    console.DisplayGridMax30m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax30m;
                    break;
                case Band.B20M:
                    console.DisplayGridMax20m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax20m;
                    break;
                case Band.B17M:
                    console.DisplayGridMax17m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax17m;
                    break;
                case Band.B15M:
                    console.DisplayGridMax15m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax15m;
                    break;
                case Band.B12M:
                    console.DisplayGridMax12m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax12m;
                    break;
                case Band.B10M:
                    console.DisplayGridMax10m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax10m;
                    break;
                case Band.B6M:
                    console.DisplayGridMax6m = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMax6m;
                    break;
                case Band.WWV:
                    console.DisplayGridMaxWWV = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMaxWWV;
                    break;
                case Band.GEN:
                    console.DisplayGridMaxGEN = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMaxGEN;
                    break;
                default:
                    console.DisplayGridMaxXVTR = (float)udDisplayGridMax.Value;
                    Display.SpectrumGridMax = (int)console.DisplayGridMaxXVTR;
                    break;
            }

        }

        private void udDisplayGridMin_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udDisplayGridMin.Value >= udDisplayGridMax.Value)
            //   udDisplayGridMin.Value = udDisplayGridMax.Value - 10;
            // Display.SpectrumGridMin = (int)udDisplayGridMin.Value;
            UpdateDisplayGridBandInfo();
            switch (console.RX1Band)
            {
                case Band.B160M:
                    console.DisplayGridMin160m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin160m;
                    break;
                case Band.B80M:
                    console.DisplayGridMin80m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin80m;
                    break;
                case Band.B60M:
                    console.DisplayGridMin60m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin60m;
                    break;
                case Band.B40M:
                    console.DisplayGridMin40m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin40m;
                    break;
                case Band.B30M:
                    console.DisplayGridMin30m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin30m;
                    break;
                case Band.B20M:
                    console.DisplayGridMin20m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin20m;
                    break;
                case Band.B17M:
                    console.DisplayGridMin17m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin17m;
                    break;
                case Band.B15M:
                    console.DisplayGridMin15m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin15m;
                    break;
                case Band.B12M:
                    console.DisplayGridMin12m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin12m;
                    break;
                case Band.B10M:
                    console.DisplayGridMin10m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin10m;
                    break;
                case Band.B6M:
                    console.DisplayGridMin6m = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMin6m;
                    break;
                case Band.WWV:
                    console.DisplayGridMinWWV = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMinWWV;
                    break;
                case Band.GEN:
                    console.DisplayGridMinGEN = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMinGEN;
                    break;
                default:
                    console.DisplayGridMinXVTR = (float)udDisplayGridMin.Value;
                    Display.SpectrumGridMin = (int)console.DisplayGridMinXVTR;
                    break;
            }

        }

        private void udDisplayGridStep_ValueChanged(object sender, System.EventArgs e)
        {
            Display.SpectrumGridStep = (int)udDisplayGridStep.Value;
        }

        private void udRX2DisplayGridMax_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udRX2DisplayGridMax.Value <= udRX2DisplayGridMin.Value)
            //    udRX2DisplayGridMax.Value = udRX2DisplayGridMin.Value + 10;
            // Display.RX2SpectrumGridMax = (int)udRX2DisplayGridMax.Value;
            UpdateDisplayGridBandInfo();
            switch (console.RX2Band)
            {
                case Band.B160M:
                    console.RX2DisplayGridMax160m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax160m;
                    break;
                case Band.B80M:
                    console.RX2DisplayGridMax80m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax80m;
                    break;
                case Band.B60M:
                    console.RX2DisplayGridMax60m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax60m;
                    break;
                case Band.B40M:
                    console.RX2DisplayGridMax40m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax40m;
                    break;
                case Band.B30M:
                    console.RX2DisplayGridMax30m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax30m;
                    break;
                case Band.B20M:
                    console.RX2DisplayGridMax20m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax20m;
                    break;
                case Band.B17M:
                    console.RX2DisplayGridMax17m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax17m;
                    break;
                case Band.B15M:
                    console.RX2DisplayGridMax15m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax15m;
                    break;
                case Band.B12M:
                    console.RX2DisplayGridMax12m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax12m;
                    break;
                case Band.B10M:
                    console.RX2DisplayGridMax10m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax10m;
                    break;
                case Band.B6M:
                    console.RX2DisplayGridMax6m = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMax6m;
                    break;
                case Band.WWV:
                    console.RX2DisplayGridMaxWWV = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMaxWWV;
                    break;
                case Band.GEN:
                    console.RX2DisplayGridMaxGEN = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMaxGEN;
                    break;
                default:
                    console.RX2DisplayGridMaxXVTR = (float)udRX2DisplayGridMax.Value;
                    Display.RX2SpectrumGridMax = (int)console.RX2DisplayGridMaxXVTR;
                    break;
            }
        }

        private void udRX2DisplayGridMin_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udRX2DisplayGridMin.Value >= udRX2DisplayGridMax.Value)
            //    udRX2DisplayGridMin.Value = udRX2DisplayGridMax.Value - 10;
            //  Display.RX2SpectrumGridMin = (int)udRX2DisplayGridMin.Value;
            UpdateDisplayGridBandInfo();
            switch (console.RX2Band)
            {
                case Band.B160M:
                    console.RX2DisplayGridMin160m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin160m;
                    break;
                case Band.B80M:
                    console.RX2DisplayGridMin80m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin80m;
                    break;
                case Band.B60M:
                    console.RX2DisplayGridMin60m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin60m;
                    break;
                case Band.B40M:
                    console.RX2DisplayGridMin40m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin40m;
                    break;
                case Band.B30M:
                    console.RX2DisplayGridMin30m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin30m;
                    break;
                case Band.B20M:
                    console.RX2DisplayGridMin20m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin20m;
                    break;
                case Band.B17M:
                    console.RX2DisplayGridMin17m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin17m;
                    break;
                case Band.B15M:
                    console.RX2DisplayGridMin15m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin15m;
                    break;
                case Band.B12M:
                    console.RX2DisplayGridMin12m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin12m;
                    break;
                case Band.B10M:
                    console.RX2DisplayGridMin10m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin10m;
                    break;
                case Band.B6M:
                    console.RX2DisplayGridMin6m = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMin6m;
                    break;
                case Band.WWV:
                    console.RX2DisplayGridMinWWV = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMinWWV;
                    break;
                case Band.GEN:
                    console.RX2DisplayGridMinGEN = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMinGEN;
                    break;
                default:
                    console.RX2DisplayGridMinXVTR = (float)udRX2DisplayGridMin.Value;
                    Display.RX2SpectrumGridMin = (int)console.RX2DisplayGridMinXVTR;
                    break;
            }
        }

        private void udRX2DisplayGridStep_ValueChanged(object sender, System.EventArgs e)
        {
            Display.RX2SpectrumGridStep = (int)udRX2DisplayGridStep.Value;
        }

        private void comboDisplayLabelAlign_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboDisplayLabelAlign.Text)
            {
                case "Left":
                    Display.DisplayLabelAlign = DisplayLabelAlignment.LEFT;
                    break;
                case "Cntr":
                    Display.DisplayLabelAlign = DisplayLabelAlignment.CENTER;
                    break;
                case "Right":
                    Display.DisplayLabelAlign = DisplayLabelAlignment.RIGHT;
                    break;
                case "Auto":
                    Display.DisplayLabelAlign = DisplayLabelAlignment.AUTO;
                    break;
                case "Off":
                    Display.DisplayLabelAlign = DisplayLabelAlignment.OFF;
                    break;
                default:
                    Display.DisplayLabelAlign = DisplayLabelAlignment.LEFT;
                    break;
            }
        }

        private void udDisplayPhasePts_ValueChanged(object sender, System.EventArgs e)
        {
            Display.PhaseNumPts = (int)udDisplayPhasePts.Value;
        }

        private void udDisplayAVGTime_ValueChanged(object sender, System.EventArgs e)
        {
            console.specRX.GetSpecRX(0).AvTau = 0.001 * (double)udDisplayAVGTime.Value;
            console.UpdateRXSpectrumDisplayVars();
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).AvTau = 0.001 * (double)udDisplayAVGTime.Value;
            console.UpdateTXSpectrumDisplayVars();
            double display_time = 1 / (double)udDisplayFPS.Value;
            int buffersToAvg = (int)((float)udDisplayAVGTime.Value * 0.001 / display_time);
            Display.DisplayAvgBlocks = (int)Math.Max(2, buffersToAvg);
        }

        private void udRX2DisplayAVGTime_ValueChanged(object sender, System.EventArgs e)
        {
            console.specRX.GetSpecRX(1).AvTau = 0.001 * (double)udRX2DisplayAVGTime.Value;
        }

        private void udDisplayMeterDelay_ValueChanged(object sender, System.EventArgs e)
        {
            console.MeterDelay = (int)udDisplayMeterDelay.Value;
        }

        private void udDisplayPeakText_ValueChanged(object sender, System.EventArgs e)
        {
            console.PeakTextDelay = (int)udDisplayPeakText.Value;
        }

        private void udDisplayCPUMeter_ValueChanged(object sender, System.EventArgs e)
        {
            console.CPUMeterDelay = (int)udDisplayCPUMeter.Value;
        }

        private void clrbtnWaterfallLow_Changed(object sender, System.EventArgs e)
        {
            Display.WaterfallLowColor = clrbtnWaterfallLow.Color;
        }

        private void clrbtnWaterfallHigh_Changed(object sender, System.EventArgs e)
        {
            Display.WaterfallHighColor = clrbtnWaterfallHigh.Color;
        }

        private void clrbtnWaterfallMid_Changed(object sender, System.EventArgs e)
        {
            Display.WaterfallMidColor = clrbtnWaterfallMid.Color;
        }

        private void udDisplayWaterfallLowLevel_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateWaterfallBandInfo();
            switch (console.RX1Band)
            {
                case Band.B160M:
                    console.WaterfallLowThreshold160m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold160m;
                    break;
                case Band.B80M:
                    console.WaterfallLowThreshold80m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold80m;
                    break;
                case Band.B60M:
                    console.WaterfallLowThreshold60m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold60m;
                    break;
                case Band.B40M:
                    console.WaterfallLowThreshold40m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold40m;
                    break;
                case Band.B30M:
                    console.WaterfallLowThreshold30m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold30m;
                    break;
                case Band.B20M:
                    console.WaterfallLowThreshold20m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold20m;
                    break;
                case Band.B17M:
                    console.WaterfallLowThreshold17m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold17m;
                    break;
                case Band.B15M:
                    console.WaterfallLowThreshold15m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold15m;
                    break;
                case Band.B12M:
                    console.WaterfallLowThreshold12m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold12m;
                    break;
                case Band.B10M:
                    console.WaterfallLowThreshold10m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold10m;
                    break;
                case Band.B6M:
                    console.WaterfallLowThreshold6m = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThreshold6m;
                    break;
                case Band.WWV:
                    console.WaterfallLowThresholdWWV = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThresholdWWV;
                    break;
                case Band.GEN:
                    console.WaterfallLowThresholdGEN = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThresholdGEN;
                    break;
                default:
                    console.WaterfallLowThresholdXVTR = (float)udDisplayWaterfallLowLevel.Value;
                    Display.WaterfallLowThreshold = console.WaterfallLowThresholdXVTR;
                    break;
            }
        }

        private void udDisplayWaterfallHighLevel_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateWaterfallBandInfo();
            switch (console.RX1Band)
            {
                case Band.B160M:
                    console.WaterfallHighThreshold160m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold160m;
                    break;
                case Band.B80M:
                    console.WaterfallHighThreshold80m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold80m;
                    break;
                case Band.B60M:
                    console.WaterfallHighThreshold60m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold60m;
                    break;
                case Band.B40M:
                    console.WaterfallHighThreshold40m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold40m;
                    break;
                case Band.B30M:
                    console.WaterfallHighThreshold30m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold30m;
                    break;
                case Band.B20M:
                    console.WaterfallHighThreshold20m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold20m;
                    break;
                case Band.B17M:
                    console.WaterfallHighThreshold17m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold17m;
                    break;
                case Band.B15M:
                    console.WaterfallHighThreshold15m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold15m;
                    break;
                case Band.B12M:
                    console.WaterfallHighThreshold12m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold12m;
                    break;
                case Band.B10M:
                    console.WaterfallHighThreshold10m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold10m;
                    break;
                case Band.B6M:
                    console.WaterfallHighThreshold6m = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThreshold6m;
                    break;
                case Band.WWV:
                    console.WaterfallHighThresholdWWV = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThresholdWWV;
                    break;
                case Band.GEN:
                    console.WaterfallHighThresholdGEN = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThresholdGEN;
                    break;
                default:
                    console.WaterfallHighThresholdXVTR = (float)udDisplayWaterfallHighLevel.Value;
                    Display.WaterfallHighThreshold = console.WaterfallHighThresholdXVTR;
                    break;
            }
        }

        private void udDisplayMultiPeakHoldTime_ValueChanged(object sender, System.EventArgs e)
        {
            console.MultimeterPeakHoldTime = (int)udDisplayMultiPeakHoldTime.Value;
        }

        private void udDisplayMultiTextHoldTime_ValueChanged(object sender, System.EventArgs e)
        {
            console.MultimeterTextPeakTime = (int)udDisplayMultiTextHoldTime.Value;
        }

        // RX2 WaterFall
        private void clrbtnRX2WaterfallLow_Changed(object sender, System.EventArgs e)
        {
            Display.RX2WaterfallLowColor = clrbtnRX2WaterfallLow.Color;
        }

        private void clrbtnRX2WaterfallHigh_Changed(object sender, System.EventArgs e)
        {
            Display.RX2WaterfallHighColor = clrbtnRX2WaterfallHigh.Color;
        }

        private void clrbtnRX2WaterfallMid_Changed(object sender, System.EventArgs e)
        {
            Display.RX2WaterfallMidColor = clrbtnRX2WaterfallMid.Color;
        }

        private void udRX2DisplayWaterfallLowLevel_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateWaterfallBandInfo();
            switch (console.RX2Band)
            {
                case Band.B160M:
                    console.RX2WaterfallLowThreshold160m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold160m;
                    break;
                case Band.B80M:
                    console.RX2WaterfallLowThreshold80m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold80m;
                    break;
                case Band.B60M:
                    console.RX2WaterfallLowThreshold60m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold60m;
                    break;
                case Band.B40M:
                    console.RX2WaterfallLowThreshold40m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold40m;
                    break;
                case Band.B30M:
                    console.RX2WaterfallLowThreshold30m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold30m;
                    break;
                case Band.B20M:
                    console.RX2WaterfallLowThreshold20m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold20m;
                    break;
                case Band.B17M:
                    console.RX2WaterfallLowThreshold17m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold17m;
                    break;
                case Band.B15M:
                    console.RX2WaterfallLowThreshold15m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold15m;
                    break;
                case Band.B12M:
                    console.RX2WaterfallLowThreshold12m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold12m;
                    break;
                case Band.B10M:
                    console.RX2WaterfallLowThreshold10m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold10m;
                    break;
                case Band.B6M:
                    console.RX2WaterfallLowThreshold6m = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThreshold6m;
                    break;
                case Band.WWV:
                    console.RX2WaterfallLowThresholdWWV = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThresholdWWV;
                    break;
                case Band.GEN:
                    console.RX2WaterfallLowThresholdGEN = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThresholdGEN;
                    break;
                default:
                    console.RX2WaterfallLowThresholdXVTR = (float)udRX2DisplayWaterfallLowLevel.Value;
                    Display.RX2WaterfallLowThreshold = console.RX2WaterfallLowThresholdXVTR;
                    break;
            }
        }

        private void udRX2DisplayWaterfallHighLevel_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateWaterfallBandInfo();
            switch (console.RX2Band)
            {
                case Band.B160M:
                    console.RX2WaterfallHighThreshold160m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold160m;
                    break;
                case Band.B80M:
                    console.RX2WaterfallHighThreshold80m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold80m;
                    break;
                case Band.B60M:
                    console.RX2WaterfallHighThreshold60m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold60m;
                    break;
                case Band.B40M:
                    console.RX2WaterfallHighThreshold40m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold40m;
                    break;
                case Band.B30M:
                    console.RX2WaterfallHighThreshold30m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold30m;
                    break;
                case Band.B20M:
                    console.RX2WaterfallHighThreshold20m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold20m;
                    break;
                case Band.B17M:
                    console.RX2WaterfallHighThreshold17m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold17m;
                    break;
                case Band.B15M:
                    console.RX2WaterfallHighThreshold15m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold15m;
                    break;
                case Band.B12M:
                    console.RX2WaterfallHighThreshold12m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold12m;
                    break;
                case Band.B10M:
                    console.RX2WaterfallHighThreshold10m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold10m;
                    break;
                case Band.B6M:
                    console.RX2WaterfallHighThreshold6m = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThreshold6m;
                    break;
                case Band.WWV:
                    console.RX2WaterfallHighThresholdWWV = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThresholdWWV;
                    break;
                case Band.GEN:
                    console.RX2WaterfallHighThresholdGEN = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThresholdGEN;
                    break;
                default:
                    console.RX2WaterfallHighThresholdXVTR = (float)udRX2DisplayWaterfallHighLevel.Value;
                    Display.RX2WaterfallHighThreshold = console.RX2WaterfallHighThresholdXVTR;
                    break;
            }
            // Display.WaterfallHighThreshold = (float)udDisplayWaterfallHighLevel.Value;
        }

        private void udDisplayScopeTime_ValueChanged(object sender, System.EventArgs e)
        {
            //console.ScopeTime = (int)udDisplayScopeTime.Value;
            int samples = (int)((double)udDisplayScopeTime.Value * Audio.OutRate / 1000000.0);
            //Debug.WriteLine("sample: "+samples);
            Audio.ScopeSamplesPerPixel = samples;
        }

        private void udDisplayMeterAvg_ValueChanged(object sender, System.EventArgs e)
        {
            double block_time = (double)udDisplayMeterDelay.Value * 0.001;
            int blocksToAvg = (int)((float)udDisplayMeterAvg.Value * 0.001 / block_time);
            blocksToAvg = Math.Max(2, blocksToAvg);
            console.MultiMeterAvgBlocks = blocksToAvg;
        }

        private void comboDisplayDriver_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboDisplayDriver.Text)
            {
                case "GDI+":
                    console.CurrentDisplayEngine = DisplayEngine.GDI_PLUS;
                    break;
                case "DirectX":
                    console.CurrentDisplayEngine = DisplayEngine.DIRECT_X;
                    break;
            }
        }

        private void udTXGridMax_ValueChanged(object sender, System.EventArgs e)
        {
            if (udTXGridMax.Value <= udTXGridMin.Value)
                udTXGridMax.Value = udTXGridMin.Value + 10;
            Display.TXSpectrumGridMax = (int)udTXGridMax.Value;
        }

        private void udTXGridMin_ValueChanged(object sender, System.EventArgs e)
        {
            if (udTXGridMin.Value >= udTXGridMax.Value)
                udTXGridMin.Value = udTXGridMax.Value - 10;
            Display.TXSpectrumGridMin = (int)udTXGridMin.Value;
        }

        private void udTXGridStep_ValueChanged(object sender, System.EventArgs e)
        {
            Display.TXSpectrumGridStep = (int)udTXGridStep.Value;
        }

        private void comboTXLabelAlign_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboTXLabelAlign.Text)
            {
                case "Left":
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.LEFT;
                    break;
                case "Cntr":
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.CENTER;
                    break;
                case "Right":
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.RIGHT;
                    break;
                case "Auto":
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.AUTO;
                    break;
                case "Off":
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.OFF;
                    break;
                default:
                    Display.TXDisplayLabelAlign = DisplayLabelAlignment.LEFT;
                    break;
            }
        }

        #endregion

        #region DSP Tab Event Handlers
        // ======================================================
        // DSP Tab Event Handlers
        // ======================================================

        private void udLMSNR_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).SetNRVals(
                (int)udLMSNRtaps.Value,
                (int)udLMSNRdelay.Value,
                1e-6 * (double)udLMSNRgain.Value,
                1e-3 * (double)udLMSNRLeak.Value);
            console.radio.GetDSPRX(0, 1).SetNRVals(
                (int)udLMSNRtaps.Value,
                (int)udLMSNRdelay.Value,
                1e-6 * (double)udLMSNRgain.Value,
                1e-3 * (double)udLMSNRLeak.Value);
        }

        private void udLMSNR2_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).SetNRVals(
                (int)udLMSNR2taps.Value,
                (int)udLMSNR2delay.Value,
                1e-6 * (double)udLMSNR2gain.Value,
                1e-3 * (double)udLMSNR2Leak.Value);
            console.radio.GetDSPRX(1, 1).SetNRVals(
                (int)udLMSNR2taps.Value,
                (int)udLMSNR2delay.Value,
                1e-6 * (double)udLMSNR2gain.Value,
                1e-3 * (double)udLMSNR2Leak.Value);
        }

        private void udDSPNB_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).NBThreshold = 0.165 * (double)(udDSPNB.Value);
            console.radio.GetDSPRX(1, 0).NBThreshold = 0.165 * (double)(udDSPNB.Value);
        }

        private void comboDSPPhoneRXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufPhoneRX = int.Parse(comboDSPPhoneRXBuf.Text);
        }

        private void comboDSPPhoneTXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufPhoneTX = int.Parse(comboDSPPhoneTXBuf.Text);
        }

        private void comboDSPFMRXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufFMRX = int.Parse(comboDSPFMRXBuf.Text);
        }

        private void comboDSPFMTXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufFMTX = int.Parse(comboDSPFMTXBuf.Text);
        }

        private void comboDSPCWRXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufCWRX = int.Parse(comboDSPCWRXBuf.Text);
        }

        private void comboDSPDigRXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufDigRX = int.Parse(comboDSPDigRXBuf.Text);
        }

        private void comboDSPDigTXBuf_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPBufDigTX = int.Parse(comboDSPDigTXBuf.Text);
        }

        private void comboDSPPhoneRXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizePhoneRX = int.Parse(comboDSPPhoneRXFiltSize.Text);
        }

        private void comboDSPPhoneTXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizePhoneTX = int.Parse(comboDSPPhoneTXFiltSize.Text);
        }

        private void comboDSPFMRXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizeFMRX = int.Parse(comboDSPFMRXFiltSize.Text);
        }

        private void comboDSPFMTXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizeFMTX = int.Parse(comboDSPFMTXFiltSize.Text);
        }

        private void comboDSPCWRXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizeCWRX = int.Parse(comboDSPCWRXFiltSize.Text);
        }

        private void comboDSPDigRXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizeDigRX = int.Parse(comboDSPDigRXFiltSize.Text);
        }

        private void comboDSPDigTXFiltSize_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltSizeDigTX = int.Parse(comboDSPDigTXFiltSize.Text);
        }

        private void comboDSPPhoneRXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypePhoneRX = (DSPFilterType)comboDSPPhoneRXFiltType.SelectedIndex;
        }

        private void comboDSPPhoneTXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypePhoneTX = (DSPFilterType)comboDSPPhoneTXFiltType.SelectedIndex;
        }

        private void comboDSPFMRXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypeFMRX = (DSPFilterType)comboDSPFMRXFiltType.SelectedIndex;
        }

        private void comboDSPFMTXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypeFMTX = (DSPFilterType)comboDSPFMTXFiltType.SelectedIndex;
        }

        private void comboDSPCWRXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypeCWRX = (DSPFilterType)comboDSPCWRXFiltType.SelectedIndex;
        }

        private void comboDSPDigRXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypeDigRX = (DSPFilterType)comboDSPDigRXFiltType.SelectedIndex;
        }

        private void comboDSPDigTXFiltType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.DSPFiltTypeDigTX = (DSPFilterType)comboDSPDigTXFiltType.SelectedIndex;
        }

        #region Image Reject

        private void udLMSANF_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).SetANFVals(
                (int)udLMSANFtaps.Value,
                (int)udLMSANFdelay.Value,
                1e-6 * (double)udLMSANFgain.Value,
                1e-3 * (double)udLMSANFLeak.Value);
            console.radio.GetDSPRX(0, 1).SetANFVals(
                (int)udLMSANFtaps.Value,
                (int)udLMSANFdelay.Value,
                1e-6 * (double)udLMSANFgain.Value,
                1e-3 * (double)udLMSANFLeak.Value);
        }

        private void udLMSANF2_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).SetANFVals(
                (int)udLMSANF2taps.Value,
                (int)udLMSANF2delay.Value,
                1e-6 * (double)udLMSANF2gain.Value,
                1e-3 * (double)udLMSANF2Leak.Value);
            console.radio.GetDSPRX(1, 1).SetANFVals(
                (int)udLMSANF2taps.Value,
                (int)udLMSANF2delay.Value,
                1e-6 * (double)udLMSANF2gain.Value,
                1e-3 * (double)udLMSANF2Leak.Value);
        }

        private void radANFPreAGC_CheckedChanged(object sender, EventArgs e)
        {
            int position;
            if (radANFPreAGC.Checked)
                position = 0;
            else
                position = 1;
            console.radio.GetDSPRX(0, 0).RXANFPosition = position;
            console.radio.GetDSPRX(0, 1).RXANFPosition = position;
            console.radio.GetDSPRX(0, 0).RXANRPosition = position;
            console.radio.GetDSPRX(0, 1).RXANRPosition = position;
            console.radio.GetDSPRX(0, 0).RXANR2Position = position;
            console.radio.GetDSPRX(0, 1).RXANR2Position = position;
        }

        private void radANF2PreAGC_CheckedChanged(object sender, EventArgs e)
        {
            int position;
            if (radANF2PreAGC.Checked)
                position = 0;
            else
                position = 1;
            console.radio.GetDSPRX(1, 0).RXANFPosition = position;
            console.radio.GetDSPRX(1, 0).RXANRPosition = position;
            console.radio.GetDSPRX(1, 0).RXANR2Position = position;
        }

        #endregion

        #region Keyer

        private void udDSPCWPitch_ValueChanged(object sender, System.EventArgs e)
        {
            console.CWPitch = (int)udDSPCWPitch.Value;
        }

        private void chkCWKeyerIambic_CheckedChanged(object sender, System.EventArgs e)
        {
            console.CWIambic = chkCWKeyerIambic.Checked;
            // if (console.NewProtocol)
            //{
            if (chkCWKeyerIambic.Checked)
                NetworkIO.SetCWIambic(1);
            else NetworkIO.SetCWIambic(0);
            //}
            //else
            //{
            //    if (chkCWKeyerIambic.Checked)
            //    {
            //        if (chkCWKeyerMode.Checked)
            //            JanusAudio.SetCWKeyerMode(2); // mode b
            //        else JanusAudio.SetCWKeyerMode(1); // mode a
            //    }
            //    else
            //        JanusAudio.SetCWKeyerMode(0); // straight/bug mode
            //}
        }

        private void udCWKeyerWeight_ValueChanged(object sender, System.EventArgs e)
        {
            NetworkIO.SetCWKeyerWeight((int)udCWKeyerWeight.Value);
        }

        private void udCWKeyerSemiBreakInDelay_ValueChanged(object sender, System.EventArgs e)
        {
            console.BreakInDelay = (double)udCWBreakInDelay.Value;
        }

        private void chkDSPKeyerSemiBreakInEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            console.CWSemiBreakInEnabled = chkCWBreakInEnabled.Checked;
            console.BreakInEnabled = chkCWBreakInEnabled.Checked;
            if (chkCWBreakInEnabled.Checked)
                NetworkIO.SetCWBreakIn(1);
            else NetworkIO.SetCWBreakIn(0);

            udCWBreakInDelay.Enabled = chkCWBreakInEnabled.Checked;
            udCWKeyerSemiBreakInDelay_ValueChanged(this, EventArgs.Empty);
        }

        private void chkDSPKeyerSidetone_CheckedChanged(object sender, System.EventArgs e)
        {
            console.CWSidetone = chkDSPKeyerSidetone.Checked;
            if (chkDSPKeyerSidetone.Checked)
                NetworkIO.SetCWSidetone(1);
            else NetworkIO.SetCWSidetone(0);

        }

        private void chkCWKeyerRevPdl_CheckedChanged(object sender, System.EventArgs e)
        {
            console.ReversePaddles = chkCWKeyerRevPdl.Checked;
        }

        private void comboKeyerConnPrimary_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!CWInput.SetPrimaryInput(comboKeyerConnPrimary.Text))
            {
                MessageBox.Show("Error using " + comboKeyerConnPrimary.Text + " for Keyer Primary Input.\n" +
                    "The port may already be in use by another application.",
                    "Error using " + comboKeyerConnPrimary.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                comboKeyerConnPrimary.Text = CWInput.PrimaryInput;
            }
        }

        private void comboKeyerConnSecondary_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            if (comboKeyerConnSecondary.Text == "CAT")
            {
                if (!chkCATEnable.Checked)
                {
                    MessageBox.Show("CAT is not Enabled.  Please enable the CAT interface before selecting this option.",
                        "CAT not enabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Hand);
                    comboKeyerConnSecondary.Text = CWInput.SecondaryInput;
                    return;
                }
                else
                {
                    CWInput.SetSecondaryInput("None");
                    console.Siolisten.UseForKeyPTT = true;
                }
            }
            else
            {
                if (console.Siolisten != null)
                {
                    console.Siolisten.UseForKeyPTT = false;
                }
            }

            if (!CWInput.SetSecondaryInput(comboKeyerConnSecondary.Text))
            {
                MessageBox.Show("Error using " + comboKeyerConnSecondary.Text + " for Keyer Secondary Input.\n" +
                    "The port may already be in use by another application.",
                    "Error using " + comboKeyerConnSecondary.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                comboKeyerConnSecondary.Text = CWInput.SecondaryInput;
                return;
            }

            switch (comboKeyerConnSecondary.Text)
            {
                case "None":
                    lblKeyerConnPTTLine.Visible = false;
                    comboKeyerConnPTTLine.Visible = false;
                    lblKeyerConnKeyLine.Visible = false;
                    comboKeyerConnKeyLine.Visible = false;
                    break;
                case "CAT":
                    lblKeyerConnPTTLine.Visible = true;
                    comboKeyerConnPTTLine.Visible = true;
                    lblKeyerConnKeyLine.Visible = true;
                    comboKeyerConnKeyLine.Visible = true;

                    comboKeyerConnPTTLine_SelectedIndexChanged(this, EventArgs.Empty);
                    comboKeyerConnKeyLine_SelectedIndexChanged(this, EventArgs.Empty);
                    break;
                default: // COMx
                    lblKeyerConnPTTLine.Visible = true;
                    comboKeyerConnPTTLine.Visible = true;
                    lblKeyerConnKeyLine.Visible = true;
                    comboKeyerConnKeyLine.Visible = true;

                    comboKeyerConnPTTLine_SelectedIndexChanged(this, EventArgs.Empty);
                    comboKeyerConnKeyLine_SelectedIndexChanged(this, EventArgs.Empty);
                    break;
            }
        }

        private void comboKeyerConnKeyLine_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboKeyerConnKeyLine.SelectedIndex < 0) return;

            if (comboKeyerConnSecondary.Text == "CAT")
            {
                switch ((KeyerLine)comboKeyerConnKeyLine.SelectedIndex)
                {
                    case KeyerLine.None:
                        console.Siolisten.KeyOnDTR = false;
                        console.Siolisten.KeyOnRTS = false;
                        break;
                    case KeyerLine.DTR:
                        console.Siolisten.KeyOnDTR = true;
                        console.Siolisten.KeyOnRTS = false;
                        break;
                    case KeyerLine.RTS:
                        console.Siolisten.KeyOnDTR = false;
                        console.Siolisten.KeyOnRTS = true;
                        break;
                }
            }
            else CWInput.SecondaryKeyLine = (KeyerLine)comboKeyerConnKeyLine.SelectedIndex;
        }

        private void comboKeyerConnPTTLine_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboKeyerConnPTTLine.SelectedIndex < 0) return;

            if (comboKeyerConnSecondary.Text == "CAT")
            {
                switch ((KeyerLine)comboKeyerConnPTTLine.SelectedIndex)
                {
                    case KeyerLine.None:
                        console.Siolisten.PTTOnDTR = false;
                        console.Siolisten.PTTOnRTS = false;
                        break;
                    case KeyerLine.DTR:
                        console.Siolisten.PTTOnDTR = true;
                        console.Siolisten.PTTOnRTS = false;
                        break;
                    case KeyerLine.RTS:
                        console.Siolisten.PTTOnDTR = false;
                        console.Siolisten.PTTOnRTS = true;
                        break;
                }
            }
            else CWInput.SecondaryPTTLine = (KeyerLine)comboKeyerConnPTTLine.SelectedIndex;
        }

        #endregion

        #region AGC

        private void udDSPAGCFixedGaindB_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXFixedAGC = (double)udDSPAGCFixedGaindB.Value;
            console.radio.GetDSPRX(0, 1).RXFixedAGC = (double)udDSPAGCFixedGaindB.Value;

            if (console.RX1AGCMode == AGCMode.FIXD)
                console.RF = (int)udDSPAGCFixedGaindB.Value;
        }

        private void udDSPAGCMaxGaindB_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAGCMaxGain = (double)udDSPAGCMaxGaindB.Value;
            console.radio.GetDSPRX(0, 1).RXAGCMaxGain = (double)udDSPAGCMaxGaindB.Value;

            if (console.RX1AGCMode != AGCMode.FIXD)
                console.RF = (int)udDSPAGCMaxGaindB.Value;
        }

        private void udDSPAGCRX2MaxGaindB_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAGCMaxGain = (double)udDSPAGCRX2MaxGaindB.Value;
            // console.radio.GetDSPRX(1, 1).RXAGCMaxGain = (double)udDSPAGCRX2MaxGaindB.Value;

            if (console.RX2AGCMode != AGCMode.FIXD)
                console.RX2RF = (int)udDSPAGCRX2MaxGaindB.Value;
        }

        private void udDSPAGCRX2FixedGaindB_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXFixedAGC = (double)udDSPAGCRX2FixedGaindB.Value;
            //  console.radio.GetDSPRX(1, 1).RXFixedAGC = (double)udDSPAGCRX2FixedGaindB.Value;

            if (console.RX2AGCMode == AGCMode.FIXD)
                console.RX2RF = (int)udDSPAGCRX2FixedGaindB.Value;
        }

        private void udDSPAGCDecay_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udDSPAGCDecay.Enabled)
            {
                console.radio.GetDSPRX(0, 0).RXAGCDecay = (int)udDSPAGCDecay.Value;
                console.radio.GetDSPRX(0, 1).RXAGCDecay = (int)udDSPAGCDecay.Value;
            }
        }

        private void udDSPAGCRX2Decay_ValueChanged(object sender, System.EventArgs e)
        {
            //  if (udDSPAGCRX2Decay.Enabled)
            {
                console.radio.GetDSPRX(1, 0).RXAGCDecay = (int)udDSPAGCRX2Decay.Value;
                //    console.radio.GetDSPRX(1, 1).RXAGCDecay = (int)udDSPAGCRX2Decay.Value;
            }
        }

        private void udDSPAGCSlope_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAGCSlope = 10 * (int)(udDSPAGCSlope.Value);
            console.radio.GetDSPRX(0, 1).RXAGCSlope = 10 * (int)(udDSPAGCSlope.Value);
        }

        private void udDSPAGCRX2Slope_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAGCSlope = 10 * (int)(udDSPAGCRX2Slope.Value);
            //    console.radio.GetDSPRX(1, 1).RXAGCSlope = 10 * (int)(udDSPAGCRX2Slope.Value);
        }

        private void udDSPAGCHangTime_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udDSPAGCHangTime.Enabled)
            {
                console.radio.GetDSPRX(0, 0).RXAGCHang = (int)udDSPAGCHangTime.Value;
                console.radio.GetDSPRX(0, 1).RXAGCHang = (int)udDSPAGCHangTime.Value;
            }
        }

        private void udDSPAGCRX2HangTime_ValueChanged(object sender, System.EventArgs e)
        {
            // if (udDSPAGCRX2HangTime.Enabled)
            {
                console.radio.GetDSPRX(1, 0).RXAGCHang = (int)udDSPAGCRX2HangTime.Value;
                //    console.radio.GetDSPRX(1, 1).RXAGCHang = (int)udDSPAGCRX2HangTime.Value;
            }
        }

        private void tbDSPAGCHangThreshold_Scroll(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAGCHangThreshold = (int)tbDSPAGCHangThreshold.Value;
            console.radio.GetDSPRX(0, 1).RXAGCHangThreshold = (int)tbDSPAGCHangThreshold.Value;
        }

        private void tbDSPAGCRX2HangThreshold_Scroll(object sender, System.EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAGCHangThreshold = (int)tbDSPAGCRX2HangThreshold.Value;
            //   console.radio.GetDSPRX(1, 1).RXAGCHangThreshold = (int)tbDSPAGCRX2HangThreshold.Value;
        }

        #endregion

        #region Leveler

        private void udDSPLevelerThreshold_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXLevelerMaxGain = (double)udDSPLevelerThreshold.Value;
        }

        private void udDSPLevelerDecay_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXLevelerDecay = (int)udDSPLevelerDecay.Value;
        }

        private void chkDSPLevelerEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXLevelerOn = chkDSPLevelerEnabled.Checked;
        }

        #endregion

        #region ALC

        private void udDSPALCMaximumGain_ValueChanged(object sender, System.EventArgs e)
        {
            WDSP.SetTXAALCMaxGain(WDSP.id(1, 0), (double)udDSPALCMaximumGain.Value);
            WDSP.ALCGain = (double)udDSPALCMaximumGain.Value;
        }

        private void udDSPALCDecay_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXALCDecay = (int)udDSPALCDecay.Value;
        }

        #endregion

        #endregion

        #region Transmit Tab Event Handlers

        private void udTXFilterHigh_ValueChanged(object sender, System.EventArgs e)
        {
            if (udTXFilterHigh.Value < udTXFilterLow.Value + 100)
            {
                udTXFilterHigh.Value = udTXFilterLow.Value + 100;
                return;
            }

            if (udTXFilterHigh.Focused &&
                (udTXFilterHigh.Value - udTXFilterLow.Value) > 3000 &&
                (console.TXFilterHigh - console.TXFilterLow) <= 3000)
            {
                (new Thread(new ThreadStart(TXBW))).Start();
            }

            console.TXFilterHigh = (int)udTXFilterHigh.Value;

        }

        private void TXBW()
        {
            MessageBox.Show("The transmit bandwidth is being increased beyond 3kHz.\n\n" +
                "As the control operator, you are responsible for compliance with current " +
                "rules and good operating practice.",
                "Warning: Transmit Bandwidth",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void udTXFilterLow_ValueChanged(object sender, System.EventArgs e)
        {
            if (udTXFilterLow.Value > udTXFilterHigh.Value - 100)
            {
                udTXFilterLow.Value = udTXFilterHigh.Value - 100;
                return;
            }

            if (udTXFilterLow.Focused &&
                (udTXFilterHigh.Value - udTXFilterLow.Value) > 3000 &&
                (console.TXFilterHigh - console.TXFilterLow) <= 3000)
            {
                (new Thread(new ThreadStart(TXBW))).Start();
            }

            console.TXFilterLow = (int)udTXFilterLow.Value;
        }

        private void udTransmitTunePower_ValueChanged(object sender, System.EventArgs e)
        {
            console.TunePower = (int)udTXTunePower.Value;
        }

        private void chkTXTunePower_CheckedChanged(object sender, System.EventArgs e)
        {
            console.TXTunePower = chkTXTunePower.Checked;
        }

        private string current_profile = "";
        private void comboTXProfileName_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboTXProfileName.SelectedIndex < 0 || initializing)
                return;

            if (chkAutoSaveTXProfile.Checked)
            {
                SaveTXProfileData();
            }
            else
            {
                if (CheckTXProfileChanged() && comboTXProfileName.Focused)
                {
                    DialogResult result = MessageBox.Show("The current profile has changed.  " +
                        "Would you like to save the current profile?",
                        "Save Current Profile?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        btnTXProfileSave_Click(this, EventArgs.Empty);
                        //return;
                    }
                    else if (result == DialogResult.Cancel)
                        return;
                }
            }

            console.TXProfile = comboTXProfileName.Text;
            DataRow[] rows = DB.ds.Tables["TxProfile"].Select(
                "'" + comboTXProfileName.Text + "' = Name");

            if (rows.Length != 1)
            {
                MessageBox.Show("Database error reading TxProfile Table.",
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DataRow dr = rows[0];
            int[] eq = null;
            eq = new int[21];
            int[] cfceq = null;
            cfceq = new int[32];

            console.EQForm.TXEQEnabled = (bool)dr["TXEQEnabled"];
            console.EQForm.NumBands = (int)dr["TXEQNumBands"];

            eq[0] = (int)dr["TXEQPreamp"];
            for (int i = 1; i < 11; i++)
                eq[i] = (int)dr["TXEQ" + i.ToString()];
            for (int i = 11; i < 21; i++)
                eq[i] = (int)dr["TxEqFreq" + (i - 10).ToString()];
            console.EQForm.TXEQ = eq;

            udTXFilterLow.Value = Math.Min(Math.Max((int)dr["FilterLow"], udTXFilterLow.Minimum), udTXFilterLow.Maximum);
            udTXFilterHigh.Value = Math.Min(Math.Max((int)dr["FilterHigh"], udTXFilterHigh.Minimum), udTXFilterHigh.Maximum);

            console.DX = (bool)dr["DXOn"];
            console.DXLevel = (int)dr["DXLevel"];

            console.CPDR = (bool)dr["CompanderOn"];
            console.CPDRLevel = (int)dr["CompanderLevel"];

            console.Mic = (int)dr["MicGain"];
            console.FMMic = (int)dr["FMMicGain"];

            chkDSPLevelerEnabled.Checked = (bool)dr["Lev_On"];
            udDSPLevelerSlope.Value = (int)dr["Lev_Slope"];
            udDSPLevelerThreshold.Value = (int)dr["Lev_MaxGain"];
            udDSPLevelerAttack.Value = (int)dr["Lev_Attack"];
            udDSPLevelerDecay.Value = (int)dr["Lev_Decay"];
            udDSPLevelerHangTime.Value = (int)dr["Lev_Hang"];
            tbDSPLevelerHangThreshold.Value = (int)dr["Lev_HangThreshold"];

            udDSPALCSlope.Value = (int)dr["ALC_Slope"];
            udDSPALCMaximumGain.Value = (int)dr["ALC_MaximumGain"];
            udDSPALCAttack.Value = (int)dr["ALC_Attack"];
            udDSPALCDecay.Value = (int)dr["ALC_Decay"];
            udDSPALCHangTime.Value = (int)dr["ALC_Hang"];
            tbDSPALCHangThreshold.Value = (int)dr["ALC_HangThreshold"];

            console.PWR = (int)dr["Power"];
            // W4TME_
            chkTXNoiseGateEnabled.Checked = (bool)dr["Dexp_On"];
            udTXNoiseGate.Value = (int)dr["Dexp_Threshold"];
            udTXNoiseGateAttenuate.Value = (int)dr["Dexp_Attenuate"];

            chkTXVOXEnabled.Checked = (bool)dr["VOX_On"];
            udTXVOXThreshold.Value = (int)dr["VOX_Threshold"];
            udTXVOXHangTime.Value = (int)dr["VOX_HangTime"];

            udTXTunePower.Value = (int)dr["Tune_Power"];
            comboTXTUNMeter.Text = (string)dr["Tune_Meter_Type"];

            // chkTXLimitSlew.Checked = (bool)dr["TX_Limit_Slew"];

            console.TXAF = (int)dr["TX_AF_Level"];

            udTXAMCarrierLevel.Value = (int)dr["AM_Carrier_Level"];

            console.ShowTXFilter = (bool)dr["Show_TX_Filter"];

            chkAudioEnableVAC.Checked = (bool)dr["VAC1_On"];
            chkAudioVACAutoEnable.Checked = (bool)dr["VAC1_Auto_On"];
            udAudioVACGainRX.Value = (int)dr["VAC1_RX_GAIN"];
            udAudioVACGainTX.Value = (int)dr["VAC1_TX_GAIN"];
            chkAudio2Stereo.Checked = (bool)dr["VAC1_Stereo_On"];
            comboAudioSampleRate2.Text = (string)dr["VAC1_Sample_Rate"];
            comboAudioBuffer2.Text = (string)dr["VAC1_Buffer_Size"];
            chkAudioIQtoVAC.Checked = (bool)dr["VAC1_IQ_Output"];
            chkAudioCorrectIQ.Checked = (bool)dr["VAC1_IQ_Correct"];
            chkVACAllowBypass.Checked = (bool)dr["VAC1_PTT_OverRide"];
            chkVACCombine.Checked = (bool)dr["VAC1_Combine_Input_Channels"];
            chkAudioLatencyManual2.Checked = (bool)dr["VAC1_Latency_On"];
            udAudioLatency2.Value = (int)dr["VAC1_Latency_Duration"];

            chkVAC2Enable.Checked = (bool)dr["VAC2_On"];
            chkVAC2AutoEnable.Checked = (bool)dr["VAC2_Auto_On"];
            udVAC2GainRX.Value = (int)dr["VAC2_RX_GAIN"];
            udVAC2GainTX.Value = (int)dr["VAC2_TX_GAIN"];
            chkAudioStereo3.Checked = (bool)dr["VAC2_Stereo_On"];
            comboAudioSampleRate3.Text = (string)dr["VAC2_Sample_Rate"];
            comboAudioBuffer3.Text = (string)dr["VAC2_Buffer_Size"];
            chkVAC2DirectIQ.Checked = (bool)dr["VAC2_IQ_Output"];
            chkVAC2DirectIQCal.Checked = (bool)dr["VAC2_IQ_Correct"];
            chkVAC2Combine.Checked = (bool)dr["VAC2_Combine_Input_Channels"];
            chkVAC2LatencyManual.Checked = (bool)dr["VAC2_Latency_On"];
            udVAC2Latency.Value = (int)dr["VAC2_Latency_Duration"];

            comboDSPPhoneRXBuf.Text = (string)dr["Phone_RX_DSP_Buffer"];
            comboDSPPhoneTXBuf.Text = (string)dr["Phone_TX_DSP_Buffer"];
            comboDSPFMRXBuf.Text = (string)dr["FM_RX_DSP_Buffer"];
            comboDSPFMTXBuf.Text = (string)dr["FM_TX_DSP_Buffer"];
            comboDSPDigRXBuf.Text = (string)dr["Digi_RX_DSP_Buffer"];
            comboDSPDigTXBuf.Text = (string)dr["Digi_TX_DSP_Buffer"];
            comboDSPCWRXBuf.Text = (string)dr["CW_RX_DSP_Buffer"];

            comboDSPPhoneRXFiltSize.Text = (string)dr["Phone_RX_DSP_Filter_Size"];
            comboDSPPhoneTXFiltSize.Text = (string)dr["Phone_TX_DSP_Filter_Size"];
            comboDSPFMRXFiltSize.Text = (string)dr["FM_RX_DSP_Filter_Size"];
            comboDSPFMTXFiltSize.Text = (string)dr["FM_TX_DSP_Filter_Size"];
            comboDSPDigRXFiltSize.Text = (string)dr["Digi_RX_DSP_Filter_Size"];
            comboDSPDigTXFiltSize.Text = (string)dr["Digi_TX_DSP_Filter_Size"];
            comboDSPCWRXFiltSize.Text = (string)dr["CW_RX_DSP_Filter_Size"];

            comboDSPPhoneRXFiltType.Text = (string)dr["Phone_RX_DSP_Filter_Type"];
            comboDSPPhoneTXFiltType.Text = (string)dr["Phone_TX_DSP_Filter_Type"];
            comboDSPFMRXFiltType.Text = (string)dr["FM_RX_DSP_Filter_Type"];
            comboDSPFMTXFiltType.Text = (string)dr["FM_TX_DSP_Filter_Type"];
            comboDSPDigRXFiltType.Text = (string)dr["Digi_RX_DSP_Filter_Type"];
            comboDSPDigTXFiltType.Text = (string)dr["Digi_TX_DSP_Filter_Type"];
            comboDSPCWRXFiltType.Text = (string)dr["CW_RX_DSP_Filter_Type"];

            radMicIn.Checked = (bool)dr["Mic_Input_On"];
            chk20dbMicBoost.Checked = (bool)dr["Mic_Input_Boost"];
            radLineIn.Checked = (bool)dr["Line_Input_On"];
            udLineInBoost.Value = (decimal)dr["Line_Input_Level"];
            chkDSPCESSB.Checked = (bool)dr["CESSB_On"];
            console.PureSignalEnabled = (bool)dr["Pure_Signal_Enabled"];

            //CFC
            chkCFCEnable.Checked = (bool)dr["CFCEnabled"];
            chkCFCPeqEnable.Checked = (bool)dr["CFCPostEqEnabled"];
            chkPHROTEnable.Checked = (bool)dr["CFCPhaseRotatorEnabled"];

            udPhRotFreq.Value = Math.Min(Math.Max((int)dr["CFCPhaseRotatorFreq"], udPhRotFreq.Minimum), udPhRotFreq.Maximum);
            udPHROTStages.Value = Math.Min(Math.Max((int)dr["CFCPhaseRotatorStages"], udPHROTStages.Minimum), udPHROTStages.Maximum);

            cfceq[0] = (int)dr["CFCPreComp"];
            for (int i = 1; i < 11; i++)
                cfceq[i] = (int)dr["CFCPreComp" + (i - 1).ToString()];

            cfceq[11] = (int)dr["CFCPostEqGain"];
            for (int i = 12; i < 22; i++)
                cfceq[i] = (int)dr["CFCPostEqGain" + (i - 12).ToString()];
            for (int i = 22; i < 32; i++)
                cfceq[i] = (int)dr["CFCEqFreq" + (i - 22).ToString()];

            CFCCOMPEQ = cfceq;

            current_profile = comboTXProfileName.Text;
        }

        private void btnTXProfileSave_Click(object sender, System.EventArgs e)
        {
            string name = InputBox.Show("Save Profile", "Please enter a profile name:",
                current_profile);

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("TX Profile Save cancelled",
                    "TX Profile",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DataRow dr = null;
            if (comboTXProfileName.Items.Contains(name))
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to overwrite the " + name + " TX Profile?",
                    "Overwrite Profile?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                foreach (DataRow d in from DataRow d in DB.ds.Tables["TxProfile"].Rows where (string)d["Name"] == name select d)
                {
                    dr = d;
                    break;
                }
            }
            else
            {
                dr = DB.ds.Tables["TxProfile"].NewRow();
                dr["Name"] = name;
            }

            dr["FilterLow"] = (int)udTXFilterLow.Value;
            dr["FilterHigh"] = (int)udTXFilterHigh.Value;
            dr["TXEQEnabled"] = console.EQForm.TXEQEnabled;
            dr["TXEQNumBands"] = console.EQForm.NumBands;
            int[] eq = console.EQForm.TXEQ;
            dr["TXEQPreamp"] = eq[0];
            for (int i = 1; i < 11; i++)
                dr["TXEQ" + i.ToString()] = eq[i];
            for (int i = 11; i < 21; i++)
                dr["TxEqFreq" + (i - 10).ToString()] = eq[i];

            dr["DXOn"] = console.DX;
            dr["DXLevel"] = console.DXLevel;
            dr["CompanderOn"] = console.CPDR;
            dr["CompanderLevel"] = console.CPDRLevel;
            dr["MicGain"] = console.Mic;
            dr["FMMicGain"] = console.FMMic;

            dr["Lev_On"] = chkDSPLevelerEnabled.Checked;
            dr["Lev_Slope"] = (int)udDSPLevelerSlope.Value;
            dr["Lev_MaxGain"] = (int)udDSPLevelerThreshold.Value;
            dr["Lev_Attack"] = (int)udDSPLevelerAttack.Value;
            dr["Lev_Decay"] = (int)udDSPLevelerDecay.Value;
            dr["Lev_Hang"] = (int)udDSPLevelerHangTime.Value;
            dr["Lev_HangThreshold"] = tbDSPLevelerHangThreshold.Value;

            dr["ALC_Slope"] = (int)udDSPALCSlope.Value;
            dr["ALC_MaximumGain"] = (int)udDSPALCMaximumGain.Value;
            dr["ALC_Attack"] = (int)udDSPALCAttack.Value;
            dr["ALC_Decay"] = (int)udDSPALCDecay.Value;
            dr["ALC_Hang"] = (int)udDSPALCHangTime.Value;
            dr["ALC_HangThreshold"] = tbDSPALCHangThreshold.Value;

            dr["Power"] = console.PWR;

            // W4TME
            dr["Dexp_On"] = chkTXNoiseGateEnabled.Checked;
            dr["Dexp_Threshold"] = (int)udTXNoiseGate.Value;
            dr["Dexp_Attenuate"] = (int)udTXNoiseGateAttenuate.Value;

            dr["VOX_On"] = chkTXVOXEnabled.Checked;
            dr["VOX_Threshold"] = (int)udTXVOXThreshold.Value;
            dr["VOX_HangTime"] = (int)udTXVOXHangTime.Value;

            dr["Tune_Power"] = (int)udTXTunePower.Value;
            dr["Tune_Meter_Type"] = (string)comboTXTUNMeter.Text;

            // dr["TX_Limit_Slew"] = (bool)chkTXLimitSlew.Checked;

            dr["TX_AF_Level"] = console.TXAF;

            dr["AM_Carrier_Level"] = (int)udTXAMCarrierLevel.Value;

            dr["Show_TX_Filter"] = (bool)console.ShowTXFilter;

            dr["VAC1_On"] = (bool)chkAudioEnableVAC.Checked;
            dr["VAC1_Auto_On"] = (bool)chkAudioVACAutoEnable.Checked;
            dr["VAC1_RX_GAIN"] = (int)udAudioVACGainRX.Value;
            dr["VAC1_TX_GAIN"] = (int)udAudioVACGainTX.Value;
            dr["VAC1_Stereo_On"] = (bool)chkAudio2Stereo.Checked;
            dr["VAC1_Sample_Rate"] = (string)comboAudioSampleRate2.Text;
            dr["VAC1_Buffer_Size"] = (string)comboAudioBuffer2.Text;
            dr["VAC1_IQ_Output"] = (bool)chkAudioIQtoVAC.Checked;
            dr["VAC1_IQ_Correct"] = (bool)chkAudioCorrectIQ.Checked;
            dr["VAC1_PTT_OverRide"] = (bool)chkVACAllowBypass.Checked;
            dr["VAC1_Combine_Input_Channels"] = (bool)chkVACCombine.Checked;
            dr["VAC1_Latency_On"] = (bool)chkAudioLatencyManual2.Checked;
            dr["VAC1_Latency_Duration"] = (int)udAudioLatency2.Value;

            dr["VAC2_On"] = (bool)chkVAC2Enable.Checked;
            dr["VAC2_Auto_On"] = (bool)chkVAC2AutoEnable.Checked;
            dr["VAC2_RX_GAIN"] = (int)udVAC2GainRX.Value;
            dr["VAC2_TX_GAIN"] = (int)udVAC2GainTX.Value;
            dr["VAC2_Stereo_On"] = (bool)chkAudioStereo3.Checked;
            dr["VAC2_Sample_Rate"] = (string)comboAudioSampleRate3.Text;
            dr["VAC2_Buffer_Size"] = (string)comboAudioBuffer3.Text;
            dr["VAC2_IQ_Output"] = (bool)chkVAC2DirectIQ.Checked;
            dr["VAC2_IQ_Correct"] = (bool)chkVAC2DirectIQCal.Checked;
            dr["VAC2_Combine_Input_Channels"] = (bool)chkVAC2Combine.Checked;
            dr["VAC2_Latency_On"] = (bool)chkVAC2LatencyManual.Checked;
            dr["VAC2_Latency_Duration"] = (int)udVAC2Latency.Value;

            dr["Phone_RX_DSP_Buffer"] = (string)comboDSPPhoneRXBuf.Text;
            dr["Phone_TX_DSP_Buffer"] = (string)comboDSPPhoneTXBuf.Text;
            dr["FM_RX_DSP_Buffer"] = (string)comboDSPFMRXBuf.Text;
            dr["FM_TX_DSP_Buffer"] = (string)comboDSPFMTXBuf.Text;
            dr["Digi_RX_DSP_Buffer"] = (string)comboDSPDigRXBuf.Text;
            dr["Digi_TX_DSP_Buffer"] = (string)comboDSPDigTXBuf.Text;
            dr["CW_RX_DSP_Buffer"] = (string)comboDSPCWRXBuf.Text;

            dr["Phone_RX_DSP_Filter_Size"] = (string)comboDSPPhoneRXFiltSize.Text;
            dr["Phone_TX_DSP_Filter_Size"] = (string)comboDSPPhoneTXFiltSize.Text;
            dr["FM_RX_DSP_Filter_Size"] = (string)comboDSPFMRXFiltSize.Text;
            dr["FM_TX_DSP_Filter_Size"] = (string)comboDSPFMTXFiltSize.Text;
            dr["Digi_RX_DSP_Filter_Size"] = (string)comboDSPDigRXFiltSize.Text;
            dr["Digi_TX_DSP_Filter_Size"] = (string)comboDSPDigTXFiltSize.Text;
            dr["CW_RX_DSP_Filter_Size"] = (string)comboDSPCWRXFiltSize.Text;

            dr["Phone_RX_DSP_Filter_Type"] = (string)comboDSPPhoneRXFiltType.Text;
            dr["Phone_TX_DSP_Filter_Type"] = (string)comboDSPPhoneTXFiltType.Text;
            dr["FM_RX_DSP_Filter_Type"] = (string)comboDSPFMRXFiltType.Text;
            dr["FM_TX_DSP_Filter_Type"] = (string)comboDSPFMTXFiltType.Text;
            dr["Digi_RX_DSP_Filter_Type"] = (string)comboDSPDigRXFiltType.Text;
            dr["Digi_TX_DSP_Filter_Type"] = (string)comboDSPDigTXFiltType.Text;
            dr["CW_RX_DSP_Filter_Type"] = (string)comboDSPCWRXFiltType.Text;

            dr["Mic_Input_On"] = (bool)radMicIn.Checked;
            dr["Mic_Input_Boost"] = (bool)chk20dbMicBoost.Checked;
            dr["Line_Input_On"] = (bool)radLineIn.Checked;
            dr["Line_Input_Level"] = udLineInBoost.Value;
            dr["CESSB_On"] = chkDSPCESSB.Checked;
            dr["Pure_Signal_Enabled"] = console.PureSignalEnabled;

            //CFC
            dr["CFCEnabled"] = chkCFCEnable.Checked;
            dr["CFCPostEqEnabled"] = chkCFCPeqEnable.Checked;
            dr["CFCPhaseRotatorEnabled"] = chkPHROTEnable.Checked;
            dr["CFCPhaseRotatorFreq"] = (int)udPhRotFreq.Value;
            dr["CFCPhaseRotatorStages"] = (int)udPHROTStages.Value;
            int[] cfceq = CFCCOMPEQ;
            dr["CFCPreComp"] = cfceq[0];
            for (int i = 1; i < 11; i++)
                dr["CFCPreComp" + (i - 1).ToString()] = cfceq[i];
            dr["CFCPostEqGain"] = cfceq[11];
            for (int i = 12; i < 22; i++)
                dr["CFCPostEqGain" + (i - 12).ToString()] = cfceq[i];
            for (int i = 22; i < 32; i++)
                dr["CFCEqFreq" + (i - 22).ToString()] = cfceq[i];

            if (!comboTXProfileName.Items.Contains(name))
            {
                DB.ds.Tables["TxProfile"].Rows.Add(dr);
                comboTXProfileName.Items.Add(name);
                comboTXProfileName.Text = name;
            }

            console.UpdateTXProfile(name);
        }

        private bool profile_deleted = false;
        private void btnTXProfileDelete_Click(object sender, System.EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "Are you sure you want to delete the " + comboTXProfileName.Text + " TX Profile?",
                "Delete Profile?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.No)
                return;

            profile_deleted = true;

            DataRow[] rows = DB.ds.Tables["TxProfile"].Select(
                "'" + comboTXProfileName.Text + "' = Name");

            if (rows.Length == 1)
                rows[0].Delete();

            int index = comboTXProfileName.SelectedIndex;
            comboTXProfileName.Items.Remove(comboTXProfileName.Text);
            if (comboTXProfileName.Items.Count > 0)
            {
                if (index > comboTXProfileName.Items.Count - 1)
                    index = comboTXProfileName.Items.Count - 1;
                comboTXProfileName.SelectedIndex = index;
            }

            console.UpdateTXProfile(comboTXProfileName.Text);
        }

        private void chkTXVOXEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            Audio.VOXEnabled = chkTXVOXEnabled.Checked;
            console.VOXEnable = chkTXVOXEnabled.Checked;
        }

        private void udTXVOXThreshold_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VOXThreshold = (float)udTXVOXThreshold.Value / 10000.0f;
            console.VOXSens = (int)udTXVOXThreshold.Value;
        }

        private void udVOXGain_ValueChanged(object sender, System.EventArgs e)
        {
            Audio.VOXGain = (float)udVOXGain.Value;// / 10000.0f;
        }

        private void udTXVOXHangTime_ValueChanged(object sender, System.EventArgs e)
        {
            console.VOXHangTime = (int)udTXVOXHangTime.Value;
        }

        private void udTXNoiseGate_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXSquelchThreshold = (float)udTXNoiseGate.Value;
            console.NoiseGate = (int)udTXNoiseGate.Value;
        }

        private void chkTXNoiseGateEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXSquelchOn = chkTXNoiseGateEnabled.Checked;
            console.NoiseGateEnabled = chkTXNoiseGateEnabled.Checked;
        }

        private void udTXAF_ValueChanged(object sender, System.EventArgs e)
        {
            console.TXAF = (int)udTXAF.Value;
        }

        private void udTXAMCarrierLevel_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXAMCarrierLevel = Math.Sqrt(0.01 * (double)udTXAMCarrierLevel.Value) * 0.5;
        }

        private void chkSaveTXProfileOnExit_CheckedChanged(object sender, EventArgs e)
        {
            console.SaveTXProfileOnExit = chkSaveTXProfileOnExit.Checked;
        }

        private void udMicGainMin_ValueChanged(object sender, System.EventArgs e)
        {
            console.MicGainMin = (int)udMicGainMin.Value;
        }

        private void udMicGainMax_ValueChanged(object sender, System.EventArgs e)
        {
            console.MicGainMax = (int)udMicGainMax.Value;
        }

        private void udLineInBoost_ValueChanged(object sender, System.EventArgs e)
        {
            console.LineInBoost = (double)udLineInBoost.Value;
        }

        private void chkShowTopControls_CheckedChanged(object sender, EventArgs e)
        {
            console.ShowTopControls = chkShowTopControls.Checked;
            console.topControlsToolStripMenuItem.Checked = chkShowTopControls.Checked;
            console.bandToolStripMenuItem.Visible = !chkShowBandControls.Checked;

            if (console.CollapsedDisplay)
                console.CollapseDisplay();
        }

        private void chkShowBandControls_CheckedChanged(object sender, EventArgs e)
        {
            console.ShowBandControls = chkShowBandControls.Checked;
            console.bandControlsToolStripMenuItem.Checked = chkShowBandControls.Checked;
            console.modeToolStripMenuItem.Visible = !chkShowModeControls.Checked;

            if (console.CollapsedDisplay)
                console.CollapseDisplay();
        }

        private void chkModeControls_CheckedChanged(object sender, EventArgs e)
        {
            console.ShowModeControls = chkShowModeControls.Checked;
            console.modeControlsToolStripMenuItem.Checked = chkShowModeControls.Checked;

            if (console.CollapsedDisplay)
                console.CollapseDisplay();
        }


        #endregion

        #region PA Settings Tab Event Handlers

        private void btnPAGainCalibration_Click(object sender, System.EventArgs e)
        {
            string s = "NOTE: this routine works well with Penelope. At present this calibration\n" +
                       "routine is NOT recommended if you are using PennyLane, Hermes or \n" +
                       "Angelia as it produces a large overshoot during the calibraion \n" +
                       "process when used with those boards! \n\n" +
                "Is a 50 Ohm dummy load connected to the amplifier?\n" +
                "\n This function is valid only with an external amplifier and Alex (or equivalent) present." +
                "\n\nFailure to use a dummy load with this routine could cause damage to the amplifier.";
            // if (radGenModelFLEX5000.Checked)
            {
                s = "Is a 50 Ohm dummy load connected to the correct antenna port (";
                switch (FWCAnt.ANT1)
                {
                    case FWCAnt.ANT1: s += "ANT 1"; break;
                    /*case FWCAnt.ANT2: s += "ANT 2"; break;
                    case FWCAnt.ANT3: s += "ANT 3"; break;*/
                }
                s += ")?\nFailure to connect a dummy load properly could cause damage to the radio.";
            }
            DialogResult dr = MessageBox.Show(s,
                "Warning: Is dummy load properly connected?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.No)
                return;

            btnPAGainCalibration.Enabled = false;
            progress = new Progress("Calibrate PA Gain");

            Thread t = new Thread(CalibratePAGain)
            {
                Name = "PA Gain Calibration Thread",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            t.Start();

            if (console.PowerOn)
                progress.Show();
        }

        private void CalibratePAGain()
        {
            bool[] run = new bool[11];

            if (radPACalAllBands.Checked)
            {
                for (int i = 0; i < 11; i++) run[i] = true;
            }
            else
            {
                run[0] = chkPA160.Checked;
                run[1] = chkPA80.Checked;
                run[2] = chkPA60.Checked;
                run[3] = chkPA40.Checked;
                run[4] = chkPA30.Checked;
                run[5] = chkPA20.Checked;
                run[6] = chkPA17.Checked;
                run[7] = chkPA15.Checked;
                run[8] = chkPA12.Checked;
                run[9] = chkPA10.Checked;
                run[10] = chkPA6.Checked;
            }
            bool done = false;
            done = chkPANewCal.Checked ? console.CalibratePAGain2(progress, run, false) : console.CalibratePAGain(progress, run, (int)udPACalPower.Value);
            if (done) MessageBox.Show("PA Gain Calibration complete.");
            btnPAGainCalibration.Enabled = true;
        }

        private void udPAGain_ValueChanged(object sender, System.EventArgs e)
        {
            console.PWR = console.PWR;
        }

        private void btnPAGainReset_Click(object sender, System.EventArgs e)
        {
            if (radGenModelANAN10.Checked || radGenModelANAN10E.Checked)
            {
                ANAN10PAGain160 = 41.0f;
                ANAN10PAGain80 = 41.2f;
                ANAN10PAGain60 = 41.3f;
                ANAN10PAGain40 = 41.3f;
                ANAN10PAGain30 = 41.0f;
                ANAN10PAGain20 = 40.5f;
                ANAN10PAGain17 = 39.9f;
                ANAN10PAGain15 = 38.8f;
                ANAN10PAGain12 = 38.8f;
                ANAN10PAGain10 = 38.8f;
                ANAN10PAGain6 = 38.8f;

                udANAN10PAGainVHF0.Value = 56.2M;
                udANAN10PAGainVHF1.Value = 56.2M;
                udANAN10PAGainVHF2.Value = 56.2M;
                udANAN10PAGainVHF3.Value = 56.2M;
                udANAN10PAGainVHF4.Value = 56.2M;
                udANAN10PAGainVHF5.Value = 56.2M;
                udANAN10PAGainVHF6.Value = 56.2M;
                udANAN10PAGainVHF7.Value = 56.2M;
                udANAN10PAGainVHF8.Value = 56.2M;
                udANAN10PAGainVHF9.Value = 56.2M;
                udANAN10PAGainVHF10.Value = 56.2M;
                udANAN10PAGainVHF11.Value = 56.2M;
                udANAN10PAGainVHF12.Value = 56.2M;
                udANAN10PAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN100B.Checked)
            {
                ANAN100BPAGain160 = 50.0f;
                ANAN100BPAGain80 = 50.5f;
                ANAN100BPAGain60 = 50.5f;
                ANAN100BPAGain40 = 50.0f;
                ANAN100BPAGain30 = 49.5f;
                ANAN100BPAGain20 = 48.5f;
                ANAN100BPAGain17 = 48.0f;
                ANAN100BPAGain15 = 47.5f;
                ANAN100BPAGain12 = 46.5f;
                ANAN100BPAGain10 = 42.0f;
                ANAN100BPAGain6 = 43.0f;

                udANAN100BPAGainVHF0.Value = 56.2M;
                udANAN100BPAGainVHF1.Value = 56.2M;
                udANAN100BPAGainVHF2.Value = 56.2M;
                udANAN100BPAGainVHF3.Value = 56.2M;
                udANAN100BPAGainVHF4.Value = 56.2M;
                udANAN100BPAGainVHF5.Value = 56.2M;
                udANAN100BPAGainVHF6.Value = 56.2M;
                udANAN100BPAGainVHF7.Value = 56.2M;
                udANAN100BPAGainVHF8.Value = 56.2M;
                udANAN100BPAGainVHF9.Value = 56.2M;
                udANAN100BPAGainVHF10.Value = 56.2M;
                udANAN100BPAGainVHF11.Value = 56.2M;
                udANAN100BPAGainVHF12.Value = 56.2M;
                udANAN100BPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN100.Checked)
            {
                ANAN100PAGain160 = 50.0f;
                ANAN100PAGain80 = 50.5f;
                ANAN100PAGain60 = 50.5f;
                ANAN100PAGain40 = 50.0f;
                ANAN100PAGain30 = 49.5f;
                ANAN100PAGain20 = 48.5f;
                ANAN100PAGain17 = 48.0f;
                ANAN100PAGain15 = 47.5f;
                ANAN100PAGain12 = 46.5f;
                ANAN100PAGain10 = 42.0f;
                ANAN100PAGain6 = 43.0f;

                udANAN100PAGainVHF0.Value = 56.2M;
                udANAN100PAGainVHF1.Value = 56.2M;
                udANAN100PAGainVHF2.Value = 56.2M;
                udANAN100PAGainVHF3.Value = 56.2M;
                udANAN100PAGainVHF4.Value = 56.2M;
                udANAN100PAGainVHF5.Value = 56.2M;
                udANAN100PAGainVHF6.Value = 56.2M;
                udANAN100PAGainVHF7.Value = 56.2M;
                udANAN100PAGainVHF8.Value = 56.2M;
                udANAN100PAGainVHF9.Value = 56.2M;
                udANAN100PAGainVHF10.Value = 56.2M;
                udANAN100PAGainVHF11.Value = 56.2M;
                udANAN100PAGainVHF12.Value = 56.2M;
                udANAN100PAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN100D.Checked && !chkBypassANANPASettings.Checked)
            {
                ANANPAGain160 = 49.5f;
                ANANPAGain80 = 50.5f;
                ANANPAGain60 = 50.5f;
                ANANPAGain40 = 50.0f;
                ANANPAGain30 = 49.0f;
                ANANPAGain20 = 48.0f;
                ANANPAGain17 = 47.0f;
                ANANPAGain15 = 46.5f;
                ANANPAGain12 = 46.0f;
                ANANPAGain10 = 43.5f;
                ANANPAGain6 = 43.0f;

                udANANPAGainVHF0.Value = 56.2M;
                udANANPAGainVHF1.Value = 56.2M;
                udANANPAGainVHF2.Value = 56.2M;
                udANANPAGainVHF3.Value = 56.2M;
                udANANPAGainVHF4.Value = 56.2M;
                udANANPAGainVHF5.Value = 56.2M;
                udANANPAGainVHF6.Value = 56.2M;
                udANANPAGainVHF7.Value = 56.2M;
                udANANPAGainVHF8.Value = 56.2M;
                udANANPAGainVHF9.Value = 56.2M;
                udANANPAGainVHF10.Value = 56.2M;
                udANANPAGainVHF11.Value = 56.2M;
                udANANPAGainVHF12.Value = 56.2M;
                udANANPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN200D.Checked && !chkBypassANANPASettings.Checked)
            {
                OrionPAGain160 = 49.5f;
                OrionPAGain80 = 50.5f;
                OrionPAGain60 = 50.5f;
                OrionPAGain40 = 50.0f;
                OrionPAGain30 = 49.0f;
                OrionPAGain20 = 48.0f;
                OrionPAGain17 = 47.0f;
                OrionPAGain15 = 46.5f;
                OrionPAGain12 = 46.0f;
                OrionPAGain10 = 43.5f;
                OrionPAGain6 = 43.0f;

                udOrionPAGainVHF0.Value = 56.2M;
                udOrionPAGainVHF1.Value = 56.2M;
                udOrionPAGainVHF2.Value = 56.2M;
                udOrionPAGainVHF3.Value = 56.2M;
                udOrionPAGainVHF4.Value = 56.2M;
                udOrionPAGainVHF5.Value = 56.2M;
                udOrionPAGainVHF6.Value = 56.2M;
                udOrionPAGainVHF7.Value = 56.2M;
                udOrionPAGainVHF8.Value = 56.2M;
                udOrionPAGainVHF9.Value = 56.2M;
                udOrionPAGainVHF10.Value = 56.2M;
                udOrionPAGainVHF11.Value = 56.2M;
                udOrionPAGainVHF12.Value = 56.2M;
                udOrionPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelHPSDR.Checked || (radGenModelANAN100D.Checked && chkBypassANANPASettings.Checked))
            {
                udPAGain160.Value = 41.0M;
                udPAGain80.Value = 41.2M;
                udPAGain60.Value = 41.3M;
                udPAGain40.Value = 41.3M;
                udPAGain30.Value = 41.0M;
                udPAGain20.Value = 40.5M;
                udPAGain17.Value = 39.9M;
                udPAGain15.Value = 38.8M;
                udPAGain12.Value = 38.8M;
                udPAGain10.Value = 38.8M;
                udPAGain6.Value = 38.8M;

                udPAGainVHF0.Value = 56.2M;
                udPAGainVHF1.Value = 56.2M;
                udPAGainVHF2.Value = 56.2M;
                udPAGainVHF3.Value = 56.2M;
                udPAGainVHF4.Value = 56.2M;
                udPAGainVHF5.Value = 56.2M;
                udPAGainVHF6.Value = 56.2M;
                udPAGainVHF7.Value = 56.2M;
                udPAGainVHF8.Value = 56.2M;
                udPAGainVHF9.Value = 56.2M;
                udPAGainVHF10.Value = 56.2M;
                udPAGainVHF11.Value = 56.2M;
                udPAGainVHF12.Value = 56.2M;
                udPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelORIONMKII.Checked)
            {
                ORIONMKIIPAGain160 = 50.0f;
                ORIONMKIIPAGain80 = 50.5f;
                ORIONMKIIPAGain60 = 50.5f;
                ORIONMKIIPAGain40 = 50.0f;
                ORIONMKIIPAGain30 = 49.5f;
                ORIONMKIIPAGain20 = 48.5f;
                ORIONMKIIPAGain17 = 48.0f;
                ORIONMKIIPAGain15 = 47.5f;
                ORIONMKIIPAGain12 = 46.5f;
                ORIONMKIIPAGain10 = 42.0f;
                ORIONMKIIPAGain6 = 43.0f;

                udORIONMKIIPAGainVHF0.Value = 56.2M;
                udORIONMKIIPAGainVHF1.Value = 56.2M;
                udORIONMKIIPAGainVHF2.Value = 56.2M;
                udORIONMKIIPAGainVHF3.Value = 56.2M;
                udORIONMKIIPAGainVHF4.Value = 56.2M;
                udORIONMKIIPAGainVHF5.Value = 56.2M;
                udORIONMKIIPAGainVHF6.Value = 56.2M;
                udORIONMKIIPAGainVHF7.Value = 56.2M;
                udORIONMKIIPAGainVHF8.Value = 56.2M;
                udORIONMKIIPAGainVHF9.Value = 56.2M;
                udORIONMKIIPAGainVHF10.Value = 56.2M;
                udORIONMKIIPAGainVHF11.Value = 56.2M;
                udORIONMKIIPAGainVHF12.Value = 56.2M;
                udORIONMKIIPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN8000D.Checked)
            {
                ANAN8000DPAGain160 = 50.0f;
                ANAN8000DPAGain80 = 50.5f;
                ANAN8000DPAGain60 = 50.5f;
                ANAN8000DPAGain40 = 50.0f;
                ANAN8000DPAGain30 = 49.5f;
                ANAN8000DPAGain20 = 48.5f;
                ANAN8000DPAGain17 = 48.0f;
                ANAN8000DPAGain15 = 47.5f;
                ANAN8000DPAGain12 = 46.5f;
                ANAN8000DPAGain10 = 42.0f;
                ANAN8000DPAGain6 = 43.0f;

                udANAN8000DPAGainVHF0.Value = 56.2M;
                udANAN8000DPAGainVHF1.Value = 56.2M;
                udANAN8000DPAGainVHF2.Value = 56.2M;
                udANAN8000DPAGainVHF3.Value = 56.2M;
                udANAN8000DPAGainVHF4.Value = 56.2M;
                udANAN8000DPAGainVHF5.Value = 56.2M;
                udANAN8000DPAGainVHF6.Value = 56.2M;
                udANAN8000DPAGainVHF7.Value = 56.2M;
                udANAN8000DPAGainVHF8.Value = 56.2M;
                udANAN8000DPAGainVHF9.Value = 56.2M;
                udANAN8000DPAGainVHF10.Value = 56.2M;
                udANAN8000DPAGainVHF11.Value = 56.2M;
                udANAN8000DPAGainVHF12.Value = 56.2M;
                udANAN8000DPAGainVHF13.Value = 56.2M;
            }

            if (radGenModelANAN7000D.Checked)
            {
                ANAN7000DPAGain160 = 51.3f;
                ANAN7000DPAGain80 = 53.3f;
                ANAN7000DPAGain60 = 53.9f;
                ANAN7000DPAGain40 = 54.0f;
                ANAN7000DPAGain30 = 54.2f;
                ANAN7000DPAGain20 = 53.6f;
                ANAN7000DPAGain17 = 53.0f;
                ANAN7000DPAGain15 = 52.7f;
                ANAN7000DPAGain12 = 51.7f;
                ANAN7000DPAGain10 = 50.3f;
                ANAN7000DPAGain6 = 46.6f;

                udANAN7000DPAGainVHF0.Value = 63.1M;
                udANAN7000DPAGainVHF1.Value = 63.1M;
                udANAN7000DPAGainVHF2.Value = 63.1M;
                udANAN7000DPAGainVHF3.Value = 63.1M;
                udANAN7000DPAGainVHF4.Value = 63.1M;
                udANAN7000DPAGainVHF5.Value = 63.1M;
                udANAN7000DPAGainVHF6.Value = 63.1M;
                udANAN7000DPAGainVHF7.Value = 63.1M;
                udANAN7000DPAGainVHF8.Value = 63.1M;
                udANAN7000DPAGainVHF9.Value = 63.1M;
                udANAN7000DPAGainVHF10.Value = 63.1M;
                udANAN7000DPAGainVHF11.Value = 63.1M;
                udANAN7000DPAGainVHF12.Value = 63.1M;
                udANAN7000DPAGainVHF13.Value = 63.1M;
            }

            if (radGenModelHermes.Checked)
            {
                HermesPAGain160 = 41.0f;
                HermesPAGain80 = 41.2f;
                HermesPAGain60 = 41.3f;
                HermesPAGain40 = 41.3f;
                HermesPAGain30 = 41.0f;
                HermesPAGain20 = 40.5f;
                HermesPAGain17 = 39.9f;
                HermesPAGain15 = 38.8f;
                HermesPAGain12 = 38.8f;
                HermesPAGain10 = 38.8f;
                HermesPAGain6 = 38.8f;

                udHermesPAGainVHF0.Value = 56.2M;
                udHermesPAGainVHF1.Value = 56.2M;
                udHermesPAGainVHF2.Value = 56.2M;
                udHermesPAGainVHF3.Value = 56.2M;
                udHermesPAGainVHF4.Value = 56.2M;
                udHermesPAGainVHF5.Value = 56.2M;
                udHermesPAGainVHF6.Value = 56.2M;
                udHermesPAGainVHF7.Value = 56.2M;
                udHermesPAGainVHF8.Value = 56.2M;
                udHermesPAGainVHF9.Value = 56.2M;
                udHermesPAGainVHF10.Value = 56.2M;
                udHermesPAGainVHF11.Value = 56.2M;
                udHermesPAGainVHF12.Value = 56.2M;
                udHermesPAGainVHF13.Value = 56.2M;
            }
        }

        #endregion

        #region Appearance Tab Event Handlers

        private void clrbtnBackground_Changed(object sender, System.EventArgs e)
        {
            Display.DisplayBackgroundColor = Color.FromArgb(tbBackgroundAlpha.Value, clrbtnBackground.Color);
            //Display.DisplayBackgroundColor = clrbtnBackground.Color;
        }

        private void clrbtnTXBackground_Changed(object sender, System.EventArgs e)
        {
            Display.TXDisplayBackgroundColor = Color.FromArgb(tbTXBackgroundAlpha.Value, clrbtnTXBackground.Color);
        }

        private void clrbtnGrid_Changed(object sender, System.EventArgs e)
        {
            Display.GridColor = Color.FromArgb(tbGridCourseAlpha.Value, clrbtnGrid.Color);
            //Display.GridColor = clrbtnGrid.Color;
        }

        private void clrbtnTXVGrid_Changed(object sender, System.EventArgs e)
        {
            Display.TXVGridColor = Color.FromArgb(tbTXVGridCourseAlpha.Value, clrbtnTXVGrid.Color);
        }

        private void clrbtnZeroLine_Changed(object sender, System.EventArgs e)
        {
            Display.GridZeroColor = clrbtnZeroLine.Color;
        }

        private void clrbtnTXZeroLine_Changed(object sender, System.EventArgs e)
        {
            Display.TXGridZeroColor = Color.FromArgb(tbTXZeroLineAlpha.Value, clrbtnTXZeroLine.Color);
        }

        private void clrbtnText_Changed(object sender, System.EventArgs e)
        {
            Display.GridTextColor = clrbtnText.Color;
        }

        private void clrbtnTXText_Changed(object sender, System.EventArgs e)
        {
            Display.GridTXTextColor = Color.FromArgb(tbTXTextAlpha.Value, clrbtnTXText.Color);
        }

        private void clrbtnDataLine_Changed(object sender, System.EventArgs e)
        {
            Display.DataLineColor = clrbtnDataLine.Color;
        }

        private void clrbtnTXDataLine_Changed(object sender, System.EventArgs e)
        {
            Display.TXDataLineColor = clrbtnTXDataLine.Color;
        }

        private void clrbtnFilter_Changed(object sender, System.EventArgs e)
        {
            Display.DisplayFilterColor = Color.FromArgb(tbRX1FilterAlpha.Value, clrbtnFilter.Color);
        }

        private void clrbtnGridTXFilter_Changed(object sender, System.EventArgs e)
        {
            Display.TXFilterColor = Color.FromArgb(tbTXFilterAlpha.Value, clrbtnGridTXFilter.Color);
        }

        private void udDisplayLineWidth_ValueChanged(object sender, System.EventArgs e)
        {
            Display.DisplayLineWidth = (float)udDisplayLineWidth.Value;
        }

        private void udTXLineWidth_ValueChanged(object sender, System.EventArgs e)
        {
            Display.TXDisplayLineWidth = (float)udTXLineWidth.Value;
        }

        private void clrbtnMeterLeft_Changed(object sender, System.EventArgs e)
        {
            console.MeterLeftColor = clrbtnMeterLeft.Color;
        }

        private void clrbtnMeterRight_Changed(object sender, System.EventArgs e)
        {
            console.MeterRightColor = clrbtnMeterRight.Color;
        }

        private void clrbtnBtnSel_Changed(object sender, System.EventArgs e)
        {
            console.ButtonSelectedColor = clrbtnBtnSel.Color;
        }

        private void clrbtnVFODark_Changed(object sender, System.EventArgs e)
        {
            console.VFOTextDarkColor = clrbtnVFODark.Color;
        }

        private void clrbtnVFOLight_Changed(object sender, System.EventArgs e)
        {
            console.VFOTextLightColor = clrbtnVFOLight.Color;
        }

        private void clrbtnBandDark_Changed(object sender, System.EventArgs e)
        {
            console.BandTextDarkColor = clrbtnBandDark.Color;
        }

        private void clrbtnBandLight_Changed(object sender, System.EventArgs e)
        {
            console.BandTextLightColor = clrbtnBandLight.Color;
        }

        private void clrbtnPeakText_Changed(object sender, System.EventArgs e)
        {
            console.PeakTextColor = clrbtnPeakText.Color;
        }

        private void clrbtnOutOfBand_Changed(object sender, System.EventArgs e)
        {
            console.OutOfBandColor = clrbtnOutOfBand.Color;
        }

        private void chkVFOSmallLSD_CheckedChanged(object sender, System.EventArgs e)
        {
            console.SmallLSD = chkVFOSmallLSD.Checked;
        }

        private void clrbtnVFOSmallColor_Changed(object sender, System.EventArgs e)
        {
            console.SmallVFOColor = clrbtnVFOSmallColor.Color;
        }

        private void clrbtnInfoButtonsColor_Changed(object sender, System.EventArgs e)
        {
            console.InfoButtonsColor = clrbtnInfoButtonsColor.Color;
        }

        private void clrbtnPeakBackground_Changed(object sender, System.EventArgs e)
        {
            console.PeakBackgroundColor = clrbtnPeakBackground.Color;
        }

        private void clrbtnMeterBackground_Changed(object sender, System.EventArgs e)
        {
            console.MeterBackgroundColor = clrbtnMeterBackground.Color;
        }

        private void clrbtnBandBackground_Changed(object sender, System.EventArgs e)
        {
            console.BandBackgroundColor = clrbtnBandBackground.Color;
        }

        private void clrbtnVFOBackground_Changed(object sender, System.EventArgs e)
        {
            console.VFOBackgroundColor = clrbtnVFOBackground.Color;
        }

        #endregion

        #region Keyboard Tab Event Handlers

        private void comboKBTuneUp1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp1 = (Keys)KeyList[comboKBTuneUp1.SelectedIndex];
        }

        private void comboKBTuneDown1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown1 = (Keys)KeyList[comboKBTuneDown1.SelectedIndex];
        }

        private void comboKBTuneUp2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp2 = (Keys)KeyList[comboKBTuneUp2.SelectedIndex];
        }

        private void comboKBTuneDown2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown2 = (Keys)KeyList[comboKBTuneDown2.SelectedIndex];
        }

        private void comboKBTuneUp3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp3 = (Keys)KeyList[comboKBTuneUp3.SelectedIndex];
        }

        private void comboKBTuneDown3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown3 = (Keys)KeyList[comboKBTuneDown3.SelectedIndex];
        }

        private void comboKBTuneUp4_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp4 = (Keys)KeyList[comboKBTuneUp4.SelectedIndex];
        }

        private void comboKBTuneDown4_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown4 = (Keys)KeyList[comboKBTuneDown4.SelectedIndex];
        }

        private void comboKBTuneUp5_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp5 = (Keys)KeyList[comboKBTuneUp5.SelectedIndex];
        }

        private void comboKBTuneDown5_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown5 = (Keys)KeyList[comboKBTuneDown5.SelectedIndex];
        }

        private void comboKBTuneUp6_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp6 = (Keys)KeyList[comboKBTuneUp6.SelectedIndex];
        }

        private void comboKBTuneDown6_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown6 = (Keys)KeyList[comboKBTuneDown6.SelectedIndex];
        }

        private void comboKBTuneUp7_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneUp7 = (Keys)KeyList[comboKBTuneUp7.SelectedIndex];
        }

        private void comboKBTuneDown7_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyTuneDown7 = (Keys)KeyList[comboKBTuneDown7.SelectedIndex];
        }

        private void comboKBBandUp_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyBandUp = (Keys)KeyList[comboKBBandUp.SelectedIndex];
        }

        private void comboKBBandDown_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyBandDown = (Keys)KeyList[comboKBBandDown.SelectedIndex];
        }

        private void comboKBFilterUp_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyFilterUp = (Keys)KeyList[comboKBFilterUp.SelectedIndex];
        }

        private void comboKBFilterDown_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyFilterDown = (Keys)KeyList[comboKBFilterDown.SelectedIndex];
        }

        private void comboKBModeUp_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyModeUp = (Keys)KeyList[comboKBModeUp.SelectedIndex];
        }

        private void comboKBModeDown_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyModeDown = (Keys)KeyList[comboKBModeDown.SelectedIndex];
        }

        private void comboKBCWDot_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyCWDot = (Keys)KeyList[comboKBCWDot.SelectedIndex];
        }

        private void comboKBCWDash_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyCWDash = (Keys)KeyList[comboKBCWDash.SelectedIndex];
        }

        private void comboKBRITUp_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyRITUp = (Keys)KeyList[comboKBRITUp.SelectedIndex];
        }

        private void comboKBRITDown_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyRITDown = (Keys)KeyList[comboKBRITDown.SelectedIndex];
        }

        private void comboKBXITUp_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyXITUp = (Keys)KeyList[comboKBXITUp.SelectedIndex];
        }

        private void comboKBXITDown_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyXITDown = (Keys)KeyList[comboKBXITDown.SelectedIndex];
        }

        private void comboKBPTTTx_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyPTTTx = (Keys)KeyList[comboKBPTTTx.SelectedIndex];
        }

        private void comboKBPTTRx_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            console.KeyPTTRx = (Keys)KeyList[comboKBPTTRx.SelectedIndex];
        }

        #endregion

        #region CAT Setup event handlers

        public void initCATandPTTprops()
        {
            console.CATEnabled = chkCATEnable.Checked;
            if (comboCATPort.Text.StartsWith("COM"))
                console.CATPort = Int32.Parse(comboCATPort.Text.Substring(3));
            console.CATPTTRTS = chkCATPTT_RTS.Checked;
            console.CATPTTDTR = chkCATPTT_DTR.Checked;
            //console.PTTBitBangEnabled = chkCATPTTEnabled.Checked; 
            if (comboCATPTTPort.Text.StartsWith("COM"))
                console.CATPTTBitBangPort = Int32.Parse(comboCATPTTPort.Text.Substring(3));
            console.CATParity = SDRSerialPort.StringToParity((string)comboCATparity.SelectedItem);
            console.CATDataBits = int.Parse((string)comboCATdatabits.SelectedItem);
            console.CATStopBits = SDRSerialPort.StringToStopBits((string)comboCATstopbits.SelectedItem);
            console.CATEnabled = chkCATEnable.Checked;

            // make sure the enabled state of bitbang ptt is correct 
            if (chkCATPTT_RTS.Checked || chkCATPTT_DTR.Checked)
            {
                chkCATPTTEnabled.Enabled = true;
            }
            else
            {
                chkCATPTTEnabled.Enabled = false;
                chkCATPTTEnabled.Checked = false;
            }

            console.CAT2Enabled = chkCAT2Enable.Checked;
            if (comboCAT2Port.Text.StartsWith("COM"))
                console.CAT2Port = Int32.Parse(comboCAT2Port.Text.Substring(3));
            // console.CATPTTRTS = chkCATPTT_RTS.Checked;
            // console.CATPTTDTR = chkCATPTT_DTR.Checked;
            //console.PTTBitBangEnabled = chkCATPTTEnabled.Checked; 
            // if (comboCATPTTPort.Text.StartsWith("COM"))
            // console.CATPTTBitBangPort = Int32.Parse(comboCATPTTPort.Text.Substring(3));
            console.CAT2Parity = SDRSerialPort.StringToParity((string)comboCAT2parity.SelectedItem);
            console.CAT2DataBits = int.Parse((string)comboCAT2databits.SelectedItem);
            console.CAT2StopBits = SDRSerialPort.StringToStopBits((string)comboCAT2stopbits.SelectedItem);
            console.CAT2Enabled = chkCAT2Enable.Checked;

            console.CAT3Enabled = chkCAT3Enable.Checked;
            if (comboCAT3Port.Text.StartsWith("COM"))
                console.CAT3Port = Int32.Parse(comboCAT3Port.Text.Substring(3));
            // console.CATPTTRTS = chkCATPTT_RTS.Checked;
            // console.CATPTTDTR = chkCATPTT_DTR.Checked;
            //console.PTTBitBangEnabled = chkCATPTTEnabled.Checked; 
            // if (comboCATPTTPort.Text.StartsWith("COM"))
            // console.CATPTTBitBangPort = Int32.Parse(comboCATPTTPort.Text.Substring(3));
            console.CAT3Parity = SDRSerialPort.StringToParity((string)comboCAT3parity.SelectedItem);
            console.CAT3DataBits = int.Parse((string)comboCAT3databits.SelectedItem);
            console.CAT3StopBits = SDRSerialPort.StringToStopBits((string)comboCAT3stopbits.SelectedItem);
            console.CAT3Enabled = chkCAT3Enable.Checked;

            console.CAT4Enabled = chkCAT4Enable.Checked;
            if (comboCAT4Port.Text.StartsWith("COM"))
                console.CAT4Port = Int32.Parse(comboCAT4Port.Text.Substring(3));
            // console.CATPTTRTS = chkCATPTT_RTS.Checked;
            // console.CATPTTDTR = chkCATPTT_DTR.Checked;
            //console.PTTBitBangEnabled = chkCATPTTEnabled.Checked; 
            // if (comboCATPTTPort.Text.StartsWith("COM"))
            // console.CATPTTBitBangPort = Int32.Parse(comboCATPTTPort.Text.Substring(3));
            console.CAT4Parity = SDRSerialPort.StringToParity((string)comboCAT4parity.SelectedItem);
            console.CAT4DataBits = int.Parse((string)comboCAT4databits.SelectedItem);
            console.CAT4StopBits = SDRSerialPort.StringToStopBits((string)comboCAT4stopbits.SelectedItem);
            console.CAT4Enabled = chkCAT4Enable.Checked;
        }

        // called in error cases to set the dialiog vars from 
        // the console properties -- sort of ugly, we should only have 1 copy 
        // of this stuff 
        public void copyCATPropsToDialogVars()
        {
            chkCATEnable.Checked = console.CATEnabled;
            string port = "COM" + console.CATPort.ToString();
            if (comboCATPort.Items.Contains(port))
                comboCATPort.Text = port;
            chkCATPTT_RTS.Checked = console.CATPTTRTS;
            chkCATPTT_DTR.Checked = console.CATPTTDTR;
            chkCATPTTEnabled.Checked = console.PTTBitBangEnabled;
            port = "COM" + console.CATPTTBitBangPort.ToString();
            if (comboCATPTTPort.Items.Contains(port))
                comboCATPTTPort.Text = port;

            // wjt fixme -- need to hand baudrate, parity, data, stop -- see initCATandPTTprops 
        }


        public void copyCAT2PropsToDialogVars()
        {
            chkCAT2Enable.Checked = console.CAT2Enabled;
            string port = "COM" + console.CAT2Port.ToString();
            if (comboCAT2Port.Items.Contains(port))
                comboCAT2Port.Text = port;
            // chkCATPTT_RTS.Checked = console.CATPTTRTS;
            // chkCATPTT_DTR.Checked = console.CATPTTDTR;
            // chkCATPTTEnabled.Checked = console.PTTBitBangEnabled;
            // port = "COM" + console.CATPTTBitBangPort.ToString();
            // if (comboCATPTTPort.Items.Contains(port))
            //  comboCATPTTPort.Text = port;
        }

        public void copyCAT3PropsToDialogVars()
        {
            chkCAT3Enable.Checked = console.CAT3Enabled;
            string port = "COM" + console.CAT3Port.ToString();
            if (comboCAT3Port.Items.Contains(port))
                comboCAT3Port.Text = port;
            // chkCATPTT_RTS.Checked = console.CATPTTRTS;
            // chkCATPTT_DTR.Checked = console.CATPTTDTR;
            // chkCATPTTEnabled.Checked = console.PTTBitBangEnabled;
            // port = "COM" + console.CATPTTBitBangPort.ToString();
            // if (comboCATPTTPort.Items.Contains(port))
            //  comboCATPTTPort.Text = port;
        }

        public void copyCAT4PropsToDialogVars()
        {
            chkCAT4Enable.Checked = console.CAT4Enabled;
            string port = "COM" + console.CAT4Port.ToString();
            if (comboCAT4Port.Items.Contains(port))
                comboCAT4Port.Text = port;
            //  chkCATPTT_RTS.Checked = console.CATPTTRTS;
            // chkCATPTT_DTR.Checked = console.CATPTTDTR;
            // chkCATPTTEnabled.Checked = console.PTTBitBangEnabled;
            // port = "COM" + console.CATPTTBitBangPort.ToString();
            // if (comboCATPTTPort.Items.Contains(port))
            //   comboCATPTTPort.Text = port;
        }

        private void chkCATEnable_CheckedChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            if (comboCATPort.Text == "" || !comboCATPort.Text.StartsWith("COM"))
            {
                if (chkCATEnable.Focused)
                {
                    if (chkCATEnable.Focused && chkCATEnable.Checked)
                    {
                        MessageBox.Show("The CAT port \"" + comboCATPort.Text + "\" is not a valid port.\n" +
                            "Please select another port.");
                        chkCATEnable.Checked = false;
                    }
                }
                return;
            }

            // make sure we're not using the same comm port as the bit banger 
            if (chkCATEnable.Checked && console.PTTBitBangEnabled &&
                (comboCATPort.Text == comboCATPTTPort.Text))
            {
                MessageBox.Show("CAT port cannot be the same as Bit Bang Port", "Port Selection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                chkCATEnable.Checked = false;
            }

            // if enabled, disable changing of serial port 
            bool enable_sub_fields = !chkCATEnable.Checked;
            comboCATPort.Enabled = enable_sub_fields;

            enableCAT_HardwareFields(enable_sub_fields);

            if (chkCATEnable.Checked)
            {
                try
                {
                    console.CATEnabled = true;
                }
                catch (Exception ex)
                {
                    console.CATEnabled = false;
                    chkCATEnable.Checked = false;
                    MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message +
                        "\n\nCAT control has been disabled.", "Error Initializing CAT control",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (comboKeyerConnSecondary.Text == "CAT" && chkCATEnable.Focused)
                {
                    MessageBox.Show("The Secondary Keyer option has been changed to None since CAT has been disabled.",
                        "CAT Disabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    comboKeyerConnSecondary.Text = "None";
                }

                if (comboCATPTTPort.Text == "CAT" && chkCATEnable.Focused)
                {
                    MessageBox.Show("The PTT Control Port option has been changed to None since CAT has been disabled.",
                        "CAT Disabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    chkCATPTT_RTS.Checked = false;
                    chkCATPTT_DTR.Checked = false;
                    comboCATPTTPort.Text = "None";
                }

                console.CATEnabled = false;
            }
        }

        private void chkCAT2Enable_CheckedChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            if (comboCAT2Port.Text == "" || !comboCAT2Port.Text.StartsWith("COM"))
            {
                if (chkCAT2Enable.Focused)
                {
                    if (chkCAT2Enable.Focused && chkCAT2Enable.Checked)
                    {
                        MessageBox.Show("The CAT port \"" + comboCAT2Port.Text + "\" is not a valid port.\n" +
                            "Please select another port.");
                        chkCAT2Enable.Checked = false;
                    }
                }
                return;
            }

            // make sure we're not using the same comm port as the bit banger 
            if (chkCAT2Enable.Checked && console.PTTBitBangEnabled &&
                (comboCAT2Port.Text == comboCATPTTPort.Text))
            {
                MessageBox.Show("CAT port cannot be the same as Bit Bang Port", "Port Selection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                chkCAT2Enable.Checked = false;
            }

            // if enabled, disable changing of serial port 
            bool enable_sub_fields = !chkCAT2Enable.Checked;
            comboCAT2Port.Enabled = enable_sub_fields;

            enableCAT2_HardwareFields(enable_sub_fields);

            if (chkCAT2Enable.Checked)
            {
                try
                {
                    console.CAT2Enabled = true;
                }
                catch (Exception ex)
                {
                    console.CAT2Enabled = false;
                    chkCAT2Enable.Checked = false;
                    MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message +
                        "\n\nCAT control has been disabled.", "Error Initializing CAT control",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                /* if (comboKeyerConnSecondary.Text == "CAT" && chkCATEnable.Focused)
                 {
                     MessageBox.Show("The Secondary Keyer option has been changed to None since CAT has been disabled.",
                         "CAT Disabled",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information);
                     comboKeyerConnSecondary.Text = "None";
                 } */
                console.CAT2Enabled = false;
            }
        }

        private void chkCAT3Enable_CheckedChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            if (comboCAT3Port.Text == "" || !comboCAT3Port.Text.StartsWith("COM"))
            {
                if (chkCAT3Enable.Focused)
                {
                    if (chkCAT3Enable.Focused && chkCAT3Enable.Checked)
                    {
                        MessageBox.Show("The CAT port \"" + comboCAT3Port.Text + "\" is not a valid port.\n" +
                            "Please select another port.");
                        chkCAT3Enable.Checked = false;
                    }
                }
                return;
            }

            // make sure we're not using the same comm port as the bit banger 
            if (chkCAT3Enable.Checked && console.PTTBitBangEnabled &&
                (comboCAT3Port.Text == comboCATPTTPort.Text))
            {
                MessageBox.Show("CAT port cannot be the same as Bit Bang Port", "Port Selection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                chkCAT3Enable.Checked = false;
            }

            // if enabled, disable changing of serial port 
            bool enable_sub_fields = !chkCAT3Enable.Checked;
            comboCAT3Port.Enabled = enable_sub_fields;

            enableCAT3_HardwareFields(enable_sub_fields);

            if (chkCAT3Enable.Checked)
            {
                try
                {
                    console.CAT3Enabled = true;
                }
                catch (Exception ex)
                {
                    console.CAT3Enabled = false;
                    chkCAT3Enable.Checked = false;
                    MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message +
                        "\n\nCAT control has been disabled.", "Error Initializing CAT control",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                /* if (comboKeyerConnSecondary.Text == "CAT" && chkCATEnable.Focused)
                 {
                     MessageBox.Show("The Secondary Keyer option has been changed to None since CAT has been disabled.",
                         "CAT Disabled",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information);
                     comboKeyerConnSecondary.Text = "None";
                 } */
                console.CAT3Enabled = false;
            }
        }

        private void chkCAT4Enable_CheckedChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            if (comboCAT4Port.Text == "" || !comboCAT4Port.Text.StartsWith("COM"))
            {
                if (chkCAT4Enable.Focused)
                {
                    if (chkCAT4Enable.Focused && chkCAT4Enable.Checked)
                    {
                        MessageBox.Show("The CAT port \"" + comboCAT4Port.Text + "\" is not a valid port.\n" +
                            "Please select another port.");
                        chkCAT4Enable.Checked = false;
                    }
                }
                return;
            }

            // make sure we're not using the same comm port as the bit banger 
            if (chkCAT4Enable.Checked && console.PTTBitBangEnabled &&
                (comboCAT4Port.Text == comboCATPTTPort.Text))
            {
                MessageBox.Show("CAT port cannot be the same as Bit Bang Port", "Port Selection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                chkCAT4Enable.Checked = false;
            }

            // if enabled, disable changing of serial port 
            bool enable_sub_fields = !chkCAT4Enable.Checked;
            comboCAT4Port.Enabled = enable_sub_fields;

            enableCAT4_HardwareFields(enable_sub_fields);

            if (chkCAT4Enable.Checked)
            {
                try
                {
                    console.CAT4Enabled = true;
                }
                catch (Exception ex)
                {
                    console.CAT4Enabled = false;
                    chkCAT4Enable.Checked = false;
                    MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message +
                        "\n\nCAT control has been disabled.", "Error Initializing CAT control",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                /* if (comboKeyerConnSecondary.Text == "CAT" && chkCATEnable.Focused)
                 {
                     MessageBox.Show("The Secondary Keyer option has been changed to None since CAT has been disabled.",
                         "CAT Disabled",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information);
                     comboKeyerConnSecondary.Text = "None";
                 } */
                console.CAT4Enabled = false;
            }
        }

        private void enableCAT_HardwareFields(bool enable)
        {
            comboCATbaud.Enabled = enable;
            comboCATparity.Enabled = enable;
            comboCATdatabits.Enabled = enable;
            comboCATstopbits.Enabled = enable;
        }

        private void enableCAT2_HardwareFields(bool enable)
        {
            comboCAT2baud.Enabled = enable;
            comboCAT2parity.Enabled = enable;
            comboCAT2databits.Enabled = enable;
            comboCAT2stopbits.Enabled = enable;
        }

        private void enableCAT3_HardwareFields(bool enable)
        {
            comboCAT3baud.Enabled = enable;
            comboCAT3parity.Enabled = enable;
            comboCAT3databits.Enabled = enable;
            comboCAT3stopbits.Enabled = enable;
        }

        private void enableCAT4_HardwareFields(bool enable)
        {
            comboCAT4baud.Enabled = enable;
            comboCAT4parity.Enabled = enable;
            comboCAT4databits.Enabled = enable;
            comboCAT4stopbits.Enabled = enable;
        }

        private void doEnablementOnBitBangEnable()
        {
            if (comboCATPTTPort.Text != "None" &&
                (chkCATPTT_RTS.Checked || chkCATPTT_DTR.Checked))  // if RTS or DTR & port is not None, enable 
            {
                chkCATPTTEnabled.Enabled = true;
            }
            else
            {
                chkCATPTTEnabled.Enabled = false;
                chkCATPTTEnabled.Checked = false; // make sure it is not checked 
            }
        }

        private void chkCATPTT_RTS_CheckedChanged(object sender, System.EventArgs e)
        {
            console.Siolisten.PTTOnRTS = chkCATPTT_RTS.Checked;
            console.CATPTTRTS = chkCATPTT_RTS.Checked;
            doEnablementOnBitBangEnable();
        }

        private void chkCATPTT_DTR_CheckedChanged(object sender, System.EventArgs e)
        {
            console.Siolisten.PTTOnDTR = chkCATPTT_DTR.Checked;
            console.CATPTTDTR = chkCATPTT_DTR.Checked;
            doEnablementOnBitBangEnable();
        }

        private void chkCATPTTEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            if (initializing) return;

            bool enable_sub_fields;

            if (comboCATPTTPort.Text == "" || (!comboCATPTTPort.Text.StartsWith("COM") &&
                !comboCATPTTPort.Text.StartsWith("CAT")))
            {
                if (chkCATPTTEnabled.Focused && chkCATPTTEnabled.Checked)
                {
                    MessageBox.Show("The PTT port \"" + comboCATPTTPort.Text + "\" is not a valid port.  Please select another port.");
                }
                chkCATPTTEnabled.Checked = false;
                return;
            }

            if (chkCATPTTEnabled.Checked && console.CATEnabled &&
                comboCATPort.Text == comboCATPTTPort.Text)
            {
                if (chkCATPTTEnabled.Focused)
                {
                    MessageBox.Show("CAT port cannot be the same as Bit Bang Port", "Port Selection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    chkCATPTTEnabled.Checked = false;
                }
                return;
            }

            if (comboCATPTTPort.Text.StartsWith("CAT")) console.PTTBitBangEnabled = false;
            else console.PTTBitBangEnabled = chkCATPTTEnabled.Checked;

            if (chkCATPTTEnabled.Checked) // if it's enabled don't allow changing settings on port 
            {
                if (comboCATPTTPort.Text.StartsWith("CAT")) console.Siolisten.UseForCATPTT = true;
                enable_sub_fields = false;
            }
            else
            {
                console.Siolisten.UseForCATPTT = false;
                enable_sub_fields = true;
            }
            chkCATPTT_RTS.Enabled = enable_sub_fields;
            chkCATPTT_DTR.Enabled = enable_sub_fields;
            comboCATPTTPort.Enabled = enable_sub_fields;
        }

        private void comboCATparity_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string selection = comboCATparity.SelectedText;
            if (selection == null) return;

            console.CATParity = SDRSerialPort.StringToParity(selection);
        }

        private void comboCAT2parity_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string selection = comboCAT2parity.SelectedText;
            if (selection == null) return;

            console.CAT2Parity = SDRSerialPort.StringToParity(selection);
        }

        private void comboCAT3parity_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string selection = comboCAT3parity.SelectedText;
            if (selection == null) return;

            console.CAT3Parity = SDRSerialPort.StringToParity(selection);
        }

        private void comboCAT4parity_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string selection = comboCAT4parity.SelectedText;
            if (selection == null) return;

            console.CAT4Parity = SDRSerialPort.StringToParity(selection);
        }

        private void comboCATPort_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCATPort.Text == "None")
            {
                if (chkCATEnable.Checked)
                {
                    if (comboCATPort.Focused)
                        chkCATEnable.Checked = false;
                }

                chkCATEnable.Enabled = false;
            }
            else chkCATEnable.Enabled = true;

            if (comboCATPort.Text.StartsWith("COM"))
                console.CATPort = Int32.Parse(comboCATPort.Text.Substring(3));
        }

        private void comboCAT2Port_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT2Port.Text == "None")
            {
                if (chkCAT2Enable.Checked)
                {
                    if (comboCAT2Port.Focused)
                        chkCAT2Enable.Checked = false;
                }

                chkCAT2Enable.Enabled = false;
            }
            else chkCAT2Enable.Enabled = true;

            if (comboCAT2Port.Text.StartsWith("COM"))
                console.CAT2Port = Int32.Parse(comboCAT2Port.Text.Substring(3));
        }

        private void comboCAT3Port_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT3Port.Text == "None")
            {
                if (chkCAT3Enable.Checked)
                {
                    if (comboCAT3Port.Focused)
                        chkCAT3Enable.Checked = false;
                }

                chkCAT3Enable.Enabled = false;
            }
            else chkCAT3Enable.Enabled = true;

            if (comboCAT3Port.Text.StartsWith("COM"))
                console.CAT3Port = Int32.Parse(comboCAT3Port.Text.Substring(3));
        }

        private void comboCAT4Port_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT4Port.Text == "None")
            {
                if (chkCAT4Enable.Checked)
                {
                    if (comboCAT4Port.Focused)
                        chkCAT4Enable.Checked = false;
                }

                chkCAT4Enable.Enabled = false;
            }
            else chkCAT4Enable.Enabled = true;

            if (comboCAT4Port.Text.StartsWith("COM"))
                console.CAT4Port = Int32.Parse(comboCAT4Port.Text.Substring(3));
        }

        private void comboCATPTTPort_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCATPTTPort.Text == "None")
            {
                if (chkCATPTTEnabled.Checked)
                {
                    if (comboCATPTTPort.Focused)
                        chkCATPTTEnabled.Checked = false;
                }

                //chkCATEnable.Enabled = false;
                doEnablementOnBitBangEnable();
            }
            else if (comboCATPTTPort.Text == "CAT")
            {
                if (!chkCATEnable.Checked)
                {
                    MessageBox.Show("CAT is not Enabled.  Please enable the CAT interface before selecting this option.",
                        "CAT not enabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Hand);
                    comboCATPTTPort.Text = "None";
                    return;
                }
                else
                {
                    // console.Siolisten.UseForCATPTT = true;
                    if (chkCATPTT_RTS.Checked || chkCATPTT_DTR.Checked)
                        doEnablementOnBitBangEnable();
                }
            }
            else
            {
                if (chkCATPTT_RTS.Checked || chkCATPTT_DTR.Checked)
                    //chkCATEnable.Enabled = true;
                    doEnablementOnBitBangEnable();
            }

            if (console.Siolisten != null && comboCATPTTPort.Text != "CAT")
                console.Siolisten.UseForCATPTT = false;

            if (comboCATPTTPort.Text.StartsWith("COM"))
                console.CATPTTBitBangPort = Int32.Parse(comboCATPTTPort.Text.Substring(3));
            if (!comboCATPTTPort.Focused)
                chkCATPTTEnabled_CheckedChanged(sender, e);
        }

        private void comboCATbaud_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCATbaud.SelectedIndex >= 0)
                console.CATBaudRate = Int32.Parse(comboCATbaud.Text);
        }

        private void comboCAT2baud_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT2baud.SelectedIndex >= 0)
                console.CAT2BaudRate = Int32.Parse(comboCAT2baud.Text);
        }

        private void comboCAT3baud_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT3baud.SelectedIndex >= 0)
                console.CAT3BaudRate = Int32.Parse(comboCAT3baud.Text);
        }

        private void comboCAT4baud_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT4baud.SelectedIndex >= 0)
                console.CAT4BaudRate = Int32.Parse(comboCAT4baud.Text);
        }

        private void comboCATdatabits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCATdatabits.SelectedIndex >= 0)
                console.CATDataBits = int.Parse(comboCATdatabits.Text);
        }

        private void comboCAT2databits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT2databits.SelectedIndex >= 0)
                console.CAT2DataBits = int.Parse(comboCAT2databits.Text);
        }

        private void comboCAT3databits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT3databits.SelectedIndex >= 0)
                console.CAT3DataBits = int.Parse(comboCAT3databits.Text);
        }

        private void comboCAT4databits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT4databits.SelectedIndex >= 0)
                console.CAT4DataBits = int.Parse(comboCAT4databits.Text);
        }

        private void comboCATstopbits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCATstopbits.SelectedIndex >= 0)
                console.CATStopBits = SDRSerialPort.StringToStopBits(comboCATstopbits.Text);
        }

        private void comboCAT2stopbits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT2stopbits.SelectedIndex >= 0)
                console.CAT2StopBits = SDRSerialPort.StringToStopBits(comboCAT2stopbits.Text);
        }

        private void comboCAT3stopbits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT3stopbits.SelectedIndex >= 0)
                console.CAT3StopBits = SDRSerialPort.StringToStopBits(comboCAT3stopbits.Text);
        }

        private void comboCAT4stopbits_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboCAT4stopbits.SelectedIndex >= 0)
                console.CAT4StopBits = SDRSerialPort.StringToStopBits(comboCAT4stopbits.Text);
        }

        private void btnCATTest_Click(object sender, System.EventArgs e)
        {
            CATTester cat = new CATTester(console);
            //this.Close();
            cat.Show();
            cat.Focus();
        }

        //Modified 10/12/08 BT to change "SDR-1000" to "PowerSDR"
        private void comboCATRigType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboCATRigType.Text)
            {
                case "PowerSDR":
                    console.CATRigType = 900;
                    break;
                case "TS-2000":
                    console.CATRigType = 19;
                    break;
                case "TS-50S":
                    console.CATRigType = 13;
                    break;
                case "TS-440":
                    console.CATRigType = 20;
                    break;
                default:
                    console.CATRigType = 19;
                    break;
            }
        }

        #endregion

        #region Test Tab Event Handlers
        public bool TestIMD
        {
            set { chkTestIMD.Checked = value; }
        }



        private static bool ttgenrun = false;
        public bool TTgenrun
        {
            get { return chkTestIMD.Checked; }
            set
            {
                ttgenrun = value;
                if (ttgenrun && !chkTestIMD.Checked) chkTestIMD.Checked = true;
                if (!ttgenrun && chkTestIMD.Checked && console.MOX) chkTestIMD.Checked = false;
            }
        }

        private void chkTestIMD_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkTestIMD.Checked)
            {
                udTestIMDFreq1.Enabled = false;
                udTestIMDFreq2.Enabled = false;
                udTwoToneLevel.Enabled = false;
                udTestIMDPower.Enabled = false;
                chkInvertTones.Enabled = false;
                double ttfreq1 = (double)udTestIMDFreq1.Value;
                double ttfreq2 = (double)udTestIMDFreq2.Value;
                double ttmag = (double)udTwoToneLevel.Value;
                double ttmag1, ttmag2;
                ttmag1 = ttmag2 = 0.49999 * Math.Pow(10.0, ttmag / 20.0);
                DSPMode mode = console.radio.GetDSPTX(0).CurrentDSPMode;
                if (chkInvertTones.Checked && ((mode == DSPMode.CWL) || (mode == DSPMode.DIGL) || (mode == DSPMode.LSB)))
                {
                    ttfreq1 = -ttfreq1;
                    ttfreq2 = -ttfreq2;
                }
                if (!console.PowerOn)
                {
                    MessageBox.Show("Power must be on to run this test.",
                        "Power is off",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Hand);
                    chkTestIMD.Checked = false;
                    return;
                }
                if (console.MOX)
                {
                    Audio.MOX = false;
                    console.MOX = false;
                    Thread.Sleep(200);
                }
                console.radio.GetDSPTX(0).TXPostGenMode = 1;
                console.radio.GetDSPTX(0).TXPostGenTTFreq1 = ttfreq1;
                console.radio.GetDSPTX(0).TXPostGenTTFreq2 = ttfreq2;
                console.radio.GetDSPTX(0).TXPostGenTTMag1 = ttmag1;
                console.radio.GetDSPTX(0).TXPostGenTTMag2 = ttmag2;
                console.radio.GetDSPTX(0).TXPostGenRun = 1;

                if (!chkTestIMDPower.Checked)
                {
                    console.PreviousPWR = console.PWR;
                    console.PWR = (int)udTestIMDPower.Value;
                }
                console.ManualMox = true;
                Audio.MOX = true;//
                console.MOX = true;
                if (!console.MOX)
                {
                    chkTestIMD.Checked = false;
                    return;
                }

                //Audio.MOX = true;
                chkTestIMD.BackColor = console.ButtonSelectedColor;
                console.psform.TTgenON = true;
            }
            else
            {
                //Audio.MOX = false;
                console.MOX = false;
                Thread.Sleep(200);
                Audio.MOX = false;//
                console.ManualMox = false;
                if (!chkTestIMDPower.Checked)
                {
                    udTestIMDPower.Value = console.PWR;
                    console.PWR = console.PreviousPWR;
                }
                chkTestIMD.BackColor = SystemColors.Control;
                console.psform.TTgenON = false;
                console.radio.GetDSPTX(0).TXPostGenRun = 0;
                udTestIMDFreq1.Enabled = true;
                udTestIMDFreq2.Enabled = true;
                udTwoToneLevel.Enabled = true;
                udTestIMDPower.Enabled = true;
                chkInvertTones.Enabled = true;
            }
        }

        private void btnTestAudioBalStart_Click(object sender, System.EventArgs e)
        {
            if (!console.PowerOn)
            {
                MessageBox.Show("Power must be on to run this test.",
                    "Power is off",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                return;
            }

            DialogResult dr = DialogResult.No;
            Audio.two_tone = false;
            // Audio.SineFreq1 = 600.0;

            do
            {
                Audio.RX1OutputSignal = Audio.SignalSource.SINE_LEFT_ONLY;
                dr = MessageBox.Show("Do you hear a tone in the left channel?",
                    "Tone in left channel?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                Audio.RX1OutputSignal = Audio.SignalSource.RADIO;

                if (dr == DialogResult.No)
                {
                    DialogResult dr2 = MessageBox.Show("Please double check cable and speaker connections.\n" +
                        "Click OK to try again (cancel to abort).",
                        "Check connections",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Asterisk);
                    if (dr2 == DialogResult.Cancel)
                    {
                        MessageBox.Show("Test Failed",
                            "Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
                        btnTestAudioBalStart.BackColor = Color.Red;
                        return;
                    }
                }
                else if (dr == DialogResult.Cancel)
                {
                    MessageBox.Show("Test Failed",
                        "Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                    btnTestAudioBalStart.BackColor = Color.Red;
                    return;
                }
            } while (dr != DialogResult.Yes);

            do
            {
                Audio.RX1OutputSignal = Audio.SignalSource.SINE_RIGHT_ONLY;
                dr = MessageBox.Show("Do you hear a tone in the right channel?",
                    "Tone in right channel?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                Audio.RX1OutputSignal = Audio.SignalSource.RADIO;

                switch (dr)
                {
                    case DialogResult.No:
                        {
                            DialogResult dr2 = MessageBox.Show("Please double check cable and speaker connections.\n" +
                                                               "Click OK to try again (cancel to abort).",
                                                               "Check connections",
                                                               MessageBoxButtons.OKCancel,
                                                               MessageBoxIcon.Asterisk);
                            if (dr2 == DialogResult.Cancel)
                            {
                                MessageBox.Show("Test Failed",
                                                "Failed",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Stop);
                                btnTestAudioBalStart.BackColor = Color.Red;
                                return;
                            }
                        }
                        break;
                    case DialogResult.Cancel:
                        MessageBox.Show("Test Failed",
                                        "Failed",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Stop);
                        btnTestAudioBalStart.BackColor = Color.Red;
                        return;
                }
            } while (dr != DialogResult.Yes);

            MessageBox.Show("Test was successful.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            btnTestAudioBalStart.BackColor = Color.Green;
        }

        private void cmboSigGenRXMode_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (cmboSigGenRXMode.SelectedIndex < 0) return;
            console.radio.GetDSPRX(0, 0).RXPreGenRun = 0;
            console.radio.GetDSPRX(1, 0).RXPreGenRun = 0;
            for (int i = 0; i < 2; i++)
            {
                switch (cmboSigGenRXMode.Text)
                {
                    case "Radio":
                        break;
                    case "Tone":
                        console.radio.GetDSPRX(i, 0).RXPreGenMode = 0;
                        console.radio.GetDSPRX(i, 0).RXPreGenToneMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                        console.radio.GetDSPRX(i, 0).RXPreGenToneFreq = (double)udRXGenFreq.Value;
                        break;
                    case "Noise":
                        console.radio.GetDSPRX(i, 0).RXPreGenMode = 2;
                        console.radio.GetDSPRX(i, 0).RXPreGenNoiseMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                        break;
                    case "Sweep":
                        console.radio.GetDSPRX(i, 0).RXPreGenMode = 3;
                        console.radio.GetDSPRX(i, 0).RXPreGenSweepMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                        console.radio.GetDSPRX(i, 0).RXPreGenSweepFreq1 = (double)udRXGenSweepLow.Value;
                        console.radio.GetDSPRX(i, 0).RXPreGenSweepFreq2 = (double)udRXGenSweepHigh.Value;
                        console.radio.GetDSPRX(i, 0).RXPreGenSweepRate = (double)udRXGenSweepRate.Value;
                        break;
                    case "Silence":
                        console.radio.GetDSPRX(i, 0).RXPreGenMode = -1;
                        break;
                }
            }

            if (chkSigGenRX1.Checked && !(cmboSigGenRXMode.Text == "Radio"))
                console.radio.GetDSPRX(0, 0).RXPreGenRun = 1;

            if (chkSigGenRX2.Checked && !(cmboSigGenRXMode.Text == "Radio"))
                console.radio.GetDSPRX(1, 0).RXPreGenRun = 1;
        }

        private void chkSigGenRX2_CheckedChanged(object sender, System.EventArgs e)
        {
            cmboSigGenRXMode_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void cmboSigGenTXMode_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (cmboSigGenTXMode.SelectedIndex < 0) return;
            console.radio.GetDSPTX(0).TXPreGenRun = 0;

            switch (cmboSigGenTXMode.Text)
            {
                case "Radio":
                    break;
                case "Tone":
                    console.radio.GetDSPTX(0).TXPreGenMode = 0;
                    console.radio.GetDSPTX(0).TXPreGenToneMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenToneFreq = (double)udTXGenFreq.Value;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Noise":
                    console.radio.GetDSPTX(0).TXPreGenMode = 2;
                    console.radio.GetDSPTX(0).TXPreGenNoiseMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Sweep":
                    console.radio.GetDSPTX(0).TXPreGenMode = 3;
                    console.radio.GetDSPTX(0).TXPreGenSweepMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenSweepFreq1 = (double)udTXGenSweepLow.Value;
                    console.radio.GetDSPTX(0).TXPreGenSweepFreq2 = (double)udTXGenSweepHigh.Value;
                    console.radio.GetDSPTX(0).TXPreGenSweepRate = (double)udTXGenSweepRate.Value;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Sawtooth":
                    console.radio.GetDSPTX(0).TXPreGenMode = 4;
                    console.radio.GetDSPTX(0).TXPreGenSawtoothMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenSawtoothFreq = (double)udTXGenFreq.Value;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Triangle":
                    console.radio.GetDSPTX(0).TXPreGenMode = 5;
                    console.radio.GetDSPTX(0).TXPreGenTriangleMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenTriangleFreq = (double)udTXGenFreq.Value;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Pulse":
                    console.radio.GetDSPTX(0).TXPreGenMode = 6;
                    console.radio.GetDSPTX(0).TXPreGenPulseMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    console.radio.GetDSPTX(0).TXPreGenPulseToneFreq = (double)udTXGenFreq.Value;
                    console.radio.GetDSPTX(0).TXPreGenPulseFreq = (double)udTXGenPulseFreq.Value;
                    console.radio.GetDSPTX(0).TXPreGenPulseDutyCycle = (double)udTXGenPulseDutyCycle.Value;
                    console.radio.GetDSPTX(0).TXPreGenPulseTransition = (double)udTXGenPulseTransition.Value;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
                case "Silence":
                    console.radio.GetDSPTX(0).TXPreGenMode = -1;
                    console.radio.GetDSPTX(0).TXPreGenRun = 1;
                    break;
            }
        }

        private void btnImpulse_Click(object sender, System.EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(ImpulseFunction));
            t.Name = "Impulse";
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();
        }

        private void ImpulseFunction()
        {
            //console.Hdw.ImpulseEnable = true;
            //Thread.Sleep(500);
            //for (int i = 0; i < (int)udImpulseNum.Value; i++)
            //{
            //    console.Hdw.Impulse();
            //    Thread.Sleep(45);
            //}
            //Thread.Sleep(500);
            //console.Hdw.ImpulseEnable = false;
        }

        #endregion

        #region Other Event Handlers
        // ======================================================
        // Display Tab Event Handlers
        // ======================================================

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            console.SetFocusMaster(true);
            if (saving)
            {
                this.Hide();
            }
            else
            {
                Thread t = new Thread(new ThreadStart(SaveOptions));
                t.Name = "Save Options Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Lowest;
                t.Start();
                this.Hide();
            }

        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            console.SetFocusMaster(true);
            Thread t = new Thread(new ThreadStart(GetOptions));
            t.Name = "Save Options Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Lowest;
            t.Start();
            this.Hide();
        }

        private void btnApply_Click(object sender, System.EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(ApplyOptions));
            t.Name = "Save Options Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        private void ApplyOptions()
        {
            if (saving) return;
            SaveOptions();
            DB.Update();
        }

        private void udGeneralLPTDelay_ValueChanged(object sender, System.EventArgs e)
        {
            //  console.LatchDelay = (int)udGeneralLPTDelay.Value;
        }

        private void Setup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            console.SetFocusMaster(true);
            this.Hide();
            e.Cancel = true;
        }

        private void btnImportDB_Click(object sender, System.EventArgs e)
        {
            string path = console.AppDataPath;
            path = path.Substring(0, path.LastIndexOf("\\"));
            openFileDialog1.InitialDirectory = path;
            bool ok = false;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ok = CompleteImport();
            }
            if (ok) console.Close();  // Save everything 
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // CompleteImport();
        }

        private bool CompleteImport()
        {
            bool success;
            //if (DB.ImportDatabase(openFileDialog1.FileName))
            //    MessageBox.Show("Database Imported Successfully");

            //-W2PA Import more carefully, allowing DBs created by previous versions to retain settings and options
            if (DB.ImportAndMergeDatabase(openFileDialog1.FileName, console.AppDataPath))
            {
                MessageBox.Show("Database Imported Successfully. Thetis will now close.\n\nPlease RE-START.");
                success = true;
            }
            else
            {
                MessageBox.Show("Database could not be imported. Previous database has been kept.");
                success = false;
            }

            // Archive old database file write a new one.
            if (success)
            {
                string archivePath = console.AppDataPath + "DB_Archive\\";
                if (!Directory.Exists(archivePath)) Directory.CreateDirectory(archivePath);
                string justFileName = console.DBFileName.Substring(console.DBFileName.LastIndexOf("\\") + 1);
                string datetime = DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString().Replace(":", ".");
                File.Copy(console.DBFileName, archivePath + "Thetis_database_" + datetime + ".xml");
                File.Delete(console.DBFileName);
                DB.WriteCurrentDB(console.DBFileName);

                //// Unnecessary to do this applicationof new settings since we close after import and preserve the newly merged database
                //// Also, not closing would allow changes to the configuration that would be overwritten
                //// Saving for later consideration
                //GetTxProfiles();  // load new database values
                //GetOptions();
                //console.GetState();
                //GetTxProfiles();
                //GetTxProfileDefs();
                //if (console.EQForm != null) Common.RestoreForm(console.EQForm, "EQForm", false);
                //if (console.XVTRForm != null) Common.RestoreForm(console.XVTRForm, "XVTR", false);
                //if (console.memoryForm != null) Common.RestoreForm(console.memoryForm, "MemoryForm", false);
                //if (console.diversityForm != null) Common.RestoreForm(console.diversityForm, "DiversityForm", false);
                //if (console.psform != null) Common.RestoreForm(console.psform, "PureSignal", false);
                ////if (console.ampView != null) Common.RestoreForm(console.XVTRForm, "AmpView", false);  //handled by PSform?
            }

            return success;

            ////Old code
            //GetTxProfiles();
            //console.UpdateTXProfile(TXProfile);

            //GetOptions();					// load all database values
            //console.GetState();
            //if (console.EQForm != null) Common.RestoreForm(console.EQForm, "EQForm", false);
            //if (console.XVTRForm != null) Common.RestoreForm(console.XVTRForm, "XVTR", false);
            //// if (console.ProdTestForm != null) Common.RestoreForm(console.ProdTestForm, "ProdTest", false);

            //SaveOptions();					// save all database values
            //console.SaveState();
            //if (console.EQForm != null) Common.SaveForm(console.EQForm, "EQForm");
            //if (console.XVTRForm != null) Common.SaveForm(console.XVTRForm, "XVTR");
            //// if (console.ProdTestForm != null) Common.SaveForm(console.ProdTestForm, "ProdTest");

            //udTransmitTunePower_ValueChanged(this, EventArgs.Empty);
            ////console.ResetMemForm();
        }

        #endregion

        private bool shift_key = false;
        private bool ctrl_key = false;
        private bool alt_key = false;
        private bool windows_key = false;
        private bool menu_key = false;

        private void txtKB_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Debug.WriteLine("KeyCode: " + e.KeyCode + " KeyData: " + e.KeyData + " KeyValue: " + e.KeyValue);
            shift_key = e.Shift;
            ctrl_key = e.Control;
            alt_key = e.Alt;

            if (e.KeyCode == Keys.LWin ||
                e.KeyCode == Keys.RWin)
                windows_key = true;

            if (e.KeyCode == Keys.Apps)
                menu_key = true;

            TextBoxTS txtbox = (TextBoxTS)sender;

            string s = "";

            if (ctrl_key) s += "Ctrl+";
            if (alt_key) s += "Alt+";
            if (shift_key) s += "Shift+";
            if (windows_key)
                s += "Win+";
            if (menu_key)
                s += "Menu+";

            if (e.KeyCode != Keys.ShiftKey &&
                e.KeyCode != Keys.ControlKey &&
                e.KeyCode != Keys.Menu &&
                e.KeyCode != Keys.RMenu &&
                e.KeyCode != Keys.LWin &&
                e.KeyCode != Keys.RWin &&
                e.KeyCode != Keys.Apps)
                s += KeyToString(e.KeyCode);

            txtbox.Text = s;
            e.Handled = true;
        }

        private void txtKB_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void txtKB_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //Debug.WriteLine("KeyUp: "+e.KeyCode.ToString());
            shift_key = e.Shift;
            ctrl_key = e.Control;
            alt_key = e.Alt;

            if (e.KeyCode == Keys.LWin ||
                e.KeyCode == Keys.RWin)
                windows_key = false;

            if (e.KeyCode == Keys.Apps)
                menu_key = false;


            TextBoxTS txtbox = (TextBoxTS)sender;

            if (txtbox.Text.EndsWith("+"))
            {
                if (shift_key || ctrl_key || alt_key ||
                    windows_key || menu_key)
                {
                    string s = "";

                    if (ctrl_key) s += "Ctrl+";
                    if (alt_key) s += "Alt+";
                    if (shift_key) s += "Shift+";
                    if (windows_key)
                        s += "Win+";
                    if (menu_key)
                        s += "Menu+";

                    txtbox.Text = s;
                }
                else
                    txtbox.Text = "Not Assigned";
            }
        }

        private void clrbtnTXFilter_Changed(object sender, System.EventArgs e)
        {
            Display.DisplayFilterTXColor = clrbtnTXFilter.Color;
        }

        #region Lost Focus Event Handlers

        private void tbDSPAudRX1APFGain_LostFocus(object sender, EventArgs e)
        {
            tbDSPAudRX1APFGain.Value = tbDSPAudRX1APFGain.Value;
        }

        private void tbDSPAudRX1subAPFGain_LostFocus(object sender, EventArgs e)
        {
            tbDSPAudRX1subAPFGain.Value = tbDSPAudRX1subAPFGain.Value;
        }

        private void tbDSPAudRX2APFGain_LostFocus(object sender, EventArgs e)
        {
            tbDSPAudRX2APFGain.Value = tbDSPAudRX2APFGain.Value;
        }

        private void udGeneralCalFreq1_LostFocus(object sender, EventArgs e)
        {
            udGeneralCalFreq1.Value = udGeneralCalFreq1.Value;
        }

        private void udOptClickTuneOffsetDIGL_LostFocus(object sender, EventArgs e)
        {
            //      udOptClickTuneOffsetDIGL.Value = udOptClickTuneOffsetDIGL.Value;
        }

        private void udOptClickTuneOffsetDIGU_LostFocus(object sender, EventArgs e)
        {
            udOptClickTuneOffsetDIGU.Value = udOptClickTuneOffsetDIGU.Value;
        }

        private void udGeneralCalFreq3_LostFocus(object sender, EventArgs e)
        {
            udGeneralCalFreq3.Value = udGeneralCalFreq3.Value;
        }

        private void udGeneralCalLevel_LostFocus(object sender, EventArgs e)
        {
            udGeneralCalLevel.Value = udGeneralCalLevel.Value;
        }

        private void udGeneralCalFreq2_LostFocus(object sender, EventArgs e)
        {
            udGeneralCalFreq2.Value = udGeneralCalFreq2.Value;
        }

        private void udFilterDefaultLowCut_LostFocus(object sender, EventArgs e)
        {
            udFilterDefaultLowCut.Value = udFilterDefaultLowCut.Value;
        }

        private void udOptMaxFilterShift_LostFocus(object sender, EventArgs e)
        {
            udOptMaxFilterShift.Value = udOptMaxFilterShift.Value;
        }

        private void udOptMaxFilterWidth_LostFocus(object sender, EventArgs e)
        {
            udOptMaxFilterWidth.Value = udOptMaxFilterWidth.Value;
        }

        private void udAudioVACGainTX_LostFocus(object sender, EventArgs e)
        {
            udAudioVACGainTX.Value = udAudioVACGainTX.Value;
        }

        private void udAudioVACGainRX_LostFocus(object sender, EventArgs e)
        {
            udAudioVACGainRX.Value = udAudioVACGainRX.Value;
        }

        private void udAudioLatency2_LostFocus(object sender, EventArgs e)
        {
            udAudioLatency2.Value = udAudioLatency2.Value;
        }

        private void udDisplayScopeTime_LostFocus(object sender, EventArgs e)
        {
            udDisplayScopeTime.Value = udDisplayScopeTime.Value;
        }

        private void udDisplayMeterAvg_LostFocus(object sender, EventArgs e)
        {
            udDisplayMeterAvg.Value = udDisplayMeterAvg.Value;
        }

        private void udDisplayMultiTextHoldTime_LostFocus(object sender, EventArgs e)
        {
            udDisplayMultiTextHoldTime.Value = udDisplayMultiTextHoldTime.Value;
        }

        private void udDisplayMultiPeakHoldTime_LostFocus(object sender, EventArgs e)
        {
            udDisplayMultiPeakHoldTime.Value = udDisplayMultiPeakHoldTime.Value;
        }

        private void udDisplayWaterfallLowLevel_LostFocus(object sender, EventArgs e)
        {
            udDisplayWaterfallLowLevel.Value = udDisplayWaterfallLowLevel.Value;
        }

        private void udDisplayWaterfallHighLevel_LostFocus(object sender, EventArgs e)
        {
            udDisplayWaterfallHighLevel.Value = udDisplayWaterfallHighLevel.Value;
        }

        private void udDisplayCPUMeter_LostFocus(object sender, EventArgs e)
        {
            udDisplayCPUMeter.Value = udDisplayCPUMeter.Value;
        }

        private void udDisplayPeakText_LostFocus(object sender, EventArgs e)
        {
            udDisplayPeakText.Value = udDisplayPeakText.Value;
        }

        private void udDisplayMeterDelay_LostFocus(object sender, EventArgs e)
        {
            udDisplayMeterDelay.Value = udDisplayMeterDelay.Value;
        }

        private void udDisplayFPS_LostFocus(object sender, EventArgs e)
        {
            udDisplayFPS.Value = udDisplayFPS.Value;
        }

        private void udDisplayAVGTime_LostFocus(object sender, EventArgs e)
        {
            udDisplayAVGTime.Value = udDisplayAVGTime.Value;
        }

        private void udDisplayPhasePts_LostFocus(object sender, EventArgs e)
        {
            udDisplayPhasePts.Value = udDisplayPhasePts.Value;
        }

        private void udDisplayGridStep_LostFocus(object sender, EventArgs e)
        {
            udDisplayGridStep.Value = udDisplayGridStep.Value;
        }

        private void udTXGridMin_LostFocus(object sender, EventArgs e)
        {
            udTXGridMin.Value = udTXGridMin.Value;
        }

        private void udTXGridStep_LostFocus(object sender, EventArgs e)
        {
            udTXGridStep.Value = udTXGridStep.Value;
        }

        private void udDisplayGridMin_LostFocus(object sender, EventArgs e)
        {
            udDisplayGridMin.Value = udDisplayGridMin.Value;
        }

        private void udDSPNB_LostFocus(object sender, EventArgs e)
        {
            udDSPNB.Value = udDSPNB.Value;
        }

        private void udLMSNRgain_LostFocus(object sender, EventArgs e)
        {
            udLMSNRgain.Value = udLMSNRgain.Value;
        }

        private void udLMSNRdelay_LostFocus(object sender, EventArgs e)
        {
            udLMSNRdelay.Value = udLMSNRdelay.Value;
        }

        private void udLMSNRtaps_LostFocus(object sender, EventArgs e)
        {
            udLMSNRtaps.Value = udLMSNRtaps.Value;
        }

        private void udLMSANFgain_LostFocus(object sender, EventArgs e)
        {
            udLMSANFgain.Value = udLMSANFgain.Value;
        }

        private void udLMSANFdelay_LostFocus(object sender, EventArgs e)
        {
            udLMSANFdelay.Value = udLMSANFdelay.Value;
        }

        private void udLMSANFtaps_LostFocus(object sender, EventArgs e)
        {
            udLMSANFtaps.Value = udLMSANFtaps.Value;
        }

        private void udDSPNB2_LostFocus(object sender, EventArgs e)
        {
            //udDSPNB2.Value = udDSPNB2.Value;
        }

        private void udDSPCWPitch_LostFocus(object sender, EventArgs e)
        {
            udDSPCWPitch.Value = udDSPCWPitch.Value;
        }

        private void udCWKeyerWeight_LostFocus(object sender, EventArgs e)
        {
            udCWKeyerWeight.Value = udCWKeyerWeight.Value;
        }

        private void udCWBreakInDelay_LostFocus(object sender, EventArgs e)
        {
            udCWBreakInDelay.Value = udCWBreakInDelay.Value;
        }

        private void udDSPLevelerHangTime_LostFocus(object sender, EventArgs e)
        {
            udDSPLevelerHangTime.Value = udDSPLevelerHangTime.Value;
        }

        private void udDSPLevelerThreshold_LostFocus(object sender, EventArgs e)
        {
            udDSPLevelerThreshold.Value = udDSPLevelerThreshold.Value;
        }

        private void udDSPLevelerSlope_LostFocus(object sender, EventArgs e)
        {
            udDSPLevelerSlope.Value = udDSPLevelerSlope.Value;
        }

        private void udDSPLevelerDecay_LostFocus(object sender, EventArgs e)
        {
            udDSPLevelerDecay.Value = udDSPLevelerDecay.Value;
        }

        private void udDSPLevelerAttack_LostFocus(object sender, EventArgs e)
        {
            udDSPLevelerAttack.Value = udDSPLevelerAttack.Value;
        }

        private void udDSPALCHangTime_LostFocus(object sender, EventArgs e)
        {
            udDSPALCHangTime.Value = udDSPALCHangTime.Value;
        }

        private void udDSPALCThreshold_LostFocus(object sender, EventArgs e)
        {
            udDSPALCMaximumGain.Value = udDSPALCMaximumGain.Value;
        }

        private void udDSPALCSlope_LostFocus(object sender, EventArgs e)
        {
            udDSPALCSlope.Value = udDSPALCSlope.Value;
        }

        private void udDSPALCDecay_LostFocus(object sender, EventArgs e)
        {
            udDSPALCDecay.Value = udDSPALCDecay.Value;
        }

        private void udDSPALCAttack_LostFocus(object sender, EventArgs e)
        {
            udDSPALCAttack.Value = udDSPALCAttack.Value;
        }

        private void udDSPAGCHangTime_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCHangTime.Value = udDSPAGCHangTime.Value;
        }

        private void udDSPAGCMaxGaindB_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCMaxGaindB.Value = udDSPAGCMaxGaindB.Value;
        }

        private void udDSPAGCSlope_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCSlope.Value = udDSPAGCSlope.Value;
        }

        private void udDSPAGCDecay_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCDecay.Value = udDSPAGCDecay.Value;
        }

        private void udDSPAGCAttack_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCAttack.Value = udDSPAGCAttack.Value;
        }

        private void udDSPAGCFixedGaindB_LostFocus(object sender, EventArgs e)
        {
            udDSPAGCFixedGaindB.Value = udDSPAGCFixedGaindB.Value;
        }

        private void udTXAMCarrierLevel_LostFocus(object sender, EventArgs e)
        {
            udTXAMCarrierLevel.Value = udTXAMCarrierLevel.Value;
        }

        private void udTXAF_LostFocus(object sender, EventArgs e)
        {
            udTXAF.Value = udTXAF.Value;
        }

        private void udTXVOXHangTime_LostFocus(object sender, EventArgs e)
        {
            udTXVOXHangTime.Value = udTXVOXHangTime.Value;
        }

        private void udTXVOXThreshold_LostFocus(object sender, EventArgs e)
        {
            udTXVOXThreshold.Value = udTXVOXThreshold.Value;
        }

        private void udTXNoiseGate_LostFocus(object sender, EventArgs e)
        {
            udTXNoiseGate.Value = udTXNoiseGate.Value;
        }

        private void udTXTunePower_LostFocus(object sender, EventArgs e)
        {
            udTXTunePower.Value = udTXTunePower.Value;
        }

        private void udTXFilterLow_LostFocus(object sender, EventArgs e)
        {
            udTXFilterLow.Value = udTXFilterLow.Value;
        }

        private void udTXFilterHigh_LostFocus(object sender, EventArgs e)
        {
            udTXFilterHigh.Value = udTXFilterHigh.Value;
        }

        private void udMicGainMax_LostFocus(object sender, EventArgs e)
        {
            udMicGainMax.Value = udMicGainMax.Value;
        }

        private void udMicGainMin_LostFocus(object sender, EventArgs e)
        {
            udMicGainMin.Value = udMicGainMin.Value;
        }

        private void udPAGain10_LostFocus(object sender, EventArgs e)
        {
            udPAGain10.Value = udPAGain10.Value;
        }

        private void udPAGain12_LostFocus(object sender, EventArgs e)
        {
            udPAGain12.Value = udPAGain12.Value;
        }

        private void udPAGain15_LostFocus(object sender, EventArgs e)
        {
            udPAGain15.Value = udPAGain15.Value;
        }

        private void udPAGain17_LostFocus(object sender, EventArgs e)
        {
            udPAGain17.Value = udPAGain17.Value;
        }

        private void udPAGain20_LostFocus(object sender, EventArgs e)
        {
            udPAGain20.Value = udPAGain20.Value;
        }

        private void udPAGain30_LostFocus(object sender, EventArgs e)
        {
            udPAGain30.Value = udPAGain30.Value;
        }

        private void udPAGain40_LostFocus(object sender, EventArgs e)
        {
            udPAGain40.Value = udPAGain40.Value;
        }

        private void udPAGain60_LostFocus(object sender, EventArgs e)
        {
            udPAGain60.Value = udPAGain60.Value;
        }

        private void udPAGain80_LostFocus(object sender, EventArgs e)
        {
            udPAGain80.Value = udPAGain80.Value;
        }

        private void udPAGain160_LostFocus(object sender, EventArgs e)
        {
            udPAGain160.Value = udPAGain160.Value;
        }

        private void udPACalPower_LostFocus(object sender, EventArgs e)
        {
            udPACalPower.Value = udPACalPower.Value;
        }

        private void udDisplayLineWidth_LostFocus(object sender, EventArgs e)
        {
            udDisplayLineWidth.Value = udDisplayLineWidth.Value;
        }

        private void udTXLineWidth_LostFocus(object sender, EventArgs e)
        {
            udTXLineWidth.Value = udTXLineWidth.Value;
        }

        private void udTXGenScale_LostFocus(object sender, EventArgs e)
        {
            udTXGenScale.Value = udTXGenScale.Value;
        }

        private void udTXGenSweepRate_LostFocus(object sender, EventArgs e)
        {
            udTXGenSweepRate.Value = udTXGenSweepRate.Value;
        }

        private void udTXGenSweepHigh_LostFocus(object sender, EventArgs e)
        {
            udTXGenSweepHigh.Value = udTXGenSweepHigh.Value;
        }

        private void udTXGenSweepLow_LostFocus(object sender, EventArgs e)
        {
            udTXGenSweepLow.Value = udTXGenSweepLow.Value;
        }

        private void udTestIMDFreq2_LostFocus(object sender, EventArgs e)
        {
            udTestIMDFreq2.Value = udTestIMDFreq2.Value;
        }

        private void udTestIMDPower_LostFocus(object sender, EventArgs e)
        {
            udTestIMDPower.Value = udTestIMDPower.Value;
        }

        private void udTestIMDFreq1_LostFocus(object sender, EventArgs e)
        {
            udTestIMDFreq1.Value = udTestIMDFreq1.Value;
        }

        private void udImpulseNum_LostFocus(object sender, EventArgs e)
        {
            udImpulseNum.Value = udImpulseNum.Value;
        }

        #endregion

        private void chkShowFreqOffset_CheckedChanged(object sender, System.EventArgs e)
        {
            Display.ShowFreqOffset = chkShowFreqOffset.Checked;
        }

        private void clrbtnBandEdge_Changed(object sender, System.EventArgs e)
        {
            Display.BandEdgeColor = clrbtnBandEdge.Color;
        }

        private void clrbtnTXBandEdge_Changed(object sender, System.EventArgs e)
        {
            Display.TXBandEdgeColor = clrbtnTXBandEdge.Color;
        }

        private void comboMeterType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboMeterType.Text == "") return;
            switch (comboMeterType.Text)
            {
                case "Original":
                    console.CurrentMeterDisplayMode = MultiMeterDisplayMode.Original;
                    break;
                case "Edge":
                    console.CurrentMeterDisplayMode = MultiMeterDisplayMode.Edge;
                    break;
                case "Analog":
                    console.CurrentMeterDisplayMode = MultiMeterDisplayMode.Analog;
                    break;
            }
        }

        private void clrbtnMeterEdgeLow_Changed(object sender, System.EventArgs e)
        {
            console.EdgeLowColor = clrbtnMeterEdgeLow.Color;
        }

        private void clrbtnMeterEdgeHigh_Changed(object sender, System.EventArgs e)
        {
            console.EdgeHighColor = clrbtnMeterEdgeHigh.Color;
        }

        private void clrbtnMeterEdgeBackground_Changed(object sender, System.EventArgs e)
        {
            console.EdgeMeterBackgroundColor = Color.FromArgb(tbMeterEdgeBackgroundAlpha.Value, clrbtnMeterEdgeBackground.Color);
            // console.EdgeMeterBackgroundColor = clrbtnMeterEdgeBackground.Color;
        }

        private void clrbtnEdgeIndicator_Changed(object sender, System.EventArgs e)
        {
            console.EdgeAVGColor = clrbtnEdgeIndicator.Color;
        }

        private void clrbtnMeterDigText_Changed(object sender, System.EventArgs e)
        {
            console.MeterDigitalTextColor = clrbtnMeterDigText.Color;
        }

        private void clrbtnMeterDigBackground_Changed(object sender, System.EventArgs e)
        {
            console.MeterDigitalBackgroundColor = clrbtnMeterDigBackground.Color;
        }

        private void clrbtnSubRXFilter_Changed(object sender, System.EventArgs e)
        {
            Display.SubRXFilterColor = Color.FromArgb(tbMultiRXFilterAlpha.Value, clrbtnSubRXFilter.Color);
        }

        private void clrbtnSubRXZero_Changed(object sender, System.EventArgs e)
        {
            Display.SubRXZeroLine = clrbtnSubRXZero.Color;
        }

        private void chkCWKeyerMode_CheckedChanged(object sender, System.EventArgs e)
        {
            // if (console.NewProtocol)
            // {
            if (chkCWKeyerMode.Checked)
                NetworkIO.SetCWKeyerMode(1); // mode b
            else
                NetworkIO.SetCWKeyerMode(0); // mode a
            // }
            //else
            //{
            //    if (chkCWKeyerIambic.Checked)
            //    {
            //        if (chkCWKeyerMode.Checked)
            //        {
            //            JanusAudio.SetCWKeyerMode(2); // mode b
            //        }
            //        else
            //        {
            //            JanusAudio.SetCWKeyerMode(1); // mode a
            //        }
            //    }
            //    else
            //        JanusAudio.SetCWKeyerMode(0); // straight/bug mode
            //}
        }

        private void chkDisableToolTips_CheckedChanged(object sender, System.EventArgs e)
        {
            toolTip1.Active = !chkDisableToolTips.Checked;
            console.DisableToolTips = chkDisableToolTips.Checked;
        }

        private void comboColorPalette_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboColorPalette.Text == "original")
            {
                console.color_sheme = ColorSheme.original;
                clrbtnWaterfallLow.Visible = true;
                clrbtnWaterfallHigh.Visible = true;
                clrbtnWaterfallMid.Visible = true;
                lblDisplayWaterfallHighColor.Visible = true;
                lblDisplayWaterfallLowColor.Visible = true;
                lblDisplayWaterfallMidColor.Visible = true;
            }
            if (comboColorPalette.Text == "enhanced")
            {
                console.color_sheme = ColorSheme.enhanced;
                clrbtnWaterfallLow.Visible = true;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = true;
                lblDisplayWaterfallMidColor.Visible = false;
            }
            if (comboColorPalette.Text == "Spectran")
            {
                clrbtnWaterfallLow.Visible = false;
                console.color_sheme = ColorSheme.SPECTRAN;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = false;
                lblDisplayWaterfallMidColor.Visible = false;
            }
            if (comboColorPalette.Text == "BlackWhite")
            {
                console.color_sheme = ColorSheme.BLACKWHITE;
                clrbtnWaterfallLow.Visible = false;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = false;
                lblDisplayWaterfallMidColor.Visible = false;
            }
            if (comboColorPalette.Text == "LinLog")
            {
                console.color_sheme = ColorSheme.LinLog;
                clrbtnWaterfallLow.Visible = false;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = false;
                lblDisplayWaterfallMidColor.Visible = false;
            }
            if (comboColorPalette.Text == "LinRad")
            {
                console.color_sheme = ColorSheme.LinRad;
                clrbtnWaterfallLow.Visible = false;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = false;
                lblDisplayWaterfallMidColor.Visible = false;
            }
            if (comboColorPalette.Text == "LinAuto")
            {
                console.color_sheme = ColorSheme.LinAuto;
                clrbtnWaterfallLow.Visible = false;
                clrbtnWaterfallHigh.Visible = false;
                clrbtnWaterfallMid.Visible = false;
                lblDisplayWaterfallHighColor.Visible = false;
                lblDisplayWaterfallLowColor.Visible = false;
                lblDisplayWaterfallMidColor.Visible = false;
            }
        }

        private void comboRX2ColorPalette_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboRX2ColorPalette.Text == "original")
            {
                console.rx2_color_sheme = ColorSheme.original;
                clrbtnRX2WaterfallLow.Visible = true;
                clrbtnRX2WaterfallHigh.Visible = true;
                clrbtnRX2WaterfallMid.Visible = true;
                lblRX2DisplayWaterfallHighColor.Visible = true;
                lblRX2DisplayWaterfallLowColor.Visible = true;
                lblRX2DisplayWaterfallMidColor.Visible = true;
            }
            if (comboRX2ColorPalette.Text == "enhanced")
            {
                console.rx2_color_sheme = ColorSheme.enhanced;
                clrbtnRX2WaterfallLow.Visible = true;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = true;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
            if (comboRX2ColorPalette.Text == "Spectran")
            {
                console.rx2_color_sheme = ColorSheme.SPECTRAN;
                clrbtnRX2WaterfallLow.Visible = false;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = false;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
            if (comboRX2ColorPalette.Text == "BlackWhite")
            {
                console.rx2_color_sheme = ColorSheme.BLACKWHITE;
                clrbtnRX2WaterfallLow.Visible = false;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = false;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
            if (comboRX2ColorPalette.Text == "LinLog")
            {
                console.rx2_color_sheme = ColorSheme.LinLog;
                clrbtnRX2WaterfallLow.Visible = false;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = false;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
            if (comboRX2ColorPalette.Text == "LinRad")
            {
                console.rx2_color_sheme = ColorSheme.LinRad;
                clrbtnRX2WaterfallLow.Visible = false;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = false;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
            if (comboRX2ColorPalette.Text == "LinAuto")
            {
                console.rx2_color_sheme = ColorSheme.LinAuto;
                clrbtnRX2WaterfallLow.Visible = false;
                clrbtnRX2WaterfallHigh.Visible = false;
                clrbtnRX2WaterfallMid.Visible = false;
                lblRX2DisplayWaterfallHighColor.Visible = false;
                lblRX2DisplayWaterfallLowColor.Visible = false;
                lblRX2DisplayWaterfallMidColor.Visible = false;
            }
        }

        private void udDisplayWaterfallAvgTime_ValueChanged(object sender, System.EventArgs e)
        {
            double buffer_time = (double)console.BlockSize1 / (double)console.SampleRate1;
            int buffersToAvg = (int)((float)udDisplayWaterfallAvgTime.Value * 0.001 / buffer_time);
            buffersToAvg = Math.Max(2, buffersToAvg);
            Display.WaterfallAvgBlocks = buffersToAvg;
        }

        private void udDisplayWaterfallUpdatePeriod_ValueChanged(object sender, System.EventArgs e)
        {
            Display.WaterfallUpdatePeriod = (int)udDisplayWaterfallUpdatePeriod.Value;
        }

        private void udRX2DisplayWaterfallAvgTime_ValueChanged(object sender, System.EventArgs e)
        {
            double buffer_time = (double)console.BlockSize2 / (double)console.SampleRateRX2;
            int buffersToAvg = (int)((float)udRX2DisplayWaterfallAvgTime.Value * 0.001 / buffer_time);
            buffersToAvg = Math.Max(2, buffersToAvg);
            Display.RX2WaterfallAvgBlocks = buffersToAvg;
        }

        private void udRX2DisplayWaterfallUpdatePeriod_ValueChanged(object sender, System.EventArgs e)
        {
            Display.RX2WaterfallUpdatePeriod = (int)udRX2DisplayWaterfallUpdatePeriod.Value;
        }

        private void chkSnapClickTune_CheckedChanged(object sender, System.EventArgs e)
        {
            console.SnapToClickTuning = chkSnapClickTune.Checked;
        }

        private void chkClickTuneFilter_CheckedChanged(object sender, System.EventArgs e)
        {
            console.ClickTuneFilter = chkClickTuneFilter.Checked;
            Display.ClickTuneFilter = chkClickTuneFilter.Checked;
        }

        private void chkShowCTHLine_CheckedChanged(object sender, System.EventArgs e)
        {
            Display.ShowCTHLine = chkShowCTHLine.Checked;
        }

        private void radPACalAllBands_CheckedChanged(object sender, System.EventArgs e)
        {
            foreach (Control c in panelAutoPACalibrate.Controls)
            // foreach (Control c in grpPAGainByBand.Controls)
            {
                if (c.Name.StartsWith("chkPA") || c.Name.StartsWith("chkBy"))
                {
                    c.Visible = !radPACalAllBands.Checked;
                }
            }
        }

        private void chkZeroBeatRIT_CheckedChanged(object sender, System.EventArgs e)
        {
            console.ZeroBeatRIT = chkZeroBeatRIT.Checked;
        }

        private void chkPANewCal_CheckedChanged(object sender, System.EventArgs e)
        {
            bool b = chkPANewCal.Checked;

            console.NewPowerCal = b;

            lblPAGainByBand160.Visible = !b;
            lblPAGainByBand80.Visible = !b;
            lblPAGainByBand60.Visible = !b;
            lblPAGainByBand40.Visible = !b;
            lblPAGainByBand30.Visible = !b;
            lblPAGainByBand20.Visible = !b;
            lblPAGainByBand17.Visible = !b;
            lblPAGainByBand15.Visible = !b;
            lblPAGainByBand12.Visible = !b;
            lblPAGainByBand10.Visible = !b;

            udPAGain160.Visible = !b;
            udPAGain80.Visible = !b;
            udPAGain60.Visible = !b;
            udPAGain40.Visible = !b;
            udPAGain30.Visible = !b;
            udPAGain20.Visible = !b;
            udPAGain17.Visible = !b;
            udPAGain15.Visible = !b;
            udPAGain12.Visible = !b;
            udPAGain10.Visible = !b;
            udPAGain6.Visible = !b;

            // if (!radGenModelFLEX5000.Checked)
            {
                lblPACalTarget.Visible = !b;
                udPACalPower.Visible = !b;
                btnPAGainReset.Visible = !b;
            }
        }

        private void udMeterDigitalDelay_ValueChanged(object sender, System.EventArgs e)
        {
            console.MeterDigDelay = (int)udMeterDigitalDelay.Value;
        }

        private void chkMouseTuneStep_CheckedChanged(object sender, System.EventArgs e)
        {
            console.MouseTuneStep = chkMouseTuneStep.Checked;
        }

        private void chkWheelReverse_CheckedChanged(object sender, EventArgs e)
        {
            console.WheelReverse = chkWheelReverse.Checked;
        }

        public void UpdateCustomTitle()
        {
            txtGenCustomTitle_TextChanged(this, EventArgs.Empty);
        }

        private void txtGenCustomTitle_TextChanged(object sender, System.EventArgs e)
        {
            string title = console.Text;
            int index = title.IndexOf("   --   ");
            if (index >= 0) title = title.Substring(0, index);
            if (!string.IsNullOrEmpty(txtGenCustomTitle.Text))
                title += "   --   " + txtGenCustomTitle.Text;
            console.Text = title;
        }

        private void chkGenAllModeMicPTT_CheckedChanged(object sender, System.EventArgs e)
        {
            console.AllModeMicPTT = chkGenAllModeMicPTT.Checked;
        }

        private void chkKWAI_CheckedChanged(object sender, System.EventArgs e)
        {
            AllowFreqBroadcast = chkKWAI.Checked;
        }

        private void chkSplitOff_CheckedChanged(object sender, System.EventArgs e)
        {
            console.DisableSplitOnBandchange = chkSplitOff.Checked;
        }

        public bool RFE_PA_TR
        {
            get { return chkEnableRFEPATR.Checked; }
            set { chkEnableRFEPATR.Checked = value; }
        }

        private void chkEnableRFEPATR_CheckedChanged(object sender, System.EventArgs e)
        {
            console.RFE_PA_TR_enable = chkEnableRFEPATR.Checked;
        }

        private void chkVACAllowBypass_CheckedChanged(object sender, System.EventArgs e)
        {
            console.AllowVACBypass = chkVACAllowBypass.Checked;
        }

        private void chkSPACEAllowBypass_CheckedChanged(object sender, System.EventArgs e)
        {
            console.AllowSPACEBypass = chkSPACEAllowBypass.Checked;
        }

        private void chkMOXAllowBypass_CheckedChanged(object sender, System.EventArgs e)
        {
            console.AllowMOXBypass = chkMOXAllowBypass.Checked;
        }

        private void chkDSPTXMeterPeak_CheckedChanged(object sender, System.EventArgs e)
        {
            console.PeakTXMeter = chkDSPTXMeterPeak.Checked;
        }

        private void chkVACCombine_CheckedChanged(object sender, System.EventArgs e)
        {
            Audio.VACCombineInput = chkVACCombine.Checked;
        }

        private void chkCWAutoSwitchMode_CheckedChanged(object sender, System.EventArgs e)
        {
            console.CWAutoModeSwitch = chkCWAutoSwitchMode.Checked;
        }

        private void clrbtnGenBackground_Changed(object sender, System.EventArgs e)//k6jca 1/13/08
        {
            //console.GenBackgroundColor = clrbtnGenBackground.Color;
        }

        public MeterTXMode TuneMeterTXMode
        {
            set
            {
                switch (value)
                {
                    case MeterTXMode.FORWARD_POWER:
                        comboTXTUNMeter.Text = "Fwd Pwr";
                        break;
                    case MeterTXMode.REVERSE_POWER:
                        comboTXTUNMeter.Text = "Ref Pwr";
                        break;
                    case MeterTXMode.SWR:
                        comboTXTUNMeter.Text = "SWR";
                        break;
                    case MeterTXMode.OFF:
                        comboTXTUNMeter.Text = "Off";
                        break;
                }
            }
        }

        private void comboTXTUNMeter_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (comboTXTUNMeter.Text)
            {
                case "Fwd Pwr":
                    console.TuneTXMeterMode = MeterTXMode.FORWARD_POWER;
                    break;
                case "Ref Pwr":
                    console.TuneTXMeterMode = MeterTXMode.REVERSE_POWER;
                    break;
                case "SWR":
                    console.TuneTXMeterMode = MeterTXMode.SWR;
                    break;
                case "Off":
                    console.TuneTXMeterMode = MeterTXMode.OFF;
                    break;
            }
        }

        private void btnResetDB_Click(object sender, System.EventArgs e)
        {
            DialogResult dr = MessageBox.Show("This will close the program, make a copy of the current\n" +
                "database to the DB_Archive folder and reset the active database\n" +
                "the next time PowerSDR is launched.\n\n" +
                "Are you sure you want to reset the database?",
                "Reset Database?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.No) return;

            console.reset_db = true;
            console.Close();
        }

        private void chkDisplayMeterShowDecimal_CheckedChanged(object sender, System.EventArgs e)
        {
            console.MeterDetail = chkDisplayMeterShowDecimal.Checked;
        }

        private void chkRTTYOffsetEnableA_CheckedChanged(object sender, System.EventArgs e)
        {
            rtty_offset_enabled_a = chkRTTYOffsetEnableA.Checked;
        }

        private void chkRTTYOffsetEnableB_CheckedChanged(object sender, System.EventArgs e)
        {
            rtty_offset_enabled_b = chkRTTYOffsetEnableB.Checked;
        }

        private void udRTTYL_ValueChanged(object sender, System.EventArgs e)
        {
            rtty_offset_low = (int)udRTTYL.Value;
        }

        private void udRTTYU_ValueChanged(object sender, System.EventArgs e)
        {
            rtty_offset_high = (int)udRTTYU.Value;
        }

        private void chkRX2AutoMuteTX_CheckedChanged(object sender, System.EventArgs e)
        {
            //Audio.RX2AutoMuteTX = chkRX2AutoMuteTX.Checked;
            console.MuteRX2OnVFOATX = chkRX2AutoMuteTX.Checked;
        }

        private void chkAudioIQtoVAC_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkAudioEnableVAC.Checked)
            {
                // console.PowerOn = false;
                //Thread.Sleep(100);
                Audio.EnableVAC1(false);
            }

            Audio.VACOutputIQ = chkAudioIQtoVAC.Checked;

            if (power && chkAudioEnableVAC.Checked)
            {
                //console.PowerOn = true;
                //Thread.Sleep(100);
                Audio.VACEnabled = chkAudioEnableVAC.Checked;
            }

            chkAudioCorrectIQ.Enabled = chkAudioIQtoVAC.Checked;
            chkAudioRX2toVAC.Enabled = chkAudioIQtoVAC.Checked;

        }

        private void chkVAC2DirectIQ_CheckedChanged(object sender, System.EventArgs e)
        {
            bool power = console.PowerOn;
            if (power && chkVAC2Enable.Checked)
            {
                // console.PowerOn = false;
                // Thread.Sleep(100);
                Audio.EnableVAC2(false);
            }

            Audio.VAC2OutputIQ = chkVAC2DirectIQ.Checked;

            if (power && chkVAC2Enable.Checked)
            {
                //console.PowerOn = true;
                //Thread.Sleep(100);
                Audio.VAC2Enabled = chkVAC2Enable.Checked;
            }

            chkVAC2DirectIQCal.Enabled = chkVAC2DirectIQ.Checked;
        }

        private void chkAudioCorrectIQ_CheckChanged(object sender, System.EventArgs e)
        {
            Audio.VACCorrectIQ = chkAudioCorrectIQ.Checked;
        }

        private void chkVAC2IQCal_CheckChanged(object sender, System.EventArgs e)
        {
            Audio.VAC2CorrectIQ = chkVAC2DirectIQCal.Checked;
        }

        private void chkRX2AutoMuteRX1OnVFOBTX_CheckedChanged(object sender, System.EventArgs e)
        {
            console.MuteRX1OnVFOBTX = chkRX2AutoMuteRX1OnVFOBTX.Checked;
        }

        private void chkRX1BlankDisplayOnVFOBTX_CheckedChanged(object sender, System.EventArgs e)
        {
            console.BlankRX1OnVFOBTX = chkRX1BlankDisplayOnVFOBTX.Checked;
        }

        private void chkRX2BlankDisplayOnVFOATX_CheckedChanged(object sender, System.EventArgs e)
        {
            console.BlankRX2OnVFOATX = chkRX2BlankDisplayOnVFOATX.Checked;
        }

        private void chkTXExpert_CheckedChanged(object sender, System.EventArgs e)
        {
            grpTXProfileDef.Visible = chkTXExpert.Checked;
        }

        private void btnTXProfileDefImport_Click(object sender, System.EventArgs e)
        {
            if (lstTXProfileDef.SelectedIndex < 0) return;

            DialogResult result = MessageBox.Show("Include this Additional TX profile in your profiles list?",
                "Include?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            string name = lstTXProfileDef.Text;
            DataRow[] rows = DB.ds.Tables["TxProfileDef"].Select(
                "'" + name + "' = Name");

            if (rows.Length != 1)
            {
                MessageBox.Show("Database error reading TXProfileDef Table.",
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DataRow dr = null;
            if (comboTXProfileName.Items.Contains(name))
            {
                result = MessageBox.Show(
                    "Are you sure you want to overwrite the " + name + " TX Profile?",
                    "Overwrite Profile?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;

                foreach (DataRow d in DB.ds.Tables["TxProfile"].Rows)
                {
                    if ((string)d["Name"] == name)
                    {
                        dr = d;
                        break;
                    }
                }
            }
            else
            {
                dr = DB.ds.Tables["TxProfile"].NewRow();
                dr["Name"] = name;
            }

            for (int i = 0; i < dr.ItemArray.Length; i++)
                dr[i] = rows[0][i];

            if (!comboTXProfileName.Items.Contains(name))
            {
                DB.ds.Tables["TxProfile"].Rows.Add(dr);
                comboTXProfileName.Items.Add(name);
                comboTXProfileName.Text = name;
            }

            console.UpdateTXProfile(name);
        }

        //-W2PA Export a single TX Profile to send to someone else for importing.
        private void ExportCurrentTxProfile()
        {
            string fileName = current_profile;

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                fileName = fileName.Replace(c.ToString(), "_");  // Remove profile name chars that are invalid in filenames.
            }

            fileName = console.AppDataPath + fileName;

            int i = 1;
            string tempFN = fileName;
            while (File.Exists(tempFN + ".xml")) {
                tempFN = fileName + Convert.ToString(i);  // Get a slightly different file name if it already exists.
                i++;           
            }
            fileName = tempFN + ".xml";

            DataRow[] rows = DB.ds.Tables["TxProfile"].Select(
                "'" + current_profile + "' = Name");
            DataRow exportRow = null;
            if (rows.Length > 0)
            {
                exportRow = rows[0];
            }
            else
            {
                MessageBox.Show("Can not locate " + current_profile + ".",  // This should never happen.
                    "Profile error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DataSet exDS = DB.ds.Clone();
            DataTable pTable = pTable = DB.ds.Tables["TxProfile"].Clone();
            pTable.ImportRow(exportRow);
            exDS.Merge(pTable);

            try
            {
                exDS.WriteXml(fileName, XmlWriteMode.WriteSchema); // Writing with schema is necessary for import
            }
            catch
            {
                MessageBox.Show("Can not write " + fileName + ".",
                    "Export error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Profile" + current_profile + " has been saved in file " + fileName,
                    "Done",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
        }

        private void Setup_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control == true && e.Alt == true)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        chkPANewCal.Visible = true;
                        grpPAGainByBand.Visible = true;
                        break;
                    case Keys.O:
                        radGenModelORIONMKII.Visible = true;
                        //radGenModelANAN8000D.Visible = true;
                        break;
                    case Keys.P:
                        chkAutoPACalibrate.Visible = true;
                        break;
                    case Keys.V:
                        grpDisplayDriverEngine.Visible = true;
                        break;
                }
            }
        }

        private void chkDisplayPanFill_CheckedChanged(object sender, System.EventArgs e)
        {
            Display.PanFill = chkDisplayPanFill.Checked;
        }

        private void chkTXPanFill_CheckedChanged(object sender, System.EventArgs e)
        {
            Display.TXPanFill = chkTXPanFill.Checked;
        }

        //   private void udF3KFanTempThresh_ValueChanged(object sender, System.EventArgs e)
        //   {
        // console.F3KTempThresh = (float)udF3KFanTempThresh.Value;
        //  }

        //   private void chkGenTX1Delay_CheckedChanged(object sender, System.EventArgs e)
        //  {
        // if (!console.fwc_init || console.CurrentModel != Model.FLEX3000) return;
        // FWC.SetAmpTX1DelayEnable(chkGenTX1Delay.Checked);
        // udGenTX1Delay.Enabled = chkGenTX1Delay.Checked;
        //  }

        //  private void udGenTX1Delay_ValueChanged(object sender, System.EventArgs e)
        //  {
        //  if (!console.fwc_init || console.CurrentModel != Model.FLEX3000) return;
        // FWC.SetAmpTX1Delay((uint)udGenTX1Delay.Value);
        //  }

        private void comboAppSkin_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\OpenHPSDR\\Skins\\";

            if (Directory.Exists(path + comboAppSkin.Text))
                Skin.Restore(comboAppSkin.Text, path, console);
            console.CurrentSkin = comboAppSkin.Text;
            console.RadarColorUpdate = true;
        }

        private void btnSkinExport_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\OpenHPSDR\\Skins\\";

            if (Directory.Exists(path + comboAppSkin.Text))
                Skin.Save(comboAppSkin.Text, path, console);
        }

        private void chkCWDisableUI_CheckedChanged(object sender, EventArgs e)
        {
            console.DisableUIMOXChanges = chkCWDisableUI.Checked;
        }

        private void chkAudioRX2toVAC_CheckedChanged(object sender, EventArgs e)
        {
            Audio.VACOutputRX2 = chkAudioRX2toVAC.Checked;
        }

        private void chkVAC2UseRX2_CheckedChanged(object sender, EventArgs e)
        {
            console.VAC2RX2 = chkVAC2UseRX2.Checked;
        }

        private void chkGenOptionsShowATUPopup_CheckedChanged(object sender, System.EventArgs e)
        {
            //  if (console.flex3000ATUForm != null && !console.flex3000ATUForm.IsDisposed)
            //    console.flex3000ATUForm.ShowFeedbackPopup = chkGenOptionsShowATUPopup.Checked;
        }

        private void chkSpaceNavControlVFOs_CheckChanged(object sender, System.EventArgs e)
        {
            console.SpaceNavControlVFOs = (bool)chkSpaceNavControlVFOs.Checked;
        }

        private void chkSpaceNAvFlyPanadapter_CheckChanged(object sender, System.EventArgs e)
        {
            console.SpaceNavFlyPanadapter = (bool)chkSpaceNavFlyPanadapter.Checked;
        }

        private void tbRX1FilterAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnFilter_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbRX1FilterAlpha, tbRX1FilterAlpha.Value.ToString());
        }

        private void tbTXFilterAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnGridTXFilter_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXFilterAlpha, tbTXFilterAlpha.Value.ToString());
        }

        private void udTXNoiseGateAttenuate_ValueChanged(object sender, System.EventArgs e)
        {
            console.radio.GetDSPTX(0).TXSquelchAttenuate = (float)udTXNoiseGateAttenuate.Value;
        }

        private void tbMultiRXFilterAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnSubRXFilter_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbMultiRXFilterAlpha, tbMultiRXFilterAlpha.Value.ToString());
        }

        public static Color DisplayGrayLineColor = Color.FromArgb(70, Color.Black); // ke9ns add default value
        public void clrbtnGrayLine_Changed(object sender, EventArgs e)
        {
            DisplayGrayLineColor = Color.FromArgb(tbGrayLineAlpha.Value, clrbtnGrayLine.Color);

        }

        public void tbGrayLineAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnGrayLine_Changed(this, EventArgs.Empty);
        }

        private void chkWheelTuneVFOB_CheckedChanged(object sender, EventArgs e)
        {
            console.WheelTunesVFOB = chkWheelTuneVFOB.Checked;
        }

        private void btnExportDB_Click(object sender, EventArgs e)
        {
            string path = console.AppDataPath;
            path = path.Substring(0, path.LastIndexOf("\\"));
            // string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //  + "\\FlexRadio Systems\\PowerSDR mRX\\";
            // string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string datetime = DateTime.Now.ToShortDateString().Replace("/", "-") + "_" +
                    DateTime.Now.ToShortTimeString().Replace(":", ".");
            //  saveFileDialog1.FileName = desktop + "\\PowerSDR_database_export_" + datetime + ".xml";
            saveFileDialog1.FileName = path + "\\Thetis_database_export_" + datetime + ".xml";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            DB.ds.WriteXml(saveFileDialog1.FileName, XmlWriteMode.WriteSchema);
        }

        private void chkPennyLane_CheckedChanged(object sender, System.EventArgs e)
        {
            int bits = NetworkIO.GetC1Bits();
            if (!chkPennyLane.Checked)
            {
                bits &= 0xdf;  // 11011111
                //console.PennyLanePresent = false;
                radPenny10MHz.Checked = false;
                radPenny10MHz.Enabled = false;
                radPennyMic.Checked = false;
                radPennyMic.Enabled = false;
                rad12288MHzPenny.Checked = false;
                rad12288MHzPenny.Enabled = false;
                grpPennyExtCtrl.Enabled = false;
                chkPennyExtCtrl.Checked = false;
                chkPennyExtCtrl.Enabled = false;
            }
            else
            {
                bits |= 0x20;   // 00100000

                chkPennyPresent.Checked = false;
                radPenny10MHz.Enabled = true;
                radPennyMic.Enabled = true;
                rad12288MHzPenny.Enabled = true;

                chkPennyExtCtrl.Enabled = true;
                if (chkPennyExtCtrl.Checked)
                {
                    grpPennyExtCtrl.Enabled = true;
                }
                console.PennyPresent = false;
                //console.PennyLanePresent = true;
                chkGeneralRXOnly.Enabled = true;
                // chkGeneralRXOnly.Enabled = false;

            }
            console.PennyLanePresent = chkPennyLane.Checked;
            NetworkIO.SetC1Bits(bits);
            checkHPSDRDefaults(sender, e);
            NetworkIO.fwVersionsChecked = false;
        }

        private void chkPennyPresent_CheckedChanged(object sender, System.EventArgs e)
        {
            int bits = NetworkIO.GetC1Bits();
            if (!chkPennyPresent.Checked)
            {
                bits &= 0xdf;  // 11011111
                //console.PennyPresent = false;
                radPenny10MHz.Checked = false;
                radPenny10MHz.Enabled = false;
                radPennyMic.Checked = false;
                radPennyMic.Enabled = false;
                rad12288MHzPenny.Checked = false;
                rad12288MHzPenny.Enabled = false;
                grpPennyExtCtrl.Enabled = false;
                chkPennyExtCtrl.Checked = false;
                chkPennyExtCtrl.Enabled = false;

            }
            else
            {
                chkPennyLane.Checked = false;
                bits |= 0x20;   // 00100000

                radPenny10MHz.Enabled = true;
                radPennyMic.Enabled = true;
                rad12288MHzPenny.Enabled = true;

                chkPennyExtCtrl.Enabled = true;
                if (chkPennyExtCtrl.Checked)
                {
                    grpPennyExtCtrl.Enabled = true;
                }
                //console.PennyPresent = true;
                // chkGeneralRXOnly.Enabled = true;  
                console.PennyLanePresent = false;
            }
            console.PennyPresent = chkPennyPresent.Checked;
            NetworkIO.SetC1Bits(bits);
            checkHPSDRDefaults(sender, e);
            NetworkIO.fwVersionsChecked = false;
        }

        private void checkHPSDRDefaults(object sender, System.EventArgs e)
        {
            if (chkPennyPresent.Checked || chkPennyLane.Checked)
            {
                radPennyMic.Checked = true;
                radPennyMic_CheckedChanged(sender, e);
            }

            if ((chkPennyPresent.Checked || chkPennyLane.Checked) && !chkMercuryPresent.Checked)
            {
                rad12288MHzPenny.Checked = true;
                rad12288MHzPenny_CheckedChanged(sender, e);
            }
            else if (chkMercuryPresent.Checked && !chkPennyPresent.Checked && !chkPennyLane.Checked)
            {
                radMercury12288MHz.Checked = true;
                radMercury12288MHz_CheckedChanged(sender, e);
            }

            if (chkMercuryPresent.Checked && !radMercury10MHz.Checked && !radAtlas10MHz.Checked &&
                !radPenny10MHz.Checked)
            {
                radMercury10MHz.Checked = true;
                radMercury10MHz_CheckedChanged(sender, e);
                radMercury12288MHz.Checked = true;
                radMercury12288MHz_CheckedChanged(sender, e);
            }

            if (!chkPennyPresent.Checked && !chkPennyLane.Checked)
            {
                chkGeneralRXOnly.Checked = true;
                // chkGeneralRXOnly.Enabled = false;
            }
            else
            {
                // chkGeneralRXOnly.Enabled = true;
                chkGeneralRXOnly.Checked = false;
            }
            return;
        }

        private void chkMercuryPresent_CheckedChanged(object sender, System.EventArgs e)
        {
            int bits = NetworkIO.GetC1Bits();
            if (!chkMercuryPresent.Checked)
            {
                radMercury10MHz.Checked = false;
                radMercury12288MHz.Checked = false;
                radMercury10MHz.Enabled = false;
                radMercury12288MHz.Enabled = false;

                bits &= 0xbf;  // 1011 1111
            }
            else
            {
                radMercury10MHz.Enabled = true;
                radMercury12288MHz.Enabled = true;

                bits |= 0x40;   // 0100 0000
                NetworkIO.SetMercTxAtten(Convert.ToInt32(chkATTOnTX.Checked));
            }
            NetworkIO.SetC1Bits(bits);
            checkHPSDRDefaults(sender, e);
            console.MercuryPresent = chkMercuryPresent.Checked;
            NetworkIO.fwVersionsChecked = false;
        }

        private void chkAlexPresent_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkAlexPresent.Checked)
            {
                chkAlexAntCtrl.Enabled = true;
                chkAlexAntCtrl.Checked = true;
                //  if (!chkAlexAntCtrl.Checked)
                //  {
                //    grpAlexAntCtrl.Enabled = false;
                // }
                console.chkSR.Enabled = true;
                // if (console.RX2PreampPresent && !radGenModelANAN100D.Checked) console.comboRX2Preamp.Visible = false;
                if (chkApolloPresent.Checked) chkApolloPresent.Checked = false;
                //  if (radGenModelHermes.Checked || radGenModelANAN10.Checked || radGenModelANAN10E.Checked || radGenModelANAN100.Checked ||
                //  radGenModelANAN100D.Checked || radGenModelANAN200D.Checked) NetworkIO.SetHermesFilter(0);
            }
            else
            {
                chkAlexAntCtrl.Checked = false;
                chkAlexAntCtrl.Enabled = false;
                grpAlexAntCtrl.Enabled = false;
                console.chkSR.Enabled = false;
                //  if (console.RX2PreampPresent) console.comboRX2Preamp.Visible = true;
            }
            // if (radGenModelANAN100D.Checked) console.comboRX2Preamp.Visible = true;
            console.AlexPresent = chkAlexPresent.Checked;
            console.SetComboPreampForHPSDR();
            // if (chkHermesStepAttenuator.Checked) chkHermesStepAttenuator.Checked = true;
            udHermesStepAttenuatorData_ValueChanged(this, EventArgs.Empty);
        }


        private void chkExcaliburPresent_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkExcaliburPresent.Checked)
            {
                radAtlas10MHz.Checked = true;
                groupBox10MhzClock.Enabled = false;
            }
            else
            {
                groupBox10MhzClock.Enabled = true;
            }
        }

        private void radAtlas10MHz_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAtlas10MHz.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radMercury10MHz.Checked = false;
                radPenny10MHz.Checked = false;
                bits &= 0xf3;  // 1111 0011
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void radMercury10MHz_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radMercury10MHz.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radAtlas10MHz.Checked = false;
                radPenny10MHz.Checked = false;
                bits &= 0xf3;  // 1111 0011
                bits |= 0x8;  // 0000 1000
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void radPenny10MHz_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radPenny10MHz.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radAtlas10MHz.Checked = false;
                radMercury10MHz.Checked = false;
                bits &= 0xf3;  // 1111 0011
                bits |= 0x4;  // 0000 0100
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void rad12288MHzPenny_CheckedChanged(object sender, System.EventArgs e)
        {
            if (rad12288MHzPenny.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radMercury12288MHz.Checked = false;

                bits &= 0xef;  // 1110 1111				
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void radMercury12288MHz_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radMercury12288MHz.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                rad12288MHzPenny.Checked = false;

                bits |= 0x10;  // 0001 0000				
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void radPennyMic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radPennyMic.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radJanusMic.Checked = false;

                bits |= 0x80;  // 1000 0000				
                NetworkIO.SetC1Bits(bits);
            }
        }

        private void radJanusMic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radJanusMic.Checked)
            {
                int bits = NetworkIO.GetC1Bits();
                radPennyMic.Checked = false;

                bits &= 0x7f;  // 0111 1111
                NetworkIO.SetC1Bits(bits);
            }
        }


        public byte BandBBitMask = 0x70; // 4x3 split
        private void UpdateOCBits()
        {
            chkPenOCrcv160_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit160_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv80_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit80_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv60_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit60_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv40_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit40_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv30_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit30_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv20_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit20_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv17_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit17_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv15_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit15_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv12_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit12_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv10_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit10_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv6_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit6_CheckedChanged(this, EventArgs.Empty);
            chkPenOCrcv2_CheckedChanged(this, EventArgs.Empty);
            chkPenOCxmit2_CheckedChanged(this, EventArgs.Empty);
        }

        private void chkPenOCrcv160_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcv1601.Checked) val += 1 << 0;
            if (chkPenOCrcv1602.Checked) val += 1 << 1;
            if (chkPenOCrcv1603.Checked) val += 1 << 2;
            if (chkPenOCrcv1604.Checked) val += 1 << 3;
            if (chkPenOCrcv1605.Checked) val += 1 << 4;
            if (chkPenOCrcv1606.Checked) val += 1 << 5;
            if (chkPenOCrcv1607.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B160M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B160M, (byte)(val & BandBBitMask), false); // 000 xxxx 0x70  000 0xxx 0x78
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit160_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmit1601.Checked) val += 1 << 0;
            if (chkPenOCxmit1602.Checked) val += 1 << 1;
            if (chkPenOCxmit1603.Checked) val += 1 << 2;
            if (chkPenOCxmit1604.Checked) val += 1 << 3;
            if (chkPenOCxmit1605.Checked) val += 1 << 4;
            if (chkPenOCxmit1606.Checked) val += 1 << 5;
            if (chkPenOCxmit1607.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B160M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B160M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv80_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcv801.Checked) val += 1 << 0;
            if (chkPenOCrcv802.Checked) val += 1 << 1;
            if (chkPenOCrcv803.Checked) val += 1 << 2;
            if (chkPenOCrcv804.Checked) val += 1 << 3;
            if (chkPenOCrcv805.Checked) val += 1 << 4;
            if (chkPenOCrcv806.Checked) val += 1 << 5;
            if (chkPenOCrcv807.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B80M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B80M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit80_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmit801.Checked) val += 1 << 0;
            if (chkPenOCxmit802.Checked) val += 1 << 1;
            if (chkPenOCxmit803.Checked) val += 1 << 2;
            if (chkPenOCxmit804.Checked) val += 1 << 3;
            if (chkPenOCxmit805.Checked) val += 1 << 4;
            if (chkPenOCxmit806.Checked) val += 1 << 5;
            if (chkPenOCxmit807.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B80M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B80M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv60_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcv601.Checked) val += 1 << 0;
            if (chkPenOCrcv602.Checked) val += 1 << 1;
            if (chkPenOCrcv603.Checked) val += 1 << 2;
            if (chkPenOCrcv604.Checked) val += 1 << 3;
            if (chkPenOCrcv605.Checked) val += 1 << 4;
            if (chkPenOCrcv606.Checked) val += 1 << 5;
            if (chkPenOCrcv607.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B60M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B60M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit60_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmit601.Checked) val += 1 << 0;
            if (chkPenOCxmit602.Checked) val += 1 << 1;
            if (chkPenOCxmit603.Checked) val += 1 << 2;
            if (chkPenOCxmit604.Checked) val += 1 << 3;
            if (chkPenOCxmit605.Checked) val += 1 << 4;
            if (chkPenOCxmit606.Checked) val += 1 << 5;
            if (chkPenOCxmit607.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B60M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B60M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv40_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv401.Checked) val += 1 << 0;
            if (chkPenOCrcv402.Checked) val += 1 << 1;
            if (chkPenOCrcv403.Checked) val += 1 << 2;
            if (chkPenOCrcv404.Checked) val += 1 << 3;
            if (chkPenOCrcv405.Checked) val += 1 << 4;
            if (chkPenOCrcv406.Checked) val += 1 << 5;
            if (chkPenOCrcv407.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B40M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B40M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit40_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit401.Checked) val += 1 << 0;
            if (chkPenOCxmit402.Checked) val += 1 << 1;
            if (chkPenOCxmit403.Checked) val += 1 << 2;
            if (chkPenOCxmit404.Checked) val += 1 << 3;
            if (chkPenOCxmit405.Checked) val += 1 << 4;
            if (chkPenOCxmit406.Checked) val += 1 << 5;
            if (chkPenOCxmit407.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B40M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B40M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv30_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv301.Checked) val += 1 << 0;
            if (chkPenOCrcv302.Checked) val += 1 << 1;
            if (chkPenOCrcv303.Checked) val += 1 << 2;
            if (chkPenOCrcv304.Checked) val += 1 << 3;
            if (chkPenOCrcv305.Checked) val += 1 << 4;
            if (chkPenOCrcv306.Checked) val += 1 << 5;
            if (chkPenOCrcv307.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B30M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B30M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit30_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit301.Checked) val += 1 << 0;
            if (chkPenOCxmit302.Checked) val += 1 << 1;
            if (chkPenOCxmit303.Checked) val += 1 << 2;
            if (chkPenOCxmit304.Checked) val += 1 << 3;
            if (chkPenOCxmit305.Checked) val += 1 << 4;
            if (chkPenOCxmit306.Checked) val += 1 << 5;
            if (chkPenOCxmit307.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B30M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B30M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv20_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv201.Checked) val += 1 << 0;
            if (chkPenOCrcv202.Checked) val += 1 << 1;
            if (chkPenOCrcv203.Checked) val += 1 << 2;
            if (chkPenOCrcv204.Checked) val += 1 << 3;
            if (chkPenOCrcv205.Checked) val += 1 << 4;
            if (chkPenOCrcv206.Checked) val += 1 << 5;
            if (chkPenOCrcv207.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B20M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B20M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit20_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit201.Checked) val += 1 << 0;
            if (chkPenOCxmit202.Checked) val += 1 << 1;
            if (chkPenOCxmit203.Checked) val += 1 << 2;
            if (chkPenOCxmit204.Checked) val += 1 << 3;
            if (chkPenOCxmit205.Checked) val += 1 << 4;
            if (chkPenOCxmit206.Checked) val += 1 << 5;
            if (chkPenOCxmit207.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B20M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B20M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv17_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv171.Checked) val += 1 << 0;
            if (chkPenOCrcv172.Checked) val += 1 << 1;
            if (chkPenOCrcv173.Checked) val += 1 << 2;
            if (chkPenOCrcv174.Checked) val += 1 << 3;
            if (chkPenOCrcv175.Checked) val += 1 << 4;
            if (chkPenOCrcv176.Checked) val += 1 << 5;
            if (chkPenOCrcv177.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B17M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B17M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit17_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit171.Checked) val += 1 << 0;
            if (chkPenOCxmit172.Checked) val += 1 << 1;
            if (chkPenOCxmit173.Checked) val += 1 << 2;
            if (chkPenOCxmit174.Checked) val += 1 << 3;
            if (chkPenOCxmit175.Checked) val += 1 << 4;
            if (chkPenOCxmit176.Checked) val += 1 << 5;
            if (chkPenOCxmit177.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B17M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B17M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv15_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv151.Checked) val += 1 << 0;
            if (chkPenOCrcv152.Checked) val += 1 << 1;
            if (chkPenOCrcv153.Checked) val += 1 << 2;
            if (chkPenOCrcv154.Checked) val += 1 << 3;
            if (chkPenOCrcv155.Checked) val += 1 << 4;
            if (chkPenOCrcv156.Checked) val += 1 << 5;
            if (chkPenOCrcv157.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B15M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B15M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit15_CheckedChanged(object sender, System.EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmit151.Checked) val += 1 << 0;
            if (chkPenOCxmit152.Checked) val += 1 << 1;
            if (chkPenOCxmit153.Checked) val += 1 << 2;
            if (chkPenOCxmit154.Checked) val += 1 << 3;
            if (chkPenOCxmit155.Checked) val += 1 << 4;
            if (chkPenOCxmit156.Checked) val += 1 << 5;
            if (chkPenOCxmit157.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B15M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B15M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 

        }


        private void chkPenOCrcv12_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv121.Checked) val += 1 << 0;
            if (chkPenOCrcv122.Checked) val += 1 << 1;
            if (chkPenOCrcv123.Checked) val += 1 << 2;
            if (chkPenOCrcv124.Checked) val += 1 << 3;
            if (chkPenOCrcv125.Checked) val += 1 << 4;
            if (chkPenOCrcv126.Checked) val += 1 << 5;
            if (chkPenOCrcv127.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B12M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B12M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit12_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit121.Checked) val += 1 << 0;
            if (chkPenOCxmit122.Checked) val += 1 << 1;
            if (chkPenOCxmit123.Checked) val += 1 << 2;
            if (chkPenOCxmit124.Checked) val += 1 << 3;
            if (chkPenOCxmit125.Checked) val += 1 << 4;
            if (chkPenOCxmit126.Checked) val += 1 << 5;
            if (chkPenOCxmit127.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B12M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B12M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv10_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv101.Checked) val += 1 << 0;
            if (chkPenOCrcv102.Checked) val += 1 << 1;
            if (chkPenOCrcv103.Checked) val += 1 << 2;
            if (chkPenOCrcv104.Checked) val += 1 << 3;
            if (chkPenOCrcv105.Checked) val += 1 << 4;
            if (chkPenOCrcv106.Checked) val += 1 << 5;
            if (chkPenOCrcv107.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B10M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B10M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit10_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit101.Checked) val += 1 << 0;
            if (chkPenOCxmit102.Checked) val += 1 << 1;
            if (chkPenOCxmit103.Checked) val += 1 << 2;
            if (chkPenOCxmit104.Checked) val += 1 << 3;
            if (chkPenOCxmit105.Checked) val += 1 << 4;
            if (chkPenOCxmit106.Checked) val += 1 << 5;
            if (chkPenOCxmit107.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B10M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B10M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }


        private void chkPenOCrcv6_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv61.Checked) val += 1 << 0;
            if (chkPenOCrcv62.Checked) val += 1 << 1;
            if (chkPenOCrcv63.Checked) val += 1 << 2;
            if (chkPenOCrcv64.Checked) val += 1 << 3;
            if (chkPenOCrcv65.Checked) val += 1 << 4;
            if (chkPenOCrcv66.Checked) val += 1 << 5;
            if (chkPenOCrcv67.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B6M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B6M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit6_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit61.Checked) val += 1 << 0;
            if (chkPenOCxmit62.Checked) val += 1 << 1;
            if (chkPenOCxmit63.Checked) val += 1 << 2;
            if (chkPenOCxmit64.Checked) val += 1 << 3;
            if (chkPenOCxmit65.Checked) val += 1 << 4;
            if (chkPenOCxmit66.Checked) val += 1 << 5;
            if (chkPenOCxmit67.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B6M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B6M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcv2_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCrcv21.Checked) val += 1 << 0;
            if (chkPenOCrcv22.Checked) val += 1 << 1;
            if (chkPenOCrcv23.Checked) val += 1 << 2;
            if (chkPenOCrcv24.Checked) val += 1 << 3;
            if (chkPenOCrcv25.Checked) val += 1 << 4;
            if (chkPenOCrcv26.Checked) val += 1 << 5;
            if (chkPenOCrcv27.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B2M, (byte)val, false);
            Penny.getPenny().setBandBBitMask(Band.B2M, (byte)(val & BandBBitMask), false); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmit2_CheckedChanged(object sender, System.EventArgs e)
        {

            int val = 0;
            if (chkPenOCxmit21.Checked) val += 1 << 0;
            if (chkPenOCxmit22.Checked) val += 1 << 1;
            if (chkPenOCxmit23.Checked) val += 1 << 2;
            if (chkPenOCxmit24.Checked) val += 1 << 3;
            if (chkPenOCxmit25.Checked) val += 1 << 4;
            if (chkPenOCxmit26.Checked) val += 1 << 5;
            if (chkPenOCxmit27.Checked) val += 1 << 6;

            Penny.getPenny().setBandABitMask(Band.B2M, (byte)val, true);
            Penny.getPenny().setBandBBitMask(Band.B2M, (byte)(val & BandBBitMask), true); // 0000xxx
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPennyExtCtrl_CheckedChanged(object sender, System.EventArgs e)
        {
            grpPennyExtCtrl.Enabled = chkPennyExtCtrl.Checked;
            grpPennyExtCtrlVHF.Enabled = chkPennyExtCtrl.Checked;
            grpExtCtrlSWL.Enabled = chkPennyExtCtrl.Checked;
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;

#if false 		
		{
			grpExtRX.Enabled = chkExtEnable.Checked;
			grpExtTX.Enabled = chkExtEnable.Checked;
			console.ExtCtrlEnabled = chkExtEnable.Checked;
		}
#endif

        }

        private void chkAlexAntCtrl_CheckedChanged(object sender, System.EventArgs e)
        {
            grpAlexAntCtrl.Enabled = chkAlexAntCtrl.Checked;
            console.AlexAntCtrlEnabled = chkAlexAntCtrl.Checked;
            // if (radGenModelANAN10.Checked) panelAlexRXAntControl.Enabled = false;
            // else panelAlexRXAntControl.Enabled = true;
        }

        private void chkMercDither_CheckedChanged(object sender, System.EventArgs e)
        {
            int v = chkMercDither.Checked ? 1 : 0;
            NetworkIO.SetADCDither(v);
        }

        private void chkMercRandom_CheckedChanged(object sender, System.EventArgs e)
        {
            int v = chkMercRandom.Checked ? 1 : 0;
            NetworkIO.SetADCRandom(v);
        }

        private RadioButtonTS[][] AlexRxAntButtons = null;
        private RadioButtonTS[][] AlexTxAntButtons = null;
        private CheckBoxTS[][] AlexRxOnlyAntCheckBoxes = null;

        private void InitAlexAntTables()
        {

            AlexRxOnlyAntCheckBoxes = new[] { new CheckBoxTS[]  { chkAlex160R1, chkAlex160R2, chkAlex160XV }, 
													   	 new[] { chkAlex80R1, chkAlex80R2, chkAlex80XV }, 
													   	 new[] { chkAlex60R1, chkAlex60R2, chkAlex60XV }, 
													   	 new[] { chkAlex40R1, chkAlex40R2, chkAlex40XV }, 
													   	 new[] { chkAlex30R1, chkAlex30R2, chkAlex30XV }, 
													   	 new[] { chkAlex20R1, chkAlex20R2, chkAlex20XV }, 
													   	 new[] { chkAlex17R1, chkAlex17R2, chkAlex17XV }, 
													   	 new[] { chkAlex15R1, chkAlex15R2, chkAlex15XV }, 
													   	 new[] { chkAlex12R1, chkAlex12R2, chkAlex12XV }, 
													   	 new[] { chkAlex10R1, chkAlex10R2, chkAlex10XV }, 
													   	 new[] { chkAlex6R1, chkAlex6R2, chkAlex6XV } 
													 };


            AlexRxAntButtons = new[] { new RadioButtonTS[] { radAlexR1_160,  radAlexR2_160, radAlexR3_160 }, 
														 new[] { radAlexR1_80,  radAlexR2_80, radAlexR3_80 },
														 new[] { radAlexR1_60,  radAlexR2_60, radAlexR3_60 },
														 new[] { radAlexR1_40,  radAlexR2_40, radAlexR3_40 },
														 new[] { radAlexR1_30,  radAlexR2_30, radAlexR3_30 },
														 new[] { radAlexR1_20,  radAlexR2_20, radAlexR3_20 },
														 new[] { radAlexR1_17,  radAlexR2_17, radAlexR3_17 },
														 new[] { radAlexR1_15,  radAlexR2_15, radAlexR3_15 },
														 new[] { radAlexR1_12,  radAlexR2_12, radAlexR3_12 },
														 new[] { radAlexR1_10,  radAlexR2_10, radAlexR3_10 },
														 new[] { radAlexR1_6,  radAlexR2_6, radAlexR3_6 }
													 };

            AlexTxAntButtons = new[] { new RadioButtonTS[] { radAlexT1_160, radAlexT2_160, radAlexT3_160 }, 
														 new[] { radAlexT1_80,  radAlexT2_80, radAlexT3_80 },
														 new[] { radAlexT1_60,  radAlexT2_60, radAlexT3_60 },
														 new[] { radAlexT1_40,  radAlexT2_40, radAlexT3_40 },
														 new[] { radAlexT1_30,  radAlexT2_30, radAlexT3_30 },
														 new[] { radAlexT1_20,  radAlexT2_20, radAlexT3_20 },
														 new[] { radAlexT1_17,  radAlexT2_17, radAlexT3_17 },
														 new[] { radAlexT1_15,  radAlexT2_15, radAlexT3_15 },
														 new[] { radAlexT1_12,  radAlexT2_12, radAlexT3_12 },
														 new[] { radAlexT1_10,  radAlexT2_10, radAlexT3_10 },
														 new[] { radAlexT1_6,  radAlexT2_6, radAlexT3_6 }
													 };
        }

        public bool SetAlexAntEnabled(bool state)
        {
            bool orig_state = grpAlexAntCtrl.Enabled;
            grpAlexAntCtrl.Enabled = state;
            return orig_state;
        }

        private void radAlexR_160_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_160.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_160.Checked) radAlexT1_160.Checked = true;
                    radAlexT2_160.Enabled = false;
                }
                else radAlexT2_160.Enabled = true;
            }
            else radAlexT2_160.Enabled = true;

            if (radAlexR3_160.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_160.Checked) radAlexT1_160.Checked = true;
                    radAlexT3_160.Enabled = false;
                }
                else radAlexT3_160.Enabled = true;
            }
            else radAlexT3_160.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B160M, false);
        }

        private void radAlexR_80_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_80.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_80.Checked) radAlexT1_80.Checked = true;
                    radAlexT2_80.Enabled = false;
                }
                else radAlexT2_80.Enabled = true;
            }
            else radAlexT2_80.Enabled = true;

            if (radAlexR3_80.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_80.Checked) radAlexT1_80.Checked = true;
                    radAlexT3_80.Enabled = false;
                }
                else radAlexT3_80.Enabled = true;
            }
            else radAlexT3_80.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B80M, false);
        }

        private void radAlexR_60_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_60.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_60.Checked) radAlexT1_60.Checked = true;
                    radAlexT2_60.Enabled = false;
                }
                else radAlexT2_60.Enabled = true;
            }
            else radAlexT2_60.Enabled = true;

            if (radAlexR3_60.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_60.Checked) radAlexT1_60.Checked = true;
                    radAlexT3_60.Enabled = false;
                }
                else radAlexT3_60.Enabled = true;
            }
            else radAlexT3_60.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B60M, false);
        }

        private void radAlexR_40_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_40.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_40.Checked) radAlexT1_40.Checked = true;
                    radAlexT2_40.Enabled = false;
                }
                else radAlexT2_40.Enabled = true;
            }
            else radAlexT2_40.Enabled = true;

            if (radAlexR3_40.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_40.Checked) radAlexT1_40.Checked = true;
                    radAlexT3_40.Enabled = false;
                }
                else radAlexT3_40.Enabled = true;
            }
            else radAlexT3_40.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B40M, false);
        }

        private void radAlexR_30_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_30.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_30.Checked) radAlexT1_30.Checked = true;
                    radAlexT2_30.Enabled = false;
                }
                else radAlexT2_30.Enabled = true;
            }
            else radAlexT2_30.Enabled = true;

            if (radAlexR3_30.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_30.Checked) radAlexT1_30.Checked = true;
                    radAlexT3_30.Enabled = false;
                }
                else radAlexT3_30.Enabled = true;
            }
            else radAlexT3_30.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B30M, false);
        }

        private void radAlexR_20_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_20.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_20.Checked) radAlexT1_20.Checked = true;
                    radAlexT2_20.Enabled = false;
                }
                else radAlexT2_20.Enabled = true;
            }
            else radAlexT2_20.Enabled = true;

            if (radAlexR3_20.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_20.Checked) radAlexT1_20.Checked = true;
                    radAlexT3_20.Enabled = false;
                }
                else radAlexT3_20.Enabled = true;
            }
            else radAlexT3_20.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B20M, false);
        }

        private void radAlexR_17_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_17.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_17.Checked) radAlexT1_17.Checked = true;
                    radAlexT2_17.Enabled = false;
                }
                else radAlexT2_17.Enabled = true;
            }
            else radAlexT2_17.Enabled = true;

            if (radAlexR3_17.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_17.Checked) radAlexT1_17.Checked = true;
                    radAlexT3_17.Enabled = false;
                }
                else radAlexT3_17.Enabled = true;
            }
            else radAlexT3_17.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B17M, false);
        }

        private void radAlexR_15_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_15.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_15.Checked) radAlexT1_15.Checked = true;
                    radAlexT2_15.Enabled = false;
                }
                else radAlexT2_15.Enabled = true;
            }
            else radAlexT2_15.Enabled = true;

            if (radAlexR3_15.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_15.Checked) radAlexT1_15.Checked = true;
                    radAlexT3_15.Enabled = false;
                }
                else radAlexT3_15.Enabled = true;
            }
            else radAlexT3_15.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B15M, false);
        }

        private void radAlexR_12_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_12.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_12.Checked) radAlexT1_12.Checked = true;
                    radAlexT2_12.Enabled = false;
                }
                else radAlexT2_12.Enabled = true;
            }
            else radAlexT2_12.Enabled = true;

            if (radAlexR3_12.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_12.Checked) radAlexT1_12.Checked = true;
                    radAlexT3_12.Enabled = false;
                }
                else radAlexT3_12.Enabled = true;
            }
            else radAlexT3_12.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B12M, false);
        }

        private void radAlexR_10_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_10.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_10.Checked) radAlexT1_10.Checked = true;
                    radAlexT2_10.Enabled = false;
                }
                else radAlexT2_10.Enabled = true;
            }
            else radAlexT2_10.Enabled = true;

            if (radAlexR3_10.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_10.Checked) radAlexT1_10.Checked = true;
                    radAlexT3_10.Enabled = false;
                }
                else radAlexT3_10.Enabled = true;
            }
            else radAlexT3_10.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B10M, false);
        }

        private void radAlexR_6_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radAlexR2_6.Checked)
            {
                if (chkBlockTxAnt2.Checked)
                {
                    if (radAlexT2_6.Checked) radAlexT1_6.Checked = true;
                    radAlexT2_6.Enabled = false;
                }
                else radAlexT2_6.Enabled = true;
            }
            else radAlexT2_6.Enabled = true;

            if (radAlexR3_6.Checked)
            {
                if (chkBlockTxAnt3.Checked)
                {
                    if (radAlexT3_6.Checked) radAlexT1_6.Checked = true;
                    radAlexT3_6.Enabled = false;
                }
                else radAlexT3_6.Enabled = true;
            }
            else radAlexT3_6.Enabled = true;

            ProcessAlexAntRadioButton(sender, Band.B6M, false);
        }

        private void radAlexT_160_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B160M, true);
        }

        private void radAlexT_80_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B80M, true);
        }

        private void radAlexT_60_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B60M, true);
        }

        private void radAlexT_40_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B40M, true);
        }

        private void radAlexT_30_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B30M, true);
        }

        private void radAlexT_20_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B20M, true);
        }

        private void radAlexT_17_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B17M, true);
        }

        private void radAlexT_15_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B15M, true);
        }

        private void radAlexT_12_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B12M, true);
        }

        private void radAlexT_10_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B10M, true);
        }

        private void radAlexT_6_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntRadioButton(sender, Band.B6M, true);
        }

        private void chkAlex160R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B160M);
        }

        private void chkAlex80R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B80M);
        }

        private void chkAlex60R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B60M);
        }

        private void chkAlex40R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B40M);
        }

        private void chkAlex30R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B30M);
        }

        private void chkAlex20R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B20M);
        }

        private void chkAlex17R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B17M);
        }

        private void chkAlex15R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B15M);
        }

        private void chkAlex12R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B12M);
        }

        private void chkAlex10R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B10M);
        }

        private void chkAlex6R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B6M);
        }

        private void chkAlex2R_CheckedChanged(object sender, System.EventArgs e)
        {
            ProcessAlexAntCheckBox(sender, Band.B2M);
        }

        private void ProcessAlexAntCheckBox(object sender, Band band)
        {

            if (sender == null) return;
            if (sender.GetType() != typeof(CheckBoxTS)) return;

            int idx = (int)band - (int)Band.B160M;

            CheckBoxTS[] cboxes = AlexRxOnlyAntCheckBoxes[idx];


            int ant = 0;
            int i;

            // find which button was changed 
            for (i = 0; i < 2; i++)
            {
                if (sender.Equals(cboxes[i]))
                {
                    if (cboxes[i].Checked)
                    {
                        ant = i + 1;
                    }
                    break;
                }
            }

            if (i == 7)
            {
                System.Console.WriteLine("internal error - did not find sender");
                return;
            }

            if (ant != 0)
            {
                // turn off unselected antenna 
                for (i = 1; i <= 2; i++)
                {
                    if (ant != i)
                    {
                        cboxes[i - 1].Checked = false;
                    }
                }
            }

            ant = 0;
            for (i = 0; i < 3; i++)
            {
                if (cboxes[i].Checked)
                {
                    ant += i + 1;
                }
            }

            Alex.getAlex().setRxOnlyAnt(band, (byte)ant);

            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();

            console.AlexAntCtrlEnabled = true; // need side effect of prop set to push data down to C code 

            return;
        }


        private void ProcessAlexAntRadioButton(object sender, Band band, bool is_xmit)
        {
            if (sender == null) return;
            if (sender.GetType() != typeof(RadioButtonTS)) return;
            RadioButtonTS radBtnTS = (RadioButtonTS)sender;
            if (!radBtnTS.Checked) return;

            int idx = (int)band - (int)Band.B160M;

            RadioButtonTS[] buttons = is_xmit ? AlexTxAntButtons[idx] : AlexRxAntButtons[idx];

            int ant = 0;

            // find which button was changed 
            for (int i = 0; i < 3; i++)
            {
                if (!sender.Equals(buttons[i])) continue;
                ant = i + 1;
                break;
            }


            if (ant == 0)
            {
                int i = 0;
                foreach (RadioButtonTS b in buttons)
                {
                    if (b.Checked)
                    {
                        ant = i + 1;
                        break;
                    }
                    i++;
                }


                // System.Console.WriteLine("internal error - did not find sender");
                //  return;
            }

            if (is_xmit)
            {
                Alex.getAlex().setTxAnt(band, (byte)ant);
                pi_TxAnt = ant;                                 // Path_Illustrator support
            }
            else
            {
                Alex.getAlex().setRxAnt(band, (byte)ant);
                pi_RxAnt = ant;                                 // Path_Illustrator support
            }

            console.AlexAntCtrlEnabled = true; // need side effect of prop set to push data down to C code 

            return;
        }

        private void btnHPSDRFreqCalReset_Click(object sender, System.EventArgs e)
        {
            HPSDRFreqCorrectFactor = 1.0;
            //  JanusAudio.FreqCorrectionFactor = 1.0;
            /*  if (console != null)
              {
                  if (console.PowerOn)
                  {
                      console.PowerOn = false;
                      Thread.Sleep(100);
                      console.PowerOn = true;
                  }
              } */
        }

        private void tpHPSDR_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (console.PowerOn && radGenModelHPSDR.Checked)
            {
                lblMercury2FWVer.Visible = console.RX2PreampPresent;

                //try // this will take an exception in the conversion for Metis .. 
                //{
                //    lblOzyFX2.Text = Convert.ToUInt32(JanusAudio.getFX2FirmwareVersionString()).ToString("Ozy FX2: 0000 00 00");
                //}
                //catch (Exception)
                //{
                //    lblOzyFX2.Text = "Ozy FX2: n/a";
                //}

                // if (console.HPSDRisMetis)
                // {
                //lblOzyFWVer.Text = JanusAudio.getOzyFWVersion().ToString("Metis: 0\\.0");
                lblOzyFX2.Text = "";
                // }

                //lblMercuryFWVer.Text = console.MercuryPresent ? JanusAudio.getMercuryFWVersion().ToString("Mercury: 0\\.0") : "Mercury: n/a";
                // if (lblMercury2FWVer.Visible)
                // lblMercury2FWVer.Text = console.MercuryPresent ? JanusAudio.getMercury2FWVersion().ToString("Mercury2: 0\\.0") : "Mercury2: n/a";
                // if (console.PennyPresent || console.PennyLanePresent)
                // lblPenelopeFWVer.Text = JanusAudio.getPenelopeFWVersion().ToString("Penny[Lane]: 0\\.0");
                // else lblPenelopeFWVer.Text = "Penny[Lane]: n/a";

                /*               int rc = JanusAudio.GetHaveSync();
                               if (rc == 1)
                                   lblSyncData.Text = "FrameSync: Yes";
                               else
                                   lblSyncData.Text = "FrameSync: No"; 
               */
            }

            if (console.PowerOn && radGenModelHermes.Checked)
            {
                // byte[] ver_bytes = new byte[1];
                // JanusAudio.GetMetisCodeVersion(ver_bytes);
                lblOzyFX2.Text = NetworkIO.FWCodeVersion.ToString("Hermes: 0\\.0");//ver_bytes[0].ToString("Hermes: 0\\.0");
                lblOzyFWVer.Text = "";
                lblMercuryFWVer.Text = "";
                lblMercury2FWVer.Text = "";
                lblPenelopeFWVer.Text = "";
            }

            if (console.PowerOn && (radGenModelANAN10.Checked || radGenModelANAN10E.Checked || radGenModelANAN100.Checked || radGenModelANAN100D.Checked))
            {
                //byte[] ver_bytes = new byte[1];
                //JanusAudio.GetMetisCodeVersion(ver_bytes);
                lblOzyFX2.Text = NetworkIO.FWCodeVersion.ToString("ANAN: 0\\.0"); //ver_bytes[0].ToString("ANAN: 0\\.0");
                lblOzyFWVer.Text = "";
                lblMercuryFWVer.Text = "";
                lblMercury2FWVer.Text = "";
                lblPenelopeFWVer.Text = "";
            }

            if (console.PowerOn && radGenModelANAN200D.Checked)
            {
                //byte[] ver_bytes = new byte[1];
                //JanusAudio.GetMetisCodeVersion(ver_bytes);
                lblOzyFX2.Text = NetworkIO.FWCodeVersion.ToString("Orion: 0\\.0"); //ver_bytes[0].ToString("Orion: 0\\.0");
                lblOzyFWVer.Text = "";
                lblMercuryFWVer.Text = "";
                lblMercury2FWVer.Text = "";
                lblPenelopeFWVer.Text = "";
            }

        }

        private void tpGeneralHardware_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            //int metis_ip_addr = JanusAudio.GetMetisIPAddr();
            //lblMetisIP.Text = IPStringFromInt(metis_ip_addr);
            lblMetisIP.Text = NetworkIO.HpSdrHwIpAddress;
            // byte[] mac_bytes = new byte[6];
            // byte[] ver_bytes = new byte[1];
            // byte[] id_bytes = new byte[1];
            // JanusAudio.GetMetisMACAddr(mac_bytes);
            //lblMetisMAC.Text = BitConverter.ToString(mac_bytes).Replace("-", ":").ToLower();
            lblMetisMAC.Text = NetworkIO.HpSdrHwMacAddress;
            //JanusAudio.GetMetisCodeVersion(ver_bytes);
            // lblMetisCodeVersion.Text = BitConverter.ToString(ver_bytes);
            //lblMetisCodeVersion.Text = ver_bytes[0].ToString("0\\.0");
            lblMetisCodeVersion.Text = NetworkIO.FWCodeVersion.ToString("0\\.0");
            // JanusAudio.GetMetisBoardID(id_bytes);
            // lblMetisBoardID.Text = BitConverter.ToString(id_bytes);
            lblMetisBoardID.Text = NetworkIO.BoardID.ToString();
            return;
        }

        public void UpdateGeneraHardware()
        {
            tpGeneralHardware.Invalidate();
        }

        private void udMaxFreq_ValueChanged(object sender, System.EventArgs e)
        {
            console.MaxFreq = (double)udMaxFreq.Value;
        }

        private void udHPSDRFreqCorrectFactor_ValueChanged(object sender, System.EventArgs e)
        {
            // if (!console.initializing)
            NetworkIO.FreqCorrectionFactor = (double)udHPSDRFreqCorrectFactor.Value;
            // JanusAudio.freqCorrectionChanged();
        }

        private void chkHERCULES_CheckedChanged(object sender, EventArgs e)
        {
            switch (chkHERCULES.Checked)
            {
                case true:
                    foreach (Control c in grpPennyExtCtrl.Controls)
                    {
                        if (c.Name.StartsWith("chkPenOC"))
                        {
                            ((CheckBoxTS)c).Checked = false;
                        }
                    }

                    chkPenOCrcv1601.Checked = true;
                    chkPenOCxmit1601.Checked = true;
                    chkPenOCrcv802.Checked = true;
                    chkPenOCxmit802.Checked = true;
                    chkPenOCrcv601.Checked = true;
                    chkPenOCxmit601.Checked = true;
                    chkPenOCrcv602.Checked = true;
                    chkPenOCxmit602.Checked = true;
                    chkPenOCrcv403.Checked = true;
                    chkPenOCxmit403.Checked = true;
                    chkPenOCrcv301.Checked = true;
                    chkPenOCxmit301.Checked = true;
                    chkPenOCrcv303.Checked = true;
                    chkPenOCxmit303.Checked = true;
                    chkPenOCrcv202.Checked = true;
                    chkPenOCxmit202.Checked = true;
                    chkPenOCrcv203.Checked = true;
                    chkPenOCxmit203.Checked = true;
                    chkPenOCrcv171.Checked = true;
                    chkPenOCxmit171.Checked = true;
                    chkPenOCrcv172.Checked = true;
                    chkPenOCxmit172.Checked = true;
                    chkPenOCrcv173.Checked = true;
                    chkPenOCxmit173.Checked = true;
                    chkPenOCrcv154.Checked = true;
                    chkPenOCxmit154.Checked = true;
                    chkPenOCrcv121.Checked = true;
                    chkPenOCxmit121.Checked = true;
                    chkPenOCrcv124.Checked = true;
                    chkPenOCxmit124.Checked = true;
                    chkPenOCrcv102.Checked = true;
                    chkPenOCxmit102.Checked = true;
                    chkPenOCrcv104.Checked = true;
                    chkPenOCxmit104.Checked = true;
                    chkPenOCrcv61.Checked = true;
                    chkPenOCxmit61.Checked = true;
                    chkPenOCrcv62.Checked = true;
                    chkPenOCxmit62.Checked = true;
                    chkPenOCrcv64.Checked = true;
                    chkPenOCxmit64.Checked = true;
                    chkPenOCrcv66.Checked = true;
                    break;
                case false:
                    foreach (Control c in grpPennyExtCtrl.Controls)
                    {
                        if (c.Name.StartsWith("chkPenOC"))
                        {
                            ((CheckBoxTS)c).Checked = false;
                        }
                    }
                    break;
            }
        }

        private void btnPennyCtrlReset_Click(object sender, EventArgs e)
        {
            switch (chkHERCULES.Checked)
            {
                case false:
                    foreach (Control c in grpPennyExtCtrl.Controls)
                    {
                        if (c.Name.StartsWith("chkPenOC"))
                        {
                            ((CheckBoxTS)c).Checked = false;
                        }
                    }
                    break;
            }
        }

        private void btnPennyCtrlVHFReset_Click(object sender, EventArgs e)
        {
            foreach (Control c in grpPennyExtCtrlVHF.Controls)
            {
                if (c.Name.StartsWith("chkPenOC"))
                {
                    ((CheckBoxTS)c).Checked = false;
                }
            }
        }

        private void btnCtrlSWLReset_Click(object sender, EventArgs e)
        {
            foreach (Control c in grpExtCtrlSWL.Controls)
            {
                if (c.Name.StartsWith("chkOC"))
                {
                    ((CheckBoxTS)c).Checked = false;
                }
            }
        }

        private void comboFRSRegion_SelectedIndexChanged(object sender, EventArgs e) //w5wc
        {
            if (comboFRSRegion.Text == "") return;

            FRSRegion region = console.CurrentRegion;
            switch (comboFRSRegion.Text)
            {
                case "Australia":
                    region = FRSRegion.Australia;
                    console.Extended = false;
                    break;
                case "Europe":
                    region = FRSRegion.Europe;
                    console.Extended = false;
                    break;
                case "India":
                    region = FRSRegion.India;
                    console.Extended = false;
                    break;
                case "Italy":
                    region = FRSRegion.Italy_Plus;
                    console.Extended = false;
                    break;
                case "Spain":
                    region = FRSRegion.Spain;
                    console.Extended = false;
                    break;
                case "Japan":
                    region = FRSRegion.Japan;
                    console.Extended = false;
                    break;
                case "United Kingdom":
                    region = FRSRegion.UK;
                    //console.Extended = false;
                    // Display.Init();
                    break;
                case "United States":
                    region = FRSRegion.US;
                    // Display.Init();
                    break;
                case "Norway":
                    region = FRSRegion.Norway;
                    console.Extended = false;
                    break;
                case "Denmark":
                    region = FRSRegion.Denmark;
                    console.Extended = false;
                    break;
                case "Latvia":
                    region = FRSRegion.Latvia;
                    console.Extended = false;
                    break;
                case "Slovakia":
                    region = FRSRegion.Slovakia;
                    console.Extended = false;
                    break;
                case "Bulgaria":
                    region = FRSRegion.Bulgaria;
                    console.Extended = false;
                    break;
                case "Greece":
                    region = FRSRegion.Greece;
                    console.Extended = false;
                    break;
                case "Hungary":
                    region = FRSRegion.Hungary;
                    console.Extended = false;
                    break;
                case "Netherlands":
                    region = FRSRegion.Netherlands;
                    console.Extended = false;
                    break;
                case "France":
                    region = FRSRegion.France;
                    console.Extended = false;
                    break;
                case "Israel":
                    region = FRSRegion.Israel;
                    console.Extended = false;
                    break;
                case "Russia":
                    region = FRSRegion.Russia;
                    console.Extended = false;
                    break;
                case "Sweden":
                    region = FRSRegion.Sweden;
                    console.Extended = false;
                    break;
                case "Extended":
                    console.Extended = true;
                    break;
            }
            if (console.CurrentRegion != region)
                console.CurrentRegion = region;

            if (!console.initializing) DB.UpdateRegion(console.CurrentRegion);
            if (console.CurrentRegion == FRSRegion.UK)
            {
                console.band_60m_register = 11;
                console.Init60mChannels();
            }
            if (console.CurrentRegion == FRSRegion.US)
            {
                console.band_60m_register = 5;
                console.Init60mChannels();
            }
        }

        private void radMicIn_CheckedChanged(object sender, EventArgs e)
        {
            if (!radMicIn.Checked) return;
            console.LineIn = false;
            radLineIn.Checked = false;
            chk20dbMicBoost.Visible = true;
            chk20dbMicBoost.Enabled = true;
            lblLineInBoost.Visible = false;
            udLineInBoost.Visible = false;
            //console.SetMicGain();
        }

        private void radLineIn_CheckedChanged(object sender, EventArgs e)
        {
            if (!radLineIn.Checked) return;
            console.LineIn = true;
            radMicIn.Checked = false;
            chk20dbMicBoost.Visible = false;
            // chk20dbMicBoost.Checked = false;
            chk20dbMicBoost.Enabled = false;
            //           if (!console.HPSDRisMetis)
            //            {
            lblLineInBoost.Visible = true;
            udLineInBoost.Visible = true;
            //            }
            //            else
            //            {
            //                lblLineInBoost.Visible = false;
            //                udLineInBoost.Visible = false;
            //           }
            //console.SetMicGain();
        }


        private string IPStringFromInt(Int32 addr, StringBuilder sb)
        {
            sb.Length = 0;
            for (int i = 0; i < 4; i++)
            {
                int j = addr & 0xff;
                addr = addr >> 8;
                sb.Append(j);
                if (i != 3)
                {
                    sb.Append(".");
                }
            }
            return sb.ToString();
        }

        private string IPStringFromInt(Int32 addr)
        {
            return IPStringFromInt(addr, new StringBuilder());
        }

        private void udGenPTTOutDelay_ValueChanged(object sender, EventArgs e)
        {
            console.PTTOutDelay = (int)udGenPTTOutDelay.Value;
        }

        //        private void chkPTTOutDelay_CheckedChanged(object sender, EventArgs e)
        //     {
        // udGenPTTOutDelay.Enabled = chkPTTOutDelay.Checked;
        //      console.PTTODelayControl = chkPTTOutDelay.Checked;
        //  }

        private void udMoxDelay_ValueChanged(object sender, EventArgs e)
        {
            console.MoxDelay = (int)udMoxDelay.Value;
        }

        private void udCWKeyUpDelay_ValueChanged(object sender, EventArgs e)
        {
            console.KeyUpDelay = (int)udCWKeyUpDelay.Value;
            udCWKeyerSemiBreakInDelay_ValueChanged(this, EventArgs.Empty);
        }

        private void udRFDelay_ValueChanged(object sender, EventArgs e)
        {
            console.RFDelay = (int)udRFDelay.Value;
        }

        private void udTXDisplayCalOffset_ValueChanged(object sender, EventArgs e)
        {
            Display.TXDisplayCalOffset = (float)udTXDisplayCalOffset.Value;
        }

        private void udTXDisplayCalOffset_LostFocus(object sender, EventArgs e)
        {
            udTXDisplayCalOffset.Value = udTXDisplayCalOffset.Value;
        }

        private void udTwoToneLevel_ValueChanged(object sender, EventArgs e)
        {
            if (chkTestIMD.Checked)
            {
                Audio.SourceScale = Math.Pow(10.0, (double)udTwoToneLevel.Value / 20.0);
            }
        }

        private void chkSMeter_CheckedChanged(object sender, EventArgs e)
        {
            console.SMeter = chkSMeter.Checked;
        }

        private void clrbtnGridFine_Changed(object sender, EventArgs e)
        {
            Display.GridPenDark = Color.FromArgb(tbGridFineAlpha.Value, clrbtnGridFine.Color);
            //Display.GridPenDark = clrbtnGridFine.Color;
        }

        private void clrbtnTXVGridFine_Changed(object sender, EventArgs e)
        {
            Display.TXVGridPenFine = Color.FromArgb(tbTXVGridFineAlpha.Value, clrbtnTXVGridFine.Color);
            //Display.GridPenDark = clrbtnGridFine.Color;
        }

        private void tbBackgroundAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnBackground_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbBackgroundAlpha, tbBackgroundAlpha.Value.ToString());
        }

        private void tbTXBackgroundAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXBackground_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXBackgroundAlpha, tbTXBackgroundAlpha.Value.ToString());
        }

        private void tbGridCourseAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnGrid_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbGridCourseAlpha, tbGridCourseAlpha.Value.ToString());
        }

        private void tbTXVGridCourseAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXVGrid_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXVGridCourseAlpha, tbTXVGridCourseAlpha.Value.ToString());
        }

        private void tbGridFineAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnGridFine_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbGridFineAlpha, tbGridFineAlpha.Value.ToString());
        }

        private void tbTXVGridFineAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXVGridFine_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXVGridFineAlpha, tbTXVGridFineAlpha.Value.ToString());
        }

        private void clrbtnHGridColor_Changed(object sender, EventArgs e)
        {
            Display.HGridColor = Color.FromArgb(tbHGridColorAlpha.Value, clrbtnHGridColor.Color);
        }

        private void clrbtnTXHGridColor_Changed(object sender, EventArgs e)
        {
            Display.TXHGridColor = Color.FromArgb(tbTXHGridColorAlpha.Value, clrbtnTXHGridColor.Color);
        }

        private void tbHGridColorAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnHGridColor_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbHGridColorAlpha, tbHGridColorAlpha.Value.ToString());
        }

        private void tbMeterEdgeBackgroundAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnMeterEdgeBackground_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbMeterEdgeBackgroundAlpha, tbMeterEdgeBackgroundAlpha.Value.ToString());
        }

        private void tbTXHGridColorAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXHGridColor_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXHGridColorAlpha, tbTXHGridColorAlpha.Value.ToString());
        }

        private void tbTXZeroLineAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXZeroLine_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXZeroLineAlpha, tbTXZeroLineAlpha.Value.ToString());
        }

        private void tbTXTextAlpha_Scroll(object sender, EventArgs e)
        {
            clrbtnTXText_Changed(this, EventArgs.Empty);
            toolTip1.SetToolTip(tbTXTextAlpha, tbTXTextAlpha.Value.ToString());
        }

        /* private void chkTXCal_CheckedChanged(object sender, EventArgs e)
         {
             udTXDisplayCalOffset.Enabled = chkTXCal.Checked;
             Display.TXDisplayCalControl = chkTXCal.Checked;
             //udTXDisplayCalOffset_ValueChanged(this, EventArgs.Empty);
         }*/

        private void chkGridControl_CheckedChanged(object sender, EventArgs e)
        {
            Display.GridControl = chkGridControl.Checked;
        }

        private void chkTXGridControl_CheckedChanged(object sender, EventArgs e)
        {
            Display.TXGridControl = chkTXGridControl.Checked;
        }

        private void radSpaceBarLastBtn_CheckedChanged(object sender, EventArgs e)
        {
            this.console.SpaceBarLastBtn = radSpaceBarLastBtn.Checked;
        }

        private void radSpaceBarPTT_CheckedChanged(object sender, EventArgs e)
        {
            this.console.SpaceBarPTT = radSpaceBarPTT.Checked;
        }

        private void radSpaceBarVOX_CheckedChanged(object sender, EventArgs e)
        {
            this.console.SpaceBarVOX = radSpaceBarVOX.Checked;
        }

        private void radSpaceBarMicMute_CheckedChanged(object sender, EventArgs e)
        {
            this.console.SpaceBarMicMute = radSpaceBarMicMute.Checked;
        }

        private void chkPenOCrcvVHF0_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF01.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF02.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF03.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF04.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF05.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF06.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF07.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF0, (byte)(val & 0x70), false); // 0000xxx
            Penny.getPenny().setBandABitMask(Band.VHF0, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF1_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF11.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF12.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF13.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF14.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF15.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF16.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF17.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF1, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF1, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF2_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF21.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF22.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF23.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF24.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF25.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF26.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF27.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF2, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF2, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF3_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF31.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF32.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF33.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF34.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF35.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF36.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF37.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF3, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF3, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF4_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF41.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF42.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF43.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF44.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF45.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF46.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF47.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF4, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF4, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF5_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF51.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF52.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF53.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF54.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF55.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF56.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF57.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF5, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF5, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF6_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF61.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF62.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF63.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF64.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF65.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF66.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF67.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF6, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF6, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF7_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF71.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF72.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF73.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF74.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF75.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF76.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF77.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF7, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF7, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCrcvVHF8_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF81.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF82.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF83.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF84.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF85.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF86.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF87.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF8, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF8, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkPenOCrcvVHF9_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF91.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF92.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF93.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF94.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF95.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF96.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF97.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF9, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF9, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkPenOCrcvVHF10_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF101.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF102.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF103.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF104.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF105.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF106.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF107.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF10, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF10, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkPenOCrcvVHF11_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCrcvVHF111.Checked) val += 1 << 0;
            if (chkPenOCrcvVHF112.Checked) val += 1 << 1;
            if (chkPenOCrcvVHF113.Checked) val += 1 << 2;
            if (chkPenOCrcvVHF114.Checked) val += 1 << 3;
            if (chkPenOCrcvVHF115.Checked) val += 1 << 4;
            if (chkPenOCrcvVHF116.Checked) val += 1 << 5;
            if (chkPenOCrcvVHF117.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF11, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.VHF11, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkPenOCxmitVHF0_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF01.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF02.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF03.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF04.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF05.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF06.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF07.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF0, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF0, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF1_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF11.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF12.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF13.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF14.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF15.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF16.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF17.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF1, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF1, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF2_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF21.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF22.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF23.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF24.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF25.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF26.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF27.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF2, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF2, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF3_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF31.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF32.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF33.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF34.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF35.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF36.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF37.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF3, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF3, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF4_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF41.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF42.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF43.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF44.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF45.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF46.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF47.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF4, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF4, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF5_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF51.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF52.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF53.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF54.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF55.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF56.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF57.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF5, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF5, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF6_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF61.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF62.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF63.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF64.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF65.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF66.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF67.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF6, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF6, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF7_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF71.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF72.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF73.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF74.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF75.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF76.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF77.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF7, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF7, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF8_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF81.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF82.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF83.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF84.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF85.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF86.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF87.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF8, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF8, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF9_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF91.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF92.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF93.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF94.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF95.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF96.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF97.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF9, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF9, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF10_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF101.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF102.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF103.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF104.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF105.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF106.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF107.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF10, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF10, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkPenOCxmitVHF11_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkPenOCxmitVHF111.Checked) val += 1 << 0;
            if (chkPenOCxmitVHF112.Checked) val += 1 << 1;
            if (chkPenOCxmitVHF113.Checked) val += 1 << 2;
            if (chkPenOCxmitVHF114.Checked) val += 1 << 3;
            if (chkPenOCxmitVHF115.Checked) val += 1 << 4;
            if (chkPenOCxmitVHF116.Checked) val += 1 << 5;
            if (chkPenOCxmitVHF117.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.VHF11, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.VHF11, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }
        
        private void chkOCrcvLMW_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcvLMW1.Checked) val += 1 << 0;
            if (chkOCrcvLMW2.Checked) val += 1 << 1;
            if (chkOCrcvLMW3.Checked) val += 1 << 2;
            if (chkOCrcvLMW4.Checked) val += 1 << 3;
            if (chkOCrcvLMW5.Checked) val += 1 << 4;
            if (chkOCrcvLMW6.Checked) val += 1 << 5;
            if (chkOCrcvLMW7.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.BLMF, (byte)(val & 0x70), false); // 0000xxx
            Penny.getPenny().setBandABitMask(Band.BLMF, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv120_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv1201.Checked) val += 1 << 0;
            if (chkOCrcv1202.Checked) val += 1 << 1;
            if (chkOCrcv1203.Checked) val += 1 << 2;
            if (chkOCrcv1204.Checked) val += 1 << 3;
            if (chkOCrcv1205.Checked) val += 1 << 4;
            if (chkOCrcv1206.Checked) val += 1 << 5;
            if (chkOCrcv1207.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B120M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B120M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv90_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv901.Checked) val += 1 << 0;
            if (chkOCrcv902.Checked) val += 1 << 1;
            if (chkOCrcv903.Checked) val += 1 << 2;
            if (chkOCrcv904.Checked) val += 1 << 3;
            if (chkOCrcv905.Checked) val += 1 << 4;
            if (chkOCrcv906.Checked) val += 1 << 5;
            if (chkOCrcv907.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B90M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B90M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv61_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv611.Checked) val += 1 << 0;
            if (chkOCrcv612.Checked) val += 1 << 1;
            if (chkOCrcv613.Checked) val += 1 << 2;
            if (chkOCrcv614.Checked) val += 1 << 3;
            if (chkOCrcv615.Checked) val += 1 << 4;
            if (chkOCrcv616.Checked) val += 1 << 5;
            if (chkOCrcv617.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B61M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B61M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv49_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv491.Checked) val += 1 << 0;
            if (chkOCrcv492.Checked) val += 1 << 1;
            if (chkOCrcv493.Checked) val += 1 << 2;
            if (chkOCrcv494.Checked) val += 1 << 3;
            if (chkOCrcv495.Checked) val += 1 << 4;
            if (chkOCrcv496.Checked) val += 1 << 5;
            if (chkOCrcv497.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B49M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B49M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv41_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv411.Checked) val += 1 << 0;
            if (chkOCrcv412.Checked) val += 1 << 1;
            if (chkOCrcv413.Checked) val += 1 << 2;
            if (chkOCrcv414.Checked) val += 1 << 3;
            if (chkOCrcv415.Checked) val += 1 << 4;
            if (chkOCrcv416.Checked) val += 1 << 5;
            if (chkOCrcv417.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B41M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B41M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv31_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv311.Checked) val += 1 << 0;
            if (chkOCrcv312.Checked) val += 1 << 1;
            if (chkOCrcv313.Checked) val += 1 << 2;
            if (chkOCrcv314.Checked) val += 1 << 3;
            if (chkOCrcv315.Checked) val += 1 << 4;
            if (chkOCrcv316.Checked) val += 1 << 5;
            if (chkOCrcv317.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B31M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B31M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv25_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv251.Checked) val += 1 << 0;
            if (chkOCrcv252.Checked) val += 1 << 1;
            if (chkOCrcv253.Checked) val += 1 << 2;
            if (chkOCrcv254.Checked) val += 1 << 3;
            if (chkOCrcv255.Checked) val += 1 << 4;
            if (chkOCrcv256.Checked) val += 1 << 5;
            if (chkOCrcv257.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B25M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B25M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv22_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv221.Checked) val += 1 << 0;
            if (chkOCrcv222.Checked) val += 1 << 1;
            if (chkOCrcv223.Checked) val += 1 << 2;
            if (chkOCrcv224.Checked) val += 1 << 3;
            if (chkOCrcv225.Checked) val += 1 << 4;
            if (chkOCrcv226.Checked) val += 1 << 5;
            if (chkOCrcv227.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B22M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B22M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkOCrcv19_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv191.Checked) val += 1 << 0;
            if (chkOCrcv192.Checked) val += 1 << 1;
            if (chkOCrcv193.Checked) val += 1 << 2;
            if (chkOCrcv194.Checked) val += 1 << 3;
            if (chkOCrcv195.Checked) val += 1 << 4;
            if (chkOCrcv196.Checked) val += 1 << 5;
            if (chkOCrcv197.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B19M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B19M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkOCrcv16_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv161.Checked) val += 1 << 0;
            if (chkOCrcv162.Checked) val += 1 << 1;
            if (chkOCrcv163.Checked) val += 1 << 2;
            if (chkOCrcv164.Checked) val += 1 << 3;
            if (chkOCrcv165.Checked) val += 1 << 4;
            if (chkOCrcv166.Checked) val += 1 << 5;
            if (chkOCrcv167.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B16M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B16M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkOCrcv14_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv141.Checked) val += 1 << 0;
            if (chkOCrcv142.Checked) val += 1 << 1;
            if (chkOCrcv143.Checked) val += 1 << 2;
            if (chkOCrcv144.Checked) val += 1 << 3;
            if (chkOCrcv145.Checked) val += 1 << 4;
            if (chkOCrcv146.Checked) val += 1 << 5;
            if (chkOCrcv147.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B14M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B14M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }

        private void chkOCrcv13_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv131.Checked) val += 1 << 0;
            if (chkOCrcv132.Checked) val += 1 << 1;
            if (chkOCrcv133.Checked) val += 1 << 2;
            if (chkOCrcv134.Checked) val += 1 << 3;
            if (chkOCrcv135.Checked) val += 1 << 4;
            if (chkOCrcv136.Checked) val += 1 << 5;
            if (chkOCrcv137.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B13M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B13M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code  
        }

        private void chkOCrcv11_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCrcv111.Checked) val += 1 << 0;
            if (chkOCrcv112.Checked) val += 1 << 1;
            if (chkOCrcv113.Checked) val += 1 << 2;
            if (chkOCrcv114.Checked) val += 1 << 3;
            if (chkOCrcv115.Checked) val += 1 << 4;
            if (chkOCrcv116.Checked) val += 1 << 5;
            if (chkOCrcv117.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B11M, (byte)(val & 0x70), false);
            Penny.getPenny().setBandABitMask(Band.B11M, (byte)val, false);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code 
        }
        
        private void chkOCxmitLMW_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmitLMW1.Checked) val += 1 << 0;
            if (chkOCxmitLMW2.Checked) val += 1 << 1;
            if (chkOCxmitLMW3.Checked) val += 1 << 2;
            if (chkOCxmitLMW4.Checked) val += 1 << 3;
            if (chkOCxmitLMW5.Checked) val += 1 << 4;
            if (chkOCxmitLMW6.Checked) val += 1 << 5;
            if (chkOCxmitLMW7.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.BLMF, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.BLMF, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit120_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit1201.Checked) val += 1 << 0;
            if (chkOCxmit1202.Checked) val += 1 << 1;
            if (chkOCxmit1203.Checked) val += 1 << 2;
            if (chkOCxmit1204.Checked) val += 1 << 3;
            if (chkOCxmit1205.Checked) val += 1 << 4;
            if (chkOCxmit1206.Checked) val += 1 << 5;
            if (chkOCxmit1207.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B120M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B120M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit90_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit901.Checked) val += 1 << 0;
            if (chkOCxmit902.Checked) val += 1 << 1;
            if (chkOCxmit903.Checked) val += 1 << 2;
            if (chkOCxmit904.Checked) val += 1 << 3;
            if (chkOCxmit905.Checked) val += 1 << 4;
            if (chkOCxmit906.Checked) val += 1 << 5;
            if (chkOCxmit907.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B90M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B90M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit61_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit611.Checked) val += 1 << 0;
            if (chkOCxmit612.Checked) val += 1 << 1;
            if (chkOCxmit613.Checked) val += 1 << 2;
            if (chkOCxmit614.Checked) val += 1 << 3;
            if (chkOCxmit615.Checked) val += 1 << 4;
            if (chkOCxmit616.Checked) val += 1 << 5;
            if (chkOCxmit617.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B61M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B61M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit49_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit491.Checked) val += 1 << 0;
            if (chkOCxmit492.Checked) val += 1 << 1;
            if (chkOCxmit493.Checked) val += 1 << 2;
            if (chkOCxmit494.Checked) val += 1 << 3;
            if (chkOCxmit495.Checked) val += 1 << 4;
            if (chkOCxmit496.Checked) val += 1 << 5;
            if (chkOCxmit497.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B49M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B49M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit41_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit411.Checked) val += 1 << 0;
            if (chkOCxmit412.Checked) val += 1 << 1;
            if (chkOCxmit413.Checked) val += 1 << 2;
            if (chkOCxmit414.Checked) val += 1 << 3;
            if (chkOCxmit415.Checked) val += 1 << 4;
            if (chkOCxmit416.Checked) val += 1 << 5;
            if (chkOCxmit417.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B41M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B41M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit31_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit311.Checked) val += 1 << 0;
            if (chkOCxmit312.Checked) val += 1 << 1;
            if (chkOCxmit313.Checked) val += 1 << 2;
            if (chkOCxmit314.Checked) val += 1 << 3;
            if (chkOCxmit315.Checked) val += 1 << 4;
            if (chkOCxmit316.Checked) val += 1 << 5;
            if (chkOCxmit317.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B31M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B31M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit25_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit251.Checked) val += 1 << 0;
            if (chkOCxmit252.Checked) val += 1 << 1;
            if (chkOCxmit253.Checked) val += 1 << 2;
            if (chkOCxmit254.Checked) val += 1 << 3;
            if (chkOCxmit255.Checked) val += 1 << 4;
            if (chkOCxmit256.Checked) val += 1 << 5;
            if (chkOCxmit257.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B25M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B25M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit22_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit221.Checked) val += 1 << 0;
            if (chkOCxmit222.Checked) val += 1 << 1;
            if (chkOCxmit223.Checked) val += 1 << 2;
            if (chkOCxmit224.Checked) val += 1 << 3;
            if (chkOCxmit225.Checked) val += 1 << 4;
            if (chkOCxmit226.Checked) val += 1 << 5;
            if (chkOCxmit227.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B22M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B22M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit19_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit191.Checked) val += 1 << 0;
            if (chkOCxmit192.Checked) val += 1 << 1;
            if (chkOCxmit193.Checked) val += 1 << 2;
            if (chkOCxmit194.Checked) val += 1 << 3;
            if (chkOCxmit195.Checked) val += 1 << 4;
            if (chkOCxmit196.Checked) val += 1 << 5;
            if (chkOCxmit197.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B19M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B19M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit16_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit161.Checked) val += 1 << 0;
            if (chkOCxmit162.Checked) val += 1 << 1;
            if (chkOCxmit163.Checked) val += 1 << 2;
            if (chkOCxmit164.Checked) val += 1 << 3;
            if (chkOCxmit165.Checked) val += 1 << 4;
            if (chkOCxmit166.Checked) val += 1 << 5;
            if (chkOCxmit167.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B16M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B16M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit14_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit141.Checked) val += 1 << 0;
            if (chkOCxmit142.Checked) val += 1 << 1;
            if (chkOCxmit143.Checked) val += 1 << 2;
            if (chkOCxmit144.Checked) val += 1 << 3;
            if (chkOCxmit145.Checked) val += 1 << 4;
            if (chkOCxmit146.Checked) val += 1 << 5;
            if (chkOCxmit147.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B14M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B14M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit13_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit131.Checked) val += 1 << 0;
            if (chkOCxmit132.Checked) val += 1 << 1;
            if (chkOCxmit133.Checked) val += 1 << 2;
            if (chkOCxmit134.Checked) val += 1 << 3;
            if (chkOCxmit135.Checked) val += 1 << 4;
            if (chkOCxmit136.Checked) val += 1 << 5;
            if (chkOCxmit137.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B13M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B13M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkOCxmit11_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;
            if (chkOCxmit111.Checked) val += 1 << 0;
            if (chkOCxmit112.Checked) val += 1 << 1;
            if (chkOCxmit113.Checked) val += 1 << 2;
            if (chkOCxmit114.Checked) val += 1 << 3;
            if (chkOCxmit115.Checked) val += 1 << 4;
            if (chkOCxmit116.Checked) val += 1 << 5;
            if (chkOCxmit117.Checked) val += 1 << 6;

            Penny.getPenny().setBandBBitMask(Band.B11M, (byte)(val & 0x70), true);
            Penny.getPenny().setBandABitMask(Band.B11M, (byte)val, true);
            console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;  // need side effect of this to push change to native code
        }

        private void chkDisable6mLNAonTX_CheckedChanged(object sender, EventArgs e)
        {
            console.Disable6mLNAonTX = chkDisable6mLNAonTX.Checked;
        }

        private void chkDisable6mLNAonRX_CheckedChanged(object sender, EventArgs e)
        {
            console.Disable6mLNAonRX = chkDisable6mLNAonRX.Checked;
        }

        private void chkDisableHPFonTX_CheckedChanged(object sender, EventArgs e)
        {
            console.DisableHPFonTX = chkDisableHPFonTX.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void chkAlexHPFBypass_CheckedChanged(object sender, EventArgs e)
        {
            console.AlexHPFBypass = chkAlexHPFBypass.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void chkAlex2HPFBypass_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex2HPFBypass = chkAlex2HPFBypass.Checked;
        }

        private void tpGeneralCalibration_Paint(object sender, PaintEventArgs e)
        {
            panelRX2LevelCal.Visible = false;
        }

        private void chkShowAGC_CheckedChanged(object sender, EventArgs e)
        {
            console.ShowAGC = chkShowAGC.Checked;
        }

        private void chkSpectrumLine_CheckedChanged(object sender, EventArgs e)
        {
            Display.SpectrumLine = chkSpectrumLine.Checked;
        }

        private void chkAGCDisplayHangLine_CheckedChanged(object sender, EventArgs e)
        {
            console.DisplayAGCHangLine = chkAGCDisplayHangLine.Checked;
        }

        private void chkAGCHangSpectrumLine_CheckedChanged(object sender, EventArgs e)
        {
            Display.RX1HangSpectrumLine = chkAGCHangSpectrumLine.Checked;
        }

        private void chkDisplayRX2GainLine_CheckedChanged(object sender, EventArgs e)
        {
            console.DisplayRX2GainLine = chkDisplayRX2GainLine.Checked;
        }

        private void chkRX2GainSpectrumLine_CheckedChanged(object sender, EventArgs e)
        {
            Display.RX2GainSpectrumLine = chkRX2GainSpectrumLine.Checked;
        }

        private void chkDisplayRX2HangLine_CheckedChanged(object sender, EventArgs e)
        {
            console.DisplayRX2HangLine = chkDisplayRX2HangLine.Checked;
        }

        private void chkRX2HangSpectrumLine_CheckedChanged(object sender, EventArgs e)
        {
            Display.RX2HangSpectrumLine = chkRX2HangSpectrumLine.Checked;
        }

        private void chkFirmwareByp_CheckedChanged(object sender, EventArgs e)
        {
            firmware_bypass = chkFirmwareByp.Checked;
        }

        private void chkFullDiscovery_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFullDiscovery.Checked) chkEnableStaticIP.Checked = false;
            //    JanusAudio.SetDiscoveryMode(1);
            //else
            //    JanusAudio.SetDiscoveryMode(0);

            NetworkIO.FastConnect = chkFullDiscovery.Checked;
        }

        private void chkStrictCharSpacing_CheckedChanged(object sender, EventArgs e)
        {
            NetworkIO.EnableCWKeyerSpacing(Convert.ToInt32(chkStrictCharSpacing.Checked));
        }

        private void chkRxOutOnTx_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRxOutOnTx.Checked)
            {
                chkEXT1OutOnTx.Checked = false;
                chkEXT2OutOnTx.Checked = false;
            }
            Alex.RxOutOnTx = chkRxOutOnTx.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void chkSWRProtection_CheckedChanged(object sender, EventArgs e)
        {
            console.SWRProtection = chkSWRProtection.Checked;
        }

        private void chkATTOnTX_CheckedChanged(object sender, EventArgs e)
        {
            console.ATTOnTX = chkATTOnTX.Checked;
        }

        private void chkAlex1_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex1_5BPHPFBypass = chkAlex1_5BPHPF.Checked;
        }

        private void chkAlex6_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex6_5BPHPFBypass = chkAlex6_5BPHPF.Checked;
        }

        private void chkAlex9_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex9_5BPHPFBypass = chkAlex9_5BPHPF.Checked;
        }

        private void chkAlex13BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex13BPHPFBypass = chkAlex13BPHPF.Checked;
        }

        private void chkAlex20BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex20BPHPFBypass = chkAlex20BPHPF.Checked;
        }

        private void chkAlex6BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex6BPHPFBypass = chkAlex6BPHPF.Checked;
        }

        private void chkAlex21_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex21_5BPHPFBypass = chkAlex21_5BPHPF.Checked;
        }

        private void chkAlex26_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex26_5BPHPFBypass = chkAlex26_5BPHPF.Checked;
        }

        private void chkAlex29_5BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex29_5BPHPFBypass = chkAlex29_5BPHPF.Checked;
        }

        private void chkAlex213BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex213BPHPFBypass = chkAlex213BPHPF.Checked;
        }

        private void chkAlex220BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex220BPHPFBypass = chkAlex220BPHPF.Checked;
        }

        private void chkAlex26BPHPF_CheckedChanged(object sender, EventArgs e)
        {
            console.Alex26BPHPFBypass = chkAlex26BPHPF.Checked;
        }

        private void tpPennyCtrl_Paint(object sender, PaintEventArgs e)
        {
            switch (console.CurrentHPSDRModel)
            {
                case HPSDRModel.HPSDR:
                    lblHFRxControl.Text = "J6 Receive Pins";
                    lblHFTxControl.Text = "J6 Transmit Pins";
                    lblVHFRxControl.Text = "J6 Receive Pins";
                    lblVHFTxControl.Text = "J6 Transmit Pins";
                    lblSWLRxControl.Text = "J6 Receive Pins";
                    lblSWLTxControl.Text = "J6 Transmit Pins";
                    break;
                case HPSDRModel.ANAN7000D:
                case HPSDRModel.ANAN8000D:
                    lblHFRxControl.Text = "OC Receive Pins";
                    lblHFTxControl.Text = "OC Transmit Pins";
                    lblVHFRxControl.Text = "OC Receive Pins";
                    lblVHFTxControl.Text = "OC Transmit Pins";
                    lblSWLRxControl.Text = "OC Receive Pins";
                    lblSWLTxControl.Text = "OC Transmit Pins";
                    break;
                default:
                    lblHFRxControl.Text = "J16 Receive Pins";
                    lblHFTxControl.Text = "J16 Transmit Pins";
                    lblVHFRxControl.Text = "J16 Receive Pins";
                    lblVHFTxControl.Text = "J16 Transmit Pins";
                    lblSWLRxControl.Text = "J16 Receive Pins";
                    lblSWLTxControl.Text = "J16 Transmit Pins";
                    break;
            }
        }

        private void chkApolloPresent_CheckedChanged(object sender, EventArgs e)
        {
            if (chkApolloPresent.Checked)
            {
                if (chkAlexPresent.Checked) chkAlexPresent.Checked = false;

                //  NetworkIO.SetHermesFilter(1);
                chkApolloFilter_CheckedChanged(this, EventArgs.Empty);
                chkApolloTuner_CheckedChanged(this, EventArgs.Empty);
            }
            //  else NetworkIO.SetHermesFilter(0);

            console.ApolloPresent = chkApolloPresent.Checked;
        }

        private void chkApolloFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (chkApolloFilter.Checked) NetworkIO.EnableApolloFilter(1);
            else NetworkIO.EnableApolloFilter(0);
        }

        private void chkApolloTuner_CheckedChanged(object sender, EventArgs e)
        {
            console.ApolloTunerEnabled = chkApolloTuner.Checked;

            // if (chkApolloTuner.Checked) JanusAudio.EnableApolloTuner(1);
            // else JanusAudio.EnableApolloTuner(0);
        }

        private void chkLevelFades_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLevelFades.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXAMDFadeLevel = 1;
                console.radio.GetDSPRX(0, 1).RXAMDFadeLevel = 1;
            }
            else
            {
                console.radio.GetDSPRX(0, 0).RXAMDFadeLevel = 0;
                console.radio.GetDSPRX(0, 1).RXAMDFadeLevel = 0;
            }
        }

        private void radLSBUSB_CheckedChanged(object sender, EventArgs e)
        {
            if (radLSBUSB.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXAMDSBMode = 0;
                console.radio.GetDSPRX(0, 1).RXAMDSBMode = 0;
            }
        }

        private void radLSB_CheckedChanged(object sender, EventArgs e)
        {
            if (radLSB.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXAMDSBMode = 1;
                console.radio.GetDSPRX(0, 1).RXAMDSBMode = 1;
            }
        }

        private void radUSB_CheckedChanged(object sender, EventArgs e)
        {
            if (radUSB.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXAMDSBMode = 2;
                console.radio.GetDSPRX(0, 1).RXAMDSBMode = 2;
            }
        }

        private void chkRX2LevelFades_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX2LevelFades.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXAMDFadeLevel = 1;
                console.radio.GetDSPRX(1, 1).RXAMDFadeLevel = 1;
            }
            else
            {
                console.radio.GetDSPRX(1, 0).RXAMDFadeLevel = 0;
                console.radio.GetDSPRX(1, 1).RXAMDFadeLevel = 0;
            }
        }

        private void radRX2LSBUSB_CheckedChanged(object sender, EventArgs e)
        {
            if (radRX2LSBUSB.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXAMDSBMode = 0;
                console.radio.GetDSPRX(1, 1).RXAMDSBMode = 0;
            }
        }

        private void radRX2LSB_CheckedChanged(object sender, EventArgs e)
        {
            if (radRX2LSB.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXAMDSBMode = 1;
                console.radio.GetDSPRX(1, 1).RXAMDSBMode = 1;
            }
        }

        private void radRX2USB_CheckedChanged(object sender, EventArgs e)
        {
            if (radRX2USB.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXAMDSBMode = 2;
                console.radio.GetDSPRX(1, 1).RXAMDSBMode = 2;
            }
        }

        private void chkAutoPACalibrate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoPACalibrate.Checked) panelAutoPACalibrate.Visible = true;
            else panelAutoPACalibrate.Visible = false;
        }

        private void chkHermesStepAttenuator_CheckedChanged(object sender, EventArgs e)
        {
            console.RX1StepAttPresent = chkHermesStepAttenuator.Checked;
            if (chkHermesStepAttenuator.Checked)
            {
                udHermesStepAttenuatorData_ValueChanged(this, EventArgs.Empty);
            }
        }

        private void udHermesStepAttenuatorData_ValueChanged(object sender, EventArgs e)
        {
            console.RX1AttenuatorData = (int)udHermesStepAttenuatorData.Value;

            if (AlexPresent && !console.ANAN10Present && !console.ANAN10EPresent /* && !console.ANAN100BPresent */)
                udHermesStepAttenuatorData.Maximum = (decimal)61;
            else udHermesStepAttenuatorData.Maximum = (decimal)31;
        }

        private void chkRX2StepAtt_CheckedChanged(object sender, EventArgs e)
        {
            console.RX2StepAttPresent = chkRX2StepAtt.Checked;
        }

        private void udAlex160mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex160mLPFStart.Value >= udAlex160mLPFEnd.Value + (decimal)0.000001)
            {
                udAlex160mLPFEnd.Value = udAlex160mLPFStart.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex160mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex160mLPFEnd.Value <= udAlex160mLPFStart.Value)
            {
                udAlex160mLPFStart.Value = udAlex160mLPFEnd.Value - (decimal)0.000001;
                // return;
            }
            else if (udAlex160mLPFEnd.Value >= udAlex80mLPFStart.Value)
            {
                udAlex80mLPFStart.Value = udAlex160mLPFEnd.Value + (decimal)0.000001;
                // return;
            }
        }

        private void udAlex80mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex80mLPFStart.Value <= udAlex160mLPFEnd.Value)
            {
                udAlex160mLPFEnd.Value = udAlex80mLPFStart.Value - (decimal)0.000001;
                return;
            }
        }

        private void udAlex80mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex80mLPFEnd.Value >= udAlex40mLPFStart.Value)
            {
                udAlex40mLPFStart.Value = udAlex80mLPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex40mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex40mLPFStart.Value <= udAlex80mLPFEnd.Value)
            {
                udAlex80mLPFEnd.Value = udAlex40mLPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex40mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex40mLPFEnd.Value >= udAlex20mLPFStart.Value)
            {
                udAlex20mLPFStart.Value = udAlex40mLPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex20mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex20mLPFStart.Value <= udAlex40mLPFEnd.Value)
            {
                udAlex40mLPFEnd.Value = udAlex20mLPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex20mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex20mLPFEnd.Value >= udAlex15mLPFStart.Value)
            {
                udAlex15mLPFStart.Value = udAlex20mLPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex15mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex15mLPFStart.Value <= udAlex20mLPFEnd.Value)
            {
                udAlex20mLPFEnd.Value = udAlex15mLPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex15mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex15mLPFEnd.Value >= udAlex10mLPFStart.Value)
            {
                udAlex10mLPFStart.Value = udAlex15mLPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex10mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex10mLPFStart.Value <= udAlex15mLPFEnd.Value)
            {
                udAlex15mLPFEnd.Value = udAlex10mLPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex10mLPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex10mLPFEnd.Value >= udAlex6mLPFStart.Value)
            {
                udAlex6mLPFStart.Value = udAlex10mLPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex6mLPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex6mLPFStart.Value <= udAlex10mLPFEnd.Value)
            {
                udAlex10mLPFEnd.Value = udAlex6mLPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex1_5HPFStart_ValueChanged(object sender, EventArgs e)
        {

        }

        private void udAlex1_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex1_5HPFEnd.Value >= udAlex6_5HPFStart.Value)
            {
                udAlex6_5HPFStart.Value = udAlex1_5HPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex6_5HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex6_5HPFStart.Value <= udAlex1_5HPFEnd.Value)
            {
                udAlex1_5HPFEnd.Value = udAlex6_5HPFStart.Value - (decimal)0.000001;
                return;
            }
        }

        private void udAlex6_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex6_5HPFEnd.Value >= udAlex9_5HPFStart.Value)
            {
                udAlex9_5HPFStart.Value = udAlex6_5HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex9_5HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex9_5HPFStart.Value <= udAlex6_5HPFEnd.Value)
            {
                udAlex6_5HPFEnd.Value = udAlex9_5HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex9_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex9_5HPFEnd.Value >= udAlex13HPFStart.Value)
            {
                udAlex13HPFStart.Value = udAlex9_5HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex13HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex13HPFStart.Value <= udAlex9_5HPFEnd.Value)
            {
                udAlex9_5HPFEnd.Value = udAlex13HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex13HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex13HPFEnd.Value >= udAlex20HPFStart.Value)
            {
                udAlex20HPFStart.Value = udAlex13HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex20HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex20HPFStart.Value <= udAlex13HPFEnd.Value)
            {
                udAlex13HPFEnd.Value = udAlex20HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex20HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex20HPFEnd.Value >= udAlex6BPFStart.Value)
            {
                udAlex6BPFStart.Value = udAlex20HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex6BPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex6BPFStart.Value <= udAlex20HPFEnd.Value)
            {
                udAlex20HPFEnd.Value = udAlex6BPFStart.Value - (decimal)0.000001;
                return;
            }
        }

        private void udAlex6BPFEnd_ValueChanged(object sender, EventArgs e)
        {

        }

        private void udAlex21_5HPFStart_ValueChanged(object sender, EventArgs e)
        {

        }

        private void udAlex21_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex21_5HPFEnd.Value >= udAlex26_5HPFStart.Value)
            {
                udAlex26_5HPFStart.Value = udAlex21_5HPFEnd.Value + (decimal)0.000001;
                return;
            }

        }

        private void udAlex26_5HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex26_5HPFStart.Value <= udAlex21_5HPFEnd.Value)
            {
                udAlex21_5HPFEnd.Value = udAlex26_5HPFStart.Value - (decimal)0.000001;
                return;
            }
        }

        private void udAlex26_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex26_5HPFEnd.Value >= udAlex29_5HPFStart.Value)
            {
                udAlex29_5HPFStart.Value = udAlex26_5HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex29_5HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex29_5HPFStart.Value <= udAlex26_5HPFEnd.Value)
            {
                udAlex26_5HPFEnd.Value = udAlex29_5HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex29_5HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex29_5HPFEnd.Value >= udAlex213HPFStart.Value)
            {
                udAlex213HPFStart.Value = udAlex29_5HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex213HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex213HPFStart.Value <= udAlex29_5HPFEnd.Value)
            {
                udAlex29_5HPFEnd.Value = udAlex213HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex213HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex213HPFEnd.Value >= udAlex220HPFStart.Value)
            {
                udAlex220HPFStart.Value = udAlex213HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex220HPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex220HPFStart.Value <= udAlex213HPFEnd.Value)
            {
                udAlex213HPFEnd.Value = udAlex220HPFStart.Value - (decimal)0.000001;
                return;
            }

        }

        private void udAlex220HPFEnd_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex220HPFEnd.Value >= udAlex26BPFStart.Value)
            {
                udAlex26BPFStart.Value = udAlex220HPFEnd.Value + (decimal)0.000001;
                return;
            }
        }

        private void udAlex26BPFStart_ValueChanged(object sender, EventArgs e)
        {
            if (udAlex26BPFStart.Value <= udAlex220HPFEnd.Value)
            {
                udAlex220HPFEnd.Value = udAlex26BPFStart.Value - (decimal)0.000001;
                return;
            }
        }

        private void udAlex26BPFEnd_ValueChanged(object sender, EventArgs e)
        {

        }

        private void chkSWRTuneProtection_CheckedChanged(object sender, EventArgs e)
        {
            console.DisableSWRonTune = chkSWRTuneProtection.Checked;
        }

        private void tbDisplayFFTSize_Scroll(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbDisplayFFTSize.Value))));
            // console.specRX.GetSpecRX(2).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbDisplayFFTSize.Value))));
            //  console.specRX.GetSpecRX(1).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbDisplayFFTSize.Value))));
            console.UpdateRXSpectrumDisplayVars();
            double bin_width = (double)Display.SampleRateRX1 / (double)console.specRX.GetSpecRX(0).FFTSize;
            lblDisplayBinWidth.Text = bin_width.ToString("N3");
            Display.RX1FFTSizeOffset = tbDisplayFFTSize.Value * 2;
            // Display.RX2FFTSizeOffset = tbDisplayFFTSize.Value * 2;
        }

        private void tbRX2DisplayFFTSize_Scroll(object sender, EventArgs e)
        {
            // console.specRX.GetSpecRX(0).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbDisplayFFTSize.Value))));
            console.specRX.GetSpecRX(1).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbRX2DisplayFFTSize.Value))));
            double bin_width = (double)Display.SampleRateRX2 / (double)console.specRX.GetSpecRX(1).FFTSize;
            lblRX2DisplayBinWidth.Text = bin_width.ToString("N3");
            // Display.RX1FFTSizeOffset = tbDisplayFFTSize.Value * 2;
            Display.RX2FFTSizeOffset = tbRX2DisplayFFTSize.Value * 2;
        }

        private void comboDispWinType_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).WindowType = comboDispWinType.SelectedIndex;
            console.UpdateRXSpectrumDisplayVars();
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).WindowType = comboDispWinType.SelectedIndex;
            console.UpdateTXSpectrumDisplayVars();
        }

        private void comboRX2DispWinType_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).WindowType = comboRX2DispWinType.SelectedIndex;
        }

        private void udDSPNBTransition_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).NBTau = 0.001 * (double)udDSPNBTransition.Value;
            console.radio.GetDSPRX(1, 0).NBTau = 0.001 * (double)udDSPNBTransition.Value;
        }

        private void udDSPNBLead_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).NBAdvTime = 0.001 * (double)udDSPNBLead.Value;
            console.radio.GetDSPRX(1, 0).NBAdvTime = 0.001 * (double)udDSPNBLead.Value;
        }

        private void udDSPNBLag_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).NBHangTime = 0.001 * (double)udDSPNBLag.Value;
            console.radio.GetDSPRX(1, 0).NBHangTime = 0.001 * (double)udDSPNBLag.Value;
        }

        private void chkCBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCBlock.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXCBLRun = true;
                console.radio.GetDSPRX(0, 1).RXCBLRun = true;
            }
            else
            {
                console.radio.GetDSPRX(0, 0).RXCBLRun = false;
                console.radio.GetDSPRX(0, 1).RXCBLRun = false;
            }
        }

        private void chkRX2CBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX2CBlock.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXCBLRun = true;
                console.radio.GetDSPRX(1, 1).RXCBLRun = true;
            }
            else
            {
                console.radio.GetDSPRX(1, 0).RXCBLRun = false;
                console.radio.GetDSPRX(1, 1).RXCBLRun = false;
            }
        }

        private void btnConfigure_Click(object sender, EventArgs e)
        {
            if (ConfigMidi2Cat == null)
            {
                console.Midi2Cat.CloseMidi2Cat();
                ConfigMidi2Cat = new Midi2Cat.Midi2CatSetupForm(console.Midi2Cat.Midi2CatDbFile);
                ConfigMidi2Cat.Show();
                ConfigMidi2Cat.Focus();
                ConfigMidi2Cat.FormClosed += new FormClosedEventHandler(ConfigMidi2CatSetupClosed);
            }
            return;
        }

        private void ConfigMidi2CatSetupClosed(object sender, FormClosedEventArgs e)
        {
            if (ConfigMidi2Cat != null)
            {
                ConfigMidi2Cat.Dispose();
                ConfigMidi2Cat = null;
                console.Midi2Cat.OpenMidi2Cat();
            }
        }

        private void btnSetIPAddr_Click(object sender, EventArgs e)
        {
            if (radStaticIP1.Checked)
            {
                console.HPSDRNetworkIPAddr = udStaticIP1.Text + "." + udStaticIP2.Text + "." +
                                             udStaticIP3.Text + "." + udStaticIP4.Text;
            }
            if (radStaticIP2.Checked)
            {
                console.HPSDRNetworkIPAddr = udStaticIP5.Text + "." + udStaticIP6.Text + "." +
                                             udStaticIP7.Text + "." + udStaticIP8.Text;
            }
            if (radStaticIP3.Checked)
            {
                console.HPSDRNetworkIPAddr = udStaticIP9.Text + "." + udStaticIP10.Text + "." +
                                             udStaticIP11.Text + "." + udStaticIP12.Text;
            }
            if (radStaticIP4.Checked)
            {
                console.HPSDRNetworkIPAddr = udStaticIP13.Text + "." + udStaticIP14.Text + "." +
                                             udStaticIP15.Text + "." + udStaticIP16.Text;
            }
        }

        private void chkEnableStaticIP_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableStaticIP.Checked) chkFullDiscovery.Checked = false;
            NetworkIO.enableStaticIP = chkEnableStaticIP.Checked;
        }

        private void chkRX1WaterfallAGC_CheckedChanged(object sender, EventArgs e)
        {
            Display.RX1WaterfallAGC = chkRX1WaterfallAGC.Checked;
            udDisplayWaterfallLowLevel.Enabled = !chkRX1WaterfallAGC.Checked;
        }

        private void chkRX2WaterfallAGC_CheckedChanged(object sender, EventArgs e)
        {
            Display.RX2WaterfallAGC = chkRX2WaterfallAGC.Checked;
            udRX2DisplayWaterfallLowLevel.Enabled = !chkRX2WaterfallAGC.Checked;
        }

        private void chkPAValues_CheckedChanged(object sender, EventArgs e)
        {
            panelPAValues.Visible = chkPAValues.Checked;
            console.PAValues = chkPAValues.Checked;
        }

        private void btnResetPAValues_Click(object sender, EventArgs e)
        {
            textDCVolts.Text = "";
            textFwdADCValue.Text = "";
            textDriveFwdADCValue.Text = "";
            textFwdVoltage.Text = "";
            textDrivePower.Text = "";
            textPAFwdPower.Text = "";
            textPARevPower.Text = "";
            textRevADCValue.Text = "";
            textRevVoltage.Text = "";
        }

        private void btnResetWattMeterValues_Click(object sender, EventArgs e)
        {
            switch (console.CurrentHPSDRModel)
            {
                case HPSDRModel.ANAN10:
                case HPSDRModel.ANAN10E:
                    ud10PA1W.Value = 1;
                    ud10PA2W.Value = 2;
                    ud10PA3W.Value = 3;
                    ud10PA4W.Value = 4;
                    ud10PA5W.Value = 5;
                    ud10PA6W.Value = 6;
                    ud10PA7W.Value = 7;
                    ud10PA8W.Value = 8;
                    ud10PA9W.Value = 9;
                    ud10PA10W.Value = 10;
                    ud10PA11W.Value = 11;
                    ud10PA12W.Value = 12;
                    ud10PA13W.Value = 13;
                    ud10PA14W.Value = 14;
                    break;
                case HPSDRModel.ANAN8000D:
                    ud200PA20W.Value = 20;
                    ud200PA40W.Value = 40;
                    ud200PA60W.Value = 60;
                    ud200PA80W.Value = 80;
                    ud200PA100W.Value = 100;
                    ud200PA120W.Value = 120;
                    ud200PA140W.Value = 140;
                    ud200PA160W.Value = 160;
                    ud200PA180W.Value = 180;
                    ud200PA200W.Value = 200;
                    ud200PA220W.Value = 220;
                    ud200PA240W.Value = 240;
                    ud200PA260W.Value = 260;
                    ud200PA280W.Value = 280;
                    break;
                default:
                    ud100PA10W.Value = 10;
                    ud100PA20W.Value = 20;
                    ud100PA30W.Value = 30;
                    ud100PA40W.Value = 40;
                    ud100PA50W.Value = 50;
                    ud100PA60W.Value = 60;
                    ud100PA70W.Value = 70;
                    ud100PA80W.Value = 80;
                    ud100PA90W.Value = 90;
                    ud100PA100W.Value = 100;
                    ud100PA110W.Value = 110;
                    ud100PA120W.Value = 120;
                    ud100PA130W.Value = 130;
                    ud100PA140W.Value = 140;
                    break;
            }
        }

        private void comboDSPRxWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            int wintype = comboDSPRxWindow.SelectedIndex;
            if (wintype < 0)
            {
                wintype = 0;
                comboDSPRxWindow.Text = "BH - 4";
            }
            console.radio.GetDSPRX(0, 0).RXBandpassWindow = wintype;
            console.radio.GetDSPRX(0, 1).RXBandpassWindow = wintype;
            console.radio.GetDSPRX(1, 0).RXBandpassWindow = wintype;
        }

        private void comboDSPTxWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            int wintype = comboDSPTxWindow.SelectedIndex;
            if (wintype < 0)
            {
                wintype = 0;
                comboDSPTxWindow.Text = "BH - 4";
            }
            console.radio.GetDSPTX(0).TXBandpassWindow = wintype;
        }

        private void radOrionPTTOff_CheckedChanged(object sender, EventArgs e)
        {
            console.MicPTTDisabled = radOrionPTTOff.Checked;
        }

        private void radOrionMicTip_CheckedChanged(object sender, EventArgs e)
        {
            if (radOrionMicTip.Checked)
                NetworkIO.SetMicTipRing(0);
            else NetworkIO.SetMicTipRing(1);
        }

        private void radOrionBiasOn_CheckedChanged(object sender, EventArgs e)
        {
            if (radOrionBiasOn.Checked)
                NetworkIO.SetMicBias(1);
            else NetworkIO.SetMicBias(0);
        }

        private void chkEXT1OutOnTx_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEXT1OutOnTx.Checked)
            {
                chkRxOutOnTx.Checked = false;
                chkEXT2OutOnTx.Checked = false;
            }
            Alex.Ext1OutOnTx = chkEXT1OutOnTx.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void chkEXT2OutOnTx_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEXT2OutOnTx.Checked)
            {
                chkRxOutOnTx.Checked = false;
                chkEXT1OutOnTx.Checked = false;
            }
            Alex.Ext2OutOnTx = chkEXT2OutOnTx.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void udTXGenScale_ValueChanged(object sender, EventArgs e)
        {
            switch (cmboSigGenTXMode.Text)
            {
                case "Tone":
                    console.radio.GetDSPTX(0).TXPreGenToneMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
                case "Noise":
                    console.radio.GetDSPTX(0).TXPreGenNoiseMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
                case "Sweep":
                    console.radio.GetDSPTX(0).TXPreGenSweepMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
                case "Sawtooth":
                    console.radio.GetDSPTX(0).TXPreGenSawtoothMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
                case "Triangle":
                    console.radio.GetDSPTX(0).TXPreGenTriangleMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
                case "Pulse":
                    console.radio.GetDSPTX(0).TXPreGenPulseMag = Math.Pow(10, (double)udTXGenScale.Value / 20.0);
                    break;
            }
        }

        private void udTXGenFreq_ValueChanged(object sender, EventArgs e)
        {
            switch (cmboSigGenTXMode.Text)
            {
                case "Tone":
                    console.radio.GetDSPTX(0).TXPreGenToneFreq = (double)udTXGenFreq.Value;
                    break;
                case "Sawtooth":
                    console.radio.GetDSPTX(0).TXPreGenSawtoothFreq = (double)udTXGenFreq.Value;
                    break;
                case "Triangle":
                    console.radio.GetDSPTX(0).TXPreGenTriangleFreq = (double)udTXGenFreq.Value;
                    break;
                case "Pulse":
                    console.radio.GetDSPTX(0).TXPreGenPulseToneFreq = (double)udTXGenFreq.Value;
                    break;
            }
        }

        private void udTXGenSweepLow_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenSweepFreq1 = (double)udTXGenSweepLow.Value;
        }

        private void udTXGenSweepHigh_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenSweepFreq2 = (double)udTXGenSweepHigh.Value;
        }

        private void udTXGenSweepRate_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenSweepRate = (double)udTXGenSweepRate.Value;
        }

        private void chkSigGenRX1_CheckedChanged(object sender, EventArgs e)
        {
            cmboSigGenRXMode_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void udRXGenScale_ValueChanged(object sender, EventArgs e)
        {
            switch (cmboSigGenRXMode.Text)
            {
                case "Tone":
                    console.radio.GetDSPRX(0, 0).RXPreGenToneMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    console.radio.GetDSPRX(1, 0).RXPreGenToneMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    break;
                case "Noise":
                    console.radio.GetDSPRX(0, 0).RXPreGenNoiseMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    console.radio.GetDSPRX(1, 0).RXPreGenNoiseMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    break;
                case "Sweep":
                    console.radio.GetDSPRX(0, 0).RXPreGenSweepMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    console.radio.GetDSPRX(1, 0).RXPreGenSweepMag = Math.Pow(10, (double)udRXGenScale.Value / 20.0);
                    break;
            }
        }

        private void udRXGenFreq_ValueChanged(object sender, EventArgs e)
        {
            switch (cmboSigGenRXMode.Text)
            {
                case "Tone":
                    console.radio.GetDSPRX(0, 0).RXPreGenToneFreq = (double)udRXGenFreq.Value;
                    console.radio.GetDSPRX(1, 0).RXPreGenToneFreq = (double)udRXGenFreq.Value;
                    break;
            }
        }

        private void udRXGenSweepLow_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXPreGenSweepFreq1 = (double)udRXGenSweepLow.Value;
            console.radio.GetDSPRX(1, 0).RXPreGenSweepFreq1 = (double)udRXGenSweepLow.Value;
        }

        private void udRXGenSweepHigh_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXPreGenSweepFreq2 = (double)udRXGenSweepHigh.Value;
            console.radio.GetDSPRX(1, 0).RXPreGenSweepFreq2 = (double)udRXGenSweepHigh.Value;
        }

        private void udRXGenSweepRate_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXPreGenSweepRate = (double)udRXGenSweepRate.Value;
            console.radio.GetDSPRX(1, 0).RXPreGenSweepRate = (double)udRXGenSweepRate.Value;
        }

        private void chkTXInhibit_CheckedChanged(object sender, EventArgs e)
        {
            console.tx_inhibit_enabled = chkTXInhibit.Checked;
        }

        private void chkTXInhibitSense_CheckedChanged(object sender, EventArgs e)
        {
            console.tx_inhibit_sense = chkTXInhibitSense.Checked;
        }

        private void udTXGenPulseFreq_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenPulseFreq = (double)udTXGenPulseFreq.Value;
        }

        private void udTXGenPulseDutyCycle_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenPulseDutyCycle = (double)udTXGenPulseDutyCycle.Value;
        }

        private void udTXGenPulseTransition_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXPreGenPulseTransition = (double)udTXGenPulseTransition.Value;
        }

        private void chkEmphPos_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXFMEmphOn = !chkEmphPos.Checked;
        }

        private void chkRemoveTone_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXFMCTCSSFilter = chkRemoveTone.Checked;
            console.radio.GetDSPRX(0, 1).RXFMCTCSSFilter = chkRemoveTone.Checked;
            console.radio.GetDSPRX(1, 0).RXFMCTCSSFilter = chkRemoveTone.Checked;
        }

        private void chkDSPEERon_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDSPEERon.Checked)
            {
                console.radio.GetDSPTX(0).TXEERModeRun = true;
                NetworkIO.EnableEClassModulation(1);
            }
            else
            {
                console.radio.GetDSPTX(0).TXEERModeRun = false;
                NetworkIO.EnableEClassModulation(0);
            }
        }

        private void udDSPEERmgain_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModeMgain = (double)udDSPEERmgain.Value;
        }

        private void udDSPEERpgain_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModePgain = (double)udDSPEERpgain.Value;
        }

        private void udDSPEERmdelay_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModeMdelay = (double)udDSPEERmdelay.Value / 1.0e+06;
        }

        private void chkDSPEERamIQ_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModeAMIQ = chkDSPEERamIQ.Checked;
        }

        private void chkHFTRRelay_CheckedChanged(object sender, EventArgs e)
        {
            console.HFTRRelay = chkHFTRRelay.Checked;
        }

        private void udHWKeyDownDelay_ValueChanged(object sender, EventArgs e)
        {
            NetworkIO.SetCWPTTDelay((int)udHWKeyDownDelay.Value);
        }

        // Path_Illustrator Properties added

        public bool RadBPHPFled
        {
            get { return radBPHPFled.Checked; }
        }

        public bool ChkDisableHPFOnTx
        {
            get { return chkDisableHPFonTX.Checked; }
        }

        public bool RadRX1ADC1
        {
            get { return radDDC0ADC0.Checked; }
        }

        public bool RadRX1ADC2
        {
            get { return radDDC0ADC1.Checked; }
        }

        public bool RadRX1ADC3
        {
            get { return radDDC0ADC2.Checked; }
        }

        public bool RadRX2ADC1
        {
            get { return radDDC1ADC0.Checked; }
        }

        public bool RadRX2ADC2
        {
            get { return radDDC1ADC1.Checked; }
        }

        public bool RadRX2ADC3
        {
            get { return radDDC1ADC2.Checked; }
        }

        public bool RadRX3ADC1
        {
            get { return radDDC2ADC0.Checked; }
        }

        public bool RadRX3ADC2
        {
            get { return radDDC2ADC1.Checked; }
        }

        public bool RadRX3ADC3
        {
            get { return radDDC2ADC2.Checked; }
        }

        public bool RadRX4ADC1
        {
            get { return radDDC3ADC0.Checked; }
        }

        public bool RadRX4ADC2
        {
            get { return radDDC3ADC1.Checked; }
        }

        public bool RadRX4ADC3
        {
            get { return radDDC3ADC2.Checked; }
        }

        public bool RadRX5ADC1
        {
            get { return radDDC4ADC0.Checked; }
        }

        public bool RadRX5ADC2
        {
            get { return radDDC4ADC1.Checked; }
        }

        public bool RadRX5ADC3
        {
            get { return radDDC4ADC2.Checked; }
        }

        public bool RadRX6ADC1
        {
            get { return radDDC5ADC0.Checked; }
        }

        public bool RadRX6ADC2
        {
            get { return radDDC5ADC1.Checked; }
        }

        public bool RadRX6ADC3
        {
            get { return radDDC5ADC2.Checked; }
        }

        public bool RadRX7ADC1
        {
            get { return radDDC6ADC0.Checked; }
        }

        public bool RadRX7ADC2
        {
            get { return radDDC6ADC1.Checked; }
        }

        public bool RadRX7ADC3
        {
            get { return radDDC6ADC2.Checked; }
        }

        public bool RadGenModelANAN10
        {
            get { return (radGenModelANAN10.Checked || radGenModelANAN10E.Checked); }
        }

        public bool RadGenModelANAN100
        {
            get { return (radGenModelANAN100.Checked || radGenModelANAN100B.Checked); }
        }

        public bool RadGenModelANAN100D
        {
            get { return radGenModelANAN100D.Checked || radGenModelANAN200D.Checked; }
        }

        public bool RadGenModelHPSDR
        {
            get { return radGenModelHPSDR.Checked; }
        }

        public bool RadGenModelHermes
        {
            get { return radGenModelHermes.Checked; }
        }

        private int pi_RxAnt;
        public int PI_RxAnt
        {
            get { return pi_RxAnt; }
        }

        private int pi_TxAnt;
        public int PI_TxAnt
        {
            get { return pi_TxAnt; }
        }

        private int alex_EXT2EXT1XVTR = 0;
        public int Alex_EXT2EXT1XVTR
        {
            get { return alex_EXT2EXT1XVTR; }
        }


        public bool ChkDisableRXOut
        {
            get { return chkDisableRXOut.Checked; }
        }

        public bool ChkRxOutOnTx
        {
            get { return chkRxOutOnTx.Checked; }
        }

        public bool ChkRx1InOnTx
        {
            get { return chkEXT2OutOnTx.Checked; }
        }

        public bool ChkRx2InOnTx
        {
            get { return chkEXT1OutOnTx.Checked; }
        }

        // end of Path_Illustrator Properties support

        private void radDDCADC_CheckedChanged(object sender, EventArgs e)
        {
            int val = 0;

            //DDC0 ADC control: bits 1 & 0
            if (radDDC0ADC0.Checked) val += 0; //bits 1 & 0 set to 00 => DDC0 to ADC0
            if (radDDC0ADC1.Checked) val += 1 << 0; //bits 1 & 0 set to 01 => DDC0 to ADC1
            if (radDDC0ADC2.Checked) val += 1 << 1; //bits 1 & 0 set to 10 => DDC0 to ADC2
            //DDC1 ADC control: bits 3 & 2
            if (radDDC1ADC0.Checked) val += 0; // bits 3 & 2 set to 00 => DDC1 to ADC0
            if (radDDC1ADC1.Checked) val += 1 << 2; // bits 3 & 2 set to 01 => DDC1 to ADC1
            if (radDDC1ADC2.Checked) val += 1 << 3; // bits 3 & 2 set to 10 => DDC1 to ADC2
            //DDC2 ADC control: bits 5 & 4
            if (radDDC2ADC0.Checked) val += 0; // bits 5 & 4 set to 00 => DDC2 to ADC0
            if (radDDC2ADC1.Checked) val += 1 << 4; // bits 5 & 4 set to 01 => DDC2 to ADC1
            if (radDDC2ADC2.Checked) val += 1 << 5; // bits 5 & 4 set to 10 => DDC2 to ADC2
            //DDC3 ADC control: bits 7 & 6
            if (radDDC3ADC0.Checked) val += 0; // bits 7 & 6 set to 00 => DDC3 to ADC0
            if (radDDC3ADC1.Checked) val += 1 << 6; // bits 7 & 6 set to 01 => DDC3 to ADC1
            if (radDDC3ADC2.Checked) val += 1 << 7;  // bits 7 & 6 set to 10 => DDC3 to ADC2

            console.RXADCCtrl1 = val;
            // JanusAudio.SetADC_cntrl1(val);
            val = 0;

            //DDC4 ADC control: bits 1 & 0
            if (radDDC4ADC0.Checked) val += 0; //bits 1 & 0 set to 00 => DDC4 to ADC0
            if (radDDC4ADC1.Checked) val += 1 << 0; //bits 1 & 0 set to 01 => DDC4 to ADC1
            if (radDDC4ADC2.Checked) val += 1 << 1; //bits 1 & 0 set to 10 => DDC4 to ADC2
            //DDC5 ADC control: bits 3 & 2
            if (radDDC5ADC0.Checked) val += 0; // bits 3 & 2 set to 00 => DDC5 to ADC0
            if (radDDC5ADC1.Checked) val += 1 << 2; // bits 3 & 2 set to 01 => DDC5 to ADC1
            if (radDDC5ADC2.Checked) val += 1 << 3; // bits 3 & 2 set to 10 => DDC5 to ADC2
            //DDC6 ADC control: bits 5 & 4
            if (radDDC6ADC0.Checked) val += 0; // bits 5 & 4 set to 00 => DDC6 to ADC0
            if (radDDC6ADC1.Checked) val += 1 << 4; // bits 5 & 4 set to 01 => DDC6 to ADC1
            if (radDDC6ADC2.Checked) val += 1 << 5; // bits 5 & 4 set to 10 => DDC6 to ADC2

            console.RXADCCtrl2 = val;
            // JanusAudio.SetADC_cntrl2(val);
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void comboDSPNOBmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nbmode = comboDSPNOBmode.SelectedIndex;
            if (nbmode < 0)
            {
                nbmode = 0;
                comboDSPNOBmode.Text = "Zero";
                comboDSPNOBmode.SelectedIndex = nbmode;
            }
            console.radio.GetDSPRX(0, 0).NBMode = nbmode;
            console.radio.GetDSPRX(1, 0).NBMode = nbmode;
        }

        private void chkClickTuneDrag_CheckedChanged(object sender, EventArgs e)
        {
            console.ClickTuneDrag = chkClickTuneDrag.Checked;
        }

        // APF

        private void chkDSPRX1APFEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (console.RX1DSPMode == DSPMode.CWL ||
                console.RX1DSPMode == DSPMode.CWU)
                console.radio.GetDSPRX(0, 0).RXAPFRun = chkDSPRX1APFEnable.Checked;

            if (radDSPRX1APFControls.Checked)
                console.APFEnabled = chkDSPRX1APFEnable.Checked;

            if (chkDSPRX1APFEnable.Checked)
            {
                if (chkDSPRX1subAPFEnable.Checked) console.RX1APFlabel = "APFb";
                else console.RX1APFlabel = "APFm";
            }
            else if (chkDSPRX1subAPFEnable.Checked) console.RX1APFlabel = "APFs";
            else console.RX1APFlabel = "";
        }

        private void chkDSPRX1subAPFEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (console.RX1DSPMode == DSPMode.CWL ||
                console.RX1DSPMode == DSPMode.CWU)
                console.radio.GetDSPRX(0, 1).RXAPFRun = chkDSPRX1subAPFEnable.Checked;

            if (radDSPRX1subAPFControls.Checked)
                console.APFEnabled = chkDSPRX1subAPFEnable.Checked;

            if (chkDSPRX1subAPFEnable.Checked)
            {
                if (chkDSPRX1APFEnable.Checked) console.RX1APFlabel = "APFb";
                else console.RX1APFlabel = "APFs";
            }
            else if (chkDSPRX1APFEnable.Checked) console.RX1APFlabel = "APFm";
            else console.RX1APFlabel = "";

        }

        private void chkDSPRX2APFEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (console.RX2DSPMode == DSPMode.CWL ||
                 console.RX2DSPMode == DSPMode.CWU)
                console.radio.GetDSPRX(1, 0).RXAPFRun = chkDSPRX2APFEnable.Checked;

            if (radDSPRX2APFControls.Checked)
                console.APFEnabled = chkDSPRX2APFEnable.Checked;

            if (chkDSPRX2APFEnable.Checked) console.RX2APFlabel = "APF";
            else console.RX2APFlabel = "";

        }

        private void tbDSPAudRX1APFGain_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX1APFGain.Value / 200.0);
            if (radDSPRX1APFControls.Checked)
                console.APFGain = tbDSPAudRX1APFGain.Value;
        }

        private void tbDSPAudRX1subAPFGain_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX1subAPFGain.Value / 200.0);
            if (radDSPRX1subAPFControls.Checked)
                console.APFGain = tbDSPAudRX1subAPFGain.Value;
        }

        private void tbDSPAudRX2APFGain_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX2APFGain.Value / 200.0);
            if (radDSPRX2APFControls.Checked)
                console.APFGain = tbDSPAudRX2APFGain.Value;
        }

        private void tbRX1APFTune_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAPFFreq = console.CWPitch + (double)tbRX1APFTune.Value;
            if (radDSPRX1APFControls.Checked)
                console.APFFreq = tbRX1APFTune.Value;
        }

        private void tbRX1subAPFTune_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXAPFFreq = console.CWPitch + (double)tbRX1subAPFTune.Value;
            if (radDSPRX1subAPFControls.Checked)
                console.APFFreq = tbRX1subAPFTune.Value;
        }

        private void tbRX2APFTune_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAPFFreq = console.CWPitch + (double)tbRX2APFTune.Value;
            if (radDSPRX2APFControls.Checked)
                console.APFFreq = tbRX2APFTune.Value;
        }

        private void tbRX1APFBW_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAPFBw = (double)tbRX1APFBW.Value;
            if (radDSPRX1APFControls.Checked)
                console.APFBandwidth = tbRX1APFBW.Value;
        }

        private void tbRX1subAPFBW_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXAPFBw = (double)tbRX1subAPFBW.Value;
            if (radDSPRX1subAPFControls.Checked)
                console.APFBandwidth = tbRX1subAPFBW.Value;
        }

        private void tbRX2APFBW_Scroll(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXAPFBw = (double)tbRX2APFBW.Value;
            if (radDSPRX2APFControls.Checked)
                console.APFBandwidth = tbRX2APFBW.Value;
        }

        private void radDSPRX1APFControls_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPRX1APFControls.Checked)
            {
                chkDSPRX1APFEnable.Checked = console.APFEnabled;
                if (console.RX1DSPMode == DSPMode.CWL ||
                    console.RX1DSPMode == DSPMode.CWU)
                    console.radio.GetDSPRX(0, 0).RXAPFRun = chkDSPRX1APFEnable.Checked;
                tbDSPAudRX1APFGain.Value = console.APFGain;
                console.radio.GetDSPRX(0, 0).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX1APFGain.Value / 200.0);
                tbRX1APFTune.Value = console.APFFreq;
                console.radio.GetDSPRX(0, 0).RXAPFFreq = console.CWPitch + (double)tbRX1APFTune.Value;
                tbRX1APFBW.Value = console.APFBandwidth;
                console.radio.GetDSPRX(0, 0).RXAPFBw = (double)tbRX1APFBW.Value;
                console.APFbtn = "RX1";
            }
        }

        private void radDSPRX1subAPFControls_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPRX1subAPFControls.Checked)
            {
                chkDSPRX1subAPFEnable.Checked = console.APFEnabled;
                if (console.RX1DSPMode == DSPMode.CWL ||
                    console.RX1DSPMode == DSPMode.CWU)
                    console.radio.GetDSPRX(0, 1).RXAPFRun = chkDSPRX1subAPFEnable.Checked;
                tbDSPAudRX1subAPFGain.Value = console.APFGain;
                console.radio.GetDSPRX(0, 1).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX1subAPFGain.Value / 200.0);
                tbRX1subAPFTune.Value = console.APFFreq;
                console.radio.GetDSPRX(0, 1).RXAPFFreq = console.CWPitch + (double)tbRX1subAPFTune.Value;
                tbRX1subAPFBW.Value = console.APFBandwidth;
                console.radio.GetDSPRX(0, 1).RXAPFBw = (double)tbRX1subAPFBW.Value;
                console.APFbtn = "SUB";
            }
        }

        private void radDSPRX2APFControls_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPRX2APFControls.Checked)
            {
                chkDSPRX2APFEnable.Checked = console.APFEnabled;
                if (console.RX2DSPMode == DSPMode.CWL ||
                    console.RX2DSPMode == DSPMode.CWU)
                    console.radio.GetDSPRX(1, 0).RXAPFRun = chkDSPRX2APFEnable.Checked;
                tbDSPAudRX2APFGain.Value = console.APFGain;
                console.radio.GetDSPRX(1, 0).RXAPFGain = Math.Pow(10.0, (double)tbDSPAudRX2APFGain.Value / 200.0);
                tbRX2APFTune.Value = console.APFFreq;
                console.radio.GetDSPRX(1, 0).RXAPFFreq = console.CWPitch + (double)tbRX2APFTune.Value;
                tbRX2APFBW.Value = console.APFBandwidth;
                console.radio.GetDSPRX(1, 0).RXAPFBw = (double)tbRX2APFBW.Value;
                console.APFbtn = "RX2";
            }
        }

        private void chkDSPRX1DollyEnable_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXADollyRun = chkDSPRX1DollyEnable.Checked;
        }

        private void chkDSPRX1DollySubEnable_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXADollyRun = chkDSPRX1SubDollyEnable.Checked;
        }

        private void chkDSPRX2DollyEnable_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXADollyRun = chkDSPRX2DollyEnable.Checked;
        }

        private void udDSPRX1DollyF0_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXADollyFreq0 = (double)udDSPRX1DollyF0.Value;
        }

        private void udDSPRX1SubDollyF0_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXADollyFreq0 = (double)udDSPRX1SubDollyF0.Value;
        }

        private void udDSPRX2DollyF0_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXADollyFreq0 = (double)udDSPRX2DollyF0.Value;
        }

        private void udDSPRX1DollyF1_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXADollyFreq1 = (double)udDSPRX1DollyF1.Value;
        }

        private void udDSPRX2DollyF1_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXADollyFreq1 = (double)udDSPRX2DollyF1.Value;
        }

        private void udDSPRX1SubDollyF1_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 1).RXADollyFreq1 = (double)udDSPRX1SubDollyF1.Value;
        }

        private void udATTOnTX_ValueChanged(object sender, EventArgs e)
        {
            // if(chkATTOnTX.Checked)
            // JanusAudio.SetTxAttenData((int)udATTOnTX.Value);
            console.TxAttenData = (int)udATTOnTX.Value;
        }

        private void ud6mLNAGainOffset_ValueChanged(object sender, EventArgs e)
        {
            console.RX6mGainOffset = (float)ud6mLNAGainOffset.Value;
        }

        private void udDSPEERpdelay_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModePdelay = (double)udDSPEERpdelay.Value / 1.0e+06;
        }

        private void chkDSPEERRunDelays_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPTX(0).TXEERModeRunDelays = chkDSPEERRunDelays.Checked;
        }

        private void udDSPEERpwmMax_ValueChanged(object sender, EventArgs e)
        {
            NetworkIO.SetEERPWMmax((int)udDSPEERpwmMax.Value);
        }

        private void udDSPEERpwmMin_ValueChanged(object sender, EventArgs e)
        {
            NetworkIO.SetEERPWMmin((int)udDSPEERpwmMin.Value);
        }

        private void comboCATPort_Click(object sender, EventArgs e)
        {
            string[] com_ports = SerialPort.GetPortNames();
            comboCATPort.Items.Clear();
            comboCATPort.Items.Add("None");
            comboCATPort.Items.AddRange(com_ports);
        }

        private void comboCAT2Port_Click(object sender, EventArgs e)
        {
            string[] com_ports = SerialPort.GetPortNames();
            comboCAT2Port.Items.Clear();
            comboCAT2Port.Items.Add("None");
            comboCAT2Port.Items.AddRange(com_ports);
        }

        private void comboCAT3Port_Click(object sender, EventArgs e)
        {
            string[] com_ports = SerialPort.GetPortNames();
            comboCAT3Port.Items.Clear();
            comboCAT3Port.Items.Add("None");
            comboCAT3Port.Items.AddRange(com_ports);
        }

        private void comboCAT4Port_Click(object sender, EventArgs e)
        {
            string[] com_ports = SerialPort.GetPortNames();
            comboCAT4Port.Items.Clear();
            comboCAT4Port.Items.Add("None");
            comboCAT4Port.Items.AddRange(com_ports);
        }

        private void comboCATPTTPort_Click(object sender, EventArgs e)
        {
            string[] com_ports = SerialPort.GetPortNames();
            comboCATPTTPort.Items.Clear();
            comboCATPTTPort.Items.Add("None");
            comboCATPTTPort.Items.Add("CAT");
            comboCATPTTPort.Items.AddRange(com_ports);
        }

        private void chkDSPCESSB_CheckedChanged(object sender, EventArgs e)
        {
            console.TxOsctrl = chkDSPCESSB.Checked;
            if (chkDSPCESSB.Checked)
            {
                if (console.radio.GetDSPTX(0).TXCompandOn)
                    console.radio.GetDSPTX(0).TXOsctrlOn = true;
            }
            else
            {
                console.radio.GetDSPTX(0).TXOsctrlOn = false;
            }
        }

        private void udRXAMSQMaxTail_ValueChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXAMSquelchMaxTail = (double)udRXAMSQMaxTail.Value;
            console.radio.GetDSPRX(0, 1).RXAMSquelchMaxTail = (double)udRXAMSQMaxTail.Value;
            console.radio.GetDSPRX(1, 0).RXAMSquelchMaxTail = (double)udRXAMSQMaxTail.Value;
            console.radio.GetDSPRX(1, 1).RXAMSquelchMaxTail = (double)udRXAMSQMaxTail.Value;
        }

        private void radDSPNR2Linear_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2Linear.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXANR2GainMethod = 0;
                console.radio.GetDSPRX(0, 1).RXANR2GainMethod = 0;
            }
        }

        private void radDSPNR2Log_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2Log.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXANR2GainMethod = 1;
                console.radio.GetDSPRX(0, 1).RXANR2GainMethod = 1;
            }
        }

        private void radDSPNR2OSMS_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2OSMS.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXANR2NPEMethod = 0;
                console.radio.GetDSPRX(0, 1).RXANR2NPEMethod = 0;
            }
        }

        private void radDSPNR2MMSE_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2MMSE.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXANR2NPEMethod = 1;
                console.radio.GetDSPRX(0, 1).RXANR2NPEMethod = 1;
            }
        }

        private void chkDSPNR2AE_CheckedChanged(object sender, EventArgs e)
        {
            int run;
            if (chkDSPNR2AE.Checked)
                run = 1;
            else
                run = 0;
            console.radio.GetDSPRX(0, 0).RXANR2AERun = run;
            console.radio.GetDSPRX(0, 1).RXANR2AERun = run;
        }

        private void radDSPNR2LinearRX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2LinearRX2.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXANR2GainMethod = 0;
                console.radio.GetDSPRX(1, 1).RXANR2GainMethod = 0;
            }
        }

        private void radDSPNR2LogRX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2LogRX2.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXANR2GainMethod = 1;
                console.radio.GetDSPRX(1, 1).RXANR2GainMethod = 1;
            }
        }

        private void radDSPNR2OSMSRX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2OSMSRX2.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXANR2NPEMethod = 0;
                console.radio.GetDSPRX(1, 1).RXANR2NPEMethod = 0;
            }
        }

        private void radDSPNR2MMSERX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radDSPNR2MMSERX2.Checked)
            {
                console.radio.GetDSPRX(1, 0).RXANR2NPEMethod = 1;
                console.radio.GetDSPRX(1, 1).RXANR2NPEMethod = 1;
            }
        }

        private void chkDSPNR2AERX2_CheckedChanged(object sender, EventArgs e)
        {
            int run;
            if (chkDSPNR2AERX2.Checked)
                run = 1;
            else
                run = 0;
            console.radio.GetDSPRX(1, 0).RXANR2AERun = run;
            console.radio.GetDSPRX(1, 1).RXANR2AERun = run;
        }

        private void radDSPNR2Gamma_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(0, 0).RXANR2GainMethod = 2;
            console.radio.GetDSPRX(0, 1).RXANR2GainMethod = 2;
        }

        private void radDSPNR2GammaRX2_CheckedChanged(object sender, EventArgs e)
        {
            console.radio.GetDSPRX(1, 0).RXANR2GainMethod = 2;
            console.radio.GetDSPRX(1, 1).RXANR2GainMethod = 2;
        }

        private void chkLimitExtAmpOnOverload_CheckedChanged(object sender, EventArgs e)
        {
            console.AmpProtect = chkLimitExtAmpOnOverload.Checked;
            if (chkLimitExtAmpOnOverload.Checked)
                cmaster.SetAmpProtectRun(0, 1);
            else
                cmaster.SetAmpProtectRun(0, 0);
        }

        private void comboFocusMasterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboFocusMasterMode.Text)
            {
                case "N1MM+ Logger":
                    txtFocusMasterWinTitle.Enabled = true;
                    txtFocusMasterWinTitle.Text = "";
                    txtFocusMasterDelay.Enabled = false;
                    txtFocusMasterUDPPort.Enabled = false;
                    txtFocusMasterDelay_TextChanged(this, EventArgs.Empty);
                    txtFocusMasterUDPPort_TextChanged(this, EventArgs.Empty);
                    console.FocusMasterMode = FocusMasterMode.Logger;
                    break;

                case "Select by Click":
                    txtFocusMasterWinTitle.Enabled = false;
                    txtFocusMasterWinTitle.Text = "";
                    txtFocusMasterDelay.Enabled = false;
                    txtFocusMasterUDPPort.Enabled = true;
                    console.FocusMasterMode = FocusMasterMode.Click;
                    break;

                case "Enter Window Title":
                    txtFocusMasterDelay.Enabled = true;
                    txtFocusMasterUDPPort.Enabled = true;
                    txtFocusMasterWinTitle.Enabled = true;
                    txtFocusMasterWinTitle.Text = "Enter Window Title and Press Enter";
                    txtFocusMasterWinTitle.Focus();
                    // console.FocusMasterMode = FocusMasterMode.Title;
                    break;

                case "None":
                    txtFocusMasterDelay.Enabled = true;
                    txtFocusMasterUDPPort.Enabled = true;
                    txtFocusMasterWinTitle.Enabled = true;
                    txtFocusMasterWinTitle.Text = "";
                    console.FocusMasterMode = FocusMasterMode.None;
                    break;
            }
        }

        private void txtFocusMasterUDPPort_TextChanged(object sender, EventArgs e)
        {
            console.FocusMasterUDPPort = int.Parse(txtFocusMasterUDPPort.Text);
        }

        private void txtFocusMasterDelay_TextChanged(object sender, EventArgs e)
        {
            console.FocusMasterDelay = int.Parse(txtFocusMasterDelay.Text);
        }

        private void txtFocusMasterWinTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                IntPtr hwtemp = IntPtr.Zero;
                foreach (Process pList in Process.GetProcesses())
                {
                    if (pList.MainWindowTitle.Equals(txtFocusMasterWinTitle.Text))
                    {
                        hwtemp = pList.MainWindowHandle;
                    }
                }

                if (hwtemp != IntPtr.Zero)
                {
                    txtFocusMasterDelay.Enabled = false;
                    txtFocusMasterWinTitle.Enabled = false;
                    console.FocusMasterWinTitle = txtFocusMasterWinTitle.Text;
                    console.FocusMasterMode = FocusMasterMode.Title;
                    console.N1MMHandle = hwtemp;
                    comboFocusMasterMode.Focus();
                }
                else
                {
                    txtFocusMasterDelay.Enabled = true;
                    txtFocusMasterWinTitle.Enabled = true;
                    txtFocusMasterWinTitle.Text = "Window not found!";
                    txtFocusMasterWinTitle.Focus();
                }
            }
        }

        private void chkEnableLEDFont_CheckedChanged(object sender, EventArgs e)
        {
            console.EnableLEDFont = chkEnableLEDFont.Checked;
        }

        private void chkDisableRXOut_CheckedChanged(object sender, EventArgs e)
        {
            console.RxOutOverride = chkDisableRXOut.Checked;
            if (console.path_Illustrator != null)
                console.path_Illustrator.pi_Changed();
        }

        private void chkSplitPins_CheckedChanged(object sender, EventArgs e)
        {
            Penny.getPenny().SplitPins = chkSplitPins.Checked;
            if (chkSplitPins.Checked)
            {
                radSplitPins4x3_CheckedChanged(this, EventArgs.Empty);
                radSplitPins3x4_CheckedChanged(this, EventArgs.Empty);
            }
            else console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;
        }

        private void radSplitPins4x3_CheckedChanged(object sender, EventArgs e)
        {
            if (radSplitPins4x3.Checked && chkSplitPins.Checked)
            {
                BandBBitMask = 0x70;
                Penny.getPenny().RxABitMask = 0xf;
                UpdateOCBits();
                // console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;
            }
        }

        private void radSplitPins3x4_CheckedChanged(object sender, EventArgs e)
        {
            if (radSplitPins3x4.Checked && chkSplitPins.Checked)
            {
                BandBBitMask = 0x78;
                Penny.getPenny().RxABitMask = 0x7;
                UpdateOCBits();
                // console.PennyExtCtrlEnabled = chkPennyExtCtrl.Checked;
            }
        }

        private void udDSPSNBThresh1_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetRXASNBAk1(WDSP.id(0, 0), (double)udDSPSNBThresh1.Value);
            WDSP.SetRXASNBAk1(WDSP.id(0, 1), (double)udDSPSNBThresh1.Value);
            WDSP.SetRXASNBAk1(WDSP.id(2, 0), (double)udDSPSNBThresh1.Value);
        }

        private void udDSPSNBThresh2_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetRXASNBAk2(WDSP.id(0, 0), (double)udDSPSNBThresh2.Value);
            WDSP.SetRXASNBAk2(WDSP.id(0, 1), (double)udDSPSNBThresh2.Value);
            WDSP.SetRXASNBAk2(WDSP.id(2, 0), (double)udDSPSNBThresh2.Value);
        }

        #region MultiNotchFilter

        int numnotches = 0;
        bool AddActive = false;
        //bool RunNotches = false;
        bool EditActive = false;

        // On the Console
        // 'OFF/ON' button for Multi Notch Filter
        private void btnMNFRun_Click(object sender, EventArgs e)
        {
            //if (RunNotches)
            //{
            //    RunNotches = false;
            //    btnMNFRun.Text = "OFF";
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(0, 0), RunNotches);
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(0, 1), RunNotches);
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(2, 0), RunNotches);
            //}
            //else
            //{
            //    RunNotches = true;
            //    btnMNFRun.Text = "ON";
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(0, 0), RunNotches);
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(0, 1), RunNotches);
            //    WDSP.RXANBPSetNotchesRun(WDSP.id(2, 0), RunNotches);
            //}
        }

        private void chkMNFRun_CheckedChanged(object sender, EventArgs e)
        {
            //WDSP.RXANBPSetNotchesRun(WDSP.id(0, 0), chkMNFRun.Checked);
            //WDSP.RXANBPSetNotchesRun(WDSP.id(0, 1), chkMNFRun.Checked);
            //WDSP.RXANBPSetNotchesRun(WDSP.id(2, 0), chkMNFRun.Checked);

            //chkMNFRun.Text = "OFF";
            //chkMNFRun.Text = "ON";

        }

        // accept input for a notch to be added
        private void btnMNFAdd_Click(object sender, EventArgs e)
        {
            btnMNFAdd.BackColor = Color.Bisque;
            udMNFFreq.BackColor = Color.Bisque;
            udMNFWidth.BackColor = Color.Bisque;
            chkMNFActive.BackColor = Color.Bisque;
            udMNFFreq.Enabled = true;
            udMNFWidth.Enabled = true;
            chkMNFActive.Enabled = true;
            AddActive = true;
            btnMNFEdit.Enabled = false;
            if (EditActive)
            {
                EditActive = false;
                btnMNFEdit.BackColor = SystemColors.Control;
            }
            // get the current number of notches
            unsafe
            {
                int nn;
                WDSP.RXANBPGetNumNotches(WDSP.id(0, 0), &nn);
                numnotches = nn;
            }
            // if there are any notches already
            if (numnotches > 0)
            {
                // increase the control maximum and the notch number by 1
                udMNFNotch.Maximum += 1;
                udMNFNotch.Value += 1;
            }
            // the new notch will be inserted just after the previous one being viewed
            // zero/initialize the values in preparation for input
            udMNFFreq.Value = 0;
            udMNFWidth.Value = 0;
            chkMNFActive.Checked = true;
        }

        // accept input for editing a notch
        private void btnMNFEdit_Click(object sender, EventArgs e)
        {
            btnMNFEdit.BackColor = Color.Bisque;
            udMNFNotch.BackColor = Color.Bisque;
            udMNFFreq.BackColor = Color.Bisque;
            udMNFWidth.BackColor = Color.Bisque;
            chkMNFActive.BackColor = Color.Bisque;
            udMNFFreq.Enabled = true;
            udMNFWidth.Enabled = true;
            chkMNFActive.Enabled = true;
            EditActive = true;
            btnMNFAdd.Enabled = false;

            if (AddActive)
            {
                AddActive = false;
                btnMNFAdd.BackColor = SystemColors.Control;
            }
        }

        // store the values from an Add or Edit operation
        private void btnMNFEnter_Click(object sender, EventArgs e)
        {
            if (AddActive)
            {
                AddActive = false;
                btnMNFAdd.BackColor = SystemColors.Control;
                udMNFFreq.BackColor = SystemColors.Control;
                udMNFWidth.BackColor = SystemColors.Control;
                chkMNFActive.BackColor = SystemColors.Control;
                WDSP.RXANBPAddNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
                WDSP.RXANBPAddNotch(WDSP.id(0, 1), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
                WDSP.RXANBPAddNotch(WDSP.id(2, 0), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
                // we have at least one notch; enable the 'Delete' button
                btnMNFDelete.Enabled = true;
            }
            if (EditActive)
            {
                EditActive = false;
                btnMNFEdit.BackColor = SystemColors.Control;
                udMNFNotch.BackColor = SystemColors.Control;
                udMNFFreq.BackColor = SystemColors.Control;
                udMNFWidth.BackColor = SystemColors.Control;
                chkMNFActive.BackColor = SystemColors.Control;
                WDSP.RXANBPEditNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
                WDSP.RXANBPEditNotch(WDSP.id(0, 1), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
                WDSP.RXANBPEditNotch(WDSP.id(2, 0), (int)udMNFNotch.Value, 1.0e6 * (double)udMNFFreq.Value, (double)udMNFWidth.Value, chkMNFActive.Checked);
            }
            // no longer accepting input; disable entry of values
            udMNFFreq.Enabled = false;
            udMNFWidth.Enabled = false;
            chkMNFActive.Enabled = false;
            btnMNFAdd.Enabled = true;
            btnMNFEdit.Enabled = true;
        }

        // cancel the Add or Edit operation
        unsafe private void btnMNFCancel_Click(object sender, EventArgs e)
        {
            int nn;
            WDSP.RXANBPGetNumNotches(WDSP.id(0, 0), &nn);
            numnotches = nn;
            if (AddActive)
            {
                AddActive = false;
                if (numnotches > 0)
                {
                    // Add had potentially increased these values ... put them back the way they were
                    udMNFNotch.Value -= 1;
                    udMNFNotch.Maximum -= 1;
                }
            }
            EditActive = false;
            // reset the controls to the values for the displayed notch, if any
            if (numnotches > 0)
            {
                double fcenter, fwidth;
                int active;
                WDSP.RXANBPGetNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, &fcenter, &fwidth, &active);
                udMNFFreq.Value = (decimal)(fcenter / 1.0e6);
                udMNFWidth.Value = (decimal)fwidth;
                if (active != 0)
                    chkMNFActive.Checked = true;
                else
                    chkMNFActive.Checked = false;
            }
            btnMNFEdit.BackColor = SystemColors.Control;
            btnMNFAdd.BackColor = SystemColors.Control;
            udMNFNotch.BackColor = SystemColors.Control;
            udMNFFreq.BackColor = SystemColors.Control;
            udMNFWidth.BackColor = SystemColors.Control;
            chkMNFActive.BackColor = SystemColors.Control;
            udMNFFreq.Enabled = false;
            udMNFWidth.Enabled = false;
            chkMNFActive.Enabled = false;
            btnMNFAdd.Enabled = true;
            btnMNFEdit.Enabled = true;
        }

        // delete a notch
        private void btnMNFDelete_Click(object sender, EventArgs e)
        {
            // delete the notch
            WDSP.RXANBPDeleteNotch(WDSP.id(0, 0), (int)udMNFNotch.Value);
            WDSP.RXANBPDeleteNotch(WDSP.id(0, 1), (int)udMNFNotch.Value);
            WDSP.RXANBPDeleteNotch(WDSP.id(2, 0), (int)udMNFNotch.Value);
            // get the number of remaining notches, 'numnotches'
            unsafe
            {
                int nn;
                WDSP.RXANBPGetNumNotches(WDSP.id(0, 0), &nn);
                numnotches = nn;
            }
            // if the notch number still points to a valid notch, get the notch information
            if ((int)udMNFNotch.Value < numnotches)
            {
                unsafe
                {
                    double fcenter, fwidth;
                    int active;
                    WDSP.RXANBPGetNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, &fcenter, &fwidth, &active);
                    udMNFFreq.Value = (decimal)(fcenter / 1.0e6);
                    udMNFWidth.Value = (decimal)fwidth;
                    if (active != 0)
                        chkMNFActive.Checked = true;
                    else
                        chkMNFActive.Checked = false;
                }
            }
            // if the deleted notch was the 'top' notch, get the next notch moving down
            else if (numnotches > 0)
            {
                unsafe
                {
                    double fcenter, fwidth;
                    int active;
                    WDSP.RXANBPGetNotch(WDSP.id(0, 0), (int)udMNFNotch.Value - 1, &fcenter, &fwidth, &active);
                    udMNFNotch.Value -= 1;
                    udMNFFreq.Value = (decimal)(fcenter / 1.0e6);
                    udMNFWidth.Value = (decimal)fwidth;
                    if (active != 0)
                        chkMNFActive.Checked = true;
                    else
                        chkMNFActive.Checked = false;
                }
            }
            // if there are no remaining notches
            else
            {
                udMNFNotch.Value = 0;
                udMNFFreq.Value = 0;
                udMNFWidth.Value = 0;
                chkMNFActive.Checked = true;
                btnMNFDelete.Enabled = false;
            }
            // set the Maximum for the 'Notch' control
            if (numnotches > 0)
                udMNFNotch.Maximum = numnotches - 1;
            else
                udMNFNotch.Maximum = 0;
        }

        private void udMNFNotch_ValueChanged(object sender, EventArgs e)
        {
            unsafe
            {
                int nn;
                WDSP.RXANBPGetNumNotches(WDSP.id(0, 0), &nn);
                numnotches = nn;
                // if there's a valid notch at the new value, restore it
                if ((int)udMNFNotch.Value < numnotches)
                {
                    double fcenter, fwidth;
                    int active;
                    WDSP.RXANBPGetNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, &fcenter, &fwidth, &active);
                    udMNFFreq.Value = (decimal)(fcenter / 1.0e6);
                    udMNFWidth.Value = (decimal)fwidth;
                    if (active != 0)
                        chkMNFActive.Checked = true;
                    else
                        chkMNFActive.Checked = false;
                }
                // else, this must be an Add operation
                else
                {
                    udMNFFreq.Value = 0;
                    udMNFWidth.Value = 0;
                    chkMNFActive.Checked = true;
                }
            }
        }


        private void chkMNFAutoIncrease_CheckedChanged(object sender, EventArgs e)
        {
            WDSP.RXANBPSetAutoIncrease(WDSP.id(0, 0), chkMNFAutoIncrease.Checked);
            WDSP.RXANBPSetAutoIncrease(WDSP.id(0, 1), chkMNFAutoIncrease.Checked);
            WDSP.RXANBPSetAutoIncrease(WDSP.id(2, 0), chkMNFAutoIncrease.Checked);
        }

        unsafe public void SaveNotchesToDatabase()
        {
            // get the number of notches that exist
            int nn;
            WDSP.RXANBPGetNumNotches(WDSP.id(0, 0), &nn);
            numnotches = nn;
            // HERE:  SAVE 'numnotches', THE NUMBER OF NOTCHES, TO THE DATABASE
            MNotchDB.List.Clear();
            for (int i = 0; i < numnotches; i++)
            {
                double fcenter, fwidth;
                int active;
                // get fcenter, fwidth, and active for a notch
                WDSP.RXANBPGetNotch(WDSP.id(0, 0), i, &fcenter, &fwidth, &active);
                // HERE:  SAVE fcenter, fwidth, and active FOR THIS NOTCH TO THE DATABASE
                MNotchDB.List.Add(new MNotch(fcenter, fwidth, Convert.ToBoolean(active)));
            }
        }

        unsafe public void RestoreNotchesFromDatabase()
        {
            // HERE:  Read the number of notches, 'numnotches' from the database
            for (int i = 0; i < MNotchDB.List.Count; i++)
            {
                double fcenter = 0.0, fwidth = 0.0;
                bool active = false;

                // HERE:  READ VALUES OF fcenter, fwidth, and active FOR NOTCH[i] FROM THE DATABASE
                fcenter = MNotchDB.List[i].FCenter;
                fwidth = MNotchDB.List[i].FWidth;
                active = MNotchDB.List[i].Active;

                WDSP.RXANBPAddNotch(WDSP.id(0, 0), i, fcenter, fwidth, active);
                WDSP.RXANBPAddNotch(WDSP.id(0, 1), i, fcenter, fwidth, active);
                WDSP.RXANBPAddNotch(WDSP.id(2, 0), i, fcenter, fwidth, active);
            }
            numnotches = MNotchDB.List.Count;
            udMNFNotch.Value = 0;
            udMNFNotch.Maximum = 0;
            if (numnotches > 0)
            {
                udMNFNotch.Maximum = numnotches - 1;
                double fcenter, fwidth;
                int active;
                WDSP.RXANBPGetNotch(WDSP.id(0, 0), (int)udMNFNotch.Value, &fcenter, &fwidth, &active);
                udMNFFreq.Value = (decimal)(fcenter / 1.0e6);
                udMNFWidth.Value = (decimal)fwidth;
                if (active != 0)
                    chkMNFActive.Checked = true;
                else
                    chkMNFActive.Checked = false;

                btnMNFDelete.Enabled = true;
            }
        }

        #endregion

        private void btnVFOFreq_Click(object sender, EventArgs e)
        {
            double vfo = console.VFOAFreq;
            if (console.RIT) vfo += (double)console.RITValue * 1e-6; // check for RIT
            udMNFFreq.Value = (decimal)vfo;
        }

        private void chkAudioExpert_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkNetworkWDT_CheckedChanged(object sender, EventArgs e)
        {
            NetworkIO.SetWatchdogTimer(Convert.ToInt32(chkNetworkWDT.Checked));
        }

        private void comboRX2DispPanDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).DetTypePan = comboRX2DispPanDetector.SelectedIndex;
        }

        private void comboRX2DispPanAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).AverageMode = comboRX2DispPanAveraging.SelectedIndex;
        }

        private void comboRX2DispWFDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).DetTypeWF = comboRX2DispWFDetector.SelectedIndex;
        }

        private void comboRX2DispWFAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).AverageModeWF = comboRX2DispWFAveraging.SelectedIndex;
        }

        private void udRX2DisplayWFAVTime_ValueChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).AvTauWF = 0.001 * (double)udRX2DisplayWFAVTime.Value;
        }

        private void comboDispPanDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).DetTypePan = comboDispPanDetector.SelectedIndex;
        }

        private void comboDispWFDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).DetTypeWF = comboDispWFDetector.SelectedIndex;
        }

        private void comboDispPanAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).AverageMode = comboDispPanAveraging.SelectedIndex;
            console.UpdateRXSpectrumDisplayVars();
            console.UpdateTXSpectrumDisplayVars();
        }

        private void comboDispWFAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).AverageModeWF = comboDispWFAveraging.SelectedIndex;
        }

        private void udDisplayAVTimeWF_ValueChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).AvTauWF = 0.001 * (double)udDisplayAVTimeWF.Value;
        }

        private void chkDispRX2Normalize_CheckedChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(1).NormOneHzPan = chkDispRX2Normalize.Checked;
        }

        private void chkDispNormalize_CheckedChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(0).NormOneHzPan = chkDispNormalize.Checked;
        }

        private void comboTXDispPanDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).DetTypePan = comboTXDispPanDetector.SelectedIndex;
        }

        private void comboTXDispPanAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).AverageMode = comboTXDispPanAveraging.SelectedIndex;
        }

        private void udTXDisplayAVGTime_ValueChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).AvTau = 0.001 * (double)udTXDisplayAVGTime.Value;
        }

        private void chkDispTXNormalize_CheckedChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).NormOneHzPan = chkDispTXNormalize.Checked;
        }

        private void tbTXDisplayFFTSize_Scroll(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).FFTSize = (int)(4096 * Math.Pow(2, Math.Floor((double)(tbTXDisplayFFTSize.Value))));
            double bin_width = (double)console.specRX.GetSpecRX(cmaster.inid(1, 0)).SampleRate / (double)console.specRX.GetSpecRX(cmaster.inid(1, 0)).FFTSize;
            lblTXDispBinWidth.Text = bin_width.ToString("N3");
            // Display.TXFFTSizeOffset = tbTXDisplayFFTSize.Value * 2;
        }

        private void comboTXDispWinType_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).WindowType = comboTXDispWinType.SelectedIndex;
        }

        private void comboTXDispWFDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).DetTypeWF = comboTXDispWFDetector.SelectedIndex;
        }

        private void comboTXDispWFAveraging_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).AverageModeWF = comboTXDispWFAveraging.SelectedIndex;
        }

        private void udTXDisplayAVTime_ValueChanged(object sender, EventArgs e)
        {
            console.specRX.GetSpecRX(cmaster.inid(1, 0)).AvTauWF = 0.001 * (double)udTXDisplayAVTime.Value;
        }

        private void chkBPF2Gnd_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF2Gnd = chkBPF2Gnd.Checked;
        }

        private void ud1_5BPF1Start_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ud1_5BPF1End_ValueChanged(object sender, EventArgs e)
        {
            if (ud1_5BPF1End.Value >= ud6_5BPF1Start.Value)
            {
                ud6_5BPF1Start.Value = ud1_5BPF1End.Value + (decimal)0.000001;
                return;
            }
        }

        private void ud6_5BPF1Start_ValueChanged(object sender, EventArgs e)
        {
            if (ud6_5BPF1Start.Value <= ud1_5BPF1End.Value)
            {
                ud1_5BPF1End.Value = ud6_5BPF1Start.Value - (decimal)0.000001;
                return;
            }
        }

        private void ud6_5BPF1End_ValueChanged(object sender, EventArgs e)
        {
            if (ud6_5BPF1End.Value >= ud9_5BPF1Start.Value)
            {
                ud9_5BPF1Start.Value = ud6_5BPF1End.Value + (decimal)0.000001;
                return;
            }
        }

        private void ud9_5BPF1Start_ValueChanged(object sender, EventArgs e)
        {
            if (ud9_5BPF1Start.Value <= ud6_5BPF1End.Value)
            {
                ud6_5BPF1End.Value = ud9_5BPF1Start.Value - (decimal)0.000001;
                return;
            }
        }

        private void ud9_5BPF1End_ValueChanged(object sender, EventArgs e)
        {
            if (ud9_5BPF1End.Value >= ud13BPF1Start.Value)
            {
                ud13BPF1Start.Value = ud9_5BPF1End.Value + (decimal)0.000001;
                return;
            }
        }

        private void ud13BPF1Start_ValueChanged(object sender, EventArgs e)
        {
            if (ud13BPF1Start.Value <= ud9_5BPF1End.Value)
            {
                ud9_5BPF1End.Value = ud13BPF1Start.Value - (decimal)0.000001;
                return;
            }
        }

        private void ud13BPF1End_ValueChanged(object sender, EventArgs e)
        {
            if (ud13BPF1End.Value >= ud20BPF1Start.Value)
            {
                ud20BPF1Start.Value = ud13BPF1End.Value + (decimal)0.000001;
                return;
            }
        }

        private void ud20BPF1Start_ValueChanged(object sender, EventArgs e)
        {
            if (ud20BPF1Start.Value <= ud13BPF1End.Value)
            {
                ud13BPF1End.Value = ud20BPF1Start.Value - (decimal)0.000001;
                return;
            }
        }

        private void ud20BPF1End_ValueChanged(object sender, EventArgs e)
        {
            if (ud20BPF1End.Value >= ud6BPF1Start.Value)
            {
                ud6BPF1Start.Value = ud20BPF1End.Value + (decimal)0.000001;
                return;
            }
        }

        private void ud6BPF1Start_ValueChanged(object sender, EventArgs e)
        {
            if (ud6BPF1Start.Value <= ud20BPF1End.Value)
            {
                ud20BPF1End.Value = ud6BPF1Start.Value - (decimal)0.000001;
                return;
            }
        }

        private void ud6BPF1End_ValueChanged(object sender, EventArgs e)
        {

        }

        private void chkBPF1_1_5BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_1_5BPBypass = chkBPF1_1_5BP.Checked;
        }

        private void chkBPF1_6_5BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_6_5BPBypass = chkBPF1_6_5BP.Checked;
        }

        private void chkBPF1_9_5BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_9_5BPBypass = chkBPF1_9_5BP.Checked;
        }

        private void chkBPF1_13BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_13BPBypass = chkBPF1_13BP.Checked;
        }

        private void chkBPF1_20BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_20BPBypass = chkBPF1_20BP.Checked;
        }

        private void chkBPF1_6BP_CheckedChanged(object sender, EventArgs e)
        {
            console.BPF1_6BPBypass = chkBPF1_6BP.Checked;
        }

        private void chkANAN8000DLEDisplayVoltsAmps_CheckedChanged(object sender, EventArgs e)
        {
            console.ANAN8000DLEDisplayVoltsAmps = chkANAN8000DLEDisplayVoltsAmps.Checked;
            //console.UpdatePAVoltsAmpsDisplay();
        }

        private void ud6mRx2LNAGainOffset_ValueChanged(object sender, EventArgs e)
        {
            console.RX6mGainOffsetRx2 = (float)ud6mRx2LNAGainOffset.Value;
        }

        private void chkEnableXVTRHF_CheckedChanged(object sender, EventArgs e)
        {
            console.EnableXVTRHF = chkEnableXVTRHF.Checked;
        }

        private void chkCFCEnable_CheckedChanged(object sender, EventArgs e)
        {
            int run;
            if (chkCFCEnable.Checked) run = 1;
            else run = 0;
            WDSP.SetTXACFCOMPRun(WDSP.id(1, 0), run);
        }

        private void setCFCProfile(object sender, EventArgs e)
        {
            const int nfreqs = 10;
            double[] F = new double[nfreqs];
            double[] G = new double[nfreqs];
            double[] E = new double[nfreqs];
            F[0] = (double)udCFC0.Value;
            F[1] = (double)udCFC1.Value;
            F[2] = (double)udCFC2.Value;
            F[3] = (double)udCFC3.Value;
            F[4] = (double)udCFC4.Value;
            F[5] = (double)udCFC5.Value;
            F[6] = (double)udCFC6.Value;
            F[7] = (double)udCFC7.Value;
            F[8] = (double)udCFC8.Value;
            F[9] = (double)udCFC9.Value;
            G[0] = (double)tbCFC0.Value;
            G[1] = (double)tbCFC1.Value;
            G[2] = (double)tbCFC2.Value;
            G[3] = (double)tbCFC3.Value;
            G[4] = (double)tbCFC4.Value;
            G[5] = (double)tbCFC5.Value;
            G[6] = (double)tbCFC6.Value;
            G[7] = (double)tbCFC7.Value;
            G[8] = (double)tbCFC8.Value;
            G[9] = (double)tbCFC9.Value;
            E[0] = (double)tbCFCEQ0.Value;
            E[1] = (double)tbCFCEQ1.Value;
            E[2] = (double)tbCFCEQ2.Value;
            E[3] = (double)tbCFCEQ3.Value;
            E[4] = (double)tbCFCEQ4.Value;
            E[5] = (double)tbCFCEQ5.Value;
            E[6] = (double)tbCFCEQ6.Value;
            E[7] = (double)tbCFCEQ7.Value;
            E[8] = (double)tbCFCEQ8.Value;
            E[9] = (double)tbCFCEQ9.Value;
            unsafe
            {
                fixed (double* Fptr = &F[0], Gptr = &G[0], Eptr = &E[0])
                {
                    WDSP.SetTXACFCOMPprofile(WDSP.id(1, 0), nfreqs, Fptr, Gptr, Eptr);
                }
            }
        }

        private void tbCFCPRECOMP_Scroll(object sender, EventArgs e)
        {
            WDSP.SetTXACFCOMPPrecomp(WDSP.id(1, 0), (double)tbCFCPRECOMP.Value);
        }

        private void chkCFCPeqEnable_CheckedChanged(object sender, EventArgs e)
        {
            int run;
            if (chkCFCPeqEnable.Checked) run = 1;
            else run = 0;
            WDSP.SetTXACFCOMPPeqRun(WDSP.id(1, 0), run);
        }

        private void chkPHROTEnable_CheckedChanged(object sender, EventArgs e)
        {
            int run;
            if (chkPHROTEnable.Checked) run = 1;
            else run = 0;
            WDSP.SetTXAPHROTRun(WDSP.id(1, 0), run);
        }

        private void udPhRotFreq_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetTXAPHROTCorner(WDSP.id(1, 0), (double)udPhRotFreq.Value);
        }

        private void udPHROTStages_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetTXAPHROTNstages(WDSP.id(1, 0), (int)udPHROTStages.Value);
        }

        private void tbCFCPEG_Scroll(object sender, EventArgs e)
        {
            WDSP.SetTXACFCOMPPrePeq(WDSP.id(1, 0), (double)tbCFCPEQGAIN.Value);
        }

        private void chkBoxHTTP_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxHTTP.Checked == true)
            {

                chkBoxHttp2.Checked = false;

                if (Console.m_terminated == true)
                {
                    Debug.WriteLine("CALL HTTPSERVER1");

                    try
                    {
                        console.HttpServer = true;
                    }
                    catch (Exception e1)
                    {
                        Debug.WriteLine("bad call " + e1);
                    }

                }

            }
            else
            {
                Http.terminate();
            }
        }

        private void chkBoxHttp2_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxHttp2.Checked == true)
            {
                chkBoxHTTP.Checked = false;
               
                Http.terminate();

                console.startHttpServer((int)udHttpPort.Value);

            }
            else
            {
                console.stopHttpServer();
            }
        }

        private void udHttpPort_MouseDown(object sender, MouseEventArgs e)
        {
            Http.terminate();
            chkBoxHTTP.Checked = false;
        }

        private void udHttpPort_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txtHttpUser_MouseDown(object sender, MouseEventArgs e)
        {
            Http.terminate();
            chkBoxHTTP.Checked = false;
        }

        private void txtHttpPass_MouseDown(object sender, MouseEventArgs e)
        {
            Http.terminate();
            chkBoxHTTP.Checked = false;
        }

        private void radTXDSB_CheckedChanged(object sender, EventArgs e)
        {
            int value = 0;
            if (radTXDSB.Checked)
                value = 0;
            else if (radTXLSB.Checked)
                value = 1;
            else if (radTXUSB.Checked)
                value = 2;
            console.radio.GetDSPTX(0).SubAMMode = value;
        }

        private void btnExportCurrentTXProfile_Click(object sender, EventArgs e)
        {
            ExportCurrentTxProfile();
        }

        private void timer_VAC_Monitor_Tick(object sender, EventArgs e)
        {
            int underflows, overflows, ringsize;
            double var;
            unsafe
            {
                ivac.getIVACdiags(0, 0, &underflows, &overflows, &var, &ringsize);
            }
            lblVAC1ovfl.Text = overflows.ToString();
            lblVAC1unfl.Text = underflows.ToString();
            lblVAC1var.Text = var.ToString("F6");
            unsafe
            {
                ivac.getIVACdiags(0, 1, &underflows, &overflows, &var, &ringsize);
            }
            lblVAC1ovfl2.Text = overflows.ToString();
            lblVAC1unfl2.Text = underflows.ToString();
            lblVAC1var2.Text = var.ToString("F6");

            unsafe
            {
                ivac.getIVACdiags(1, 0, &underflows, &overflows, &var, &ringsize);
            }
            lblVAC2ovfl.Text = overflows.ToString();
            lblVAC2unfl.Text = underflows.ToString();
            lblVAC2var.Text = var.ToString("F6");
            unsafe
            {
                ivac.getIVACdiags(1, 1, &underflows, &overflows, &var, &ringsize);
            }
            lblVAC2ovfl2.Text = overflows.ToString();
            lblVAC2unfl2.Text = underflows.ToString();
            lblVAC2var2.Text = var.ToString("F6");
        }

        private void chkVAC1_Force_CheckedChanged(object sender, EventArgs e)
        {
            bool force = chkVAC1_Force.Checked;
            double fvar = (double)udVAC1_Force.Value;
            ivac.forceIVACvar(0, 0, force, fvar);
        }

        private void chkVAC1_Force2_CheckedChanged(object sender, EventArgs e)
        {
            bool force = chkVAC1_Force2.Checked;
            double fvar = (double)udVAC1_Force2.Value;
            ivac.forceIVACvar(0, 1, force, fvar);
        }

        private void chkVAC2_Force_CheckedChanged(object sender, EventArgs e)
        {
            bool force = chkVAC2_Force.Checked;
            double fvar = (double)udVAC2_Force.Value;
            ivac.forceIVACvar(1, 0, force, fvar);
        }

        private void chkVAC2_Force2_CheckedChanged(object sender, EventArgs e)
        {
            bool force = chkVAC2_Force2.Checked;
            double fvar = (double)udVAC2_Force2.Value;
            ivac.forceIVACvar(1, 1, force, fvar);
        }

        private void chkBlockTxAnt2_CheckedChanged(object sender, EventArgs e)
        {
            radAlexR_160_CheckedChanged(this, EventArgs.Empty);
            radAlexR_80_CheckedChanged(this, EventArgs.Empty);
            radAlexR_60_CheckedChanged(this, EventArgs.Empty);
            radAlexR_40_CheckedChanged(this, EventArgs.Empty);
            radAlexR_30_CheckedChanged(this, EventArgs.Empty);
            radAlexR_20_CheckedChanged(this, EventArgs.Empty);
            radAlexR_17_CheckedChanged(this, EventArgs.Empty);
            radAlexR_15_CheckedChanged(this, EventArgs.Empty);
            radAlexR_12_CheckedChanged(this, EventArgs.Empty);
            radAlexR_10_CheckedChanged(this, EventArgs.Empty);
            radAlexR_6_CheckedChanged(this, EventArgs.Empty);
        }

        private void chkBlockTxAnt3_CheckedChanged(object sender, EventArgs e)
        {
            radAlexR_160_CheckedChanged(this, EventArgs.Empty);
            radAlexR_80_CheckedChanged(this, EventArgs.Empty);
            radAlexR_60_CheckedChanged(this, EventArgs.Empty);
            radAlexR_40_CheckedChanged(this, EventArgs.Empty);
            radAlexR_30_CheckedChanged(this, EventArgs.Empty);
            radAlexR_20_CheckedChanged(this, EventArgs.Empty);
            radAlexR_17_CheckedChanged(this, EventArgs.Empty);
            radAlexR_15_CheckedChanged(this, EventArgs.Empty);
            radAlexR_12_CheckedChanged(this, EventArgs.Empty);
            radAlexR_10_CheckedChanged(this, EventArgs.Empty);
            radAlexR_6_CheckedChanged(this, EventArgs.Empty);
        }

        private void chkLPFBypass_CheckedChanged(object sender, EventArgs e)
        {
            console.LPFBypass = chkLPFBypass.Checked;
        }

        private void lblVAC1ovfl_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(0, 0);
        }

        private void lblVAC1unfl_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(0, 0);
        }

        private void lblVAC1ovfl2_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(0, 1);
        }

        private void lblVAC1unfl2_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(0, 1);
        }

        private void lblVAC2ovfl_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(1, 0);
        }

        private void lblVAC2unfl_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(1, 0);
        }

        private void lblVAC2ovfl2_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(1, 1);
        }

        private void lblVAC2unfl2_Click(object sender, EventArgs e)
        {
            ivac.resetIVACdiags(1, 1);
        }

        private void timer_LED_Mirror_Tick(object sender, EventArgs e)
        {
            int LED_data = 0;
            LED_data = cmaster.getLEDs();
            Color[] LEDColor = new Color[10];
            for (int i = 0; i < 10; i++)
                if (((LED_data >> i) & 1) == 1)
                    LEDColor[i] = Color.Red;
                else
                    LEDColor[i] = Color.Black;
            lblLED01.BackColor = LEDColor[0];
            lblLED02.BackColor = LEDColor[1];
            lblLED03.BackColor = LEDColor[2];
            lblLED04.BackColor = LEDColor[3];
            lblLED05.BackColor = LEDColor[4];
            lblLED06.BackColor = LEDColor[5];
            lblLED07.BackColor = LEDColor[6];
            lblLED08.BackColor = LEDColor[7];
            lblLED09.BackColor = LEDColor[8];
            lblLED10.BackColor = LEDColor[9];
        }
    }

    #region PADeviceInfo Helper Class

    public class PADeviceInfo
    {
        private string _Name;
        private int _Index;

        public string Name
        {
            get { return _Name; }
        }

        public int Index
        {
            get { return _Index; }
        }

        public PADeviceInfo(String argName, int argIndex)
        {
            _Name = argName;
            _Index = argIndex;
        }

        public override string ToString()
        {
            return _Name;
        }
    }

    #endregion

    #region 60m Channels Class

    public class Channel60m
    {
        private double freq;
        public Channel60m(double f)
            : base()
        {
            freq = f;
        }

        public double Freq
        {
            get
            {
                return freq;
            }
            set
            {
                freq = value;
            }
        }
    }

    #endregion

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Thetis
{
    public partial class rxaControls : DockContent //DockDotNET.DockWindow
    {
        private int fwid;
        private int stid;
        private int chid;
       // private Console console;

        public rxaControls(int id)
        {
            InitializeComponent();
            CloseButton = false;
            CloseButtonVisible = false;

            fwid = id;
            stid = fwid - 2;                                                // ChannelMaster stream id
            chid = 2 * stid;                                                // WDSP channel id
        }

        private void comboAudioSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void rxaControls_Paint(object sender, PaintEventArgs e)
        {
            this.BringToFront();
        }

        private void rxaControls_FormClosing(object sender, FormClosingEventArgs e)
        {
            cmaster.Getrxa(fwid).pDisplay.pauseDisplayThread = true;
        }

        private void rxaControls_FormClosed(object sender, FormClosedEventArgs e)
        {
            cmaster.Getrxa(fwid).pDisplay.pauseDisplayThread = false;
        }

        private void rxaControls_Activated(object sender, EventArgs e)
        {
            this.BringToFront();
            //cmaster.Getrxa(fwid).pDisplay.pauseDisplayThread = false;
        }

        private void rxaControls_Deactivate(object sender, EventArgs e)
        {
            this.BringToFront();
            //cmaster.Getrxa(fwid).pDisplay.pauseDisplayThread = false;
        }

        private void rxaControls_MdiChildActivate(object sender, EventArgs e)
        {
            this.BringToFront();
        }


    }
}

/*  Midi2Cat

Description: A subsystem that facilitates mapping Windows MIDI devices to CAT commands.
 
Copyright (C) 2016 Andrew Mansfield, M0YGG

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at:  midi2cat@cametrix.com

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Midi2Cat.IO;

namespace Thetis.Midi2Cat
{
    public partial class Midi2CatSetupForm : Form
    {
        private string DbFile;
        private List<MidiDeviceSetup> Setups;
        int startDelay = 5;
        int MsgToShow = 0;
        // The messages are spoof... we need all the time to ensure the consoles are synchronised with setup, otherwise bad things happen...
        string[] startupMessages = {"Closing Thetis Midi Input","Closing Thetis Midi Output","Attaching Midi Input and Output to Midi Setup","Midi Setup is opening Midi input and output", "Synchronising Console Controls with Midi Setup", "Initialising Database."};

        public Midi2CatSetupForm(string DbFile)
        {
            this.DbFile = DbFile;
            InitializeComponent();
        }

        private void Midi2CatSetupForm_Load(object sender, EventArgs e)
        {
            startTimer.Enabled = true;
            LoadSetup();
        }


        private void LoadSetup()
        {
            Setups = new List<MidiDeviceSetup>();
            MidiDevices devices = new MidiDevices();
            int idx = 0;
            foreach (var InDevice in devices.InDevices)
            {
                MidiDeviceSetup ctrl = new MidiDeviceSetup(DbFile, InDevice, idx++);
                Setups.Add(ctrl);
                TabPage page = new TabPage(InDevice);
                page.Controls.Add(ctrl);
                ctrl.Dock= DockStyle.Fill;
                devicesTabControl.TabPages.Add(page);
            }
        }

        private void Midi2CatSetupForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (MidiDeviceSetup setup in Setups)
            {
                setup.Parent = null;
                setup.Dispose();
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void startTimer_Tick(object sender, EventArgs e)
        {
            if (startDelay >= 0)
            {
                progressLabel.Text = startupMessages[MsgToShow++];
                startDelay--;
            }
            else
            {
                startTimer.Enabled = false;
                startupPanel.Visible = false;
                devicesTabControl.Visible = true;
            }
        }

    }
}

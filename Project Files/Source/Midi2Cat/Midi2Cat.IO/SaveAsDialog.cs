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
using System.Windows.Forms;

namespace Midi2Cat.IO
{
    public partial class SaveAsDialog : Form
    {
        public SaveAsDialog()
        {
            InitializeComponent();
        }

        public string MappingName
        {
            get
            {
                return textBox1.Text;
            }
        }

        public string[] ExistingMappings
        {
            set
            {
                mappingsLB.Items.Clear();
                foreach (string mapping in value)
                {
                    mappingsLB.Items.Add(mapping);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains('-'))
            {
                NoDashLab.Visible = true;
                saveButton.Enabled = false;
            }
            else
            {
                NoDashLab.Visible = false;
                saveButton.Enabled = (textBox1.Text.Trim().Length > 0);
            }
        }

        private void mappingsLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = (string)mappingsLB.SelectedItem;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            bool DoIt = true;
            string value=textBox1.Text.Trim().ToLower();
            foreach (var mapping in mappingsLB.Items)
            {
                if (value == ((string)mapping).ToLower())
                {
                    if (MessageBox.Show("You are about to overwrite an existing saved mapping.\nDo you want to continue?",
                        "Overwrite an existing saved mapping?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        DoIt = false;
                    }
                    break;
                }
            }
            if (DoIt )
                this.DialogResult = DialogResult.OK;
        }
    }
}

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

using Midi2Cat.Data;
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
    public partial class OrganiseDialog : Form
    {
        Midi2CatDatabase DB = null;
        string DeviceName = null;

        public OrganiseDialog(Midi2CatDatabase DB,string DeviceName)
        {
            this.DB = DB;
            this.DeviceName = DeviceName;
            InitializeComponent();
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

        private void mappingsLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            EndRename();
            if (mappingsLB.SelectedIndex >= 0)
            {
                renameButton.Enabled = true;
                deleteButton.Enabled = true;
            }
            else
            {
                renameButton.Enabled = false;
                deleteButton.Enabled = false;
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if ( mappingsLB.SelectedItem!=null)
            {
                DB.RemoveSavedMapping(DeviceName, ((string)mappingsLB.SelectedItem));
                ExistingMappings = DB.GetSavedMappings();
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            StartRename();
        }

        private void mappingsLB_DoubleClick(object sender, EventArgs e)
        {
            StartRename();
        }

        private void StartRename()
        {
            renameButton.Enabled = false;
            deleteButton.Enabled = false;
            if (mappingsLB.SelectedItem != null)
            {
                mappingsLB.BackColor = Color.Silver;
                renameTB.Left = mappingsLB.Left + 4;
                renameTB.Width = mappingsLB.Width-6;
                renameTB.Top = mappingsLB.Top + ((mappingsLB.SelectedIndex - mappingsLB.TopIndex) * mappingsLB.ItemHeight) + 2;
                renameTB.Text = ((string)mappingsLB.SelectedItem);
                renameTB.Visible = true;
                renameTB.Focus();
                renameTB.SelectAll();
            }
        }

        private void EndRename()
        {
            renameTB.Visible = false;
            mappingsLB.BackColor = Color.White;

        }

        private void renameTB_Leave(object sender, EventArgs e)
        {
            CommitRename();
        }

        private void renameTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EndRename();
            }
            else if (e.KeyCode == Keys.Return)
            {
                CommitRename();
            }
        }

        private void CommitRename()
        {
            string OldName = ((string)mappingsLB.SelectedItem);
            string NewName = renameTB.Text.Trim();
            if (NewName.Length > 0 && OldName.ToLower() != NewName.ToLower())
            {
                if (DB.GetSavedMappings().Contains(NewName) == false)
                {
                    DB.RenameSavedMapping(DeviceName, OldName, NewName);
                }
            }
            EndRename();
            ExistingMappings = DB.GetSavedMappings();
        }
    }
}

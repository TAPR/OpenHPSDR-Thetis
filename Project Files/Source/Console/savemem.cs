//=================================================================
// savemem.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
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

using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PowerSDR
{
	public class SaveMem : System.Windows.Forms.Form
	{
		#region Variable Declaration

		Console console;

		private System.Windows.Forms.ComboBoxTS cmboGroup;
		private System.Windows.Forms.LabelTS lblGroup;
		private System.Windows.Forms.LabelTS lblFrequency;
		private System.Windows.Forms.TextBoxTS textFreq;
		private System.Windows.Forms.ComboBoxTS cmboMode;
		private System.Windows.Forms.LabelTS lblMode;
		private System.Windows.Forms.LabelTS lblFilter;
		private System.Windows.Forms.ComboBoxTS cmboFilter;
		private System.Windows.Forms.TextBoxTS textCallsign;
		private System.Windows.Forms.LabelTS lblCallsign;
		private System.Windows.Forms.LabelTS lblComments;
		private System.Windows.Forms.TextBoxTS textComments;
		private System.Windows.Forms.CheckBoxTS ckScan;
		private System.Windows.Forms.LabelTS lblSquelch;
		private System.Windows.Forms.LabelTS lblStepSize;
		private System.Windows.Forms.ComboBoxTS cmboStepSize;
		private System.Windows.Forms.ButtonTS btnCancel;
		private System.Windows.Forms.ButtonTS btnOK;
		private System.Windows.Forms.LabelTS lblAGC;
		private System.Windows.Forms.ComboBoxTS cmboAGC;
		private System.Windows.Forms.NumericUpDownTS updnSquelch;
		
        private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor and Destructor

		public SaveMem(Console c)
		{
			InitializeComponent();
			console = c;

			InitAGCModes();
			InitDSPModes();

			cmboGroup.DataSource = DB.ds.Tables["GroupList"];
			cmboGroup.ValueMember = "GroupID";
			cmboGroup.DisplayMember = "GroupName";
			cmboMode.SelectedIndex = (int)console.RX1DSPMode;

            Common.RestoreForm(this, "SaveMemForm", false);

			if(console.RX1DSPMode != DSPMode.DRM &&
				console.RX1DSPMode != DSPMode.SPEC)
				cmboFilter.SelectedIndex = (int)console.RX1Filter;
			cmboStepSize.SelectedIndex = console.StepSize;
			cmboAGC.SelectedIndex = (int)console.RX1AGCMode;
			updnSquelch.Value = console.Squelch;

			textFreq.Text = console.VFOAFreq.ToString("f6");
			ckScan.Checked = true;
			cmboGroup.SelectedIndex = 0;

			this.ActiveControl = btnOK;		// OK has focus initially
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveMem));
            this.cmboGroup = new System.Windows.Forms.ComboBoxTS();
            this.lblGroup = new System.Windows.Forms.LabelTS();
            this.lblFrequency = new System.Windows.Forms.LabelTS();
            this.textFreq = new System.Windows.Forms.TextBoxTS();
            this.cmboMode = new System.Windows.Forms.ComboBoxTS();
            this.lblMode = new System.Windows.Forms.LabelTS();
            this.lblFilter = new System.Windows.Forms.LabelTS();
            this.cmboFilter = new System.Windows.Forms.ComboBoxTS();
            this.textCallsign = new System.Windows.Forms.TextBoxTS();
            this.lblCallsign = new System.Windows.Forms.LabelTS();
            this.lblComments = new System.Windows.Forms.LabelTS();
            this.textComments = new System.Windows.Forms.TextBoxTS();
            this.ckScan = new System.Windows.Forms.CheckBoxTS();
            this.lblSquelch = new System.Windows.Forms.LabelTS();
            this.lblStepSize = new System.Windows.Forms.LabelTS();
            this.cmboStepSize = new System.Windows.Forms.ComboBoxTS();
            this.btnCancel = new System.Windows.Forms.ButtonTS();
            this.btnOK = new System.Windows.Forms.ButtonTS();
            this.lblAGC = new System.Windows.Forms.LabelTS();
            this.cmboAGC = new System.Windows.Forms.ComboBoxTS();
            this.updnSquelch = new System.Windows.Forms.NumericUpDownTS();
            ((System.ComponentModel.ISupportInitialize)(this.updnSquelch)).BeginInit();
            this.SuspendLayout();
            // 
            // cmboGroup
            // 
            this.cmboGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboGroup.DropDownWidth = 112;
            this.cmboGroup.Location = new System.Drawing.Point(72, 16);
            this.cmboGroup.Name = "cmboGroup";
            this.cmboGroup.Size = new System.Drawing.Size(112, 21);
            this.cmboGroup.TabIndex = 0;
            // 
            // lblGroup
            // 
            this.lblGroup.Image = null;
            this.lblGroup.Location = new System.Drawing.Point(16, 16);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(48, 23);
            this.lblGroup.TabIndex = 1;
            this.lblGroup.Text = "Group:";
            // 
            // lblFrequency
            // 
            this.lblFrequency.Image = null;
            this.lblFrequency.Location = new System.Drawing.Point(200, 16);
            this.lblFrequency.Name = "lblFrequency";
            this.lblFrequency.Size = new System.Drawing.Size(64, 23);
            this.lblFrequency.TabIndex = 2;
            this.lblFrequency.Text = "Frequency:";
            // 
            // textFreq
            // 
            this.textFreq.Location = new System.Drawing.Point(264, 16);
            this.textFreq.Name = "textFreq";
            this.textFreq.Size = new System.Drawing.Size(88, 20);
            this.textFreq.TabIndex = 3;
            // 
            // cmboMode
            // 
            this.cmboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboMode.DropDownWidth = 112;
            this.cmboMode.Location = new System.Drawing.Point(72, 48);
            this.cmboMode.MaxDropDownItems = 12;
            this.cmboMode.Name = "cmboMode";
            this.cmboMode.Size = new System.Drawing.Size(112, 21);
            this.cmboMode.TabIndex = 4;
            this.cmboMode.SelectedIndexChanged += new System.EventHandler(this.comboMode_SelectedIndexChanged);
            // 
            // lblMode
            // 
            this.lblMode.Image = null;
            this.lblMode.Location = new System.Drawing.Point(16, 48);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(48, 23);
            this.lblMode.TabIndex = 5;
            this.lblMode.Text = "Mode:";
            // 
            // lblFilter
            // 
            this.lblFilter.Image = null;
            this.lblFilter.Location = new System.Drawing.Point(16, 80);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(48, 23);
            this.lblFilter.TabIndex = 6;
            this.lblFilter.Text = "Filter:";
            // 
            // cmboFilter
            // 
            this.cmboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboFilter.DropDownWidth = 112;
            this.cmboFilter.Items.AddRange(new object[] {
            "Filter1",
            "Filter2",
            "Filter3",
            "Filter4",
            "Filter5",
            "Filter6",
            "Filter7",
            "Filter8",
            "Filter9",
            "Filter10",
            "Var1",
            "Var2",
            "None"});
            this.cmboFilter.Location = new System.Drawing.Point(72, 80);
            this.cmboFilter.MaxDropDownItems = 13;
            this.cmboFilter.Name = "cmboFilter";
            this.cmboFilter.Size = new System.Drawing.Size(112, 21);
            this.cmboFilter.TabIndex = 7;
            // 
            // textCallsign
            // 
            this.textCallsign.Location = new System.Drawing.Point(264, 48);
            this.textCallsign.Name = "textCallsign";
            this.textCallsign.Size = new System.Drawing.Size(88, 20);
            this.textCallsign.TabIndex = 8;
            // 
            // lblCallsign
            // 
            this.lblCallsign.Image = null;
            this.lblCallsign.Location = new System.Drawing.Point(200, 48);
            this.lblCallsign.Name = "lblCallsign";
            this.lblCallsign.Size = new System.Drawing.Size(48, 24);
            this.lblCallsign.TabIndex = 9;
            this.lblCallsign.Text = "Callsign:";
            // 
            // lblComments
            // 
            this.lblComments.Image = null;
            this.lblComments.Location = new System.Drawing.Point(16, 144);
            this.lblComments.Name = "lblComments";
            this.lblComments.Size = new System.Drawing.Size(64, 23);
            this.lblComments.TabIndex = 10;
            this.lblComments.Text = "Comments:";
            // 
            // textComments
            // 
            this.textComments.Location = new System.Drawing.Point(72, 144);
            this.textComments.Name = "textComments";
            this.textComments.Size = new System.Drawing.Size(280, 20);
            this.textComments.TabIndex = 11;
            // 
            // ckScan
            // 
            this.ckScan.Image = null;
            this.ckScan.Location = new System.Drawing.Point(160, 176);
            this.ckScan.Name = "ckScan";
            this.ckScan.Size = new System.Drawing.Size(56, 24);
            this.ckScan.TabIndex = 12;
            this.ckScan.Text = "Scan";
            this.ckScan.Visible = false;
            // 
            // lblSquelch
            // 
            this.lblSquelch.Image = null;
            this.lblSquelch.Location = new System.Drawing.Point(200, 80);
            this.lblSquelch.Name = "lblSquelch";
            this.lblSquelch.Size = new System.Drawing.Size(48, 23);
            this.lblSquelch.TabIndex = 13;
            this.lblSquelch.Text = "Squelch:";
            // 
            // lblStepSize
            // 
            this.lblStepSize.Image = null;
            this.lblStepSize.Location = new System.Drawing.Point(16, 112);
            this.lblStepSize.Name = "lblStepSize";
            this.lblStepSize.Size = new System.Drawing.Size(56, 23);
            this.lblStepSize.TabIndex = 15;
            this.lblStepSize.Text = "Step Size:";
            // 
            // cmboStepSize
            // 
            this.cmboStepSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboStepSize.DropDownWidth = 112;
            this.cmboStepSize.Items.AddRange(new object[] {
            "1Hz",
            "10Hz",
            "50Hz",
            "100Hz",
            "250Hz",
            "500Hz",
            "1kHz",
            "5kHz",
            "9kHz",
            "10kHz",
            "100kHz",
            "250kHz",
            "500kHz",
            "1MHz",
            "10MHz"});
            this.cmboStepSize.Location = new System.Drawing.Point(72, 112);
            this.cmboStepSize.MaxDropDownItems = 12;
            this.cmboStepSize.Name = "cmboStepSize";
            this.cmboStepSize.Size = new System.Drawing.Size(112, 21);
            this.cmboStepSize.TabIndex = 16;
            // 
            // btnCancel
            // 
            this.btnCancel.Image = null;
            this.btnCancel.Location = new System.Drawing.Point(216, 176);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Image = null;
            this.btnOK.Location = new System.Drawing.Point(72, 176);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 18;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblAGC
            // 
            this.lblAGC.Image = null;
            this.lblAGC.Location = new System.Drawing.Point(200, 112);
            this.lblAGC.Name = "lblAGC";
            this.lblAGC.Size = new System.Drawing.Size(48, 23);
            this.lblAGC.TabIndex = 19;
            this.lblAGC.Text = "AGC:";
            // 
            // cmboAGC
            // 
            this.cmboAGC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboAGC.DropDownWidth = 88;
            this.cmboAGC.Location = new System.Drawing.Point(264, 112);
            this.cmboAGC.Name = "cmboAGC";
            this.cmboAGC.Size = new System.Drawing.Size(88, 21);
            this.cmboAGC.TabIndex = 20;
            // 
            // updnSquelch
            // 
            this.updnSquelch.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updnSquelch.Location = new System.Drawing.Point(264, 80);
            this.updnSquelch.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.updnSquelch.Minimum = new decimal(new int[] {
            160,
            0,
            0,
            -2147483648});
            this.updnSquelch.Name = "updnSquelch";
            this.updnSquelch.Size = new System.Drawing.Size(48, 20);
            this.updnSquelch.TabIndex = 21;
            this.updnSquelch.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            // 
            // SaveMem
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(368, 214);
            this.Controls.Add(this.updnSquelch);
            this.Controls.Add(this.cmboAGC);
            this.Controls.Add(this.lblAGC);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.cmboStepSize);
            this.Controls.Add(this.lblStepSize);
            this.Controls.Add(this.textComments);
            this.Controls.Add(this.textCallsign);
            this.Controls.Add(this.textFreq);
            this.Controls.Add(this.lblSquelch);
            this.Controls.Add(this.ckScan);
            this.Controls.Add(this.lblComments);
            this.Controls.Add(this.lblCallsign);
            this.Controls.Add(this.cmboFilter);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.lblMode);
            this.Controls.Add(this.cmboMode);
            this.Controls.Add(this.lblFrequency);
            this.Controls.Add(this.lblGroup);
            this.Controls.Add(this.cmboGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SaveMem";
            this.Text = "PowerSDR Save Memory Channel";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SaveMem_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.updnSquelch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region Misc Routines

		private void InitAGCModes()
		{
			for(AGCMode agc=AGCMode.FIRST+1; agc<AGCMode.LAST; agc++)
			{
				string s = agc.ToString().ToLower();
				s = s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length-1);
				cmboAGC.Items.Add(s);
			}
		}

		private void InitDSPModes()
		{
			for(DSPMode m=DSPMode.FIRST+1; m<DSPMode.LAST; m++)
				cmboMode.Items.Add(m.ToString());
		}

		#endregion

		#region Event Handlers

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
            DataRow dr = DB.ds.Tables["Memory"].NewRow();

			dr["GroupID"] = cmboGroup.SelectedIndex;

			if(textFreq.Text != "")
				dr["Freq"] = textFreq.Text;

			dr["ModeID"] = cmboMode.SelectedIndex;

			dr["FilterID"] = cmboFilter.SelectedIndex;

			if(textCallsign.Text != "")
				dr["Callsign"] = textCallsign.Text;

			if(textComments.Text != "")
				dr["Comments"] = textComments.Text;

			dr["Scan"] = (int)ckScan.CheckState;

			dr["Squelch"] = (int)updnSquelch.Value;

			dr["StepSizeID"] = cmboStepSize.SelectedIndex;

			dr["AGCID"] = cmboAGC.SelectedIndex;

            DB.ds.Tables["Memory"].Rows.Add(dr);
            if (DB.ds.Tables["Memory"].Rows.Count == 1)
				console.MemForm.SetIndex();
			this.Close();
		}

		private void comboMode_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(cmboMode.SelectedIndex < 0) return;

			int filter_index = cmboFilter.SelectedIndex;
			cmboFilter.Items.Clear();

			for(Filter f = Filter.FIRST+1; f<Filter.NONE; f++)
				cmboFilter.Items.Add(console.rx1_filters[cmboMode.SelectedIndex].GetName(f));
			cmboFilter.Items.Add("None");

			if(cmboMode.SelectedIndex == (int)DSPMode.DRM ||
				cmboMode.SelectedIndex == (int)DSPMode.SPEC)
			{
				cmboFilter.SelectedIndex = (int)Filter.NONE;
				cmboFilter.Enabled = false;
			}
			else
			{
				if(filter_index > 0) cmboFilter.SelectedIndex = filter_index;
				else cmboFilter.SelectedIndex = 0;
				cmboFilter.Enabled = true;
			}
		}

		#endregion

        private void SaveMem_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Common.SaveForm(this, "SaveMemForm");
        }
	}
}

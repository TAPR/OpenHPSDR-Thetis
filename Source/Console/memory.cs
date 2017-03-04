//=================================================================
// memory.cs
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
	public class Memory : System.Windows.Forms.Form
	{
		#region Variable Declaration

		private ArrayList modes;
		private ArrayList filters;
		private ArrayList agc_modes;
		private ArrayList step_sizes;

		private DataSet ds;

		private DataGridTableStyle ts;
		private DataGridComboBoxColumn columnGroupID;
		private DataGridTextBoxColumn columnFrequency;
		private DataGridComboBoxColumn columnMode;
		private DataGridComboBoxColumn columnFilter;
		private DataGridTextBoxColumn columnCallsign;
		private DataGridTextBoxColumn columnComments;
		//private DataGridBoolColumn columnScan;	
		private DataGridTextBoxColumn columnSquelch;
		private DataGridComboBoxColumn columnStepSize;
		private DataGridComboBoxColumn columnAGC;

		private Console console;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.ButtonTS btnHidden;
		private System.Windows.Forms.ButtonTS btnRecall;
		private System.Windows.Forms.CheckBoxTS chkEdit;
		private System.Windows.Forms.LabelTS lblFind;
		private System.Windows.Forms.ButtonTS btnFindGo;
		private System.Windows.Forms.TextBoxTS txtFilter;
		private System.Windows.Forms.ButtonTS btnDelete;
		private System.Windows.Forms.ButtonTS btnClose;
		private System.Windows.Forms.CheckBox chkCloseOnRecall;
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor and Destructor

		public Memory(Console c)
		{
			InitializeComponent();
			console = c;

			// Setup arrays
			modes = new ArrayList();
			for(DSPMode m=DSPMode.FIRST+1; m<DSPMode.LAST; m++)
				modes.Add(new DataBind((int)m, m.ToString()));
//			modes.Add(new DataBind(0, "LSB"));
//			modes.Add(new DataBind(1, "USB"));
//			modes.Add(new DataBind(2, "DSB"));
//			modes.Add(new DataBind(3, "CWL"));
//			modes.Add(new DataBind(4, "CWU"));
//			modes.Add(new DataBind(5, "FMN"));
//			modes.Add(new DataBind(6, "AM"));
//			modes.Add(new DataBind(7, "SAM"));
//			modes.Add(new DataBind(8, "SPEC"));
//			modes.Add(new DataBind(9, "DRM"));

			filters = new ArrayList();
			filters.Add(new DataBind(0, "Filter1"));
			filters.Add(new DataBind(1, "Filter2"));
			filters.Add(new DataBind(2, "Filter3"));
			filters.Add(new DataBind(3, "Filter4"));
			filters.Add(new DataBind(4, "Filter5"));
			filters.Add(new DataBind(5, "Filter6"));
			filters.Add(new DataBind(6, "Filter7"));
			filters.Add(new DataBind(7, "Filter8"));
			filters.Add(new DataBind(8, "Filter9"));
			filters.Add(new DataBind(9, "Filter10"));
			filters.Add(new DataBind(10, "Var1"));
			filters.Add(new DataBind(11, "Var2"));
			filters.Add(new DataBind(12, "None"));

			agc_modes = new ArrayList();
			for(AGCMode agc = AGCMode.FIRST+1; agc<AGCMode.LAST; agc++)
			{
				string s = agc.ToString().ToLower();
				s = s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length-1);
				agc_modes.Add(new DataBind((int)agc, s));
			}

			step_sizes = new ArrayList();
			step_sizes.Add(new DataBind(0, "1Hz"));
			step_sizes.Add(new DataBind(1, "10Hz"));
			step_sizes.Add(new DataBind(2, "50Hz"));
			step_sizes.Add(new DataBind(3, "100Hz"));
			step_sizes.Add(new DataBind(4, "250Hz"));
			step_sizes.Add(new DataBind(5, "500Hz"));
			step_sizes.Add(new DataBind(6, "1kHz"));
			step_sizes.Add(new DataBind(7, "5kHz"));
            step_sizes.Add(new DataBind(8, "9kHz"));
			step_sizes.Add(new DataBind(9, "10kHz"));
			step_sizes.Add(new DataBind(10, "100kHz"));
            step_sizes.Add(new DataBind(11, "250kHz"));
            step_sizes.Add(new DataBind(12, "500kHz"));
			step_sizes.Add(new DataBind(13, "1MHz"));
			step_sizes.Add(new DataBind(14, "10MHz"));

			// Init DataGrid components
			InitDataGrid();

			// Use database memory table
			ds = DB.ds;
			
			dataGrid1.DataSource = ds.Tables["Memory"];
			ds.Tables["Memory"].Columns["GroupID"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["Freq"].DefaultValue = 7.0;
			ds.Tables["Memory"].Columns["ModeID"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["FilterID"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["Callsign"].DefaultValue = " ";
			ds.Tables["Memory"].Columns["Comments"].DefaultValue = " ";
			//ds.Tables["Memory"].Columns["Scan"].DefaultValue = 1;
			ds.Tables["Memory"].Columns["Squelch"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["StepSizeID"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["AGCID"].DefaultValue = 2;
			ds.Tables["Memory"].Columns["Gain"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["FilterLow"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["FilterHigh"].DefaultValue = 0;
			ds.Tables["Memory"].Columns["CreateDate"].DefaultValue = DateTime.Now;

			// Bind combobox datasources
			columnGroupID.ComboBox.DataSource = 
				ds.Tables["GroupList"];
			columnGroupID.ComboBox.DisplayMember = "GroupName";
			columnGroupID.ComboBox.ValueMember = "GroupID";

			columnMode.ComboBox.DataSource = modes;
			columnMode.ComboBox.DisplayMember = "Text";
			columnMode.ComboBox.ValueMember = "Index";

			columnFilter.ComboBox.DataSource = filters;
			columnFilter.ComboBox.DisplayMember = "Text";
			columnFilter.ComboBox.ValueMember = "Index";

			columnAGC.ComboBox.DataSource = agc_modes;
			columnAGC.ComboBox.DisplayMember = "Text";
			columnAGC.ComboBox.ValueMember = "Index";

			columnStepSize.ComboBox.DataSource = step_sizes;
			columnStepSize.ComboBox.DisplayMember = "Text";
			columnStepSize.ComboBox.ValueMember = "Index";

			chkEdit_CheckedChanged(this, EventArgs.Empty);
			ds.Tables["Memory"].DefaultView.Sort = "Freq";
			dataGrid1.CurrentRowIndex = 0;

			ds.Tables["Memory"].DefaultView.AllowNew = false;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Memory));
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.btnHidden = new System.Windows.Forms.ButtonTS();
			this.btnRecall = new System.Windows.Forms.ButtonTS();
			this.chkEdit = new System.Windows.Forms.CheckBoxTS();
			this.txtFilter = new System.Windows.Forms.TextBoxTS();
			this.lblFind = new System.Windows.Forms.LabelTS();
			this.btnFindGo = new System.Windows.Forms.ButtonTS();
			this.btnDelete = new System.Windows.Forms.ButtonTS();
			this.btnClose = new System.Windows.Forms.ButtonTS();
			this.chkCloseOnRecall = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGrid1
			// 
			this.dataGrid1.AlternatingBackColor = System.Drawing.Color.Gainsboro;
			this.dataGrid1.BackColor = System.Drawing.Color.Silver;
			this.dataGrid1.CaptionBackColor = System.Drawing.SystemColors.Highlight;
			this.dataGrid1.CaptionFont = new System.Drawing.Font("Tahoma", 8F);
			this.dataGrid1.CaptionForeColor = System.Drawing.Color.White;
			this.dataGrid1.CaptionVisible = false;
			this.dataGrid1.DataMember = "";
			this.dataGrid1.FlatMode = true;
			this.dataGrid1.ForeColor = System.Drawing.Color.Black;
			this.dataGrid1.GridLineColor = System.Drawing.Color.White;
			this.dataGrid1.HeaderBackColor = System.Drawing.Color.DarkGray;
			this.dataGrid1.HeaderForeColor = System.Drawing.Color.Black;
			this.dataGrid1.LinkColor = System.Drawing.SystemColors.Highlight;
			this.dataGrid1.Location = new System.Drawing.Point(24, 16);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.Black;
			this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.White;
			this.dataGrid1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			this.dataGrid1.SelectionForeColor = System.Drawing.Color.White;
			this.dataGrid1.Size = new System.Drawing.Size(720, 368);
			this.dataGrid1.TabIndex = 0;
			this.dataGrid1.DoubleClick += new System.EventHandler(this.dataGrid1_DoubleClick);
			// 
			// btnHidden
			// 
			this.btnHidden.Image = null;
			this.btnHidden.Location = new System.Drawing.Point(232, 56);
			this.btnHidden.Name = "btnHidden";
			this.btnHidden.TabIndex = 1;
			this.btnHidden.Text = "button1";
			this.btnHidden.Visible = false;
			// 
			// btnRecall
			// 
			this.btnRecall.Image = null;
			this.btnRecall.Location = new System.Drawing.Point(120, 432);
			this.btnRecall.Name = "btnRecall";
			this.btnRecall.TabIndex = 2;
			this.btnRecall.Text = "Recall";
			this.btnRecall.Click += new System.EventHandler(this.btnRecall_Click);
			// 
			// chkEdit
			// 
			this.chkEdit.Appearance = System.Windows.Forms.Appearance.Button;
			this.chkEdit.Image = null;
			this.chkEdit.Location = new System.Drawing.Point(32, 432);
			this.chkEdit.Name = "chkEdit";
			this.chkEdit.Size = new System.Drawing.Size(75, 23);
			this.chkEdit.TabIndex = 3;
			this.chkEdit.Text = "Edit";
			this.chkEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkEdit.CheckedChanged += new System.EventHandler(this.chkEdit_CheckedChanged);
			// 
			// txtFilter
			// 
			this.txtFilter.Enabled = false;
			this.txtFilter.Location = new System.Drawing.Point(376, 432);
			this.txtFilter.Name = "txtFilter";
			this.txtFilter.Size = new System.Drawing.Size(120, 20);
			this.txtFilter.TabIndex = 4;
			this.txtFilter.Text = "";
			this.txtFilter.Visible = false;
			// 
			// lblFind
			// 
			this.lblFind.Image = null;
			this.lblFind.Location = new System.Drawing.Point(328, 432);
			this.lblFind.Name = "lblFind";
			this.lblFind.Size = new System.Drawing.Size(40, 23);
			this.lblFind.TabIndex = 5;
			this.lblFind.Text = "Filter:";
			this.lblFind.Visible = false;
			// 
			// btnFindGo
			// 
			this.btnFindGo.Enabled = false;
			this.btnFindGo.Image = null;
			this.btnFindGo.Location = new System.Drawing.Point(496, 432);
			this.btnFindGo.Name = "btnFindGo";
			this.btnFindGo.Size = new System.Drawing.Size(32, 20);
			this.btnFindGo.TabIndex = 6;
			this.btnFindGo.Text = "Go";
			this.btnFindGo.Visible = false;
			this.btnFindGo.Click += new System.EventHandler(this.btnFindGo_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Image = null;
			this.btnDelete.Location = new System.Drawing.Point(208, 432);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.TabIndex = 7;
			this.btnDelete.Text = "Delete";
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnClose
			// 
			this.btnClose.Image = null;
			this.btnClose.Location = new System.Drawing.Point(672, 432);
			this.btnClose.Name = "btnClose";
			this.btnClose.TabIndex = 8;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// chkCloseOnRecall
			// 
			this.chkCloseOnRecall.Checked = true;
			this.chkCloseOnRecall.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkCloseOnRecall.Location = new System.Drawing.Point(120, 400);
			this.chkCloseOnRecall.Name = "chkCloseOnRecall";
			this.chkCloseOnRecall.Size = new System.Drawing.Size(104, 16);
			this.chkCloseOnRecall.TabIndex = 9;
			this.chkCloseOnRecall.Text = "Close on Recall";
			// 
			// Memory
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(767, 484);
			this.Controls.Add(this.chkCloseOnRecall);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnDelete);
			this.Controls.Add(this.btnFindGo);
			this.Controls.Add(this.lblFind);
			this.Controls.Add(this.txtFilter);
			this.Controls.Add(this.chkEdit);
			this.Controls.Add(this.btnRecall);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.btnHidden);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Memory";
			this.Text = "PowerSDR Memory";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Memory_Closing);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Misc Routines

		private void InitDataGrid()
		{
			columnGroupID = new DataGridComboBoxColumn();
			columnGroupID.HeaderText = "Group";
			columnGroupID.MappingName = "GroupID";
			columnGroupID.Width = 80;
			columnGroupID.NullText = "";

			columnFrequency = new DataGridTextBoxColumn();
			columnFrequency.HeaderText = "Freq";
			columnFrequency.MappingName = "Freq";
			columnFrequency.Width = 60;
			columnFrequency.NullText = "";	
			columnFrequency.Format = "f6";

			columnMode = new DataGridComboBoxColumn();
			columnMode.HeaderText = "Mode";
			columnMode.MappingName = "ModeID";
			columnMode.Width = 60;
			columnMode.NullText = "";

			columnFilter = new DataGridComboBoxColumn();
			columnFilter.HeaderText = "Filter";
			columnFilter.MappingName = "FilterID";
			columnFilter.Width = 60;
			columnFilter.NullText = "";

			columnCallsign = new DataGridTextBoxColumn();
			columnCallsign.HeaderText = "Callsign";
			columnCallsign.MappingName = "Callsign";
			columnCallsign.Width = 50;
			columnCallsign.NullText = "";

			columnComments = new DataGridTextBoxColumn();
			columnComments.HeaderText = "Comments";
			columnComments.MappingName = "Comments";
			columnComments.Width = 160;
			columnComments.NullText = "";

			/*columnScan = new DataGridBoolColumn();
			columnScan.HeaderText = "Scan";
			columnScan.MappingName = "Scan";
			columnScan.ReadOnly = false;
			columnScan.AllowNull = false;
			columnScan.Width = 30;*/
			
			columnSquelch = new DataGridTextBoxColumn();
			columnSquelch.HeaderText = "Squelch";
			columnSquelch.MappingName = "Squelch";
			columnSquelch.Width = 50;
			columnSquelch.NullText = "";

			columnStepSize = new DataGridComboBoxColumn();
			columnStepSize.HeaderText = "Step Size";
			columnStepSize.MappingName = "StepSizeID";
			columnStepSize.Width = 70;
			columnStepSize.NullText = "";

			columnAGC = new DataGridComboBoxColumn();
			columnAGC.HeaderText = "AGC";
			columnAGC.MappingName = "AGCID";
			columnAGC.Width = 60;
			columnAGC.NullText = "";

			ts = new DataGridTableStyle();
			ts.MappingName = "Memory";
			ts.GridColumnStyles.Add(columnGroupID);	
			ts.GridColumnStyles.Add(columnFrequency);								
			ts.GridColumnStyles.Add(columnMode);
			ts.GridColumnStyles.Add(columnFilter);
			ts.GridColumnStyles.Add(columnCallsign);
			ts.GridColumnStyles.Add(columnComments);
			//ts.GridColumnStyles.Add(columnScan);
			ts.GridColumnStyles.Add(columnSquelch);
			ts.GridColumnStyles.Add(columnStepSize);
			ts.GridColumnStyles.Add(columnAGC);

			dataGrid1.TableStyles.Add(ts);

			dataGrid1.PreferredRowHeight = 
				columnGroupID.ComboBox.Height + 1;

			CopyDefaultTableStyle(dataGrid1, ts);
		}

		// Copy the display-related properties of the given DataGrid
		// to the given DataGridTableStyle
		private void CopyDefaultTableStyle(DataGrid datagrid,
			DataGridTableStyle ts)
		{
			ts.AllowSorting = datagrid.AllowSorting;
			ts.AlternatingBackColor = datagrid.AlternatingBackColor;
			ts.BackColor = datagrid.BackColor;
			ts.ColumnHeadersVisible = datagrid.ColumnHeadersVisible;
			ts.ForeColor = datagrid.ForeColor;
			ts.GridLineColor = datagrid.GridLineColor;
			ts.GridLineStyle = datagrid.GridLineStyle;
			ts.HeaderBackColor = datagrid.HeaderBackColor;
			ts.HeaderFont = datagrid.HeaderFont;
			ts.HeaderForeColor = datagrid.HeaderForeColor;
			ts.LinkColor = datagrid.LinkColor;
			ts.PreferredColumnWidth = datagrid.PreferredColumnWidth;
			ts.PreferredRowHeight = datagrid.PreferredRowHeight;
			ts.ReadOnly = datagrid.ReadOnly;
			ts.RowHeadersVisible = datagrid.RowHeadersVisible;
			ts.RowHeaderWidth = datagrid.RowHeaderWidth;
			ts.SelectionBackColor = datagrid.SelectionBackColor;
			ts.SelectionForeColor = datagrid.SelectionForeColor;
		}

		public bool ScanMemory(bool firstTime)
		{
			bool scan = false;
			DataRow dr = null;
			BindingManagerBase bm = dataGrid1.BindingContext[dataGrid1.DataSource, dataGrid1.DataMember]; 

			if(firstTime)
				bm.Position = 0;

			if(bm.Position == bm.Count-1)
			{
				bm.Position = 0;
				return false;
			}

			while(scan == false)
			{
				if(!firstTime)
					bm.Position++;
				dr = ((DataRowView)bm.Current).Row;
				
				if((bool)dr["Scan"] == true)
					scan = true;
				else if(bm.Position == bm.Count-1)
				{
					bm.Position = 0;
					return false;
				}

				firstTime = false;
			}
		
			btnRecall_Click(this, EventArgs.Empty);
			return true;
		}

		public void SetIndex()
		{
			dataGrid1.CurrentRowIndex = 0;
		}

		#endregion

		#region Event Handlers

		private void btnRecall_Click(object sender, System.EventArgs e)
		{
			BindingManagerBase bm = dataGrid1.BindingContext[dataGrid1.DataSource, dataGrid1.DataMember]; 

			int mode, filter, step, agc, squelch;
			double freq;

			if(bm.Position < 0)		// index is not valid
				return;

			DataRow dr = ((DataRowView)bm.Current).Row;
			mode = (int)dr["ModeID"];
			filter = (int)dr["FilterID"];
			freq = (double)dr["Freq"];
			step = (int)dr["StepSizeID"];
			agc = (int)dr["AGCID"];
			squelch = (int)dr["Squelch"];

			console.MemoryRecall(mode, filter, freq, step, agc, squelch);
			if(chkCloseOnRecall.Checked)
				this.Hide();
		}

		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			BindingManagerBase bm = dataGrid1.BindingContext[dataGrid1.DataSource, dataGrid1.DataMember];

			if(bm.Position < 0)
				return;

			DialogResult dr = MessageBox.Show("Are you sure you want to delete this memory?",
				"Delete Memory?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if(dr == DialogResult.No)
				return;

			bm.RemoveAt(bm.Position);
		}

		private void dataGrid1_DoubleClick(object sender, System.EventArgs e)
		{
			btnRecall_Click(sender, e);
		}

		private void chkEdit_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkEdit.Checked)
				chkEdit.BackColor = Color.Yellow;
			else
				chkEdit.BackColor = SystemColors.Control;

			//ds.Tables["Memory"].DefaultView.AllowNew = chkEdit.Checked;
			dataGrid1.TableStyles[0].ReadOnly = !chkEdit.Checked;
		}
		
		private void txtFind_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyData == Keys.Enter)
				btnFindGo_Click(sender, EventArgs.Empty);
		}

		private void Memory_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.ActiveControl = btnHidden;
			this.Hide();
			btnClose.Focus();
			//dataGrid1.CurrentRowIndex++;		// in order to save changes to current row
			// dataGrid1.CurrentRowIndex--;		// in order to save changes to current row
			e.Cancel = true;
		}

		private void btnFindGo_Click(object sender, System.EventArgs e)
		{
			txtFilter.Enabled = false;
			string filterStr = txtFilter.Text.Trim();
			ds.Tables["Memory"].DefaultView.RowFilter = filterStr;
			txtFilter.Enabled = true;
		}

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion
	}	

	#region DataBind Code

	public class DataBind
	{
		private int index;
		private string text;

		public DataBind(int i, string s)
		{
			index = i;
			text = s;
		}

		public int Index
		{
			get { return index; }
		}

		public string Text
		{
			get { return text; }
		}
	}

	#endregion

	#region ComboBoxColumn Code

	// Derive class from DataGridTextBoxColumn
	public class DataGridComboBoxColumn : DataGridTextBoxColumn
	{
		// Hosted ComboBox control
		private ComboBoxTS comboBox;
		private CurrencyManager cm;
		private int iCurrentRow;
		
		// Constructor - create combobox, register selection change event handler,
		// register lose focus event handler
		public DataGridComboBoxColumn()
		{
			this.cm = null;

			// Create ComboBox and force DropDownList style
			this.comboBox = new ComboBoxTS();
			this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

			// Add event handler for notification of when ComboBox loses focus
			this.comboBox.Leave += new EventHandler(comboBox_Leave);
		}
		
		// Property to provide access to ComboBox
		public ComboBoxTS ComboBox
		{
			get { return comboBox; }
		}																				 
																									
		// On edit, add scroll event handler, and display combo box
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);

			if (!readOnly && cellIsVisible)
			{
				// Save current row in the datagrid and currency manager associated with
				// the data source for the datagrid
				this.iCurrentRow = rowNum;
				this.cm = source;

				// Add event handler for datagrid scroll notification
				this.DataGridTableStyle.DataGrid.Scroll += new EventHandler(DataGrid_Scroll);

				// Site the combo box control within the bounds of the current cell
				this.comboBox.Parent = this.TextBox.Parent;
				Rectangle rect = this.DataGridTableStyle.DataGrid.GetCurrentCellBounds();
				this.comboBox.Location = rect.Location;
				this.comboBox.Size = new Size(this.TextBox.Size.Width, this.comboBox.Size.Height);

				// Set combo box selection to given text
				this.comboBox.SelectedIndex = this.comboBox.FindStringExact(this.TextBox.Text);

				// Make the ComboBox visible and place on top text box control
				this.comboBox.Show();
				this.comboBox.BringToFront();
				this.comboBox.Focus();
			}
			else
			{
				this.comboBox.Hide();
			}
		}

		// Given a row, get the value member associated with a row.  Use the value
		// member to find the associated display member by iterating over bound datasource
		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			// Given a row number in the datagrid, get the display member
			object obj =  base.GetColumnValueAtRow(source, rowNum);

			// Iterate through the datasource bound to the ColumnComboBox
			// Don't confuse this datasource with the datasource of the associated datagrid
			CurrencyManager cm = (CurrencyManager) 
				(this.DataGridTableStyle.DataGrid.BindingContext[this.comboBox.DataSource]);
			// Assumes the associated DataGrid is bound to a DataView, or DataTable that
			// implements a default DataView
			if(cm.List.GetType() == typeof(DataView))
			{
				DataView dataview = ((DataView)cm.List);
				int i;
				for (i = 0; i < dataview.Count; i++)
				{
					if (obj.Equals(dataview[i][this.comboBox.ValueMember]))
						break;
				}

				if (i < dataview.Count)
					return dataview[i][this.comboBox.DisplayMember];

				return DBNull.Value;
			}
			else if(cm.List.GetType() == typeof(ArrayList))
			{
				ArrayList array = ((ArrayList)cm.List);
				int i;
				for(i=0; i<array.Count; i++)
				{
					if((int)obj == ((DataBind)array[i]).Index)
						break;
				}

				if(i<array.Count)
					return ((DataBind)array[i]).Text;
				else return DBNull.Value;
			}
			else return DBNull.Value;
		}

		// Given a row and a display member, iterating over bound datasource to find
		// the associated value member.  Set this value member.
		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			object s = value;

			// Iterate through the datasource bound to the ColumnComboBox
			// Don't confuse this datasource with the datasource of the associated datagrid
			CurrencyManager cm = (CurrencyManager) 
				(this.DataGridTableStyle.DataGrid.BindingContext[this.comboBox.DataSource]);
			// Assumes the associated DataGrid is bound to a DataView, or DataTable that
			// implements a default DataView
			if(cm.List.GetType() == typeof(DataView))
			{
				DataView dataview = ((DataView)cm.List);
				int i;

				for (i = 0; i < dataview.Count; i++)
				{
					if (s.Equals(dataview[i][this.comboBox.DisplayMember]))
						break;
				}

				// If set item was found return corresponding value, otherwise return DbNull.Value
				if(i < dataview.Count)
					s =  dataview[i][this.comboBox.ValueMember];
				else
					s = DBNull.Value;
			}
			else if(cm.List.GetType() == typeof(ArrayList))
			{
				ArrayList array = (ArrayList)cm.List;
				int i;
				for(i=0; i<array.Count; i++)
				{
					if((string)s == ((DataBind)array[i]).Text)
						break;
				}

				if(i<array.Count)
					s = ((DataBind)array[i]).Index;
				else
					s = DBNull.Value;
			}
			else s = DBNull.Value;

			if(source.Position == rowNum)
			{
				base.SetColumnValueAtRow(source, rowNum, s);
			}
			
		}

		// On datagrid scroll, hide the combobox
		private void DataGrid_Scroll(object sender, EventArgs e)
		{
			this.comboBox.Hide();
		}

		// On combo box losing focus, set the column value, hide the combo box,
		// and unregister scroll event handler
		private void comboBox_Leave(object sender, EventArgs e)
		{		
			string s;
			if(this.comboBox.SelectedItem.GetType() == typeof(DataRowView))
			{
				DataRowView rowView = (DataRowView) this.comboBox.SelectedItem;
				s = (string) rowView.Row[this.comboBox.DisplayMember];
			}
			else if(this.comboBox.SelectedItem.GetType() == typeof(DataBind))
			{
				s = ((DataBind)this.ComboBox.SelectedItem).Text;
			}
			else return;

			SetColumnValueAtRow(this.cm, this.iCurrentRow, s);
			Invalidate();

			this.comboBox.Hide();
			this.DataGridTableStyle.DataGrid.Scroll -= new EventHandler(DataGrid_Scroll);			
		}
	}

	#endregion

}

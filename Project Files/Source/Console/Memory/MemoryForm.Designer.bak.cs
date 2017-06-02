//=================================================================
// MEmoryForm.Designer.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2011  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

namespace Thetis
{
    partial class MemoryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemoryForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkMemoryFormClose = new System.Windows.Forms.CheckBoxTS();
            this.btnSelect = new System.Windows.Forms.ButtonTS();
            this.btnMemoryRecordDelete = new System.Windows.Forms.ButtonTS();
            this.btnMemoryRecordCopy = new System.Windows.Forms.ButtonTS();
            this.MemoryRecordAdd = new System.Windows.Forms.ButtonTS();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(982, 423);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.DoubleClick += new System.EventHandler(this.btnSelect_Click);
            // 
            // chkMemoryFormClose
            // 
            this.chkMemoryFormClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMemoryFormClose.Image = null;
            this.chkMemoryFormClose.Location = new System.Drawing.Point(411, 453);
            this.chkMemoryFormClose.Name = "chkMemoryFormClose";
            this.chkMemoryFormClose.Size = new System.Drawing.Size(151, 20);
            this.chkMemoryFormClose.TabIndex = 12;
            this.chkMemoryFormClose.Text = "Close after selection";
            this.toolTip1.SetToolTip(this.chkMemoryFormClose, "Check to close the Memory window after an entry has been selected");
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSelect.Image = null;
            this.btnSelect.Location = new System.Drawing.Point(315, 450);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "Select";
            this.toolTip1.SetToolTip(this.btnSelect, "Make the selected memory active ");
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnMemoryRecordDelete
            // 
            this.btnMemoryRecordDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMemoryRecordDelete.Image = null;
            this.btnMemoryRecordDelete.Location = new System.Drawing.Point(174, 450);
            this.btnMemoryRecordDelete.Name = "btnMemoryRecordDelete";
            this.btnMemoryRecordDelete.Size = new System.Drawing.Size(75, 23);
            this.btnMemoryRecordDelete.TabIndex = 4;
            this.btnMemoryRecordDelete.Text = "Delete";
            this.toolTip1.SetToolTip(this.btnMemoryRecordDelete, "Delete the current row");
            this.btnMemoryRecordDelete.UseVisualStyleBackColor = true;
            this.btnMemoryRecordDelete.Click += new System.EventHandler(this.btnMemoryRecordDelete_Click);
            // 
            // btnMemoryRecordCopy
            // 
            this.btnMemoryRecordCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMemoryRecordCopy.Image = null;
            this.btnMemoryRecordCopy.Location = new System.Drawing.Point(93, 450);
            this.btnMemoryRecordCopy.Name = "btnMemoryRecordCopy";
            this.btnMemoryRecordCopy.Size = new System.Drawing.Size(75, 23);
            this.btnMemoryRecordCopy.TabIndex = 3;
            this.btnMemoryRecordCopy.Text = "Copy";
            this.toolTip1.SetToolTip(this.btnMemoryRecordCopy, "Create a new row with the same values as the currently selected row");
            this.btnMemoryRecordCopy.UseVisualStyleBackColor = true;
            this.btnMemoryRecordCopy.Click += new System.EventHandler(this.btnMemoryRecordCopy_Click);
            // 
            // MemoryRecordAdd
            // 
            this.MemoryRecordAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemoryRecordAdd.Image = null;
            this.MemoryRecordAdd.Location = new System.Drawing.Point(12, 450);
            this.MemoryRecordAdd.Name = "MemoryRecordAdd";
            this.MemoryRecordAdd.Size = new System.Drawing.Size(75, 23);
            this.MemoryRecordAdd.TabIndex = 2;
            this.MemoryRecordAdd.Text = "Add";
            this.toolTip1.SetToolTip(this.MemoryRecordAdd, "Add a new Memory entry based on the current console settings");
            this.MemoryRecordAdd.UseVisualStyleBackColor = true;
            this.MemoryRecordAdd.Click += new System.EventHandler(this.MemoryRecordAdd_Click);
            // 
            // MemoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1006, 488);
            this.Controls.Add(this.chkMemoryFormClose);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnMemoryRecordDelete);
            this.Controls.Add(this.btnMemoryRecordCopy);
            this.Controls.Add(this.MemoryRecordAdd);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MemoryForm";
            this.Text = "Memory Interface";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MemoryForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ButtonTS MemoryRecordAdd;
        private System.Windows.Forms.ButtonTS btnMemoryRecordCopy;
        private System.Windows.Forms.ButtonTS btnMemoryRecordDelete;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ButtonTS btnSelect;
        private System.Windows.Forms.CheckBoxTS chkMemoryFormClose;
    }
}
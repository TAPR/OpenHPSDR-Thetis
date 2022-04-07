namespace Thetis
{
    partial class frmIPv4Picker
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
            this.btnCancel = new System.Windows.Forms.ButtonTS();
            this.btnSelect = new System.Windows.Forms.ButtonTS();
            this.comboAddresses = new System.Windows.Forms.ComboBoxTS();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = null;
            this.btnCancel.Location = new System.Drawing.Point(81, 167);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(52, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSelect.Image = null;
            this.btnSelect.Location = new System.Drawing.Point(12, 167);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(52, 23);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // comboAddresses
            // 
            this.comboAddresses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboAddresses.FormattingEnabled = true;
            this.comboAddresses.Items.AddRange(new object[] {
            "192",
            "168",
            "0",
            "26"});
            this.comboAddresses.Location = new System.Drawing.Point(12, 12);
            this.comboAddresses.Name = "comboAddresses";
            this.comboAddresses.Size = new System.Drawing.Size(125, 150);
            this.comboAddresses.TabIndex = 0;
            // 
            // frmIPv4Picker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(149, 202);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.comboAddresses);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmIPv4Picker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IPv4 Address";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBoxTS comboAddresses;
        private System.Windows.Forms.ButtonTS btnSelect;
        private System.Windows.Forms.ButtonTS btnCancel;
    }
}
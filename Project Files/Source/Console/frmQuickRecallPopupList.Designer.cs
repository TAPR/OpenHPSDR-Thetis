
namespace Thetis
{
    partial class frmQuickRecallPopupList
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
            this.lstboxFrequencies = new Thetis.QuickRecallListBox();
            this.SuspendLayout();
            // 
            // lstboxFrequencies
            // 
            this.lstboxFrequencies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstboxFrequencies.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstboxFrequencies.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstboxFrequencies.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstboxFrequencies.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstboxFrequencies.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lstboxFrequencies.FormattingEnabled = true;
            this.lstboxFrequencies.ItemHeight = 20;
            this.lstboxFrequencies.Location = new System.Drawing.Point(8, 5);
            this.lstboxFrequencies.Name = "lstboxFrequencies";
            this.lstboxFrequencies.Size = new System.Drawing.Size(105, 320);
            this.lstboxFrequencies.TabIndex = 0;
            this.lstboxFrequencies.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstboxFrequencies_MouseClick);
            // 
            // frmQuickRecallPopupList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(120, 335);
            this.Controls.Add(this.lstboxFrequencies);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmQuickRecallPopupList";
            this.Text = "frmQuickRecallPopupList";
            this.ResumeLayout(false);

        }

        #endregion

        private QuickRecallListBox lstboxFrequencies;
    }
}
namespace Thetis
{
    partial class frmMeterDisplay
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
            this.picContainer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picContainer)).BeginInit();
            this.SuspendLayout();
            // 
            // picContainer
            // 
            this.picContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picContainer.Location = new System.Drawing.Point(4, 4);
            this.picContainer.Name = "picContainer";
            this.picContainer.Size = new System.Drawing.Size(276, 252);
            this.picContainer.TabIndex = 0;
            this.picContainer.TabStop = false;
            // 
            // frmMeterDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.picContainer);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "frmMeterDisplay";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RX";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMeterDisplay_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picContainer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picContainer;
    }
}
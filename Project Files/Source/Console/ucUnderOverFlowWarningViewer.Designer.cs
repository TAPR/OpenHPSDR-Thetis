namespace Thetis
{
    partial class ucUnderOverFlowWarningViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tmrFade = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrFade
            // 
            this.tmrFade.Tick += new System.EventHandler(this.tmrFade_Tick);
            // 
            // UnderOverFlowWarningViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Transparent;
            this.MaximumSize = new System.Drawing.Size(15, 15);
            this.MinimumSize = new System.Drawing.Size(15, 15);
            this.Name = "UnderOverFlowWarningViewer";
            this.Size = new System.Drawing.Size(15, 15);
            this.Load += new System.EventHandler(this.UnderOverFlowWarningViewer_Load);
            this.Click += new System.EventHandler(this.UnderOverFlowWarningViewer_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.UnderOverFlowWarningViewer_Paint);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer tmrFade;
    }
}


namespace Thetis
{
    partial class ucLGPicker
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
            this.SuspendLayout();
            // 
            // ucLGPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "ucLGPicker";
            this.Size = new System.Drawing.Size(315, 53);
            this.EnabledChanged += new System.EventHandler(this.LGPicker_EnabledChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LGPicker_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LGPicker_MouseDown);
            this.MouseLeave += new System.EventHandler(this.LGPicker_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LGPicker_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LGPicker_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
    }
}

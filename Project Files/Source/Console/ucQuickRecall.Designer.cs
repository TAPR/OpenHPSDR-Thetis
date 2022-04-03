
namespace Thetis
{
    partial class ucQuickRecall
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
            this.btnList = new System.Windows.Forms.ButtonTS();
            this.btnNext = new System.Windows.Forms.ButtonTS();
            this.btnPrevious = new System.Windows.Forms.ButtonTS();
            this.lblFlashColour = new System.Windows.Forms.LabelTS();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btnList
            // 
            this.btnList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnList.FlatAppearance.BorderSize = 0;
            this.btnList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnList.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnList.Image = null;
            this.btnList.Location = new System.Drawing.Point(20, 0);
            this.btnList.Name = "btnList";
            this.btnList.Size = new System.Drawing.Size(20, 20);
            this.btnList.TabIndex = 2;
            this.btnList.TabStop = false;
            this.btnList.Text = "V";
            this.toolTip1.SetToolTip(this.btnList, "QuickRecall will store a frequency/mode if you stay on that frequency for 4 secon" +
        "ds. Click to show list.");
            this.btnList.UseVisualStyleBackColor = false;
            this.btnList.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnNext
            // 
            this.btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNext.Image = null;
            this.btnNext.Location = new System.Drawing.Point(40, 0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(20, 20);
            this.btnNext.TabIndex = 1;
            this.btnNext.TabStop = false;
            this.btnNext.Text = ">";
            this.toolTip1.SetToolTip(this.btnNext, "Next frequency in QuickRecall list");
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPrevious.FlatAppearance.BorderSize = 0;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevious.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnPrevious.Image = null;
            this.btnPrevious.Location = new System.Drawing.Point(0, 0);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(20, 20);
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.TabStop = false;
            this.btnPrevious.Text = "<";
            this.toolTip1.SetToolTip(this.btnPrevious, "Previous frequency in QuickRecall list");
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // lblFlashColour
            // 
            this.lblFlashColour.BackColor = System.Drawing.Color.Red;
            this.lblFlashColour.Image = null;
            this.lblFlashColour.Location = new System.Drawing.Point(25, 6);
            this.lblFlashColour.Name = "lblFlashColour";
            this.lblFlashColour.Size = new System.Drawing.Size(14, 14);
            this.lblFlashColour.TabIndex = 3;
            this.toolTip1.SetToolTip(this.lblFlashColour, "Green = just added, Orange = udated mode");
            this.lblFlashColour.Click += new System.EventHandler(this.lblFlashColour_Click);
            // 
            // ucQuickRecall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblFlashColour);
            this.Controls.Add(this.btnList);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Name = "ucQuickRecall";
            this.Size = new System.Drawing.Size(89, 34);
            this.BackColorChanged += new System.EventHandler(this.ucQuickRecall_BackColorChanged);
            this.Resize += new System.EventHandler(this.ucQuickRecall_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ButtonTS btnPrevious;
        private System.Windows.Forms.ButtonTS btnNext;
        private System.Windows.Forms.ButtonTS btnList;
        private System.Windows.Forms.LabelTS lblFlashColour;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

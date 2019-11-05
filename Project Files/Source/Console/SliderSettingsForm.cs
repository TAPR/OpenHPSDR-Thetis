//=================================================================
// SliderSettingsForm.cs
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
//=================================================================using System;

namespace Thetis
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// Analogue sliders view for collapsed display.
    /// </summary>
    public class SliderSettingsForm : System.Windows.Forms.Form
    {
        #region Variable Declaration

        private Console console;
        private System.Windows.Forms.TrackBarTS tbRX1AF;
        private GroupBoxTS grpRX1;
        private LabelTS lblPan;
        private TrackBarTS tbRX1Pan;
        private TrackBarTS tbRX1Sql;
        private TrackBarTS tbRX1RF;
        private GroupBoxTS grpRX2;
        private TrackBarTS tbRX2Pan;
        private TrackBarTS tbRX2Sql;
        private TrackBarTS tbRX2RF;
        private TrackBarTS tbRX2AF;
        private GroupBoxTS grpSubRX;
        private TrackBarTS tbSubRXPan;
        private TrackBarTS tbDrive;
        private TrackBarTS tbMasterAF;
        private TrackBarTS tbSubRXAF;
        private LabelTS lblDrive;
        private LabelTS lblMasterAF;
        private CheckBoxTS chkRX1Sql;
        private CheckBoxTS chkRX2Sql;
        private ButtonTS btnClose;
        private LabelTS labelTS1;
        private LabelTS labelTS5;
        private LabelTS labelTS4;
        private LabelTS labelTS11;
        private TrackBarTS tbRX2Atten;
        private CheckBoxTS chkRX2Mute;
        private LabelTS labelTS3;
        private LabelTS labelTS2;
        private LabelTS labelTS10;
        private TrackBarTS tbRX1Atten;
        private LabelTS labelTS7;
        private CheckBoxTS chkRX1Mute;
        private LabelTS labelTS8;
        private LabelTS labelTS9;
        private LabelTS labelTS6;
        private CheckBoxTS chkSubRX;
        private System.ComponentModel.IContainer components = null;


        #endregion

        #region Constructor and Destructor

        public SliderSettingsForm(Console c)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            console = c;
        }

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

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnClose = new System.Windows.Forms.ButtonTS();
            this.lblMasterAF = new System.Windows.Forms.LabelTS();
            this.lblDrive = new System.Windows.Forms.LabelTS();
            this.grpSubRX = new System.Windows.Forms.GroupBoxTS();
            this.chkSubRX = new System.Windows.Forms.CheckBoxTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.tbSubRXPan = new System.Windows.Forms.TrackBarTS();
            this.tbSubRXAF = new System.Windows.Forms.TrackBarTS();
            this.tbDrive = new System.Windows.Forms.TrackBarTS();
            this.grpRX2 = new System.Windows.Forms.GroupBoxTS();
            this.lblPan = new System.Windows.Forms.LabelTS();
            this.labelTS11 = new System.Windows.Forms.LabelTS();
            this.tbRX2Atten = new System.Windows.Forms.TrackBarTS();
            this.chkRX2Mute = new System.Windows.Forms.CheckBoxTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.chkRX2Sql = new System.Windows.Forms.CheckBoxTS();
            this.tbRX2Pan = new System.Windows.Forms.TrackBarTS();
            this.tbRX2Sql = new System.Windows.Forms.TrackBarTS();
            this.tbRX2RF = new System.Windows.Forms.TrackBarTS();
            this.tbRX2AF = new System.Windows.Forms.TrackBarTS();
            this.tbMasterAF = new System.Windows.Forms.TrackBarTS();
            this.grpRX1 = new System.Windows.Forms.GroupBoxTS();
            this.labelTS10 = new System.Windows.Forms.LabelTS();
            this.tbRX1Atten = new System.Windows.Forms.TrackBarTS();
            this.labelTS7 = new System.Windows.Forms.LabelTS();
            this.chkRX1Mute = new System.Windows.Forms.CheckBoxTS();
            this.labelTS8 = new System.Windows.Forms.LabelTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.chkRX1Sql = new System.Windows.Forms.CheckBoxTS();
            this.tbRX1Pan = new System.Windows.Forms.TrackBarTS();
            this.tbRX1Sql = new System.Windows.Forms.TrackBarTS();
            this.tbRX1RF = new System.Windows.Forms.TrackBarTS();
            this.tbRX1AF = new System.Windows.Forms.TrackBarTS();
            this.grpSubRX.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSubRXPan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSubRXAF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbDrive)).BeginInit();
            this.grpRX2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Atten)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Pan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Sql)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2RF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2AF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMasterAF)).BeginInit();
            this.grpRX1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Atten)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Pan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Sql)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1RF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1AF)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Image = null;
            this.btnClose.Location = new System.Drawing.Point(522, 267);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 40);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblMasterAF
            // 
            this.lblMasterAF.AutoSize = true;
            this.lblMasterAF.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.lblMasterAF.Image = null;
            this.lblMasterAF.Location = new System.Drawing.Point(112, 299);
            this.lblMasterAF.Name = "lblMasterAF";
            this.lblMasterAF.Size = new System.Drawing.Size(80, 13);
            this.lblMasterAF.TabIndex = 10;
            this.lblMasterAF.Text = "Master AF Gain";
            // 
            // lblDrive
            // 
            this.lblDrive.AutoSize = true;
            this.lblDrive.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.lblDrive.Image = null;
            this.lblDrive.Location = new System.Drawing.Point(368, 299);
            this.lblDrive.Name = "lblDrive";
            this.lblDrive.Size = new System.Drawing.Size(49, 13);
            this.lblDrive.TabIndex = 9;
            this.lblDrive.Text = "TX Drive";
            // 
            // grpSubRX
            // 
            this.grpSubRX.Controls.Add(this.chkSubRX);
            this.grpSubRX.Controls.Add(this.labelTS5);
            this.grpSubRX.Controls.Add(this.labelTS4);
            this.grpSubRX.Controls.Add(this.tbSubRXPan);
            this.grpSubRX.Controls.Add(this.tbSubRXAF);
            this.grpSubRX.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.grpSubRX.Location = new System.Drawing.Point(478, 9);
            this.grpSubRX.Name = "grpSubRX";
            this.grpSubRX.Size = new System.Drawing.Size(155, 249);
            this.grpSubRX.TabIndex = 8;
            this.grpSubRX.TabStop = false;
            this.grpSubRX.Text = "Sub RX";
            // 
            // chkSubRX
            // 
            this.chkSubRX.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkSubRX.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkSubRX.Image = null;
            this.chkSubRX.Location = new System.Drawing.Point(30, 164);
            this.chkSubRX.Name = "chkSubRX";
            this.chkSubRX.Size = new System.Drawing.Size(110, 28);
            this.chkSubRX.TabIndex = 11;
            this.chkSubRX.Text = "Enable Sub RX";
            this.chkSubRX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkSubRX.UseVisualStyleBackColor = true;
            this.chkSubRX.CheckedChanged += new System.EventHandler(this.chkSubRX_CheckedChanged);
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(52, 230);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(48, 13);
            this.labelTS5.TabIndex = 10;
            this.labelTS5.Text = "L/R Pan";
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(61, 16);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(20, 13);
            this.labelTS4.TabIndex = 10;
            this.labelTS4.Text = "AF";
            // 
            // tbSubRXPan
            // 
            this.tbSubRXPan.Location = new System.Drawing.Point(5, 198);
            this.tbSubRXPan.Maximum = 100;
            this.tbSubRXPan.Name = "tbSubRXPan";
            this.tbSubRXPan.Size = new System.Drawing.Size(144, 45);
            this.tbSubRXPan.TabIndex = 4;
            this.tbSubRXPan.Scroll += new System.EventHandler(this.tbSubRXPan_Scroll);
            // 
            // tbSubRXAF
            // 
            this.tbSubRXAF.Location = new System.Drawing.Point(55, 32);
            this.tbSubRXAF.Maximum = 100;
            this.tbSubRXAF.Name = "tbSubRXAF";
            this.tbSubRXAF.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbSubRXAF.Size = new System.Drawing.Size(45, 131);
            this.tbSubRXAF.TabIndex = 1;
            this.tbSubRXAF.Scroll += new System.EventHandler(this.tbSubRXAF_Scroll);
            // 
            // tbDrive
            // 
            this.tbDrive.Location = new System.Drawing.Point(326, 267);
            this.tbDrive.Maximum = 100;
            this.tbDrive.Name = "tbDrive";
            this.tbDrive.Size = new System.Drawing.Size(150, 45);
            this.tbDrive.TabIndex = 3;
            this.tbDrive.Scroll += new System.EventHandler(this.tbDrive_Scroll);
            // 
            // grpRX2
            // 
            this.grpRX2.Controls.Add(this.lblPan);
            this.grpRX2.Controls.Add(this.labelTS11);
            this.grpRX2.Controls.Add(this.tbRX2Atten);
            this.grpRX2.Controls.Add(this.chkRX2Mute);
            this.grpRX2.Controls.Add(this.labelTS3);
            this.grpRX2.Controls.Add(this.labelTS2);
            this.grpRX2.Controls.Add(this.labelTS1);
            this.grpRX2.Controls.Add(this.chkRX2Sql);
            this.grpRX2.Controls.Add(this.tbRX2Pan);
            this.grpRX2.Controls.Add(this.tbRX2Sql);
            this.grpRX2.Controls.Add(this.tbRX2RF);
            this.grpRX2.Controls.Add(this.tbRX2AF);
            this.grpRX2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.grpRX2.Location = new System.Drawing.Point(248, 9);
            this.grpRX2.Name = "grpRX2";
            this.grpRX2.Size = new System.Drawing.Size(218, 249);
            this.grpRX2.TabIndex = 7;
            this.grpRX2.TabStop = false;
            this.grpRX2.Text = "RX2";
            // 
            // lblPan
            // 
            this.lblPan.AutoSize = true;
            this.lblPan.Image = null;
            this.lblPan.Location = new System.Drawing.Point(75, 230);
            this.lblPan.Name = "lblPan";
            this.lblPan.Size = new System.Drawing.Size(48, 13);
            this.lblPan.TabIndex = 6;
            this.lblPan.Text = "L/R Pan";
            // 
            // labelTS11
            // 
            this.labelTS11.AutoSize = true;
            this.labelTS11.Image = null;
            this.labelTS11.Location = new System.Drawing.Point(165, 16);
            this.labelTS11.Name = "labelTS11";
            this.labelTS11.Size = new System.Drawing.Size(32, 13);
            this.labelTS11.TabIndex = 15;
            this.labelTS11.Text = "Atten";
            // 
            // tbRX2Atten
            // 
            this.tbRX2Atten.Location = new System.Drawing.Point(168, 32);
            this.tbRX2Atten.Maximum = 31;
            this.tbRX2Atten.Name = "tbRX2Atten";
            this.tbRX2Atten.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX2Atten.Size = new System.Drawing.Size(45, 131);
            this.tbRX2Atten.TabIndex = 15;
            this.tbRX2Atten.Scroll += new System.EventHandler(this.tbRX2Atten_Scroll);
            // 
            // chkRX2Mute
            // 
            this.chkRX2Mute.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX2Mute.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRX2Mute.Image = null;
            this.chkRX2Mute.Location = new System.Drawing.Point(6, 164);
            this.chkRX2Mute.Name = "chkRX2Mute";
            this.chkRX2Mute.Size = new System.Drawing.Size(40, 28);
            this.chkRX2Mute.TabIndex = 9;
            this.chkRX2Mute.Text = "Mute";
            this.chkRX2Mute.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Mute.UseVisualStyleBackColor = true;
            this.chkRX2Mute.CheckedChanged += new System.EventHandler(this.chkRX2Mute_CheckedChanged);
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(115, 16);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(28, 13);
            this.labelTS3.TabIndex = 8;
            this.labelTS3.Text = "SQL";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(65, 16);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(21, 13);
            this.labelTS2.TabIndex = 7;
            this.labelTS2.Text = "RF";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(18, 16);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(20, 13);
            this.labelTS1.TabIndex = 5;
            this.labelTS1.Text = "AF";
            // 
            // chkRX2Sql
            // 
            this.chkRX2Sql.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX2Sql.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRX2Sql.Image = null;
            this.chkRX2Sql.Location = new System.Drawing.Point(111, 164);
            this.chkRX2Sql.Name = "chkRX2Sql";
            this.chkRX2Sql.Size = new System.Drawing.Size(40, 28);
            this.chkRX2Sql.TabIndex = 6;
            this.chkRX2Sql.Text = "SQL";
            this.chkRX2Sql.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Sql.UseVisualStyleBackColor = true;
            this.chkRX2Sql.CheckedChanged += new System.EventHandler(this.chkRX2Sql_CheckedChanged);
            // 
            // tbRX2Pan
            // 
            this.tbRX2Pan.Location = new System.Drawing.Point(35, 198);
            this.tbRX2Pan.Maximum = 100;
            this.tbRX2Pan.Name = "tbRX2Pan";
            this.tbRX2Pan.Size = new System.Drawing.Size(150, 45);
            this.tbRX2Pan.TabIndex = 4;
            this.tbRX2Pan.Scroll += new System.EventHandler(this.tbRX2Pan_Scroll);
            // 
            // tbRX2Sql
            // 
            this.tbRX2Sql.Location = new System.Drawing.Point(114, 32);
            this.tbRX2Sql.Maximum = 0;
            this.tbRX2Sql.Minimum = -160;
            this.tbRX2Sql.Name = "tbRX2Sql";
            this.tbRX2Sql.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX2Sql.Size = new System.Drawing.Size(45, 131);
            this.tbRX2Sql.TabIndex = 3;
            this.tbRX2Sql.Scroll += new System.EventHandler(this.tbRX2Sql_Scroll);
            // 
            // tbRX2RF
            // 
            this.tbRX2RF.Location = new System.Drawing.Point(60, 32);
            this.tbRX2RF.Maximum = 120;
            this.tbRX2RF.Minimum = -20;
            this.tbRX2RF.Name = "tbRX2RF";
            this.tbRX2RF.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX2RF.Size = new System.Drawing.Size(45, 131);
            this.tbRX2RF.TabIndex = 2;
            this.tbRX2RF.Scroll += new System.EventHandler(this.tbRX2RF_Scroll);
            // 
            // tbRX2AF
            // 
            this.tbRX2AF.Location = new System.Drawing.Point(6, 32);
            this.tbRX2AF.Maximum = 100;
            this.tbRX2AF.Name = "tbRX2AF";
            this.tbRX2AF.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX2AF.Size = new System.Drawing.Size(45, 131);
            this.tbRX2AF.TabIndex = 1;
            this.tbRX2AF.Scroll += new System.EventHandler(this.tbRX2AF_Scroll);
            // 
            // tbMasterAF
            // 
            this.tbMasterAF.Location = new System.Drawing.Point(85, 267);
            this.tbMasterAF.Maximum = 100;
            this.tbMasterAF.Name = "tbMasterAF";
            this.tbMasterAF.Size = new System.Drawing.Size(150, 45);
            this.tbMasterAF.TabIndex = 2;
            this.tbMasterAF.Scroll += new System.EventHandler(this.tbMasterAF_Scroll);
            // 
            // grpRX1
            // 
            this.grpRX1.Controls.Add(this.labelTS10);
            this.grpRX1.Controls.Add(this.tbRX1Atten);
            this.grpRX1.Controls.Add(this.labelTS7);
            this.grpRX1.Controls.Add(this.chkRX1Mute);
            this.grpRX1.Controls.Add(this.labelTS8);
            this.grpRX1.Controls.Add(this.labelTS9);
            this.grpRX1.Controls.Add(this.labelTS6);
            this.grpRX1.Controls.Add(this.chkRX1Sql);
            this.grpRX1.Controls.Add(this.tbRX1Pan);
            this.grpRX1.Controls.Add(this.tbRX1Sql);
            this.grpRX1.Controls.Add(this.tbRX1RF);
            this.grpRX1.Controls.Add(this.tbRX1AF);
            this.grpRX1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.grpRX1.Location = new System.Drawing.Point(9, 9);
            this.grpRX1.Name = "grpRX1";
            this.grpRX1.Size = new System.Drawing.Size(226, 249);
            this.grpRX1.TabIndex = 3;
            this.grpRX1.TabStop = false;
            this.grpRX1.Text = "RX1";
            // 
            // labelTS10
            // 
            this.labelTS10.AutoSize = true;
            this.labelTS10.Image = null;
            this.labelTS10.Location = new System.Drawing.Point(165, 16);
            this.labelTS10.Name = "labelTS10";
            this.labelTS10.Size = new System.Drawing.Size(32, 13);
            this.labelTS10.TabIndex = 14;
            this.labelTS10.Text = "Atten";
            // 
            // tbRX1Atten
            // 
            this.tbRX1Atten.Location = new System.Drawing.Point(168, 32);
            this.tbRX1Atten.Maximum = 31;
            this.tbRX1Atten.Name = "tbRX1Atten";
            this.tbRX1Atten.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX1Atten.Size = new System.Drawing.Size(45, 131);
            this.tbRX1Atten.TabIndex = 13;
            this.tbRX1Atten.Scroll += new System.EventHandler(this.tbRX1Atten_Scroll);
            // 
            // labelTS7
            // 
            this.labelTS7.AutoSize = true;
            this.labelTS7.Image = null;
            this.labelTS7.Location = new System.Drawing.Point(109, 16);
            this.labelTS7.Name = "labelTS7";
            this.labelTS7.Size = new System.Drawing.Size(28, 13);
            this.labelTS7.TabIndex = 12;
            this.labelTS7.Text = "SQL";
            // 
            // chkRX1Mute
            // 
            this.chkRX1Mute.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX1Mute.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRX1Mute.Image = null;
            this.chkRX1Mute.Location = new System.Drawing.Point(6, 164);
            this.chkRX1Mute.Name = "chkRX1Mute";
            this.chkRX1Mute.Size = new System.Drawing.Size(40, 28);
            this.chkRX1Mute.TabIndex = 10;
            this.chkRX1Mute.Text = "Mute";
            this.chkRX1Mute.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Mute.UseVisualStyleBackColor = true;
            this.chkRX1Mute.CheckedChanged += new System.EventHandler(this.chkRX1Mute_CheckedChanged);
            // 
            // labelTS8
            // 
            this.labelTS8.AutoSize = true;
            this.labelTS8.Image = null;
            this.labelTS8.Location = new System.Drawing.Point(59, 16);
            this.labelTS8.Name = "labelTS8";
            this.labelTS8.Size = new System.Drawing.Size(21, 13);
            this.labelTS8.TabIndex = 11;
            this.labelTS8.Text = "RF";
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(12, 16);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(20, 13);
            this.labelTS9.TabIndex = 10;
            this.labelTS9.Text = "AF";
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(73, 230);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(48, 13);
            this.labelTS6.TabIndex = 10;
            this.labelTS6.Text = "L/R Pan";
            // 
            // chkRX1Sql
            // 
            this.chkRX1Sql.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX1Sql.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRX1Sql.Image = null;
            this.chkRX1Sql.Location = new System.Drawing.Point(110, 164);
            this.chkRX1Sql.Name = "chkRX1Sql";
            this.chkRX1Sql.Size = new System.Drawing.Size(40, 28);
            this.chkRX1Sql.TabIndex = 5;
            this.chkRX1Sql.Text = "SQL";
            this.chkRX1Sql.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Sql.UseVisualStyleBackColor = true;
            this.chkRX1Sql.CheckedChanged += new System.EventHandler(this.chkRX1Sql_CheckedChanged);
            // 
            // tbRX1Pan
            // 
            this.tbRX1Pan.Location = new System.Drawing.Point(33, 198);
            this.tbRX1Pan.Maximum = 100;
            this.tbRX1Pan.Name = "tbRX1Pan";
            this.tbRX1Pan.Size = new System.Drawing.Size(150, 45);
            this.tbRX1Pan.TabIndex = 4;
            this.tbRX1Pan.Scroll += new System.EventHandler(this.tbRX1Pan_Scroll);
            // 
            // tbRX1Sql
            // 
            this.tbRX1Sql.Location = new System.Drawing.Point(114, 32);
            this.tbRX1Sql.Maximum = 0;
            this.tbRX1Sql.Minimum = -160;
            this.tbRX1Sql.Name = "tbRX1Sql";
            this.tbRX1Sql.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX1Sql.Size = new System.Drawing.Size(45, 131);
            this.tbRX1Sql.TabIndex = 3;
            this.tbRX1Sql.Scroll += new System.EventHandler(this.tbRX1Sql_Scroll);
            // 
            // tbRX1RF
            // 
            this.tbRX1RF.Location = new System.Drawing.Point(60, 32);
            this.tbRX1RF.Maximum = 120;
            this.tbRX1RF.Minimum = -20;
            this.tbRX1RF.Name = "tbRX1RF";
            this.tbRX1RF.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX1RF.Size = new System.Drawing.Size(45, 131);
            this.tbRX1RF.TabIndex = 2;
            this.tbRX1RF.Scroll += new System.EventHandler(this.tbRX1RF_Scroll);
            // 
            // tbRX1AF
            // 
            this.tbRX1AF.Location = new System.Drawing.Point(6, 32);
            this.tbRX1AF.Maximum = 100;
            this.tbRX1AF.Name = "tbRX1AF";
            this.tbRX1AF.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRX1AF.Size = new System.Drawing.Size(45, 131);
            this.tbRX1AF.TabIndex = 1;
            this.tbRX1AF.Scroll += new System.EventHandler(this.tbRX1AF_Scroll);
            // 
            // SliderSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(645, 313);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblMasterAF);
            this.Controls.Add(this.lblDrive);
            this.Controls.Add(this.grpSubRX);
            this.Controls.Add(this.tbDrive);
            this.Controls.Add(this.grpRX2);
            this.Controls.Add(this.tbMasterAF);
            this.Controls.Add(this.grpRX1);
            this.Name = "SliderSettingsForm";
            this.Text = "Analogue Gain Control Settings";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.SliderSettingsForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SliderSettingsForm_Closing);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SliderSettingsForm_FormClosing);
            this.grpSubRX.ResumeLayout(false);
            this.grpSubRX.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSubRXPan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSubRXAF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbDrive)).EndInit();
            this.grpRX2.ResumeLayout(false);
            this.grpRX2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Atten)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Pan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2Sql)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2RF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX2AF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMasterAF)).EndInit();
            this.grpRX1.ResumeLayout(false);
            this.grpRX1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Atten)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Pan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1Sql)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1RF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRX1AF)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Properties
        public int RX1Gain
        {
            get
            {
                if (tbRX1AF != null) return tbRX1AF.Value;
                else return -1;
            }
            set
            {
                if (tbRX1AF != null) tbRX1AF.Value = value;

            }
        }

        public int RX2Gain
        {
            get
            {
                if (tbRX2AF != null) return tbRX2AF.Value;
                else return -1;
            }
            set
            {
                if (tbRX2AF != null) tbRX2AF.Value = value;

            }
        }

        public int SubRXGain
        {
            get
            {
                if (tbSubRXAF != null) return tbSubRXAF.Value;
                else return -1;
            }
            set
            {
                if (tbSubRXAF != null) tbSubRXAF.Value = value;
                //                ptbRX0Gain_Scroll(this, EventArgs.Empty);

            }
        }

        public int RX1RFGainAGC
        {
            get
            {
                if (tbRX1RF != null) return tbRX1RF.Value;
                else return -1;
            }
            set
            {
                if (tbRX1RF != null) tbRX1RF.Value = value;

            }
        }

        public int RX2RFGainAGC
        {
            get
            {
                if (tbRX2RF != null) return tbRX2RF.Value;
                else return -1;
            }
            set
            {
                if (tbRX2RF != null) tbRX2RF.Value = value;

            }
        }

        public int RX1LRPan
        {
            get
            {
                if (tbRX1Pan != null) return tbRX1Pan.Value;
                else return -1;
            }
            set
            {
                if (tbRX1Pan != null) tbRX1Pan.Value = value;

            }
        }

        public int RX2LRPan
        {
            get
            {
                if (tbRX2Pan != null) return tbRX2Pan.Value;
                else return -1;
            }
            set
            {
                if (tbRX2Pan != null) tbRX2Pan.Value = value;

            }
        }

        public int SubRXLRPan
        {
            get
            {
                if (tbSubRXPan != null) return tbSubRXPan.Value;
                else return -1;
            }
            set
            {
                if (tbSubRXPan != null) tbSubRXPan.Value = value;
                //                ptbRX0Gain_Scroll(this, EventArgs.Empty);

            }
        }

        public int RX1Squelch
        {
            get
            {
                if (tbRX1Sql != null) return tbRX1Sql.Value;
                else return -1;
            }
            set
            {
                if (tbRX1Sql != null) tbRX1Sql.Value = value;

            }
        }

        public int RX2Squelch
        {
            get
            {
                if (tbRX2Sql != null) return tbRX2Sql.Value;
                else return -1;
            }
            set
            {
                if (tbRX2Sql != null) tbRX2Sql.Value = value;

            }
        }

        public int RX1Atten
        {
            get
            {
                if (tbRX1Atten != null) return tbRX1Atten.Value;
                else return -1;
            }
            set
            {
                if (value <= tbRX1Atten.Maximum)
                    tbRX1Atten.Value = value;
                else
                    tbRX1Atten.Value = tbRX2Atten.Maximum;
                }
        }

        public int RX2Atten
        {
            get
            {
                if (tbRX2Atten != null) return tbRX2Atten.Value;
                else return -1;
            }
            set
            {
                if (tbRX2Atten != null)
                {
                    if (value <= tbRX2Atten.Maximum)
                        tbRX2Atten.Value = value;
                    else
                        tbRX2Atten.Value = tbRX2Atten.Maximum;
                }

            }
        }

        public bool RX1SquelchOnOff
        {
            get
            {
                if (chkRX1Sql != null) return chkRX1Sql.Checked;
                else return false;
            }
            set
            {
                if (chkRX1Sql != null) chkRX1Sql.Checked = value;

            }
        }

        public bool RX2SquelchOnOff
        {
            get
            {
                if (chkRX2Sql != null) return chkRX2Sql.Checked;
                else return false;
            }
            set
            {
                if (chkRX2Sql != null) chkRX2Sql.Checked = value;

            }
        }

        public bool RX1MuteOnOff
        {
            get
            {
                if (chkRX1Mute != null) return chkRX1Mute.Checked;
                else return false;
            }
            set
            {
                if (chkRX1Mute != null) chkRX1Mute.Checked = value;

            }
        }

        public bool RX2MuteOnOff
        {
            get
            {
                if (chkRX2Mute != null) return chkRX2Mute.Checked;
                else return false;
            }
            set
            {
                if (chkRX2Mute != null) chkRX2Mute.Checked = value;

            }
        }

        public bool SubRXOnOff
        {
            get
            {
                if (chkSubRX != null) return chkSubRX.Checked;
                else return false;
            }
            set
            {
                if (chkSubRX != null) chkSubRX.Checked = value;

            }
        }

        public int MasterAFGain
        {
            get
            {
                if (tbMasterAF != null) return tbMasterAF.Value;
                else return -1;
            }
            set
            {
                if (tbMasterAF != null) tbMasterAF.Value = value;

            }
        }

        public int TXDrive
        {
            get
            {
                if (tbDrive != null) return tbDrive.Value;
                else return -1;
            }
            set
            {
                if (tbDrive != null) tbDrive.Value = value;

            }
        }



        #endregion
        #region Event Handlers

        private void SliderSettingsForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "SliderSettingsForm");
        }
        #endregion

        //
        // copy initial settings from console controls when form shown
        //
        private void SliderSettingsForm_Activated(object sender, EventArgs e)
        {
            tbRX1AF.Value = console.RX0Gain;
            tbRX2AF.Value = console.RX2Gain;
            tbSubRXAF.Value = console.RX1Gain;
            tbRX1RF.Value = console.RF;
            tbRX2RF.Value = console.RX2RF;
            tbRX1Sql.Value = console.Squelch;
            tbRX2Sql.Value = console.Squelch2;
            tbRX1Pan.Value = console.PanMainRX;
            tbRX2Pan.Value = console.RX2Pan;
            // clip atten at 31dB (Andromeda is designed for 7000 series RF)
            if (console.SetupForm.HermesAttenuatorData <= tbRX1Atten.Maximum)
                tbRX1Atten.Value = console.SetupForm.HermesAttenuatorData;
            else
                tbRX1Atten.Value = tbRX1Atten.Maximum;
            if (console.RX2ATT <= tbRX2Atten.Maximum)
                tbRX2Atten.Value = console.RX2ATT;
            else
                tbRX2Atten.Value = tbRX2Atten.Maximum;
            tbRX2Atten.Value = console.RX2ATT;
            tbSubRXPan.Value = console.PanSubRX;
            tbMasterAF.Value = console.AF;
            tbDrive.Value = console.PWR;
            if (console.CATSquelch == 1)
                chkRX1Sql.Checked = true;
            else
                chkRX1Sql.Checked = false;
            if (console.CATSquelch2 == "1")
                chkRX2Sql.Checked = true;
            else
                chkRX2Sql.Checked = false;
            chkRX1Mute.Checked = console.MUT;
            chkRX2Mute.Checked = console.MUT2;
            if (console.CATMultRX == "1")
                chkSubRX.Checked = true;
            else
                chkSubRX.Checked = false;

        }

        private void tbRX1AF_Scroll(object sender, EventArgs e)
        {
            console.RX0Gain = tbRX1AF.Value;
        }

        private void tbRX2AF_Scroll(object sender, EventArgs e)
        {
            console.RX2Gain = tbRX2AF.Value;
        }

        private void tbSubRXAF_Scroll(object sender, EventArgs e)
        {
            console.RX1Gain = tbSubRXAF.Value;
        }

        private void tbRX1RF_Scroll(object sender, EventArgs e)
        {
            console.RF = tbRX1RF.Value;
        }

        private void tbRX2RF_Scroll(object sender, EventArgs e)
        {
            console.RX2RF = tbRX2RF.Value;
        }

        private void tbRX1Sql_Scroll(object sender, EventArgs e)
        {
            console.Squelch = tbRX1Sql.Value;
        }

        private void tbRX2Sql_Scroll(object sender, EventArgs e)
        {
            console.Squelch2 = tbRX2Sql.Value;
        }

        private void tbRX1Pan_Scroll(object sender, EventArgs e)
        {
            console.PanMainRX = tbRX1Pan.Value;
        }

        private void tbRX2Pan_Scroll(object sender, EventArgs e)
        {
            console.RX2Pan = tbRX2Pan.Value;
        }

        private void tbSubRXPan_Scroll(object sender, EventArgs e)
        {
            console.PanSubRX = tbSubRXPan.Value;
        }

        private void tbMasterAF_Scroll(object sender, EventArgs e)
        {
            console.AF = tbMasterAF.Value;
        }

        private void tbDrive_Scroll(object sender, EventArgs e)
        {
            console.PWR = tbDrive.Value;
        }

        private void chkRX1Sql_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX1Sql.Checked == true)
                console.CATSquelch = 1;
            else
                console.CATSquelch = 0;
        }

        private void chkRX2Sql_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX2Sql.Checked == true)
                console.CATSquelch2 = "1";
            else
                console.CATSquelch2 = "0";
        }

        private void chkSubRX_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSubRX.Checked == true)
                console.CATMultRX = "1";
            else
                console.CATMultRX = "0";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SliderSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void tbRX1Atten_Scroll(object sender, EventArgs e)
        {
            console.SetupForm.HermesAttenuatorData = tbRX1Atten.Value;
        }

        private void tbRX2Atten_Scroll(object sender, EventArgs e)
        {
            console.RX2ATT = tbRX2Atten.Value;
        }

        private void chkRX1Mute_CheckedChanged(object sender, EventArgs e)
        {
            console.MUT = chkRX1Mute.Checked;
        }

        private void chkRX2Mute_CheckedChanged(object sender, EventArgs e)
        {
            console.MUT2 = chkRX2Mute.Checked;
        }
    }
}

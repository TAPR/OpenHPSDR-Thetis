//=================================================================
// Spot.cs
// created by Darrin Kohn ke9ns
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
//
//=================================================================
// Fractional year:
//g = (360/365.25)*(N + hour/24)    //  N=day number 1-365


// declination:
// D = 0.396372 - 22.91327 * cos(g) + 4.02543 * sin(g) - 0.387205 * cos( 2 * g)+
//   + 0.051967 * sin(2 * g) - 0.154527 * cos( 3 * g) + 0.084798 * sin( 3 * g)


//=================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;                    // ke9ns add for stringbuilder
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
//using System.Runtime.Serialization.Json;
using System.Net.Sockets;                // ke9ns add for tcpip internet connections
using System.Threading.Tasks;

namespace Thetis
{



    public class SpotControl : System.Windows.Forms.Form
    {
        private static System.Reflection.Assembly myAssembly2 = System.Reflection.Assembly.GetExecutingAssembly();
        public static Stream Map_image = myAssembly2.GetManifestResourceStream("Thetis.Resources.picD1.png");     // MAP

        private static System.Reflection.Assembly myAssembly1 = System.Reflection.Assembly.GetExecutingAssembly();
        public static Stream sun_image = myAssembly1.GetManifestResourceStream("Thetis.Resources.sun.png");       // SUN


        public static Console console;   // ke9ns mod  to allow console to pass back values to setup screen

        public Setup setupForm;   // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 
        // public static StackControl StackForm;                     // ke9ns add  communications with spot.cs and stack

        public DXMemList dxmemlist;

        //   public static Display display;

        //   public Setup setupForm;                        // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 




        //   private ArrayList file_list;
        private string wave_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Thetis";

        private Button SWLbutton;
        private Button SSBbutton;
        public TextBox textBox1;
        public TextBox nodeBox1;
        private TextBox textBox3;
        public TextBox callBox;
        public TextBox portBox2;
        private TextBox statusBox;
        private Button button1;
        private CheckBox checkBoxUSspot;
        private CheckBox checkBoxWorld;
        private TextBox statusBoxSWL;
        private Label label1;
        private Label label2;
        private CheckBoxTS chkAlwaysOnTop;
        public CheckBoxTS chkDXMode;
        public CheckBoxTS chkSUN;
        private ToolTip toolTip1;
        public CheckBoxTS chkGrayLine;
        private Button btnTrack;
        public CheckBoxTS chkPanMode;
        public TextBox nameBox;
        public CheckBoxTS chkMapCall;
        public CheckBoxTS chkMapCountry;
        public CheckBoxTS chkMapBand;
        public CheckBoxTS chkBoxCW;
        public CheckBoxTS chkBoxSSB;
        public CheckBoxTS chkBoxDIG;
        public CheckBoxTS chkBoxPan;
        private DataGridView dataGridView1;
        public CheckBoxTS chkBoxSWL2;
        public CheckBoxTS chkBoxMem;
        public DataGridView dataGridView2;
        private IContainer components;


        #region Constructor and Destructor

        public SpotControl(Console c)
        {
            InitializeComponent();
            console = c;
            Display.SpotForm = this;  // allows Display to see public data (not public static data)
            // StackControl.SpotForm = this; // allows Stack to see public data from spot

            Common.RestoreForm(this, "SpotForm", true);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OpenHPSDR\\";
            string file_name = path + "DXMemory.xml";

            // dataGridView1.Dock = DockStyle.Fill;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.DataSource = console.DXMemList.List; // ke9ns get list of memories from memorylist.cs is where the file is opened and saved

            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = true;
            dataGridView1.AutoGenerateColumns = false;


            if (!File.Exists(file_name))
            {
                console.DXMemList.List.Add(new DXMemRecord("k1rfi.com:7300"));
                console.DXMemList.List.Add(new DXMemRecord("ve7cc.net:23"));
                console.DXMemList.List.Add(new DXMemRecord("telnet.reversebeacon.net:7000"));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));
                console.DXMemList.List.Add(new DXMemRecord(""));


                console.DXMemList.Save1();

                Debug.WriteLine("create DXURL File");

            }


        } // spotcontrol

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpotControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.SWLbutton = new System.Windows.Forms.Button();
            this.SSBbutton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.nodeBox1 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.callBox = new System.Windows.Forms.TextBox();
            this.portBox2 = new System.Windows.Forms.TextBox();
            this.statusBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxUSspot = new System.Windows.Forms.CheckBox();
            this.checkBoxWorld = new System.Windows.Forms.CheckBox();
            this.statusBoxSWL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnTrack = new System.Windows.Forms.Button();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.chkBoxMem = new System.Windows.Forms.CheckBoxTS();
            this.chkBoxSWL2 = new System.Windows.Forms.CheckBoxTS();
            this.chkBoxPan = new System.Windows.Forms.CheckBoxTS();
            this.chkBoxDIG = new System.Windows.Forms.CheckBoxTS();
            this.chkBoxSSB = new System.Windows.Forms.CheckBoxTS();
            this.chkBoxCW = new System.Windows.Forms.CheckBoxTS();
            this.chkMapBand = new System.Windows.Forms.CheckBoxTS();
            this.chkMapCountry = new System.Windows.Forms.CheckBoxTS();
            this.chkMapCall = new System.Windows.Forms.CheckBoxTS();
            this.chkPanMode = new System.Windows.Forms.CheckBoxTS();
            this.chkGrayLine = new System.Windows.Forms.CheckBoxTS();
            this.chkSUN = new System.Windows.Forms.CheckBoxTS();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
            this.chkDXMode = new System.Windows.Forms.CheckBoxTS();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // SWLbutton
            // 
            this.SWLbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SWLbutton.Location = new System.Drawing.Point(585, 446);
            this.SWLbutton.Name = "SWLbutton";
            this.SWLbutton.Size = new System.Drawing.Size(75, 23);
            this.SWLbutton.TabIndex = 2;
            this.SWLbutton.Text = "Spot SWL";
            this.toolTip1.SetToolTip(this.SWLbutton, "Click to turn On/Off Shortwave Spotting to the Panadapter");
            this.SWLbutton.UseVisualStyleBackColor = true;
            this.SWLbutton.Click += new System.EventHandler(this.SWLbutton_Click);
            // 
            // SSBbutton
            // 
            this.SSBbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SSBbutton.Location = new System.Drawing.Point(12, 513);
            this.SSBbutton.Name = "SSBbutton";
            this.SSBbutton.Size = new System.Drawing.Size(75, 23);
            this.SSBbutton.TabIndex = 1;
            this.SSBbutton.Text = "Spot DX";
            this.toolTip1.SetToolTip(this.SSBbutton, "Click to Turn On/Off Dx Cluster Spotting to both this DX text window and Panadapt" +
        "er\r\n");
            this.SSBbutton.UseVisualStyleBackColor = true;
            this.SSBbutton.Click += new System.EventHandler(this.spotSSB_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.BackColor = System.Drawing.Color.LightYellow;
            this.textBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(12, 107);
            this.textBox1.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(729, 280);
            this.textBox1.TabIndex = 6;
            this.textBox1.TabStop = false;
            this.textBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseDown);
            this.textBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseUp);
            // 
            // nodeBox1
            // 
            this.nodeBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nodeBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nodeBox1.Location = new System.Drawing.Point(667, 450);
            this.nodeBox1.MaxLength = 50;
            this.nodeBox1.Name = "nodeBox1";
            this.nodeBox1.Size = new System.Drawing.Size(84, 22);
            this.nodeBox1.TabIndex = 6;
            this.nodeBox1.Text = "spider.ham-radio-deluxe.com";
            this.toolTip1.SetToolTip(this.nodeBox1, "Enter in a DX Cluster URL address here");
            this.nodeBox1.Visible = false;
            this.nodeBox1.TextChanged += new System.EventHandler(this.nodeBox_TextChanged);
            this.nodeBox1.Leave += new System.EventHandler(this.nodeBox_Leave);
            this.nodeBox1.MouseEnter += new System.EventHandler(this.nodeBox_MouseEnter);
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(272, 7);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(469, 94);
            this.textBox3.TabIndex = 8;
            this.textBox3.TabStop = false;
            this.textBox3.Text = resources.GetString("textBox3.Text");
            // 
            // callBox
            // 
            this.callBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.callBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.callBox.Location = new System.Drawing.Point(654, 513);
            this.callBox.MaxLength = 20;
            this.callBox.Name = "callBox";
            this.callBox.Size = new System.Drawing.Size(87, 22);
            this.callBox.TabIndex = 5;
            this.callBox.Text = "Callsign";
            this.toolTip1.SetToolTip(this.callBox, "Enter Your Call sign to login to the DX Cluster here");
            this.callBox.TextChanged += new System.EventHandler(this.callBox_TextChanged);
            this.callBox.Leave += new System.EventHandler(this.callBox_Leave);
            this.callBox.MouseEnter += new System.EventHandler(this.callBox_MouseEnter);
            // 
            // portBox2
            // 
            this.portBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.portBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portBox2.Location = new System.Drawing.Point(695, 447);
            this.portBox2.MaxLength = 7;
            this.portBox2.Name = "portBox2";
            this.portBox2.Size = new System.Drawing.Size(56, 22);
            this.portBox2.TabIndex = 7;
            this.portBox2.Text = "0";
            this.toolTip1.SetToolTip(this.portBox2, "Enter in Dx Cluster URL Port# here");
            this.portBox2.Visible = false;
            this.portBox2.TextChanged += new System.EventHandler(this.portBox_TextChanged);
            this.portBox2.Leave += new System.EventHandler(this.portBox_Leave);
            this.portBox2.MouseEnter += new System.EventHandler(this.portBox_MouseEnter);
            // 
            // statusBox
            // 
            this.statusBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBox.Location = new System.Drawing.Point(12, 404);
            this.statusBox.Name = "statusBox";
            this.statusBox.Size = new System.Drawing.Size(156, 22);
            this.statusBox.TabIndex = 11;
            this.statusBox.Text = "Off";
            this.toolTip1.SetToolTip(this.statusBox, "Click to Test connection\r\nIf it goes back to \"Spotting\" then the connection is go" +
        "od");
            this.statusBox.Click += new System.EventHandler(this.statusBox_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(100, 513);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(68, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Pause";
            this.toolTip1.SetToolTip(this.button1, "Click to Pause the DX Text window (if spots are coming through too fast)\r\nUpdates" +
        " to the Panadapter will still occur");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxUSspot
            // 
            this.checkBoxUSspot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxUSspot.AutoSize = true;
            this.checkBoxUSspot.Location = new System.Drawing.Point(12, 462);
            this.checkBoxUSspot.Name = "checkBoxUSspot";
            this.checkBoxUSspot.Size = new System.Drawing.Size(163, 17);
            this.checkBoxUSspot.TabIndex = 14;
            this.checkBoxUSspot.Text = "North American Spotters only";
            this.checkBoxUSspot.UseVisualStyleBackColor = true;
            this.checkBoxUSspot.CheckedChanged += new System.EventHandler(this.checkBoxUSspot_CheckedChanged);
            // 
            // checkBoxWorld
            // 
            this.checkBoxWorld.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxWorld.AutoSize = true;
            this.checkBoxWorld.Location = new System.Drawing.Point(12, 485);
            this.checkBoxWorld.Name = "checkBoxWorld";
            this.checkBoxWorld.Size = new System.Drawing.Size(182, 17);
            this.checkBoxWorld.TabIndex = 15;
            this.checkBoxWorld.Text = "Exclude North American Spotters";
            this.checkBoxWorld.UseVisualStyleBackColor = true;
            this.checkBoxWorld.CheckedChanged += new System.EventHandler(this.checkBoxWorld_CheckedChanged);
            // 
            // statusBoxSWL
            // 
            this.statusBoxSWL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusBoxSWL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBoxSWL.Location = new System.Drawing.Point(585, 406);
            this.statusBoxSWL.Name = "statusBoxSWL";
            this.statusBoxSWL.Size = new System.Drawing.Size(156, 22);
            this.statusBoxSWL.TabIndex = 16;
            this.statusBoxSWL.Text = "Off";
            this.toolTip1.SetToolTip(this.statusBoxSWL, "Status of ShortWave spotter list transfer to PowerSDR memory\r\n");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 387);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Status of DX Cluster";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(582, 390);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Status of SWL Spotter";
            // 
            // btnTrack
            // 
            this.btnTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTrack.Location = new System.Drawing.Point(272, 513);
            this.btnTrack.Name = "btnTrack";
            this.btnTrack.Size = new System.Drawing.Size(75, 23);
            this.btnTrack.TabIndex = 62;
            this.btnTrack.Text = "Track";
            this.toolTip1.SetToolTip(this.btnTrack, "Click to Turn on/off GrayLine and/or Sun Tracking");
            this.btnTrack.UseVisualStyleBackColor = true;
            this.btnTrack.Click += new System.EventHandler(this.btnTrack_Click);
            // 
            // nameBox
            // 
            this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameBox.Location = new System.Drawing.Point(683, 450);
            this.nameBox.MaxLength = 20;
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(46, 22);
            this.nameBox.TabIndex = 64;
            this.nameBox.Text = "name";
            this.toolTip1.SetToolTip(this.nameBox, "Enter Your Call sign to login to the DX Cluster here");
            this.nameBox.Visible = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.OliveDrab;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Location = new System.Drawing.Point(12, 7);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(254, 94);
            this.dataGridView1.TabIndex = 72;
            this.toolTip1.SetToolTip(this.dataGridView1, "Enter DX address : port#\r\nExample:  k1rfi.com:7300\r\n");
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.DoubleClick += new System.EventHandler(this.dataGridView1_DoubleClick);
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowDrop = true;
            this.dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.AllowUserToDeleteRows = false;
            this.dataGridView2.AllowUserToResizeColumns = false;
            this.dataGridView2.AllowUserToResizeRows = false;
            this.dataGridView2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.OliveDrab;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView2.Location = new System.Drawing.Point(463, 281);
            this.dataGridView2.MultiSelect = false;
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(254, 94);
            this.dataGridView2.TabIndex = 75;
            this.toolTip1.SetToolTip(this.dataGridView2, "Enter DX address : port#\r\nExample:  k1rfi.com:7300\r\n");
            this.dataGridView2.Visible = false;
            // 
            // chkBoxMem
            // 
            this.chkBoxMem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxMem.Image = null;
            this.chkBoxMem.Location = new System.Drawing.Point(474, 511);
            this.chkBoxMem.Name = "chkBoxMem";
            this.chkBoxMem.Size = new System.Drawing.Size(128, 24);
            this.chkBoxMem.TabIndex = 74;
            this.chkBoxMem.Text = "MEMORIES to Pan";
            this.toolTip1.SetToolTip(this.chkBoxMem, "Show Memories directly on Panadapter.\r\n\r\nLEFT CLICK on visible Memory + CTRL to s" +
        "et Mode\r\n\r\nLEFT CLICK on PAN + ALT + M keys to save New Memory\r\n");
            this.chkBoxMem.CheckedChanged += new System.EventHandler(this.chkBoxMem_CheckedChanged);
            // 
            // chkBoxSWL2
            // 
            this.chkBoxSWL2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxSWL2.Image = null;
            this.chkBoxSWL2.Location = new System.Drawing.Point(676, 450);
            this.chkBoxSWL2.Name = "chkBoxSWL2";
            this.chkBoxSWL2.Size = new System.Drawing.Size(65, 24);
            this.chkBoxSWL2.TabIndex = 73;
            this.chkBoxSWL2.Text = "Alternate SWL2.txt";
            this.toolTip1.SetToolTip(this.chkBoxSWL2, "Show Country or Calls on Map for just the Panadapter freq you are viewing.\r\n");
            this.chkBoxSWL2.Visible = false;
            this.chkBoxSWL2.CheckedChanged += new System.EventHandler(this.chkBoxSWL2_CheckedChanged);
            // 
            // chkBoxPan
            // 
            this.chkBoxPan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxPan.Image = null;
            this.chkBoxPan.Location = new System.Drawing.Point(416, 474);
            this.chkBoxPan.Name = "chkBoxPan";
            this.chkBoxPan.Size = new System.Drawing.Size(113, 24);
            this.chkBoxPan.TabIndex = 71;
            this.chkBoxPan.Text = "Map just Pan";
            this.toolTip1.SetToolTip(this.chkBoxPan, "Show Country or Calls on Map for just the Panadapter freq you are viewing.\r\n");
            this.chkBoxPan.CheckedChanged += new System.EventHandler(this.chkBoxPan_CheckedChanged);
            // 
            // chkBoxDIG
            // 
            this.chkBoxDIG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxDIG.Checked = true;
            this.chkBoxDIG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxDIG.Image = null;
            this.chkBoxDIG.Location = new System.Drawing.Point(181, 462);
            this.chkBoxDIG.Name = "chkBoxDIG";
            this.chkBoxDIG.Size = new System.Drawing.Size(85, 24);
            this.chkBoxDIG.TabIndex = 70;
            this.chkBoxDIG.Text = "Spot Digital";
            this.toolTip1.SetToolTip(this.chkBoxDIG, "Show Digital spots when checked (like RTTY, PSK, etc)\r\n");
            // 
            // chkBoxSSB
            // 
            this.chkBoxSSB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxSSB.Checked = true;
            this.chkBoxSSB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxSSB.Image = null;
            this.chkBoxSSB.Location = new System.Drawing.Point(181, 432);
            this.chkBoxSSB.Name = "chkBoxSSB";
            this.chkBoxSSB.Size = new System.Drawing.Size(85, 24);
            this.chkBoxSSB.TabIndex = 69;
            this.chkBoxSSB.Text = "Spot Phone";
            this.toolTip1.SetToolTip(this.chkBoxSSB, "Show SSB spots when checked\r\n");
            // 
            // chkBoxCW
            // 
            this.chkBoxCW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxCW.Checked = true;
            this.chkBoxCW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxCW.Image = null;
            this.chkBoxCW.Location = new System.Drawing.Point(181, 404);
            this.chkBoxCW.Name = "chkBoxCW";
            this.chkBoxCW.Size = new System.Drawing.Size(85, 24);
            this.chkBoxCW.TabIndex = 68;
            this.chkBoxCW.Text = "Spot CW";
            this.toolTip1.SetToolTip(this.chkBoxCW, "Show CW spots when checked\r\n");
            // 
            // chkMapBand
            // 
            this.chkMapBand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMapBand.Checked = true;
            this.chkMapBand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMapBand.Image = null;
            this.chkMapBand.Location = new System.Drawing.Point(416, 446);
            this.chkMapBand.Name = "chkMapBand";
            this.chkMapBand.Size = new System.Drawing.Size(113, 24);
            this.chkMapBand.TabIndex = 67;
            this.chkMapBand.Text = "Map just Band";
            this.toolTip1.SetToolTip(this.chkMapBand, "Show Country or Calls on Map for the Band you are on.\r\n");
            this.chkMapBand.CheckedChanged += new System.EventHandler(this.chkMapBand_CheckedChanged);
            // 
            // chkMapCountry
            // 
            this.chkMapCountry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMapCountry.Image = null;
            this.chkMapCountry.Location = new System.Drawing.Point(416, 393);
            this.chkMapCountry.Name = "chkMapCountry";
            this.chkMapCountry.Size = new System.Drawing.Size(88, 22);
            this.chkMapCountry.TabIndex = 66;
            this.chkMapCountry.Text = "Map Country";
            this.toolTip1.SetToolTip(this.chkMapCountry, "Show Dx spot Countries on Map\r\n");
            this.chkMapCountry.CheckedChanged += new System.EventHandler(this.chkMapCountry_CheckedChanged);
            // 
            // chkMapCall
            // 
            this.chkMapCall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMapCall.Checked = true;
            this.chkMapCall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMapCall.Image = null;
            this.chkMapCall.Location = new System.Drawing.Point(416, 421);
            this.chkMapCall.Name = "chkMapCall";
            this.chkMapCall.Size = new System.Drawing.Size(88, 24);
            this.chkMapCall.TabIndex = 65;
            this.chkMapCall.Text = "Map Calls";
            this.toolTip1.SetToolTip(this.chkMapCall, "Show DX Spot Call signs on Map");
            this.chkMapCall.CheckedChanged += new System.EventHandler(this.chkMapCall_CheckedChanged);
            // 
            // chkPanMode
            // 
            this.chkPanMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkPanMode.Checked = true;
            this.chkPanMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPanMode.Image = null;
            this.chkPanMode.Location = new System.Drawing.Point(272, 451);
            this.chkPanMode.Name = "chkPanMode";
            this.chkPanMode.Size = new System.Drawing.Size(148, 23);
            this.chkPanMode.TabIndex = 63;
            this.chkPanMode.Text = "Special PanaFall Mode\r\n";
            this.toolTip1.SetToolTip(this.chkPanMode, "When Checked, will Display RX1 in Panafall mode, with a small waterfall for bette" +
        "r viewing of the map");
            this.chkPanMode.CheckedChanged += new System.EventHandler(this.chkPanMode_CheckedChanged);
            // 
            // chkGrayLine
            // 
            this.chkGrayLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkGrayLine.Checked = true;
            this.chkGrayLine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGrayLine.Image = null;
            this.chkGrayLine.Location = new System.Drawing.Point(272, 428);
            this.chkGrayLine.Name = "chkGrayLine";
            this.chkGrayLine.Size = new System.Drawing.Size(105, 17);
            this.chkGrayLine.TabIndex = 61;
            this.chkGrayLine.Text = "GrayLine Track";
            this.toolTip1.SetToolTip(this.chkGrayLine, "GrayLine will show on Panadapter Display\r\nBut only when using KE9SN6_World skin o" +
        "nly\r\nAnd only when RX1 is in Panadapter Mode with RX2 Display OFF");
            this.chkGrayLine.CheckedChanged += new System.EventHandler(this.chkGrayLine_CheckedChanged);
            // 
            // chkSUN
            // 
            this.chkSUN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkSUN.Checked = true;
            this.chkSUN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSUN.Image = null;
            this.chkSUN.Location = new System.Drawing.Point(272, 400);
            this.chkSUN.Name = "chkSUN";
            this.chkSUN.Size = new System.Drawing.Size(92, 24);
            this.chkSUN.TabIndex = 60;
            this.chkSUN.Text = "SunTracking\r\n";
            this.toolTip1.SetToolTip(this.chkSUN, "Sun will show on Panadapter screen \r\nBut only when using KE9SN6_World 3 only\r\nAnd" +
        " only when RX1 is in Panadapter Mode with RX2 Display OFF");
            this.chkSUN.CheckedChanged += new System.EventHandler(this.chkSUN_CheckedChanged);
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAlwaysOnTop.Image = null;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(638, 485);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(103, 24);
            this.chkAlwaysOnTop.TabIndex = 58;
            this.chkAlwaysOnTop.Text = "Always On Top";
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            // 
            // chkDXMode
            // 
            this.chkDXMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkDXMode.Checked = true;
            this.chkDXMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDXMode.Image = null;
            this.chkDXMode.Location = new System.Drawing.Point(654, 515);
            this.chkDXMode.Name = "chkDXMode";
            this.chkDXMode.Size = new System.Drawing.Size(91, 24);
            this.chkDXMode.TabIndex = 59;
            this.chkDXMode.Text = "Parse \"DX Spot\" Mode";
            this.chkDXMode.UseVisualStyleBackColor = true;
            this.chkDXMode.Visible = false;
            // 
            // SpotControl
            // 
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(753, 548);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.chkBoxMem);
            this.Controls.Add(this.chkBoxSWL2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.chkBoxPan);
            this.Controls.Add(this.chkBoxDIG);
            this.Controls.Add(this.chkBoxSSB);
            this.Controls.Add(this.chkBoxCW);
            this.Controls.Add(this.chkMapBand);
            this.Controls.Add(this.chkMapCountry);
            this.Controls.Add(this.chkMapCall);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.chkPanMode);
            this.Controls.Add(this.btnTrack);
            this.Controls.Add(this.chkGrayLine);
            this.Controls.Add(this.chkSUN);
            this.Controls.Add(this.chkAlwaysOnTop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusBoxSWL);
            this.Controls.Add(this.checkBoxWorld);
            this.Controls.Add(this.checkBoxUSspot);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.statusBox);
            this.Controls.Add(this.portBox2);
            this.Controls.Add(this.callBox);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.nodeBox1);
            this.Controls.Add(this.SSBbutton);
            this.Controls.Add(this.SWLbutton);
            this.Controls.Add(this.chkDXMode);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.Name = "SpotControl";
            this.Text = "DX / SWL Spotter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpotControl_FormClosing);
            this.Load += new System.EventHandler(this.SpotControl_Load);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.SpotControl_Layout);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        } //initializecomponents





        #endregion

        #region Properties



        #endregion

        #region Event Handlers







        #endregion



        public static string callB = "callsign";                     // ke9ns add call sign for dx spotter
        public static string nodeB = "spider.ham-radio-deluxe.com";  // ke9ns add node for dx spotter
        public static string portB = "8000";                        // ke9ns add port# 
        public static string nameB = "HB9DRV-9>";                   // ke9ns add port# 


        public static string DXCALL  // this is called or set in console
        {
            get { return callB; }
            set
            {
                callB = value;

            } // set
        } // callsign

        public static string DXNODE  // this is called or set in console
        {
            get { return nodeB; }
            set
            {
                nodeB = value;

            } // set
        } // callsign
        public static string DXNAME  // this is called or set in console
        {
            get { return nameB; }
            set
            {
                nameB = value;

            } // set
        } // callsign
        public static string DXPORT  // this is called or set in console
        {
            get { return portB; }
            set
            {
                portB = value;

            } // set
        } // callsign



        private void SpotControl_Load(object sender, EventArgs e)
        {

            nameBox.Text = nameB;
            callBox.Text = callB;
            nodeBox1.Text = nodeB;
            portBox2.Text = portB;

            try
            {
                if (Convert.ToInt16(portBox2.Text) < 20)
                {
                    dataGridView1.CurrentCell = dataGridView1[0, Convert.ToInt16(portBox2.Text)];
                    Debug.WriteLine("retrieved the index from storage");

                }
            }
            catch (Exception)
            {
                dataGridView1.CurrentCell = dataGridView1[0, 0];

            }
        }

        //=======================================================================================================================
        private void SpotControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            callB = callBox.Text;  // values to save in ke9ns.dat file
            nodeB = nodeBox1.Text;
            portB = portBox2.Text;
            nameB = nameBox.Text;

            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "SpotForm");

            console.DXMemList.Save1(); // save dx spotter list
        }

        public static byte SP_Active = 0;  // 1= DX Spot feature ON, 2=logging in 3=waiting for spots
        public static byte SP2_Active = 0; // DX Spot: 0=closed so ok to open again if you want, 1=in process of shutting down
        public static byte SP4_Active = 0; // 1=processing valid DX spot. 0=not processing new DX spot

        public static byte SP1_Active = 0; // SWL active
        public static byte SP3_Active = 0; // 1=SWL database loaded up, so no need to reload if you turn if OFF

        public static byte SP5_Active = 0; // 1= turn on tracking mode, but you might not have sun or grayline on

        //=======================================================================================
        //=======================================================================================
        // ke9ns SWL spotter // www.eibispace.de to get sked.csv file to read
        private void SWLbutton_Click(object sender, EventArgs e)
        {
            string file_name = " ";

            if (chkBoxSWL2.Checked == true)
            {
                file_name = console.AppDataPath + "SWL2.txt"; // Sigmera
            }
            else
            {
                file_name = console.AppDataPath + "SWL.csv"; //   eibispace.de  sked - b15.csv
            }

            if (!File.Exists(file_name))
            {
                Debug.WriteLine("problem no SWL.CSV file found ");
                statusBoxSWL.ForeColor = Color.Red;

                statusBoxSWL.Text = "No SWL.csv file found";

                return;
            }

            if ((SP1_Active == 0))
            {
                SP1_Active = 1;

                //    Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                //   Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

                Thread t = new Thread(new ThreadStart(SWLSPOTTER));

                t.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                t.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

                t.Name = "SWL Spotter Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                t.Start();

                //  while (t.IsAlive)
                //  {
                //      Thread.Sleep(50);
                //      Application.DoEvents();
                //  }
            }
            else
            {
                SP1_Active = 0; // turn off SWL spotter

                statusBoxSWL.ForeColor = Color.Red;
                statusBoxSWL.Text = "Off";

                if (SP_Active == 0)
                {
                    console.spotterMenu.ForeColor = Color.Red;
                    console.spotterMenu.Text = "Spot";
                }

            } // SWL not active  

        } // SWLbutton_Click

        // these are pulled from SWL.csv file
        public static string[] SWL_Station = new string[20000];       // Station name
        public static int[] SWL_Freq = new int[20000];              // in hz
        public static byte[] SWL_Band = new byte[20000];              // in Mhz
        public static int[] SWL_BandL = new int[31];              // index for each start of mhz listed in swl.csv

        public static string[] SWL_Lang = new string[20000];          // language of transmitter
        public static int[] SWL_TimeN = new int[20000];                // UTC time of operation ON air
        public static int[] SWL_TimeF = new int[20000];                // UTC time of operation OFF air
        public static string[] SWL_Mode = new string[20000];          // operating mode
        public static string[] SWL_Day = new string[20000];          // days of operation
        public static string[] SWL_Loc = new string[20000];          // location of transmitter
        public static string[] SWL_Target = new string[20000];          // target area of station

        public static int[] SWL_Pos = new int[20000];                // related to W on the panadapter screen

        public static int SWL_Index;  //  max number of spots in memory currently
        public static int SWL_Index1;  // local index that reset back to 0 after reaching max
        public static int SWL_Index3;  //  
        public static int VFOHLast;

        public static int Lindex; // low index spot
        public static int Hindex; // high index spot

        public static FileStream stream2;          // for reading SWL.csv file
        public static BinaryReader reader2;

        public static byte Flag1 = 0; // flag to skip header line in SWL.csv file

        public static DateTime UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public static string FD = UTCD.ToString("HHmm");                                       // get 24hr 4 digit UTC NOW

        public static int UTCNEW1 = Convert.ToInt16(FD);                                       // convert 24hr UTC to int

        //=======================================================================================
        //=======================================================================================
        //ke9ns start SWL spotting
        private void SWLSPOTTER()
        {

            string file_name = " ";

            if (chkBoxSWL2.Checked == true)
            {
                file_name = console.AppDataPath + "SWL2.txt"; // sigmera
            }
            else
            {
                file_name = console.AppDataPath + "SWL.csv"; //  sked - b15.csv  
            }

            if (File.Exists(file_name))
            {

                try
                {
                    stream2 = new FileStream(file_name, FileMode.Open); // open file
                    reader2 = new BinaryReader(stream2, Encoding.ASCII);
                }
                catch (Exception)
                {
                    SP1_Active = 0; // turn off SWL spotter

                    statusBoxSWL.ForeColor = Color.Red;
                    statusBoxSWL.Text = "Off";


                    if (SP_Active == 0)
                    {
                        console.spotterMenu.ForeColor = Color.Red;
                        console.spotterMenu.Text = "Spot";
                    }
                    return;
                }

                var result = new StringBuilder();

                if (SP3_Active == 0) // dont reset if already scanned in  database
                {
                    SWL_Index1 = 0; // how big is the SWL.CSV data file in lines
                    SWL_Index = 0; // start at 1 mhz
                    Flag1 = 0;

                }
                statusBoxSWL.Text = "Reading ";

                for (; ; )
                {

                    if (SP3_Active == 1) // aleady scanned database
                    {
                        break; // dont rescan database over 
                    }

                    if (SP1_Active == 0)
                    {

                        reader2.Close();    // close  file
                        stream2.Close();   // close stream

                        statusBoxSWL.ForeColor = Color.Black;
                        statusBoxSWL.Text = "Off";

                        if (SP_Active == 0)
                        {
                            console.spotterMenu.ForeColor = Color.White;
                            console.spotterMenu.Text = "Spot";
                        }

                        return;
                    }

                    statusBoxSWL.ForeColor = Color.Red;
                    //    statusBoxSWL.Text = "Reading " + SWL_Index1.ToString();


                    if (SP_Active == 0)
                    {
                        console.spotterMenu.ForeColor = Color.Yellow;
                        console.spotterMenu.Text = "Reading";
                    }

                    try
                    {
                        var newChar = (char)reader2.ReadChar();

                        if (((chkBoxSWL2.Checked == false) && (newChar == '\r')) || ((chkBoxSWL2.Checked == true) && (newChar == '\n')))
                        {
                            if (chkBoxSWL2.Checked == false) newChar = (char)reader2.ReadChar(); // read \n char to finishline

                            if (Flag1 == 1)
                            {

                                string[] values;

                                //--------------------------------------------------------------------------------------------
                                if (chkBoxSWL2.Checked == true) // SWL2.TXT file only
                                {
                                    values = result.ToString().Split('|'); // split line up into segments divided by | char

                                    SWL_Freq[SWL_Index1] = (int)(Convert.ToDouble(values[0]) * 1000000); // get freq and convert to hz


                                    SWL_Band[SWL_Index1] = (byte)(SWL_Freq[SWL_Index1] / 1000000); // get freq and convert to mhz


                                    if (SWL_Band[SWL_Index1] > SWL_Index)
                                    {
                                        //  Debug.WriteLine("INDEX MHZ " + SWL_Index + " index1 " + SWL_Index1);
                                        SWL_BandL[SWL_Index] = SWL_Index1;                                   // SWL_BandL[0] = highest index under 1mhz, SWL_BandL[1] = highest index under 2mhz
                                        VFOHLast = 0; // refresh pan screen while loading
                                        SWL_Index++;
                                    }


                                    SWL_TimeN[SWL_Index1] = 0;
                                    SWL_TimeF[SWL_Index1] = 2400;

                                    SWL_Mode[SWL_Index1] = values[1]; // get opeating mode

                                    SWL_Day[SWL_Index1] = "na"; // get days ON

                                    SWL_Loc[SWL_Index1] = "na"; // get location of station

                                    //  Debug.WriteLine("namebefore " + values[2]);

                                    if (values[2].Contains("_")) // get rid of date time stamp
                                    {
                                        int ind = values[2].IndexOf("_");

                                        var temp = new StringBuilder();
                                        if ((ind - 7) > 0) temp.Append(values[2].Substring(0, ind - 7));

                                        if (values[2].Length > (ind + 5))
                                        {
                                            //  Debug.WriteLine("len " + values[2].Length);

                                            //  Debug.WriteLine("ind " + ind);
                                            temp.Append(values[2].Substring(ind + 5, values[2].Length - ind - 5));

                                        }

                                        values[2] = temp.ToString();

                                        //   Debug.WriteLine("nameafter " + values[2]);

                                    } // contains _

                                    if (values[2].Contains("_")) // 2nd look: get rid of date time stamp
                                    {
                                        int ind = values[2].IndexOf("_");

                                        var temp = new StringBuilder();
                                        if ((ind - 7) > 0) temp.Append(values[2].Substring(0, ind - 7));

                                        if (values[2].Length > (ind + 5))
                                        {
                                            //  Debug.WriteLine("len " + values[2].Length);

                                            //   Debug.WriteLine("ind " + ind);
                                            temp.Append(values[2].Substring(ind + 5, values[2].Length - ind - 5));

                                        }

                                        values[2] = temp.ToString();

                                        //   Debug.WriteLine("nameafter " + values[2]);

                                    } // 2nd check contains _


                                    SWL_Station[SWL_Index1] = values[2]; // get station name


                                    SWL_Lang[SWL_Index1] = "na"; // get language

                                    SWL_Target[SWL_Index1] = "na"; // get station target area


                                    SWL_Index1++;



                                }
                                //--------------------------------------------------------------------------------------------
                                else // SWL.CSV file only
                                {
                                    values = result.ToString().Split(';'); // split line up into segments divided by ;

                                    SWL_Freq[SWL_Index1] = (int)(Convert.ToDouble(values[0]) * 1000); // get freq and convert to hz

                                    SWL_Band[SWL_Index1] = (byte)(SWL_Freq[SWL_Index1] / 1000000); // get freq and convert to mhz


                                    if (SWL_Band[SWL_Index1] > SWL_Index)
                                    {
                                        //  Debug.WriteLine("INDEX MHZ " + SWL_Index + " index1 " + SWL_Index1);
                                        SWL_BandL[SWL_Index] = SWL_Index1;                                   // SWL_BandL[0] = highest index under 1mhz, SWL_BandL[1] = highest index under 2mhz
                                        VFOHLast = 0; // refresh pan screen while loading
                                        SWL_Index++;
                                    }


                                    SWL_TimeN[SWL_Index1] = Convert.ToInt16(values[1].Substring(0, 4)); // get time ON (24hr 4 digit UTC)
                                    SWL_TimeF[SWL_Index1] = Convert.ToInt16(values[1].Substring(5, 4)); // get time OFF

                                    SWL_Day[SWL_Index1] = values[2]; // get days ON

                                    SWL_Loc[SWL_Index1] = values[3]; // get location of station

                                    SWL_Mode[SWL_Index1] = "AM"; // get opeating mode

                                    SWL_Station[SWL_Index1] = values[4]; // get station name

                                    SWL_Lang[SWL_Index1] = values[5]; // get language

                                    SWL_Target[SWL_Index1] = values[6]; // get station target area


                                    if (SWL_Index > 0)
                                    {
                                        if ((SWL_Station[SWL_Index1 - 1] == SWL_Station[SWL_Index1]) && (SWL_Freq[SWL_Index1 - 1] == SWL_Freq[SWL_Index1]))// if same name and freq then check times
                                        {
                                            if ((SWL_TimeN[SWL_Index1 - 1] == SWL_TimeN[SWL_Index1]) && (SWL_TimeF[SWL_Index1 - 1] == SWL_TimeF[SWL_Index1])) goto BYPASS; // duplicate

                                        }

                                    }

                                    SWL_Index1++;


                                BYPASS: ;

                                    //   Debug.Write(" freq " + SWL_Freq[SWL_Index1]);
                                    //   Debug.Write(" Band " + SWL_Band[SWL_Index1]);
                                    //   Debug.Write(" ON time " + SWL_TimeN[SWL_Index1]);
                                    //    Debug.Write(" OFF time " + SWL_TimeF[SWL_Index1]);
                                    //    Debug.Write(" days " + SWL_Day[SWL_Index1]);
                                    //   Debug.Write(" LOC " + SWL_Loc[SWL_Index1]);
                                    //  Debug.Write(" Station Name " + SWL_Station[SWL_Index1]);
                                    //  Debug.Write(" Lang " + SWL_Lang[SWL_Index1]);
                                    //   Debug.WriteLine(" target " + SWL_Target[SWL_Index1]);

                                    // remarks
                                    // ? P
                                    // ? Start
                                    // ? Stop 


                                } // SWL.CSV file only



                            } // SWL Spots
                            else Flag1 = 1;

                            result = new StringBuilder(); // clean up for next line

                        }
                        else
                        {
                            result.Append(newChar);  // save char
                        }

                    }
                    catch (EndOfStreamException)
                    {
                        SWL_Index1--;
                        // textBox1.Text = "End of SWL FILE at "+ SWL_Index1.ToString();

                        break; // done with file
                    }
                    catch (Exception e)
                    {
                        //  Debug.WriteLine("excpt======== " + e);
                        //     textBox1.Text = e.ToString();

                        break; // done with file
                    }


                } // for loop until end of file is reached


                // Debug.WriteLine("reached SWL end of file");

                reader2.Close();    // close  file
                stream2.Close();   // close stream

                SP3_Active = 1; // done loading swl database (Good)

                statusBoxSWL.ForeColor = Color.Blue;
                statusBoxSWL.Text = "SWL Spotting " + SWL_Index1.ToString();


                if (SP_Active == 0)
                {
                    console.spotterMenu.ForeColor = Color.Yellow;
                    console.spotterMenu.Text = "SWL Spot";
                }

                //==============================================================================
                //==============================================================================
                // ke9ns process lines of SWL data here

            } // if file exists


            //-----------------------------------------
            // ke9ns track ISS



            /*
            try
            {

                // http://api.open-notify.org/iss-now.json

                textBox1.Text += "Attempt to connect to ISS Now http \r\n";

                serverPath = "http://api.open-notify.org/iss-now.json";

            
                HttpWebRequest request = WebRequest.Create(serverPath) as HttpWebRequest;

                textBox1.Text += "Attempt to download ISS position \r\n";

                using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("Server error (HTTP {0}: {1}).", response.StatusCode, response.StatusDescription));
                    }

                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());


                   Response jsonResponse = objResponse as Response;

                }




                //  Stream responseStream = response.GetResponseStream();
                //   StreamReader reader = new StreamReader(responseStream);
                //    noaa = reader.ReadToEnd();

                //  reader.Close();
                //   response.Close();
                //   Debug.WriteLine("noaa=== " + noaa);

                textBox1.Text += "ISS  Download complete \r\n";

            }
            catch (Exception ex)
            {
                //   Debug.WriteLine("ISS fault=== " + ex);
                textBox1.Text += "Failed to get ISS data \r\n";

            }
*/

        } // SWLSPOTTER








        //=======================================================================================
        //=======================================================================================
        //ke9ns start DX spotting
        private void spotSSB_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("TESt");

            Debug.WriteLine("========row " + dataGridView1.CurrentCell.RowIndex);

            Debug.WriteLine("========URL " + (string)dataGridView1["dxurl", dataGridView1.CurrentCell.RowIndex].Value);



            if ((SP2_Active == 0) && (SP_Active == 0) && (callBox.Text != "callsign") && (callBox.Text != null))
            {


                //  console.DXMemList.List.Add(new DXMemRecord(console.DXMemList.List[dataGridView1.CurrentCell.RowIndex]));

                //  Common.SaveForm(this, "MemoryForm");    

                Thread t = new Thread(new ThreadStart(SPOTTER));

                t.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                t.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");


                SP_Active = 1;
                t.Name = "Spotter Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal; // normal
                t.Start();

                textBox1.Text = "Clicked to Open DX Spider \r\n";

            }
            else if ((callBox.Text == "callsign") || (callBox.Text == null))
            {

                Debug.WriteLine("callbox " + callBox.Text);

                textBox1.Text += "Must put your CALL Sign in the CALLSIGN box (lower Right of this window)\r\n";
                callBox.ForeColor = Color.Red;
                callBox.BackColor = Color.Yellow;


            }
            else
            {

                SP_Active = 0; // turn off DX Spotter

                statusBox.ForeColor = Color.Red;
                console.spotterMenu.ForeColor = Color.Red;

                console.spotterMenu.Text = "Closing";
                statusBox.Text = "Closing";

                textBox1.Text += "Clicked to Close Socket (click again to Force Closed)\r\n";

                if (SP2_Active != 0)
                {
                    textBox1.Text += "Force closed \r\n";

                    try
                    {
                        SP_writer.Close();
                        SP_reader.Close();
                    }
                    catch (Exception)
                    {
                        Debug.Write("writer/reader was not open to close");
                    }

                    try
                    {

                        networkStream.Close();
                        client.Close();
                    }
                    catch (Exception)
                    {
                        Debug.Write("networkstream was never open to close");
                    }

                    SP_Active = 0; // turn off DX Spotter
                    SP2_Active = 0; // turn off DX Spotter
                }
                else SP2_Active = 1; // in process of shutting down.


            } // turn DX spotting off


        } //  spotSSB_Click




        //====================================================================================================
        //====================================================================================================
        // ke9ns add Thread routine

        public static string[] SP_Time;
        public static string[] SP_Freq;
        public static string[] SP_Call;
        public static TcpClient client;                                               //           ' socket

        public static NetworkStream networkStream;                                   //         ' stream
        public static BinaryWriter SP_writer;
        public static BinaryReader SP_Reader;

        public static StreamReader SP_reader;
        public static StreamWriter SP_Writer;

        public static string message1; // DX messages
        public static string message2; // blank start
        public static string message3; // login messages

        public static string[] DX_Station = new string[1000];       // dx call sign
        public static int[] DX_Freq = new int[1000];                // in hz
        public static string[] DX_Spotter = new string[1000];       // spotter call sign
        public static string[] DX_Message = new string[1000];       // message
        public static int[] DX_Mode = new int[1000];                // mode parse from message string 0=ssb,1=cw,2=rtty,3=psk,4=olivia,5=jt65,6=contesa,7=fsk,8=mt63,9=domi,10=packtor, 11=fm, 12=drm, 13=sstv, 14=am
        public static int[] DX_Mode2 = new int[1000];               // mode2 parse from message string 0=normal , +up in hz or -dn in hz
        public static int[] DX_Time = new int[1000];                // GMT
        public static string[] DX_Grid = new string[1000];          // grid
        public static string[] DX_Age = new string[1000];           // how old is the spot


        public static int[] DX_X = new int[1000];                   // x pixel location on map (before any scaling) Longitude
        public static int[] DX_Y = new int[1000];                   // y pixel location on map (before any scaling) Latitude
        public static string[] DX_country = new string[1000];       // country

        public static int DX_Index = 0;                               //  max number of spots in memory currently
        public static int DX_Index1 = 0;                             // local index that reset back to 0 after reaching max
        public static int DX_Last = 0;                               //  last # in DX_Index (used for DXLOC_Mapper)spotter(
        public static int Map_Last = 0;                               //  last map checkbox change (used for DXLOC_Mapper) 1=update grayline 2=update spots on map only
        public static int DXK_Last = 0;                               //  last # in console.DXK (used for DXLOC_Mapper)

        public static string[] DX_FULLSTRING = new string[1000];       // full undecoded message

        public static byte DX_new = 0;
        public static string DX_temp = "          ";

        public static int UTCNEW = Convert.ToInt16(FD);                        // convert 24hr UTC to int
        public static int UTCLAST = 0;                                        // last utc time for determining when to check again
        public static int UTCLASTMIN = 0;                                        // last utc time for determining when to check again

        private bool detectEncodingFromByteOrderMarks = true;

        private bool pause = false; // true = pause dx spot window update.

        private static byte Flag8 = 0; // 1= DX_Index value changed due to spot age cut, 

        //====================================================================================================
        //====================================================================================================
        // ke9ns add Thread routine (get DX spots)

        private void SPOTTER()  // ke9ns Thread opeation (runs in en-us culture) opens internet connection to genearte list of dx spots
        {

            DXLOC_FILE(); // open DXLOC.txt file and put into array of lat/lon values vs prefix


            try // opening socket
            {
                textBox1.Text += "Attempt Opening socket \r\n";

                client = new TcpClient(); // for new socket


                DXMemRecord nodeBox5 = new DXMemRecord(console.DXMemList.List[dataGridView1.CurrentCell.RowIndex]); // ke9ns 

                nodeBox1.Text = nodeBox5.DXURL; // get string from DXMemory file based on current pointed to index

                Debug.WriteLine("node " + nodeBox1.Text);


                if (nodeBox1.Text.Contains(":") == true)
                {
                    int ind = nodeBox1.Text.IndexOf(":") + 1;
                    int ind1 = nodeBox1.Text.Length;

                    portBox2.Text = dataGridView1.CurrentCell.RowIndex.ToString(); // to store

                    string PORT1 = nodeBox1.Text.Substring(ind, ind1 - ind);
                    string URL1 = nodeBox1.Text.Substring(0, ind - 1);

                    Debug.WriteLine("url " + URL1);
                    Debug.WriteLine("port " + PORT1);
                    Debug.WriteLine("index " + portBox2.Text);


                    client.Connect(URL1, Convert.ToInt16(PORT1));      // 'EXAMPLE  client.Connect("192.168.0.149", 230) 
                }
                else
                {
                    Debug.WriteLine("NO PORT# detected us 7000 ");

                    client.Connect(nodeBox1.Text, 7000);      // 'EXAMPLE  client.Connect("192.168.0.149", 230) 
                }


                networkStream = client.GetStream();

                SP_reader = new StreamReader(networkStream, Encoding.ASCII, detectEncodingFromByteOrderMarks); //Encoding.UTF8  or detectEncodingFromByteOrderMarks
                SP_writer = new BinaryWriter(networkStream, Encoding.UTF7);


                var sb = new StringBuilder(message2);
                statusBox.ForeColor = Color.Red;
                console.spotterMenu.ForeColor = Color.Red;

                statusBox.Text = "Socket";
                console.spotterMenu.Text = "Socket";

                textBox1.Text += "Got Socket \r\n";


                for (; SP_Active > 0; ) // shut down socket and thread if SP_Active = 1
                {

                    if (SP_Active == 1) // if you shut down dont attempt to read next spot
                    {
                        sb.Append((char)SP_reader.Read(), 1);

                        message3 = sb.ToString();

                        if ((message3.Contains("login: ")) || (message3.Contains("Please enter your call: ")))
                        {
                            textBox1.Text += "Got login: prompt \r\n";


                            sb = new StringBuilder(message2); // clear sb string over


                            char[] message5 = callBox.Text.ToCharArray(); // get your call sign

                            for (int i = 0; i < message5.Length; i++)    // do it this way because telnet server wants slow typing
                            {
                                SP_writer.Write((char)message5[i]);

                            }  // for loop length of your call sign

                            SP_writer.Write((char)13);

                            SP_writer.Write((char)10);


                            statusBox.ForeColor = Color.Red;
                            console.spotterMenu.ForeColor = Color.Red;

                            statusBox.Text = "Login";
                            console.spotterMenu.Text = "Login";


                            SP_Active = 2; // logging in

                        } // look for login:

                    } // SP_active = 1
                    else if (SP_Active == 2)
                    {
                        SP_Active = 3; // logging in

                        statusBox.ForeColor = Color.Green;
                        console.spotterMenu.ForeColor = Color.Blue;

                        statusBox.Text = "Spotting";
                        console.spotterMenu.Text = "Spot";

                        textBox1.Text += "Waiting for DX Spots \r\n";


                        //  DX_Index = 0; // start at begining

                    } // SP_active == 2


                    //------------------------------------------------------------------------
                    // ke9nsformat:  
                    // 0     6            23 26          39                  70    76   80818283 (83 with everything or 79 with no Grid)
                    // DX de ke9ns:  7003.5  kc9ffv      up 5                0204Z en52 \a\a\r\n
                    //------------------------------------------------------------------------
                    else if (SP_Active > 2)
                    {
                        sb = new StringBuilder(); // clear sb string over again

                        try // use try since its a socket and can fail
                        {
                            //  SP_reader.BaseStream.ReadTimeout = 3000; // 5000 cause character Read to break every 5 seconds to check age of DX spots

                            //-------------------------------------------------------------------------------------------------------------------------------------
                            // ke9ns wait for a new message

                            for (; !(sb.ToString().Contains("\r\n")); ) //  wait for end of line
                            {
                                processDXAGE();

                                Thread.Sleep(50); // slow down the thread here

                                sb.Append((char)SP_reader.Read());  // get next char from socket and add it to build the next dx spot string to parse out 


                                if (SP_Active == 0)
                                {
                                    Debug.WriteLine("break====="); // if user wants to shut down operation 
                                    break;
                                }

                                if (sb.ToString().Length > 90)
                                {
                                    Debug.WriteLine("Leng ====="); // string too long (something happened
                                    sb = new StringBuilder(); // clear sb string over again
                                }


                            }// for (;!(sb.ToString().Contains("\r\n"));) //  wait for end of line
                            //-------------------------------------------------------------------------------------------------------------------------------------


                            statusBox.ForeColor = Color.Green;
                            statusBox.Text = "Spotting";
                            SP_Active = 3;


                            sb.Replace("\a", "");// get rig of bell 
                            sb.Replace("\r", "");// get rig of cr 
                            sb.Replace("\n", "");// get rig of line feed 

                            int qq = sb.Length;
                            // Debug.WriteLine("message1 length " + qq);

                            if (qq == 75) // if no grid, then add spaces and CR and line feed
                            {
                                sb.Append("     "); // keep all strings the same length
                            }

                            message1 = sb.ToString(); // post message
                            message1.TrimEnd('\0');

                            // ke9ns so at this point all messages are 82 characters long (as though they have a grid#, even if they dont)
                            //   Debug.WriteLine("message2 length " + message1.Length);

                        }
                        catch // read timeout comes here
                        {
                            //  processDXAGE();
                            //   if (Flag8 == 0) continue; // if DX_Index value changed due to age, then proceess otherwise continue around
                            //  Flag8 = 0;

                            continue;

                        } // end of catch (read timeout comes here)


                        Debug.WriteLine("message " + message1);


                        //-------------------------------------------------------------------------------------------------------------------------------------
                        // ke9ns process received message
                        if ((message1.StartsWith("DX de ") == true) && (message1.Length > 76)) // string can be 77 (with no grid) or 82 (with grid)
                        {

                            DX_Index1 = 250; // use 900 as a temp holding spot. always fill from the top


                            // grab DX_Spotter=======================================================================================
                            try
                            {
                                DX_Spotter[DX_Index1] = message1.Substring(6, 10); // get dx call with : at the end
                                Debug.WriteLine("DX_Call " + DX_Station[DX_Index1]);

                                int pos = 10;
                                if (DX_Spotter[DX_Index1].Contains(":"))
                                {
                                    pos = DX_Spotter[DX_Index1].IndexOf(':'); // find the :
                                }
                                else
                                {
                                    pos = DX_Spotter[DX_Index1].IndexOf(' '); // find the first space instead of the :
                                }


                                DX_Spotter[DX_Index1] = DX_Spotter[DX_Index1].Substring(0, pos); // reduce the call without the :

                                sb = new StringBuilder(DX_Spotter[DX_Index1]); // clear sb string over again
                                sb.Append('>');
                                sb.Insert(0, '<'); // to differentiate the spotter from the spotted

                                DX_Spotter[DX_Index1] = sb.ToString();

                            }
                            catch (FormatException e)
                            {
                                DX_Spotter[DX_Index1] = "NA";

                                //    textBox1.Text = e.ToString();
                            }
                            catch (ArgumentOutOfRangeException e)
                            {

                                //    textBox1.Text = e.ToString();
                            }
                            //    Debug.WriteLine("DX_Call " + DX_Station[DX_Index1]);

                            // grab DX_Freq========================================================================================
                            try
                            {

                                DX_Freq[DX_Index1] = (int)((double)Convert.ToDouble(message1.Substring(15, 9)) * (double)1000.0); //  get dx freq 7016.0  in khz 

                                if ((DX_Freq[DX_Index1] >= 1800000) && (DX_Freq[DX_Index1] <= 1830000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 3500000) && (DX_Freq[DX_Index1] <= 3600000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 7000000) && (DX_Freq[DX_Index1] <= 7125000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 10000000) && (DX_Freq[DX_Index1] <= 11000000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 14000000) && (DX_Freq[DX_Index1] <= 14150000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 18000000) && (DX_Freq[DX_Index1] <= 18110000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 21000000) && (DX_Freq[DX_Index1] <= 21200000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 24800000) && (DX_Freq[DX_Index1] <= 24930000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 28000000) && (DX_Freq[DX_Index1] <= 28300000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 50000000) && (DX_Freq[DX_Index1] <= 50100000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if ((DX_Freq[DX_Index1] >= 144000000) && (DX_Freq[DX_Index1] <= 144100000))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode
                                }
                                else if (
                                    (DX_Freq[DX_Index1] == 1885000) || (DX_Freq[DX_Index1] == 1900000) || (DX_Freq[DX_Index1] == 1945000) || (DX_Freq[DX_Index1] == 1985000)
                                    || (DX_Freq[DX_Index1] == 3825000) || (DX_Freq[DX_Index1] == 3870000) || (DX_Freq[DX_Index1] == 3880000) || (DX_Freq[DX_Index1] == 38850000)
                                     || (DX_Freq[DX_Index1] == 7290000) || (DX_Freq[DX_Index1] == 7295000) || (DX_Freq[DX_Index1] == 14286000) || (DX_Freq[DX_Index1] == 18150000)
                                     || (DX_Freq[DX_Index1] == 21285000) || (DX_Freq[DX_Index1] == 21425000) || ((DX_Freq[DX_Index1] >= 29000000) && (DX_Freq[DX_Index1] < 29200000))
                                     || (DX_Freq[DX_Index1] == 50400000) || (DX_Freq[DX_Index1] == 50250000) || (DX_Freq[DX_Index1] == 144400000) || (DX_Freq[DX_Index1] == 144425000)
                                     || (DX_Freq[DX_Index1] == 144280000) || (DX_Freq[DX_Index1] == 144450000)

                                    )
                                {
                                    DX_Mode[DX_Index1] = 14; // AM mode
                                }
                                else if (
                                         ((DX_Freq[DX_Index1] >= 146000000) && (DX_Freq[DX_Index1] <= 148000000)) || ((DX_Freq[DX_Index1] >= 29200000) && (DX_Freq[DX_Index1] <= 29270000))
                                       || ((DX_Freq[DX_Index1] >= 144500000) && (DX_Freq[DX_Index1] <= 144900000)) || ((DX_Freq[DX_Index1] >= 145100000) && (DX_Freq[DX_Index1] <= 145500000))
                                       )
                                {
                                    DX_Mode[DX_Index1] = 114; // FM mode
                                }

                                else
                                {
                                    DX_Mode[DX_Index1] = 0; // ssb mode
                                }


                            } // try to determine if in the cw portion or ssb portion of each band
                            catch (FormatException e)
                            {
                                DX_Freq[DX_Index1] = 0;
                                DX_Mode[DX_Index1] = 0; // ssb mode
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                DX_Freq[DX_Index1] = 0;
                                DX_Mode[DX_Index1] = 0; // ssb mode
                            }
                            Debug.WriteLine("DX_Freq " + DX_Freq[DX_Index1]);


                            // grad DX_Station=========================================================================================

                            try
                            {
                                DX_Station[DX_Index1] = message1.Substring(26, 13); // get dx call with : at the end
                                int pos = DX_Station[DX_Index1].IndexOf(' '); // find the
                                DX_Station[DX_Index1] = DX_Station[DX_Index1].Substring(0, pos); // reduce the call without the
                            }
                            catch (FormatException)
                            {
                                DX_Spotter[DX_Index1] = "NA";
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                DX_Spotter[DX_Index1] = "NA";
                            }
                            Debug.WriteLine("DX_Spotter " + DX_Spotter[DX_Index1]);

                            // grab comments
                            try
                            {
                                DX_Mode2[DX_Index1] = 0; // reset split hz

                                DX_Message[DX_Index1] = message1.Substring(39, 29).ToLower(); // get dx call with : at the end


                                if (DX_Message[DX_Index1].Contains("cw"))
                                {
                                    DX_Mode[DX_Index1] = 1; // cw mode

                                }
                                else if (DX_Message[DX_Index1].Contains(" rty ") || DX_Message[DX_Index1].Contains("rtty"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 2; // RTTY mode

                                }
                                else if (DX_Message[DX_Index1].Contains("psk"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 3; // psk mode

                                }
                                else if (DX_Message[DX_Index1].Contains("oliv"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 4; // olivia mode

                                }
                                else if (DX_Message[DX_Index1].Contains("jt65"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 5; // jt65 mode

                                }
                                else if (DX_Message[DX_Index1].Contains("contesa"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 6; // contesa mode

                                }
                                else if (DX_Message[DX_Index1].Contains("fsk"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 7; // fsk mode

                                }
                                else if (DX_Message[DX_Index1].Contains("mt63"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 8; // mt63 mode

                                }
                                else if (DX_Message[DX_Index1].Contains("domi"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 9; // domino mode

                                }
                                else if (DX_Message[DX_Index1].Contains("packact") || DX_Message[DX_Index1].Contains("packtor") || DX_Message[DX_Index1].Contains("amtor"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 10; // pactor mode

                                }
                                else if (DX_Message[DX_Index1].Contains("fm "))
                                {
                                    if (chkBoxSSB.Checked != true) continue; // check for a SSB mode spot
                                    DX_Mode[DX_Index1] = 11; // fm mode

                                }
                                else if (DX_Message[DX_Index1].Contains("drm"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 12; // DRM mode

                                }
                                else if (DX_Message[DX_Index1].Contains("sstv"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 13; // sstv mode

                                }
                                else if (DX_Message[DX_Index1].Contains("easypal"))
                                {
                                    if (chkBoxDIG.Checked != true) continue; // check for a Digitla mode spot
                                    DX_Mode[DX_Index1] = 12; // drm mode

                                }
                                else if (DX_Message[DX_Index1].Contains(" am ") || DX_Message[DX_Index1].Contains(" sam "))
                                {
                                    if (chkBoxSSB.Checked != true) continue; // check for a SSB mode spot
                                    DX_Mode[DX_Index1] = 14; // AM mode

                                }




                                if (DX_Mode[DX_Index1] == 0)
                                {

                                    if (chkBoxSSB.Checked != true)
                                    {
                                        Debug.WriteLine("bypass ssb because not looking for ssb");
                                        continue; // check for a SSB mode spot
                                    }

                                }

                                if (DX_Mode[DX_Index1] == 1)
                                {

                                    if (chkBoxCW.Checked != true)
                                    {
                                        Debug.WriteLine("bypass CW because not looking for CW");
                                        continue; // check for a CW mode spot
                                    }

                                }

                                //----------------------------------------------------------

                                // grab GRID #
                                DX_Grid[DX_Index1] = message1.Substring(76, 4); // get grid

                                sb = new StringBuilder(DX_Grid[DX_Index1]); // clear sb string over again
                                sb.Append(')');
                                sb.Insert(0, '('); // to differentiate the spotter from the spotted

                                DX_Grid[DX_Index1] = sb.ToString();




                                if (DX_Message[DX_Index1].Contains("<") && DX_Message[DX_Index1].Contains(">")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf(">") + 1;

                                    try // 
                                    {
                                        DX_Grid[DX_Index1] = DX_Message[DX_Index1].Substring(ind, 6);
                                        Debug.WriteLine("FOUND COMMENT GRID " + DX_Grid[DX_Index1]);

                                    }
                                    catch // 
                                    {

                                    }

                                } // get Grid from comments


                                //----------------------------------------------------------

                                DX_Mode2[DX_Index1] = 0;
                                //  resultString = Regex.Match(subjectString, @"\d+").Value;  Int32.Parse(resultString) will then give you the number.

                                if (DX_Message[DX_Index1].Contains("up")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("up") + 2;

                                    try // try 1
                                    {
                                        int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000));
                                        Debug.WriteLine("Found UP split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000));
                                            Debug.WriteLine("Found UP split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000));
                                                Debug.WriteLine("Found UP split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                int ind1 = DX_Message[DX_Index1].IndexOf("up") - 4; //

                                                try // try 4
                                                {

                                                    int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 4)) * 1000));
                                                    Debug.WriteLine("Found UP split hz" + split_hz);
                                                    DX_Mode2[DX_Index1] = split_hz;
                                                }
                                                catch // catch 4
                                                {
                                                    ind1++; //

                                                    try // try 5
                                                    {

                                                        int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 3)) * 1000));
                                                        Debug.WriteLine("Found UP split hz" + split_hz);
                                                        DX_Mode2[DX_Index1] = split_hz;
                                                    }
                                                    catch // catch 5
                                                    {
                                                        ind1++; //

                                                        try // try 6
                                                        {

                                                            int split_hz = (int)(Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 2)) * 1000));
                                                            Debug.WriteLine("Found UP split hz" + split_hz);
                                                            DX_Mode2[DX_Index1] = split_hz;
                                                        }
                                                        catch // catch 6
                                                        {

                                                            Debug.WriteLine("failed to find up value================");
                                                            DX_Mode2[DX_Index1] = 1000; // 1khz up

                                                        } // catch6   (2 digits to left side)

                                                    } // catch5   (3 digits to left side)

                                                } // catch4   (4 digits to left side)

                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)


                                } // split up

                                else if (DX_Message[DX_Index1].Contains("dn")) // check for split down
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("dn") + 2;

                                    try // try 1
                                    {
                                        int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000));
                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000));
                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000));
                                                Debug.WriteLine("Found dn split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                int ind1 = DX_Message[DX_Index1].IndexOf("dn") - 4; //

                                                try // try 4
                                                {
                                                    int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 4)) * 1000));
                                                    Debug.WriteLine("Found dn split hz" + split_hz);
                                                    DX_Mode2[DX_Index1] = split_hz;
                                                }
                                                catch // catch 4
                                                {
                                                    ind++; //

                                                    try // try 5
                                                    {
                                                        int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 3)) * 1000));
                                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                                        DX_Mode2[DX_Index1] = split_hz;
                                                    }
                                                    catch // catch 5
                                                    {
                                                        ind1++; //

                                                        try // try 6
                                                        {
                                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 2)) * 1000));
                                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                                            DX_Mode2[DX_Index1] = split_hz;
                                                        }
                                                        catch // catch 6
                                                        {

                                                            Debug.WriteLine("failed to find dn value================");
                                                            DX_Mode2[DX_Index1] = -1000; // 1khz dn

                                                        } // catch6   (2 digits to left side)

                                                    } // catch5   (3 digits to left side)

                                                } // catch4   (4 digits to left side)

                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)


                                } // split down
                                else if (DX_Message[DX_Index1].Contains("dwn")) // check for split down
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("dwn") + 3;

                                    try // try 1
                                    {
                                        int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000));
                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000));
                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000));
                                                Debug.WriteLine("Found dn split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                int ind1 = DX_Message[DX_Index1].IndexOf("dwn") - 4; //

                                                try // try 4
                                                {
                                                    int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 4)) * 1000));
                                                    Debug.WriteLine("Found dn split hz" + split_hz);
                                                    DX_Mode2[DX_Index1] = split_hz;
                                                }
                                                catch // catch 4
                                                {
                                                    ind1++; //

                                                    try // try 5
                                                    {
                                                        int split_hz = (int)(-Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 3)) * 1000);
                                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                                        DX_Mode2[DX_Index1] = split_hz;
                                                    }
                                                    catch // catch 5
                                                    {
                                                        ind1++; //

                                                        try // try 6
                                                        {
                                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 2)) * 1000));
                                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                                            DX_Mode2[DX_Index1] = split_hz;
                                                        }
                                                        catch // catch 6
                                                        {

                                                            Debug.WriteLine("failed to find dn value================");
                                                            DX_Mode2[DX_Index1] = -1000; // 1khz dn

                                                        } // catch6   (2 digits to left side)

                                                    } // catch5   (3 digits to left side)

                                                } // catch4   (4 digits to left side)

                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)


                                } // split down
                                else if (DX_Message[DX_Index1].Contains("down")) // check for split down
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("down") + 4;

                                    try // try 1
                                    {
                                        int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000));
                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000));
                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000));
                                                Debug.WriteLine("Found dn split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                int ind1 = DX_Message[DX_Index1].IndexOf("down") - 4; //

                                                try // try 4
                                                {
                                                    int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 4)) * 1000));
                                                    Debug.WriteLine("Found dn split hz" + split_hz);
                                                    DX_Mode2[DX_Index1] = split_hz;
                                                }
                                                catch // catch 4
                                                {
                                                    ind1++; //

                                                    try // try 5
                                                    {
                                                        int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 3)) * 1000));
                                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                                        DX_Mode2[DX_Index1] = split_hz;
                                                    }
                                                    catch // catch 5
                                                    {
                                                        ind1++; //

                                                        try // try 6
                                                        {
                                                            int split_hz = (int)(-Math.Abs(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind1, 2)) * 1000));
                                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                                            DX_Mode2[DX_Index1] = split_hz;
                                                        }
                                                        catch // catch 6
                                                        {

                                                            Debug.WriteLine("failed to find dn value================");
                                                            DX_Mode2[DX_Index1] = -1000; // 1khz dn

                                                        } // catch6   (2 digits to left side)

                                                    } // catch5   (3 digits to left side)

                                                } // catch4   (4 digits to left side)

                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)

                                } // split down
                                else if ((DX_Message[DX_Index1].Contains("9+")) || (DX_Message[DX_Index1].Contains("59+"))) // check for split
                                {
                                    // ignore + if its part of s9+
                                }

                                else if (DX_Message[DX_Index1].Contains("+")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("+") + 1;

                                    try // try 1
                                    {
                                        int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000);
                                        Debug.WriteLine("Found UP split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000);
                                            Debug.WriteLine("Found UP split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000);
                                                Debug.WriteLine("Found UP split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                Debug.WriteLine("failed to find up value================");
                                                DX_Mode2[DX_Index1] = 0; // 


                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)

                                    //  if (DX_Mode2[DX_Index1] > 9000) DX_Mode2[DX_Index1] = 0;

                                } // split up+

                                else if (DX_Message[DX_Index1].Contains(" -")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("-") + 1;

                                    try // try 1
                                    {
                                        int split_hz = (int)(-Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 4)) * 1000);
                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;
                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(-Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 3)) * 1000);
                                            Debug.WriteLine("Found dn split hz" + split_hz);
                                            DX_Mode2[DX_Index1] = split_hz;
                                        }
                                        catch // catch 2
                                        {

                                            try // try 3
                                            {
                                                int split_hz = (int)(-Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 2)) * 1000);
                                                Debug.WriteLine("Found dn split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;
                                            }
                                            catch // catch 3
                                            {

                                                Debug.WriteLine("failed to find up value================");
                                                DX_Mode2[DX_Index1] = 0; // 


                                            } // catch3   (2 digits to right side)

                                        } //catch2  (3 digits to right side)

                                    } // catch 1   (4 digits to right side)


                                } // split dwn -

                                else if (DX_Message[DX_Index1].Contains("qsx")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("qsx") + 3;

                                    try // try 1
                                    {
                                        int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 8)) * 1000);
                                        Debug.WriteLine("Found dn split hz" + split_hz);
                                        DX_Mode2[DX_Index1] = split_hz;

                                        DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];


                                    }
                                    catch // catch 1
                                    {

                                        try // try 2
                                        {
                                            int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 7)) * 1000);

                                            if (split_hz < 10000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX .412  then treat it with the same mhz as DX_Freq
                                            else if (split_hz < 100000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX 18.412  then it must be in mhz

                                            Debug.WriteLine("Found qrx split hz" + split_hz);

                                            DX_Mode2[DX_Index1] = split_hz;
                                            DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];


                                        }
                                        catch // catch 2
                                        {
                                            try // try 3
                                            {
                                                int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 6)) * 1000);

                                                if (split_hz < 10000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX .412  then treat it with the same mhz as DX_Freq
                                                else if (split_hz < 100000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX 18.412  then it must be in mhz

                                                Debug.WriteLine("Found dn split hz" + split_hz);

                                                DX_Mode2[DX_Index1] = split_hz;
                                                DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];


                                            }
                                            catch // catch 3
                                            {

                                                Debug.WriteLine("failed to find up value================");
                                                DX_Mode2[DX_Index1] = 0; // 

                                            } // catch 3   (6 digits to right side)

                                        } // catch 2   (7 digits to right side)

                                    } // catch 1   (8 digits to right side)


                                } // split qrx

                                else if (DX_Message[DX_Index1].Contains("qrz")) // check for split
                                {

                                    int ind = DX_Message[DX_Index1].IndexOf("qrz") + 3;

                                    try // try 1
                                    {
                                        int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 8)) * 1000);

                                        if (split_hz < 10000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX .412  then treat it with the same mhz as DX_Freq
                                        else if (split_hz < 100000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX 18.412  then it must be in mhz

                                        Debug.WriteLine("Found qrz split hz" + split_hz);

                                        DX_Mode2[DX_Index1] = split_hz;
                                        DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];


                                    }
                                    catch // catch 1
                                    {
                                        try // try 2
                                        {
                                            int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 7)) * 1000);

                                            if (split_hz < 10000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX .412  then treat it with the same mhz as DX_Freq
                                            else if (split_hz < 100000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX 18.412  then it must be in mhz

                                            Debug.WriteLine("Found qrz split hz" + split_hz);

                                            DX_Mode2[DX_Index1] = split_hz;
                                            DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];

                                        }
                                        catch // catch 2
                                        {
                                            try // try 3
                                            {
                                                int split_hz = (int)(Convert.ToDouble(DX_Message[DX_Index1].Substring(ind, 6)) * 1000);

                                                if (split_hz < 10000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX .412  then treat it with the same mhz as DX_Freq
                                                else if (split_hz < 100000) split_hz = (DX_Freq[DX_Index1] / 1000000) + split_hz; // if its QRX 18.412  then it must be in mhz

                                                Debug.WriteLine("Found dn split hz" + split_hz);
                                                DX_Mode2[DX_Index1] = split_hz;

                                                DX_Mode2[DX_Index1] = DX_Mode2[DX_Index1] - DX_Freq[DX_Index1];


                                            }
                                            catch // catch 3
                                            {

                                                Debug.WriteLine("failed to find up value================");
                                                DX_Mode2[DX_Index1] = 0; // 

                                            } // catch 3   (6 digits to right side)

                                        } // catch 2   (7 digits to right side)

                                    } // catch 1   (8 digits to right side)


                                } // split qrz




                            } // try to parse dx spot message above
                            catch (FormatException e)
                            {
                                Debug.WriteLine("mode issue" + e);

                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                Debug.WriteLine("mode1 issue" + e);

                            }
                            //  Debug.WriteLine("DX_Message " + DX_Message[DX_Index1]);

                            // grab time
                            try
                            {
                                //  DX_Time[DX_Index1] = Convert.ToInt16(message1.Substring(70, 4)); // get time from dx spot

                                DX_Time[DX_Index1] = UTCNEW; // use the my UTC because some spotters have issues and the spot has the wrong time in it.


                            }
                            catch (Exception)
                            {
                                DX_Time[DX_Index1] = UTCNEW;

                            }

                            //   Debug.WriteLine("DX_Time " + DX_Time[DX_Index1])



                            // set age of spot to 0;
                            DX_Age[DX_Index1] = "00"; // reset to start




                            //=================================================================================================
                            //=================================================================================================

                            // CHECK HERE FOR (NA) NORTH AMERICAN,  OR EXCLUDE NORTH AMERICAN SPOTS

                            //=================================================================
                            //=================================================================
                            //=================================================================
                            // ke9ns DX SPOT FILTERS (EXCLUDE NA HERE)
                            if (checkBoxWorld.Checked) // filter out US calls signs
                            {

                                string us1 = DX_Spotter[DX_Index1].Substring(1, 1); // grab first char of Spotter callsign becuase I added a < > around the spotter callsign
                                string us2 = DX_Spotter[DX_Index1].Substring(2, 1); // grab second char of Spotter callsign

                                //   Debug.WriteLine("us1 " + us1 + " us2 " + us2);

                                Regex r = new Regex("[KNWAX]"); // first char (include X for Mexico in the NA spots)
                                Regex r1 = new Regex("[A-Z0-9]"); // 2nd char to select as a NA spot
                                Regex r2 = new Regex("[ABCDEFGYO]"); // 2nd char // for V as the first char for Canada
                                Regex r3 = new Regex("[YGFIJK]"); // 2nd char // for C as the first char


                                if ((us1 == "V") || (us1 == "C")) // check for Canada (NA)
                                {
                                    if (((us1 == "V") && (r2.IsMatch(us2))) || ((us1 == "C") && (r3.IsMatch(us2))))
                                    {
                                        Debug.WriteLine("bypass4a " + DX_Spotter[DX_Index1]);
                                        continue; // dont show spot if not on the r1 list
                                    }
                                    goto PASS2; // if the 1st letter is not a US letter then GOOD use SPOT

                                }
                                else
                                {
                                    if ((r.IsMatch(us1)))
                                    {
                                        Debug.WriteLine("bypass3 " + DX_Spotter[DX_Index1]);
                                        continue;// dont show spot if not on the r list
                                    }

                                    // Debug.WriteLine("============CHECK3, fist us1 letter good for not being NA " + DX_Spotter[DX_Index1]);
                                    goto PASS2; // if the 1st letter is not a US letter then GOOD use SPOT

                                }

                                if ((r1.IsMatch(us2)))
                                {
                                    Debug.WriteLine("bypass4 " + DX_Spotter[DX_Index1]);
                                    continue; // dont show spot if not on the r1 list
                                }


                            }
                            else if (checkBoxUSspot.Checked) // filter out call signs outside of NA
                            {

                                string us1 = DX_Spotter[DX_Index1].Substring(1, 1);// was 0,1 now 1,1 because I added <>
                                string us2 = DX_Spotter[DX_Index1].Substring(2, 1);// was 1,1

                                Debug.WriteLine("us1 " + us1 + " us2 " + us2);


                                Regex r = new Regex("[KNWAVX]"); // first char
                                Regex r1 = new Regex("[A-Z0-9]"); // 2nd char
                                Regex r2 = new Regex("[ABCDEFGYO]"); // 2nd char // for V as the first char
                                Regex r3 = new Regex("[YGFIJK]"); // 2nd char // for C as the first char

                                if (!(r.IsMatch(us1)))
                                {
                                    Debug.WriteLine("bypass1 " + DX_Spotter[DX_Index1]);
                                    continue;// dont show spot if not on the r list
                                }

                                if ((us1 == "V") || (us1 == "C"))
                                {
                                    if (((us1 == "V") && !(r2.IsMatch(us2))) || ((us1 == "C") && !(r3.IsMatch(us2))))
                                    {
                                        Debug.WriteLine("bypass2a " + DX_Spotter[DX_Index1]);
                                        continue; // dont show spot if not on the r1 list
                                    }
                                }
                                else
                                {
                                    if (!(r1.IsMatch(us2)))
                                    {
                                        Debug.WriteLine("bypass2 " + DX_Spotter[DX_Index1]);
                                        continue; // dont show spot if not on the r1 list
                                    }
                                }


                            } // checkBoxUSspot.Checked)

                            SP4_Active = 1; // processing message

                            //=================================================================
                        //=================================================================
                        //=================================================================
                        // ke9ns check for STATION DUPLICATES , there can only be 1 possible duplicate per spot added (but IGNORE if on 2nd band)
                        PASS2: int xx = 0;


                            for (int ii = 0; ii <= DX_Index; ii++)
                            {
                                if ((xx == 0) && (DX_Station[DX_Index1] == DX_Station[ii]))
                                {
                                    if ((Math.Abs(DX_Freq[DX_Index1] - DX_Freq[ii])) < 1000000)
                                    {

                                        xx = 1;
                                        Debug.WriteLine("station dup============" + DX_Freq[ii] + " dup " + DX_Freq[DX_Index1] + " dup " + DX_Station[DX_Index1] + " dup " + DX_Station[ii]);
                                    } // freq too close so its a dup
                                }

                                if (xx == 1)
                                {
                                    DX_FULLSTRING[ii] = DX_FULLSTRING[ii + 1]; // 

                                    DX_Station[ii] = DX_Station[ii + 1];
                                    DX_Freq[ii] = DX_Freq[ii + 1];
                                    DX_Spotter[ii] = DX_Spotter[ii + 1];
                                    DX_Message[ii] = DX_Message[ii + 1];
                                    DX_Grid[ii] = DX_Grid[ii + 1];
                                    DX_Time[ii] = DX_Time[ii + 1];
                                    DX_Age[ii] = DX_Age[ii + 1];
                                    DX_Mode[ii] = DX_Mode[ii + 1];
                                    DX_Mode2[ii] = DX_Mode2[ii + 1];

                                    DX_country[ii] = DX_country[ii + 1];
                                    DX_X[ii] = DX_X[ii + 1];
                                    DX_Y[ii] = DX_Y[ii + 1];

                                }

                            } // for ii check for dup in list
                            DX_Index = (DX_Index - xx);  // readjust the length of the spot list after taking out the duplicates

                            //=================================================================================================
                            //=================================================================================================
                            // ke9ns check for FREQ DUPLICATES , there can only be 1 possible duplicate per spot added (but IGNORE if on 2nd band)
                            xx = 0;
                            for (int ii = 0; ii <= DX_Index; ii++)
                            {
                                if ((xx == 0) && (DX_Freq[DX_Index1] == DX_Freq[ii])) // if you already have this station in the spot list on the screen remove the old spot
                                {
                                    xx = 1;
                                    Debug.WriteLine("freq dup============");
                                }

                                if (xx == 1)
                                {
                                    DX_FULLSTRING[ii] = DX_FULLSTRING[ii + 1]; // 

                                    DX_Station[ii] = DX_Station[ii + 1];
                                    DX_Freq[ii] = DX_Freq[ii + 1];
                                    DX_Spotter[ii] = DX_Spotter[ii + 1];
                                    DX_Message[ii] = DX_Message[ii + 1];
                                    DX_Grid[ii] = DX_Grid[ii + 1];
                                    DX_Time[ii] = DX_Time[ii + 1];
                                    DX_Age[ii] = DX_Age[ii + 1];
                                    DX_Mode[ii] = DX_Mode[ii + 1];
                                    DX_Mode2[ii] = DX_Mode2[ii + 1];

                                    DX_country[ii] = DX_country[ii + 1];
                                    DX_X[ii] = DX_X[ii + 1];
                                    DX_Y[ii] = DX_Y[ii + 1];
                                }

                            } // for ii check for dup in list
                            DX_Index = (DX_Index - xx);  // readjust the length of the spot list after taking out the duplicates


                            //=================================================================================================
                            //=================================================================================================
                            // ke9ns  passed the spotter, dx station , freq, and time test

                            DX_Index++; // jump to PASS2 if it passed the valid call spotter test

                            if (DX_Index > 80)
                            {
                                Debug.WriteLine("DX SPOT REACH 80 ");
                                DX_Index = 80; // you have reached max spots
                            }

                            //   Debug.WriteLine("index "+ DX_Index);



                            //=================================================================================================
                            //=================================================================================================
                            // ke9ns FILO buffer after taking out duplicate from above
                            for (int ii = DX_Index; ii > 0; ii--)
                            {
                                DX_FULLSTRING[ii] = DX_FULLSTRING[ii - 1]; // move array stack down one (oldest dropped off)

                                DX_Station[ii] = DX_Station[ii - 1];
                                DX_Freq[ii] = DX_Freq[ii - 1];
                                DX_Spotter[ii] = DX_Spotter[ii - 1];
                                DX_Message[ii] = DX_Message[ii - 1];
                                DX_Grid[ii] = DX_Grid[ii - 1];
                                DX_Time[ii] = DX_Time[ii - 1];
                                DX_Age[ii] = DX_Age[ii - 1];
                                DX_Mode[ii] = DX_Mode[ii - 1];
                                DX_Mode2[ii] = DX_Mode2[ii - 1];

                                DX_country[ii] = DX_country[ii - 1];
                                DX_X[ii] = DX_X[ii - 1];
                                DX_Y[ii] = DX_Y[ii - 1];

                            } // for ii

                            DX_FULLSTRING[0] = message1; // add newest message to top


                            DX_Station[0] = DX_Station[DX_Index1];    //insert new spot on top of list now
                            DX_Freq[0] = DX_Freq[DX_Index1];
                            DX_Spotter[0] = DX_Spotter[DX_Index1];
                            DX_Message[0] = DX_Message[DX_Index1];
                            DX_Grid[0] = DX_Grid[DX_Index1];
                            DX_Time[0] = DX_Time[DX_Index1];
                            DX_Age[0] = DX_Age[DX_Index1];
                            DX_Mode[0] = DX_Mode[DX_Index1];
                            DX_Mode2[0] = DX_Mode2[DX_Index1];

                            //------------------------------------------------------------------------------------
                            //------------------------------------------------------------------------------------
                            //------------------------------------------------------------------------------------
                            // Crosscheck Station Call sign Prefix with data from DXLOC.txt (lat and lon) 
                            // and create a list of Country, Callsign, X, Y on unscaled map


                            updatemapspots();


                            //------------------------------------------------------------------------------------
                            //------------------------------------------------------------------------------------
                            //------------------------------------------------------------------------------------

                            Debug.WriteLine("INSTALL NEW [0]=========== " + DX_Index);


                            processTCPMessage(); // send to spot window
                            Debug.WriteLine("INSTALL NEW [1]=========== " + DX_Index);


                            SP4_Active = 0; // done processing message


                            //      Debug.WriteLine("Aindex " + DX_Index);

                        } // (message1.StartsWith("DX de ") valid message


                        else if (message1.Contains(" disconnected"))
                        {
                            textBox1.Text += "Your Socket was disconnected \r\n";

                            statusBox.ForeColor = Color.Red;
                            console.spotterMenu.ForeColor = Color.Red;

                            console.spotterMenu.Text = "Closed12345";
                            statusBox.Text = "Closed";


                            SP_writer.Close();                  // close down now
                            SP_reader.Close();
                            networkStream.Close();

                            client.Close();
                            //   Debug.WriteLine("END DX SPOT thread");

                            statusBox.ForeColor = Color.Black;
                            console.spotterMenu.ForeColor = Color.White;

                            console.spotterMenu.Text = "Spot";
                            statusBox.Text = "Off";
                            textBox1.Text += "All closed \r\n";
                            SP_Active = 0;
                            SP2_Active = 0;

                            return;
                        } // if disconnected



                    } // SP_active == 3 (getting spots here)


                } // for loop forever for this spotter thread

                // if you reach here, its because your closing down the socket

                //    Debug.WriteLine("END DX SPOT thread");


                statusBox.ForeColor = Color.Red;
                console.spotterMenu.ForeColor = Color.Red;

                console.spotterMenu.Text = "Closing";
                statusBox.Text = "Closing";

                textBox1.Text += "Asked to Close \r\n";


                SP_writer.Close();                  // close down now
                SP_reader.Close();
                networkStream.Close();


                client.Close();
                //   Debug.WriteLine("END DX SPOT thread");

                statusBox.ForeColor = Color.Black;
                console.spotterMenu.ForeColor = Color.White;

                console.spotterMenu.Text = "Spot";
                statusBox.Text = "Off";
                textBox1.Text += "All closed \r\n";
                SP2_Active = 0;
                SP_Active = 0;
                return;


            } // try
            catch (SocketException SE)
            {
                textBox1.Text += "Socket Forced closed \r\n";

                Debug.WriteLine("cannot open socket or socket closed on me" + SE);
                statusBox.ForeColor = Color.Red;
                console.spotterMenu.ForeColor = Color.Red;

                statusBox.Text = "Socket";
                console.spotterMenu.Text = "Socket";

                try
                {
                    SP_writer.Close();
                    SP_reader.Close();
                }
                catch (Exception)
                {


                }

                try
                {
                    networkStream.Close();
                    client.Close();
                }
                catch (Exception)
                {

                }


                SP_Active = 0;
                SP2_Active = 0;

                //   textBox1.Text += "Socket crash Done \r\n";
                statusBox.Text = "Closed";
                console.spotterMenu.Text = "Spot";

                //    textBox1.Text = SE.ToString();

                return;

            }
            catch (Exception e1)
            {
                textBox1.Text += "Socket Forced closed \r\n";

                Debug.WriteLine("socket exception issue" + e1);

                statusBox.ForeColor = Color.Red;
                console.spotterMenu.ForeColor = Color.Red;

                statusBox.Text = "Socket";
                console.spotterMenu.Text = "Socket";

                try
                {
                    SP_writer.Close();
                    SP_reader.Close();
                }
                catch (Exception)
                {


                }

                try
                {
                    networkStream.Close();
                    client.Close();
                }
                catch (Exception)
                {

                }

                SP2_Active = 0;

                statusBox.Text = "Closed";
                console.spotterMenu.Text = "Spot";


                return;
            }


        } // SPOTTER Thread



        //===================================================================================
        // ke9ns add update dx call and country on map
        private void updatemapspots()
        {


            //------------------------------------------------------------------------------------
            //------------------------------------------------------------------------------------
            //------------------------------------------------------------------------------------
            //------------------------------------------------------------------------------------
            // Crosscheck Station Call sign Prefix with data from DXLOC.txt (lat and lon) 
            // and create a list of Country, Callsign, X, Y on unscaled map

            if ((SP8_Active == 1) && (SP_Active > 2) && (SP5_Active == 1) && (DX_Index > 0)) // do if dxloc.txt file loaded in
            {

                int Sun_WidthY1 = Sun_Bot1 - Sun_Top1;             // # of Y pixels from top to bottom of map

                int Sun_Width = Sun_Right - Sun_Left;              //used by sun track routine

                Debug.WriteLine("MAPPING======");

                DX_Y[0] = 0;
                DX_X[0] = 0;
                DX_country[0] = null;

                int kk = 0;

                for (; kk < DXLOC_Index1; kk++)  // list of call sign prefixes and there corresponding LAT/LON  DXLOC_Index1 is from when file was read into memory
                {
                    if (DX_Station[0].StartsWith(DXLOC_prefix[kk]) == true) // look for a dx spot callsign prefix to make a match with the dxloc.txt list
                    {
                        if (DXLOC_prefix1[kk] != null)
                        {

                            if (DX_Station[0].Contains(DXLOC_prefix1[kk]) == false) continue; // dont choose if not a match

                        }

                        DX_Y[0] = (int)(((180 - (DXLOC_LAT[kk] + 90)) / 180.0) * Sun_WidthY1) + Sun_Top1;  //latitude 90N to -90S

                        DX_X[0] = (int)(((DXLOC_LON[kk] + 180.0) / 360.0) * Sun_Width) + Sun_Left;  // longitude -180W to +180E

                        DX_country[0] = DXLOC_country[kk]; // save country into dx spotter list (pulled from dxloc list)

                        Debug.WriteLine("MAPPER " + DX_Station[0] + " " + DX_X[0] + " " + DX_Y[0] + " cntry " + DX_country[0] + " prefix " + DXLOC_prefix[kk] + " lat " + DXLOC_LAT[kk] + " lon " + DXLOC_LON[kk]);

                        break; // got a match so break

                    }

                } // for kk loop for DXLOC in memory

                if (kk == DXLOC_Index1) // no match found
                {
                    DX_country[0] = " -- "; // dont have a match so need to add to list

                    Debug.WriteLine("MAPPER NO MACH FOR Station" + DX_Station[0]);

                }

            } // sp8_active = 1
            else
            {
                DX_country[0] = " -- "; // dont have a match so need to add to list

                //  Debug.WriteLine("mapper OFF");

            }


        } //  updatemapspots()








        //===================================================================================
        // ke9ns add process message for spot.cs window by right clicking
        private void processTCPMessage()
        {

            string bigmessage = null;

            for (int ii = 0; ii < DX_Index; ii++)
            {

                if (DX_Age[ii] == null) DX_Age[ii] = "00";
                else if (DX_Age[ii] == "  ") DX_Age[ii] = "00";

                if (DX_Age[ii].Length == 1) DX_Age[ii] = "0" + DX_Age[ii];

                //----------------------------------------------------------
                string DXmode = "    "; // 5 spaces

                if (DX_Mode[ii] == 0) DXmode = " ssb ";
                else if (DX_Mode[ii] == 1) DXmode = " cw  ";
                else if (DX_Mode[ii] == 2) DXmode = " rtty";
                else if (DX_Mode[ii] == 3) DXmode = " psk ";
                else if (DX_Mode[ii] == 4) DXmode = " oliv";
                else if (DX_Mode[ii] == 5) DXmode = " jt65";
                else if (DX_Mode[ii] == 6) DXmode = " cont";
                else if (DX_Mode[ii] == 7) DXmode = " fsk ";
                else if (DX_Mode[ii] == 8) DXmode = " mt63";
                else if (DX_Mode[ii] == 9) DXmode = " domi";
                else if (DX_Mode[ii] == 10) DXmode = " pack";
                else if (DX_Mode[ii] == 11) DXmode = " fm  ";
                else if (DX_Mode[ii] == 12) DXmode = " drm ";
                else if (DX_Mode[ii] == 13) DXmode = " sstv";
                else if (DX_Mode[ii] == 14) DXmode = " am  ";

                else DXmode = "     ";

                bigmessage += (DX_FULLSTRING[ii] + DXmode + " " + (DX_country[ii].PadRight(8)).Substring(0, 8) + " :" + DX_Age[ii] + "\r\n");

            } // for loop to update dx spot window

            if (pause == false) textBox1.Text = bigmessage; // update screen


        } //processTCPMessage


        //===================================================================================
        // ke9ns add process message for spot.cs window by right clicking
        private void processDXAGE()
        {


            UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);


            FD = UTCD.ToString("HHmm");                                       // get 24hr 4 digit UTC NOW
            UTCNEW = Convert.ToInt16(FD);                                    // convert 24hr UTC to int



            int hh = Convert.ToInt16(UTCD.ToString("HH"));
            int mm = Convert.ToInt16(UTCD.ToString("mm"));

            int UTCNEWMIN = mm + (60 * hh);
            //   Debug.WriteLine("time in minutes only " + UTCNEWMIN);



            if ((UTCLAST == 0) && (UTCNEW != 0))  // for startup only (ie. run 1 time)
            {
                UTCLAST = UTCNEW;
                DX_Time[0] = UTCNEW;
                UTCLASTMIN = UTCNEWMIN;

            }

            if ((DX_Index > 0) && (UTCNEW != UTCLAST))
            {
                int xxx = 0;

                UTCLAST = UTCNEW;

                int UTCDIFFMIN = UTCNEWMIN - UTCLASTMIN;  // difference in minutes from last time we checked the dx spots for age

                Debug.WriteLine("Time to Check DX Spot Age =========== " + DX_Index + " UTCNEWMIN " + UTCNEWMIN + " UTCLASTMIN " + UTCLASTMIN + " UTCDIFFMIN " + UTCDIFFMIN);

                UTCLASTMIN = UTCNEWMIN; // save for next go around

                if (UTCDIFFMIN < 0) // this indicates you crossed the timeline to the next day
                {
                    UTCDIFFMIN = 1440 - UTCDIFFMIN; // make positive again
                }

                for (int ii = DX_Index - 1; ii >= 0; ii--) // move from bottom of list up toward top of list
                {

                    //  int UTCDIFF = Math.Abs(UTCNEW - DX_Time[ii]); // time difference 
                    //DX_Age[ii] = UTCDIFF.ToString("00"); // 2 digits


                    int UTCAGE = Convert.ToInt32(DX_Age[ii]) + UTCDIFFMIN; // current age difference for DX spots

                    DX_Age[ii] = UTCAGE.ToString(); // age your DX spot


                    int kk = 0; // look at very bottom of list + 1


                    if (UTCAGE > 35) // if its an old SPOT then remove it from the list
                    {

                        Flag8 = 1; // signal that the DX_Index will change due to an old spot being removed

                        kk = ii; // 

                        xxx++; //shorten dx_Index by 1

                        Debug.WriteLine("time expire, remove=========spot " + DX_Time[ii] + " current time " + UTCLAST + " UTCDIFFMIN " + UTCDIFFMIN + " ii " + ii + " station " + DX_Station[ii]);
                        //   Debug.WriteLine("KK " + kk);
                        //   Debug.WriteLine("XXX " + xxx);


                        for (; kk < (DX_Index - xxx); kk++)
                        {

                            DX_FULLSTRING[kk] = DX_FULLSTRING[kk + xxx]; // 

                            DX_Station[kk] = DX_Station[kk + xxx];
                            DX_Freq[kk] = DX_Freq[kk + xxx];
                            DX_Spotter[kk] = DX_Spotter[kk + xxx];
                            DX_Message[kk] = DX_Message[kk + xxx];
                            DX_Grid[kk] = DX_Grid[kk + xxx];
                            DX_Time[kk] = DX_Time[kk + xxx];
                            DX_Age[kk] = DX_Age[kk + xxx];
                            DX_Mode[kk] = DX_Mode[kk + xxx];
                            DX_Mode2[kk] = DX_Mode2[kk + xxx];

                        } // for loop:  push OK Spots from bottom of list up as you delete old spots from list

                    } // TIMEOUT exceeded remove old spot



                } // for ii check for dup in list

                DX_Index = DX_Index - xxx;  // update DX_Index list (shorten if any old spots deleted)

                //   Debug.WriteLine("END=========== " + DX_Index);

                //   Debug.WriteLine(" ");

                processTCPMessage(); // update spot window (remove old spots)

                return;

            } // UTC NEW != LAST
            else
            {
                return; // skip
            }

        } // processDXAGE


        private void nameBox_MouseEnter(object sender, EventArgs e)
        {
            // ToolTip tt = new ToolTip();
            //  tt.Show("Name Name of DX Spider node with a > symbol at the end: Example: HB9DRV-9> or NN1D> ", nameBox, 10, 60, 2000);

        }

        private void callBox_MouseEnter(object sender, EventArgs e)
        {
            // ToolTip tt = new ToolTip();
            //  tt.Show("Your Call Sign to login to DX Spider node. Note: you must have used this call with this node prior to this first time ", callBox, 10, 60, 2000);
        }

        private void nodeBox_MouseEnter(object sender, EventArgs e)
        {
            // ToolTip tt = new ToolTip();
            //  tt.Show("Dx Spider node address ", nodeBox, 10, 60, 2000);
        }

        private void portBox_MouseEnter(object sender, EventArgs e)
        {
            //  ToolTip tt = new ToolTip();
            //  tt.Show("Port # that goes with the node address", portBox, 10, 60, 2000);
        }



        private void callBox_TextChanged(object sender, EventArgs e)
        {
            DX_Index = 0; // start over if change node

        }

        private void nameBox_TextChanged(object sender, EventArgs e)
        {

            DX_Index = 0; // start over if change node


        }

        private void portBox_TextChanged(object sender, EventArgs e)
        {
            DX_Index = 0; // start over if change node

        }

        private void nodeBox_TextChanged(object sender, EventArgs e)
        {

            DX_Index = 0; // start over if change node


        }


        private void SpotControl_Layout(object sender, LayoutEventArgs e)
        {




        }

        private void nameBox_Leave(object sender, EventArgs e)
        {
            callB = callBox.Text;
            nodeB = nodeBox1.Text;
            portB = portBox2.Text;
            nameB = nameBox.Text;
        }

        private void callBox_Leave(object sender, EventArgs e)
        {

            callB = callBox.Text;
            nodeB = nodeBox1.Text;
            portB = portBox2.Text;
            nameB = nameBox.Text;
        }

        private void nodeBox_Leave(object sender, EventArgs e)
        {

            callB = callBox.Text;
            nodeB = nodeBox1.Text;
            portB = portBox2.Text;
            nameB = nameBox.Text;
        }

        private void portBox_Leave(object sender, EventArgs e)
        {

            callB = callBox.Text;
            nodeB = nodeBox1.Text;
            portB = portBox2.Text;
            nameB = nameBox.Text;
        }


        private void checkBoxUSspot_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBoxUSspot.Checked == true)
            {

                //   Debug.WriteLine("US SPOT CHECKED");
                checkBoxWorld.Checked = false;

            }
            else
            {
                //   Debug.WriteLine("US SPOT UN-CHECKED");
            }

        } //checkBoxUSspot_CheckedChanged(


        private void checkBoxWorld_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxWorld.Checked == true)
            {
                checkBoxUSspot.Checked = false;
                //   Debug.WriteLine("world SPOT CHECKED");
            }
            else
            {
                //   Debug.WriteLine("world SPOT UN-CHECKED");
            }
        } // checkBoxWorld_CheckedChanged

        private void statusBox_Click(object sender, EventArgs e)
        {

            if ((SP_Active == 3)) // if DX cluster active then test it by sending a CR
            {

                try
                {
                    statusBox.ForeColor = Color.Red;

                    statusBox.Text = "Test Sent <CR>";

                    SP_writer.Write((char)13);
                    SP_writer.Write((char)10);
                }
                catch (Exception)
                {
                    statusBox.Text = "Failed Test";

                }

            } // if connection was supposed to be active

        } // statusBox_Click



        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            textBox1.ShortcutsEnabled = false;


            chkDXMode.Checked = true;

            if (e.Button == MouseButtons.Left)
            {

                int ii = textBox1.SelectionStart;

                byte iii = (byte)(ii / 99); // get line  /82  or /86 if AGE turned on or 91 if mode is also on /99 if country added

                if (DX_Index > iii)
                {
                    int freq1 = DX_Freq[iii];

                    if ((freq1 < 5000000) || ((freq1 > 6000000) && (freq1 < 8000000))) // check for bands using LSB
                    {
                        if (chkDXMode.Checked == true)
                        {
                            if (DX_Mode[iii] == 0) console.RX1DSPMode = DSPMode.LSB;
                            else if (DX_Mode[iii] == 1) console.RX1DSPMode = DSPMode.CWL;
                            else if (DX_Mode[iii] == 2) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 3) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 4) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 5) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 6) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 7) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 8) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 9) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 10) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 11) console.RX1DSPMode = DSPMode.FM;
                            else if (DX_Mode[iii] == 12) console.RX1DSPMode = DSPMode.LSB;
                            else if (DX_Mode[iii] == 13) console.RX1DSPMode = DSPMode.DIGL;
                            else if (DX_Mode[iii] == 14) console.RX1DSPMode = DSPMode.SAM;
                            else console.RX1DSPMode = DSPMode.LSB;


                        }
                        else
                        {
                            console.RX1DSPMode = DSPMode.LSB;
                        }
                    } // LSB
                    else
                    {
                        if (chkDXMode.Checked == true)
                        {

                            if (DX_Mode[iii] == 0) console.RX1DSPMode = DSPMode.USB;
                            else if (DX_Mode[iii] == 1) console.RX1DSPMode = DSPMode.CWU;
                            else if (DX_Mode[iii] == 2) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 3) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 4) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 5) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 6) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 7) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 8) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 9) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 10) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 11) console.RX1DSPMode = DSPMode.FM;
                            else if (DX_Mode[iii] == 12) console.RX1DSPMode = DSPMode.USB;
                            else if (DX_Mode[iii] == 13) console.RX1DSPMode = DSPMode.DIGU;
                            else if (DX_Mode[iii] == 14) console.RX1DSPMode = DSPMode.SAM;
                            else console.RX1DSPMode = DSPMode.USB;

                        }
                        else
                        {
                            console.RX1DSPMode = DSPMode.USB;
                        }
                    }


                    console.VFOAFreq = (double)freq1 / 1000000; // convert to MHZ

                    if (chkDXMode.Checked == true)
                    {

                        if (DX_Mode2[iii] != 0)
                        {

                            console.VFOBFreq = (double)(freq1 + DX_Mode2[iii]) / 1000000; // convert to MHZ
                            console.chkVFOSplit.Checked = true; // turn on  split

                            Debug.WriteLine("split here" + (freq1 + DX_Mode2[iii]));

                        }
                        else
                        {
                            console.chkVFOSplit.Checked = false; // turn off split

                        }


                    } // dxmode checked


                    button1.Focus();

                    Map_Last = 2; // redraw map spots

                } // make sure index you clicked on is within range

            } // left mouse button
            else if (e.Button == MouseButtons.Right)
            {

                if (SP4_Active == 0) // only process lookup if not processing a new spot which might cause issue
                {
                    int ii = textBox1.GetCharIndexFromPosition(e.Location);

                    byte iii = (byte)(ii / 99);  // get line  /82  or /86 if AGE turned on or 91 if mode is also on /99 if country added

                    if (DX_Index > iii)
                    {
                        string DXName = DX_Station[iii];

                        //  Debug.WriteLine("Line " + iii + " Name " + DXName);

                        try
                        {
                            System.Diagnostics.Process.Start("https://www.qrz.com/db/" + DXName);   // System.Diagnostics.Process.Start("http://www.microsoft.com");
                        }
                        catch
                        {
                            //     Debug.WriteLine("bad station");
                            // if not a URL then ignore
                        }
                    }

                } // not actively processing a new spot

            }

        } // textBox1_MouseUp



        //=========================================================================================
        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {

            this.TopMost = chkAlwaysOnTop.Checked;
        }


        //=========================================================================================
        private void button1_Click(object sender, EventArgs e)
        {
            if (pause)
            {
                pause = false;
                button1.Text = "Pause";
            }
            else
            {
                pause = true;
                button1.Text = "Paused";
            }

        }

        public static Image Skin1 = null; // temp holder for orignal skin image in picdisplay

        //=========================================================================================
        private void chkSUN_CheckedChanged(object sender, EventArgs e)
        {

            if (!chkSUN.Checked && !chkGrayLine.Checked)
            {
                if (Skin1 != null) console.picDisplay.BackgroundImage = Skin1; // put back original image
            }
            if (SP_Active != 0)
            {

                if (chkSUN.Checked || chkGrayLine.Checked)
                {

                    if (Skin1 == null) Skin1 = console.picDisplay.BackgroundImage;
                    console.picDisplay.SizeMode = PictureBoxSizeMode.StretchImage;

                    if (MAP == null) console.picDisplay.BackgroundImage = Image.FromStream(Map_image);
                    else console.picDisplay.BackgroundImage = MAP;

                } // SUN or GRAY LINE checked

            } // dx spot on

            Map_Last = 1;

        } // chkSUN_CheckedChanged



        //=========================================================================================
        private void chkGrayLine_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkSUN.Checked && !chkGrayLine.Checked)
            {
                if (Skin1 != null) console.picDisplay.BackgroundImage = Skin1; // put back original image
            }

            if (SP_Active != 0)
            {

                if (chkSUN.Checked || chkGrayLine.Checked)
                {
                    if (Skin1 == null) Skin1 = console.picDisplay.BackgroundImage;
                    console.picDisplay.SizeMode = PictureBoxSizeMode.StretchImage;

                    if (MAP == null) console.picDisplay.BackgroundImage = Image.FromStream(Map_image);
                    else console.picDisplay.BackgroundImage = MAP;

                } // only do if SUN or GRAY LINE checked

            } // dx spot active

            Map_Last = 1;

        } // chkGrayLine_CheckedChanged





        private static DisplayMode LastDisplayMode = 0;

        //===============================================================================================================================
        //===============================================================================================================================
        //===============================================================================================================================
        //===============================================================================================================================
        // turn on/off tracking (sun and/or grayline)
        private void btnTrack_Click(object sender, EventArgs e)
        {

            if (SP5_Active == 0)  // if OFF then turn ON
            {

                if (chkPanMode.Checked) Display.map = 1;
                else Display.map = 0;

                btnTrack.Text = "Track ON";

                LastDisplayMode = Display.CurrentDisplayMode; // save the display mode that you were in before you turned on special panafall mode

                if (chkPanMode.Checked) Display.CurrentDisplayMode = DisplayMode.PANAFALL;


                // Display.GridOff = 1; // Force Gridlines off but dont change setupform setting
                Display.GridControl = false;
                if ((chkSUN.Checked == true) || (chkGrayLine.Checked == true))
                {

                    if (Skin1 == null) Skin1 = console.picDisplay.BackgroundImage;
                    console.picDisplay.SizeMode = PictureBoxSizeMode.StretchImage;

                    if (MAP == null) console.picDisplay.BackgroundImage = Image.FromStream(Map_image);
                    else console.picDisplay.BackgroundImage = MAP;

                }


                Thread t = new Thread(new ThreadStart(TrackSun));

                t.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                t.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");


                SP5_Active = 1;
                t.Name = "Track Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                t.Start();

                textBox1.Text = "Clicked to Turn on GrayLine Sun Tracker\r\n";

            }
            else
            {

                SP5_Active = 0;                     // turn off tracking

                Display.map = 0;                    // tell display program to got back to standard panafall mode

                if (chkPanMode.Checked) Display.CurrentDisplayMode = LastDisplayMode;

                // if (console.SetupForm.chkGridControl.Checked == true) Display.GridOff = 1; // put gridlines back the way they were
                // else Display.GridOff = 0; // gridlines ON
                Display.GridControl = console.SetupForm.chkGridControl.Checked;
                btnTrack.Text = "Track";

                textBox1.Text += "Click to turn off GrayLine Sun Tracking\r\n";

                if (Skin1 != null) console.picDisplay.BackgroundImage = Skin1; // put back original image

            } // turn Tracking  off


        } //  btnTrack_Click





        public static int UTCLAST2 = 0;                                        // for Sun Tracker

        public static double UTC100A = 0;                                       // UTC time in hours.100th of min for grayline

        public static double UTC100 = 0;                                       // UTC time as a factor of 1 (1=2400 UTC, 0= 0000UTC) 
        public static double UTCDAY = 0;                                       // UTC day as a factor of 1 (0=1day, 1=365)
        public static int[] Gray_X = new int[400];                             // for mapping the gray line x and y 
        public static int[] Gray_Y = new int[400];

        public static int Sun_Right = 0;
        public static int Sun_Left = 0;

        public static int[,] GrayLine_Pos = new int[1000, 3];                      // [0,]=is lat 180 to 0 to -180 (top to bottom)
        public static int[,] GrayLine_Pos1 = new int[1000, 3];                      // [0,]=is lat 180 to 0 to -180 (top to bottom)
        public static int[] GrayLine_Pos2 = new int[1000];
        public static int[] GrayLine_Pos3 = new int[1000];

        public static int zz = 0; // determine the y pixel for this latitude grayline


        public static int Sun_X = 0;  // position of SUN in picDisplay window (based on ke9ns world map skin)
        public static int Sun_Y = 0;  // position of SUN in picDisplay window (based on ke9ns world map skin) 
        public static int Sun_Top1 = 0;  // position of SUN in picDisplay window (based on ke9ns world map skin)
        public static int Sun_Bot1 = 0;  // position of SUN in picDisplay window (based on ke9ns world map skin) 

        public static bool SUN = false; // true = on
        public static bool GRAYLINE = false; // true = on

        public static int suncounter = 0; // for space weather
        public static int SFI = 0;       // for Space weather
        public static int SN = 0;        // for Space weather
        public static int Aindex = 0;    // for Space weather
        public static int Kindex = 0;    // for Space weather
        public static string RadioBlackout = " ";
        public static string GeoBlackout = " ";
        private string serverPath;       // for Space weather



        //====================================================================================================
        //====================================================================================================
        // ke9ns add compute angle to sun (for dusk grayline) // equations in this section below provided by Roland Leigh
        private int SUNANGLE(double lat, double lon)  // ke9ns Thread opeation (runs in en-us culture) opens internet connection to genearte list of dx spots
        {

            double N = 0.017214206321 * DateTime.UtcNow.DayOfYear; // 2 * PI / 365 * Day

            double latitude = lat;
            double longitude = lon;
            latitude = latitude / 57.296;                         // lat * Math.PI / 180;  convert angle to rads

            double EQT = 0.000075 + (0.001868 * Math.Cos(N)) - (0.032077 * Math.Sin(N)) - (0.014615 * Math.Cos(2 * N)) - (0.040849 * Math.Sin(2 * N));

            double th = Math.PI * ((UTC100A / 12.0) - 1.0 + (longitude / 180.0)) + EQT;

            double delta = 0.006918 - (0.399912 * Math.Cos(N)) + (0.070257 * Math.Sin(N)) - (0.006758 * Math.Cos(2 * N)) + (0.000907 * Math.Sin(2 * N)) - (0.002697 * Math.Cos(3 * N)) + (0.00148 * Math.Sin(3 * N));

            double cossza = (Math.Sin(delta) * Math.Sin(latitude)) + (Math.Cos(delta) * Math.Cos(latitude) * Math.Cos(th));

            double SZA = (Math.Atan(-cossza / Math.Sqrt(-cossza * cossza + 1.0)) + 2.0 * Math.Atan(1)) * 57.296;  // ' the 57.296 converts this SZA to degrees

            return (int)Math.Abs(SZA); // 90=horizon, 108=total darkness, 100=dusk


        } // sunangle



        public static Color GrayLine_Last = Color.FromArgb(70, Color.Black);                       // used to check if setup.cs changed the color
        public static Band RX1Band_Last = 0;                                                      // to track a change in RX1 band

        private static Font font1 = new Font("Ariel", 10.5f, FontStyle.Regular, GraphicsUnit.Pixel);  // ke9ns add dx spot call sign font style
        private static Font font2 = new Font("Ariel", 9.0f, FontStyle.Regular, GraphicsUnit.Pixel);  // ke9ns add dx spot call sign font style

        private static Color grid_text_color = Color.Yellow;
        SolidBrush grid_text_brush = new SolidBrush(grid_text_color);
        SolidBrush redbrush = new SolidBrush(Color.Red);

        public static Image MAP = null; // holds bitmap image for SUN and GRAY LINE

        private static int[] spots = new int[100];  // holder for all the spots current on your entire band.

        public static int VFOLOW = 0;   // set in console rx1band for use in the mapper
        public static int VFOHIGH = 0;



        //===============================================================================================================================
        //===============================================================================================================================
        //===============================================================================================================================
        //===============================================================================================================================
        private void TrackSun()  // ke9ns Thread opeation (runs in en-us culture) To create and draw Sun and/or Grayline Track
        {


            //-------------------------------------------------------------------
            // ke9ns grayline check 
            // horizontal lines top to bottom (North)90 to 0 to (-SOUTH)90
            // vertical lines left to right  -West(180) to 0 to +East(180)
            // Solstice June and Dec  (reach northern or southern extreme (about +/-24deg)
            // Equinox: March and Sept  (pass the equator)
            //
            // Sunset:                SUNANGLE = 90    (horizon = 90deg)
            // Civil Twilight:        SUNANGLE = 91 to 95 
            // Civil Dusk:            SUNANGLE = 96 
            // Nautical Twilight:     SUNANGLE = 97 to 101
            // Nautical Dusk:         SUNANGLE = 102
            // Astronomical Twilight: SUNANGLE = 103 to 107
            // Astronomical Dusk:     SUNANGLE = 108
            // Night:                 SUNANGLE > 108

            // ftp://ftp.swpc.noaa.gov/pub/latest/wwv.txt

            /*
            :Product: Geophysical Alert Message wwv.txt
            : Issued: 2016 Mar 31 2110 UTC
            # Prepared by the US Dept. of Commerce, NOAA, Space Weather Prediction Center
            #
            #          Geophysical Alert Message
            #
            Solar - terrestrial indices for 31 March follow.
              Solar flux 82 and estimated planetary A - index 7.
              The estimated planetary K - index at 2100 UTC on 31 March was 0.

              No space weather storms were observed for the past 24 hours.

              No space weather storms are predicted for the next 24 hours

         */

            console.SetupForm.clrbtnGrayLine_Changed(this, EventArgs.Empty);               // get color from setup at startup

            textBox1.Text += "Attempt login to:  NOAA Space Weather Prediction Center \r\n";

            if (Console.noaaON == 0)
            {
                NOAA(); // get noaa space data
            }

            //   textBox1.Text += "NOAA Download complete \r\n";


            //--------------------------------------------------------------------------------------------
            // stay in this thread loop until you turn off tracking
            for (; SP5_Active == 1; )
            {

                Thread.Sleep(50);

                if (SP5_Active == 0) continue;

                if (((chkSUN.Checked == true) || (chkGrayLine.Checked == true)) &&
                                ((Display.CurrentDisplayMode == DisplayMode.PANADAPTER) || (Display.CurrentDisplayMode == DisplayMode.PANAFALL)))
                {

                    UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                    FD = UTCD.ToString("HHmm");                                       // get 24hr 4 digit UTC NOW
                    UTCNEW = Convert.ToInt16(FD);                                    // convert 24hr UTC to int


                    if (chkSUN.Checked == true) SUN = true; // activate display
                    else SUN = false;

                    if (chkGrayLine.Checked == true) GRAYLINE = true; // activate display
                    else GRAYLINE = false;


                    // Do a SUN, GRAYLINE, or DX country/callsign update

                    // Check for TIME CHANGE
                    // Check for GrayLine COLOR change
                    // Check for DX spot list change
                    // Check in any checkboxes changed state
                    // Check if the number of spots on map changed (DXK is the # of spots on the current panadapter)
                    // Check for Transmitting (dont update if transmitting)

                    if ((!console.MOX) && ((UTCNEW != UTCLAST2) || (Setup.DisplayGrayLineColor != GrayLine_Last) || (Map_Last > 0) ||
                        (((DX_Index != DX_Last) || (Console.DXK != DXK_Last) || (console.RX1Band != RX1Band_Last)) && (SP8_Active == 1))))
                    {


                        Debug.WriteLine("Update DX Spots on Screen=================");

                        GrayLine_Last = Setup.DisplayGrayLineColor; // store last color for compare next time

                        DXK_Last = Console.DXK;

                        DX_Last = DX_Index;                    // if the DX spot list changed

                        RX1Band_Last = console.RX1Band;

                        if ((UTCNEW != UTCLAST2) || (Setup.DisplayGrayLineColor != GrayLine_Last) || (Map_Last == 1))
                        {
                            Debug.WriteLine("Update GrayLine=================");

                            UTCLAST2 = UTCNEW;                            // store time for compare next time

                            //=================================================================================================
                            //=================================================================================================
                            // ke9ns Position of SUN (viewed from SUN) using the ke9ns world skin 
                            // X  left edge starts 5.6% in, right edge ends 94.7%  (with Display.DIS_X at 100%)
                            // y  top edge starts 7.8% in, bottom edge ends 90.1%  (with Display.DIS_Y at 100%)
                            // equirectangle project map has longitude lines every 15deg (1 per hour) center is 0 UTC and sun moves to the left
                            // left edge is 2359 UTC (right edge is 0 UTC)

                            //  Debug.WriteLine("Mouse x " + Console.DX_X); // mouse on picDisplay coordinates
                            //  Debug.WriteLine("mouse y " + Console.DX_Y);

                            //   Debug.WriteLine("x pos " + Display.DIS_X); // 
                            //  Debug.WriteLine("y pos " + Display.DIS_Y);

                            double g1 = (double)(360 / 365.25 * (DateTime.UtcNow.DayOfYear + (DateTime.UtcNow.Hour / 24))); // convert days to angle

                            g1 = g1 * Math.PI / 180;                             // convert angle to rads

                            double D = (double)(0.396372 - (22.91327 * Math.Cos(g1)) + (4.02543 * Math.Sin(g1)) - (0.387205 * Math.Cos(2 * g1))
                                + (0.051967 * Math.Sin(2 * g1)) - (0.154527 * Math.Cos(3 * g1)) + (0.084798 * Math.Sin(3 * g1)));

                            D = D / 24;                                        // convert to percent of 100

                            Sun_Top1 = 26;                                     // 45 Y pixel location of top of map
                            Sun_Bot1 = 465;                                    // 485 Y pixel locaiton of bottom of map 

                            int Sun_WidthY1 = Sun_Bot1 - Sun_Top1;             // # of Y pixels from top to bottom of map


                            int Sun_Top = 187;                                 // 207 y pixel location North Hem summer solstice
                            int Sun_Bot = 303;                                 // 324 Y pixel location of North Hem winter solstice

                            int Sun_WidthY = Sun_Bot - Sun_Top;                // # of Y pixes width between solstices
                            int Sun_WidthHalf = (Sun_WidthY / 2) + Sun_Top;    // y= 265 put you at equator

                            int Sun_Diff = Sun_WidthHalf - Sun_Top;            // 

                            Sun_Y = (int)(Sun_WidthHalf - (double)(Sun_Diff * D)); // position of SUN on longitude of map (based of time of year)

                            UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                            FD = UTCD.ToString("HH");
                            UTC100 = Convert.ToInt16(FD);
                            FD = UTCD.ToString("mm");

                            UTC100A = (UTC100 + (Convert.ToInt16(FD) / 60F));  // used by SUNANGLE routine

                            UTC100 = (UTC100 + (Convert.ToInt16(FD) / 60F)) / 24F; // used by SUN track routine convert to 100% 2400 = 100%

                            Sun_Left = 57;                                       // Left side at equator used by Grayline routine
                            Sun_Right = 939;

                            int Sun_Width = Sun_Right - Sun_Left; //used by sun track routine

                            Sun_X = (int)(Sun_Left + (float)(Sun_Width * (1.0 - UTC100))); // position of SUN on equator based on time of day

                            if ((GRAYLINE == true))
                            {

                                int qq = 0;       // index for accumulating lat edges
                                int ww = 0;       // 0=no edges found, 1=1 edge found, 2=2 edges found

                                int tt = 0;
                                bool check_for_light = true; // true = in the dark, so looking for light, false = in the light, so looking for the dark
                                int tempsun_ang = 0; // temp holder for sun angle


                                //----------------------------------------------------------------------------------
                                //----------------------------------------------------------------------------------
                                // check for Dark (Right side of map)
                                //----------------------------------------------------------------------------------
                                //----------------------------------------------------------------------------------

                                for (double lat = 90.0; lat >= -90.0; lat = lat - 0.5)  // horizontal lines top to bottom (North)90 to 0 to -90 (South)
                                {

                                    if ((SUNANGLE(lat, -180.0) >= 96) && (SUNANGLE(lat, 180.0) >= 96)) tt = 1; // dark on edges of screen 
                                    else tt = 0; // light on at least 1 side

                                    zz = (int)((qq / 360.0 * Sun_WidthY1) + Sun_Top1); // 360 = number of latitude points, determine the y pixel for this latitude grayline

                                    GrayLine_Pos[zz, 0] = GrayLine_Pos[zz, 1] = 0;

                                    if (SUNANGLE(lat, 0.0) < 96) check_for_light = false; // your in light so check for dark
                                    else check_for_light = true; // >= 96 your in dark so check for light


                                    for (double lon = 0.0; lon <= 180.0; lon = lon + 0.5)
                                    {
                                        tempsun_ang = SUNANGLE(lat, lon); // pos angle from 0 to 120


                                        if (check_for_light == true) // in dark, looking for light
                                        {

                                            if (tempsun_ang < 96) // found light
                                            {

                                                GrayLine_Pos[zz, ww] = (int)(((lon + 180.0) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                GrayLine_Pos[zz + 2, ww] = GrayLine_Pos[zz + 1, ww] = GrayLine_Pos[zz, ww]; // make sure to cover unused pixels

                                                ww++;
                                                if (ww == 2) break;   // both edges found so done

                                                lon = lon + 40.0; // 30 jump a little to save time
                                                check_for_light = false; // now in light so check for dark

                                            } // found light

                                        } // your in dark so check for light
                                        else // in light so check for dark
                                        {

                                            if (tempsun_ang >= 96) // in Dark (found it)
                                            {

                                                GrayLine_Pos[zz, ww] = (int)(((lon + 180.0) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                GrayLine_Pos[zz + 2, ww] = GrayLine_Pos[zz + 1, ww] = GrayLine_Pos[zz, ww]; // make sure to cover unused pixels

                                                ww++;
                                                if (ww == 2) break;   // both edges found so done

                                                lon = lon + 40.0;         // 30jump a little to save time
                                                check_for_light = true; // in dark so now check for light

                                            } // found dark


                                        }// in light so check for dark


                                    } // for lon (right side of map)

                                    //----------------------------------------------------------------------------------
                                    //----------------------------------------------------------------------------------
                                    // check for Dark (left side of map
                                    //----------------------------------------------------------------------------------
                                    //----------------------------------------------------------------------------------

                                    if (ww < 2) // still need at least 1 edge (maybe 2)
                                    {

                                        if (SUNANGLE(lat, 0.0) < 96) check_for_light = false; // your in light so check for dark
                                        else check_for_light = true; // >= 90 your in dark so check for light

                                        for (double lon = 0.0; lon >= -180.0; lon = lon - 0.5)  // vertical lines left to right 0 to -180 (west) (check left side of map)
                                        {
                                            tempsun_ang = SUNANGLE(lat, lon);

                                            if (check_for_light == true)
                                            {

                                                if (tempsun_ang < 96) // found light
                                                {

                                                    GrayLine_Pos[zz, ww] = (int)(((180.0 + lon) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                    GrayLine_Pos[zz + 2, ww] = GrayLine_Pos[zz + 1, ww] = GrayLine_Pos[zz, ww];

                                                    ww++;
                                                    if (ww == 2) break;      // if we have 2 edge then done

                                                    lon = lon - 40.0;           // jump a little to save time
                                                    check_for_light = false; // now in light so check for dark

                                                } // found light

                                            } // your in dark so check for light
                                            else // in light so check for dark
                                            {

                                                if (tempsun_ang >= 96) // in Dark (found it)
                                                {
                                                    GrayLine_Pos[zz, ww] = (int)(((180.0 + lon) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                    GrayLine_Pos[zz + 2, ww] = GrayLine_Pos[zz + 1, ww] = GrayLine_Pos[zz, ww];

                                                    ww++;
                                                    if (ww == 2) break;    // if we have 2 edge then done

                                                    lon = lon - 40;         // jump a little to save time
                                                    check_for_light = true; // in dark so now check for light

                                                } // found dark


                                            }// in light so check for dark


                                        } // for lon  (left side of map)


                                    } // ww as < 2 on the right side attempt


                                    if (ww == 0) // if still less than 2 edges then just zero out
                                    {
                                        GrayLine_Pos[zz + 2, 0] = GrayLine_Pos[zz + 1, 0] = GrayLine_Pos[zz, 0] = 0;
                                        GrayLine_Pos[zz + 2, 1] = GrayLine_Pos[zz + 1, 1] = GrayLine_Pos[zz, 1] = 0;
                                    }
                                    else if (ww == 1)
                                    {
                                        GrayLine_Pos[zz + 2, 0] = GrayLine_Pos[zz + 1, 0] = GrayLine_Pos[zz, 0] = GrayLine_Pos[zz, 0] + GrayLine_Pos[zz, 1];
                                        GrayLine_Pos[zz + 2, 1] = GrayLine_Pos[zz + 1, 1] = GrayLine_Pos[zz, 1] = GrayLine_Pos[zz, 0];
                                    }

                                    ww = 0; // start over for next lat

                                    if (tt == 1) // if dark on both edges then figure out which is which and signal display
                                    {

                                        if ((GrayLine_Pos[zz, 0] - GrayLine_Pos[zz, 1]) > 0)
                                        {
                                            GrayLine_Pos2[zz + 2] = GrayLine_Pos2[zz + 1] = GrayLine_Pos2[zz] = 1; // ,0 is on right side, ,1 is on left side
                                        }
                                        else if ((GrayLine_Pos[zz, 1] - GrayLine_Pos[zz, 0]) > 0)
                                        {
                                            GrayLine_Pos2[zz + 2] = GrayLine_Pos2[zz + 1] = GrayLine_Pos2[zz] = 2; // ,0 is on left side, ,1 is on right side
                                        }
                                        else
                                        {
                                            GrayLine_Pos2[zz + 2] = GrayLine_Pos2[zz + 1] = GrayLine_Pos2[zz] = 0;             // dark in center of map, (standard)
                                        }

                                    }
                                    else
                                    {
                                        GrayLine_Pos2[zz + 2] = GrayLine_Pos2[zz + 1] = GrayLine_Pos2[zz] = 0;             // dark in center of map, (standard)
                                    }

                                    qq++; // get next lat

                                } //  for (int lat = 90;lat >= -90;lat--)   horizontal lines top to bottom (North)90 to 0 to -90 (South)



                                //-------------------------------------------------------------------
                                //-------------------------------------------------------------------
                                // check for dusk (right side first)
                                //-------------------------------------------------------------------
                                //-------------------------------------------------------------------
                                qq = 0;
                                ww = 0;

                                for (double lat = 90.0; lat >= -90.0; lat = lat - 0.5)  // horizontal lines top to bottom (North)90 to 0 to -90 (South)
                                {


                                    if ((SUNANGLE(lat, -180.0) >= 90) && (SUNANGLE(lat, 180.0) >= 90)) tt = 1; // dark on edges of screen 
                                    else tt = 0; // light on at least 1 side

                                    zz = (int)((qq / 360.0 * Sun_WidthY1) + Sun_Top1); // 360 = number of latitude points, determine the y pixel for this latitude grayline

                                    GrayLine_Pos1[zz, 0] = GrayLine_Pos1[zz, 1] = 0;

                                    if (SUNANGLE(lat, 0.0) < 90) check_for_light = false; // your in light so check for dark
                                    else check_for_light = true; // >= 96 your in dark so check for light


                                    for (double lon = 0.0; lon <= 180.0; lon = lon + 0.5)
                                    {
                                        tempsun_ang = SUNANGLE(lat, lon); // pos angle from 0 to 120


                                        if (check_for_light == true) // in dark, looking for light
                                        {

                                            if (tempsun_ang < 90) // found light
                                            {

                                                GrayLine_Pos1[zz, ww] = (int)(((lon + 180.0) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                GrayLine_Pos1[zz + 2, ww] = GrayLine_Pos1[zz + 1, ww] = GrayLine_Pos1[zz, ww]; // make sure to cover unused pixels

                                                ww++;
                                                if (ww == 2) break;   // both edges found so done

                                                lon = lon + 40.0; // jump a little to save time
                                                check_for_light = false; // now in light so check for dark

                                            } // found light

                                        } // your in dark so check for light
                                        else // in light so check for dark
                                        {

                                            if (tempsun_ang >= 90) // in Dark (found it)
                                            {

                                                GrayLine_Pos1[zz, ww] = (int)(((lon + 180.0) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                GrayLine_Pos1[zz + 2, ww] = GrayLine_Pos1[zz + 1, ww] = GrayLine_Pos1[zz, ww]; // make sure to cover unused pixels

                                                ww++;
                                                if (ww == 2) break;   // both edges found so done

                                                lon = lon + 40.0;         // jump a little to save time
                                                check_for_light = true; // in dark so now check for light

                                            } // found dark


                                        }// in light so check for dark


                                    } // for lon (right side of map)

                                    //----------------------------------------------------------------------------------
                                    //----------------------------------------------------------------------------------
                                    // check for Dark (left side of map
                                    //----------------------------------------------------------------------------------
                                    //----------------------------------------------------------------------------------

                                    if (ww < 2) // still need at least 1 edge (maybe 2)
                                    {

                                        if (SUNANGLE(lat, 0.0) < 90) check_for_light = false; // your in light so check for dark
                                        else check_for_light = true; // >= 90 your in dark so check for light

                                        for (double lon = 0.0; lon >= -180.0; lon = lon - 0.5)  // vertical lines left to right 0 to -180 (west) (check left side of map)
                                        {
                                            tempsun_ang = SUNANGLE(lat, lon);

                                            if (check_for_light == true)
                                            {

                                                if (tempsun_ang < 90) // found light
                                                {

                                                    GrayLine_Pos1[zz, ww] = (int)(((180.0 + lon) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline
                                                    GrayLine_Pos1[zz + 2, ww] = GrayLine_Pos1[zz + 1, ww] = GrayLine_Pos1[zz, ww];

                                                    ww++;
                                                    if (ww == 2) break;      // if we have 2 edge then done

                                                    lon = lon - 40.0;           // jump a little to save time
                                                    check_for_light = false; // now in light so check for dark

                                                } // found light

                                            } // your in dark so check for light
                                            else // in light so check for dark
                                            {

                                                if (tempsun_ang >= 90) // in Dark (found it)
                                                {
                                                    GrayLine_Pos1[zz, ww] = (int)(((180.0 + lon) / 360.0 * Sun_Width) + Sun_Left); // determine x pixel for this longitude grayline

                                                    GrayLine_Pos1[zz + 2, ww] = GrayLine_Pos1[zz + 1, ww] = GrayLine_Pos1[zz, ww];

                                                    ww++;
                                                    if (ww == 2) break;    // if we have 2 edge then done

                                                    lon = lon - 40.0;         // jump a little to save time
                                                    check_for_light = true; // in dark so now check for light

                                                } // found dark


                                            }// in light so check for dark


                                        } // for lon  (left side of map)


                                    } // ww as < 2 on the right side attempt


                                    if (ww == 0) // if still less than 2 edges then just zero out
                                    {
                                        GrayLine_Pos1[zz + 2, 0] = GrayLine_Pos1[zz + 1, 0] = GrayLine_Pos1[zz, 0] = 0;
                                        GrayLine_Pos1[zz + 2, 1] = GrayLine_Pos1[zz + 1, 1] = GrayLine_Pos1[zz, 1] = 0;
                                    }
                                    else if (ww == 1)
                                    {
                                        GrayLine_Pos1[zz + 2, 0] = GrayLine_Pos1[zz + 1, 0] = GrayLine_Pos1[zz, 0] = GrayLine_Pos1[zz, 0] + GrayLine_Pos1[zz, 1];
                                        GrayLine_Pos1[zz + 2, 1] = GrayLine_Pos1[zz + 1, 1] = GrayLine_Pos1[zz, 1] = GrayLine_Pos1[zz, 0];
                                    }

                                    ww = 0; // start over for next lat

                                    if (tt == 1) // if dark on both edges then figure out which is which and signal display
                                    {

                                        if ((GrayLine_Pos1[zz, 0] - GrayLine_Pos1[zz, 1]) > 0)
                                        {
                                            GrayLine_Pos3[zz + 2] = GrayLine_Pos3[zz + 1] = GrayLine_Pos3[zz] = 1; // ,0 is on right side, ,1 is on left side
                                        }
                                        else if ((GrayLine_Pos1[zz, 1] - GrayLine_Pos1[zz, 0]) > 0)
                                        {
                                            GrayLine_Pos3[zz + 2] = GrayLine_Pos3[zz + 1] = GrayLine_Pos3[zz] = 2; // ,0 is on left side, ,1 is on right side
                                        }
                                        else
                                        {
                                            GrayLine_Pos3[zz + 2] = GrayLine_Pos3[zz + 1] = GrayLine_Pos3[zz] = 0;             // dark in center of map, (standard)
                                        }

                                    }
                                    else
                                    {
                                        GrayLine_Pos3[zz + 2] = GrayLine_Pos3[zz + 1] = GrayLine_Pos3[zz] = 0;             // dark in center of map, (standard)
                                    }

                                    qq++; // get next lat

                                } //  for (int lat = 90;lat >= -90;lat--)   horizontal lines top to bottom (North)90 to 0 to -90 (South)


                            } // GRAYLINE = true

                        } //  if ( (UTCNEW != UTCLAST2) || (Setup.DisplayGrayLineColor != GrayLine_Last) )  time changeed or color change


                        if ((Map_Last == 2) && (SP4_Active == 0)) updatemapspots(); // if you need a map update make sure your not in the middle of updating your dx spot list

                        Map_Last = 0;

                        if (SP5_Active == 0) continue;


                        //-------------------------------------------------------------------------------------------------
                        //-------------------------------------------------------------------------------------------------
                        // draw sun tracker and gray line
                        //-------------------------------------------------------------------------------------------------
                        //-------------------------------------------------------------------------------------------------

                        MAP = new Bitmap(Map_image); // load up Map image

                        Graphics g = Graphics.FromImage(MAP);

                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        //  g.SmoothingMode = SmoothingMode.AntiAlias;


                        if (SUN == true)
                        {

                            Image src = new Bitmap(sun_image); // load up SUN image ( use PNG to allow transparent background)

                            g.DrawImage(src, Sun_X - 10, Sun_Y - 10, 23, 27); // draw SUN 20 x 20 pixel

                            if (Console.noaaON == 0)
                            {
                                g.DrawString("SFI " + SFI.ToString("D"), font1, grid_text_brush, Sun_X + 15, Sun_Y - 10);
                                g.DrawString("A " + Aindex.ToString("D") + ", " + RadioBlackout, font1, grid_text_brush, Sun_X + 15, Sun_Y);

                                if (suncounter > 40) // check every 40 minutes
                                {
                                    NOAA();
                                    suncounter = 0;
                                    Debug.WriteLine("NOAA GET=============");

                                }
                                else
                                {
                                    suncounter++;
                                }
                            }
                            else
                            {

                                g.DrawString("SFI " + Console.SFI.ToString("D"), font1, grid_text_brush, Sun_X + 15, Sun_Y - 10);
                                g.DrawString("A " + Console.Aindex.ToString("D") + ", " + Console.RadioBlackout, font1, grid_text_brush, Sun_X + 15, Sun_Y);

                            }

                        } // sun tracker enabled


                        //---------------------------------------------------------------------------
                        if (GRAYLINE == true)
                        {

                            Pen p3 = new Pen(GrayLine_Last, 1.0f); // dark
                            Pen p4 = new Pen(GrayLine_Last, 1.0f); // dusk


                            for (int ee = Sun_Top1; ee < Sun_Bot1; ee++)
                            {

                                //-----------------------------------------------------------------
                                // ke9ns dusk
                                if (GrayLine_Pos3[ee] == 0) // not dusk on edges on screen
                                {
                                    g.DrawLine(p4, GrayLine_Pos1[ee, 0], ee, GrayLine_Pos1[ee, 1], ee);
                                }
                                else if (GrayLine_Pos3[ee] == 1)
                                {
                                    g.DrawLine(p4, GrayLine_Pos1[ee, 0], ee, Sun_Right, ee);
                                    g.DrawLine(p4, GrayLine_Pos1[ee, 1], ee, Sun_Left, ee);
                                }
                                else
                                {
                                    g.DrawLine(p4, GrayLine_Pos1[ee, 1], ee, Sun_Right, ee);
                                    g.DrawLine(p4, GrayLine_Pos1[ee, 0], ee, Sun_Left, ee);
                                }


                                //-----------------------------------------------------------------
                                // ke9ns dark
                                if (GrayLine_Pos2[ee] == 0)  // not dark on edges on screen
                                {
                                    g.DrawLine(p3, GrayLine_Pos[ee, 0], ee, GrayLine_Pos[ee, 1], ee);
                                }
                                else if (GrayLine_Pos2[ee] == 1)
                                {
                                    g.DrawLine(p3, GrayLine_Pos[ee, 0], ee, Sun_Right, ee);
                                    g.DrawLine(p3, GrayLine_Pos[ee, 1], ee, Sun_Left, ee);
                                }
                                else
                                {
                                    g.DrawLine(p3, GrayLine_Pos[ee, 1], ee, Sun_Right, ee);
                                    g.DrawLine(p3, GrayLine_Pos[ee, 0], ee, Sun_Left, ee);
                                }


                            }  // for loop  

                        } // if GRAYLINE enabled




                        if (SP8_Active == 1) // parse map display just by band, but red dots are for all bands
                        {
                            //-------------------------------------------------------------------------------------
                            //-------------------------------------------------------------------------------------
                            //-------------------------------------------------------------------------------------
                            //-------------------------------------------------------------------------------------
                            // draw country or call sign on map

                            string[] country = new string[200];
                            string[] call = new string[200];

                            int[] yy = new int[200];
                            int Flag11 = 0;

                            int kk = 0;
                            int rr = 0;

                            int zz = 0;

                            //  Debug.WriteLine("Band " + console.RX1Band);
                            //  Debug.WriteLine("BandLOW " + VFOLOW);
                            //  Debug.WriteLine("BandHIGH " + VFOHIGH);



                            for (int ii = 0; ii < DX_Index; ii++) // red dot always all bands
                            {

                                if ((DX_X[ii] != 0) && (DX_Y[ii] != 0))
                                {

                                    g.FillRectangle(redbrush, DX_X[ii], DX_Y[ii], 3, 3);  // place red dot on map (all bands)


                                    if (chkMapBand.Checked == true) // find band your on and its low and upper limits
                                    {

                                        if ((DX_Freq[ii] >= VFOLOW) && (DX_Freq[ii] <= VFOHIGH))
                                        {
                                            spots[zz++] = ii;                    // ii is the actual DX_INdex pos the the KK holds
                                            // in the display routine this is Display.holder[kk] = ii
                                        }

                                    } // band only


                                } // have a lat/long for the spot

                            } // for ii DX_Index  (full dx spot list)


                            if (chkBoxPan.Checked == true) // show spots on map for just your panadapter
                            {

                                for (int ii = 0; ii < Thetis.Console.DXK; ii++) // dx call sign or country name on map is just for the band your on
                                {

                                    if ((DX_X[Display.holder[ii]] != 0) && (DX_Y[Display.holder[ii]] != 0))  // dont even bother with the spot if X and Y = 0 since that means no data to plot
                                    {

                                        if (chkMapCountry.Checked == true) // spot country on map
                                        {
                                            g.DrawString(DX_country[Display.holder[ii]], font2, grid_text_brush, DX_X[Display.holder[ii]], DX_Y[Display.holder[ii]]); // use Pandapdater holder[] data

                                        } // chkMapCountry true = draw country name on map

                                        else if (chkMapCall.Checked == true)  // else show call signs on map
                                        {

                                            for (rr = 0; rr < kk; rr++)  // check all accumulated countrys from the current DX_index list
                                            {
                                                if (country[rr] == DX_country[Display.holder[ii]])  // use Pandapdater holder[] data
                                                {
                                                    yy[rr] = yy[rr] + 10; // multiple calls for same country stack downward
                                                    Flag11 = 1;
                                                    break;
                                                }


                                            } // for rr loop


                                            if (Flag11 == 0)
                                            {
                                                country[kk] = DX_country[Display.holder[ii]]; // add to list
                                                yy[kk] = 0;
                                            }

                                            kk++; // increment for next country

                                            Flag11 = 0; // reset flag
                                            g.DrawString(DX_Station[Display.holder[ii]], font2, grid_text_brush, DX_X[Display.holder[ii]], DX_Y[Display.holder[ii]] + yy[rr]); // Station  name

                                        } // chkMapCall true = draw all sign on map


                                    } //  if ((DX_X[ii] != 0) && (DX_Y[ii] != 0))


                                } // for ii index loop

                            } // chkboxpan

                            else if (chkMapBand.Checked == true) //  show spots on map for your entire band
                            {

                                for (int ii = 0; ii < zz; ii++) // dx call sign or country name on map is just for the band your on
                                {

                                    if ((DX_X[spots[ii]] != 0) && (DX_Y[spots[ii]] != 0))  // dont even bother with the spot if X and Y = 0 since that means no data to plot
                                    {

                                        if (chkMapCountry.Checked == true) // spot country on map
                                        {
                                            g.DrawString(DX_country[spots[ii]], font2, grid_text_brush, DX_X[spots[ii]], DX_Y[spots[ii]]); // use Pandapdater holder[] data

                                        } // chkMapCountry true = draw country name on map

                                        else if (chkMapCall.Checked == true)  // else show call signs on map
                                        {

                                            for (rr = 0; rr < kk; rr++)  // check all accumulated countrys from the current DX_index list
                                            {
                                                if (country[rr] == DX_country[spots[ii]])  // use Pandapdater holder[] data
                                                {
                                                    yy[rr] = yy[rr] + 10; // multiple calls for same country stack downward
                                                    Flag11 = 1;
                                                    break;
                                                }


                                            } // for rr loop


                                            if (Flag11 == 0)
                                            {
                                                country[kk] = DX_country[spots[ii]]; // add to list
                                                yy[kk] = 0;
                                            }

                                            kk++; // increment for next country

                                            Flag11 = 0; // reset flag

                                            g.DrawString(DX_Station[spots[ii]], font2, grid_text_brush, DX_X[spots[ii]], DX_Y[spots[ii]] + yy[rr]); // Station  name


                                        } // chkMapCall true = draw all sign on map


                                    } //  if ((DX_X[ii] != 0) && (DX_Y[ii] != 0))


                                } // for ii index loop
                            } // chkMapBand true = just show spots on map for the band you can see
                            else
                            {

                                for (int ii = 0; ii < DX_Index; ii++) // dx call sign or country name on map is just for the band your on
                                {

                                    if ((DX_X[ii] != 0) && (DX_Y[ii] != 0))
                                    {

                                        if (chkMapCountry.Checked == true) // spot country on map
                                        {

                                            g.DrawString(DX_country[ii], font2, grid_text_brush, DX_X[ii], DX_Y[ii]); // country name



                                        } // chkMapCountry true = draw country name on map

                                        else if (chkMapCall.Checked == true)  // show call signs on map
                                        {

                                            for (rr = 0; rr < kk; rr++)  // check all accumulated countrys from the current DX_index list
                                            {
                                                if (country[rr] == DX_country[ii])
                                                {
                                                    yy[rr] = yy[rr] + 10; // multiple calls for same country stack downward
                                                    Flag11 = 1;
                                                    break;
                                                }


                                            } // for rr loop


                                            if (Flag11 == 0)
                                            {
                                                country[kk] = DX_country[ii]; // add to list
                                                yy[kk] = 0;
                                            }

                                            kk++; // increment for next country

                                            Flag11 = 0; // reset flag

                                            g.DrawString(DX_Station[ii], font2, grid_text_brush, DX_X[ii], DX_Y[ii] + yy[rr]); // Station  name


                                        } // chkMapCall true = draw all sign on map


                                    } //  if ((DX_X[ii] != 0) && (DX_Y[ii] != 0))


                                } // for ii index loop

                            } // chkMapBand false = show all spots on map

                        } // SP8_Active = 1


                        //----------------------------------------------------------------------------------------------------
                        // update MAP background

                        console.picDisplay.SizeMode = PictureBoxSizeMode.StretchImage;           // put image back onto picDisplay background image
                        console.picDisplay.BackgroundImage = MAP;                                  // MAP.Save("test.bmp");  save modified map_image to actual file on hard drive





                    } // check every 1 minutes or unless spots change

                } // only check in in panadapter mode since you cant see it in any other mode
                else
                {
                    SUN = false;
                    GRAYLINE = false;
                }

            } // for loop (SP5_Active == 1)

        } // TrackSun







        public void chkPanMode_CheckedChanged(object sender, EventArgs e)
        {

            if (SP5_Active == 1) // only check if tracking is already on
            {
                if (chkPanMode.Checked)
                {

                    Display.map = 1;
                    Display.CurrentDisplayMode = DisplayMode.PANAFALL;


                }
                else
                {
                    Display.map = 0;
                    Display.CurrentDisplayMode = LastDisplayMode;
                }
            }
        } // chkPanMode_CheckedChanged



        //=========================================================================================


        private static int DXLOC_Index1 = 0;
        private static int SP8_Active = 0;    // 1=DX LOC scanned into memory

        // data obtained from DXLOC.txt file
        public static string[] DXLOC_prefix = new string[2000];       // prefix (must start with)
        public static string[] DXLOC_prefix1 = new string[2000];      // prefix (must also contain) /
        public static string[] DXLOC_prefix2 = new string[2000];      // prefix (must exclude) \

        public static string[] DXLOC_lat = new string[2000];          // text of lat
        public static string[] DXLOC_lon = new string[2000];          // text of  lon
        public static double[] DXLOC_LAT = new double[2000];          // latitude  
        public static double[] DXLOC_LON = new double[2000];          //  longitude
        public static string[] DXLOC_country = new string[2000];      // country
        public static string[] DXLOC_continent = new string[2000];    // continent


        //=======================================================================================
        //=======================================================================================
        //ke9ns Open and read DXLOC.txt file here (put into array of prefix vs lat/lon value)
        //   Fields:
        //
        //0	    DXCC number
        //1*	ARRL DXCC prefix,
        //2*    DXCC Entity name,
        //3*	Continent,
        //4*	Latitude,
        //5*	Longitude,
        //6	    CQ Zone,
        //7	    ITU Zone,
        //8	Active (A) or Deleted (D),
        //9	Date from becoming a valid Entity,
        //10	Possible prefixes from ITU Assigned Blocks to Sovereign UN Territory(s)
        public void DXLOC_FILE()
        {

            string file_name = console.AppDataPath + "DXLOC.txt"; // //  sked - b15.csv

            textBox1.Text += "Attempting to open DX Location list\r\n";

            if (File.Exists(file_name))
            {

                textBox1.Text += "Reading DX Location list\r\n";

                try
                {
                    stream2 = new FileStream(file_name, FileMode.Open); // open  file
                    reader2 = new BinaryReader(stream2, Encoding.ASCII);

                }
                catch (Exception)
                {
                    SP8_Active = 0;
                    Debug.WriteLine("NO DX LOC FILE============================");
                    return;


                }
                var result = new StringBuilder();

                if (SP8_Active == 0) // dont reset if already scanned in  database
                {
                    DXLOC_Index1 = 0; // how big is the DXLOC data file in lines

                }

                for (; ; ) // read file and extract data from it and close it and set sp8_active = 1 when done
                {

                    if (SP8_Active == 1) // aleady scanned database
                    {
                        break; // dont rescan database over 
                    }

                    if (SP_Active == 0)
                    {
                        reader2.Close();    // close  file
                        stream2.Close();   // close stream

                        return;
                    }



                    try
                    {
                        var newChar = (char)reader2.ReadChar();

                        if ((newChar == '\r'))
                        {
                            newChar = (char)reader2.ReadChar(); // read \n char to finishline

                            string[] values = result.ToString().Split(','); // split line up into segments divided by ,


                            //   Debug.Write(DXLOC_Index1.ToString());

                            DXLOC_prefix[DXLOC_Index1] = values[1].Substring(1, values[1].Length - 2);                       // call sign prefix
                            //    Debug.Write(" prefix>" + DXLOC_prefix[DXLOC_Index1]);


                            if (DXLOC_prefix[DXLOC_Index1].Contains("/")) // indicating an extra character the call sign must contain
                            {
                                DXLOC_prefix1[DXLOC_Index1] = DXLOC_prefix[DXLOC_Index1].Substring(DXLOC_prefix[DXLOC_Index1].Length - 1, 1);
                                DXLOC_prefix[DXLOC_Index1] = DXLOC_prefix[DXLOC_Index1].Substring(0, DXLOC_prefix[DXLOC_Index1].Length - 2);

                            }
                            else DXLOC_prefix1[DXLOC_Index1] = null;

                            if (DXLOC_prefix[DXLOC_Index1].Contains("\\")) // indicating an extra character the call sign must not contain
                            {
                                DXLOC_prefix2[DXLOC_Index1] = DXLOC_prefix[DXLOC_Index1].Substring(DXLOC_prefix[DXLOC_Index1].Length - 1, 1);
                                DXLOC_prefix[DXLOC_Index1] = DXLOC_prefix[DXLOC_Index1].Substring(0, DXLOC_prefix[DXLOC_Index1].Length - 2);


                            }
                            else DXLOC_prefix2[DXLOC_Index1] = null;

                            //    Debug.Write(" prefix>" + DXLOC_prefix[DXLOC_Index1]);

                            //    Debug.Write(" pre/ " + DXLOC_prefix1[DXLOC_Index1]);
                            //   Debug.Write(" pre\\ " + DXLOC_prefix2[DXLOC_Index1]);



                            DXLOC_country[DXLOC_Index1] = values[2].Substring(1, values[2].Length - 2);                       // call sign country
                            // Debug.Write(" country>" + DXLOC_country[DXLOC_Index1]);


                            DXLOC_continent[DXLOC_Index1] = values[3].Substring(1, values[3].Length - 2);                     // call sign continent
                            //  Debug.Write(" continent>" + DXLOC_continent[DXLOC_Index1]);

                            DXLOC_lat[DXLOC_Index1] = values[4];                          // call sign lat
                            //  Debug.Write(" lat>" + DXLOC_lat[DXLOC_Index1]);

                            DXLOC_lon[DXLOC_Index1] = values[5];                          // call sign lon
                            //  Debug.Write(" lon>" + DXLOC_lon[DXLOC_Index1]);

                            // horizontal lines top to bottom (North)90 to 0 to (-SOUTH)90
                            // vertical lines left to right  -West(180) to 0 to +East(180)


                            if (DXLOC_lat[DXLOC_Index1].Contains("N")) // pos 90 North
                            {
                                int ff = DXLOC_lat[DXLOC_Index1].IndexOf('N') - 1;

                                try
                                {
                                    DXLOC_LAT[DXLOC_Index1] = Convert.ToDouble(DXLOC_lat[DXLOC_Index1].Substring(1, ff));
                                    //     Debug.Write(" LAT>" + DXLOC_LAT[DXLOC_Index1]);

                                }
                                catch (Exception)
                                {

                                    Debug.WriteLine(" NORTH " + DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    DXLOC_LAT[DXLOC_Index1] = 0;


                                }

                            }  // pos 90 North
                            else if (DXLOC_lat[DXLOC_Index1].Contains("S")) // neg 90 North
                            {

                                int ff = DXLOC_lat[DXLOC_Index1].IndexOf('S') - 1;

                                try
                                {
                                    DXLOC_LAT[DXLOC_Index1] = -Convert.ToDouble(DXLOC_lat[DXLOC_Index1].Substring(1, ff));
                                    //    Debug.Write(" LAT>" + DXLOC_LAT[DXLOC_Index1]);

                                }
                                catch (Exception)
                                {
                                    DXLOC_LAT[DXLOC_Index1] = 0;
                                    Debug.WriteLine(" SOUTH " + DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    Debug.Write(" prefix>" + DXLOC_prefix[DXLOC_Index1]);

                                }

                            }  // neg 90 North


                            if (DXLOC_lon[DXLOC_Index1].Contains("W")) // neg 180 west
                            {
                                int ff = DXLOC_lon[DXLOC_Index1].IndexOf('W') - 1;

                                try
                                {

                                    DXLOC_LON[DXLOC_Index1] = -Convert.ToDouble(DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    //  Debug.WriteLine(" LON>" + DXLOC_LON[DXLOC_Index1]);

                                }
                                catch (Exception)
                                {
                                    Debug.WriteLine(" WEST " + DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    DXLOC_LON[DXLOC_Index1] = 0;
                                }

                            }  // neg 180 West
                            else if (DXLOC_lon[DXLOC_Index1].Contains("E")) // pos 180 East
                            {
                                int ff = DXLOC_lon[DXLOC_Index1].IndexOf('E') - 1;

                                try
                                {

                                    DXLOC_LON[DXLOC_Index1] = Convert.ToDouble(DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    //  Debug.WriteLine(" LON>" + DXLOC_LON[DXLOC_Index1]);

                                }
                                catch (Exception)
                                {
                                    Debug.WriteLine(" EAST " + DXLOC_lon[DXLOC_Index1].Substring(1, ff));
                                    DXLOC_LON[DXLOC_Index1] = 0;
                                }

                            }  // pos 180 East



                            DXLOC_Index1++;

                            if (DXLOC_Index1 > 2000) break;

                            result = new StringBuilder(); // clean up for next line

                        } // \r
                        else
                        {
                            result.Append(newChar);  // save char
                        }

                    }
                    catch (EndOfStreamException)
                    {
                        DXLOC_Index1--;

                        if (DXLOC_Index1 < 10) textBox1.Text += "No DXLOC.txt list file found in database folder\r\n";
                        else textBox1.Text += "End of DX LOC FILE at " + DXLOC_Index1.ToString() + "\r\n";

                        break; // done with file
                    }
                    catch (Exception)
                    {
                        //  Debug.WriteLine("excpt======== " + e);
                        //     textBox1.Text = e.ToString();

                        if (DXLOC_Index1 < 10) textBox1.Text += "No DXLOC.txt list file found in database folder\r\n";

                        break; // done with file
                    }


                } // for loop until end of file is reached


                //   Debug.WriteLine("reached DXLOC end of file" + DXLOC_Index1.ToString());
                textBox1.Text += "Reached End of DXLOC.txt FILE with # " + DXLOC_Index1.ToString() + "\r\n";


                reader2.Close();    // close  file
                stream2.Close();   // close stream


                SP8_Active = 1; // done loading DXLOC database (Good)



            } // if file exists
            else
            {
                SP8_Active = 0;
                Debug.WriteLine("NO DX LOC FILE============================");
            }



        } // DXMAP()

        private void chkMapCountry_CheckedChanged(object sender, EventArgs e)
        {

            if (chkMapCountry.Checked == true) chkMapCall.Checked = false;
            Map_Last = 1;

        }

        private void chkMapCall_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMapCall.Checked == true) chkMapCountry.Checked = false;
            Map_Last = 1;

        }

        private void chkMapBand_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMapBand.Checked == true) chkBoxPan.Checked = false;
            Map_Last = 1;

        }

        private void chkBoxPan_CheckedChanged(object sender, EventArgs e)
        {

            if (chkBoxPan.Checked == true) chkMapBand.Checked = false;
            Map_Last = 1;
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {

            textBox1.ShortcutsEnabled = false; // added to eliminate the contextmenu from popping up

        }





        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (console.DXMemList.List.Count == 0) return; // nothing in the list, exit

            int index = dataGridView1.CurrentCell.RowIndex;


            if ((index < 0) || (index > (console.DXMemList.List.Count - 1))) return;// index out of range

            //   DXMemRecord recordToRestore = new DXMemRecord((DXMemRecord)DXURL.SelectedItem);

            Debug.WriteLine("Double CLick=" + index);
        }


        public static int RIndex1 = 0;

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            RIndex1 = e.RowIndex; // last row you clicked on 

        }

        public static bool SWL2 = false;

        private void chkBoxSWL2_CheckedChanged(object sender, EventArgs e)
        {
            SP3_Active = 0;
            SP1_Active = 0;

            if (chkBoxSWL2.Checked == true) SWL2 = true;
            else SWL2 = false;
        }


        public static byte SP6_Active = 0; // 1= turn on MEMORY in panadapter

        private void chkBoxMem_CheckedChanged(object sender, EventArgs e)
        {

            if (chkBoxMem.Checked == true)
            {

                dataGridView2.DataSource = console.MemoryList.List;   // ke9ns get list of memories from memorylist.cs is where the file is opened and saved

                SP6_Active = 1;

                //  comboFMMemory.DataSource = MemoryList.List;
                //  comboFMMemory.DisplayMember = "Name";
                //  comboFMMemory.ValueMember = "Name";


                //  Debug.WriteLine("comboFM " + (string)dataGridView1["Name", dataGridView1.CurrentCell.RowIndex].Value);

                //  Debug.WriteLine("comboFM " + (string)dataGridView2[2, 0].Value);
                //  Debug.WriteLine("comboFM1 " + dataGridView2[1, 3].ToString());
                Debug.WriteLine("Rows Count " + dataGridView2.Rows.Count);

            }
            else
            {
                SP6_Active = 0;
            }

            //  MemoryList X = console.MemoryList.List;
            //   MemoryRecord recordToRestore = new MemoryRecord((MemoryRecord)comboFMMemory.SelectedItem);

            //  console.RecallMemory(recordToRestore);

        } //chkboxmem_checked

        //========================================================================
        //========================================================================

        public void NOAA()
        {

            serverPath = "ftp://ftp.swpc.noaa.gov/pub/latest/wwv.txt";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverPath);

            //  textBox1.Text += "Attempt to download Space Weather \r\n";

            request.KeepAlive = true;
            request.UsePassive = true;
            request.UseBinary = true;

            request.Method = WebRequestMethods.Ftp.DownloadFile;
            string username = "anonymous";
            string password = "guest";
            request.Credentials = new NetworkCredential(username, password);

            string noaa = null;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                noaa = reader.ReadToEnd();

                reader.Close();
                response.Close();
                //   Debug.WriteLine("noaa=== " + noaa);

                //   textBox1.Text += "NOAA Download complete \r\n";



                //--------------------------------------------------------------------
                if (noaa.Contains("Solar flux ")) // 
                {

                    int ind = noaa.IndexOf("Solar flux ") + 11;

                    try
                    {
                        SFI = (int)(Convert.ToDouble(noaa.Substring(ind, 3)));
                        Debug.WriteLine("SFI " + SFI);
                    }
                    catch (Exception)
                    {
                        SFI = 0;
                    }


                } // SFI

                if (noaa.Contains("A-index ")) // 
                {

                    int ind = noaa.IndexOf("A-index ") + 8;

                    try
                    {
                        Aindex = (int)(Convert.ToDouble(noaa.Substring(ind, 2)));
                        Debug.WriteLine("Aindex " + Aindex);
                    }
                    catch (Exception)
                    {
                        Aindex = 0;
                    }


                } // Aindex

                if (noaa.Contains("Radio blackouts reaching the ")) // 
                {

                    int ind = noaa.IndexOf("Radio blackouts reaching the ") + 29;

                    try
                    {
                        RadioBlackout = noaa.Substring(ind, 2);
                        Debug.WriteLine("Radio Blackout " + RadioBlackout);
                    }
                    catch (Exception)
                    {
                        RadioBlackout = " ";
                    }


                } // radio blackouts

                if (!noaa.Contains("No space weather storms ") && noaa.Contains("Geomagnetic storms reaching the ")) // 
                {

                    int ind = noaa.IndexOf("Geomagnetic storms reaching the ") + 32;

                    try
                    {
                        GeoBlackout = noaa.Substring(ind, 2);
                        Debug.WriteLine("Geomagnetic storms" + GeoBlackout);
                    }
                    catch (Exception)
                    {
                        GeoBlackout = " ";
                    }


                } // radio blackouts

                if (RadioBlackout != " ")
                {
                    RadioBlackout = RadioBlackout + GeoBlackout;
                    Debug.WriteLine("radio-geo " + RadioBlackout);


                }
                else
                {
                    RadioBlackout = GeoBlackout;
                    Debug.WriteLine("geo " + RadioBlackout);
                }

            }
            catch (Exception ex)
            {
                //   Debug.WriteLine("noaa fault=== " + ex);
                textBox1.Text += "Failed to download Space Weather \r\n";

            }


        } // NOAA


    } // Spotcontrol


} // powersdr

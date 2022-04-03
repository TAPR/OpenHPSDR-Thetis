
namespace Thetis
{
    partial class frmFilterManager
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node0");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("System Filters", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Node1");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("User Filters", new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.treeFilters = new System.Windows.Forms.TreeView();
            this.btnMore = new System.Windows.Forms.Button();
            this.labelTS12 = new System.Windows.Forms.LabelTS();
            this.labelTS11 = new System.Windows.Forms.LabelTS();
            this.labelTS10 = new System.Windows.Forms.LabelTS();
            this.groupBoxTS5 = new System.Windows.Forms.GroupBoxTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.labelTS8 = new System.Windows.Forms.LabelTS();
            this.labelTS7 = new System.Windows.Forms.LabelTS();
            this.textBoxTS8 = new System.Windows.Forms.TextBoxTS();
            this.textBoxTS7 = new System.Windows.Forms.TextBoxTS();
            this.textBoxTS6 = new System.Windows.Forms.TextBoxTS();
            this.bandStackListBox1 = new Thetis.BandStackListBox();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.btnUpdate = new System.Windows.Forms.ButtonTS();
            this.btnAddNew = new System.Windows.Forms.ButtonTS();
            this.groupBoxTS4 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS8 = new System.Windows.Forms.ButtonTS();
            this.buttonTS9 = new System.Windows.Forms.ButtonTS();
            this.lvlSelectedSubModes = new System.Windows.Forms.ListBox();
            this.lstAvailableSubModes = new System.Windows.Forms.ListBox();
            this.chkFilterOnSubModes = new System.Windows.Forms.CheckBoxTS();
            this.groupBoxTS3 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS6 = new System.Windows.Forms.ButtonTS();
            this.buttonTS7 = new System.Windows.Forms.ButtonTS();
            this.lstSelectedModes = new System.Windows.Forms.ListBox();
            this.lstAvailableModes = new System.Windows.Forms.ListBox();
            this.chkFilterOnModes = new System.Windows.Forms.CheckBoxTS();
            this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS5 = new System.Windows.Forms.ButtonTS();
            this.buttonTS4 = new System.Windows.Forms.ButtonTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.txtToFreq = new System.Windows.Forms.TextBoxTS();
            this.txtFromFreq = new System.Windows.Forms.TextBoxTS();
            this.lstFrequencies = new System.Windows.Forms.ListBox();
            this.chkFilterOnFrequencies = new System.Windows.Forms.CheckBoxTS();
            this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS3 = new System.Windows.Forms.ButtonTS();
            this.buttonTS2 = new System.Windows.Forms.ButtonTS();
            this.lstSelectedBands = new System.Windows.Forms.ListBox();
            this.lstAvailableBands = new System.Windows.Forms.ListBox();
            this.chkFilterOnBands = new System.Windows.Forms.CheckBoxTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.txtDescription = new System.Windows.Forms.TextBoxTS();
            this.txtName = new System.Windows.Forms.TextBoxTS();
            this.txtGUID = new System.Windows.Forms.TextBoxTS();
            this.btnDelete = new System.Windows.Forms.ButtonTS();
            this.groupBoxTS5.SuspendLayout();
            this.groupBoxTS4.SuspendLayout();
            this.groupBoxTS3.SuspendLayout();
            this.groupBoxTS2.SuspendLayout();
            this.groupBoxTS1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeFilters
            // 
            this.treeFilters.BackColor = System.Drawing.Color.Silver;
            this.treeFilters.Location = new System.Drawing.Point(12, 12);
            this.treeFilters.Name = "treeFilters";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Node0";
            treeNode2.Name = "Node0";
            treeNode2.Text = "System Filters";
            treeNode3.Name = "Node1";
            treeNode3.Text = "Node1";
            treeNode4.Name = "Node1";
            treeNode4.Text = "User Filters";
            this.treeFilters.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode4});
            this.treeFilters.Size = new System.Drawing.Size(190, 388);
            this.treeFilters.TabIndex = 0;
            // 
            // btnMore
            // 
            this.btnMore.Location = new System.Drawing.Point(811, 10);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(92, 23);
            this.btnMore.TabIndex = 18;
            this.btnMore.Text = "More >>";
            this.btnMore.UseVisualStyleBackColor = true;
            this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
            // 
            // labelTS12
            // 
            this.labelTS12.AutoSize = true;
            this.labelTS12.Image = null;
            this.labelTS12.Location = new System.Drawing.Point(1168, 232);
            this.labelTS12.Name = "labelTS12";
            this.labelTS12.Size = new System.Drawing.Size(25, 13);
            this.labelTS12.TabIndex = 23;
            this.labelTS12.Text = "930";
            // 
            // labelTS11
            // 
            this.labelTS11.AutoSize = true;
            this.labelTS11.Image = null;
            this.labelTS11.Location = new System.Drawing.Point(1168, 260);
            this.labelTS11.Name = "labelTS11";
            this.labelTS11.Size = new System.Drawing.Size(31, 13);
            this.labelTS11.TabIndex = 22;
            this.labelTS11.Text = "1272";
            // 
            // labelTS10
            // 
            this.labelTS10.AutoSize = true;
            this.labelTS10.Image = null;
            this.labelTS10.Location = new System.Drawing.Point(922, 195);
            this.labelTS10.Name = "labelTS10";
            this.labelTS10.Size = new System.Drawing.Size(42, 13);
            this.labelTS10.TabIndex = 21;
            this.labelTS10.Text = "Entries:";
            // 
            // groupBoxTS5
            // 
            this.groupBoxTS5.Controls.Add(this.labelTS9);
            this.groupBoxTS5.Controls.Add(this.labelTS8);
            this.groupBoxTS5.Controls.Add(this.labelTS7);
            this.groupBoxTS5.Controls.Add(this.textBoxTS8);
            this.groupBoxTS5.Controls.Add(this.textBoxTS7);
            this.groupBoxTS5.Controls.Add(this.textBoxTS6);
            this.groupBoxTS5.Location = new System.Drawing.Point(925, 10);
            this.groupBoxTS5.Name = "groupBoxTS5";
            this.groupBoxTS5.Size = new System.Drawing.Size(319, 169);
            this.groupBoxTS5.TabIndex = 20;
            this.groupBoxTS5.TabStop = false;
            this.groupBoxTS5.Text = "Last Visted Info";
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(20, 83);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(62, 13);
            this.labelTS9.TabIndex = 5;
            this.labelTS9.Text = "CenterFreq:";
            // 
            // labelTS8
            // 
            this.labelTS8.AutoSize = true;
            this.labelTS8.Image = null;
            this.labelTS8.Location = new System.Drawing.Point(20, 57);
            this.labelTS8.Name = "labelTS8";
            this.labelTS8.Size = new System.Drawing.Size(60, 13);
            this.labelTS8.TabIndex = 4;
            this.labelTS8.Text = "Frequency:";
            // 
            // labelTS7
            // 
            this.labelTS7.AutoSize = true;
            this.labelTS7.Image = null;
            this.labelTS7.Location = new System.Drawing.Point(17, 31);
            this.labelTS7.Name = "labelTS7";
            this.labelTS7.Size = new System.Drawing.Size(63, 13);
            this.labelTS7.TabIndex = 3;
            this.labelTS7.Text = "Description:";
            // 
            // textBoxTS8
            // 
            this.textBoxTS8.Location = new System.Drawing.Point(86, 80);
            this.textBoxTS8.Name = "textBoxTS8";
            this.textBoxTS8.Size = new System.Drawing.Size(100, 20);
            this.textBoxTS8.TabIndex = 2;
            // 
            // textBoxTS7
            // 
            this.textBoxTS7.Location = new System.Drawing.Point(86, 54);
            this.textBoxTS7.Name = "textBoxTS7";
            this.textBoxTS7.Size = new System.Drawing.Size(100, 20);
            this.textBoxTS7.TabIndex = 1;
            // 
            // textBoxTS6
            // 
            this.textBoxTS6.Location = new System.Drawing.Point(86, 28);
            this.textBoxTS6.Name = "textBoxTS6";
            this.textBoxTS6.Size = new System.Drawing.Size(100, 20);
            this.textBoxTS6.TabIndex = 0;
            // 
            // bandStackListBox1
            // 
            this.bandStackListBox1.BackColor = System.Drawing.Color.Silver;
            this.bandStackListBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.bandStackListBox1.FormattingEnabled = true;
            this.bandStackListBox1.Location = new System.Drawing.Point(970, 195);
            this.bandStackListBox1.LockImageLocked = null;
            this.bandStackListBox1.LockImageUnLocked = null;
            this.bandStackListBox1.Memory = null;
            this.bandStackListBox1.Name = "bandStackListBox1";
            this.bandStackListBox1.Size = new System.Drawing.Size(162, 173);
            this.bandStackListBox1.SpecificReturnImage = null;
            this.bandStackListBox1.SpecificReturnIndex = -1;
            this.bandStackListBox1.TabIndex = 19;
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(487, 41);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(45, 13);
            this.labelTS6.TabIndex = 17;
            this.labelTS6.Text = "(unique)";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Image = null;
            this.btnUpdate.Location = new System.Drawing.Point(730, 377);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 16;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnAddNew
            // 
            this.btnAddNew.Image = null;
            this.btnAddNew.Location = new System.Drawing.Point(827, 377);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(75, 23);
            this.btnAddNew.TabIndex = 15;
            this.btnAddNew.Text = "Add New";
            this.btnAddNew.UseVisualStyleBackColor = true;
            // 
            // groupBoxTS4
            // 
            this.groupBoxTS4.BackColor = System.Drawing.Color.Silver;
            this.groupBoxTS4.Controls.Add(this.buttonTS8);
            this.groupBoxTS4.Controls.Add(this.buttonTS9);
            this.groupBoxTS4.Controls.Add(this.lvlSelectedSubModes);
            this.groupBoxTS4.Controls.Add(this.lstAvailableSubModes);
            this.groupBoxTS4.Controls.Add(this.chkFilterOnSubModes);
            this.groupBoxTS4.Location = new System.Drawing.Point(562, 212);
            this.groupBoxTS4.Name = "groupBoxTS4";
            this.groupBoxTS4.Size = new System.Drawing.Size(341, 153);
            this.groupBoxTS4.TabIndex = 14;
            this.groupBoxTS4.TabStop = false;
            this.groupBoxTS4.Text = "Filter on SubModes";
            // 
            // buttonTS8
            // 
            this.buttonTS8.Image = null;
            this.buttonTS8.Location = new System.Drawing.Point(147, 84);
            this.buttonTS8.Name = "buttonTS8";
            this.buttonTS8.Size = new System.Drawing.Size(48, 23);
            this.buttonTS8.TabIndex = 12;
            this.buttonTS8.Text = "<<";
            this.buttonTS8.UseVisualStyleBackColor = true;
            // 
            // buttonTS9
            // 
            this.buttonTS9.Image = null;
            this.buttonTS9.Location = new System.Drawing.Point(147, 55);
            this.buttonTS9.Name = "buttonTS9";
            this.buttonTS9.Size = new System.Drawing.Size(48, 23);
            this.buttonTS9.TabIndex = 11;
            this.buttonTS9.Text = ">>";
            this.buttonTS9.UseVisualStyleBackColor = true;
            // 
            // lvlSelectedSubModes
            // 
            this.lvlSelectedSubModes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lvlSelectedSubModes.FormattingEnabled = true;
            this.lvlSelectedSubModes.Location = new System.Drawing.Point(201, 19);
            this.lvlSelectedSubModes.Name = "lvlSelectedSubModes";
            this.lvlSelectedSubModes.Size = new System.Drawing.Size(120, 121);
            this.lvlSelectedSubModes.TabIndex = 10;
            // 
            // lstAvailableSubModes
            // 
            this.lstAvailableSubModes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstAvailableSubModes.FormattingEnabled = true;
            this.lstAvailableSubModes.Location = new System.Drawing.Point(21, 20);
            this.lstAvailableSubModes.Name = "lstAvailableSubModes";
            this.lstAvailableSubModes.Size = new System.Drawing.Size(120, 121);
            this.lstAvailableSubModes.TabIndex = 9;
            // 
            // chkFilterOnSubModes
            // 
            this.chkFilterOnSubModes.AutoSize = true;
            this.chkFilterOnSubModes.Image = null;
            this.chkFilterOnSubModes.Location = new System.Drawing.Point(106, 0);
            this.chkFilterOnSubModes.Name = "chkFilterOnSubModes";
            this.chkFilterOnSubModes.Size = new System.Drawing.Size(15, 14);
            this.chkFilterOnSubModes.TabIndex = 0;
            this.chkFilterOnSubModes.UseVisualStyleBackColor = true;
            // 
            // groupBoxTS3
            // 
            this.groupBoxTS3.BackColor = System.Drawing.Color.Silver;
            this.groupBoxTS3.Controls.Add(this.buttonTS6);
            this.groupBoxTS3.Controls.Add(this.buttonTS7);
            this.groupBoxTS3.Controls.Add(this.lstSelectedModes);
            this.groupBoxTS3.Controls.Add(this.lstAvailableModes);
            this.groupBoxTS3.Controls.Add(this.chkFilterOnModes);
            this.groupBoxTS3.Location = new System.Drawing.Point(562, 53);
            this.groupBoxTS3.Name = "groupBoxTS3";
            this.groupBoxTS3.Size = new System.Drawing.Size(341, 153);
            this.groupBoxTS3.TabIndex = 13;
            this.groupBoxTS3.TabStop = false;
            this.groupBoxTS3.Text = "Filter on Modes";
            // 
            // buttonTS6
            // 
            this.buttonTS6.Image = null;
            this.buttonTS6.Location = new System.Drawing.Point(147, 84);
            this.buttonTS6.Name = "buttonTS6";
            this.buttonTS6.Size = new System.Drawing.Size(48, 23);
            this.buttonTS6.TabIndex = 12;
            this.buttonTS6.Text = "<<";
            this.buttonTS6.UseVisualStyleBackColor = true;
            // 
            // buttonTS7
            // 
            this.buttonTS7.Image = null;
            this.buttonTS7.Location = new System.Drawing.Point(147, 55);
            this.buttonTS7.Name = "buttonTS7";
            this.buttonTS7.Size = new System.Drawing.Size(48, 23);
            this.buttonTS7.TabIndex = 11;
            this.buttonTS7.Text = ">>";
            this.buttonTS7.UseVisualStyleBackColor = true;
            // 
            // lstSelectedModes
            // 
            this.lstSelectedModes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstSelectedModes.FormattingEnabled = true;
            this.lstSelectedModes.Location = new System.Drawing.Point(201, 19);
            this.lstSelectedModes.Name = "lstSelectedModes";
            this.lstSelectedModes.Size = new System.Drawing.Size(120, 121);
            this.lstSelectedModes.TabIndex = 10;
            // 
            // lstAvailableModes
            // 
            this.lstAvailableModes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstAvailableModes.FormattingEnabled = true;
            this.lstAvailableModes.Location = new System.Drawing.Point(21, 20);
            this.lstAvailableModes.Name = "lstAvailableModes";
            this.lstAvailableModes.Size = new System.Drawing.Size(120, 121);
            this.lstAvailableModes.TabIndex = 9;
            // 
            // chkFilterOnModes
            // 
            this.chkFilterOnModes.AutoSize = true;
            this.chkFilterOnModes.Image = null;
            this.chkFilterOnModes.Location = new System.Drawing.Point(86, 0);
            this.chkFilterOnModes.Name = "chkFilterOnModes";
            this.chkFilterOnModes.Size = new System.Drawing.Size(15, 14);
            this.chkFilterOnModes.TabIndex = 0;
            this.chkFilterOnModes.UseVisualStyleBackColor = true;
            // 
            // groupBoxTS2
            // 
            this.groupBoxTS2.BackColor = System.Drawing.Color.Silver;
            this.groupBoxTS2.Controls.Add(this.buttonTS5);
            this.groupBoxTS2.Controls.Add(this.buttonTS4);
            this.groupBoxTS2.Controls.Add(this.labelTS5);
            this.groupBoxTS2.Controls.Add(this.labelTS4);
            this.groupBoxTS2.Controls.Add(this.txtToFreq);
            this.groupBoxTS2.Controls.Add(this.txtFromFreq);
            this.groupBoxTS2.Controls.Add(this.lstFrequencies);
            this.groupBoxTS2.Controls.Add(this.chkFilterOnFrequencies);
            this.groupBoxTS2.Location = new System.Drawing.Point(211, 260);
            this.groupBoxTS2.Name = "groupBoxTS2";
            this.groupBoxTS2.Size = new System.Drawing.Size(341, 105);
            this.groupBoxTS2.TabIndex = 9;
            this.groupBoxTS2.TabStop = false;
            this.groupBoxTS2.Text = "Filter on Frequencies";
            // 
            // buttonTS5
            // 
            this.buttonTS5.Image = null;
            this.buttonTS5.Location = new System.Drawing.Point(147, 65);
            this.buttonTS5.Name = "buttonTS5";
            this.buttonTS5.Size = new System.Drawing.Size(48, 23);
            this.buttonTS5.TabIndex = 14;
            this.buttonTS5.Text = "<<";
            this.buttonTS5.UseVisualStyleBackColor = true;
            // 
            // buttonTS4
            // 
            this.buttonTS4.Image = null;
            this.buttonTS4.Location = new System.Drawing.Point(147, 35);
            this.buttonTS4.Name = "buttonTS4";
            this.buttonTS4.Size = new System.Drawing.Size(48, 23);
            this.buttonTS4.TabIndex = 13;
            this.buttonTS4.Text = ">>";
            this.buttonTS4.UseVisualStyleBackColor = true;
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(16, 64);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(23, 13);
            this.labelTS5.TabIndex = 13;
            this.labelTS5.Text = "To:";
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(6, 38);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(33, 13);
            this.labelTS4.TabIndex = 10;
            this.labelTS4.Text = "From:";
            // 
            // txtToFreq
            // 
            this.txtToFreq.Location = new System.Drawing.Point(45, 61);
            this.txtToFreq.Name = "txtToFreq";
            this.txtToFreq.Size = new System.Drawing.Size(96, 20);
            this.txtToFreq.TabIndex = 12;
            // 
            // txtFromFreq
            // 
            this.txtFromFreq.Location = new System.Drawing.Point(45, 35);
            this.txtFromFreq.Name = "txtFromFreq";
            this.txtFromFreq.Size = new System.Drawing.Size(96, 20);
            this.txtFromFreq.TabIndex = 11;
            // 
            // lstFrequencies
            // 
            this.lstFrequencies.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstFrequencies.FormattingEnabled = true;
            this.lstFrequencies.Location = new System.Drawing.Point(201, 19);
            this.lstFrequencies.Name = "lstFrequencies";
            this.lstFrequencies.Size = new System.Drawing.Size(120, 69);
            this.lstFrequencies.TabIndex = 10;
            // 
            // chkFilterOnFrequencies
            // 
            this.chkFilterOnFrequencies.AutoSize = true;
            this.chkFilterOnFrequencies.Image = null;
            this.chkFilterOnFrequencies.Location = new System.Drawing.Point(113, 0);
            this.chkFilterOnFrequencies.Name = "chkFilterOnFrequencies";
            this.chkFilterOnFrequencies.Size = new System.Drawing.Size(15, 14);
            this.chkFilterOnFrequencies.TabIndex = 0;
            this.chkFilterOnFrequencies.UseVisualStyleBackColor = true;
            // 
            // groupBoxTS1
            // 
            this.groupBoxTS1.BackColor = System.Drawing.Color.Silver;
            this.groupBoxTS1.Controls.Add(this.buttonTS3);
            this.groupBoxTS1.Controls.Add(this.buttonTS2);
            this.groupBoxTS1.Controls.Add(this.lstSelectedBands);
            this.groupBoxTS1.Controls.Add(this.lstAvailableBands);
            this.groupBoxTS1.Controls.Add(this.chkFilterOnBands);
            this.groupBoxTS1.Location = new System.Drawing.Point(211, 101);
            this.groupBoxTS1.Name = "groupBoxTS1";
            this.groupBoxTS1.Size = new System.Drawing.Size(341, 153);
            this.groupBoxTS1.TabIndex = 8;
            this.groupBoxTS1.TabStop = false;
            this.groupBoxTS1.Text = "Filter on Bands";
            // 
            // buttonTS3
            // 
            this.buttonTS3.Image = null;
            this.buttonTS3.Location = new System.Drawing.Point(147, 84);
            this.buttonTS3.Name = "buttonTS3";
            this.buttonTS3.Size = new System.Drawing.Size(48, 23);
            this.buttonTS3.TabIndex = 12;
            this.buttonTS3.Text = "<<";
            this.buttonTS3.UseVisualStyleBackColor = true;
            // 
            // buttonTS2
            // 
            this.buttonTS2.Image = null;
            this.buttonTS2.Location = new System.Drawing.Point(147, 55);
            this.buttonTS2.Name = "buttonTS2";
            this.buttonTS2.Size = new System.Drawing.Size(48, 23);
            this.buttonTS2.TabIndex = 11;
            this.buttonTS2.Text = ">>";
            this.buttonTS2.UseVisualStyleBackColor = true;
            // 
            // lstSelectedBands
            // 
            this.lstSelectedBands.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstSelectedBands.FormattingEnabled = true;
            this.lstSelectedBands.Location = new System.Drawing.Point(201, 19);
            this.lstSelectedBands.Name = "lstSelectedBands";
            this.lstSelectedBands.Size = new System.Drawing.Size(120, 121);
            this.lstSelectedBands.TabIndex = 10;
            // 
            // lstAvailableBands
            // 
            this.lstAvailableBands.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lstAvailableBands.FormattingEnabled = true;
            this.lstAvailableBands.Location = new System.Drawing.Point(21, 20);
            this.lstAvailableBands.Name = "lstAvailableBands";
            this.lstAvailableBands.Size = new System.Drawing.Size(120, 121);
            this.lstAvailableBands.TabIndex = 9;
            // 
            // chkFilterOnBands
            // 
            this.chkFilterOnBands.AutoSize = true;
            this.chkFilterOnBands.Image = null;
            this.chkFilterOnBands.Location = new System.Drawing.Point(86, 0);
            this.chkFilterOnBands.Name = "chkFilterOnBands";
            this.chkFilterOnBands.Size = new System.Drawing.Size(15, 14);
            this.chkFilterOnBands.TabIndex = 0;
            this.chkFilterOnBands.UseVisualStyleBackColor = true;
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(208, 67);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(63, 13);
            this.labelTS3.TabIndex = 7;
            this.labelTS3.Text = "Description:";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(229, 41);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(38, 13);
            this.labelTS2.TabIndex = 6;
            this.labelTS2.Text = "Name:";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(229, 15);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(37, 13);
            this.labelTS1.TabIndex = 5;
            this.labelTS1.Text = "GUID:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(277, 64);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(255, 20);
            this.txtDescription.TabIndex = 4;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(277, 38);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(204, 20);
            this.txtName.TabIndex = 3;
            // 
            // txtGUID
            // 
            this.txtGUID.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtGUID.Location = new System.Drawing.Point(277, 12);
            this.txtGUID.Name = "txtGUID";
            this.txtGUID.ReadOnly = true;
            this.txtGUID.Size = new System.Drawing.Size(255, 20);
            this.txtGUID.TabIndex = 2;
            // 
            // btnDelete
            // 
            this.btnDelete.Image = null;
            this.btnDelete.Location = new System.Drawing.Point(211, 377);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // frmFilterManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(1256, 411);
            this.Controls.Add(this.labelTS12);
            this.Controls.Add(this.labelTS11);
            this.Controls.Add(this.labelTS10);
            this.Controls.Add(this.groupBoxTS5);
            this.Controls.Add(this.bandStackListBox1);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.labelTS6);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnAddNew);
            this.Controls.Add(this.groupBoxTS4);
            this.Controls.Add(this.groupBoxTS3);
            this.Controls.Add(this.groupBoxTS2);
            this.Controls.Add(this.groupBoxTS1);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtGUID);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.treeFilters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(1272, 450);
            this.Name = "frmFilterManager";
            this.Text = "BandStack2 Filter Manager";
            this.groupBoxTS5.ResumeLayout(false);
            this.groupBoxTS5.PerformLayout();
            this.groupBoxTS4.ResumeLayout(false);
            this.groupBoxTS4.PerformLayout();
            this.groupBoxTS3.ResumeLayout(false);
            this.groupBoxTS3.PerformLayout();
            this.groupBoxTS2.ResumeLayout(false);
            this.groupBoxTS2.PerformLayout();
            this.groupBoxTS1.ResumeLayout(false);
            this.groupBoxTS1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeFilters;
        private System.Windows.Forms.ButtonTS btnDelete;
        private System.Windows.Forms.TextBoxTS txtGUID;
        private System.Windows.Forms.TextBoxTS txtName;
        private System.Windows.Forms.TextBoxTS txtDescription;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.GroupBoxTS groupBoxTS1;
        private System.Windows.Forms.ButtonTS buttonTS3;
        private System.Windows.Forms.ButtonTS buttonTS2;
        private System.Windows.Forms.ListBox lstSelectedBands;
        private System.Windows.Forms.ListBox lstAvailableBands;
        private System.Windows.Forms.CheckBoxTS chkFilterOnBands;
        private System.Windows.Forms.GroupBoxTS groupBoxTS2;
        private System.Windows.Forms.ButtonTS buttonTS5;
        private System.Windows.Forms.ButtonTS buttonTS4;
        private System.Windows.Forms.LabelTS labelTS5;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.TextBoxTS txtToFreq;
        private System.Windows.Forms.TextBoxTS txtFromFreq;
        private System.Windows.Forms.ListBox lstFrequencies;
        private System.Windows.Forms.CheckBoxTS chkFilterOnFrequencies;
        private System.Windows.Forms.GroupBoxTS groupBoxTS3;
        private System.Windows.Forms.ButtonTS buttonTS6;
        private System.Windows.Forms.ButtonTS buttonTS7;
        private System.Windows.Forms.ListBox lstSelectedModes;
        private System.Windows.Forms.ListBox lstAvailableModes;
        private System.Windows.Forms.CheckBoxTS chkFilterOnModes;
        private System.Windows.Forms.GroupBoxTS groupBoxTS4;
        private System.Windows.Forms.ButtonTS buttonTS8;
        private System.Windows.Forms.ButtonTS buttonTS9;
        private System.Windows.Forms.ListBox lvlSelectedSubModes;
        private System.Windows.Forms.ListBox lstAvailableSubModes;
        private System.Windows.Forms.CheckBoxTS chkFilterOnSubModes;
        private System.Windows.Forms.ButtonTS btnAddNew;
        private System.Windows.Forms.ButtonTS btnUpdate;
        private System.Windows.Forms.LabelTS labelTS6;
        private System.Windows.Forms.Button btnMore;
        private BandStackListBox bandStackListBox1;
        private System.Windows.Forms.GroupBoxTS groupBoxTS5;
        private System.Windows.Forms.LabelTS labelTS9;
        private System.Windows.Forms.LabelTS labelTS8;
        private System.Windows.Forms.LabelTS labelTS7;
        private System.Windows.Forms.TextBoxTS textBoxTS8;
        private System.Windows.Forms.TextBoxTS textBoxTS7;
        private System.Windows.Forms.TextBoxTS textBoxTS6;
        private System.Windows.Forms.LabelTS labelTS10;
        private System.Windows.Forms.LabelTS labelTS11;
        private System.Windows.Forms.LabelTS labelTS12;
    }
}
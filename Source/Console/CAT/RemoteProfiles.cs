using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Thetis
{
	/// <summary>
	/// Summary description for RemoteProfiles.
	/// </summary>
	public class RemoteProfiles : Form
	{
		private Button btnClose;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private XmlDocument pdoc;
		private ComboBox cboProfiles;
		private Console console;
		private CATParser parser;
		private string model;
		private string profile;
		private System.Windows.Forms.Label label1;
		private bool started = false;
		private bool updating = false;

		public RemoteProfiles(Console c)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();


			console = c;
			parser = new CATParser(console);
			model = console.CurrentModel.ToString().ToLower();
			if(model.StartsWith("s"))
				this.Text = "Remote Profiles for an "+model.ToUpper();
			else
				this.Text = "Remote Profiles for a "+model.ToUpper();

//			GetProfiles();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// 
		/// </summary>
		private void GetProfiles()
		{
			pdoc = new XmlDocument();

			if(File.Exists(Application.StartupPath+"\\command.xml"))
			{
				pdoc.Load(Application.StartupPath+"\\command.xml");
				DisplayProfileNames();
				started = true;
			}
			else
			{
				MessageBox.Show("Unable to locate command.xml","File Missing",MessageBoxButtons.OK);
				Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void DisplayProfileNames()
		{
			XPathNavigator nav = pdoc.CreateNavigator();
			XPathNodeIterator itr = nav.Select("profiles/profile[@radio='"+model.ToLower()+"']");
			if (itr.Count > 0)
			{
				cboProfiles.Items.Clear();
				while (itr.MoveNext())
				{
					cboProfiles.Items.Add(itr.Current.GetAttribute("name", "").ToUpper());
				}
			}
			if(cboProfiles.Items.Count >= 0)
				cboProfiles.SelectedIndex = 0;
			
		}


		private void ReadProfile()
		{
			string ans = "";
			XPathNavigator nav = pdoc.CreateNavigator();
			XPathNodeIterator itr = nav.Select("profiles/profile[@name='"+profile+"' and @radio='"+model+"']/command");
			if(itr.Count > 0)
			{
				while(itr.MoveNext())
				{
					ans = parser.Get(itr.Current.ToString());
				}
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnClose = new System.Windows.Forms.Button();
			this.cboProfiles = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(272, 112);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(72, 24);
			this.btnClose.TabIndex = 0;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// cboProfiles
			// 
			this.cboProfiles.Location = new System.Drawing.Point(16, 48);
			this.cboProfiles.Name = "cboProfiles";
			this.cboProfiles.Size = new System.Drawing.Size(328, 21);
			this.cboProfiles.TabIndex = 1;
			this.cboProfiles.SelectedIndexChanged += new System.EventHandler(this.cboProfiles_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Profile Name";
			// 
			// RemoteProfiles
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 150);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cboProfiles);
			this.Controls.Add(this.btnClose);
			this.Name = "RemoteProfiles";
			this.Activated += new System.EventHandler(this.RemoteProfiles_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void cboProfiles_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			profile = cboProfiles.SelectedItem.ToString().ToLower();
			if(started && !updating)
				ReadProfile();
		}

		private void RemoteProfiles_Activated(object sender, System.EventArgs e)
		{
			updating = true;
			GetProfiles();
			updating = false;
		}

	}
}

//=================================================================
// common.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.Linq;
    using System.IO.Ports;

namespace Thetis
{
	public class Common
	{
		public static void ControlList(Control c, ref ArrayList a)
		{
			if(c.Controls.Count > 0)
			{
                foreach (Control c2 in c.Controls)
                {
                    ControlList(c2, ref a);
                }
			}

			if(c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(CheckBoxTS) ||
				c.GetType() == typeof(ComboBoxTS) || c.GetType() == typeof(ComboBox) ||
				c.GetType() == typeof(NumericUpDownTS) || c.GetType() == typeof(NumericUpDown) ||
				c.GetType() == typeof(RadioButtonTS) || c.GetType() == typeof(RadioButton) ||
				c.GetType() == typeof(TextBoxTS) || c.GetType() == typeof(TextBox) ||
				c.GetType() == typeof(TrackBarTS) || c.GetType() == typeof(TrackBar) ||
				c.GetType() == typeof(ColorButton))
				a.Add(c);

		}
        
        public static void SaveForm(Form form, string tablename)
		{
			ArrayList a = new ArrayList();
			ArrayList temp = new ArrayList();

			ControlList(form, ref temp);

			foreach(Control c in temp)				// For each control
			{
				if(c.GetType() == typeof(CheckBoxTS))
					a.Add(c.Name+"/"+((CheckBoxTS)c).Checked.ToString());
				else if(c.GetType() == typeof(ComboBoxTS))
				{
					//if(((ComboBox)c).SelectedIndex >= 0)
					a.Add(c.Name+"/"+((ComboBoxTS)c).Text);
				}
				else if(c.GetType() == typeof(NumericUpDownTS))
					a.Add(c.Name+"/"+((NumericUpDownTS)c).Value.ToString());
				else if(c.GetType() == typeof(RadioButtonTS))
					a.Add(c.Name+"/"+((RadioButtonTS)c).Checked.ToString());
				else if(c.GetType() == typeof(TextBoxTS))
					a.Add(c.Name+"/"+((TextBoxTS)c).Text);
				else if(c.GetType() == typeof(TrackBarTS))
					a.Add(c.Name+"/"+((TrackBarTS)c).Value.ToString());
				else if(c.GetType() == typeof(ColorButton))
				{
					Color clr = ((ColorButton)c).Color;
					a.Add(c.Name+"/"+clr.R+"."+clr.G+"."+clr.B+"."+clr.A);
				}
#if(DEBUG)
				else if(c.GetType() == typeof(GroupBox) ||
					c.GetType() == typeof(CheckBoxTS) ||
					c.GetType() == typeof(ComboBox) ||
					c.GetType() == typeof(NumericUpDown) ||
					c.GetType() == typeof(RadioButton) ||
					c.GetType() == typeof(TextBox) ||
					c.GetType() == typeof(TrackBar))
					Debug.WriteLine(c.Name+" needs to be converted to a Thread Safe control.");
#endif
			}
			a.Add("Top/"+form.Top);
			a.Add("Left/"+form.Left);
			a.Add("Width/"+form.Width);
			a.Add("Height/"+form.Height);

			DB.SaveVars(tablename, ref a);		// save the values to the DB
		}

		public static void RestoreForm(Form form, string tablename, bool restore_size)
		{
			ArrayList temp = new ArrayList();		// list of all first level controls
			ControlList(form, ref temp);

			ArrayList checkbox_list = new ArrayList();
			ArrayList combobox_list = new ArrayList();
			ArrayList numericupdown_list = new ArrayList();
			ArrayList radiobutton_list = new ArrayList();
			ArrayList textbox_list = new ArrayList();
			ArrayList trackbar_list = new ArrayList();
			ArrayList colorbutton_list = new ArrayList();

			//ArrayList controls = new ArrayList();	// list of controls to restore
			foreach(Control c in temp)
			{
				if(c.GetType() == typeof(CheckBoxTS))			// the control is a CheckBoxTS
					checkbox_list.Add(c);
				else if(c.GetType() == typeof(ComboBoxTS))		// the control is a ComboBox
					combobox_list.Add(c);
				else if(c.GetType() == typeof(NumericUpDownTS))	// the control is a NumericUpDown
					numericupdown_list.Add(c);
				else if(c.GetType() == typeof(RadioButtonTS))	// the control is a RadioButton
					radiobutton_list.Add(c);
				else if(c.GetType() == typeof(TextBoxTS))		// the control is a TextBox
					textbox_list.Add(c);
				else if(c.GetType() == typeof(TrackBarTS))		// the control is a TrackBar (slider)
					trackbar_list.Add(c);
				else if(c.GetType() == typeof(ColorButton))
					colorbutton_list.Add(c);
			}
			temp.Clear();	// now that we have the controls we want, delete first list 

			ArrayList a = DB.GetVars(tablename);						// Get the saved list of controls
			a.Sort();
			
			// restore saved values to the controls
			foreach(string s in a)				// string is in the format "name,value"
			{
				string[] vals = s.Split('/');
				if(vals.Length > 2)
				{
					for(int i=2; i<vals.Length; i++)
						vals[1] += "/"+vals[i];
				}

				string name = vals[0];
				string val = vals[1];

				switch(name)
				{
					case "Top":
						form.StartPosition = FormStartPosition.Manual;
						int top = int.Parse(val);
						/*if(top < 0) top = 0;
						if(top > Screen.PrimaryScreen.Bounds.Height-form.Height && Screen.AllScreens.Length == 1)
							top = Screen.PrimaryScreen.Bounds.Height-form.Height;*/
						form.Top = top;
						break;
					case "Left":
						form.StartPosition = FormStartPosition.Manual;
						int left = int.Parse(val);
						/*if(left < 0) left = 0;
						if(left > Screen.PrimaryScreen.Bounds.Width-form.Width && Screen.AllScreens.Length == 1)
							left = Screen.PrimaryScreen.Bounds.Width-form.Width;*/
						form.Left = left;
						break;
					case "Width":
						if(restore_size)
						{
							int width = int.Parse(val);
							/*if(width + form.Left > Screen.PrimaryScreen.Bounds.Width && Screen.AllScreens.Length == 1)
								form.Left -= (width+form.Left-Screen.PrimaryScreen.Bounds.Width);*/
							form.Width = width;
						}
						break;
					case "Height":
						if(restore_size)
						{
							int height = int.Parse(val);
							/*if(height + form.Top > Screen.PrimaryScreen.Bounds.Height && Screen.AllScreens.Length == 1)
								form.Top -= (height+form.Top-Screen.PrimaryScreen.Bounds.Height);*/
							form.Height = height;
						}
						break;
				}

				if(s.StartsWith("chk"))			// control is a CheckBoxTS
				{
					for(int i=0; i<checkbox_list.Count; i++)
					{	// look through each control to find the matching name
						CheckBoxTS c = (CheckBoxTS)checkbox_list[i];
						if(c.Name.Equals(name))		// name found
						{
							c.Checked = bool.Parse(val);	// restore value
							i = checkbox_list.Count+1;
						}
						if(i == checkbox_list.Count)
							MessageBox.Show("Control not found: "+name);
					}
				}
				else if(s.StartsWith("combo"))	// control is a ComboBox
				{
					for(int i=0; i<combobox_list.Count; i++)
					{	// look through each control to find the matching name
						ComboBoxTS c = (ComboBoxTS)combobox_list[i];
						if(c.Name.Equals(name))		// name found
						{
							c.Text = val;	// restore value
							i = combobox_list.Count+1;
							if(c.Text != val) Debug.WriteLine("Warning: "+form.Name+"."+name+" did not set to "+val);
						}
						if(i == combobox_list.Count)
							MessageBox.Show("Control not found: "+name);
					}
				}
				else if(s.StartsWith("ud"))
				{
					for(int i=0; i<numericupdown_list.Count; i++)
					{	// look through each control to find the matching name
						NumericUpDownTS c = (NumericUpDownTS)numericupdown_list[i];
						if(c.Name.Equals(name))		// name found
						{
							decimal num = decimal.Parse(val);

							if(num > c.Maximum) num = c.Maximum;		// check endpoints
							else if(num < c.Minimum) num = c.Minimum;
							c.Value = num;			// restore value
							i = numericupdown_list.Count+1;
						}
						if(i == numericupdown_list.Count)
							MessageBox.Show("Control not found: "+name);	
					}
				}
				else if(s.StartsWith("rad"))
				{	// look through each control to find the matching name
					for(int i=0; i<radiobutton_list.Count; i++)
					{
						RadioButtonTS c = (RadioButtonTS)radiobutton_list[i];
						if(c.Name.Equals(name))		// name found
						{
							if(!val.ToLower().Equals("true") && !val.ToLower().Equals("false"))
								val = "True";
							c.Checked = bool.Parse(val);	// restore value
							i = radiobutton_list.Count+1;
						}
						if(i == radiobutton_list.Count)
							MessageBox.Show("Control not found: "+name);
					}
				}
				else if(s.StartsWith("txt"))
				{	// look through each control to find the matching name
					for(int i=0; i<textbox_list.Count; i++)
					{
						TextBoxTS c = (TextBoxTS)textbox_list[i];
						if(c.Name.Equals(name))		// name found
						{
							c.Text = val;	// restore value
							i = textbox_list.Count+1;
						}
						if(i == textbox_list.Count)
							MessageBox.Show("Control not found: "+name);
					}
				}
				else if(s.StartsWith("tb"))
				{
					// look through each control to find the matching name
					for(int i=0; i<trackbar_list.Count; i++)
					{
						TrackBarTS c = (TrackBarTS)trackbar_list[i];
						if(c.Name.Equals(name))		// name found
						{
							int num = int.Parse(val);
							if(num > c.Maximum) num = c.Maximum;
							if(num < c.Minimum) num = c.Minimum;
							c.Value = num;
							i = trackbar_list.Count+1;
						}
						if(i == trackbar_list.Count)
							MessageBox.Show("Control not found: "+name);
					}
				}
				else if(s.StartsWith("clrbtn"))
				{
					string[] colors = val.Split('.');
					if(colors.Length == 4)
					{
						int R,G,B,A;
						R = Int32.Parse(colors[0]);
						G = Int32.Parse(colors[1]);
						B = Int32.Parse(colors[2]);
						A = Int32.Parse(colors[3]);

						for(int i=0; i<colorbutton_list.Count; i++)
						{
							ColorButton c = (ColorButton)colorbutton_list[i];
							if(c.Name.Equals(name))		// name found
							{
								c.Color = Color.FromArgb(A, R, G, B);
								i = colorbutton_list.Count+1;
							}
							if(i == colorbutton_list.Count)
								MessageBox.Show("Control not found: "+name);
						}
					}
				}
			}

			ForceFormOnScreen(form);
		}

		public static void ForceFormOnScreen(Form f)
		{
            Screen[] screens = Screen.AllScreens;
			bool on_screen = false;

			int left = 0, right = 0, top = 0, bottom = 0;

			for(int i=0; i<screens.Length; i++)
			{
				if(screens[i].Bounds.Left < left)
					left = screens[i].Bounds.Left;

				if(screens[i].Bounds.Top < top)
					top = screens[i].Bounds.Top;

				if(screens[i].Bounds.Bottom > bottom)
					bottom = screens[i].Bounds.Bottom;

				if(screens[i].Bounds.Right > right)
					right = screens[i].Bounds.Right;
			}

			if(f.Left > left &&
				f.Top > top &&
				f.Right < right &&
				f.Bottom < bottom)
            	on_screen = true;				

			if(!on_screen)
			{
				//f.Location = new Point(0, 0);

				if(f.Left < left)
					f.Left = left;

				if(f.Top < top)
					f.Top = top;

				if(f.Bottom > bottom)
				{
					if((f.Top - (f.Bottom-bottom)) >= top)
						f.Top -= (f.Bottom-bottom);
					else f.Top = 0;
				}

				if(f.Right > right)
				{
					if((f.Left - (f.Right-right)) >= left)
						f.Left -= (f.Right-right);
					else f.Left = 0;
				}
			}
		}

		public static void TabControlInsert(TabControl tc, TabPage tp, int index)
		{
			tc.SuspendLayout();
			// temp storage to rearrange tabs
			TabPage[] temp = new TabPage[tc.TabPages.Count+1];

			// copy pages in order and insert new page when needed
			for(int i=0; i<tc.TabPages.Count+1; i++)
			{
				if(i < index) temp[i] = tc.TabPages[i];
				else if(i == index) temp[i] = tp;
				else if(i > index) temp[i] = tc.TabPages[i-1];
			}
			
			// erase all tab pages
			while(tc.TabPages.Count > 0)
				tc.TabPages.RemoveAt(0);

			// add them back with new page inserted
			for(int i=0; i<temp.Length; i++)
				tc.TabPages.Add(temp[i]);

			tc.ResumeLayout();
		}

        public static string[] SortedComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort<string>(ports, delegate(string strA, string strB)
            {
                try
                {
                    int idA = int.Parse(strA.Substring(3));
                    int idB = int.Parse(strB.Substring(3));

                    return idA.CompareTo(idB);
                }
                catch (Exception)
                {
                    return strA.CompareTo(strB);
                }
            });
            return ports;
        }

        public static string RevToString(uint rev)
        {
            return ((byte)(rev >> 24)).ToString() + "." +
                ((byte)(rev >> 16)).ToString() + "." +
                ((byte)(rev >> 8)).ToString() + "." +
                ((byte)(rev >> 0)).ToString();
        }
	}
}
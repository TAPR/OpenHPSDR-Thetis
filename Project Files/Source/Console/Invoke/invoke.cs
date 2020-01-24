//=================================================================
// invoke.cs
//=================================================================
// This file implements a thread-safe controls based on the
// System.Windows.Forms classes.
// Copyright (C) 2005  Eric Wachsmann
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
// You may contact us via email at: eric@flex-radio.com.
// Paper mail may be sent to: 
//    Eric Wachsmann C/O FlexRadio Systems, 8900 Marybank Dr., Austin, TX  78750, USA.
//=================================================================

using System.ComponentModel;
using System.Windows.Forms;
//using System.Drawing;

namespace System.Windows.Forms
{
	public class UI
	{		
		// Use of Invoke routines is required when accessing UI controls from
		// a thread other than the one that created them.

		public delegate void SetCtrlDel(Control c, object val);
		public delegate void CtrlVoidFunc(Control c, object[] obj);
		public delegate object CtrlRetFunc(Control c, object[] obj);

		#region Object

		#region Functions

		public static object CallObjectEquals(Control c, object[] obj)
		{
			return c.Equals(obj[0]);
		}

		public static object CallObjectGetHashCode(Control c, object[] obj)
		{
			return c.GetHashCode();
		}
		
		public static object CallObjectGetType(Control c, object[] obj)
		{
			return c.GetType();
		}

		public static object CallObjectToString(Control c, object[] obj)
		{
			return c.ToString();
		}

		#endregion

		#endregion

		#region MarshalByRefObject
		
		//public static object CallMarshalByRefObjectCreateObjRef(Control c, object[] obj)
		//{
		//	return c.CreateObjRef((Type)(obj[0]));
		//}

		public static object CallMarshalByRefObjectGetLifetimeService(Control c, object[] obj)
		{
			return c.GetLifetimeService();
		}

		public static object CallMarshalByRefObjectInitializeLifetimeService(Control c, object[] obj)
		{
			return c.GetLifetimeService();
		}
		
		#endregion
		
		#region Control

		#region Properties

		public static void SetControlAccessibleDefaultActionDescription(Control c, object val)
		{
			c.AccessibleDefaultActionDescription = (string)val;
		}

		public static void SetControlAccessibleDescription(Control c, object val)
		{
			c.AccessibleDescription = (string)val;
		}

		public static void SetControlAccessibleName(Control c, object val)
		{
			c.AccessibleName = (string)val;
		}

		public static void SetControlAccessibleRole(Control c, object val)
		{
			c.AccessibleRole = (AccessibleRole)val;
		}

		public static void SetControlAllowDrop(Control c, object val)
		{
			c.AllowDrop = (bool)val;
		}

		public static void SetControlAnchor(Control c, object val)
		{
			c.Anchor = (AnchorStyles)val;
		}

		public static void SetControlBackColor(Control c, object val)
		{
			c.BackColor = (System.Drawing.Color)val;
		}

		public static void SetControlBackgroundImage(Control c, object val)
		{
			c.BackgroundImage = (System.Drawing.Image)val;
		}

		public static void SetControlBindingContext(Control c, object val)
		{
			c.BindingContext = (BindingContext)val;
		}

		public static void SetControlBounds(Control c, object val)
		{
			c.Bounds = (System.Drawing.Rectangle)val;
		}

		public static void SetControlCapture(Control c, object val)
		{
			c.Capture = (bool)val;
		}
		
		public static void SetControlCausesValidation(Control c, object val)
		{
			c.CausesValidation = (bool)val;
		}

		public static void SetControlClientSize(Control c, object val)
		{
			c.ClientSize = (System.Drawing.Size)val;
		}

		public static void SetControlContextMenuStrip(Control c, object val)
		{
			c.ContextMenuStrip = (ContextMenuStrip)val;
		}

		public static void SetControlCursor(Control c, object val)
		{
			c.Cursor = (Cursor)val;
		}

		public static void SetControlDock(Control c, object val)
		{
			c.Dock = (DockStyle)val;
		}

		public static void SetControlEnabled(Control c, object val)
		{
			c.Enabled = (bool)val;
		}

		public static void SetControlFont(Control c, object val)
		{
			c.Font = (System.Drawing.Font)val;
		}

		public static void SetControlForeColor(Control c, object val)
		{
			c.ForeColor = (System.Drawing.Color)val;
		}

		public static void SetControlHeight(Control c, object val)
		{
			c.Height = (int)val;
		}

		public static void SetControlImeMode(Control c, object val)
		{
			c.ImeMode = (ImeMode)val;
		}

		public static void SetControlIsAccessible(Control c, object val)
		{
			c.IsAccessible = (bool)val;
		}
	
		public static void SetControlLeft(Control c, object val)
		{
			c.Left = (int)val;
		}	

		public static void SetControlLocation(Control c, object val)
		{
			c.Location = (System.Drawing.Point)val;
		}

		public static void SetControlName(Control c, object val)
		{
			c.Name = (string)val;
		}

		public static void SetControlParent(Control c, object val)
		{
			c.Parent = (Control)val;
		}

		public static void SetControlRegion(Control c, object val)
		{
			c.Region = (System.Drawing.Region)val;
		}

		public static void SetControlRightToLeft(Control c, object val)
		{
			c.RightToLeft = (RightToLeft)val;
		}

		public static void SetControlSite(Control c, object val)
		{
			c.Site = (ISite)val;
		}

		public static void SetControlSize(Control c, object val)
		{
			c.Size = (System.Drawing.Size)val;
		}

		public static void SetControlTabIndex(Control c, object val)
		{
			c.TabIndex = (int)val;
		}

		public static void SetControlTabStop(Control c, object val)
		{
			c.TabStop = (bool)val;
		}

		public static void SetControlTag(Control c, object val)
		{
			c.Tag = val;
		}

		public static void SetControlText(Control c, object val)
		{
			c.Text = (string)val;
		}

		public static void SetControlTop(Control c, object val)
		{
			c.Top = (int)val;
		}

		public static void SetControlVisible(Control c, object val)
		{
			c.Visible = (bool)val;
		}

		public static void SetControlWidth(Control c, object val)
		{
			c.Width = (int)val;
		}

		#endregion

		#region Functions

		public static object CallControlContains(Control c, object[] obj)
		{
			return c.Contains((Control)(obj[0]));
		}

		public static object CallControlCreateGraphics(Control c, object[] obj)
		{
			return c.CreateGraphics();
		}

		public static object CallControlDoDragDrop(Control c, object[] obj)
		{
			return c.DoDragDrop(obj[0], (DragDropEffects)obj[1]);
		}

		public static object CallControlFindForm(Control c, object[] obj)
		{
			return c.FindForm();
		}

		public static object CallControlFocus(Control c, object[] obj)
		{
			return c.Focus();
		}

		public static object CallControlGetChildAtPoint(Control c, object[] obj)
		{
			return c.GetChildAtPoint((System.Drawing.Point)obj[0]);
		}

		public static object CallControlGetContainerControl(Control c, object[] obj)
		{
			return c.GetContainerControl();
		}
	
		public static object CallControlGetNextControl(Control c, object[] obj)
		{
			return c.GetNextControl((Control)(obj[0]), (bool)(obj[1]));
		}

		public static void CallControlInvalidate(Control c, object[] obj)
		{
			if(obj.Length == 1)
			{
				if(obj[0].GetType() == typeof(bool))
					c.Invalidate((bool)obj[0]);
				else if(obj[0].GetType() == typeof(System.Drawing.Rectangle))
					c.Invalidate((System.Drawing.Rectangle)obj[0]);
				else if(obj[0].GetType() == typeof(System.Drawing.Region))
					c.Invalidate((System.Drawing.Region)obj[0]);
			}
			else if(obj.Length == 2)
			{
				if(obj[0].GetType() == typeof(System.Drawing.Rectangle) &&
					obj[1].GetType() == typeof(bool))
					c.Invalidate((System.Drawing.Rectangle)obj[0], (bool)obj[1]);
				else if(obj[0].GetType() == typeof(System.Drawing.Region) &&
					obj[1].GetType() == typeof(bool))
					c.Invalidate((System.Drawing.Region)obj[0], (bool)obj[1]);
			}
		}

		public static void CallControlPerformLayout(Control c, object[] obj)
		{
			c.PerformLayout((Control)obj[0], (string)obj[1]);
		}

		public static object CallControlPointToClient(Control c, object[] obj)
		{
			return c.PointToClient((System.Drawing.Point)obj[0]);
		}

		public static object CallControlPointToScreen(Control c, object[] obj)
		{
			return c.PointToScreen((System.Drawing.Point)obj[0]);
		}

		public static object CallControlPreProcessMessage(Control c, object[] obj)
		{
			Message msg = (Message)obj[0];
			return c.PreProcessMessage(ref msg);
		}

		public static object CallControlRectangleToClient(Control c, object[] obj)
		{
			return c.RectangleToClient((System.Drawing.Rectangle)obj[0]);
		}

		public static object CallControlRectangleToScreen(Control c, object[] obj)
		{
			return c.RectangleToScreen((System.Drawing.Rectangle)obj[0]);
		}

		public static void CallControlResumeLayout(Control c, object[] obj)
		{
			c.ResumeLayout((bool)obj[0]);
		}
		
		public static void CallControlScale(Control c, object[] obj)
		{
			if(obj.Length == 1)
				c.Scale((System.Drawing.SizeF)obj[0]);
		}

		public static object CallControlSelectNextControl(Control c, object[] obj)
		{
			return c.SelectNextControl((Control)obj[0], (bool)obj[1], (bool)obj[2],
				(bool)obj[3], (bool)obj[4]);
		}

		public static void CallControlSetBounds(Control c, object[] obj)
		{
			if(obj.Length == 4)
				c.SetBounds((int)obj[0], (int)obj[1], (int)obj[2], (int)obj[3]);
			if(obj.Length == 5)
				c.SetBounds((int)obj[0], (int)obj[1], (int)obj[2], (int)obj[3],
					(BoundsSpecified)obj[4]);
		}	

		#endregion

		#endregion

		#region ScrollableControl

		#region Properties

		public static void SetScrollableControlAutoScroll(Control c, object val)
		{
			((ScrollableControl)c).AutoScroll = (bool)val;
		}	
	
		public static void SetScrollableControlAutoScrollMargin(Control c, object val)
		{
			((ScrollableControl)c).AutoScrollMargin = (System.Drawing.Size)val;
		}

		public static void SetScrollableControlAutoScrollMinSize(Control c, object val)
		{
			((ScrollableControl)c).AutoScrollMinSize = (System.Drawing.Size)val;
		}

		public static void SetScrollableControlAutoScrollPosition(Control c, object val)
		{
			((ScrollableControl)c).AutoScrollPosition = (System.Drawing.Point)val;
		}

		#endregion

		#region Functions

		public static void CallScrollableControlScrollControlIntoView(Control c, object[] val)
		{
			((ScrollableControl)c).ScrollControlIntoView((Control)(val[0]));
		}

		public static void CallScrollableControlSetAutoScrollMargin(Control c, object[] val)
		{
			((ScrollableControl)c).SetAutoScrollMargin((int)val[0], (int)val[1]);
		}

		#endregion

		#endregion

		#region ContainerControl

		#region Properties

		public static void SetContainerControlActiveControl(Control c, object val)
		{
			((ContainerControl)c).ActiveControl = (Control)val;
		}

		#endregion

		#region Functions

		public static object CallContainerControlValidate(Control c, object[] val)
		{
			return ((ContainerControl)c).Validate();
		}		

		#endregion

		#endregion

		#region UpDownBase
		
		#region Properties

		public static void SetUpDownBaseBorderStyle(Control c, object val)
		{
			((UpDownBase)c).BorderStyle = (BorderStyle)val;
		}

		public static void SetUpDownBaseInterceptArrowKeys(Control c, object val)
		{
			((UpDownBase)c).InterceptArrowKeys = (bool)val;
		}

		public static void SetUpDownBaseReadOnly(Control c, object val)
		{
			((UpDownBase)c).ReadOnly = (bool)val;
		}

		public static void SetUpDownBaseTextAlign(Control c, object val)
		{
			((UpDownBase)c).TextAlign = (HorizontalAlignment)val;
		}

		public static void SetUpDownBaseUpDownAlign(Control c, object val)
		{
			((UpDownBase)c).UpDownAlign = (LeftRightAlignment)val;
		}		

		#endregion

		#region Functions

		public static void CallUpDownBaseSelect(Control c, object[] val)
		{
			((UpDownBase)c).Select((int)val[0], (int)val[1]);
		}	
		

		#endregion

		#endregion

		#region TextBoxBase

		#region Properties

		public static void SetTextBoxBaseAcceptsTab(Control c, object val)
		{
			((TextBoxBase)c).AcceptsTab = (bool)val;
		}

		public static void SetTextBoxBaseAutoSize(Control c, object val)
		{
			((TextBoxBase)c).AutoSize = (bool)val;
		}

		public static void SetTextBoxBaseBackColor(Control c, object val)
		{
			((TextBoxBase)c).BackColor = (System.Drawing.Color)val;
		}

		public static void SetTextBoxBaseBorderStyle(Control c, object val)
		{
			((TextBoxBase)c).BorderStyle = (BorderStyle)val;
		}

		public static void SetTextBoxBaseForeColor(Control c, object val)
		{
			((TextBoxBase)c).ForeColor = (System.Drawing.Color)val;
		}

		public static void SetTextBoxBaseHideSelection(Control c, object val)
		{
			((TextBoxBase)c).HideSelection = (bool)val;
		}

		public static void SetTextBoxBaseLines(Control c, object val)
		{
			((TextBoxBase)c).Lines = (string[])val;
		}

		public static void SetTextBoxBaseMaxLength(Control c, object val)
		{
			((TextBoxBase)c).MaxLength = (int)val;
		}

		public static void SetTextBoxBaseModified(Control c, object val)
		{
			((TextBoxBase)c).Modified = (bool)val;
		}

		public static void SetTextBoxBaseMultiline(Control c, object val)
		{
			((TextBoxBase)c).Multiline = (bool)val;
		}

		public static void SetTextBoxBaseReadOnly(Control c, object val)
		{
			((TextBoxBase)c).ReadOnly = (bool)val;
		}

		public static void SetTextBoxBaseSelectedText(Control c, object val)
		{
			((TextBoxBase)c).SelectedText = (string)val;
		}

		public static void SetTextBoxBaseSelectionStart(Control c, object val)
		{
			((TextBoxBase)c).SelectionStart = (int)val;
		}

		public static void SetTextBoxBaseWordWrap(Control c, object val)
		{
			((TextBoxBase)c).WordWrap = (bool)val;
		}

		#endregion

		#region Functions

		public static void CallTextBoxBaseAppendText(Control c, object[] val)
		{
			((TextBoxBase)c).AppendText((string)(val[0]));
		}

		public static void CallTextBoxBaseSelect(Control c, object[] val)
		{
			((TextBoxBase)c).Select((int)val[0], (int)val[1]);
		}

		#endregion

		#endregion

		#region ListControl

		#region Properties

		public static void SetListControlDataSource(Control c, object val)
		{
			((ListControl)c).DataSource = val;
		}

		public static void SetListControlDisplayMember(Control c, object val)
		{
			((ListControl)c).DisplayMember = (string)val;
		}

		public static void SetListControlSelectedValue(Control c, object val)
		{
			((ListControl)c).SelectedValue = val;
		}

		#endregion

		#region Functions

		public static object CallListControlGetItemText(Control c, object[] obj)
		{
			return ((ListControl)c).GetItemText(obj[0]);
		}

		#endregion

		#endregion

		#region ButtonBase

		#region Properties

		public static void SetButtonBaseFlatStyle(Control c, object val)
		{
			((ButtonBase)c).FlatStyle = (FlatStyle)val;
		}

		public static void SetButtonBaseImage(Control c, object val)
		{
			((ButtonBase)c).Image = (System.Drawing.Image)val;
		}

		public static void SetButtonBaseImageAlign(Control c, object val)
		{
			((ButtonBase)c).ImageAlign = (System.Drawing.ContentAlignment)val;
		}

		public static void SetButtonBaseImageIndex(Control c, object val)
		{
			((ButtonBase)c).ImageIndex = (int)val;
		}

		public static void SetButtonBaseImageList(Control c, object val)
		{
			((ButtonBase)c).ImageList = (ImageList)val;
		}

		public static void SetButtonBaseImeMode(Control c, object val)
		{
			((ButtonBase)c).ImeMode = (ImeMode)val;
		}

		public static void SetButtonBaseTextAlign(Control c, object val)
		{
			((ButtonBase)c).TextAlign = (System.Drawing.ContentAlignment)val;
		}		

		#endregion

		#endregion

		#region Button

		#region Properties

		public static void SetButtonDialogResult(Control c, object val)
		{
			((Button)c).DialogResult = (DialogResult)val;
		}
		
		#endregion

		#region Functions

		public static void CallButtonNotifyDefault(Control c, object[] obj)
		{
			((Button)c).NotifyDefault((bool)obj[0]);
		}

		#endregion

		#endregion

		#region ComboBox

		#region Properties

		public static void SetComboBoxBackColor(Control c, object val)
		{
			((ComboBox)c).BackColor = (System.Drawing.Color)val;
		}

		public static void SetComboBoxDrawMode(Control c, object val)
		{
			((ComboBox)c).DrawMode = (DrawMode)val;
		}

		public static void SetComboBoxDropDownStyle(Control c, object val)
		{
			((ComboBox)c).DropDownStyle = (ComboBoxStyle)val;
		}

		public static void SetComboBoxDropDownWidth(Control c, object val)
		{
			((ComboBox)c).DropDownWidth = (int)val;
		}

		public static void SetComboBoxDroppedDown(Control c, object val)
		{
			((ComboBox)c).DroppedDown = (bool)val;
		}

		public static void SetComboBoxForeColor(Control c, object val)
		{
			((ComboBox)c).ForeColor = (System.Drawing.Color)val;
		}

		public static void SetComboBoxIntegralHeight(Control c, object val)
		{
			((ComboBox)c).IntegralHeight = (bool)val;
		}

		public static void SetComboBoxMaxDropDownItems(Control c, object val)
		{
			((ComboBox)c).MaxDropDownItems = (int)val;
		}

		public static void SetComboBoxMaxLength(Control c, object val)
		{
			((ComboBox)c).MaxLength = (int)val;
		}

		public static void SetComboBoxSelectedIndex(Control c, object val)
		{
			((ComboBox)c).SelectedIndex = (int)val;
		}

		public static void SetComboBoxSelectedItem(Control c, object val)
		{
			((ComboBox)c).SelectedItem = val;
		}

		public static void SetComboBoxSelectedText(Control c, object val)
		{
			((ComboBox)c).SelectedText = (string)val;
		}

		public static void SetComboBoxSelectionLength(Control c, object val)
		{
			((ComboBox)c).SelectionLength = (int)val;
		}

		public static void SetComboBoxSelectionStart(Control c, object val)
		{
			((ComboBox)c).SelectionStart = (int)val;
		}

		public static void SetComboBoxSorted(Control c, object val)
		{
			((ComboBox)c).Sorted = (bool)val;
		}

		public static void SetComboBoxText(Control c, object val)
		{
			((ComboBox)c).Text = (string)val;
		}

		public static void SetComboBoxValueMember(Control c, object val)
		{
			((ComboBox)c).ValueMember = (string)val;
		}

		#endregion

		#region Functions

		public static object CallComboBoxFindString(Control c, object[] obj)
		{
			if(obj.Length == 1)
				return ((ComboBox)c).FindString((string)obj[0]);
			else //if(obj.Length == 2)
				return ((ComboBox)c).FindString((string)obj[0], (int)obj[1]);
		}

		public static object CallComboBoxFindStringExact(Control c, object[] obj)
		{
			if(obj.Length == 1)
				return ((ComboBox)c).FindStringExact((string)obj[0]);
			else //if(obj.Length == 2)
				return ((ComboBox)c).FindStringExact((string)obj[0], (int)obj[1]);
		}

		public static object CallComboBoxGetItemHeight(Control c, object[] obj)
		{
			return ((ComboBox)c).GetItemHeight((int)obj[0]);
		}

		#endregion

		#endregion

		#region TextBox

		#region Properties

		public static void SetTextBoxAcceptsReturn(Control c, object val)
		{
			((TextBox)c).AcceptsReturn = (bool)val;
		}

		public static void SetTextBoxCharacterCasing(Control c, object val)
		{
			((TextBox)c).CharacterCasing = (CharacterCasing)val;
		}

		public static void SetTextBoxPasswordChar(Control c, object val)
		{
			((TextBox)c).PasswordChar = (char)val;
		}

		public static void SetTextBoxScrollBars(Control c, object val)
		{
			((TextBox)c).ScrollBars = (ScrollBars)val;
		}

		public static void SetTextBoxSelectionLength(Control c, object val)
		{
			((TextBox)c).SelectionLength = (int)val;
		}

		public static void SetTextBoxText(Control c, object val)
		{
			((TextBox)c).Text = (string)val;
		}

		public static void SetTextBoxTextAlign(Control c, object val)
		{
			((TextBox)c).TextAlign = (HorizontalAlignment)val;
		}

		#endregion

		#endregion

		#region CheckBox

		#region Properties

		public static void SetCheckBoxAppearance(Control c, object val)
		{
			((CheckBox)c).Appearance = (Appearance)val;
		}

		public static void SetCheckBoxAutoCheck(Control c, object val)
		{
			((CheckBox)c).AutoCheck = (bool)val;
		}

		public static void SetCheckBoxCheckAlign(Control c, object val)
		{
			((CheckBox)c).CheckAlign = (System.Drawing.ContentAlignment)val;
		}

		public static void SetCheckBoxChecked(Control c, object val)
		{
			((CheckBox)c).Checked = (bool)val;
		}

		public static void SetCheckBoxCheckState(Control c, object val)
		{
			((CheckBox)c).CheckState = (CheckState)val;
		}

		public static void SetCheckBoxThreeState(Control c, object val)
		{
			((CheckBox)c).ThreeState = (bool)val;
		}	

		#endregion

		#endregion

		#region RadioButton

		#region Properties

		public static void SetRadioButtonAppearance(Control c, object val)
		{
			((RadioButton)c).Appearance = (Appearance)val;
		}

		public static void SetRadioButtonAutoCheck(Control c, object val)
		{
			((RadioButton)c).AutoCheck = (bool)val;
		}

		public static void SetRadioButtonCheckAlign(Control c, object val)
		{
			((RadioButton)c).CheckAlign = (System.Drawing.ContentAlignment)val;
		}

		public static void SetRadioButtonChecked(Control c, object val)
		{
			((RadioButton)c).Checked = (bool)val;
		}

		public static void SetRadioButtonTextAlign(Control c, object val)
		{
			((RadioButton)c).TextAlign = (System.Drawing.ContentAlignment)val;
		}	

		#endregion

		#endregion

		#region NumericUpDown

		#region Properties

		public static void SetNumericUpDownDecimalPlaces(Control c, object val)
		{
			((NumericUpDown)c).DecimalPlaces = (int)val;
		}

		public static void SetNumericUpDownHexadecimal(Control c, object val)
		{
			((NumericUpDown)c).Hexadecimal = (bool)val;
		}

		public static void SetNumericUpDownIncrement(Control c, object val)
		{
			((NumericUpDown)c).Increment = (decimal)val;
		}

		public static void SetNumericUpDownMaximum(Control c, object val)
		{
			((NumericUpDown)c).Maximum = (decimal)val;
		}

		public static void SetNumericUpDownMinimum(Control c, object val)
		{
			((NumericUpDown)c).Minimum = (decimal)val;
		}

		public static void SetNumericUpDownText(Control c, object val)
		{
			((NumericUpDown)c).Text = (string)val;
		}

		public static void SetNumericUpDownThousandsSeparator(Control c, object val)
		{
			((NumericUpDown)c).ThousandsSeparator = (bool)val;
		}

		public static void SetNumericUpDownValue(Control c, object val)
		{
			((NumericUpDown)c).Value = (decimal)val;
		}	

		#endregion

		#endregion

		#region TrackBar

		#region Properties

		public static void SetTrackBarAutoSize(Control c, object val)
		{
			((TrackBar)c).AutoSize = (bool)val;
		}

		public static void SetTrackBarLargeChange(Control c, object val)
		{
			((TrackBar)c).LargeChange = (int)val;
		}

		public static void SetTrackBarMaximum(Control c, object val)
		{
			((TrackBar)c).Maximum = (int)val;
		}

		public static void SetTrackBarMinimum(Control c, object val)
		{
			((TrackBar)c).Minimum = (int)val;
		}

		public static void SetTrackBarOrientation(Control c, object val)
		{
			((TrackBar)c).Orientation = (Orientation)val;
		}

		public static void SetTrackBarSmallChange(Control c, object val)
		{
			((TrackBar)c).SmallChange = (int)val;
		}

		public static void SetTrackBarTickFrequency(Control c, object val)
		{
			((TrackBar)c).TickFrequency = (int)val;
		}

		public static void SetTrackBarTickStyle(Control c, object val)
		{
			((TrackBar)c).TickStyle = (TickStyle)val;
		}

		public static void SetTrackBarValue(Control c, object val)
		{
			((TrackBar)c).Value = (int)val;
		}

		#endregion 

		#region Functions

		public static void CallTrackBarSetRange(Control c, object[] obj)
		{
			((TrackBar)c).SetRange((int)obj[0], (int)obj[1]);
		}

		#endregion

		#endregion

		#region Label

		#region Properties

		public static void SetLabelAutoSize(Control c, object val)
		{
			((Label)c).AutoSize = (bool)val;
		}

		public static void SetLabelBorderStyle(Control c, object val)
		{
			((Label)c).BorderStyle = (BorderStyle)val;
		}

		public static void SetLabelFlatStyle(Control c, object val)
		{
			((Label)c).FlatStyle = (FlatStyle)val;
		}

		public static void SetLabelImage(Control c, object val)
		{
			((Label)c).Image = (System.Drawing.Image)val;
		}

		public static void SetLabelImageAlign(Control c, object val)
		{
			((Label)c).ImageAlign = (System.Drawing.ContentAlignment)val;
		}

		public static void SetLabelImageIndex(Control c, object val)
		{
			((Label)c).ImageIndex = (int)val;
		}

		public static void SetLabelImageList(Control c, object val)
		{
			((Label)c).ImageList = (ImageList)val;
		}

		public static void SetLabelText(Control c, object val)
		{
			((Label)c).Text = (string)val;
		}

		public static void SetLabelTextAlign(Control c, object val)
		{
			((Label)c).TextAlign = (System.Drawing.ContentAlignment)val;
		}

		public static void SetLabelUseMnemonic(Control c, object val)
		{
			((Label)c).UseMnemonic = (bool)val;
		}

		#endregion

		#endregion

		#region GroupBox

		#region Properties

		public static void SetGroupBoxAllowDrop(Control c, object val)
		{
			((GroupBox)c).AllowDrop = (bool)val;
		}

		public static void SetGroupBoxFlatStyle(Control c, object val)
		{
			((GroupBox)c).FlatStyle = (FlatStyle)val;
		}
	
		public static void SetGroupBoxText(Control c, object val)
		{
			((GroupBox)c).Text = (string)val;
		}	

		#endregion

		#endregion

		#region Panel

		#region Properties

		public static void SetPanelBorderStyle(Control c, object val)
		{
			((Panel)c).BorderStyle = (BorderStyle)val;
		}

		public static void SetPanelTabStop(Control c, object val)
		{
			((Panel)c).TabStop = (bool)val;
		}

		public static void SetPanelText(Control c, object val)
		{
			((Panel)c).Text = (string)val;
		}

		#endregion

		#endregion
	}
}
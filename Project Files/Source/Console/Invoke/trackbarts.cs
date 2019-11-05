//=================================================================
// trackbarts.cs
//=================================================================
// This file implements a thread-safe TrackBar control based on the
// System.Windows.Forms.TrackBar class.
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
using System.Drawing;
using System.Runtime.Remoting;

namespace System.Windows.Forms
{
	public class TrackBarTS : TrackBar
	{
		#region Properties

		public new string AccessibleDefaultActionDescription
		{
			get { return base.AccessibleDefaultActionDescription; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleDefaultActionDescription),
						new object[] { this, value });
				else base.AccessibleDefaultActionDescription = value;
			}
		}

		public new string AccessibleDescription
		{
			get { return base.AccessibleDescription; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleDescription), new object[] {this, value});
				else base.AccessibleDescription = value;
			}
		}

		public new string AccessibleName
		{
			get { return base.AccessibleName; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleName), new object[] {this, value});
				else base.AccessibleName = value;
			}
		}

		public new AccessibleRole AccessibleRole
		{
			get { return base.AccessibleRole; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAccessibleRole), new object[] {this, value});
				else base.AccessibleRole = value;
			}
		}

		public new bool AllowDrop
		{
			get { return base.AllowDrop; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAllowDrop), new object[] {this, value});
				else base.AllowDrop = value;
			}
		}

		public new AnchorStyles Anchor
		{
			get { return base.Anchor; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlAnchor), new object[] {this, value});
				else base.Anchor = value;
			}
		}

		public new bool AutoSize
		{
			get { return base.AutoSize; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarAutoSize), new object[] {this, value});
				base.AutoSize = value;
			}
		}
		
		public new Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlBackColor), new object[] {this, value});
				else base.BackColor = value;
			}
		}

		public new virtual BindingContext BindingContext
		{
			get { return base.BindingContext; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlBindingContext),
						new object[] { this, value });
				else base.BindingContext = value;
			}
		}

		public new Rectangle Bounds
		{
			get { return base.Bounds; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlBounds),
						new object[] { this, value });
				else base.Bounds = value;
			}
		}

		public new bool Capture
		{
			get { return base.Capture; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlCapture),
						new object[] { this, value });
				else base.Capture = value;
			}
		}

		public new bool CausesValidation
		{
			get { return base.CausesValidation; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlCausesValidation), new object[] {this, value});
				else base.CausesValidation = value;
			}
		}

		public new System.Drawing.Size ClientSize
		{
			get { return base.ClientSize; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlClientSize),
						new object[] { this, value });
				else base.ClientSize = value;
			}
		}

		public new ContextMenu ContextMenu
		{
			get { return base.ContextMenu; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlContextMenu), new object[] {this, value});
				else base.ContextMenu = value;
			}
		}

		public new Cursor Cursor
		{
			get { return base.Cursor; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlCursor), new object[] {this, value});
				else base.Cursor = value;
			}
		}

		public new DockStyle Dock
		{
			get { return base.Dock; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlDock), new object[] {this, value});
				else base.Dock = value;
			}
		}

		public new bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlEnabled), new object[] {this, value});
				else base.Enabled = value;
			}
		}

		public new int Height
		{
			get { return base.Height; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlHeight), new object[] {this, value});
				else base.Height = value;
			}
		}

		public new bool IsAccessible
		{
			get { return base.IsAccessible; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlIsAccessible), new object[] {this, value});
				else base.IsAccessible = value;
			}
		}

		public new int LargeChange
		{
			get { return base.LargeChange; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarLargeChange), new object[] {this, value});
				else base.LargeChange = value;
			}
		}

		public new int Left
		{
			get { return base.Left; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlLeft), new object[] { this, value });
				else base.Left = value;
			}
		}

		public new System.Drawing.Point Location
		{
			get { return base.Location; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlLocation), new object[] {this, value});
				else base.Location = value;
			}
		}

		public new int Maximum
		{
			get { return base.Maximum; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarMaximum), new object[] {this, value});
				else base.Maximum = value;
			}
		}

		public new int Minimum
		{
			get { return base.Minimum; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarMinimum), new object[] {this, value});
				else base.Minimum = value;
			}
		}

		public new string Name
		{
			get { return base.Name; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlName), new object[] {this, value});
				else base.Name = value;
			}
		}

		public new Orientation Orientation
		{
			get { return base.Orientation; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarOrientation), new object[] {this, value});
				else base.Orientation = value;
			}
		}

		public new Control Parent
		{
			get { return base.Parent; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlParent), new object[] {this, value});
				else base.Parent = value;
			}
		}

		public new Region Region
		{
			get { return base.Region; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlRegion), new object[] {this, value});
				else base.Region = value;
			}
		}

		public new RightToLeft RightToLeft
		{
			get { return base.RightToLeft; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlRightToLeft), new object[] {this, value});
				else base.RightToLeft = value;
			}
		}

		public new ISite Site
		{
			get { return base.Site; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlSite), new object[] {this, value});
				else base.Site = value;
			}
		}

		public new System.Drawing.Size Size
		{
			get { return base.Size; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlSize), new object[] {this, value});
				else base.Size = value;
			}
		}

		public new int SmallChange
		{
			get { return base.SmallChange; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarSmallChange), new object[] {this, value});
				else base.SmallChange = value;
			}
		}

		public new int TabIndex
		{
			get { return base.TabIndex; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlTabIndex), new object[] {this, value});
				else base.TabIndex = value;
			}
		}

		public new bool TabStop
		{
			get { return base.TabStop; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlTabStop), new object[] {this, value});
				else base.TabStop = value;
			}
		}

		public new object Tag
		{
			get { return base.Tag; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlTag), new object[] {this, value});
				else base.Tag = value;
			}
		}

		public new int TickFrequency
		{
			get { return base.TickFrequency; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarTickFrequency), new object[] {this, value});
				else base.TickFrequency = value;
			}
		}

		public new TickStyle TickStyle
		{
			get { return base.TickStyle; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarTickStyle), new object[] {this, value});
				else base.TickStyle = value;
			}
		}

		public new int Top
		{
			get { return base.Top; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlTop), new object[] {this, value});
				else base.Top = value;
			}
		}

		public new int Value
		{
			get { return base.Value; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetTrackBarValue), new object[] {this, value});
				else base.Value = value;
			}
		}

		public new bool Visible
		{
			get { return base.Visible; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlVisible), new object[] {this, value});
				else base.Visible = value;
			}
		}

		public new int Width
		{
			get { return base.Width; }
			set
			{
				if(base.InvokeRequired)
					this.Invoke(new UI.SetCtrlDel(UI.SetControlWidth), new object[] {this, value});
				else base.Width = value;
			}
		}

		#endregion

		#region Functions

		public new void BringToFront()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.BringToFront));
			else base.BringToFront();
		}

		public new bool Contains(Control ctl)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlContains),
					new object[] {this, new object[] {ctl}});
				return (bool)this.EndInvoke(result);
			}
			else return base.Contains(ctl);
		}

		public new void CreateControl()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.CreateControl));
			else base.CreateControl();
		}

		public new Graphics CreateGraphics()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlCreateGraphics),
					new object[] { this, new object[] { null }});
				return (Graphics)this.EndInvoke(result);
			}
			else return base.CreateGraphics();
		}

		//public new virtual ObjRef CreateObjRef(Type requestedType)
		//{
		//	if(base.InvokeRequired)
		//	{
		//		IAsyncResult result = this.BeginInvoke(
		//			new UI.CtrlRetFunc(UI.CallMarshalByRefObjectCreateObjRef),
		//			new object[] { this, new object[] { requestedType }});
		//		return (ObjRef)this.EndInvoke(result);
		//	}
		//	else return base.CreateObjRef(requestedType);
		//}

		public new virtual void Dispose()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Dispose));
			else base.Dispose();
		}

		public new DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlDoDragDrop),
					new object[] { this, new object[] { data, allowedEffects }});
				return (DragDropEffects)this.EndInvoke(result);
			}
			else return base.DoDragDrop(data, allowedEffects);
		}

		public new virtual object Equals(object obj)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallObjectEquals),
					new object[] { this, new object[] {obj}});
				return this.EndInvoke(result);
			}
			else return base.Equals(obj);
		}

		public new Form FindForm()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlFindForm),
					new object[] { this, new object[] {null}});
				return (Form)this.EndInvoke(result);
			}
			else return base.FindForm();
		}

		public new bool Focus()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlFocus),
					new object[] { this, new object[] {null}});
				return (bool)this.EndInvoke(result);
			}
			else return base.Focus();
		}

		public new Control GetChildAtPoint(System.Drawing.Point pt)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlGetChildAtPoint),
					new object[] { this, new object[] {pt}});
				return (Control)this.EndInvoke(result);
			}
			else return base.GetChildAtPoint(pt);
		}

		public new IContainerControl GetContainerControl()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlGetContainerControl),
					new object[] { this, new object[] {}});
				return (IContainerControl)this.EndInvoke(result);
			}
			else return base.GetContainerControl();
		}
        
		public new virtual int GetHashCode()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallObjectGetHashCode),
					new object[] { this, new object[] {}});
				return (int)this.EndInvoke(result);
			}
			else return base.GetHashCode();
		}

		public new virtual object GetLifetimeService()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallMarshalByRefObjectGetLifetimeService),
					new object[] { this, new object[] {}});
				return this.EndInvoke(result);
			}
			else return base.GetLifetimeService();
		}

		public new Control GetNextControl(Control ctl, bool forward)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlGetNextControl),
					new object[] { this, new object[] { ctl, forward }});
				return (Control)this.EndInvoke(result);
			}
			else return base.GetNextControl(ctl, forward);
		}

		public new Type GetType()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallObjectGetType),
					new object[] { this, new object[] {}});
				return (Type)this.EndInvoke(result);
			}
			else return base.GetType();
		}

		public new void Hide()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Hide));
			else base.Hide();
		}

		public new virtual object InitializeLifetimeService()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallMarshalByRefObjectInitializeLifetimeService),
					new object[] { this, new object[] {}});
				return this.EndInvoke(result);
			}
			else return base.InitializeLifetimeService();
		}

		public new void Invalidate()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Invalidate));
			else base.Invalidate();
		}

		public new void Invalidate(bool invalidateChildren)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlInvalidate),
					new object[] { this, new object[] {invalidateChildren}});
			}
			else base.Invalidate(invalidateChildren);
		}

		public new void Invalidate(Rectangle rc)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlInvalidate),
					new object[] { this, new object[] {rc}});
			}
			else base.Invalidate(rc);
		}

		public new void Invalidate(Region region)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlInvalidate),
					new object[] { this, new object[] {region}});
			}
			else base.Invalidate(region);
		}

		public new void Invalidate(Rectangle rc, bool invalidateChildren)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlInvalidate),
					new object[] { this, new object[] { rc, invalidateChildren }});
			}
			else base.Invalidate(rc, invalidateChildren);
		}

		public new void Invalidate(Region region, bool invalidateChildren)
		{
			if(base.InvokeRequired)
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlInvalidate),
					new object[] { this, new object[] { region, invalidateChildren }});
			else base.Invalidate(region, invalidateChildren);
		}

		public new void PerformLayout()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.PerformLayout));
			else base.PerformLayout();
		}

		public new void PerformLayout(Control affectedControl, string affectedProperty)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlPerformLayout),
					new object[] { this, new object[] { affectedControl, affectedProperty }});
			}
			else base.PerformLayout(affectedControl, affectedProperty);
		}

		public new System.Drawing.Point PointToClient(System.Drawing.Point p)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlPointToClient),
					new object[] { this, new object[] {p}});
				return (System.Drawing.Point)this.EndInvoke(result);
			}
			else return base.PointToClient(p);
		}

		public new System.Drawing.Point PointToScreen(System.Drawing.Point p)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlPointToScreen),
					new object[] { this, new object[] {p}});
				return (System.Drawing.Point)this.EndInvoke(result);
			}
			else return base.PointToScreen(p);
		}

		public new virtual bool PreProcessMessage(ref Message msg)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlPreProcessMessage),
					new object[] { this, new object[] {msg}});
				return (bool)this.EndInvoke(result);
			}
			else return base.PreProcessMessage(ref msg);
		}

		public new Rectangle RectangleToClient(Rectangle r)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlRectangleToClient),
					new object[] { this, new object[] {r}});
				return (Rectangle)this.EndInvoke(result);
			}
			else return base.RectangleToClient(r);
		}

		public new Rectangle RectangleToScreen(Rectangle r)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlRectangleToScreen),
					new object[] { this, new object[] {r}});
				return (Rectangle)this.EndInvoke(result);
			}
			else return base.RectangleToScreen(r);
		}

		public new virtual void Refresh()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Refresh));
			else base.Refresh();
		}

		public new virtual void ResetBackColor()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetBackColor));
			else base.ResetBackColor();
		}

		public new void ResetBindings()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetBindings));
			else base.ResetBindings();
		}

		public new virtual void ResetCursor()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetCursor));
			else base.ResetCursor();
		}

		public new virtual void ResetFont()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetFont));
			else base.ResetFont();
		}

		public new virtual void ResetForeColor()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetForeColor));
			else base.ResetForeColor();
		}

		public new void ResetImeMode()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetImeMode));
			else base.ResetImeMode();
		}

		public new virtual void ResetRightToLeft()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetRightToLeft));
			else base.ResetRightToLeft();
		}

		public new virtual void ResetText()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResetText));
			else base.ResetText();
		}

		public new void ResumeLayout()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.ResumeLayout));
			else base.ResumeLayout();
		}

		public new void ResumeLayout(bool performLayout)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlResumeLayout),
					new object[] { this, new object[] {performLayout}});
			}
			else base.ResumeLayout(performLayout);
		}

		public new void Scale(SizeF ratio)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlScale),
					new object[] { this, new object[] {ratio}});
			}
			else base.Scale(ratio);
		}

		public new void Select()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Select));
			else base.Select();
		}

		public new bool SelectNextControl(Control ctl,
			bool forward, bool tabStopOnly,	bool nested, bool wrap)
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallControlSelectNextControl),
					new object[] { this,
									 new object[] { ctl, forward, tabStopOnly, nested, wrap }});
				return (bool)this.EndInvoke(result);
			}
			else return base.SelectNextControl(ctl, forward, tabStopOnly, nested, wrap);
		}

		public new void SendToBack()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.SendToBack));
			else base.SendToBack();
		}

		public new void SetBounds(int x, int y, int width, int height)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlSetBounds),
					new object[] { this, new object[] { x, y, width, height }});
			}
			else base.SetBounds(x, y, width, height);
		}

		public new void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallControlSetBounds),
					new object[] { this, 
									 new object[] { x, y, width, height, specified }});
			}
			else base.SetBounds(x, y, width, height, specified);
		}

		public new void SetRange(int minValue, int maxValue)
		{
			if(base.InvokeRequired)
			{
				this.Invoke(
					new UI.CtrlVoidFunc(UI.CallTrackBarSetRange),
					new object[] {this, new object[] { minValue, maxValue }});
			}
			else base.SetRange(minValue, maxValue);
		}

		public new void Show()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Show));
			else base.Show();
		}

		public new void SuspendLayout()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.SuspendLayout));
			else base.SuspendLayout();
		}

		public new virtual string ToString()
		{
			if(base.InvokeRequired)
			{
				IAsyncResult result = this.BeginInvoke(
					new UI.CtrlRetFunc(UI.CallObjectToString),
					new object[] { this, new object[] {}});
				return (string)this.EndInvoke(result);
			}
			else return base.ToString();
		}

		public new void Update()
		{
			if(base.InvokeRequired)
				this.Invoke(new MethodInvoker(base.Update));
			else base.Update();
		}

		#endregion
	}
}
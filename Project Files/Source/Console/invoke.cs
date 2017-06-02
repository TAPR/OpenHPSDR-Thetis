//=================================================================
// invoke.cs
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
//=================================================================

using System.Windows.Forms;
using System.Drawing;

namespace Thetis
{
	public class UI
	{		
		// Use of Invoke routines is required when accessing UI controls from
		// a thread other than the one that created them.

		public delegate void SetCtrlDel(Control c, object val);

		public static void SetPanel(Control c, object val)
		{
			Panel p = (Panel)c;
			Color clr = (Color)val;
			p.BackColor = clr;
		}
	}
}
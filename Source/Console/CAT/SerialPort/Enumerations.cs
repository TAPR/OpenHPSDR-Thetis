//=================================================================
// Enumerations.cs
//=================================================================
// Copyright (C) 2005 Phil Covington
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
// You may contact the author via email at: p.covington@gmail.com
//=================================================================

using System;

namespace SerialPorts
{	
	public enum Handshake
	{
		None,
		XOnXOff,
		RequestToSend,
		RequestToSendXOnXOff
	};

	public enum Parity  
	{
		None = 0,
		Odd = 1,
		Even = 2,
		Mark = 3,
		Space = 4
	};

	public enum StopBits 
	{		
		One = 0,
		Two = 1,
		OnePointFive = 2		
	};	

	[System.Flags] public enum SerialEvents 
	{
		None = 0x0,
		ReceivedChars = 0x00000001,
		CtsChanged = 0x00000002,
		DsrChanged = 0x00000004,
		CDChanged = 0x0000008,
		Ring = 0x00000010,
		Break = 0x00000020, 
		TxFull = 0x00000040,
		RxOver = 0x00000080,
		Overrun = 0x00000100, 
		RxParity = 0x00000200,
		Frame = 0x00000400, 
		EofReceived = 0x00000800,
		All = 0x00000fff
	}
}

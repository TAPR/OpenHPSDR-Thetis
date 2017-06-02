//=================================================================
// SafeHandle.cs
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
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace SerialPorts
{
	internal abstract class SafeHandle
	{
		private int inUse;  
		private int closed; 
		private int realHandle;   
		private const int InvalidHandle = -1;

		protected internal SafeHandle(IntPtr handle)
		{
			if (handle == (IntPtr) InvalidHandle)
				throw new ArgumentException("HandleProtector doesn't expect an invalid handle!");
			inUse = 1;
			closed = 0;
			realHandle = handle.ToInt32();    
		}

		internal IntPtr Handle 
		{
			get { return (IntPtr) realHandle; }
		}

		internal bool IsClosed 
		{
			get { return closed != 0; }
		}
		
		internal bool TryAddRef(ref bool incremented)
		{
			if (closed == 0) 
			{
				Interlocked.Increment(ref inUse);
				incremented = true;
				if (closed == 0)
					return true;
				Release();
				incremented = false;
			}
			return false;
		}

		internal void Release()
		{
			if (Interlocked.Decrement(ref inUse) == 0) 
			{
				int h = realHandle;
				if (h != InvalidHandle) 
				{
					if (h == Interlocked.CompareExchange(ref realHandle, InvalidHandle, h)) 
					{
						FreeHandle(new IntPtr(h));
					}
				}
			}
		}

		protected internal abstract void FreeHandle(IntPtr handle);

		internal void Close()
		{
			int c = closed;
			if (c != 1) 
			{
				if (c == Interlocked.CompareExchange(ref closed, 1, c)) 
				{
					Release();
				}
			}
		}

		internal void ForciblyMarkAsClosed()
		{
			closed = 1;
			realHandle = InvalidHandle;
		}

	}
}

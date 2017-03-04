//=================================================================
// Resources.cs
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
 

using System.IO;
using System.Security;
using System.Resources;
using System.Globalization;
using System.Collections;
using System.Security.Permissions;
using System.Text;
using System.Configuration.Assemblies;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.CompilerServices;

namespace SerialPorts
{
	public class Resources
	{		
		internal static ResourceManager SystemResMgr;
		internal static System.Object m_resMgrLockObject;
		internal static bool m_loadingResource;
		
		internal static System.String GetResourceString(System.String key)
		{
			if (SystemResMgr == null)
				InitResourceManager();
			System.String s;			
			lock(m_resMgrLockObject) 
			{
				if (m_loadingResource) 
				{
					return "[Resource lookup failed - infinite recursion detected.  Resource name: "+key+']';
				}
				m_loadingResource = true;
				s = SystemResMgr.GetString(key, null);
				m_loadingResource = false;
			}
			return s;
		}

		internal static System.String GetResourceString(System.String key, params System.Object[]values)
		{
			if (SystemResMgr == null)
				InitResourceManager();
			System.String s;			
			lock(m_resMgrLockObject) 
			{
				if (m_loadingResource)
					return "[Resource lookup failed - infinite recursion detected.  Resource name: "+key+']';
				m_loadingResource = true;
				s = SystemResMgr.GetString(key, null);
				m_loadingResource = false;
			}
			return System.String.Format(s, values);
		}

		private static ResourceManager InitResourceManager()
		{
			if (SystemResMgr == null) 
			{
				lock(typeof(System.Environment)) 
				{
					if (SystemResMgr == null) 
					{
						m_resMgrLockObject = new System.Object();
						SystemResMgr = new ResourceManager("mscorlib", typeof(System.String).Assembly);
					}
				}
			}
			return SystemResMgr;
		}	
	
		internal static void EndOfFile() 
		{
			throw new EndOfStreamException(GetResourceString("IO.EOF_ReadBeyondEOF"));
		}
    
		private static System.String GetMessage(int errorCode) 
		{
			StringBuilder sb = new StringBuilder(512);
			int result = Win32API_Serial.FormatMessage(FORMAT_MESSAGE_IGNORE_INSERTS |
				FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY,
				Win32API_Serial.NULL, errorCode, 0, sb, sb.Capacity, Win32API_Serial.NULL);
			if (result != 0) 
			{
				System.String s = sb.ToString();
				return s;
			}
			else 
			{
				return GetResourceString("IO_UnknownError", errorCode);
			}
		}

		internal static void FileNotOpen() 
		{
			throw new System.ObjectDisposedException(null, GetResourceString("ObjectDisposed_FileClosed"));
		}

		internal static void StreamIsClosed() 
		{
			throw new System.ObjectDisposedException(null, GetResourceString("ObjectDisposed_StreamClosed"));
		}
    
		internal static void MemoryStreamNotExpandable() 
		{
			throw new System.NotSupportedException(GetResourceString("NotSupported_MemStreamNotExpandable"));
		}
    
		internal static void ReaderClosed() 
		{
			throw new System.ObjectDisposedException(null, GetResourceString("ObjectDisposed_ReaderClosed"));
		}

		internal static void ReadNotSupported() 
		{
			throw new System.NotSupportedException(GetResourceString("NotSupported_UnreadableStream"));
		}
    
		internal static void SeekNotSupported() 
		{
			throw new System.NotSupportedException(GetResourceString("NotSupported_UnseekableStream"));
		}

		internal static void WrongAsyncResult() 
		{
			throw new System.ArgumentException(GetResourceString("Arg_WrongAsyncResult"));
		}

		internal static void EndReadCalledTwice() 
		{
			throw new System.InvalidOperationException(GetResourceString("InvalidOperation_EndReadCalledMultiple"));
		}

		internal static void EndWriteCalledTwice() 
		{
			throw new System.InvalidOperationException(GetResourceString("InvalidOperation_EndWriteCalledMultiple"));
		}

		internal static void WinIOError() 
		{
			int errorCode = Marshal.GetLastWin32Error();
			WinIOError(errorCode, System.String.Empty);
		}
    
		internal static void WinIOError(int errorCode, System.String str) 
		{
			switch (errorCode) 
			{
				case Win32API_Serial.ERROR_FILE_NOT_FOUND:
					if (str.Length == 0)
						throw new FileNotFoundException(GetResourceString("IO.FileNotFound"));
					else
						throw new FileNotFoundException(System.String.Format(GetResourceString("IO.FileNotFound_FileName"), str), str);
                
				case Win32API_Serial.ERROR_PATH_NOT_FOUND:
					if (str.Length == 0)
						throw new DirectoryNotFoundException(GetResourceString("IO.PathNotFound_NoPathName"));
					else
						throw new DirectoryNotFoundException(System.String.Format(GetResourceString("IO.PathNotFound_Path"), str));

				case Win32API_Serial.ERROR_ACCESS_DENIED:
					if (str.Length == 0)
						throw new System.UnauthorizedAccessException(GetResourceString("UnauthorizedAccess_IODenied_NoPathName"));
					else
						throw new System.UnauthorizedAccessException(System.String.Format(GetResourceString("UnauthorizedAccess_IODenied_Path"), str));

				case Win32API_Serial.ERROR_FILENAME_EXCED_RANGE:
					throw new PathTooLongException(GetResourceString("IO.PathTooLong"));

				case Win32API_Serial.ERROR_INVALID_PARAMETER:
					throw new IOException(GetMessage(errorCode), Win32API_Serial.MakeHRFromErrorCode(errorCode));

				case Win32API_Serial.ERROR_SHARING_VIOLATION:
					if (str.Length == 0)
						throw new IOException(GetResourceString("IO.IO_SharingViolation_NoFileName"));
					else
						throw new IOException(GetResourceString("IO.IO_SharingViolation_File", str));

				case Win32API_Serial.ERROR_FILE_EXISTS:
					if (str.Length == 0)
						goto default;
					throw new IOException(System.String.Format(GetResourceString("IO.IO_FileExists_Name"), str));

				default:
					throw new IOException(GetMessage(errorCode), Win32API_Serial.MakeHRFromErrorCode(errorCode));
			}
		}
    
		internal static void WriteNotSupported() 
		{
			throw new System.NotSupportedException(GetResourceString("NotSupported_UnwritableStream"));
		}

		internal static void WriterClosed() 
		{
			throw new System.ObjectDisposedException(null, GetResourceString("ObjectDisposed_WriterClosed"));
		}

		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
		private const int FORMAT_MESSAGE_FROM_SYSTEM    = 0x00001000;
		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;

		internal const int ERROR_FILE_NOT_FOUND = Win32API_Serial.ERROR_FILE_NOT_FOUND;
		internal const int ERROR_PATH_NOT_FOUND = Win32API_Serial.ERROR_PATH_NOT_FOUND;
		internal const int ERROR_ACCESS_DENIED  = Win32API_Serial.ERROR_ACCESS_DENIED;
		internal const int ERROR_INVALID_PARAMETER = Win32API_Serial.ERROR_INVALID_PARAMETER;
	}
}
	
	

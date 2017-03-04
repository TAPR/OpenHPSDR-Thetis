//=================================================================
// Win32API_Serial.cs
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
using System.Configuration.Assemblies;
using System.Runtime.Remoting;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Resources;
using System.Runtime;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Threading;

namespace SerialPorts
{
	internal sealed class Win32API_Serial
	{
		internal const int OPEN_EXISTING = 3;
		internal const int OPEN_ALWAYS = 4;
		internal const int FILE_FLAG_NOBUFFERING = 0x500000;
		internal const int FILE_FLAG_OVERLAPPED   = 0x40000000;
		internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
		
		internal const int FILE_TYPE_UNKNOWN = 0;
		internal const int FILE_TYPE_DISK = 1;
		internal const int FILE_TYPE_CHAR = 2;
		internal const int FILE_TYPE_PIPE = 3;

		internal const String KERNEL32 = "kernel32.dll";

		internal const int GENERIC_READ  = unchecked((int) 0x80000000);
		internal const int GENERIC_WRITE  = 0x40000000;
		
		internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		internal static readonly IntPtr NULL = IntPtr.Zero;
		
		internal const int ERROR_BROKEN_PIPE = 109;
		internal const int ERROR_NO_DATA = 232;
		internal const int ERROR_HANDLE_EOF = 38;
		internal const int ERROR_IO_PENDING = 997;
		internal const int ERROR_FILE_NOT_FOUND = 0x2;
		internal const int ERROR_PATH_NOT_FOUND = 0x3;
		internal const int ERROR_ACCESS_DENIED  = 0x5;
		internal const int ERROR_INVALID_HANDLE = 0x6;
		internal const int ERROR_SHARING_VIOLATION = 0x20;
		internal const int ERROR_FILE_EXISTS = 0x50;
		internal const int ERROR_INVALID_PARAMETER = 0x57;
		internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;  // filename too long.

		internal const byte ONESTOPBIT = 0;
		internal const byte ONE5STOPBITS = 1;
		internal const byte TWOSTOPBITS = 2;

		internal const int DTR_CONTROL_DISABLE = 0x00;
		internal const int DTR_CONTROL_ENABLE = 0x01;
		internal const int DTR_CONTROL_HANDSHAKE = 0x02;

		internal const int RTS_CONTROL_DISABLE = 0x00;
		internal const int RTS_CONTROL_ENABLE = 0x01;
		internal const int RTS_CONTROL_HANDSHAKE = 0x02;
		internal const int RTS_CONTROL_TOGGLE = 0x03;
		
		internal const int  MS_CTS_ON = 0x10;
		internal const int  MS_DSR_ON = 0x20;
		internal const int  MS_RING_ON = 0x40;
		internal const int  MS_RLSD_ON  = 0x80;

		internal const byte EOFCHAR = (byte) 26;

		internal const int FBINARY = 0;
		internal const int FPARITY = 1; 
		internal const int FOUTXCTSFLOW = 2; 
		internal const int FOUTXDSRFLOW = 3; 
		internal const int FDTRCONTROL = 4; 
		internal const int FDSRSENSITIVITY = 6; 
		internal const int FTXCONTINUEONXOFF = 7; 
		internal const int FOUTX = 8; 
		internal const int FINX = 9; 
		internal const int FERRORCHAR = 10; 
		internal const int FNULL = 11; 
		internal const int FRTSCONTROL = 12; 
		internal const int FABORTONOERROR = 14; 
		internal const int FDUMMY2 = 15; 

		internal const int PURGE_TXABORT     =  0x0001;  
		internal const int PURGE_RXABORT     =  0x0002;  
		internal const int PURGE_TXCLEAR     =  0x0004;  
		internal const int PURGE_RXCLEAR     =  0x0008;  

		internal const byte DEFAULTXONCHAR = (byte) 17;
		internal const byte DEFAULTXOFFCHAR = (byte) 19;
		
		internal const int CLRDTR = 6;
		
		internal const int EV_RXCHAR = 0x01;
		internal const int EV_RXFLAG = 0x02;
		internal const int EV_CTS = 0x08;
		internal const int EV_DSR = 0x10;
		internal const int EV_RLSD = 0x20;
		internal const int EV_BREAK = 0x40;
		internal const int EV_ERR = 0x80;
		internal const int EV_RING = 0x100;
		internal const int ALL_EVENTS = 0x1fb;	
		
		internal const int CE_RXOVER = 0x01;
		internal const int CE_OVERRUN = 0x02;
		internal const int CE_PARITY = 0x04;
		internal const int CE_FRAME = 0x08;
		internal const int CE_BREAK = 0x10;
		internal const int CE_TXFULL = 0x100;
		
		internal const int MAXDWORD = -1;	
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		internal static extern bool SetEvent(IntPtr eventHandle);

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		internal static extern int SleepEx(
			int dwMilliseconds,  
			bool bAlertable        
			);

	
		internal struct DCB 
		{			
			public uint DCBlength; 
			public uint BaudRate; 
			public uint Flags;
			public ushort wReserved; 
			public ushort XonLim; 
			public ushort XoffLim; 
			public byte ByteSize; 
			public byte Parity; 
			public byte StopBits; 
			public byte XonChar; 
			public byte XoffChar; 
			public byte ErrorChar; 
			public byte EofChar; 
			public byte EvtChar; 
			public ushort wReserved1; 
		}

		internal struct COMSTAT 
		{
			public uint Flags;
			public uint cbInQue;
			public uint cbOutQue;
		}

		internal struct COMMTIMEOUTS 
		{
			public int ReadIntervalTimeout; 
			public int ReadTotalTimeoutMultiplier; 
			public int ReadTotalTimeoutConstant; 
			public int WriteTotalTimeoutMultiplier; 
			public int WriteTotalTimeoutConstant; 
		}

		internal struct COMMPROP 
		{ 
			public ushort  wPacketLength; 
			public ushort  wPacketVersion; 
			public int dwServiceMask; 
			public int dwReserved1; 
			public int dwMaxTxQueue; 
			public int dwMaxRxQueue; 
			public int dwMaxBaud; 
			public int dwProvSubType; 
			public int dwProvCapabilities; 
			public int dwSettableParams; 
			public int dwSettableBaud; 
			public ushort wSettableData; 
			public ushort  wSettableStopParity; 
			public int dwCurrentTxQueue; 
			public int dwCurrentRxQueue; 
			public int dwProvSpec1; 
			public int dwProvSpec2; 
			public char wcProvChar;
		} 
		

		[StructLayout(LayoutKind.Sequential)]
			internal class SECURITY_ATTRIBUTES 
		{
			internal int nLength = 0;
			internal int lpSecurityDescriptor = 0;
			internal int bInheritHandle = 0;
		}
		

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern IntPtr CreateFile(String lpFileName,
			int dwDesiredAccess, int dwShareMode,
			IntPtr securityAttrs, int dwCreationDisposition,
			int dwFlagsAndAttributes, IntPtr hTemplateFile);
    
		[DllImport(Win32API_Serial.KERNEL32)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool GetCommState(
			IntPtr hFile,  
			ref DCB lpDCB    
			);	
	
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool SetCommState(
			IntPtr hFile,  
			ref DCB lpDCB    
			);		

	
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool GetCommModemStatus(
			IntPtr hFile,        
			ref int lpModemStat  
			);

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool SetupComm(
			IntPtr hFile,    
			int dwInQueue,  
			int dwOutQueue  
			);
	
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool SetCommTimeouts(
			IntPtr hFile,                  
			ref COMMTIMEOUTS lpCommTimeouts  
			);
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool GetCommTimeouts(
			IntPtr hFile,                  
			ref COMMTIMEOUTS lpCommTimeouts  
			);


		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool SetCommBreak(
			IntPtr hFile                 
			);

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool ClearCommBreak(
			IntPtr hFile                 
			);
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool ClearCommError(
			IntPtr hFile,                 
			ref int lpErrors,
			ref COMSTAT lpStat
			);
			
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool PurgeComm(
			IntPtr hFile,  
			uint dwFlags  
			);

	
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern bool GetCommProperties(
			IntPtr hFile,           
			ref COMMPROP lpCommProp   
			);


		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int ReadFile(IntPtr handle, byte* bytes, int numBytesToRead, out int numBytesRead, NativeOverlapped* overlapped);

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int ReadFile(IntPtr handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);
        
		[DllImport(Win32API_Serial.KERNEL32 , SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int WriteFile(IntPtr handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, NativeOverlapped* lpOverlapped);

		[DllImport(Win32API_Serial.KERNEL32 , SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		unsafe internal static extern int WriteFile(IntPtr handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten, NativeOverlapped* lpOverlapped);
		
		[DllImport(Win32API_Serial.KERNEL32 , SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		internal static extern int GetFileType(
			IntPtr hFile   
			);
		[DllImport(Win32API_Serial.KERNEL32 , SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]
		internal static extern bool EscapeCommFunction(
			IntPtr hFile, 
			int dwFunc		
			);
		
		[DllImport(Win32API_Serial.KERNEL32 , SetLastError=true), SuppressUnmanagedCodeSecurityAttribute]	
		unsafe internal static extern bool WaitCommEvent(
			IntPtr hFile,                
			ref int lpEvtMask,           
			NativeOverlapped* lpOverlapped    
			);
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		unsafe internal static extern IntPtr CreateEvent(
			IntPtr lpEventAttributes, 
			bool bManualReset,                       
			bool bInitialState,                      
			String lpName                           
			);
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]	
		unsafe internal static extern bool SetCommMask(
			IntPtr hFile,
			int dwEvtMask
		);
		
		
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]	
		internal static extern int WaitForSingleObject(
			IntPtr hHandle,        
			int dwMilliseconds   
			);

		[DllImport(Win32API_Serial.KERNEL32, CharSet=CharSet.Auto)]
		internal static extern int FormatMessage(int dwFlags, IntPtr lpSource,
			int dwMessageId, int dwLanguageId, StringBuilder lpBuffer,
			int nSize, IntPtr va_list_arguments);
		
		internal static int MakeHRFromErrorCode(int errorCode)
		{			
			return unchecked(((int)0x80070000) | errorCode);
		}

		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern int GetLastError();
	
		[DllImport(Win32API_Serial.KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
		internal static extern int GetTickCount();
		
	}
	
}

//=================================================================
// JustinIO.cs
//=================================================================
// Copyright (C) 2005  Bill Tracey
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
//=================================================================
// adapted from http://www.aciss.com/justin/io.htm 
//
// This is a modified version of JustinIO and has only been tested with 
// the virtual comm ports from MixW ComEmulDrv.  It probably will not work with an 
// actual hardware port as we pay no attention to baud rates, stop bits, parity etc. 
// 
// 
// Things to know about MixW CommEmulDrv 
//  -- overlapped IO apparently will not work 
//  -- events do not seem to be supported 
//  -- timeouts do not work 
//=================================================================

#define DBG_PRINT

using System;
using System.Threading; 
using System.Runtime.InteropServices;


namespace ModifiedJustinIO {
	class CommPort {

		public int PortNum; 
		public int BaudRate;
		public byte ByteSize;
		public byte Parity; // 0-4=no,odd,even,mark,space 
		public byte StopBits; // 0,1,2 = 1, 1.5, 2 
//		private int ReadTimeout;
		public bool isVirtualPort = false;
		
		//comm port win32 file handle
		private int hComm = -1;
		
		public bool Opened = false;
		 
		//win32 api constants
		private const uint GENERIC_READ = 0x80000000;
		private const uint GENERIC_WRITE = 0x40000000;
		private const int OPEN_EXISTING = 3;		
		private const int INVALID_HANDLE_VALUE = -1;

		// bit banger constants for the EscapeCommFunction 
		private const uint CLRBREAK = 9;
		private const uint CLRDTR = 6;
		private const uint CLRRTS = 4;
		private const uint RESETDEV = 7;
		private const uint SETBREAK = 8;
		private const uint SETDTR = 5;
		private const uint SETRTS = 3;
		private const uint SETXOFF = 1;
		private const uint SETXON = 2;

		// bit defs for modem status 
		private const uint CTS_ON = 0x0010;
		private const uint DSR_ON = 0x0020;
		private const uint RI_ON = 0x0040;
		private const uint RLSD_ON = 0x0080;
		
		[StructLayout(LayoutKind.Sequential)]
		public struct DCB {
			//taken from c struct in platform sdk 
			public int DCBlength;           // sizeof(DCB) 
			public int BaudRate;            // current baud rate
			/* these are the c struct bit fields, bit twiddle flag to set
			public int fBinary;          // binary mode, no EOF check 
			public int fParity;          // enable parity checking 
			public int fOutxCtsFlow;      // CTS output flow control 
			public int fOutxDsrFlow;      // DSR output flow control 
			public int fDtrControl;       // DTR flow control type 
			public int fDsrSensitivity;   // DSR sensitivity 
			public int fTXContinueOnXoff; // XOFF continues Tx 
			public int fOutX;          // XON/XOFF out flow control 
			public int fInX;           // XON/XOFF in flow control 
			public int fErrorChar;     // enable error replacement 
			public int fNull;          // enable null stripping 
			public int fRtsControl;     // RTS flow control 
			public int fAbortOnError;   // abort on error 
			public int fDummy2;        // reserved 
			*/
			public uint flags;
			public ushort wReserved;          // not currently used 
			public ushort XonLim;             // transmit XON threshold 
			public ushort XoffLim;            // transmit XOFF threshold 
			public byte ByteSize;           // number of bits/byte, 4-8 
			public byte Parity;             // 0-4=no,odd,even,mark,space 
			public byte StopBits;           // 0,1,2 = 1, 1.5, 2 
			public char XonChar;            // Tx and Rx XON character 
			public char XoffChar;           // Tx and Rx XOFF character 
			public char ErrorChar;          // error replacement character 
			public char EofChar;            // end of input character 
			public char EvtChar;            // received event character 
			public ushort wReserved1;         // reserved; do not use 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SERIAL_STATUS
		{
			public UInt32 Errors;
			public UInt32 HoldReasons; 
			public UInt32 AmountInInQueue; 
			public UInt32 AmountInOutQueue; 
			public Byte EofReceived; 
			public Byte WaitForImmediate; 
		}
		private const uint SIZE_SERIAL_STATUS = 18; 


//// wjt added 
//		// some handy defines from the ddk 
//#define IOCTL_SERIAL_GET_COMMSTATUS     CTL_CODE(FILE_DEVICE_SERIAL_PORT,27,METHOD_BUFFERED,FILE_ANY_ACCESS)
//		//
//		// This structure is used to get the current error and
//		// general status of the driver.
//		//
//
//		typedef struct _SERIAL_STATUS 
//				{
//					ULONG Errors;
//					ULONG HoldReasons;
//					ULONG AmountInInQueue;
//					ULONG AmountInOutQueue;
//					BOOLEAN EofReceived;
//					BOOLEAN WaitForImmediate;
//				} SERIAL_STATUS,*PSERIAL_STATUS;
//
////
//// This structure is used to get the current error and
//// general status of the driver.
////
//
//#define CTL_CODE( DeviceType, Function, Method, Access ) (                 \
//		((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method) \
//		)
//#define FILE_DEVICE_SERIAL_PORT         0x0000001b
//#define METHOD_BUFFERED                 0
//#define FILE_ANY_ACCESS                 0
//		typedef UCHAR BOOLEAN;           // winnt



		private const uint FILE_DEVICE_SERIAL_PORT = 0x1b; 
		private const uint FILE_ANY_ACCESS = 0; 
		private const uint METHOD_BUFFERED = 0; 
		private const uint FUNCTION_GET_COMMSTATUS = 27; 

		private const uint IOCTL_SERIAL_GET_COMMSTATUS =  (FILE_DEVICE_SERIAL_PORT << 16) | 
			               (FILE_ANY_ACCESS << 14)  | (FUNCTION_GET_COMMSTATUS << 2) | METHOD_BUFFERED; 


		[StructLayout(LayoutKind.Sequential)]
		private struct COMMTIMEOUTS {  
		  public int ReadIntervalTimeout; 
		  public int ReadTotalTimeoutMultiplier; 
		  public int ReadTotalTimeoutConstant; 
		  public int WriteTotalTimeoutMultiplier; 
		  public int WriteTotalTimeoutConstant; 
		} 	

		[StructLayout(LayoutKind.Sequential)]	
		private struct OVERLAPPED { 
		    public int  Internal; 
		    public int  InternalHigh; 
		    public int  Offset; 
		    public int  OffsetHigh; 
		    public int hEvent; 
		}  
		
		[DllImport("kernel32.dll")]
		private static extern int CreateFile(
		  string lpFileName,                         // file name
		  uint dwDesiredAccess,                      // access mode
		  int dwShareMode,                          // share mode
		  int lpSecurityAttributes,					// SD
		  int dwCreationDisposition,                // how to create
		  int dwFlagsAndAttributes,                 // file attributes
		  int hTemplateFile                        // handle to template file
		);
		[DllImport("kernel32.dll")]
		private static extern bool GetCommState(
		  int hFile,  // handle to communications device
		  ref DCB lpDCB    // device-control block
		);	
		[DllImport("kernel32.dll")]
		private static extern bool BuildCommDCB(
		  string lpDef,  // device-control string
		  ref DCB lpDCB     // device-control block
		);
		[DllImport("kernel32.dll")]
		private static extern bool SetCommState(
		  int hFile,  // handle to communications device
		  ref DCB lpDCB    // device-control block
		);
		[DllImport("kernel32.dll")]
		private static extern bool GetCommTimeouts(
		  int hFile,                  // handle to comm device
		  ref COMMTIMEOUTS lpCommTimeouts  // time-out values
		);	
		[DllImport("kernel32.dll")]	
		private static extern bool SetCommTimeouts(
		  int hFile,                  // handle to comm device
		  ref COMMTIMEOUTS lpCommTimeouts  // time-out values
		);
		[DllImport("kernel32.dll")]
		private static extern bool ReadFile(
		  int hFile,                // handle to file
		  byte[] lpBuffer,             // data buffer
		  int nNumberOfBytesToRead,  // number of bytes to read
		  ref int lpNumberOfBytesRead, // number of bytes read
		  IntPtr lpOverlapped    // overlapped buffer
		);
		[DllImport("kernel32.dll")]	
		private static extern bool WriteFile(
		  int hFile,                    // handle to file
		  byte[] lpBuffer,                // data buffer
		  int nNumberOfBytesToWrite,     // number of bytes to write
		  ref int lpNumberOfBytesWritten,  // number of bytes written
		  IntPtr lpOverlapped        // overlapped buffer
		);
		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(
		  int hObject   // handle to object
		);
		[DllImport("kernel32.dll")]
		private static extern uint GetLastError();

		[DllImport("kernel32.dll")]
		private static extern Boolean GetCommModemStatus
			(
			int hFile,
		    ref uint lpModemStat
			);


		[DllImport("kernel32.dll")]
		private static extern Boolean EscapeCommFunction
			(
			int hFile,
			uint dwFunc
			);

		//		BOOL DeviceIoControl(
		//			HANDLE hDevice,
		//			DWORD dwIoControlCode,
		//			LPVOID lpInBuffer,
		//			DWORD nInBufferSize,
		//			LPVOID lpOutBuffer,
		//			DWORD nOutBufferSize,
		//			LPDWORD lpBytesReturned,
		//			LPOVERLAPPED lpOverlapped
		//			);
		[DllImport("kernel32.dll")]
		private static extern Boolean DeviceIoControl
			(
			int hfile, 
			UInt32 IOctlcode, 
			ref SERIAL_STATUS inbuf,          // fixme ... should not be specific 
			UInt32 inbufsize, 
			ref SERIAL_STATUS outbuf, // fixme .... this should not be specific 
			UInt32 outbufsize,
			ref UInt32 return_count, 
			IntPtr overlap
		    ); 

			

//		private long StartTicks = 0;  // Ticks are 100's of ns since 1/1/0001

		private void dbgWriteLine(string s) 
		{ 
			return;
//			if ( !PowerSDR.Console.dbgPrint ) return;  
//			
//			long now = System.DateTime.Now.Ticks; 			
//			long new_last = now; 
//			if ( StartTicks == 0 ) 
//			{ 				
//				StartTicks = now;
//			} 						
//			now -= StartTicks; 
//			now *= 100; // 100's of nsec to nsecs 
//			now = now / 1000000; // convert to mllis 
//			System.Console.WriteLine("jio: [" + now + "] " + s); 			
//			return; 
		} 

	

		public void Open() 
		{
		 
		 DCB dcbCommPort = new DCB();
		 COMMTIMEOUTS ctoCommPort = new COMMTIMEOUTS();	
		 
		 
		  // OPEN THE COMM PORT.
		  
			// see http://support.microsoft.com/default.aspx?scid=kb;%5BLN%5D;115831 for the reasons for 
			// the bizarre comm port syntax 
		  hComm = CreateFile("\\\\.\\COM" + PortNum ,GENERIC_READ | GENERIC_WRITE,0, 0,OPEN_EXISTING,0,0);
		
		  // IF THE PORT CANNOT BE OPENED, BAIL OUT.
		  if(hComm == INVALID_HANDLE_VALUE) {
		  		throw(new ApplicationException("Comm Port \"COM" + PortNum + "\"  Can Not Be Opened"));
		  }
		
		  // SET THE COMM TIMEOUTS.
			
		  GetCommTimeouts(hComm,ref ctoCommPort);


	      ctoCommPort.ReadIntervalTimeout = -1; 
		  ctoCommPort.ReadTotalTimeoutConstant = 0; /* ReadTimeout */ 
		  ctoCommPort.ReadTotalTimeoutMultiplier = 0;
	      ctoCommPort.WriteTotalTimeoutConstant = 50; 
		  ctoCommPort.WriteTotalTimeoutMultiplier = 35; // @ 300 bps, about 27 msecs per char 
		  if ( SetCommTimeouts(hComm,ref ctoCommPort) == false ) 
		  {
				Console.WriteLine("SetCommTO failed\n"); 
		  }
	      
		
		  // SET BAUD RATE, PARITY, WORD SIZE, AND STOP BITS.
		  GetCommState(hComm, ref dcbCommPort);
		  dcbCommPort.BaudRate=BaudRate;
		  dcbCommPort.flags=0;
		  // dcbCommPort.fBinary=1;
		  dcbCommPort.flags|=1;

		  if (Parity>0)
		  {
		 	  //dcb.fParity=1
		      dcbCommPort.flags|=2;
		  }
			
		  dcbCommPort.Parity=Parity;
		  dcbCommPort.ByteSize=ByteSize;
		  dcbCommPort.StopBits=StopBits;



		  if (!SetCommState(hComm, ref dcbCommPort))
		  {
			 uint ErrorNum=GetLastError();
		     throw(new ApplicationException("Comm Port COM" + PortNum + " Can Not Be Opened.  Error=" + ErrorNum + "."));
		  }
		  //unremark to see if setting took correctly
		  //DCB dcbCommPort2 = new DCB();
		  //GetCommState(hComm, ref dcbCommPort2);
		  Opened = true;
		  
		}
		
		public void Close() {
			if (hComm!=INVALID_HANDLE_VALUE) {
				// lock ( this ) 
				{ 
					CloseHandle(hComm);
				}
			}
		}

		// convert an array of bytes to a string -- assuming bytes are ascii char
		// fixme ... surely there's a libarary routine to do this!
		private static string bytesToString(byte[] b, uint n)
		{
			if ( n == 0 ) return "";

			char[] cbuf = new char[n];
			for ( int i = 0; i < n; i++ )
			{
				cbuf[i] = (char)b[i];
			}
			return new string(cbuf, 0, (int)n);
		}

		
//		private void whackTimeouts() 
//		{
//			// SET THE COMM TIMEOUTS.
//			COMMTIMEOUTS ctoCommPort = new COMMTIMEOUTS(); 		
//			// GetCommTimeouts(hComm,ref ctoCommPort);
//
//			//		  ctoCommPort.ReadTotalTimeoutConstant = ReadTimeout;
//			//		  ctoCommPort.ReadTotalTimeoutMultiplier = 0;
//
//			ctoCommPort.ReadIntervalTimeout = UInt32.MaxValue;
//			ctoCommPort.ReadTotalTimeoutMultiplier = 0; 
//			ctoCommPort.ReadTotalTimeoutConstant = 0;
//			ctoCommPort.WriteTotalTimeoutMultiplier = 0;
//			ctoCommPort.WriteTotalTimeoutConstant = 0;  
//			bool temp = SetCommTimeouts(hComm,ref ctoCommPort);
//			Console.WriteLine("SetCommTimeOuts: " + temp); 
//			COMMTIMEOUTS cto =  new COMMTIMEOUTS(); 
//			temp = GetCommTimeouts(hComm, ref cto); 
//			Console.WriteLine("GetCommTimeOuts: " + temp); 
//			Console.WriteLine("cto: Interval: " + cto.ReadIntervalTimeout + " Mult: " + 
//					           cto.ReadTotalTimeoutMultiplier + " Const: " + cto.ReadTotalTimeoutConstant); 
//		}


//		public byte[] Read(int NumBytes) {
//			byte[] BufBytes;
//			byte[] OutBytes;
//			BufBytes = new byte[NumBytes];
//			if (hComm!=INVALID_HANDLE_VALUE) {
//				OVERLAPPED ovlCommPort = new OVERLAPPED();
//				int BytesRead=0;
//				ReadFile(hComm,BufBytes,NumBytes,ref BytesRead,ref ovlCommPort);
//				OutBytes = new byte[BytesRead];
//				Array.Copy(BufBytes,OutBytes,BytesRead);
//			} 
//			else {
//				throw(new ApplicationException("Comm Port Not Open"));
//			}
//			return OutBytes;
//		}


		// returns count of chars available to be read ... sort of a hack as this is using an ioctl
		// that is not in the documentation but does appear in the dd source 
		// this is needed when using the MixW CommEmulDrv virt serial ports  becuse it does not respect 
		// the SetCommTimeout values, so a read with no data avail will block forever 
		// 
		private uint checkIfCharsAvail() 
		{
			UInt32 returned_count = SIZE_SERIAL_STATUS; 
			if ( !Opened ) return 0; 
			SERIAL_STATUS ss = new SERIAL_STATUS(); 		
			// Console.WriteLine("Calling ioctl...."); 
			// Thread.Sleep(1000); 
			Boolean result = DeviceIoControl(hComm, IOCTL_SERIAL_GET_COMMSTATUS, 
				ref ss, SIZE_SERIAL_STATUS, ref ss, SIZE_SERIAL_STATUS, ref returned_count, (IntPtr)0); 
			// Console.WriteLine("....ioctl returned"); 
			// Thread.Sleep(1000); 
			if ( result == false ) 
			{
				Console.WriteLine("JustinIO: IOControl failed!"); 
				return 0;													  
			}
			return ss.AmountInInQueue; 
		}

		// read chars from serial port, returns number of chars 
		// this routine will not block waiting for chars, if there are none avail it will 
		// return 0 immediately 
		// 
		public int Read(byte[] buf_to_fill, int max_size) 
		{ 
			if ( hComm == INVALID_HANDLE_VALUE ) return -1;  // bail if we don't have handle; 
			if ( !Opened ) return -1; // or if we're not open 
			int numread = 0; 
			// lock ( this ) 
			{ 
				
				// whackTimeouts(); 
				uint avail = 1; 
				if ( isVirtualPort )  // if we're using a mixw port, need to check if chars avail 
					              //  because timeouts don't work on it. 
				{ 
					avail = checkIfCharsAvail(); 
				}				
				if ( avail > 0 ) 
				{ 
					dbgWriteLine("ReadStart"); 
					ReadFile(hComm, buf_to_fill, max_size, ref numread, (IntPtr)0); 
					dbgWriteLine("Read " + numread + " >" + bytesToString(buf_to_fill, (uint)numread) + "<" );
				}
			}
			return numread; 
		}
		
//		public void Write(byte[] WriteBytes) {
//			if (hComm!=INVALID_HANDLE_VALUE) {
//				OVERLAPPED ovlCommPort = new OVERLAPPED();
//				int BytesWritten = 0;
//				WriteFile(hComm,WriteBytes,WriteBytes.Length,ref BytesWritten,ref ovlCommPort);
//			}
//			else {
//				throw(new ApplicationException("Comm Port Not Open"));
//			}		
//		}

		// returns number of bytes written 
		//
		public int Write(byte[] b, int count) 
		{
			if ( hComm == INVALID_HANDLE_VALUE ) return 0; //fixme .. error handling? 
			if ( !Opened ) return 0; // fixme ... real error handling -- exception? 
			int num_written = 0; 
			// lock ( this ) 
			{ 
				try 
				{ 
					dbgWriteLine("Write start: count=" + count); 
					WriteFile(hComm, b, count, ref num_written, (IntPtr)0); 
					dbgWriteLine("Wrote " + num_written + "/" + count + " >" + bytesToString(b, (uint)count) + "<" );
				}
				catch ( ThreadInterruptedException ) 
				{
					Console.WriteLine("jio: caught interrupted exception"); 
				}

			}
			return num_written; 
		}

		// routines to bit bang RTS and DTR 
		public void setRTS(bool on) 
		{
			if ( !Opened ) return; // fixme -- throw exception 
			uint bits; 
			if ( on ) bits = SETRTS; 
			else bits = CLRRTS; 
			lock ( this ) 
			{ 
				EscapeCommFunction(hComm, bits); // fixme -- error checking needed 
			}
		}

		public void setDTR(bool on) 
		{
			if ( !Opened ) return; // fixme -- throw exception? 
			uint bits; 
			if ( on ) bits = SETDTR; 
			else bits = CLRDTR; 
			lock ( this ) 
			{ 
				EscapeCommFunction(hComm, bits); // fixme -- error checking 
			}
		}


		// routines to look @ the modmem status bits 

		private bool isModemBitOn(uint which_bit) 
		{ 
			if ( !Opened ) return false; // fixme -- throw exception ? 
			lock ( this ) 
			{ 
				uint modem_bits = 0; 
				// Console.WriteLine("calling modem status"); 
				if ( GetCommModemStatus(hComm, ref modem_bits) == false )  // fixme -- need real error checking -- throw exception? 
				{	
					return false; 
				}
				// Console.WriteLine("modem status returned"); 
				if ( (modem_bits & which_bit) == 0 ) return false; 
				else return true; 
			}
		}

		public bool isCTS() 
		{ 
			return isModemBitOn(CTS_ON); 
		}

		public bool isDSR() 
		{
			return isModemBitOn(DSR_ON); 
			
		}
		public bool isRI()
		{
			return isModemBitOn(RI_ON); 
		}
		public bool isRLSD() 
		{
			return isModemBitOn(RLSD_ON); 
		}
	}

	class HexCon {
	//converter hex string to byte and byte to hex string
		public static string ByteToString(byte[] InBytes) {
			string StringOut="";
			foreach (byte InByte in InBytes) {
				StringOut=StringOut + String.Format("{0:X2} ",InByte);
			}
			return StringOut; 
		}
		public static byte[] StringToByte(string InString) {
			string[] ByteStrings;
			ByteStrings = InString.Split(" ".ToCharArray());
			byte[] ByteOut;
			ByteOut = new byte[ByteStrings.Length-1];
			for (int i = 0;i==ByteStrings.Length-1;i++) {
				ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
			} 
			return ByteOut;
		}
	}
}
//=================================================================
// SerialStream.cs
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
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Resources;
using System.Runtime;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Data;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;


namespace SerialPorts
{
	
	public delegate void SerialEventHandler(object source, SerialEventArgs e);
	internal delegate int WaitEventCallback();
	internal class SerialStream : Stream 
	{
		private string portName;
		private byte parityReplace = (byte) '?';
		private bool dtrEnable;
		private bool rtsEnable;
		private bool inBreak = false;				
		private Handshake handshake;
		
		private Win32API_Serial.DCB dcb;
		private Win32API_Serial.COMMTIMEOUTS commTimeouts;
		private Win32API_Serial.COMSTAT comStat;
		private Win32API_Serial.COMMPROP commProp;
		
		private const long dsrTimeout = 0L;
		private const int maxDataBits = 8;
		private const int minDataBits = 5;
		private SafeHandle _safeHandle;  
		internal bool lastOpTimedOut = false;	
		private byte[] tempBuf;					
		
		private WaitEventCallback myWaitCommCallback;
		private AsyncCallback myAsyncCallback;
		private Object state;
	
		private unsafe static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(SerialStream.AsyncFSCallback);
			
		internal event SerialEventHandler ReceivedEvent;	
		internal event SerialEventHandler PinChangedEvent; 
		internal event SerialEventHandler ErrorEvent;		

		public override bool CanRead
		{
			get { return (!_safeHandle.IsClosed); }
		}
		
		public override bool CanSeek
		{	
			get { return false; }
		}
		
		public override bool CanWrite
		{	
			get { return (!_safeHandle.IsClosed); }
		}

		public override long Length
		{
			get { throw new NotSupportedException(Resources.GetResourceString("NotSupported_UnseekableStream")); }
		}		
		
		public override long Position
		{
			get { throw new NotSupportedException(Resources.GetResourceString("NotSupported_UnseekableStream")); }
			set { throw new NotSupportedException(Resources.GetResourceString("NotSupported_UnseekableStream")); }		
		}

		internal int BaudRate 
		{ 
			get { return (int) dcb.BaudRate; }
			set 
			{
				if (value <= 0 || (value > commProp.dwMaxBaud && commProp.dwMaxBaud > 0)) 
				{
					if (commProp.dwMaxBaud == 0) 
					{
						throw new ArgumentOutOfRangeException("baudRate", 
							Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
					} 
					else 
					{
						throw new ArgumentOutOfRangeException("baudRate", 
							Resources.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", 0, commProp.dwMaxBaud));
					}
				}
				if(value != dcb.BaudRate) 
				{
					int baudRateOld = (int) dcb.BaudRate;
					dcb.BaudRate = (uint) value;	
					
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						dcb.BaudRate = (uint) baudRateOld;
						Resources.WinIOError();
					}
				}
			}
		}

		internal int DataBits 
		{ 
			get  { return (int) dcb.ByteSize; } 
			set 
			{	
				if(value < minDataBits || value > maxDataBits) 
				{
					throw new ArgumentOutOfRangeException("dataBits", 
						Resources.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", minDataBits, maxDataBits));			
				}
				if (value != dcb.ByteSize) 
				{
					byte byteSizeOld = dcb.ByteSize;
					dcb.ByteSize = (byte) value;	
					
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						dcb.ByteSize = byteSizeOld;
						Resources.WinIOError();
					}		
				}	
			}
		}

		
		internal bool DiscardNull
		{ 
			get {	return (GetDcbFlag(Win32API_Serial.FNULL) == 1);}
			set 
			{
				int fNullFlag = GetDcbFlag(Win32API_Serial.FNULL);
				if(value == true && fNullFlag == 0 || value == false && fNullFlag == 1) 
				{
					int fNullOld = fNullFlag;
					SetDcbFlag(Win32API_Serial.FNULL, value ? 1 : 0);
			
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						SetDcbFlag(Win32API_Serial.FNULL, fNullOld);
						Resources.WinIOError();
					} 								
				}
			}
		}
		
		internal bool DtrEnable 
		{ 
			get	{	return dtrEnable; } 
			set 
			{  
				if(value != dtrEnable) 
				{
					bool dtrEnableOld = dtrEnable;
					int fDtrControlOld = GetDcbFlag(Win32API_Serial.FDTRCONTROL);

					dtrEnable = value;	
					SetDcbFlag(Win32API_Serial.FDTRCONTROL, dtrEnable ? 1 : 0);

					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						dtrEnable = dtrEnableOld;
						SetDcbFlag(Win32API_Serial.FDTRCONTROL, fDtrControlOld);
						Resources.WinIOError();
					} 							
				}
			} 
		}

		internal Handshake Handshake 
		{
			get  { return handshake; } 
			set  
			{ 
			
				if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff) 
					throw new ArgumentOutOfRangeException("handshake", Resources.GetResourceString("ArgumentOutOfRange_Enum"));
				
				if(value != handshake) 
				{
					Handshake handshakeOld = handshake;
					int fInOutXOld = GetDcbFlag(Win32API_Serial.FINX);
					int fOutxCtsFlowOld = GetDcbFlag(Win32API_Serial.FOUTXCTSFLOW);
					int fRtsControlOld = GetDcbFlag(Win32API_Serial.FRTSCONTROL);
					
					handshake = value;
					int fInXOutXFlag = (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0;					
					SetDcbFlag(Win32API_Serial.FINX, fInXOutXFlag);
					SetDcbFlag(Win32API_Serial.FOUTX, fInXOutXFlag);

					SetDcbFlag(Win32API_Serial.FOUTXCTSFLOW, (handshake == Handshake.RequestToSend ||
						handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

					if ((handshake == Handshake.RequestToSend ||
						handshake == Handshake.RequestToSendXOnXOff)) 
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_HANDSHAKE);
					}
					else if(rtsEnable)
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_ENABLE);
					}
					else
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_DISABLE);
					}

					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						handshake = handshakeOld;
						SetDcbFlag(Win32API_Serial.FINX, fInOutXOld);
						SetDcbFlag(Win32API_Serial.FOUTX, fInOutXOld);
						SetDcbFlag(Win32API_Serial.FOUTXCTSFLOW, fOutxCtsFlowOld);
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, fRtsControlOld);
						Resources.WinIOError();
					}
					
				}	
			}		
		}
	
		internal Parity Parity 
		{ 
			get 	{	return (Parity) dcb.Parity; 	} 
			set 
			{ 
				if(value < Parity.None || value > Parity.Space)
					throw new ArgumentOutOfRangeException("parity", Resources.GetResourceString("ArgumentOutOfRange_Enum"));;
				
				if((byte) value != dcb.Parity) 
				{
					byte parityOld = dcb.Parity;
					
					int fParityOld = GetDcbFlag(Win32API_Serial.FPARITY);
					byte ErrorCharOld = dcb.ErrorChar;
					int fErrorCharOld = GetDcbFlag(Win32API_Serial.FPARITY);
					dcb.Parity = (byte) value;	

					int parityFlag = (dcb.Parity == (byte) Parity.None) ? 1 : 0;
					SetDcbFlag(Win32API_Serial.FPARITY, parityFlag);
					if (parityFlag == 1) 
					{
						SetDcbFlag(Win32API_Serial.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
						dcb.ErrorChar = parityReplace;
					} 
					else 
					{
						SetDcbFlag(Win32API_Serial.FERRORCHAR, 0);
						dcb.ErrorChar = (byte) '\0';
					}
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						dcb.Parity = parityOld;
						SetDcbFlag(Win32API_Serial.FPARITY, fParityOld);
				
						dcb.ErrorChar = ErrorCharOld;
						SetDcbFlag(Win32API_Serial.FERRORCHAR, fErrorCharOld);
				
						Resources.WinIOError();
					}								
				}	
			}
		} 

		internal byte ParityReplace 
		{ 
			get {	return parityReplace; } 
			set 
			{ 
				if(value != parityReplace) 
				{
					byte parityReplaceOld = parityReplace;
					byte errorCharOld = dcb.ErrorChar;
					int fErrorCharOld = GetDcbFlag(Win32API_Serial.FERRORCHAR);

					parityReplace = value;	
					if (GetDcbFlag(Win32API_Serial.FPARITY) == 1)
					{
						SetDcbFlag(Win32API_Serial.FERRORCHAR, (parityReplace != '\0')? 1 : 0);
						dcb.ErrorChar = parityReplace;
					}
					else
					{
						SetDcbFlag(Win32API_Serial.FERRORCHAR, 0);
						dcb.ErrorChar = (byte) '\0';
					}
								
				
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						parityReplace = parityReplaceOld;
						SetDcbFlag(Win32API_Serial.FERRORCHAR, fErrorCharOld);
						dcb.ErrorChar = errorCharOld;
						Resources.WinIOError();
					} 							
				}
			} 
		}


		internal int ReadTimeout  
		{ 
			get 
			{	
				int constant = commTimeouts.ReadTotalTimeoutConstant; 				
				
				if (constant == Int32.MaxValue) return SerialPort.InfiniteTimeout;
				else return constant;
				
				
			} 
			set 
			{ 	
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException("readTimeout", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				int oldReadConstant = commTimeouts.ReadTotalTimeoutConstant;
				int oldReadInterval = commTimeouts.ReadIntervalTimeout;
				int oldReadMultipler = commTimeouts.ReadTotalTimeoutMultiplier;
				
				if (value == 0) {
					commTimeouts.ReadTotalTimeoutConstant = commTimeouts.ReadTotalTimeoutMultiplier = 0;
					commTimeouts.ReadIntervalTimeout = Win32API_Serial.MAXDWORD;
				} else if (value == SerialPort.InfiniteTimeout) {
					
					commTimeouts.ReadTotalTimeoutConstant = Int32.MaxValue;
					commTimeouts.ReadTotalTimeoutMultiplier = commTimeouts.ReadIntervalTimeout = Win32API_Serial.MAXDWORD;
				} else {
					commTimeouts.ReadTotalTimeoutConstant = value;
					commTimeouts.ReadTotalTimeoutMultiplier = commTimeouts.ReadIntervalTimeout = Win32API_Serial.MAXDWORD;
				}
				if (Win32API_Serial.SetCommTimeouts(_safeHandle.Handle, ref commTimeouts) == false) 
				{
					commTimeouts.ReadTotalTimeoutConstant = oldReadConstant;
					commTimeouts.ReadTotalTimeoutMultiplier = oldReadMultipler;
					commTimeouts.ReadIntervalTimeout = oldReadInterval;
					Resources.WinIOError();
				}
			} 
		}

		internal bool RtsEnable
		{ 
			get { return rtsEnable; } 
			set 
			{ 
				if(value != rtsEnable) 
				{
					bool rtsEnableOld = rtsEnable;
					int fRtsControlOld = GetDcbFlag(Win32API_Serial.FRTSCONTROL);

					rtsEnable = value;
					if ((handshake == Handshake.RequestToSend ||
						handshake == Handshake.RequestToSendXOnXOff)) 
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_HANDSHAKE);
					}
					else if(rtsEnable)
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_ENABLE);
					}
					else
					{
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_DISABLE);
					}
							
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						rtsEnable = rtsEnableOld;
						SetDcbFlag(Win32API_Serial.FRTSCONTROL, fRtsControlOld);
						Resources.WinIOError();
					}								
				}
			} 
		}
		
		internal StopBits StopBits 
		{ 
			get 
			{	
				switch(dcb.StopBits) 
				{
					case Win32API_Serial.ONESTOPBIT:
						return StopBits.One;
					case Win32API_Serial.ONE5STOPBITS:
						return StopBits.OnePointFive;
					case Win32API_Serial.TWOSTOPBITS:
						return StopBits.Two;
					default:
						Debug.Assert(true, "Invalid Stopbits value " + dcb.StopBits);
						return StopBits.One;
				
				}
			} 
			set 
			{
				if(value < StopBits.One || value > StopBits.OnePointFive)
					throw new ArgumentOutOfRangeException("stopBits", Resources.GetResourceString("ArgumentOutOfRange_Enum"));
				
				byte nativeValue = 0;
				if (value == StopBits.One) nativeValue = (byte) Win32API_Serial.ONESTOPBIT;
				else if (value == StopBits.OnePointFive) nativeValue = (byte) Win32API_Serial.ONE5STOPBITS; 
				else if (value == StopBits.Two) nativeValue = (byte) Win32API_Serial.TWOSTOPBITS;
				else Debug.Assert(true, "Invalid Stopbits value " + value);
				
				
				if(nativeValue != dcb.StopBits) 
				{
					byte stopBitsOld = dcb.StopBits;
				
					if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
					{
						dcb.StopBits = stopBitsOld;
						Resources.WinIOError();
					}								
				}	
			}
		}

		internal int WriteTimeout
		{ 
			get 
			{			
				int timeout = commTimeouts.WriteTotalTimeoutConstant; 
				return (timeout == 0) ? SerialPort.InfiniteTimeout : timeout;
			} 
			set 
			{ 	
				if (value <= 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException("WriteTimeout", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				int oldWriteConstant = commTimeouts.WriteTotalTimeoutConstant;
				commTimeouts.WriteTotalTimeoutConstant = ((value == SerialPort.InfiniteTimeout) ? 0 : value);
								
				if (Win32API_Serial.SetCommTimeouts(_safeHandle.Handle, ref commTimeouts) == false) 
				{
					commTimeouts.WriteTotalTimeoutConstant = oldWriteConstant;
					Resources.WinIOError();
				}
			} 
		}
		
		
		internal bool CDHolding 
		{ 
			get 
			{			
				int pinStatus = 0;
				if (Win32API_Serial.GetCommModemStatus(_safeHandle.Handle, ref pinStatus) == false)
					Resources.WinIOError();

				return (Win32API_Serial.MS_RLSD_ON & pinStatus) != 0;
			}
		}

		
		internal bool CtsHolding 
		{ 
			get 
			{			
				int pinStatus = 0;
				if (Win32API_Serial.GetCommModemStatus(_safeHandle.Handle, ref pinStatus) == false)
					Resources.WinIOError();
				return (Win32API_Serial.MS_CTS_ON & pinStatus) != 0;
			}
		
		}

		internal bool DsrHolding 
		{ 
			get 
			{
				int pinStatus = 0;
				if (Win32API_Serial.GetCommModemStatus(_safeHandle.Handle, ref pinStatus) == false)
					Resources.WinIOError();

				return (Win32API_Serial.MS_DSR_ON & pinStatus) != 0;
			}
		}

		
		internal int InBufferBytes 
		{ 
			get 
			{
				int errorCode = 0; 
				if (Win32API_Serial.ClearCommError(_safeHandle.Handle, ref errorCode, ref comStat)  == false) 
				{
					Resources.WinIOError();
				}
				return (int) comStat.cbInQue;
			} 
		}

		internal int OutBufferBytes 
		{ 
			get 
			{
				int errorCode = 0; 
				if (Win32API_Serial.ClearCommError(_safeHandle.Handle, ref errorCode, ref comStat)  == false)
					Resources.WinIOError();
				return (int) comStat.cbOutQue;
	
			}	
		} 			

		internal SerialStream(string resource, int baudRate, Parity parity, int dataBits, StopBits stopBits, int readTimeout, int writeTimeout, Handshake handshake, 
			bool dtrEnable, bool rtsEnable, bool discardNull, byte parityReplace) 
		{
		
			IntPtr tempHandle = Win32API_Serial.CreateFile("\\\\.\\" + resource,
				Win32API_Serial.GENERIC_READ | Win32API_Serial.GENERIC_WRITE,
				0,    
				Win32API_Serial.NULL, 
				Win32API_Serial.OPEN_EXISTING, 
				Win32API_Serial.FILE_FLAG_OVERLAPPED |
				Win32API_Serial.FILE_ATTRIBUTE_NORMAL,    
				Win32API_Serial.NULL  
				);

			if (tempHandle == Win32API_Serial.INVALID_HANDLE_VALUE) 
			{
				int errorCode = Marshal.GetLastWin32Error();
				switch(errorCode) 
				{
					case Win32API_Serial.ERROR_FILE_NOT_FOUND:
						throw new FileNotFoundException("resource", Resources.GetResourceString("IO.FileNotFound_FileName", resource));
		
					case Win32API_Serial.ERROR_ACCESS_DENIED:
						throw new UnauthorizedAccessException(Resources.GetResourceString("UnauthorizedAccess_IODenied_Path", resource));
					default:
						Resources.WinIOError();
						break;
				}
			}
			

			// kd5tfd -- fix Keyspan problem -- Keyspan USB serial adapters return FILE_TYPE_UNKNOWN here so we 
			// allow that to accomodate the Keyspan adapater.  (Sept 2006) 
			int ftype = Win32API_Serial.GetFileType(tempHandle);
			if ( (ftype  != Win32API_Serial.FILE_TYPE_CHAR) && 
			     (ftype  != Win32API_Serial.FILE_TYPE_UNKNOWN) ) 
				throw new ArgumentException("resource", Resources.GetResourceString("Arg_InvalidResourceFile"));
			
			_safeHandle = new __SafeHandle(tempHandle, true);
			
			this.portName = resource;
			this.handshake = handshake;
			this.dtrEnable = dtrEnable;
			this.rtsEnable = rtsEnable;
			this.parityReplace = parityReplace;
			
			tempBuf = new byte[1];			
		
			commProp = new Win32API_Serial.COMMPROP();
			if (Win32API_Serial.GetCommProperties(_safeHandle.Handle, ref commProp) == false) 
			{
				Win32API_Serial.CloseHandle(_safeHandle.Handle);
				Resources.WinIOError();
			}
			if (baudRate > commProp.dwMaxBaud && commProp.dwMaxBaud > 0) 
				throw new ArgumentOutOfRangeException("baudRate", "Requested baud greater than maximum for this device driver = " + commProp.dwMaxBaud);
			
			comStat = new Win32API_Serial.COMSTAT();
			dcb = new Win32API_Serial.DCB();
			
			InitializeDCB(baudRate, parity, dataBits, stopBits, discardNull);
		
			commTimeouts.ReadIntervalTimeout = (readTimeout == SerialPort.InfiniteTimeout) ? 0 : Win32API_Serial.MAXDWORD;
			commTimeouts.ReadTotalTimeoutMultiplier		= (readTimeout > 0 && readTimeout != SerialPort.InfiniteTimeout)
															? Win32API_Serial.MAXDWORD : 0;
			commTimeouts.ReadTotalTimeoutConstant		=  (readTimeout > 0 && readTimeout != SerialPort.InfiniteTimeout) ?
															readTimeout : 0;
			commTimeouts.WriteTotalTimeoutMultiplier	= 0;
			commTimeouts.WriteTotalTimeoutConstant		=  ((writeTimeout == SerialPort.InfiniteTimeout) ?
															0 : writeTimeout);
			if (Win32API_Serial.SetCommTimeouts(_safeHandle.Handle, ref commTimeouts) == false) 
			{
				Win32API_Serial.CloseHandle(_safeHandle.Handle);
				Resources.WinIOError();
			}		
			
			if (!ThreadPool.BindHandle(_safeHandle.Handle)) 
			{
				throw new IOException(Resources.GetResourceString("IO.IO_BindHandleFailed"));
			}
		
			myWaitCommCallback = new WaitEventCallback(WaitForCommEvent);
			myAsyncCallback = new AsyncCallback(EndWaitForCommEvent);
			state = null;	
			
			IAsyncResult ar = myWaitCommCallback.BeginInvoke(myAsyncCallback, state);
		}
	
		~SerialStream()
		{
			if (_safeHandle != null) 
			{
				Dispose(false);
			}
		}
		
		protected virtual void Dispose(bool disposing)
		{		
			if (_safeHandle != null) 
			{
				if (!_safeHandle.IsClosed) 
				{					
					if(!Win32API_Serial.EscapeCommFunction(_safeHandle.Handle, Win32API_Serial.CLRDTR)) 
					{
						Resources.WinIOError();
					}
					Flush();
					_safeHandle.Close();
				}
			}		
		}

		public override IAsyncResult BeginRead(byte[] array, int offset,int numBytes, AsyncCallback userCallback, object stateObject)  
		{
			return BeginRead(array, offset, numBytes, userCallback, stateObject, ReadTimeout);
		}
		
		
		internal IAsyncResult BeginRead(byte[] array, int offset,int numBytes, 
			AsyncCallback userCallback, object stateObject, int timeout)  
		{	
			if (array==null)
				throw new ArgumentNullException("array");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (numBytes < 0)
				throw new ArgumentOutOfRangeException("numBytes", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (array.Length - offset < numBytes)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			return BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0, timeout);
		}
		
		public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, 
			AsyncCallback userCallback, object stateObject) 
		{
			return BeginWrite(array, offset, numBytes, userCallback, stateObject, WriteTimeout);
		}
			
		internal IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, 
			AsyncCallback userCallback, object stateObject, int timeout) 
		{
			if (inBreak)
				throw new InvalidOperationException("BeginWrite in break");
			if (array==null)
				throw new ArgumentNullException("array");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (numBytes < 0)
				throw new ArgumentOutOfRangeException("numBytes", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (array.Length - offset < numBytes)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));

			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			return BeginWriteCore(array, offset, numBytes, userCallback, stateObject, timeout);
		}

		internal void ClearBreak()
		{
			if (Win32API_Serial.ClearCommBreak(_safeHandle.Handle) == false) 
				Resources.WinIOError();
			inBreak = false;
		}

		public override void Close()
		{
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		internal void DiscardInBuffer()
		{
			
			if (Win32API_Serial.PurgeComm(_safeHandle.Handle, Win32API_Serial.PURGE_RXCLEAR) == false)
				Resources.WinIOError();
		}

		internal void DiscardOutBuffer()
		{
			if (Win32API_Serial.PurgeComm(_safeHandle.Handle, Win32API_Serial.PURGE_TXCLEAR) == false)
				Resources.WinIOError();
		}

		public unsafe override int EndRead(IAsyncResult asyncResult) 
		{
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			if (asyncResult==null)
				throw new ArgumentNullException("asyncResult");

			AsyncSerialStream_AsyncResult afsar = asyncResult as AsyncSerialStream_AsyncResult;
			if (afsar==null || afsar._isWrite)
				Resources.WrongAsyncResult();

			if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0))
				Resources.EndReadCalledTwice();
			
			
			WaitHandle wh = afsar.AsyncWaitHandle;
				if (wh != null) 
				{
					
					if (!afsar.IsCompleted) 
					{
						do {
							int beginTicks = Win32API_Serial.GetTickCount();
							wh.WaitOne();
							int currentTimeout = ReadTimeout;
							int endTicks = Win32API_Serial.GetTickCount();
							if (endTicks - beginTicks >= currentTimeout && currentTimeout != SerialPort.InfiniteTimeout)
								lastOpTimedOut = true;		
							else 
								lastOpTimedOut = false;
						} while (afsar._numBytes == 0 && ReadTimeout == SerialPort.InfiniteTimeout);
						
						afsar._isComplete = true;
					}
					wh.Close();
				}

				if (afsar._errorCode != 0)
					Resources.WinIOError(afsar._errorCode, portName);
				
				ReadTimeout = afsar._oldTimeout;
				return afsar._numBytes + afsar._numBufferedBytes;
		}

		public unsafe override void EndWrite(IAsyncResult asyncResult) 
		{		
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			if (inBreak)
				throw new InvalidOperationException("EndWrite in break");
			if (asyncResult==null)
				throw new ArgumentNullException("asyncResult");

			AsyncSerialStream_AsyncResult afsar = asyncResult as AsyncSerialStream_AsyncResult;
			if (afsar==null || !afsar._isWrite)
				Resources.WrongAsyncResult();

			if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0))
				Resources.EndWriteCalledTwice();

			WaitHandle wh = afsar.AsyncWaitHandle;
			if (wh != null) 
			{
				if (!afsar.IsCompleted) 
				{
					int beginTicks = Win32API_Serial.GetTickCount();
					wh.WaitOne();
					int currentTimeout = WriteTimeout;
					int endTicks = Win32API_Serial.GetTickCount();
					if (endTicks - beginTicks >= currentTimeout && currentTimeout != SerialPort.InfiniteTimeout)
						throw new TimeoutException("Write Timed Out: " + (endTicks - beginTicks) + " > " + currentTimeout);
					afsar._isComplete = true;
				}
				wh.Close();
			}

			if (afsar._errorCode != 0)
				Resources.WinIOError(afsar._errorCode, portName);
			
			WriteTimeout = afsar._oldTimeout;
			return;
		}

		public override void Flush() 
		{ 
			if (_safeHandle.Handle == Win32API_Serial.NULL) throw new InvalidOperationException("Flush - Stream not open!");
			DiscardInBuffer();
			DiscardOutBuffer();
			return;
		}

		public override int Read([In, Out] byte[] array, int offset, int count) 
		{
			return Read(array, offset, count, ReadTimeout);
		} 
		
		internal int Read([In, Out] byte[] array, int offset, int count, int timeout) 
		{
			if (array==null)
				throw new ArgumentNullException("array", Resources.GetResourceString("ArgumentNull_Buffer"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (array.Length - offset < count)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
            if (count == 0) return 0; 
            
            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Read - called with timeout " + timeout);
            
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
            
			IAsyncResult result = BeginReadCore(array, offset, count, null, null, 0, timeout);
			return EndRead(result);
		}
		

		public override int ReadByte() 
		{
			return ReadByte(ReadTimeout);
		}
		
		internal int ReadByte(int timeout) 
		{
			if (_safeHandle.IsClosed) Resources.FileNotOpen();			
			
			IAsyncResult result = BeginReadCore(tempBuf, 0, 1, null, null, 0, timeout);
			int res = EndRead(result);
			
			if (lastOpTimedOut)
				return -1;
			else 
				return tempBuf[0];
		}

		public override long Seek(long offset, SeekOrigin origin) 
		{
			throw new NotSupportedException(Resources.GetResourceString("NotSupported_UnseekableStream")); 
		}

		internal void SetBreak() 
		{			
			if (Win32API_Serial.SetCommBreak(_safeHandle.Handle) == false) 
				Resources.WinIOError();
			inBreak = true;
		}
			
		public override void SetLength(long value) 
		{
			throw new NotSupportedException(Resources.GetResourceString("NotSupported_UnseekableStream")); 
		}

		public override void Write(byte[] array, int offset, int count) 
		{
			Write(array, offset, count, WriteTimeout);
		}

		internal void Write(byte[] array, int offset, int count, int timeout)
		{			
			if (inBreak)
				throw new InvalidOperationException("Write in break");
			if (array==null)
				throw new ArgumentNullException("write buffer", Resources.GetResourceString("ArgumentNull_Array"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("write offset", Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("write count", Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			if (count == 0) return;				
			if (array.Length - offset < count)
				throw new ArgumentException("write count",Resources.GetResourceString("ArgumentOutOfRange_OffsetOut"));
			Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Write - write timeout is " + timeout);
			
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			
			IAsyncResult result = BeginWriteCore(array, offset, count, null, null, timeout);
			EndWrite(result);
	
			return;
		}

		public override void WriteByte(byte value) 
		{
			WriteByte(value, WriteTimeout);
		}
		
		internal void WriteByte(byte value, int timeout) 
		{
			if (inBreak)
				throw new InvalidOperationException("WriteByte in break");
			
			if (_safeHandle.IsClosed) Resources.FileNotOpen();
			tempBuf[0] = value;
			IAsyncResult ar = BeginWriteCore(tempBuf, 0, 1, null, null, timeout);
			EndWrite(ar);
		
			return;
		}

		private void InitializeDCB(int baudRate, Parity parity, int dataBits, StopBits stopBits, bool discardNull)
		{
			if (Win32API_Serial.GetCommState(_safeHandle.Handle, ref dcb) == false) 
			{
				Resources.WinIOError();
			}			
			dcb.DCBlength = (uint) System.Runtime.InteropServices.Marshal.SizeOf(dcb);
			
			dcb.BaudRate = (uint) baudRate;
			dcb.ByteSize = (byte) dataBits;			

			switch (stopBits) 
			{
				case StopBits.One:
					dcb.StopBits = Win32API_Serial.ONESTOPBIT;
					break;
				case StopBits.OnePointFive:
					dcb.StopBits = Win32API_Serial.ONE5STOPBITS;
					break;
				case StopBits.Two:
					dcb.StopBits = Win32API_Serial.TWOSTOPBITS;
					break;
				default:
					Debug.Assert(true, "TBD");
					break;
			}
			
			dcb.Parity = (byte) parity;
			SetDcbFlag(Win32API_Serial.FPARITY, ((parity == Parity.None)  ?  0  :  1));
		
			SetDcbFlag(Win32API_Serial.FBINARY, 1);	
			
			SetDcbFlag(Win32API_Serial.FOUTXCTSFLOW, ((handshake == Handshake.RequestToSend ||
				handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));
			SetDcbFlag(Win32API_Serial.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
			SetDcbFlag(Win32API_Serial.FDTRCONTROL, (dtrEnable) ? Win32API_Serial.DTR_CONTROL_ENABLE : Win32API_Serial.DTR_CONTROL_DISABLE);
			SetDcbFlag(Win32API_Serial.FDSRSENSITIVITY, 0); 
			SetDcbFlag(Win32API_Serial.FINX, (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
			SetDcbFlag(Win32API_Serial.FOUTX,(handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
			
			if (parity != Parity.None)
			{
				SetDcbFlag(Win32API_Serial.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
				dcb.ErrorChar = parityReplace;
			}
			else
			{
				SetDcbFlag(Win32API_Serial.FERRORCHAR, 0);
				dcb.ErrorChar = (byte) '\0';
			}
			
			SetDcbFlag(Win32API_Serial.FNULL, discardNull ? 1 : 0); 
			
					
			if ((handshake == Handshake.RequestToSend ||
				handshake == Handshake.RequestToSendXOnXOff)) 
			{
				SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_HANDSHAKE);	
			}
			else if(rtsEnable)
			{
				SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_ENABLE);
			}
			else
			{
				SetDcbFlag(Win32API_Serial.FRTSCONTROL, Win32API_Serial.RTS_CONTROL_DISABLE);
			}
			
			dcb.XonChar = Win32API_Serial.DEFAULTXONCHAR;				
			dcb.XoffChar = Win32API_Serial.DEFAULTXOFFCHAR;
			
			dcb.XonLim = dcb.XoffLim = (ushort) (commProp.dwCurrentRxQueue / 4);  
				
			dcb.EofChar = Win32API_Serial.EOFCHAR;	
			
			dcb.EvtChar = Win32API_Serial.EOFCHAR;
			
			if (Win32API_Serial.SetCommState(_safeHandle.Handle, ref dcb) == false) 
			{
				Resources.WinIOError();
			}
		}

		internal int GetDcbFlag(int whichFlag) 
		{
			uint mask;
			
			Debug.Assert(whichFlag >= Win32API_Serial.FBINARY && whichFlag <= Win32API_Serial.FDUMMY2, "GetDcbFlag needs to fit into enum!");
			
			if (whichFlag == Win32API_Serial.FDTRCONTROL || whichFlag == Win32API_Serial.FRTSCONTROL) 
			{
				mask = 0x3;
			} 
			else if (whichFlag == Win32API_Serial.FDUMMY2) 
			{
				mask = 0x1FFFF;
			} 
			else 
			{
				mask = 0x1;
			} 
			uint result = dcb.Flags & (mask << whichFlag);
			return (int) (result >> whichFlag);
		}

		internal void SetDcbFlag(int whichFlag, int setting) 
		{
			uint mask;
			setting = setting << whichFlag; 
			
			Debug.Assert(whichFlag >= Win32API_Serial.FBINARY && whichFlag <= Win32API_Serial.FDUMMY2, "SetDcbFlag needs to fit into enum!");
			
			if (whichFlag == Win32API_Serial.FDTRCONTROL || whichFlag == Win32API_Serial.FRTSCONTROL) 
			{
				mask = 0x3;
			} 
			else if (whichFlag == Win32API_Serial.FDUMMY2) 
			{
				mask = 0x1FFFF;
			} 
			else 
			{
				mask = 0x1;
			}

			dcb.Flags &= ~(mask << whichFlag);
			
			dcb.Flags |= ((uint) setting);
		}
		
		unsafe private AsyncSerialStream_AsyncResult BeginReadCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject, int numBufferedBytesRead, int timeout)
		{

			AsyncSerialStream_AsyncResult asyncResult = new AsyncSerialStream_AsyncResult();
			asyncResult._userCallback = userCallback;
			asyncResult._userStateObject = stateObject;
			asyncResult._isWrite = false;
			
			asyncResult._oldTimeout = ReadTimeout; 
			ReadTimeout = timeout;	
			
			asyncResult._numBufferedBytes = numBufferedBytesRead;

			ManualResetEvent waitHandle = new ManualResetEvent(false);
			asyncResult._waitHandle = waitHandle;

			Overlapped overlapped = new Overlapped(0, 0, 0, asyncResult);

			NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback);
			int hr = 0;
			int r = ReadFileNative(_safeHandle, array, offset, numBytes,
			 intOverlapped, out hr);
			
			if (r==-1) 
			{
				if (hr != Win32API_Serial.ERROR_IO_PENDING) 
				{
					if (hr == Win32API_Serial.ERROR_HANDLE_EOF)
						Resources.EndOfFile();
					else
						Resources.WinIOError(hr, String.Empty);
				}
			}

			return asyncResult;
		}

		unsafe private AsyncSerialStream_AsyncResult BeginWriteCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject, int timeout) 
		{
			
			AsyncSerialStream_AsyncResult asyncResult = new AsyncSerialStream_AsyncResult();
			asyncResult._userCallback = userCallback;
			asyncResult._userStateObject = stateObject;
			asyncResult._isWrite = true;
			
			asyncResult._oldTimeout = WriteTimeout;		
			WriteTimeout = timeout;
			
			ManualResetEvent waitHandle = new ManualResetEvent(false); 
			asyncResult._waitHandle = waitHandle;

			Overlapped overlapped = new Overlapped(0, 0, 0, asyncResult);

			NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback);
			int hr = 0;
			int r = WriteFileNative(_safeHandle, array, offset, numBytes, intOverlapped, out hr);

			if (r==-1) 
			{
				if (hr != Win32API_Serial.ERROR_IO_PENDING) 
				{
				
					if (hr == Win32API_Serial.ERROR_HANDLE_EOF)
						Resources.EndOfFile();
					else
						Resources.WinIOError(hr, String.Empty);
				}
			}
			return asyncResult;
		}


		internal unsafe int ReadFileNative(SafeHandle hp, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
		{
		
			if (bytes.Length - offset < count)
				throw new IndexOutOfRangeException(Resources.GetResourceString("IndexOutOfRange_IORaceCondition"));

			if (bytes.Length==0) 
			{
				hr = 0;
				return 0;
			}

			int r = 0;
			int numBytesRead = 0;

			bool incremented = false;
			try 
			{
				if (hp.TryAddRef(ref incremented)) 
				{
					fixed(byte* p = bytes) 
					{		
						r = Win32API_Serial.ReadFile(hp.Handle, p + offset, count, Win32API_Serial.NULL, overlapped);
					}
				}
				else
					hr = Win32API_Serial.ERROR_INVALID_HANDLE;  // Handle was closed.
			}
			finally 
			{
				if (incremented) hp.Release();
			}

			if (r==0) 
			{
				hr = Marshal.GetLastWin32Error();
				
				if (hr == Win32API_Serial.ERROR_INVALID_HANDLE)
					_safeHandle.ForciblyMarkAsClosed();

				return -1;
			}
			else
				hr = 0;
			return numBytesRead;
		}
			
		internal unsafe int WriteFileNative(SafeHandle hp, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr) 
		{

			if (bytes.Length - offset < count)
				throw new IndexOutOfRangeException(Resources.GetResourceString("IndexOutOfRange_IORaceCondition"));

			if (bytes.Length==0) 
			{
				hr = 0;
				return 0;
			}

			int numBytesWritten = 0;
			int r = 0;
            
			bool incremented = false;
			try 
			{
				if (hp.TryAddRef(ref incremented)) 
				{
					fixed(byte* p = bytes) 
					{
						r = Win32API_Serial.WriteFile(hp.Handle, p + offset, count, Win32API_Serial.NULL, overlapped);
					}
				}
				else
					hr = Win32API_Serial.ERROR_INVALID_HANDLE;  
			}
			finally 
			{
				if (incremented) hp.Release();
			}

			if (r==0) 
			{
				hr = Marshal.GetLastWin32Error();
				if (hr == Win32API_Serial.ERROR_INVALID_HANDLE)
					_safeHandle.ForciblyMarkAsClosed();

				return -1;
			}
			else
				hr = 0;
			return numBytesWritten;          
		}

		private unsafe int WaitForCommEvent() 
		{
			int eventsOccurred = 0;
			Win32API_Serial.SetCommMask(_safeHandle.Handle, Win32API_Serial.ALL_EVENTS);
			
			AsyncSerialStream_AsyncResult asyncResult = new AsyncSerialStream_AsyncResult();
			asyncResult._userCallback = null;
			asyncResult._userStateObject = null;
			asyncResult._isWrite = false;
			asyncResult._oldTimeout = -1;
			asyncResult._numBufferedBytes = 0;
			ManualResetEvent waitHandle = new ManualResetEvent(false);
			asyncResult._waitHandle = waitHandle;
			
			Overlapped overlapped = new Overlapped(0, 0, 0, asyncResult);

			NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback);
			
			if (Win32API_Serial.WaitCommEvent(_safeHandle.Handle, ref eventsOccurred, intOverlapped) == false) 
			{
				int hr = Marshal.GetLastWin32Error();
				if (hr == Win32API_Serial.ERROR_IO_PENDING) 
				{
					int temp = Win32API_Serial.WaitForSingleObject(waitHandle.Handle, -1);
					if(temp == 0) // no error
						return eventsOccurred;
					else
						Resources.WinIOError();
				}		
				else
					Resources.WinIOError();
				
			}  
			return eventsOccurred;
		}
		
		[OneWayAttribute()]
		private void EndWaitForCommEvent(IAsyncResult ar)
		{
	      
			int errorEvents = (int) (SerialEvents.Frame | SerialEvents.Overrun 
				| SerialEvents.RxOver | SerialEvents.RxParity | SerialEvents.TxFull);
			int receivedEvents = (int) (SerialEvents.ReceivedChars | SerialEvents.EofReceived);	
			int pinChangedEvents = (int) (SerialEvents.Break | SerialEvents.CDChanged | SerialEvents.CtsChanged |
				SerialEvents.Ring | SerialEvents.DsrChanged);
			
			WaitEventCallback myWaitCommCallback = 
				(WaitEventCallback) ((AsyncResult)ar).AsyncDelegate;
			int nativeEvents = myWaitCommCallback.EndInvoke(ar);
			int errors = 0;
			int events = 0;
			
			if ((nativeEvents & Win32API_Serial.EV_ERR) != 0) 
			{
				if (Win32API_Serial.ClearCommError(_safeHandle.Handle, ref errors, ref comStat) == false) 
				{
					Resources.WinIOError();
				}
				
				if ((errors & Win32API_Serial.CE_RXOVER) != 0) events |= (int) SerialEvents.RxOver;
				if ((errors & Win32API_Serial.CE_OVERRUN)  != 0) events |= (int)  SerialEvents.Overrun;
				if ((errors & Win32API_Serial.CE_PARITY) != 0) events |= (int) SerialEvents.RxParity;
				if ((errors & Win32API_Serial.CE_FRAME) != 0) events|= (int) SerialEvents.Frame;
				if ((errors & Win32API_Serial.CE_TXFULL) != 0) events |= (int) SerialEvents.TxFull;
				if ((errors & Win32API_Serial.CE_BREAK) != 0) events |= (int) SerialEvents.Break;

			}
			if ((nativeEvents & Win32API_Serial.EV_RXCHAR) != 0) events |= (int) SerialEvents.ReceivedChars;
			if ((nativeEvents & Win32API_Serial.EV_RXFLAG) != 0) events |= (int) SerialEvents.EofReceived;
			if ((nativeEvents & Win32API_Serial.EV_CTS) != 0) events |= (int) SerialEvents.CtsChanged;
			if ((nativeEvents & Win32API_Serial.EV_DSR) != 0) events |= (int) SerialEvents.DsrChanged;
			if ((nativeEvents & Win32API_Serial.EV_RLSD) != 0) events |= (int) SerialEvents.CDChanged;
			if ((nativeEvents & Win32API_Serial.EV_RING) != 0) events |= (int) SerialEvents.Ring;
			if ((nativeEvents & Win32API_Serial.EV_BREAK) != 0) events |= (int) SerialEvents.Break;
			
			
			if ((events & errorEvents) != 0 && ErrorEvent != null) 
				ErrorEvent(this, new SerialEventArgs((SerialEvents) (events & errorEvents))); 
			if ((events & pinChangedEvents) != 0 && PinChangedEvent != null)
				PinChangedEvent(this, new SerialEventArgs((SerialEvents) (events & pinChangedEvents)));
			if ((events & receivedEvents) != 0 && ReceivedEvent != null) 
				ReceivedEvent(this, new SerialEventArgs((SerialEvents) (events & receivedEvents)));
				
			myWaitCommCallback.BeginInvoke(myAsyncCallback, state); // start it over again.	
		}
		
		
		unsafe private static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
		{
			Overlapped overlapped = Overlapped.Unpack(pOverlapped);
		    
			AsyncSerialStream_AsyncResult asyncResult = 
				(AsyncSerialStream_AsyncResult)overlapped.AsyncResult;
			asyncResult._numBytes = (int)numBytes;
            
			asyncResult._errorCode = (int)errorCode;

			WaitHandle wh = asyncResult._waitHandle;
			if (wh != null) 
			{
				bool r = Win32API_Serial.SetEvent(wh.Handle);
				if (!r) Resources.WinIOError();
			}
			asyncResult._isComplete = true;

			asyncResult._completedSynchronously = false;
			AsyncCallback userCallback = asyncResult._userCallback;
			if (userCallback!=null)
				userCallback(asyncResult);
			Overlapped.Free(pOverlapped);
		}
		
		
		unsafe internal class AsyncSerialStream_AsyncResult : IAsyncResult
		{
			internal AsyncCallback _userCallback;
            
			internal Object _userStateObject;
			internal WaitHandle _waitHandle;			
			internal bool _isWrite;     
			internal bool _isComplete;
			internal bool _completedSynchronously;  
			internal int _EndXxxCalled;   
			internal int _numBytes;     
			internal int _numBufferedBytes;
			internal int _errorCode;
				
			internal int _oldTimeout;	
			/// <include file='doc\ModFileStream.uex' path='docs/doc[@for="ModFileStream.AsyncSerialStream_AsyncResult.AsyncState"]/*' />
			public virtual Object AsyncState
			{
				get { return _userStateObject; }
			}

			/// <include file='doc\ModFileStream.uex' path='docs/doc[@for="ModFileStream.AsyncSerialStream_AsyncResult.IsCompleted"]/*' />
			public bool IsCompleted
			{
				get { return _isComplete; }
				set { _isComplete = value; }
			}

			/// <include file='doc\ModFileStream.uex' path='docs/doc[@for="ModFileStream.AsyncSerialStream_AsyncResult.AsyncWaitHandle"]/*' />
			public WaitHandle AsyncWaitHandle
			{
				get { return _waitHandle; }
			}

			public bool CompletedSynchronously
			{
				get { return _completedSynchronously; }
			}			
		}
	
		private sealed class __SafeHandle : SafeHandle
		{
			private bool _ownsHandle;

			internal __SafeHandle(IntPtr handle, bool ownsHandle) : base(handle)
			{
				_ownsHandle = ownsHandle;
			}

			protected internal override void FreeHandle(IntPtr handle)
			{
				if (_ownsHandle)
				{
					//Important!  Must cancel all events before closing file!!!
					Win32API_Serial.SetCommMask(handle, 0);
					//Now we can close the file handle
					Win32API_Serial.CloseHandle(handle);
				}
			}
		}
	}
}

//=================================================================
// SerialPort.cs
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SerialPorts
{
	public class SerialPort 
	{
		#region Variable Declaration

		public const int InfiniteTimeout = -1;		
			
		private const int maxDataBits = 8;
		private const int minDataBits = 5;		
			
		private const int defaultBufferSize = 1024;	
		private byte[] inBuffer = new byte[defaultBufferSize];
		private int readPos = 0;	
		private int readLen = 0;	
		private char[] oneChar = new char[1];
		
		// Events
		public event SerialEventHandler ErrorEvent;
		public event SerialEventHandler PinChangedEvent;
		public event SerialEventHandler ReceivedEvent; 

		#endregion
		
		#region Constructors and Destructor

		public SerialPort()
		{

		}

		public SerialPort(string resource) : this (resource, defaultBaudRate, defaultParity, defaultDataBits, defaultStopBits) 
		{

		}

		public SerialPort(string resource, int baudRate) : this (resource, baudRate, defaultParity, defaultDataBits, defaultStopBits) 
		{

		}

		public SerialPort(string resource, int baudRate, Parity parity) : this (resource, baudRate, parity, defaultDataBits, defaultStopBits) 
		{

		}

		public SerialPort(string resource, int baudRate, Parity parity, int dataBits) : this (resource, baudRate, parity, dataBits, defaultStopBits) 
		{

		}

		public SerialPort(string resource, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
		{
			this.portName = resource;
			this.baudRate = baudRate;
			this.parity = parity;
			this.dataBits = dataBits;
			this.StopBits = stopBits;		
		}

		protected void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (isOpen)
				{
					internalSerialStream.Flush();
					Close();
				}
			}			
		} 

		#endregion

		#region Properties

		private SerialStream internalSerialStream = null;
		public Stream BaseStream 
		{
			get { return internalSerialStream; }
		}

		private const int defaultBaudRate = 9600;
		private int baudRate = defaultBaudRate;
		public int BaudRate 
		{ 
			get { return baudRate;	}
			set
			{ 
				if (isOpen)
					internalSerialStream.BaudRate = value;
				baudRate = value; 
			}
		}

		public bool CDHolding 
		{
			get 
			{ 
				if (!isOpen)
					throw new InvalidOperationException("CDHolding - port not open");
				return internalSerialStream.CDHolding;
			}
		}

		public bool CtsHolding 
		{
			get 
			{ 
				if (!isOpen)
					throw new InvalidOperationException("CtsHolding - port not open");
				return internalSerialStream.CtsHolding;
			}
		}

		public bool DsrHolding 
		{
			get 
			{ 
				if (!isOpen)
					throw new InvalidOperationException("DsrHolding - port not open");
				return internalSerialStream.DsrHolding;
			}
		}

		private const int defaultDataBits = 8;
		private int dataBits = defaultDataBits;
		public int DataBits 
		{ 
			get { return dataBits;	}
			set 
			{
				if (isOpen) 
					internalSerialStream.DataBits = value;
				dataBits = value;
			}
		}

		private const bool defaultDiscardNull = false;
		private bool discardNull = defaultDiscardNull;
		public bool DiscardNull
		{
			get { return discardNull; }
			set 
			{
				if (isOpen)
					internalSerialStream.DiscardNull = value;
				discardNull = value;
			}
		}		

		private const bool defaultDtrEnable = false;
		private bool dtrEnable = defaultDtrEnable;
		public bool DtrEnable 
		{ 
			get { return dtrEnable;	}
			set 
			{
				if (isOpen) 
					internalSerialStream.DtrEnable = value;
				dtrEnable = value;
			}
		}

		private Encoding encoding = new ASCIIEncoding(); 
		public Encoding Encoding
		{
			get { return encoding; }
			set { encoding = value; }
		}

		private const SerialEvents defaultEventFilter = SerialEvents.All; 
		private SerialEvents eventFilter = defaultEventFilter;
		public SerialEvents EventFilter
		{
			get { return eventFilter; }
			set { eventFilter = value; }
		}

		private const Handshake defaultHandshake = Handshake.None;
		private Handshake handshake = defaultHandshake;
		public Handshake Handshake 
		{
			get { return handshake;	}
			set 
			{
				if (isOpen) 
					internalSerialStream.Handshake = value;
				handshake = value;
			}	
		}

		private bool inBreak = false;
		public bool InBreak
		{
			get { return inBreak; }
		}

		public int InBufferBytes 
		{ 
			get 
			{
				if (!isOpen)
					throw new InvalidOperationException("InBufferBytes - port not open");
				return internalSerialStream.InBufferBytes + readLen - readPos; 
			}
		}

		private bool isOpen = false;
		public bool IsOpen
		{
			get { return isOpen; }
		}

		public int OutBufferBytes 
		{ 
			get 
			{
				if (!isOpen)
					throw new InvalidOperationException("OutBufferBytes - port not open");
				return internalSerialStream.OutBufferBytes;
			}
		} 			

		private const Parity defaultParity = Parity.None;
		private Parity parity = defaultParity;
		public Parity Parity 
		{  
			get { return parity; }
			set 
			{
				if (isOpen) 
					internalSerialStream.Parity = value;
				parity = value;
			}
		} 

		private const byte defaultParityReplace = (byte) '?';
		private byte parityReplace = defaultParityReplace;
		public byte ParityReplace
		{
			get { return parityReplace;	}
			set
			{
				if (isOpen) 
					internalSerialStream.ParityReplace = value;
				parityReplace = value;
			}
		}

		private const string defaultPortName = "COM1";
		private string portName = defaultPortName;
		public string PortName 
		{ 
			get { return portName; }
			set 
			{
				if (isOpen)
					throw new InvalidOperationException("PortName - port open");
				portName = value;
			}
		}

		private const int defaultReadTimeout = SerialPort.InfiniteTimeout;
		private int readTimeout = defaultReadTimeout;
		public int ReadTimeout  
		{ 
			get { return readTimeout; } 

			set 
			{
				if (isOpen) 
					internalSerialStream.ReadTimeout = value;
				readTimeout = value;
			}
		}

		private const int defaultReceivedBytesThreshold = 1;
		private int receivedBytesThreshold = defaultReceivedBytesThreshold;
		public int ReceivedBytesThreshold 
		{			
			get { return receivedBytesThreshold; } 
			set 
			{ 
				if (value <= 0) 
					throw new ArgumentOutOfRangeException("receivedBytesThreshold", 
						Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
				receivedBytesThreshold = value;
			}
		}	

		private const bool defaultRtsEnable = false;		
		private bool rtsEnable = defaultRtsEnable;
		public bool RtsEnable 
		{ 
			get { return rtsEnable;	}
			set 
			{
				if (isOpen) 
					internalSerialStream.RtsEnable = value;
				rtsEnable = value;
			}
		}

		private const StopBits defaultStopBits = StopBits.One;
		private StopBits stopBits = defaultStopBits;
		public StopBits StopBits 
		{ 
			get { return stopBits; }
			set 
			{
				if (isOpen) 
					internalSerialStream.StopBits = value;
				stopBits = value;
			}
		}

		private const int defaultWriteTimeout = SerialPort.InfiniteTimeout;
		private int writeTimeout = defaultWriteTimeout;	
		public int WriteTimeout
		{ 
			get	{ return writeTimeout; }
			set 
			{
				if (isOpen) 
					internalSerialStream.WriteTimeout = value;
				writeTimeout = value;
			}
		}

		#endregion

		#region Misc Routines

		public void Open() 
		{
			if (isOpen) 
				throw new InvalidOperationException("Serial Port - port already open!");
			if (parity < Parity.None || parity > Parity.Space)
				throw new ArgumentOutOfRangeException("parity", Resources.GetResourceString("ArgumentOutOfRange_Enum"));
			if (dataBits < minDataBits || dataBits > maxDataBits)
				throw new ArgumentOutOfRangeException("dataBits", 
					Resources.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", minDataBits, maxDataBits));
			if (stopBits < StopBits.One || stopBits > StopBits.OnePointFive)
				throw new ArgumentOutOfRangeException("stopBits", Resources.GetResourceString("ArgumentOutOfRange_Enum"));
			if (baudRate <= 0)
				throw new ArgumentOutOfRangeException("baudRate", Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			if (portName == null) 
				throw new ArgumentNullException("resource", Resources.GetResourceString("ArgumentNull_String"));
			if (handshake < Handshake.None || handshake > Handshake.RequestToSendXOnXOff)
				throw new ArgumentOutOfRangeException("handshake", Resources.GetResourceString("ArgumentOutOfRange_Enum"));	
			if (readTimeout < 0 && readTimeout != SerialPort.InfiniteTimeout) 
				throw new ArgumentOutOfRangeException("readTimeout", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (writeTimeout <= 0 && writeTimeout != SerialPort.InfiniteTimeout) 
				throw new ArgumentOutOfRangeException("writeTimeout", Resources.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			if (portName.StartsWith("\\\\"))
				throw new ArgumentException("resource", Resources.GetResourceString("Arg_SecurityException"));

			internalSerialStream = new SerialStream(portName, baudRate, parity, dataBits, stopBits, readTimeout,
				writeTimeout, handshake, dtrEnable, rtsEnable, discardNull, parityReplace);

			internalSerialStream.ErrorEvent += new SerialEventHandler(CatchErrorEvents);
			internalSerialStream.PinChangedEvent += new SerialEventHandler(CatchPinChangedEvents);
			internalSerialStream.ReceivedEvent += new SerialEventHandler(CatchReceivedEvents); 

			isOpen = true;
		}

		public void Close() 
		{
			if (!isOpen)
				throw new InvalidOperationException("Serial Port - port already closed!");
			if (internalSerialStream != null) 
			{
				internalSerialStream.Close();
				internalSerialStream = null;
			}
			isOpen = false;
		}		

		public int Read(byte[] buffer, int offset, int count) 
		{
			if (!isOpen) 
				throw new InvalidOperationException("Serial Port Read - port not open");
			if (buffer==null)
				throw new ArgumentNullException("buffer", Resources.GetResourceString("ArgumentNull_Buffer"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (buffer.Length - offset < count)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
			int beginReadPos = readPos;

			byte [] tempReturnBuffer = new byte[count];

			if (readLen - readPos >= 1)
			{
				int min = (readLen - readPos < count) ? readLen - readPos : count;
				Buffer.BlockCopy(inBuffer, readPos, buffer, 0, min);
				readPos += min;
				if (min == count) {
					if (readPos == readLen) readPos = readLen = 0;	// just a check to see if we can reset buffer
					return count;
				}
				
				if (InBufferBytes == 0) return min;
			}

			int bytesLeftToRead = count - (readPos - beginReadPos);

			if (bytesLeftToRead + readLen >= inBuffer.Length) ResizeBuffer();

			int returnCount = internalSerialStream.Read(inBuffer, readLen, bytesLeftToRead);

			Buffer.BlockCopy(inBuffer, beginReadPos, buffer, offset, returnCount + (readPos - beginReadPos));
			readLen = readPos = 0; 
			return returnCount + readPos - beginReadPos; // return the number of bytes we threw into the buffer plus what we had.
		}

		public int Read() 
		{
			return ReadOneChar(readTimeout);
		}

		private int ReadOneChar(int timeout) 
		{	
			int beginReadPos = readPos;
			int nextByte;
			int timeUsed = 0;
			Debug.Assert(isOpen, "ReadOneChar - port not open");
			
			if (timeout < 0 && timeout != SerialPort.InfiniteTimeout) return -1;
			
			if (encoding.GetCharCount(inBuffer, readPos, readLen - readPos) != 0) 
			{
				do 
				{
					nextByte = (int) inBuffer[readPos++];
				} while (encoding.GetCharCount(inBuffer, beginReadPos, readPos - beginReadPos) < 1);
				encoding.GetChars(inBuffer, beginReadPos, readPos - beginReadPos, oneChar, 0);
				return oneChar[0];
			} 
			else 
			{
				
				if (timeout == 0) {
					if (InBufferBytes + readPos >= inBuffer.Length) ResizeBuffer();
					int bytesRead = internalSerialStream.Read(inBuffer, readLen, InBufferBytes - (readLen - readPos)); // read all immediately avail.
					readLen += bytesRead;
					
					if (ReadBufferIntoChars(oneChar, 0, 1) == 0) return -1;
					else return oneChar[0];
				}
				
				int startTicks = Win32API_Serial.GetTickCount();
				do 
				{
					timeUsed = Win32API_Serial.GetTickCount() - startTicks;
					if (timeout != SerialPort.InfiniteTimeout && (timeout - timeUsed <= 0)) 
					{
						nextByte = -1;
						break;	
					}
					nextByte = internalSerialStream.ReadByte((timeout == InfiniteTimeout) ? InfiniteTimeout : timeout - timeUsed);
					if (nextByte == -1) break;		
					if (readLen >= inBuffer.Length) ResizeBuffer();
					inBuffer[readLen++] = (byte) nextByte;	
					readPos++;
				} while (encoding.GetCharCount(inBuffer, beginReadPos, readPos - beginReadPos) < 1);
			}
			
			if (nextByte == -1) return -1;		
			
			encoding.GetChars(inBuffer, beginReadPos, readPos - beginReadPos, oneChar, 0);
			return oneChar[0];
		}

		public int Read(char[] buffer, int offset, int count)
		{
			if (buffer==null)
				throw new ArgumentNullException("buffer", Resources.GetResourceString("ArgumentNull_Buffer"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (buffer.Length - offset < count)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
			if (!isOpen) 
				throw new InvalidOperationException("Serial Port Read - port not open");
				
			if (count == 0) return 0;	
			
			int bytesInDriver = InBufferBytes - (readLen - readPos);
			if (readLen + bytesInDriver >= inBuffer.Length) ResizeBuffer();
			internalSerialStream.Read(inBuffer, readLen, bytesInDriver);	
			readLen += bytesInDriver;
			
			int charsWeAlreadyHave = encoding.GetCharCount(inBuffer, readPos, readLen - readPos); 	
			if (charsWeAlreadyHave >= count) 
			{
				return ReadBufferIntoChars(buffer, offset, count);
			}
			
			if (readTimeout == 0) return ReadBufferIntoChars(buffer, offset, count);
			 
			int startTicks = Win32API_Serial.GetTickCount();
			int justRead;
			do {
				internalSerialStream.Read(inBuffer, readLen, Encoding.GetMaxByteCount(count - charsWeAlreadyHave));
				justRead = ReadBufferIntoChars(buffer, offset, count);
				if (justRead > 0) return justRead;
			} while (readTimeout == SerialPort.InfiniteTimeout || readTimeout - (Win32API_Serial.GetTickCount() - startTicks) > 0);
			
			return 0;
		}
		
		private int ReadBufferIntoChars(char[] buffer, int offset, int count) 
		{
			if (buffer==null)
				throw new ArgumentNullException("buffer", Resources.GetResourceString("ArgumentNull_Buffer"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (buffer.Length - offset < count)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
			if (!isOpen) 
				throw new InvalidOperationException("Serial Port Read - port not open");
			if (count == 0) return 0;
			
			int totalBytesRead = 0;	
			int totalCharsRead = 0;	
			int totalBytesJustRead; 
			int totalCharsJustRead; 
			int lastFullCharPos = readPos; 
			int backtrack = 0;	
			
		
			if (encoding.GetMaxByteCount(1) == 1) 
			{		
				int bytesToRead = (count < (readLen - readPos) ? count : readLen - readPos);
				 
				encoding.GetChars(inBuffer, readPos, bytesToRead, buffer, offset);
								
				readPos += bytesToRead;
				if (readPos == readLen) readPos = readLen = 0;
				return bytesToRead;
			} 
			else 
			{
				do 
				{
					backtrack = 0;
					totalBytesJustRead = count - totalCharsRead;	
					totalBytesRead += totalBytesJustRead;	
					totalCharsJustRead = encoding.GetCharCount(inBuffer, lastFullCharPos, readPos + totalBytesRead - lastFullCharPos);
					if (totalCharsJustRead > 0) 
					{
						do 
						{
							backtrack += 1;
						} while (encoding.GetCharCount(inBuffer, lastFullCharPos, readPos + totalBytesRead - lastFullCharPos - backtrack) == totalCharsJustRead);
						lastFullCharPos = readPos + totalBytesRead - backtrack + 1;	// go back to starting position of last known char.
						totalCharsRead += totalCharsJustRead;
					}
				} while (totalCharsRead < count); 
				
				int numCharsRead = encoding.GetChars(inBuffer, readPos, lastFullCharPos - readPos, buffer, offset);
				readPos = lastFullCharPos;
				
				if (readPos == readLen) readPos = readLen = 0;
				return numCharsRead;
			}
		}
		
		public int ReadByte() 
		{
			if (!isOpen) 
				throw new InvalidOperationException("Serial Port Read - port not open");
			if (readLen != readPos) 		
				return inBuffer[readPos++];	
			
			return internalSerialStream.ReadByte(); 
		}	
	
		public string ReadAvailable() 
		{
			byte [] bytesReceived = new byte[InBufferBytes];

			if (readPos < readLen)	
			{			
				Buffer.BlockCopy(inBuffer, readPos, bytesReceived, 0, readLen - readPos);
			}
			internalSerialStream.Read(bytesReceived, readLen - readPos, bytesReceived.Length - (readLen - readPos));	// get everything
			int numCharsReceived = Encoding.GetCharCount(bytesReceived);	
			int lastFullCharIndex = bytesReceived.Length;
			
			if (numCharsReceived == 0) 
			{
				Buffer.BlockCopy(bytesReceived, 0, inBuffer, 0, bytesReceived.Length);				
				readPos = 0;
				readLen = bytesReceived.Length;
				return "";
			}
				
			do 
			{
				lastFullCharIndex--;
			} while (Encoding.GetCharCount(bytesReceived, 0, lastFullCharIndex) == numCharsReceived);
			
		
			readPos = readLen = 0;
			
			Buffer.BlockCopy(bytesReceived, lastFullCharIndex + 1, inBuffer, 0, bytesReceived.Length - (lastFullCharIndex + 1));
			return Encoding.GetString(bytesReceived, 0, lastFullCharIndex + 1);
		}
	 
		public string ReadLine() 
		{
			if (!isOpen) 
				throw new InvalidOperationException("Serial Port Read - port not open");
            	
			string inBufferString;
			bool carriageReturnFlag = false;
			int startTicks = Win32API_Serial.GetTickCount();
			int lastChar;	
			int beginReadPos = readPos;

			char [] charTestBuf = new char[1];
			charTestBuf[0] = '\r';
			int crLength = encoding.GetByteCount(charTestBuf);
			charTestBuf[0] = '\n'; 
			int lfLength = encoding.GetByteCount(charTestBuf);					
			int timeUsed = 0;
			int timeNow;
			
			readLen += internalSerialStream.Read(inBuffer, readLen, InBufferBytes - (readLen - readPos));
			
			while (true) 
			{
				timeNow = Win32API_Serial.GetTickCount();
				lastChar = ReadOneChar((readTimeout == InfiniteTimeout) ? InfiniteTimeout : readTimeout - timeUsed);
				timeUsed += Win32API_Serial.GetTickCount() - timeNow;
				
				if (lastChar == -1) break;	
				
				if ((char) lastChar == '\r') 
				{	
					if (InBufferBytes == 0) 
					{
						inBufferString = encoding.GetString(inBuffer, beginReadPos, readPos - beginReadPos - crLength);
						readPos = readLen = 0;	
						return inBufferString;
					} 
					else if (carriageReturnFlag == true) 
					{
						inBufferString = encoding.GetString(inBuffer, beginReadPos, readPos - beginReadPos - 2 * crLength);
						readPos -= crLength;	
						return inBufferString;
					} 
					else 
					{
						carriageReturnFlag = true; 
					} 
				} 
				else if ((char) lastChar == '\n') 
				{
					if (carriageReturnFlag == true) 
					{
						inBufferString = encoding.GetString(inBuffer, beginReadPos, readPos - beginReadPos - crLength - lfLength);
						if (readPos == readLen) readPos = readLen = 0;
						return inBufferString;
					} 
					else 
					{
						inBufferString = encoding.GetString(inBuffer, beginReadPos, readPos - beginReadPos  - lfLength);
						if (readPos == readLen) readPos = readLen = 0;
						return inBufferString;
					}
				} 
				else 
				{
					if (carriageReturnFlag == true) 
					{
						charTestBuf[0] = (char) lastChar; 
						int lastCharLength = encoding.GetByteCount(charTestBuf);
						inBufferString = encoding.GetString(inBuffer, beginReadPos, readPos - beginReadPos - crLength - lastCharLength);
						readPos -= lastCharLength;	
						return inBufferString;
					}
				}
				
			}			
			
			readPos = beginReadPos;
			
			return (string) null; 
		}
			
		public void Write(string str) 
		{
			if (!isOpen)
				throw new InvalidOperationException("Serial Port Write - port not open!");
			if (str == null) 
				throw new ArgumentNullException("write buffer", Resources.GetResourceString("ArgumentNull_String"));
			if (str.Length == 0) return;	
			byte [] bytesToWrite;
		
			bytesToWrite = encoding.GetBytes(str);
		
			internalSerialStream.Write(bytesToWrite, 0, bytesToWrite.Length, writeTimeout);
		}
		
		public void Write(char[] buffer, int offset, int count) {
			if (!isOpen)
				throw new InvalidOperationException("Serial Port Write - port not open!");
			if (buffer == null) 
				throw new ArgumentNullException("write buffer", Resources.GetResourceString("ArgumentNull_String"));
			if (buffer.Length == 0) return;
				
			byte [] byteArray = Encoding.GetBytes(buffer,offset, count);
			Write(byteArray, 0, byteArray.Length);
			
		}
		
		public void Write(byte[] buffer, int offset, int count)
		{
			if (!isOpen)
				throw new InvalidOperationException("Serial Port Write - port not open!");
			if (buffer==null)
				throw new ArgumentNullException("buffer", Resources.GetResourceString("ArgumentNull_Buffer"));
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", Resources.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			if (buffer.Length - offset < count)
				throw new ArgumentException(Resources.GetResourceString("Argument_InvalidOffLen"));
			if (buffer.Length == 0) return;
			
			internalSerialStream.Write(buffer, offset, count, writeTimeout);
		}

		public void SetBreak()
		{
			if (!isOpen)
				throw new InvalidOperationException("SetBreak - port not open");
			internalSerialStream.SetBreak();
			inBreak = true;
		}		

		public void ClearBreak()
		{
			if (!isOpen)
				throw new InvalidOperationException("ClearBreak - port not open");
			internalSerialStream.ClearBreak();
			inBreak = false;
		}

		public void DiscardInBuffer()
		{
			if (!isOpen)
				throw new InvalidOperationException("DiscardInBuffer - port not open");
			internalSerialStream.DiscardInBuffer();
		}

		public void DiscardOutBuffer()
		{
			if (!isOpen)
				throw new InvalidOperationException("DiscardOutBuffer - port not open");
			internalSerialStream.DiscardOutBuffer();
		}

		private void CatchErrorEvents(object src, SerialEventArgs e) 
		{
			int eventsCaught = (int) e.EventType & (int) eventFilter; 
			if ((eventsCaught & (int) (SerialEvents.Frame | SerialEvents.Overrun | SerialEvents.RxOver 
				| SerialEvents.RxParity | SerialEvents.TxFull)) != 0) 
			{
				 if (ErrorEvent != null) ErrorEvent(src, e);
			}
		}
		
		private void CatchPinChangedEvents(object src, SerialEventArgs e) 
		{
			int eventsCaught = (int) e.EventType & (int) eventFilter; 
			if (((eventsCaught & (int) (SerialEvents.CDChanged | SerialEvents.CtsChanged | SerialEvents.DsrChanged | SerialEvents.Ring | SerialEvents.Break)) != 0)) 
			{
				 if (PinChangedEvent != null) PinChangedEvent(src, e);
			}	
		}
		
		private void CatchReceivedEvents(object src, SerialEventArgs e)
		{
			int eventsCaught = (int) e.EventType & (int) eventFilter; 
			int inBufferBytes = InBufferBytes;
   
			if (((eventsCaught & (int) (SerialEvents.ReceivedChars | SerialEvents.EofReceived)) != 0) && (InBufferBytes >= receivedBytesThreshold))
				if (ReceivedEvent != null) ReceivedEvent(src, e);   
		} 
		
		private void ResizeBuffer() 
		{
			Debug.Assert(inBuffer.Length >= readLen, "ResizeBuffer - readLen > inBuffer.Length");
			byte[] newBuffer = new byte[inBuffer.Length * 2];
			Buffer.BlockCopy(inBuffer, 0, newBuffer, 0, inBuffer.Length);
			inBuffer = newBuffer;
		}		

		#endregion
	}
}

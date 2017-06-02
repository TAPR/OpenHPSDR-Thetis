//=================================================================
// SDRSerialPort.cs
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
// Serial port support for PowerSDR support of CAT and serial port control  
//=================================================================

#define DBG_PRINT

using System;
using System.Threading; 
using ModifiedJustinIO; 

namespace SDRSerialSupport
{
	/// <summary>
	/// Summary description for SDRSerialPort.
	/// </summary>

	

	public class SDRSerialPort
	{
		private bool keepRecvThreadRunning = true; 
//		private bool keepXmitThreadRunning = true; 
		private CommPort commPort; 
		private bool isOpen = false; 
		private bool bitBangOnly = false; 
		private Thread recvThread = null; 
//		private Thread xmitThread = null; 
		private int recvThreadPollingInterval = 100; // fixme -- make a prop 
		private bool close_pending = false; 

		internal class recvBufStruct
		{
			private readonly uint BUF_SIZE = 500;
			private uint head;  // points to next byte to be read
			private uint tail;  // points to next byte to be written
			private byte[] buf;
			

			public recvBufStruct()
			{
				buf = new byte[BUF_SIZE];
				head = 0;
				tail = 0;
			}

			// fixme ... add overflow detectiion

			private static uint incrementWithWrap(uint counter, uint wrap_value)
			{
				++counter;
				if ( counter >= wrap_value )
				{
					counter = 0;
				}
				return counter;
			}
			

			public void waitForData() 
			{ 
				Monitor.Enter(this); 
				try 
				{ 
					Monitor.Wait(this); 
				}
				catch  ( ThreadInterruptedException ) {}
				finally 
				{
					Monitor.Exit(this); 
				}									
			} 

			public void waitForData(int msecs) 
			{
				Monitor.Enter(this); 
				try 
				{
					Monitor.Wait(this, msecs); 
				}
				catch ( ThreadInterruptedException ) {} 
				finally 
				{ 
					Monitor.Exit(this); 
				}
				// System.Console.WriteLine("waitForData msecs=" + msecs + " returns"); 
			}

			public void put(byte[] b, uint count)
			{
				if ( count <= 0 ) return; // do nothing
				
				Monitor.Enter(this); 
				try 
				{
					int i = 0;
					while  ( i < count )
					{
						buf[tail] = b[i];      // fixme -- do this with C# equiv of memcpy
						++i;
						tail = incrementWithWrap(tail, BUF_SIZE);
						if ( head == tail )  /// overflew ... bump head in front of tail
						{
							head = incrementWithWrap(head, BUF_SIZE);
						}
					}
					// Console.WriteLine("put: h=" + head + " t=" + tail);
					Monitor.PulseAll(this); // poke anyone waiting for data 
				}						
				finally 
				{
					Monitor.Exit(this); 
				}
				return;
			}

			// get bytes from the buffer  -- returns number of bytes read
			public uint get(byte[] b, uint count )
			{
				if ( count == 0 ) return 0; // no-op
				lock ( this )
				{
					if ( head == tail ) return 0;
					uint avail_bytes;
					if ( head < tail ) avail_bytes = tail - head;
					else avail_bytes = BUF_SIZE - head  + tail;
					if ( count > avail_bytes ) count = avail_bytes;
					int i = 0;
					while ( i < count )
					{
						b[i] = buf[head];
						++i;
						head = incrementWithWrap(head, BUF_SIZE);
					}
					// Console.WriteLine("get: h=" + head + " t=" + tail);
				}

				return count;
			}
		}

		private recvBufStruct recvBuf = new recvBufStruct(); // wjt fixme .. should be done @ create time not needed 
		                                                     // for bit bang mode 

		private recvBufStruct xmitBuf = new recvBufStruct(); // wjt fixme ... do @ create, not needed for bb mode 

		public enum Parity
		{
			FIRST = -1,
			NONE, ODD, EVEN, MARK, SPACE
		}

		public static Parity stringToParity(string s) 
		{
			if ( s == "none" ) return Parity.NONE; 
			if ( s == "odd" ) return Parity.ODD;
			if ( s == "even" ) return Parity.EVEN; 
			if ( s == "space" ) return Parity.SPACE; 
			if ( s == "mark" ) return Parity.MARK; 
			return Parity.NONE;  // error -- default to none
		}
		
		public enum StopBits { FIRST=-1,  ONE, ONE_AND_HALF, TWO }

		public static StopBits stringToStopBits(string s) 
		{
			if ( s == "1" ) return StopBits.ONE; 
			if ( s == "1.5" ) return StopBits.ONE_AND_HALF; 
			if ( s == "2" ) return StopBits.TWO; 
			return StopBits.ONE; // error -- default 
		}

		public enum DataBits { FIRST=-1, EIGHT, SEVEN, SIX } 

		public static DataBits stringToDataBits(string s) 
		{
			if ( s == "8" ) return DataBits.EIGHT; 
			if ( s == "7" ) return DataBits.SEVEN; 
			if ( s == "6" ) return DataBits.SIX; 
			return DataBits.EIGHT; 
		}


		public SDRSerialPort(int portidx)
		{
			commPort = new CommPort(); 
			commPort.PortNum = portidx; 

			// for comm emul these don't matter -- need to externalize for real hardware 
			commPort.Parity = 0; 
			commPort.StopBits = 2; 
			commPort.ByteSize = 8; 
			commPort.BaudRate = 1200; 
			// commPort.ReadTimeout = 50; 
		}


		public void setVirtual(bool is_virtual) 
		{
			if ( commPort.Opened ) return; // no changing once we're running 
			commPort.isVirtualPort = is_virtual;; 
		}
		// set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
		// 
		public void setCommParms(int baudrate, Parity p, DataBits data, StopBits stop)  
		{ 
			if ( commPort.Opened ) return; // bail out if it's already open 
			if ( commPort.isVirtualPort ) return; // bail out -- makes no sense for mixw virt port 

			commPort.BaudRate = baudrate; 
			byte parity_as_int; 
			switch ( p ) 
			{ 
				case Parity.NONE: 
					parity_as_int = 0; 
					break; 
				case Parity.ODD: 
					parity_as_int = 1; 
					break; 
				case Parity.EVEN:
					parity_as_int = 2; 
					break; 
				case Parity.MARK: 
					parity_as_int = 3; 
					break; 
				case Parity.SPACE:
					parity_as_int = 4; 
					break; 
				default: 
					parity_as_int = 0; 
					break; 
			}
			commPort.Parity = parity_as_int; 
			byte data_bits_as_int; 
			switch ( data ) 
			{
				case DataBits.EIGHT: 
					data_bits_as_int = 8; 
					break;
				case DataBits.SEVEN:
					data_bits_as_int = 7; 
					break; 
				case DataBits.SIX: 
					data_bits_as_int = 6; 
					break; 
				default: 
					data_bits_as_int = 8; 
					break; 
			}
			commPort.ByteSize = data_bits_as_int; 
			byte stop_bits_as_int; 
			switch ( stop ) 
			{
				case StopBits.ONE: 
					stop_bits_as_int = 0; 
					break; 
				case StopBits.ONE_AND_HALF: 
					stop_bits_as_int = 1; 
					break; 
				case StopBits.TWO: 
					stop_bits_as_int = 2; 
					break; 
				default: 
					stop_bits_as_int = 0; 
					break; 
			}
			commPort.StopBits = stop_bits_as_int; 
		}
			

		// runs sucking data out of the comm port and into the recv buffer. 
		// sort of a hack but needs to be polling because ComEmulDrv ports do not 
		// seem to work w/ events. 
		//  this thread is now somewhat misnamed as it handles both reading and writing 
		//  Read and write need to be run on the same thread as Windows does not seem to be 
		// able to handle read on one thread and write on another without deadlocking from time 
		// to time.  
		// not a big problem .. we set the comm port to not block and then alternate reading and 
		// writing data from the port. 
		private void RecvThread()
		{
			
			byte[] recv_buf = new byte[100];  // 20 bytes should be enough
			byte[] xmit_buf = new byte[100]; 
			int num_read;
			uint num_to_write; 
			int num_written;
#if DEBUG
			Console.WriteLine("SDRSerialPort.RecvThread alive and well"); 
#endif
			while ( keepRecvThreadRunning ) { 			
				try 
				{
					num_read = commPort.Read(recv_buf, 100); // fixme -- error checking 							
					// Console.WriteLine("numread=" + num_read);
					if ( num_read > 0 ) recvBuf.put(recv_buf, (uint)num_read);			
					num_to_write = xmitBuf.get(xmit_buf,100); 
					if ( num_to_write > 0 ) 
					{ 
						num_written = commPort.Write(xmit_buf, (int)num_to_write); 					
					} 
					if ( keepRecvThreadRunning && !close_pending ) xmitBuf.waitForData(recvThreadPollingInterval); 
					// Thread.Sleep(recvThreadPollingInterval);
				}
				catch ( ThreadInterruptedException ) {} 
			}
			if ( close_pending ) 
			{
				commPort.Close(); 
			}
#if DEBUG				 
			Console.WriteLine("SDRSerialPort.RecvThread dies"); 
#endif
		}


//		// pump chars out the serial port while we have them 
//		private void XmitThread()
//		{			
//			byte[] xmit_buf = new byte[100]; 
//
//			int num_written;
//			uint num_to_write; 
//			Console.WriteLine("SDRSerialPort.XmitThread alive and well"); 
//			try 
//			{ 
//				while ( keepXmitThreadRunning )
//				{
//					num_to_write = xmitBuf.get(xmit_buf, 100); 
//					if ( num_to_write > 0 ) 
//					{
//						num_written = commPort.Write(xmit_buf, (int)num_to_write); 					
//						// Console.WriteLine("write: " + num_written + " " + num_to_write); 
//					}
//					if ( keepXmitThreadRunning ) xmitBuf.waitForData();				
//				}				
//				if ( close_pending ) 
//				{ 
//					commPort.Close(); 
//				} 
//			}
//			catch ( ThreadAbortException taex ) 
//			{
//				Console.WriteLine("SDRSerialPort.XmiThread - ThreadAbortException"); 
//				commPort.Close(); 
//			}
//			Console.WriteLine("SDRSerialPort.XmitThread dies"); 
//		}
//
		public uint get(byte[] b, uint space_avail)
		{
			if ( !isOpen ) return 0; // fixme error checking 
			if ( bitBangOnly ) return 0; // fixme -- throw exception? 
			return recvBuf.get(b, space_avail);
		}


		// 
		// this enqueues chars to be sent by the xmit thread. 
		// wjt fixme ... for the virt ports we don't really need the write thread... perhaps if it's using 
		// a virt port we should just write it directly?? 
		// 
		public uint put(byte[] b, uint count) 
		{
			if ( bitBangOnly ) return 0;  // fixme -- throw exception? 
			xmitBuf.put(b, count); 

			return count; // wjt fixme -- hack -- we don't know if we actually wrote things 

			// return (uint)commPort.Write(b, (int)count); 
		}

		public int Create()
		{
			return Create(false); 
		}
		

		// create port -- start recv thread if we're not bit bang only 
		public int Create(bool bit_bang_only) 
		{ 
			bitBangOnly = bit_bang_only; 
			if ( isOpen )
			{			
				return -1;
			}
			commPort.Open();  // fixMe -- error check 
			isOpen = true; 
		
			if ( !bit_bang_only ) 
			{ 							
				// start receive thread
				recvThread = new Thread(new ThreadStart(RecvThread));
				recvThread.Name = "SDRSerialSupport.recvThread.COM" + commPort.PortNum; 
				recvThread.IsBackground = true; 
				recvThread.Start();

				// and the xmit thread
//				xmitThread = new Thread(new ThreadStart(XmitThread)); 
//				xmitThread.Name = "SDRSerialSupport.xmitThread.COM" + commPort.PortNum; 
//				xmitThread.IsBackground = true; 
//				xmitThread.Start(); 
			}
			return 0; // all is well
		}
				  
				  
		public void Destroy()
		{
			keepRecvThreadRunning = false; 
//			keepXmitThreadRunning = false; 
			if ( bitBangOnly ) 
			{ 
				commPort.Close(); 
			}
			else 
			{ 
				// for not bitbang port let the recvThread close it 
				close_pending = true; 
			}
//			if ( recvThread != null ) 
//			{ 
//				recvThread.Interrupt(); 
//				System.Console.WriteLine("calling abort on recvThread"); 
//				recvThread.Abort(); 
//			}
//			if ( xmitThread != null ) 
//			{
//				if ( xmitBuf != null ) 
//				{
//
//					Monitor.Enter(xmitBuf); 
//					try 
//					{ 
//						Monitor.PulseAll(xmitBuf); 
//					}
//					finally 
//					{
//						Monitor.Exit(xmitBuf); 
//					}
//				}
//				xmitThread.Interrupt(); 			
//				System.Console.WriteLine("calling abort on xmitThread"); 
//				xmitThread.Abort(); 
//			}
//			if ( recvThread != null ) 
//			{ 
//				System.Console.WriteLine("calling Join on recvThread"); 
//				recvThread.Join(); 
//				System.Console.WriteLine("recvThread join returned"); 
//			}
//			if ( xmitThread != null ) 
//			{
//				System.Console.WriteLine("calling Join on xmitThread"); 
//				xmitThread.Join(); 
//				System.Console.WriteLine("xmitThread join returned"); 
//			}

			isOpen = false; 			
		}		

		public bool isCTS() 
		{ 		
			if ( !isOpen ) return false; // fixme error check 
			return commPort.isCTS(); 			
		}

		public bool isDSR() 
		{
			if ( !isOpen ) return false; // fixme error check 
			return commPort.isDSR(); 
			
		}
		public bool isRI()
		{
			if ( !isOpen ) return false; // fixme error check 
			return commPort.isRI(); 
		}

		public bool isRLSD() 
		{
			if ( !isOpen ) return false; // fixme error check 
			return commPort.isRLSD(); 
		}
	}
}

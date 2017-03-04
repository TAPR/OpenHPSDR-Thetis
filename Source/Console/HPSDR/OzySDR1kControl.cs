/*
*
* Copyright (C) 2007 Bill Tracey, KD5TFD 
*
* Copyright (C) 2006 Philip A. Covington, N8VB
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
 


// 
// This class provides access to FX2 routines to control an SDR1k connected to the 25 pin port on Ozy 
// 

namespace Thetis
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
   // using HPSDR_USB_LIB_V1;

	public class OzySDR1kControl
	{
		// the following set of declarations MUST match the values used in the FX2 code - hpsdr_commands.h 
		public static readonly byte VRQ_SDR1K_CTL = 0x0d; 
		// private static readonly byte SDR1KCTRL_SET_DATA_REG = 0x1; 
		public static readonly byte SDR1KCTRL_LATCH = 0x2; 
		public static readonly byte SDR1KCTRL_DDS_RESET = 0x3;
		public static readonly byte SDR1KCTRL_DDS_WRITE = 0x4; 
		public static readonly byte SDR1KCTRL_SR_LOAD = 0x5; 
		public static readonly byte SDR1KCTRL_READ_STATUS = 0x6; 
		public static readonly byte SDR1KCTRL_READ_VERSION = 0x7; 
		public static readonly byte VRT_VENDOR_IN	= 0xC0; 
		public static readonly byte VRT_VENDOR_OUT	= 0x40; 

		// these must match the declarations in sdr1kctl.c used in the fx2 code 
		public static readonly byte LATCH_EXT = 0x1;
		public static readonly byte LATCH_BPF = 0x2;
		public static readonly byte LATCH_DAT = 0x4; 
		public static readonly byte LATCH_ADR = 0x8;

		// instance vars 
		private IntPtr usbDevHandle; 
		private static OzySDR1kControl Singleton = null; 
		// private static readonly int OZY_VID = 0xfffe; 
		// private static readonly int OZY_PID = 0x7; 
		private bool hasRFE; 
		private bool hasPA; 

		private OzySDR1kControl()
		{
		
		}
	
		private int Open(string appName, uint config) { return 0; }
		public static int Close() { return 0; }

		private static void checkSingleton() 
		{ 
			if ( Singleton == null ) 
			{ 
				throw new ApplicationException("Ozy interface not initialized"); 
			} 
			return; 
		} 

		public static int Latch(byte latch, byte data) 
		{
			checkSingleton(); 
			return Singleton.latch(latch, data); 
		} 


		public static int DDSReset() 
		{
			checkSingleton(); 
			return Singleton.dds_reset();
		}


		public static int DDSWrite(byte addr, byte data) 
		{
			checkSingleton(); 
			return Singleton.dds_write(addr, data); 
		}


		public static int SRLoad(byte reg, byte data) 
		{
			checkSingleton(); 
			return Singleton.sr_load(reg, data); 
		}

		public static int GetStatusPort() { 
			checkSingleton(); 
			return Singleton.get_status_port(); 			
		}


		public static int GetADC() {
			return 0; 
		}
		
#if false 
		private int dbgGate = 0; 
		private int read_ok_count = 0; 
		private int read_notok_count = 0; 
#endif 

		// the following two arrays need to be in sync -- used to remap PIO status bit from 
		// Ozy to the parallel port format PowerSDR expects 
		private static readonly byte[] StatusSwizzle_inMask = { 0x01, 0x02, 0x04, 0x08, 0x10 } ;
		private static readonly byte[] StatusSwizzle_outBit = { 0x40, 0x80, 0x20, 0x10, 0x08 } ;

		// Ozy's GPIO status bits do not match the ordering of bits one gets from a PC parralel 
		// port which PowerSDR is exepcting - so map them into the order that PowerSDR is expecting
		private static int swizzleStatusBits(int bits_in) 
		{
			int bits_out = 0; 
			for ( int i = 0; i < StatusSwizzle_inMask.Length; i++ ) 
			{ 
				if ( (bits_in & StatusSwizzle_inMask[i]) != 0 ) 
				{
					bits_out |= (int)((uint)StatusSwizzle_outBit[i]); 
				}
			}
			// invert S7 
			if ( (bits_out & 0x80) != 0 ) 
			{ 
				bits_out &= 0x7f; 
			}
			else 
			{ 
				bits_out |= 0x80; 
			} 
			return bits_out; 
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private int get_status_port() 
		{
			byte[] buf = new byte[1];

            int rc = JanusAudio.WriteControlMsg(usbDevHandle,
                 VRT_VENDOR_IN,
                 VRQ_SDR1K_CTL,
                 SDR1KCTRL_READ_STATUS,
                 0,
                 buf,
                 buf.Length,
                 1000);
			if ( rc == buf.Length ) 
			{ 
				rc = (int)((uint)(buf[0])); 				
				rc = swizzleStatusBits(rc); 
#if false 
				++read_ok_count; 
#endif 
			}
			else 
			{ 
				rc = -1;
#if false 
				++read_notok_count; 
#endif 
				
			} 
			// Thread.Sleep(10); 
#if false 
			if ( dbgGate == 100 ) 
			{ 
				System.Console.WriteLine("get_status: data: 0x" + rc.ToString("X") + " read_ok: " + read_ok_count + " read_not_ok: " + read_notok_count); 				
			}
			++dbgGate; 
			if ( dbgGate >= 101 ) 
			{
				dbgGate = 0; 
				read_ok_count = 0; 
				read_notok_count = 0; 
			}
#endif 
			return rc;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private int latch(byte latchid, byte data) 
		{

            int rc = 0; JanusAudio.WriteControlMsg(usbDevHandle,
                                                       VRT_VENDOR_OUT,
                                                       VRQ_SDR1K_CTL,
                                                       SDR1KCTRL_LATCH,
                                                       (latchid << 8) | data,
                                                       null,
                                                       0,
                                                       1000);
			// Thread.Sleep(10); 
			// System.Console.WriteLine("latch: latchid: " + latchid + " data: " + data + " rc: " + rc); 
			return rc; 		
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private int dds_reset() 
		{
            int rc = JanusAudio.WriteControlMsg(usbDevHandle,
                 VRT_VENDOR_OUT,
                 VRQ_SDR1K_CTL,
                 SDR1KCTRL_DDS_RESET,
                 0,
                 null,
                 0,
                 1000);
			// Thread.Sleep(10); 
			// System.Console.WriteLine("dds_reset: rc: " + rc); 
			return rc; 		
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private int dds_write(byte addr, byte data) 
		{
            int rc = JanusAudio.WriteControlMsg(usbDevHandle,
                 VRT_VENDOR_OUT,
                 VRQ_SDR1K_CTL,
                 SDR1KCTRL_DDS_WRITE,
                 (addr << 8) | data,
                 null,
                 0,
                 1000);
			// Thread.Sleep(10); 
			// System.Console.WriteLine("dds_write: addr:" + addr + " data: " + data + " rc: " + rc); 
			return rc;
		} 

		[MethodImpl(MethodImplOptions.Synchronized)]
		private int sr_load(byte reg, byte data) 
		{
            int rc = JanusAudio.WriteControlMsg(usbDevHandle,
                 VRT_VENDOR_OUT,
                 VRQ_SDR1K_CTL,
                 SDR1KCTRL_SR_LOAD,
                 (reg << 8) | data,
                 null,
                 0,
                 1000);
			// Thread.Sleep(10); 
			// System.Console.WriteLine("sr_load: reg:" + reg + " data: " + data + " rc: " + rc); 
			return rc;			
		} 

		private bool init(bool rfe, bool pa) 
		{
			hasRFE = rfe; 
			hasPA = pa; 

			JanusAudio.initOzy(); 

			IntPtr OzyHandle = JanusAudio.OzyOpen(); 
			if ( OzyHandle != (IntPtr)0 ) 
			{ 
				usbDevHandle =  JanusAudio.OzyHandleToRealHandle(OzyHandle); 
			}
			else 
			{
				usbDevHandle = IntPtr.Zero; 
			}

			if ( usbDevHandle.Equals(IntPtr.Zero) ) 
			{
				System.Console.WriteLine("Ozy init fails"); 
				return false; 
			}
			/* else */
			// System.Console.WriteLine("Ozy init succeeds"); 
			return true; 
		}

		// returns true on success, false otherwise 
		public static bool Init(bool rfe, bool pa) 
		{
			if ( Singleton != null ) 
			{ 
				System.Console.WriteLine("warning: OzySDR1kControl - already initialized\n"); 
				return true; 
			} 
			/* else  */ 
			Singleton = new OzySDR1kControl(); 

			bool result =  Singleton.init(rfe, pa); 
			if ( result == false ) 
			{ 
				Singleton = null; 
			}
			// System.Console.WriteLine("OzyControl: Init returns: " + result); 
			return result; 
		}

		public static void Exit() {}
	}
}

/* 	This file is part of a program that implements a Software-Defined Radio.
	The code in this file is derived from routines originally written by
	Pierre-Philippe Coupard for his CWirc X-chat program. That program
	is issued under the GPL and is
	Copyright (C) Pierre-Philippe Coupard - 18/06/2003
	This derived version is
	Copyright (C) 2004-2009 by Frank Brickle, AB2KT and Bob McGwier, N4HY

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

	The authors can be reached by email at

	ab2kt@arrl.net
	or
	rwmcgwier@comcast.net

	or by paper mail at

	The DTTS Microwave Society
	6 Kathleen Place
	Bridgewater, NJ 08807
*/

namespace PowerSDR
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using System.IO.Ports;

	public class CWKeyer2
	{
		#region Variables and Properties

		public Thread keyer_thread, cw_tone_thread;
        private bool threads_running = false;
		private int keyermode = 0;
		public int KeyerMode
		{
			get { return keyermode; }
			set
			{
				keyermode = value;
				RadioDSP.KeyerIambicMode = keyermode;
			}
		}
		private HiPerfTimer timer;
		private float msdel;
		private Console console;

		private KeyerLine secondary_ptt_line = KeyerLine.NONE;
		public KeyerLine SecondaryPTTLine
		{ 
			get { return secondary_ptt_line; }
			set
			{
				secondary_ptt_line = value;
			}
		}
		private SIOListenerII siolisten;
		public SIOListenerII Siolisten 
		{
			get { return siolisten; }
			set 
			{
				siolisten = value;
			}
		}
		private bool cat_enabled= false; 
		public bool CATEnabled 
		{
			set { cat_enabled = value; }
		}

		private KeyerLine secondary_key_line = KeyerLine.NONE;
		public KeyerLine SecondaryKeyLine
		{
			get { return secondary_key_line; }
			set
			{
				secondary_key_line = value;
				switch(secondary_key_line)
				{
					case KeyerLine.NONE:
						break;
					case KeyerLine.DTR:
						DttSP.SetWhichKey(1);
						SP2DotKey = true;
						break;
					case KeyerLine.RTS:
						SP2DotKey = false;
						DttSP.SetWhichKey(0);
						break;
				}
			}
		}

		private bool auto_switch_mode = false;
		public bool AutoSwitchMode
		{
			get { return auto_switch_mode; }
			set { auto_switch_mode = value; }
		}

		private DSPMode rx1_dsp_mode = DSPMode.LSB;
		public DSPMode RX1DSPMode
		{
			get { return rx1_dsp_mode; }
			set { rx1_dsp_mode = value; }
		}

		private bool fwc_dot = false;
		public bool FWCDot
		{
			get { return fwc_dot; }
			set	
			{
				fwc_dot = value;
				if(value && auto_switch_mode)
				{
					switch(rx1_dsp_mode)
					{
						case DSPMode.CWL:
						case DSPMode.CWU:
							break;
						case DSPMode.LSB:
						case DSPMode.DIGL:
							console.RX1DSPMode = DSPMode.CWL;
							break;
						default:
							console.RX1DSPMode = DSPMode.CWU;
							break;
					}
				}
			}
		}

		private bool fwc_dash = false;
		public bool FWCDash
		{
			get { return fwc_dash; }
			set 
			{
				fwc_dash = value;
				if(value && auto_switch_mode)
				{
					switch(rx1_dsp_mode)
					{
						case DSPMode.CWL:
						case DSPMode.CWU:
							break;
						case DSPMode.LSB:
						case DSPMode.DIGL:
							console.RX1DSPMode = DSPMode.CWL;
							break;
						default:
							console.RX1DSPMode = DSPMode.CWU;
							break;
					}
				}
			}
		}

		private bool memorykey = false;
		public bool MemoryKey
		{
			get { return memorykey; }
			set {memorykey = value; }
		}

		private bool memoryptt = false;
		public bool MemoryPTT
		{
			get { return memoryptt; }
			set { memoryptt = value; }
		}

		private HW hw;
		public HW Hdw 
		{
			set { hw = value; }
			get { return hw ; }
		}

		private string primary_conn_port = "Radio";
		public string PrimaryConnPort
		{
			get { return primary_conn_port; }
			set
			{
                if (primary_conn_port == "Radio" && hw == null)
                    hw = console.Hdw;
                    
				primary_conn_port = value;
				switch(primary_conn_port)
				{
					case "Radio":
					case "SDR":                        
						if (sp.IsOpen) sp.Close();	
						break;
                    case "Ozy/Hermes":  // fpga does not need to be opened or closed 
                        break;
                    default:
						if (sp.IsOpen) sp.Close();
						sp.PortName = primary_conn_port;
						try
						{
							sp.Open();
							sp.DtrEnable=true;
#if true 
							sp.RtsEnable=true;
#else
							// wjtFIXME! - sr xmit support 
							sp.RtsEnable=false; // kd5tfd changed this to false for soft rock xmit support 
#endif 

						}
						catch(Exception) 
						{
							MessageBox.Show("Primary Keyer Port ["+primary_conn_port+"] could not be opened.");	
							primary_conn_port = "Radio";
						}
						break;
				}
			}
		}

		private string secondary_conn_port = "None";
		public string SecondaryConnPort
		{
			get { return secondary_conn_port; }
			set
			{
				secondary_conn_port = value;
				switch(secondary_conn_port)
				{
					case "None":
						if (sp2.IsOpen) sp2.Close();
						break;
					case "CAT":
						if(!cat_enabled)
							MessageBox.Show("CAT was selected for the Keyer Secondary Port, but CAT is not enabled.");

						break;
#if false
					// wjtFIXME!! - merged from KD5TFD's HPSDR 1.6.3 tree - sr xmit 
					case "BB-PTT": 
						Console c = Console.getConsole(); 	
						if ( c != null ) 
						{ 
							if ( ! c.PTTBitBangEnabled || c.serialPTT == null ) 
							{ 
								MessageBox.Show("Bit Bang PTT was selected for the Keyer Secondary Port, but Bit Bang PTT is not enabled.");
							}						}
						break; 
#endif 

					default: // COMx
						if(sp2.IsOpen) sp2.Close();
						sp2.PortName = secondary_conn_port;
						try
						{
							sp2.Open();
							sp2.DtrEnable=true;
							sp2.RtsEnable=true;
						}
						catch(Exception) 
						{
							MessageBox.Show("Comport for keyer program could not be opened\n");
							secondary_conn_port = "None";
						}
						break;
				}
			}
		}
		
		private bool sp2dotkey = false;
		public bool SP2DotKey
		{
			set {sp2dotkey = value;}
			get {return sp2dotkey;}
		}

		private bool keyerptt = false;
		public bool KeyerPTT
		{
			get { return keyerptt; }
		}

		public SerialPort sp = new SerialPort(); 
		public SerialPort sp2 = new SerialPort();

		#endregion

		#region Constructor and Destructor

		private bool SetThreadAffinity(int cpu)
		{
			try
			{
				int ret = Win32.SetThreadAffinityMask(Win32.GetCurrentThread(), new IntPtr(1 << cpu));
				if(ret == 0) return false;
			} 
			catch (Exception)
			{ 
				return false;
			}
			return true;
		}

		public CWKeyer2(Console c)
		{
			console = c;
			//if(console.CurrentModel == Model.SDR1000)
                hw = console.Hdw;
			siolisten = console.Siolisten;
			Thread.Sleep(50);
			DttSP.NewKeyer(600.0f, true, 0.0f, 3.0f, 25.0f, (float)Audio.SampleRate1);
			RadioDSP.KeyerIambicMode = 0;
			Thread.Sleep(50);

            threads_running = true;

			cw_tone_thread = new Thread(new ThreadStart(DttSP.KeyerSoundThread));
            cw_tone_thread.Name = "CW Sound Thread";
            cw_tone_thread.Priority = ThreadPriority.Highest;
            cw_tone_thread.IsBackground = true;

            cw_tone_thread.Start();
			
			
			keyer_thread  = new Thread(new ThreadStart(KeyThread));
            keyer_thread.Name = "CW KeyThread";
            keyer_thread.Priority = ThreadPriority.Highest;
            keyer_thread.IsBackground = true;
            keyer_thread.Start();

			timer = new HiPerfTimer();			
		}
		
		 ~CWKeyer2()
		{
			// Destructor logic here, make sure threads cleaned up
			DttSP.StopKeyer();
			Thread.Sleep(50);
            threads_running = false;
			Thread.Sleep(50);
			DttSP.DeleteKeyer();
		}

		#endregion

		#region Thread Functions

		public void KeyThread()
		{
			//SetThreadAffinity(1);
			bool extkey_dash, extkey_dot, keyprog;
			
			do 
			{
				DttSP.KeyerStartedWait();
				for(;DttSP.KeyerRunning();) 
				{
					keyprog = false;
					timer.Start();
					DttSP.PollTimerWait();
					switch(primary_conn_port)
					{
						case "SDR":
							byte b = hw.StatusPort();
							extkey_dash = ((b & (byte)StatusPin.Dash) != 0);
							extkey_dot  = ((b & (byte)StatusPin.Dot) != 0);
							break;

						case "Radio":
							extkey_dot = fwc_dot;
							extkey_dash = fwc_dash;
							break;

                        case "Ozy/Hermes":
                            //int tmp = JanusAudio.GetDotDashPTT();
                            // System.Console.WriteLine("dd: " + tmp); 
                            extkey_dot = fwc_dot; // ((tmp & 0x4) != 0);
                            extkey_dash = fwc_dash; // ((tmp & 0x2) != 0);                           
                            break;

                        default: // COM port
							extkey_dash = sp.CtsHolding;
							extkey_dot  = sp.DsrHolding;
							break;
					}

					// handle CWX
					if(!extkey_dash && !extkey_dot)
					{
						if (memoryptt) 
						{
							//console ptt on
							keyprog = true;
							extkey_dot = extkey_dash = memorykey;
						} 
						else 
						{
							//console ptt off
							keyprog = false;							
						}
					}
					
					if (!extkey_dash && !extkey_dot) // don't override primary
					{
						switch(secondary_conn_port)
						{
							case "None":
								break;
							case "CAT":							
							switch(secondary_ptt_line)
							{
								case KeyerLine.NONE:
									if (sp2dotkey) extkey_dash  = siolisten.SIO.isDSR();
									else extkey_dot = siolisten.SIO.isCTS();
									break;
								case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
									keyerptt = siolisten.SIO.isDSR();
									//										extkey_dot = System.Convert.ToByte(sp2.CtsHolding);
									break;
								case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
									keyerptt = siolisten.SIO.isCTS();
									//										extkey_dash  = System.Convert.ToByte(sp2.DsrHolding);
									break;								
							}

							switch(secondary_key_line)
							{
								case KeyerLine.NONE:
									if (sp2dotkey) extkey_dash  = siolisten.SIO.isDSR();
									else extkey_dot = siolisten.SIO.isCTS();
									break;
								case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
									extkey_dot = siolisten.SIO.isDSR();
									//										Debug.WriteLine("extkey_dot: "+extkey_dot);
									break;
								case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
									extkey_dash = siolisten.SIO.isCTS();
									break;
							}
								
								//								if (extkey_dash || extkey_dot)
								//									keyprog = true;
								//								else keyprog = false;
								//								//Debug.WriteLine("keyprog: "+keyprog);
								break;

#if false
						// wjtFIXME!! - merged from KD5TFD's HPSDR 1.6.3 tree - sr xmit 
						case "BB-PTT":
							Console c = Console.getConsole();
							
							
							if ((extkey_dash==0) && (extkey_dot == 0)) // don't override primary
							{
								switch(secondary_ptt_line)
								{
									case KeyerLine.NONE:
										if (sp2dotkey) extkey_dash  = System.Convert.ToByte(c.serialPTT.isDSR() );
										else extkey_dot = System.Convert.ToByte(c.serialPTT.isCTS() );
										break;
									case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
										keyerptt =   c.serialPTT.isDSR();
										//										extkey_dot = System.Convert.ToByte(sp2.CtsHolding);
										break;
									case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
										keyerptt = c.serialPTT.isCTS();
										//										extkey_dash  = System.Convert.ToByte(sp2.DsrHolding);
										break;								
								}

								switch(secondary_key_line)
								{
									case KeyerLine.NONE:
										if (sp2dotkey) extkey_dash  = System.Convert.ToByte(c.serialPTT.isDSR());
										else extkey_dot = System.Convert.ToByte(c.serialPTT.isCTS());
										break;
									case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
										extkey_dot = System.Convert.ToByte(c.serialPTT.isDSR());
										//										Debug.WriteLine("extkey_dot: "+extkey_dot);
										break;
									case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
										extkey_dash = System.Convert.ToByte(c.serialPTT.isCTS());
										break;
								}

								if ((extkey_dash+extkey_dot) != 0)
									keyprog=1;
								else keyprog=0;
								//								Debug.WriteLine("keyprog: "+keyprog);
							} 
							else keyprog = 0;
							break;
#endif 

							default: // comm port
							switch(secondary_ptt_line)
							{
								case KeyerLine.NONE:
									if (sp2dotkey) extkey_dash  = sp2.DsrHolding;
									else extkey_dot = sp2.CtsHolding;
									break;
								case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
									keyerptt = sp2.DsrHolding;
									break;
								case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
									keyerptt = sp2.CtsHolding;
									break;								
							}

							switch(secondary_key_line)
							{
								case KeyerLine.NONE:
									if (sp2dotkey) extkey_dash  = sp2.DsrHolding;
									else extkey_dot = sp2.CtsHolding;
									break;
								case KeyerLine.DTR: // look at DSR since we are on the other side of the null modem cable
									extkey_dot = sp2.DsrHolding;
									//										Debug.WriteLine("extkey_dot: "+extkey_dot);
									break;
								case KeyerLine.RTS: // look at CTS since we are on the other side of the null modem cable
									extkey_dash = sp2.CtsHolding;
									break;
							}
								
								break;
						}
						if (extkey_dash || extkey_dot)
							keyprog = true;
						else keyprog = false;
						//Debug.WriteLine("keyprog: "+keyprog);
					}
					timer.Stop();
					msdel = (float)timer.DurationMsec;
					//Debug.WriteLine("Dash: "+extkey_dash+" Dot: "+extkey_dot);
					DttSP.KeyValue(msdel, extkey_dash, extkey_dot, keyprog);
				}
			} while(threads_running);
		}

		#endregion
	}
}

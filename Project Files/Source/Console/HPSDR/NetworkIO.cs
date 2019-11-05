/*
*
* Copyright (C) 2006 Bill Tracey, KD5TFD, bill@ewjt.com 
* Copyright (C) 2010-2016  Doug Wigley
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

namespace Thetis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Net.NetworkInformation;

    partial class NetworkIO
    {
        public NetworkIO()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        public static bool isFirmwareLoaded = false;

        //private static void dummy_to_remove_warning() // added to remove fallacious warning about variable never being used
        //{
        //    isFirmwareLoaded = isFirmwareLoaded;
        //}

        // get ozy firmware version string - 8 bytes.  returns 
        // null for error 
        //private static string getOzyFirmwareString()
        //{
        //    IntPtr oz_h = OzyOpen();

        //    if (oz_h == (IntPtr)0)
        //    {
        //        return null;
        //    }
        //    IntPtr usb_h = JanusAudio.OzyHandleToRealHandle(oz_h);
        //    if (usb_h == (IntPtr)0)
        //    {
        //        JanusAudio.OzyClose(oz_h);
        //        return null;
        //    }

        //    byte[] buf = new byte[8];
        //    // int rc = WriteControlMsg(usb_h, 
        //    //OzySDR1kControl.VRT_VENDOR_IN, //0xC0
        //    // OzySDR1kControl.VRQ_SDR1K_CTL, //0x0d
        //    // OzySDR1kControl.SDR1KCTRL_READ_VERSION, // 0x7
        //    // 0, buf, buf.Length, 1000); 

        //    int rc = GetOzyID(usb_h, buf, buf.Length);

        //    // System.Console.WriteLine("read version rc: " + rc); 

        //    string result = null;

        //    if (rc == 8)    // got length we expected 
        //    {
        //        char[] cbuf = new char[8];
        //        for (int i = 0; i < 8; i++)
        //        {
        //            cbuf[i] = (char)(buf[i]);
        //        }
        //        result = new string(cbuf);
        //        System.Console.WriteLine("version: >" + result + "<");
        //    }
        //    JanusAudio.OzyClose(oz_h);
        //    return result;
        //}

        public static string setC1Opts(string opt)
        {
            int bits;
            int off_mask = 0xff;
            int on_mask = 0;

            string result = null;

            switch (opt)
            {
                case "--Atlas10MHz":
                    off_mask = 0xf3;  // 11110011
                    on_mask = 0;       // 10 meg atlas == 00xx
                    result = "Atlas10";
                    break;

                case "--Penny10MHz":
                    off_mask = 0xf3;  // 11110011
                    on_mask = 0x4;      // 10 meg penny == 01xx 
                    result = "Penny10";
                    break;

                case "--Mercury10Mhz":
                    off_mask = 0xf3;  // 11110011
                    on_mask = 0x8;      // 10 meg merc == 10xx 
                    result = "Merc10";
                    break;

                case "--Mercury125MHz":
                    off_mask = 0xef;     // 11101111
                    on_mask = 0x10;
                    result = "Merc125";
                    break;

                case "--CfgPenny":
                    off_mask = 0x9f;     // 10011111
                    on_mask = 0x20;
                    result = "CfgPenny";
                    break;

                case "--CfgMercury":
                    off_mask = 0x9f;     // 10011111
                    on_mask = 0x40;
                    result = "CfgMerc";
                    break;

                case "--CfgBoth":
                    off_mask = 0x9f;     // 10011111
                    on_mask = 0x60;
                    result = "CfgBoth";
                    break;

                case "--PennyMic":
                    off_mask = 0x7f;     // 01111111
                    on_mask = 0x80;
                    result = "PennyMic";
                    break;
            }

            bits = GetC1Bits();
            bits &= off_mask;
            bits |= on_mask;
            SetC1Bits(bits);
            return result;
        }

        private static string fx2_fw_version = "n/a";

        public static string getFX2FirmwareVersionString()
        {
            return fx2_fw_version;
        }

        private static float swr_protect = 1.0f;
        public static float SWRProtect
        {
            get { return swr_protect; }
            set { swr_protect = value; }
        }

        public static void SetOutputPower(float f)
        {
            if (f < 0.0)
            {
                f = 0.0F;
            }
            if (f >= 1.0)
            {
                f = 1.0F;
            }

            int i = (int)(255 * f * swr_protect);
            //System.Console.WriteLine("output power i: " + i); 
            SetOutputPowerFactor(i);
        }

         // get the name of this PC and, using it, the IP address of the first adapter
        //static string strHostName = Dns.GetHostName();
        // public static IPAddress[] addr = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        // get a socket to send and receive on
        static Socket socket; // = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // set an endpoint
        static IPEndPoint iep;
        static byte[] data = new byte[1444];
        const int DiscoveryPort = 1024;
        const int LocalPort = 0;
        public static bool enableStaticIP = false;
        public static uint static_host_network = 0;
        public static bool FastConnect = false;
        public static HPSDRHW BoardID = HPSDRHW.Hermes;
        public static byte FWCodeVersion = 0;
        public static string EthernetHostIPAddress = "";
        public static int EthernetHostPort = 0;
        public static string HpSdrHwIpAddress = "";
        public static string HpSdrHwMacAddress = "";
        public static byte NumRxs = 0;
        private const int IP_SUCCESS = 0;
        private const short VERSION = 2;
        public static int initRadio()
        {
            int rc;
           // System.Console.WriteLine("Static IP: " + Console.getConsole().HPSDRNetworkIPAddr);
            int adapterIndex = adapterSelected - 1;
            IPAddress[] addr = null;
            bool cleanup = false;

            try
            {
                addr = Dns.GetHostAddresses(Dns.GetHostName());
            }
            catch (SocketException e)
            {
                Win32.WSAData data = new Win32.WSAData();
                int result = 0;

                result = Win32.WSAStartup(VERSION, out data);
                if (result != IP_SUCCESS)
                {
                    System.Console.WriteLine(data.description);
                    Win32.WSACleanup();
                }

                addr = Dns.GetHostAddresses(Dns.GetHostName());
                cleanup = true;
                // System.Console.WriteLine("SocketException caught!!!");
                // System.Console.WriteLine("Source : " + e.Source);
                // System.Console.WriteLine("Message : " + e.Message);           
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception caught!!!");
                System.Console.WriteLine("Source : " + e.Source);
                System.Console.WriteLine("Message : " + e.Message);
            }

            GetNetworkInterfaces();

            List<IPAddress> addrList = new List<IPAddress>();

            // make a list of all the adapters that we found in Dns.GetHostEntry(strHostName).AddressList
            foreach (IPAddress a in addr)
            {
                // make sure to get only IPV4 addresses!
                // test added because Erik Anderson noted an issue on Windows 7.  May have been in the socket
                // construction or binding below.
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    addrList.Add(a);
                }
            }

            bool foundRadio = false;
            List<HPSDRDevice> hpsdrd = new List<HPSDRDevice>();

            if (enableStaticIP)
            {
                HpSdrHwIpAddress = Console.getConsole().HPSDRNetworkIPAddr;

                IPAddress remoteIp = IPAddress.Parse(HpSdrHwIpAddress);
                IPEndPoint remoteEndPoint = new IPEndPoint(remoteIp, 0);
                Socket sock = new Socket(
                                      AddressFamily.InterNetwork,
                                      SocketType.Dgram,
                                      ProtocolType.Udp);
                IPEndPoint localEndPoint = QueryRoutingInterface(sock, remoteEndPoint);
                EthernetHostIPAddress = IPAddress.Parse(localEndPoint.Address.ToString()).ToString();

                sock.Close();
                sock = null;

                // if success set foundRadio to true, and fill in ONE hpsdrd entry.
                IPAddress targetIP;
                IPAddress hostIP;
                if (IPAddress.TryParse(EthernetHostIPAddress, out hostIP) && IPAddress.TryParse(HpSdrHwIpAddress, out targetIP))
                {
                    System.Console.WriteLine(String.Format("Attempting connect to host adapter {0}, Static IP {1}", EthernetHostIPAddress, HpSdrHwIpAddress));

                    if (DiscoverRadioOnPort(ref hpsdrd, hostIP, targetIP))
                    {
                        foundRadio = true;

                        // make sure that there is only one entry in the list!
                        if (hpsdrd.Count > 0)
                        {
                            // remove the extra ones that don't match!
                            HPSDRDevice m2 = null;
                            foreach (var m in hpsdrd)
                            {
                                if (m.IPAddress.CompareTo(HpSdrHwIpAddress) == 0)
                                {
                                    m2 = m;
                                }
                            }

                            // clear the list and put our single element in it, if we found it.
                            hpsdrd.Clear();
                            if (m2 != null)
                            {
                                hpsdrd.Add(m2);
                            }
                            else
                            {
                                foundRadio = false;
                            }
                        }
                    }
                }
            }

            if (FastConnect && (EthernetHostIPAddress.Length > 0) && (HpSdrHwIpAddress.Length > 0))
            {
                // if success set foundRadio to true, and fill in ONE hpsdrd entry.
                IPAddress targetIP;
                IPAddress hostIP;
                if (IPAddress.TryParse(EthernetHostIPAddress, out hostIP) && IPAddress.TryParse(HpSdrHwIpAddress, out targetIP))
                {
                    System.Console.WriteLine(String.Format("Attempting fast re-connect to host adapter {0}, IP {1}", EthernetHostIPAddress, HpSdrHwIpAddress));

                    if (DiscoverRadioOnPort(ref hpsdrd, hostIP, targetIP))
                    {
                        foundRadio = true;

                        // make sure that there is only one entry in the list!
                        if (hpsdrd.Count > 0)
                        {
                            // remove the extra ones that don't match!
                            HPSDRDevice m2 = null;
                            foreach (var m in hpsdrd)
                            {
                                if (m.IPAddress.CompareTo(HpSdrHwIpAddress) == 0)
                                {
                                    m2 = m;
                                }
                            }

                            // clear the list and put our single element in it, if we found it.
                            hpsdrd.Clear();
                            if (m2 != null)
                            {
                                hpsdrd.Add(m2);
                            }
                            else
                            {
                                foundRadio = false;
                            }
                        }
                    }
                }
            }

            if (!foundRadio)
            {
                foreach (IPAddress ipa in addrList)
                {
                    if (DiscoverRadioOnPort(ref hpsdrd, ipa, null))
                    {
                        foundRadio = true;
                    }
                }
            }

            if (!foundRadio)
            {
                if (cleanup)
                    Win32.WSACleanup();
                return -1;
            }

            int chosenDevice = 0;
            BoardID = hpsdrd[chosenDevice].deviceType;
            FWCodeVersion = hpsdrd[chosenDevice].codeVersion;
            HpSdrHwIpAddress = hpsdrd[chosenDevice].IPAddress;
            HpSdrHwMacAddress = hpsdrd[chosenDevice].MACAddress;
            EthernetHostIPAddress = hpsdrd[chosenDevice].hostPortIPAddress.ToString();
            EthernetHostPort = hpsdrd[chosenDevice].localPort;
            NumRxs = hpsdrd[chosenDevice].numRxs;

            if (BoardID == HPSDRHW.HermesII)
            {
                if (FWCodeVersion < 103)
                {
                    fwVersionMsg = "Invalid Firmware!\nRequires 10.3 or greater. ";
                    return -101;                                                                                       
                }
            }

            rc = nativeInitMetis(HpSdrHwIpAddress, EthernetHostIPAddress, EthernetHostPort);
            return -rc;
        }

        public static int initOzy()
        {
            Console c = Console.getConsole();

            if (c != null)
            {
                return initRadio();
            }

            /* else */
            isFirmwareLoaded = true;
            return 0;
        }

        public static bool fwVersionsChecked = false;
        private static string fwVersionMsg = null;

        public static string getFWVersionErrorMsg()
        {
            return fwVersionMsg;
        }

        public static bool forceFWGood = false;

        private static bool legacyDotDashPTT = false;

        // checks if the firmware versions are consistent - returns false if they are not 
        // and set fwVersionmsg to point to an appropriate message
        private static bool fwVersionsGood()
        {
            return true;

            // bool result = true;
            // Console c = Console.getConsole();
            // int penny_ver = 0;
            // int mercury_ver = 0;
            //// byte[] metis_ver = new byte[1];
            // byte metis_ver = 0;
            // int mercury2_ver = 0;

            //// if (c.CurrentHPSDRModel == HPSDRModel.ANAN100D) c.RX2PreampPresent = true;
            // if (forceFWGood == true || c.CurrentModel == Model.HERMES)
            // {
            //     System.Console.WriteLine("Firmware ver check forced good!");
            //     return true;
            // }

            // if (c != null && c.HPSDRisMetis)
            // {
            //     GetCodeVersion(metis_ver);

            //     if (c.PowerOn)
            //     {
            //         byte metis_vernum = metis_ver;
            //         mercury_ver = getMercuryFWVersion();

            //         if (c.PennyPresent || c.PennyLanePresent)
            //         {
            //             do
            //             {
            //                 // Thread.Sleep(500);
            //                 penny_ver = getPenelopeFWVersion();
            //                 if (penny_ver < 16 || penny_ver > 80)
            //                 {
            //                     Thread.Sleep(500);
            //                     penny_ver = getPenelopeFWVersion();
            //                     if (penny_ver > 0) break;
            //                     penny_ver = getPenelopeFWVersion();
            //                     if (penny_ver == 0) break;
            //                 }
            //             }
            //             while (penny_ver <= 15);
            //         }

            //         switch (metis_vernum)
            //         {
            //             case 16:
            //             case 17:
            //                 if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 16)) ||
            //                     (c != null && c.MercuryPresent && (mercury_ver != 31 && mercury_ver != 71)))
            //                 {
            //                     result = false;
            //                     c.SetupForm.alex_fw_good = false;
            //                 }
            //                 break;
            //             case 18:
            //                 if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 17)) ||
            //                     (c != null && c.MercuryPresent && (mercury_ver != 32)))
            //                 {
            //                     result = false;
            //                     c.SetupForm.alex_fw_good = false;
            //                 }
            //                 break;
            //             case 19:
            //             case 20:
            //             case 21:
            //             case 22:
            //             case 23:
            //             case 24:
            //             case 25:
            //                 if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 17)) ||
            //                     (c != null && c.MercuryPresent && (mercury_ver != 33)))
            //                 {
            //                     result = false;
            //                     c.SetupForm.alex_fw_good = false;
            //                 }
            //                 break;
            //             case 26:
            //             case 27:
            //             case 28:
            //             case 29:
            //             case 30:
            //                 if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 18)) ||
            //                     (c != null && c.MercuryPresent && (mercury_ver != 34)))
            //                 {
            //                     result = false;
            //                     c.SetupForm.alex_fw_good = false;
            //                 }
            //                 break;
            //             default:
            //                 // fwVersionMsg = "Invalid Firmware Level.\nPowerSDR requires Mercury v3.1\nYou have version: " + mercury_ver.ToString("0\\.0");
            //                 result = false;
            //                 c.SetupForm.alex_fw_good = false;
            //                 break;
            //         }

            //         mercury2_ver = getMercury2FWVersion();
            //         if (mercury2_ver == 0)
            //         {
            //             Thread.Sleep(300);
            //             mercury2_ver = getMercury2FWVersion();
            //         }
            //         if (mercury2_ver < 32 || mercury2_ver == 127) //check if physical rx2 present
            //             c.RX2PreampPresent = false;
            //         else
            //             c.RX2PreampPresent = true;

            //         if (c.SetupForm.FirmwareBypass == true) result = true;

            //         if (!result)
            //             fwVersionMsg = "Invalid Firmware.\nYou have Metis: " + metis_ver.ToString("0\\.0") +
            //                                            "\nMercury:" + mercury_ver.ToString("0\\.0") +
            //                                            "\nPenny:" + penny_ver.ToString("0\\.0");
            //     }
            //     return result;
            // }

            //string fx2_version_string = getFX2FirmwareVersionString();
            ////int merc_ver = 0;
            //int ozy_ver = 0;
            //// int merc2_ver = 0;
            ////System.Console.WriteLine("fx2: " + fx2_version_string); 
            ////System.Console.WriteLine("ozy: " + ozy_ver); 
            ////System.Console.WriteLine("merc: " + merc_ver); 
            ////System.Console.WriteLine("penny: " + penny_ver); 
            ////  c.SetI2CSpeed();
            //// Thread.Sleep(100);

            //if (fx2_version_string.CompareTo("20090524") >= 0)
            //{
            //    //   do
            //    for (int i = 0; i < 5; i++)
            //    {
            //        ozy_ver = getOzyFWVersion();
            //        if (ozy_ver > 17) break;
            //        Thread.Sleep(100);
            //    }
            //    //  while (ozy_ver < 12);
            //    // Thread.Sleep(2000);
            //    if (c.MercuryPresent)
            //    {
            //        //  do
            //        //  for (int i = 0; i < 2; i++)
            //        {
            //            mercury_ver = getMercuryFWVersion();
            //            // if (mercury_ver > 0) break;
            //            Thread.Sleep(100);
            //        }
            //        mercury_ver = getMercuryFWVersion();
            //        mercury2_ver = getMercury2FWVersion();
            //    }

            //    if (c.PennyPresent || c.PennyLanePresent)
            //    {
            //        do
            //        {
            //            // Thread.Sleep(500);
            //            penny_ver = getPenelopeFWVersion();
            //            if (penny_ver < 11)
            //            {
            //                Thread.Sleep(500);
            //                penny_ver = getPenelopeFWVersion();
            //                if (penny_ver > 0) break;
            //                penny_ver = getPenelopeFWVersion();
            //                if (penny_ver == 0) break;
            //            }
            //        }
            //        while (penny_ver <= 10);
            //    }
            //    switch (ozy_ver)
            //    {
            //        case 18:
            //            // if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 13)) ||
            //            //      (c != null && c.MercuryPresent && (mercury_ver != 29)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 19:
            //            // if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 14)) ||
            //            //      (c != null && c.MercuryPresent && (mercury_ver != 29)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 20:
            //            // if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 15)) ||
            //            //    (c != null && c.MercuryPresent && (mercury_ver != 30)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 21:
            //            if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 16)) ||
            //                (c != null && c.MercuryPresent && (mercury_ver != 31)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 22:
            //            if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 17)) ||
            //                (c != null && c.MercuryPresent && (mercury_ver != 32 && mercury_ver != 33)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 23:
            //        case 24:
            //            if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 17)) ||
            //                (c != null && c.MercuryPresent && (mercury_ver != 33)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        case 25:
            //        case 26:
            //        case 27:
            //        case 28:
            //            if ((c != null && (c.PennyPresent || c.PennyLanePresent) && (penny_ver != 18)) ||
            //               (c != null && c.MercuryPresent && (mercury_ver != 34)))
            //            {
            //                result = false;
            //                c.SetupForm.alex_fw_good = false;
            //            }
            //            break;
            //        default:
            //            result = false;
            //            c.SetupForm.alex_fw_good = false;
            //            break;
            //    }

            //    mercury2_ver = getMercury2FWVersion();
            //    if (mercury2_ver == 0)
            //    {
            //        Thread.Sleep(300);
            //        mercury2_ver = getMercury2FWVersion();
            //    }
            //    if (mercury2_ver < 32 || mercury2_ver == 127) //check if physical rx2 present
            //        c.RX2PreampPresent = false;
            //    else
            //        c.RX2PreampPresent = true;

            //    if (c.SetupForm.FirmwareBypass == true) result = true;

            //    if (!result)
            //        fwVersionMsg = "Invalid Firmware.\nYou have Ozy: " + ozy_ver.ToString("0\\.0") +
            //                                               "\nMercury: " + mercury_ver.ToString("0\\.0") +
            //                                                 "\nPenny: " + penny_ver.ToString("0\\.0");
            //}
            // return result;
        }

        // returns -101 for firmware version error 
        unsafe public static int StartAudio(/*int sample_rate, int samples_per_block,*/ PA19.PaStreamCallback cb) //, int sample_bits)
        {
            if (initRadio() != 0)
            {
                return 1;
            }

            int result = StartAudioNative(/*sample_rate, samples_per_block ,*/ cb); //, sample_bits);

            if (result == 0 && !fwVersionsChecked)
            {
               // Thread.Sleep(100); // wait for frames 
                if (!fwVersionsGood())
                {
                    result = -101;
                }
                else
                {
                    fwVersionsChecked = true;
                }
            }

            return result;
        }

         unsafe public static int GetDotDashPTT()
        {
            int bits = nativeGetDotDashPTT();
            if (legacyDotDashPTT)  // old style dot and ptt overloaded on 0x1 bit, new style dot on 0x4, ptt on 0x1 
            {
                if ((bits & 0x1) != 0)
                {
                    bits |= 0x4;
                }
            }
            return bits;
        }

        private static double freq_correction_factor = 1.0;
        public static double FreqCorrectionFactor
        {
            get { return freq_correction_factor; }
            set
            {
                freq_correction_factor = value;
                freqCorrectionChanged();
            }
        }

        public static void freqCorrectionChanged()
        {
            if (!Console.FreqCalibrationRunning)    // we can't be applying freq correction when cal is running 
            {
                VFOfreq(0, lastVFOfreq[0][0], 0);
                VFOfreq(1, lastVFOfreq[0][1], 0);
                VFOfreq(2, lastVFOfreq[0][2], 0);
                VFOfreq(3, lastVFOfreq[0][3], 0);
                VFOfreq(0, lastVFOfreq[1][0], 1);
            }
        }

        private static double[][] lastVFOfreq = new double[2][] { new double[] { 0.0, 0.0, 0.0, 0.0 }, new double[] { 0.0 } };
        unsafe public static void VFOfreq(int id, double f, int tx)
        {
            lastVFOfreq[tx][id] = f;
            int f_freq;
            f_freq = (int)((f * 1e6) * freq_correction_factor);
            if (f_freq >= 0)
                // SetVFOfreq(id, f_freq, tx);                  // sending freq to firmware
                SetVFOfreq(id, Freq2PW(f_freq), tx);            // sending phaseword to firmware
        }

        public static int Freq2PW(int freq)                     // freq to phaseword conversion
        {
            long pw = 0;
            pw = ((long)Math.Pow(2, 32) * freq / 122880000);
            return (int)pw;
        }

        private static double low_freq_offset;
        public static double LowFreqOffset
        {
            get { return low_freq_offset; }
            set
            {
                low_freq_offset = value;
            }
        }

        private static double high_freq_offset;
        public static double HighFreqOffset
        {
            get { return high_freq_offset; }
            set
            {
                high_freq_offset = value;
            }
        }
     
        // Taken from: KISS Konsole
        public static List<NetworkInterface> foundNics = new List<NetworkInterface>();
        public static List<NicProperties> nicProperties = new List<NicProperties>();
        public static string numberOfIPAdapters;
        public static string Network_interfaces = null;  // holds a list with the description of each Network Adapter
        public static int adapterSelected = 1;           // from Setup form, the number of the Network Adapter to use

        public static void GetNetworkInterfaces()
        {
            // creat a string that contains the name and speed of each Network adapter 
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foundNics.Clear();
            nicProperties.Clear();

            Network_interfaces = "";
            int adapterNumber = 1;

            foreach (var netInterface in nics)
            {
                if ((netInterface.OperationalStatus == OperationalStatus.Up ||
                     netInterface.OperationalStatus == OperationalStatus.Unknown) &&
                    (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                 netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            NicProperties np = new NicProperties();
                            np.ipv4Address = addrInfo.Address;
                            np.ipv4Mask = addrInfo.IPv4Mask;
                            nicProperties.Add(np);
                        }
                    }
                }

                // if the length of the network adapter name is > 31 characters then trim it, if shorter then pad to 31.
                // Need to use fixed width font - Courier New
                string speed = "  " + (netInterface.Speed / 1000000).ToString() + "T";
                if (netInterface.Description.Length > 31)
                {
                    Network_interfaces += adapterNumber++.ToString() + ". " + netInterface.Description.Remove(31) + speed + "\n";
                }
                else
                {
                    Network_interfaces += adapterNumber++.ToString() + ". " + netInterface.Description.PadRight(31, ' ') + speed + "\n";
                }

                foundNics.Add(netInterface);
            }

            /*
                        foreach (NetworkInterface adapter in nics)
                        {
                            IPInterfaceProperties properties = adapter.GetIPProperties();

                            // if it's not 'up' (operational), ignore it.  (Dan Quigley, 13 Aug 2011)
                            if (adapter.OperationalStatus != OperationalStatus.Up)
                                continue;

                            // if it's a loopback interface, ignore it!
                            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                                continue;

                            // get rid of non-ethernet addresses
                            if ((adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet) && (adapter.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet))
                                continue;

                            System.Console.WriteLine("");      // a blank line
                            System.Console.WriteLine(adapter.Description);
                            System.Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                            System.Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                            System.Console.WriteLine("  Physical Address ........................ : {0}", adapter.GetPhysicalAddress().ToString());
                            System.Console.WriteLine("  Is receive only.......................... : {0}", adapter.IsReceiveOnly);
                            System.Console.WriteLine("  Multicast................................ : {0}", adapter.SupportsMulticast);
                            System.Console.WriteLine("  Speed    ................................ : {0}", adapter.Speed);

                            // list unicast addresses
                            UnicastIPAddressInformationCollection c = properties.UnicastAddresses;
                            foreach (UnicastIPAddressInformation a in c)
                            {
                                IPAddress addr = a.Address;
                                System.Console.WriteLine("  Unicast Addr ............................ : {0}", addr.ToString());
                                IPAddress mask = a.IPv4Mask;
                                System.Console.WriteLine("  Unicast Mask ............................ : {0}", (mask == null ? "null" : mask.ToString()));

                                NicProperties np = new NicProperties();
                                np.ipv4Address = a.Address;
                                np.ipv4Mask = a.IPv4Mask;

                                nicProperties.Add(np);
                            }

                            // list multicast addresses
                            MulticastIPAddressInformationCollection m = properties.MulticastAddresses;
                            foreach (MulticastIPAddressInformation a in m)
                            {
                                IPAddress addr = a.Address;
                                System.Console.WriteLine("  Multicast Addr .......................... : {0}", addr.ToString());
                            }

                            // if the length of the network adapter name is > 31 characters then trim it, if shorter then pad to 31.
                            // Need to use fixed width font - Courier New
                            string speed = "  " + (adapter.Speed / 1000000).ToString() + "T";
                            if (adapter.Description.Length > 31)
                            {
                                Network_interfaces += adapterNumber++.ToString() + ". " + adapter.Description.Remove(31) + speed + "\n";
                            }
                            else
                            {
                                Network_interfaces += adapterNumber++.ToString() + ". " + adapter.Description.PadRight(31, ' ') + speed + "\n";
                            }

                            foundNics.Add(adapter);
                        }
            */
            System.Console.WriteLine(Network_interfaces);

            // display number of adapters on Setup form
            numberOfIPAdapters = (adapterNumber - 1).ToString();
        }

        private static bool DiscoverRadioOnPort(ref List<HPSDRDevice> hpsdrdList, IPAddress HostIP, IPAddress targetIP)
        {
            bool result = false;

            // configure a new socket object for each Ethernet port we're scanning
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Listen to data on this PC's IP address. Allow the program to allocate a free port.
            iep = new IPEndPoint(HostIP, LocalPort);  // was iep = new IPEndPoint(ipa, 0);

            try
            {
                // bind to socket and Port
                socket.Bind(iep);
                //   socket.ReceiveBufferSize = 0xFFFFF;   // no lost frame counts at 192kHz with this setting
                socket.Blocking = true;

                IPEndPoint localEndPoint = (IPEndPoint)socket.LocalEndPoint;
                System.Console.WriteLine("Looking for radio using host adapter IP {0}, port {1}", localEndPoint.Address, localEndPoint.Port);

                if (Discovery(ref hpsdrdList, iep, targetIP))
                {
                    result = true;
                }

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Caught an exception while binding a socket to endpoint {0}.  Exception was: {1} ", iep.ToString(), ex.ToString());
                result = false;
            }
            finally
            {
                socket.Close();
                socket = null;
            }

            return result;
        }


        private static bool Discovery(ref List<HPSDRDevice> hpsdrdList, IPEndPoint iep, IPAddress targetIP)
        {
            // set up HPSDR Metis discovery packet
            string MAC;
            byte[] DiscoveryPacket = new byte[60];
            Array.Clear(DiscoveryPacket, 0, DiscoveryPacket.Length);
            DiscoveryPacket[4] = 0x02;
            bool radio_found = false;            // true when we find a radio
            bool static_ip_ok = true;
            int time_out = 0;

            // set socket option so that broadcast is allowed.
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            // need this so we can Broadcast on the socket
            IPEndPoint broadcast;// = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            string receivedIP = "";   // the IP address Metis obtains; assigned, from DHCP or APIPA (169.254.x.y)

            IPAddress hostPortIPAddress = iep.Address;
            IPAddress hostPortMask = IPAddress.Broadcast;

            // find the subnet mask that goes with this host port
            foreach (NicProperties n in nicProperties)
            {
                if (hostPortIPAddress.Equals(n.ipv4Address))
                {
                    hostPortMask = n.ipv4Mask;
                    break;
                }
            }

            // send every second until we either find a radio or exceed the number of attempts
            while (!radio_found)            // #### djm should loop for a while in case there are multiple radios
            {
                // send a broadcast to port 1024
                // try target ip address 1 time if static
                if (enableStaticIP && static_ip_ok)
                    broadcast = new IPEndPoint(targetIP, DiscoveryPort);
                else
                    // try directed broadcast address
                    broadcast = new IPEndPoint(IPAddressExtensions.GetBroadcastAddress(hostPortIPAddress, hostPortMask), DiscoveryPort);
                   // try global broadcast address last resort
                   // broadcast = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
                socket.SendTo(DiscoveryPacket, broadcast);

                // now listen on send port for any radio
                System.Console.WriteLine("Ready to receive.... ");
                int recv;
                byte[] data = new byte[100];

                bool data_available;

                // await possibly multiple replies, if there are multiple radios on this port,
                // which MIGHT be the 'any' port, 0.0.0.0
                do
                {
                    // Poll the port to see if data is available 
                    data_available = socket.Poll(100000, SelectMode.SelectRead);  // wait 100 msec  for time out    

                    if (data_available)
                    {
                        EndPoint remoteEP = new IPEndPoint(IPAddress.None, 0);
                        recv = socket.ReceiveFrom(data, ref remoteEP);                 // recv has number of bytes we received
                        //string stringData = Encoding.ASCII.GetString(data, 0, recv); // use this to print the received data

                        System.Console.WriteLine("RAW Discovery data = " + BitConverter.ToString(data, 0, recv));
                        // see what port this came from at the remote end
                        // IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                        // System.Console.WriteLine(" Remote Port # = ", remoteIpEndPoint.Port);

                        string junk = Convert.ToString(remoteEP);  // see code in DataLoop
                        string[] words = junk.Split(':');
                        System.Console.Write(words[1]);

                        // get Metis MAC address from the payload
                        byte[] mac = { 0, 0, 0, 0, 0, 0 };
                        Array.Copy(data, 5, mac, 0, 6);
                        MAC = BitConverter.ToString(mac);

                        // check for HPSDR frame ID and type 2 (not currently streaming data, which also means 'not yet in use')
                        // changed to filter a proper discovery packet from the radio, even if alreay in use!  This prevents the need to power-cycle the radio.

                        if ((data[0] == 0x0) &&
                            (data[1] == 0x0) &&
                            (data[2] == 0x0) &&
                            (data[3] == 0x0) &&
                            (data[4] == 0x2))
                        {
                            System.Console.WriteLine("\nFound a radio on the network.  Checking whether it qualifies");

                            // get Metis IP address from the IPEndPoint passed to ReceiveFrom.
                            IPEndPoint ripep = (IPEndPoint)remoteEP;
                            IPAddress receivedIPAddr = ripep.Address;
                            receivedIP = receivedIPAddr.ToString();
                            IPEndPoint localEndPoint = (IPEndPoint)socket.LocalEndPoint;
                            System.Console.WriteLine("Looking for radio using host adapter IP {0}, port {1}", localEndPoint.Address, localEndPoint.Port);

                            System.Console.WriteLine("IP from IP Header = " + receivedIP);
                            System.Console.WriteLine("MAC address from payload = " + MAC);
                            if (!SameSubnet(receivedIPAddr, hostPortIPAddress, hostPortMask))
                            {
                                // device is NOT on the subnet that this port actually services.  Do NOT add to list!
                                System.Console.WriteLine("Not on subnet of host adapter! Adapter IP {0}, Adapter mask {1}",
                                    hostPortIPAddress.ToString(), hostPortMask.ToString());
                            }
                           // else if (receivedIPAddr.Equals(hostPortIPAddress))
                           // {
                             //   System.Console.WriteLine("Rejected: contains same IP address as the host adapter; not from a Metis/Hermes/Griffin");
                            //}
                            else if (MAC.Equals("00-00-00-00-00-00"))
                            {
                                System.Console.WriteLine("Rejected: contains bogus MAC address of all-zeroes");
                            }
                            else
                            {
                                HPSDRDevice hpsdrd = new HPSDRDevice();
                                hpsdrd.IPAddress = receivedIP;
                                hpsdrd.MACAddress = MAC;
                                hpsdrd.deviceType = (HPSDRHW)data[11];
                                hpsdrd.codeVersion = data[13];
                                hpsdrd.hostPortIPAddress = hostPortIPAddress;
                                hpsdrd.localPort = localEndPoint.Port;
                                hpsdrd.MercuryVersion_0 = data[14];
                                hpsdrd.MercuryVersion_1 = data[15];
                                hpsdrd.MercuryVersion_2 = data[16];
                                hpsdrd.MercuryVersion_3 = data[17];
                                hpsdrd.PennyVersion = data[18];
                                hpsdrd.MetisVersion = data[19];
                                hpsdrd.numRxs = data[20];

                                if (targetIP != null)
                                {
                                    if (hpsdrd.IPAddress.CompareTo(targetIP.ToString()) == 0)
                                    {
                                        radio_found = true;
                                        hpsdrdList.Add(hpsdrd);
                                        return true;
                                    }
                                }
                                else
                                {
                                    radio_found = true;
                                    hpsdrdList.Add(hpsdrd);
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("No data  from Port = ");
                        if ((++time_out) > 5)
                        {
                            System.Console.WriteLine("Time out!");
                            return false;
                        }
                        static_ip_ok = false;
                    }
                } while (data_available);
            }

            return radio_found;
        }

        /// <summary>
        /// Determines whether the board and hostAdapter IPAddresses are on the same subnet,
        /// using subnetMask to make the determination.  All addresses are IPV4 addresses
        /// </summary>
        /// <param name="board">IP address of the remote device</param>
        /// <param name="hostAdapter">IP address of the ethernet adapter</param>
        /// <param name="subnetMask">subnet mask to use to determine if the above 2 IPAddresses are on the same subnet</param>
        /// <returns>true if same subnet, false otherwise</returns>
        public static bool SameSubnet(IPAddress board, IPAddress hostAdapter, IPAddress subnetMask)
        {
            byte[] boardBytes = board.GetAddressBytes();
            byte[] hostAdapterBytes = hostAdapter.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (boardBytes.Length != hostAdapterBytes.Length)
            {
                return false;
            }
            if (subnetMaskBytes.Length != hostAdapterBytes.Length)
            {
                return false;
            }

            for (int i = 0; i < boardBytes.Length; ++i)
            {
                byte boardByte = (byte)(boardBytes[i] & subnetMaskBytes[i]);
                byte hostAdapterByte = (byte)(hostAdapterBytes[i] & subnetMaskBytes[i]);
                if (boardByte != hostAdapterByte)
                {
                    return false;
                }
            }
            return true;
        }

        // Taken From: https://searchcode.com/codesearch/view/7464800/
        private static IPEndPoint QueryRoutingInterface(
                  Socket socket,
                  IPEndPoint remoteEndPoint)
        {
            SocketAddress address = remoteEndPoint.Serialize();

            byte[] remoteAddrBytes = new byte[address.Size];
            for (int i = 0; i < address.Size; i++)
            {
                remoteAddrBytes[i] = address[i];
            }

            byte[] outBytes = new byte[remoteAddrBytes.Length];
            socket.IOControl(
                        IOControlCode.RoutingInterfaceQuery,
                        remoteAddrBytes,
                        outBytes);
            for (int i = 0; i < address.Size; i++)
            {
                address[i] = outBytes[i];
            }

            EndPoint ep = remoteEndPoint.Create(address);
            return (IPEndPoint)ep;
        }

    }

    // Taken from: http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
    public static class IPAddressExtensions
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
    }

    public class HPSDRDevice
    {
        public HPSDRHW deviceType;      // which type of device (currently Metis or Hermes)
        public byte codeVersion;        // reported code version type
        public string IPAddress;        // currently, an IPV4 address
        public string MACAddress;       // a physical (MAC) address
        public IPAddress hostPortIPAddress;
        public int localPort;
        public byte MercuryVersion_0;
        public byte MercuryVersion_1;
        public byte MercuryVersion_2;
        public byte MercuryVersion_3;
        public byte PennyVersion;
        public byte MetisVersion;
        public byte numRxs;
    }

    public class NicProperties
    {
        public IPAddress ipv4Address;
        public IPAddress ipv4Mask;
    }


}

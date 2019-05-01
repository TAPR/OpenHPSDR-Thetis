﻿using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    unsafe partial class NetworkIO
    {
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetOutputPowerFactor(int i);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeInitMetisSockets();

        [DllImport("ChannelMaster.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nativeInitMetis(String netaddr, String localaddr, int localport);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetXVTREnable(int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWBPacketsPerFrame(int pps);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWBUpdateRate(int ur);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWBEnable(int adc, int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendHighPriority(int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSampleRate(int id, int rate);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDDCRate(int id, int rate);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CmdRx();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getOOO();

        [DllImport("ChannelMaster.dll", EntryPoint = "create_rnet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateRNet();

        [DllImport("ChannelMaster.dll", EntryPoint = "destroy_rnet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyRNet();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMetisIPAddr();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMACAddr(byte[] addr_bytes);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCodeVersion(byte[] addr_bytes);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetBoardID(byte[] addr_bytes);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int StartAudioNative( PA19.PaStreamCallback cb);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int StopAudio();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetC1Bits(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAlexHPFBits(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAlexLPFBits(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DisablePA(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTRXrelay(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAlex2HPFBits(int bits);
       
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetBPF2Gnd(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAlex2LPFBits(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableApolloFilter(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableApolloTuner(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableApolloAutoTune(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableEClassModulation(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEERPWMmin(int min);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEERPWMmax(int max);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getUserADC0();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getUserADC1();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getUserADC2();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getUserADC3();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUserOut0(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUserOut1(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUserOut2(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUserOut3(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getUserI01(); // TX Inhibit input sense

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getUserI02();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getUserI03();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getUserI04();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)] // controls PureSignal
        public static extern void SetPureSignal(int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)] // sets single receiver
        public static extern void EnableRx(int id, int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)] // sets multiple receivers
        public static extern void EnableRxs(int Rxs);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)] // sets multiple receivers
        public static extern void EnableRxSync(int id, int sync);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetC1Bits();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nativeGetDotDashPTT();  // bit 0 = ptt, bit1 = dash asserted, bit 2 = dot asserted 

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLegacyDotDashPTT(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPttOut(int xmitbit);  // bit xmitbit ==0, recv mode, != 0, xmit mode

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetVFOfreq(int id, int freq, int tx);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWatchdogTimer(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMicBoost(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLineIn(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLineBoost(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAlexAtten(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADCDither(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADCRandom(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTxAttenData(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMercTxAtten(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRX1Preamp(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRX2Preamp(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADC1StepAttenData(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADC2StepAttenData(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADC3StepAttenData(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMicTipRing(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMicBias(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMicPTT(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getAndResetADC_Overload();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getHaveSync();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getExciterPower();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float getRevPower();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float getFwdPower();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getHermesDCVoltage();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableCWKeyer(int enable);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWSidetoneVolume(int vol);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWPTTDelay(int delay);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWHangTime(int hang);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWSidetoneFreq(int freq);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWKeyerSpeed(int speed);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWKeyerMode(int mode);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWKeyerWeight(int weight);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableCWKeyerSpacing(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReversePaddles(int bits);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWDash(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWDot(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWX(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWIambic(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWBreakIn(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCWSidetone(int bit);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetOCBits(int b);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAntBits(int rx_ant, int tx_ant, int rx_out, bool tx);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMKIIBPF(int bpf);
        
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRxADC(int n);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADC_cntrl1(int g);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetADC_cntrl1();

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetADC_cntrl2(int g);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetADC_cntrl2();


    }
}

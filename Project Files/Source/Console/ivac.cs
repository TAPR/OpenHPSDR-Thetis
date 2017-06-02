namespace Thetis
{
    using System;
    using System.Runtime.InteropServices;

    class ivac
    {
        #region ivac method definitions

        // vac 

        [DllImport("ChannelMaster.dll", EntryPoint = "StartAudioIVAC", CallingConvention = CallingConvention.Cdecl)] // StartAudioVAC()
        public static extern int StartAudioIVAC(int id);

        [DllImport("ChannelMaster.dll", EntryPoint = "StopAudioIVAC", CallingConvention = CallingConvention.Cdecl)] // StopAudioVAC()
        public static extern void StopAudioIVAC(int id);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACstereo", CallingConvention = CallingConvention.Cdecl)] // SetVACStereo(int enable)
        public static extern void SetIVACstereo(int id, int stereo);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACrun", CallingConvention = CallingConvention.Cdecl)] // SetVACEnabled(int enabled)
        public static extern void SetIVACrun(int id, int run);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACiqType", CallingConvention = CallingConvention.Cdecl)] // SetVACOutputIQ(int enabled)
        public static extern void SetIVACiqType(int id, int type);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACmicRate", CallingConvention = CallingConvention.Cdecl)] // SetBlockSize(int size)
        // public static extern void SetIVACmicRate(int id, int rate);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACaudioRate", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACaudioRate(int id, int rate);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACtxmonRate", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACtxmonRate(int id, int rate);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvacRate", CallingConvention = CallingConvention.Cdecl)] // SetBlockSizeVAC(int size)
        public static extern void SetIVACvacRate(int id, int rate);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACmicSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate1(int size)
        // public static extern void SetIVACmicSize(int id, int size);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACiqSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate1(int size)
        // public static extern void SetIVACiqSize(int id, int size);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACaudioSize", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACaudioSize(int id, int size);

        // [DllImport("WDSP.dll", EntryPoint = "SetIVACtxmonSize", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACtxmonSize(int id, int size);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvacSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate2(int size)
        public static extern void SetIVACvacSize(int id, int size);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVAChostAPIindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVAChostAPIindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACinputDEVindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACinputDEVindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACoutputDEVindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACoutputDEVindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACnumChannels", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACnumChannels(int id, int n);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACInLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACInLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACOutLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACOutLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPAInLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPAInLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPAOutLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPAOutLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACpreamp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACpreamp(int id, double preamp);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACbypass", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACbypass(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACRBReset", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACRBReset(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACvox(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACrxscale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACrxscale(int id, double scale);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACcombine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACcombine(int id, int combine);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACmon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACmon(int id, int mon);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACmox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACmox(int id, int mox);
        #endregion

    }
}

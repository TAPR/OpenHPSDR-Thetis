//=================================================================
// deltacp.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
    using System.Runtime.InteropServices;
    using System.Text;

namespace Thetis
{
	class DeltaCP
	{
		#region Constants

		public const int DP_ERR_SUCCESS = 0;
		public const int DP_ERR_FAILURE = -1;
		public const int DP_ERR_NOT_OPEN = -2;
		public const int DP_ERR_INVALID_DEVNUM = -3;

		// List of Error Messages from the Kernel Driver.
		public const uint ERROR_TYPE = 0x0E0000000;

		// Invalid Buffer Latency value.
		public const uint ICEAPI_ERR_INVALID_BUFLATENCY = ERROR_TYPE|0x00000001;
		// Invalid Mute parameter.
		public const uint ICEAPI_ERR_INVALID_MUTE = ERROR_TYPE|0x00000002;
		// Invalid Channel ID parameter.
		public const uint ICEAPI_ERR_INVALID_CHAN_ID = ERROR_TYPE|0x00000003;
		// Driver is opened. This command cannot proceed with driver opened.
		public const uint ICEAPI_ERR_DRIVER_OPEN = ERROR_TYPE|0x00000004;
		// The hardware handle is invalid.
		public const uint ICEAPI_ERR_INVALID_HANDLE = ERROR_TYPE|0x00000005;
		// The Clock Source is invalid.
		public const uint ICEAPI_ERR_INVALID_CLKSRC = ERROR_TYPE|0x00000006;
		// The Output Source is invalid.
		public const uint ICEAPI_ERR_INVALID_OUTSRC = ERROR_TYPE|0x00000007;
		// Invalid Sample Rate for Spdif Master Clock.
		public const uint ICEAPI_ERR_INVALID_SPDIF_RATE = ERROR_TYPE|0x00000009;
		// Invalid Parameter.
		public const uint ICEAPI_ERR_INVALID_PARAM = ERROR_TYPE|0x0000000A;
		// Hardware In use or busy.
		public const uint ICEAPI_ERR_HW_INUSE = ERROR_TYPE|0x0000000B;
		// Not enough memory.
		public const uint ICEAPI_ERR_OUTOFMEM = ERROR_TYPE|0x0000000C;
		// The external sample rate is different from the desired one.
		public const uint ICEAPI_ERR_EXT_SAMPLERATE_DIFF = ERROR_TYPE|0x00000010;
		// Invalid Sample Rate for Internal Master Clock.
		public const uint ICEAPI_ERR_INVALID_SAMPLE_RATE = ERROR_TYPE|0x00000011;
		// Driver is opened for CLKSRC change request. This command cannot proceed with driver opened.
		public const uint ICEAPI_ERR_DRIVER_OPEN_CLKSRC = ERROR_TYPE|0x00000012;
		// Unknown hardware error.
		public const uint ICEAPI_ERR_HW_UNKNOWN = ERROR_TYPE|0x000000FF;

		#endregion

		#region Structs

		public struct VERSION_INFO_STRUC
		{
			public uint dwDriverVersion;
			public uint dwDriverRevision;
			public uint dwDriverRelease;
			public uint dwDriverBuildNum;
		}

		public struct VARI_LEVEL_INFO_STRUC
		{
			public uint dwNumControls;
			public uint dwNumSteps;
			public uint dwMinLevel;
			public uint dwMaxLevel;
			public uint dwLevelPlus4dBu;
			public uint dwLevelCons0dBu;
			public uint dwLevelMinus10dBV;
		}

		public struct VARI_LEVEL_VALUE_STRUC
		{
			public uint dwPort1Level;
			public uint dwPort2Level;
			public uint dwPort3Level;
			public uint dwPort4Level;
			public uint dwPort5Level;
			public uint dwPort6Level;
			public uint dwPort7Level;
			public uint dwPort8Level;
		}

		#endregion

		#region Function Definitions

		[DllImport("deltapnl.dll", EntryPoint="dpGetVersion")]
		public static extern int dpGetVersion();
		public static string GetVersion()
		{
			int ver = dpGetVersion();
			int os_ver = ver >> 24;
			int maj_ver = ((ver >> 16) & 0xff);
			int min_ver = ((ver >> 8) & 0xff);
			int build_num = ver & 0xff;
			string s = os_ver+"."+
				maj_ver+"."+
				min_ver+"."+
				build_num;
			return s;
		}

		[DllImport("deltapnl.dll", EntryPoint="dpInitInterface",
			CallingConvention=CallingConvention.Winapi)]
		public static extern int Init();

		[DllImport("deltapnl.dll", EntryPoint="dpCloseInterface",
			 CallingConvention=CallingConvention.Winapi)]
		public static extern int Close();

		[DllImport("deltapnl.dll", EntryPoint="dpGetDriverType",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern short dpGetDriverType(short *type);
		unsafe public static int GetDriverType()
		{
			short n = 0;
			dpGetDriverType(&n);
			return n;
		}

		[DllImport("deltapnl.dll", EntryPoint="dpGetDriverVersion",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetDriverVersion(VERSION_INFO_STRUC *pDriverVersionInfo);
		unsafe public static string GetDriverVersion()
		{
			VERSION_INFO_STRUC v = new VERSION_INFO_STRUC();
			dpGetDriverVersion(&v);
			return v.dwDriverVersion+"."+v.dwDriverRevision+"."+
				v.dwDriverRelease+"."+v.dwDriverBuildNum;
		}

		[DllImport("deltapnl.dll", EntryPoint="dpGetDriverGangSupport",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern short dpGetDriverGangSupport(short* pdwDriverGangSupport);
		unsafe public static int GetDriverGangSupport()
		{
			short n = 0;
			dpGetDriverGangSupport(&n);
			return (int)n;
		}

		[DllImport("deltapnl.dll", EntryPoint="dpGetManufactureName",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern short dpGetManufactureName(
			StringBuilder pszManufactureName, short iManufactureNameSize);
		unsafe public static string GetManufactureName()
		{
			StringBuilder str = new StringBuilder(50);
			dpGetManufactureName(str, 50);
			return str.ToString();
		}

		[DllImport("deltapnl.dll", EntryPoint="dpGetNumHwDevices",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetNumHwDevices(short* num);
		unsafe public static int GetNumHwDevices()
		{
			short i = 0;
			dpGetNumHwDevices(&i);
			return (int)i;
		}

		[DllImport("deltapnl.dll", EntryPoint="dpRefreshDevData",
			 CallingConvention=CallingConvention.Winapi)]
		public static extern int RefreshDevData(int iDeviceNum);

		[DllImport("deltapnl.dll", EntryPoint="dpGetDevNames",
			 CallingConvention=CallingConvention.Winapi)]
		private static extern short dpGetDevNames(
			short iDeviceNum,
			StringBuilder pszDeviceLongName,
			short iDeviceLongNameSize,
            StringBuilder pszDeviceShortName,
			short iDeviceShortNameSize);
		public static string GetShortName(short iDeviceNum)
		{
			StringBuilder long_name = new StringBuilder(50);
			StringBuilder short_name = new StringBuilder(50);
			dpGetDevNames(iDeviceNum, long_name, 50, short_name, 50);
			return short_name.ToString();
		}
		public static string GetLongName(short iDeviceNum)
		{
			StringBuilder long_name = new StringBuilder(50);
			StringBuilder short_name = new StringBuilder(50);
			dpGetDevNames(iDeviceNum, long_name, 50, short_name, 50);
			return long_name.ToString();
		}

		[DllImport("deltapnl.dll", EntryPoint="dpGetDevVariLevelInInfo",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetDevVariLevelInInfo(
			int iDeviceNum,
			VARI_LEVEL_INFO_STRUC *pVariLevelInInfo);

		[DllImport("deltapnl.dll", EntryPoint="dpSetDevVariLevelIn",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpSetDevVariLevelIn(
			int iDeviceNum,
			VARI_LEVEL_VALUE_STRUC *pVariLevelInValues);

		[DllImport("deltapnl.dll", EntryPoint="dpGetDevVariLevelIn",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetDevVariLevelIn(
			int iDeviceNum,
			VARI_LEVEL_VALUE_STRUC *pVariLevelInValues);

		[DllImport("deltapnl.dll", EntryPoint="dpGetDevVariLevelOutInfo",
			CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetDevVariLevelOutInfo(
			int iDeviceNum,
			VARI_LEVEL_INFO_STRUC *pVariLevelOutInfo);

		[DllImport("deltapnl.dll", EntryPoint="dpSetDevVariLevelOut",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpSetDevVariLevelOut(
			int iDeviceNum,
			VARI_LEVEL_VALUE_STRUC *pVariLevelOutValues);

		[DllImport("deltapnl.dll", EntryPoint="dpGetDevVariLevelOut",
			 CallingConvention=CallingConvention.Winapi)]
		unsafe private static extern int dpGetDevVariLevelOut(
			int iDeviceNum,
			VARI_LEVEL_VALUE_STRUC *pVariLevelOutValues);

		unsafe public static void SetLevels()
		{
			VARI_LEVEL_INFO_STRUC vari = new VARI_LEVEL_INFO_STRUC();
			int retval = dpGetDevVariLevelOutInfo(0, &vari);
			if(retval != 0) return;

			VARI_LEVEL_VALUE_STRUC val = new VARI_LEVEL_VALUE_STRUC();
			val.dwPort1Level = vari.dwLevelMinus10dBV;
			val.dwPort2Level = vari.dwLevelMinus10dBV;
			val.dwPort3Level = vari.dwLevelMinus10dBV;
			val.dwPort4Level = vari.dwLevelMinus10dBV;
			val.dwPort5Level = vari.dwLevelMinus10dBV;
			val.dwPort6Level = vari.dwLevelMinus10dBV;
		
			retval = dpSetDevVariLevelOut(0, &val);
			if(retval != 0) return;
			
			retval = dpGetDevVariLevelInInfo(0, &vari);
			if(retval != 0) return;

			retval = dpGetDevVariLevelIn(0, &val);
			if(retval != 0) return;

			val.dwPort1Level = vari.dwLevelPlus4dBu;
			val.dwPort2Level = vari.dwLevelPlus4dBu;
			//val.dwPort3Level = vari.dwLevelMinus10dBV;
			//val.dwPort4Level = vari.dwLevelMinus10dBV;
			//val.dwPort5Level = vari.dwLevelMinus10dBV;
			//val.dwPort6Level = vari.dwLevelMinus10dBV;

			retval = dpSetDevVariLevelIn(0, &val);
		}

		#endregion
	}
}
/*  Midi2Cat

Description: A subsystem that facilitates mapping Windows MIDI devices to CAT commands.
 
Copyright (C) 2016 Andrew Mansfield, M0YGG

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at:  midi2cat@cametrix.com

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Midi2Cat.IO
{
    public static class WinMM
    {
        #region Misc Declarations

        public const int MAXPNAMELEN = 32;

        [StructLayout(LayoutKind.Sequential)]
        public struct MidiHeader
        {
            public IntPtr data;
            public int bufferLength;
            public int bytesRecorded;
            public int user;
            public int flags;
            public IntPtr lpNext;
            public int reserved;
            public int offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] dwReserved;
        }
        #endregion

        #region MidiIn Declarations

        [DllImport("winmm.dll", EntryPoint = "midiInGetNumDevs", CharSet = CharSet.Ansi)]
        public static extern int MidiInGetNumDevs();

        [DllImport("winmm.dll", EntryPoint = "midiInGetDevCaps", CharSet = CharSet.Ansi)]
        public static extern int MidiInGetDevCaps(int uDeviceID, ref MIDIINCAPS caps, int cbMidiInCaps);

        [DllImport("winmm.dll", EntryPoint = "midiInOpen", CharSet = CharSet.Ansi)]
        public static extern int MidiInOpen(out IntPtr lphMidiIn, uint uDeviceID, MidiInCallback dwCallback, IntPtr dwInstance, MidiInOpenFlags dwFlags);

        [DllImport("winmm.dll", EntryPoint = "midiInClose", CharSet = CharSet.Ansi)]
        public static extern int MidiInClose(IntPtr hMidiIn);

        [DllImport("winmm.dll", EntryPoint = "midiInReset", CharSet = CharSet.Ansi)]
        public static extern int MidiInReset(IntPtr hMidiIn);

        [DllImport("winmm.dll", EntryPoint = "midiInStart", CharSet = CharSet.Ansi)]
        public static extern int MidiInStart(IntPtr hMidiIn);

        [DllImport("winmm.dll", EntryPoint = "midiInStop", CharSet = CharSet.Ansi)]
        public static extern int MidiInStop(IntPtr hMidiIn);

        [DllImport("winmm.dll", EntryPoint = "midiInAddBuffer")]
        public static extern int MidiInAddBuffer(int hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

        [DllImport("winmm.dll", EntryPoint = "midiInPrepareHeader")]
        public static extern int MidiInPrepareHeader(int hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

        [DllImport("winmm.dll", EntryPoint = "midiInUnprepareHeader")]
        public static extern int MidiInUnprepareHeader(int hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

        [DllImport("winmm.dll", EntryPoint = "midiInGetErrorText")]
        public static extern int MidiInGetErrorText(int wError, StringBuilder lpText, int cchText);

        unsafe public delegate int MidiInCallback(int hMidiIn, int wMsg, int dwInstance, int dwParam1, int dwParam2);

        public struct MIDIINCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
            public int dwSupport;
        }

        #endregion

        #region MidiOut Declarations

        [DllImport("winmm.dll", EntryPoint = "midiOutGetNumDevs")]
        public static extern int MidiOutGetNumDevs();

        [DllImport("winmm.dll", EntryPoint = "midiOutGetDevCaps")]
        public static extern int MidiOutGetDevCaps(int uDeviceID, ref MIDIOUTCAPS caps, int cbMidiOutCaps);

        [DllImport("winmm.dll", EntryPoint = "midiOutOpen")]
        public static extern int MidiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, MidiOutOpenFlags dwFlags);

        [DllImport("winmm.dll", EntryPoint = "midiOutClose")]
        public static extern int MidiOutClose(IntPtr hMidiOut);

        [DllImport("winmm.dll", EntryPoint = "midiOutShortMsg")]
        public static extern int MidiOutShortMessage(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll", EntryPoint = "midiOutLongMsg")]
        public static extern int MidiOutLongMessage(int handle, IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll", EntryPoint = "midiOutPrepareHeader")]
        public static extern int MidiOutPrepareHeader(int handle, IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll", EntryPoint = "midiOutUnprepareHeader")]
        public static extern int MidiOutUnprepareHeader(int handle, IntPtr headerPtr, int sizeOfMidiHeader);

        public struct MIDIOUTCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
            public short wTechnology;
            public short wVoices;
            public short wNotes;
            public short wChannelMask;
            public int dwSupport;
        }

        [Flags]
        public enum MidiInOpenFlags
        {
            Null = 0,
            Window = 0x10000,
            Task = 0x20000,
            Function = 0x30000,
            MidiIoStatus = 0x00020,
        }

        [Flags]
        public enum MidiOutOpenFlags
        {
            Null,
            Function,
            Thread,
            Window,
            Event,
        }
        #endregion
    }
}

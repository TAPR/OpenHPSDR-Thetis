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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midi2Cat.IO
{
    public class MidiDevices
    {
        public int DeviceInCount
        {
            get
            {
                return WinMM.MidiInGetNumDevs();
            }
        }

        public int DeviceOutCount
        {
            get
            {
                return WinMM.MidiOutGetNumDevs();
            }
        }

        public Collection<string> InDevices
        {
            get
            {
                Collection<string> inDevices = new Collection<string>();
                int numInDevices = DeviceInCount;
                for (int i = 0; i < numInDevices; i++)
                {
                    inDevices.Add(MidiInGetName(i));
                }
                return inDevices;
            }
        }

        public Collection<string> OutDevices
        {
            get
            {
                Collection<string> outDevices = new Collection<string>();
                int numOutDevices = DeviceOutCount;
                for (int i = 0; i < numOutDevices; i++)
                {
                    outDevices.Add(MidiOutGetName(i));
                }
                return outDevices;
            }
        }

        public static string MidiInGetName(int index)
        {
            WinMM.MIDIINCAPS caps = new WinMM.MIDIINCAPS();
            int error = WinMM.MidiInGetDevCaps(index, ref caps, 44);

            if (error == 0) return caps.szPname;
            else return "";
        }

        public static string MidiOutGetName(int index)
        {
            WinMM.MIDIOUTCAPS caps = new WinMM.MIDIOUTCAPS();
            int error = WinMM.MidiOutGetDevCaps(index, ref caps, 52);

            if (error == 0) return caps.szPname;
            else return "";
        }
    }
}

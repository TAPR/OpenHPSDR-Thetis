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
using System.Text;
using System.Threading.Tasks;

namespace Midi2Cat.Data 
{
    public class FixUp
    {
        public static ControlType FixControlType(int controlType)
        {
            ControlType ct = (ControlType)controlType;
            /* fix up removed control types */
            if (controlType == 2)
                controlType = 1;
            else if (controlType == 3)
                controlType = 4;
            return ct;
        }
    }

    public enum CmdState
    {
        NoChange = -1,
        Off=0,
        On=1,
    }

    public enum MidiEvent
    {
        Note_Off = 0x08,
        Note_On = 0x09,
        Polyphonic_Key_Pressure = 0x0A,
        Control_Change = 0x0B,
        Program_Change = 0x0C,
        Channel_Pressure = 0x0D,
        Pitch_Bend = 0x0E
    }

    public enum ControlType
    {
        Unknown = 0,
        Button = 1,
        // Toggle = 2,
        // Slider = 3,
        Knob_or_Slider = 4,
        Wheel = 5
    }

    public enum Direction
    {
        None,
        In,
        Out
    }

    public enum Status
    {
        Unknown,
        Open,
        Closed,
        Error
    }

    public enum MappingFilter
    {
        None,
        Active,
        InActive,
    }

    public enum Command
    {
        //NoteOn = 0x90,
        // NoteOff = 0x80,
        // Aftertouch0 = 0xA0,
        //Aftertouch1 = 0xA1,
        Controller = 0xB0,
        // PolyPressure = 0xA0,
        // ProgramChange = 0xC0,
        // ChannelPressure = 0xD0,
        //  PitchWheel = 0xE0
    }
    /*
            private enum Controller
            {
                HoldPedal1 = 64,
                HoldPedal2 = 69,
            }

            private enum Note
            {
                Mox = 0,
                PAADC = 1,
                Dot = 61,
                Dash = 63,
                MicDown = 66,
                MicUp = 68,
                MicFast = 70,
            }
            */
}

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

using Midi2Cat.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Midi2Cat.Data
{
    public class MidMessageHandler
    {
        public ProcessMidiMessageHandler CmdHandler;
        public ProcessMidiMessageToggleHandler ToggleCmdHandler;
        public string MidiOutCmdDown;
        public string MidiOutCmdUp;
        public string MidiOutCmdSetValue;
    }

    public class ControllerBinding
    {
        public string DeviceName;
        public int DeviceIndex;
        public MidiDevice Device;
        public Dictionary<int, MidMessageHandler> CmdBindings;
    }
}

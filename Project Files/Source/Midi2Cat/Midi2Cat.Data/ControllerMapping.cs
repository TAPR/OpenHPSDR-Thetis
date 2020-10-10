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

Modifications to support the Behringer CMD PL-1 controller
by Chris Codella, W2PA, Feb 2017.  Indicated by //-W2PA comment lines.

*/
using Midi2Cat.IO;

namespace Midi2Cat.Data
{
    public delegate void ProcessMidiMessageHandler(int msg, MidiDevice device); //-W2PA Added device parameter for msg flow to MIDI device
    public delegate CmdState ProcessMidiMessageToggleHandler(int msg, MidiDevice device);

    public class ControllerMapping
    {
        public int MidiControlId { get; set; }
        public string MidiControlName { get; set; }
        public ControlType MidiControlType { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public CatCmd CatCmdId { get; set; }
        public string MidiOutCmdDown { get; set; }
        public string MidiOutCmdUp { get; set; }
        public string MidiOutCmdSetValue { get; set; }

        // Not stored in the DB properies/events
        public CatCommandAttribute CatCmd { get; set; }
        public ProcessMidiMessageHandler onProcessMidiMessage;

        public override string ToString()
        {
            return MidiControlName + " -> " + CatCmd.Desc;
        }
    }
}

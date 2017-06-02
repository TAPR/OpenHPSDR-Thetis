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
using Midi2Cat.IO;
using Midi2Cat.Data;
using System.Reflection;

namespace Midi2Cat
{
    public class MidiMessageManager
    {
        public string DbFile { get; set; }

        Midi2CatDatabase DB = null;
        object midi_handler_lock = new object();
        Dictionary<int, ControllerBinding> bindings = new Dictionary<int, ControllerBinding>();
        Object midi2CatCommands;

        public MidiMessageManager(Object Midi2CatCommands, string DbFile)
        {
            this.midi2CatCommands = Midi2CatCommands;
            this.DbFile = DbFile;
        }

        public void Open() 
        {
            if (bindings.Count > 0)
            {
                Close();
            }

            MidiDevices devices = new MidiDevices();
            DB = new Midi2CatDatabase(DbFile);
            int Idx=0;
            foreach (string inDevice in devices.InDevices)
            {
                // see if we can find settings for this device
                List<ControllerMapping> mappings = DB.GetMappings(inDevice, MappingFilter.Active); 
                if (mappings.Count > 0)
                {
                    InitDevice(inDevice, mappings, Idx);
                }
                Idx++;
            }
        }

        public void Close()
        {
            foreach( var binding in bindings.Values)
            {
                binding.Device.CloseMidiIn();
                binding.Device.CloseMidiOut();
            }
            bindings.Clear();
        }

        void InitDevice(string deviceName,List<ControllerMapping> mappings, int Idx)
        {
            MidiDevice device = new MidiDevice();
            device.OpenMidiIn(Idx, deviceName);

            Dictionary<int, MidMessageHandler> deviceBindings = BindMappingHandlers(mappings);
            ControllerBinding ctrlBinding= new ControllerBinding{ DeviceName=deviceName, DeviceIndex=Idx, Device=device, CmdBindings=deviceBindings};

            bindings.Add(Idx, ctrlBinding);


            device.onMidiDebugMessage += onMidiDebugMsg;
            device.onMidiInput += OnMidiInput;
        }

        Dictionary<int, MidMessageHandler> BindMappingHandlers(List<ControllerMapping> mappings)
        {
            Dictionary<int, MidMessageHandler> Bindings = new Dictionary<int, MidMessageHandler>();
            foreach (ControllerMapping mapping in mappings)
            {
                try
                {
                    string methodName = mapping.CatCmdId.ToString();
                    Type midi2CatCommandsType = midi2CatCommands.GetType();
                    MethodInfo methodInfo = midi2CatCommandsType.GetMethod(methodName);

                    if (mapping.CatCmd.IsToggled == false)
                    {
                        ProcessMidiMessageHandler binding = (ProcessMidiMessageHandler)Delegate.CreateDelegate(typeof(ProcessMidiMessageHandler), midi2CatCommands, methodInfo);
                        MidMessageHandler handler = new MidMessageHandler { CmdHandler = binding, MidiOutCmdDown = mapping.MidiOutCmdDown, MidiOutCmdUp = mapping.MidiOutCmdUp, MidiOutCmdSetValue = mapping.MidiOutCmdSetValue };
                        Bindings.Add(mapping.MidiControlId, handler);
                    }
                    else
                    {
                        ProcessMidiMessageToggleHandler binding = (ProcessMidiMessageToggleHandler)Delegate.CreateDelegate(typeof(ProcessMidiMessageToggleHandler), midi2CatCommands, methodInfo);
                        MidMessageHandler handler = new MidMessageHandler { ToggleCmdHandler = binding, MidiOutCmdDown = mapping.MidiOutCmdDown, MidiOutCmdUp = mapping.MidiOutCmdUp, MidiOutCmdSetValue = mapping.MidiOutCmdSetValue };
                        Bindings.Add(mapping.MidiControlId, handler);
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("!!!ERROR!!! MidiMessageManager:BindMappingHandlers: failed to bind mapping "+mapping.CatCmdId.ToString() );
                }
            }
            return Bindings;
        }

        void onMidiDebugMsg(int Device, Direction direction, Status status, string msg1, string msg2)
        {
            System.Diagnostics.Debug.WriteLine("Device:{0}, Direction:{1}, Status:{2}, msg1:{3}, msg2{4}", Device, direction, status, msg1, msg2);
        }

        void OnMidiInput(MidiDevice Device, int DeviceIdx, int ControlId, int Data, int Status, int Voice, int Channel)
        {
            try
            {
                ControllerBinding ctrlBinding;
                if (bindings.TryGetValue(DeviceIdx, out ctrlBinding))
                {
                    MidMessageHandler handlers;
                    CmdState state=CmdState.NoChange;

                    if (ctrlBinding.CmdBindings.TryGetValue(ControlId, out handlers))
                    {
                        if (handlers != null)
                        {
                            lock (midi_handler_lock)
                            {
                                if (handlers.CmdHandler != null)
                                {
                                    handlers.CmdHandler(Data);
                                    if (Data <= 0)
                                        state = CmdState.Off;
                                    else
                                        state = CmdState.On;
                                }
                                else if (handlers.ToggleCmdHandler != null)
                                {
                                    state = handlers.ToggleCmdHandler(Data);
                                }

                                if (state == CmdState.On && handlers.MidiOutCmdDown != null)
                                {
                                    Device.SendMsg(Channel,0x7F, Status, ControlId,  handlers.MidiOutCmdDown);
                                }
                                if (state == CmdState.Off && handlers.MidiOutCmdUp != null)
                                {
                                    Device.SendMsg(Channel, 0x00, Status,ControlId, handlers.MidiOutCmdUp);
                                }
                                if (handlers.MidiOutCmdSetValue != null)
                                {
                                    Device.SendMsg(Channel, Data, Status, ControlId, handlers.MidiOutCmdSetValue);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

    }
}

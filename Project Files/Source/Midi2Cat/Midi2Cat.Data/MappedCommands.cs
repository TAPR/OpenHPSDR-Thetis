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
using System.Data;
using System.Linq;
using System.Text;

namespace Midi2Cat.Data
{
    public class MappedCommand
    {
        public int CmdId { get; set; }
        public string Description { get; set; }
        public ControlType ControlType { get; set; }
        public string Controller { get; set; }
        public string ControlName { get; set; }
        public string Remove { get; set; }

        public MappedCommand(DataRow row)
        {
            CmdId = (int)row["CmdId"];
            Description = (string)row["CmdDescription"];
            Controller = "";
            ControlName = "";
            ControlType = ControlType.Unknown;
            Remove = ""; 
        }

        public MappedCommand(MappedCommand src)
        {
            CmdId = src.CmdId;
            Description = src.Description;
            Controller = src.Controller;
            ControlName = src.ControlName;
            ControlType = src.ControlType;
            Remove = src.Remove;
        }
    }

    public class MappedCommands : List<MappedCommand>
    {
        public MappedCommands(DataTable Cmds, DataTable controllerDT)
        {
            foreach (DataRow row in Cmds.Rows)
            {
                if ((CatCmd)(row["CmdId"]) == CatCmd.None) 
                    continue;
                MappedCommand mappedCommand=new MappedCommand(row);
                if (GetDeviceMappings(mappedCommand, controllerDT) <= 0)
                {
                    mappedCommand.ControlType = ((ControlType)row["ControlType"]);
                    this.Add(mappedCommand);
                }
            }
        }

        private int GetDeviceMappings(MappedCommand mappedCmd, DataTable controllerDT)
        {
            int NumAdded = 0;
            foreach (DataRow dr in controllerDT.Rows)
            {                        
                if ((int)(dr["CatCmdId"]) == mappedCmd.CmdId)
                {
                    MappedCommand mc = new MappedCommand(mappedCmd);
                    mc.Controller = controllerDT.TableName;
                    mc.ControlName = (string)dr["MidiControlName"];
                    mc.ControlType = FixUp.FixControlType(((int)dr["MidiControlType"]));
                    mc.Remove = "Unmap";
                    this.Add(mc);
                    NumAdded++;
                }
            }
            return NumAdded;
        }

        

    } // class
}//namespace

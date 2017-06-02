//=================================================================
// CWInput.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace Thetis
{
    class CWInput
    {
        public static event SerialRXEventHandler serial_rx_event;
        private static SDRSerialPort primary_com_port;
        private static SDRSerialPort secondary_com_port;

        private static string primary_input = "Radio";
        public static string PrimaryInput
        {
            get { return primary_input; }
        }

        private static string secondary_input = "None";
        public static string SecondaryInput
        {
            get { return secondary_input; }
        }

        private static KeyerLine secondary_ptt_line = KeyerLine.None;
        public static KeyerLine SecondaryPTTLine
        {
            get { return secondary_ptt_line; }
            set
            {
                secondary_ptt_line = value;
                if (secondary_input.ToUpper().StartsWith("COM") && secondary_com_port != null)
                {
                    switch (value)
                    {
                        case KeyerLine.DTR:
                            secondary_com_port.PTTOnRTS = false;
                            secondary_com_port.PTTOnDTR = true;
                            break;
                        case KeyerLine.RTS:
                            secondary_com_port.PTTOnDTR = false;
                            secondary_com_port.PTTOnRTS = true;
                            break;
                        case KeyerLine.None:
                            secondary_com_port.PTTOnRTS = false;
                            secondary_com_port.PTTOnDTR = false;
                            break;
                    }
                }
            }
        }

        private static KeyerLine secondary_key_line = KeyerLine.None;
        public static KeyerLine SecondaryKeyLine
        {
            get { return secondary_key_line; }
            set
            {
                secondary_key_line = value;
                if (secondary_input.ToUpper().StartsWith("COM") && secondary_com_port != null)
                {
                    switch (value)
                    {
                        case KeyerLine.DTR:
                            secondary_com_port.KeyOnRTS = false;
                            secondary_com_port.KeyOnDTR = true;
                            break;
                        case KeyerLine.RTS:
                            secondary_com_port.KeyOnDTR = false;
                            secondary_com_port.KeyOnRTS = true;
                            break;
                        case KeyerLine.None:
                            secondary_com_port.KeyOnRTS = false;
                            secondary_com_port.KeyOnDTR = false;
                            break;
                    }
                }
            }
        }

        private static bool keyerptt = false;
        public static bool KeyerPTT
        {
            get { return keyerptt; }
            set { keyerptt = value; }
        }

        private static bool cat_ptt = false;
        public static bool CATPTT
        {
            get { return cat_ptt; }
            set { cat_ptt = value; }
        }
        
        public static bool SetPrimaryInput(string s)
        {
            if (s.ToUpper().StartsWith("COM") && s.Length > 3)
            {
                int port = 0;
                bool valid = int.TryParse(s.Substring(3, s.Length-3), out port);
                
                if (!valid) return false;

                if (primary_com_port != null)
                {
                    if (primary_com_port.IsOpen)
                        primary_com_port.Close();
                    primary_com_port = null;
                }

                primary_com_port = new SDRSerialPort(port);//, null);
                try
                {
                    primary_com_port.Open();
                }
                catch (Exception)
                {
                    primary_com_port = null;
                    return false;
                }

                if (!primary_com_port.IsOpen)
                {
                    primary_com_port = null;
                    return false;
                }

                primary_com_port.UseForPaddles = true;
                primary_input = s;
                return true;
            }

            switch (s)
            {
                case "Radio":
                case "CAT":
                case "None":
                    if (primary_com_port != null)
                    {
                        if (primary_com_port.IsOpen)
                            primary_com_port.Close();
                        primary_com_port = null;
                    }
                    primary_input = s;
                    break;
            }
            return true;
        }

        public static bool SetSecondaryInput(string s)
        {
            if (s.ToUpper().StartsWith("COM") && s.Length > 3)
            {
                int port = 0;
                bool valid = int.TryParse(s.Substring(3, s.Length - 3), out port);

                if (!valid) return false;

                if (secondary_com_port != null)
                {
                    if (secondary_com_port.IsOpen)
                        secondary_com_port.Close();
                    secondary_com_port = null;
                }

                secondary_com_port = new SDRSerialPort(port);//, null);

                try
                {
                    secondary_com_port.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (!secondary_com_port.IsOpen)
                {
                    secondary_com_port = null;
                    return false;
                }

                secondary_com_port.UseForKeyPTT = true;
                SecondaryKeyLine = secondary_key_line;
                SecondaryPTTLine = secondary_ptt_line;
                secondary_input = s;
                return true;
            }

            switch (s)
            {
                case "Radio":
                case "CAT":
                case "None":
                    if (secondary_com_port != null)
                    {
                        if (secondary_com_port.IsOpen)
                            secondary_com_port.Close();
                        secondary_com_port = null;
                    }
                    primary_input = s;
                    break;
            }
            return true;
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(primary_com_port.BasePort.ReadExisting()));
        }

    }
}

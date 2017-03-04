//==============================================================
//Written by: Philip A Covington, N8VB
//
//This software is licensed under the GNU General Public License
//==============================================================
//SerialRXEvent.cs
//
//==============================================================

using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    public delegate void SerialRXEventHandler(object source, SerialRXEvent e);

    public class SerialRXEvent : EventArgs
    {
        internal string buffer = null;

        public SerialRXEvent(string buffer)
        {
            this.buffer = buffer;
        }
    }

}

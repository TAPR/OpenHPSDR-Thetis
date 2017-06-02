//=================================================================
// notchlist.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2011  FlexRadio Systems
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
// You may contact us via email at: sales@flexradio.com.
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

namespace Thetis
{
    class NotchList
    {
        private static List<Notch> list = new List<Notch>();
        public static List<Notch> List
        {
            get { return list; }
        }

        /// <summary>
        /// Figures out whether a notch is nearby.
        /// </summary>
        /// <param name="freq">The RF frequency in MHz</param>
        /// <param name="delta">The delta in Hz</param>
        /// <returns>True if a filter is within the freq +/- delta window, False otherwise.</returns>
        public static bool NotchNearFreq(double freq, int delta)
        {
            double delta_mhz = delta*1e-6;
            foreach (Notch n in list)
            {
                if (Math.Abs(freq - n.Freq) < delta_mhz)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a list of notches in a given RF window.  This takes into
        /// account the bandwidth of the notch.
        /// </summary>
        /// <param name="freq">The RF frequency in MHz</param>
        /// <param name="low">The low offset from the RF frequency in Hz</param>
        /// <param name="high">The high offset from the RF frequency in Hz</param>
        /// <returns>A List of the notches within the RF window</returns>
        public static List<Notch> NotchesInBW(double freq, int low, int high)
        {
            List<Notch> l = new List<Notch>();
            double min = freq + low*1e-6;
            double max = freq + high*1e-6;

            foreach (Notch n in list)
            {
                if (n.Freq + 200*1e-6 > min && n.Freq - 200*1e-6 < max)
                    l.Add(n);
            }

            return l;
        }

        /// <summary>
        /// Removed the first notch found inside the supplied parameters
        /// </summary>
        /// <param name="notch">The notch to be removed from the list</param>
        /// <returns>void</returns>
        public static void RemoveNotch(Notch notch)
        {
            list.Remove(notch);
        }

    }
}

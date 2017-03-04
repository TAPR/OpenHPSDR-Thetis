//=================================================================
// Channel.cs
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

namespace Thetis
{
    public class Channel : IComparable
    {
        /// <summary>
        /// Center frequency for the channel in MHz
        /// </summary>
        private double freq;
        public double Freq
        {
            get { return freq; }
            set { freq = value; }
        }

        /// <summary>
        /// Bandwidth of the channel in Hz
        /// </summary>
        private int bw;
        public int BW
        {
            get { return bw; }
            set { bw = value; }
        }

        /// <summary>
        /// Creates a channel object
        /// </summary>
        /// <param name="f">Starting frequency in MHz</param>
        /// <param name="bandwidth">Starting bandwidth in Hz</param>
        public Channel(double f, int bandwidth)
        {
            freq = f;
            bw = bandwidth;            
        }

        public Channel(double f, int bandwidth, bool perm, int dep)
        {
            freq = f;
            bw = bandwidth;
        }

        /// <summary>
        /// Displays the Channel details in a string
        /// </summary>
        /// <returns>The contents of the Channel in a string</returns>
        public override string ToString()
        {
            return freq.ToString("R") + " MHz| " + bw + " Hz";
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            return this.freq.CompareTo(((Channel)obj).freq);
        }

        public Channel Copy()
        {
            return new Channel(this.freq, this.bw);
        }

        public bool InBW(double low, double high)
        {
            double channel_low = Freq - (BW / 2) * 1e-6;
            double channel_high = Freq + (BW / 2) * 1e-6;

            return ((low > channel_low && low < channel_high) ||
                (high > channel_low && high < channel_high) ||
                (channel_low > low && channel_high < high));
        }
    }
}

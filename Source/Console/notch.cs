//=================================================================
// notch.cs
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
    class Notch : IComparable
    {
        /// <summary>
        /// Center frequency for the filter in MHz
        /// </summary>
        private double freq;
        public double Freq
        {
            get { return freq; }
            set { freq = value; }
        }

        /// <summary>
        /// Bandwidth of the notch in Hz
        /// </summary>
        private int bw;
        public int BW
        {
            get { return bw; }
            set { bw = value; }
        }

        /// <summary>
        /// Whether to store this notch for later use
        /// </summary>
        bool permanent = false;
        public bool Permanent
        {
            get { return permanent; }
            set { permanent = value; }
        }

        /// <summary>
        /// True if the notch details should be shown
        /// </summary>
        private bool details;
        public bool Details
        {
            get { return details; }
            set { details = value; }
        }

        /// <summary>
        /// If details are being shown, this will let us know for which receiver
        /// </summary>
        private int rx;
        public int RX
        {
            get { return rx; }
            set { rx = value; }
        }

        /// <summary>
        /// Depth of the notch = 1, 2, or 3 for how many notches to place on the frequency
        /// </summary>
        private int depth;
        public int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        /// <summary>
        /// Creates a notch object
        /// </summary>
        /// <param name="f">Starting frequency in MHz</param>
        /// <param name="bandwidth">Starting bandwidth in Hz</param>
        public Notch(double f, int bandwidth)
        {
            freq = f;
            bw = bandwidth;
            rx = 1;
            depth = 1;
        }

        public Notch(double f, int bandwidth, bool perm, int dep)
        {
            freq = f;
            bw = bandwidth;
            permanent = perm;
            rx = 1;
            depth = dep;
        }

        /// <summary>
        /// Displays the notch details in a string
        /// </summary>
        /// <returns>The contents of the notch in a string</returns>
        public override string ToString()
        {
            return freq.ToString("R") + " MHz| " + bw + " Hz| perm: " + permanent + "| depth: " + depth;
        }

        public static Notch Parse(string s)
        {
            int index = s.IndexOf("MHz");
            double f = double.Parse(s.Substring(0, index));

            int index2 = s.LastIndexOf("Hz");
            int bandwidth = int.Parse(s.Substring(index+5, index2-(index+5)));

            index = s.IndexOf("perm:");
            index2 = s.IndexOf("| depth:");

            int depth;
            bool perm;
            if (index2 != -1)
            {
                perm = bool.Parse(s.Substring(index + 5, index2 - (index + 5)));
                depth = int.Parse(s.Substring(index2 + 8));
                if (depth < 1 || depth > 3) depth = 1;
            }
            else
            {
                perm = bool.Parse(s.Substring(index + 5));
                depth = 1; 
            }

            return new Notch(f, bandwidth, perm, depth);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            return freq.CompareTo(((Notch)obj).freq);
        }

        public Notch Copy()
        {
            return new Notch(freq, bw, permanent, depth);
        }
    }
}

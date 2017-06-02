//=================================================================
// MemoryRecord.cs
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.ComponentModel;

namespace Thetis
{
    public class MemoryRecord : IComparable, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor required for serialization
        /// </summary>
        public MemoryRecord()
        {

        }

        public MemoryRecord(string _group, double _rxfreq, string _name, DSPMode _dsp_mode, bool _scan,
            string _tune_step, FMTXMode _repeater_mode, double _fm_tx_offset_mhz, bool _ctcss_on, double _ctcss_freq,
            int _power, int _deviation, bool _split, double _txfreq, Filter _filter, int _filterlow, int _filterhigh, 
            string _comments, AGCMode _agc_mode, int _agc_thresh)
        {
            group = _group;
            rx_freq = _rxfreq;
            name = _name;
            dsp_mode = _dsp_mode;
            scan = _scan;
            tune_step = _tune_step;
            repeater_mode = _repeater_mode;
            rptr_offset = _fm_tx_offset_mhz;
            ctcss_on = _ctcss_on;
            ctcss_freq = _ctcss_freq;
            power = _power;
            deviation = _deviation;
            split = _split;
            tx_freq = _txfreq;
            rx_filter = _filter;
            rx_filter_low = _filterlow;
            rx_filter_high = _filterhigh;
            comments = _comments;
            agc_mode = _agc_mode;
            agct = _agc_thresh;
        }

        public MemoryRecord(MemoryRecord rec)
        {
            group = rec.group;
            rx_freq = rec.rx_freq;
            name = rec.name;
            dsp_mode = rec.dsp_mode;
            scan = rec.scan;
            tune_step = rec.tune_step;
            repeater_mode = rec.repeater_mode;
            rptr_offset = rec.rptr_offset;
            ctcss_on = rec.ctcss_on;
            ctcss_freq = rec.ctcss_freq;
            power = rec.power;
            deviation = rec.deviation;
            split = rec.split;
            tx_freq = rec.tx_freq;
            rx_filter = rec.rx_filter;
            rx_filter_low = rec.rx_filter_low;
            rx_filter_high = rec.rx_filter_high;
            comments = rec.comments;
            agc_mode = rec.agc_mode;
            agct = rec.agct;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged; // to implement the INotifyPropertyChanged interface

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, e);
        }

        #endregion

        #region Properties

        private string group = "";
        public string Group
        {
            get { return group; }
            set
            {
                group = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Group"));
            }
        }

        private double rx_freq = 10.0;
        public double RXFreq
        {
            get { return rx_freq; }
            set
            {
                rx_freq = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("RXFreq"));
            }
        }

        private string name = "";
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private DSPMode dsp_mode = DSPMode.LSB;
        public DSPMode DSPMode
        {
            get { return dsp_mode; }
            set
            {
                dsp_mode = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("DSPMode"));
            }
        }

        private bool scan = true;
        public bool Scan
        {
            get { return scan; }
            set
            {
                scan = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Scan"));
            }
        }

        private string tune_step = "10Hz";
        public string TuneStep
        {
            get { return tune_step; }
            set
            {
                tune_step = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("TuneStep"));
            }
        }

        private FMTXMode repeater_mode = 0;
        public FMTXMode RPTR
        {
            get { return repeater_mode; }
            set
            {
                repeater_mode = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("RPT"));
            }
        }

        private double rptr_offset = 0.1;
        /// <summary>
        /// Repeater (FM TX) Offset in MHz
        /// </summary>        
        public double RPTROffset
        {
            get { return rptr_offset; }
            set
            {
                rptr_offset = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("RPTOffset"));
            }
        }

        private bool ctcss_on = false;
        public bool CTCSSOn
        {
            get { return ctcss_on; }
            set
            {
                ctcss_on = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("CTCSSOn"));
            }
        }

        private double ctcss_freq = 0.0;
        public double CTCSSFreq
        {
            get { return ctcss_freq; }
            set
            {
                ctcss_freq = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("CTCSSFreq"));
            }
        }        

        private int deviation = 5000;
        public int Deviation
        {
            get { return deviation; }
            set
            {
                deviation = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Deviation"));
            }
        }

        private int power = 0;
        public int Power
        {
            get { return power; }
            set
            {
                power = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Power"));
            }
        }

        private bool split = false;
        public bool Split
        {
            get { return split; }
            set
            {
                split = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Split"));
            }
        }

        private double tx_freq = 10.0;
        public double TXFreq
        {
            get { return tx_freq; }
            set
            {
                tx_freq = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("TXFreq"));
            }
        }

        private Filter rx_filter = 0;
        public Filter RXFilter
        {
            get { return rx_filter; }
            set
            {
                rx_filter = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Filter"));
            }
        }

        private int rx_filter_low = 0;
        public int RXFilterLow
        {
            get { return rx_filter_low; }
            set
            {
                rx_filter_low = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("FilterLow"));
            }
        }

        private int rx_filter_high = 0;
        public int RXFilterHigh
        {
            get { return rx_filter_high; }
            set
            {
                rx_filter_high = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("FilterHigh"));
            }
        }

        private string comments = "";
        public string Comments
        {
            get { return comments; }
            set
            {
                comments = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Comments"));
            }
        }

        private AGCMode agc_mode = AGCMode.MED;
        public AGCMode AGCMode
        {
            get { return agc_mode; }
            set
            {
                agc_mode = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("AGCMode"));
            }
        }

        private int agct = 80;
        public int AGCT
        {
            get { return agct; }
            set
            {
                agct = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("AGCT"));
            }
        }

        #endregion

        #region Routines

        public int CompareTo(object obj) // to implement the IComparable interface
        {
            MemoryRecord rec = (MemoryRecord)obj;
            if (this.Group != rec.Group)
                return this.Group.CompareTo(rec.Group);

            if (this.RXFreq != rec.RXFreq)
                return this.RXFreq.CompareTo(rec.RXFreq);

            return this.Name.CompareTo(rec.Name);
        }

        #endregion
    }
}

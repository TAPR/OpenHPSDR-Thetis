//=================================================================
// MemoryRecord.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2003-2013  FlexRadio Systems
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /*
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
        */
        //=====================================================================================================================
        // ke9ns add start/stop date and time and determine if repeating and if you want to record audio
        public MemoryRecord(string _group, double _rxfreq, string _name, DSPMode _dsp_mode, bool _scan,
                 string _tune_step, FMTXMode _repeater_mode, double _fm_tx_offset_mhz, bool _ctcss_on, double _ctcss_freq,
                 int _power, int _deviation, bool _split, double _txfreq, Filter _filter, int _filterlow, int _filterhigh,
                 string _comments, AGCMode _agc_mode, int _agc_thresh, 
                 DateTime _StartDate, bool _ScheduleOn, int _Duration, bool _Repeating, bool _Recording, bool _Repeatingm, int _Extra) // string _StartTime, 
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

            startdate = _StartDate; // ke9ns add  for scheduled freq change and optional recording
            scheduleon = _ScheduleOn; // ke9ns add  for scheduled freq change and optional recording 
            duration = _Duration;// ke9ns add  for scheduled freq change and optional recording
            repeating = _Repeating;// ke9ns add  for scheduled freq change and optional recording
            recording = _Recording;// ke9ns add  for scheduled freq change and optional recording
            repeatingm = _Repeatingm;// ke9ns add  for scheduled freq change and optional recording
            extra = _Extra;// ke9ns add  for scheduled freq change and optional recording

      /*   
       
            startdate = StartDate; // ke9ns add  for scheduled freq change and optional recording
            scheduleon = ScheduleOn; // ke9ns add  for scheduled freq change and optional recording 
            duration = Duration;// ke9ns add  for scheduled freq change and optional recording
            repeating = Repeating;// ke9ns add  for scheduled freq change and optional recording
            recording = Recording;// ke9ns add  for scheduled freq change and optional recording
            repeatingm = Repeatingm;// ke9ns add  for scheduled freq change and optional recording
            extra = Extra;// ke9ns add  for scheduled freq change and optional recording
*/


        } // memoryrecord


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


            startdate = rec.startdate; // ke9ns add  for scheduled freq change and optional recording
            scheduleon = rec.scheduleon; // ke9ns add  for scheduled freq change and optional recording 
            duration = rec.duration; // ke9ns add  for scheduled freq change and optional recording
            repeating = rec.repeating;// ke9ns add  for scheduled freq change and optional recording
            recording = rec.recording;// ke9ns add  for scheduled freq change and optional recording
            repeatingm = rec.repeatingm;// ke9ns add  for scheduled freq change and optional recording
            extra = rec.extra;// ke9ns add  for scheduled freq change and optional recording


        } //  MemoryRecord(MemoryRecord rec)


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


        //========================================================================
        // ke9ns mod  NAME is now can be set as a hyperlink
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




        //================================================================================================
        //==================================================================================================
        // ke9ns add  also used in memoryform and CATCommands.cs

        private DateTime startdate = DateTime.Now;  // ke9ns add Date and Time for schedule

        public DateTime StartDate
        {
            get { return startdate; }
            set
            {
                startdate = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("StartDate"));
            }
        }

      
        private int duration = 25; // ke9ns Duration of recording if option enabled
        public int Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Duration"));
            }
        }

        private bool recording = false;  // ke9ns add Turn recording during schedule on/off
        public bool Recording
        {
            get { return recording; }
            set
            {
                recording = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Recording"));
            }
        }

        private bool repeating = false;  // ke9ns add repeat schedule weekly
        public bool Repeating
        {
            get { return repeating; }
            set
            {
                repeating = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Repeating"));
            }
        }

        private bool repeatingm = false;  // ke9ns add repeat schedule weekly
        public bool Repeatingm
        {
            get { return repeatingm; }
            set
            {
                repeatingm = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Repeatingm"));
            }
        }

        //============================================================================


   


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
/*
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
*/
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

       
      

        private bool scheduleon = false; // ke9ns add Turn Scedule ON/OFF
        public bool ScheduleOn
        {
            get { return scheduleon; }
            set
            {
                scheduleon = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("ScheduleOn"));
            }
        }
        private int extra = 0; // ke9ns Duration of recording if option enabled
        public int Extra
        {
            get { return extra; }
            set
            {
                extra = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Extra"));
            }
        }




        // ke9ns add above
        //================================================================================================
        //==================================================================================================



        #endregion

        #region Routines

        public int CompareTo(object obj) // to implement the IComparable interface
        {
            MemoryRecord rec = (MemoryRecord)obj;

            if (this.Group != rec.Group)
            {
                return this.Group.CompareTo(rec.Group);
            }

            if (this.RXFreq != rec.RXFreq)
            {
                return this.RXFreq.CompareTo(rec.RXFreq);
            }

            return this.Name.CompareTo(rec.Name);

        } // compareto

        #endregion
    } // memoryrecord

} // powerSDR

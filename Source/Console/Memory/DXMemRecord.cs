//=================================================================
// DXMemRecord.cs
// created by ke9ns
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

//=================================================================

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thetis
{
    public class DXMemRecord : IComparable, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor required for serialization
        /// </summary>
        public DXMemRecord()
        {

        }

        public DXMemRecord(string _dxurl)
        {
           
            dxurl = _dxurl;
          

        }

        public DXMemRecord(DXMemRecord rec)
        {
            
            dxurl = rec.dxurl;
          
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

        private string dxurl = "k1rfi.com:7300";
        public string DXURL
        {
            get { return dxurl; }
            set
            {
                dxurl = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("DXURL"));
            }
        }




        #endregion

        #region Routines

        public int CompareTo(object obj) // to implement the IComparable interface
        {
            DXMemRecord rec = (DXMemRecord)obj;

            if (this.DXURL != rec.DXURL)
            {
                return this.DXURL.CompareTo(rec.DXURL);
            }

           

            return this.DXURL.CompareTo(rec.DXURL);

        } // compareto

        #endregion
    } // DXMemrecord

} // powerSDR

//=================================================================
// MemoryList.cs
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
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Thetis
{
    public class MemoryList
    {
        #region Constructor

        public MemoryList()
        {

        }

        #endregion

        #region Properties

        private SortableBindingList<MemoryRecord> list = new SortableBindingList<MemoryRecord>();
        public SortableBindingList<MemoryRecord> List
        {
            get
            {
                return list;
            }
        }

        private static int current_major_version = 1;
        private int major_version = 1;        
        public int MajorVersion
        {
            get { return major_version; }
            set { major_version = value; }
        }

        private static int current_minor_version = 1;
        private int minor_version = 1;        
        public int MinorVersion
        {
            get { return minor_version; }
            set { minor_version = value; }
        }

        #endregion

        #region Routines

        public void Save()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + "\\OpenHPSDR\\";
            string file_name = path + "memory.xml";

            TextWriter writer = new StreamWriter(file_name);

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(MemoryList),
                    new Type[] { typeof(MemoryRecord), typeof(SortableBindingList<MemoryRecord>), typeof(int) });
                ser.Serialize(writer, this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            writer.Close();
        }

        public static MemoryList Restore()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + "\\OpenHPSDR\\";
            string file_name = path + "memory.xml";

            MemoryList mem_list = new MemoryList();
            if (!File.Exists(file_name)) return mem_list; // no file, just return an empty list

            StreamReader reader = new StreamReader(file_name);

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(MemoryList),
                    new Type[] { typeof(MemoryRecord), typeof(SortableBindingList<MemoryRecord>), typeof(int) });
                mem_list = (MemoryList)ser.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            reader.Close();

            return mem_list;
        }

        public void CheckVersion()
        {
            if (this.major_version == MemoryList.current_major_version &&
                this.minor_version == MemoryList.current_minor_version)
                return;

            if (this.major_version == 1 && this.minor_version == 0)
            {
                // go modify the data as appropriate
            }
        }

        #endregion
    }
}

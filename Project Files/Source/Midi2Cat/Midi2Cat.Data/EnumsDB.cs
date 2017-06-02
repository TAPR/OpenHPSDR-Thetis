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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Midi2Cat.Data
{
    public class EnumsDB
    {
        private DataSet _ds;

        public DataSet ds
        {
            get
            {
                if (_ds == null)
                {
                    _ds = new DataSet();
                    AddControlTypes();
                    AddCatCmds();
                }
                return _ds;
            }
        }

        private void AddControlTypes()
        {
            ds.Tables.Add("ControlTypes");
            DataTable t = null;
            DataColumn[] keys = new DataColumn[2];
            t = ds.Tables["ControlTypes"];

            t.Columns.Add("ControlId", typeof(int));
            t.Columns.Add("ControlDescription", typeof(string));
            keys[0] = t.Columns[0];
            keys[1] = t.Columns[1];
            t.PrimaryKey = keys;

            ControlType[] values = (ControlType[])Enum.GetValues(typeof(ControlType));
            foreach (ControlType value in values)
            {
                DataRow dr = t.NewRow();
                dr["ControlId"] = (int)value;
                dr["ControlDescription"] = value.ToString().Replace("_"," ");
                t.Rows.Add(dr);
                dr.AcceptChanges();
            }
        }

        private void AddCatCmds()
        {
            ds.Tables.Add("CatCmds");
            DataTable t = null;
            DataColumn[] keys = new DataColumn[1];
            t = ds.Tables["CatCmds"];
            t.Columns.Add("CmdId", typeof(int));
            t.Columns.Add("CmdDescription", typeof(string));
            t.Columns.Add("ControlType", typeof(int));
            t.Columns.Add("InUse", typeof(bool));
            keys[0] = t.Columns[0];
            t.PrimaryKey = keys;

            CatCmd[] values = (CatCmd[])Enum.GetValues(typeof(CatCmd));
            foreach (CatCmd value in values)
            {
                CatCommandAttribute attr = CatCmdDb.Get(value);
                DataRow dr = t.NewRow();
                dr["CmdId"] = (int)value;
                dr["CmdDescription"] = attr.Desc;
                dr["ControlType"] = attr.ControlType;
                dr["InUse"] = false;
                t.Rows.Add(dr);
                dr.AcceptChanges();
            }
        }

        public void BindToDataSource(BindingSource source, string tableName)
        {
            DataTableCollection tables = ds.Tables;
            DataView view = new DataView(tables[tableName]);
            source.DataSource = view;
            //source.DataMember = tableName;
        }

        public void SetCatCmdInUse(CatCmd catCmd, bool inUse)
        {
            DataTable t = ds.Tables["CatCmds"];
            DataRow dr = t.Rows.Find(catCmd);
            if (dr != null)
            {
                dr["InUse"] = inUse;
                dr.AcceptChanges();
            }
        }
                
    }//class
}//namespace

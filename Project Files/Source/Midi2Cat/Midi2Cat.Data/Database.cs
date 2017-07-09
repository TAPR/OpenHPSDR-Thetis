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

Modifications to support the Behringer CMD PL-1 controller
by Chris Codella, W2PA, Feb 2017.  Indicated by //-W2PA comment lines.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Midi2Cat.Data
{
    public class Midi2CatDatabase
    {
        public DataSet importDS=null;
        public DataSet ds;

        private const string SettingsTable = "Midi2Cat--Settings";
        private const string DefaultTableName = "Not Saved";
        private string file_name = null;

        public Midi2CatDatabase(string fileName)
        {
            file_name = fileName;
            ds = new DataSet("Midi2CatData");
            try
            {
                if (File.Exists(file_name))
                {
                    ds.ReadXml(file_name);
                }
                else
                {
                }
            }
            catch
            {
            }
            return;
        }

        public string FileName
        {
            get
            {
                return file_name;
            }
        }

        public void SaveChanges(string MidiDeviceName)
        {
            try
            {
                if (MidiDeviceName != null)
                {
                    string LoadedMappingName = GetLoadedMappingName(MidiDeviceName);
                    if (LoadedMappingName != null)
                    {
                        SaveMappingAs(MidiDeviceName, LoadedMappingName, true);
                    }
                }
                ds.WriteXml(file_name, XmlWriteMode.WriteSchema);
            }
            catch
            {
                return;
            }
        }
        
        public void Exit()
        {
            SaveChanges(null);
            ds = null;
        }

        private DataTable GetTable(string MidiDeviceName, DataSet overrideDS = null)
        {
            DataSet DS = ds;
            if (overrideDS != null)
                DS = overrideDS;
            DataTable t = null;
            if (DS.Tables.IndexOf(MidiDeviceName) < 0)
            {
                DS.Tables.Add(MidiDeviceName);
                DataColumn[] keys = new DataColumn[1];
                t = DS.Tables[MidiDeviceName];
                t.Columns.Add("MidiControlId", typeof(int));
                t.Columns.Add("MidiControlName", typeof(string));
                t.Columns.Add("MidiControlType", typeof(int));
                t.Columns.Add("MinValue", typeof(int));
                t.Columns.Add("MaxValue", typeof(int));
                t.Columns.Add("CatCmdId", typeof(int));
                t.Columns.Add("MidiOutCmdDown", typeof(string));
                t.Columns.Add("MidiOutCmdUp", typeof(string));
                t.Columns.Add("MidiOutCmdSetValue", typeof(string));
                keys[0] = t.Columns[0];
                t.PrimaryKey = keys;
            }
            else
            {
                t = DS.Tables[MidiDeviceName];
                // Add Columns if missing 
                bool columnAdded = false;
                if (t.Columns["MidiOutCmdDown"] == null)
                {
                    columnAdded = true;
                    t.Columns.Add("MidiOutCmdDown", typeof(string));
                }
                if (t.Columns["MidiOutCmdUp"] == null)
                {
                    columnAdded = true;
                    t.Columns.Add("MidiOutCmdUp", typeof(string));
                }
                if (t.Columns["MidiOutCmdSetValue"] == null)
                {
                    columnAdded = true;
                    t.Columns.Add("MidiOutCmdSetValue", typeof(string));
                }
                if (columnAdded)
                {
                    SaveChanges(MidiDeviceName);
                }
            }
            return t;
        }

        public void AddRow(string MidiDeviceName, DataRow row)
        {
            DataTable t = GetTable(MidiDeviceName);
            t.Rows.Add(row);
        }

        private void AddRow(string MidiDeviceName, ControllerMapping mapping)
        {
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = PopulateRow(t.NewRow(), mapping);
            t.Rows.Add(dr);
        }

        private bool UpdateRow(string MidiDeviceName, ControllerMapping mapping)
        {
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows.Find(mapping.MidiControlId);
            if (dr != null)
            {
                dr = PopulateRow(dr, mapping);
                return true;
            }
            return false;
        }

        private ControllerMapping GetRow(string MidiDeviceName, int Idx)
        {
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows[Idx];
            if (dr != null)
            {
                return PopulateMapping(dr);
            }
            return null;
        }

        public void DeleteRow(string MidiDeviceName, ControllerMapping mapping)
        {
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows.Find(mapping.MidiControlId);
            if (dr != null)
            {
                t.Rows.Remove(dr);
            }
        }

        public void DeleteRow(string MidiDeviceName, int MidiControlId)
        {
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows.Find(MidiControlId);
            if (dr != null)
            {
                t.Rows.Remove(dr);
            }
        }

        public void UpdateOrAdd(string MidiDeviceName, ControllerMapping mapping)
        {
            if (UpdateRow(MidiDeviceName, mapping) == false)
            {
                AddRow(MidiDeviceName, mapping);
            }
        }

        public List<ControllerMapping> GetMappings(string MidiDeviceName, MappingFilter filter)
        {
            List<ControllerMapping> mappings = new List<ControllerMapping>();
            DataTable t = GetTable(MidiDeviceName);
            for (int i = 0; i < t.Rows.Count; i++)
            {
                ControllerMapping mapping = GetRow(MidiDeviceName, i);
                mapping.CatCmd = CatCmdDb.Get(mapping.CatCmd.CatCommandId);
                if (filter == MappingFilter.None)
                    mappings.Add(mapping);
                else if (filter == MappingFilter.Active)
                {
                    if (mapping.CatCmd.CatCommandId != CatCmd.None)
                        mappings.Add(mapping);
                }
                else if (filter == MappingFilter.InActive)
                {
                    if (mapping.CatCmd.CatCommandId == CatCmd.None)
                        mappings.Add(mapping);
                }
            }
            return mappings;
        }

        public ControllerMapping GetMapping(string MidiDeviceName, int MidiControlId)
        {
            ControllerMapping mapping = null;
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows.Find(MidiControlId);
            if (dr != null)
            {
                mapping = PopulateMapping(dr);
            }
            return mapping;
        }

        public ControllerMapping GetReverseMapping(string MidiDeviceName, CatCmd cmd)  //-W2PA To allow flowing commands back to MIDI devices
        {
            ControllerMapping mapping = null;
            DataTable t = GetTable(MidiDeviceName);
            DataRow dr = t.Rows.Find(cmd);

            string sel = "CatCmdId = " + Convert.ToString((int)cmd);
            DataRow[] dr1 = t.Select(sel);

            if (dr1.Length == 0) return mapping;
            if (dr1[0] != null)
            {
                mapping = PopulateMapping(dr1[0]);
            }
            return mapping;
        }

        private ControllerMapping PopulateMapping(DataRow dr)
        {
            ControllerMapping mapping = new ControllerMapping();
            mapping.MidiControlId = (int)dr["MidiControlId"];
            mapping.MidiControlName = (string)dr["MidiControlName"];
            mapping.MidiControlType = FixUp.FixControlType(((int)dr["MidiControlType"]));
            mapping.MinValue = (int)dr["MinValue"];
            mapping.MaxValue = (int)dr["MaxValue"];
            mapping.CatCmdId = (CatCmd)dr["CatCmdId"];
            mapping.CatCmd = CatCmdDb.Get(mapping.CatCmdId);
            mapping.MidiOutCmdDown = ConvertFromDBVal<string>(dr["MidiOutCmdDown"]);
            mapping.MidiOutCmdUp = ConvertFromDBVal<string>(dr["MidiOutCmdUp"]);
            mapping.MidiOutCmdSetValue = ConvertFromDBVal<string>(dr["MidiOutCmdSetValue"]);
            return mapping;
        }

        public DataRow PopulateRow(DataRow dr, ControllerMapping mapping)
        {
            dr["MidiControlId"] = mapping.MidiControlId;
            dr["MidiControlName"] = mapping.MidiControlName;
            dr["MidiControlType"] = mapping.MidiControlType;
            dr["MinValue"] = mapping.MinValue;
            dr["MaxValue"] = mapping.MaxValue;
            dr["CatCmdId"] = (int)mapping.CatCmd.CatCommandId;
            dr["MidiOutCmdDown"] = mapping.MidiOutCmdDown;
            dr["MidiOutCmdUp"] = mapping.MidiOutCmdUp;
            dr["MidiOutCmdSetValue"] = mapping.MidiOutCmdSetValue;
            return dr;
        }

        public void BindToDataSource(BindingSource source, string MidiDeviceName)
        {
            source.DataSource = ds;
            GetTable(MidiDeviceName);
            source.DataMember = MidiDeviceName;
        }

        public bool IsDeviceSetup(string MidiDeviceName)
        {
            if (ds.Tables.IndexOf(MidiDeviceName) >= 0)
            {
                DataTable dt = GetTable(MidiDeviceName);
                List<ControllerMapping> mappings = GetMappings(MidiDeviceName, MappingFilter.Active);
                if (mappings.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveMapping(string MidiDeviceName, int catCmd)
        {
            if (ds.Tables.IndexOf(MidiDeviceName) >= 0)
            {
                DataTable dt = GetTable(MidiDeviceName);
                foreach (DataRow row in dt.Rows)
                {
                    if (((int)row["CatCmdId"]) == catCmd)
                    {
                        row["CatCmdId"] = (int)CatCmd.None;
                        row["MidiOutCmdSetValue"] = "";
                        row.AcceptChanges();
                        break;
                    }
                }
            }
        }

        public T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }

        public bool SaveMappingAs(string midiDeviceName, string Name, bool replace)
        {
            if ((ds.Tables.IndexOf(Name) >= 0) && replace == false)
                return false;
            DataTable srcDT = GetTable(midiDeviceName);
            DataTable destDT = GetTable(Name);
            destDT.Clear();
            foreach (DataRow srcRow in srcDT.Rows)
            {
                destDT.ImportRow(srcRow);
            }
            string prefix = GetPrefixFromMidiDeviceName(midiDeviceName);
            SetSetting(prefix+"LoadedMapping", Name);
            this.SaveChanges(null);
            return true;
        }

        public string[] GetSavedMappings()
        {
            List<string> tables = new List<string>();
            foreach (DataTable table in this.ds.Tables)
            {
                if (table.TableName.Contains('-') == false )
                {
                    if (table.TableName != DefaultTableName)
                    {
                        tables.Add(table.TableName);
                    }
                }
            }
            return tables.ToArray();
        }


        public bool LoadMapping(string midiDeviceName, string Name)
        {
            if (midiDeviceName.ToLower() == Name.ToLower() )
                return false;

            DataTable destDT = GetTable(midiDeviceName);
            DataTable srcDT = GetTable(Name);
            destDT.Rows.Clear();
            foreach (DataRow srcRow in srcDT.Rows)
            {
                destDT.ImportRow(srcRow);
            }
            string prefix = GetPrefixFromMidiDeviceName(midiDeviceName);
            SetSetting(prefix+"LoadedMapping", Name);
            this.SaveChanges(null);
            return true;
        }

        public bool ExportMappings(string fileName, string[] mappings)
        {
            try
            {
                DataSet destDS = new DataSet("ExportMidi2Cat");
                foreach (DataTable table in this.ds.Tables)
                {
                    if (table.TableName.Contains('-') == false)
                    {
                        if (mappings.Contains(table.TableName))
                        {
                            DataTable destDT = GetTable(table.TableName, destDS);
                            DataTable srcDT = GetTable(table.TableName);
                            foreach (DataRow srcRow in srcDT.Rows)
                            {
                                destDT.ImportRow(srcRow);
                            }
                        }
                    }
                }
                destDS.WriteXml(fileName, XmlWriteMode.WriteSchema);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool ImportMappings(string fileName)
        {
            importDS = new DataSet("Midi2CatData");
            try
            {
                if (File.Exists(fileName))
                {
                    importDS.ReadXml(fileName);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public string[] GetImportedMappings()
        {
            List<string> tables = new List<string>();
            foreach (DataTable table in importDS.Tables)
            {
                if (table.TableName.Contains('-') == false)
                    tables.Add(table.TableName);
            }
            return tables.ToArray();
        }

        public bool AddFromImport( string[] mappings)
        {
            bool Rc = true;
            try
            {
                foreach (DataTable table in importDS.Tables)
                {
                    if (table.TableName.Contains('-') == false)
                    {
                        if (mappings.Contains(table.TableName))
                        {
                            string newName = table.TableName;
                            string suffix = "";
                            do
                            {
                                if (this.ds.Tables.Contains(table.TableName+suffix) == false)
                                    break;
                                System.Threading.Thread.Sleep(1010);
                                suffix = " Imported on " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToString("HH:mm:ss");
                            } while (true);

                            DataTable destDT = GetTable(table.TableName + suffix);
                            DataTable srcDT = GetTable(table.TableName, importDS);
                            foreach (DataRow srcRow in srcDT.Rows)
                            {
                                destDT.ImportRow(srcRow);
                            }
                        }
                    }
                }
                this.SaveChanges(null);
            }
            catch
            {
                Rc=false;
            }
            importDS = null;
            return Rc;
        }

        public bool RemoveSavedMapping(string midiDeviceName, string Name)
        {
            bool Rc=true;
            try
            {
                if (Name.Contains('-') == false)
                {
                    this.ds.Tables.Remove(Name);
                    this.SaveChanges(null);
                    string LoadedMapping = GetLoadedMappingName(midiDeviceName);
                    if (Name == LoadedMapping)
                    {
                        string prefix = GetPrefixFromMidiDeviceName(midiDeviceName);
                        SetSetting(prefix + "LoadedMapping", DefaultTableName);
                    }
                }
                else
                    Rc = false;
            }
            catch
            {
                Rc=false;
            }
            return Rc;
        }

        public bool RenameSavedMapping(string midiDeviceName, string OldName, string NewName)
        {
            bool Rc = true;
            try
            {
                if (NewName.Contains('-') == false)
                {
                    this.ds.Tables[OldName].TableName = NewName;
                    this.SaveChanges(null);
                    string LoadedMapping = GetLoadedMappingName(midiDeviceName);
                    if (OldName == LoadedMapping)
                    {
                        string prefix = GetPrefixFromMidiDeviceName(midiDeviceName);
                        SetSetting(prefix + "LoadedMapping", NewName);
                    }
                }
                else
                    Rc = false;
            }
            catch
            {
                Rc = false;
            }
            return Rc;
        }


        private DataTable GetSettingTable()
        {
            DataSet DS = ds;
            DataTable t = null;
            if (DS.Tables.IndexOf(SettingsTable) < 0)
            {
                DS.Tables.Add(SettingsTable);
                DataColumn[] keys = new DataColumn[1];
                t = DS.Tables[SettingsTable];
                t.Columns.Add("Name", typeof(string));
                t.Columns.Add("Value", typeof(string));
                t.Columns.Add("ValueType", typeof(string));
                keys[0] = t.Columns[0];
                t.PrimaryKey = keys;
            }
            else
            {
                t = DS.Tables[SettingsTable];
            }
            return t;
        }

        private void SetSetting(string name, string value)
        {
            DataTable dt = GetSettingTable();
            DataRow dr = dt.Rows.Find(name);
            if (dr != null)
            {
                dr["Value"] = value;
                dr["ValueType"] = "string";
                dr.AcceptChanges();
            }
            else
            {
                dr = dt.NewRow();
                dr["Name"] = name;
                dr["Value"] = value;
                dr["ValueType"] = "string";
                dt.Rows.Add(dr);
            }
            SaveChanges(null);
        }

        private DataRow GetSetting(string name)
        {
            DataTable dt = GetSettingTable();
            return dt.Rows.Find(name);
        }

        private string GetStringSetting(string name, string defaultValue)
        {
            DataRow dr = GetSetting(name);
            if (dr != null)
            {
                return (string)dr["Value"];
            }
            else
            {
                return defaultValue;
            }
        }

        private string GetPrefixFromMidiDeviceName(string midiDeviceName)
        {
            int pos = midiDeviceName.IndexOf('-');
            if (pos >= 0)
            {
                return midiDeviceName.Substring(0, pos+1);
            }
            else
                return null;
        }

        public string GetLoadedMappingName(string midiDeviceName)
        {
            string prefix = GetPrefixFromMidiDeviceName(midiDeviceName);
            return GetStringSetting(prefix + "LoadedMapping", DefaultTableName);
        }

    }
}

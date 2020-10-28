//=================================================================
// titlebar.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems 
// Copyright (C) 2010-2020  Doug Wigley
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
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Thetis
{
    class TitleBar
    {
        public const string BUILD_NAME = "";
        public const string BUILDER = "(G7KLJ BRANCH compiled on";

        public static DateTime BuildDate()
        {
            //System.IntPtr verText = cmaster.Build_date();
            //string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(verText);
            //return s;
            return Common.AppBuildDate();

        }

        private static string m_sTitleString = "";

        public static string GetString()
        {
            if (m_sTitleString.Length == 0)
            {
                string version = Common.GetVerNum();
                string s = "Thetis";
                // G7KLJ: Since c# does not support the __DATE__ macro, I know no better way of doing this:


                s += " v" + version;
                s += " " + BUILDER;
                s += " " + BuildDate();
                s += ")";
                m_sTitleString = s;
            }
            return m_sTitleString;
        }
    }
}
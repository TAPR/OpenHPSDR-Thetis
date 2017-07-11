//=================================================================
// titlebar.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems 
// Copyright (C) 2010-2017  Doug Wigley
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

namespace Thetis
{
    class TitleBar
    {
        public const string BUILD_NAME = "";
        public const string BUILD_DATE = "(7/11/17) beta";

        public static string GetString()
        {
            string version = GetVerNum();
            string s = "Thetis";
           // if (BUILD_NAME != "") s += " " + BUILD_NAME;
            s += " v" + version;
            if (BUILD_DATE != "") s += " " + BUILD_DATE;

            return s;
        }

        // returns the PowerSDR version number in "a.b.c" format
        public static string GetVerNum()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
        }
    }
}
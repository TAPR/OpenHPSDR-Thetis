//=================================================================
// SortedBindingList.cs
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
// ref: (http://www.tech.windowsapplication1.com/content/sortable-binding-list-custom-data-objects)
//=================================================================

namespace Thetis
{
    public class SortableBindingList<T> : System.ComponentModel.BindingList<T>
    {
        // constructors
        public SortableBindingList()
        {

        }

        public SortableBindingList(System.Collections.Generic.List<T> list)
            : base(list) { }

        // fields
        private bool m_IsSorted;
        private System.ComponentModel.ListSortDirection m_SortDirection;
        private System.ComponentModel.PropertyDescriptor m_SortProperty;

        // properties
        protected override System.ComponentModel.ListSortDirection SortDirectionCore { get { return m_SortDirection; } }
        protected override System.ComponentModel.PropertyDescriptor SortPropertyCore { get { return m_SortProperty; } }
        protected override bool IsSortedCore { get { return m_IsSorted; } }
        protected override bool SupportsSortingCore { get { return true; } }

        // methods
        protected override void RemoveSortCore() { m_IsSorted = false; }
        protected override void ApplySortCore(System.ComponentModel.PropertyDescriptor prop, System.ComponentModel.ListSortDirection direction)
        {
            if (prop.PropertyType.GetInterface("IComparable") == null)
                return;
            var _List = this.Items as System.Collections.Generic.List<T>;
            if (_List == null)
            {
                m_IsSorted = false;
            }
            else
            {
                var _Comparer = new PropertyComparer(prop.Name, direction);
                _List.Sort(_Comparer);
                m_IsSorted = true;
                m_SortDirection = direction;
                m_SortProperty = prop;
            }
            OnListChanged(new System.ComponentModel.ListChangedEventArgs(System.ComponentModel.ListChangedType.Reset, -1));
        }

        // sub class
        public class PropertyComparer : System.Collections.Generic.IComparer<T>
        {
            // properties
            private System.Reflection.PropertyInfo PropInfo { get; set; }
            private System.ComponentModel.ListSortDirection Direction { get; set; }

            // methods
            public PropertyComparer(string propName, System.ComponentModel.ListSortDirection direction)
            {
                this.PropInfo = typeof(T).GetProperty(propName);
                this.Direction = direction;
            }
            public int Compare(T x, T y)
            {
                var _X = PropInfo.GetValue(x, null);
                var _Y = PropInfo.GetValue(y, null);
                if (Direction == System.ComponentModel.ListSortDirection.Ascending)
                    return System.Collections.Comparer.Default.Compare(_X, _Y);
                else
                    return System.Collections.Comparer.Default.Compare(_Y, _X);
            }
        }
    }
}

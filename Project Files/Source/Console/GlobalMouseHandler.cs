using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Thetis
{
    public delegate void MouseMovedEvent();

    public class GlobalMouseHandler : IMessageFilter
    {
        private const int WM_MOUSEMOVE = 0x0200;
        //private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        public event MouseMovedEvent MouseUp;
        //public event MouseMovedEvent MouseDown;

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            // if (m.Msg == WM_MOUSEMOVE)
            //if (m.Msg == WM_LBUTTONDOWN)
            //{
            //    if (MouseUp != null)
            //    {
            //        MouseDown();
            //    }
            //}

            if (m.Msg == WM_LBUTTONUP)
            {
                if (MouseUp != null)
                {
                    MouseUp();
                }
            }

            // Always allow message to continue to the next filter control
            return false;
        }

        #endregion
    }
}

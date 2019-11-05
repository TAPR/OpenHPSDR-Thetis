using System.Diagnostics;
using System.Windows.Forms;

namespace RawInput_dll
{
    public class PreMessageFilter : IMessageFilter
    {
        private bool m_bIgnoreNextWheelEvent = false;

        public bool IgnoreNextWheelEvent {
            get { return m_bIgnoreNextWheelEvent; }
            set { m_bIgnoreNextWheelEvent = value; }
        }

        // true  to filter the message and stop it from being dispatched 
        // false to allow the message to continue to the next filter or control.
        public bool PreFilterMessage(ref Message m)
        {            
            if (m.Msg == Win32.WM_MOUSEWHEEL && m_bIgnoreNextWheelEvent)// || m.Msg == Win32.WM_KEYDOWN)
            {
                m_bIgnoreNextWheelEvent = false;
                return true;
            }
            return false;
        }
    }
}

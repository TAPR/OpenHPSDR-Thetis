using System;

namespace RawInput_dll
{
    public class RawInputEventArg : EventArgs
    {
        public RawInputEventArg(KeyPressEvent arg)
        {
            KeyPressEvent = arg;
        }
        public RawInputEventArg(MouseEvent arg)
        {
            MouseEvent = arg;
        }

        public KeyPressEvent KeyPressEvent { get; private set; }
        public MouseEvent MouseEvent { get; private set; }
    }
}

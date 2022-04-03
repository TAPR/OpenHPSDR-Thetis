//=================================================================
// MW0LGE 2022
//=================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmInfoBarPopup : Form
    {
        public class PopupActionSelected : EventArgs
        {
            public ucInfoBar.ActionTypes Action;
            public bool ButtonState;
            public MouseButtons Button;
        }

        public event EventHandler<PopupActionSelected> ActionClicked;
        private bool _bHasButtons = false;
        private Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> _states;

        public frmInfoBarPopup()
        {
            InitializeComponent();
        }
        public bool HasButtons
        {
            get { return _bHasButtons; }
        }

        public void SetStates(Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> states, ucInfoBar.ActionState b1, ucInfoBar.ActionState b2)
        {
            if (states == null) return;

            int added = 0;
            _states = states;

            Dictionary<string, CheckBoxTS> chkBoxes = getCheckboxesDictionary();
            foreach(CheckBoxTS chkBox in chkBoxes.Values)
            {
                chkBox.Visible = false;
            }
            _bHasButtons = false;

            foreach (KeyValuePair<ucInfoBar.ActionTypes, ucInfoBar.ActionState> kvp in _states)
            {
                ucInfoBar.ActionState state = kvp.Value;

                if (state != null && state.Action != b1.Action && state.Action != b2.Action)
                {
                    string s = "chkButton" + (added + 1).ToString();
                    if (chkBoxes.ContainsKey(s))
                    {
                        CheckBoxTS cb = chkBoxes[s];
                        cb.Tag = (int)state.Action; // used in click
                        cb.Text = state.DisplayString;
                        cb.Checked = state.Checked;
                        toolTip1.SetToolTip(cb, state.TipString);
                        cb.Visible = true;
                        added++;
                    }
                }
            }

            if (added > 0)
            {
                _bHasButtons = true;
                this.Height = (added * (chkButton1.Size.Height + 1)) + 4;
            }
            else
            {                
                // none added, set size to something?
            }
        }
      
        private Dictionary<string, CheckBoxTS> getCheckboxesDictionary()
        {
            // get all chk
            Dictionary<string, CheckBoxTS> chkBoxes = new Dictionary<string, CheckBoxTS>();
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(CheckBoxTS))
                {
                    chkBoxes.Add(c.Name, (CheckBoxTS)c);
                }
            }
            return chkBoxes;
        }
        private void chkButton1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseButtons mb = MouseButtons.None;

            CheckBoxTS cb = sender as CheckBoxTS;
            MouseEventArgs me = e as MouseEventArgs;

            if (me != null)
                mb = me.Button;

            if (cb != null)
            {
                int index = int.Parse(cb.Name.Substring(9, 1)) - 1;

                ActionClicked?.Invoke(sender, new PopupActionSelected()
                {
                    Action = (ucInfoBar.ActionTypes)cb.Tag,
                    ButtonState = cb.Checked,
                    Button = e.Button
                });
            }
        }
        public CheckBoxTS GetPopupButton(int index)
        {
            // use for skins
            Dictionary<string, CheckBoxTS> chkBoxes = getCheckboxesDictionary();
            if (chkBoxes == null) return null;

            string s = "chkButton" + (index + 1).ToString();
            if (chkBoxes.ContainsKey(s))
                return chkBoxes[s];

            return null;
        }
    }
}

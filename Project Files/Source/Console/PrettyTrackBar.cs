//=================================================================
// PrettyTrackBar.cs
//=================================================================
// PrettyTrackBar implements a TrackBar with a background and thumb
// image for improved aesthetics compared to the .NET TrackBar.
// Copyright (C) 2009  FlexRadio Systems
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
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Thetis
{
    [DefaultEvent("Scroll")]
    public class PrettyTrackBar : PictureBox
    {
        #region Variable Declaration

        private Rectangle head_rect;
        private bool sliding = false;
        private int down_x;
        private int down_y;

        #endregion

        #region Constructor and Destructor

        public PrettyTrackBar()
        {

            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        #endregion

        #region Misc Routines

        private void UpdateHeadRectPos()
        {
            int head_x = 0;
            int head_y = 0;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    int width;
                    if (head_image == null)
                    {
                        width = this.Width;
                        head_x = (int)((val - min) / (double)(max - min) * width);
                        head_y = (int)(this.Height / 2);
                    }
                    else
                    {
                        width = this.Width - head_image.Width - Padding.Horizontal;
                        head_x = (int)Math.Round((val - min) / (double)(max - min) * width) + Padding.Left;
                        head_y = (int)((this.Height - Padding.Vertical) / 2 - head_image.Height / 2) + Padding.Top;
                    }
                    break;
                case Orientation.Vertical:
                    int height;
                    if (head_image == null)
                    {
                        height = this.Height;
                        head_x = (int)(this.Width / 2);
                        head_y = (int)((val - min) / (double)(max - min) * height);
                    }
                    else
                    {
                        height = this.Height - head_image.Height - Padding.Vertical;
                        head_x = (int)((this.Width - Padding.Horizontal) / 2 - head_image.Width / 2) + Padding.Top;
                        head_y = (int)(height - (val - min) / (double)(max - min) * height) + Padding.Top;
                    }
                    break;
            }

            head_rect.X = head_x;
            head_rect.Y = head_y;
        }

        #endregion

        #region Properties

        private Image head_image = null;
        public Image HeadImage
        {
            get { return head_image; }
            set
            {
                head_image = value;
                if (head_image != null)
                {
                    head_rect.Width = head_image.Width;
                    head_rect.Height = head_image.Height;
                }
                else
                {
                    head_rect.Width = 1;
                    head_rect.Height = 1;
                }
                UpdateHeadRectPos();
                this.Invalidate();
            }
        }

        private int min = 0;
        public int Minimum
        {
            get { return min; }
            set
            {
                min = value;
                if (val < min) val = min;
                UpdateHeadRectPos();
                this.Invalidate();
            }
        }

        private int max = 100;
        public int Maximum
        {
            get { return max; }
            set
            {
                max = value;
                if (val > max) val = max;
                UpdateHeadRectPos();
                this.Invalidate();
            }
        }

        private int val = 0;
        public int Value
        {
            get { return val; }
            set
            {
                val = value;
                if (val < min) val = min;
                if (val > max) val = max;
                UpdateHeadRectPos();
                this.Invalidate();
            }
        }

        private int small_change = 1;
        public int SmallChange
        {
            get { return small_change; }
            set { small_change = value; }
        }

        private int large_change = 5;
        public int LargeChange
        {
            get { return large_change; }
            set { large_change = value; }
        }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                this.Invalidate();
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnEnabledChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (head_rect.IsEmpty) return;

            if (this.Enabled)
            {
                if (head_rect.Contains(e.X, e.Y))
                {
                    down_x = e.X;
                    down_y = e.Y;
                    sliding = true;
                }
                else
                {
                    int old_val = val;
                    int new_val;
                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            if (e.Y >= head_rect.Y && e.Y <= head_rect.Y + head_rect.Height)
                            {
                                if (e.X < head_rect.X) new_val = old_val - large_change;
                                else new_val = old_val + large_change;

                                if (new_val < min) new_val = min;
                                if (new_val > max) new_val = max;

                                Value = new_val;
                                OnScroll(this, EventArgs.Empty);
                            }
                            break;
                        case Orientation.Vertical:
                            if (e.X >= head_rect.X && e.X <= head_rect.X + head_rect.Width)
                            {
                                if (e.Y > head_rect.Y) new_val = old_val - large_change;
                                else new_val = old_val + large_change;

                                if (new_val < min) new_val = min;
                                if (new_val > max) new_val = max;

                                Value = new_val;
                                OnScroll(this, EventArgs.Empty);
                            }
                            break;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (head_rect.IsEmpty) return;

            if (this.Enabled && sliding)
            {
                int old_val = val;
                int new_val = old_val;
                int delta = 0;
                int width;
                int height;
                double percent = 0.0;

                switch (orientation)
                {
                    case Orientation.Horizontal:
                        delta = e.X - down_x;
                        width = this.Width - head_image.Width - Padding.Horizontal;

                        if (head_rect.X <= Padding.Left && delta < 0) return;
                        else if (head_rect.X >= (width + Padding.Left) && delta > 0) return;

                        percent = (head_rect.X - Padding.Left + delta) / (double)width;
                        new_val = min + (int)Math.Round((percent * (max - min)));
                        if (new_val < min) new_val = min;
                        if (new_val > max) new_val = max;

                        down_x = e.X;
                        if (down_x < Padding.Left) down_x = Padding.Left;
                        if (down_x > width + Padding.Left + head_image.Width) down_x = width + Padding.Left + head_image.Width;

                        head_rect.X += delta;
                        if (head_rect.X < Padding.Left) head_rect.X = Padding.Left;
                        if (head_rect.X > width + Padding.Left) head_rect.X = width + Padding.Left;
                        break;
                    case Orientation.Vertical:
                        delta = e.Y - down_y;
                        height = this.Height - head_image.Height - Padding.Vertical;

                        if (head_rect.Y <= Padding.Top && delta < 0) return;
                        else if (head_rect.Y >= (height + Padding.Top) && delta > 0) return;

                        percent = 1.0 - (head_rect.Y - Padding.Top + delta) / (double)height;
                        new_val = min + (int)(percent * (max - min));
                        if (new_val < min) new_val = min;
                        if (new_val > max) new_val = max;

                        down_y = e.Y;
                        if (down_y < Padding.Top) down_y = Padding.Top;
                        if (down_y > height + Padding.Top + head_image.Height) down_y = height + Padding.Top + head_image.Height;

                        head_rect.Y += delta;
                        if (head_rect.Y < Padding.Top) head_rect.Y = Padding.Top;
                        if (head_rect.Y > height + Padding.Top) head_rect.Y = height + Padding.Top;
                        break;
                }

                if (new_val != old_val)
                {
                    val = new_val;
                    OnScroll(this, EventArgs.Empty);
                }
                this.Invalidate();
                //Debug.WriteLine("delta: "+delta+"  percent: "+percent.ToString("f4")+"  new_val: " + new_val+"  down_x: "+down_x+"  head_x: "+head_rect.X);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (head_rect.IsEmpty) return;

            sliding = false;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;

            if (this.Enabled)
            {
                // draw background
                /*if(base.BackgroundImage != null)
                    g.DrawImage(base.BackgroundImage, 0, 0);*/

                // draw head
                if (head_image != null)
                    g.DrawImage(head_image, head_rect.X, head_rect.Y, head_image.Width, head_image.Height);
            }
            else
            {
                // disabled version of the thumb - same but dimmer
                float brightness = 1.0f;    // no change in brightness
                float contrast = 0.5f;      // half the contrast
                float gamma = 1.0f;         // no change in gamma
                float newBrightness = brightness - 1.0f;
                float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, 1.0f, 0},     // don't scale alpha
                    new float[] {newBrightness, newBrightness, newBrightness, 0, 1}};

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

                // draw background
                /*if(base.BackgroundImage != null)
                    g.DrawImage(base.BackgroundImage,
                        new Rectangle(0, 0, this.Width, this.Height),
                        0, 0, base.BackgroundImage.Width, base.BackgroundImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);*/

                // draw head
                if (head_image != null)
                    g.DrawImage(head_image,
                        new Rectangle(head_rect.X, head_rect.Y, head_image.Width, head_image.Height),
                        0, 0, head_image.Width, head_image.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);

                imageAttributes.Dispose();
                imageAttributes = null;
            }
        }

        public delegate void ScrollHandler(object sender, EventArgs e);
        public event ScrollHandler Scroll;
        protected virtual void OnScroll(object sender, EventArgs e)
        {
            if (Scroll != null)
            {
                Scroll(sender, e);
            }
        }

        #endregion

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta >= 120 && this.Value + large_change <= this.Maximum)
            {
                Value += large_change; 
                OnScroll(this, EventArgs.Empty);
            }
            else if (e.Delta <= -120 && this.Value - large_change >= this.Minimum)
            {
                Value -= large_change;
                OnScroll(this, EventArgs.Empty);
            }
            base.OnMouseWheel(e);
        }

    }
}
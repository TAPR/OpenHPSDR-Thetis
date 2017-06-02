//=================================================================
// display.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2016 Doug Wigley (W5WC)
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
//
// Waterfall AGC Modifications Copyright (C) 2013 Phil Harman (VK6APH)
// DirectX functions borrowed from GeneisisRadio Copyright (C)2010,2011,2012 YT7PWR Goran Radivojevic
//

using System.Linq;

namespace Thetis
{
    using System;
    // using System.IO;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    //using SlimDX;
    //using SlimDX.Direct3D9;
    //using SlimDX.Windows;
    //  using ManagedCuda;
    //  using ManagedCuda.BasicTypes;
    //  using ManagedCuda.VectorTypes;
    //  using ManagedCuda.CudaFFT;
    //using System.Linq;

    //struct Vertex
    //{
    //    public Vector4 Position;
    //    public int Color;
    //}

    //struct DXRectangle
    //{
    //    public int x1;
    //    public int x2;
    //    public int x3;
    //    public int x4;
    //    public int y1;
    //    public int y2;
    //    public int y3;
    //    public int y4;
    //}

    //struct VerticalString
    //{
    //    public int pos_x;
    //    public int pos_y;
    //    public string label;
    //    public Color color;
    //}

    //struct HorizontalString
    //{
    //    public int pos_x;
    //    public int pos_y;
    //    public string label;
    //    public Color color;
    //}

    //struct HistogramData
    //{
    //    public int X;
    //    public int Y;
    //    public Color color;
    //}

    class Display
    {
        #region Variable Declaration

        public const float CLEAR_FLAG = -999.999F;				// for resetting buffers
        public const int BUFFER_SIZE = 4096;

        public static Console console;
        public static SpotControl SpotForm;                     // ke9ns add  communications with spot.cs and dx spotter
        public static string background_image = null;

        private static int[] histogram_data = null;					// histogram display buffer
        private static int[] histogram_history;					// histogram counter

        public static float[] new_display_data;					// Buffer used to store the new data from the DSP for the display
        public static float[] current_display_data;				// Buffer used to store the current data for the display
        public static float[] new_display_data_bottom;
        public static float[] current_display_data_bottom;

        public static float[] new_scope_data;
        public static float[] current_scope_data;

        public static float[] rx1_average_buffer;					// Averaged display data buffer
        public static float[] rx2_average_buffer;
        public static float[] rx1_peak_buffer;						// Peak hold display data buffer
        public static float[] rx2_peak_buffer;

        //waterfall
        public static float[] new_waterfall_data;
        public static float[] current_waterfall_data;
        public static float[] new_waterfall_data_bottom;
        public static float[] current_waterfall_data_bottom;
        // public static float[] waterfall_display_data;
        // public static float[] average_waterfall_buffer;
        private static int waterfall_counter;
        private static Bitmap waterfall_bmp = null;					// saved waterfall picture for display
        private static Bitmap waterfall_bmp2 = null;
        private static float[] waterfall_data;

        #endregion

        #region Properties

        public static float FrameDelta { get; private set; }

        private static bool refresh_panadapter_grid = true;                 // yt7pwr
        public static bool RefreshPanadapterGrid
        {
            set { refresh_panadapter_grid = value; }
        }

        private static bool tnf_zoom = false;
        public static bool TNFZoom
        {
            get { return tnf_zoom; }
            set
            {
                tnf_zoom = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static bool tnf_active = true;
        public static bool TNFActive
        {
            get { return tnf_active; }
            set
            {
                tnf_active = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        public static Rectangle AGCKnee = new Rectangle();
        public static Rectangle AGCHang = new Rectangle();
        public static Rectangle AGCRX2Knee = new Rectangle();
        public static Rectangle AGCRX2Hang = new Rectangle();

        //Color notch_on_color = Color.DarkGreen;
        //Color notch_highlight_color = Color.Chartreuse;
        //Color notch_perm_on_color = Color.DarkRed;
        //Color notch_perm_highlight_color = Color.DeepPink;
        private static Color notch_on_color = Color.Olive;
        private static Color notch_on_color_zoomed = Color.FromArgb(190, 128, 128, 0);
        private static Color notch_highlight_color = Color.YellowGreen;
        private static Color notch_highlight_color_zoomed = Color.FromArgb(190, 154, 205, 50);
        private static Color notch_perm_on_color = Color.DarkGreen;
        private static Color notch_perm_highlight_color = Color.Chartreuse;
        private static Color notch_off_color = Color.Gray;


        private static Color channel_background_on = Color.FromArgb(150, Color.DodgerBlue);
        private static Color channel_background_off = Color.FromArgb(100, Color.RoyalBlue);
        private static Color channel_foreground = Color.Cyan;

        private static double notch_zoom_start_freq;
        public static double NotchZoomStartFreq
        {
            get { return notch_zoom_start_freq; }
            set { notch_zoom_start_freq = value; }
        }

        private static ColorSheme color_sheme = ColorSheme.enhanced;
        public static ColorSheme ColorSheme
        {
            get { return color_sheme; }

            set { color_sheme = value; }
        }

        private static ColorSheme rx2_color_sheme = ColorSheme.enhanced;
        public static ColorSheme RX2ColorSheme
        {
            get { return rx2_color_sheme; }

            set { rx2_color_sheme = value; }
        }

        private static bool reverse_waterfall = false;
        public static bool ReverseWaterfall
        {
            get { return reverse_waterfall; }
            set { reverse_waterfall = value; }
        }

        private static bool pan_fill = true;
        public static bool PanFill
        {
            get { return pan_fill; }
            set { pan_fill = value; }
        }

        private static bool tx_pan_fill = true;
        public static bool TXPanFill
        {
            get { return tx_pan_fill; }
            set { tx_pan_fill = value; }
        }

        //private static System.Drawing.Font pan_font = new System.Drawing.Font("Arial", 9);
        //public static System.Drawing.Font PanFont
        //{
        //    get { return pan_font; }
        //    set
        //    {
        //        pan_font = value;
        //        refresh_panadapter_grid = true;
        //        if (!console.booting)
        //        {
        //            panadapter_font.Dispose();
        //            panadapter_font = new SlimDX.Direct3D9.Font(device, pan_font);
        //        }
        //    }
        //}

        //private static SlimDX.Direct3D9.Font panadapter_font = null;
        //public static SlimDX.Direct3D9.Font PanadapterFont
        //{
        //    get { return panadapter_font; }
        //    set
        //    {
        //        panadapter_font = value;
        //    }
        //}

        private static Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);
        public static Color PanFillColor
        {
            get { return pan_fill_color; }
            set { pan_fill_color = value; }
        }

        private static bool tx_on_vfob = false;
        public static bool TXOnVFOB
        {
            get { return tx_on_vfob; }
            set
            {
                tx_on_vfob = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static bool display_duplex = false;
        public static bool DisplayDuplex
        {
            get { return display_duplex; }
            set { display_duplex = value; }
        }

        private static bool split_display = false;
        public static bool SplitDisplay
        {
            get { return split_display; }
            set
            {
                split_display = value;
                refresh_panadapter_grid = true;
            }
        }

        /*private static DisplayMode current_display_mode_top = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayModeTop
        {
            get { return current_display_mode_top; }
            set 
            {
                current_display_mode_top = value;
                if(split_display) DrawBackground();
            }
        }*/

        private static DisplayMode current_display_mode_bottom = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayModeBottom
        {
            get { return current_display_mode_bottom; }
            set
            {
                current_display_mode_bottom = value;
                if (split_display) refresh_panadapter_grid = true;
            }
        }

        private static int rx1_filter_low;
        public static int RX1FilterLow
        {
            get { return rx1_filter_low; }
            set { rx1_filter_low = value; }
        }

        private static int rx1_filter_high;
        public static int RX1FilterHigh
        {
            get { return rx1_filter_high; }
            set { rx1_filter_high = value; }
        }

        private static int rx2_filter_low;
        public static int RX2FilterLow
        {
            get { return rx2_filter_low; }
            set { rx2_filter_low = value; }
        }

        private static int rx2_filter_high;
        public static int RX2FilterHigh
        {
            get { return rx2_filter_high; }
            set { rx2_filter_high = value; }
        }

        private static int tx_filter_low;
        public static int TXFilterLow
        {
            get { return tx_filter_low; }
            set { tx_filter_low = value; }
        }

        private static int tx_filter_high;
        public static int TXFilterHigh
        {
            get { return tx_filter_high; }
            set { tx_filter_high = value; }
        }

        private static bool sub_rx1_enabled = false;
        public static bool SubRX1Enabled
        {
            get { return sub_rx1_enabled; }
            set
            {
                sub_rx1_enabled = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static bool split_enabled = false;
        public static bool SplitEnabled
        {
            get { return split_enabled; }
            set
            {
                split_enabled = value;
                if (current_display_mode == DisplayMode.PANADAPTER && draw_tx_filter)
                    refresh_panadapter_grid = true;
            }
        }

        private static bool show_freq_offset = false;
        public static bool ShowFreqOffset
        {
            get { return show_freq_offset; }
            set
            {
                show_freq_offset = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static double freq;
        public static double FREQ
        {
            get { return freq; }
            set
            {
                freq = value;
            }
        }


        private static long vfoa_hz;
        public static long VFOA
        {
            get { return vfoa_hz; }
            set
            {
                vfoa_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static long vfoa_sub_hz;
        public static long VFOASub //multi-rx freq
        {
            get { return vfoa_sub_hz; }
            set
            {
                vfoa_sub_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static long vfob_hz;
        public static long VFOB //split tx freq
        {
            get { return vfob_hz; }
            set
            {
                vfob_hz = value;
                if ((current_display_mode == DisplayMode.PANADAPTER && split_enabled && draw_tx_filter) ||
                    (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled) ||
                    (current_display_mode == DisplayMode.PANADAPTER && split_display))
                    refresh_panadapter_grid = true;
            }
        }

        private static long vfob_sub_hz;
        public static long VFOBSub
        {
            get { return vfob_sub_hz; }
            set
            {
                vfob_sub_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static int rx_display_bw;
        public static int RXDisplayBW
        {
            get { return rx_display_bw; }
            set
            {
                rx_display_bw = value;
            }
        }

        private static int rit_hz;
        public static int RIT
        {
            get { return rit_hz; }
            set
            {
                rit_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static int xit_hz;
        public static int XIT
        {
            get { return xit_hz; }
            set
            {
                xit_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER && (draw_tx_filter || mox))
                    refresh_panadapter_grid = true;
            }
        }

        private static int freq_diff = 0;
        public static int FreqDiff
        {
            get { return freq_diff; }
            set
            {
                freq_diff = value;
            }
        }

        private static int rx2_freq_diff = 0;
        public static int RX2FreqDiff
        {
            get { return rx2_freq_diff; }
            set
            {
                rx2_freq_diff = value;
            }
        }

        private static int cw_pitch = 600;
        public static int CWPitch
        {
            get { return cw_pitch; }
            set { cw_pitch = value; }
        }

        //=======================================================

        private static PixelFormat WtrColor = PixelFormat.Format24bppRgb;  //          


        public static int map = 0; // ke9ns add 1=map mode (panafall but only a small waterfall) and only when just in RX1 mode)

        public static int H1 = 0;  //  ke9ns add used to fool draw routines when displaying in 3rds 
        public static int H2 = 0;  //  ke9ns add used to fool draw routines when displaying in 4ths   

        public static int K9 = 0;  // ke9ns add rx1 display mode selector:  1=water,2=pan,3=panfall, 5=panfall with RX2 on any mode, 7=special map viewing panafall
        public static int K10 = 0; // ke9ns add rx2 display mode selector: 0=off 1=water,2=pan, 5=panfall

        private static int K11 = 0; // ke9ns add set to 5 in RX1 in panfall, otherwise 0


        private static int K10LAST = 0; // ke9ns add flag to check for only changes in display mode rx2
        private static int K9LAST = 0;  // ke9ns add flag to check for only changes in display mode rx1

        private static int K13 = 0; // ke9ns add original H size before being reduced and past onto drawwaterfall to create bmp file size correctly
        public static int K14 = 0; // ke9ns add used to draw the bmp waterfall 1 time when you changed display modes.
        private static int K15 = 1; // ke9ns add used to pass the divider factor back to the init() routine to keep the bmp waterfall size correct

        private static float temp_low_threshold = 0; // ke9ns add to switch between TX and RX low level waterfall levels
        private static float temp_high_threshold = 0; // ke9ns add for TX upper level

        public static int DIS_X = 0; // ke9ns add always the size of picdisplay
        public static int DIS_Y = 0; // ke9ns add

        //========================================================

        public static bool specready = false;

        private static int H = 0;	// target height
        private static int W = 0;	// target width
        private static Control target = null;
        public static Control Target
        {
            get { return target; }
            set
            {
                target = value;
                H = target.Height;
                W = target.Width;
                Audio.ScopeDisplayWidth = W;
                if (specready)
                {
                    // if (rx1_dsp_mode == DSPMode.DRM)
                    //   console.specRX.GetSpecRX(0).Pixels = 4096;
                    // else
                    console.specRX.GetSpecRX(0).Pixels = W;
                    console.specRX.GetSpecRX(1).Pixels = W;
                    console.specRX.GetSpecRX(cmaster.inid(1, 0)).Pixels = W;
                }

            }
        }

        //private static int waterfall_H = 0;	// target height
        //private static int waterfall_W = 0;	// target width
        //private static Control waterfall_target = null;
        //public static Control WaterfallTarget
        //{
        //    get { return waterfall_target; }
        //    set
        //    {
        //        waterfall_target = value;
        //        waterfall_H = waterfall_target.Height;
        //        waterfall_W = waterfall_target.Width;
        //    }
        //}

        private static int rx_display_low = -4000;
        public static int RXDisplayLow
        {
            get { return rx_display_low; }
            set { rx_display_low = value; }
        }

        private static int rx_display_high = 4000;
        public static int RXDisplayHigh
        {
            get { return rx_display_high; }
            set { rx_display_high = value; }
        }

        private static int rx2_display_low = -4000;
        public static int RX2DisplayLow
        {
            get { return rx2_display_low; }
            set { rx2_display_low = value; }
        }

        private static int rx2_display_high = 4000;
        public static int RX2DisplayHigh
        {
            get { return rx2_display_high; }
            set { rx2_display_high = value; }
        }

        private static int tx_display_low = -4000;
        public static int TXDisplayLow
        {
            get { return tx_display_low; }
            set { tx_display_low = value; }
        }

        private static int tx_display_high = 4000;
        public static int TXDisplayHigh
        {
            get { return tx_display_high; }
            set { tx_display_high = value; }
        }

        private static int rx_spectrum_display_low = -4000;
        public static int RXSpectrumDisplayLow
        {
            get { return rx_spectrum_display_low; }
            set { rx_spectrum_display_low = value; }
        }

        private static int rx_spectrum_display_high = 4000;
        public static int RXSpectrumDisplayHigh
        {
            get { return rx_spectrum_display_high; }
            set { rx_spectrum_display_high = value; }
        }

        private static int rx2_spectrum_display_low = -4000;
        public static int RX2SpectrumDisplayLow
        {
            get { return rx2_spectrum_display_low; }
            set { rx2_spectrum_display_low = value; }
        }

        private static int rx2_spectrum_display_high = 4000;
        public static int RX2SpectrumDisplayHigh
        {
            get { return rx2_spectrum_display_high; }
            set { rx2_spectrum_display_high = value; }
        }

        private static int tx_spectrum_display_low = -4000;
        public static int TXSpectrumDisplayLow
        {
            get { return tx_spectrum_display_low; }
            set { tx_spectrum_display_low = value; }
        }

        private static int tx_spectrum_display_high = 4000;
        public static int TXSpectrumDisplayHigh
        {
            get { return tx_spectrum_display_high; }
            set { tx_spectrum_display_high = value; }
        }

        private static float rx1_preamp_offset = 0.0f;
        public static float RX1PreampOffset
        {
            get { return rx1_preamp_offset; }
            set { rx1_preamp_offset = value; }
        }

        private static float alex_preamp_offset = 0.0f;
        public static float AlexPreampOffset
        {
            get { return alex_preamp_offset; }
            set { alex_preamp_offset = value; }
        }

        private static float rx2_preamp_offset = 0.0f;
        public static float RX2PreampOffset
        {
            get { return rx2_preamp_offset; }
            set { rx2_preamp_offset = value; }
        }

        private static bool tx_display_cal_control = false;
        public static bool TXDisplayCalControl
        {
            get { return tx_display_cal_control; }
            set { tx_display_cal_control = value; }
        }

        private static float rx1_display_cal_offset;					// display calibration offset in dB
        public static float RX1DisplayCalOffset
        {
            get { return rx1_display_cal_offset; }
            set
            {
                rx1_display_cal_offset = value;
            }
        }

        private static float rx2_display_cal_offset;					// display calibration offset in dB
        public static float RX2DisplayCalOffset
        {
            get { return rx2_display_cal_offset; }
            set { rx2_display_cal_offset = value; }
        }

        private static float rx1_fft_size_offset;					// display calibration offset in dB
        public static float RX1FFTSizeOffset
        {
            get { return rx1_fft_size_offset; }
            set
            {
                rx1_fft_size_offset = value;
            }
        }

        private static float rx2_fft_size_offset;					// display calibration offset in dB
        public static float RX2FFTSizeOffset
        {
            get { return rx2_fft_size_offset; }
            set { rx2_fft_size_offset = value; }
        }


        private static float tx_display_cal_offset = 0f;					// display calibration offset in dB
        public static float TXDisplayCalOffset
        {
            get { return tx_display_cal_offset; }
            set
            {
                //if (tx_display_cal_control)
                tx_display_cal_offset = value;
                //else tx_display_cal_offset = rx1_display_cal_offset;
            }
        }

        private static HPSDRModel current_hpsdr_model = HPSDRModel.HERMES;
        public static HPSDRModel CurrentHPSDRModel
        {
            get { return current_hpsdr_model; }
            set { current_hpsdr_model = value; }
        }

        private static int display_cursor_x;						// x-coord of the cursor when over the display
        public static int DisplayCursorX
        {
            get { return display_cursor_x; }
            set { display_cursor_x = value; }
        }

        private static int display_cursor_y;						// y-coord of the cursor when over the display
        public static int DisplayCursorY
        {
            get { return display_cursor_y; }
            set { display_cursor_y = value; }
        }

        private static bool grid_control = true;
        public static bool GridControl
        {
            get { return grid_control; }
            set { grid_control = value; }
        }

        private static bool show_agc = true;
        public static bool ShowAGC
        {
            get { return show_agc; }
            set { show_agc = value; }
        }

        private static bool spectrum_line = true;
        public static bool SpectrumLine
        {
            get { return spectrum_line; }
            set { spectrum_line = value; }
        }

        private static bool display_agc_hang_line = true;
        public static bool DisplayAGCHangLine
        {
            get { return display_agc_hang_line; }
            set { display_agc_hang_line = value; }
        }

        private static bool rx1_hang_spectrum_line = true;
        public static bool RX1HangSpectrumLine
        {
            get { return rx1_hang_spectrum_line; }
            set { rx1_hang_spectrum_line = value; }
        }

        private static bool display_rx2_gain_line = true;
        public static bool DisplayRX2GainLine
        {
            get { return display_rx2_gain_line; }
            set { display_rx2_gain_line = value; }
        }

        private static bool rx2_gain_spectrum_line = true;
        public static bool RX2GainSpectrumLine
        {
            get { return rx2_gain_spectrum_line; }
            set { rx2_gain_spectrum_line = value; }
        }

        private static bool display_rx2_hang_line = true;
        public static bool DisplayRX2HangLine
        {
            get { return display_rx2_hang_line; }
            set { display_rx2_hang_line = value; }
        }

        private static bool rx2_hang_spectrum_line = true;
        public static bool RX2HangSpectrumLine
        {
            get { return rx2_hang_spectrum_line; }
            set { rx2_hang_spectrum_line = value; }
        }

        private static bool tx_grid_control = true;
        public static bool TXGridControl
        {
            get { return tx_grid_control; }
            set { tx_grid_control = value; }
        }

        private static ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;
        public static ClickTuneMode CurrentClickTuneMode
        {
            get { return current_click_tune_mode; }
            set { current_click_tune_mode = value; }
        }

        private static int scope_time = 50;
        public static int ScopeTime
        {
            get { return scope_time; }
            set { scope_time = value; }
        }

        private static int sample_rate_rx1 = 48000;
        public static int SampleRateRX1
        {
            get { return sample_rate_rx1; }
            set { sample_rate_rx1 = value; }
        }

        private static int sample_rate_rx2 = 48000;
        public static int SampleRateRX2
        {
            get { return sample_rate_rx2; }
            set { sample_rate_rx2 = value; }
        }

        private static int sample_rate_tx = 48000;
        public static int SampleRateTX
        {
            get { return sample_rate_tx; }
            set { sample_rate_tx = value; }
        }

        private static bool high_swr = false;
        public static bool HighSWR
        {
            get { return high_swr; }
            set { high_swr = value; }
        }

        private static DisplayEngine current_display_engine = DisplayEngine.GDI_PLUS;
        public static DisplayEngine CurrentDisplayEngine
        {
            get { return current_display_engine; }
            set { current_display_engine = value; }
        }

        private static bool mox = false;
        public static bool MOX
        {
            get { return mox; }
            set { mox = value; }
        }


        private static bool blank_bottom_display = false;
        public static bool BlankBottomDisplay
        {
            get { return blank_bottom_display; }
            set { blank_bottom_display = value; }
        }

        private static DSPMode rx1_dsp_mode = DSPMode.USB;
        public static DSPMode RX1DSPMode
        {
            get { return rx1_dsp_mode; }
            set { rx1_dsp_mode = value; }
        }

        private static DSPMode rx2_dsp_mode = DSPMode.USB;
        public static DSPMode RX2DSPMode
        {
            get { return rx2_dsp_mode; }
            set { rx2_dsp_mode = value; }
        }

        private static DisplayMode current_display_mode = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayMode
        {
            get { return current_display_mode; }
            set
            {
                //PrepareDisplayVars(value);

                current_display_mode = value;

                if (console.PowerOn)
                    console.pause_DisplayThread = true;

                /*switch(current_display_mode)
                {
                    case DisplayMode.PANADAPTER:
                    case DisplayMode.WATERFALL:
                        DttSP.SetPWSmode(0, 0, 1);
                        DttSP.NotPan = false;
                        break;
                    default:
                        DttSP.SetPWSmode(0, 0, 0);
                        DttSP.NotPan = true;
                        break;
                }*/

                switch (current_display_mode)
                {
                    case DisplayMode.PHASE2:
                        Audio.phase = true;
                        break;
                    case DisplayMode.SCOPE:
                    case DisplayMode.SCOPE2:
                    case DisplayMode.PANASCOPE:
                    case DisplayMode.SPECTRASCOPE:
                        cmaster.CMSetScopeRun(0, true);
                        break;
                    default:
                        Audio.phase = false;
                        cmaster.CMSetScopeRun(0, false);
                        break;
                }
                if (average_on) ResetRX1DisplayAverage();
                if (peak_on) ResetRX1DisplayPeak();

                // if (draw_display_thread != null)
                //   draw_display_thread.Abort();
                //  DirectXRelease();
                //  Target = picDisplay;
                // Display.WaterfallTarget = picWaterfall;
                // DirectXInit();

                // if (PowerOn)
                // {
                //   Thread.Sleep(100);
                //   draw_display_thread = new Thread(new ThreadStart(RunDisplay_DirectX));
                //   draw_display_thread.Name = "Draw Display Thread";
                //  draw_display_thread.Priority = ThreadPriority.Normal;
                //  draw_display_thread.IsBackground = true;
                // draw_display_thread.Start();
                //  }
                refresh_panadapter_grid = true;
                console.pause_DisplayThread = false;
            }
        }

        private static float max_x;								// x-coord of maxmimum over one display pass
        public static float MaxX
        {
            get { return max_x; }
            set { max_x = value; }
        }

        private static float max_y;								// y-coord of maxmimum over one display pass
        public static float MaxY
        {
            get { return max_y; }
            set { max_y = value; }
        }

        private static float scope_max_x;								// x-coord of maxmimum over one display pass
        public static float ScopeMaxX
        {
            get { return scope_max_x; }
            set { scope_max_x = value; }
        }

        private static float scope_max_y;								// y-coord of maxmimum over one display pass
        public static float ScopeMaxY
        {
            get { return scope_max_y; }
            set { scope_max_y = value; }
        }

        private static bool average_on;							// True if the Average button is pressed
        public static bool AverageOn
        {
            get { return average_on; }
            set
            {
                average_on = value;
                if (!average_on) ResetRX1DisplayAverage();
            }
        }

        private static bool rx2_avg_on;
        public static bool RX2AverageOn
        {
            get { return rx2_avg_on; }
            set
            {
                rx2_avg_on = value;
                if (!rx2_avg_on) ResetRX2DisplayAverage();
            }
        }

        private static bool rx2_enabled;
        public static bool RX2Enabled
        {
            get { return rx2_enabled; }
            set { rx2_enabled = value; }
        }
 
        private static bool peak_on;							// True if the Peak button is pressed
        public static bool PeakOn
        {
            get { return peak_on; }
            set
            {
                peak_on = value;
                if (!peak_on) ResetRX1DisplayPeak();
            }
        }

        private static bool rx2_peak_on;
        public static bool RX2PeakOn
        {
            get { return rx2_peak_on; }
            set
            {
                rx2_peak_on = value;
                if (!rx2_peak_on) ResetRX2DisplayPeak();
            }
        }

        private static bool data_ready;					// True when there is new display data ready from the DSP
        public static bool DataReady
        {
            get { return data_ready; }
            set { data_ready = value; }
        }

        private static bool data_ready_bottom;
        public static bool DataReadyBottom
        {
            get { return data_ready_bottom; }
            set { data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready_bottom;
        public static bool WaterfallDataReadyBottom
        {
            get { return waterfall_data_ready_bottom; }
            set { waterfall_data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready;
        public static bool WaterfallDataReady
        {
            get { return waterfall_data_ready; }
            set { waterfall_data_ready = value; }
        }

        public static float display_avg_mult_old = 1 - (float)1 / 5;
        public static float display_avg_mult_new = (float)1 / 5;
        private static int display_avg_num_blocks = 5;
        public static int DisplayAvgBlocks
        {
            get { return display_avg_num_blocks; }
            set
            {
                display_avg_num_blocks = value;
                display_avg_mult_old = 1 - (float)1 / display_avg_num_blocks;
                display_avg_mult_new = (float)1 / display_avg_num_blocks;
            }
        }

        public static float waterfall_avg_mult_old = 1 - (float)1 / 18;
        public static float waterfall_avg_mult_new = (float)1 / 18;
        private static int waterfall_avg_num_blocks = 18;
        public static int WaterfallAvgBlocks
        {
            get { return waterfall_avg_num_blocks; }
            set
            {
                waterfall_avg_num_blocks = value;
                waterfall_avg_mult_old = 1 - (float)1 / waterfall_avg_num_blocks;
                waterfall_avg_mult_new = (float)1 / waterfall_avg_num_blocks;
            }
        }

        public static float rx2_waterfall_avg_mult_old = 1 - (float)1 / 18;
        public static float rx2_waterfall_avg_mult_new = (float)1 / 18;
        private static int rx2_waterfall_avg_num_blocks = 18;
        public static int RX2WaterfallAvgBlocks
        {
            get { return rx2_waterfall_avg_num_blocks; }
            set
            {
                rx2_waterfall_avg_num_blocks = value;
                rx2_waterfall_avg_mult_old = 1 - (float)1 / rx2_waterfall_avg_num_blocks;
                rx2_waterfall_avg_mult_new = (float)1 / rx2_waterfall_avg_num_blocks;
            }
        }

        private static int spectrum_grid_max = 0;
        public static int SpectrumGridMax
        {
            get { return spectrum_grid_max; }
            set
            {
                spectrum_grid_max = value;
                // Thread.Sleep(100);
                refresh_panadapter_grid = true;
            }
        }

        private static int spectrum_grid_min = -160;
        public static int SpectrumGridMin
        {
            get { return spectrum_grid_min; }
            set
            {
                spectrum_grid_min = value;
                // Thread.Sleep(100);
                refresh_panadapter_grid = true;
            }
        }

        private static int spectrum_grid_step = 10;
        public static int SpectrumGridStep
        {
            get { return spectrum_grid_step; }
            set
            {
                spectrum_grid_step = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int rx2_spectrum_grid_max = 0;
        public static int RX2SpectrumGridMax
        {
            get { return rx2_spectrum_grid_max; }
            set
            {
                rx2_spectrum_grid_max = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int rx2_spectrum_grid_min = -160;
        public static int RX2SpectrumGridMin
        {
            get { return rx2_spectrum_grid_min; }
            set
            {
                rx2_spectrum_grid_min = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int rx2_spectrum_grid_step = 10;
        public static int RX2SpectrumGridStep
        {
            get { return rx2_spectrum_grid_step; }
            set
            {
                rx2_spectrum_grid_step = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int tx_spectrum_grid_max = 30;
        public static int TXSpectrumGridMax
        {
            get { return tx_spectrum_grid_max; }
            set
            {
                tx_spectrum_grid_max = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int tx_spectrum_grid_min = -100;
        public static int TXSpectrumGridMin
        {
            get { return tx_spectrum_grid_min; }
            set
            {
                tx_spectrum_grid_min = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int tx_spectrum_grid_step = 10;
        public static int TXSpectrumGridStep
        {
            get { return tx_spectrum_grid_step; }
            set
            {
                tx_spectrum_grid_step = value;
                refresh_panadapter_grid = true;
            }
        }

        private static Color band_edge_color = Color.Red;
        private static Pen band_edge_pen = new Pen(band_edge_color);
        public static Color BandEdgeColor
        {
            get { return band_edge_color; }
            set
            {
                band_edge_color = value;
                band_edge_pen.Color = band_edge_color;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color tx_band_edge_color = Color.Red;
        private static Pen tx_band_edge_pen = new Pen(tx_band_edge_color);
        public static Color TXBandEdgeColor
        {
            get { return tx_band_edge_color; }
            set
            {
                tx_band_edge_color = value;
                tx_band_edge_pen.Color = tx_band_edge_color;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color sub_rx_zero_line_color = Color.LightSkyBlue;
        private static Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color);
        public static Color SubRXZeroLine
        {
            get { return sub_rx_zero_line_color; }
            set
            {
                sub_rx_zero_line_color = value;
                sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
                if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color sub_rx_filter_color = Color.Blue;
        private static SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);
        public static Color SubRXFilterColor
        {
            get { return sub_rx_filter_color; }
            set
            {
                sub_rx_filter_color = value;
                sub_rx_filter_brush.Color = sub_rx_filter_color;
                if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color grid_text_color = Color.Yellow;
        private static SolidBrush grid_text_brush = new SolidBrush(grid_text_color);
        private static Pen grid_text_pen = new Pen(grid_text_color);
        public static Color GridTextColor
        {
            get { return grid_text_color; }
            set
            {
                grid_text_color = value;
                grid_text_brush.Color = grid_text_color;
                grid_text_pen.Color = grid_text_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_tx_text_color = Color.FromArgb(255, Color.Yellow);
        private static SolidBrush grid_tx_text_brush = new SolidBrush(Color.FromArgb(255, grid_tx_text_color));
        public static Color GridTXTextColor
        {
            get { return grid_tx_text_color; }
            set
            {
                grid_tx_text_color = value;
                grid_tx_text_brush.Color = grid_tx_text_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_zero_color = Color.Red;
        private static Pen grid_zero_pen = new Pen(grid_zero_color);
        public static Color GridZeroColor
        {
            get { return grid_zero_color; }
            set
            {
                grid_zero_color = value;
                grid_zero_pen.Color = grid_zero_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_grid_zero_color = Color.FromArgb(255, Color.Red);
        private static Pen tx_grid_zero_pen = new Pen(Color.FromArgb(255, tx_grid_zero_color));
        public static Color TXGridZeroColor
        {
            get { return tx_grid_zero_color; }
            set
            {
                tx_grid_zero_color = value;
                tx_grid_zero_pen.Color = tx_grid_zero_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen = new Pen(grid_color);
        public static Color GridColor
        {
            get { return grid_color; }
            set
            {
                grid_color = value;
                grid_pen.Color = grid_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_vgrid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen = new Pen(tx_vgrid_color);
        public static Color TXVGridColor
        {
            get { return tx_vgrid_color; }
            set
            {
                tx_vgrid_color = value;
                tx_vgrid_pen.Color = tx_vgrid_color;
                refresh_panadapter_grid = true;
            }
        }


        private static Color hgrid_color = Color.White;
        private static Pen hgrid_pen = new Pen(hgrid_color);
        public static Color HGridColor
        {
            get { return hgrid_color; }
            set
            {
                hgrid_color = value;
                hgrid_pen = new Pen(hgrid_color);
                refresh_panadapter_grid = true;
            }
        }


        private static Color tx_hgrid_color = Color.White;
        private static Pen tx_hgrid_pen = new Pen(tx_hgrid_color);
        public static Color TXHGridColor
        {
            get { return tx_hgrid_color; }
            set
            {
                tx_hgrid_color = value;
                tx_hgrid_pen = new Pen(tx_hgrid_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color data_line_color = Color.White;
        private static Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1.0F);
        private static Pen data_line_fpen = new Pen(Color.FromArgb(100, data_line_color));
        public static Color DataLineColor
        {
            get { return data_line_color; }
            set
            {
                data_line_color = value;
                data_line_pen.Color = data_line_color;
                data_line_fpen.Color = Color.FromArgb(100, data_line_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_data_line_color = Color.White;
        private static Pen tx_data_line_pen = new Pen(new SolidBrush(tx_data_line_color), 1.0F);
        private static Pen tx_data_line_fpen = new Pen(Color.FromArgb(100, tx_data_line_color));
        public static Color TXDataLineColor
        {
            get { return tx_data_line_color; }
            set
            {
                tx_data_line_color = value;
                tx_data_line_pen.Color = tx_data_line_color;
                tx_data_line_fpen.Color = Color.FromArgb(100, tx_data_line_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_pen_dark = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen_inb = new Pen(grid_pen_dark);
        public static Color GridPenDark
        {
            get { return grid_pen_dark; }
            set
            {
                grid_pen_dark = value;
                grid_pen_inb.Color = grid_pen_dark;
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_vgrid_pen_fine = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen_inb = new Pen(tx_vgrid_pen_fine);
        public static Color TXVGridPenFine
        {
            get { return tx_vgrid_pen_fine; }
            set
            {
                tx_vgrid_pen_fine = value;
                tx_vgrid_pen_inb.Color = tx_vgrid_pen_fine;
                refresh_panadapter_grid = true;
            }
        }

        private static Color display_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush display_filter_brush = new SolidBrush(display_filter_color);
        private static Pen cw_zero_pen = new Pen(Color.FromArgb(255, display_filter_color));
        public static Color DisplayFilterColor
        {
            get { return display_filter_color; }
            set
            {
                display_filter_color = value;
                display_filter_brush.Color = display_filter_color;
                cw_zero_pen.Color = Color.FromArgb(255, display_filter_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush tx_filter_brush = new SolidBrush(tx_filter_color);
        public static Color TXFilterColor
        {
            get { return tx_filter_color; }
            set
            {
                tx_filter_color = value;
                tx_filter_brush.Color = tx_filter_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color display_filter_tx_color = Color.Yellow;
        private static Pen tx_filter_pen = new Pen(display_filter_tx_color);
        public static Color DisplayFilterTXColor
        {
            get { return display_filter_tx_color; }
            set
            {
                display_filter_tx_color = value;
                tx_filter_pen.Color = display_filter_tx_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color display_background_color = Color.Black;
        private static SolidBrush display_background_brush = new SolidBrush(display_background_color);
        public static Color DisplayBackgroundColor
        {
            get { return display_background_color; }
            set
            {
                display_background_color = value;
                display_background_brush.Color = display_background_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color tx_display_background_color = Color.Black;
        private static SolidBrush tx_display_background_brush = new SolidBrush(tx_display_background_color);
        public static Color TXDisplayBackgroundColor
        {
            get { return tx_display_background_color; }
            set
            {
                tx_display_background_color = value;
                tx_display_background_brush.Color = tx_display_background_color;
                refresh_panadapter_grid = true;
            }
        }

        private static bool draw_tx_filter = false;
        public static bool DrawTXFilter
        {
            get { return draw_tx_filter; }
            set
            {
                draw_tx_filter = value;
                refresh_panadapter_grid = true;
            }
        }

        private static bool show_cwzero_line = false;
        public static bool ShowCWZeroLine
        {
            get { return show_cwzero_line; }
            set
            {
                show_cwzero_line = value;
            }
        }

        private static bool draw_tx_cw_freq = false;
        public static bool DrawTXCWFreq
        {
            get { return draw_tx_cw_freq; }
            set
            {
                draw_tx_cw_freq = value;
                refresh_panadapter_grid = true;
            }
        }

        private static Color waterfall_low_color = Color.Black;
        public static Color WaterfallLowColor
        {
            get { return waterfall_low_color; }
            set { waterfall_low_color = value; }
        }

        private static Color waterfall_mid_color = Color.Red;
        public static Color WaterfallMidColor
        {
            get { return waterfall_mid_color; }
            set { waterfall_mid_color = value; }
        }

        private static Color waterfall_high_color = Color.Yellow;
        public static Color WaterfallHighColor
        {
            get { return waterfall_high_color; }
            set { waterfall_high_color = value; }
        }

        private static float waterfall_high_threshold = -80.0F;
        public static float WaterfallHighThreshold
        {
            get { return waterfall_high_threshold; }
            set { waterfall_high_threshold = value; }
        }

        private static float waterfall_low_threshold = -130.0F;
        public static float WaterfallLowThreshold
        {
            get { return waterfall_low_threshold; }
            set { waterfall_low_threshold = value; }
        }

        //================================================================
        // ke9ns add signal from console about Grayscale ON/OFF
        private static byte Gray_Scale = 0; //  ke9ns ADD from console 0=RGB  1=Gray
        public static byte GrayScale       // this is called or set in console
        {
            get { return Gray_Scale; }
            set
            {
                Gray_Scale = value;
            } // set
        } // grayscale


        //================================================================
        // kes9ns add signal from setup grid lines on/off
        private static byte grid_off = 0; //  ke9ns ADD from setup 0=normal  1=gridlines off
        public static byte GridOff       // this is called or set in setup
        {
            get { return grid_off; }
            set
            {
                grid_off = value;
            } // set

        } // grid_off


        private static Color rx2_waterfall_low_color = Color.Black;
        public static Color RX2WaterfallLowColor
        {
            get { return rx2_waterfall_low_color; }
            set { rx2_waterfall_low_color = value; }
        }

        private static Color rx2_waterfall_mid_color = Color.Red;
        public static Color RX2WaterfallMidColor
        {
            get { return rx2_waterfall_mid_color; }
            set { rx2_waterfall_mid_color = value; }
        }

        private static Color rx2_waterfall_high_color = Color.Yellow;
        public static Color RX2WaterfallHighColor
        {
            get { return rx2_waterfall_high_color; }
            set { rx2_waterfall_high_color = value; }
        }

        private static float rx2_waterfall_high_threshold = -80.0F;
        public static float RX2WaterfallHighThreshold
        {
            get { return rx2_waterfall_high_threshold; }
            set { rx2_waterfall_high_threshold = value; }
        }

        private static float rx2_waterfall_low_threshold = -130.0F;
        public static float RX2WaterfallLowThreshold
        {
            get { return rx2_waterfall_low_threshold; }
            set { rx2_waterfall_low_threshold = value; }
        }

        private static float display_line_width = 1.0F;
        public static float DisplayLineWidth
        {
            get { return display_line_width; }
            set
            {
                display_line_width = value;
                data_line_pen.Width = display_line_width;
            }
        }

        private static float tx_display_line_width = 1.0F;
        public static float TXDisplayLineWidth
        {
            get { return tx_display_line_width; }
            set
            {
                tx_display_line_width = value;
                tx_data_line_pen.Width = tx_display_line_width;
            }
        }

        private static DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;
        public static DisplayLabelAlignment DisplayLabelAlign
        {
            get { return display_label_align; }
            set
            {
                display_label_align = value;
                refresh_panadapter_grid = true;
            }
        }

        private static DisplayLabelAlignment tx_display_label_align = DisplayLabelAlignment.LEFT;
        public static DisplayLabelAlignment TXDisplayLabelAlign
        {
            get { return tx_display_label_align; }
            set
            {
                tx_display_label_align = value;
                refresh_panadapter_grid = true;
            }
        }

        private static int phase_num_pts = 100;
        public static int PhaseNumPts
        {
            get { return phase_num_pts; }
            set { phase_num_pts = value; }
        }

        private static bool click_tune_filter = true;
        public static bool ClickTuneFilter
        {
            get { return click_tune_filter; }
            set { click_tune_filter = value; }
        }

        private static bool show_cth_line = false;
        public static bool ShowCTHLine
        {
            get { return show_cth_line; }
            set { show_cth_line = value; }
        }

        private static double f_center = vfoa_hz;
        public static double F_Center
        {
            get { return f_center; }
            set
            {
                // if(current_click_tune_mode == ClickTuneMode.VFOA)
                f_center = value;
                // else
                //    f_center = vfoa_hz;

            }
        }

        private static int top_size = 0;
        public static int TopSize
        {
            get { return top_size; }
            set { top_size = value; }
        }

        private static int _lin_corr = 2;
        public static int LinCor
        {
            get
            {
                return _lin_corr;
            }
            set
            {
                _lin_corr = value;
            }
        }

        private static int _linlog_corr = -14;
        public static int LinLogCor
        {
            get
            {
                return _linlog_corr;
            }
            set
            {
                _linlog_corr = value;
            }
        }

        private static SolidBrush pana_text_brush = new SolidBrush(Color.Khaki);
        private static System.Drawing.Font pana_font = new System.Drawing.Font("Tahoma", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        private static Pen dhp = new Pen(Color.FromArgb(0, 255, 0)),
                           dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255)),
                           dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0));

        private static System.Drawing.Font font14 = new System.Drawing.Font("Arial", 14, FontStyle.Bold),
                            font9 = new System.Drawing.Font("Arial", 9);


        #endregion


        // #region DirectX
        #region Properties

        // private static RenderType directx_render_type = RenderType.NONE;
        // public static RenderType DirectXRenderType
        // {
        //    get { return directx_render_type; }
        //     set { directx_render_type = value; }
        // }

        #endregion

        #region General Routines

        //private static readonly Clock clock = new Clock();
        //private static float framesPerSecond = 0.0f;
        //private static float frameAccumulator;
        //private static int frameCount;
        //private static Device device;
        //private static Device waterfall_dx_device;
        //private static Direct3D d3d;
        //private static int g_iAdapter;
        // private static bool bDeviceFound;
        //  private static CudaStopWatch stopwatch;
        //  private static CudaContext ctx;

        //        unsafe public static bool DirectXInit()
        //        {
        //            d3d = new Direct3D();
        //            if (console.booting || DX_reinit) return true;
        //            // {
        //            try
        //            {
        //                DX_reinit = true;
        //                // render_mutex.WaitOne();
        //                AdapterInformation adapterInfo = d3d.Adapters.DefaultAdapter;
        //                // if (!console.booting)

        //                switch (current_display_mode)
        //                {
        //                    case DisplayMode.PANADAPTER:
        //                    case DisplayMode.PANAFALL:
        //                    //case DisplayMode.PANAFALL_INV:
        //                    case DisplayMode.PANASCOPE:
        //                        // target = console.picDisplay;
        //                        // W = target.Width;
        //                        // H = target.Height;
        //                        // WaterfallTarget = console.picWaterfall;
        //                        // waterfallX_data = new float[waterfall_W];
        //                        // panadapterX_scope_data = new float[waterfall_W * 2];
        //                        break;

        //                    case DisplayMode.WATERFALL:
        //                        // WaterfallTarget = console.picWaterfall;
        //                        // waterfallX_data = new float[waterfall_W];
        //                        // panadapterX_scope_data = new float[waterfall_W * 2];
        //                        break;
        //                }

        //                panadapterX_data = new float[W];
        //                panadapterX_data_bottom = new float[W];
        //                waterfallX_data = new float[waterfall_W];
        //                refresh_panadapter_grid = true;
        //                W = target.Width;
        //                H = target.Height;
        //                histogram_data = new int[W];
        //                histogram_verts = new HistogramData[W * 4];
        //                histogram_history = new int[W];

        //                Parallel.For(0, W - 1, i =>
        //                {
        //                    histogram_data[i] = Int32.MaxValue;
        //                    histogram_history[i] = 0;
        //                    histogram_verts[i].X = i;
        //                    histogram_verts[i].Y = H;
        //                    histogram_verts[i].color = Color.Green;
        //                });

        //                rx1_average_buffer = new float[BUFFER_SIZE];	// initialize averaging buffer array
        //                rx1_average_buffer[0] = CLEAR_FLAG;		// set the clear flag

        //                average_waterfall_buffer = new float[BUFFER_SIZE];	// initialize averaging buffer array
        //                average_waterfall_buffer[0] = CLEAR_FLAG;		// set the clear flag

        //                rx1_peak_buffer = new float[BUFFER_SIZE];
        //                rx1_peak_buffer[0] = CLEAR_FLAG;

        //                rx2_average_buffer = new float[BUFFER_SIZE];	// initialize averaging buffer array
        //                rx2_average_buffer[0] = CLEAR_FLAG;		// set the clear flag

        //                rx2_peak_buffer = new float[BUFFER_SIZE];
        //                rx2_peak_buffer[0] = CLEAR_FLAG;

        //                new_display_data = new float[BUFFER_SIZE];
        //                current_display_data = new float[BUFFER_SIZE];
        //                new_display_data_bottom = new float[BUFFER_SIZE];
        //                current_display_data_bottom = new float[BUFFER_SIZE];
        //                new_scope_data = new float[BUFFER_SIZE];
        //                new_waterfall_data = new float[BUFFER_SIZE];
        //                current_scope_data = new float[BUFFER_SIZE];
        //                current_waterfall_data = new float[BUFFER_SIZE];
        //                waterfall_display_data = new float[BUFFER_SIZE];

        //                Parallel.For(0, BUFFER_SIZE - 1, i =>
        //            {
        //                new_display_data[i] = -200.0f;
        //                current_display_data[i] = -200.0f;
        //                new_display_data_bottom[i] = -200.0f;
        //                current_display_data_bottom[i] = -200.0f;
        //                new_scope_data[i] = -200f;
        //                new_waterfall_data[i] = -200.0f;
        //                current_scope_data[i] = -200f;
        //                current_waterfall_data[i] = -200.0f;
        //                waterfall_display_data[i] = -200.0f;
        //            });

        //                panadapterX_data = new float[W];
        //                waterfallX_data = new float[waterfall_W];

        //                switch (current_display_mode)
        //                {
        //                    case (DisplayMode.PANADAPTER):
        //                    case (DisplayMode.PANAFALL):
        //                        //target = (Control)console.picDisplay;
        //                        waterfall_target = (Control)console.picWaterfall;
        //                        break;
        //                    case (DisplayMode.WATERFALL):
        //                        waterfall_target = (Control)console.picWaterfall;
        //                        break;
        //                    default:
        //                        // target = (Control)console.picDisplay;
        //                        break;
        //                }

        //                PresentParameters pp = new PresentParameters();
        //                pp.Windowed = true;
        //                pp.SwapEffect = SwapEffect.Discard;
        //                pp.Multisample = MultisampleType.None;
        //                pp.EnableAutoDepthStencil = true;
        //                pp.AutoDepthStencilFormat = Format.D16;
        //                pp.PresentFlags = PresentFlags.DiscardDepthStencil;
        //                pp.PresentationInterval = PresentInterval.Default;
        //                pp.BackBufferFormat = Format.X8R8G8B8;
        //                pp.BackBufferHeight = target.Height;
        //                pp.BackBufferWidth = target.Width;
        //                pp.BackBufferCount = 1;

        //#if true
        //                // bDeviceFound = false;
        //                // CUdevice[] cudaDevices = null;
        //                for (g_iAdapter = 0; g_iAdapter < d3d.AdapterCount; g_iAdapter++)
        //                {
        //                    device = new Device(d3d, d3d.Adapters[g_iAdapter].Adapter,
        //                        DeviceType.Hardware,
        //                        target.Handle,
        //                        CreateFlags.FpuPreserve |
        //                        CreateFlags.HardwareVertexProcessing |
        //                      //  CreateFlags.PureDevice |
        //                        CreateFlags.Multithreaded, pp);

        //                    waterfall_dx_device = new Device(d3d, d3d.Adapters[g_iAdapter].Adapter,
        //                        DeviceType.Hardware,
        //                        waterfall_target.Handle,
        //                        CreateFlags.FpuPreserve |
        //                        CreateFlags.HardwareVertexProcessing |
        //                      //  CreateFlags.PureDevice |
        //                        CreateFlags.Multithreaded, pp);

        //                    /*  try
        //                      {
        //                          cudaDevices = CudaContext.GetDirectXDevices(device.ComPointer,
        //                               CUd3dXDeviceList.All, CudaContext.DirectXVersion.D3D9);
        //                          bDeviceFound = cudaDevices.Length > 0;
        //                          //  Console.WriteLine("> Display Device #" + d3d.Adapters[g_iAdapter].Adapter
        //                          //      + ": \"" + d3d.Adapters[g_iAdapter].Details.Description + "\" supports Direct3D9 and CUDA.");
        //                          break;
        //                      }
        //                      catch (CudaException)
        //                      {
        //                          //No Cuda device found for this Direct3D9 device
        //                          //  Console.WriteLine("> Display Device #" + d3d.Adapters[g_iAdapter].Adapter
        //                          //     + ": \"" + d3d.Adapters[g_iAdapter].Details.Description + "\" supports Direct3D9 but not CUDA.");
        //                      } */
        //                }

        //                // we check to make sure we have found a cuda-compatible D3D device to work on  
        //                /* if (!bDeviceFound)
        //                 {
        //                     // Console.WriteLine("No CUDA-compatible Direct3D9 device available");
        //                     if (device != null)
        //                         device.Dispose();
        //                     // Close();
        //                     return false;
        //                 } */
        //                // ctx = new CudaContext(cudaDevices[0], device.ComPointer, CUCtxFlags.BlockingSync, CudaContext.DirectXVersion.D3D9);

        //#else
        //                //int adapterOrdinal = d3d.Adapters.Default.Adapter;
        //                   Capabilities hardware = d3d.GetDeviceCaps(d3d.Adapters.DefaultAdapter.Adapter, DeviceType.Hardware);

        //                   if ((hardware.VertexShaderVersion >= new Version(1, 1)) &&
        //                       (hardware.PixelShaderVersion >= new Version(1, 1)))
        //                   {
        //                       //CreateFlags flags = CreateFlags.HardwareVertexProcessing;

        //                     //  if ((hardware.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
        //                       //    CreateFlags.HardwareVertexProcessing;

        //                     //  if ((hardware.DeviceCaps & DeviceCaps.PureDevice) != 0)
        //                         //  flags |= CreateFlags.PureDevice;

        //                       // }

        //                       //   try
        //                       //  {
        //                       device = new Device(d3d, 0, 
        //                           DeviceType.Hardware,
        //                           target.Handle, 
        //                           CreateFlags.FpuPreserve |
        //                           CreateFlags.HardwareVertexProcessing |
        //                           CreateFlags.Multithreaded, pp);

        //                       waterfall_dx_device = new Device(d3d, 0,
        //                         DeviceType.Hardware, 
        //                         waterfall_target.Handle,
        //                         CreateFlags.FpuPreserve |
        //                         CreateFlags.HardwareVertexProcessing |
        //                         CreateFlags.Multithreaded, pp);
        //                   }
        //                   else
        //                   {
        //                       device = new Device(d3d, 0,
        //                           DeviceType.Reference,
        //                           target.Handle, CreateFlags.SoftwareVertexProcessing |
        //                           CreateFlags.FpuPreserve, pp);
        //                   }

        //#endif

        //                //  directx_render_type = RenderType.HARDWARE;
        //                device.SetRenderState(RenderState.AlphaBlendEnable, true);
        //                device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
        //                device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.DestinationAlpha);
        //                device.SetRenderState(RenderState.Lighting, false);
        //                // device.SetRenderState(RenderState.AntialiasedLineEnable, true);


        //                var vertexElems = new[] {
        //                        new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
        //                        new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
        //                        VertexElement.VertexDeclarationEnd
        //                        };

        //                var vertexDecl = new VertexDeclaration(device, vertexElems);
        //                device.VertexDeclaration = vertexDecl;
        //                var vertexDecl1 = new VertexDeclaration(waterfall_dx_device, vertexElems);
        //                waterfall_dx_device.VertexDeclaration = vertexDecl1;

        //                waterfall_bmp = new Bitmap(waterfall_target.Width, waterfall_target.Height,
        //                   PixelFormat.Format32bppPArgb);
        //                BitmapData bitmapData = waterfall_bmp.LockBits
        //                      (new Rectangle(0, 0, waterfall_bmp.Width,
        //                      waterfall_bmp.Height), ImageLockMode.ReadWrite,
        //                      waterfall_bmp.PixelFormat);

        //                waterfall_bmp_size = bitmapData.Stride * waterfall_bmp.Height;
        //                waterfall_bmp_stride = bitmapData.Stride;
        //                waterfall_memory = new byte[waterfall_bmp_size];
        //                waterfall_bmp.UnlockBits(bitmapData);
        //                waterfall_rect = new Rectangle(0, 0, waterfall_target.Width, waterfall_target.Height);
        //                backbuf = waterfall_dx_device.GetBackBuffer(0, 0);

        //                panadapter_font = new SlimDX.Direct3D9.Font(device, pan_font);


        //                    Panadapter_Sprite = null;
        //                    WaterfallTexture = new Texture(waterfall_dx_device, waterfall_target.Width, waterfall_target.Height, 0,
        //                        Usage.None, Format.X8R8G8B8, Pool.Managed);
        //                    Waterfall_texture_size.Width = waterfall_target.Width;
        //                    Waterfall_texture_size.Height = waterfall_target.Height;
        //                    Waterfall_Sprite = new Sprite(waterfall_dx_device);


        //                if (Panadapter_Event == null)
        //                    Panadapter_Event = new AutoResetEvent(true);
        //                // if (Waterfall_Event == null)
        //                //    Waterfall_Event = new AutoResetEvent(true);

        //                WaterfallLine_vb = new VertexBuffer(waterfall_dx_device, waterfallX_data.Length * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                WaterfallLine_verts = new Vertex[waterfall_W];

        //                PanLine_vb = new VertexBuffer(device, panadapterX_data.Length * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                PanLine_vb_fill = new VertexBuffer(device, W * 2 * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                PanLine_verts = new Vertex[W];
        //                PanLine_verts_fill = new Vertex[W * 2];

        //                PanLine_bottom_vb = new VertexBuffer(device, panadapterX_data_bottom.Length * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                PanLine_bottom_vb_fill = new VertexBuffer(device, W * 2 * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                PanLine_bottom_verts = new Vertex[W];
        //                PanLine_bottom_verts_fill = new Vertex[W * 2];

        //                // if ((current_display_mode == DisplayMode.PANASCOPE) || (current_display_mode == DisplayMode.WATERFALL))
        //                //  {
        //                // ScopeLine_vb = new VertexBuffer(waterfall_dx_device, panadapterX_scope_data.Length * 20, 8, 0, 0);
        //                // ScopeLine_verts = new Vertex[waterfall_W * 2];
        //                //  }
        //                // else if ((current_display_mode == DisplayMode.PHASE) || (current_display_mode == DisplayMode.PHASE2))
        //                {
        //                    Phase_verts = new Vertex[PhaseNumPts * 2];
        //                    Phase_vb = new VertexBuffer(device, phase_num_pts * 2 * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                }
        //                //  else if (current_display_mode == DisplayMode.HISTOGRAM)
        //                {
        //                    HistogramLine_verts = new Vertex[W * 6];
        //                    Histogram_vb = new VertexBuffer(device, W * 4 * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                }

        //               // panadapter_verts = new Vector2[W];
        //               // panadapter_line = new Line(device);
        //               // panadapter_line.Antialias = true;
        //               // panadapter_line.Width = display_line_width;
        //               // panadapter_line.GLLines = true;
        //               // panadapter_fill_verts = new Vector2[W * 2];
        //               // panadapter_fill_line = new Line(device);
        //               // panadapter_fill_line.Antialias = true;
        //               // panadapter_fill_line.Width = 1.0f;
        //               // panadapter_fill_line.GLLines = true;
        //                // high_swr_font = new Font(device, new System.Drawing.Font("Arial", 14F, FontStyle.Bold));
        //                DX_reinit = false;

        //                // render_mutex.ReleaseMutex();
        //                clock.Start();

        //                return true;
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("DirectX init general fault!\n" + ex.ToString());
        //                // directx_render_type = RenderType.NONE;
        //                //render_mutex.ReleaseMutex();
        //                DX_reinit = false;
        //                return false;
        //            }
        //        }

        //        public static void DirectXRelease()
        //        {
        //            try
        //            {
        //                if (!console.booting && !DX_reinit)
        //                {
        //                    DX_reinit = true;
        //                    waterfallX_data = null;
        //                    panadapterX_data = null;
        //                    panadapterX_data_bottom = null;
        //                    // scope_min = null;
        //                    //scope_max = null;
        //                    new_display_data = null;
        //                    new_display_data_bottom = null;
        //                    backbuf = null;

        //                    new_waterfall_data = null;
        //                    current_display_data = null;
        //                    current_display_data_bottom = null;

        //                    current_waterfall_data = null;
        //                    waterfall_display_data = null;

        //                    histogram_data = null;
        //                    histogram_history = null;

        //                    rx1_average_buffer = null;
        //                    rx2_average_buffer = null;
        //                    //  average_waterfall_buffer = null;

        //                    rx1_peak_buffer = null;
        //                    rx2_peak_buffer = null;

        //                    if (waterfall_bmp != null)
        //                    {
        //                        waterfall_bmp.Dispose();
        //                    }
        //                    waterfall_bmp = null;

        //                    // Panadapter_Sprite.Dispose();
        //                    // Panadapter_Sprite = null;
        //                    // Waterfall_Sprite.Dispose();
        //                    //  Waterfall_Sprite = null;

        //                    //  if (PanadapterTexture != null)
        //                    //  {
        //                    //     PanadapterTexture.Dispose();
        //                    //     PanadapterTexture = null;
        //                    //  }

        //                    // if (WaterfallTexture != null)
        //                    // {
        //                    //     WaterfallTexture.Dispose();
        //                    //      WaterfallTexture = null;
        //                    // }

        //                    // if (Panadapter_Event != null)
        //                    // {
        //                    //    Panadapter_Event.Close();
        //                    //    Panadapter_Event = null;
        //                    //  }

        //                    //  if (Waterfall_Event != null)
        //                    //  {
        //                    //     Waterfall_Event.Close();
        //                    //     Waterfall_Event = null;
        //                    // }

        //                    if (VerLines_vb != null)
        //                    {
        //                        VerLines_vb.Dispose();
        //                       // VerLines_vb = null;
        //                    }

        //                    if (VerLines_bottom_vb != null)
        //                    {
        //                        VerLines_bottom_vb.Dispose();
        //                      //  VerLines_bottom_vb = null;
        //                    }

        //                    if (HorLines_vb != null)
        //                    {
        //                        HorLines_vb.Dispose();
        //                       // HorLines_vb = null;
        //                    }

        //                    if (HorLines_bottom_vb != null)
        //                    {
        //                        HorLines_bottom_vb.Dispose();
        //                       // HorLines_bottom_vb = null;
        //                    }

        //                    if (PanLine_vb != null)
        //                    {
        //                        PanLine_vb.Dispose();
        //                        PanLine_vb.Dispose();
        //                    }

        //                    if (PanLine_bottom_vb != null)
        //                    {
        //                        PanLine_bottom_vb.Dispose();
        //                        PanLine_bottom_vb.Dispose();
        //                    }

        //                    if (PanLine_vb_fill != null)
        //                    {
        //                        PanLine_vb_fill.Dispose();
        //                        PanLine_vb_fill.Dispose();
        //                    }

        //                    if (PanLine_bottom_vb_fill != null)
        //                    {
        //                        PanLine_bottom_vb_fill.Dispose();
        //                        PanLine_bottom_vb_fill.Dispose();
        //                    }

        //                    if (vertical_label != null)
        //                        vertical_label = null;

        //                    if (vertical_bottom_label != null)
        //                        vertical_bottom_label = null;

        //                    if (horizontal_label != null)
        //                        horizontal_label = null;

        //                    if (horizontal_bottom_label != null)
        //                        horizontal_bottom_label = null;

        //                    if (Phase_vb != null)
        //                    {
        //                        Phase_vb.Dispose();
        //                        // Phase_vb.Dispose();
        //                    }

        //                    if (device != null)
        //                    {
        //                        device.Dispose();
        //                       // device = null;
        //                    }

        //                   //  if (d3d != null) d3d.Dispose();
        //                    // if (ctx != null) ctx.Dispose();

        //                    if (waterfall_dx_device != null)
        //                    {
        //                        waterfall_dx_device.Dispose();
        //                       // waterfall_dx_device = null;
        //                    }

        //                   // high_swr_font.Dispose();
        //                   // panadapter_fill_verts = null;
        //                  //  panadapter_line = null;
        //                   // panadapter_fill_verts = null;
        //                  //  panadapter_verts = null;

        //                    //render_mutex.ReleaseMutex();

        //                    foreach (var item in ObjectTable.Objects)
        //                        item.Dispose();

        //                    DX_reinit = false;
        //                }
        //            }

        //            catch (Exception ex)
        //            {
        //                DX_reinit = false;
        //                //render_mutex.ReleaseMutex();
        //                Debug.Write("DX release error!" + ex.ToString());
        //            }
        //        }

        //        public static void RenderVFO(int W, int H, int rx, bool bottom)
        //        {
        //            bool local_mox = false;
        //            if (mox && rx == 1 && !tx_on_vfob) local_mox = true;
        //            if (mox && rx == 2 && tx_on_vfob) local_mox = true;
        //            int low = 0;
        //            int high = 0;
        //            int filter_low, filter_high;
        //            int[] step_list = { 10, 20, 25, 50 };
        //           // int step_power = 1;
        //           // int step_index = 0;
        //           // int freq_step_size = 50;
        //            int center_line_x = 0;
        //            // top_size = (int)((double)grid_step * H / y_range);

        //            if (local_mox) // get filter limits
        //            {
        //                low = tx_display_low;
        //                high = tx_display_high;
        //                filter_low = tx_filter_low;
        //                filter_high = tx_filter_high;
        //            }
        //            else if (rx == 2)
        //            {
        //                low = rx2_display_low;
        //                high = rx2_display_high;
        //                filter_low = rx2_filter_low;
        //                filter_high = rx2_filter_high;
        //            }
        //            else //if(rx == 1)
        //            {
        //                low = rx_display_low;
        //                high = rx_display_high;
        //                filter_low = rx1_filter_low;
        //                filter_high = rx1_filter_high;
        //            }

        //            if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
        //               (rx2_dsp_mode == DSPMode.DRM && rx == 2))
        //            {
        //                filter_low = -5000;
        //                filter_high = 5000;
        //            }

        //            center_line_x = (int)(-(double)low / (high - low) * W);

        //            // Calculate horizontal step size
        //          //  int width = high - low;
        //           // while (width / freq_step_size > 10)
        //           // {
        //            //    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //             //   step_index = (step_index + 1) % 4;
        //             //   if (step_index == 0) step_power++;
        //          //  }
        //           // double w_pixel_step = (double)W * freq_step_size / width;
        //           // int w_steps = width / freq_step_size;

        //            // calculate vertical step size
        //            int h_steps = (spectrum_grid_max - spectrum_grid_min) / spectrum_grid_step;
        //            double h_pixel_step = (double)H / h_steps;

        //            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
        //            {
        //                // draw Sub RX filter
        //                // get filter screen coordinates
        //                int filter_left = (int)((float)(filter_low - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                int filter_right = (int)((float)(filter_high - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);

        //                // make the filter display at least one pixel wide.
        //                if (filter_left == filter_right) filter_right = filter_left + 1;
        //                // draw rx filter
        //                if (bottom)
        //                {
        //                    VFOBrect.x1 = filter_left; // p0x:[left]
        //                    VFOBrect.y1 = H + H; // p0y[bottom]
        //                    VFOBrect.x2 = filter_left; // p1x[left]
        //                    VFOBrect.y2 = H + top_size; // (int)(pan_font.Size); // p1y[top]
        //                    VFOBrect.x3 = filter_right; // p2x[right]
        //                    VFOBrect.y3 = H + H; // p2y[bottom]
        //                    VFOBrect.x4 = filter_right; // p3x[right]
        //                    VFOBrect.y4 = H + top_size; // (int)(pan_font.Size);  // p3y[top]
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //                else
        //                {
        //                    VFOBrect.x1 = filter_left; // p0x:[left]
        //                    VFOBrect.y1 = H; // p0y[bottom]
        //                    VFOBrect.x2 = filter_left; // p1x[left]
        //                    VFOBrect.y2 = top_size; // (int)(pan_font.Size); // p1y[top]
        //                    VFOBrect.x3 = filter_right; // p2x[right]
        //                    VFOBrect.y3 = H; // p2y[bottom]
        //                    VFOBrect.x4 = filter_right; // p3x[right]
        //                    VFOBrect.y4 = top_size; // (int)(pan_font.Size);  // p3y[top]
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //                // draw Sub RX 0Hz line
        //                int x = (int)((float)(vfoa_sub_hz - vfoa_hz - low) / (high - low) * W);
        //                if (bottom)
        //                {
        //                    RenderVerticalLine(device, x, H, sub_rx_filter_color);
        //                }
        //                else
        //                {
        //                    RenderVerticalLine(device, x, H, sub_rx_filter_color);
        //                }
        //            }

        //            if (!(local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)))
        //            {
        //                // get filter screen coordinates
        //                int filter_left = (int)((float)(filter_low - low) / (high - low) * W);
        //                int filter_right = (int)((float)(filter_high - low) / (high - low) * W);

        //                // make the filter display at least one pixel wide.
        //                if (filter_left == filter_right) filter_right = filter_left + 1;
        //                if (bottom)
        //                {
        //                    VFOArect.x1 = filter_left; // p0x:[left]
        //                    VFOArect.y1 = H + H;// p0y[bottom]
        //                    VFOArect.x2 = filter_left; // p1x[left]
        //                    VFOArect.y2 = H + top_size; // (int)(pan_font.Size); // p1y[top]
        //                    VFOArect.x3 = filter_right; // p2x[right]
        //                    VFOArect.y3 = H + H; // p2y[bottom]
        //                    VFOArect.x4 = filter_right; // p3x[right]
        //                    VFOArect.y4 = H + top_size; // (int)(pan_font.Size);  // p3y[top]
        //                    RenderRectangle(device, VFOArect, display_filter_color);
        //                }
        //                else
        //                {
        //                    VFOArect.x1 = filter_left; // p0x:[left]
        //                    VFOArect.y1 = H;// p0y[bottom]
        //                    VFOArect.x2 = filter_left; // p1x[left]
        //                    VFOArect.y2 = top_size; // (int)(pan_font.Size); // p1y[top]
        //                    VFOArect.x3 = filter_right; // p2x[right]
        //                    VFOArect.y3 = H; // p2y[bottom]
        //                    VFOArect.x4 = filter_right; // p3x[right]
        //                    VFOArect.y4 = top_size; // (int)(pan_font.Size);  // p3y[top]
        //                    RenderRectangle(device, VFOArect, display_filter_color);
        //                }
        //            }

        //            if (!local_mox && draw_tx_filter &&
        //                (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
        //            {
        //                // get tx filter limits
        //                int filter_left_x;
        //                int filter_right_x;

        //                if (tx_on_vfob)
        //                {
        //                    if (!split_enabled)
        //                    {
        //                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz - rit_hz) / (high - low) * W);
        //                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz - rit_hz) / (high - low) * W);
        //                    }
        //                    else
        //                    {
        //                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz - rit_hz + (vfob_sub_hz - vfoa_hz)) / (high - low) * W);
        //                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz - rit_hz + (vfob_sub_hz - vfoa_hz)) / (high - low) * W);
        //                    }
        //                }
        //                else
        //                {
        //                    if (!split_enabled)
        //                    {
        //                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz - rit_hz) / (high - low) * W);
        //                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz - rit_hz) / (high - low) * W);
        //                    }
        //                    else
        //                    {
        //                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);
        //                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);
        //                    }
        //                }

        //                if (bottom && tx_on_vfob)
        //                {
        //                    RenderVerticalLine(device, filter_left_x, H + top_size, filter_left_x, H + H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_left_x + 1, H + top_size, filter_left_x + 1, H + H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_right_x, H + top_size, filter_right_x, H + H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_right_x + 1, H + top_size, filter_right_x + 1, H + H, tx_filter_color);
        //                }
        //                else if (!tx_on_vfob)
        //                {
        //                    RenderVerticalLine(device, filter_left_x, top_size, filter_left_x, H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_left_x + 1, top_size, filter_left_x + 1, H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_right_x, top_size, filter_right_x, H, tx_filter_color);
        //                    RenderVerticalLine(device, filter_right_x + 1, top_size, filter_right_x + 1, H, tx_filter_color);
        //                }
        //            }

        //            if (!local_mox && draw_tx_cw_freq &&
        //                (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //            {
        //                int pitch = cw_pitch;
        //                if (rx1_dsp_mode == DSPMode.CWL)
        //                    pitch = -cw_pitch;

        //                int cw_line_x;
        //                if (!split_enabled)
        //                    cw_line_x = (int)((float)(pitch - low + xit_hz - rit_hz) / (high - low) * W);
        //                else
        //                    cw_line_x = (int)((float)(pitch - low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);

        //                if (bottom)
        //                {
        //                    // g.DrawLine(tx_filter_pen, cw_line_x, H + top, cw_line_x, H + H);
        //                    //  RenderVerticalLine(device, cw_line_x, H + top_size, cw_line_x, H + H, tx_filter_color);
        //                    //g.DrawLine(tx_filter_pen, cw_line_x + 1, H + top, cw_line_x + 1, H + H);
        //                    //  RenderVerticalLine(device, cw_line_x + 1, H + top_size, cw_line_x + 1, H + H, tx_filter_color);
        //                }
        //                else
        //                {
        //                    RenderVerticalLine(device, cw_line_x, top_size, cw_line_x, H, tx_filter_color);
        //                    RenderVerticalLine(device, cw_line_x + 1, top_size, cw_line_x + 1, H, tx_filter_color);
        //                }
        //            }

        //            /*   else //tx
        //               {
        //                   if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //                   {
        //                       // get filter screen coordinates
        //                       filter_left = (int)((float)(-low - ((filter_high - filter_low) / 2) + vfoa_hz) / (high - low) * W);
        //                       filter_right = (int)((float)(-low + ((filter_high - filter_low) / 2) + vfoa_hz) / (high - low) * W);

        //                       // make the filter display at least one pixel wide.
        //                       if (filter_left == filter_right) filter_right = filter_left + 1;
        //                       VFOArect.x1 = filter_left; // p0x:[left]
        //                       VFOArect.y1 = H;// p0y[bottom]
        //                       VFOArect.x2 = filter_left; // p1x[left]
        //                       VFOArect.y2 = top_size; // (int)(pan_font.Size); // p1y[top]
        //                       VFOArect.x3 = filter_right; // p2x[right]
        //                       VFOArect.y3 = H; // p2y[bottom]
        //                       VFOArect.x4 = filter_right; // p3x[right]
        //                       VFOArect.y4 = top_size; // (int)(pan_font.Size);  // p3y[top]
        //                       RenderRectangle(device, VFOArect, display_filter_color);
        //                   }
        //                   else
        //                   {
        //                       if (!split_enabled)
        //                       {
        //                           // get filter screen coordinates
        //                           filter_left = (int)((float)(filter_low - low) / (high - low) * W);
        //                           filter_right = (int)((float)(filter_high - low) / (high - low) * W);

        //                           // make the filter display at least one pixel wide.
        //                           if (filter_left == filter_right) filter_right = filter_left + 1;

        //                           // draw Main TX 0Hz line
        //                           int x = (int)((float)(-low) / (high - low) * W);
        //                           RenderVerticalLine(device, x, H, grid_zero_color);
        //                           // RenderVerticalLine(device, x+1, H, grid_zero_color);

        //                           VFOArect.x1 = filter_left; // p0x:[left]
        //                           VFOArect.y1 = H;// p0y[bottom]
        //                           VFOArect.x2 = filter_left; // p1x[left]
        //                           VFOArect.y2 = top_size; // (int)(pan_font.Size); // p1y[top]
        //                           VFOArect.x3 = filter_right; // p2x[right]
        //                           VFOArect.y3 = H; // p2y[bottom]
        //                           VFOArect.x4 = filter_right; // p3x[right]
        //                           VFOArect.y4 = top_size; // (int)(pan_font.Size);  // p3y[top]
        //                           RenderRectangle(device, VFOArect, display_filter_color);
        //                       }
        //                   }
        //               }*/

        //            // Draw a Zero Beat line on CW filter
        //            if (!local_mox && show_cwzero_line &&
        //                (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //            {
        //                int pitch = cw_pitch;
        //                if (rx1_dsp_mode == DSPMode.CWL)
        //                    pitch = -cw_pitch;

        //                int cw_line_x1;
        //                if (!split_enabled)
        //                    cw_line_x1 = (int)((float)(pitch - low) / (high - low) * W);
        //                else
        //                    cw_line_x1 = (int)((float)(pitch - low + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);

        //                if (bottom)
        //                {
        //                    // g.DrawLine(tx_filter_pen, cw_line_x1, H + top, cw_line_x1, H + H);
        //                    // RenderVerticalLine(device, cw_line_x1, H + top_size, cw_line_x1, H + H, tx_filter_color);
        //                    // g.DrawLine(tx_filter_pen, cw_line_x1 + 1, H + top, cw_line_x1 + 1, H + H);
        //                    // RenderVerticalLine(device, cw_line_x1 + 1, H + top_size, cw_line_x1 + 1, H + H, tx_filter_color);
        //                }
        //                else
        //                {
        //                    // g.DrawLine(cw_zero_pen, cw_line_x1, top, cw_line_x1, H);
        //                    RenderVerticalLine(device, cw_line_x1, top_size, cw_line_x1, H, tx_filter_color);
        //                    //  g.DrawLine(tx_filter_pen, cw_line_x1 + 1, top, cw_line_x1 + 1, H);
        //                    RenderVerticalLine(device, cw_line_x1 + 1, top_size, cw_line_x1 + 1, H, tx_filter_color);
        //                }
        //            }

        //            if (!local_mox && show_cwzero_line &&
        //                (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
        //            {
        //                int pitch = cw_pitch;
        //                if (rx1_dsp_mode == DSPMode.CWL)
        //                    pitch = -cw_pitch;

        //                int cw_line_x1;
        //                if (!split_enabled)
        //                    cw_line_x1 = (int)((float)(pitch - low) / (high - low) * W);
        //                else
        //                    cw_line_x1 = (int)((float)(pitch - low + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);

        //                if (bottom)
        //                {
        //                    // g.DrawLine(tx_filter_pen, cw_line_x1, H + top, cw_line_x1, H + H);
        //                    // RenderVerticalLine(device, cw_line_x1, H + top_size, cw_line_x1, H + H, tx_filter_color);
        //                    // g.DrawLine(tx_filter_pen, cw_line_x1 + 1, H + top, cw_line_x1 + 1, H + H);
        //                    // RenderVerticalLine(device, cw_line_x1 + 1, H + top_size, cw_line_x1 + 1, H + H, tx_filter_color);
        //                }
        //                else
        //                {
        //                    // g.DrawLine(cw_zero_pen, cw_line_x1, top, cw_line_x1, H);
        //                    // RenderVerticalLine(device, cw_line_x1, top_size, cw_line_x1, H, tx_filter_color);
        //                    //  g.DrawLine(tx_filter_pen, cw_line_x1 + 1, top, cw_line_x1 + 1, H);
        //                    //  RenderVerticalLine(device, cw_line_x1 + 1, top_size, cw_line_x1 + 1, H, tx_filter_color);
        //                }
        //            }

        //            // draw Main RX 0Hz line
        //            if (center_line_x >= 0 && center_line_x <= W)
        //            {
        //                if (bottom)
        //                {
        //                    RenderVerticalLine(device, center_line_x, H + top_size, center_line_x, H + H, grid_zero_color);
        //                }
        //                else
        //                {
        //                    RenderVerticalLine(device, center_line_x, H, grid_zero_color);
        //                }
        //            }

        //            // draw long cursor & filter overlay
        //            if (current_click_tune_mode != ClickTuneMode.Off)
        //            {
        //                if (bottom)
        //                {
        //                    if (ClickTuneFilter)
        //                    {
        //                        if (display_cursor_y > H)
        //                        {
        //                            double freq_low = freq + filter_low;
        //                            double freq_high = freq + filter_high;
        //                            int filter_left = 0;
        //                            int filter_right = 0;

        //                            if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //                            {
        //                                int cw_filter_left = (int)((freq_low - low) / (high - low) * W); //x1
        //                                int cw_filter_right = (int)((freq_high - low) / (high - low) * W); //x2
        //                                filter_left = display_cursor_x - ((cw_filter_right - cw_filter_left) / 2);
        //                                filter_right = (filter_left + (cw_filter_right - cw_filter_left));
        //                            }
        //                            else
        //                            {
        //                                filter_left = (int)((freq_low - low) / (high - low) * W); //x1
        //                                filter_right = (int)((freq_high - low) / (high - low) * W); //x2
        //                            }

        //                            // make the filter display at least one pixel wide.
        //                            if (filter_left == filter_right) filter_right = filter_left + 1;

        //                            VFOATunerect_bottom.x1 = filter_left; // p0x:[left]
        //                            VFOATunerect_bottom.y1 = H + H;// p0y[bottom]
        //                            VFOATunerect_bottom.x2 = filter_left; // p1x[left]
        //                            VFOATunerect_bottom.y2 = H + top_size; // (int)(pan_font.Size); // p1y[top]
        //                            VFOATunerect_bottom.x3 = filter_right; // p2x[right]
        //                            VFOATunerect_bottom.y3 = H + H; // p2y[bottom]
        //                            VFOATunerect_bottom.x4 = filter_right; // p3x[right]
        //                            VFOATunerect_bottom.y4 = H + top_size; // (int)(pan_font.Size);  // p3y[top]
        //                            RenderRectangle(device, VFOATunerect_bottom, display_filter_color);

        //                            if (current_click_tune_mode == ClickTuneMode.VFOA)
        //                            {
        //                                RenderVerticalLine(device, display_cursor_x, top_size + H, display_cursor_x, H + H, grid_text_color);
        //                                if (ShowCTHLine)
        //                                    RenderHorizontalLine(device, 0, display_cursor_y, grid_text_color);
        //                            }
        //                            else if (current_click_tune_mode == ClickTuneMode.VFOB)
        //                            {
        //                                RenderVerticalLine(device, display_cursor_x, H, Color.Red);
        //                                if (ShowCTHLine)
        //                                    RenderHorizontalLine(device, 0, display_cursor_y, grid_text_color);
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (ClickTuneFilter)
        //                    {
        //                        if (display_cursor_y <= H)
        //                        {
        //                            double freq_low = freq + filter_low;
        //                            double freq_high = freq + filter_high;
        //                            int filter_left = 0;
        //                            int filter_right = 0;

        //                            if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //                            {
        //                                int cw_filter_left = (int)((freq_low - low) / (high - low) * W); //x1
        //                                int cw_filter_right = (int)((freq_high - low) / (high - low) * W); //x2
        //                                filter_left = display_cursor_x - ((cw_filter_right - cw_filter_left) / 2);
        //                                filter_right = (filter_left + (cw_filter_right - cw_filter_left));
        //                            }
        //                            else
        //                            {
        //                                filter_left = (int)((freq_low - low) / (high - low) * W); //x1
        //                                filter_right = (int)((freq_high - low) / (high - low) * W); //x2
        //                            }

        //                            // make the filter display at least one pixel wide.
        //                            if (filter_left == filter_right) filter_right = filter_left + 1;

        //                            VFOATunerect.x1 = filter_left; // p0x:[left]
        //                            VFOATunerect.y1 = H;// p0y[bottom]
        //                            VFOATunerect.x2 = filter_left; // p1x[left]
        //                            VFOATunerect.y2 = top_size; // (int)(pan_font.Size); // p1y[top]
        //                            VFOATunerect.x3 = filter_right; // p2x[right]
        //                            VFOATunerect.y3 = H; // p2y[bottom]
        //                            VFOATunerect.x4 = filter_right; // p3x[right]
        //                            VFOATunerect.y4 = top_size; // (int)(pan_font.Size);  // p3y[top]
        //                            RenderRectangle(device, VFOATunerect, display_filter_color);

        //                            if (current_click_tune_mode == ClickTuneMode.VFOA)
        //                            {
        //                                RenderVerticalLine(device, display_cursor_x, top_size, display_cursor_x, H, grid_text_color);
        //                                if (ShowCTHLine)
        //                                    RenderHorizontalLine(device, 0, display_cursor_y, grid_text_color);
        //                            }
        //                            else if (current_click_tune_mode == ClickTuneMode.VFOB)
        //                            {
        //                                RenderVerticalLine(device, display_cursor_x, H, Color.Red);
        //                                if (ShowCTHLine)
        //                                    RenderHorizontalLine(device, 0, display_cursor_y, grid_text_color);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        public static void Render_VFOB()  // yt7pwr
        //        {
        //            int low = rx2_display_low;					// initialize variables
        //            int high = rx2_display_high;
        //            int filter_low, filter_high;
        //            int[] step_list = { 10, 20, 25, 50 };
        //            int step_power = 1;
        //            int step_index = 0;
        //            int freq_step_size = 50;
        //            int filter_right = 0;
        //            int filter_left = 0;
        //            // int filter_low_subRX = 0;
        //            // int filter_high_subRX = 0;
        //            // int grid_step = spectrum_grid_step;
        //            // if (split_display) grid_step *= 2;
        //            // int y_range = spectrum_grid_max - spectrum_grid_min;

        //            // int top = (int)((double)grid_step * H / y_range);

        //            if (mox && !(rx1_dsp_mode == DSPMode.CWL ||
        //                 rx1_dsp_mode == DSPMode.CWU)) // get filter limits
        //            {
        //                filter_low = tx_filter_low;
        //                filter_high = tx_filter_high;
        //            }
        //            else
        //            {
        //                filter_low = rx2_filter_low;
        //                filter_high = rx2_filter_high;
        //                //  filter_low_subRX = DttSP.RXFilterLowCutSubRX;
        //                // filter_high_subRX = DttSP.RXFilterHighCutSubRX;
        //            }

        //            // Calculate horizontal step size
        //            int width = high - low;
        //            while (width / freq_step_size > 10)
        //            {
        //                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //                step_index = (step_index + 1) % 4;
        //                if (step_index == 0) step_power++;
        //            }

        //            int w_steps = width / freq_step_size;

        //            // calculate vertical step size
        //            int h_steps = (spectrum_grid_max - spectrum_grid_min) / spectrum_grid_step;
        //            double h_pixel_step = (double)H / h_steps;

        //            if (sub_rx1_enabled && !mox)
        //            {
        //                if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //                {
        //                    // draw Sub RX filter
        //                    // get filter screen coordinates
        //                    // filter_left = (int)((float)(-low - ((filter_high_subRX - filter_low_subRX) / 2)
        //                    // + vfob_hz - losc_hz) / (high - low) * panadapter_W);
        //                    // filter_right = (int)((float)(-low + ((filter_high_subRX - filter_low_subRX) / 2)
        //                    // + vfob_hz - losc_hz) / (high - low) * panadapter_W);
        //                    filter_left = (int)((float)(filter_low - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                    filter_right = (int)((float)(filter_high - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);

        //                    // make the filter display at least one pixel wide.
        //                    if (filter_left == filter_right) filter_right = filter_left + 1;
        //                    VFOBrect.x1 = filter_right;
        //                    VFOBrect.y1 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x2 = filter_right;
        //                    VFOBrect.y2 = H;
        //                    VFOBrect.x3 = filter_left;
        //                    VFOBrect.y3 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x4 = filter_left;
        //                    VFOBrect.y4 = H;
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //                else
        //                {
        //                    // draw Sub RX filter
        //                    // get filter screen coordinates
        //                    //filter_left = (int)((float)(filter_low_subRX - low + vfob_hz - losc_hz) / (high - low) * panadapter_W);
        //                    // filter_right = (int)((float)(filter_high_subRX - low + vfob_hz - losc_hz) / (high - low) * panadapter_W);

        //                    filter_left = (int)((float)(filter_low - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                    filter_right = (int)((float)(filter_high - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                    // make the filter display at least one pixel wide.
        //                    if (filter_left == filter_right) filter_right = filter_left + 1;

        //                    // draw Sub RX 0Hz line
        //                    // int sub_rx_zero_line = (int)((float)(vfob_hz - losc_hz - low) / (high - low) * panadapter_W);
        //                    int sub_rx_zero_line = (int)((float)(vfoa_sub_hz - vfoa_hz - low) / (high - low) * W);
        //                    RenderVerticalLine(device, sub_rx_zero_line, H, sub_rx_zero_line_color);
        //                    VFOBrect.x1 = filter_right;
        //                    VFOBrect.y1 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x2 = filter_right;
        //                    VFOBrect.y2 = H;
        //                    VFOBrect.x3 = filter_left;
        //                    VFOBrect.y3 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x4 = filter_left;
        //                    VFOBrect.y4 = H;
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //            }

        //            if (split_enabled && mox)
        //            {
        //                if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //                {
        //                    // draw Sub RX filter
        //                    // get filter screen coordinates
        //                    filter_left = (int)((float)(filter_low - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                    filter_right = (int)((float)(filter_high - low + vfoa_sub_hz - vfoa_hz - rit_hz) / (high - low) * W);
        //                    // make the filter display at least one pixel wide.
        //                    if (filter_left == filter_right) filter_right = filter_left + 1;

        //                    VFOBrect.x1 = filter_right;
        //                    VFOBrect.y1 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x2 = filter_right;
        //                    VFOBrect.y2 = H;
        //                    VFOBrect.x3 = filter_left;
        //                    VFOBrect.y3 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x4 = filter_left;
        //                    VFOBrect.y4 = H;
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //                else
        //                {
        //                    // get filter screen coordinates
        //                    filter_left = (int)((float)(filter_low - low) / (high - low) * W);
        //                    filter_right = (int)((float)(filter_high - low) / (high - low) * W);

        //                    int x = (int)((float)(-low) / (high - low) * W);
        //                    RenderVerticalLine(device, x, H, sub_rx_zero_line_color);
        //                    VFOBrect.x1 = filter_right;
        //                    VFOBrect.y1 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x2 = filter_right;
        //                    VFOBrect.y2 = H;
        //                    VFOBrect.x3 = filter_left;
        //                    VFOBrect.y3 = top_size; // (int)(pan_font.Size);
        //                    VFOBrect.x4 = filter_left;
        //                    VFOBrect.y4 = H;
        //                    RenderRectangle(device, VFOBrect, sub_rx_filter_color);
        //                }
        //            }
        //        }

        //        private static void RenderRectangle(Device dev, DXRectangle rect, Color color)
        //        {
        //            Vertex[] verts = new Vertex[4];
        //            var vb = new VertexBuffer(dev, 4 * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

        //            verts[0] = new Vertex();
        //            verts[0].Color = color.ToArgb();
        //            verts[0].Position = new Vector4(rect.x1, rect.y1, 0.0f, 0.0f);
        //            verts[1] = new Vertex();
        //            verts[1].Color = color.ToArgb();
        //            verts[1].Position = new Vector4(rect.x2, rect.y2, 0.0f, 0.0f);
        //            verts[2] = new Vertex();
        //            verts[2].Color = color.ToArgb();
        //            verts[2].Position = new Vector4(rect.x3, rect.y3, 0.0f, 0.0f);
        //            verts[3] = new Vertex();
        //            verts[3].Color = color.ToArgb();
        //            verts[3].Position = new Vector4(rect.x4, rect.y4, 0.0f, 0.0f);

        //            vb.Lock(0, 0, LockFlags.None).WriteRange(verts, 0, 4);
        //            vb.Unlock();
        //            device.SetStreamSource(0, vb, 0, 20);
        //            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        //            vb.Dispose();
        //        }

        //        private static void RenderVerticalLines(Device dev, VertexBuffer vertex, int count)         // yt7pwr
        //        {
        //            dev.SetStreamSource(0, vertex, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, count);
        //        }

        //        private static void RenderHorizontalLines(Device dev, VertexBuffer vertex, int count)        // yt7pwr
        //        {
        //            dev.SetStreamSource(0, vertex, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, count);
        //        }

        //        private static void RenderVerticalLine(Device dev, int x, int y, Color color)                // yt7pwr
        //        {
        //            var vb = new VertexBuffer(dev, 2 * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

        //            vb.Lock(0, 0, LockFlags.None).WriteRange(new[] {
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x, (float)(top_size), 0.0f, 0.0f) },
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x, (float)y, 0.0f, 0.0f) }
        //                 });
        //            vb.Unlock();

        //            dev.SetStreamSource(0, vb, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, 1);

        //            vb.Dispose();
        //        }

        //        private static void RenderVerticalLine(Device dev, int x1, int y1, int x2, int y2, Color color)
        //        {
        //            var vb = new VertexBuffer(dev, 2 * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

        //            vb.Lock(0, 0, LockFlags.None).WriteRange(new[] {
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x1, (float)y1, 0.0f, 0.0f) },
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x2, (float)y2, 0.0f, 0.0f) }
        //                 });
        //            vb.Unlock();

        //            dev.SetStreamSource(0, vb, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, 1);

        //            vb.Dispose();
        //        }

        //        private static void RenderHorizontalLine(Device dev, int x, int y, Color color)              // yt7pwr
        //        {
        //            var vb = new VertexBuffer(dev, 2 * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

        //            vb.Lock(0, 0, LockFlags.None).WriteRange(new[] {
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x, (float)y, 0.0f, 0.0f) },
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)W, (float)y, 0.0f, 0.0f) }
        //                 });
        //            vb.Unlock();

        //            dev.SetStreamSource(0, vb, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, 1);

        //            vb.Dispose();
        //        }

        //        private static void RenderHorizontalLine(Device dev, int x1, int y1, int x2, int y2, Color color)              // yt7pwr
        //        {
        //            var vb = new VertexBuffer(dev, 2 * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

        //            vb.Lock(0, 0, LockFlags.None).WriteRange(new[] {
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x1, (float)y1, 0.0f, 0.0f) },
        //                new Vertex() { Color = color.ToArgb(), Position = new Vector4((float)x2, (float)y2, 0.0f, 0.0f) }
        //                 });
        //            vb.Unlock();

        //            dev.SetStreamSource(0, vb, 0, 20);
        //            dev.DrawPrimitives(PrimitiveType.LineList, 0, 1);

        //            vb.Dispose();
        //        }

        //        private static void RenderPanadapterLine(Device dev, int W, int H, int rx, bool bottom)        // yt7pwr
        //        {
        //            if (pan_fill)// && (current_display_mode == DisplayMode.PANADAPTER || current_display_mode == DisplayMode.PANAFALL ||
        //                 // current_display_mode_bottom == DisplayMode.PANADAPTER || current_display_mode_bottom == DisplayMode.PANAFALL))
        //            {
        //                int j = 0;
        //                int i = 0;
        //                if (bottom)
        //                {
        //                    for (i = 0; i < W * 2; i++)
        //                    {
        //                        PanLine_bottom_verts_fill[i] = new Vertex();
        //                        PanLine_bottom_verts_fill[i].Color = pan_fill_color.ToArgb();
        //                        PanLine_bottom_verts_fill[i].Position = new Vector4(i / 2, panadapterX_data_bottom[j] + H, 0.0f, 0.0f);

        //                        PanLine_bottom_verts_fill[i + 1] = new Vertex();
        //                        PanLine_bottom_verts_fill[i + 1].Color = pan_fill_color.ToArgb();
        //                        PanLine_bottom_verts_fill[i + 1].Position = new Vector4(i / 2, H + H, 0.0f, 0.0f);
        //                        i++;
        //                        j++;
        //                    }

        //                    PanLine_bottom_vb_fill.Lock(0, 0, LockFlags.None).WriteRange(PanLine_bottom_verts_fill, 0, W * 2);
        //                    PanLine_bottom_vb_fill.Unlock();

        //                    dev.SetStreamSource(0, PanLine_bottom_vb_fill, 0, 20);
        //                    dev.DrawPrimitives(PrimitiveType.LineList, 0, W);
        //                }
        //                else
        //                {
        //                    for (i = 0; i < W * 2; i++)
        //                    {
        //                        PanLine_verts_fill[i] = new Vertex();
        //                        PanLine_verts_fill[i].Color = pan_fill_color.ToArgb();
        //                        PanLine_verts_fill[i].Position = new Vector4(i / 2, panadapterX_data[j], 0.0f, 0.0f);

        //                        PanLine_verts_fill[i + 1] = new Vertex();
        //                        PanLine_verts_fill[i + 1].Color = pan_fill_color.ToArgb();
        //                        PanLine_verts_fill[i + 1].Position = new Vector4(i / 2, H, 0.0f, 0.0f);
        //                        i++;
        //                        j++;
        //                    }

        //                    PanLine_vb_fill.Lock(0, 0, LockFlags.None).WriteRange(PanLine_verts_fill, 0, W * 2);
        //                    PanLine_vb_fill.Unlock();

        //                    dev.SetStreamSource(0, PanLine_vb_fill, 0, 20);
        //                    dev.DrawPrimitives(PrimitiveType.LineList, 0, W);
        //                }
        //            }

        //            if (bottom)
        //            {
        //                for (int i = 0; i < W; i++)
        //                {
        //                    PanLine_bottom_verts[i] = new Vertex();
        //                    if (mox) PanLine_bottom_verts[i].Color = tx_data_line_color.ToArgb();
        //                    else PanLine_bottom_verts[i].Color = data_line_color.ToArgb();
        //                    PanLine_bottom_verts[i].Position = new Vector4(i, panadapterX_data_bottom[i] + H, 0.0f, 0.0f);
        //                }

        //                PanLine_bottom_vb.Lock(0, 0, LockFlags.None).WriteRange(PanLine_bottom_verts, 0, W);
        //                PanLine_bottom_vb.Unlock();

        //                dev.SetStreamSource(0, PanLine_bottom_vb, 0, 20);
        //                dev.DrawPrimitives(PrimitiveType.LineStrip, 0, W - 1);
        //            }
        //            else
        //            {
        //                for (int i = 0; i < W; i++)
        //                {
        //                    PanLine_verts[i] = new Vertex();
        //                    if (mox) PanLine_verts[i].Color = tx_data_line_color.ToArgb();
        //                    else PanLine_verts[i].Color = data_line_color.ToArgb();
        //                    PanLine_verts[i].Position = new Vector4(i, panadapterX_data[i], 0.0f, 0.0f);
        //                }

        //                PanLine_vb.Lock(0, 0, LockFlags.None).WriteRange(PanLine_verts, 0, W);
        //                PanLine_vb.Unlock();

        //                dev.SetStreamSource(0, PanLine_vb, 0, 20);
        //                dev.DrawPrimitives(PrimitiveType.LineStrip, 0, W - 1);
        //            }
        //        }

        //#region General Routines

        //  #region GDI+

        /*  private static void UpdateDisplayPeak(float[] buffer, float[] new_data)
          {
              if (buffer[0] == CLEAR_FLAG)
              {
                  //Debug.WriteLine("Clearing peak buf"); 
                  for (int i = 0; i < BUFFER_SIZE; i++)
                      buffer[i] = new_data[i];
              }
              else
              {
                  for (int i = 0; i < BUFFER_SIZE; i++)
                  {
                      if (new_data[i] > buffer[i])
                          buffer[i] = new_data[i];
                      new_data[i] = buffer[i];
                  }
              }
          } */

        #region Drawing Routines
        // ======================================================
        // Drawing Routines
        // ======================================================

        /*   private static float dBToPixel(float dB)
           {
               return (float)(spectrum_grid_max - dB) * Target.Height / (spectrum_grid_max - spectrum_grid_min);
           }

           private static float PixelToDb(float y)
           {
               if (y <= H / 2) y *= 2.0f;
               else y = (y - H / 2) * 2.0f;

               return (float)(spectrum_grid_max - y * (double)(spectrum_grid_max - spectrum_grid_min) / H);
           }
           private static void DrawOffBackground(Graphics g, int W, int H, bool bottom)
           {
               // draw background
               //  g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

               // if (high_swr && !bottom)
               //  g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
           }

           private static float[] scope_min = new float[W];
           public static float[] ScopeMin
           {
               get { return scope_min; }
               set { scope_min = value; }
           }

           private static float[] scope_max = new float[W];
           public static float[] ScopeMax
           {
               get { return scope_max; }
               set { scope_max = value; }
           }
           private static float[] scope2_min = new float[W];
           public static float[] Scope2Min
           {
               get { return scope2_min; }
               set { scope2_min = value; }
           }

           private static float[] scope2_max = new float[W];
           public static float[] Scope2Max
           {
               get { return scope2_max; }
               set { scope2_max = value; }
           }  */

        //private static void ConvertDisplayData()
        //{
        //    if (!split_display)
        //    {
        //        switch (current_display_mode)
        //        {
        //            case DisplayMode.SPECTRUM:
        //                ConvertDataForSpectrum(W, H, false);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //                ConvertDataForPanadapter(W, H, 1, false);
        //                break;
        //            case DisplayMode.SCOPE:
        //                ConvertDataForScope(W, H, false);
        //                break;
        //            case DisplayMode.PHASE:
        //            case DisplayMode.PHASE2:
        //                ConvertDataForPhase(W, H, false);
        //                break;
        //            case DisplayMode.WATERFALL:
        //                // ConvertDataForPanadapter(W, H, 1, false);
        //                ConvertDataForWaterfall(waterfall_W, waterfall_H, 1, false);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                ConvertDataForHistogram();
        //                break;
        //            case DisplayMode.PANAFALL:
        //                //  ConvertDataForPanadapter();
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (current_display_mode)
        //        {
        //            case DisplayMode.SPECTRUM:
        //                ConvertDataForSpectrum(W, H / 2, false);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //                ConvertDataForPanadapter(W, H / 2, 1, false);
        //                break;
        //            case DisplayMode.SCOPE:
        //                ConvertDataForScope(W, H / 2, false);
        //                // DrawScope(e.Graphics, W, H / 2, false);
        //                break;
        //            case DisplayMode.PHASE:
        //                ConvertDataForPhase(W, H / 2, false);
        //                // DrawPhase(e.Graphics, W, H / 2, false);
        //                break;
        //            case DisplayMode.PHASE2:
        //                ConvertDataForPhase(W, H / 2, false);
        //                // DrawPhase2(e.Graphics, W, H / 2, false);
        //                break;
        //            case DisplayMode.WATERFALL:
        //                // DrawWaterfall(e.Graphics, W, H / 2, 1, false);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                ConvertDataForHistogram();
        //                // DrawHistogram(e.Graphics, W, H / 2);
        //                break;
        //            case DisplayMode.OFF:
        //                // DrawOffBackground(e.Graphics, W, H / 2, false);
        //                break;
        //            default:
        //                break;
        //        }

        //        switch (current_display_mode_bottom)
        //        {
        //            case DisplayMode.SPECTRUM:
        //                ConvertDataForSpectrum(W, H / 2, true);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //                ConvertDataForPanadapter(W, H / 2, 2, true);
        //                break;
        //            case DisplayMode.SCOPE:
        //                ConvertDataForScope(W, H / 2, true);
        //                // DrawScope(e.Graphics, W, H / 2, true);
        //                break;
        //            case DisplayMode.PHASE:
        //                ConvertDataForPhase(W, H / 2, true);
        //                // DrawPhase(e.Graphics, W, H / 2, true);
        //                break;
        //            case DisplayMode.PHASE2:
        //                ConvertDataForPhase(W, H / 2, true);
        //                //DrawPhase2(e.Graphics, W, H / 2, true);
        //                break;
        //            case DisplayMode.WATERFALL:
        //                // DrawWaterfall(e.Graphics, W, H / 2, 2, true);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                ConvertDataForHistogram();
        //                // DrawHistogram(e.Graphics, W, H / 2);
        //                break;
        //            case DisplayMode.OFF:
        //                // DrawOffBackground(e.Graphics, W, H / 2, true);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

        //public static void RenderDisplayData()
        //{
        //    if (!split_display)
        //    {
        //        switch (current_display_mode)
        //        {
        //            //#if false
        //            case DisplayMode.WATERFALL:
        //                if (!mox)
        //                {
        //                    DataRectangle rect = WaterfallTexture.LockRectangle(0, waterfall_rect, LockFlags.None);
        //                    waterfall_data_stream = rect.Data;
        //                    waterfall_data_stream.Position = 0;
        //                    waterfall_data_stream.Write(waterfall_memory, 0, ((int)waterfall_data_stream.Length));
        //                    WaterfallTexture.UnlockRectangle(0);
        //                    waterfall_dx_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 0.0f, 0);
        //                    Waterfall_Sprite.Begin(SpriteFlags.AlphaBlend);
        //                    Waterfall_Sprite.Draw(WaterfallTexture, Waterfall_texture_size, (Color4)Color.White);
        //                    Waterfall_Sprite.End();
        //                    waterfall_dx_device.BeginScene();
        //                    device.SetRenderState(RenderState.AlphaBlendEnable, true);
        //                    device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
        //                    device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.DestinationAlpha);
        //                    RenderVerticalLine(waterfall_dx_device, 0, 0, Color.Black);
        //                    waterfall_dx_device.EndScene();
        //                    waterfall_dx_device.Present();
        //                }
        //                break;
        //            //#endif
        //            case DisplayMode.SPECTRUM:
        //                RenderSpectrum(W, H, false);
        //                RenderPanadapterLine(device, W, H, 1, false);
        //                break;
        //            case DisplayMode.SCOPE:
        //                RenderScope(W, H, false);
        //                RenderPanadapterLine(device, W, H, 1, false);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //            case DisplayMode.PANAFALL:
        //                // case DisplayMode.WATERFALL:
        //                if (!console.PowerOn) return;
        //                //#if false
        //                // if (console.PowerOn)
        //                // {
        //                // RenderPanadapterLine(device);
        //                if (refresh_panadapter_grid)
        //                {
        //                    //  VerLines_vb = null;
        //                  //  HorLines_vb = null;
        //                    vertical_label = null;
        //                    horizontal_label = null;

        //                    RenderPanadapterGrid(W, H, 1, false);

        //                    refresh_panadapter_grid = false;
        //                }
        //                else
        //                {
        //                    //if (show_horizontal_grid)
        //                    RenderHorizontalLines(device, HorLines_vb, h_steps);
        //                    // if (show_vertical_grid)
        //                    RenderVerticalLines(device, VerLines_vb, 61);

        //                }

        //                for (int i = 0; i < f_steps; i++)
        //                {
        //                    panadapter_font.DrawString(null, vertical_label[i].label,
        //                    vertical_label[i].pos_x, vertical_label[i].pos_y, vertical_label[i].color.ToArgb());
        //                }

        //                for (int i = 1; i < h_steps; i++)
        //                {
        //                    panadapter_font.DrawString(null, horizontal_label[i].label,
        //                    horizontal_label[i].pos_x, horizontal_label[i].pos_y, horizontal_label[i].color.ToArgb());
        //                }
        //                //#endif
        //                //  panadapter_font.DrawString(null, "BW: " + rx_display_bw.ToString() + "Hz", 50, 30, Color.Red.ToArgb());                                        
        //                // panadapter_font.DrawString(null, "Resolution: " + bwres + "Hz/d", 50, 30, Color.Red.ToArgb());
        //                // panadapter_font.DrawString(null, "Step Size: " + bw_step_size.ToString("f1") + "Hz/p", 50, 60, Color.Red.ToArgb());
        //                panadapter_font.DrawString(null, "Width: " + W.ToString(), 50, 30, Color.Red.ToArgb());
        //                panadapter_font.DrawString(null, "Heigth: " + H.ToString(), 50, 45, Color.Red.ToArgb());
        //                panadapter_font.DrawString(null, "fps: " + framesPerSecond.ToString("F3"), 50, 60, Color.Red.ToArgb());

        //                RenderVFO(W, H, 1, false);

        //                // if (sub_rx1_enabled || split_enabled)
        //                //   Render_VFOB();

        //                // DrawAGCLines(W);

        //                RenderPanadapterLine(device, W, H, 1, false);

        //                break;
        //            case DisplayMode.PHASE:
        //                if (console.PowerOn)
        //                    RenderPhase(device, W, H, false);
        //                break;
        //            case DisplayMode.PHASE2:
        //                if (console.PowerOn)
        //                    RenderPhase2(device);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                if (console.PowerOn)
        //                {
        //                    if (refresh_panadapter_grid)
        //                    {
        //                        vertical_label = null;
        //                        horizontal_label = null;
        //                        RenderPanadapterGrid(W, H, 1, false);
        //                        refresh_panadapter_grid = false;
        //                    }
        //                    else
        //                    {
        //                        // if (show_horizontal_grid)
        //                        RenderHorizontalLines(device, HorLines_vb, h_steps);
        //                        // if (show_vertical_grid)
        //                        RenderVerticalLines(device, VerLines_vb, 56);
        //                    }

        //                    for (int i = 0; i < 10 + 1; i++)
        //                    {
        //                        panadapter_font.DrawString(null, vertical_label[i].label,
        //                        vertical_label[i].pos_x, vertical_label[i].pos_y, vertical_label[i].color.ToArgb());
        //                    }

        //                    for (int i = 2; i < h_steps; i++)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_label[i].label,
        //                        horizontal_label[i].pos_x, horizontal_label[i].pos_y, horizontal_label[i].color.ToArgb());
        //                    }

        //                    RenderHistogram(device);
        //                }
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (current_display_mode)
        //        {
        //            case DisplayMode.WATERFALL:
        //                /*                        waterfall_dx_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 0.0f, 0);
        //                                        Waterfall_Sprite.Begin(SpriteFlags.AlphaBlend);
        //                                        Waterfall_Sprite.Draw(WaterfallTexture, Waterfall_texture_size, (Color4)Color.White);
        //                                        Waterfall_Sprite.End();
        //                                        waterfall_dx_device.BeginScene();
        //                                        RenderVerticalLine(waterfall_dx_device, 100, 100, Color.Red);
        //                                        waterfall_dx_device.EndScene();
        //                                        waterfall_dx_device.Present();
        //                                        RenderWaterfall();*/
        //                break;
        //            case DisplayMode.SPECTRUM:
        //                RenderSpectrum(W, H / 2, false);
        //                RenderPanadapterLine(device, W, H / 2, 1, false);
        //                break;
        //            case DisplayMode.SCOPE:
        //                RenderScope(W, H / 2, false);
        //                RenderPanadapterLine(device, W, H / 2, 1, false);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //            case DisplayMode.PANAFALL:
        //                /*                        if (current_display_mode == DisplayMode.PANAFALL)
        //                                        {
        //                                            waterfall_dx_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 0.0f, 0);
        //                                            Waterfall_Sprite.Begin(SpriteFlags.AlphaBlend);
        //                                            Waterfall_Sprite.Draw(WaterfallTexture, Waterfall_texture_size, (Color4)Color.White);
        //                                            Waterfall_Sprite.End();
        //                                            waterfall_dx_device.BeginScene();
        //                                            RenderVerticalLine(waterfall_dx_device, 100, 100, Color.Red);
        //                                            waterfall_dx_device.EndScene();
        //                                            waterfall_dx_device.Present();
        //                                         }*/

        //                if (console.PowerOn)
        //                {
        //                    //#if false

        //                    if (refresh_panadapter_grid)
        //                    {
        //                       // VerLines_vb = null;
        //                      //  HorLines_vb = null;
        //                        vertical_label = null;
        //                        horizontal_label = null;

        //                        RenderPanadapterGrid(W, H / 2, 1, false);

        //                    }
        //                    else
        //                    {
        //                        //if (show_horizontal_grid)
        //                        RenderHorizontalLines(device, HorLines_vb, h_steps_top);
        //                        // if (show_vertical_grid)
        //                        RenderVerticalLines(device, VerLines_vb, 61);

        //                    }

        //                    for (int i = 0; i < f_steps_top; i++)
        //                    {
        //                        panadapter_font.DrawString(null, vertical_label[i].label,
        //                        vertical_label[i].pos_x, vertical_label[i].pos_y, vertical_label[i].color.ToArgb());

        //                    }

        //                    for (int i = 1; i < h_steps_top; i++)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_label[i].label,
        //                        horizontal_label[i].pos_x, horizontal_label[i].pos_y, horizontal_label[i].color.ToArgb());

        //                    }
        //                    //#endif
        //                    //  panadapter_font.DrawString(null, "BW: " + rx_display_bw.ToString() + "Hz", 50, 30, Color.Red.ToArgb());
        //                    //  panadapter_font.DrawString(null, "Resolution: " + bwres + "Hz/d", 50, 45, Color.Red.ToArgb());
        //                    //  panadapter_font.DrawString(null, "Step Size: " + bw_step_size.ToString("f1") + "Hz/p", 50, 60, Color.Red.ToArgb());
        //                    panadapter_font.DrawString(null, "Width: " + W.ToString(), 50, 30, Color.Red.ToArgb());
        //                    panadapter_font.DrawString(null, "Heigth: " + H.ToString(), 50, 45, Color.Red.ToArgb());
        //                    panadapter_font.DrawString(null, "fps: " + framesPerSecond.ToString("F3"), 50, 60, Color.Red.ToArgb());

        //                    RenderVFO(W, H / 2, 1, false);

        //                    // if (sub_rx1_enabled || split_enabled)
        //                    //   Render_VFOB();

        //                    // DrawAGCLines(W);

        //                    RenderPanadapterLine(device, W, H / 2, 1, false);

        //                }
        //                break;
        //            case DisplayMode.PHASE:
        //                if (console.PowerOn)
        //                    RenderPhase(device, W, H / 2, false);
        //                break;
        //            case DisplayMode.PHASE2:
        //                if (console.PowerOn)
        //                    RenderPhase2(device);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                if (console.PowerOn)
        //                {
        //                    if (refresh_panadapter_grid)
        //                    {
        //                        vertical_label = null;
        //                        horizontal_label = null;
        //                        RenderPanadapterGrid(W, H, 1, false);
        //                        refresh_panadapter_grid = false;
        //                    }
        //                    else
        //                    {
        //                        // if (show_horizontal_grid)
        //                        RenderHorizontalLines(device, HorLines_vb, h_steps);
        //                        // if (show_vertical_grid)
        //                        RenderVerticalLines(device, VerLines_vb, 56);
        //                    }

        //                    for (int i = 0; i < 10 + 1; i++)
        //                    {
        //                        panadapter_font.DrawString(null, vertical_label[i].label,
        //                        vertical_label[i].pos_x, vertical_label[i].pos_y, vertical_label[i].color.ToArgb());
        //                    }

        //                    for (int i = 2; i < h_steps; i++)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_label[i].label,
        //                        horizontal_label[i].pos_x, horizontal_label[i].pos_y, horizontal_label[i].color.ToArgb());
        //                    }

        //                    RenderHistogram(device);
        //                }
        //                break;
        //        }

        //        switch (current_display_mode_bottom)
        //        {
        //            case DisplayMode.WATERFALL:
        //                /*                        waterfall_dx_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 0.0f, 0);
        //                                        Waterfall_Sprite.Begin(SpriteFlags.AlphaBlend);
        //                                        Waterfall_Sprite.Draw(WaterfallTexture, Waterfall_texture_size, (Color4)Color.White);
        //                                        Waterfall_Sprite.End();
        //                                        waterfall_dx_device.BeginScene();
        //                                        RenderVerticalLine(waterfall_dx_device, 100, 100, Color.Red);
        //                                        waterfall_dx_device.EndScene();
        //                                        waterfall_dx_device.Present();
        //                                        RenderWaterfall();*/
        //                break;
        //            case DisplayMode.SPECTRUM:
        //                RenderSpectrum(W, H / 2, true);
        //                RenderPanadapterLine(device, W, H / 2, 2, true);
        //                break;
        //            case DisplayMode.SCOPE:
        //                RenderScope(W, H / 2, true);
        //                RenderPanadapterLine(device, W, H / 2, 2, true);
        //                break;
        //            case DisplayMode.PANADAPTER:
        //            case DisplayMode.PANAFALL:
        //                /*                        if (current_display_mode == DisplayMode.PANAFALL)
        //                                        {
        //                                            waterfall_dx_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 0.0f, 0);
        //                                            Waterfall_Sprite.Begin(SpriteFlags.AlphaBlend);
        //                                            Waterfall_Sprite.Draw(WaterfallTexture, Waterfall_texture_size, (Color4)Color.White);
        //                                            Waterfall_Sprite.End();
        //                                            waterfall_dx_device.BeginScene();
        //                                            RenderVerticalLine(waterfall_dx_device, 100, 100, Color.Red);
        //                                            waterfall_dx_device.EndScene();
        //                                            waterfall_dx_device.Present();
        //                                         }*/

        //                if (console.PowerOn)
        //                {
        //                    //#if false                           
        //                    if (refresh_panadapter_grid)
        //                    {
        //                      //  VerLines_bottom_vb = null;
        //                      //  HorLines_bottom_vb = null;
        //                        vertical_bottom_label = null;
        //                        horizontal_bottom_label = null;

        //                        RenderPanadapterGrid(W, H / 2, 2, true);

        //                        refresh_panadapter_grid = false;
        //                    }
        //                    else
        //                    {
        //                        //if (show_horizontal_grid)

        //                        // if (show_vertical_grid)

        //                        RenderHorizontalLines(device, HorLines_bottom_vb, h_steps_bottom);
        //                        RenderVerticalLines(device, VerLines_bottom_vb, 61);
        //                    }

        //                    for (int i = 0; i < f_steps_bottom; i++)
        //                    {
        //                        panadapter_font.DrawString(null, vertical_bottom_label[i].label,
        //                        vertical_bottom_label[i].pos_x, vertical_bottom_label[i].pos_y, vertical_bottom_label[i].color.ToArgb());
        //                    }

        //                    for (int i = 1; i < h_steps_bottom; i++)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_bottom_label[i].label,
        //                        horizontal_bottom_label[i].pos_x, horizontal_bottom_label[i].pos_y, horizontal_bottom_label[i].color.ToArgb());
        //                    }
        //                    //#endif                          

        //                    RenderVFO(W, H / 2, 2, true);

        //                    // if (sub_rx1_enabled || split_enabled)
        //                    //   Render_VFOB();


        //                    // DrawAGCLines(W);

        //                    RenderPanadapterLine(device, W, H / 2, 2, true);
        //                }
        //                break;
        //            case DisplayMode.PHASE:
        //                if (console.PowerOn)
        //                    RenderPhase(device, W, H / 2, true);
        //                break;
        //            case DisplayMode.PHASE2:
        //                if (console.PowerOn)
        //                    RenderPhase2(device);
        //                break;
        //            case DisplayMode.HISTOGRAM:
        //                if (console.PowerOn)
        //                {
        //                    if (refresh_panadapter_grid)
        //                    {
        //                        vertical_label = null;
        //                        horizontal_label = null;
        //                        RenderPanadapterGrid(W, H, 1, false);
        //                        refresh_panadapter_grid = false;
        //                    }
        //                    else
        //                    {
        //                        // if (show_horizontal_grid)
        //                        RenderHorizontalLines(device, HorLines_vb, h_steps);
        //                        // if (show_vertical_grid)
        //                        RenderVerticalLines(device, VerLines_vb, 56);
        //                    }

        //                    for (int i = 0; i < 10 + 1; i++)
        //                    {
        //                        panadapter_font.DrawString(null, vertical_label[i].label,
        //                        vertical_label[i].pos_x, vertical_label[i].pos_y, vertical_label[i].color.ToArgb());
        //                    }

        //                    for (int i = 2; i < h_steps; i++)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_label[i].label,
        //                        horizontal_label[i].pos_x, horizontal_label[i].pos_y, horizontal_label[i].color.ToArgb());
        //                    }

        //                    RenderHistogram(device);
        //                }
        //                break;
        //        }
        //    }
        //}

        //        public static void RenderDirectX()
        //        {
        //            if (!console.PowerOn) return;
        //            FrameDelta = clock.Update();
        //            try
        //            {
        //                //render_mutex.WaitOne();

        //                if ((device == null) || (waterfall_dx_device == null))
        //                {
        //                    //render_mutex.ReleaseMutex();
        //                    return;
        //                }

        //                if (DX_reinit)
        //                {
        //                    return;
        //                }

        //                frameAccumulator += FrameDelta;
        //                ++frameCount;
        //                if (frameAccumulator >= 1.0f)
        //                {
        //                    framesPerSecond = frameCount / frameAccumulator;
        //                    frameAccumulator = 0.0f;
        //                    frameCount = 0;
        //                }

        //                // setup data
        //                ConvertDisplayData();

        //                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, display_background_color.ToArgb(), 1.0f, 0);

        //                //  if (Panadapter_Sprite != null)
        //                //  {
        //                //  Panadapter_Sprite.Begin(SpriteFlags.SortTexture | SpriteFlags.AlphaBlend);
        //                // if (PanadapterTexture != null)
        //                // {
        //                //Panadapter_Sprite.Draw(PanadapterTexture, Panadapter_texture_size, (Color4)Color.White);
        //                // }
        //                //if (console.HighSWR)
        //                // {
        //                // SlimDX.Direct3D9.Font high_swr_font = new SlimDX.Direct3D9.Font(device,
        //                // new System.Drawing.Font("Arial", 14.0f, FontStyle.Bold));
        //                //  high_swr_font.DrawString(Panadapter_Sprite, string.Format("High SWR"),
        //                //   new Rectangle(40, 20, 0, 0), DrawTextFormat.NoClip, Color.Red);
        //                // }
        //                //  Panadapter_Sprite.End();
        //                //  }
        //                //Begin the scene
        //                device.BeginScene();
        //                RenderDisplayData();

        //                //End the scene
        //                device.EndScene();
        //                device.Present();
        //                //render_mutex.ReleaseMutex();
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.Write("Error in RenderDirectX()\n" + ex.ToString());
        //                //render_mutex.ReleaseMutex();
        //                //Panadapter_Event.Set();
        //            }
        //        }

        //        unsafe private static void ConvertDataForPanadapter(int W, int H, int rx, bool bottom)  // changes yt7pwr
        //        {
        //            try
        //            {
        //                if (bottom)
        //                {
        //                    if (panadapterX_data_bottom == null || panadapterX_data_bottom.Length != W)
        //                        panadapterX_data_bottom = new float[W]; // array of points to display
        //                }
        //                else
        //                {
        //                    if (panadapterX_data == null || panadapterX_data.Length != W)
        //                        panadapterX_data = new float[W]; // array of points to display
        //                }

        //                // if (waterfallX_data == null || waterfallX_data.Length != W)
        //                //   waterfallX_data = new float[W];
        //                float slope = 0.0f;				        	            	// samples to process per pixel
        //                int num_samples = 0;					                    // number of samples to process
        //                int start_sample_index = 0;			        	            // index to begin looking at samples
        //                int Low = 0;// rx_display_low;
        //                int High = 0;// rx_display_high;
        //                int sample_rate;

        //                bool local_mox = false;
        //                if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
        //                if (rx == 2 && tx_on_vfob && mox) local_mox = true;
        //                float local_max_y = float.MinValue;
        //                max_y = Int32.MinValue;
        //                bool waterfall = false;
        //                int grid_max = 0;
        //                int grid_min = 0;

        //                if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
        //                if (rx == 2 && tx_on_vfob && mox) local_mox = true;
        //                if (rx == 1 && tx_on_vfob && mox && !console.RX2Enabled) local_mox = true;
        //                if (CurrentDisplayMode == DisplayMode.PANAFALL) waterfall = true;

        //                if (rx == 2)
        //                {
        //                    if (local_mox)// && tx_on_vfob)
        //                    {
        //                        Low = tx_display_low;
        //                        High = tx_display_high;
        //                        grid_max = tx_spectrum_grid_max;
        //                        grid_min = tx_spectrum_grid_min;
        //                        sample_rate = sample_rate_tx;
        //                    }
        //                    else
        //                    {
        //                        Low = rx2_display_low;
        //                        High = rx2_display_high;
        //                        grid_max = rx2_spectrum_grid_max;
        //                        grid_min = rx2_spectrum_grid_min;
        //                        sample_rate = sample_rate_rx2;
        //                    }
        //                }
        //                else
        //                {
        //                    if (local_mox && !waterfall) // && !tx_on_vfob)
        //                    {
        //                        Low = tx_display_low;
        //                        High = tx_display_high;
        //                        grid_max = tx_spectrum_grid_max;
        //                        grid_min = tx_spectrum_grid_min;
        //                        sample_rate = sample_rate_tx;
        //                    }
        //                    else
        //                    {
        //                        Low = rx_display_low;
        //                        High = rx_display_high;
        //                        grid_max = spectrum_grid_max;
        //                        grid_min = spectrum_grid_min;
        //                        sample_rate = sample_rate_rx1;
        //                    }
        //                }

        //                int yRange = grid_max - grid_min;

        //                if (rx == 1 && data_ready)
        //                {
        //                    // get new data
        //                    if (local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //                    {
        //                        for (int i = 0; i < current_display_data.Length; i++)
        //                            current_display_data[i] = grid_min - rx1_display_cal_offset; //-200.0f;
        //                    }
        //                    else
        //                    {
        //                        fixed (void* rptr = &new_display_data[0])
        //                        fixed (void* wptr = &current_display_data[0])
        //                            Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //                        //  Array.Copy(new_display_data, current_display_data, current_display_data.Length);

        //                        // DataReady = false;
        //                    }
        //                    data_ready = false;
        //                }
        //                else if (rx == 2 && data_ready_bottom)
        //                {
        //                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
        //                    {
        //                        for (int i = 0; i < current_display_data_bottom.Length; i++)
        //                            current_display_data_bottom[i] = spectrum_grid_min - rx2_display_cal_offset;
        //                    }
        //                    else
        //                    {
        //                        fixed (void* rptr = &new_display_data_bottom[0])
        //                        fixed (void* wptr = &current_display_data_bottom[0])
        //                            Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //                        // Array.Copy(new_display_data_bottom, current_display_data_bottom, current_display_data_bottom.Length);
        //                    }
        //                    data_ready_bottom = false;
        //                }

        //                if (rx == 1 && average_on && local_mox && !waterfall)
        //                {
        //                    console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
        //                }
        //                else if (rx == 2 && rx2_avg_on && local_mox)
        //                    console.UpdateRX2DisplayAverage(rx2_average_buffer, current_display_data_bottom);

        //                if (rx == 1 && peak_on && local_mox && !waterfall)
        //                    UpdateDisplayPeak(rx1_peak_buffer, current_display_data);
        //                else
        //                    if (rx == 2 && rx2_peak_on && local_mox)
        //                        UpdateDisplayPeak(rx2_peak_buffer, current_display_data_bottom);

        //                if (local_mox && !waterfall)
        //                {
        //                    start_sample_index = (BUFFER_SIZE >> 1) + (int)((Low * BUFFER_SIZE) / sample_rate);
        //                    num_samples = (int)((BUFFER_SIZE * (High - Low)) / sample_rate);
        //                    if (start_sample_index < 0) start_sample_index += 4096;
        //                    if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
        //                        num_samples = (BUFFER_SIZE - start_sample_index);

        //                    //Debug.WriteLine("start_sample_index: "+start_sample_index);
        //                    slope = (float)num_samples / (float)W;
        //                }

        //                for (int i = 0; i < W; i++)
        //                {
        //                    float max = float.MinValue;
        //                    float dval = i * slope + start_sample_index;
        //                    int lindex = (int)Math.Floor(dval);
        //                    int rindex = (int)Math.Floor(dval + slope);

        //                    if (rx == 1)
        //                    {
        //                        if (local_mox && !waterfall)
        //                        {
        //                            if (slope <= 1.0 || lindex == rindex)
        //                            {
        //                                max = current_display_data[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data[(lindex + 1) % 4096] * (dval - (float)lindex);
        //                            }
        //                            else
        //                            {
        //                                for (int j = lindex; j < rindex; j++)
        //                                    if (current_display_data[j % 4096] > max) max = current_display_data[j % 4096];
        //                            }
        //                        }

        //                        else max = current_display_data[i];
        //                    }
        //                    else if (rx == 2)
        //                    {
        //                        if (local_mox)
        //                        {
        //                            if (slope <= 1.0 || lindex == rindex)
        //                            {
        //                                max = current_display_data_bottom[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data_bottom[(lindex + 1) % 4096] * (dval - (float)lindex);
        //                            }
        //                            else
        //                            {
        //                                for (int j = lindex; j < rindex; j++)
        //                                    if (current_display_data_bottom[j % 4096] > max) max = current_display_data_bottom[j % 4096];
        //                            }
        //                        }
        //                        else max = current_display_data_bottom[i];
        //                    }

        //                    if (rx == 1)
        //                    {
        //                        if (local_mox && !waterfall) max += tx_display_cal_offset;
        //                        else if (mox && tx_on_vfob && !waterfall)
        //                        {
        //                            if (console.RX2Enabled) max += rx1_display_cal_offset;
        //                            else max += tx_display_cal_offset;
        //                        }
        //                        else max += rx1_display_cal_offset;
        //                    }
        //                    else if (rx == 2)
        //                    {
        //                        if (local_mox) max += tx_display_cal_offset;
        //                        else max += rx2_display_cal_offset;
        //                    }

        //                    if (!local_mox || (local_mox && waterfall))
        //                    {
        //                        if (rx == 1) max += rx1_preamp_offset;
        //                        else if (rx == 2) max += rx2_preamp_offset;
        //                    }

        //                    if (max > local_max_y)
        //                    {
        //                        local_max_y = max;
        //                        max_x = i;
        //                    }

        //                   /* if (bottom)
        //                        panadapterX_data_bottom[i] = (int)(Math.Floor((grid_max - max) * H / yRange));
        //                    else
        //                    {
        //                        panadapter_verts[i].X = i;
        //                        panadapter_verts[i].Y = ((int)Math.Min(Math.Floor((grid_max - max) * H / yRange), H));

        //                        panadapterX_data[i] = ((int)Math.Min(Math.Floor((grid_max - max) * H / yRange), H));
        //                        // panadapterX_data[i] = (int)(Math.Floor((spectrum_grid_max - max) * H / yRange));
        //                    } */
        //                    if (bottom) panadapterX_data_bottom[i] = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
        //                    else panadapterX_data[i] = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);

        //                }

        //                if (!bottom) max_y = local_max_y;

        //                if (bottom)
        //                {
        //                    panadapterX_data_bottom[0] = panadapterX_data_bottom[W - 1];
        //                    panadapterX_data_bottom[W - 1] = panadapterX_data_bottom[W - 2];
        //                }
        //                else
        //                {
        //                    panadapterX_data[0] = panadapterX_data[W - 1];
        //                    panadapterX_data[W - 1] = panadapterX_data[W - 2];
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.Write(ex.ToString());
        //            }
        //        }

        //       // private static float bw_step_size = 0;
        //       // private static string bwres = "";
        //        private static int h_steps = 0;
        //        private static int f_steps = 0;
        //        private static int h_steps_top = 0;
        //        private static int f_steps_top = 0;
        //        private static int h_steps_bottom = 0;
        //        private static int f_steps_bottom = 0;
        //        private static VerticalString[] vertical_label;
        //        private static VerticalString[] vertical_bottom_label;
        //        private static int vgrid;
        //        private static int offsetL;
        //        private static HorizontalString[] horizontal_label;
        //        private static HorizontalString[] horizontal_bottom_label;
        //        private static int vertexsize = Marshal.SizeOf(typeof(Vertex));
        //        private static int vertexsize2x = 2 * vertexsize;
        //        private static int vertexsize4x = 4 * vertexsize;

        //        private static void RenderPanadapterGrid(int W, int H, int rx, bool bottom)
        //        {
        //            bool on_actual_fgrid = false;
        //            bool local_mox = false;
        //            bool waterfall = false;
        //            if (mox && rx == 1 && !tx_on_vfob) local_mox = true;
        //            if (mox && rx == 2 && tx_on_vfob) local_mox = true;
        //            if (CurrentDisplayMode == DisplayMode.PANAFALL) waterfall = true;

        //            int low = 0;
        //            int high = 0;
        //            int mid_w = W / 2;
        //            int[] step_list = { 1, 5, 10, 20, 25, 50 };
        //            int step_power = 1;
        //            int step_index = 0;
        //            int freq_step_size = 50;
        //            int inbetweenies = 5;
        //            int grid_max = 0;
        //            int grid_min = 0;
        //            int grid_step = 0;
        //            int first_vgrid = 0;
        //            int[] band_edge_list;

        //            if (local_mox && !waterfall)// || (mox && tx_on_vfob))
        //            {
        //                grid_max = tx_spectrum_grid_max;
        //                grid_min = tx_spectrum_grid_min;
        //                grid_step = tx_spectrum_grid_step;
        //                low = tx_display_low;
        //                high = tx_display_high;
        //            }
        //            else if (rx == 2)
        //            {
        //                grid_max = rx2_spectrum_grid_max;
        //                grid_min = rx2_spectrum_grid_min;
        //                grid_step = rx2_spectrum_grid_step;
        //                low = rx2_display_low;
        //                high = rx2_display_high;
        //            }
        //            else
        //            {
        //                grid_max = spectrum_grid_max;
        //                grid_min = spectrum_grid_min;
        //                grid_step = spectrum_grid_step;
        //                low = rx_display_low;
        //                high = rx_display_high;
        //            }

        //            int y_range = grid_max - grid_min;
        //            int center_line_x = (int)(-(double)low / (high - low) * W);

        //            // Calculate horizontal step size
        //            int width = high - low;
        //            while (width / freq_step_size > 10)
        //            {
        //                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //                step_index = (step_index + 1) % 4;
        //                if (step_index == 0) step_power++;
        //            }
        //            f_steps = (width / freq_step_size) + 1;

        //            // calculate vertical step size
        //            h_steps = y_range / grid_step;
        //            top_size = (int)((double)grid_step * H / y_range);
        //           // int top = (int)((double)grid_step * H / y_range);
        //            if (bottom) // preserve steps
        //            {
        //                h_steps_bottom = h_steps;
        //                f_steps_bottom = f_steps;
        //            }
        //            else
        //            {
        //                h_steps_top = h_steps;
        //                f_steps_top = f_steps;
        //            }

        //            double vfo;

        //            if (rx == 1)
        //            {
        //                if (local_mox && !tx_on_vfob)
        //                {
        //                    vfo = split_enabled ? vfoa_sub_hz : vfoa_hz;
        //                    vfo += xit_hz;
        //                }
        //                else vfo = vfoa_hz + rit_hz;
        //            }
        //            else //if(rx==2)
        //            {
        //                if (local_mox && tx_on_vfob)
        //                    vfo = vfob_hz + xit_hz;
        //                else vfo = vfob_hz + rit_hz;
        //            }

        //            if (!bottom)
        //            {
        //                switch (rx1_dsp_mode)
        //                {
        //                    case DSPMode.CWL:
        //                        vfo += cw_pitch;
        //                        break;
        //                    case DSPMode.CWU:
        //                        vfo -= cw_pitch;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                switch (rx2_dsp_mode)
        //                {
        //                    case DSPMode.CWL:
        //                        vfo += cw_pitch;
        //                        break;
        //                    case DSPMode.CWU:
        //                        vfo -= cw_pitch;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }

        //            int f_steps_top_old = 0;
        //            int f_steps_bottom_old = 0;
        //            int h_steps_top_old = 0;
        //            int h_steps_bottom_old = 0;
        //            float scale = 0.0f;
        //            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
        //            long vfo_delta = (long)(vfo - vfo_round);

        //            // #if false
        //           // f_steps = (width / freq_step_size) + 1;
        //            // Initialize Vertex Buffers
        //            if (bottom)
        //            {
        //                if (VerLines_bottom_vb == null || f_steps_bottom_old != f_steps_bottom)
        //                    VerLines_bottom_vb = new VertexBuffer(device,
        //                        61 * vertexsize2x,
        //                        Usage.WriteOnly,
        //                        VertexFormat.None,
        //                        Pool.Managed);
        //                /*  if (HorLines_bottom_vb == null || h_steps_old != h_steps)
        //                  {
        //                      if (HorLines_bottom_vb != null)
        //                      {
        //                          HorLines_bottom_vb.Dispose();
        //                      }
        //                      HorLines_bottom_vb = null;
        //                      HorLines_bottom_vb = new VertexBuffer(device, (h_steps * 2) * 20, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
        //                  } */

        //                if (HorLines_bottom_vb == null || h_steps_bottom_old != h_steps_bottom)
        //                    HorLines_bottom_vb = new VertexBuffer(device,
        //                        h_steps * vertexsize2x,
        //                        Usage.WriteOnly,
        //                        VertexFormat.None,
        //                        Pool.Managed);
        //                if (vertical_bottom_label == null)
        //                    vertical_bottom_label = new VerticalString[f_steps];
        //                if (horizontal_bottom_label == null)
        //                    horizontal_bottom_label = new HorizontalString[h_steps];
        //                h_steps_bottom_old = h_steps_bottom;
        //                f_steps_bottom_old = f_steps_bottom;
        //            }
        //            else
        //            {
        //                if (VerLines_vb == null || f_steps_top_old != f_steps_top)
        //                    VerLines_vb = new VertexBuffer(device,
        //                         61 * vertexsize2x,
        //                         Usage.WriteOnly,
        //                         VertexFormat.None,
        //                         Pool.Managed);
        //               if (HorLines_vb == null || h_steps_top_old != h_steps_top)
        //                    HorLines_vb = new VertexBuffer(device,
        //                        h_steps * vertexsize2x,
        //                        Usage.WriteOnly,
        //                        VertexFormat.None,
        //                        Pool.Managed);
        //                if (vertical_label == null)
        //                    vertical_label = new VerticalString[f_steps];
        //                if (horizontal_label == null)
        //                    horizontal_label = new HorizontalString[h_steps];
        //                h_steps_top_old = h_steps_top;
        //                f_steps_top_old = f_steps_top;
        //            }
        //            // #endif

        //            // Draw vertical lines
        //            // #if false
        //            int loop = 0;
        //            for (int i = 0; i < f_steps; i++) // draws major vertical lines
        //            {
        //                //#if false
        //                int fgrid = i * freq_step_size + (low / freq_step_size) * freq_step_size;
        //                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1e6;
        //                vgrid = (int)((double)(fgrid - vfo_delta - low) / (high - low) * W);

        //                if (i == 0) first_vgrid = vgrid;
        //                if (bottom)
        //                {
        //                    VerLines_bottom_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                    {
        //                        new Vertex() { Color = grid_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H + top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = grid_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H + H, 0.0f, 0.0f) },
        //                    });
        //                    VerLines_bottom_vb.Unlock();

        //                    RenderVerticalLine(device, vgrid, H + top_size, vgrid, H + H, grid_color);
        //                }
        //                else
        //                {
        //                    VerLines_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                    {
        //                        new Vertex() { Color = grid_color.ToArgb(), Position = new Vector4((float)vgrid, (float)top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = grid_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H, 0.0f, 0.0f) },
        //                    });
        //                    VerLines_vb.Unlock();

        //                    RenderVerticalLine(device, vgrid, top_size, vgrid, H, grid_color);
        //                }
        //                //#endif

        //                switch (console.CurrentRegion)
        //                {
        //                    case FRSRegion.US:
        //                    case FRSRegion.Extended:
        //                        if (
        //                             actual_fgrid == 0.1357 || actual_fgrid == 0.1378 ||
        //                             actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
        //                             actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
        //                             actual_fgrid == 3.5 || actual_fgrid == 4.0 ||
        //                             actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
        //                             actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                             actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                             actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                             actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                             actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                             actual_fgrid == 50.0 || actual_fgrid == 54.0 ||
        //                             actual_fgrid == 144.0 || actual_fgrid == 148.0) 
        //                            on_actual_fgrid = true;
        //                        break;
        //                        case FRSRegion.Spain:
        //                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
        //                           on_actual_fgrid = true;
        //                        break;
        //                         case FRSRegion.India:
        //                            if (actual_fgrid == 1.81 || actual_fgrid == 1.86 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.9 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
        //                         on_actual_fgrid = true;
        //                        break;
        //                        case FRSRegion.Europe:
        //                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
        //                        on_actual_fgrid = true;
        //                        break;
        //                          case FRSRegion.UK:
        //                            if (actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
        //                                actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 5.2585 || actual_fgrid == 5.4065 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.0 || actual_fgrid == 52.0)
        //                     on_actual_fgrid = true;
        //                        break;
        //                        case FRSRegion.Italy_Plus:
        //                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 6.975 || actual_fgrid == 7.2 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
        //                     on_actual_fgrid = true;
        //                        break;
        //                         case FRSRegion.Japan:
        //                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
        //                            actual_fgrid == 1.81 || actual_fgrid == 1.825 ||
        //                            actual_fgrid == 1.9075 || actual_fgrid == 1.9125 ||
        //                            actual_fgrid == 3.5 || actual_fgrid == 3.575 ||
        //                            actual_fgrid == 3.599 || actual_fgrid == 3.612 ||
        //                            actual_fgrid == 3.68 || actual_fgrid == 3.687 ||
        //                            actual_fgrid == 3.702 || actual_fgrid == 3.716 ||
        //                            actual_fgrid == 3.745 || actual_fgrid == 3.77 ||
        //                            actual_fgrid == 3.791 || actual_fgrid == 3.805 ||
        //                            actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
        //                            actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                            actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                            actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                            actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                            actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                            actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                            actual_fgrid == 50.0 || actual_fgrid == 54.0)
        //                   on_actual_fgrid = true;
        //                        break;
        //                         case FRSRegion.Australia:
        //                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
        //                                actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
        //                                actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
        //                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
        //                    on_actual_fgrid = true;
        //                        break;
        //                          case FRSRegion.Norway:
        //                            if (actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
        //                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
        //                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
        //                                actual_fgrid == 5.26 || actual_fgrid == 5.41 ||
        //                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
        //                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
        //                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
        //                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
        //                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
        //                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
        //                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
        //                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
        //                   on_actual_fgrid = true;
        //                        break;
        //                }
        //                if (on_actual_fgrid)
        //                {
        //                    on_actual_fgrid = false;
        //                    if (bottom) // band edges
        //                    {
        //                        VerLines_bottom_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                        {
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H + top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H + H, 0.0f, 0.0f) },
        //                            });
        //                        VerLines_bottom_vb.Unlock();

        //                        RenderVerticalLine(device, vgrid, H + top_size, vgrid, H + H, band_edge_color);
        //                    }
        //                    else
        //                    {
        //                        VerLines_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                        {
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)vgrid, (float)top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)vgrid, (float)H, 0.0f, 0.0f) },
        //                            });
        //                        VerLines_vb.Unlock();

        //                        RenderVerticalLine(device, vgrid, top_size, vgrid, H, band_edge_color);
        //                    }

        ////#if false
        //                    if (bottom)
        //                    {
        //                        vertical_bottom_label[i].label = actual_fgrid.ToString("f3");
        //                        if (actual_fgrid < 10) offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 14;
        //                        else if (actual_fgrid < 100.0) offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 11;
        //                        else offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 8;
        //                        panadapter_font.DrawString(null, vertical_bottom_label[i].label, vgrid - offsetL, H, grid_zero_color.ToArgb());
        //                        vertical_bottom_label[i].pos_x = (vgrid - offsetL);
        //                        vertical_bottom_label[i].pos_y = H;
        //                        vertical_bottom_label[i].color = grid_zero_color;
        //                    }
        //                    else
        //                    {
        //                        vertical_label[i].label = actual_fgrid.ToString("f3");
        //                        if (actual_fgrid < 10) offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 14;
        //                        else if (actual_fgrid < 100.0) offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 11;
        //                        else offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 8;
        //                        panadapter_font.DrawString(null, vertical_label[i].label, vgrid - offsetL, 0, grid_zero_color.ToArgb());
        //                        vertical_label[i].pos_x = (vgrid - offsetL);
        //                        vertical_label[i].pos_y = 0;
        //                        vertical_label[i].color = grid_zero_color;
        //                    }
        //                }
        //                else
        //                {
        //                    if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
        //                    {
        //                        if (bottom)
        //                        {
        //                            vertical_bottom_label[i].label = actual_fgrid.ToString("f3"); //wa6ahl

        //                            if (actual_fgrid < 10) offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 14;
        //                            else if (actual_fgrid < 100.0) offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 11;
        //                            else offsetL = (int)((vertical_bottom_label[i].label.Length + 1) * 4.1) - 8;
        //                        }
        //                        else
        //                        {
        //                            vertical_label[i].label = actual_fgrid.ToString("f3"); //wa6ahl

        //                            if (actual_fgrid < 10) offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 14;
        //                            else if (actual_fgrid < 100.0) offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 11;
        //                            else offsetL = (int)((vertical_label[i].label.Length + 1) * 4.1) - 8;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        string temp_string;
        //                        int jper;
        //                        if (bottom)
        //                        {
        //                            vertical_bottom_label[i].label = actual_fgrid.ToString("f5");
        //                            temp_string = vertical_bottom_label[i].label;
        //                            jper = vertical_bottom_label[i].label.IndexOf('.') + 4;
        //                            vertical_bottom_label[i].label = vertical_bottom_label[i].label.Insert(jper, " ");

        //                            if (actual_fgrid < 10) offsetL = (int)((vertical_bottom_label[i].label.Length) * 4.1) - 14;
        //                            else if (actual_fgrid < 100.0) offsetL = (int)((vertical_bottom_label[i].label.Length) * 4.1) - 11;
        //                            else offsetL = (int)((vertical_bottom_label[i].label.Length) * 4.1) - 8;
        //                        }
        //                        else
        //                        {
        //                            vertical_label[i].label = actual_fgrid.ToString("f5");
        //                            temp_string = vertical_label[i].label;
        //                            jper = vertical_label[i].label.IndexOf('.') + 4;
        //                            vertical_label[i].label = vertical_label[i].label.Insert(jper, " ");

        //                            if (actual_fgrid < 10) offsetL = (int)((vertical_label[i].label.Length) * 4.1) - 14;
        //                            else if (actual_fgrid < 100.0) offsetL = (int)((vertical_label[i].label.Length) * 4.1) - 11;
        //                            else offsetL = (int)((vertical_label[i].label.Length) * 4.1) - 8;
        //                        }
        //                    }
        //                    if (bottom)
        //                    {
        //                        vertical_bottom_label[i].pos_x = (vgrid - offsetL);
        //                        vertical_bottom_label[i].pos_y = H;
        //                        vertical_bottom_label[i].color = grid_text_color;
        //                    }
        //                    else
        //                    {
        //                        vertical_label[i].pos_x = (vgrid - offsetL);
        //                        vertical_label[i].pos_y = 0;
        //                        vertical_label[i].color = grid_text_color;
        //                    }
        ////#endif
        //                }


        //                //#if false
        //                // Draw inbetweens
        //                int fgrid_2 = ((i + 1) * freq_step_size) + (int)((low / freq_step_size) * freq_step_size);
        //                int x_2 = (int)(((float)(fgrid_2 - vfo_delta - low) / width * W));
        //                scale = (float)(x_2 - vgrid) / inbetweenies;

        //                for (int j = 1; j < inbetweenies; j++)
        //                 {
        //                    float x3 = (float)vgrid + (j * scale);
        //                    if (bottom)
        //                    {
        //                        VerLines_bottom_vb.Lock((loop + j) * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                        {
        //                       new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4(x3, (float)H + top_size, 0.0f, 0.0f) },
        //                       new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4(x3, (float)H + H, 0.0f, 0.0f) },
        //                           });
        //                        VerLines_bottom_vb.Unlock();

        //                        RenderVerticalLine(device, (int)x3, H + top_size, (int)x3, H + H, grid_pen_dark);
        //                    }
        //                    else
        //                    {
        //                        VerLines_vb.Lock((loop + j) * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                        {
        //                       new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4(x3, (float)top_size, 0.0f, 0.0f) },
        //                       new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4(x3, (float)H, 0.0f, 0.0f) },
        //                            });
        //                        VerLines_vb.Unlock();

        //                        RenderVerticalLine(device, (int)x3, top_size, (int)x3, H, grid_pen_dark);
        //                    }
        //                }
        //                //#endif
        //                loop += 5;
        //                //r loop_num++;
        //                // i += 5;
        //            }
        //            //#endif
        //            //  }
        //            //#endif
        //            //  loop_num = 0;
        //            //#if false
        //            for (int j = 0; j < 3; j++)
        //            {
        //                loop++;
        //                int x3 = (first_vgrid > 0 && first_vgrid > (int)scale) ? first_vgrid - (j + 1) * (int)scale : -1;
        //                // int x3 = 0;
        //                // if (first_vgrid > 0 && first_vgrid > (int)scale)
        //                //  x3 = first_vgrid - (j + 1) * (int)scale;
        //                // else
        //                //   x3 = -1;

        //                if (bottom)
        //                {
        //                    VerLines_bottom_vb.Lock((j + loop) * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                    {
        //                        new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4((float)x3, (float)H + top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4((float)x3, (float)H + H, 0.0f, 0.0f) },
        //                    });
        //                    VerLines_bottom_vb.Unlock();

        //                    RenderVerticalLine(device, x3, H + top_size, x3, H + H, grid_pen_dark);
        //                }
        //                else
        //                {
        //                    VerLines_vb.Lock((j + loop) * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                    {
        //                        new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4((float)x3, (float)top_size, 0.0f, 0.0f) },
        //                        new Vertex() { Color = grid_pen_dark.ToArgb(), Position = new Vector4((float)x3, (float)H, 0.0f, 0.0f) },
        //                     });
        //                    VerLines_vb.Unlock();

        //                    RenderVerticalLine(device, x3, top_size, x3, H, grid_pen_dark);
        //                }
        //            }
        //            //#endif

        //            //#if false
        //            switch (console.CurrentRegion)
        //            {
        //                case FRSRegion.Australia:
        //                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000, 
        //                 3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 
        //                 18168000, 21000000, 21450000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //                case FRSRegion.UK:
        //                    band_edge_list = new int[] { 472000, 479000, 1800000, 2000000, 3500000, 3800000,
        //                5258500, 5406500, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
        //                21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
        //                    break;
        //                case FRSRegion.India:
        //                    band_edge_list = new int[]{ 1810000, 1860000, 3500000, 3900000, 7000000, 7200000, 
        //                10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
        //                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //                case FRSRegion.Norway:
        //                    band_edge_list = new int[]{ 1800000, 2000000, 3500000, 4000000, 5260000, 5410000,
        //                7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
        //                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //                case FRSRegion.US:
        //                case FRSRegion.Extended:
        //                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
        //                7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
        //                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //                case FRSRegion.Japan:
        //                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1810000, 1907500, 1912500, 
        //                3500000, 3575000, 3599000, 3612000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
        //                7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
        //                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //                default:
        //                    band_edge_list = new int[]{1800000, 2000000, 3500000, 4000000, 7000000, 7300000, 
        //                10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
        //                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
        //                    break;
        //            }

        //            bool first = true;
        //            //  #if false
        //            loop++;
        //            if (bottom)
        //            {
        //               VerLines_bottom_vb.Lock(loop * 40, vertexsize4x, LockFlags.None).WriteRange(new[] 
        //                {    // clear first!
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                 }); 
        //            }
        //            else
        //            {
        //                VerLines_vb.Lock(loop * 40, vertexsize4x, LockFlags.None).WriteRange(new[] 
        //                {    // clear first!
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                        new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
        //                });
        //            }
        //            //#endif

        //            //#if false
        //            for (int i = 0; i < band_edge_list.Length; i++)
        //            {
        //                double band_edge_offset = band_edge_list[i] - vfo;

        //                if (band_edge_offset >= low && band_edge_offset <= high)
        //                {
        //                    int temp_vline = (int)((double)(band_edge_offset - low) / (high - low) * W);//wa6ahl

        //                    if (first)
        //                    {
        //                        if (bottom)
        //                        {
        //                            VerLines_bottom_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                            {
        //                               new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H + top_size, 0.0f, 0.0f) },
        //                               new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H + H, 0.0f, 0.0f) },
        //                            });
        //                            VerLines_bottom_vb.Unlock();

        //                            RenderVerticalLine(device, temp_vline, H + top_size, temp_vline, H + H, band_edge_color);
        //                        }
        //                        else
        //                        {
        //                            VerLines_vb.Lock(loop * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                            {
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)top_size, 0.0f, 0.0f) },
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H, 0.0f, 0.0f) },
        //                            });
        //                            VerLines_vb.Unlock();

        //                            RenderVerticalLine(device, temp_vline, top_size, temp_vline, H, band_edge_color);
        //                        }
        //                        first = false;
        //                    }
        //                    else
        //                    {
        //                        if (bottom)
        //                        {
        //                            VerLines_bottom_vb.Lock(loop + 1 * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                            {
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H + top_size, 0.0f, 0.0f) },
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H + H, 0.0f, 0.0f) },
        //                            });
        //                            VerLines_bottom_vb.Unlock();

        //                            RenderVerticalLine(device, temp_vline, H + top_size, temp_vline, H + H, band_edge_color);
        //                        }
        //                        else
        //                        {
        //                            VerLines_vb.Lock(loop + 1 * 40, vertexsize2x, LockFlags.None).WriteRange(new[] 
        //                            {
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)top_size, 0.0f, 0.0f) },
        //                                new Vertex() { Color = band_edge_color.ToArgb(), Position = new Vector4((float)temp_vline, (float)H, 0.0f, 0.0f) },
        //                            });
        //                            VerLines_vb.Unlock();

        //                            RenderVerticalLine(device, temp_vline, top_size, temp_vline, H, band_edge_color);
        //                        }
        //                    }

        //                }
        //            }
        //            //#endif

        //            //#if false
        //            // Draw horizontal lines
        //            for (int i = 1; i < h_steps; i++)
        //            {
        //                int xOffset = 0;
        //                int num = grid_max - i * grid_step;
        //                int y = (int)((double)(grid_max - num) * H / y_range); // +(int)pan_font.Size;
        //                // if (show_horizontal_grid)
        //                // {
        //                if (bottom)
        //                {
        //                    HorLines_bottom_vb.Lock(i * 40, vertexsize2x, LockFlags.None).WriteRange(new[] {
        //                        new Vertex() { Color = hgrid_color.ToArgb(), Position = new Vector4(0.0f, (float)H + y, 0.0f, 0.0f) },
        //                        new Vertex() { Color = hgrid_color.ToArgb(), Position = new Vector4((float)W, (float)H + y, 0.0f, 0.0f) },
        //                    });
        //                    HorLines_bottom_vb.Unlock();

        //                    RenderHorizontalLine(device, 0, H + y, hgrid_color);
        //                }
        //                else
        //                {
        //                    HorLines_vb.Lock(i * 40, vertexsize2x, LockFlags.None).WriteRange(new[] {
        //                        new Vertex() { Color = hgrid_color.ToArgb(), Position = new Vector4(0.0f, (float)y, 0.0f, 0.0f) },
        //                        new Vertex() { Color = hgrid_color.ToArgb(), Position = new Vector4((float)W, (float)y, 0.0f, 0.0f) },
        //                    });
        //                    HorLines_vb.Unlock();

        //                    RenderHorizontalLine(device, 0, y, hgrid_color);
        //                }
        //                // }

        //                // Draw horizontal line labels
        //                num = grid_max - i * spectrum_grid_step;
        //                if (bottom)
        //                {
        //                    horizontal_bottom_label[i].label = num.ToString();
        //                    if (horizontal_bottom_label[i].label.Length == 3) xOffset = 5;
        //                }
        //                else
        //                {
        //                    horizontal_label[i].label = num.ToString();
        //                    if (horizontal_label[i].label.Length == 3) xOffset = 5;
        //                }

        //                //int offset = (int)(label.Length*4.1);
        //                if (display_label_align != DisplayLabelAlignment.LEFT &&
        //                    display_label_align != DisplayLabelAlignment.AUTO &&
        //                    (rx1_dsp_mode == DSPMode.USB ||
        //                    rx1_dsp_mode == DSPMode.CWU))
        //                    xOffset -= 32;
        //                float size = pan_font.Size * 3;

        //                int x = 0;
        //                switch (display_label_align)
        //                {
        //                    case DisplayLabelAlignment.LEFT:
        //                        x = xOffset + 3;
        //                        break;
        //                    case DisplayLabelAlignment.CENTER:
        //                        x = center_line_x + xOffset;
        //                        break;
        //                    case DisplayLabelAlignment.RIGHT:
        //                        x = (int)(W - size);
        //                        break;
        //                    case DisplayLabelAlignment.AUTO:
        //                        x = xOffset + 3;
        //                        break;
        //                    case DisplayLabelAlignment.OFF:
        //                        x = W;
        //                        break;
        //                }

        //                console.DisplayGridX = x;
        //                console.DisplayGridW = (int)(x + size);
        //                y -= 8;
        //                if (y + 9 < H)
        //                {
        //                    if (bottom)
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_bottom_label[i].label, x, y + H, grid_text_color.ToArgb());
        //                        horizontal_bottom_label[i].pos_x = x;
        //                        horizontal_bottom_label[i].pos_y = y + H;
        //                        horizontal_bottom_label[i].color = grid_text_color;
        //                    }
        //                    else
        //                    {
        //                        panadapter_font.DrawString(null, horizontal_label[i].label, x, y, grid_text_color.ToArgb());
        //                        horizontal_label[i].pos_x = x;
        //                        horizontal_label[i].pos_y = y;
        //                        horizontal_label[i].color = grid_text_color;
        //                    }
        //                }
        //            }
        //            //#endif

        //            // int bw_resolution = freq_step_size / 5;
        //            // bwres = bw_resolution.ToString();
        //            // bw_step_size = (rx_display_bw / (float)W);
        //        }

        //        /*  private static int waterfall_update_period = 100; // in ms
        //          public static int WaterfallUpdatePeriod
        //          {
        //              get { return waterfall_update_period; }
        //              set { waterfall_update_period = value; }
        //          } */

        //       // private static HiPerfTimer timer_waterfall = new HiPerfTimer();
        //       // private static HiPerfTimer timer_waterfall2 = new HiPerfTimer();
        //        //private static float[] waterfall_data;
        //        unsafe static public bool ConvertDataForWaterfall(int W, int H, int rx, bool bottom)    // yt7pwr
        //        {
        //            // if (current_display_mode == DisplayMode.WATERFALL)
        //            //  RenderWaterfallGrid(ref g, W, H);
        //            if (waterfall_data == null || waterfall_data.Length < W)
        //                waterfall_data = new float[W];			                    // array of points to display
        //          //  float slope = 0.0F;						                        // samples to process per pixel
        //          //  int num_samples = 0;					                        // number of samples to process
        //           // int start_sample_index = 0;				                        // index to begin looking at samples
        //            int low = 0;
        //            int high = 0;
        //            low = rx_display_low;
        //            high = rx_display_high;
        //            max_y = Int32.MinValue;
        //            int R = 0, G = 0, B = 0;	                                	// variables to save Red, Green and Blue component values

        //            if (console.PowerOn)
        //            {
        //                int yRange = spectrum_grid_max - spectrum_grid_min;

        //                if (waterfall_data_ready && !mox)
        //                {
        //                    if (console.TUN || mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //                    {
        //                        for (int i = 0; i < current_waterfall_data.Length; i++)
        //                            current_waterfall_data[i] = -200.0f;
        //                    }
        //                    else
        //                    {
        //                        // get new data
        //                        // fixed (void* rptr = &new_waterfall_data[0])
        //                        //fixed (void* wptr = &current_waterfall_data[0])
        //                        // Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //                        Array.Copy(new_waterfall_data, current_waterfall_data, current_waterfall_data.Length);
        //                        waterfall_data_ready = false;
        //                    }
        //                }

        //                // if (average_on)
        //                //  console.UpdateDirectXDisplayWaterfallAverage();
        //                // if (peak_on)
        //                // UpdateDisplayPeak();

        //                // timer_waterfall.Stop();
        //                // if (timer_waterfall.DurationMsec > waterfall_update_period)
        //                {
        //                    // timer_waterfall.Start();
        //                  //  num_samples = (high - low);

        //                  //  start_sample_index = (BUFFER_SIZE >> 1) + (int)((low * BUFFER_SIZE) / sample_rate);
        //                  //  num_samples = (int)((high - low) * BUFFER_SIZE / sample_rate);
        //                  //  start_sample_index = (start_sample_index + 4096) % 4096;
        //                  //  if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
        //                      //  num_samples = BUFFER_SIZE - start_sample_index;

        //                   // slope = (float)num_samples / (float)W;
        //                    for (int i = 0; i < W; i++)
        //                    {
        //                        float max = float.MinValue;
        //                       // float dval = i * slope + start_sample_index;
        //                       // int lindex = (int)Math.Floor(dval);
        //                       // int rindex = (int)Math.Floor(dval + slope);

        //                        max = current_waterfall_data[i];

        //                     /*   if (slope <= 1 || lindex == rindex)
        //                            max = current_waterfall_data[lindex] * ((float)lindex - dval + 1) +
        //                                current_waterfall_data[(lindex + 1) % 4096] * (dval - (float)lindex);
        //                        else
        //                        {
        //                            for (int j = lindex; j < rindex; j++)
        //                                if (current_waterfall_data[j % 4096] > max) max = current_waterfall_data[j % 4096];
        //                        } */

        //                        max += rx1_display_cal_offset;
        //                        if (!mox) max += rx1_preamp_offset;

        //                        if (max > max_y)
        //                        {
        //                            max_y = max;
        //                            max_x = i;
        //                        }

        //                        waterfall_data[i] = max;
        //                    }

        //                    int pixel_size = 4;
        //                    int row = 0;

        //                    if (!mox)
        //                    {
        //                        if (reverse_waterfall)
        //                        {
        //                            // first scroll image up
        //                            Array.Copy(waterfall_memory, waterfall_bmp_stride, waterfall_memory, 0,
        //                                waterfall_bmp_size - waterfall_bmp_stride);
        //                            row = (waterfall_bmp_size - waterfall_bmp_stride);
        //                        }
        //                        else
        //                        {
        //                            // first scroll image down
        //                            Array.Copy(waterfall_memory, 0, waterfall_memory, waterfall_bmp_stride,
        //                                waterfall_bmp_size - waterfall_bmp_stride);
        //                        }

        //                        int i = 0;
        //                        switch (color_sheme)
        //                        {
        //                            case (ColorSheme.original):                        // tre color only
        //                                {
        //                                    // draw new data
        //                                    for (i = 0; i < W; i++)	// for each pixel in the new line
        //                                    {
        //                                        if (waterfall_data[i] <= waterfall_low_threshold)		// if less than low threshold, just use low color
        //                                        {
        //                                            R = WaterfallLowColor.R;
        //                                            G = WaterfallLowColor.G;
        //                                            B = WaterfallLowColor.B;
        //                                        }
        //                                        else if (waterfall_data[i] >= WaterfallHighThreshold)// if more than high threshold, just use high color
        //                                        {
        //                                            R = WaterfallHighColor.R;
        //                                            G = WaterfallHighColor.G;
        //                                            B = WaterfallHighColor.B;
        //                                        }
        //                                        else // use a color between high and low
        //                                        {
        //                                            float percent = (waterfall_data[i] - waterfall_low_threshold) / (WaterfallHighThreshold - waterfall_low_threshold);
        //                                            if (percent <= 0.5)	// use a gradient between low and mid colors
        //                                            {
        //                                                percent *= 2;

        //                                                R = (int)((1 - percent) * WaterfallLowColor.R + percent * WaterfallMidColor.R);
        //                                                G = (int)((1 - percent) * WaterfallLowColor.G + percent * WaterfallMidColor.G);
        //                                                B = (int)((1 - percent) * WaterfallLowColor.B + percent * WaterfallMidColor.B);
        //                                            }
        //                                            else				// use a gradient between mid and high colors
        //                                            {
        //                                                percent = (float)(percent - 0.5) * 2;

        //                                                R = (int)((1 - percent) * WaterfallMidColor.R + percent * WaterfallHighColor.R);
        //                                                G = (int)((1 - percent) * WaterfallMidColor.G + percent * WaterfallHighColor.G);
        //                                                B = (int)((1 - percent) * WaterfallMidColor.B + percent * WaterfallHighColor.B);
        //                                            }
        //                                        }

        //                                        // set pixel color
        //                                        waterfall_memory[((i * pixel_size) + 3) + row] = (byte)waterfall_alpha;
        //                                        waterfall_memory[((i * pixel_size) + 2) + row] = (byte)R;
        //                                        waterfall_memory[((i * pixel_size) + 1) + row] = (byte)G;
        //                                        waterfall_memory[((i * pixel_size) + 0) + row] = (byte)B;
        //                                    }
        //                                }
        //                                break;

        //                            case (ColorSheme.enhanced): // SV1EIO
        //                                {
        //                                    // draw new data
        //                                    for (i = 0; i < W; i++)	// for each pixel in the new line
        //                                    {
        //                                        if (waterfall_data[i] <= waterfall_low_threshold)
        //                                        {
        //                                            R = WaterfallLowColor.R;
        //                                            G = WaterfallLowColor.G;
        //                                            B = WaterfallLowColor.B;
        //                                        }
        //                                        else if (waterfall_data[i] >= WaterfallHighThreshold)
        //                                        {
        //                                            R = 192;
        //                                            G = 124;
        //                                            B = 255;
        //                                        }
        //                                        else // value is between low and high
        //                                        {
        //                                            // float range = WaterfallHighThreshold - waterfall_low_threshold;
        //                                            // float offset = waterfall_data[i] - waterfall_low_threshold;
        //                                            // float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
        //                                            float overall_percent = (waterfall_data[i] - waterfall_low_threshold) /
        //                                                (WaterfallHighThreshold - waterfall_low_threshold);

        //                                            if (overall_percent < (float)2 / 9) // background to blue
        //                                            {
        //                                                float local_percent = overall_percent / ((float)2 / 9);
        //                                                R = (int)((1.0 - local_percent) * WaterfallLowColor.R);
        //                                                G = (int)((1.0 - local_percent) * WaterfallLowColor.G);
        //                                                B = (int)(WaterfallLowColor.B + local_percent * (255 - WaterfallLowColor.B));
        //                                            }
        //                                            else if (overall_percent < (float)3 / 9) // blue to blue-green
        //                                            {
        //                                                float local_percent = (overall_percent - (float)2 / 9) / ((float)1 / 9);
        //                                                R = 0;
        //                                                G = (int)(local_percent * 255);
        //                                                B = 255;
        //                                            }
        //                                            else if (overall_percent < (float)4 / 9) // blue-green to green
        //                                            {
        //                                                float local_percent = (overall_percent - (float)3 / 9) / ((float)1 / 9);
        //                                                R = 0;
        //                                                G = 255;
        //                                                B = (int)((1.0 - local_percent) * 255);
        //                                            }
        //                                            else if (overall_percent < (float)5 / 9) // green to red-green
        //                                            {
        //                                                float local_percent = (overall_percent - (float)4 / 9) / ((float)1 / 9);
        //                                                R = (int)(local_percent * 255);
        //                                                G = 255;
        //                                                B = 0;
        //                                            }
        //                                            else if (overall_percent < (float)7 / 9) // red-green to red
        //                                            {
        //                                                float local_percent = (overall_percent - (float)5 / 9) / ((float)2 / 9);
        //                                                R = 255;
        //                                                G = (int)((1.0 - local_percent) * 255);
        //                                                B = 0;
        //                                            }
        //                                            else if (overall_percent < (float)8 / 9) // red to red-blue
        //                                            {
        //                                                float local_percent = (overall_percent - (float)7 / 9) / ((float)1 / 9);
        //                                                R = 255;
        //                                                G = 0;
        //                                                B = (int)(local_percent * 255);
        //                                            }
        //                                            else // red-blue to purple end
        //                                            {
        //                                                float local_percent = (overall_percent - (float)8 / 9) / ((float)1 / 9);
        //                                                R = (int)((0.75 + 0.25 * (1.0 - local_percent)) * 255);
        //                                                G = (int)(local_percent * 255 * 0.5);
        //                                                B = 255;
        //                                            }
        //                                        }

        //                                        // set pixel color
        //                                        waterfall_memory[((i * pixel_size) + 3) + row] = (byte)waterfall_alpha;
        //                                        waterfall_memory[((i * pixel_size) + 2) + row] = (byte)R;
        //                                        waterfall_memory[((i * pixel_size) + 1) + row] = (byte)G;
        //                                        waterfall_memory[((i * pixel_size) + 0) + row] = (byte)B;
        //                                    }
        //                                }
        //                                break;

        //                            case (ColorSheme.SPECTRAN):
        //                                {
        //                                    // draw new data
        //                                    for (i = 0; i < W; i++)	// for each pixel in the new line
        //                                    {
        //                                        if (waterfall_data[i] <= waterfall_low_threshold)
        //                                        {
        //                                            R = 0;
        //                                            G = 0;
        //                                            B = 0;
        //                                        }
        //                                        else if (waterfall_data[i] >= WaterfallHighThreshold) // white
        //                                        {
        //                                            R = 240;
        //                                            G = 240;
        //                                            B = 240;
        //                                        }
        //                                        else // value is between low and high
        //                                        {
        //                                            float range = WaterfallHighThreshold - waterfall_low_threshold;
        //                                            float offset = waterfall_data[i] - waterfall_low_threshold;
        //                                            float local_percent = ((100.0f * offset) / range);

        //                                            if (local_percent < 5.0f)
        //                                            {
        //                                                R = G = 0;
        //                                                B = (int)local_percent * 5;
        //                                            }
        //                                            else if (local_percent < 11.0f)
        //                                            {
        //                                                R = G = 0;
        //                                                B = (int)local_percent * 5;
        //                                            }
        //                                            else if (local_percent < 22.0f)
        //                                            {
        //                                                R = G = 0;
        //                                                B = (int)local_percent * 5;
        //                                            }
        //                                            else if (local_percent < 44.0f)
        //                                            {
        //                                                R = G = 0;
        //                                                B = (int)local_percent * 5;
        //                                            }
        //                                            else if (local_percent < 51.0f)
        //                                            {
        //                                                R = G = 0;
        //                                                B = (int)local_percent * 5;
        //                                            }
        //                                            else if (local_percent < 66.0f)
        //                                            {
        //                                                R = G = (int)(local_percent - 50) * 2;
        //                                                B = 255;
        //                                            }
        //                                            else if (local_percent < 77.0f)
        //                                            {
        //                                                R = G = (int)(local_percent - 50) * 3;
        //                                                B = 255;
        //                                            }
        //                                            else if (local_percent < 88.0f)
        //                                            {
        //                                                R = G = (int)(local_percent - 50) * 4;
        //                                                B = 255;
        //                                            }
        //                                            else if (local_percent < 99.0f)
        //                                            {
        //                                                R = G = (int)(local_percent - 50) * 5;
        //                                                B = 255;
        //                                            }
        //                                        }

        //                                        // set pixel color
        //                                        waterfall_memory[((i * pixel_size) + 3) + row] = (byte)waterfall_alpha;
        //                                        waterfall_memory[((i * pixel_size) + 2) + row] = (byte)R;
        //                                        waterfall_memory[((i * pixel_size) + 1) + row] = (byte)G;
        //                                        waterfall_memory[((i * pixel_size) + 0) + row] = (byte)B;
        //                                    }
        //                                }
        //                                break;

        //                            case (ColorSheme.BLACKWHITE):
        //                                {
        //                                    // draw new data
        //                                    for (i = 0; i < W; i++)	// for each pixel in the new line
        //                                    {
        //                                        if (waterfall_data[i] <= waterfall_low_threshold)
        //                                        {
        //                                            B = 0;
        //                                        }
        //                                        else if (waterfall_data[i] >= WaterfallHighThreshold) // white
        //                                        {
        //                                            B = 255;
        //                                        }
        //                                        else // value is between low and high
        //                                        {
        //                                            float range = WaterfallHighThreshold - waterfall_low_threshold;
        //                                            float offset = waterfall_data[i] - waterfall_low_threshold;
        //                                            float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
        //                                            float local_percent = ((100.0f * offset) / range);
        //                                            float contrast = (console.SetupForm.DisplayContrast / 100);
        //                                            B = (int)((local_percent / 100) * 255);
        //                                        }

        //                                        // set pixel color
        //                                        waterfall_memory[((i * pixel_size) + 3) + row] = (byte)waterfall_alpha;
        //                                        waterfall_memory[((i * pixel_size) + 2) + row] = (byte)B;
        //                                        waterfall_memory[((i * pixel_size) + 1) + row] = (byte)B;
        //                                        waterfall_memory[((i * pixel_size) + 0) + row] = (byte)B;
        //                                    }
        //                                    break;
        //                                }
        //                        }
        //                    }
        //                }
        //            }
        //            return true;
        //        }

        unsafe private static void DrawAGCLines(int W)
        {
            /*   int Low = rx_display_low;					// initialize variables
               int High = rx_display_high;
               int filter_low, filter_high;

               if (mox) // get filter limits
               {
                   filter_low = tx_filter_low;
                   filter_high = tx_filter_high;
               }
               else //if(rx == 1)
               {
                   filter_low = rx1_filter_low;
                   filter_high = rx1_filter_high;
               }
               //else //if(rx == 2)
               //{
               //filter_low = rx2_filter_low;
               //filter_high = rx2_filter_high;
               //}

               //if((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
               //	(rx2_dsp_mode == DSPMode.DRM && rx == 2))
               //{
               //filter_low = -5000;
               //filter_high = 5000;
               //}

               if (console.PowerOn)
               {
                   // get filter screen coordinates
                   int filter_left_x = (int)((float)(filter_low - Low) / (High - Low) * W);
                   int filter_right_x = (int)((float)(filter_high - Low) / (High - Low) * W);
                   int x1_gain, x2_gain, x3_gain, x1_hang, x2_hang, x3_hang;
                   if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                   if (display_agc_gain_spectrum_line)
                   {
                       x1_gain = 40;
                       x2_gain = W - 40;
                       x3_gain = 50;
                   }
                   else
                   {
                       x1_gain = filter_left_x;
                       x2_gain = filter_right_x;
                       x3_gain = x1_gain;
                   }

                   if (display_agc_hang_spectrum_line)
                   {
                       x1_hang = 40;
                       x2_hang = W - 40;
                       x3_hang = 50;
                   }
                   else
                   {
                       x1_hang = filter_left_x;
                       x2_hang = filter_right_x;
                       x3_hang = x1_hang;
                   }

                   float cal_offset = 0.0f;
                   switch (console.RX1AGCMode)
                   {
                       case AGCMode.FIXD:
                           cal_offset = -18.0f;
                           break;
                       default:
                           cal_offset = 106.0f + (rx1_display_cal_offset +
                               (rx1_preamp_offset - alex_preamp_offset));
                           break;
                   }
                   // get AGC-T level
                   double thresh = 0.0; ;
                   float agcknee_y_value = 0.0f;
                   double hang;
                   int agc_hang_y = 0;
                   int agc_fixed_gain = console.SetupForm.AGCFixedGain;
                   string agc = "";

                   // get Hang Threshold level
                   DttSP.GetRXAGCHangLevel(0, 0, &hang);

                   DttSP.GetRXAGCThresh(0, 0, &thresh);
                   //thresh = Math.Round(thresh);
                   //Debug.WriteLine("thresh:" + thresh);

                   switch (console.RX1AGCMode)
                   {
                       case AGCMode.FIXD:
                           agcknee_y_value = dBToPixel(-(float)agc_fixed_gain + cal_offset);
                           // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                           agc = "-F";
                           break;
                       default:
                           agcknee_y_value = dBToPixel((float)thresh + cal_offset);

                           // if (display_agc_hang_line)
                           if (display_agc_hang_line && console.RX1AGCMode != AGCMode.MED && console.RX1AGCMode != AGCMode.FAST)
                           {
                               agc_hang_y = (int)dBToPixel((float)hang + cal_offset);

                               Hangrect.x1 = 40;// left bottom
                               Hangrect.y1 = agc_hang_y;
                               Hangrect.x2 = 40;// left top
                               Hangrect.y2 = agc_hang_y - 8;
                               Hangrect.x3 = 48;// right bottom
                               Hangrect.y3 = agc_hang_y;

                               Hangrect.x4 = 48;// right top
                               Hangrect.y4 = agc_hang_y - 8;
                               RenderRectangle(device, Hangrect, Color.Yellow);

                               RenderHorizontalLine(device, x1_hang, agc_hang_y, x2_hang, agc_hang_y, Color.Yellow);
                           }
                           agc = "-G";
                           break;
                   }

                   if (display_agc_gain_line)
                   {
                       Gainrect.x1 = 40;// left bottom
                       Gainrect.y1 = (int)agcknee_y_value;
                       Gainrect.x2 = 40;// left top
                       Gainrect.y2 = (int)agcknee_y_value - 8;
                       Gainrect.x3 = 48;// right bottom
                       Gainrect.y3 = (int)agcknee_y_value;

                       Gainrect.x4 = 48;// right top
                       Gainrect.y4 = (int)agcknee_y_value - 8;
                       RenderRectangle(device, Gainrect, Color.YellowGreen);

                       RenderHorizontalLine(device, x1_gain, (int)agcknee_y_value, x2_gain, (int)agcknee_y_value, Color.YellowGreen);
                   }
               }*/
        }

        //unsafe static private void ConvertDataForSpectrum(int W, int H, bool bottom)
        //{
        //    try
        //    {
        //        //float[] data = new float[W];			// array of points to display
        //        float slope = 0.0f;						// samples to process per pixel
        //        int num_samples = 0;					// number of samples to process
        //        int start_sample_index = 0;				// index to begin looking at samples
        //        int low = 0;
        //        int high = 0;
        //       // max_y = Int32.MinValue;
        //        float local_max_y = float.MinValue;
        //        int grid_max = 0;
        //        int grid_min = 0;
        //        int sample_rate;

        //        if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
        //        {
        //            low = rx_spectrum_display_low;
        //            high = rx_spectrum_display_high;
        //            grid_max = spectrum_grid_max;
        //            grid_min = spectrum_grid_min;
        //            sample_rate = sample_rate_rx1;
        //        }
        //        else
        //        {
        //            low = tx_spectrum_display_low;
        //            high = tx_spectrum_display_high;
        //            grid_max = tx_spectrum_grid_max;
        //            grid_min = tx_spectrum_grid_min;
        //            sample_rate = sample_rate_tx;
        //        }
        //        if (rx1_dsp_mode == DSPMode.DRM)
        //        {
        //            low = 2500;
        //            high = 21500;
        //        }

        //        int yRange = grid_max - grid_min;

        //        if (!bottom && data_ready)
        //        {
        //            if (mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //            {
        //                for (int i = 0; i < current_display_data.Length; i++)
        //                    current_display_data[i] = spectrum_grid_min - rx1_display_cal_offset;
        //                //current_display_data[i] = -200.0f;
        //            }
        //            else
        //            {
        //                 fixed (void* rptr = &new_display_data[0])
        //                 fixed (void* wptr = &current_display_data[0])
        //                   Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //                //Array.Copy(new_display_data, current_display_data, current_display_data.Length);
        //            }
        //            data_ready = false;
        //        }
        //        else if (bottom && data_ready_bottom)
        //        {
        //            /*if(mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
        //            {
        //                for(int i=0; i<current_display_data_bottom.Length; i++)
        //                    current_display_data_bottom[i] = -200.0f;
        //            }
        //            else */
        //            {
        //                 fixed (void* rptr = &new_display_data_bottom[0])
        //                 fixed (void* wptr = &current_display_data_bottom[0])
        //                  Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //               // Array.Copy(new_display_data_bottom, current_display_data_bottom, current_display_data_bottom.Length);
        //            }
        //            data_ready_bottom = false;
        //        }

        //      /*  if (!bottom && average_on)
        //            console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
        //        else if (bottom && rx2_avg_on)
        //            console.UpdateRX2DisplayAverage(rx2_average_buffer, current_display_data_bottom);

        //        if (!bottom && peak_on)
        //            UpdateDisplayPeak(rx1_peak_buffer, current_display_data);
        //        else if (bottom && rx2_peak_on)
        //            UpdateDisplayPeak(rx2_peak_buffer, current_display_data_bottom); */

        //        start_sample_index = (BUFFER_SIZE >> 1) + (int)((low * BUFFER_SIZE) / sample_rate);
        //        num_samples = (int)((high - low) * BUFFER_SIZE / sample_rate);

        //        if (start_sample_index < 0) start_sample_index = 0;
        //        if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
        //            num_samples = BUFFER_SIZE - start_sample_index;

        //        slope = (float)num_samples / (float)W;
        //        for (int i = 0; i < W; i++)
        //        {
        //            float max = float.MinValue;
        //            float dval = i * slope + start_sample_index;
        //            int lindex = (int)Math.Floor(dval);

        //            if (!bottom)
        //            {
        //                if (!mox)
        //                    max = current_display_data[i];
        //                else
        //                {
        //                    if (slope <= 1)
        //                        max = current_display_data[lindex] * ((float)lindex - dval + 1) + current_display_data[lindex + 1] * (dval - (float)lindex);
        //                    else
        //                    {
        //                        int rindex = (int)Math.Floor(dval + slope);
        //                        if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
        //                        for (int j = lindex; j < rindex; j++)
        //                            if (current_display_data[j] > max) max = current_display_data[j];
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (slope <= 1)
        //                    max = current_display_data_bottom[lindex] * ((float)lindex - dval + 1) + current_display_data_bottom[lindex + 1] * (dval - (float)lindex);
        //                else
        //                {
        //                    int rindex = (int)Math.Floor(dval + slope);
        //                    if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
        //                    for (int j = lindex; j < rindex; j++)
        //                        if (current_display_data_bottom[j] > max) max = current_display_data_bottom[j];
        //                }
        //            }

        //            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
        //            {
        //                if (!bottom) max += rx1_display_cal_offset;
        //                else max += rx2_display_cal_offset;
        //            }
        //            else
        //            {
        //                //if (!bottom) 
        //                max += tx_display_cal_offset;
        //            }

        //            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
        //            {
        //                if (!bottom) max += rx1_preamp_offset - alex_preamp_offset;
        //                else max += rx2_preamp_offset;
        //            }

        //            if (max > local_max_y)
        //            {
        //                local_max_y = max;
        //                max_x = i;
        //            }

        //            if (bottom) panadapterX_data_bottom[i] = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
        //            else panadapterX_data[i] = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex.ToString());
        //    }
        //}

        //unsafe static private bool ConvertDataForHistogram()
        //{
        //    try
        //    {
        //        if (points == null || points.Length < W)
        //            points = new Point[W];			// array of points to display

        //        float slope = 0.0F;					        	// samples to process per pixel
        //        int num_samples = 0;					// number of samples to process
        //        int start_sample_index = 0;				// index to begin looking at samples
        //        int low = 0;
        //        int high = 0;
        //        int sample_rate;
        //        max_y = Int32.MinValue;

        //        if (!mox)								        // Receive Mode
        //        {
        //            low = rx_display_low;
        //            high = rx_display_high;
        //            sample_rate = sample_rate_rx1;
        //        }
        //        else									        // Transmit Mode
        //        {
        //            low = tx_display_low;
        //            high = tx_display_high;
        //            sample_rate = sample_rate_tx;
        //        }

        //        int yRange = spectrum_grid_max - spectrum_grid_min;

        //        if (data_ready)
        //        {
        //            // get new data
        //            // fixed (void* rptr = &new_display_data[0])
        //            // fixed (void* wptr = &current_display_data[0])
        //            //  Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //            Array.Copy(new_display_data, current_display_data, current_display_data.Length);

        //            data_ready = false;
        //        }

        //        if (average_on)
        //            console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
        //        if (peak_on)
        //            UpdateDisplayPeak(rx1_peak_buffer, current_display_data);

        //        num_samples = (high - low);

        //        start_sample_index = (BUFFER_SIZE >> 1) + (int)((low * BUFFER_SIZE) / sample_rate);
        //        num_samples = (int)((high - low) * BUFFER_SIZE / sample_rate);
        //        if (start_sample_index < 0) start_sample_index = 0;
        //        if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
        //            num_samples = BUFFER_SIZE - start_sample_index;

        //        slope = (float)num_samples / (float)W;
        //        for (int i = 0; i < W; i++)
        //        {
        //            float max = float.MinValue;
        //            float dval = i * slope + start_sample_index;
        //            int lindex = (int)Math.Floor(dval);
        //            if (slope <= 1)
        //                max = current_display_data[lindex] * ((float)lindex - dval + 1) + current_display_data[lindex + 1] * (dval - (float)lindex);
        //            else
        //            {
        //                int rindex = (int)Math.Floor(dval + slope);
        //                if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
        //                for (int j = lindex; j < rindex; j++)
        //                    if (current_display_data[j] > max) max = current_display_data[j];

        //            }

        //            max += rx1_display_cal_offset;
        //            if (!mox) max += rx1_preamp_offset;

        //            switch (rx1_dsp_mode)
        //            {
        //                case DSPMode.SPEC:
        //                    max += 6.0F;
        //                    break;
        //            }
        //            if (max > max_y)
        //            {
        //                max_y = max;
        //                max_x = i;
        //            }

        //            points[i].X = i;
        //            points[i].Y = (int)(Math.Floor((spectrum_grid_max - max) * H / yRange));
        //        }

        //        // get the average
        //        float avg = 0.0F;
        //        int sum = 0;
        //        int k = 0;
        //        foreach (Point p in points)
        //            sum += p.Y;

        //        avg = (float)((float)sum / points.Length / 1.12);

        //        for (int i = 0; i < W; i++)
        //        {
        //            if (points[i].Y < histogram_data[i])
        //            {
        //                histogram_history[i] = 0;
        //                histogram_data[i] = points[i].Y;
        //            }
        //            else
        //            {
        //                histogram_history[i]++;
        //                if (histogram_history[i] > 51)
        //                {
        //                    histogram_history[i] = 0;
        //                    histogram_data[i] = points[i].Y;
        //                }

        //                int alpha = (int)Math.Max(255 - histogram_history[i] * 5, 0);
        //                Color color = Color.FromArgb(alpha, 0, 255, 0);
        //                int height = points[i].Y - histogram_data[i];
        //                histogram_verts[i].Y = histogram_data[i];
        //                histogram_verts[i].color = color;
        //            }

        //            if (points[i].Y >= avg)		// value is below the average
        //            {
        //                Color color = Color.FromArgb(150, 0, 0, 255);
        //                histogram_verts[W + k].Y = points[i].Y;
        //                histogram_verts[W + k].color = color;
        //                histogram_verts[W + k + 1].Y = points[i].Y;
        //                histogram_verts[W + k + 1].color = color;
        //            }
        //            else
        //            {
        //                Color color = Color.FromArgb(150, 0, 0, 255);
        //                histogram_verts[W + k].Y = points[i].Y;
        //                histogram_verts[W + k].color = color;
        //                color = Color.FromArgb(255, 255, 0, 0);
        //                histogram_verts[W + k + 1].Y = (int)Math.Floor(avg);
        //                histogram_verts[W + k + 1].color = color;
        //            }
        //            k += 2;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex.ToString());
        //        return false;
        //    }
        //}

        //unsafe private static void ConvertDataForPhase(int W, int H, bool bottom)
        //{
        //    try
        //    {
        //        int num_points = phase_num_pts;

        //        if (!bottom && data_ready)
        //        {
        //            // get new data
        //            // fixed (void* rptr = &new_display_data[0])
        //            // fixed (void* wptr = &current_display_data[0])
        //            // Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //            Array.Copy(new_display_data, current_display_data, current_display_data.Length);
        //            data_ready = false;
        //        }
        //        else if (bottom && data_ready_bottom)
        //        {
        //            // get new data
        //            // fixed (void* rptr = &new_display_data_bottom[0])
        //            //fixed (void* wptr = &current_display_data_bottom[0])
        //            //  Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //            Array.Copy(new_display_data_bottom, current_display_data_bottom, current_display_data_bottom.Length);
        //            data_ready_bottom = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex.ToString());
        //    }
        //}

        //unsafe private static void ConvertDataForScope(int W, int H, bool bottom)
        //{
        //    try
        //    {
        //        int i;

        //        if (data_ready)
        //        {
        //            // get new data
        //            // fixed (void* rptr = &new_display_data[0])
        //            // fixed (void* wptr = &current_display_data[0])
        //            // Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
        //            Array.Copy(new_display_data, current_display_data, current_display_data.Length);

        //            data_ready = false;
        //        }

        //        double num_samples = scope_time * 48;
        //        double slope = num_samples / (double)W;

        //        //float[] data = new float[W];				// create Point array
        //        for (i = 0; i < W; i++)						// fill point array
        //        {
        //            int pixels = (int)(H / 2 * current_display_data[(int)Math.Floor(i * slope)]);
        //            int y = H / 2 - pixels;
        //            if (y < max_y)
        //            {
        //                max_y = y;
        //                max_x = i;
        //            }

        //            panadapterX_data[i] = y;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex.ToString());
        //    }
        //}

        //// private static void ConvertDataForWaterfalll()
        //// {
        ////    if (device == null) return;
        ////  }

        //private static void RenderScope(int W, int H, bool bottom)
        //{
        //    // Add horizontal line
        //    RenderHorizontalLine(device, 0, H / 2, grid_color);

        //    // Add vertical line
        //    RenderVerticalLine(device, W / 2, H, grid_color);
        //}

        //private static void RenderSpectrum(int W, int H, bool bottom)
        //{
        //    System.Drawing.Font font = new System.Drawing.Font("Arial", 9);
        //    device.VertexFormat = VertexFormat.None;

        //    int low = 0;								// init limit variables
        //    int high = 0;
        //    int grid_max = 0;
        //    int grid_min = 0;
        //    int grid_step = 0; // spectrum_grid_step;

        //    if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
        //    {
        //        low = rx_spectrum_display_low;				// get RX display limits
        //        high = rx_spectrum_display_high;
        //    }
        //    else
        //    {
        //        if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
        //        {
        //            low = rx_spectrum_display_low;
        //            high = rx_spectrum_display_high;
        //        }
        //        else
        //        {
        //            low = tx_spectrum_display_low;			// get RX display limits
        //            high = tx_spectrum_display_high;
        //        }
        //    }

        //    int center_line_x = (int)(-(double)low / (high - low) * W);

        //    int mid_w = W / 2;
        //    int[] step_list = { 10, 20, 25, 50 };
        //    int step_power = 1;
        //    int step_index = 0;
        //    int freq_step_size = 50;

        //    if (!mox || (mox && tx_on_vfob && console.RX2Enabled))// || (mox && tx_on_vfob))
        //    {
        //        grid_max = spectrum_grid_max;
        //        grid_min = spectrum_grid_min;
        //        grid_step = spectrum_grid_step;
        //    }
        //    else if (mox)
        //    {
        //        grid_max = tx_spectrum_grid_max;
        //        grid_min = tx_spectrum_grid_min;
        //        grid_step = tx_spectrum_grid_step;
        //    }

        //    int y_range = grid_max - grid_min;
        //    if (split_display) grid_step *= 2;

        //    if (high == 0)
        //    {
        //        int f = -low;
        //        // Calculate horizontal step size
        //        while (f / freq_step_size > 7)
        //        {
        //            freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //            step_index = (step_index + 1) % 4;
        //            if (step_index == 0) step_power++;
        //        }
        //        float pixel_step_size = (float)(W * freq_step_size / f);

        //        int num_steps = f / freq_step_size;

        //        // Draw vertical lines
        //        for (int i = 1; i <= num_steps; i++)
        //        {
        //            int x = W - (int)Math.Floor(i * pixel_step_size);	// for negative numbers
        //            if (bottom)
        //                RenderVerticalLine(device, x, H + H, grid_color);
        //            else RenderVerticalLine(device, x, H, grid_color);

        //            // Draw vertical line labels
        //            int num = i * freq_step_size;
        //            string label = num.ToString();
        //            int offset = (int)((label.Length + 1) * 4.1);
        //            if (x - offset >= 0)
        //            {
        //                if (bottom) panadapter_font.DrawString(null, label, x - offset, H, grid_text_color.ToArgb());
        //                else panadapter_font.DrawString(null, label, x - offset, 0, grid_text_color.ToArgb());
        //            }
        //        }

        //        // Draw horizontal lines
        //        int V = (int)(grid_max - grid_min);
        //        num_steps = V / grid_step;
        //        pixel_step_size = H / num_steps;

        //        for (int i = 1; i < num_steps; i++)
        //        {
        //            int xOffset = 0;
        //            int num = grid_max - i * grid_step;
        //            int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);

        //            if (bottom) RenderHorizontalLine(device, 0, H + y, W, H + y, hgrid_color);
        //            else RenderHorizontalLine(device, 0, y, W, y, hgrid_color);

        //            // Draw horizontal line labels
        //            string label = num.ToString();
        //            int offset = (int)(label.Length * 4.1);
        //            if (label.Length == 3) xOffset = 7;
        //            float size = pan_font.Size * 3;
        //            y -= 8;
        //            int x = 0;
        //            switch (display_label_align)
        //            {
        //                case DisplayLabelAlignment.LEFT:
        //                    x = xOffset + 3;
        //                    break;
        //                case DisplayLabelAlignment.CENTER:
        //                    x = W / 2 + xOffset;
        //                    break;
        //                case DisplayLabelAlignment.RIGHT:
        //                case DisplayLabelAlignment.AUTO:
        //                    x = (int)(W - size);
        //                    break;
        //                case DisplayLabelAlignment.OFF:
        //                    x = W;
        //                    break;
        //            }

        //            if (y + 9 < H)
        //            {
        //                if (bottom) panadapter_font.DrawString(null, label, x, H + y, grid_text_color.ToArgb());
        //                else panadapter_font.DrawString(null, label, x, y, grid_text_color.ToArgb());
        //            }
        //        }

        //        // Draw middle vertical line
        //        if (rx1_dsp_mode == DSPMode.AM ||
        //            rx1_dsp_mode == DSPMode.SAM ||
        //            rx1_dsp_mode == DSPMode.FM ||
        //            rx1_dsp_mode == DSPMode.DSB ||
        //            rx1_dsp_mode == DSPMode.SPEC)
        //            if (bottom)
        //            {
        //                RenderVerticalLine(device, W - 1, H, W - 1, H + H, grid_zero_color);
        //                RenderVerticalLine(device, W - 2, H, W - 2, H + H, grid_zero_color);
        //            }
        //            else
        //            {
        //                RenderVerticalLine(device, W - 1, 0, W - 1, H, grid_zero_color);
        //                RenderVerticalLine(device, W - 2, 0, W - 2, H, grid_zero_color);
        //            }
        //    }

        //    else if (low == 0)
        //    {
        //        int f = high;
        //        // Calculate horizontal step size
        //        while (f / freq_step_size > 7)
        //        {
        //            freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //            step_index = (step_index + 1) % 4;
        //            if (step_index == 0) step_power++;
        //        }
        //        float pixel_step_size = (float)(W * freq_step_size / f);
        //        int num_steps = f / freq_step_size;

        //        // Draw vertical lines
        //        for (int i = 1; i <= num_steps; i++)
        //        {
        //            int x = (int)Math.Floor(i * pixel_step_size);// for positive numbers

        //            if (bottom) RenderVerticalLine(device, x, H, x, H + H, grid_color);
        //            else RenderVerticalLine(device, x, 0, x, H, grid_color);

        //            // Draw vertical line labels
        //            int num = i * freq_step_size;
        //            string label = num.ToString();
        //            int offset = (int)(label.Length * 4.1);
        //            if (x - offset + label.Length * 7 < W)
        //            {
        //                if (bottom) panadapter_font.DrawString(null, label, x - offset, H, grid_text_color.ToArgb());
        //                else panadapter_font.DrawString(null, label, x - offset, 0, grid_text_color.ToArgb());
        //            }
        //        }

        //        // Draw horizontal lines
        //        int V = (int)(spectrum_grid_max - spectrum_grid_min);
        //        int numSteps = V / spectrum_grid_step;
        //        pixel_step_size = H / numSteps;
        //        for (int i = 1; i < numSteps; i++)
        //        {
        //            int xOffset = 0;
        //            int num = spectrum_grid_max - i * spectrum_grid_step;
        //            int y = (int)Math.Floor((double)(spectrum_grid_max - num) * H / y_range);

        //            if (bottom) RenderHorizontalLine(device, 0, H + y, W, H + y, hgrid_color);
        //            else RenderHorizontalLine(device, 0, y, W, y, hgrid_color);

        //            // Draw horizontal line labels
        //            string label = num.ToString();
        //            if (label.Length == 3) xOffset = 7;
        //            float size = pan_font.Size * 3;

        //            int x = 0;
        //            switch (display_label_align)
        //            {
        //                case DisplayLabelAlignment.LEFT:
        //                case DisplayLabelAlignment.AUTO:
        //                    x = xOffset + 3;
        //                    break;
        //                case DisplayLabelAlignment.CENTER:
        //                    x = W / 2 + xOffset;
        //                    break;
        //                case DisplayLabelAlignment.RIGHT:
        //                    x = (int)(W - size);
        //                    break;
        //                case DisplayLabelAlignment.OFF:
        //                    x = W;
        //                    break;
        //            }

        //            y -= 8;
        //            if (y + 9 < H)
        //            {
        //                if (bottom) panadapter_font.DrawString(null, label, x, H + y, grid_text_color.ToArgb());
        //                else panadapter_font.DrawString(null, label, x, y, grid_text_color.ToArgb());
        //            }

        //            // Draw middle vertical line
        //            if (rx1_dsp_mode == DSPMode.AM ||
        //                rx1_dsp_mode == DSPMode.SAM ||
        //                rx1_dsp_mode == DSPMode.FM ||
        //                rx1_dsp_mode == DSPMode.DSB ||
        //                rx1_dsp_mode == DSPMode.SPEC)
        //                if (bottom)
        //                {
        //                    RenderVerticalLine(device, 0, H, 0, H + H, grid_zero_color);
        //                    RenderVerticalLine(device, 1, H, 1, H + H, grid_zero_color);
        //                }
        //                else
        //                {
        //                    RenderVerticalLine(device, 0, 0, 0, H, grid_zero_color);
        //                    RenderVerticalLine(device, 1, 0, 1, H, grid_zero_color);
        //                }
        //        }
        //    }
        //    if (low < 0 && high > 0)
        //    {
        //        int f = high;

        //        // Calculate horizontal step size
        //        while (f / freq_step_size > 4)
        //        {
        //            freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
        //            step_index = (step_index + 1) % 4;
        //            if (step_index == 0) step_power++;
        //        }
        //        int pixel_step_size = W / 2 * freq_step_size / f;
        //        int num_steps = f / freq_step_size;

        //        // Draw vertical lines
        //        for (int i = 1; i <= num_steps; i++)
        //        {
        //            int xLeft = mid_w - (i * pixel_step_size);	    // for negative numbers
        //            int xRight = mid_w + (i * pixel_step_size);		// for positive numbers
        //            if (bottom)
        //            {
        //                RenderVerticalLine(device, xLeft, H, xLeft, H + H, grid_color);
        //                RenderVerticalLine(device, xRight, H, xRight, H + H, grid_color);
        //            }
        //            else
        //            {
        //                RenderVerticalLine(device, xLeft, 0, xLeft, H, grid_color);
        //                RenderVerticalLine(device, xRight, 0, xRight, H, grid_color);
        //            }

        //            // Draw vertical line labels
        //            int num = i * freq_step_size;
        //            string label = num.ToString();
        //            int offsetL = (int)((label.Length + 1) * 4.1);
        //            int offsetR = (int)(label.Length * 4.1);
        //            if (xLeft - offsetL >= 0)
        //            {
        //                if (bottom)
        //                {
        //                    panadapter_font.DrawString(null, "-" + label, xLeft - offsetL, H, grid_text_color.ToArgb());
        //                    panadapter_font.DrawString(null, "-" + label, xRight - offsetR, H, grid_text_color.ToArgb());
        //                }
        //                else
        //                {
        //                    panadapter_font.DrawString(null, "-" + label, xLeft - offsetL, 0, grid_text_color.ToArgb());
        //                    panadapter_font.DrawString(null, "-" + label, xRight - offsetR, 0, grid_text_color.ToArgb());
        //                }
        //            }
        //        }

        //        // Draw horizontal lines
        //        int V = (int)(spectrum_grid_max - spectrum_grid_min);
        //        int numSteps = V / spectrum_grid_step;
        //        pixel_step_size = H / numSteps;
        //        for (int i = 1; i < numSteps; i++)
        //        {
        //            int xOffset = 0;
        //            int num = spectrum_grid_max - i * spectrum_grid_step;
        //            int y = (int)Math.Floor((double)(spectrum_grid_max - num) * H / y_range);

        //            if (bottom) RenderHorizontalLine(device, 0, H + y, W, H + y, hgrid_color);
        //            else RenderHorizontalLine(device, 0, y, W, y, hgrid_color);

        //            // Draw horizontal line labels
        //            string label = num.ToString();
        //            if (label.Length == 3) xOffset = 7;
        //            int offset = (int)(label.Length * 4.1);
        //            float size = pan_font.Size * 3;

        //            int x = 0;
        //            switch (display_label_align)
        //            {
        //                case DisplayLabelAlignment.LEFT:
        //                    x = xOffset + 3;
        //                    break;
        //                case DisplayLabelAlignment.CENTER:
        //                case DisplayLabelAlignment.AUTO:
        //                    x = W / 2 + xOffset;
        //                    break;
        //                case DisplayLabelAlignment.RIGHT:
        //                    x = (int)(W - size);
        //                    break;
        //                case DisplayLabelAlignment.OFF:
        //                    x = W;
        //                    break;
        //            }

        //            y -= 8;
        //            if (y + 9 < H)
        //            {
        //                if (bottom) panadapter_font.DrawString(null, label, x, H + y, grid_text_color.ToArgb());
        //                else panadapter_font.DrawString(null, label, x, y, grid_text_color.ToArgb());
        //            }
        //        }

        //        // Draw middle vertical line
        //        if (rx1_dsp_mode == DSPMode.AM ||
        //            rx1_dsp_mode == DSPMode.SAM ||
        //            rx1_dsp_mode == DSPMode.FM ||
        //            rx1_dsp_mode == DSPMode.DSB ||
        //            rx1_dsp_mode == DSPMode.SPEC)
        //            if (bottom)
        //            {
        //                RenderVerticalLine(device, mid_w, H, mid_w, H + H, grid_zero_color);
        //                RenderVerticalLine(device, mid_w - 1, H, mid_w - 1, H + H, grid_zero_color);
        //            }
        //            else
        //            {
        //                RenderVerticalLine(device, mid_w, 0, mid_w, H, grid_zero_color);
        //                RenderVerticalLine(device, mid_w - 1, 0, mid_w - 1, H, grid_zero_color);
        //            }
        //    }
        //}

        //private static void RenderHistogram(Device dev)     // yt7pwr
        //{
        //    int i = 0;
        //    int j = 0;
        //    int k = 0;

        //    try
        //    {
        //        for (i = 0; i < W * 2; i++)
        //        {
        //            HistogramLine_verts[i] = new Vertex();
        //            HistogramLine_verts[i].Color = histogram_verts[j].color.ToArgb();
        //            HistogramLine_verts[i].Position = new Vector4(j, points[j].Y, 0.0f, 0.0f);
        //            HistogramLine_verts[i + 1] = new Vertex();
        //            HistogramLine_verts[i + 1].Color = histogram_verts[j].color.ToArgb();
        //            HistogramLine_verts[i + 1].Position = new Vector4(j, histogram_data[j], 0.0f, 0.0f);
        //            HistogramLine_verts[i + 2] = new Vertex();
        //            HistogramLine_verts[i + 2].Color = histogram_verts[j].color.ToArgb();
        //            HistogramLine_verts[i + 2].Position = new Vector4(j + 1, points[j].Y, 0.0f, 0.0f);
        //            HistogramLine_verts[i + 3] = new Vertex();
        //            HistogramLine_verts[i + 3].Color = histogram_verts[j].color.ToArgb();
        //            HistogramLine_verts[i + 3].Position = new Vector4(j + 1, histogram_data[j], 0.0f, 0.0f);
        //            i += 3;
        //            j += 2;
        //        }

        //        Histogram_vb.Lock(0, 0, LockFlags.None).WriteRange(HistogramLine_verts, 0, W * 2);
        //        Histogram_vb.Unlock();

        //        dev.SetStreamSource(0, Histogram_vb, 0, 20);
        //        dev.DrawPrimitives(PrimitiveType.LineList, 0, W);

        //        j = 0;
        //        for (i = 0; i < W * 4; i++)
        //        {
        //            HistogramLine_verts[W + i] = new Vertex();
        //            HistogramLine_verts[W + i].Color = histogram_verts[W + j].color.ToArgb();
        //            HistogramLine_verts[W + i].Position = new Vector4(k, H, 0.0f, 0.0f);
        //            HistogramLine_verts[W + i + 1] = new Vertex();
        //            HistogramLine_verts[W + i + 1].Color = histogram_verts[W + j].color.ToArgb();
        //            HistogramLine_verts[W + i + 1].Position = new Vector4(k, histogram_verts[W + j].Y, 0.0f, 0.0f);
        //            HistogramLine_verts[W + i + 2] = new Vertex();
        //            HistogramLine_verts[W + i + 2].Color = histogram_verts[W + j + 1].color.ToArgb();
        //            HistogramLine_verts[W + i + 2].Position = new Vector4(k, histogram_verts[W + j].Y, 0.0f, 0.0f);
        //            HistogramLine_verts[W + i + 3] = new Vertex();
        //            HistogramLine_verts[W + i + 3].Color = histogram_verts[W + j + 1].color.ToArgb();
        //            HistogramLine_verts[W + i + 3].Position = new Vector4(k, histogram_verts[W + j + 1].Y, 0.0f, 0.0f);
        //            i += 3;
        //            j += 2;
        //            k++;
        //        }

        //        Histogram_vb.Lock(0, 0, LockFlags.None).WriteRange(HistogramLine_verts, W, W * 4);
        //        Histogram_vb.Unlock();

        //        dev.SetStreamSource(0, Histogram_vb, 0, 20);
        //        dev.DrawPrimitives(PrimitiveType.LineList, 0, W * 2);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex.ToString());
        //    }
        //}

        //private static void RenderPhase(Device dev, int W, int H, bool bottom)        // yt7pwr
        //{
        //    int x, y;

        //    for (int i = 0, j = 0; i < phase_num_pts; i++, j += 8)	// fill point array
        //    {
        //        if (bottom)
        //        {
        //            x = (int)(current_display_data_bottom[i * 2] * H / 2);
        //            y = (int)(current_display_data_bottom[i * 2 + 1] * H / 2);
        //        }
        //        else
        //        {
        //            x = (int)(current_display_data[i * 2] * H / 2);
        //            y = (int)(current_display_data[i * 2 + 1] * H / 2);
        //        }
        //        Phase_verts[i] = new Vertex();
        //        Phase_verts[i].Color = data_line_color.ToArgb();
        //        Phase_verts[i].Position = new Vector4(W / 2 + x, H / 2 + y, 0.0f, 0.0f);
        //        if (bottom) Phase_verts[i].Position = new Vector4(W / 2 + x, H + H / 2 + y, 0.0f, 0.0f);
        //    }

        //    Phase_vb.Lock(0, 0, LockFlags.None).WriteRange(Phase_verts, 0, phase_num_pts);
        //    Phase_vb.Unlock();

        //    dev.SetStreamSource(0, Phase_vb, 0, 20);
        //    dev.DrawPrimitives(PrimitiveType.LineStrip, 0, phase_num_pts - 1);
        //}

        //private static void RenderPhase2(Device dev)        // yt7pwr
        //{
        //    int x, y;

        //    for (int i = 0, j = 0; i < phase_num_pts; i++, j += 8)	// fill point array
        //    {
        //        x = (int)(current_display_data[i * 2] * H * 0.5 * 500);
        //        y = (int)(current_display_data[i * 2 + 1] * H * 0.5 * 500);
        //        Phase_verts[i] = new Vertex();
        //        Phase_verts[i].Color = data_line_color.ToArgb();
        //        Phase_verts[i].Position = new Vector4(W * 0.5f + x, H * 0.5f + y, 0.0f, 0.0f);
        //    }

        //    Phase_vb.Lock(0, 0, LockFlags.None).WriteRange(Phase_verts, 0, phase_num_pts);
        //    Phase_vb.Unlock();

        //    dev.SetStreamSource(0, Phase_vb, 0, 20);
        //    dev.DrawPrimitives(PrimitiveType.LineStrip, 0, phase_num_pts - 1);
        //}


        #region GDI+ General Routines

        public static void Init()
        {
            histogram_data = new int[W];
            histogram_history = new int[W];
            for (int i = 0; i < W; i++)
            {
                histogram_data[i] = Int32.MaxValue;
                histogram_history[i] = 0;
            }

            //display_bmp = new Bitmap(W, H);
            //display_graphics = Graphics.FromImage(display_bmp);
            waterfall_bmp = new Bitmap(W, H - 20, PixelFormat.Format24bppRgb);	// initialize waterfall display
            waterfall_bmp2 = new Bitmap(W, H - 20, PixelFormat.Format24bppRgb);

            rx1_average_buffer = new float[BUFFER_SIZE];	// initialize averaging buffer array
            rx1_average_buffer[0] = CLEAR_FLAG;		// set the clear flag

            rx2_average_buffer = new float[BUFFER_SIZE];	// initialize averaging buffer array
            rx2_average_buffer[0] = CLEAR_FLAG;		// set the clear flag

            rx1_peak_buffer = new float[BUFFER_SIZE];
            rx1_peak_buffer[0] = CLEAR_FLAG;

            rx2_peak_buffer = new float[BUFFER_SIZE];
            rx2_peak_buffer[0] = CLEAR_FLAG;

            //background_image_mutex = new Mutex(false);

            new_display_data = new float[BUFFER_SIZE];
            current_display_data = new float[BUFFER_SIZE];
            new_display_data_bottom = new float[BUFFER_SIZE];
            current_display_data_bottom = new float[BUFFER_SIZE];

            new_waterfall_data = new float[BUFFER_SIZE];
            current_waterfall_data = new float[BUFFER_SIZE];
            new_waterfall_data_bottom = new float[BUFFER_SIZE];
            current_waterfall_data_bottom = new float[BUFFER_SIZE];

            // waterfall_display_data = new float[BUFFER_SIZE];
            // average_waterfall_buffer = new float[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                new_display_data[i] = -200.0f;
                current_display_data[i] = -200.0f;
                new_display_data_bottom[i] = -200.0f;
                current_display_data_bottom[i] = -200.0f;
                new_waterfall_data[i] = -200.0f;
                current_waterfall_data[i] = -200.0f;
                new_waterfall_data_bottom[i] = -200.0f;
                current_waterfall_data_bottom[i] = -200.0f;
                // waterfall_display_data[i] = -200.0f;
                // average_waterfall_buffer[i] = -200.0f;
            }
        }

        public static void DrawBackground()
        {
            // draws the background image for the display based
            // on the current selected display mode.

            if (current_display_engine == DisplayEngine.GDI_PLUS)
            {
                /*switch(current_display_mode)
                {
                    case DisplayMode.SPECTRUM:
                        DrawSpectrumGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.PANADAPTER:
                        DrawPanadapterGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.SCOPE:
                        DrawScopeGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.PHASE:
                        DrawPhaseGrid(ref background_bmp, W, H);
                        break;	
                    case DisplayMode.PHASE2:
                        DrawPhaseGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.WATERFALL:
                        DrawWaterfallGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.HISTOGRAM:
                        DrawSpectrumGrid(ref background_bmp, W, H);
                        break;
                    case DisplayMode.OFF:
                        DrawOffBackground(ref background_bmp, W, H);
                        break;
                    default:
                        break;
                }
*/
                target.Invalidate();
            }
            /*else if(current_display_engine == DisplayEngine.DIRECT_X)
            {
                switch(current_display_mode)
                {
                    case DisplayMode.SPECTRUM:
                        current_background = SetupSpectrum();
                        break;
                    case DisplayMode.PANADAPTER:
                        break;
                    case DisplayMode.SCOPE:
                        current_background = SetupScope();
                        break;
                    case DisplayMode.PHASE:
                        break;	
                    case DisplayMode.PHASE2:
                        break;
                    case DisplayMode.WATERFALL:
                        break;
                    case DisplayMode.HISTOGRAM:
                        break;
                    case DisplayMode.OFF:
                        break;
                    default:
                        break;
                }				
				
                // redraw screen now if not starting up and if in standby
                //if(console.SetupForm != null && !console.PowerOn) RenderDirectX();
            }*/
        }

        // This draws a little callout on the notch to show it's frequency and bandwidth
        // xlimit is the right side of the picDisplay
        private static void drawNotchStatus(Graphics g, Notch n, int x, int y, int x_limit, int height)
        {
            // if we're not supposed to be drawing this, return to caller
            if (!n.Details) return;
            // in case notch is showing on RX1 & RX2, just show it for the one that was clicked
            if ((y < height && n.RX == 2) || (y > height && n.RX == 1)) return;
            // first we need to test if it is OK to draw the box to the right of the notch ... I don't
            // know the panadapter limits in x, so I will use a constant.  This needs to be replaced
            int x_distance_from_notch = 40;
            int y_distance_from_bot = 20;
            int box_width = 120;
            int box_height = 55;
            int x_start, y_start, x_pin, y_pin;
            // determine if it will fit in the panadapter to the right of the notch
            if (x + box_width + x_distance_from_notch > x_limit)
            {
                // draw to the left
                x_pin = x - x_distance_from_notch;
                y_pin = y - y_distance_from_bot;
                x_start = x_pin - box_width;
                y_start = y_pin - (box_height / 2);
            }
            else
            {
                // draw to the right
                x_start = x + x_distance_from_notch;
                x_pin = x_start;
                y_pin = y - y_distance_from_bot;
                y_start = y_pin - (box_height / 2);
            }

            // such pretty colors of green, hardcoded for your viewing pleasure
            Color c = Color.DarkOliveGreen;
            Pen p = new Pen(Color.DarkOliveGreen, 1);
            Brush b = new SolidBrush(Color.Chartreuse);
            // Draw a nice rectangle to write into
            g.FillRectangle(new SolidBrush(c), x_start, y_start, box_width, box_height);
            // draw a left and right line on the side of the rectancle
            g.DrawLine(p, x, y, x_pin, y_pin);
            // get the Hz part of the frequescy because we want to set it off from the actual number so it looks neato
            int right_three = (int)(n.Freq * 1e6) - (int)(n.Freq * 1e3) * 1000;
            double left_three = (((int)(n.Freq * 1e3)) / 1e3);
            //string perm = n.Permanent ? "*" : "";
            g.DrawString("RF Tracking Notch", // + perm,
                new System.Drawing.Font("Trebuchet MS", 9, FontStyle.Underline),
                b, new Point(x_start + 5, y_start + 5));
            g.DrawString(left_three.ToString("f3") + " " + right_three.ToString("d3") + " MHz",
                new System.Drawing.Font("Trebuchet MS", 9, FontStyle.Regular),
                b, new Point(x_start + 5, y_start + 20));
            g.DrawString(n.BW.ToString("d") + " Hz wide",
                new System.Drawing.Font("Trebuchet MS", 9, FontStyle.Regular),
                b, new Point(x_start + 5, y_start + 35));
        }

        /// <summary>
        /// draws the vertical bar to highlight where a notch is on the panadapter
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="left">left side of notch in pixel location</param>
        /// <param name="right">right side of notch, pixel location</param>
        /// <param name="top">top of bar</param>
        /// <param name="H">height of bar</param>
        /// <param name="on">color for notch on</param>
        /// <param name="off">color for notch off</param>
        /// <param name="highlight">highlight color to draw highlights on bar</param>
        /// <param name="active">true if notches are turned on</param>
        static void drawNotchBar(Graphics g, Notch n, int left, int right, int top, int height, Color c, Color h)
        {
            int width = right - left;
            int hash_spacing_pixels = 1;
            switch (n.Depth)
            {
                case 1:
                    hash_spacing_pixels = 12;
                    break;
                case 2:
                    hash_spacing_pixels = 8;
                    break;
                case 3:
                    hash_spacing_pixels = 4;
                    break;
            }

            // get a purty pen to draw with 
            Pen p = new Pen(h, 1);

            // shade in the notch
            g.FillRectangle(new SolidBrush(c), left, top, width, height);

            // draw a left and right line on the side of the rectangle if wide enough
            if (width > 2 && tnf_active)
            {
                g.DrawLine(p, left, top, left, top + height - 1);
                g.DrawLine(p, right, top, right, top + height - 1);

                // first draw down left side of notch indicator horizontal lines -- a series of 45-degree hashes
                for (int y = top + hash_spacing_pixels; y < top + height - 1 + width; y += hash_spacing_pixels)
                {
                    int start_y = y;
                    int start_x = left;
                    int end_x = right;
                    int end_y = start_y - width;

                    int min_y = top;
                    int max_y = top + height - 1;

                    // if we are about to over-draw past the top of the rectangle, we must restrain ourselves!
                    if (end_y < min_y)
                    {
                        end_x -= (min_y - end_y);
                        end_y = top;
                    }

                    // if we are about to over-draw past the bottom of the rectangle, we must restrain ourselves!
                    if (start_y > max_y)
                    {
                        start_x += (start_y - max_y);
                        start_y = max_y;
                    }

                    g.DrawLine(p, start_x, start_y, end_x, end_y);
                }
            }
        }

        /// <summary>
        /// draws the vertical bar to highlight where a channel is on the panadapter
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="n">Channel object reference</param>
        /// <param name="left">left side of notch in pixel location</param>
        /// <param name="right">right side of notch, pixel location</param>
        /// <param name="top">top of bar</param>
        /// <param name="H">height of bar</param>
        /// <param name="on">color for notch on</param>
        /// <param name="off">color for notch off</param>
        /// <param name="highlight">highlight color to draw highlights on bar</param>
        /// <param name="active">true if notches are turned on</param>
        static void drawChannelBar(Graphics g, Channel chan, int left, int right, int top, int height, Color c, Color h)
        {
            int width = right - left;

            // get a purty pen to draw with 
            Pen p = new Pen(h, 1);

            // shade in the notch
            g.FillRectangle(new SolidBrush(c), left, top, width, height);

            // draw a left and right line on the side of the rectancle if wide enough
            if (width > 2)
            {
                //g.DrawLine(p, left - 1, top, left - 1, top + height - 1);
                g.DrawLine(p, left, top, left, top + height - 1);
                g.DrawLine(p, right, top, right, top + height - 1);
                //g.DrawLine(p, right+1, top, right+1, top + height - 1);
            }
        }

        #endregion

        #region GDI+

        unsafe public static void RenderGDIPlus(ref PaintEventArgs e)
        {

            try
            {

                if (!split_display)
                {
                    switch (current_display_mode)
                    {
                        case DisplayMode.SPECTRUM:
                            K9 = 4;
                            K11 = 0;
                            DrawSpectrum(e.Graphics, W, H, false);
                            break;
                        case DisplayMode.PANADAPTER:
                            K9 = 2;
                            K11 = 0;
                            DrawPanadapter(e.Graphics, W, H, 1, false);
                            break;
                        case DisplayMode.SCOPE:
                            K9 = 4;
                            K11 = 0;
                            DrawScope(e.Graphics, W, H, false);
                            break;
                        case DisplayMode.SCOPE2:
                            DrawScope2(e.Graphics, W, H, false);
                            break;
                        case DisplayMode.PHASE:
                            K9 = 4;
                            K11 = 0;
                            DrawPhase(e.Graphics, W, H, false);
                            break;
                        case DisplayMode.PHASE2:
                            K9 = 4;
                            K11 = 0;
                            DrawPhase2(e.Graphics, W, H, false);
                            break;
                        case DisplayMode.WATERFALL:
                            K9 = 1;
                            K11 = 0;
                            DrawWaterfall(e.Graphics, W, H, 1, false);
                            break;
                        case DisplayMode.HISTOGRAM:
                            K9 = 4;
                            K11 = 0;
                            DrawHistogram(e.Graphics, W, H);
                            break;
                        case DisplayMode.PANAFALL:
                            //    split_display = true;
                            //    DrawPanadapter(e.Graphics, W, H / 2, 1, false);
                            //    DrawWaterfall(e.Graphics, W, H / 2, 1, true);
                            //    split_display = false;
                            if (map == 1) // ke9ns add  if in special map viewing panafall mode
                            {

                                K9 = 7;             //special panafall mode for sun/grayline tracking mode
                                K11 = 0;

                                DrawPanadapter(e.Graphics, W, 5 * H / 6, 1, false);    //     in pure panadapter mode: update = DrawPanadapter(e.Graphics, W, H, 1, false);
                                DrawWaterfall(e.Graphics, W, 5 * H / 6, 1, true);        // bottom half RX2 is not on
                                split_display = false;
                            }
                            else
                            {
                                K9 = 3;
                                K11 = 0;

                                split_display = true; // use wide vertgrid because your saying split
                                DrawPanadapter(e.Graphics, W, H / 2, 1, false); //top half 
                                DrawWaterfall(e.Graphics, W, H / 2, 1, true); // bottom half RX2 is not on
                            split_display = false;
                            }
                            break;
                        case DisplayMode.PANASCOPE:
                            K9 = 4;
                            K11 = 0;
                            split_display = true;
                            DrawPanadapter(e.Graphics, W, H / 2, 1, false);
                            DrawScope(e.Graphics, W, H / 2, true);
                            split_display = false;
                            break;
                        case DisplayMode.SPECTRASCOPE:
                            K9 = 4;
                            K11 = 0;
                            split_display = true;
                            DrawSpectrum(e.Graphics, W, H / 2, false);
                            DrawScope(e.Graphics, W, H / 2, true);
                            split_display = false;
                            break;
                        case DisplayMode.OFF:
                            K9 = 0;
                            K11 = 0;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (current_display_mode)
                    {
                        case DisplayMode.SPECTRUM:
                            K9 = 4;
                            K11 = 0;
                            DrawSpectrum(e.Graphics, W, H / 2, false);
                            break;
                        case DisplayMode.PANADAPTER:
                            K9 = 2;
                            K11 = 0;
                            DrawPanadapter(e.Graphics, W, H / 2, 1, false);
                            break;
                        case DisplayMode.SCOPE:
                            K9 = 4;
                            K11 = 0;
                            DrawScope(e.Graphics, W, H / 2, false);
                            break;
                        case DisplayMode.PHASE:
                            K9 = 4;
                            K11 = 0;
                            DrawPhase(e.Graphics, W, H / 2, false);
                            break;
                            K9 = 4;
                            K11 = 0;
                        case DisplayMode.PHASE2:
                            DrawPhase2(e.Graphics, W, H / 2, false);
                            break;
                        case DisplayMode.WATERFALL:
                            K9 = 6;
                            K11 = 0;
                            DrawWaterfall(e.Graphics, W, H / 2, 1, false);
                            break;
                        case DisplayMode.HISTOGRAM:
                            K9 = 4;
                            K11 = 0;
                            DrawHistogram(e.Graphics, W, H / 2);
                            break;
                        case DisplayMode.PANAFALL:   // ke9ns pan rX1 (KE9NS ADDED CODE)
                            K9 = 5;
                            K11 = 5;

                            switch (current_display_mode_bottom)  // ke9ns check RX2 to see what to do with both RX1 and RX2
                            {
                                case DisplayMode.PANADAPTER:
                                    K10 = 2;
                                    DrawPanadapter(e.Graphics, W, H / 3, 1, false); // RX1 panadapter top 1/3
                                    DrawWaterfall(e.Graphics, W, H / 3, 1, true);     // RX1 waterfall middle 1/3
                                    DrawPanadapter(e.Graphics, W, 2 * H / 3, 2, true); // RX2  bottom 1/3
                                    break;

                                case DisplayMode.WATERFALL:
                                    K10 = 1;
                                    DrawPanadapter(e.Graphics, W, H / 3, 1, false); // RX1 panadapter top 1/3
                                    DrawWaterfall(e.Graphics, W, H / 3, 1, true);     // RX1 waterfall middle 1/3
                                    DrawWaterfall(e.Graphics, W, 2 * H / 3, 2, true);  // RX2 bottom 1/3
                                    break;
                                case DisplayMode.PANAFALL:   // ke9ns pan (KE9NS ADDED CODE)  rx2 panafall with RX1 panafall as well
                                    K10 = 5;
                                    DrawPanadapter(e.Graphics, W, H / 4, 1, false); // RX1 panadapter top 1/4
                                    DrawWaterfall(e.Graphics, W, H / 4, 1, true);     // RX1 waterfall middle 1/4
                                    DrawPanadapter(e.Graphics, W, 2 * H / 4, 2, true);
                                    DrawWaterfall(e.Graphics, W, 3 * H / 4, 2, true);
                                    break;

                        case DisplayMode.OFF:
                                    K10 = 0;
                                    DrawOffBackground(e.Graphics, W, H / 2, true);
                                    K9 = 3;
                                    K11 = 0;
                                    split_display = true; // use wide vertgrid because your saying split
                                    DrawPanadapter(e.Graphics, W, H / 2, 1, false); //top half 
                                    DrawWaterfall(e.Graphics, W, H / 2, 1, true); // bottom half RX2 is not on
                                    split_display = false;
                                    break; // rx2 off

                            } // switch (current_display_mode_bottom)
                            break;  // rx1 panafall
                        case DisplayMode.OFF:
                            K9 = 0;
                            K11 = 0;
                            DrawOffBackground(e.Graphics, W, H / 2, false);
                            break;
                        default:
                            break;
                    }

                    if (K11 == 0) //if rx1 is in panafall skip below
                    {
                    switch (current_display_mode_bottom)
                    {
                        case DisplayMode.SPECTRUM:
                                K10 = 0;
                            DrawSpectrum(e.Graphics, W, H / 2, true);
                            break;
                        case DisplayMode.PANADAPTER:
                                K10 = 2;
                            DrawPanadapter(e.Graphics, W, H / 2, 2, true);
                            break;
                        case DisplayMode.SCOPE:
                                K10 = 0;
                            DrawScope(e.Graphics, W, H / 2, true);
                            break;
                        case DisplayMode.PHASE:
                                K10 = 0;
                            DrawPhase(e.Graphics, W, H / 2, true);
                            break;
                        case DisplayMode.PHASE2:
                                K10 = 0;
                            DrawPhase2(e.Graphics, W, H / 2, true);
                            break;
                        case DisplayMode.WATERFALL:
                                K10 = 1;
                            DrawWaterfall(e.Graphics, W, H / 2, 2, true);
                            break;
                        case DisplayMode.HISTOGRAM:
                                K10 = 0;
                            DrawHistogram(e.Graphics, W, H / 2);
                            break;
                        case DisplayMode.OFF:
                                K10 = 0;
                            DrawOffBackground(e.Graphics, W, H / 2, true);
                                switch (current_display_mode) // ke9ns split display (RX1 top  and RX2 on bottom)
                                {
                                    case DisplayMode.PANAFALL:
                                        K9 = 3;
                                        K11 = 0;

                                        split_display = true; // use wide vertgrid because your saying split
                                        DrawPanadapter(e.Graphics, W, H / 2, 1, false); //top half 
                                        DrawWaterfall(e.Graphics, W, H / 2, 1, true); // bottom half RX2 is not on
                                        split_display = false;
                                        break;
                                }
                                break;
                            case DisplayMode.PANAFALL:
                                K10 = 2;
                                DrawPanadapter(e.Graphics, W, H / 2, 2, true); // RX2  (standard mode)
                            break;
                        default:
                                K10 = 2;
                                DrawPanadapter(e.Graphics, W, H / 2, 2, true); // RX2  (standard mode)
                            break;
                    }
                }
                    else // rx1 in panafall mode
                    {
                        switch (current_display_mode_bottom)  // ke9ns pan
                        {

                            case DisplayMode.OFF:
                                K10 = 0;
                                DrawOffBackground(e.Graphics, W, H / 2, true);
                                break; // RX2 OFF
                        } 
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }

        }

        private static void UpdateDisplayPeak(float[] buffer, float[] new_data)
        {
            if (buffer[0] == CLEAR_FLAG)
            {
                //Debug.WriteLine("Clearing peak buf"); 
                for (int i = 0; i < BUFFER_SIZE; i++)
                    buffer[i] = new_data[i];
            }
            else
            {
                for (int i = 0; i < BUFFER_SIZE; i++)
                {
                    if (new_data[i] > buffer[i])
                        buffer[i] = new_data[i];
                    new_data[i] = buffer[i];
                }
            }
        }

        #region Drawing Routines
        // ======================================================
        // Drawing Routines
        // ======================================================


        private static void DrawPhaseGrid(ref Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            for (double i = 0.50; i < 3; i += .50)	// draw 3 concentric circles
            {
                if (bottom) g.DrawEllipse(grid_pen, (int)(W / 2 - H * i / 2), H + (int)(H / 2 - H * i / 2), (int)(H * i), (int)(H * i));
                else g.DrawEllipse(grid_pen, (int)(W / 2 - H * i / 2), (int)(H / 2 - H * i / 2), (int)(H * i), (int)(H * i));
            }
            if (high_swr && !bottom)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
        }

        private static void DrawScopeGrid(ref Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            /* using (Pen grid_pen = new Pen(grid_color))
            if(bottom)
            {
                g.DrawLine(grid_pen, 0, H+H/2, W, H+H/2);	// draw horizontal line
                g.DrawLine(grid_pen, W/2, H, W/2, H+H);	// draw vertical line
            }
            else
            {
                g.DrawLine(grid_pen, 0, H/2, W, H/2);	// draw horizontal line
                g.DrawLine(grid_pen, W/2, 0, W/2, H);	// draw vertical line
            } */
            //using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Red))
                if (high_swr && !bottom)
                    g.DrawString("High SWR", font14, brush, 245, 20);
        }

        private static void DrawSpectrumGrid(ref Graphics g, int W, int H, bool bottom)
        {

            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            int low = 0;								// init limit variables
            int high = 0;

            int center_line_x = (int)(-(double)low / (high - low) * W);

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;				// get RX display limits
                high = rx_spectrum_display_high;
            }
            else
            {
                if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
                {
                    low = rx_spectrum_display_low;
                    high = rx_spectrum_display_high;
                }
                else
                {
                    low = tx_spectrum_display_low;			// get RX display limits
                    high = tx_spectrum_display_high;
                }
            }

            int mid_w = W / 2;
            int[] step_list = { 10, 20, 25, 50 };
            int step_power = 1;
            int step_index = 0;
            int freq_step_size = 50;

            //  int grid_step = spectrum_grid_step;
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0; // spectrum_grid_step;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))// || (mox && tx_on_vfob))
            {
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
                grid_step = spectrum_grid_step;
            }
            else if (mox)
            {
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                grid_step = tx_spectrum_grid_step;
            }

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            if (high == 0)
            {
                int f = -low;
                // Calculate horizontal step size
                while (f / freq_step_size > 7)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                float pixel_step_size = (float)(W * freq_step_size / f);

                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int x = W - (int)Math.Floor(i * pixel_step_size);	// for negative numbers

                    if (bottom) g.DrawLine(grid_pen, x, H, x, H + H);
                    else g.DrawLine(grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)((label.Length + 1) * 4.1);
                    if (x - offset >= 0)
                    {
                        if (bottom) g.DrawString("-" + label, font9, grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else g.DrawString("-" + label, font9, grid_text_brush, x - offset, (float)Math.Floor(H * .01));
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                num_steps = V / grid_step;
                pixel_step_size = H / num_steps;

                for (int i = 1; i < num_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);

                    if (bottom) g.DrawLine(hgrid_pen, 0, H + y, W, H + y);
                    else g.DrawLine(hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)g.MeasureString("-", font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = g.MeasureString(label, font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        console.DisplayGridX = x;
                        console.DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) g.DrawString(label, font9, grid_text_brush, x, H + y);
                            g.DrawString(label, font9, grid_text_brush, x, y);
                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                    rx1_dsp_mode == DSPMode.SAM ||
                    rx1_dsp_mode == DSPMode.FM ||
                    rx1_dsp_mode == DSPMode.DSB ||
                    rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        g.DrawLine(grid_zero_pen, W - 1, H, W - 1, H + H);
                        g.DrawLine(grid_zero_pen, W - 2, H, W - 2, H + H);
                    }
                    else
                    {
                        g.DrawLine(grid_zero_pen, W - 1, 0, W - 1, H);
                        g.DrawLine(grid_zero_pen, W - 2, 0, W - 2, H);
                    }
            }
            else if (low == 0)
            {
                int f = high;
                // Calculate horizontal step size
                while (f / freq_step_size > 7)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                float pixel_step_size = (float)(W * freq_step_size / f);
                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int x = (int)Math.Floor(i * pixel_step_size);// for positive numbers

                    if (bottom) g.DrawLine(grid_pen, x, H, x, H + H);
                    else g.DrawLine(grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)(label.Length * 4.1);
                    if (x - offset + label.Length * 7 < W)
                    {
                        if (bottom) g.DrawString(label, font9, grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else g.DrawString(label, font9, grid_text_brush, x - offset, (float)Math.Floor(H * .01));
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                int numSteps = V / grid_step;
                pixel_step_size = H / numSteps;
                for (int i = 1; i < numSteps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);

                    if (bottom) g.DrawLine(hgrid_pen, 0, H + y, W, H + y);
                    else g.DrawLine(hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)g.MeasureString("-", font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = g.MeasureString(label, font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        console.DisplayGridX = x;
                        console.DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) g.DrawString(label, font9, grid_text_brush, x, H + y);
                            g.DrawString(label, font9, grid_text_brush, x, y);
                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                   rx1_dsp_mode == DSPMode.SAM ||
                   rx1_dsp_mode == DSPMode.FM ||
                   rx1_dsp_mode == DSPMode.DSB ||
                   rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        g.DrawLine(grid_zero_pen, 0, H, 0, H + H);
                        g.DrawLine(grid_zero_pen, 1, H, 1, H + H);
                    }
                    else
                    {
                        g.DrawLine(grid_zero_pen, 0, 0, 0, H);
                        g.DrawLine(grid_zero_pen, 1, 0, 1, H);
                    }
            }
            else if (low < 0 && high > 0)
            {
                int f = high;

                // Calculate horizontal step size
                while (f / freq_step_size > 4)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                int pixel_step_size = W / 2 * freq_step_size / f;
                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int xLeft = mid_w - (i * pixel_step_size);			// for negative numbers
                    int xRight = mid_w + (i * pixel_step_size);		// for positive numbers
                    if (bottom)
                    {
                        g.DrawLine(grid_pen, xLeft, H, xLeft, H + H);		// draw left line
                        g.DrawLine(grid_pen, xRight, H, xRight, H + H);		// draw right line
                    }
                    else
                    {
                        g.DrawLine(grid_pen, xLeft, 0, xLeft, H);		// draw left line
                        g.DrawLine(grid_pen, xRight, 0, xRight, H);		// draw right line
                    }

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offsetL = (int)((label.Length + 1) * 4.1);
                    int offsetR = (int)(label.Length * 4.1);
                    if (xLeft - offsetL >= 0)
                    {
                        if (bottom)
                        {
                            g.DrawString("-" + label, font9, grid_text_brush, xLeft - offsetL, H + (float)Math.Floor(H * .01));
                            g.DrawString(label, font9, grid_text_brush, xRight - offsetR, H + (float)Math.Floor(H * .01));
                        }
                        else
                        {
                            g.DrawString("-" + label, font9, grid_text_brush, xLeft - offsetL, (float)Math.Floor(H * .01));
                            g.DrawString(label, font9, grid_text_brush, xRight - offsetR, (float)Math.Floor(H * .01));
                        }
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                int numSteps = V / grid_step;
                pixel_step_size = H / numSteps;
                for (int i = 1; i < numSteps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);
                    if (bottom) g.DrawLine(grid_pen, 0, H + y, W, H + y);
                    else g.DrawLine(grid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)g.MeasureString("-", font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = g.MeasureString(label, font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }

                        console.DisplayGridX = x;
                        console.DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) g.DrawString(label, font9, grid_text_brush, x, H + y);
                            g.DrawString(label, font9, grid_text_brush, x, y);
                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                   rx1_dsp_mode == DSPMode.SAM ||
                   rx1_dsp_mode == DSPMode.FM ||
                   rx1_dsp_mode == DSPMode.DSB ||
                   rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        g.DrawLine(grid_zero_pen, mid_w, H, mid_w, H + H);
                        g.DrawLine(grid_zero_pen, mid_w - 1, H, mid_w - 1, H + H);
                    }
                    else
                    {
                        g.DrawLine(grid_zero_pen, mid_w, 0, mid_w, H);
                        g.DrawLine(grid_zero_pen, mid_w - 1, 0, mid_w - 1, H);
                    }
            }

            if (high_swr && !bottom)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
        }

        //=========================================================
        // ke9ns draw panadapter grid
        //=========================================================

        public static int[] holder2 = new int[100];                           // ke9ns add MEMORY Spot used to allow the vertical lines to all be drawn first so the call sign text can draw over the top of it.
        public static int[] holder3 = new int[100];                          // ke9ns add

        public static int[] holder = new int[100];                           // ke9ns add DX Spot used to allow the vertical lines to all be drawn first so the call sign text can draw over the top of it.
        public static int[] holder1 = new int[100];                          // ke9ns add
        private static Font font1 = new Font("Ariel", 9, FontStyle.Regular);  // ke9ns add dx spot call sign font style

        private static Pen p1 = new Pen(Color.YellowGreen, 2.0f);             // ke9ns add vert line color and thickness  DXSPOTTER
        private static Pen p3 = new Pen(Color.Blue, 2.5f);                   // ke9ns add vert line color and thickness    MEMORY
        private static Pen p2 = new Pen(Color.Purple, 2.0f);                  // ke9ns add color for vert line of SWL list

        private static SizeF length;                                          // ke9ns add length of call sign so we can do usb/lsb and define a box to click into
        private static SizeF length1;                                          // ke9ns add length of call sign so we can do usb/lsb and define a box to click into

        private static bool low = false;                                     // ke9ns add true=LSB, false=USB
        private static int rx2 = 0;                                          // ke9ns add 0-49 spots for rx1 panadapter window for qrz callup  (50-100 for rx2)
        private static int rx3 = 0;                                          // ke9ns add 0-49 spots for rx1 panadapter window for qrz callup  (50-100 for rx2)

        public static int VFOLow = 0;                                       // ke9ns low freq (left side of screen) in HZ (used in DX_spot)
        public static int VFOHigh = 0;                                      // ke9ns high freq (right side of screen) in HZ
        public static int VFODiff = 0;                                      // ke9ns diff high-low

        static float zoom_height = 1.5f;   // Should be > 1.  H = H/zoom_height
        unsafe private static void DrawPanadapterGrid(ref Graphics g, int W, int H, int rx, bool bottom)
        {
            // draw background
            // g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            bool local_mox = false;
            bool displayduplex = false;
            if (mox && rx == 1 && !tx_on_vfob) local_mox = true;
            if (mox && rx == 2 && tx_on_vfob) local_mox = true;
            if (rx == 1 && tx_on_vfob && mox && !console.RX2Enabled) local_mox = true;
            int Low = 0;// = rx_display_low;					// initialize variables
            int High = 0;// = rx_display_high;
            int mid_w = W / 2;
            int[] step_list = { 10, 20, 25, 50 };
            int step_power = 1;
            int step_index = 0;
            int freq_step_size = 50;
            int inbetweenies = 5;
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0; // spectrum_grid_step;
            int f_diff = 0;
            int sample_rate;

            if ((CurrentDisplayMode == DisplayMode.PANAFALL && display_duplex) ||
               (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;

            if (local_mox && !displayduplex)// || (mox && tx_on_vfob))
            {
                Low = tx_display_low;
                High = tx_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                grid_step = tx_spectrum_grid_step;
                sample_rate = sample_rate_tx;
                g.FillRectangle(tx_display_background_brush, 0, bottom ? H : 0, W, H);

                if (rx == 1 && !console.VFOBTX) f_diff = freq_diff;
                else f_diff = rx2_freq_diff;
            }
            else if (rx == 2)
            {
                if (local_mox && displayduplex)// || (mox && tx_on_vfob))
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;
                }
                grid_max = rx2_spectrum_grid_max;
                grid_min = rx2_spectrum_grid_min;
                grid_step = rx2_spectrum_grid_step;
                sample_rate = sample_rate_rx2;
                g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);
                f_diff = rx2_freq_diff;
            }
            else
            {
                Low = rx_display_low;
                High = rx_display_high;
                if (local_mox)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);
                f_diff = freq_diff;
            }

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            int filter_low, filter_high;
            int center_line_x;
            int[] band_edge_list;

            if (local_mox) // get filter limits
            {
                filter_low = tx_filter_low;
                filter_high = tx_filter_high;
            }
            else if (rx == 2)
            {
                filter_low = rx2_filter_low;
                filter_high = rx2_filter_high;
            }
            else //if(rx == 1)
            {
                filter_low = rx1_filter_low;
                filter_high = rx1_filter_high;
            }

            if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
                (rx2_dsp_mode == DSPMode.DRM && rx == 2))
            {
                filter_low = -5000;
                filter_high = 5000;
            }

            int width = High - Low;
            //d center_line_x = (int)(-(double)Low / width * W);

            // Calculate horizontal step size
            while (width / freq_step_size > 10)
            {
                /*inbetweenies = step_list[step_index] / 10;
                if (inbetweenies == 1) inbetweenies = 10;*/
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }
            double w_pixel_step = (double)W * freq_step_size / width;
            int w_steps = width / freq_step_size;

            // calculate vertical step size
            int h_steps = (grid_max - grid_min) / grid_step;
            double h_pixel_step = (double)H / h_steps;
            int top = (int)((double)grid_step * H / y_range);

            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            {
                // draw Sub RX filter
                // get filter screen coordinates
                int filter_left_x = (int)((float)(filter_low - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);
                int filter_right_x = (int)((float)(filter_high - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);

                // make the filter display at least one pixel wide.
                if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                // draw rx filter
                if (bottom)
                {
                    g.FillRectangle(sub_rx_filter_brush,	// draw filter overlay
                        filter_left_x, H + top, filter_right_x - filter_left_x, H + H - top);
                }
                else
                {
                    g.FillRectangle(sub_rx_filter_brush,	// draw filter overlay
                        filter_left_x, top, filter_right_x - filter_left_x, H - top);
                }

                // draw Sub RX 0Hz line
                int x = (int)((float)(vfoa_sub_hz - vfoa_hz - Low) / width * W);
                if (bottom)
                {
                    g.DrawLine(sub_rx_zero_line_pen, x, H + top, x, H + H);
                    g.DrawLine(sub_rx_zero_line_pen, x - 1, H + top, x - 1, H + H);
                }
                else
                {
                    g.DrawLine(sub_rx_zero_line_pen, x, top, x, H);
                    g.DrawLine(sub_rx_zero_line_pen, x - 1, top, x - 1, H);
                }
            }
            // draw RX filter
            if (!local_mox)// && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
            {
                int filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
                int filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);

                // make the filter display at least one pixel wide.
                if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                if (bottom)
                {
                    //g.FillRectangle(display_filter_brush,	// draw filter overlay
                    //	filter_left_x, H + top, filter_right_x-filter_left_x, H + H - top);
                    g.FillRectangle(display_filter_brush, filter_left_x, H + top, filter_right_x - filter_left_x, H + H - top);
                }
                else
                {
                    g.FillRectangle(display_filter_brush,	// draw filter overlay
                        filter_left_x, top, filter_right_x - filter_left_x, H - top);
                }
            }

            if (local_mox && (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            {
                // get filter screen coordinates
                // int filter_left_x = (int)((float)(filter_low - Low) / (High - Low) * W);
                // int filter_right_x = (int)((float)(filter_high - Low) / (High - Low) * W);
                int filter_left_x = (int)((float)(filter_low - Low - f_diff + xit_hz) / width * W);
                int filter_right_x = (int)((float)(filter_high - Low - f_diff + xit_hz) / width * W);

                // make the filter display at least one pixel wide.
                if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                // draw tx filter
                if (bottom)
                {
                    g.FillRectangle(tx_filter_brush,	// draw filter overlay
                        filter_left_x, H + top, filter_right_x - filter_left_x, H + H - top);
                }
                else if (console.VFOATX || (!displayduplex && console.VFOBTX))
                {
                    g.FillRectangle(tx_filter_brush,	// draw filter overlay
                      filter_left_x, top, filter_right_x - filter_left_x, H - top);
                }
            }

            if (!mox && draw_tx_filter &&
                (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            {
                // get tx filter limits
                int filter_left_x;
                int filter_right_x;

                if (tx_on_vfob)
                {
                    if (!split_enabled)
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz) / width * W);
                    }
                    else
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfob_hz - vfoa_hz)) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfob_hz - vfoa_hz)) / width * W);
                    }
                }
                else
                {
                    if (!split_enabled)
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
                    }
                    else
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                    }
                }

                if (bottom && tx_on_vfob)
                {
                    g.DrawLine(tx_filter_pen, filter_left_x, H + top, filter_left_x, H + H);		// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_left_x + 1, H + top, filter_left_x + 1, H + H);	// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x, H + top, filter_right_x, H + H);	// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x + 1, H + top, filter_right_x + 1, H + H);// draw tx filter overlay
                }
                else if (!tx_on_vfob)
                {
                    g.DrawLine(tx_filter_pen, filter_left_x, top, filter_left_x, H);		// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_left_x + 1, top, filter_left_x + 1, H);	// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x, top, filter_right_x, H);	// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x + 1, top, filter_right_x + 1, H);// draw tx filter overlay
                }
            }

            if (!local_mox /* && !tx_on_vfob */ && draw_tx_cw_freq &&
                (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
            {
                int pitch = cw_pitch;
                if (rx1_dsp_mode == DSPMode.CWL)
                    pitch = -cw_pitch;

                int cw_line_x;
                if (!split_enabled)
                    cw_line_x = (int)((float)(pitch - Low - f_diff + xit_hz - rit_hz) / width * W);
                else
                    cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                g.DrawLine(tx_filter_pen, cw_line_x, top, cw_line_x, H);
                g.DrawLine(tx_filter_pen, cw_line_x + 1, top, cw_line_x + 1, H);
            }

            if (!local_mox && tx_on_vfob && draw_tx_cw_freq &&
                (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
            {
                int pitch = cw_pitch;
                if (rx2_dsp_mode == DSPMode.CWL)
                    pitch = -cw_pitch;

                int cw_line_x;
                if (!split_enabled)
                    cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz) / width * W);
                else
                    cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                g.DrawLine(tx_filter_pen, cw_line_x, H + top, cw_line_x, H + H);
                g.DrawLine(tx_filter_pen, cw_line_x + 1, H + top, cw_line_x + 1, H + H);

            }

            // draw 60m channels if in view
            if (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK)
            {
                foreach (Channel c in Console.Channels60m)
                {
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;
                    if (local_mox) rit = 0;

                    if (bottom)
                    {
                        rf_freq = vfob_hz;
                    }

                    if (c.InBW((rf_freq + Low) * 1e-6, (rf_freq + High) * 1e-6)) // is channel visible?
                    {
                        bool on_channel = console.RX1IsOn60mChannel(c); // only true if you are on channel and are in an acceptable mode
                        if (bottom) on_channel = console.RX2IsOn60mChannel(c);

                        DSPMode mode = rx1_dsp_mode;
                        if (bottom) mode = rx2_dsp_mode;

                        switch (mode)
                        {
                            case DSPMode.USB:
                            case DSPMode.DIGU:
                            case DSPMode.CWL:
                            case DSPMode.CWU:
                            case DSPMode.AM:
                            case DSPMode.SAM:
                                break;
                            default:
                                on_channel = false; // make sure other modes do not look as if they could transmit
                                break;
                        }

                        // offset for CW Pitch to align display
                        if (bottom)
                        {
                            switch (rx2_dsp_mode)
                            {
                                case (DSPMode.CWL):
                                    rf_freq += cw_pitch;
                                    break;
                                case (DSPMode.CWU):
                                    rf_freq -= cw_pitch;
                                    break;
                            }
                        }
                        else
                        {
                            switch (rx1_dsp_mode)
                            {
                                case (DSPMode.CWL):
                                    rf_freq += cw_pitch;
                                    break;
                                case (DSPMode.CWU):
                                    rf_freq -= cw_pitch;
                                    break;
                            }
                        }

                        int chan_left_x = (int)((float)(c.Freq * 1e6 - rf_freq - c.BW / 2 - Low - rit) / width * W);
                        int chan_right_x = (int)((float)(c.Freq * 1e6 - rf_freq + c.BW / 2 - Low - rit) / width * W);

                        if (chan_right_x == chan_left_x)
                            chan_right_x = chan_left_x + 1;

                        // decide colors to draw notch
                        Color c1 = channel_background_off;
                        Color c2 = channel_foreground;

                        if (on_channel)
                        {
                            c1 = channel_background_on;
                        }

                        if (bottom)
                            drawChannelBar(g, c, chan_left_x, chan_right_x, H + top, H - top, c1, c2);
                        else
                            drawChannelBar(g, c, chan_left_x, chan_right_x, top, H - top, c1, c2);

                        //if (bottom)
                        //    drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, H + top + 75, W, H);
                        //else
                        //    drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, top + 75, W, H);
                    }
                }
            }

            // draw notches if in RX
            if (!local_mox)
            {
                List<Notch> notches;
                if (!bottom)
                    notches = NotchList.NotchesInBW((double)vfoa_hz * 1e-6, Low, High);
                else
                    notches = NotchList.NotchesInBW((double)vfob_hz * 1e-6, Low, High);

                //draw notch bars in this for loop
                foreach (Notch n in notches)
                {
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;

                    if (bottom)
                    {
                        rf_freq = vfob_hz;
                    }

                    if (bottom)
                    {
                        switch (rx2_dsp_mode)
                        {
                            case (DSPMode.CWL):
                                rf_freq += cw_pitch;
                                break;
                            case (DSPMode.CWU):
                                rf_freq -= cw_pitch;
                                break;
                        }
                    }
                    else
                    {
                        switch (rx1_dsp_mode)
                        {
                            case (DSPMode.CWL):
                                rf_freq += cw_pitch;
                                break;
                            case (DSPMode.CWU):
                                rf_freq -= cw_pitch;
                                break;
                        }
                    }

                    int notch_left_x = (int)((float)(n.Freq * 1e6 - rf_freq - n.BW / 2 - Low - rit) / width * W);
                    int notch_right_x = (int)((float)(n.Freq * 1e6 - rf_freq + n.BW / 2 - Low - rit) / width * W);

                    if (notch_right_x == notch_left_x)
                        notch_right_x = notch_left_x + 1;

                    if (tnf_zoom && n.Details &&
                        ((bottom && n.RX == 2) ||
                        (!bottom && n.RX == 1)))
                    {
                        int zoomed_notch_center_freq = (int)(notch_zoom_start_freq * 1e6 - rf_freq - rit);

                        int original_bw = High - Low;
                        int zoom_bw = original_bw / 10;

                        int low = zoomed_notch_center_freq - zoom_bw / 2;
                        int high = zoomed_notch_center_freq + zoom_bw / 2;

                        if (low < Low) // check left limit
                        {
                            low = Low;
                            high = Low + zoom_bw;
                        }
                        else if (high > High) // check right limit
                        {
                            high = High;
                            low = High - zoom_bw;
                        }

                        int zoom_bw_left_x = (int)((float)(low - Low) / width * W);
                        int zoom_bw_right_x = (int)((float)(high - Low) / width * W);

                        Pen p = new Pen(Color.White, 2.0f);

                        if (!bottom)
                        {
                            // draw zoomed bandwidth outline
                            Point[] left_zoom_line_points = {
                                new Point(0, (int)(H/zoom_height)),
                                new Point(zoom_bw_left_x-1,(int)(0.5*H*(1+1/zoom_height))),
                                new Point(zoom_bw_left_x-1, H) };
                            g.DrawLines(p, left_zoom_line_points);

                            Point[] right_zoom_line_points = {
                                new Point(W, (int)(H/zoom_height)),
                                new Point(zoom_bw_right_x+1, (int)(0.5*H*(1+1/zoom_height))),
                                new Point(zoom_bw_right_x+1, H) };
                            g.DrawLines(p, right_zoom_line_points);

                            //grey out non-zoomed in area on actual panadapter
                            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, H / zoom_height, zoom_bw_left_x, H - H / zoom_height);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), zoom_bw_right_x, H / zoom_height, W - zoom_bw_right_x, H - H / zoom_height);
                        }
                        else
                        {
                            // draw zoomed bandwidth outline
                            Point[] left_zoom_line_points = {
                                new Point(0, H+(int)(H/zoom_height)),
                                new Point(zoom_bw_left_x-1, H+(int)(0.5*H*(1+1/zoom_height))),
                                new Point(zoom_bw_left_x-1, H+H) };
                            g.DrawLines(p, left_zoom_line_points);

                            Point[] right_zoom_line_points = {
                                new Point(W, H+(int)(H/zoom_height)),
                                new Point(zoom_bw_right_x+1, H+(int)(0.5*H*(1+1/zoom_height))),
                                new Point(zoom_bw_right_x+1, H+H) };
                            g.DrawLines(p, right_zoom_line_points);

                            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), 0, H + H / zoom_height, zoom_bw_left_x, H + H - H / zoom_height);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), zoom_bw_right_x, H + H / zoom_height, W - zoom_bw_right_x, H + H - H / zoom_height);
                        }
                    }

                    // decide colors to draw notch
                    Color c1 = notch_on_color;
                    Color c2 = notch_highlight_color;

                    if (!tnf_active)
                    {
                        c1 = notch_off_color;
                        c2 = Color.Black;
                    }
                    else if (n.Permanent)
                    {
                        c1 = notch_perm_on_color;
                        c2 = notch_perm_highlight_color;
                    }

                    if (bottom)
                        drawNotchBar(g, n, notch_left_x, notch_right_x, H + top, H - top, c1, c2);
                    else
                        drawNotchBar(g, n, notch_left_x, notch_right_x, top, H - top, c1, c2);

                    //if (bottom)
                    //    drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, H + top + 75, W, H);
                    //else
                    //    drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, top + 75, W, H);
                }

                //draw notch statuses in this for loop
                if (!tnf_zoom)
                {
                    foreach (Notch n in notches)
                    {
                        long rf_freq = vfoa_hz;
                        int rit = rit_hz;

                        if (bottom)
                        {
                            rf_freq = vfob_hz;
                        }

                        if (bottom)
                        {
                            switch (rx2_dsp_mode)
                            {
                                case (DSPMode.CWL):
                                    rf_freq += cw_pitch;
                                    break;
                                case (DSPMode.CWU):
                                    rf_freq -= cw_pitch;
                                    break;
                            }
                        }
                        else
                        {
                            switch (rx1_dsp_mode)
                            {
                                case (DSPMode.CWL):
                                    rf_freq += cw_pitch;
                                    break;
                                case (DSPMode.CWU):
                                    rf_freq -= cw_pitch;
                                    break;
                            }
                        }

                        int notch_left_x = (int)((float)(n.Freq * 1e6 - rf_freq - n.BW / 2 - Low - rit) / width * W);
                        int notch_right_x = (int)((float)(n.Freq * 1e6 - rf_freq + n.BW / 2 - Low - rit) / width * W);

                        if (notch_right_x == notch_left_x)
                            notch_right_x = notch_left_x + 1;

                        if (bottom)
                            drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, H + top + 75, W, H);
                        else
                            drawNotchStatus(g, n, (notch_left_x + notch_right_x) / 2, top + 75, W, H);
                    }
                }
            }

            // Draw a Zero Beat line on CW filter
            if (!local_mox && show_cwzero_line &&
                (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
            {
                int pitch = cw_pitch;
                if (rx1_dsp_mode == DSPMode.CWL)
                    pitch = -cw_pitch;

                int cw_line_x1;
                if (!split_enabled)
                    //cw_line_x1 = (int)((float)(pitch - Low) / width * W);
                    cw_line_x1 = (int)((float)(pitch - Low - f_diff) / width * W);
                else
                    cw_line_x1 = (int)((float)(pitch - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                g.DrawLine(cw_zero_pen, cw_line_x1, top, cw_line_x1, H);
                g.DrawLine(tx_filter_pen, cw_line_x1 + 1, top, cw_line_x1 + 1, H);

            }

            if (!local_mox && show_cwzero_line &&
                (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
            {
                int pitch = cw_pitch;
                if (rx2_dsp_mode == DSPMode.CWL)
                    pitch = -cw_pitch;

                int cw_line_x1;
                if (!split_enabled)
                    cw_line_x1 = (int)((float)(pitch - Low - f_diff) / width * W);
                else
                    cw_line_x1 = (int)((float)(pitch - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                g.DrawLine(tx_filter_pen, cw_line_x1, H + top, cw_line_x1, H + H);
                g.DrawLine(tx_filter_pen, cw_line_x1 + 1, H + top, cw_line_x1 + 1, H + H);
            }

            // Draw 0Hz vertical line if visible
            if (local_mox)
                center_line_x = (int)((float)(-f_diff - Low + xit_hz) / width * W); // locked 0 line
            else
                center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line

            if (center_line_x >= 0 && center_line_x <= W)
            {
                if (bottom)
                {
                    if (local_mox)
                    {
                        g.DrawLine(tx_grid_zero_pen, center_line_x, H + top, center_line_x, H + H);
                        g.DrawLine(tx_grid_zero_pen, center_line_x + 1, H + top, center_line_x + 1, H + H);
                    }
                    else
                    {
                        g.DrawLine(grid_zero_pen, center_line_x, H + top, center_line_x, H + H);
                        g.DrawLine(grid_zero_pen, center_line_x + 1, H + top, center_line_x + 1, H + H);
                    }
                }
                else
                {
                    if (local_mox)
                    {
                        g.DrawLine(tx_grid_zero_pen, center_line_x, top, center_line_x, H);
                        g.DrawLine(tx_grid_zero_pen, center_line_x + 1, top, center_line_x + 1, H);
                    }
                    else
                    {
                        g.DrawLine(grid_zero_pen, center_line_x, top, center_line_x, H);
                        g.DrawLine(grid_zero_pen, center_line_x + 1, top, center_line_x + 1, H);
                    }
                }
            }

            if (show_freq_offset)
            {
                if (bottom)
                {
                    if (local_mox)
                    {
                        g.DrawString("0", font9, tx_grid_zero_pen.Brush, center_line_x - 5, H + (float)Math.Floor(H * .01));
                    }
                    else
                    {
                        g.DrawString("0", font9, grid_zero_pen.Brush, center_line_x - 5, H + (float)Math.Floor(H * .01));
                    }
                }
                else
                {
                    if (local_mox)
                    {
                        g.DrawString("0", font9, tx_grid_zero_pen.Brush, center_line_x - 5, (float)Math.Floor(H * .01));
                    }
                    {
                        g.DrawString("0", font9, grid_zero_pen.Brush, center_line_x - 5, (float)Math.Floor(H * .01));
                    }
                }
            }

            double vfo;

            /*    if (mox)
                    {
                        if(split_enabled)
                            vfo = vfoa_sub_hz;
                        else
                            vfo = vfoa_hz;
                        vfo += xit_hz;
                    }
                    else if(rx==1)
                    {
                            vfo = vfoa_hz + rit_hz;
                    }
                    else //if(rx==2)
                    {
                        vfo = vfob_hz + rit_hz;
                    } */

            if (rx == 1)
            {
                if (local_mox)
                {
                    if (split_enabled) vfo = vfoa_sub_hz;
                    else vfo = vfoa_hz;
                    vfo += xit_hz;
                }
                else if (mox && tx_on_vfob)
                {
                    if (console.RX2Enabled) vfo = vfoa_hz + rit_hz;
                    else vfo = vfoa_sub_hz;
                }
                else vfo = vfoa_hz + rit_hz;
            }
            else //if(rx==2)
            {
                if (local_mox)
                    vfo = vfob_hz + xit_hz;
                else
                {
                    if (console.VFOSync) vfo = vfob_hz + rit_hz;
                    else vfo = vfob_hz;
                }
            }

            if (!bottom)
            {
                switch (rx1_dsp_mode)
                {
                    case DSPMode.CWL:
                        vfo += cw_pitch;
                        break;
                    case DSPMode.CWU:
                        vfo -= cw_pitch;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (rx2_dsp_mode)
                {
                    case DSPMode.CWL:
                        vfo += cw_pitch;
                        break;
                    case DSPMode.CWU:
                        vfo -= cw_pitch;
                        break;
                    default:
                        break;
                }
            }

            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            int f_steps = (width / freq_step_size) + 1;

            // Draw vertical lines - band edge markers and freq text
            for (int i = 0; i < f_steps + 1; i++)
            {
                string label;
                int offsetL;
                int offsetR;

                int fgrid = i * freq_step_size + (Low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - Low) / width * W);
                string freq_num = actual_fgrid.ToString();

                if (!show_freq_offset)
                {
                    switch (console.CurrentRegion)
                    {
                        case FRSRegion.LAST:
                            if (!show_freq_offset)
                            {
                                if (bottom)
                                    g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else
                                    g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom)
                                {
                                    if (local_mox) g.DrawString(label, font9, tx_band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                    else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                }
                                else
                                {
                                    if (local_mox) g.DrawString(label, font9, tx_band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                                    else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                                }
                                int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                                int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                                float scale = (float)(x_2 - vgrid) / inbetweenies;

                                for (int j = 1; j < inbetweenies; j++)
                                {
                                    float x3 = (float)vgrid + (j * scale);
                                    if (bottom)
                                    {
                                        if (local_mox) g.DrawLine(tx_vgrid_pen_inb, x3, H + top, x3, H + H);
                                        else g.DrawLine(grid_pen_inb, x3, H + top, x3, H + H);
                                    }
                                    else
                                    {
                                        if (local_mox) g.DrawLine(tx_vgrid_pen_inb, x3, top, x3, H);
                                        else g.DrawLine(grid_pen_inb, x3, top, x3, H);
                                    }
                                }
                                break;
                            }
                            break;
                        case FRSRegion.US:
                        case FRSRegion.Extended:
                            if (actual_fgrid == 0.1357 || actual_fgrid == 0.1378 ||
                                actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                                actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
                                         actual_fgrid == 3.5 || actual_fgrid == 4.0 ||
                                         actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                         actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                         actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                         actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                         actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                         actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0 ||
                                actual_fgrid == 144.0 || actual_fgrid == 148.0)
                            {

                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Spain:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.India:
                            if (actual_fgrid == 1.81 || actual_fgrid == 1.86 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.9 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Europe:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.UK:
                            if (actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                                actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 5.2585 || actual_fgrid == 5.4065 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 52.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Italy_Plus:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 6.975 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Japan:
                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
                            actual_fgrid == 1.81 || actual_fgrid == 1.825 ||
                            actual_fgrid == 1.9075 || actual_fgrid == 1.9125 ||
                            actual_fgrid == 3.5 || actual_fgrid == 3.575 ||
                            actual_fgrid == 3.599 || actual_fgrid == 3.612 ||
                            actual_fgrid == 3.68 || actual_fgrid == 3.687 ||
                            actual_fgrid == 3.702 || actual_fgrid == 3.716 ||
                            actual_fgrid == 3.745 || actual_fgrid == 3.77 ||
                            actual_fgrid == 3.791 || actual_fgrid == 3.805 ||
                            actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                            actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                            actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                            actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                            actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                            actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                            actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                            actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Australia:
                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
                                actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                                actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Norway:
                            if (actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
                                actual_fgrid == 5.26 || actual_fgrid == 5.41 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;
                        default:
                            //draw vertical in between lines
                            if (grid_control)
                            {
                                if (bottom)
                                {
                                    if (local_mox) g.DrawLine(tx_vgrid_pen, vgrid, H + top, vgrid, H + H);
                                    else g.DrawLine(grid_pen, vgrid, H + top, vgrid, H + H);
                                }
                                else
                                {
                                    if (local_mox) g.DrawLine(tx_vgrid_pen, vgrid, top, vgrid, H);
                                    else g.DrawLine(grid_pen, vgrid, top, vgrid, H);			//wa6ahl
                                }
                                int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                                int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                                float scale = (float)(x_2 - vgrid) / inbetweenies;

                                for (int j = 1; j < inbetweenies; j++)
                                {
                                    float x3 = (float)vgrid + (j * scale);
                                    if (bottom)
                                    {
                                        if (local_mox) g.DrawLine(tx_vgrid_pen_inb, x3, H + top, x3, H + H);
                                        else g.DrawLine(grid_pen_inb, x3, H + top, x3, H + H);
                                    }
                                    else
                                    {
                                        if (local_mox) g.DrawLine(tx_vgrid_pen_inb, x3, top, x3, H);
                                        else g.DrawLine(grid_pen_inb, x3, top, x3, H);
                                    }
                                }
                            }

                            if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                            {
                                label = actual_fgrid.ToString("f3"); //wa6ahl

                                // if(actual_fgrid > 1300.0)
                                //label = label.Substring(label.Length-4);

                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                            }
                            else
                            {
                                //display freqencies
                                string temp_string;
                                int jper;
                                //label = actual_fgrid.ToString("f5"); //x100
                                label = actual_fgrid.ToString("f4");
                                temp_string = label;
                                jper = label.IndexOf('.') + 4;
                                label = label.Insert(jper, " ");

                                //if(actual_fgrid > 1300.0)
                                //	label = label.Substring(label.Length-4);

                                if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                                else offsetL = (int)((label.Length) * 4.1) - 8;
                            }

                            if (bottom)
                            {
                                if (local_mox) g.DrawString(label, font9, grid_tx_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                            }
                            else
                            {
                                if (local_mox) g.DrawString(label, font9, grid_tx_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                            }
                            break;
                    }
                }
                else
                {
                    vgrid = Convert.ToInt32((double)-(fgrid - Low) / (Low - High) * W);	//wa6ahl
                    if (bottom)
                    {
                        if (local_mox) g.DrawLine(tx_vgrid_pen, vgrid, H + top, vgrid, H + H);
                        else g.DrawLine(grid_pen, vgrid, H + top, vgrid, H + H);
                    }
                    else
                    {
                        if (local_mox) g.DrawLine(tx_vgrid_pen, vgrid, top, vgrid, H);
                        else g.DrawLine(grid_pen, vgrid, top, vgrid, H);			//wa6ahl
                    }
                    //double new_fgrid = (vfoa_hz + fgrid) / 1000000;

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        if (bottom)
                        {
                            if (local_mox) g.DrawString(label, font9, grid_tx_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                            else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                        }
                        else
                        {
                            if (local_mox) g.DrawString(label, font9, grid_tx_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                            else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                        }
                    }
                }
            }

            switch (console.CurrentRegion)
            {
                case FRSRegion.Australia:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000, 
				 3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 
				 18168000, 21000000, 21450000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.UK:
                    band_edge_list = new int[] { 472000, 479000, 1810000, 2000000, 3500000, 3800000,
				5258500, 5406500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
				21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
                    break;
                case FRSRegion.India:
                    band_edge_list = new int[]{ 1810000, 1860000, 3500000, 3900000, 7000000, 7200000, 
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
				24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Norway:
                    band_edge_list = new int[]{ 1800000, 2000000, 3500000, 4000000, 5260000, 5410000,
				7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
				24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.US:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
				7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
				24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Japan:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1810000, 1907500, 1912500, 
                3500000, 3575000, 3599000, 3612000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
				7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
				24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                default:
                    band_edge_list = new int[]{1800000, 2000000, 3500000, 4000000, 7000000, 7300000, 
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
				24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
            }

            for (int i = 0; i < band_edge_list.Length; i++)
            {
                double band_edge_offset = band_edge_list[i] - vfo;
                if (band_edge_offset >= Low && band_edge_offset <= High)
                {
                    int temp_vline = (int)((double)(band_edge_offset - Low) / width * W);//wa6ahl
                    if (bottom)
                    {
                        if (local_mox) g.DrawLine(tx_band_edge_pen, temp_vline, H + top, temp_vline, H + H);
                        else g.DrawLine(band_edge_pen, temp_vline, H + top, temp_vline, H + H);//wa6ahl                        
                    }
                    else
                    {
                        if (local_mox) g.DrawLine(tx_band_edge_pen, temp_vline, top, temp_vline, H);
                        else g.DrawLine(band_edge_pen, temp_vline, top, temp_vline, H);//wa6ahl
                    }
                }
                //if(i == 1 && !show_freq_offset) break;
            }

            if (grid_control)
            {
                // Draw horizontal lines
                for (int i = 1; i < h_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)((double)(grid_max - num) * H / y_range);

                    if (bottom)
                    {
                        if (local_mox) g.DrawLine(tx_hgrid_pen, 0, H + y, W, H + y);
                        else g.DrawLine(hgrid_pen, 0, H + y, W, H + y);
                    }
                    else
                    {
                        if (local_mox) g.DrawLine(tx_hgrid_pen, 0, y, W, y);
                        else g.DrawLine(hgrid_pen, 0, y, W, y);
                    }
                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        num = grid_max - i * grid_step;
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)g.MeasureString("-", font9).Width - 2;
                        // int offset = (int)(label.Length * 4.1);
                        SizeF size = g.MeasureString(label, font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        console.DisplayGridX = x;
                        console.DisplayGridW = (int)(x + size.Width);
                        // label = label + size.Width.ToString();
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom)
                            {
                                if (local_mox)
                                    g.DrawString(label, font9, grid_tx_text_brush, x, H + y);
                                else
                                    g.DrawString(label, font9, grid_text_brush, x, H + y);
                            }

                            else
                            {
                                if (local_mox)
                                    g.DrawString(label, font9, grid_tx_text_brush, x, y);
                                else
                                    g.DrawString(label, font9, grid_text_brush, x, y);
                            }
                        }
                    }
                }
            }

            // draw long cursor & filter overlay
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p;
                p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                //else if (current_click_tune_mode == ClickTuneMode.VFOAC)
                //  p = Pens.Blue;
                // else p = Pens.Green;
                if (bottom)
                {
                    if (ClickTuneFilter)
                    {
                        if (display_cursor_y > H)
                        {
                            double freq_low = freq + filter_low;
                            double freq_high = freq + filter_high;
                            int x1 = (int)((freq_low - Low) / width * W);
                            int x2 = (int)((freq_high - Low) / width * W);
                            //g.FillRectangle(display_filter_brush, x1, top, x2 - x1, H - top);

                            if (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)
                            {
                                g.FillRectangle(display_filter_brush, display_cursor_x -
                                    ((x2 - x1) / 2), H + top, x2 - x1, H + H - top);
                            }
                            else
                            {
                                g.FillRectangle(display_filter_brush, x1, H + top, x2 - x1, H + H - top);
                            }
                        }

                        if (display_cursor_y > H)
                        {
                            g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
                            if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                        }
                    }
                }
                else
                {
                    if (ClickTuneFilter)
                    {
                        if (display_cursor_y <= H)
                        {
                            double freq_low = freq + filter_low;
                            double freq_high = freq + filter_high;
                            int x1 = (int)((freq_low - Low) / width * W);
                            int x2 = (int)((freq_high - Low) / width * W);
                            //g.FillRectangle(display_filter_brush, x1, top, x2 - x1, H - top);

                            if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
                            {
                                g.FillRectangle(display_filter_brush, display_cursor_x -
                                    ((x2 - x1) / 2), top, x2 - x1, H - top);
                            }
                            else
                            {
                                g.FillRectangle(display_filter_brush, x1, top, x2 - x1, H - top);
                            }
                        }

                        if (display_cursor_y <= H)
                        {
                            g.DrawLine(p, display_cursor_x, top, display_cursor_x, H);
                            if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                        }
                    }
                }
            }

            if (console.PowerOn && (current_display_mode == DisplayMode.PANADAPTER ||
                current_display_mode == DisplayMode.PANAFALL ||
                current_display_mode == DisplayMode.PANASCOPE))
            {
                // get filter screen coordinates
                int filter_left_x = (int)((float)(filter_low - Low) / width * W);
                int filter_right_x = (int)((float)(filter_high - Low) / width * W);
                if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                int x1_rx1_gain = 0, x2_rx1_gain = 0, x3_rx1_gain = 0, x1_rx1_hang = 0, x2_rx1_hang = 0, x3_rx1_hang = 0;
                int x1_rx2_gain = 0, x2_rx2_gain = 0, x3_rx2_gain = 0, x1_rx2_hang = 0, x2_rx2_hang = 0, x3_rx2_hang = 0;

                if (rx == 1)
                {
                    if (spectrum_line)
                    {
                        x1_rx1_gain = 40;
                        x2_rx1_gain = W - 40;
                        x3_rx1_gain = 50;
                    }
                    else
                    {
                        x1_rx1_gain = filter_left_x;
                        x2_rx1_gain = filter_right_x;
                        x3_rx1_gain = x1_rx1_gain;
                    }

                    if (rx1_hang_spectrum_line)
                    {
                        x1_rx1_hang = 40;
                        x2_rx1_hang = W - 40;
                        x3_rx1_hang = 50;
                    }
                    else
                    {
                        x1_rx1_hang = filter_left_x;
                        x2_rx1_hang = filter_right_x;
                        x3_rx1_hang = x1_rx1_hang;
                    }
                }
                else
                {
                    if (rx2_gain_spectrum_line)
                    {
                        x1_rx2_gain = 40;
                        x2_rx2_gain = W - 40;
                        x3_rx2_gain = 50;
                    }
                    else
                    {
                        x1_rx2_gain = filter_left_x;
                        x2_rx2_gain = filter_right_x;
                        x3_rx2_gain = x1_rx2_gain;
                    }

                    if (rx2_hang_spectrum_line)
                    {
                        x1_rx2_hang = 40;
                        x2_rx2_hang = W - 40;
                        x3_rx2_hang = 50;
                    }
                    else
                    {
                        x1_rx2_hang = filter_left_x;
                        x2_rx2_hang = filter_right_x;
                        x3_rx2_hang = x1_rx2_hang;
                    }
                }

                if (rx == 1 && !local_mox)
                {
                    float rx1_cal_offset = 0.0f;
                    switch (console.RX1AGCMode)
                    {
                        case AGCMode.FIXD:
                            rx1_cal_offset = -18.0f;
                            break;
                        default:
                            rx1_cal_offset = 2.0f + (rx1_display_cal_offset +
                                (rx1_preamp_offset - alex_preamp_offset) - rx1_fft_size_offset);
                            break;
                    }
                    // get AGC-T level
                    double rx1_thresh = 0.0;
                    float rx1_agcknee_y_value = 0.0f;
                    // DttSP.GetRXAGCThresh(0, 0, &rx1_thresh);
                    WDSP.GetRXAAGCThresh(WDSP.id(0, 0), &rx1_thresh, 4096.0, sample_rate);

                    rx1_thresh = Math.Round(rx1_thresh);

                    // get Hang Threshold level
                    double rx1_hang = 0.0;
                    float rx1_agc_hang_y = 0.0f;
                    //DttSP.GetRXAGCHangLevel(0, 0, &rx1_hang);
                    WDSP.GetRXAAGCHangLevel(WDSP.id(0, 0), &rx1_hang);
                    int rx1_agc_fixed_gain = console.SetupForm.AGCFixedGain;
                    string rx1_agc = "";
                    switch (console.RX1AGCMode)
                    {
                        case AGCMode.FIXD:
                            rx1_agcknee_y_value = dBToPixel(-(float)rx1_agc_fixed_gain + rx1_cal_offset);
                            // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                            rx1_agc = "-F";
                            break;
                        default:
                            rx1_agcknee_y_value = dBToPixel((float)rx1_thresh + rx1_cal_offset);
                            rx1_agc_hang_y = dBToPixel((float)rx1_hang + rx1_cal_offset);
                            if (console.RX2Enabled || split_display)
                                rx1_agc_hang_y = rx1_agc_hang_y / 2;
                            //show hang line
                            if (display_agc_hang_line && console.RX1AGCMode != AGCMode.MED && console.RX1AGCMode != AGCMode.FAST)
                            {
                                AGCHang.Height = 8; AGCHang.Width = 8; AGCHang.X = 40;
                                AGCHang.Y = (int)rx1_agc_hang_y - AGCHang.Height;
                                g.FillRectangle(Brushes.Yellow, AGCHang);
                                using (Pen p = new Pen(Color.Yellow))
                                {
                                    p.DashStyle = DashStyle.Dot;
                                    g.DrawLine(p, x3_rx1_hang, rx1_agc_hang_y, x2_rx1_hang, rx1_agc_hang_y);
                                    g.DrawString("-H", pana_font, pana_text_brush, AGCHang.X + AGCHang.Width, AGCHang.Y - (AGCHang.Height / 2));
                                }
                            }
                            rx1_agc = "-G";
                            break;
                    }

                    if (console.RX2Enabled || split_display)
                        rx1_agcknee_y_value = rx1_agcknee_y_value / 2;
                    // show agc line
                    if (show_agc)
                    {
                        AGCKnee.Height = 8; AGCKnee.Width = 8; AGCKnee.X = 40;
                        AGCKnee.Y = (int)rx1_agcknee_y_value - AGCKnee.Height;
                        g.FillRectangle(Brushes.YellowGreen, AGCKnee);
                        using (Pen p = new Pen(Color.YellowGreen))
                        {
                            p.DashStyle = DashStyle.Dot;
                            g.DrawLine(p, x1_rx1_gain, rx1_agcknee_y_value, x2_rx1_gain, rx1_agcknee_y_value);
                            g.DrawString(rx1_agc, pana_font, pana_text_brush, AGCKnee.X + AGCKnee.Width, AGCKnee.Y - (AGCKnee.Height / 2));
                        }
                    }
                }
                else if (rx == 2 && !local_mox)
                {
                    float rx2_cal_offset = 0.0f;
                    double rx2_thresh = 0.0;
                    float rx2_agcknee_y_value = 0.0f;
                    double rx2_hang = 0.0;
                    float rx2_agc_hang_y = 0.0f;
                    string rx2_agc = "";
                    int rx2_agc_fixed_gain = 0;

                    switch (console.RX2AGCMode)
                    {
                        case AGCMode.FIXD:
                            rx2_cal_offset = -18.0f;
                            break;
                        default:
                            rx2_cal_offset = 2.0f + (rx1_display_cal_offset +
                                  rx2_preamp_offset) - rx2_fft_size_offset;
                            break;
                    }
                    // get AGC-T level
                    // DttSP.GetRXAGCThresh(2, 0, &rx2_thresh);
                    WDSP.GetRXAAGCThresh(WDSP.id(2, 0), &rx2_thresh, 4096.0, sample_rate);
                    rx2_thresh = Math.Round(rx2_thresh);

                    // DttSP.GetRXAGCHangLevel(2, 0, &rx2_hang);
                    WDSP.GetRXAAGCHangLevel(WDSP.id(2, 0), &rx2_hang);
                    rx2_agc_fixed_gain = console.SetupForm.AGCRX2FixedGain;

                    // g.DrawString("RX2HangT: " + rx2_hang.ToString("f3"),
                    // new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Red), 50, 30);
                    //  g.DrawString("RX2AGCT: " + rx2_thresh.ToString("f3"),
                    // new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Red), 50, 50);

                    switch (console.RX2AGCMode)
                    {
                        case AGCMode.FIXD:
                            rx2_agcknee_y_value = dBToRX2Pixel(-(float)rx2_agc_fixed_gain + rx2_cal_offset);
                            // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                            rx2_agc = "-F";
                            break;
                        default:
                            rx2_agcknee_y_value = dBToRX2Pixel((float)rx2_thresh + rx2_cal_offset);
                            rx2_agc_hang_y = dBToRX2Pixel((float)rx2_hang + rx2_cal_offset + rx2_fft_size_offset);
                            rx2_agc_hang_y *= 0.5f;
                            if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
                            {
                                AGCRX2Hang.Height = 8; AGCRX2Hang.Width = 8; AGCRX2Hang.X = 40;
                                AGCRX2Hang.Y = ((int)rx2_agc_hang_y + H) - AGCRX2Hang.Height;
                                g.FillRectangle(Brushes.Yellow, AGCRX2Hang);
                                using (Pen p = new Pen(Color.Yellow))
                                {
                                    p.DashStyle = DashStyle.Dot;
                                    g.DrawLine(p, x3_rx2_hang, rx2_agc_hang_y + H, x2_rx2_hang, rx2_agc_hang_y + H);
                                    g.DrawString("-H", pana_font, pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - (AGCRX2Hang.Height / 2));
                                }
                            }
                            rx2_agc = "-G";
                            break;
                    }

                    rx2_agcknee_y_value *= 0.5f;
                    if (display_rx2_gain_line)
                    {
                        AGCRX2Knee.Height = 8; AGCRX2Knee.Width = 8; AGCRX2Knee.X = 40;
                        AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + H) - AGCRX2Knee.Height;
                        g.FillRectangle(Brushes.YellowGreen, AGCRX2Knee);
                        using (Pen p = new Pen(Color.YellowGreen))
                        {
                            p.DashStyle = DashStyle.Dot;
                            g.DrawLine(p, x1_rx2_gain, rx2_agcknee_y_value + H, x2_rx2_gain, rx2_agcknee_y_value + H);
                            g.DrawString(rx2_agc, pana_font, pana_text_brush, AGCRX2Knee.X + AGCRX2Knee.Width, AGCRX2Knee.Y - (AGCRX2Knee.Height / 2));
                        }
                    }
                }
            }
            if (high_swr && rx == 1)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);

            // g.DrawString(UDPbuf, font14, Brushes.Red, 245, 20);


            // ke9ns add draw DX SPOTS on pandapter
            //=====================================================================
            //=====================================================================

            if (SpotControl.SP_Active != 0)
            {

                int iii = 0;                          // ke9ns add stairstep holder

                int kk = 0;                           // ke9ns add index for holder[] after you draw the vert line, then draw calls (so calls can overlap the vert lines)

                int vfo_hz = (int)vfoa_hz;    // vfo freq in hz

                int H1a = H / 2;            // length of vertical line (based on rx1 and rx2 display window configuration)
                int H1b = 20;               // starting point of vertical line

                // RX1/RX2 PanF/Pan = 5,2 (K9,K10)(short)  PanF/PanF = 5,5, (short) Pan/Pan 2,2 (long)
                if (bottom)                 // if your drawing to the bottom 
                {
                    if ((K9 == 2) && (K10 == 2)) H1a = H + (H / 2); // long
                    else H1a = H + (H / 4); // short

                    H1b = H + 20;

                    vfo_hz = (int)vfob_hz;
                    Console.DXK2 = 0;        // RX2 index to allow call signs to draw after all the vert lines on the screen

                }
                else
                {
                    Console.DXK = 0;        // RX1 index to allow call signs to draw after all the vert lines on the screen
                }

                VFOLow = vfo_hz + RXDisplayLow;    // low freq (left side) in hz
                VFOHigh = vfo_hz + RXDisplayHigh; // high freq (right side) in hz
                VFODiff = VFOHigh - VFOLow;       // diff in hz

                if ((vfo_hz < 5000000) || ((vfo_hz > 6000000) && (vfo_hz < 8000000))) low = true; // LSB
                else low = false;     // usb

                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------
                // draw DX spots
                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------

                for (int ii = 0; ii < SpotControl.DX_Index; ii++)     // Index through entire DXspot to find what is on this panadapter (draw vert lines first)
                {

                    if ((SpotControl.DX_Freq[ii] >= VFOLow) && (SpotControl.DX_Freq[ii] <= VFOHigh))
                    {
                        int VFO_DXPos = (int)((((float)W / (float)VFODiff) * (float)(SpotControl.DX_Freq[ii] - VFOLow))); // determine DX spot line pos on current panadapter screen

                        holder[kk] = ii;                    // ii is the actual DX_INdex pos the the KK holds
                        holder1[kk] = VFO_DXPos;

                        kk++;

                        g.DrawLine(p1, VFO_DXPos, H1b, VFO_DXPos, H1a);   // draw vertical line

                    }

                } // for loop through DX_Index


                int bb = 0;
                if (bottom)
                {
                    Console.DXK2 = kk; // keep a count for the bottom QRZ hyperlink
                    bb = Console.MMK4;
                }
                else
                {
                    Console.DXK = kk; // count of spots in current panadapter
                    bb = Console.MMK3;
                }


                //--------------------------------------------------------------------------------------------
                for (int ii = 0; ii < kk; ii++) // draw call signs to screen in order to draw over the vert lines
                {

                    // font
                    if (low) // 1=LSB so draw on left side of line
                    {

                        if (Console.DXR == 0) // display Spotted on Pan
                        {
                            length = g.MeasureString(SpotControl.DX_Station[holder[ii]], font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii] - (int)length.Width;
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            g.DrawString(SpotControl.DX_Station[holder[ii]], font1, grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii);
                        }
                        else // display SPOTTER on Pan (not the Spotted)
                        {
                            length = g.MeasureString(SpotControl.DX_Spotter[holder[ii]], font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii] - (int)length.Width;
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            g.DrawString(SpotControl.DX_Spotter[holder[ii]], font1, grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii);

                        }

                        if (bottom) rx2 = 50; // allow only 50 qrz spots per Receiver
                        else rx2 = 0;

                        if (!mox) // only do when not transmitting
                        {
                            Console.DXW[ii + rx2] = (int)length.Width;    // this is all for QRZ hyperlinking 
                            Console.DXH[ii + rx2] = (int)length.Height;
                            Console.DXX[ii + rx2] = holder1[ii] - (int)length.Width;
                            Console.DXY[ii + rx2] = H1b + iii;
                            Console.DXS[ii + rx2] = SpotControl.DX_Station[holder[ii]];

                        }


                    } // LSB side


                    else   // 0=usb so draw on righ side of line (normal)
                    {
                        if (Console.DXR == 0) // spot
                        {
                            length = g.MeasureString(SpotControl.DX_Station[holder[ii]], font1); //  not needed here but used for qrz hyperlinking

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii];
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            g.DrawString(SpotControl.DX_Station[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii); // DX station name
                        }
                        else // spotter
                        {
                            length = g.MeasureString(SpotControl.DX_Spotter[holder[ii]], font1); //  not needed here but used for qrz hyperlinking

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii];
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            g.DrawString(SpotControl.DX_Spotter[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii); // DX station name

                        }

                        if (bottom) rx2 = 50;
                        else rx2 = 0;

                        if (!mox) // only do when not transmitting
                        {
                            Console.DXW[ii + rx2] = (int)length.Width;   // this is all for QRZ hyperlinking 
                            Console.DXH[ii + rx2] = (int)length.Height;
                            Console.DXX[ii + rx2] = holder1[ii];
                            Console.DXY[ii + rx2] = H1b + iii;
                            Console.DXS[ii + rx2] = SpotControl.DX_Station[holder[ii]];
                        }

                        if (vfo_hz >= 50000000) // 50000000 or 50mhz
                        {
                            iii = iii + 11;
                            g.DrawString(SpotControl.DX_Grid[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii); // DX grid name
                        }

                    } // USB side

                    iii = iii + 11;
                    if (iii > 90) iii = 0;


                }// for loop through DX_Index

            }
        }

        private static float dBToPixel(float dB)
        {
            return (float)(spectrum_grid_max - dB) * Target.Height / (spectrum_grid_max - spectrum_grid_min);
        }

        private static float dBToRX2Pixel(float dB)
        {
            return (float)(rx2_spectrum_grid_max - dB) * Target.Height / (rx2_spectrum_grid_max - rx2_spectrum_grid_min);
        }

        private static float PixelToDb(float y)
        {
            if (y <= H / 2) y *= 2.0f;
            else y = (y - H / 2) * 2.0f;

            return (float)(spectrum_grid_max - y * (double)(spectrum_grid_max - spectrum_grid_min) / H);
        }

        private static void DrawWaterfallGrid(ref Graphics g, int W, int H, int rx, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            int low = 0;					// initialize variables
            int high = 0;
            int mid_w = W / 2;
            int[] step_list = { 10, 20, 25, 50 };
            int step_power = 1;
            int step_index = 0;
            int freq_step_size = 50;
            int filter_low, filter_high;
            bool local_mox = false;
            int grid_max = 0;
            int grid_min = 0;
            int f_diff = 0;

            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;

            if (rx == 2)
            {
                if (local_mox)
                {
                    low = tx_display_low;
                    high = tx_display_high;
                    //low = rx2_display_low;
                    //high = rx2_display_high;
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    low = rx2_display_low;
                    high = rx2_display_high;
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                }

                f_diff = rx2_freq_diff;
            }
            else
            {
                if (local_mox && !display_duplex ||
                    mox && tx_on_vfob && !rx2_enabled && !display_duplex)
                {
                    low = tx_display_low;
                    high = tx_display_high;
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    low = rx_display_low;
                    high = rx_display_high;
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                }

                f_diff = freq_diff;
            }

            int center_line_x; 
            int y_range = grid_max - grid_min;

            if (rx == 1)
            {
                //if (mox)
                if (local_mox  ||
                    mox && tx_on_vfob && !rx2_enabled)
                {
                    filter_low = tx_filter_low;
                    filter_high = tx_filter_high;
                }
                else
                {
                    filter_low = rx1_filter_low;
                    filter_high = rx1_filter_high;
                }

            }
            else //if(rx==2)
            {
                if (!local_mox)
                {
                    filter_low = rx2_filter_low;
                    filter_high = rx2_filter_high;
                }
                else
                {
                    filter_low = tx_filter_low;
                    filter_high = tx_filter_high;
                }
            }

            if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
                (rx2_dsp_mode == DSPMode.DRM && rx == 2))
            {
                filter_low = -5000;
                filter_high = 5000;
            }

            // Calculate horizontal step size
            int width = high - low;
            while (width / freq_step_size > 10)
            {
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }
            double w_pixel_step = (double)W * freq_step_size / width;
            int w_steps = width / freq_step_size;

            // calculate vertical step size
            int h_steps = (spectrum_grid_max - spectrum_grid_min) / spectrum_grid_step;
            double h_pixel_step = (double)H / h_steps;
            int top = (int)((double)spectrum_grid_step * H / y_range);
            if (bottom) top *= 2;

            if (!bottom)
            {
                if (!mox && sub_rx1_enabled)
                {
                    // draw Sub RX 0Hz line
                    int x = (int)((float)(vfoa_sub_hz - vfoa_hz - low) / (high - low) * W);
                    g.DrawLine(sub_rx_zero_line_pen, x, 0, x, top);
                    g.DrawLine(sub_rx_zero_line_pen, x - 1, 0, x - 1, top);

                    // draw Sub RX filter
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - low + vfoa_sub_hz - vfoa_hz) / (high - low) * W);
                    int filter_right_x = (int)((float)(filter_high - low + vfoa_sub_hz - vfoa_hz) / (high - low) * W);

                    // make the filter display at least one pixel wide.
                    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                    // draw rx filter
                    g.FillRectangle(display_filter_brush,	// draw filter overlay
                    filter_left_x, 0, filter_right_x - filter_left_x, top);
                }

                if (!(mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)))
                {
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - low - f_diff) / (high - low) * W);
                    int filter_right_x = (int)((float)(filter_high - low - f_diff) / (high - low) * W);

                    // make the filter display at least one pixel wide.
                    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                    // draw rx filter
                    g.FillRectangle(display_filter_brush,	// draw filter overlay
                     filter_left_x, 0, filter_right_x - filter_left_x, top + 10);

                }

                if (!mox && draw_tx_filter &&
                    (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
                {
                    // get tx filter limits
                    int filter_left_x;
                    int filter_right_x;

                    if (!split_enabled)
                    {
                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz) / (high - low) * W);
                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz) / (high - low) * W);
                    }
                    else
                    {
                        filter_left_x = (int)((float)(tx_filter_low - low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);
                        filter_right_x = (int)((float)(tx_filter_high - low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);
                    }

                    g.DrawLine(tx_filter_pen, filter_left_x, H, filter_left_x, H + top);		// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_left_x + 1, H, filter_left_x + 1, H + top);	// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x, H, filter_right_x, H + top);		// draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x + 1, H, filter_right_x + 1, H + top);	// draw tx filter overlay
                }

                if (!mox && draw_tx_cw_freq &&
                    (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    int pitch = cw_pitch;
                    if (rx1_dsp_mode == DSPMode.CWL)
                        pitch = -cw_pitch;

                    int cw_line_x;
                    if (!split_enabled)
                        cw_line_x = (int)((float)(pitch - low + xit_hz) / (high - low) * W);
                    else
                        cw_line_x = (int)((float)(pitch - low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / (high - low) * W);

                    g.DrawLine(tx_filter_pen, cw_line_x, top, cw_line_x, H);
                    g.DrawLine(tx_filter_pen, cw_line_x + 1, top, cw_line_x + 1, H);
                }
            }
            else
            {
                if (!(mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
                {
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - low - f_diff) / (high - low) * W);
                    int filter_right_x = (int)((float)(filter_high - low - f_diff) / (high - low) * W);

                    // make the filter display at least one pixel wide.
                    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                    // draw rx filter
                    g.FillRectangle(display_filter_brush, filter_left_x, H,
                        filter_right_x - filter_left_x, top);

                }

            }
            double vfo;

            /*     if (mox)
                 {
                     vfo = split_enabled ? vfoa_sub_hz : vfoa_hz;
                     vfo += xit_hz;
                 }
                 else if (rx == 1)
                 {
                     vfo = vfoa_hz + rit_hz;
                 }
                 else //if(rx==2)
                 {
                     vfo = vfob_hz + rit_hz;
                 }

                 switch (rx1_dsp_mode)
                 {
                     case DSPMode.CWL:
                         vfo += cw_pitch;
                         break;
                     case DSPMode.CWU:
                         vfo -= cw_pitch;
                         break;
                     default:
                         break;
                 }
                 */
            if (rx == 1)
            {
                if (local_mox)
                {
                    if (split_enabled) vfo = vfoa_sub_hz;
                    else vfo = vfoa_hz;
                    vfo += xit_hz;
                }
                else if (mox && tx_on_vfob)
                {
                    if (console.RX2Enabled) vfo = vfoa_hz + rit_hz;
                    else vfo = vfoa_sub_hz;
                }
                else vfo = vfoa_hz + rit_hz;
            }
            else //if(rx==2)
            {
                if (local_mox)
                    vfo = vfob_hz + xit_hz;
                else vfo = vfob_hz + rit_hz;
            }

            if (!bottom)
            {
                switch (rx1_dsp_mode)
                {
                    case DSPMode.CWL:
                        vfo += cw_pitch;
                        break;
                    case DSPMode.CWU:
                        vfo -= cw_pitch;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (rx2_dsp_mode)
                {
                    case DSPMode.CWL:
                        vfo += cw_pitch;
                        break;
                    case DSPMode.CWU:
                        vfo -= cw_pitch;
                        break;
                    default:
                        break;
                }
            }

            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            // Draw vertical lines
            for (int i = 0; i <= h_steps + 1; i++)
            {
                string label;
                int offsetL;
                int offsetR;

                int fgrid = i * freq_step_size + (low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - low) / (high - low) * W);

                if (!show_freq_offset)
                {
                    switch (console.CurrentRegion)
                    {
                        case FRSRegion.US:
                            if (actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
                            actual_fgrid == 3.5 || actual_fgrid == 4.0 ||
                            actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                            actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                            actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                            actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                            actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                            actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                            actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                            actual_fgrid == 50.0 || actual_fgrid == 54.0 ||
                            actual_fgrid == 144.0 || actual_fgrid == 148.0)
                            {
                                /* if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                 else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);*/

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.India:
                            if (actual_fgrid == 1.81 || actual_fgrid == 1.86 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.Spain:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.Europe:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.UK:
                            if (actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 5.2585 || actual_fgrid == 5.4065 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.Italy_Plus:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 6.975 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.Japan:
                            if (actual_fgrid == .137 || actual_fgrid == .138 ||
                            actual_fgrid == 1.81 || actual_fgrid == 1.825 ||
                            actual_fgrid == 1.907 || actual_fgrid == 1.913 ||
                            actual_fgrid == 3.5 || actual_fgrid == 3.575 ||
                            actual_fgrid == 3.599 || actual_fgrid == 3.612 ||
                            actual_fgrid == 3.68 || actual_fgrid == 3.687 ||
                            actual_fgrid == 3.702 || actual_fgrid == 3.716 ||
                            actual_fgrid == 3.745 || actual_fgrid == 3.77 ||
                            actual_fgrid == 3.791 || actual_fgrid == 3.805 ||
                            actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                            actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                            actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                            actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                            actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                            actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                            actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                            actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        case FRSRegion.Australia:
                            if (actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                if (bottom) g.DrawLine(band_edge_pen, vgrid, H + top, vgrid, H + H);
                                else g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f3");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                if (bottom) g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                else g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, (float)Math.Floor(H * .01));

                                break;
                            }
                            else
                                goto default;

                        default:
                            {

                                if (freq_step_size >= 2000)
                                {
                                    double t100;
                                    double t1000;
                                    t100 = (actual_fgrid * 100);
                                    t1000 = (actual_fgrid * 1000);

                                    int it100 = (int)t100;
                                    int it1000 = (int)t1000;

                                    int it100x10 = it100 * 10;

                                    if (it100x10 == it1000)
                                    {
                                    }
                                    else
                                    {
                                        grid_pen.DashStyle = DashStyle.Dot;
                                    }
                                }
                                else
                                {
                                    if (freq_step_size == 1000)
                                    {
                                        double t200;
                                        double t2000;
                                        t200 = (actual_fgrid * 200);
                                        t2000 = (actual_fgrid * 2000);

                                        int it200 = (int)t200;
                                        int it2000 = (int)t2000;

                                        int it200x10 = it200 * 10;

                                        if (it200x10 == it2000)
                                        {
                                        }
                                        else
                                        {
                                            grid_pen.DashStyle = DashStyle.Dot;
                                        }
                                    }
                                    else
                                    {
                                        double t1000;
                                        double t10000;
                                        t1000 = (actual_fgrid * 1000);
                                        t10000 = (actual_fgrid * 10000);

                                        int it1000 = (int)t1000;
                                        int it10000 = (int)t10000;

                                        int it1000x10 = it1000 * 10;

                                        if (it1000x10 == it10000)
                                        {
                                        }
                                        else
                                        {
                                            grid_pen.DashStyle = DashStyle.Dot;
                                        }
                                    }
                                }
                                /* if (bottom) g.DrawLine(grid_pen, vgrid, H + top, vgrid, H + H);
                                 else g.DrawLine(grid_pen, vgrid, top, vgrid, H);			//wa6ahl
                                 grid_pen.DashStyle = DashStyle.Solid;*/

                                if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                                {
                                    label = actual_fgrid.ToString("f3"); //wa6ahl

                                    //if(actual_fgrid > 1300.0)
                                    //	label = label.Substring(label.Length-4);

                                    if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                    else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                    else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                                }
                                else
                                {
                                    string temp_string;
                                    int jper;
                                    label = actual_fgrid.ToString("f4");
                                    temp_string = label;
                                    jper = label.IndexOf('.') + 4;
                                    label = label.Insert(jper, " ");

                                    //if(actual_fgrid > 1300.0)
                                    //	label = label.Substring(label.Length-4);

                                    if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                                    else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                                    else offsetL = (int)((label.Length) * 4.1) - 8;
                                }
                                switch (current_display_mode)  //w3sz added switch for waterfall frequency labels
                                {
                                    // case DisplayMode.PANAFALL:
                                    //  break;
                                    default:
                                        if (bottom) g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                                        else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                                        break;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    /* vgrid = Convert.ToInt32((double)-(fgrid-low)/(low-high)*W);	//wa6ahl
                    if(bottom) g.DrawLine(grid_pen, vgrid, H+top, vgrid, H+H);
                    else g.DrawLine(grid_pen, vgrid, top, vgrid, H); */
                    //wa6ahl

                    //double new_fgrid = (vfoa_hz + fgrid) / 1000000;

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        if (bottom) g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, H + (float)Math.Floor(H * .01));
                        else g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, (float)Math.Floor(H * .01));
                    }
                }
            }

            /*   int[] band_edge_list = { 1800000, 2000000, 3500000, 4000000,
                                          7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 21000000, 21450000,
                                          24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
			
               for(int i=0; i<band_edge_list.Length; i++)
               {
                   double band_edge_offset = band_edge_list[i] - vfo;
                   if (band_edge_offset >= low && band_edge_offset <= high)
                   {
                       int temp_vline =  (int)((double)(band_edge_offset-low)/(high-low)*W);//wa6ahl
                       if(bottom) g.DrawLine(new Pen(band_edge_color), temp_vline, H, temp_vline, H+top);//wa6ahl
                       else g.DrawLine(new Pen(band_edge_color), temp_vline, 0, temp_vline, top);
                   }
                   if(i == 1 && !show_freq_offset) break;
               } */

            /*// Draw horizontal lines
            for(int i=1; i<h_steps; i++)
            {
                int xOffset = 0;
                int num = spectrum_grid_max - i*spectrum_grid_step;
                int y = (int)((double)(spectrum_grid_max - num)*H/y_range);
                g.DrawLine(grid_pen, 0, y, W, y);

                // Draw horizontal line labels
                if(i != 1) // avoid intersecting vertical and horizontal labels
                {
                    num = spectrum_grid_max - i*spectrum_grid_step;
                    string label = num.ToString();
                    if(label.Length == 3) xOffset = 7;
                    //int offset = (int)(label.Length*4.1);
                    if(display_label_align != DisplayLabelAlignment.LEFT &&
                        display_label_align != DisplayLabelAlignment.AUTO &&
                        (current_dsp_mode == DSPMode.USB ||
                        current_dsp_mode == DSPMode.CWU))
                        xOffset -= 32;
                    SizeF size = g.MeasureString(label, font);

                    int x = 0;
                    switch(display_label_align)
                    {
                        case DisplayLabelAlignment.LEFT:
                            x = xOffset + 3;
                            break;
                        case DisplayLabelAlignment.CENTER:
                            x = center_line_x+xOffset;
                            break;
                        case DisplayLabelAlignment.RIGHT:
                            x = (int)(W-size.Width);
                            break;
                        case DisplayLabelAlignment.AUTO:
                            x = xOffset + 3;
                            break;
                        case DisplayLabelAlignment.OFF:
                            x = W;
                            break;
                    }

                    y -= 8;
                    if(y+9 < H)
                        g.DrawString(label, font, grid_text_brush, x, y);
                }
            }*/

            // Draw 0Hz vertical line if visible
            center_line_x = (int)((float)(-f_diff - low) / width * W); // locked 0 line

            if (center_line_x >= 0 && center_line_x <= W)
            {
                if (!bottom)
                {
                    g.DrawLine(new Pen(grid_zero_color), center_line_x, 0, center_line_x, top + 10);
                    g.DrawLine(new Pen(grid_zero_color), center_line_x + 1, 0, center_line_x + 1, top + 10);
                }
                else
                {
                    g.DrawLine(new Pen(grid_zero_color), center_line_x, H, center_line_x, H + top);
                    g.DrawLine(new Pen(grid_zero_color), center_line_x + 1, H, center_line_x + 1, H + top);
                }
            }

            if (show_freq_offset)
            {
                if (bottom) g.DrawString("0", font9, grid_zero_pen.Brush, center_line_x - 5, H + (float)Math.Floor(H * .01));
                else g.DrawString("0", font9, grid_zero_pen.Brush, center_line_x - 5, (float)Math.Floor(H * .01));
            }

            if (high_swr && !bottom)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
        }

        private static void DrawOffBackground(Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            if (high_swr && !bottom)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
        }

        private static float[] scope_min = new float[W];
        public static float[] ScopeMin
        {
            get { return scope_min; }
            set { scope_min = value; }
        }
        private static float[] scope_max = new float[W];
        public static float[] ScopeMax
        {
            get { return scope_max; }
            set { scope_max = value; }
        }
        unsafe private static bool DrawScope(Graphics g, int W, int H, bool bottom)
        {
            if (scope_min.Length < W)
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }
            if (scope_max.Length < W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }

            DrawScopeGrid(ref g, W, H, bottom);

            Point[] points = new Point[W * 2];			// create Point array
            for (int i = 0; i < W; i++)						// fill point array
            {
                int pixel = 0;
                if (bottom) pixel = (int)(H / 2 * scope_max[i]);
                else pixel = (int)(H / 2 * scope_max[i]);
                int y = H / 2 - pixel;
                points[i].X = i;
                points[i].Y = y;
                if (bottom) points[i].Y += H;

                if (bottom) pixel = (int)(H / 2 * scope_min[i]);
                else pixel = (int)(H / 2 * scope_min[i]);
                y = H / 2 - pixel;
                points[W * 2 - 1 - i].X = i;
                points[W * 2 - 1 - i].Y = y;
                if (bottom) points[W * 2 - 1 - i].Y += H;
                //if(points[W*2-1-i].Y == points[i].Y)
                //	points[W*2-1-i].Y += 1; 
            }
            //using (Pen data_line_pen = new Pen(new SolidBrush(data_line_color), display_line_width))
            {
                // draw the connected points
                g.DrawLines(data_line_pen, points);
                g.FillPolygon(data_line_pen.Brush, points, System.Drawing.Drawing2D.FillMode.Alternate);
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p;
                p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                if (bottom) g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H + H);
                else g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        private static float[] scope2_min = new float[W];
        public static float[] Scope2Min
        {
            get { return scope2_min; }
            set { scope2_min = value; }
        }
        private static float[] scope2_max = new float[W];
        public static float[] Scope2Max
        {
            get { return scope2_max; }
            set { scope2_max = value; }
        }

        unsafe private static bool DrawScope2(Graphics g, int W, int H, bool bottom)
        {
            if (scope_min.Length < W)
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }

            if (scope_max.Length < W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }

            // if (scope2_min.Length < W)
            // {
            //    scope2_min = new float[W];
            //    Audio.Scope2Min = scope2_min;
            //}
            if (scope2_max.Length < W)
            {
                scope2_max = new float[W];
                Audio.Scope2Max = scope2_max;
            }

            //DrawScopeGrid(ref g, W, H, bottom);

            Point[] points = new Point[W]; // * 2];			// create Point array
            /* for (int i = 0; i < W; i++)						// fill point array
             {
                 int pixel = 0;
                 int pixel2 = 0;

                 pixel = (int)(H / 2 * scope_max[i]);
                 int y = H / 2 - pixel;
              
                 pixel2 = (int)(H / 2 * scope2_max[i]);
                 int x = H / 2 - pixel2;

                 points[i].X = x; //x; //Horizontal
                 points[i].Y = y; //y; //Vertical    */


            /*  pixel = (int)(H / 2 * scope_min[i]);
              y = H / 2 - pixel;

              pixel2 = (int)(H / 2 * scope2_min[i]);
              x = H / 2 - pixel2;

              points[W * 2 - 1 - i].X = x; //-Horizontal
              points[W * 2 - 1 - i].Y = y; //-Vertical           
          }*/
            int y1 = (int)(H * 0.25f);
            int y2 = (int)(H * 0.5f);
            int y3 = (int)(H * 0.75f);
            using (Pen y1_pen = new Pen(Color.FromArgb(64, 64, 64)),
                       y2_pen = new Pen(Color.FromArgb(48, 48, 48)),
                       y3_pen = new Pen(Color.FromArgb(64, 64, 64)))
            {
                g.DrawLine(y1_pen, 0, y1, W, y1);
                g.DrawLine(y2_pen, 0, y2, W, y2);
                g.DrawLine(y3_pen, 0, y3, W, y3);
            }

            int samples = scope2_max.Length;
            float xScale = (float)samples / W;
            float yScale = (float)H / 4;

            // draw the left input samples
            points[0].X = 0;
            points[0].Y = (int)(y1 - (scope2_max[0] * yScale));

            for (int x = 0; x < W; x++)
            {
                int i = (int)Math.Truncate((float)x * xScale);
                int y = (int)(y1 - (scope2_max[i] * yScale));
                points[x].X = x;
                points[x].Y = y;
            }

            // draw the connected points
            using (Pen waveform_line_pen = new Pen(Color.LightGreen, 0.2f))
            {
                //g.DrawPolygon(data_line_pen, points);
                g.DrawLines(waveform_line_pen, points);
                //g.FillPolygon(new SolidBrush(data_line_pen.Color), points);
                // draw the right input samples

                points[0].X = 0;
                points[0].Y = (int)(y2 - (scope_max[0] * yScale));
                for (int x = 0; x < W; x++)
                {
                    int i = (int)Math.Truncate((float)x * xScale);
                    int y = (int)(y3 - (scope_max[i] * yScale));
                    //int X = (int)(scope2_max[i] * W);
                    points[x].X = x;// (int)(scope2_max[x]);
                    points[x].Y = y;// y;
                }
                // draw the waveform
                g.DrawLines(waveform_line_pen, points);
            }

            return true;
        }

        unsafe private static bool DrawPhase(Graphics g, int W, int H, bool bottom)
        {
            DrawPhaseGrid(ref g, W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                data_ready_bottom = false;
            }


            Point[] points = new Point[num_points];		// declare Point array
            for (int i = 0, j = 0; i < num_points; i++, j += 8)	// fill point array
            {
                int x = 0;
                int y = 0;
                if (bottom)
                {
                    x = (int)(current_display_data_bottom[i * 2] * H / 2);
                    y = (int)(current_display_data_bottom[i * 2 + 1] * H / 2);
                }
                else
                {
                    x = (int)(current_display_data[i * 2] * H / 2);
                    y = (int)(current_display_data[i * 2 + 1] * H / 2);
                }
                points[i].X = W / 2 + x;
                points[i].Y = H / 2 + y;
                if (bottom) points[i].Y += H;
            }

            // draw each point
            //using (Pen data_line_pen = new Pen(new SolidBrush (data_line_color), display_line_width))
            for (int i = 0; i < num_points; i++)
                g.DrawRectangle(data_line_pen, points[i].X, points[i].Y, 1, 1);

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p;
                p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
            }
            return true;
        }

        unsafe private static void DrawPhase2(Graphics g, int W, int H, bool bottom)
        {
            DrawPhaseGrid(ref g, W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                data_ready_bottom = false;
            }

            Point[] points = new Point[num_points];		// declare Point array
            for (int i = 0; i < num_points; i++)	// fill point array
            {
                int x = 0;
                int y = 0;
                if (bottom)
                {
                    x = (int)(current_display_data_bottom[i * 2] * H * 0.5 * 500);
                    y = (int)(current_display_data_bottom[i * 2 + 1] * H * 0.5 * 500);
                }
                else
                {
                    x = (int)(current_display_data[i * 2] * H * 0.5 * 500);
                    y = (int)(current_display_data[i * 2 + 1] * H * 0.5 * 500);
                }
                points[i].X = (int)(W * 0.5 + x);
                points[i].Y = (int)(H * 0.5 + y);
                if (bottom) points[i].Y += H;
            }

            // draw each point
            //using ( Pen data_line_pen = new Pen(new SolidBrush(data_line_color), display_line_width))
            for (int i = 0; i < num_points; i++)
                g.DrawRectangle(data_line_pen, points[i].X, points[i].Y, 1, 1);

            // draw long cursor
            if (current_click_tune_mode == ClickTuneMode.Off) return;
            Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
            if (bottom) g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
            else g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
            g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
        }

        private static Point[] points;
        unsafe static private bool DrawSpectrum(Graphics g, int W, int H, bool bottom)
        {
            DrawSpectrumGrid(ref g, W, H, bottom);
            if (points == null || points.Length < W)
                points = new Point[W];			// array of points to display
            float slope = 0.0f;						// samples to process per pixel
            int num_samples = 0;					// number of samples to process
            int start_sample_index = 0;				// index to begin looking at samples
            int low = 0;
            int high = 0;
            float local_max_y = float.MinValue;
            int grid_max = 0;
            int grid_min = 0;
            int sample_rate;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
                sample_rate = sample_rate_rx1;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                sample_rate = sample_rate_tx;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int yRange = grid_max - grid_min;

            if (!bottom && data_ready)
            {
                if (mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    for (int i = 0; i < current_display_data.Length; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                }
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                /*  if(mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                  {
                      for(int i=0; i<current_display_data_bottom.Length; i++)
                          current_display_data_bottom[i] = -200.0f;
                  }
                  else */
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                }
                data_ready_bottom = false;
            }

            /*   if (!bottom && average_on)
                   console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
               else if (bottom && rx2_avg_on)
                   console.UpdateRX2DisplayAverage(rx2_average_buffer, current_display_data_bottom);

               if (!bottom && peak_on)
                   UpdateDisplayPeak(rx1_peak_buffer, current_display_data);
               else if (bottom && rx2_peak_on)
                   UpdateDisplayPeak(rx2_peak_buffer, current_display_data_bottom);
               */
            start_sample_index = (BUFFER_SIZE >> 1) + (int)((low * BUFFER_SIZE) / sample_rate);
            num_samples = (int)((high - low) * BUFFER_SIZE / sample_rate);

            if (start_sample_index < 0) start_sample_index = 0;
            if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
                num_samples = BUFFER_SIZE - start_sample_index;

            slope = (float)num_samples / (float)W;
            for (int i = 0; i < W; i++)
            {
                float max = float.MinValue;
                float dval = i * slope + start_sample_index;
                int lindex = (int)Math.Floor(dval);
                if (!bottom)
                {
                    if (!mox)
                        max = current_display_data[i];
                    else
                    {
                        if (slope <= 1)
                            max = current_display_data[lindex] * ((float)lindex - dval + 1) + current_display_data[lindex + 1] * (dval - (float)lindex);
                        else
                        {
                            int rindex = (int)Math.Floor(dval + slope);
                            if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
                            for (int j = lindex; j < rindex; j++)
                                if (current_display_data[j] > max) max = current_display_data[j];
                        }
                    }
                }
                else
                {
                    if (slope <= 1)
                        max = current_display_data_bottom[lindex] * ((float)lindex - dval + 1) + current_display_data_bottom[lindex + 1] * (dval - (float)lindex);
                    else
                    {
                        int rindex = (int)Math.Floor(dval + slope);
                        if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
                        for (int j = lindex; j < rindex; j++)
                            if (current_display_data_bottom[j] > max) max = current_display_data_bottom[j];
                    }
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (!bottom) max += rx1_display_cal_offset;
                    else max += rx2_display_cal_offset;
                }
                else
                {
                    //if (!bottom) 
                    max += tx_display_cal_offset;
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (!bottom) max += rx1_preamp_offset - alex_preamp_offset;
                    else max += rx2_preamp_offset;
                }

                if (max > local_max_y)
                {
                    local_max_y = max;
                    max_x = i;
                }

                points[i].X = i;
                points[i].Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
                if (bottom) points[i].Y += H;
            }

            max_y = local_max_y;
            //using (Pen data_line_pen = new Pen(new SolidBrush(data_line_color), display_line_width))
            g.DrawLines(data_line_pen, points);

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                if (bottom)
                {
                    g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
                    g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                }
                else
                {
                    g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                    g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                }
            }

            return true;
        }

        unsafe static private bool DrawPanadapter(Graphics g, int W, int H, int rx, bool bottom)
        {
            DrawPanadapterGrid(ref g, W, H, rx, bottom);

            if (pan_fill)
            {
                if (points == null || points.Length < W + 2)
                    points = new Point[W + 2];
            }
            else
            {
                if (points == null || points.Length < W)
                    points = new Point[W];			// array of points to display
            }
            //float slope = 0.0F;						// samples to process per pixel
            //int num_samples = 0;					// number of samples to process
            //int start_sample_index = 0;				// index to begin looking at samples
            int Low = 0;// rx_display_low;
            int High = 0;// rx_display_high;
            // int yRange = spectrum_grid_max - spectrum_grid_min;
            float local_max_y = float.MinValue;
            bool local_mox = false;
            bool displayduplex = false;
            int sample_rate;

            int grid_max = 0;
            int grid_min = 0;

            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;
            if (rx == 1 && tx_on_vfob && mox && !console.RX2Enabled) local_mox = true;

            if ((CurrentDisplayMode == DisplayMode.PANAFALL /* && (console.NReceivers <= 2 && display_duplex)*/) ||
                //(CurrentDisplayMode == DisplayMode.PANAFALL && console.NReceivers > 2) ||
               (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;

            if (rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    sample_rate = sample_rate_rx2;
                }
            }
            else
            {
                if (local_mox)// && !displayduplex) // && !tx_on_vfob)
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                }
                else
                {
                    Low = rx_display_low;
                    High = rx_display_high;
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    sample_rate = sample_rate_rx1;
                }
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                Low += 12000;
                High += 12000;
            }

            int yRange = grid_max - grid_min;
            int width = High - Low;

            if (rx == 1 && data_ready)
            {
                if (!displayduplex && (local_mox || (mox && tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    for (int i = 0; i < current_display_data.Length; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                }
                data_ready = false;
            }
            else if (rx == 2 && data_ready_bottom)
            {
                if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                {
                    for (int i = 0; i < current_display_data_bottom.Length; i++)
                        current_display_data_bottom[i] = grid_min - rx2_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                }

                data_ready_bottom = false;
            }

            //if (rx == 1 && average_on && local_mox && !displayduplex)
            //{
            //    console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
            //}
            //else if (rx == 2 && rx2_avg_on && local_mox)
            //    console.UpdateRX2DisplayAverage(rx2_average_buffer, current_display_data_bottom);

            //if (rx == 1 && peak_on && local_mox && !displayduplex)
            //    UpdateDisplayPeak(rx1_peak_buffer, current_display_data);
            //else
            //    if (rx == 2 && rx2_peak_on && local_mox)
            //        UpdateDisplayPeak(rx2_peak_buffer, current_display_data_bottom);
#if false            
            if (rx == 1)
            {
              /*  start_sample_index = (BUFFER_SIZE >> 1) + (int)((Low * BUFFER_SIZE) / sample_rate);
                 num_samples = (int)((BUFFER_SIZE * (High - Low)) / sample_rate);
               // num_samples = (int)((High - Low) / (sample_rate * 3) * BUFFER_SIZE);
                if (start_sample_index < 0) start_sample_index += 4096;
                if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
                    num_samples = (BUFFER_SIZE - start_sample_index);

                //Debug.WriteLine("start_sample_index: "+start_sample_index);
                slope = (float)num_samples / (float)W; */
            }
            else
            {
                start_sample_index = (BUFFER_SIZE >> 1) + (int)((Low * BUFFER_SIZE) / sample_rate);
                num_samples = (int)((BUFFER_SIZE * (High - Low)) / sample_rate);
               // num_samples = (int)((High - Low) / (sample_rate * 3) * BUFFER_SIZE);
                if (start_sample_index < 0) start_sample_index += 4096;
                if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
                    num_samples = (BUFFER_SIZE - start_sample_index);

                //Debug.WriteLine("start_sample_index: "+start_sample_index);
                slope = (float)num_samples / (float)W;
            }
#endif

            //if (local_mox && !displayduplex)// || rx1_dsp_mode == DSPMode.DRM)
            //{
            //    start_sample_index = (BUFFER_SIZE >> 1) + ((Low * BUFFER_SIZE) / sample_rate);
            //    num_samples = (width * BUFFER_SIZE / sample_rate);
            //    if (start_sample_index < 0) start_sample_index += 4096;
            //    if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
            //        num_samples = (BUFFER_SIZE - start_sample_index);

            //    //Debug.WriteLine("start_sample_index: "+start_sample_index);
            //    slope = (float)num_samples / (float)W;
            //}

            try
            {
                Parallel.For(0, W, (i) => //for (int i = 0; i < W; i++)
                {
                    float max = float.MinValue;
                    //float dval = i * slope + start_sample_index;
                    //int lindex = (int)Math.Floor(dval);
                    //int rindex = (int)Math.Floor(dval + slope);

                    if (rx == 1)
                    {
                        //if (local_mox && !displayduplex)// || rx1_dsp_mode == DSPMode.DRM)
                        //{
                        //    if (slope <= 1.0 || lindex == rindex)
                        //    {
                        //        max = current_display_data[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data[(lindex + 1) % 4096] * (dval - (float)lindex);
                        //    }
                        //    else
                        //    {
                        //        for (int j = lindex; j < rindex; j++)
                        //            if (current_display_data[j % 4096] > max) max = current_display_data[j % 4096];
                        //    }
                        //}

                        //else 
                        max = current_display_data[i];
                    }
                    else if (rx == 2)
                    {
                        //if (local_mox)
                        //{
                        //    if (slope <= 1.0 || lindex == rindex)
                        //    {
                        //        max = current_display_data_bottom[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data_bottom[(lindex + 1) % 4096] * (dval - (float)lindex);
                        //    }
                        //    else
                        //    {
                        //        for (int j = lindex; j < rindex; j++)
                        //            if (current_display_data_bottom[j % 4096] > max) max = current_display_data_bottom[j % 4096];
                        //    }
                        //}
                        //else 
                        max = current_display_data_bottom[i];
                    }

                    if (rx == 1)
                    {
                        if (local_mox) max += tx_display_cal_offset;
                        else if (mox && tx_on_vfob && !displayduplex)
                        {
                            if (console.RX2Enabled) max += rx1_display_cal_offset;
                            else max += tx_display_cal_offset;
                        }
                        else max += rx1_display_cal_offset;
                    }
                    else if (rx == 2)
                    {
                        if (local_mox) max += tx_display_cal_offset;
                        else max += rx2_display_cal_offset;
                    }

                    if (!local_mox || (local_mox && displayduplex))
                    {
                        if (rx == 1) max += rx1_preamp_offset;
                        else if (rx == 2) max += rx2_preamp_offset;
                    }

                    if (max > local_max_y)
                    {
                        local_max_y = max;
                        max_x = i;
                    }

                    points[i].X = i;
                    points[i].Y = (int)(Math.Floor((grid_max - max) * H / yRange));
                    /* if (points[i].Y < 0)
                     {
                        // Trace.WriteLine(i + " " + max.ToString("f2") + points[i].Y.ToString("f2"));
                        // Trace.WriteLine(new_display_data[i].ToString("f2") + " " + current_display_data[i].ToString("f2"));

                         points[i].Y = H;
                     }*/
                    points[i].Y = Math.Min(points[i].Y, H);

                    if (bottom) points[i].Y += H;
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            if (!bottom) max_y = local_max_y;

            try
            {
                if (pan_fill)
                {
                    points[W].X = W; points[W].Y = H;
                    points[W + 1].X = 0; points[W + 1].Y = H;
                    if (bottom)
                    {
                        points[W].Y += H;
                        points[W + 1].Y += H;
                    }
                    // data_line_pen.Color = Color.FromArgb(100, 255, 255, 255);
                    if (local_mox) g.FillPolygon(tx_data_line_fpen.Brush, points);
                    else g.FillPolygon(data_line_fpen.Brush, points);
                    // g.FillPolygon(data_line_fpen.Brush, points, System.Drawing.Drawing2D.FillMode.Winding);
                    points[W] = points[W - 1];
                    points[W + 1] = points[W - 1];
                    data_line_pen.Color = data_line_color;
                    if (local_mox) g.DrawLines(tx_data_line_pen, points);
                    else g.DrawLines(data_line_pen, points);
                }
                else
                {
                    if (local_mox) g.DrawLines(tx_data_line_pen, points);
                    else g.DrawLines(data_line_pen, points);
                }
            }
            catch (Exception ex)
            {
                // Trace.WriteLine(ex);
            }
#if false
            // draw notch zoom if enabled
            if (tnf_zoom)
            {

                List<Notch> notches;
                if (!bottom)
                    notches = NotchList.NotchesInBW((double)vfoa_hz * 1e-6, Low, High);
                else
                    notches = NotchList.NotchesInBW((double)vfob_hz * 1e-6, Low, High);

                Notch notch = null;
                foreach (Notch n in notches)
                {
                    if (n.Details)
                    {
                        notch = n;
                        break;
                    }
                }

                if (notch != null &&
                    ((bottom && notch.RX == 2) ||
                    (!bottom && notch.RX == 1)))
                {
                    // draw zoom background
                    if (bottom) g.FillRectangle(new SolidBrush(Color.FromArgb(230, 0, 0, 0)), 0, H, W, H / zoom_height);
                    else g.FillRectangle(new SolidBrush(Color.FromArgb(230, 0, 0, 0)), 0, 0, W, H / zoom_height);


                    // calculate data needed for zoomed notch
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;

                    if (bottom)
                    {
                        rf_freq = vfob_hz;
                    }

                    if (bottom)
                    {
                        switch (rx2_dsp_mode)
                        {
                            case (DSPMode.CWL):
                                rf_freq += cw_pitch;
                                break;
                            case (DSPMode.CWU):
                                rf_freq -= cw_pitch;
                                break;
                        }
                    }
                    else
                    {
                        switch (rx1_dsp_mode)
                        {
                            case (DSPMode.CWL):
                                rf_freq += cw_pitch;
                                break;
                            case (DSPMode.CWU):
                                rf_freq -= cw_pitch;
                                break;
                        }
                    }

                    int zoomed_notch_center_freq = (int)(notch_zoom_start_freq * 1e6 - rf_freq - rit);

                    int original_bw = High - Low;
                    int zoom_bw = original_bw / 10;

                    int low = zoomed_notch_center_freq - zoom_bw / 2;
                    int high = zoomed_notch_center_freq + zoom_bw / 2;

                    if (low < Low) // check left limit
                    {
                        low = Low;
                        high = Low + zoom_bw;
                    }
                    else if (high > High) // check right limit
                    {
                        high = High;
                        low = High - zoom_bw;
                    }

                    // decide colors to draw notch
                    Color c1 = notch_on_color_zoomed;
                    Color c2 = notch_highlight_color_zoomed;

                    if (!tnf_active)
                    {
                        c1 = notch_off_color;
                        c2 = Color.Black;
                    }
                    else if (notch.Permanent)
                    {
                        c1 = notch_perm_on_color;
                        c2 = notch_perm_highlight_color;
                    }

                    int notch_zoom_left_x = (int)((float)(notch.Freq * 1e6 - rf_freq - notch.BW / 2 - low - rit) / (high - low) * W);
                    int notch_zoom_right_x = (int)((float)(notch.Freq * 1e6 - rf_freq + notch.BW / 2 - low - rit) / (high - low) * W);

                    if (notch_zoom_left_x == notch_zoom_right_x)
                        notch_zoom_right_x = notch_zoom_left_x + 1;

                    //draw zoomed notch bars
                    if (!bottom)
                        drawNotchBar(g, notch, notch_zoom_left_x, notch_zoom_right_x, 0, (int)(H / zoom_height), c1, c2);
                    else
                        drawNotchBar(g, notch, notch_zoom_left_x, notch_zoom_right_x, H, (int)(H / zoom_height), c1, c2);

                    // draw data
                    // start_sample_index = (BUFFER_SIZE >> 1) + (int)((low * BUFFER_SIZE) / sample_rate);
                    // num_samples = (int)((high - low) * BUFFER_SIZE / sample_rate);
                    // if (start_sample_index < 0) start_sample_index += 4096;
                    // if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
                    //     num_samples = BUFFER_SIZE - start_sample_index;

                    //Debug.WriteLine("start_sample_index: "+start_sample_index);
                    // slope = (float)num_samples / (float)W;
                    //int grid_max = spectrum_grid_min + (spectrum_grid_max - spectrum_grid_min) / 2;
                    for (int i = 0; i < W; i++)
                    {
                        float max = float.MinValue;
                        // float dval = i * slope + start_sample_index;
                        // int lindex = (int)Math.Floor(dval);
                        // int rindex = (int)Math.Floor(dval + slope);

                        if (rx == 1)
                        {
                            max = current_display_data[i];
                            /* if (slope <= 1.0 || lindex == rindex)
                             {
                                 max = current_display_data[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data[(lindex + 1) % 4096] * (dval - (float)lindex);
                             }
                             else
                             {
                                 for (int j = lindex; j < rindex; j++)
                                     if (current_display_data[j % 4096] > max) max = current_display_data[j % 4096];
                             } */
                        }
                        else if (rx == 2)
                        {
                            max = current_display_data_bottom[i];
                            /* if (slope <= 1.0 || lindex == rindex)
                             {
                                 max = current_display_data_bottom[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data_bottom[(lindex + 1) % 4096] * (dval - (float)lindex);
                             }
                             else
                             {
                                 for (int j = lindex; j < rindex; j++)
                                     if (current_display_data_bottom[j % 4096] > max) max = current_display_data_bottom[j % 4096];
                             } */
                        }

                        /*   if (rx == 1) max += rx1_display_cal_offset;
                           else if (rx == 2) max += rx2_display_cal_offset;

                           if (!local_mox)
                           {
                               if (rx == 1) max += rx1_preamp_offset;
                               else if (rx == 2) max += rx2_preamp_offset;
                           } */

                        if (rx == 1)
                        {
                            if (local_mox || (mox && tx_on_vfob)) max += tx_display_cal_offset;
                            else max += rx1_display_cal_offset;
                        }
                        else if (rx == 2)
                        {
                            if (local_mox) max += tx_display_cal_offset;
                            else max += rx2_display_cal_offset;
                        }

                        if (!local_mox)
                        {
                            if (rx == 1) max += rx1_preamp_offset;
                            else if (rx == 2) max += rx2_preamp_offset;
                        }

                        if (max > local_max_y)
                        {
                            local_max_y = max;
                            max_x = i;
                        }

                        points[i].X = i;
                        points[i].Y = (int)(Math.Floor((grid_max - max) * H / zoom_height / yRange));    //used to be 6
                        points[i].Y = Math.Min(points[i].Y, H);
                        if (bottom) points[i].Y += H;
                    }

                    if (pan_fill)
                    {
                        points[W].X = W; points[W].Y = (int)(H / zoom_height);
                        points[W + 1].X = 0; points[W + 1].Y = (int)(H / zoom_height);
                        if (bottom)
                        {
                            points[W].Y += H;
                            points[W + 1].Y += H;
                        }
                        data_line_pen.Color = Color.FromArgb(100, 255, 255, 255);
                        g.FillPolygon(data_line_pen.Brush, points, System.Drawing.Drawing2D.FillMode.Alternate);
                        points[W] = points[W - 1];
                        points[W + 1] = points[W - 1];
                        data_line_pen.Color = data_line_color;
                        g.DrawLines(data_line_pen, points);
                    }
                    else g.DrawLines(data_line_pen, points);
                }
            }
#endif
            points = null;

            // draw long cursor
            try
            {
                if (current_click_tune_mode != ClickTuneMode.Off)
                {
                    Pen p;
                    if (current_click_tune_mode == ClickTuneMode.VFOA)
                        p = new Pen(grid_text_color);
                    else p = new Pen(Color.Red);
                    if (bottom)
                    {
                        if (display_cursor_y > H)
                        {
                            g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
                            g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                        }
                    }
                    else
                    {
                        if (display_cursor_y <= H)
                        {
                            g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                            g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        private static bool rx1_waterfall_agc = false;
        public static bool RX1WaterfallAGC
        {
            get { return rx1_waterfall_agc; }
            set { rx1_waterfall_agc = value; }
        }

        private static bool rx2_waterfall_agc = false;
        public static bool RX2WaterfallAGC
        {
            get { return rx2_waterfall_agc; }
            set { rx2_waterfall_agc = value; }
        }

        private static int waterfall_update_period = 100; // in ms
        public static int WaterfallUpdatePeriod
        {
            get { return waterfall_update_period; }
            set { waterfall_update_period = value; }
        }

        private static int rx2_waterfall_update_period = 100; // in ms
        public static int RX2WaterfallUpdatePeriod
        {
            get { return rx2_waterfall_update_period; }
            set { rx2_waterfall_update_period = value; }
        }

        private static readonly HiPerfTimer timer_waterfall = new HiPerfTimer();
        private static readonly HiPerfTimer timer_waterfall2 = new HiPerfTimer();
        private static float RX1waterfallPreviousMinValue = 0.0f;
        private static float RX2waterfallPreviousMinValue = 0.0f;
        unsafe static private bool DrawWaterfall(Graphics g, int W, int H, int rx, bool bottom)
        {
            if (grid_control) DrawWaterfallGrid(ref g, W, H, rx, bottom);

            if (waterfall_data == null || waterfall_data.Length < W)
            {
                waterfall_data = new float[W];		// array of points to display
            }
            float slope = 0.0F;						// samples to process per pixel
            int num_samples = 0;					// number of samples to process
            int start_sample_index = 0;				// index to begin looking at samples
            int Low = 0;
            int High = 0;
            float local_max_y = float.MinValue;
            bool local_mox = false;
            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;
            float local_min_y_w3sz = float.MaxValue;  //added by w3sz
            float display_min_w3sz = float.MaxValue; //added by w3sz
            float display_max_w3sz = float.MinValue; //added by w3sz
            float min_y_w3sz = float.MaxValue;  //w3sz
            int R = 0, G = 0, B = 0;
            int grid_max = 0;
            int grid_min = 0;
            bool displayduplex = true; // false;
            float low_threshold = 0.0f;
            float high_threshold = 0.0f;
            float waterfall_minimum = 0.0f;
            float rx1_waterfall_minimum = 0.0f;
            float rx2_waterfall_minimum = 0.0f;
            ColorSheme cSheme = ColorSheme.enhanced;
            Color low_color = Color.Black;
            Color mid_color = Color.Red;
            Color high_color = Color.Blue;
            int sample_rate;

            if (rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    sample_rate = sample_rate_rx2;
                }

                cSheme = rx2_color_sheme;
                low_color = rx2_waterfall_low_color;
                mid_color = rx2_waterfall_mid_color;
                high_color = rx2_waterfall_high_color;
                high_threshold = rx2_waterfall_high_threshold;

                if (rx2_waterfall_agc)
                {
                    waterfall_minimum = rx2_waterfall_minimum;
                    low_threshold = RX2waterfallPreviousMinValue;
                }
                else
                    low_threshold = rx2_waterfall_low_threshold;
            }
            else
            {
                cSheme = color_sheme;
                low_color = waterfall_low_color;
                mid_color = waterfall_mid_color;
                high_color = waterfall_high_color;
                Low = rx_display_low;
                High = rx_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
                high_threshold = waterfall_high_threshold;
                sample_rate = sample_rate_rx1;

                if (rx1_waterfall_agc)
                {
                    waterfall_minimum = rx1_waterfall_minimum;
                    low_threshold = RX1waterfallPreviousMinValue;
                }
                else
                    low_threshold = waterfall_low_threshold;
            }

            if (console.PowerOn)
            {
                if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
                    (rx2_dsp_mode == DSPMode.DRM && rx == 2))
                {
                    Low += 12000;
                    High += 12000;
                }

                if (rx == 1 && waterfall_data_ready)
                {
                    if (!displayduplex && local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < current_waterfall_data.Length; i++)
                            current_waterfall_data[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data[0])
                        fixed (void* wptr = &current_waterfall_data[0])
                            Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));

                    }
                    waterfall_data_ready = false;
                }
                else if (rx == 2 && waterfall_data_ready_bottom)
                {
                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < current_waterfall_data_bottom.Length; i++)
                            current_waterfall_data_bottom[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data_bottom[0])
                        fixed (void* wptr = &current_waterfall_data_bottom[0])
                            Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                    }

                    waterfall_data_ready_bottom = false;
                }

                int duration = 0;
                if (rx == 1)
                {
                    timer_waterfall.Stop();
                    duration = (int)timer_waterfall.DurationMsec;
                }
                else if (rx == 2)
                {
                    timer_waterfall2.Stop();
                    duration = (int)timer_waterfall2.DurationMsec;
                }

                if ((rx == 1 && (duration > waterfall_update_period || duration < 0)) ||
                    (rx == 2 && (duration > rx2_waterfall_update_period || duration < 0)))
                {
                    if (rx == 1) timer_waterfall.Start();
                    else if (rx == 2) timer_waterfall2.Start();

                    for (int i = 0; i < W; i++)
                    {
                        float max = float.MinValue;
 
                        if (rx == 1)
                        {
                            max = current_waterfall_data[i];
                        }
                        else if (rx == 2)
                        {
                            max = current_waterfall_data_bottom[i];
                        }

                        if (!local_mox)
                        {
                            if (rx == 1) max += rx1_display_cal_offset;
                            else if (rx == 2) max += rx2_display_cal_offset;
                        }
                        else
                        {
                            if (rx == 1) max += rx1_display_cal_offset;
                            else if (rx == 2) max += tx_display_cal_offset;
                        }

                        if (!local_mox)
                        {
                            if (rx == 1) max += (rx1_preamp_offset - alex_preamp_offset);
                            else if (rx == 2) max += (rx2_preamp_offset);
                        }
                        else
                        {
                            if (rx == 1) max += (rx1_preamp_offset - alex_preamp_offset);
                        }

                        if (max > local_max_y)
                        {
                            local_max_y = max;
                            max_x = i;
                        }

                        //below added by w3sz
                        if (max < local_min_y_w3sz)
                        {
                            local_min_y_w3sz = max;
                        }
                        //end of addition by w3sz

                        waterfall_data[i] = max;
                    }

                    max_y = local_max_y;
                    min_y_w3sz = local_min_y_w3sz;

                    BitmapData bitmapData;
                    if (rx == 1)
                    {
                        switch (current_display_mode)   //w3sz added switch for panafall waterfall height
                        {
                            case DisplayMode.PANAFALL:
                                bitmapData = waterfall_bmp.LockBits(
                                    new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height / 2),
                                    ImageLockMode.ReadWrite,
                                    waterfall_bmp.PixelFormat);// /2 added by w3sz for panafall
                                break;
                            default:
                                if (console.RX2Enabled)
                                {
                                    bitmapData = waterfall_bmp.LockBits(
                                        new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height / 2 - 10),
                                        ImageLockMode.ReadWrite,
                                        waterfall_bmp.PixelFormat);
                                }
                                else
                                {
                                    bitmapData = waterfall_bmp.LockBits(
                                        new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height),
                                        ImageLockMode.ReadWrite,
                                        waterfall_bmp.PixelFormat);
                                }
                                break;
                        }
                    }
                    else
                    {
                        bitmapData = waterfall_bmp2.LockBits(
                           new Rectangle(0, 0, waterfall_bmp2.Width, waterfall_bmp2.Height / 2),
                           ImageLockMode.ReadWrite,
                           waterfall_bmp2.PixelFormat);
                    }

                    int pixel_size = 3;
                    byte* row = null;

                    // first scroll image
                    int total_size = bitmapData.Stride * bitmapData.Height;		// find buffer size
                    Win32.memcpy(new IntPtr((int)bitmapData.Scan0 + bitmapData.Stride).ToPointer(),
                        bitmapData.Scan0.ToPointer(),
                        total_size - bitmapData.Stride);

                    row = (byte*)bitmapData.Scan0;

                    //int i = 0;
                    switch (cSheme)
                    {
                        case (ColorSheme.original):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    //int R, G, B;		// variables to save Red, Green and Blue component values

                                    if (waterfall_data[i] <= low_threshold)		// if less than low threshold, just use low color
                                    {
                                        R = low_color.R;
                                        G = low_color.G;
                                        B = low_color.B;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)// if more than high threshold, just use high color
                                    {
                                        R = high_color.R;
                                        G = high_color.G;
                                        B = high_color.B;
                                    }
                                    else // use a color between high and low
                                    {
                                        float percent = (waterfall_data[i] - low_threshold) / (high_threshold - low_threshold);
                                        if (percent <= 0.5)	// use a gradient between low and mid colors
                                        {
                                            percent *= 2;

                                            R = (int)((1 - percent) * low_color.R + percent * mid_color.R);
                                            G = (int)((1 - percent) * low_color.G + percent * mid_color.G);
                                            B = (int)((1 - percent) * low_color.B + percent * mid_color.B);
                                        }
                                        else				// use a gradient between mid and high colors
                                        {
                                            percent = (float)(percent - 0.5) * 2;

                                            R = (int)((1 - percent) * mid_color.R + percent * high_color.R);
                                            G = (int)((1 - percent) * mid_color.G + percent * high_color.G);
                                            B = (int)((1 - percent) * mid_color.B + percent * high_color.B);
                                        }
                                    }
                                    // set pixel color
                                    row[i * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)R;
                                }
                            }
                            //waterfall_data[i] = (float)i/W*(high_threshold - low_threshold) + low_threshold;
                            break;

                        case (ColorSheme.enhanced):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = low_color.R;
                                        G = low_color.G;
                                        B = low_color.B;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 192;
                                        G = 124;
                                        B = 255;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.

                                        if (overall_percent < (float)2 / 9) // background to blue
                                        {
                                            float local_percent = overall_percent / ((float)2 / 9);
                                            R = (int)((1.0 - local_percent) * low_color.R);
                                            G = (int)((1.0 - local_percent) * low_color.G);
                                            B = (int)(low_color.B + local_percent * (255 - low_color.B));
                                        }
                                        else if (overall_percent < (float)3 / 9) // blue to blue-green
                                        {
                                            float local_percent = (overall_percent - (float)2 / 9) / ((float)1 / 9);
                                            R = 0;
                                            G = (int)(local_percent * 255);
                                            B = 255;
                                        }
                                        else if (overall_percent < (float)4 / 9) // blue-green to green
                                        {
                                            float local_percent = (overall_percent - (float)3 / 9) / ((float)1 / 9);
                                            R = 0;
                                            G = 255;
                                            B = (int)((1.0 - local_percent) * 255);
                                        }
                                        else if (overall_percent < (float)5 / 9) // green to red-green
                                        {
                                            float local_percent = (overall_percent - (float)4 / 9) / ((float)1 / 9);
                                            R = (int)(local_percent * 255);
                                            G = 255;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 9) // red-green to red
                                        {
                                            float local_percent = (overall_percent - (float)5 / 9) / ((float)2 / 9);
                                            R = 255;
                                            G = (int)((1.0 - local_percent) * 255);
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)8 / 9) // red to red-blue
                                        {
                                            float local_percent = (overall_percent - (float)7 / 9) / ((float)1 / 9);
                                            R = 255;
                                            G = 0;
                                            B = (int)(local_percent * 255);
                                        }
                                        else // red-blue to purple end
                                        {
                                            float local_percent = (overall_percent - (float)8 / 9) / ((float)1 / 9);
                                            R = (int)((0.75 + 0.25 * (1.0 - local_percent)) * 255);
                                            G = (int)(local_percent * 255 * 0.5);
                                            B = 255;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[i * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)R;
                                }
                            }
                            break;

                        case (ColorSheme.SPECTRAN):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= WaterfallHighThreshold) // white
                                    {
                                        R = 240;
                                        G = 240;
                                        B = 240;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = WaterfallHighThreshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float local_percent = ((100.0f * offset) / range);

                                        if (local_percent < 5.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 11.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 22.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 44.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 51.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 66.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 2;
                                            B = 255;
                                        }
                                        else if (local_percent < 77.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 3;
                                            B = 255;
                                        }
                                        else if (local_percent < 88.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 4;
                                            B = 255;
                                        }
                                        else if (local_percent < 99.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 5;
                                            B = 255;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[i * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)R;
                                }
                            }
                            break;

                        case (ColorSheme.BLACKWHITE):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= WaterfallHighThreshold) // white
                                    {
                                        R = 255;
                                        G = 255;
                                        B = 255;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = WaterfallHighThreshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float local_percent = ((100.0f * offset) / range);
                                        R = (int)((local_percent / 100) * 255);
                                        G = R;
                                        B = R;
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[i * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)R;
                                }
                            }
                            break;

                        case (ColorSheme.LinLog):
                            {
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold + LinLogCor;
                                        float spec_bits = 1024;
                                        float overall_percent = (spec_bits * offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
                                        float log_fract = (float)(Math.Log10(spec_bits));
                                        if (overall_percent == 0)
                                        {
                                            overall_percent = (float)0.001;
                                        }
                                        overall_percent = (float)(Math.Log10(overall_percent));

                                        if (overall_percent < log_fract / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color changed by w3sz

                                    row[i * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)B;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorSheme.LinRad):
                            {
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold + LinCor;
                                        float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


                                        if (overall_percent < (float)1 / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color changed by w3sz
                                    row[i * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)B;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorSheme.LinAuto):
                            {
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    display_min_w3sz = min_y_w3sz - 5; //w3sz for histogram equilization
                                    display_max_w3sz = max_y; //w3sz for histogram equalization
                                    //	if (i == (W - 1))
                                    //	{
                                    //		System.Console.WriteLine("Low is" + low_threshold + "   " + display_min_w3sz);
                                    //	
                                    //		System.Console.WriteLine("High is" + high_threshold + "   " + max_y);
                                    //}	

                                    if (waterfall_data[i] <= display_min_w3sz)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= display_max_w3sz)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = display_max_w3sz - display_min_w3sz;
                                        float offset = waterfall_data[i] - display_min_w3sz; // + GlobalClass.LinCor;
                                        float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


                                        if (overall_percent < (float)1 / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    // set pixel color changed by w3sz
                                    row[i * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)B;
                                }
                            }
                            break;
                    }


                    if (rx == 1)
                        waterfall_bmp.UnlockBits(bitmapData);
                    else
                        waterfall_bmp2.UnlockBits(bitmapData);

                    if (rx == 1)
                        RX1waterfallPreviousMinValue = (((RX1waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc
                    else
                        RX2waterfallPreviousMinValue = ((RX2waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10 + 1; //wfagc
                }

                if (bottom)
                {
                    if (rx == 1) g.DrawImageUnscaled(waterfall_bmp, 0, H + 20);
                    else if (rx == 2) g.DrawImageUnscaled(waterfall_bmp2, 0, H + 20);
                }
                else
                {
                    if (rx == 1) g.DrawImageUnscaled(waterfall_bmp, 0, 20);	// draw the image on the background	
                    else if (rx == 2) g.DrawImageUnscaled(waterfall_bmp2, 0, 20);	// draw the image on the background	
                }
            }
            waterfall_counter++;

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                if (bottom)
                {
                    if (display_cursor_y > H)
                    {
                        g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
                        if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                    }
                    else g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H + H);
                }
                else
                {
                    if (display_cursor_y <= H)
                    {
                        g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                        if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                    }
                }
            }

            return true;
        }

        unsafe static private bool DrawHistogram(Graphics g, int W, int H)
        {
            DrawSpectrumGrid(ref g, W, H, false);
            if (points == null || points.Length < W)
                points = new Point[W];			// array of points to display
            float slope = 0.0F;						// samples to process per pixel
            int num_samples = 0;					// number of samples to process
            int start_sample_index = 0;				// index to begin looking at samples
            int low = 0;
            int high = 0;
            float local_max_y = Int32.MinValue;
            int sample_rate;

            if (!mox)								// Receive Mode
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                sample_rate = sample_rate_rx1;
            }
            else									// Transmit Mode
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                sample_rate = sample_rate_tx;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int yRange = spectrum_grid_max - spectrum_grid_min;

            if (data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));

                data_ready = false;
            }

            /*   if (average_on)
               {
                   //if(!bottom)
                   console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
                   //else
                   //console.UpdateRX2DisplayAverage(rx2_average_buffer, current_display_data_bottom);
               }
               if (peak_on)
               {
                   //if(!bottom)
                   UpdateDisplayPeak(rx1_peak_buffer, current_display_data);
                   //else
                   //	UpdateDisplayPeak(rx2_peak_buffer, current_display_data_bottom);
               } */

            num_samples = (high - low);

            start_sample_index = (BUFFER_SIZE >> 1) + ((low * BUFFER_SIZE) / sample_rate);
            num_samples = ((high - low) * BUFFER_SIZE / sample_rate);
            if (start_sample_index < 0)
                start_sample_index = 0;
            if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
                num_samples = BUFFER_SIZE - start_sample_index;

            slope = (float)num_samples / (float)W;
            for (int i = 0; i < W; i++)
            {
                float max = float.MinValue;
                float dval = i * slope + start_sample_index;
                int lindex = (int)Math.Floor(dval);

                if (!mox) max = current_display_data[i];
                else
                {
                    if (slope <= 1)
                        max = current_display_data[lindex] * ((float)lindex - dval + 1) + current_display_data[lindex + 1] * (dval - (float)lindex);
                    else
                    {
                        int rindex = (int)Math.Floor(dval + slope);
                        if (rindex > BUFFER_SIZE) rindex = BUFFER_SIZE;
                        for (int j = lindex; j < rindex; j++)
                            if (current_display_data[j] > max) max = current_display_data[j];
                    }
                }

                if (!mox)
                {
                    max += rx1_display_cal_offset;
                }
                else
                {
                    max += tx_display_cal_offset;
                }

                if (!mox) max += (rx1_preamp_offset - alex_preamp_offset);

                switch (rx1_dsp_mode)
                {
                    case DSPMode.SPEC:
                        max += 6.0F;
                        break;
                }
                if (max > local_max_y)
                {
                    local_max_y = max;
                    max_x = i;
                }

                points[i].X = i;
                points[i].Y = (int)Math.Min((Math.Floor((spectrum_grid_max - max) * H / yRange)), H);
            }

            max_y = local_max_y;

            // get the average
            float avg = 0.0F;
            int sum = 0;
            foreach (Point p in points)
                sum += p.Y;

            avg = (float)((float)sum / points.Length / 1.12);

            for (int i = 0; i < W; i++)
            {
                if (points[i].Y < histogram_data[i])
                {
                    histogram_history[i] = 0;
                    histogram_data[i] = points[i].Y;
                }
                else
                {
                    histogram_history[i]++;
                    if (histogram_history[i] > 51)
                    {
                        histogram_history[i] = 0;
                        histogram_data[i] = points[i].Y;
                    }

                    int alpha = Math.Max(255 - histogram_history[i] * 5, 0);
                    int height = points[i].Y - histogram_data[i];
                    dhp.Color = Color.FromArgb(alpha, 0, 255, 0);

                    // using (Pen dhp = new Pen(Color.FromArgb(alpha, 0, 255, 0)))
                    g.DrawRectangle(dhp, i, histogram_data[i], 1, height);
                }
                //using (Pen dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255)),
                //  dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0)))
                if (points[i].Y >= avg)		// value is below the average
                {
                    g.DrawRectangle(dhp1, points[i].X, points[i].Y, 1, H - points[i].Y);
                }
                else
                {
                    g.DrawRectangle(dhp1, points[i].X, (int)Math.Floor(avg), 1, H - (int)Math.Floor(avg));
                    g.DrawRectangle(dhp2, points[i].X, points[i].Y, 1, (int)Math.Floor(avg) - points[i].Y);
                }
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
                g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        public static void ResetRX1DisplayAverage()
        {
            if (rx1_average_buffer != null)
                rx1_average_buffer[0] = CLEAR_FLAG;	// set reset flag
        }

        public static void ResetRX2DisplayAverage()
        {
            if (rx2_average_buffer != null)
                rx2_average_buffer[0] = CLEAR_FLAG;	// set reset flag
        }

        public static void ResetRX1DisplayPeak()
        {
            if (rx1_peak_buffer != null)
                rx1_peak_buffer[0] = CLEAR_FLAG; // set reset flag
        }

        public static void ResetRX2DisplayPeak()
        {
            if (rx2_peak_buffer != null)
                rx2_peak_buffer[0] = CLEAR_FLAG; // set reset flag
        }

        #endregion

        #endregion
        #endregion
        #endregion

    }
}
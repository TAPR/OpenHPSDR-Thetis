//=================================================================
// display.cs
//=================================================================
// Thetis is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2020  Doug Wigley (W5WC)
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
// SharpDX used for all DirectX - MW0LGE

using System.Linq;

namespace Thetis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Buffers;

    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    // fix clashes with sharpdx
    using Bitmap = System.Drawing.Bitmap;
    using Rectangle = System.Drawing.Rectangle;
    using Color = System.Drawing.Color;
    using Brush = System.Drawing.Brush;
    using Point = System.Drawing.Point;
    using Pen = System.Drawing.Pen;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using DashStyle = System.Drawing.Drawing2D.DashStyle;
    // SharpDX clashes
    using AlphaMode = SharpDX.Direct2D1.AlphaMode;
    using Device = SharpDX.Direct3D11.Device;
    using RectangleF = SharpDX.RectangleF;
    using SDXPixelFormat = SharpDX.Direct2D1.PixelFormat;
    
    class Display
    {
        #region Variable Declaration

        public const float CLEAR_FLAG = -999.999F;				// for resetting buffers
        public const int BUFFER_SIZE = 16384;

        public static Console console;
        public static SpotControl SpotForm;                     // ke9ns add  communications with spot.cs and dx spotter
        public static string background_image = null;

        private static int[] histogram_data = null;					// histogram display buffer
        private static int[] histogram_history;					// histogram counter

        public static float[] new_display_data;					// Buffer used to store the new data from the DSP for the display
        public static float[] current_display_data;				// Buffer used to store the current data for the display
        public static float[] new_display_data_bottom;
        public static float[] current_display_data_bottom;

        public static float[] rx1_average_buffer;					// Averaged display data buffer
        public static float[] rx2_average_buffer;
        public static float[] rx1_peak_buffer;						// Peak hold display data buffer
        public static float[] rx2_peak_buffer;

        //waterfall
        public static float[] new_waterfall_data;
        public static float[] current_waterfall_data;
        public static float[] new_waterfall_data_bottom;
        public static float[] current_waterfall_data_bottom;

        private static float[] waterfall_data;

        private static Bitmap waterfall_bmp = null;					// saved waterfall picture for display
        private static Bitmap waterfall_bmp2 = null;

        private static SharpDX.Direct2D1.Bitmap waterfall_bmp_dx2d = null;					// MW0LGE
        private static SharpDX.Direct2D1.Bitmap waterfall_bmp2_dx2d = null;

        #endregion

        #region Properties

        public static float FrameDelta { get; private set; }

        private static bool tnf_active = true;
        public static bool TNFActive {
            get { return tnf_active; }
            set {
                tnf_active = value;
            }
        }

        private static bool m_bFramRateIssue = true;
        public static bool FrameRateIssue {
            get { return m_bFramRateIssue; }
            set { m_bFramRateIssue = value; }
        }

        public static Rectangle AGCKnee = new Rectangle();
        public static Rectangle AGCHang = new Rectangle();
        public static Rectangle AGCRX2Knee = new Rectangle();
        public static Rectangle AGCRX2Hang = new Rectangle();

        private static Color notch_callout_active_color = Color.Chartreuse;
        private static Color notch_callout_inactive_color = Color.OrangeRed;

        private static Color notch_highlight_color = Color.Chartreuse;
        private static Color notch_tnf_off_colour = Color.Olive;

        private static Color notch_active_colour = Color.Yellow;
        private static Color notch_inactive_colour = Color.Gray;

        private static Color notch_bw_colour = Color.Yellow;
        private static Color notch_bw_colour_inactive = Color.Gray;

        private static Color channel_background_on = Color.FromArgb(150, Color.DodgerBlue);
        private static Color channel_background_off = Color.FromArgb(100, Color.RoyalBlue);
        private static Color channel_foreground = Color.Cyan;

        private static Pen m_pTNFInactive = new Pen(notch_tnf_off_colour, 1);
        private static Brush m_bTNFInactive = new SolidBrush(changeAlpha(notch_tnf_off_colour, 92));

        private static Pen m_pNotchActive = new Pen(notch_active_colour, 1);
        private static Pen m_pNotchInactive = new Pen(notch_inactive_colour, 1);
        private static Pen m_pHighlighted = new Pen(notch_highlight_color, 1);

        private static Brush m_bBWFillColour = new SolidBrush(changeAlpha(notch_bw_colour, 92));
        private static Brush m_bBWFillColourInactive = new SolidBrush(changeAlpha(notch_bw_colour_inactive, 92));
        private static Brush m_bBWHighlighedFillColour = new SolidBrush(changeAlpha(notch_highlight_color, 92));

        private static Brush m_bTextCallOutActive = new SolidBrush(notch_callout_active_color);
        private static Brush m_bTextCallOutInactive = new SolidBrush(notch_callout_inactive_color);
        private static Font m_fntCallOutFont = new System.Drawing.Font("Trebuchet MS", 9, FontStyle.Regular);

        private static ColorSheme color_sheme = ColorSheme.enhanced;
        public static ColorSheme ColorSheme {
            get { return color_sheme; }

            set { color_sheme = value; }
        }

        private static ColorSheme rx2_color_sheme = ColorSheme.enhanced;
        public static ColorSheme RX2ColorSheme {
            get { return rx2_color_sheme; }

            set { rx2_color_sheme = value; }
        }

        private static bool reverse_waterfall = false;
        public static bool ReverseWaterfall {
            get { return reverse_waterfall; }
            set { reverse_waterfall = value; }
        }

        private static bool pan_fill = true;
        public static bool PanFill {
            get { return pan_fill; }
            set { pan_fill = value; }
        }

        private static bool tx_pan_fill = true;
        public static bool TXPanFill {
            get { return tx_pan_fill; }
            set { tx_pan_fill = value; }
        }

        private static Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);
        public static Color PanFillColor {
            get { return pan_fill_color; }
            set { pan_fill_color = value; }
        }

        private static bool tx_on_vfob = false;
        public static bool TXOnVFOB {
            get { return tx_on_vfob; }
            set {
                tx_on_vfob = value;
            }
        }

        private static bool display_duplex = true;
        public static bool DisplayDuplex {
            get { return display_duplex; }
            set { display_duplex = value; }
        }

        private static Object m_objSplitDisplayLock = new Object();
        private static bool split_display = false;
        public static bool SplitDisplay {
            get { return split_display; }
            set {
                lock (m_objSplitDisplayLock)
                {
                    split_display = value;
                }
            }
        }

        private static DisplayMode current_display_mode_bottom = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayModeBottom {
            get { return current_display_mode_bottom; }
            set {
                current_display_mode_bottom = value;
            }
        }

        private static int rx1_filter_low;
        public static int RX1FilterLow {
            get { return rx1_filter_low; }
            set { rx1_filter_low = value; }
        }

        private static int rx1_filter_high;
        public static int RX1FilterHigh {
            get { return rx1_filter_high; }
            set { rx1_filter_high = value; }
        }

        private static int rx2_filter_low;
        public static int RX2FilterLow {
            get { return rx2_filter_low; }
            set { rx2_filter_low = value; }
        }

        private static int rx2_filter_high;
        public static int RX2FilterHigh {
            get { return rx2_filter_high; }
            set { rx2_filter_high = value; }
        }

        private static int tx_filter_low;
        public static int TXFilterLow {
            get { return tx_filter_low; }
            set { tx_filter_low = value; }
        }

        private static int tx_filter_high;
        public static int TXFilterHigh {
            get { return tx_filter_high; }
            set { tx_filter_high = value; }
        }

        private static bool sub_rx1_enabled = false;
        public static bool SubRX1Enabled {
            get { return sub_rx1_enabled; }
            set {
                sub_rx1_enabled = value;
            }
        }

        private static bool split_enabled = false;
        public static bool SplitEnabled {
            get { return split_enabled; }
            set {
                split_enabled = value;
            }
        }

        private static bool show_freq_offset = false;
        public static bool ShowFreqOffset {
            get { return show_freq_offset; }
            set {
                show_freq_offset = value;
            }
        }

        private static bool show_zero_line = true;
        public static bool ShowZeroLine {
            get { return show_zero_line; }
            set {
                show_zero_line = value;
            }
        }

        private static double freq;
        public static double FREQ {
            get { return freq; }
            set {
                freq = value;
            }
        }


        private static long vfoa_hz;
        public static long VFOA {
            get { return vfoa_hz; }
            set {
                vfoa_hz = value;
            }
        }

        private static long vfoa_sub_hz;
        public static long VFOASub //multi-rx freq
        {
            get { return vfoa_sub_hz; }
            set {
                vfoa_sub_hz = value;
            }
        }

        private static long vfob_hz;
        public static long VFOB //split tx freq
        {
            get { return vfob_hz; }
            set {
                vfob_hz = value;
            }
        }

        private static long vfob_sub_hz;
        public static long VFOBSub {
            get { return vfob_sub_hz; }
            set {
                vfob_sub_hz = value;
            }
        }

        private static int rx_display_bw;
        public static int RXDisplayBW {
            get { return rx_display_bw; }
            set {
                rx_display_bw = value;
            }
        }

        private static int rit_hz;
        public static int RIT {
            get { return rit_hz; }
            set {
                rit_hz = value;
            }
        }

        private static int xit_hz;
        public static int XIT {
            get { return xit_hz; }
            set {
                xit_hz = value;
            }
        }

        private static int freq_diff = 0;
        public static int FreqDiff {
            get { return freq_diff; }
            set {
                freq_diff = value;
            }
        }

        private static int rx2_freq_diff = 0;
        public static int RX2FreqDiff {
            get { return rx2_freq_diff; }
            set {
                rx2_freq_diff = value;
            }
        }

        private static int cw_pitch = 600;
        public static int CWPitch {
            get { return cw_pitch; }
            set { cw_pitch = value; }
        }

        private static int m_nPhasePointSize = 1;
        public static int PhasePointSize {
            get { return m_nPhasePointSize; }
            set { m_nPhasePointSize = value; }
        }
        private static bool m_bShowFPS = false;
        public static bool ShowFPS {
            get { return m_bShowFPS; }
            set { m_bShowFPS = value; }
        }

        //=======================================================

        //private static PixelFormat WtrColor = PixelFormat.Format24bppRgb;  //          


        private static bool m_bSpecialPanafall = false; // ke9ns add 1=map mode (panafall but only a small waterfall) and only when just in RX1 mode)

        //public static int H1 = 0;  //  ke9ns add used to fool draw routines when displaying in 3rds 
        //public static int H2 = 0;  //  ke9ns add used to fool draw routines when displaying in 4ths   

        public static int K9 = 0;  // ke9ns add rx1 display mode selector:  1=water,2=pan,3=panfall, 5=panfall with RX2 on any mode, 7=special map viewing panafall
        public static int K10 = 0; // ke9ns add rx2 display mode selector: 0=off 1=water,2=pan, 5=panfall

        //private static int K11 = 0; // ke9ns add set to 5 in RX1 in panfall, otherwise 0


        //private static int K10LAST = 0; // ke9ns add flag to check for only changes in display mode rx2
        //private static int K9LAST = 0;  // ke9ns add flag to check for only changes in display mode rx1

        //private static int K13 = 0; // ke9ns add original H size before being reduced and past onto drawwaterfall to create bmp file size correctly
        //public static int K14 = 0; // ke9ns add used to draw the bmp waterfall 1 time when you changed display modes.
        //private static int K15 = 1; // ke9ns add used to pass the divider factor back to the init() routine to keep the bmp waterfall size correct

        //private static float temp_low_threshold = 0; // ke9ns add to switch between TX and RX low level waterfall levels
        //private static float temp_high_threshold = 0; // ke9ns add for TX upper level

        //public static int DIS_X = 0; // ke9ns add always the size of picdisplay
        //public static int DIS_Y = 0; // ke9ns add

        //========================================================

        public static bool specready = false;

        private static int displayTargetHeight = 0;	// target height
        private static int displayTargetWidth = 0;	// target width
        private static Control displayTarget = null;
        public static Control Target {
            get { return displayTarget; }
            set {
                displayTarget = value;

                init(displayTarget.Width, displayTarget.Height);  // we cant change displayTargetWidth, without first updating arrays ! //MW0LGE

                displayTargetHeight = displayTarget.Height;
                displayTargetWidth = displayTarget.Width;
                displayTargetWidth = Math.Min(displayTargetWidth, BUFFER_SIZE); //MW0LGE prevent larger width than BUFFERSIZE
                Audio.ScopeDisplayWidth = displayTargetWidth;

                ResetWaterfallBmp();
                ResetWaterfallBmp2();

                if (specready)
                {
                    console.specRX.GetSpecRX(0).Pixels = displayTargetWidth;
                    console.specRX.GetSpecRX(1).Pixels = displayTargetWidth;
                    console.specRX.GetSpecRX(cmaster.inid(1, 0)).Pixels = displayTargetWidth;
                }
            }
        }

        private static int rx_display_low = -4000;
        public static int RXDisplayLow {
            get { return rx_display_low; }
            set { rx_display_low = value; }
        }

        private static int rx_display_high = 4000;
        public static int RXDisplayHigh {
            get { return rx_display_high; }
            set { rx_display_high = value; }
        }

        private static int rx2_display_low = -4000;
        public static int RX2DisplayLow {
            get { return rx2_display_low; }
            set { rx2_display_low = value; }
        }

        private static int rx2_display_high = 4000;
        public static int RX2DisplayHigh {
            get { return rx2_display_high; }
            set { rx2_display_high = value; }
        }

        private static int tx_display_low = -4000;
        public static int TXDisplayLow {
            get { return tx_display_low; }
            set { tx_display_low = value; }
        }

        private static int tx_display_high = 4000;
        public static int TXDisplayHigh {
            get { return tx_display_high; }
            set { tx_display_high = value; }
        }

        private static int rx_spectrum_display_low = -4000;
        public static int RXSpectrumDisplayLow {
            get { return rx_spectrum_display_low; }
            set { rx_spectrum_display_low = value; }
        }

        private static int rx_spectrum_display_high = 4000;
        public static int RXSpectrumDisplayHigh {
            get { return rx_spectrum_display_high; }
            set { rx_spectrum_display_high = value; }
        }

        private static int rx2_spectrum_display_low = -4000;
        public static int RX2SpectrumDisplayLow {
            get { return rx2_spectrum_display_low; }
            set { rx2_spectrum_display_low = value; }
        }

        private static int rx2_spectrum_display_high = 4000;
        public static int RX2SpectrumDisplayHigh {
            get { return rx2_spectrum_display_high; }
            set { rx2_spectrum_display_high = value; }
        }

        private static int tx_spectrum_display_low = -4000;
        public static int TXSpectrumDisplayLow {
            get { return tx_spectrum_display_low; }
            set { tx_spectrum_display_low = value; }
        }

        private static int tx_spectrum_display_high = 4000;
        public static int TXSpectrumDisplayHigh {
            get { return tx_spectrum_display_high; }
            set { tx_spectrum_display_high = value; }
        }

        private static float rx1_preamp_offset = 0.0f;
        public static float RX1PreampOffset {
            get { return rx1_preamp_offset; }
            set { rx1_preamp_offset = value; }
        }

        private static float alex_preamp_offset = 0.0f;
        public static float AlexPreampOffset {
            get { return alex_preamp_offset; }
            set { alex_preamp_offset = value; }
        }

        private static float rx2_preamp_offset = 0.0f;
        public static float RX2PreampOffset {
            get { return rx2_preamp_offset; }
            set { rx2_preamp_offset = value; }
        }

        private static bool tx_display_cal_control = false;
        public static bool TXDisplayCalControl {
            get { return tx_display_cal_control; }
            set { tx_display_cal_control = value; }
        }

        private static float rx1_display_cal_offset;					// display calibration offset in dB
        public static float RX1DisplayCalOffset {
            get { return rx1_display_cal_offset; }
            set {
                rx1_display_cal_offset = value;
            }
        }

        private static float rx2_display_cal_offset;					// display calibration offset in dB
        public static float RX2DisplayCalOffset {
            get { return rx2_display_cal_offset; }
            set { rx2_display_cal_offset = value; }
        }

        private static float rx1_fft_size_offset;					// display calibration offset in dB
        public static float RX1FFTSizeOffset {
            get { return rx1_fft_size_offset; }
            set {
                rx1_fft_size_offset = value;
            }
        }

        private static float rx2_fft_size_offset;					// display calibration offset in dB
        public static float RX2FFTSizeOffset {
            get { return rx2_fft_size_offset; }
            set { rx2_fft_size_offset = value; }
        }


        private static float tx_display_cal_offset = 0f;					// display calibration offset in dB
        public static float TXDisplayCalOffset {
            get { return tx_display_cal_offset; }
            set {
                tx_display_cal_offset = value;
            }
        }

        private static HPSDRModel current_hpsdr_model = HPSDRModel.ANAN7000D;
        public static HPSDRModel CurrentHPSDRModel {
            get { return current_hpsdr_model; }
            set { current_hpsdr_model = value; }
        }

        private static int display_cursor_x;						// x-coord of the cursor when over the display
        public static int DisplayCursorX {
            get { return display_cursor_x; }
            set { display_cursor_x = value; }
        }

        private static int display_cursor_y;						// y-coord of the cursor when over the display
        public static int DisplayCursorY {
            get { return display_cursor_y; }
            set { display_cursor_y = value; }
        }

        private static bool grid_control = true;
        public static bool GridControl {
            get { return grid_control; }
            set { grid_control = value; }
        }

        private static bool show_agc = true;
        public static bool ShowAGC {
            get { return show_agc; }
            set { show_agc = value; }
        }

        private static bool spectrum_line = true;
        public static bool SpectrumLine {
            get { return spectrum_line; }
            set { spectrum_line = value; }
        }

        private static bool display_agc_hang_line = true;
        public static bool DisplayAGCHangLine {
            get { return display_agc_hang_line; }
            set { display_agc_hang_line = value; }
        }

        private static bool rx1_hang_spectrum_line = true;
        public static bool RX1HangSpectrumLine {
            get { return rx1_hang_spectrum_line; }
            set { rx1_hang_spectrum_line = value; }
        }

        private static bool display_rx2_gain_line = true;
        public static bool DisplayRX2GainLine {
            get { return display_rx2_gain_line; }
            set { display_rx2_gain_line = value; }
        }

        private static bool rx2_gain_spectrum_line = true;
        public static bool RX2GainSpectrumLine {
            get { return rx2_gain_spectrum_line; }
            set { rx2_gain_spectrum_line = value; }
        }

        private static bool display_rx2_hang_line = true;
        public static bool DisplayRX2HangLine {
            get { return display_rx2_hang_line; }
            set { display_rx2_hang_line = value; }
        }

        private static bool rx2_hang_spectrum_line = true;
        public static bool RX2HangSpectrumLine {
            get { return rx2_hang_spectrum_line; }
            set { rx2_hang_spectrum_line = value; }
        }

        private static bool tx_grid_control = true;
        public static bool TXGridControl {
            get { return tx_grid_control; }
            set { tx_grid_control = value; }
        }

        private static ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;
        public static ClickTuneMode CurrentClickTuneMode {
            get { return current_click_tune_mode; }
            set { current_click_tune_mode = value; }
        }

        private static int scope_time = 50;
        public static int ScopeTime {
            get { return scope_time; }
            set { scope_time = value; }
        }

        private static int sample_rate_rx1 = 384000;
        public static int SampleRateRX1 {
            get { return sample_rate_rx1; }
            set { sample_rate_rx1 = value; }
        }

        private static int sample_rate_rx2 = 384000;
        public static int SampleRateRX2 {
            get { return sample_rate_rx2; }
            set { sample_rate_rx2 = value; }
        }

        private static int sample_rate_tx = 192000;
        public static int SampleRateTX {
            get { return sample_rate_tx; }
            set { sample_rate_tx = value; }
        }

        private static bool high_swr = false;
        public static bool HighSWR {
            get { return high_swr; }
            set { high_swr = value; }
        }

        private static DisplayEngine current_display_engine = DisplayEngine.GDI_PLUS;
        public static DisplayEngine CurrentDisplayEngine {
            get { return current_display_engine; }
            set { current_display_engine = value; }
        }

        private static bool mox = false;
        public static bool MOX {
            get { return mox; }
            set { mox = value; }
        }


        private static bool blank_bottom_display = false;
        public static bool BlankBottomDisplay {
            get { return blank_bottom_display; }
            set { blank_bottom_display = value; }
        }

        private static DSPMode rx1_dsp_mode = DSPMode.USB;
        public static DSPMode RX1DSPMode {
            get { return rx1_dsp_mode; }
            set { rx1_dsp_mode = value; }
        }

        private static DSPMode rx2_dsp_mode = DSPMode.USB;
        public static DSPMode RX2DSPMode {
            get { return rx2_dsp_mode; }
            set { rx2_dsp_mode = value; }
        }

        private static MNotch m_objHightlightedNotch;
        public static MNotch HighlightNotch {
            get { return m_objHightlightedNotch; }
            set { m_objHightlightedNotch = value; }
        }

        private static DisplayMode current_display_mode = DisplayMode.PANAFALL;
        public static DisplayMode CurrentDisplayMode {
            get { return current_display_mode; }
            set {
                current_display_mode = value;

                if (console.PowerOn)
                    console.pause_DisplayThread = true;

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
                console.pause_DisplayThread = false;
            }
        }

        private static float max_x;								// x-coord of maxmimum over one display pass
        public static float MaxX {
            get { return max_x; }
            set { max_x = value; }
        }

        private static float max_y;								// y-coord of maxmimum over one display pass
        public static float MaxY {
            get { return max_y; }
            set { max_y = value; }
        }

        private static float scope_max_x;								// x-coord of maxmimum over one display pass
        public static float ScopeMaxX {
            get { return scope_max_x; }
            set { scope_max_x = value; }
        }

        private static float scope_max_y;								// y-coord of maxmimum over one display pass
        public static float ScopeMaxY {
            get { return scope_max_y; }
            set { scope_max_y = value; }
        }

        private static bool average_on;							// True if the Average button is pressed
        public static bool AverageOn {
            get { return average_on; }
            set {
                average_on = value;
                if (!average_on) ResetRX1DisplayAverage();
            }
        }

        private static bool rx2_avg_on;
        public static bool RX2AverageOn {
            get { return rx2_avg_on; }
            set {
                rx2_avg_on = value;
                if (!rx2_avg_on) ResetRX2DisplayAverage();
            }
        }

        private static bool rx2_enabled;
        public static bool RX2Enabled {
            get { return rx2_enabled; }
            set { rx2_enabled = value; }
        }

        private static bool peak_on;							// True if the Peak button is pressed
        public static bool PeakOn {
            get { return peak_on; }
            set {
                peak_on = value;
                if (!peak_on) ResetRX1DisplayPeak();
            }
        }

        private static bool rx2_peak_on;
        public static bool RX2PeakOn {
            get { return rx2_peak_on; }
            set {
                rx2_peak_on = value;
                if (!rx2_peak_on) ResetRX2DisplayPeak();
            }
        }

        private static bool data_ready;					// True when there is new display data ready from the DSP
        public static bool DataReady {
            get { return data_ready; }
            set { data_ready = value; }
        }

        private static bool data_ready_bottom;
        public static bool DataReadyBottom {
            get { return data_ready_bottom; }
            set { data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready_bottom;
        public static bool WaterfallDataReadyBottom {
            get { return waterfall_data_ready_bottom; }
            set { waterfall_data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready;
        public static bool WaterfallDataReady {
            get { return waterfall_data_ready; }
            set { waterfall_data_ready = value; }
        }

        private static int spectrum_grid_max = -40;
        public static int SpectrumGridMax {
            get { return spectrum_grid_max; }
            set {
                spectrum_grid_max = value;
            }
        }

        private static int spectrum_grid_min = -140;
        public static int SpectrumGridMin {
            get { return spectrum_grid_min; }
            set {
                spectrum_grid_min = value;
            }
        }

        private static int spectrum_grid_step = 5;
        public static int SpectrumGridStep {
            get { return spectrum_grid_step; }
            set {
                spectrum_grid_step = value;
            }
        }

        private static int rx2_spectrum_grid_max = -40;
        public static int RX2SpectrumGridMax {
            get { return rx2_spectrum_grid_max; }
            set {
                rx2_spectrum_grid_max = value;
            }
        }

        private static int rx2_spectrum_grid_min = -140;
        public static int RX2SpectrumGridMin {
            get { return rx2_spectrum_grid_min; }
            set {
                rx2_spectrum_grid_min = value;
            }
        }

        private static int rx2_spectrum_grid_step = 5;
        public static int RX2SpectrumGridStep {
            get { return rx2_spectrum_grid_step; }
            set {
                rx2_spectrum_grid_step = value;
            }
        }

        private static int tx_spectrum_grid_max = 20;
        public static int TXSpectrumGridMax {
            get { return tx_spectrum_grid_max; }
            set {
                tx_spectrum_grid_max = value;
            }
        }

        private static int tx_spectrum_grid_min = -80;
        public static int TXSpectrumGridMin {
            get { return tx_spectrum_grid_min; }
            set {
                tx_spectrum_grid_min = value;
            }
        }

        private static int tx_spectrum_grid_step = 5;
        public static int TXSpectrumGridStep {
            get { return tx_spectrum_grid_step; }
            set {
                tx_spectrum_grid_step = value;
            }
        }

        private static int tx_wf_amp_max = 30;
        public static int TXWFAmpMax {
            get { return tx_wf_amp_max; }
            set {
                tx_wf_amp_max = value;
            }
        }

        private static int tx_wf_amp_min = -70;
        public static int TXWFAmpMin {
            get { return tx_wf_amp_min; }
            set {
                tx_wf_amp_min = value;
            }
        }

        private static Pen peak_blob_pen = new Pen(Color.OrangeRed);
        private static Pen peak_blob_text_pen = new Pen(Color.YellowGreen);

        private static Color band_edge_color = Color.Red;
        private static Pen band_edge_pen = new Pen(band_edge_color);
        public static Color BandEdgeColor {
            get { return band_edge_color; }
            set {
                band_edge_color = value;
                band_edge_pen.Color = band_edge_color;
                buildDX2Resources();
            }
        }

        private static Color tx_band_edge_color = Color.Red;
        private static Pen tx_band_edge_pen = new Pen(tx_band_edge_color);
        public static Color TXBandEdgeColor {
            get { return tx_band_edge_color; }
            set {
                tx_band_edge_color = value;
                tx_band_edge_pen.Color = tx_band_edge_color;
                buildDX2Resources();
            }
        }

        private static Color sub_rx_zero_line_color = Color.LightSkyBlue;
        private static Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color, 2); // MW0LGE width 2
        public static Color SubRXZeroLine {
            get { return sub_rx_zero_line_color; }
            set {
                sub_rx_zero_line_color = value;
                sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
                buildDX2Resources();
            }
        }

        private static Color sub_rx_filter_color = Color.Blue;
        private static SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);
        public static Color SubRXFilterColor {
            get { return sub_rx_filter_color; }
            set {
                sub_rx_filter_color = value;
                sub_rx_filter_brush.Color = sub_rx_filter_color;
                buildDX2Resources();
            }
        }

        private static Color grid_text_color = Color.Yellow;
        private static SolidBrush grid_text_brush = new SolidBrush(grid_text_color);
        private static Pen grid_text_pen = new Pen(grid_text_color);
        public static Color GridTextColor {
            get { return grid_text_color; }
            set {
                grid_text_color = value;
                grid_text_brush.Color = grid_text_color;
                grid_text_pen.Color = grid_text_color;
                buildDX2Resources();
            }
        }

        private static Color grid_tx_text_color = Color.FromArgb(255, Color.Yellow);
        private static SolidBrush grid_tx_text_brush = new SolidBrush(Color.FromArgb(255, grid_tx_text_color));
        public static Color GridTXTextColor {
            get { return grid_tx_text_color; }
            set {
                grid_tx_text_color = value;
                grid_tx_text_brush.Color = grid_tx_text_color;
                buildDX2Resources();
            }
        }

        private static Color grid_zero_color = Color.Red;
        private static Pen grid_zero_pen = new Pen(grid_zero_color, 2); // MW0LGE width 2
        public static Color GridZeroColor {
            get { return grid_zero_color; }
            set {
                grid_zero_color = value;
                grid_zero_pen.Color = grid_zero_color;
                buildDX2Resources();
            }
        }

        private static Color tx_grid_zero_color = Color.FromArgb(255, Color.Red);
        private static Pen tx_grid_zero_pen = new Pen(Color.FromArgb(255, tx_grid_zero_color), 2); //MW0LGE width 2
        public static Color TXGridZeroColor {
            get { return tx_grid_zero_color; }
            set {
                tx_grid_zero_color = value;
                tx_grid_zero_pen.Color = tx_grid_zero_color;
                buildDX2Resources();
            }
        }

        private static Color grid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen = new Pen(grid_color);
        public static Color GridColor {
            get { return grid_color; }
            set {
                grid_color = value;
                grid_pen.Color = grid_color;
                buildDX2Resources();
            }
        }

        private static Color tx_vgrid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen = new Pen(tx_vgrid_color);
        public static Color TXVGridColor {
            get { return tx_vgrid_color; }
            set {
                tx_vgrid_color = value;
                tx_vgrid_pen.Color = tx_vgrid_color;
                buildDX2Resources();
            }
        }


        private static Color hgrid_color = Color.White;
        private static Pen hgrid_pen = new Pen(hgrid_color);
        public static Color HGridColor {
            get { return hgrid_color; }
            set {
                hgrid_color = value;
                hgrid_pen = new Pen(hgrid_color);
                buildDX2Resources();
            }
        }


        private static Color tx_hgrid_color = Color.White;
        private static Pen tx_hgrid_pen = new Pen(tx_hgrid_color);
        public static Color TXHGridColor {
            get { return tx_hgrid_color; }
            set {
                tx_hgrid_color = value;
                tx_hgrid_pen = new Pen(tx_hgrid_color);
                buildDX2Resources();
            }
        }

        //MW0LGE
        private static Color data_fill_color = Color.FromArgb(128, Color.Blue);
        private static Pen data_fill_pen = new Pen(new SolidBrush(data_fill_color), 1.0F);
        private static Pen data_fill_fpen = new Pen(data_fill_color);
        public static Color DataFillColor {
            get { return data_fill_color; }
            set {
                data_fill_color = value;
                data_fill_pen.Color = data_fill_color;
                data_fill_fpen.Color = data_fill_color;
                buildDX2Resources();
            }
        }

        private static Color data_line_color = Color.White;
        private static Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1.0F);
        private static Pen data_line_fpen = new Pen(Color.FromArgb(100, data_line_color));
        public static Color DataLineColor {
            get { return data_line_color; }
            set {
                data_line_color = value;
                data_line_pen.Color = data_line_color;
                data_line_fpen.Color = Color.FromArgb(100, data_line_color);
                buildDX2Resources();
            }
        }

        private static Color tx_data_line_color = Color.White;
        private static Pen tx_data_line_pen = new Pen(new SolidBrush(tx_data_line_color), 1.0F);
        private static Pen tx_data_line_fpen = new Pen(Color.FromArgb(100, tx_data_line_color));
        public static Color TXDataLineColor {
            get { return tx_data_line_color; }
            set {
                tx_data_line_color = value;
                tx_data_line_pen.Color = tx_data_line_color;
                tx_data_line_fpen.Color = Color.FromArgb(100, tx_data_line_color);
                buildDX2Resources();
            }
        }

        private static Color grid_pen_dark = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen_inb = new Pen(grid_pen_dark);
        public static Color GridPenDark {
            get { return grid_pen_dark; }
            set {
                grid_pen_dark = value;
                grid_pen_inb.Color = grid_pen_dark;
                buildDX2Resources();
            }
        }

        private static Color tx_vgrid_pen_fine = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen_inb = new Pen(tx_vgrid_pen_fine);
        public static Color TXVGridPenFine {
            get { return tx_vgrid_pen_fine; }
            set {
                tx_vgrid_pen_fine = value;
                tx_vgrid_pen_inb.Color = tx_vgrid_pen_fine;
                buildDX2Resources();
            }
        }

        private static Color display_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush display_filter_brush = new SolidBrush(display_filter_color);
        private static Pen cw_zero_pen = new Pen(Color.FromArgb(255, display_filter_color), 2); // MW0LGE width 2
        public static Color DisplayFilterColor {
            get { return display_filter_color; }
            set {
                display_filter_color = value;
                display_filter_brush.Color = display_filter_color;
                cw_zero_pen.Color = Color.FromArgb(255, display_filter_color);
                buildDX2Resources();
            }
        }

        private static Color tx_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush tx_filter_brush = new SolidBrush(tx_filter_color);
        public static Color TXFilterColor {
            get { return tx_filter_color; }
            set {
                tx_filter_color = value;
                tx_filter_brush.Color = tx_filter_color;
                buildDX2Resources();
            }
        }

        private static Color display_filter_tx_color = Color.Yellow;
        private static Pen tx_filter_pen = new Pen(display_filter_tx_color, 2); // width 2 MW0LGE
        public static Color DisplayFilterTXColor {
            get { return display_filter_tx_color; }
            set {
                display_filter_tx_color = value;
                tx_filter_pen.Color = display_filter_tx_color;
                buildDX2Resources();
            }
        }

        private static Color display_background_color = Color.Black;
        private static SolidBrush display_background_brush = new SolidBrush(display_background_color);
        public static Color DisplayBackgroundColor {
            get { return display_background_color; }
            set {
                display_background_color = value;
                display_background_brush.Color = display_background_color;
                buildDX2Resources();
            }
        }

        private static Color tx_display_background_color = Color.Black;
        private static SolidBrush tx_display_background_brush = new SolidBrush(tx_display_background_color);
        public static Color TXDisplayBackgroundColor {
            get { return tx_display_background_color; }
            set {
                tx_display_background_color = value;
                tx_display_background_brush.Color = tx_display_background_color;
                buildDX2Resources();
            }
        }

        //MW0LGE
        private static bool m_bShowTXFilterOnWaterfall = true;
        public static bool ShowTXFilterOnWaterfall {
            get { return m_bShowTXFilterOnWaterfall; }
            set {
                m_bShowTXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowRXFilterOnWaterfall = true;
        public static bool ShowRXFilterOnWaterfall {
            get { return m_bShowRXFilterOnWaterfall; }
            set {
                m_bShowRXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowTXZeroLineOnWaterfall = true;
        public static bool ShowTXZeroLineOnWaterfall {
            get { return m_bShowTXZeroLineOnWaterfall; }
            set {
                m_bShowTXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowRXZeroLineOnWaterfall = true;
        public static bool ShowRXZeroLineOnWaterfall {
            get { return m_bShowRXZeroLineOnWaterfall; }
            set {
                m_bShowRXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowTXFilterOnRXWaterfall = true;
        public static bool ShowTXFilterOnRXWaterfall {
            get { return m_bShowTXFilterOnRXWaterfall; }
            set {
                m_bShowTXFilterOnRXWaterfall = value;
            }
        }

        private static bool draw_tx_filter = true;
        public static bool DrawTXFilter {
            get { return draw_tx_filter; }
            set {
                draw_tx_filter = value;
            }
        }

        private static bool show_cwzero_line = false;
        public static bool ShowCWZeroLine {
            get { return show_cwzero_line; }
            set {
                show_cwzero_line = value;
            }
        }

        private static bool draw_tx_cw_freq = false;
        public static bool DrawTXCWFreq {
            get { return draw_tx_cw_freq; }
            set {
                draw_tx_cw_freq = value;
            }
        }

        private static Color waterfall_low_color = Color.Black;
        public static Color WaterfallLowColor {
            get { return waterfall_low_color; }
            set { waterfall_low_color = value; }
        }

        private static Color waterfall_mid_color = Color.Red;
        public static Color WaterfallMidColor {
            get { return waterfall_mid_color; }
            set { waterfall_mid_color = value; }
        }

        private static Color waterfall_high_color = Color.Yellow;
        public static Color WaterfallHighColor {
            get { return waterfall_high_color; }
            set { waterfall_high_color = value; }
        }

        private static float waterfall_high_threshold = -80.0F;
        public static float WaterfallHighThreshold {
            get { return waterfall_high_threshold; }
            set { waterfall_high_threshold = value; }
        }

        private static float waterfall_low_threshold = -130.0F;
        public static float WaterfallLowThreshold {
            get { return waterfall_low_threshold; }
            set { waterfall_low_threshold = value; }
        }

        //================================================================
        // ke9ns add signal from console about Grayscale ON/OFF
        private static byte Gray_Scale = 0; //  ke9ns ADD from console 0=RGB  1=Gray
        public static byte GrayScale       // this is called or set in console
        {
            get { return Gray_Scale; }
            set {
                Gray_Scale = value;
            }
        }


        //================================================================
        // kes9ns add signal from setup grid lines on/off
        private static byte grid_off = 0; //  ke9ns ADD from setup 0=normal  1=gridlines off
        public static byte GridOff       // this is called or set in setup
        {
            get { return grid_off; }
            set {
                grid_off = value;
            }
        }


        private static Color rx2_waterfall_low_color = Color.Black;
        public static Color RX2WaterfallLowColor {
            get { return rx2_waterfall_low_color; }
            set { rx2_waterfall_low_color = value; }
        }

        private static Color rx2_waterfall_mid_color = Color.Red;
        public static Color RX2WaterfallMidColor {
            get { return rx2_waterfall_mid_color; }
            set { rx2_waterfall_mid_color = value; }
        }

        private static Color rx2_waterfall_high_color = Color.Yellow;
        public static Color RX2WaterfallHighColor {
            get { return rx2_waterfall_high_color; }
            set { rx2_waterfall_high_color = value; }
        }

        private static float rx2_waterfall_high_threshold = -80.0F;
        public static float RX2WaterfallHighThreshold {
            get { return rx2_waterfall_high_threshold; }
            set { rx2_waterfall_high_threshold = value; }
        }

        private static float rx2_waterfall_low_threshold = -130.0F;
        public static float RX2WaterfallLowThreshold {
            get { return rx2_waterfall_low_threshold; }
            set { rx2_waterfall_low_threshold = value; }
        }

        private static float display_line_width = 1.0F;
        public static float DisplayLineWidth {
            get { return display_line_width; }
            set {
                display_line_width = value;
                data_line_pen.Width = display_line_width;
            }
        }

        private static float tx_display_line_width = 1.0F;
        public static float TXDisplayLineWidth {
            get { return tx_display_line_width; }
            set {
                tx_display_line_width = value;
                tx_data_line_pen.Width = tx_display_line_width;
            }
        }

        private static DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;
        public static DisplayLabelAlignment DisplayLabelAlign {
            get { return display_label_align; }
            set {
                display_label_align = value;
            }
        }

        private static DisplayLabelAlignment tx_display_label_align = DisplayLabelAlignment.CENTER;
        public static DisplayLabelAlignment TXDisplayLabelAlign {
            get { return tx_display_label_align; }
            set {
                tx_display_label_align = value;
            }
        }

        private static int phase_num_pts = 100;
        public static int PhaseNumPts {
            get { return phase_num_pts; }
            set { phase_num_pts = value; }
        }

        private static bool click_tune_filter = false;
        public static bool ClickTuneFilter {
            get { return click_tune_filter; }
            set { click_tune_filter = value; }
        }

        private static bool show_cth_line = false;
        public static bool ShowCTHLine {
            get { return show_cth_line; }
            set { show_cth_line = value; }
        }

        private static double f_center = vfoa_hz;
        public static double F_Center {
            get { return f_center; }
            set {
                f_center = value;
            }
        }

        private static int top_size = 0;
        public static int TopSize {
            get { return top_size; }
            set { top_size = value; }
        }

        private static int _lin_corr = 2;
        public static int LinCor {
            get {
                return _lin_corr;
            }
            set {
                _lin_corr = value;
            }
        }

        private static int _linlog_corr = -14;
        public static int LinLogCor {
            get {
                return _linlog_corr;
            }
            set {
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

        #region General Routines


        #region GDI+ General Routines

        // use pools to reduce GC
        private static ArrayPool<float> m_objFloatPool = ArrayPool<float>.Shared;
        private static ArrayPool<int> m_objIntPool = ArrayPool<int>.Shared;

        private static void init(int W, int H)
        {
            if (histogram_data != null) m_objIntPool.Return(histogram_data);
            if (histogram_history != null) m_objIntPool.Return(histogram_history);

            histogram_data = m_objIntPool.Rent(W);
            histogram_history = m_objIntPool.Rent(W);

            Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
            {
                histogram_data[i] = Int32.MaxValue;
                histogram_history[i] = 0;
            });

            //if (current_display_engine == DisplayEngine.GDI_PLUS)
            //{
            //    if (waterfall_bmp != null) waterfall_bmp.Dispose(); //MW0GLE added dispose
            //    if (waterfall_bmp2 != null) waterfall_bmp2.Dispose();
            //    waterfall_bmp = new Bitmap(W, H - 20, PixelFormat.Format24bppRgb);  // initialize waterfall display
            //    waterfall_bmp2 = new Bitmap(W, H - 20, PixelFormat.Format24bppRgb);
            //}
            //else // dx2d MW0LGE
            //{
            //    lock (m_objDX2Lock)
            //    {
            //        if (waterfall_bmp_dx2d != null) waterfall_bmp_dx2d.Dispose();
            //        if (waterfall_bmp2_dx2d != null) waterfall_bmp2_dx2d.Dispose();
            //        waterfall_bmp_dx2d = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2(W, H - 20), new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            //        waterfall_bmp2_dx2d = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2(W, H - 20), new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            //    }
            //}

            if (rx1_average_buffer != null) m_objFloatPool.Return(rx1_average_buffer);
            if (rx2_average_buffer != null) m_objFloatPool.Return(rx2_average_buffer);
            if (rx1_peak_buffer != null) m_objFloatPool.Return(rx1_peak_buffer);
            if (rx2_peak_buffer != null) m_objFloatPool.Return(rx2_peak_buffer);

            //displayTargetWidth  or BUFFER_SIZE?
            rx1_average_buffer = m_objFloatPool.Rent(W);	// initialize averaging buffer array
            rx1_average_buffer[0] = CLEAR_FLAG;	// set the clear flag

            rx2_average_buffer = m_objFloatPool.Rent(W);  // initialize averaging buffer array
            rx2_average_buffer[0] = CLEAR_FLAG;		// set the clear flag

            rx1_peak_buffer = m_objFloatPool.Rent(W);
            rx1_peak_buffer[0] = CLEAR_FLAG;

            rx2_peak_buffer = m_objFloatPool.Rent(W);
            rx2_peak_buffer[0] = CLEAR_FLAG;

            resetMaximums(1);
            resetMaximums(2);

            if (new_display_data != null) m_objFloatPool.Return(new_display_data);
            if (current_display_data != null) m_objFloatPool.Return(current_display_data);
            if (new_display_data_bottom != null) m_objFloatPool.Return(new_display_data_bottom);
            if (current_display_data_bottom != null) m_objFloatPool.Return(current_display_data_bottom);
            if (new_waterfall_data != null) m_objFloatPool.Return(new_waterfall_data);
            if (current_waterfall_data != null) m_objFloatPool.Return(current_waterfall_data);
            if (new_waterfall_data_bottom != null) m_objFloatPool.Return(new_waterfall_data_bottom);
            if (current_waterfall_data_bottom != null) m_objFloatPool.Return(current_waterfall_data_bottom);

            // cant be W width, as more info can be stored in these, for example scope data
            new_display_data = m_objFloatPool.Rent(BUFFER_SIZE);
            current_display_data = m_objFloatPool.Rent(BUFFER_SIZE);
            new_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);
            current_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);

            new_waterfall_data = m_objFloatPool.Rent(W);
            current_waterfall_data = m_objFloatPool.Rent(W);
            new_waterfall_data_bottom = m_objFloatPool.Rent(W);
            current_waterfall_data_bottom = m_objFloatPool.Rent(W);

            Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
            {
                new_display_data[i] = -200.0f;
                current_display_data[i] = -200.0f;
                new_display_data_bottom[i] = -200.0f;
                current_display_data_bottom[i] = -200.0f;
                new_waterfall_data[i] = -200.0f;
                current_waterfall_data[i] = -200.0f;
                new_waterfall_data_bottom[i] = -200.0f;
                current_waterfall_data_bottom[i] = -200.0f;
            });
        }

        public static void DrawBackground()
        {
            // draws the background image for the display based
            // on the current selected display mode.

            if (current_display_engine == DisplayEngine.GDI_PLUS)
            {
                displayTarget.Invalidate();
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

            // shade in the channel
            g.FillRectangle(new SolidBrush(c), left, top, width, height);

            // draw a left and right line on the side of the rectancle if wide enough
            if (width > 2)
            {
                g.DrawLine(p, left, top, left, top + height - 1);
                g.DrawLine(p, right, top, right, top + height - 1);
            }
        }

        #endregion

        #region GDI+

        unsafe public static void RenderGDIPlus(ref PaintEventArgs e)
        {
            // MW0LGE to do, sort out K variables, and stuff for Map overlay, some has been commented for now
            // just to get dual panafalls up and running
            try
            {
                float fTopHeight;
                fTopHeight = SpecialPanafall ? 0.8f : 0.5f;

                if (m_bAntiAlias)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                }
                else
                {
                    e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                }

                if (!split_display)
                {
                    m_nRX1DisplayHeight = displayTargetHeight;
                    switch (current_display_mode)
                    {
                        case DisplayMode.SPECTRUM:
                            K9 = 4;
                            //K11 = 0;
                            DrawSpectrum(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PANADAPTER:
                            K9 = 2;
                            //K11 = 0;
                            DrawPanadapter(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            break;
                        case DisplayMode.SCOPE:
                            K9 = 4;
                            //K11 = 0;
                            DrawScope(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.SCOPE2:
                            DrawScope2(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PHASE:
                            K9 = 4;
                            //K11 = 0;
                            DrawPhase(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PHASE2:
                            K9 = 4;
                            //K11 = 0;
                            DrawPhase2(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.WATERFALL:
                            K9 = 1;
                            //K11 = 0;
                            DrawWaterfall(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            break;
                        case DisplayMode.HISTOGRAM:
                            K9 = 4;
                            //K11 = 0;
                            DrawHistogram(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight);
                            break;
                        case DisplayMode.PANAFALL:
                            K9 = 3;
                            //K11 = 0;
                            m_nRX1DisplayHeight = (int)(displayTargetHeight * m_fPanafallSplitPerc);
                            split_display = PanafallSplitBarPos <= (displayTargetHeight / 2); // add more granularity, TODO change based on avaialble height
                            DrawPanadapter(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            DrawWaterfall(e.Graphics, PanafallSplitBarPos, displayTargetWidth, displayTargetHeight - m_nRX1DisplayHeight, 1, true);
                            split_display = false;
                            break;
                        case DisplayMode.PANASCOPE:
                            K9 = 4;
                            //K11 = 0;
                            m_nRX1DisplayHeight = displayTargetHeight / 2;
                            split_display = true;
                            DrawPanadapter(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            DrawScope(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, true);
                            split_display = false;
                            break;
                        case DisplayMode.SPECTRASCOPE:
                            K9 = 4;
                            //K11 = 0;
                            m_nRX1DisplayHeight = displayTargetHeight / 2;
                            split_display = true;
                            DrawSpectrum(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            DrawScope(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, true);
                            split_display = false;
                            break;
                        case DisplayMode.OFF:
                            K9 = 0;
                            //K11 = 0;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    m_nRX1DisplayHeight = displayTargetHeight / 2;
                    switch (current_display_mode)
                    {
                        case DisplayMode.SPECTRUM:
                            K9 = 4;
                            //K11 = 0;
                            DrawSpectrum(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PANADAPTER:
                            K9 = 2;
                            //K11 = 0;
                            DrawPanadapter(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            break;
                        case DisplayMode.SCOPE:
                            K9 = 4;
                            //K11 = 0;
                            DrawScope(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PHASE:
                            K9 = 4;
                            //K11 = 0;
                            DrawPhase(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.PHASE2:
                            //K9 = 4;
                            //K11 = 0; // these were outside phase2 for some reason //MW0LGE , commented for now
                            DrawPhase2(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        case DisplayMode.WATERFALL:
                            K9 = 6;
                            //K11 = 0;
                            DrawWaterfall(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                            break;
                        case DisplayMode.HISTOGRAM:
                            K9 = 4;
                            //K11 = 0;
                            DrawHistogram(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight);
                            break;
                        case DisplayMode.PANAFALL:   // ke9ns pan rX1 (KE9NS ADDED CODE)
                            K9 = 5;
                            //K11 = 5;

                            m_nRX1DisplayHeight = displayTargetHeight / 4;
                            DrawPanadapter(e.Graphics, 0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);// MW0LGE - rx2
                            DrawWaterfall(e.Graphics, m_nRX1DisplayHeight, displayTargetWidth, m_nRX1DisplayHeight, 1, true);// MW0LGE - rx2
                            break;  // rx1 panafall
                        case DisplayMode.OFF:
                            K9 = 0;
                            //K11 = 0;
                            DrawOffBackground(e.Graphics, displayTargetWidth, m_nRX1DisplayHeight, false);
                            break;
                        default:
                            break;
                    }

                    // MW0LGE - rx2if (K11 == 0) //if rx1 is in panafall skip below
                    // MW0LGE - rx2{
                    m_nRX2DisplayHeight = displayTargetHeight / 2;
                    switch (current_display_mode_bottom)
                    {
                        //case DisplayMode.SPECTRUM:
                        //        K10 = 0;
                        //    DrawSpectrum(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight, true);
                        //    break;
                        case DisplayMode.PANADAPTER:
                            K10 = 2;
                            DrawPanadapter(e.Graphics, m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                            break;
                        //case DisplayMode.SCOPE:
                        //        K10 = 0;
                        //    DrawScope(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight, true);
                        //    break;
                        //case DisplayMode.PHASE:
                        //        K10 = 0;
                        //    DrawPhase(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight, true);
                        //    break;
                        //case DisplayMode.PHASE2:
                        //        K10 = 0;
                        //    DrawPhase2(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight, true);
                        //    break;
                        case DisplayMode.WATERFALL:
                            K10 = 1;
                            DrawWaterfall(e.Graphics, m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                            break;
                        //case DisplayMode.HISTOGRAM:
                        //        K10 = 0;
                        //    DrawHistogram(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight);
                        //    break;
                        case DisplayMode.OFF:
                            K10 = 0;
                            DrawOffBackground(e.Graphics, displayTargetWidth, m_nRX2DisplayHeight, true);
                            /*switch (current_display_mode) // ke9ns split display (RX1 top  and RX2 on bottom)
                            {
                                case DisplayMode.PANAFALL:
                                    K9 = 3;
                                    K11 = 0;

                                    split_display = true; // use wide vertgrid because your saying split
                                    DrawPanadapter(e.Graphics, W, H / 2, 1, false); //top half 
                                    DrawWaterfall(e.Graphics, W, H / 2, 1, true); // bottom half RX2 is not on
                                    split_display = false;
                                    break;
                            }*/ //MW0LGE - rx2
                            break;
                        case DisplayMode.PANAFALL:
                            K10 = 2;
                            //DrawPanadapter(e.Graphics, W, H / 2, 2, true); // RX2  (standard mode)  // MW0LGE - rx2
                            m_nRX2DisplayHeight = displayTargetHeight / 4;
                            DrawPanadapter(e.Graphics, m_nRX2DisplayHeight * 2, displayTargetWidth, m_nRX2DisplayHeight, 2, false);  // MW0LGE - rx2
                            DrawWaterfall(e.Graphics, m_nRX2DisplayHeight * 3, displayTargetWidth, m_nRX2DisplayHeight, 2, true); // MW0LGE - rx2
                            break;
                        default:
                            //MW0LGE//K10 = 2;
                            //DrawPanadapter(e.Graphics, W, H / 2, 2, true); // RX2  (standard mode)
                            break;
                            // MW0LGE - rx2}
                    }
                    // MW0LGE - rx2 else // rx1 in panafall mode
                    // MW0LGE - rx2{
                    // MW0LGE - rx2switch (current_display_mode_bottom)  // ke9ns pan
                    // MW0LGE - rx2{

                    // MW0LGE - rx2case DisplayMode.OFF:
                    // MW0LGE - rx2K10 = 0;
                    // MW0LGE - rx2DrawOffBackground(e.Graphics, W, H / 2, true);
                    // MW0LGE - rx2break; // RX2 OFF
                    // MW0LGE - rx2}
                    // MW0LGE - rx2}
                }

                // HIGH swr display warning
                if (high_swr)
                {
                    e.Graphics.DrawString("High SWR", font14, Brushes.Red, 245, 20);
                    using (Pen p = new Pen(Color.Red, 6f))
                    {
                        e.Graphics.DrawRectangle(p, 3, 3, displayTargetWidth - 6, displayTargetHeight - 6);
                    }
                }

                //MW0LGE framrate issue, this will be true if less than desired frame rate achevied, set in RunDisplay thread of console.cs
                if (m_bFramRateIssue) e.Graphics.FillRectangle(new SolidBrush(Color.Red), 0, 0, 8, 8);

                calcFps();
                if (m_bShowFPS) e.Graphics.DrawString(m_nFps.ToString(), m_fntCallOutFont, m_bTextCallOutActive, 10, 0);
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
        }

        private static void UpdateDisplayPeak(float[] buffer, float[] new_data)
        {
            if (buffer[0] == CLEAR_FLAG)
            {
                for (int i = 0; i < displayTargetWidth/*BUFFER_SIZE*/; i++)
                    buffer[i] = new_data[i];
            }
            else
            {
                for (int i = 0; i < displayTargetWidth/*BUFFER_SIZE*/; i++)
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
        }

        private static void DrawScopeGrid(ref Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);
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
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0;

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
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
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
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
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

                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
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

        //private static SizeF length;                                          // ke9ns add length of call sign so we can do usb/lsb and define a box to click into
        //private static SizeF length1;                                          // ke9ns add length of call sign so we can do usb/lsb and define a box to click into

        private static bool m_bLSB = false;                                     // ke9ns add true=LSB, false=USB
        private static int rx2 = 0;                                          // ke9ns add 0-49 spots for rx1 panadapter window for qrz callup  (50-100 for rx2)
        //private static int rx3 = 0;                                          // ke9ns add 0-49 spots for rx1 panadapter window for qrz callup  (50-100 for rx2)

        public static int VFOLow = 0;                                       // ke9ns low freq (left side of screen) in HZ (used in DX_spot)
        public static int VFOHigh = 0;                                      // ke9ns high freq (right side of screen) in HZ
        public static int VFODiff = 0;                                      // ke9ns diff high-low

        static void clearBackground(ref Graphics g, int rx, int W, int H, bool bottom)
        {
            // MW0LGE
            if (rx == 1)
            {
                if (bottom)
                {
                    g.FillRectangle(display_background_brush, 0, H, W, H);
                }
                else
                {
                    g.FillRectangle(display_background_brush, 0, 0, W, H);
                }
            }
            else if (rx == 2)
            {
                if (bottom)
                {
                    if (current_display_mode_bottom == DisplayMode.PANAFALL)
                    {
                        g.FillRectangle(display_background_brush, 0, H * 3, W, H);
                    }
                    else g.FillRectangle(display_background_brush, 0, H, W, H);
                }
                else
                {
                    g.FillRectangle(display_background_brush, 0, H * 2, W, H);
                }
            }
        }

        static float zoom_height = 1.5f;   // Should be > 1.  H = H/zoom_height

        private static void drawFilterOverlay(ref Graphics g, SolidBrush brush, int filter_left_x, int filter_right_x, int W, int H, int rx, int top, bool bottom, int nVerticalShfit)
        {
            // make sure something visible
            if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            // draw rx filter
            int nWidth = filter_right_x - filter_left_x;

            g.FillRectangle(brush, filter_left_x, nVerticalShfit + top, nWidth, H - top);
        }

        private static int getVerticalShift(int rx, bool bottom, int W, int H)
        {
            // return how much we have to shift down by based on display mode, and height;
            int nVerticalShift = 0;

            if (rx == 1)
            {
                if (current_display_mode == DisplayMode.PANAFALL && bottom) nVerticalShift = H;
            }
            else
            {
                if (current_display_mode_bottom == DisplayMode.PANAFALL)
                {
                    if (bottom) nVerticalShift = 3 * H;
                    else nVerticalShift = 2 * H;
                }
                else
                    nVerticalShift = H;
            }

            return nVerticalShift;
        }

        private static Color changeAlpha(Color c, int A)
        {
            return Color.FromArgb(A, c.R, c.G, c.B);
        }

        unsafe private static void drawPanadapterAndWaterfallGrid(ref Graphics g, int nVerticalShift, int W, int H, int rx, bool bottom, bool bIsWaterfall = false)
        {
            // MW0LGE
            // this now draws the grid for either panadapter or waterfall, pass in a bool to pick
            //
            DisplayLabelAlignment label_align = display_label_align;
            bool local_mox = false;
            bool displayduplex = isRxDuplex(rx);
            if (mox && rx == 1 && !tx_on_vfob) local_mox = true;
            if (mox && rx == 2 && tx_on_vfob) local_mox = true;
            if (rx == 1 && tx_on_vfob && mox && !console.RX2Enabled) local_mox = true;
            int Low = 0;					// initialize variables
            int High = 0;
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

            //MW0LGE
            //int nVerticalShift = getVerticalShift(rx, bottom, W, H);
            int cwSideToneShift = getCWSideToneShift(rx);
            int cwSideToneShiftInverted = cwSideToneShift * -1; // invert the sign as cw zero lines/tx lines etc are a shift in opposite direction to the grid

            if (rx == 1)
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    if (displayduplex)
                    {
                        Low = rx_display_low;
                        High = rx_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                    sample_rate = sample_rate_tx;
                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx_display_low;
                    High = rx_display_high;
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                f_diff = freq_diff;
            }
            else// if(rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    if (displayduplex)
                    {
                        Low = rx2_display_low;
                        High = rx2_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                    sample_rate = sample_rate_tx;
                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    grid_step = rx2_spectrum_grid_step;
                    sample_rate = sample_rate_rx2;
                }

                f_diff = rx2_freq_diff;
            }

            if (local_mox && !displayduplex)// || (mox && tx_on_vfob))
            {
                Low = tx_display_low;
                High = tx_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                grid_step = tx_spectrum_grid_step;
                sample_rate = sample_rate_tx;
                label_align = tx_display_label_align;
                if (rx == 1 && !console.VFOBTX) f_diff = freq_diff;
                else f_diff = rx2_freq_diff;
            }
            else if (rx == 2)
            {
                if (local_mox && displayduplex)// || (mox && tx_on_vfob))
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                    label_align = tx_display_label_align;
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
                    label_align = tx_display_label_align;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                f_diff = freq_diff;
            }

            //MW0LGE - TODO CHECK - just pulled from PowerSDR at the mo
            //
            ////-W2PA Correct for transmit scale shifts in split and CTUN modes
            //double diff;
            //diff = 1.0e6 * (console.CenterFrequency - console.VFOAFreq);  // Correction for CTUN: DispCenter - VFOA
            //int ctunDiff = Convert.ToInt32(diff);

            //diff = 1.0e6 * (console.VFOBFreq - console.VFOAFreq);  // Correction for split: VFOB - VFOA
            //int splitDiff = Convert.ToInt32(diff);

            //if (SplitEnabled && RX2Enabled)
            //{
            //    diff = 1.0e6 * (console.VFOASubFreq - console.VFOAFreq);  // Correction for split with RX2 on: VFOASub - VFOA
            //    splitDiff = Convert.ToInt32(diff);
            //}

            ////-W2PA Correct the display for various transmit mode combinations
            //if (local_mox && rx == 1)
            //{
            //    if (SplitEnabled) // Split with either VFOB or VFOAsub on TX, VFOA on RX
            //    {
            //        if (!display_duplex) //DUP off
            //        {
            //            if (console.VFOBTX)  // Tx on VFOB
            //            {
            //                if (console.TUN)  // Adjust display scale and cursor for CW offset in TUN mode
            //                {
            //                    if (rx1_dsp_mode == DSPMode.LSB || rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.DIGL)
            //                    {
            //                        Low -= CWPitch;  // Lower<whatever> mode
            //                        High -= CWPitch;
            //                        f_diff += CWPitch;
            //                    }
            //                    else
            //                    {
            //                        Low += CWPitch;  // All others
            //                        High += CWPitch;
            //                        f_diff -= CWPitch;
            //                    }
            //                }

            //                if (RX2Enabled)
            //                {
            //                    ; // No corrections necessary
            //                }
            //            }
            //            else //Tx on VFOAsub  DUP off
            //            {
            //                ; // no correction necessary here
            //            }
            //        }
            //        else //DUP on and Split 
            //        {
            //            if (console.CTuneDisplay)
            //            {
            //                Low += ctunDiff;
            //                High += ctunDiff;
            //                f_diff -= ctunDiff;
            //            }

            //            if (console.TUN)  // Adjust display scale and cursor for CW offset in TUN mode
            //            {
            //                if (rx1_dsp_mode == DSPMode.LSB || rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.DIGL)
            //                {
            //                    Low -= CWPitch;  // Lower<whatever> mode
            //                    High -= CWPitch;
            //                    f_diff += CWPitch;
            //                }
            //                else
            //                {
            //                    Low += CWPitch;  // All others
            //                    High += CWPitch;
            //                    f_diff -= CWPitch;
            //                }
            //            }

            //            if (console.VFOBTX)  //Tx on VFOB
            //            {
            //                ; // no correction necessary                                                  
            //            }
            //            else //Tx on VFOAsub
            //            {
            //                Low -= splitDiff;
            //                High -= splitDiff;
            //                f_diff += splitDiff;
            //            }
            //        }
            //    }
            //    else // Simplex operation, i.e. VFO controls RX and TX  
            //    {
            //        if (console.CTuneDisplay)
            //        {
            //            ; // no correction necessary here
            //        }

            //        if (console.TUN) // && display_duplex)  // Adjust display scale and cursor for CW offset in TUN mode (when in duplex only MW0LGE) TODO CHECK
            //        {
            //            if (rx1_dsp_mode == DSPMode.LSB || rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.DIGL)
            //            {
            //                Low -= CWPitch;  // Lower<whatever> mode
            //                High -= CWPitch;
            //                f_diff += CWPitch;
            //            }
            //            else
            //            {
            //                Low += CWPitch;  // All others
            //                High += CWPitch;
            //                f_diff -= CWPitch;
            //            }
            //        }
            //    }
            //}  // end display corrections for transmit mode combinations



            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            int filter_low, filter_high;
            int center_line_x;

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

            // Calculate horizontal step size
            while (width / freq_step_size > 10)
            {
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }

            //MW0LGE
            // calculate vertical step size
            int h_steps = (grid_max - grid_min) / grid_step;
            int top;

            if (bIsWaterfall) top = 20; //change top so that the filter gap doesnt change, inore grid spacing
            else top = (int)((double)grid_step * H / y_range); // top is based on grid spacing

            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            {
                if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX filter
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);

                    drawFilterOverlay(ref g, sub_rx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }

                if ((bIsWaterfall && m_bShowRXZeroLineOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX 0Hz line
                    int x = (int)((float)(vfoa_sub_hz - vfoa_hz - Low) / width * W);
                    //if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                    //{
                    //    g.DrawLine(sub_rx_zero_line_pen, x, H + top, x, H + H);
                    //}
                    //else
                    //{
                    //    g.DrawLine(sub_rx_zero_line_pen, x, top, x, H);
                    //}
                    g.DrawLine(sub_rx_zero_line_pen, x, nVerticalShift + top, x, nVerticalShift + H);
                }
            }

            if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (!local_mox)// && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    // draw RX filter
                    int filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);

                    drawFilterOverlay(ref g, display_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }
            }
            if ((bIsWaterfall && m_bShowTXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (local_mox && (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
                {
                    int filter_left_x = (int)((float)(filter_low - Low - f_diff + xit_hz) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low - f_diff + xit_hz) / width * W);

                    drawFilterOverlay(ref g, tx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }
            }

            if ((!bIsWaterfall || (bIsWaterfall && m_bShowTXFilterOnRXWaterfall)) && //MW0LGE
                !mox && draw_tx_filter &&
                (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            {
                // get tx filter limits
                int filter_left_x;
                int filter_right_x;

                if (tx_on_vfob)
                {
                    if (!split_enabled)
                    {
                        // MW0LGE - f_diff
                        filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
                    }
                    else
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
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

                if ((rx == 2 && tx_on_vfob) || (rx == 1 && !(tx_on_vfob && rx2_enabled)))
                {
                    g.DrawLine(tx_filter_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H);        // draw tx filter overlay
                    g.DrawLine(tx_filter_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H);	// draw tx filter overlay
                }
            }

            // draw 60m channels if in view - not on the waterfall //MW0LGE
            if (!bIsWaterfall && (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK))
            {
                foreach (Channel c in Console.Channels60m)
                {
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;
                    if (local_mox) rit = 0;

                    if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                    {
                        rf_freq = vfob_hz;
                    }

                    if (c.InBW((rf_freq + Low) * 1e-6, (rf_freq + High) * 1e-6)) // is channel visible?
                    {
                        bool on_channel = console.RX1IsIn60mChannel(c); // only true if you are on channel and are in an acceptable mode
                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) on_channel = console.RX2IsIn60mChannel(c);

                        DSPMode mode = rx1_dsp_mode;
                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) mode = rx2_dsp_mode;

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
                        rf_freq += cwSideToneShift;

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

                        //MW0LGE
                        drawChannelBar(g, c, chan_left_x, chan_right_x, nVerticalShift + top, H - top, c1, c2);
                    }
                }
            }

            // draw notches if in RX
            if (!local_mox && !bIsWaterfall)
            {
                long rf_freq = vfoa_hz;
                int rit = rit_hz;

                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                {
                    rf_freq = vfob_hz;
                }

                rf_freq += cwSideToneShift;
 
                Pen p;
                Brush b;
                Brush t;

                List<MNotch> notches = MNotchDB.NotchesInBW(rf_freq, Low, High);
                foreach (MNotch n in notches)
                {

                    int notch_centre_x = (int)((float)(n.FCenter - rf_freq - Low - rit) / width * W);
                    int notch_left_x = (int)((float)(n.FCenter - rf_freq - n.FWidth / 2 - Low - rit) / width * W);
                    int notch_right_x = (int)((float)(n.FCenter - rf_freq + n.FWidth / 2 - Low - rit) / width * W);

                    if (tnf_active)
                    {
                        if (n.Active)
                        {
                            p = m_pNotchActive;
                            b = m_bBWFillColour;
                        }
                        else
                        {
                            p = m_pNotchInactive;
                            b = m_bBWFillColourInactive;
                        }
                    }
                    else
                    {
                        p = m_pTNFInactive;
                        b = m_bTNFInactive;
                    }

                    //overide highlighed
                    if (n == m_objHightlightedNotch)
                    {
                        if (n.Active)
                        {
                            t = m_bTextCallOutActive;
                        }
                        else
                        {
                            t = m_bTextCallOutInactive;
                        }
                        p = m_pHighlighted;
                        b = m_bBWHighlighedFillColour;

                        //display text callout info 1/4 the way down the notch when being highlighted
                        //TODO: check right edge of screen, put on left of notch if no room
                        String temp_text = (n.FCenter / 1e6).ToString("f6") + "MHz";
                        int nTmp = temp_text.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) + 4;

                        g.DrawString("F: " + String.Copy(temp_text.Insert(nTmp, " ")), m_fntCallOutFont, t, notch_right_x + 4, nVerticalShift + top + (H / 4));
                        g.DrawString("W: " + n.FWidth.ToString("f0") + "Hz", m_fntCallOutFont, t, notch_right_x + 4, nVerticalShift + top + (H / 4) + 12);
                    }

                    // the middle notch line
                    g.DrawLine(p, notch_centre_x, nVerticalShift + top, notch_centre_x, nVerticalShift + H);

                    // only draw area fill if wide enough
                    if (notch_left_x != notch_right_x)
                    {
                        g.FillRectangle(b, notch_left_x, nVerticalShift + top, notch_right_x - notch_left_x, H - top);
                    }
                }
            }

            // Draw a CW Zero Beat + TX line on CW filter
            if (!bIsWaterfall)
            {
                if (show_cwzero_line)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx1_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        //if (!split_enabled)
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);
                        //else
                        //    cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        g.DrawLine(cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx2_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        //if (!split_enabled)
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);
                        //else
                        //    cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        g.DrawLine(cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H);
                    }
                }
                if (draw_tx_cw_freq)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx1_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        g.DrawLine(tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx2_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        g.DrawLine(tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H);
                    }
                }
            }

            //      MW0LGE
            if (local_mox)
                center_line_x = (int)((float)(-f_diff - Low + xit_hz) / width * W); // locked 0 line
            else
                center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line

            // Draw 0Hz vertical line if visible
            if ((!bIsWaterfall && show_zero_line) |
                (bIsWaterfall && ((m_bShowRXZeroLineOnWaterfall & !local_mox) || (m_bShowTXZeroLineOnWaterfall & local_mox)))) // MW0LGE
            {
                if (center_line_x >= 0 && center_line_x <= W)
                {
                    Pen pnPen = local_mox ? tx_grid_zero_pen : grid_zero_pen;

                    g.DrawLine(pnPen, center_line_x, nVerticalShift + top, center_line_x, nVerticalShift + H);
                }
            }

            if (show_freq_offset)
            {
                Brush brBrush = local_mox ? tx_grid_zero_pen.Brush : grid_zero_pen.Brush;

                g.DrawString("0", font9, brBrush, center_line_x - 5, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
            }

            //MW0LGE
            int[] band_edge_list;
            switch (console.CurrentRegion)
            {
                case FRSRegion.US:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5330500, 5406400, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Germany:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 51000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region1:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region2:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5351500, 5366500, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Region3:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 3900000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Spain:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 1850000, 3500000, 3800000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
            21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Australia:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000,
             3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000,
             18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.UK:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
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
                case FRSRegion.Japan:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1825000, 1907500, 1912500,
                        3500000, 3575000, 3599000, 3612000, 3680000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                default: // same as region3 but with extended 80m out to 4mhz
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
            }
            //--

            double vfo;

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

            //MW0LGE - TODO CHECK
            vfo += cwSideToneShift;
 
            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            int f_steps = (width / freq_step_size) + 1;

            // Draw vertical lines - band edge markers and freq text
            for (int i = -1; i < f_steps + 1; i++)  // MW0LGE was from i=0, fixes inbetweenies not drawn if major is < 0
            {
                string label;
                int offsetL;
                int offsetR;
                int fgrid = i * freq_step_size + (Low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - Low) / width * W);
                string freq_num = actual_fgrid.ToString();

                bool bBandEdge = false;

                if (!show_freq_offset)
                {
                    //--------------
                    //MW0LGE
                    for (int ii = 0; ii < band_edge_list.Length; ii++)
                    {
                        if (actual_fgrid == (double)band_edge_list[ii] / 1000000)
                        {
                            bBandEdge = true;
                            break;
                        }
                    }

                    Pen pnMajorLine;
                    Pen pnInbetweenLine;
                    Brush brTextBrush;

                    if (bBandEdge)
                    {
                        if (local_mox)
                        {
                            pnMajorLine = tx_band_edge_pen;
                            pnInbetweenLine = tx_vgrid_pen_inb;
                            brTextBrush = tx_band_edge_pen.Brush;
                        }
                        else
                        {
                            pnMajorLine = band_edge_pen;
                            pnInbetweenLine = grid_pen_inb;
                            brTextBrush = band_edge_pen.Brush;
                        }
                    }
                    else
                    {
                        if (local_mox)
                        {
                            pnMajorLine = tx_vgrid_pen;
                            pnInbetweenLine = tx_vgrid_pen_inb;
                            brTextBrush = grid_tx_text_brush;
                        }
                        else
                        {
                            pnMajorLine = grid_pen;
                            pnInbetweenLine = grid_pen_inb;
                            brTextBrush = grid_text_brush;
                        }
                    }
                    //--

                    //draw vertical in between lines
                    if (grid_control && !bIsWaterfall)
                    {
                        g.DrawLine(pnMajorLine, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);

                        int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                        int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                        float scale = (float)(x_2 - vgrid) / inbetweenies;

                        for (int j = 1; j < inbetweenies; j++)
                        {
                            float x3 = (float)vgrid + (j * scale);

                            g.DrawLine(pnInbetweenLine, x3, nVerticalShift + top, x3, nVerticalShift + H);
                        }
                    }

                    if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                    {
                        label = actual_fgrid.ToString("f3");
                        if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                        else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                    }
                    else
                    {
                        //display freqencies
                        //string temp_string;
                        int jper;
                        label = actual_fgrid.ToString("f4");
                        //temp_string = label;
                        jper = label.IndexOf('.') + 4;
                        label = label.Insert(jper, " ");

                        if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                        else offsetL = (int)((label.Length) * 4.1) - 8;
                    }

                    g.DrawString(label, font9, brTextBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    //--------------
                }
                else
                {
                    vgrid = Convert.ToInt32((double)-(fgrid - Low) / (Low - High) * W);	//wa6ahl

                    if (!bIsWaterfall)
                    {
                        Pen pnPen = local_mox ? tx_vgrid_pen : grid_pen;

                        g.DrawLine(pnPen, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);
                    }

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        Brush brBrush = local_mox ? grid_tx_text_brush : grid_text_brush;

                        g.DrawString(label, font9, brBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    }
                }
            }

            if (!bIsWaterfall)
            {
                // This block of code draws any band edge lines that might not be shown
                // because of the stepped nature of the code above

                //--------------
                //MW0LGE
                Pen pnMajorLine;

                if (local_mox)
                {
                    pnMajorLine = tx_band_edge_pen;
                }
                else
                {
                    pnMajorLine = band_edge_pen;
                }
                //--

                for (int i = 0; i < band_edge_list.Length; i++)
                {
                    double band_edge_offset = band_edge_list[i] - vfo;
                    if (band_edge_offset >= Low && band_edge_offset <= High)
                    {
                        int temp_vline = (int)((double)(band_edge_offset - Low) / width * W);//wa6ahl

                        //MW0LGE
                        g.DrawLine(pnMajorLine, temp_vline, nVerticalShift + top, temp_vline, nVerticalShift + H);
                    }
                }
            }
            //--

            if (grid_control && !bIsWaterfall)
            {
                for (int i = 1; i < h_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)((double)(grid_max - num) * H / y_range);

                    // MW0LGE
                    if (local_mox)
                    {
                        g.DrawLine(tx_hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);
                    }
                    else
                    {
                        g.DrawLine(hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);
                    }

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        num = grid_max - i * grid_step;
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)g.MeasureString("-", font9).Width - 2;
                        SizeF size = g.MeasureString(label, font9);

                        int x = 0;
                        switch (label_align)//MW0LGE display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                if (rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else x = center_line_x + xOffset;
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
                        if (rx == 1)
                        {
                            console.RX1DisplayGridX = x;
                            console.RX1DisplayGridW = (int)(x + size.Width);
                        }
                        else
                        {
                            console.RX2DisplayGridX = x;
                            console.RX2DisplayGridW = (int)(x + size.Width);
                        }
                        y -= 8;
                        if (y + 9 < H)
                        {
                            // MW0LGE
                            if (local_mox)
                            {
                                g.DrawString(label, font9, grid_tx_text_brush, x, nVerticalShift + y);
                            }
                            else
                            {
                                g.DrawString(label, font9, grid_text_brush, x, nVerticalShift + y);
                            }
                        }
                    }
                }
            }

            // draw long cursor & filter overlay
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                bool bShow = false;

                Pen p;
                // if we are sub tx then the cross will be red
                p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;

                int y1 = nVerticalShift + top;
                int y2 = H;

                if (rx == 1)
                {
                    if (rx2_enabled)
                    {
                        bShow = (current_display_mode == DisplayMode.PANAFALL) ? display_cursor_y <= 2 * H : display_cursor_y <= H;
                    }
                    else
                    {
                        bShow = true;
                    }
                }
                else
                {
                    bShow = (current_display_mode_bottom == DisplayMode.PANAFALL) ? display_cursor_y > 2 * H : display_cursor_y > H;
                }

                if (bShow)
                {
                    double freq_low = freq + filter_low;
                    double freq_high = freq + filter_high;
                    int x1 = (int)((freq_low - Low) / width * W);
                    int x2 = (int)((freq_high - Low) / width * W);

                    if (ClickTuneFilter) { // only show filter if option set MW0LGE
                        if (((rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU) && rx == 1) || ((rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU) && rx == 2))
                        {
                            g.FillRectangle(display_filter_brush, display_cursor_x -
                                ((x2 - x1) / 2), y1, x2 - x1, y2 - top);
                        }
                        else
                        {
                            g.FillRectangle(display_filter_brush, x1, y1, x2 - x1, y2 - top);
                        }
                    }

                    g.DrawLine(p, display_cursor_x, y1 - top, display_cursor_x, (y1 - top) + y2);

                    // draw horiz cursor line
                    if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                }
            }

            // MW0LGE all the code for F/G/H overlay line/grab boxes
            if (!bIsWaterfall && !local_mox)
            {
                //MW0LGE include bottom check
                if (console.PowerOn && (((current_display_mode == DisplayMode.PANADAPTER ||
                    current_display_mode == DisplayMode.PANAFALL ||
                    current_display_mode == DisplayMode.PANASCOPE) && rx == 1)
                    || ((current_display_mode_bottom == DisplayMode.PANADAPTER ||
                    current_display_mode_bottom == DisplayMode.PANAFALL ||
                    current_display_mode_bottom == DisplayMode.PANASCOPE) && rx == 2)))
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

                    if (rx == 1/* && !local_mox*/)
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
                        WDSP.GetRXAAGCThresh(WDSP.id(0, 0), &rx1_thresh, 4096.0, sample_rate);

                        rx1_thresh = Math.Round(rx1_thresh);

                        // get Hang Threshold level
                        double rx1_hang = 0.0;
                        float rx1_agc_hang_y = 0.0f;
                        WDSP.GetRXAAGCHangLevel(WDSP.id(0, 0), &rx1_hang);
                        int rx1_agc_fixed_gain = console.SetupForm.AGCFixedGain;
                        string rx1_agc = "";
                        switch (console.RX1AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx1_agcknee_y_value = dBToPixel(-(float)rx1_agc_fixed_gain + rx1_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx1_agc = "-F";
                                break;
                            default:
                                rx1_agcknee_y_value = dBToPixel((float)rx1_thresh + rx1_cal_offset, H);
                                rx1_agc_hang_y = dBToPixel((float)rx1_hang + rx1_cal_offset, H);

                                if (console.RX2Enabled && CurrentDisplayMode == DisplayMode.PANAFALL)
                                    rx1_agc_hang_y = rx1_agc_hang_y / 4;
                                else if (console.RX2Enabled || split_display)
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

                        rx1_agcknee_y_value += nVerticalShift;

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
                    else if (rx == 2/* && !local_mox*/)
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
                        WDSP.GetRXAAGCThresh(WDSP.id(2, 0), &rx2_thresh, 4096.0, sample_rate);
                        rx2_thresh = Math.Round(rx2_thresh);

                        WDSP.GetRXAAGCHangLevel(WDSP.id(2, 0), &rx2_hang);
                        rx2_agc_fixed_gain = console.SetupForm.AGCRX2FixedGain;

                        switch (console.RX2AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx2_agcknee_y_value = dBToRX2Pixel(-(float)rx2_agc_fixed_gain + rx2_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx2_agc = "-F";
                                break;
                            default:
                                rx2_agcknee_y_value = dBToRX2Pixel((float)rx2_thresh + rx2_cal_offset, H);
                                rx2_agc_hang_y = dBToRX2Pixel((float)rx2_hang + rx2_cal_offset, H);// + rx2_fft_size_offset);  MW0LGE   NOT IN RX1 WHY?  TODO CHECK

                                //if ((console.RX2Enabled || split_display) && current_display_mode_bottom != DisplayMode.PANAFALL)
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 2;
                                //else
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 4;

                                rx2_agc_hang_y += nVerticalShift;

                                if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
                                {
                                    AGCRX2Hang.Height = 8; AGCRX2Hang.Width = 8; AGCRX2Hang.X = 40;

                                    //MW0LGE
                                    //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + 2 * H) - AGCRX2Hang.Height;
                                    //else
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + H) - AGCRX2Hang.Height;
                                    AGCRX2Hang.Y = (int)rx2_agc_hang_y - AGCRX2Hang.Height;

                                    g.FillRectangle(Brushes.Yellow, AGCRX2Hang);
                                    using (Pen p = new Pen(Color.Yellow))
                                    {
                                        p.DashStyle = DashStyle.Dot;

                                        //MW0LGE 06/02/20 fix no draw of lines on RX2
                                        //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                        g.DrawLine(p, x3_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, x2_rx2_hang, rx2_agc_hang_y/* + 2 * H*/);
                                        //else
                                        //    g.DrawLine(p, x3_rx2_hang, rx2_agc_hang_y + H, x2_rx2_hang, rx2_agc_hang_y + H);

                                        g.DrawString("-H", pana_font, pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - (AGCRX2Hang.Height / 2));
                                    }
                                }
                                rx2_agc = "-G";
                                break;
                        }

                        //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 4;
                        //else
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 2;

                        if (display_rx2_gain_line)
                        {
                            rx2_agcknee_y_value += nVerticalShift;

                            AGCRX2Knee.Height = 8; AGCRX2Knee.Width = 8; AGCRX2Knee.X = 40;

                            //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + 2 * H) - AGCRX2Knee.Height;
                            //else
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + H) - AGCRX2Knee.Height;
                            AGCRX2Knee.Y = (int)rx2_agcknee_y_value - AGCRX2Knee.Height;

                            g.FillRectangle(Brushes.YellowGreen, AGCRX2Knee);
                            using (Pen p = new Pen(Color.YellowGreen))
                            {
                                p.DashStyle = DashStyle.Dot;

                                //MW0LGE 06/02/20 fix no draw of lines on RX2
                                //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                g.DrawLine(p, x1_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, x2_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/);
                                //else
                                    //g.DrawLine(p, x1_rx2_gain, rx2_agcknee_y_value + H, x2_rx2_gain, rx2_agcknee_y_value + H);

                                g.DrawString(rx2_agc, pana_font, pana_text_brush, AGCRX2Knee.X + AGCRX2Knee.Width, AGCRX2Knee.Y - (AGCRX2Knee.Height / 2));
                            }
                        }
                    }
                }
            }

            // ke9ns add draw DX SPOTS on pandapter
            //=====================================================================
            //=====================================================================

            if (!bIsWaterfall && SpotControl.SP_Active != 0)
            {

                int iii = 0;                          // ke9ns add stairstep holder

                int kk = 0;                           // ke9ns add index for holder[] after you draw the vert line, then draw calls (so calls can overlap the vert lines)

                int vfo_hz = (int)vfoa_hz;    // vfo freq in hz

                int H1a = H / 2;            // length of vertical line (based on rx1 and rx2 display window configuration)
                int H1b = 20;               // starting point of vertical line

                int rxDisplayLow = RXDisplayLow;
                int rxDisplayHigh = RXDisplayHigh;
                SizeF length;

                // RX1/RX2 PanF/Pan = 5,2 (K9,K10)(short)  PanF/PanF = 5,5, (short) Pan/Pan 2,2 (long)
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))                 // if your drawing to the bottom 
                {
                    //if ((K9 == 2) && (K10 == 2)) H1a = H + (H / 2); // long
                    //else H1a = H + (H / 4); // short

                    //H1b = H + 20;

                    vfo_hz = (int)vfob_hz;
                    rxDisplayLow = RX2DisplayLow;
                    rxDisplayHigh = RX2DisplayHigh;

                    Console.DXK2 = 0;        // RX2 index to allow call signs to draw after all the vert lines on the screen
                }
                else
                {
                    Console.DXK = 0;        // RX1 index to allow call signs to draw after all the vert lines on the screen
                }

                VFOLow = vfo_hz + rxDisplayLow;    // low freq (left side) in hz
                VFOHigh = vfo_hz + rxDisplayHigh; // high freq (right side) in hz
                VFODiff = VFOHigh - VFOLow;       // diff in hz

                if ((vfo_hz < 5000000) || ((vfo_hz > 6000000) && (vfo_hz < 8000000))) m_bLSB = true; // LSB
                else m_bLSB = false;     // usb

                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------
                // draw DX spots
                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------

                for (int ii = 0; ii < SpotControl.DX_Index; ii++)     // Index through entire DXspot to find what is on this panadapter (draw vert lines first)
                {

                    if ((SpotControl.DX_Freq[ii] >= VFOLow) && (SpotControl.DX_Freq[ii] <= VFOHigh))
                    {
                        int VFO_DXPos = (int)((((float)W / (float)VFODiff) * (float)(SpotControl.DX_Freq[ii] + cwSideToneShiftInverted - VFOLow))); // determine DX spot line pos on current panadapter screen

                        holder[kk] = ii;                    // ii is the actual DX_INdex pos the the KK holds
                        holder1[kk] = VFO_DXPos;

                        kk++;

                        g.DrawLine(p1, VFO_DXPos, H1b + nVerticalShift, VFO_DXPos, H1a + nVerticalShift);   // draw vertical line

                    }

                } // for loop through DX_Index


                int bb = 0;
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
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
                    if (m_bLSB) // 1=LSB so draw on left side of line
                    {

                        if (Console.DisplaySpot) // display Spotted on Pan
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

                            g.DrawString(SpotControl.DX_Station[holder[ii]], font1, grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);
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

                            g.DrawString(SpotControl.DX_Spotter[holder[ii]], font1, grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50; // allow only 50 qrz spots per Receiver
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
                        if (Console.DisplaySpot) // spot
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

                            g.DrawString(SpotControl.DX_Station[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name
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

                            g.DrawString(SpotControl.DX_Spotter[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50;
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
                            g.DrawString(SpotControl.DX_Grid[holder[ii]], font1, grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX grid name
                        }

                    } // USB side

                    iii = iii + 11;
                    if (iii > 90) iii = 0;


                }// for loop through DX_Index

            }
        }

        private static float dBToPixel(float dB, int H)
        {
            return (float)(spectrum_grid_max - dB) * H / (spectrum_grid_max - spectrum_grid_min);
        }

        private static float dBToRX2Pixel(float dB, int H)
        {
            return (float)(rx2_spectrum_grid_max - dB) * H / (rx2_spectrum_grid_max - rx2_spectrum_grid_min);
        }

        private static float PixelToDb(float y, int H)
        {
            if (y <= H / 2) y *= 2.0f;
            else y = (y - H / 2) * 2.0f;

            return (float)(spectrum_grid_max - y * (double)(spectrum_grid_max - spectrum_grid_min) / H);
        }

        //private static void DrawWaterfallGrid(ref Graphics g, int W, int H, int rx, bool bottom)
        //{
        //    drawPanadapterAndWaterfallGrid(ref g, W, H, rx, bottom, true); // true  =  drawing waterfall grid

        //    // all this semi duplicated code ditched,  now use common function passing true/false if you want waterfall or pana grid
        //}

        private static void DrawOffBackground(Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);
        }

        private static float[] scope_min = new float[displayTargetWidth];
        public static float[] ScopeMin {
            get { return scope_min; }
            set { scope_min = value; }
        }
        private static float[] scope_max = new float[displayTargetWidth];
        public static float[] ScopeMax {
            get { return scope_max; }
            set { scope_max = value; }
        }

        private static Point[] points;
        private static Point[] pointsStore1;
        private static Point[] pointsStore2;
        private static int lastResize = -999; // -999 startup state, ensures arrays will be generated

        private static void updateSharePointsArray(int nW)
        {
            // purpose of function is to limit the number of times the arrays are built, to reduce GC work
            if (nW == lastResize)
            {
                points = pointsStore1;
            }
            else if (nW == lastResize + 2)
            {
                points = pointsStore2;
            }
            else
            {
                pointsStore1 = new Point[nW];
                pointsStore2 = new Point[nW + 2];
                points = pointsStore1;
                lastResize = nW;
            }
        }

        unsafe private static bool DrawScope(Graphics g, int W, int H, bool bottom)
        {
            // MW0LGE
            // this will never work correctly, as drawlines does not know where the end of the array is
            // if you extend the width, then reduce it, the array will have old junk in there
            // that drawlines will happily go and draw !
            //if (scope_min.Length < W)
            //{
            //    scope_min = new float[W];
            //    Audio.ScopeMin = scope_min;
            //}
            //if (scope_max.Length < W)
            //{
            //    scope_max = new float[W];
            //    Audio.ScopeMax = scope_max;
            //}
            if (scope_min.Length != W)
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }
            if (scope_max.Length != W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }

            DrawScopeGrid(ref g, W, H, bottom);

            updateSharePointsArray(W * 2);

            for (int i = 0; i < W; i++)						// fill point array
            {
                int pixel = 0;
                /*if (bottom) pixel = (int)(H / 2 * scope_max[i]);
                else*/
                pixel = (int)(H / 2 * scope_max[i]);
                int y = H / 2 - pixel;
                points[i].X = i;
                points[i].Y = y;
                if (bottom) points[i].Y += H;

                /*if (bottom) pixel = (int)(H / 2 * scope_min[i]);
                else*/
                pixel = (int)(H / 2 * scope_min[i]);
                y = H / 2 - pixel;
                points[W * 2 - 1 - i].X = i;
                points[W * 2 - 1 - i].Y = y;
                if (bottom) points[W * 2 - 1 - i].Y += H;
            }

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

        private static float[] scope2_min = new float[displayTargetWidth];
        public static float[] Scope2Min {
            get { return scope2_min; }
            set { scope2_min = value; }
        }
        private static float[] scope2_max = new float[displayTargetWidth];
        public static float[] Scope2Max {
            get { return scope2_max; }
            set { scope2_max = value; }
        }

        unsafe private static bool DrawScope2(Graphics g, int W, int H, bool bottom)
        {
            if (scope_min.Length != W)  // could leave < W here, but bring in line with DrawScope MW0LGE
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }
            if (scope_max.Length != W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }
            if (scope2_min.Length != W)
            {
                scope2_min = new float[W];
                Audio.Scope2Min = scope2_min;
            }
            if (scope2_max.Length != W)
            {
                scope2_max = new float[W];
                Audio.Scope2Max = scope2_max;
            }

            updateSharePointsArray(W);

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

            int samples = W;// scope2_max.Length;   //MW0LGE blimey
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
                g.DrawLines(waveform_line_pen, points);
                // draw the right input samples
                points[0].X = 0;
                points[0].Y = (int)(y3 - (scope_max[0] * yScale));
                for (int x = 0; x < W; x++)
                {
                    int i = (int)Math.Truncate((float)x * xScale);
                    int y = (int)(y3 - (scope_max[i] * yScale));
                    points[x].X = x;
                    points[x].Y = y;
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
                    Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                data_ready_bottom = false;
            }

            updateSharePointsArray(num_points);

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

            int nShift = m_nPhasePointSize / 2;

            // draw each point
            for (int i = 0; i < num_points; i++)
                g.DrawRectangle(data_line_pen, points[i].X - nShift, points[i].Y - nShift, m_nPhasePointSize, m_nPhasePointSize);

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
                    Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                data_ready_bottom = false;
            }

            updateSharePointsArray(num_points);

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

            int nShift = m_nPhasePointSize / 2;

            // draw each point
            for (int i = 0; i < num_points; i++)
                g.DrawRectangle(data_line_pen, points[i].X - nShift, points[i].Y - nShift, m_nPhasePointSize, m_nPhasePointSize);

            // draw long cursor
            if (current_click_tune_mode == ClickTuneMode.Off) return;
            Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;
            if (bottom) g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
            else g.DrawLine(p, display_cursor_x, H, display_cursor_x, H + H);
            g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
        }

        //private static Point[] points;
        unsafe static private bool DrawSpectrum(Graphics g, int W, int H, bool bottom)
        {
            DrawSpectrumGrid(ref g, W, H, bottom);

            updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = float.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
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
                    for (int i = 0; i < W/*current_display_data.Length*/; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                }
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                }
                data_ready_bottom = false;
            }

            for (int i = 0; i < W; i++)
            {
                float max = float.MinValue;
                if (!bottom)
                {
                    max = current_display_data[i];
                }
                else
                {
                    max = current_display_data_bottom[i];
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (!bottom) max += rx1_display_cal_offset;
                    else max += rx2_display_cal_offset;
                }
                else
                {
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

        unsafe static private bool DrawPanadapter(Graphics g, int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            if (grid_control)
            {
                clearBackground(ref g, rx, W, H, bottom);
                drawPanadapterAndWaterfallGrid(ref g, nVerticalShift, W, H, rx, bottom, false);
            }

            if (pan_fill)
            {
                updateSharePointsArray(W + 2);
            }
            else
            {
                updateSharePointsArray(W);
            }

            float local_max_y = float.MinValue;
            bool local_mox = false;
            bool displayduplex = isRxDuplex(rx);

            int grid_max = 0;
            int grid_min = 0;

            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;
            if (rx == 1 && tx_on_vfob && mox && !console.RX2Enabled) local_mox = true;

            //MW0LGE
            int yRange;
            float[] data;

            if (rx == 1)
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready)
                {
                    if (!displayduplex && (local_mox || (mox && tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_display_data.Length*/; i++)
                            current_display_data[i] = grid_min - rx1_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data[0])
                        fixed (void* wptr = &current_display_data[0])
                            Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                    }
                    data_ready = false;
                }
                data = current_display_data;
            }
            else// if(rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready_bottom)
                {

                    //MW0LGE//if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    if (blank_bottom_display || (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
                    {
                        for (int i = 0; i < W/*current_display_data_bottom.Length*/; i++)
                            current_display_data_bottom[i] = grid_min - rx2_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data_bottom[0])
                        fixed (void* wptr = &current_display_data_bottom[0])
                            Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                    }

                    data_ready_bottom = false;
                }
                data = current_display_data_bottom;
            }

            //try
            //{
            float max;
            //MW0LGE block of code pulled from loop, += in there for now - TODO
            float fOffset = 0;
            int nOffset2 = 0;

            if (rx == 1)
            {
                if (local_mox) fOffset += tx_display_cal_offset;
                else if (mox && tx_on_vfob && !displayduplex)
                {
                    if (console.RX2Enabled) fOffset += rx1_display_cal_offset;
                    else fOffset += tx_display_cal_offset;
                }
                else fOffset += rx1_display_cal_offset;
            }
            else if (rx == 2)
            {
                if (local_mox) fOffset += tx_display_cal_offset;
                else fOffset += rx2_display_cal_offset;
            }

            if (!local_mox || (local_mox && displayduplex))
            {
                if (rx == 1) fOffset += rx1_preamp_offset;
                else if (rx == 2) fOffset += rx2_preamp_offset;
            }

            if (bottom) nOffset2 = H;
            else if (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2) nOffset2 = 2 * H;  //MW0LGE

            int filter_left_x = 0, filter_right_x = 0;
            if (m_bPeakBlobMaximums)
            {
                resetMaximums(rx);
                if (m_bInsideFilterOnly) getFilterXPositions(rx, W, local_mox, displayduplex, out filter_left_x, out filter_right_x);
            }

            //---
            for (int i = 0; i < W; i++)
            {
                max = data[i] + fOffset;

                if (max > local_max_y)
                {
                    local_max_y = max;
                    max_x = i;
                }

                points[i].X = i;
                points[i].Y = (int)(Math.Floor((grid_max - max) * H / yRange));
                points[i].Y = Math.Min(points[i].Y, H);

                points[i].Y += nOffset2;

                if (m_bPeakBlobMaximums)
                {
                    if (m_bInsideFilterOnly)
                    {
                        bool bInsideFilter = (i >= filter_left_x && i <= filter_right_x);
                        if (bInsideFilter) processMaximums(rx, max, i, points[i].Y);
                    }
                    else
                    {
                        processMaximums(rx, max, i, points[i].Y);
                    }
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    Trace.WriteLine(ex);
            //}

            if (!bottom) max_y = local_max_y;

            //try
            //{
            if (pan_fill)
            {
                points[W].X = W; points[W].Y = H;
                points[W + 1].X = 0; points[W + 1].Y = H;
                if (bottom)
                {
                    points[W].Y += H;
                    points[W + 1].Y += H;
                }
                else if (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)                     //MW0LGE - RX2
                {
                    points[W].Y += 2 * H;
                    points[W + 1].Y += 2 * H;
                }

                if (local_mox) g.FillPolygon(tx_data_line_fpen.Brush, points);
                else g.FillPolygon(data_fill_fpen.Brush, points);//MW0LGE

                points[W] = points[W - 1];
                points[W + 1] = points[W - 1];
                //data_line_pen.Color = data_line_color;
                if (local_mox) g.DrawLines(tx_data_line_pen, points);
                else g.DrawLines(data_line_pen, points);
            }
            else
            {
                if (local_mox) g.DrawLines(tx_data_line_pen, points);
                else g.DrawLines(data_line_pen, points);
            }

            // peak blobs
            if (m_bPeakBlobMaximums)
            {
                Maximums[] maximums;
                if (rx == 1)
                    maximums = m_nRX1Maximums;
                else
                    maximums = m_nRX2Maximums;

                for (int n = 0; n < m_nNumberOfMaximums; n++)
                {
                    if (maximums[n].Enabled)
                    {
                        if (m_bBlobPeakHold && m_bBlobPeakHoldFade)
                        {
                            double fTmp = (double)(m_objPeakHoldTimer.ElapsedMsec - maximums[n].Time) / m_fBlobPeakHoldMS;
                            fTmp = fTmp > 1 ? 1f : fTmp;

                            fTmp *= (Math.PI / 2);
                            fTmp = Math.Cos(fTmp);

                            peak_blob_pen.Color = Color.FromArgb((int)(fTmp * 255), peak_blob_pen.Color.R, peak_blob_pen.Color.G, peak_blob_pen.Color.B);
                            peak_blob_text_pen.Color = Color.FromArgb((int)(fTmp * 255), peak_blob_text_pen.Color.R, peak_blob_text_pen.Color.G, peak_blob_text_pen.Color.B);
                        }
                        else
                        {
                            peak_blob_pen.Color = Color.FromArgb(peak_blob_pen.Color.R, peak_blob_pen.Color.G, peak_blob_pen.Color.B);
                            peak_blob_text_pen.Color = Color.FromArgb(peak_blob_text_pen.Color.R, peak_blob_text_pen.Color.G, peak_blob_text_pen.Color.B);
                        }
                        g.DrawEllipse(peak_blob_pen, maximums[n].X - 5, maximums[n].MaxY_pixel - 5, 10, 10);
                        g.DrawString(maximums[n].MaxY.ToString("f1") + " (" + (n + 1).ToString() + ")", m_fntCallOutFont, peak_blob_text_pen.Brush, maximums[n].X + 6, maximums[n].MaxY_pixel - 8);
                    }
                }
            }

            return true;
        }

        //MW0LGE - these properties auto AGC on the waterfall, so that
        //spectrum/grid based max/mins can be used without getting changed by agc
        private static bool m_bRX1_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX1 {
            get { return m_bRX1_spectrum_thresholds; }
            set { m_bRX1_spectrum_thresholds = value; }
        }
        private static bool m_bRX2_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX2 {
            get { return m_bRX2_spectrum_thresholds; }
            set { m_bRX2_spectrum_thresholds = value; }
        }
        //--

        private static bool rx1_waterfall_agc = false;
        public static bool RX1WaterfallAGC {
            get { return rx1_waterfall_agc; }
            set { rx1_waterfall_agc = value; }
        }

        private static bool rx2_waterfall_agc = false;
        public static bool RX2WaterfallAGC {
            get { return rx2_waterfall_agc; }
            set { rx2_waterfall_agc = value; }
        }

        private static int waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int WaterfallUpdatePeriod {
            get { return waterfall_update_period; }
            set { waterfall_update_period = value; }
        }

        private static int rx2_waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int RX2WaterfallUpdatePeriod {
            get { return rx2_waterfall_update_period; }
            set { rx2_waterfall_update_period = value; }
        }

        //private static readonly HiPerfTimer timer_waterfall = new HiPerfTimer();
        //private static readonly HiPerfTimer timer_waterfall2 = new HiPerfTimer();
        private static float RX1waterfallPreviousMinValue = 0.0f;
        private static float RX2waterfallPreviousMinValue = 0.0f;

        unsafe static private bool DrawWaterfall(Graphics g, int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            //if (grid_control) clearBackground(ref g, rx, W, H, bottom);

            // grid draw now moved to end, so that everything can get put
            // on top of waterfall
            //if (grid_control) DrawWaterfallGrid(ref g, W, H, rx, bottom);

            if (waterfall_data == null || waterfall_data.Length < W)
            {
                waterfall_data = new float[W];		// array of points to display
            }
            float local_max_y = float.MinValue;
            bool local_mox = false;
            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;
            float local_min_y_w3sz = float.MaxValue;
            float display_min_w3sz = float.MaxValue;
            float display_max_w3sz = float.MinValue;
            float min_y_w3sz = float.MaxValue;
            int R = 0, G = 0, B = 0;
            int grid_max = 0;
            int grid_min = 0;
            bool displayduplex = isRxDuplex(rx);
            float low_threshold = 0.0f;
            float high_threshold = 0.0f;
            float waterfall_minimum = 0.0f;
            //float rx1_waterfall_minimum = 0.0f;
            //float rx2_waterfall_minimum = 0.0f;
            ColorSheme cSheme = ColorSheme.enhanced;
            Color low_color = Color.Black;
            Color mid_color = Color.Red;
            Color high_color = Color.Blue;
            int sample_rate = 0;//MW0LGE

            if (rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    //waterfall_minimum = rx2_waterfall_minimum; // check
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    sample_rate = sample_rate_rx2;
                    high_threshold = rx2_waterfall_high_threshold;
                    if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
                    {
                        //waterfall_minimum = rx2_waterfall_minimum;
                        low_threshold = RX2waterfallPreviousMinValue;
                    }
                    else low_threshold = rx2_waterfall_low_threshold;
                }
                cSheme = rx2_color_sheme;
                low_color = rx2_waterfall_low_color;
                mid_color = rx2_waterfall_mid_color;
                high_color = rx2_waterfall_high_color;
            }
            else
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    //waterfall_minimum = rx1_waterfall_minimum; // check
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    sample_rate = sample_rate_rx1;
                    low_threshold = RX1waterfallPreviousMinValue;
                    high_threshold = waterfall_high_threshold;
                    if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
                    {
                        //waterfall_minimum = rx1_waterfall_minimum;
                        low_threshold = RX1waterfallPreviousMinValue;
                    }
                    else low_threshold = waterfall_low_threshold;
                }
                cSheme = color_sheme;
                low_color = waterfall_low_color;
                mid_color = waterfall_mid_color;
                high_color = waterfall_high_color;
            }

            if (console.PowerOn)
            {
                if (rx == 1 && waterfall_data_ready)
                {
                    if (!displayduplex && local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_waterfall_data.Length*/; i++)
                            current_waterfall_data[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data[0])
                        fixed (void* wptr = &current_waterfall_data[0])
                            Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));

                    }
                    waterfall_data_ready = false;
                }
                else if (rx == 2 && waterfall_data_ready_bottom)
                {
                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_waterfall_data_bottom.Length*/; i++)
                            current_waterfall_data_bottom[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data_bottom[0])
                        fixed (void* wptr = &current_waterfall_data_bottom[0])
                            Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));
                    }

                    waterfall_data_ready_bottom = false;
                }

                //int duration = 0;
                //if (rx == 1)
                //{
                //    timer_waterfall.Stop();
                //    duration = (int)timer_waterfall.DurationMsec;
                //}
                //else if (rx == 2)
                //{
                //    timer_waterfall2.Stop();
                //    duration = (int)timer_waterfall2.DurationMsec;
                //}

                bool bRXdraw = false;
                if (rx == 1)
                {
                    m_nRX1WaterFallFrameCount++;
                    if (m_nRX1WaterFallFrameCount >= waterfall_update_period)
                    {
                        m_nRX1WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }
                else
                {
                    m_nRX2WaterFallFrameCount++;
                    if (m_nRX2WaterFallFrameCount >= rx2_waterfall_update_period)
                    {
                        m_nRX2WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }

                //if ((rx == 1 && (duration > waterfall_update_period || duration < 0)) ||
                //    (rx == 2 && (duration > rx2_waterfall_update_period || duration < 0)))
                if(bRXdraw)
                {
                    //MW0LGE
                    float[] data;

                    if (rx == 1) {
                        data = current_waterfall_data;
                        //timer_waterfall.Start();
                    }
                    else/* if (rx == 2)*/ {
                        data = current_waterfall_data_bottom;
                        //timer_waterfall2.Start();
                    }

                    float max;
                    float fOffset = 0; ///MW0LGE - block of code moved out of for loop +- for now, TODO

                    if (!local_mox)
                    {
                        if (rx == 1) fOffset += rx1_display_cal_offset;
                        else if (rx == 2) fOffset += rx2_display_cal_offset;
                    }
                    else
                    {
                        if (rx == 1) fOffset += rx1_display_cal_offset;
                        else if (rx == 2) fOffset += tx_display_cal_offset;
                    }

                    if (!local_mox)
                    {
                        if (rx == 1) fOffset += (rx1_preamp_offset - alex_preamp_offset);
                        else if (rx == 2) fOffset += (rx2_preamp_offset);
                    }
                    else
                    {
                        if (rx == 1) fOffset += (rx1_preamp_offset - alex_preamp_offset);
                    }

                    for (int i = 0; i < W; i++)
                    {
                        max = data[i] + fOffset; //MW0LGE

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

                    //MW0LGE
                    if (rx == 1)
                    {
                        bitmapData = waterfall_bmp.LockBits(
                            new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height),
                            ImageLockMode.ReadWrite,
                            waterfall_bmp.PixelFormat);
                    }
                    else //rx2
                    {
                        bitmapData = waterfall_bmp2.LockBits(
                            new Rectangle(0, 0, waterfall_bmp2.Width, waterfall_bmp2.Height),
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

                    switch (cSheme)
                    {
                        case (ColorSheme.original):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
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

                                    if (waterfall_minimum > waterfall_data[i])
                                        waterfall_minimum = waterfall_data[i];

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
                                    display_min_w3sz = min_y_w3sz - 5; //for histogram equilization
                                    display_max_w3sz = max_y; //for histogram equalization

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

                if (rx == 1)
                {
                    g.DrawImageUnscaled(waterfall_bmp, 0, nVerticalShift + 20);
                }
                else
                {
                    g.DrawImageUnscaled(waterfall_bmp2, 0, nVerticalShift + 20);
                }
                //MW0LGE
                //if (bottom)
                //{
                //    if (rx == 1) g.DrawImageUnscaled(waterfall_bmp, 0, H + 20);
                //    else if (rx == 2)
                //    {
                //        switch (current_display_mode_bottom)
                //        {
                //            case DisplayMode.PANAFALL:
                //                g.DrawImageUnscaled(waterfall_bmp2, 0, 3 * H + 20);
                //                break;
                //            default:
                //                g.DrawImageUnscaled(waterfall_bmp2, 0, H + 20);
                //                break;
                //        }
                //    }
                //}
                //else
                //{
                //    if (rx == 1) g.DrawImageUnscaled(waterfall_bmp, 0, 20); // draw the image on the background	
                //    else if (rx == 2) g.DrawImageUnscaled(waterfall_bmp2, 0, 20);   // draw the image on the background	
                //}
                //-

            }

            // MW0LGE now draw any grid/labels/scales over the top of waterfall
            if (grid_control) drawPanadapterAndWaterfallGrid(ref g, nVerticalShift, W, H, rx, bottom, true);

            return true;
        }

        unsafe static private bool DrawHistogram(Graphics g, int W, int H)
        {
            DrawSpectrumGrid(ref g, W, H, false);

            updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = Int32.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int yRange = grid_max - grid_min;
            if (data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, W/*BUFFER_SIZE*/ * sizeof(float));

                data_ready = false;
            }

            int sum = 0;

            for (int i = 0; i < W; i++)
            {
                float max = max = current_display_data[i];

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
                points[i].Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);

                sum += points[i].Y;
            }

            max_y = local_max_y;

            // get the average
            float avg = 0.0F;

            //foreach (Point p in points)
            //    sum += p.Y;

            avg = (float)((float)sum / W / 1.12);

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
                    g.DrawRectangle(dhp, i, histogram_data[i], 1, height);
                }
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

        public static void ResetWaterfallBmp(/*int scale*/)
        {
            //if (!bIsPanafall)
            //{
            //    if (Display.RX2Enabled)
            //        Display.ResetWaterfallBmp(2);
            //    else
            //        Display.ResetWaterfallBmp(1);
            //}
            //else
            //{
            //    if (Display.RX2Enabled)
            //        Display.ResetWaterfallBmp(4);
            //    else
            //        Display.ResetWaterfallBmp(2);
            //}

            int H = displayTargetHeight;
            if (current_display_mode == DisplayMode.PANAFALL) H /= 2;
            if (rx2_enabled) H /= 2;

            //override for splitter pos, when only one rx and it is panafall
            if (!rx2_enabled && current_display_mode == DisplayMode.PANAFALL) H = displayTargetHeight - PanafallSplitBarPos;

            if (current_display_engine == DisplayEngine.GDI_PLUS)
            {
                if (waterfall_bmp != null) waterfall_bmp.Dispose();
                waterfall_bmp = new Bitmap(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20, PixelFormat.Format24bppRgb);
            }
            else
            {
                lock (m_objDX2Lock)
                {
                    SharpDX.Direct2D1.Bitmap tmp = null;
                    if (waterfall_bmp_dx2d != null && !waterfall_bmp_dx2d.IsDisposed)
                    {
                        if (displayTargetWidth == waterfall_bmp_dx2d.Size.Width)
                        {
                            // make copy only if widths equal
                            tmp = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2((int)waterfall_bmp_dx2d.Size.Width, (int)waterfall_bmp_dx2d.Size.Height),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                            tmp.CopyFromBitmap(waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (waterfall_bmp_dx2d != null) waterfall_bmp_dx2d.Dispose();
                    waterfall_bmp_dx2d = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20), new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                    if (tmp != null)
                    {
                        // copy old waterfall into new bitmap
                        waterfall_bmp_dx2d.CopyFromBitmap(tmp, new SharpDX.Point(0, 0)); // anything outside will be 'ignored'
                        tmp.Dispose();
                    }
                }
            }
        }

        public static void ResetWaterfallBmp2(/*int scale*/)
        {
            int H = displayTargetHeight;
            if (current_display_mode_bottom == DisplayMode.PANAFALL) H /= 2;
            H /= 2; // it will always be

            if (current_display_engine == DisplayEngine.GDI_PLUS)
            {
                if (waterfall_bmp2 != null) waterfall_bmp2.Dispose();
                waterfall_bmp2 = new Bitmap(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20, PixelFormat.Format24bppRgb);
            }
            else
            {
                lock (m_objDX2Lock)
                {
                    SharpDX.Direct2D1.Bitmap tmp = null;
                    if (waterfall_bmp2_dx2d != null && !waterfall_bmp2_dx2d.IsDisposed)
                    {
                        if (displayTargetWidth == waterfall_bmp2_dx2d.Size.Width)
                        {
                            // make copy only if widths equal
                            tmp = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2((int)waterfall_bmp2_dx2d.Size.Width, (int)waterfall_bmp2_dx2d.Size.Height),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                            tmp.CopyFromBitmap(waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (waterfall_bmp2_dx2d != null) waterfall_bmp2_dx2d.Dispose();
                    waterfall_bmp2_dx2d = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20), new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                    if (tmp != null)
                    {
                        // copy old waterfall into new bitmap
                        waterfall_bmp2_dx2d.CopyFromBitmap(tmp, new SharpDX.Point(0, 0)); // anything outside will be 'ignored'
                        tmp.Dispose();
                    }
                }
            }
        }

        #endregion

        #endregion
        #endregion


        // directx mw0lge

        private static bool m_bHighlightNumberScaleRX1 = false;
        private static bool m_bHighlightNumberScaleRX2 = false;
        public static bool HighlightNumberScaleRX1 {
            get { return m_bHighlightNumberScaleRX1; }
            set {
                m_bHighlightNumberScaleRX1 = value;
            }
        }
        public static bool HighlightNumberScaleRX2 {
            get { return m_bHighlightNumberScaleRX2; }
            set {
                m_bHighlightNumberScaleRX2 = value;
            }
        }
        private static bool m_bDX2Setup = false;
        private static Surface surface;
        private static Device device;
        private static SwapChain swapChain;
        private static RenderTargetView renderView;
        private static Texture2D backBuffer;
        private static RenderTarget d2dRenderTarget;
        private static SharpDX.Direct2D1.Factory d2dFactory;
        private static Object m_objDX2Lock = new Object();
        public static void ShutdownDX2D()
        {
            lock (m_objDX2Lock)
            {
                if (!m_bDX2Setup) return;

                waterfall_bmp_dx2d.Dispose();
                waterfall_bmp2_dx2d.Dispose();

                d2dRenderTarget.Dispose();
                surface.Dispose();
                backBuffer.Dispose();
                renderView.Dispose();
                d2dFactory.Dispose();

                m_bDX2Setup = false;
            }
        }
        public static void InitDX2D()
        {
            lock (m_objDX2Lock)
            {
                if (m_bDX2Setup) return;

                try
                {
                    SwapChainDescription desc = new SwapChainDescription()
                    {
                        BufferCount = 1,
                        ModeDescription =
                                           new ModeDescription(displayTargetWidth, displayTargetHeight,
                                                               new Rational(/*console.MAX_FPS*/console.DisplayFPS, 1), Format.B8G8R8A8_UNorm),
                        IsWindowed = true,
                        OutputHandle = displayTarget.Handle,
                        SampleDescription = new SampleDescription(1, 0), // no multi sampling (1 sample), no antialiasing
                        SwapEffect = SwapEffect.Discard,
                        Usage = Usage.RenderTargetOutput,
                        Flags = SwapChainFlags.None
                    };
                    
                    Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out device, out swapChain);

                    //int maxQuality = device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, 2); // 2 = MSAA_2, 2 times multisampling

                    //SharpDX.DXGI.Device1 dd = device.QueryInterface<SharpDX.DXGI.Device1>();
                    //dd.MaximumFrameLatency = 1;

                    d2dFactory = new SharpDX.Direct2D1.Factory();
                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
                    renderView = new RenderTargetView(device, backBuffer);
                    surface = backBuffer.QueryInterface<Surface>();
                    d2dRenderTarget = new RenderTarget(d2dFactory, surface,
                                                                    new RenderTargetProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                    
                    ResetWaterfallBmp();
                    ResetWaterfallBmp2();

                    m_bDX2Setup = true;

                    buildDX2Resources();
                    buildFontsDX2D();
                    SetDX2BackgoundImage(console.PicDisplayBackgroundImage);
                }
                catch (Exception e)
                {
                    // issue setting up dx
                    // msg box and switch engine back to GDI+ ?
                    ShutdownDX2D();
                    MessageBox.Show("Problem initialising DirectX" + System.Environment.NewLine + "Switching to GDI+" + System.Environment.NewLine + System.Environment.NewLine + e.ToString(), "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    console.CurrentDisplayEngine = DisplayEngine.GDI_PLUS;  //TODO the setupform wont show correct state
                }
            }
        }
        public static void ResetDX2DModeDescription()
        {
            // used to reset the FPS on the swapChain
            try
            {
                lock (m_objDX2Lock)
                {
                    if (!m_bDX2Setup) return;
                    ModeDescription modeDesc = new ModeDescription(displayTargetWidth, displayTargetHeight,
                                                       new Rational(console.DisplayFPS, 1), Format.B8G8R8A8_UNorm);                    
                    swapChain.ResizeTarget(ref modeDesc);
                }
            }
            catch (Exception e)
            {

            }
        }
        public static void ResizeDX2D()
        {
            try
            {
                lock (m_objDX2Lock)
                {
                    if (!m_bDX2Setup) return;

                    d2dRenderTarget.Dispose();
                    surface.Dispose();
                    renderView.Dispose();
                    backBuffer.Dispose();

                    swapChain.ResizeBuffers(1, displayTargetWidth, displayTargetHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
                    renderView = new RenderTargetView(device, backBuffer);
                    surface = backBuffer.QueryInterface<Surface>();
                    d2dRenderTarget = new RenderTarget(d2dFactory, surface,
                                                                    new RenderTargetProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                }
            }
            catch (Exception e)
            {
                //ShutdownDX2D();
                //MessageBox.Show("DirectX ResizeDX2D() failure\n" + e.Message, "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static int PanafallSplitBarPos {
            get { return (int)(displayTargetHeight * m_fPanafallSplitPerc); }
            //set { 
            //    m_nPanafallSplitBarPosY = value;
            //}
        }
        private static float m_fPanafallSplitPerc = 0.5f;
        public static float PanafallSplitBarPerc {
            get { return m_fPanafallSplitPerc; }
            set {
                m_fPanafallSplitPerc = value;
            }
        }
        private static bool m_bAntiAlias = false;
        public static bool AntiAlias {
            get { return m_bAntiAlias; }
            set { m_bAntiAlias = value; }
        }
        public static bool SpecialPanafall {
            get { return m_bSpecialPanafall; }
            set {
                m_bSpecialPanafall = value;
                if (m_bSpecialPanafall)
                {
                    m_fPanafallSplitPerc = 0.8f;
                }
                else
                {
                    m_fPanafallSplitPerc = 0.5f;
                }
                ResetWaterfallBmp();
            }
        }

        private static int m_nRX1DisplayHeight = 0;
        public static int RX1DisplayHeight {
            get { return m_nRX1DisplayHeight; }
            set { }
        }
        private static int m_nRX2DisplayHeight = 0;
        public static int RX2DisplayHeight {
            get { return m_nRX2DisplayHeight; }
            set { }
        }

        private static Vector2 m_pixelShift = new Vector2(0.5f, 0.5f);
        public static void RenderDX2D()
        {
            try
            {
                //float fTopHeight;
                //fTopHeight = SpecialPanafall ? 0.8f : 0.5f;

                lock (m_objDX2Lock)
                {
                    if (!m_bDX2Setup) return; // moved inside the lock so that a change in state by shutdown becomes thread safe

                    d2dRenderTarget.BeginDraw();

                    // midldle pixel align shift, NOTE: waterfall will switch internally to identity, and then restore
                    Matrix3x2 t = d2dRenderTarget.Transform;
                    t.TranslationVector = m_pixelShift;
                    d2dRenderTarget.Transform = t;

                    if (m_bAntiAlias)
                    {
                        d2dRenderTarget.AntialiasMode = AntialiasMode.PerPrimitive; // this will antialias even if multisampling is off
                    }
                    else
                    {
                        d2dRenderTarget.AntialiasMode = AntialiasMode.Aliased; // this will result in non antialiased lines only if multisampling = 1
                    }

                    d2dRenderTarget.TextAntialiasMode = TextAntialiasMode.Default;

                    d2dRenderTarget.Clear(SharpDX.Color.Black);

                    if (m_bitmapBackground != null)
                    {
                        // draw background image
                        RectangleF rectDest = new RectangleF(0, 0, displayTargetWidth, displayTargetHeight);
                        d2dRenderTarget.DrawBitmap(m_bitmapBackground, rectDest, 1f, BitmapInterpolationMode.Linear);
                    }


                    if (!split_display)
                    {
                        m_nRX1DisplayHeight = displayTargetHeight;
                        switch (current_display_mode)
                        {
                            case DisplayMode.SPECTRUM:
                                DrawSpectrumDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.SCOPE:
                                DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.SCOPE2:
                                DrawScope2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE:
                                DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE2:
                                DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.HISTOGRAM:
                                DrawHistogramDX2D(displayTargetWidth, m_nRX1DisplayHeight);
                                break;
                            case DisplayMode.PANAFALL:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = (int)(displayTargetHeight * m_fPanafallSplitPerc);
                                    split_display = PanafallSplitBarPos <= (displayTargetHeight / 2); // add more granularity, TODO change based on avaialble height
                                    DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                    DrawWaterfallDX2D(PanafallSplitBarPos, displayTargetWidth, displayTargetHeight - m_nRX1DisplayHeight, 1, true);
                                    split_display = false;
                                }
                                break;
                            case DisplayMode.PANASCOPE:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = displayTargetHeight / 2;
                                    split_display = true;
                                    DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                    DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, true);
                                    split_display = false;
                                }
                                break;
                            case DisplayMode.SPECTRASCOPE:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = displayTargetHeight / 2;
                                    split_display = true;
                                    DrawSpectrumDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                    DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, true);
                                    split_display = false;
                                }
                                break;
                        }
                    }
                    else
                    {
                        m_nRX1DisplayHeight = displayTargetHeight / 2;
                        switch (current_display_mode)
                        {
                            case DisplayMode.SPECTRUM:
                                DrawSpectrumDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.SCOPE:
                                DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            //case DisplayMode.SCOPE2:
                            //    DrawScope2DX2D(displayTargetWidth, displayTargetHeight / 2, false);
                            //    break;
                            case DisplayMode.PHASE:
                                DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE2:
                                DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.HISTOGRAM:
                                DrawHistogramDX2D(displayTargetWidth, m_nRX1DisplayHeight);
                                break;
                            case DisplayMode.PANAFALL:
                                m_nRX1DisplayHeight = displayTargetHeight / 4;
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                DrawWaterfallDX2D(m_nRX1DisplayHeight, displayTargetWidth, m_nRX1DisplayHeight, 1, true);
                                break;
                                //case DisplayMode.PANASCOPE:
                                //    DrawPanadapterDX2D(displayTargetWidth, displayTargetHeight / 4, 1, false);
                                //    DrawScopeDX2D(displayTargetWidth, displayTargetHeight / 4, true);
                                //    break;
                                //case DisplayMode.SPECTRASCOPE:
                                //    DrawSpectrumDX2D(displayTargetWidth, displayTargetHeight / 4, false);
                                //    DrawScopeDX2D(displayTargetWidth, displayTargetHeight / 4, true);
                                //    break;
                        }

                        m_nRX2DisplayHeight = displayTargetHeight / 2;
                        switch (current_display_mode_bottom)
                        {
                            //case DisplayMode.SPECTRUM:
                            //    DrawSpectrumDX2D(displayTargetWidth, m_nRX2DisplayHeight, true);
                            //    break;
                            //case DisplayMode.SCOPE:
                            //    DrawScopeDX2D(displayTargetWidth, m_nRX2DisplayHeight, true);
                            //    break;
                            //case DisplayMode.SCOPE2:
                            //    DrawScope2DX2D(displayTargetWidth, displayTargetHeight / 2, true);
                            //    break;
                            //case DisplayMode.PHASE:
                            //    DrawPhaseDX2D(displayTargetWidth, m_nRX2DisplayHeight, true);
                            //    break;
                            //case DisplayMode.PHASE2:
                            //    DrawPhase2DX2D(displayTargetWidth, m_nRX2DisplayHeight, true);
                            //    break;
                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                            //case DisplayMode.HISTOGRAM:
                            //    DrawHistogramDX2D(displayTargetWidth, m_nRX2DisplayHeight);
                            //    break;
                            case DisplayMode.PANAFALL:
                                m_nRX2DisplayHeight = displayTargetHeight / 4;
                                DrawPanadapterDX2D(m_nRX2DisplayHeight * 2, displayTargetWidth, m_nRX2DisplayHeight, 2, false);
                                DrawWaterfallDX2D(m_nRX2DisplayHeight * 3, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                                //case DisplayMode.PANASCOPE:
                                //    DrawPanadapterDX2D(displayTargetWidth, displayTargetHeight / 4, 2, false);
                                //    DrawScopeDX2D(displayTargetWidth, displayTargetHeight / 4, true);
                                //    break;
                                //case DisplayMode.SPECTRASCOPE:
                                //    DrawSpectrumDX2D(displayTargetWidth, displayTargetHeight / 4, false);
                                //    DrawScopeDX2D(displayTargetWidth, displayTargetHeight / 4, true);
                        }
                    }

                    // HIGH swr display warning
                    if (high_swr)
                    {
                        drawStringDX2D("High SWR", fontDX2d_font14, m_bDX2_Red, 245, 20);
                        d2dRenderTarget.DrawRectangle(new RectangleF(3, 3, displayTargetWidth - 6, displayTargetHeight - 6), m_bDX2_Red, 6f);
                    }

                    if (m_bFramRateIssue) d2dRenderTarget.FillRectangle(new RectangleF(0, 0, 8, 8), m_bDX2_Red);

                    calcFps();
                    if (m_bShowFPS) d2dRenderTarget.DrawText(m_nFps.ToString(), fontDX2d_callout, new RectangleF(10, 0, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);

                    // undo the translate
                    d2dRenderTarget.Transform = Matrix3x2.Identity;
                    
                    d2dRenderTarget.EndDraw();

                    // render
                    try
                    {
                        swapChain.Present(m_nVBlanks, PresentFlags.None); // PresentFlags.DoNotWait);
                    }
                    catch (SharpDXException se)
                    {
                        //// ignore SharpDXException... caputure 0x887A000A = DXGI_ERROR_WAS_STILL_DRAWING (only if DoNotWait used)
                        //if (!(((se.ResultCode.Code & 0x887A000A) == 0x887A000A) || ((se.HResult & 0x887A000A) == 0x887A000A)))
                        //{
                        //    Common.LogString("swapchain issue HR : " + se.HResult.ToString());
                        //    Common.LogString("swapchain issue RC : " + se.ResultCode.Code.ToString());
                        //    Common.LogException(se);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                ShutdownDX2D();
                Common.LogException(e);
            }
        }

        private static int m_nVBlanks = 0;
        public static int VerticalBlanks {
            get { return m_nVBlanks; }
            set {
                int v = value;
                if (v < 0) v = 0;
                if (v > 4) v = 4;
                m_nVBlanks = value; 
            }
        }

        private static int m_nFps = 0;
        private static int m_nFrameCount = 0;
        private static HiPerfTimer m_objFrameStartTimer = new HiPerfTimer();
        private static double m_fLastTime = m_objFrameStartTimer.ElapsedMsec;
        private static void calcFps()
        {
            double t = m_objFrameStartTimer.ElapsedMsec;
            m_nFrameCount++;
            if (t >= m_fLastTime + 1000)
            {
                double late = t - (m_fLastTime + 1000);
                if (late > 2000 || late < 0) late = 0; // ignore if too late
                m_nFps = m_nFrameCount;
                m_nFrameCount = 0;
                m_fLastTime = t - late;
            }
        }

        private static bool m_bPeakBlobMaximums = true;
        public static bool ShowPeakBlobs {
            get { return m_bPeakBlobMaximums; }
            set { m_bPeakBlobMaximums = value; }
        }
        private static bool m_bInsideFilterOnly = false;
        public static bool ShowPeakBlobsInsideFilterOnly {
            get { return m_bInsideFilterOnly; }
            set { m_bInsideFilterOnly = value; }
        }
        private static int m_nNumberOfMaximums = 3;
        public static int NumberOfPeakBlobs {
            get { return m_nNumberOfMaximums; }
            set {
                int t = value;
                if (t < 1) t = 1;
                // just use rx1 as both same length
                if (t > m_nRX1Maximums.Length) t = m_nRX1Maximums.Length;
                m_nNumberOfMaximums = t; 
            }
        }

        private struct Maximums
        {
            public float MaxY;
            public int X;
            public int MaxY_pixel;
            public bool Enabled;
            public double Time;
        }
        static private int isOccupied(int rx, int nX)
        {
            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int nRet = -1; // -1 returned if nothing in this area

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                int p1 = Math.Abs(nX - maximums[n].X);

                if (maximums[n].Enabled && p1 < 10) // 10 being the radius of the ellipse/circle
                { 
                    nRet = n;
                    break;
                }
            }
            return nRet;
        }

        static private HiPerfTimer m_objPeakHoldTimer = new HiPerfTimer();
        static private Maximums[] m_nRX1Maximums = new Maximums[20]; // max of 20 blobs
        static private Maximums[] m_nRX2Maximums = new Maximums[20]; // max of 20 blobs
        static private void processMaximums(int rx, float nMaxY, int nX, int nMaxY_pixel)
        {
            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int nOccupiedIndex = isOccupied(rx, nX);

            if (nOccupiedIndex >= 0)
            {
                if (nMaxY >= maximums[nOccupiedIndex].MaxY)
                {
                    maximums[nOccupiedIndex].Enabled = true;
                    maximums[nOccupiedIndex].MaxY = nMaxY;
                    maximums[nOccupiedIndex].X = nX;
                    maximums[nOccupiedIndex].MaxY_pixel = nMaxY_pixel;
                    maximums[nOccupiedIndex].Time = m_objPeakHoldTimer.ElapsedMsec;
                    Array.Sort<Maximums>(maximums, (x, y) => y.MaxY.CompareTo(x.MaxY));
                }
                return;
            }

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                if (nMaxY > maximums[n].MaxY)
                {
                    //move them down
                    for (int nn = m_nNumberOfMaximums - 1; nn > n; nn--)
                    {
                        maximums[nn].Enabled = maximums[nn - 1].Enabled;
                        maximums[nn].MaxY = maximums[nn-1].MaxY;
                        maximums[nn].X = maximums[nn-1].X;
                        maximums[nn].MaxY_pixel = maximums[nn-1].MaxY_pixel;
                        maximums[nn].Time = maximums[nn - 1].Time;
                    }

                    //add new
                    maximums[n].Enabled = true;
                    maximums[n].MaxY = nMaxY;
                    maximums[n].X = nX;
                    maximums[n].MaxY_pixel = nMaxY_pixel;
                    maximums[n].Time = m_objPeakHoldTimer.ElapsedMsec;

                    break;
                }
            }
        }

        static private void resetMaximums(int rx)
        {
            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                if (!m_bBlobPeakHold || (m_bBlobPeakHold && m_objPeakHoldTimer.ElapsedMsec >= maximums[n].Time + m_fBlobPeakHoldMS))
                {
                    maximums[n].Enabled = false;
                    maximums[n].MaxY = float.MinValue;
                }
            }
        }

        static private void getFilterXPositions(int rx, int W, bool local_mox, bool displayduplex, out int filter_left_x, out int filter_right_x)
        {
            int Low, High, f_diff, filter_low, filter_high;

            if (rx == 1)
            {
                Low = rx_display_low;
                High = rx_display_high;
                f_diff = freq_diff;
                filter_low = rx1_filter_low;
                filter_high = rx1_filter_high;
            }
            else
            {
                Low = rx2_display_low;
                High = rx2_display_high;
                f_diff = rx2_freq_diff;
                filter_low = rx2_filter_low;
                filter_high = rx2_filter_high;
            }
            if (local_mox)  // && !tx_on_vfob)
            {
                if (!displayduplex)
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                }
                filter_low = tx_filter_low;
                filter_high = tx_filter_high;
            }
            int width = High - Low;
            filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
            filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);
        }

        static private bool m_bBlobPeakHold = false;
        static public bool BlobPeakHold {
            get { return m_bBlobPeakHold; }
            set { m_bBlobPeakHold = value; }
        }
        static private double m_fBlobPeakHoldMS = 500f;
        static public double BlobPeakHoldMS {
            get { return m_fBlobPeakHoldMS; }
            set { m_fBlobPeakHoldMS = value; }
        }
        static private bool m_bBlobPeakHoldFade = false;
        static public bool BlobPeakHoldFade {
            get { return m_bBlobPeakHoldFade; }
            set { m_bBlobPeakHoldFade = value; }
        }

        static private bool isRxDuplex(int rx)
        {
            bool displayduplex = false;

            if (rx == 1)
            {
                if ((CurrentDisplayMode == DisplayMode.PANAFALL && display_duplex) ||
                        (CurrentDisplayMode == DisplayMode.WATERFALL && display_duplex) ||
                        (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;
            }
            else
            {
                if ((CurrentDisplayModeBottom == DisplayMode.PANAFALL && display_duplex) ||
                        (CurrentDisplayModeBottom == DisplayMode.WATERFALL && display_duplex) ||
                        (CurrentDisplayModeBottom == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;
            }


            return displayduplex;
        }

        static private SharpDX.Direct2D1.Ellipse m_objEllipse = new SharpDX.Direct2D1.Ellipse(Vector2.Zero, 5f, 5f);
        unsafe static private bool DrawPanadapterDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            if (grid_control)
            {
                clearBackgroundDX2D(rx, W, H, bottom);
                drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, false);
            }

            float local_max_y = float.MinValue;
            float local_max_x = float.MinValue;

            bool local_mox = false;
            bool displayduplex = isRxDuplex(rx);

            int grid_max = 0;
            int grid_min = 0;

            if (mox) {
                if (rx == 1 && !tx_on_vfob) local_mox = true;
                if (rx == 2 && tx_on_vfob) local_mox = true;
                if (rx == 1 && tx_on_vfob && !console.RX2Enabled) local_mox = true;
            }

            int yRange;
            float[] data;

            if (rx == 1)
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready)
                {
                    if (!displayduplex && (local_mox || (mox && tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_display_data.Length*/; i++)
                            current_display_data[i] = grid_min - rx1_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data[0])
                        fixed (void* wptr = &current_display_data[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                    }
                    data_ready = false;
                }
                data = current_display_data;
            }
            else// if(rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready_bottom)
                {

                    //MW0LGE//if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    if (blank_bottom_display || (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
                    {
                        for (int i = 0; i < W/*current_display_data_bottom.Length*/; i++)
                            current_display_data_bottom[i] = grid_min - rx2_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data_bottom[0])
                        fixed (void* wptr = &current_display_data_bottom[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                    }

                    data_ready_bottom = false;
                }
                data = current_display_data_bottom;
            }

            float max;
            float fOffset;

            if (rx == 1)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else if (mox && tx_on_vfob && !displayduplex)
                {
                    if (console.RX2Enabled) fOffset = rx1_display_cal_offset;
                    else fOffset = tx_display_cal_offset;
                }
                else fOffset = rx1_display_cal_offset;
            }
            else //if (rx == 2)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else fOffset = rx2_display_cal_offset;
            }

            if (!local_mox || (local_mox && displayduplex))
            {
                if (rx == 1) fOffset += rx1_preamp_offset;
                else /*if (rx == 2)*/ fOffset += rx2_preamp_offset;
            }

            //MW0LGE not used, as filling vertically with lines is faster than a filled very detailed
            //geometry. Just kept for reference
            //PathGeometry sharpGeometry = new PathGeometry(d2dRenderTarget.Factory);
            //using (GeometrySink geo = sharpGeometry.Open())
            //geo.BeginFigure(new SharpDX.Vector2(0, lowerH), FigureBegin.Filled);
            //geo.AddLine(new SharpDX.Vector2(i, Y));
            //        geo.EndFigure(FigureEnd.Closed);
            //        geo.Close();
            //        geo.Dispose();
            //    }
            //sharpGeometry.Dispose();

            SharpDX.Direct2D1.Brush lineBrush;
            SharpDX.Direct2D1.Brush fillBrush;

            if (local_mox)
            {
                lineBrush = m_bDX2_tx_data_line_pen_brush;
                fillBrush = m_bDX2_tx_data_line_fpen_brush;
            }
            else
            {
                lineBrush = m_bDX2_data_line_pen_brush;
                fillBrush = m_bDX2_data_fill_fpen_brush;
            }
            float penWidth = data_line_pen.Width;

            // calc start pos
            int Y;
            max = data[0] + fOffset;
            //Y = (int)(Math.Floor((grid_max - max) * H / yRange));
            Y = (int)(((grid_max - max) * H / yRange) - 0.5f); // -0.5 to mimic floor
            //Y = Math.Min(Y, H);
            Y = Y < H ? Y : H;
            Y += nVerticalShift;

            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 bottomPoint = new SharpDX.Vector2(0, nVerticalShift + H);
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2(0, Y);

            float local_max_Pixel_y = float.MinValue;

            int filter_left_x=0, filter_right_x=0;
            if (m_bPeakBlobMaximums)
            {
                resetMaximums(rx);
                if (m_bInsideFilterOnly) getFilterXPositions(rx, W, local_mox, displayduplex, out filter_left_x, out filter_right_x);
            }

            unchecked // we dont expect any overflows
            {
                for (int i = 0; i < W; i++)
                {
                    max = data[i] + fOffset;

                    //Y = (int)(Math.Floor((grid_max - max) * H / yRange));
                    Y = (int)(((grid_max - max) * H / yRange) - 0.5f); // -0.5 to mimic floor
                    //Y = Math.Min(Y, H);
                    Y = Y < H ? Y : H;
                    Y += nVerticalShift;

                    if (max > local_max_y)
                    {
                        // store peak
                        local_max_y = max;
                        local_max_x = i;
                        local_max_Pixel_y = Y;
                    }

                    point.X = i;
                    point.Y = Y;

                    if (m_bPeakBlobMaximums)
                    {
                        if (m_bInsideFilterOnly)
                        {
                            bool bInsideFilter = (i >= filter_left_x && i <= filter_right_x);
                            if(bInsideFilter) processMaximums(rx, max, i, Y);
                        }
                        else
                        {
                            processMaximums(rx, max, i, Y);
                        }
                    }

                    if (pan_fill)
                    {
                        // draw vertical line, this is so much faster than FillGeometry as the geo created would be so complex any fill alogorthm would struggle
                        bottomPoint.X = i;
                        d2dRenderTarget.DrawLine(bottomPoint, point, fillBrush); // defaults to 1f width
                    }

                    d2dRenderTarget.DrawLine(previousPoint, point, lineBrush, penWidth);

                    previousPoint = point;
                }

                // peak blobs
                if (m_bPeakBlobMaximums)
                {
                    Maximums[] maximums;
                    if (rx == 1)
                        maximums = m_nRX1Maximums;
                    else
                        maximums = m_nRX2Maximums;
                    
                    for (int n = 0; n < m_nNumberOfMaximums; n++)
                    {
                        if (maximums[n].Enabled) {
                            if (m_bBlobPeakHold && m_bBlobPeakHoldFade)
                            {
                                double fTmp = (double)(m_objPeakHoldTimer.ElapsedMsec - maximums[n].Time) / m_fBlobPeakHoldMS;
                                fTmp = fTmp > 1 ? 1f : fTmp;

                                fTmp *= (Math.PI / 2);
                                fTmp = Math.Cos(fTmp);

                                m_bDX2_PeakBlob.Opacity = (float)fTmp;
                                m_bDX2_PeakBlobText.Opacity = (float)fTmp;
                            }
                            else
                            {
                                m_bDX2_PeakBlob.Opacity = 1;
                                m_bDX2_PeakBlobText.Opacity = 1;
                            }
                            m_objEllipse.Point.X = maximums[n].X;
                            m_objEllipse.Point.Y = maximums[n].MaxY_pixel;
                            d2dRenderTarget.DrawEllipse(m_objEllipse, m_bDX2_PeakBlob);
                            d2dRenderTarget.DrawText(maximums[n].MaxY.ToString("f1") + " (" + (n + 1).ToString() + ")", fontDX2d_callout, new RectangleF(m_objEllipse.Point.X + 6, m_objEllipse.Point.Y - 8, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_PeakBlobText, DrawTextOptions.None);
                        }
                    }
                }
            }

            if (!bottom)
            {
                max_y = local_max_y;
                max_x = local_max_x;
            }

            return true;
        }

        public static void ResetWaterfallTimers()
        {
            //timer_waterfall.Stop();
            //timer_waterfall2.Stop();
            //timer_waterfall.Start();
            //timer_waterfall2.Start();
            m_nRX1WaterFallFrameCount = 0;
            m_nRX2WaterFallFrameCount = 0;
        }

        private static int m_nRX1WaterFallFrameCount = 0; // 1=every frame, 2= every other, etc
        private static int m_nRX2WaterFallFrameCount = 0;

        unsafe static private bool DrawWaterfallDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            // undo the rendertarget transform that is used to move linedraws to middle of pixel grid
            Matrix3x2 originalTransform = d2dRenderTarget.Transform;
            d2dRenderTarget.Transform = Matrix3x2.Identity;

            if (waterfall_data == null || waterfall_data.Length < W)
            {
                waterfall_data = new float[W];		// array of points to display
            }

            float local_max_y = float.MinValue;
            bool local_mox = false;
            if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
            if (rx == 2 && tx_on_vfob && mox) local_mox = true;
            float local_min_y_w3sz = float.MaxValue;
            float display_min_w3sz = float.MaxValue;
            float display_max_w3sz = float.MinValue;
            float min_y_w3sz = float.MaxValue;
            int R = 0, G = 0, B = 0;
            int grid_max = 0;
            int grid_min = 0;
            bool displayduplex = isRxDuplex(rx);
            float low_threshold = 0.0f;
            float high_threshold = 0.0f;
            float waterfall_minimum = 0.0f;
            //float rx1_waterfall_minimum = 0.0f;
            //float rx2_waterfall_minimum = 0.0f;
            ColorSheme cSheme = ColorSheme.enhanced;
            Color low_color = Color.Black;
            Color mid_color = Color.Red;
            Color high_color = Color.Blue;
            int sample_rate = 0;//MW0LGE

            if (rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    //waterfall_minimum = rx2_waterfall_minimum; // check
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    sample_rate = sample_rate_rx2;
                    high_threshold = rx2_waterfall_high_threshold;
                    if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
                    {
                        //waterfall_minimum = rx2_waterfall_minimum;
                        low_threshold = RX2waterfallPreviousMinValue;
                    }
                    else low_threshold = rx2_waterfall_low_threshold;
                }
                cSheme = rx2_color_sheme;
                low_color = rx2_waterfall_low_color;
                mid_color = rx2_waterfall_mid_color;
                high_color = rx2_waterfall_high_color;
            }
            else
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    sample_rate = sample_rate_tx;
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    //waterfall_minimum = rx1_waterfall_minimum; // check
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    sample_rate = sample_rate_rx1;
                    low_threshold = RX1waterfallPreviousMinValue;
                    high_threshold = waterfall_high_threshold;
                    if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
                    {
                        //waterfall_minimum = rx1_waterfall_minimum;
                        low_threshold = RX1waterfallPreviousMinValue;
                    }
                    else low_threshold = waterfall_low_threshold;
                }
                cSheme = color_sheme;
                low_color = waterfall_low_color;
                mid_color = waterfall_mid_color;
                high_color = waterfall_high_color;
            }

            if (console.PowerOn)
            {
                if (rx == 1 && waterfall_data_ready)
                {
                    if (!displayduplex && local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_waterfall_data.Length*/; i++)
                            current_waterfall_data[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data[0])
                        fixed (void* wptr = &current_waterfall_data[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));

                    }
                    waterfall_data_ready = false;
                }
                else if (rx == 2 && waterfall_data_ready_bottom)
                {
                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < W/*current_waterfall_data_bottom.Length*/; i++)
                            current_waterfall_data_bottom[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data_bottom[0])
                        fixed (void* wptr = &current_waterfall_data_bottom[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                    }

                    waterfall_data_ready_bottom = false;
                }

                //double duration;
                bool bRXdraw = false;
                //if (rx == 1)
                //{
                //    timer_waterfall.Stop();
                //    duration = timer_waterfall.DurationMsec;
                //    if (duration >= waterfall_update_period)
                //    {
                //        bRXdraw = true;
                //        timer_waterfall.Start();
                //    }
                //}
                //else/* if (rx == 2)*/
                //{
                //    timer_waterfall2.Stop();
                //    duration = timer_waterfall2.DurationMsec;
                //    if (duration >= rx2_waterfall_update_period)
                //    {
                //        bRXdraw = true;
                //        timer_waterfall2.Start();
                //    }
                //}

                if (rx == 1)
                {
                    m_nRX1WaterFallFrameCount++;
                    if (m_nRX1WaterFallFrameCount >= waterfall_update_period)
                    {
                        m_nRX1WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }
                else
                {
                    m_nRX2WaterFallFrameCount++;
                    if (m_nRX2WaterFallFrameCount >= rx2_waterfall_update_period)
                    {
                        m_nRX2WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }

                if(bRXdraw)
                { 
                    //MW0LGE
                    float[] data;

                    if (rx == 1)
                    {
                        data = current_waterfall_data;
                    }
                    else/* if (rx == 2)*/
                    {
                        data = current_waterfall_data_bottom;
                    }

                    float max;
                    float fOffset = 0; ///MW0LGE - block of code moved out of for loop +- for now, TODO

                    if (!local_mox)
                    {
                        if (rx == 1) fOffset += rx1_display_cal_offset;
                        else if (rx == 2) fOffset += rx2_display_cal_offset;
                    }
                    else
                    {
                        if (rx == 1) fOffset += rx1_display_cal_offset;
                        else if (rx == 2) fOffset += tx_display_cal_offset;
                    }

                    if (!local_mox)
                    {
                        if (rx == 1) fOffset += (rx1_preamp_offset - alex_preamp_offset);
                        else if (rx == 2) fOffset += (rx2_preamp_offset);
                    }
                    else
                    {
                        if (rx == 1) fOffset += (rx1_preamp_offset - alex_preamp_offset);
                    }

                    for (int i = 0; i < W; i++)
                    {
                        max = data[i] + fOffset; //MW0LGE

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


                    //BitmapData bitmapData;

                    int pixel_size = 4;
                    byte[] row = new byte[W * pixel_size];

                    SharpDX.Direct2D1.Bitmap topPixels;
                    byte nbBitmapAlpaha = 255;

                    // get top pixels, store into new bitmap, ready to display them lower down by 1 pixel
                    if (rx == 1)
                    {
                        topPixels = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2((int)waterfall_bmp_dx2d.Size.Width, (int)waterfall_bmp_dx2d.Size.Height - 1), 
                            new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                        topPixels.CopyFromBitmap(waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));                        
                    }
                    else //rx2
                    {
                        topPixels = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2((int)waterfall_bmp2_dx2d.Size.Width, (int)waterfall_bmp2_dx2d.Size.Height - 1),
                            new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

                        topPixels.CopyFromBitmap(waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));
                    }

                    #region colours
                    switch (cSheme)
                    {
                        case (ColorSheme.original):
                            {
                                // draw new data
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
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

                                    if (waterfall_minimum > waterfall_data[i])
                                        waterfall_minimum = waterfall_data[i];

                                    row[i * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[i * pixel_size + 1] = (byte)G;
                                    row[i * pixel_size + 2] = (byte)B;
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorSheme.LinAuto):
                            {
                                for (int i = 0; i < W; i++)	// for each pixel in the new line
                                {
                                    display_min_w3sz = min_y_w3sz - 5; //for histogram equilization
                                    display_max_w3sz = max_y; //for histogram equalization

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
                                    row[i * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;
                    }
                    #endregion

                    if (rx == 1)
                    {
                        waterfall_bmp_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                        waterfall_bmp_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                    }
                    else
                    {
                        waterfall_bmp2_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                        waterfall_bmp2_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                    }

                    topPixels.Dispose();

                    if (rx == 1)
                        RX1waterfallPreviousMinValue = (((RX1waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc
                    else
                        RX2waterfallPreviousMinValue = ((RX2waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10 + 1; //wfagc
                }

                if (rx == 1)
                {
                    d2dRenderTarget.DrawBitmap(waterfall_bmp_dx2d, new RectangleF(0, nVerticalShift + 20, waterfall_bmp_dx2d.Size.Width, waterfall_bmp_dx2d.Size.Height), 1f, BitmapInterpolationMode.Linear);
                }
                else
                {
                    d2dRenderTarget.DrawBitmap(waterfall_bmp2_dx2d, new RectangleF(0, nVerticalShift + 20, waterfall_bmp2_dx2d.Size.Width, waterfall_bmp2_dx2d.Size.Height), 1f, BitmapInterpolationMode.Linear);
                }
            }

            // return the transform to what it was
            d2dRenderTarget.Transform = originalTransform;

            // MW0LGE now draw any grid/labels/scales over the top of waterfall
            if (grid_control) drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, true);

            return true;
        }

        private static Color4 convertColour(Color c)
        {
            return new Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        private static SharpDX.Direct2D1.SolidColorBrush convertBrush(SolidBrush b)
        {
            return new SharpDX.Direct2D1.SolidColorBrush(d2dRenderTarget, convertColour(b.Color));
        }

        public static void SetDX2BackgoundImage(System.Drawing.Image image)
        {
            if (!m_bDX2Setup) return;

            Bitmap graphicsImage = null;

            if (image != null) graphicsImage = new Bitmap(image);

            SetDX2BackgoundImage(graphicsImage);

            if (graphicsImage != null) graphicsImage.Dispose();
        }
        public static void SetDX2BackgoundImage(System.Drawing.Bitmap bitmap)
        {
            lock (m_objDX2Lock)
            {
                if (!m_bDX2Setup) return;

                if (m_bitmapBackground != null) m_bitmapBackground.Dispose();

                m_bitmapBackground = null;
                if (bitmap != null) m_bitmapBackground = SDXBitmapFromSysBitmap(d2dRenderTarget, bitmap);
            }
        }

        private static SharpDX.Direct2D1.Bitmap m_bitmapBackground;
        private static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmap(RenderTarget device, System.Drawing.Bitmap bitmap)
        {
            Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapProperties bitmapProperties = new BitmapProperties(new SDXPixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            Size2 size = new Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (DataStream tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new SharpDX.Direct2D1.Bitmap(device, size, tempStream, stride, bitmapProperties);
            }
        }

        //--------------------------
        private static SharpDX.Direct2D1.Brush m_bDX2_data_fill_fpen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_fpen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_pen_brush;

        private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_zero_line_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_cw_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchActive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColour;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColourInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pTNFInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTNFInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_grid_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_hgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_hgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_pen;

        private static SharpDX.Direct2D1.Brush m_bDX2_display_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutActive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pHighlighted;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWHighlighedFillColour;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_band_edge_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen_inb;
        private static SharpDX.Direct2D1.Brush m_bDX2_band_edge_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen_inb;
        private static SharpDX.Direct2D1.Brush m_bDX2_Red;
        private static SharpDX.Direct2D1.Brush m_bDX2_Yellow;
        private static SharpDX.Direct2D1.Brush m_bDX2_YellowGreen;

        private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlob;
        private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlobText;

        private static SharpDX.Direct2D1.Brush m_bDX2_grid_tx_text_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_pana_text_brush;

        private static SharpDX.Direct2D1.Brush m_bDX2_p1;

        private static SharpDX.Direct2D1.Brush m_bDX2_display_background_brush;

        private static SharpDX.Direct2D1.Brush m_bDX2_y1_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_y2_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_waveform_line_pen;

        private static SharpDX.Direct2D1.Brush m_bDX2_dhp;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp1;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp2;

        private static SharpDX.Direct2D1.StrokeStyle m_styleDots;

        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumberScale;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumbers;
        //--------------------------
        private static void buildDX2Resources()
        {
            lock (m_objDX2Lock)
            {
                if (!m_bDX2Setup) return;

                if (m_bDX2_data_fill_fpen_brush != null) m_bDX2_data_fill_fpen_brush.Dispose();
                if (m_bDX2_data_line_pen_brush != null) m_bDX2_data_line_pen_brush.Dispose();
                if (m_bDX2_tx_data_line_fpen_brush != null) m_bDX2_tx_data_line_fpen_brush.Dispose();
                if (m_bDX2_tx_data_line_pen_brush != null) m_bDX2_tx_data_line_pen_brush.Dispose();

                if (m_bDX2_p1 != null) m_bDX2_p1.Dispose();
                if (m_bDX2_display_background_brush != null) m_bDX2_display_background_brush.Dispose();

                if (m_bDX2_m_bHightlightNumbers != null) m_bDX2_m_bHightlightNumbers.Dispose();
                if (m_bDX2_m_bHightlightNumberScale != null) m_bDX2_m_bHightlightNumberScale.Dispose();

                if (m_bDX2_grid_tx_text_brush != null) m_bDX2_grid_tx_text_brush.Dispose();
                if (m_bDX2_grid_text_brush != null) m_bDX2_grid_text_brush.Dispose();
                if (m_bDX2_pana_text_brush != null) m_bDX2_pana_text_brush.Dispose();

                if (m_bDX2_display_filter_brush != null) m_bDX2_display_filter_brush.Dispose();
                if (m_bDX2_tx_filter_brush != null) m_bDX2_tx_filter_brush.Dispose();
                if (m_bDX2_m_bTextCallOutActive != null) m_bDX2_m_bTextCallOutActive.Dispose();
                if (m_bDX2_m_bTextCallOutInactive != null) m_bDX2_m_bTextCallOutInactive.Dispose();
                if (m_bDX2_m_pHighlighted != null) m_bDX2_m_pHighlighted.Dispose();
                if (m_bDX2_m_bBWHighlighedFillColour != null) m_bDX2_m_bBWHighlighedFillColour.Dispose();
                if (m_bDX2_tx_band_edge_pen != null) m_bDX2_tx_band_edge_pen.Dispose();
                if (m_bDX2_tx_vgrid_pen_inb != null) m_bDX2_tx_vgrid_pen_inb.Dispose();
                if (m_bDX2_band_edge_pen != null) m_bDX2_band_edge_pen.Dispose();
                if (m_bDX2_grid_pen_inb != null) m_bDX2_grid_pen_inb.Dispose();
                if (m_bDX2_Red != null) m_bDX2_Red.Dispose();
                if (m_bDX2_Yellow != null) m_bDX2_Yellow.Dispose();
                if (m_bDX2_YellowGreen != null) m_bDX2_YellowGreen.Dispose();
                
                if (m_bDX2_PeakBlob != null) m_bDX2_PeakBlob.Dispose();
                if (m_bDX2_PeakBlobText != null) m_bDX2_PeakBlobText.Dispose();

                if (m_bDX2_sub_rx_filter_brush != null) m_bDX2_sub_rx_filter_brush.Dispose();
                if (m_bDX2_sub_rx_zero_line_pen != null) m_bDX2_sub_rx_zero_line_pen.Dispose();
                if (m_bDX2_tx_filter_pen != null) m_bDX2_tx_filter_pen.Dispose();
                if (m_bDX2_cw_zero_pen != null) m_bDX2_cw_zero_pen.Dispose();
                if (m_bDX2_m_pNotchActive != null) m_bDX2_m_pNotchActive.Dispose();
                if (m_bDX2_m_bBWFillColour != null) m_bDX2_m_bBWFillColour.Dispose();
                if (m_bDX2_m_pNotchInactive != null) m_bDX2_m_pNotchInactive.Dispose();
                if (m_bDX2_m_bBWFillColourInactive != null) m_bDX2_m_bBWFillColourInactive.Dispose();
                if (m_bDX2_m_pTNFInactive != null) m_bDX2_m_pTNFInactive.Dispose();
                if (m_bDX2_m_bTNFInactive != null) m_bDX2_m_bTNFInactive.Dispose();
                if (m_bDX2_tx_grid_zero_pen != null) m_bDX2_tx_grid_zero_pen.Dispose();
                if (m_bDX2_grid_zero_pen != null) m_bDX2_grid_zero_pen.Dispose();
                if (m_bDX2_tx_vgrid_pen != null) m_bDX2_tx_hgrid_pen.Dispose();
                if (m_bDX2_grid_pen != null) m_bDX2_grid_pen.Dispose();
                if (m_bDX2_tx_hgrid_pen != null) m_bDX2_tx_hgrid_pen.Dispose();
                if (m_bDX2_hgrid_pen != null) m_bDX2_hgrid_pen.Dispose();
                if (m_bDX2_grid_text_pen != null) m_bDX2_grid_text_pen.Dispose();

                if (m_bDX2_y1_brush != null) m_bDX2_y1_brush.Dispose();
                if (m_bDX2_y2_brush != null) m_bDX2_y1_brush.Dispose();
                if (m_bDX2_waveform_line_pen != null) m_bDX2_waveform_line_pen.Dispose();

                if (m_bDX2_dhp != null) m_bDX2_dhp.Dispose();
                if (m_bDX2_dhp != null) m_bDX2_dhp1.Dispose();
                if (m_bDX2_dhp != null) m_bDX2_dhp2.Dispose();

                if (m_styleDots != null) m_styleDots.Dispose();

                m_bDX2_data_fill_fpen_brush = convertBrush((SolidBrush)data_fill_fpen.Brush);
                m_bDX2_data_line_pen_brush = convertBrush((SolidBrush)data_line_pen.Brush);
                m_bDX2_tx_data_line_fpen_brush = convertBrush((SolidBrush)tx_data_line_fpen.Brush);
                m_bDX2_tx_data_line_pen_brush = convertBrush((SolidBrush)tx_data_line_pen.Brush);

                m_bDX2_p1 = convertBrush((SolidBrush)p1.Brush);
                m_bDX2_display_background_brush = convertBrush((SolidBrush)display_background_brush);

                m_bDX2_m_bHightlightNumbers = convertBrush(new SolidBrush(Color.FromArgb(255, 255, 255)));
                m_bDX2_m_bHightlightNumberScale = convertBrush(new SolidBrush(Color.FromArgb(192, 64, 64, 64)));

                m_bDX2_grid_tx_text_brush = convertBrush((SolidBrush)grid_tx_text_brush);
                m_bDX2_grid_text_brush = convertBrush((SolidBrush)grid_text_brush);
                m_bDX2_pana_text_brush = convertBrush((SolidBrush)pana_text_brush);

                m_bDX2_display_filter_brush = convertBrush((SolidBrush)display_filter_brush);
                m_bDX2_tx_filter_brush = convertBrush((SolidBrush)tx_filter_brush);
                m_bDX2_m_bTextCallOutActive = convertBrush((SolidBrush)m_bTextCallOutActive);
                m_bDX2_m_bTextCallOutInactive = convertBrush((SolidBrush)m_bTextCallOutInactive);
                m_bDX2_m_pHighlighted = convertBrush((SolidBrush)m_pHighlighted.Brush);
                m_bDX2_m_bBWHighlighedFillColour = convertBrush((SolidBrush)m_bBWHighlighedFillColour);
                m_bDX2_tx_band_edge_pen = convertBrush((SolidBrush)tx_band_edge_pen.Brush);
                m_bDX2_tx_vgrid_pen_inb = convertBrush((SolidBrush)tx_vgrid_pen_inb.Brush);
                m_bDX2_band_edge_pen = convertBrush((SolidBrush)band_edge_pen.Brush);
                m_bDX2_grid_pen_inb = convertBrush((SolidBrush)grid_pen_inb.Brush);

                m_bDX2_Red = convertBrush(new SolidBrush(Color.Red));
                m_bDX2_Yellow = convertBrush(new SolidBrush(Color.Yellow));
                m_bDX2_YellowGreen = convertBrush(new SolidBrush(Color.YellowGreen));

                m_bDX2_PeakBlob = convertBrush(new SolidBrush(Color.OrangeRed));
                m_bDX2_PeakBlobText = convertBrush(new SolidBrush(Color.Chartreuse));

                m_bDX2_y1_brush = convertBrush(new SolidBrush(Color.FromArgb(64, 64, 64)));
                m_bDX2_y2_brush = convertBrush(new SolidBrush(Color.FromArgb(48, 48, 48)));
                m_bDX2_waveform_line_pen = convertBrush(new SolidBrush(Color.LightGreen));

                m_bDX2_dhp = convertBrush(new SolidBrush(Color.FromArgb(0, 255, 0)));
                m_bDX2_dhp1 = convertBrush(new SolidBrush(Color.FromArgb(150, 0, 0, 255)));
                m_bDX2_dhp2 = convertBrush(new SolidBrush(Color.FromArgb(150, 255, 0, 0)));

                m_bDX2_sub_rx_filter_brush = convertBrush((SolidBrush)sub_rx_filter_brush);
                m_bDX2_sub_rx_zero_line_pen = convertBrush((SolidBrush)sub_rx_zero_line_pen.Brush);
                m_bDX2_tx_filter_pen = convertBrush((SolidBrush)tx_filter_pen.Brush);
                m_bDX2_cw_zero_pen = convertBrush((SolidBrush)cw_zero_pen.Brush);
                m_bDX2_m_pNotchActive = convertBrush((SolidBrush)m_pNotchActive.Brush);
                m_bDX2_m_bBWFillColour = convertBrush((SolidBrush)m_bBWFillColour);
                m_bDX2_m_pNotchInactive = convertBrush((SolidBrush)m_pNotchInactive.Brush);
                m_bDX2_m_bBWFillColourInactive = convertBrush((SolidBrush)m_bBWFillColourInactive);
                m_bDX2_m_pTNFInactive = convertBrush((SolidBrush)m_pTNFInactive.Brush);
                m_bDX2_m_bTNFInactive = convertBrush((SolidBrush)m_bTNFInactive);
                m_bDX2_tx_grid_zero_pen = convertBrush((SolidBrush)tx_grid_zero_pen.Brush);
                m_bDX2_grid_zero_pen = convertBrush((SolidBrush)grid_zero_pen.Brush);

                m_bDX2_tx_vgrid_pen = convertBrush((SolidBrush)tx_vgrid_pen.Brush);
                m_bDX2_grid_pen = convertBrush((SolidBrush)grid_pen.Brush);
                m_bDX2_tx_hgrid_pen = convertBrush((SolidBrush)tx_hgrid_pen.Brush);
                m_bDX2_hgrid_pen = convertBrush((SolidBrush)hgrid_pen.Brush);
                m_bDX2_grid_text_pen = convertBrush((SolidBrush)grid_text_pen.Brush);

                StrokeStyleProperties ssp = new StrokeStyleProperties();
                ssp = new SharpDX.Direct2D1.StrokeStyleProperties() { DashOffset = 2, DashStyle = SharpDX.Direct2D1.DashStyle.Dash };

                m_styleDots = new StrokeStyle(d2dFactory, ssp);                
            }
        }
        //--------------------------
        private static SharpDX.DirectWrite.Factory fontFactory;
        //
        private static SharpDX.DirectWrite.TextFormat fontDX2d_callout;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font9;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_panafont;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font14;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font1;
        //--------------------------
        private static void buildFontsDX2D()
        {
            lock (m_objDX2Lock)
            {
                if (!m_bDX2Setup) return;

                if (fontFactory != null) fontFactory.Dispose();
                if (fontDX2d_callout != null) fontDX2d_callout.Dispose();
                if (fontDX2d_font9 != null) fontDX2d_font9.Dispose();
                if (fontDX2d_panafont != null) fontDX2d_panafont.Dispose();
                if (fontDX2d_font14 != null) fontDX2d_font14.Dispose();
                if (fontDX2d_font1 != null) fontDX2d_font1.Dispose();

                fontFactory = new SharpDX.DirectWrite.Factory();

                fontDX2d_callout = new SharpDX.DirectWrite.TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, (m_fntCallOutFont.Size/72) * d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9 = new SharpDX.DirectWrite.TextFormat(fontFactory, font9.FontFamily.Name, (font9.Size / 72) * d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_panafont = new SharpDX.DirectWrite.TextFormat(fontFactory, pana_font.FontFamily.Name, (pana_font.Size / 72) * d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font14 = new SharpDX.DirectWrite.TextFormat(fontFactory, font14.FontFamily.Name, (font14.Size / 72) * d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font1 = new SharpDX.DirectWrite.TextFormat(fontFactory, font1.FontFamily.Name, (font1.Size / 72) * d2dRenderTarget.DotsPerInch.Width);
            }
        }
        static void clearBackgroundDX2D(int rx, int W, int H, bool bottom)
        {
            // MW0LGE
            if (rx == 1)
            {
                if (bottom)
                {
                    d2dRenderTarget.FillRectangle(new RectangleF(0, H, W, H), m_bDX2_display_background_brush);
                }
                else
                {
                    d2dRenderTarget.FillRectangle(new RectangleF(0, 0, W, H), m_bDX2_display_background_brush);
                }
            }
            else if (rx == 2)
            {
                if (bottom)
                {
                    if (current_display_mode_bottom == DisplayMode.PANAFALL)
                    {
                        d2dRenderTarget.FillRectangle(new RectangleF(0, H * 3, W, H), m_bDX2_display_background_brush);
                    }
                    else d2dRenderTarget.FillRectangle(new RectangleF(0, H, W, H), m_bDX2_display_background_brush);
                }
                else
                {
                    d2dRenderTarget.FillRectangle(new RectangleF(0, H * 2, W, H), m_bDX2_display_background_brush);
                }
            }
        }
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, float strokeWidth = 1f )
        {
            //0.5f's to move into 'centre' of desired pixel
            d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1/* + 0.5f*/, y1/* + 0.5f*/), new SharpDX.Vector2(x2/* + 0.5f*/, y2/* + 0.5f*/), b, strokeWidth);
        }
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, StrokeStyle strokeStyle, float strokeWidth = 1f)
        {
            //0.5f's to move into 'centre' of desired pixel
            d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1/* + 0.5f*/, y1/* + 0.5f*/), new SharpDX.Vector2(x2/* + 0.5f*/, y2/* + 0.5f*/), b, strokeWidth, strokeStyle);
        }
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);

            d2dRenderTarget.FillRectangle(rect, b);
        }
        private static void drawElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
        {
            Ellipse e = new Ellipse(new SharpDX.Vector2(xMiddle/* + 0.5f*/, yMiddle/* + 0.5f*/), w / 2, h / 2);

            d2dRenderTarget.DrawEllipse(e, b);
        }
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, Rectangle r)
        {
            RectangleF rect = new RectangleF(r.X, r.Y, r.Width, r.Height);

            d2dRenderTarget.FillRectangle(rect, b);
        }
        private static void drawStringDX2D(String s, SharpDX.DirectWrite.TextFormat tf, SharpDX.Direct2D1.Brush b, float x, float y)
        {
            RectangleF rect = new RectangleF(x, y, float.PositiveInfinity, float.PositiveInfinity);
            d2dRenderTarget.DrawText(s, tf, rect, b, DrawTextOptions.None);
        }
        private static void drawFilterOverlayDX2D(SharpDX.Direct2D1.Brush brush, int filter_left_x, int filter_right_x, int W, int H, int rx, int top, bool bottom, int nVerticalShfit)
        {
            // make sure something visible
            if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            // draw rx filter
            int nWidth = filter_right_x - filter_left_x;

            RectangleF rect = new RectangleF(filter_left_x, nVerticalShfit + top, nWidth, H - top);
            d2dRenderTarget.FillRectangle(rect, brush);
        }
        private static void drawChannelBarDX2D(Channel chan, int left, int right, int top, int height, Color c, Color h)
        {
            int width = right - left;

            // get a purty pen to draw with 
            Pen p = new Pen(h, 1);

            // shade in the channel
            drawFillRectangleDX2D(convertBrush(new SolidBrush(c)), left, top, width, height);

            // draw a left and right line on the side of the rectancle if wide enough
            if (width > 2)
            {
                drawLineDX2D(convertBrush((SolidBrush)p.Brush), left, top, left, top + height - 1, p.Width);
                drawLineDX2D(convertBrush((SolidBrush)p.Brush), right, top, right, top + height - 1, p.Width);
            }
        }
        private static Dictionary<string, System.Drawing.SizeF> stringSizes = new Dictionary<string, System.Drawing.SizeF>();
        private static System.Drawing.SizeF measureStringDX2D(String s, SharpDX.DirectWrite.TextFormat tf)
        {
            // keep cache of calced sizes as this is quite a slow process
            //String key = s + tf.FontFamilyName + tf.FontSize; // very unlikely we will have different fonts of same string, let us just use 's' as key

            if (stringSizes.ContainsKey(s)) return stringSizes[s];

            SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(fontFactory, s, tf, float.PositiveInfinity, float.PositiveInfinity);
            if(stringSizes.Count>100) stringSizes.Remove(stringSizes.Keys.First()); // keep 100, dump oldest
            System.Drawing.SizeF sz = new System.Drawing.SizeF(layout.Metrics.Width, layout.Metrics.Height);
            layout.Dispose();
            stringSizes.Add(s, sz);

            return sz;
        }
        //--------------------------

        private static int getCWSideToneShift(int rx)
        {
            int nRet = 0;
            DSPMode mode = (rx==1) ? rx1_dsp_mode : rx2_dsp_mode;

            switch (mode)
            {
                case (DSPMode.CWL):
                    nRet = cw_pitch;
                    break;
                case (DSPMode.CWU):
                    nRet = -cw_pitch;
                    break;
            }

            return nRet;
        }
        private static void drawPanadapterAndWaterfallGridDX2D(int nVerticalShift, int W, int H, int rx, bool bottom, bool bIsWaterfall = false)
        {
            // MW0LGE
            // this now draws the grid for either panadapter or waterfall, pass in a bool to pick
            //
            DisplayLabelAlignment label_align = display_label_align;
            bool local_mox = false;
            bool displayduplex = isRxDuplex(rx);
            if (mox)
            {
                if (rx == 1 && !tx_on_vfob) local_mox = true;
                if (rx == 2 && tx_on_vfob) local_mox = true;
                if (rx == 1 && tx_on_vfob && !console.RX2Enabled) local_mox = true;
            }
            int Low = 0;					// initialize variables
            int High = 0;
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

            #region setup
            //MW0LGE
            //int nVerticalShift = getVerticalShift(rx, bottom, W, H);
            int cwSideToneShift = getCWSideToneShift(rx);
            int cwSideToneShiftInverted = cwSideToneShift * -1; // invert the sign as cw zero lines/tx lines etc are a shift in opposite direction to the grid

            if (rx == 1)
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    if (displayduplex)
                    {
                        Low = rx_display_low;
                        High = rx_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                    sample_rate = sample_rate_tx;
                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx_display_low;
                    High = rx_display_high;
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                f_diff = freq_diff;
            }
            else// if(rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    if (displayduplex)
                    {
                        Low = rx2_display_low;
                        High = rx2_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                    sample_rate = sample_rate_tx;
                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    grid_step = rx2_spectrum_grid_step;
                    sample_rate = sample_rate_rx2;
                }

                f_diff = rx2_freq_diff;
            }

            if (local_mox && !displayduplex)// || (mox && tx_on_vfob))
            {
                Low = tx_display_low;
                High = tx_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                grid_step = tx_spectrum_grid_step;
                sample_rate = sample_rate_tx;
                label_align = tx_display_label_align;
                if (rx == 1 && !console.VFOBTX) f_diff = freq_diff;
                else f_diff = rx2_freq_diff;
            }
            else if (rx == 2)
            {
                if (local_mox && displayduplex)// || (mox && tx_on_vfob))
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                    label_align = tx_display_label_align;
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
                    label_align = tx_display_label_align;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                    sample_rate = sample_rate_rx1;
                }
                f_diff = freq_diff;
            }

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            int filter_low, filter_high;
            int center_line_x;

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

            // Calculate horizontal step size
            while (width / freq_step_size > 10)
            {
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }

            //MW0LGE
            // calculate vertical step size
            int h_steps = (grid_max - grid_min) / grid_step;
            int top;

            if (bIsWaterfall) top = 20; //change top so that the filter gap doesnt change, inore grid spacing
            else top = (int)((double)grid_step * H / y_range); // top is based on grid spacing
            #endregion

            #region RX filter and filter lines
            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            {
                if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX filter
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_sub_rx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }

                if ((bIsWaterfall && m_bShowRXZeroLineOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX 0Hz line
                    int x = (int)((float)(vfoa_sub_hz - vfoa_hz - Low) / width * W);
                    //if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                    //{
                    //    drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, x, H + top, x, H + H, 2);
                    //}
                    //else
                    //{
                    //    drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, x, top, x, H, 2);
                    //}
                    drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, x, nVerticalShift + top, x, nVerticalShift + H, 2);
                }
            }

            if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (!local_mox)// && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    // draw RX filter
                    int filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_display_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }
            }
            #endregion

            #region Tx filter and tx lines
            if ((bIsWaterfall && m_bShowTXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (local_mox && (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
                {
                    // draw TX filter
                    int filter_left_x = (int)((float)(filter_low - Low - f_diff + xit_hz) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low - f_diff + xit_hz) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_tx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }
            }

            if ((!bIsWaterfall || (bIsWaterfall && m_bShowTXFilterOnRXWaterfall)) && //MW0LGE
                !mox && draw_tx_filter &&
                (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            {
                // get tx filter limits
                int filter_left_x;
                int filter_right_x;

                if (tx_on_vfob)
                {
                    if (!split_enabled)
                    {
                        // MW0LGE - f_diff
                        filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
                    }
                    else
                    {
                        filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                        filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
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

                if ((rx == 2 && tx_on_vfob) || (rx == 1 && !(tx_on_vfob && rx2_enabled)))
                {
                    drawLineDX2D(m_bDX2_tx_filter_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, tx_filter_pen.Width);        // draw tx filter overlay
                    drawLineDX2D(m_bDX2_tx_filter_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, tx_filter_pen.Width);	// draw tx filter overlay
                }
            }

            //if (rx == 1 && !local_mox /* && !tx_on_vfob */ && draw_tx_cw_freq &&
            //    (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
            //{
            //    //int pitch = cw_pitch;
            //    //if (rx1_dsp_mode == DSPMode.CWL)
            //    //    pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x, nVerticalShift + top, cw_line_x, nVerticalShift + H, tx_filter_pen.Width); //MW0LGE
            //}

            //if (rx==2 && !local_mox && tx_on_vfob && draw_tx_cw_freq &&
            //    (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
            //{
            //    //int pitch = cw_pitch;
            //    //if (rx2_dsp_mode == DSPMode.CWL)
            //    //    pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x, nVerticalShift + top, cw_line_x, nVerticalShift + H, tx_filter_pen.Width); //MW0LGE
            //}
            #endregion

            #region 60m channels
            // draw 60m channels if in view - not on the waterfall //MW0LGE
            if (!bIsWaterfall && (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK))
            {
                foreach (Channel c in Console.Channels60m)
                {
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;
                    if (local_mox) rit = 0;

                    if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                    {
                        rf_freq = vfob_hz;
                    }

                    if (c.InBW((rf_freq + Low) * 1e-6, (rf_freq + High) * 1e-6)) // is channel visible?
                    {
                        bool on_channel = console.RX1IsIn60mChannel(c); // only true if you are on channel and are in an acceptable mode
                        DSPMode mode = rx1_dsp_mode;

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                        {
                            on_channel = console.RX2IsIn60mChannel(c);
                            mode = rx2_dsp_mode;
                        }

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
                        rf_freq += cwSideToneShift;
                        //if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                        //{
                        //    switch (rx2_dsp_mode)
                        //    {
                        //        case (DSPMode.CWL):
                        //            rf_freq += cw_pitch;
                        //            break;
                        //        case (DSPMode.CWU):
                        //            rf_freq -= cw_pitch;
                        //            break;
                        //    }
                        //}
                        //else
                        //{
                        //    switch (rx1_dsp_mode)
                        //    {
                        //        case (DSPMode.CWL):
                        //            rf_freq += cw_pitch;
                        //            break;
                        //        case (DSPMode.CWU):
                        //            rf_freq -= cw_pitch;
                        //            break;
                        //    }
                        //}

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

                        //MW0LGE
                        drawChannelBarDX2D(c, chan_left_x, chan_right_x, nVerticalShift + top, H - top, c1, c2);
                    }
                }
            }
            #endregion

            #region notches
            // draw notches if in RX
            if (!local_mox && !bIsWaterfall)
            {
                long rf_freq = vfoa_hz;
                int rit = rit_hz;

                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                {
                    rf_freq = vfob_hz;
                }

                rf_freq += cwSideToneShift;

                SharpDX.Direct2D1.Brush p;
                SharpDX.Direct2D1.Brush b;
                SharpDX.Direct2D1.Brush t;

                List<MNotch> notches = MNotchDB.NotchesInBW(rf_freq, Low, High);
                foreach (MNotch n in notches)
                {

                    int notch_centre_x = (int)((float)(n.FCenter - rf_freq - Low - rit) / width * W);
                    int notch_left_x = (int)((float)(n.FCenter - rf_freq - n.FWidth / 2 - Low - rit) / width * W);
                    int notch_right_x = (int)((float)(n.FCenter - rf_freq + n.FWidth / 2 - Low - rit) / width * W);

                    if (tnf_active)
                    {
                        if (n.Active)
                        {
                            p = m_bDX2_m_pNotchActive;
                            b = m_bDX2_m_bBWFillColour;
                        }
                        else
                        {
                            p = m_bDX2_m_pNotchInactive;
                            b = m_bDX2_m_bBWFillColourInactive;
                        }
                    }
                    else
                    {
                        p = m_bDX2_m_pTNFInactive;
                        b = m_bDX2_m_bTNFInactive;
                    }

                    //overide if highlighed
                    if (n == m_objHightlightedNotch)
                    {
                        if (n.Active)
                        {
                            t = m_bDX2_m_bTextCallOutActive;
                        }
                        else
                        {
                            t = m_bDX2_m_bTextCallOutInactive;
                        }
                        p = m_bDX2_m_pHighlighted;
                        b = m_bDX2_m_bBWHighlighedFillColour;

                        //display text callout info 1/4 the way down the notch when being highlighted
                        //TODO: check right edge of screen, put on left of notch if no room
                        String temp_text = (n.FCenter / 1e6).ToString("f6") + "MHz";
                        int nTmp = temp_text.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) + 4;

                        drawStringDX2D("F: " + String.Copy(temp_text.Insert(nTmp, " ")), fontDX2d_callout, t, notch_right_x + 4, nVerticalShift + top + (H / 4));
                        drawStringDX2D("W: " + n.FWidth.ToString("f0") + "Hz", fontDX2d_callout, t, notch_right_x + 4, nVerticalShift + top + (H / 4) + 12);
                    }

                    // the middle notch line
                    drawLineDX2D(p, notch_centre_x, nVerticalShift + top, notch_centre_x, nVerticalShift + H);

                    // only draw area fill if wide enough
                    if (notch_left_x != notch_right_x)
                    {
                        drawFillRectangleDX2D(b, notch_left_x, nVerticalShift + top, notch_right_x - notch_left_x, H - top);
                    }
                }
            }// END NOTCH
            #endregion

            #region CW zero and tx lines
            // Draw a CW Zero Beat + TX line on CW filter
            if (!bIsWaterfall)
            {
                if (show_cwzero_line)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx1_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        //if (!split_enabled)
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);
                        //else
                        //    cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, cw_zero_pen.Width);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx2_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        //if (!split_enabled)
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);
                        //else
                        //    cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, cw_zero_pen.Width);
                    }
                }
                if (draw_tx_cw_freq)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx1_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        //int pitch = cw_pitch;
                        //if (rx2_dsp_mode == DSPMode.CWL)
                        //    pitch = -cw_pitch;

                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }
                }
            }
            #endregion

            //      MW0LGE
            if (local_mox)
                center_line_x = (int)((float)(-f_diff - Low + xit_hz) / width * W); // locked 0 line
            else
                center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line

            // Draw 0Hz vertical line if visible
            if ((!bIsWaterfall && show_zero_line) |
                (bIsWaterfall && ((m_bShowRXZeroLineOnWaterfall & !local_mox) || (m_bShowTXZeroLineOnWaterfall & local_mox)))) // MW0LGE
            {
                if (center_line_x >= 0 && center_line_x <= W)
                {
                    float pw = local_mox ? tx_grid_zero_pen.Width : grid_zero_pen.Width;
                    SharpDX.Direct2D1.Brush pnPen = local_mox ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen;

                    drawLineDX2D(pnPen, center_line_x, nVerticalShift + top, center_line_x, nVerticalShift + H, pw);
                }
            }

            if (show_freq_offset)
            {
                SharpDX.Direct2D1.Brush brBrush = local_mox ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen;

                drawStringDX2D("0", fontDX2d_font9, brBrush, center_line_x - 5, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
            }

            #region Band edges, H+V lines and labels
            //MW0LGE
            int[] band_edge_list;
            switch (console.CurrentRegion)
            {
                case FRSRegion.US:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5330500, 5406400, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Germany:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 51000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region1:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region2:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5351500, 5366500, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Region3:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 3900000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Spain:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 1850000, 3500000, 3800000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
            21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Australia:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000,
             3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000,
             18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.UK:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
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
                case FRSRegion.Japan:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1825000, 1907500, 1912500,
                        3500000, 3575000, 3599000, 3612000, 3680000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                default: // same as region3 but with extended 80m out to 4mhz
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
            }
            //--

            double vfo;

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

            //MW0LGE - TODO CHECK
            vfo += cwSideToneShift;
            //if (rx == 1)
            //{
            //    switch (rx1_dsp_mode)
            //    {
            //        case DSPMode.CWL:
            //            vfo += cw_pitch;
            //            break;
            //        case DSPMode.CWU:
            //            vfo -= cw_pitch;
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //else // if(rx==2)
            //{
            //    switch (rx2_dsp_mode)
            //    {
            //        case DSPMode.CWL:
            //            vfo += cw_pitch;
            //            break;
            //        case DSPMode.CWU:
            //            vfo -= cw_pitch;
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //--

            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            int f_steps = (width / freq_step_size) + 1;


            // Draw vertical lines - band edge markers and freq text
            SharpDX.Direct2D1.Brush pnMajorLine;
            SharpDX.Direct2D1.Brush pnInbetweenLine;
            SharpDX.Direct2D1.Brush brTextBrush;

            for (int i = -1; i < f_steps + 1; i++) // MW0LGE was from i=0, fixes inbetweenies not drawn if major is < 0
            {
                string label;
                int offsetL;
                int offsetR;
                int fgrid = i * freq_step_size + (Low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - Low) / width * W);
                //string freq_num = actual_fgrid.ToString();

                bool bBandEdge = false;

                if (!show_freq_offset)
                {

                    //--------------
                    //MW0LGE
                    for (int ii = 0; ii < band_edge_list.Length; ii++)
                    {
                        if (actual_fgrid == (double)band_edge_list[ii] / 1000000)
                        {
                            bBandEdge = true;
                            break;
                        }
                    }

                    if (bBandEdge)
                    {
                        if (local_mox)
                        {
                            pnMajorLine = m_bDX2_tx_band_edge_pen;
                            pnInbetweenLine = m_bDX2_tx_vgrid_pen_inb;
                            brTextBrush = m_bDX2_tx_band_edge_pen;
                        }
                        else
                        {
                            pnMajorLine = m_bDX2_band_edge_pen;
                            pnInbetweenLine = m_bDX2_grid_pen_inb;
                            brTextBrush = m_bDX2_band_edge_pen;
                        }
                    }
                    else
                    {
                        if (local_mox)
                        {
                            pnMajorLine = m_bDX2_tx_vgrid_pen;
                            pnInbetweenLine = m_bDX2_tx_vgrid_pen_inb;
                            brTextBrush = m_bDX2_grid_tx_text_brush;
                        }
                        else
                        {
                            pnMajorLine = m_bDX2_grid_pen;
                            pnInbetweenLine = m_bDX2_grid_pen_inb;
                            brTextBrush = m_bDX2_grid_text_brush;
                        }
                    }
                    //--

                    //draw vertical in between lines
                    if (grid_control && !bIsWaterfall)
                    {
                        drawLineDX2D(pnMajorLine, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);

                        int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                        int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                        float scale = (float)(x_2 - vgrid) / inbetweenies;

                        for (int j = 1; j < inbetweenies; j++)
                        {
                            float x3 = (float)vgrid + (j * scale);

                            drawLineDX2D(pnInbetweenLine, x3, nVerticalShift + top, x3, nVerticalShift + H);
                        }
                    }

                    if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                    {
                        label = actual_fgrid.ToString("f3");
                        if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                        else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                    }
                    else
                    {
                        //display freqencies
                        //string temp_string;
                        int jper;
                        label = actual_fgrid.ToString("f4");
                        //temp_string = label;
                        jper = label.IndexOf('.') + 4;
                        label = label.Insert(jper, " ");

                        if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                        else offsetL = (int)((label.Length) * 4.1) - 8;
                    }

                    drawStringDX2D(label, fontDX2d_font9, brTextBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    //--------------
                }
                else
                {
                    vgrid = Convert.ToInt32((double)-(fgrid - Low) / (Low - High) * W);	//wa6ahl

                    if (!bIsWaterfall)
                    {
                        SharpDX.Direct2D1.Brush pnPen = local_mox ? m_bDX2_tx_vgrid_pen : m_bDX2_grid_pen;

                        drawLineDX2D(pnPen, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);
                    }

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        SharpDX.Direct2D1.Brush brBrush = local_mox ? m_bDX2_grid_tx_text_brush : m_bDX2_grid_text_brush;

                        drawStringDX2D(label, fontDX2d_font9, brBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    }
                }
            }

            if (!bIsWaterfall)
            {
                // This block of code draws any band edge lines that might not be shown
                // because of the stepped nature of the code above

                //--------------
                //MW0LGE

                if (local_mox)
                {
                    pnMajorLine = m_bDX2_tx_band_edge_pen;
                }
                else
                {
                    pnMajorLine = m_bDX2_band_edge_pen;
                }
                //--

                for (int i = 0; i < band_edge_list.Length; i++)
                {
                    double band_edge_offset = band_edge_list[i] - vfo;
                    if (band_edge_offset >= Low && band_edge_offset <= High)
                    {
                        int temp_vline = (int)((double)(band_edge_offset - Low) / width * W);//wa6ahl

                        //MW0LGE
                        drawLineDX2D(pnMajorLine, temp_vline, nVerticalShift + top, temp_vline, nVerticalShift + H);
                    }
                }
            }
            //--

            if (grid_control && !bIsWaterfall)
            {
                SharpDX.Direct2D1.Brush brTextLabel;

                // highlight the number/scales
                if ((m_bHighlightNumberScaleRX1 && rx == 1) || (m_bHighlightNumberScaleRX2 && rx == 2))
                {
                    if (rx == 1)
                    {
                        drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX1DisplayGridX, nVerticalShift + top, console.RX1DisplayGridW - console.RX1DisplayGridX, H - top);
                    }
                    else
                    {
                        drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX2DisplayGridX, nVerticalShift + top, console.RX2DisplayGridW - console.RX2DisplayGridX, H - top);

                    }
                    brTextLabel = m_bDX2_m_bHightlightNumbers;
                }
                else
                {
                    if (local_mox)
                    {
                        brTextLabel = m_bDX2_grid_tx_text_brush;
                    }
                    else
                    {
                        brTextLabel = m_bDX2_grid_text_brush;
                    }

                }
                // Draw horizontal lines
                int nLeft = 0;
                int nRight = 0;
                int nW = (int)measureStringDX2D("-999", fontDX2d_font9).Width + 12;

                switch (label_align)//MW0LGE display_label_align)
                {
                    case DisplayLabelAlignment.LEFT:
                        nLeft = 0;
                        nRight = nW;
                        break;
                    case DisplayLabelAlignment.CENTER:
                        if (rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
                        {
                            nLeft = center_line_x - nW;
                            nRight = nLeft + nW;
                        }
                        else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
                        {
                            nLeft = center_line_x - nW;
                            nRight = nLeft + nW;
                        }
                        else {
                            nLeft = center_line_x;
                            nRight = nLeft + nW;
                        }
                        break;
                    case DisplayLabelAlignment.RIGHT:
                        nLeft = W - nW;
                        nRight = W;
                        break;
                    case DisplayLabelAlignment.AUTO:
                        nLeft = 0;
                        nRight = nW;
                        break;
                    case DisplayLabelAlignment.OFF:
                        nLeft = W;
                        nRight = W + nW;
                        break;
                }

                for (int i = 1; i < h_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)((double)(grid_max - num) * H / y_range);

                    // MW0LGE
                    //if (local_mox)
                    //{
                        drawLineDX2D(local_mox ? m_bDX2_tx_hgrid_pen : m_bDX2_hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);
                    //}
                    //else
                    //{
                    //    drawLineDX2D(m_bDX2_hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);
                    //}

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        num = grid_max - i * grid_step;
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("0", fontDX2d_font9).Width;// use 0 here instead of a - sign
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (label_align)//MW0LGE display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                if(rx == 1 && (rx1_dsp_mode==DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else x = center_line_x + xOffset;
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
                        //if (x < nTmpMinX) nTmpMinX = x;
                        //if ((int)(x + size.Width) + 10 > nTmpMaxXWidth) nTmpMaxXWidth = (int)(x + size.Width + 10);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            drawStringDX2D(label, fontDX2d_font9, brTextLabel, x, nVerticalShift + y);
                        }
                    }
                }

                // assign back to console so that it knows where we need to be mouse over
                if (rx == 1)
                {
                    console.RX1DisplayGridX = nLeft;
                    console.RX1DisplayGridW = nRight;
                }
                else
                {
                    console.RX2DisplayGridX = nLeft;
                    console.RX2DisplayGridW = nRight;
                }
            }
            #endregion

            #region long cursor and right click overlay
            // draw long cursor & filter overlay
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                bool bShow = false;

                SharpDX.Direct2D1.Brush p;
                // if we are sub tx then the cross will be red
                p = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;

                int y1 = nVerticalShift + top;
                int y2 = H;

                if (rx == 1)
                {
                    if (rx2_enabled)
                    {
                        bShow = (current_display_mode == DisplayMode.PANAFALL) ? display_cursor_y <= 2 * H : display_cursor_y <= H;
                    }
                    else
                    {
                        bShow = true;
                    }
                }
                else
                {
                    bShow = (current_display_mode_bottom == DisplayMode.PANAFALL) ? display_cursor_y > 2 * H : display_cursor_y > H;
                }

                if (bShow)
                {
                    double freq_low = freq + filter_low;
                    double freq_high = freq + filter_high;
                    int x1 = (int)((freq_low - Low) / width * W);
                    int x2 = (int)((freq_high - Low) / width * W);

                    if (ClickTuneFilter)
                    { // only show filter if option set MW0LGE
                        if (((rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU) && rx == 1) || ((rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU) && rx == 2))
                        {
                            drawFillRectangleDX2D(m_bDX2_display_filter_brush, display_cursor_x -
                                ((x2 - x1) / 2), y1, x2 - x1, y2 - top);
                        }
                        else
                        {
                            drawFillRectangleDX2D(m_bDX2_display_filter_brush, x1, y1, x2 - x1, y2 - top);
                        }
                    }

                    drawLineDX2D(p, display_cursor_x, y1 - top, display_cursor_x, (y1 - top) + y2);

                    // draw horiz cursor line
                    if (ShowCTHLine) drawLineDX2D(p, 0, display_cursor_y, W, display_cursor_y);
                }               
            }
            #endregion

            #region F/G/H line and grabs
            // MW0LGE all the code for F/G/H overlay line/grab boxes
            if (!bIsWaterfall && !local_mox)
            {
                //MW0LGE include bottom check
                if (console.PowerOn && (((current_display_mode == DisplayMode.PANADAPTER ||
                    current_display_mode == DisplayMode.PANAFALL ||
                    current_display_mode == DisplayMode.PANASCOPE) && rx == 1)
                    || ((current_display_mode_bottom == DisplayMode.PANADAPTER ||
                    current_display_mode_bottom == DisplayMode.PANAFALL ||
                    current_display_mode_bottom == DisplayMode.PANASCOPE) && rx == 2)))
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

                    if (rx == 1/* && !local_mox*/)
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
                        // get Hang Threshold level
                        double rx1_thresh = 0.0;
                        float rx1_agcknee_y_value = 0.0f;
                        double rx1_hang = 0.0;
                        float rx1_agc_hang_y = 0.0f;
                        unsafe { 
                            WDSP.GetRXAAGCThresh(WDSP.id(0, 0), &rx1_thresh, 4096.0, sample_rate);
                            WDSP.GetRXAAGCHangLevel(WDSP.id(0, 0), &rx1_hang);
                        }
                        rx1_thresh = Math.Round(rx1_thresh);
                        int rx1_agc_fixed_gain = console.SetupForm.AGCFixedGain;
                        string rx1_agc = "";
                        switch (console.RX1AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx1_agcknee_y_value = dBToPixel(-(float)rx1_agc_fixed_gain + rx1_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx1_agc = "-F";
                                break;
                            default:
                                rx1_agcknee_y_value = dBToPixel((float)rx1_thresh + rx1_cal_offset, H);
                                rx1_agc_hang_y = dBToPixel((float)rx1_hang + rx1_cal_offset, H);

                                //if (console.RX2Enabled && CurrentDisplayMode == DisplayMode.PANAFALL)
                                //    rx1_agc_hang_y = rx1_agc_hang_y / 4;
                                //else if (console.RX2Enabled || split_display)
                                //    rx1_agc_hang_y = rx1_agc_hang_y / 2;

                                rx1_agc_hang_y += nVerticalShift;

                                //show hang line
                                if (display_agc_hang_line && console.RX1AGCMode != AGCMode.MED && console.RX1AGCMode != AGCMode.FAST)
                                {
                                    AGCHang.Height = 8; AGCHang.Width = 8; AGCHang.X = 40;
                                    AGCHang.Y = (int)rx1_agc_hang_y - AGCHang.Height;
                                    drawFillRectangleDX2D(m_bDX2_Yellow, AGCHang);
                                    drawLineDX2D(m_bDX2_Yellow, x3_rx1_hang, rx1_agc_hang_y, x2_rx1_hang, rx1_agc_hang_y, m_styleDots);
                                    drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCHang.X + AGCHang.Width, AGCHang.Y - (AGCHang.Height / 2));
                                }
                                rx1_agc = "-G";
                                break;
                        }

                        //if (console.RX2Enabled && CurrentDisplayMode == DisplayMode.PANAFALL)
                        //    rx1_agcknee_y_value = rx1_agcknee_y_value / 4;
                        //else if (console.RX2Enabled || split_display)
                        //    rx1_agcknee_y_value = rx1_agcknee_y_value / 2;

                        rx1_agcknee_y_value += nVerticalShift;

                        // show agc line
                        if (show_agc)
                        {
                            AGCKnee.Height = 8; AGCKnee.Width = 8; AGCKnee.X = 40;
                            AGCKnee.Y = (int)rx1_agcknee_y_value - AGCKnee.Height;
                            drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCKnee);
                            drawLineDX2D(m_bDX2_YellowGreen, x1_rx1_gain, rx1_agcknee_y_value, x2_rx1_gain, rx1_agcknee_y_value, m_styleDots);
                            drawStringDX2D(rx1_agc, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCKnee.X + AGCKnee.Width, AGCKnee.Y - (AGCKnee.Height / 2));
                        }
                    }
                    else// if (rx == 2/* && !local_mox*/)
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
                        unsafe
                        {
                            // get AGC-T level
                            WDSP.GetRXAAGCThresh(WDSP.id(2, 0), &rx2_thresh, 4096.0, sample_rate);
                            rx2_thresh = Math.Round(rx2_thresh);
                            WDSP.GetRXAAGCHangLevel(WDSP.id(2, 0), &rx2_hang);
                            rx2_agc_fixed_gain = console.SetupForm.AGCRX2FixedGain;
                        }
                        switch (console.RX2AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx2_agcknee_y_value = dBToRX2Pixel(-(float)rx2_agc_fixed_gain + rx2_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx2_agc = "-F";
                                break;
                            default:
                                rx2_agcknee_y_value = dBToRX2Pixel((float)rx2_thresh + rx2_cal_offset, H);
                                rx2_agc_hang_y = dBToRX2Pixel((float)rx2_hang + rx2_cal_offset, H);// + rx2_fft_size_offset);  MW0LGE   NOT IN RX1 WHY?  TODO CHECK

                                //if ((console.RX2Enabled || split_display) && current_display_mode_bottom != DisplayMode.PANAFALL)
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 2;
                                //else
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 4;

                                rx2_agc_hang_y += nVerticalShift;

                                if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
                                {
                                    AGCRX2Hang.Height = 8; AGCRX2Hang.Width = 8; AGCRX2Hang.X = 40;

                                    //MW0LGE
                                    //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + 2 * H) - AGCRX2Hang.Height;
                                    //else
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + H) - AGCRX2Hang.Height;
                                    AGCRX2Hang.Y = (int)rx2_agc_hang_y - AGCRX2Hang.Height;

                                    drawFillRectangleDX2D(m_bDX2_Yellow, AGCRX2Hang);
                                    //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                    //{
                                    drawLineDX2D(m_bDX2_Yellow, x3_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, x2_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, m_styleDots);
                                    //}
                                    //else
                                    //{
                                    //    drawLineDX2D(m_bDX2_Yellow, x3_rx2_hang, rx2_agc_hang_y + H, x2_rx2_hang, rx2_agc_hang_y + H, m_styleDots);
                                    //}
                                    drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - (AGCRX2Hang.Height / 2));
                                }
                                rx2_agc = "-G";
                                break;
                        }

                        //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 4;
                        //else
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 2;

                        if (display_rx2_gain_line)
                        {
                            rx2_agcknee_y_value += nVerticalShift;

                            AGCRX2Knee.Height = 8; AGCRX2Knee.Width = 8; AGCRX2Knee.X = 40;

                            //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + 2 * H) - AGCRX2Knee.Height;
                            //else
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + H) - AGCRX2Knee.Height;
                            AGCRX2Knee.Y = (int)rx2_agcknee_y_value - AGCRX2Knee.Height;

                            drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCRX2Knee);
                            //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                            //{
                            drawLineDX2D(m_bDX2_YellowGreen, x1_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, x2_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, m_styleDots);
                            //}
                            //else
                            //{
                            //    drawLineDX2D(m_bDX2_YellowGreen, x1_rx2_gain, rx2_agcknee_y_value + H, x2_rx2_gain, rx2_agcknee_y_value + H, m_styleDots);
                            //}
                            drawStringDX2D(rx2_agc, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Knee.X + AGCRX2Knee.Width, AGCRX2Knee.Y - (AGCRX2Knee.Height / 2));
                        }
                    }
                }
            }
            #endregion

            #region Spots
            // ke9ns add draw DX SPOTS on pandapter
            //=====================================================================
            //=====================================================================

            if (!bIsWaterfall && SpotControl.SP_Active != 0)
            {

                int iii = 0;                          // ke9ns add stairstep holder

                int kk = 0;                           // ke9ns add index for holder[] after you draw the vert line, then draw calls (so calls can overlap the vert lines)

                int vfo_hz = (int)vfoa_hz;    // vfo freq in hz

                int H1a = H / 2;            // length of vertical line (based on rx1 and rx2 display window configuration)
                int H1b = 20;               // starting point of vertical line

                int rxDisplayLow = RXDisplayLow;
                int rxDisplayHigh = RXDisplayHigh;
                SizeF length;

                // RX1/RX2 PanF/Pan = 5,2 (K9,K10)(short)  PanF/PanF = 5,5, (short) Pan/Pan 2,2 (long)
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))                 // if your drawing to the bottom 
                {
                    //if ((K9 == 2) && (K10 == 2)) H1a = H + (H / 2); // long
                    //else H1a = H + (H / 4); // short

                    //H1b = H + 20;

                    vfo_hz = (int)vfob_hz;
                    rxDisplayLow = RX2DisplayLow;
                    rxDisplayHigh = RX2DisplayHigh;

                    Console.DXK2 = 0;        // RX2 index to allow call signs to draw after all the vert lines on the screen
                }
                else
                {
                    Console.DXK = 0;        // RX1 index to allow call signs to draw after all the vert lines on the screen
                }

                VFOLow = vfo_hz + rxDisplayLow;    // low freq (left side) in hz
                VFOHigh = vfo_hz + rxDisplayHigh; // high freq (right side) in hz
                VFODiff = VFOHigh - VFOLow;       // diff in hz

                if ((vfo_hz < 5000000) || ((vfo_hz > 6000000) && (vfo_hz < 8000000))) m_bLSB = true; // LSB
                else m_bLSB = false;     // usb

                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------
                // draw DX spots
                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------

                for (int ii = 0; ii < SpotControl.DX_Index; ii++)     // Index through entire DXspot to find what is on this panadapter (draw vert lines first)
                {

                    if ((SpotControl.DX_Freq[ii] >= VFOLow) && (SpotControl.DX_Freq[ii] <= VFOHigh))
                    {
                        int VFO_DXPos = (int)((((float)W / (float)VFODiff) * (float)(SpotControl.DX_Freq[ii] + cwSideToneShiftInverted - VFOLow))); // determine DX spot line pos on current panadapter screen

                        holder[kk] = ii;                    // ii is the actual DX_INdex pos the the KK holds
                        holder1[kk] = VFO_DXPos;

                        kk++;

                        drawLineDX2D(m_bDX2_p1, VFO_DXPos, H1b + nVerticalShift, VFO_DXPos, H1a + nVerticalShift);   // draw vertical line

                    }

                } // for loop through DX_Index


                int bb = 0;
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
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
                    if (m_bLSB) // 1=LSB so draw on left side of line
                    {

                        if (Console.DisplaySpot) // display Spotted on Pan
                        {
                            length = measureStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

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

                            drawStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);
                        }
                        else // display SPOTTER on Pan (not the Spotted)
                        {
                            length = measureStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

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

                            drawStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50; // allow only 50 qrz spots per Receiver
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
                        if (Console.DisplaySpot) // spot
                        {
                            length = measureStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1); //  not needed here but used for qrz hyperlinking

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

                            drawStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name
                        }
                        else // spotter
                        {
                            length = measureStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1); //  not needed here but used for qrz hyperlinking

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

                            drawStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50;
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
                            drawStringDX2D(SpotControl.DX_Grid[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX grid name
                        }

                    } // USB side

                    iii = iii + 11;
                    if (iii > 90) iii = 0;


                }// for loop through DX_Index

            }
            #endregion
        }
        
        unsafe static private bool DrawSpectrumDX2D(int W, int H, bool bottom)
        {
            DrawSpectrumGridDX2D(W, H, bottom);

            //updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = float.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
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
                    for (int i = 0; i < W/*current_display_data.Length*/; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                }
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                }
                data_ready_bottom = false;
            }

            int Y;
            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2();

            //inital state for X,Y, so we dont get a line from 0,0
            float max;
            if (!bottom)
            {
                max = current_display_data[0];
            }
            else
            {
                max = current_display_data_bottom[0];
            }

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                if (!bottom) max += rx1_display_cal_offset;
                else max += rx2_display_cal_offset;
            }
            else
            {
                max += tx_display_cal_offset;
            }

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                if (!bottom) max += rx1_preamp_offset - alex_preamp_offset;
                else max += rx2_preamp_offset;
            }

            Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
            previousPoint.X = 0;// + 0.5f; // the 0.5f pushes it into the middle of a 'pixel', so that it is not drawn half in one, and half in the other
            previousPoint.Y = Y;// + 0.5f;

            for (int i = 0; i < W; i++)
            {
                if (!bottom)
                {
                    max = current_display_data[i];
                }
                else
                {
                    max = current_display_data_bottom[i];
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (!bottom) max += rx1_display_cal_offset;
                    else max += rx2_display_cal_offset;
                }
                else
                {
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

                Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
                point.X = i;// + 0.5f; // the 0.5f pushes it into the middle of a 'pixel', so that it is not drawn half in one, and half in the other
                point.Y = Y;// + 0.5f;

                d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_data_line_pen_brush, data_line_pen.Width);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            max_y = local_max_y;

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_brush : m_bDX2_Red;
                if (bottom)
                {
                    drawLineDX2D(b, display_cursor_x, H, display_cursor_x, H + H, grid_text_pen.Width);
                    drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
                }
                else
                {
                    drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H, grid_text_pen.Width);
                    drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
                }
            }

            return true;
        }

        private static void DrawSpectrumGridDX2D(int W, int H, bool bottom)
        {
            // draw background
            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);

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
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0;

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
                    int x = W - (int)Math.Floor(i * pixel_step_size);   // for negative numbers

                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, x, H, x, H + H);
                    else drawLineDX2D(m_bDX2_grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)((label.Length + 1) * 4.1);
                    if (x - offset >= 0)
                    {
                        if (bottom) drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, (float)Math.Floor(H * .01));
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

                    if (bottom) drawLineDX2D(m_bDX2_hgrid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("0", fontDX2d_font9).Width;// use 0 here instead of a - sign
                        //int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

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
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);
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
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, H, W - 1, H + H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, H, W - 2, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, 0, W - 1, H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, 0, W - 2, H);
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

                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, x, H, x, H + H);
                    else drawLineDX2D(m_bDX2_grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)(label.Length * 4.1);
                    if (x - offset + label.Length * 7 < W)
                    {
                        if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, (float)Math.Floor(H * .01));
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

                    if (bottom) drawLineDX2D(m_bDX2_hgrid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

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
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);

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
                        drawLineDX2D(m_bDX2_grid_pen, 0, H, 0, H + H);
                        drawLineDX2D(m_bDX2_grid_pen, 1, H, 1, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_pen, 0, 0, 0, H);
                        drawLineDX2D(m_bDX2_grid_pen, 1, 0, 1, H);
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
                        drawLineDX2D(m_bDX2_grid_pen, xLeft, H, xLeft, H + H);		// draw left line
                        drawLineDX2D(m_bDX2_grid_pen, xRight, H, xRight, H + H);		// draw right line
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_pen, xLeft, 0, xLeft, H);		// draw left line
                        drawLineDX2D(m_bDX2_grid_pen, xRight, 0, xRight, H);		// draw right line
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
                            drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, xLeft - offsetL, H + (float)Math.Floor(H * .01));
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, xRight - offsetR, H + (float)Math.Floor(H * .01));
                        }
                        else
                        {
                            drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, xLeft - offsetL, (float)Math.Floor(H * .01));
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, xRight - offsetR, (float)Math.Floor(H * .01));
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
                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_grid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

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

                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);
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
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w, H, mid_w, H + H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w - 1, H, mid_w - 1, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w, 0, mid_w, H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w - 1, 0, mid_w - 1, H);
                    }
            }
        }

        unsafe private static bool DrawScopeDX2D(int W, int H, bool bottom)
        {
            if (scope_min.Length != W)
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }
            if (scope_max.Length != W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }

            DrawScopeGridDX2D(W, H, bottom);

            SharpDX.Vector2 pointMin = new SharpDX.Vector2();
            SharpDX.Vector2 pointMax = new SharpDX.Vector2();
            //SharpDX.Vector2 previousPointMin = new SharpDX.Vector2();
            //SharpDX.Vector2 previousPointMax = new SharpDX.Vector2();


            int pixel = 0;
            int y;
            //// inital state
            //if (bottom) pixel = (int)(H / 2 * scope_max[0]);
            //else pixel = (int)(H / 2 * scope_max[0]);
            //y = H / 2 - pixel;
            //previousPointMax.X = 0;
            //previousPointMax.Y = y;
            //if (bottom) previousPointMax.Y += H;

            //if (bottom) pixel = (int)(H / 2 * scope_min[0]);
            //else pixel = (int)(H / 2 * scope_min[0]);
            //y = H / 2 - pixel;
            //previousPointMin.X = 0;
            //previousPointMin.Y = y;
            //if (bottom) previousPointMin.Y += H;

            // the 0.5f's to move to centre pixel
            for (int i = 0; i < W; i++)						// fill point array
            {
                /*if (bottom) pixel = (int)(H / 2 * scope_max[i]);
                else */pixel = (int)(H / 2 * scope_max[i]);
                y = H / 2 - pixel;
                pointMax.X = i;// + 0.5f;
                pointMax.Y = y;// + 0.5f;
                if (bottom) pointMax.Y += H;

                /*if (bottom) pixel = (int)(H / 2 * scope_min[i]);
                else */pixel = (int)(H / 2 * scope_min[i]);
                y = H / 2 - pixel;
                pointMin.X = i;// + 0.5f;
                pointMin.Y = y;// + 0.5f;
                if (bottom) pointMin.Y += H;

                d2dRenderTarget.DrawLine(pointMin, pointMax, m_bDX2_data_line_pen_brush);

                // any real point in drawing these outlines for top/bottom?
                //d2dRenderTarget.DrawLine(previousPointMin, pointMin, m_bDX2_data_line_pen_brush);
                //previousPointMin.X = i;
                //previousPointMin.Y = pointMin.Y;

                //d2dRenderTarget.DrawLine(previousPointMax, pointMax, m_bDX2_data_line_pen_brush);
                //previousPointMax.X = i;
                //previousPointMax.Y = pointMax.Y;
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        private static void DrawScopeGridDX2D(int W, int H, bool bottom)
        {
            // draw background
            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);
        }

        unsafe private static bool DrawScope2DX2D(int W, int H, bool bottom)
        {
            if (scope_min.Length != W)
            {
                scope_min = new float[W];
                Audio.ScopeMin = scope_min;
            }

            if (scope_max.Length != W)
            {
                scope_max = new float[W];
                Audio.ScopeMax = scope_max;
            }

            if (scope2_min.Length != W)
            {
                scope2_min = new float[W];
                Audio.Scope2Min = scope2_max;
            }
            if (scope2_max.Length != W)
            {
                scope2_max = new float[W];
                Audio.Scope2Max = scope2_max;
            }

            int y1 = (int)(H * 0.25f);
            int y2 = (int)(H * 0.5f);
            int y3 = (int)(H * 0.75f);
            drawLineDX2D(m_bDX2_y1_brush, 0, y1, W, y1);
            drawLineDX2D(m_bDX2_y2_brush, 0, y2, W, y2);
            drawLineDX2D(m_bDX2_y1_brush, 0, y3, W, y3);

            int samples = W;// scope2_max.Length;   //MW0LGE blimey
            float xScale = (float)samples / W;
            float yScale = (float)H / 4;

            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2();

            // the 0.5f's to move to middle pixel
            // draw the left input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y1 - (scope2_max[0] * yScale));

            for (int x = 0; x < W; x++)
            {
                int i = (int)Math.Truncate((float)x * xScale);
                int y = (int)(y1 - (scope2_max[i] * yScale));
                point.X = x;// + 0.5f;
                point.Y = y;// + 0.5f;

                d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_waveform_line_pen);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            // draw the right input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y3 - (scope_max[0] * yScale));
            for (int x = 0; x < W; x++)
            {
                int i = (int)Math.Truncate((float)x * xScale);
                int y = (int)(y3 - (scope_max[i] * yScale));
                point.X = x;// + 0.5f;
                point.Y = y;// + 0.5f;

                d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_waveform_line_pen);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            return true;
        }

        unsafe private static bool DrawPhaseDX2D(int W, int H, bool bottom)
        {
            DrawPhaseGridDX2D(W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                data_ready_bottom = false;
            }

            int nShift = m_nPhasePointSize / 2;

            SharpDX.Vector2 point = new SharpDX.Vector2();

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
                point.X = W / 2 + x;
                point.Y = H / 2 + y;
                if (bottom) point.Y += H;

                drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, point.X - nShift, point.Y - nShift, m_nPhasePointSize, m_nPhasePointSize);
            }
            
            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        private static void DrawPhaseGridDX2D(int W, int H, bool bottom)
        {
            // draw background
            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);

            for (double i = 0.50; i < 3; i += .50)	// draw 3 concentric circles
            {
                if (bottom) drawElipseDX2D(m_bDX2_grid_pen, (int)(W / 2), H + (int)(H / 2), (int)(H * i), (int)(H * i));
                else drawElipseDX2D(m_bDX2_grid_pen, (int)(W / 2), (int)(H / 2), (int)(H * i), (int)(H * i));
            }
        }

        unsafe private static void DrawPhase2DX2D(int W, int H, bool bottom)
        {
            DrawPhaseGridDX2D(W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));
                data_ready_bottom = false;
            }

            int nShift = m_nPhasePointSize / 2;

            SharpDX.Vector2 point = new SharpDX.Vector2();

            for (int i = 0; i < num_points; i++)
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
                point.X = (int)(W * 0.5 + x);
                point.Y = (int)(H * 0.5 + y);
                if (bottom) point.Y += H;

                drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, point.X - nShift, point.Y - nShift, m_nPhasePointSize, m_nPhasePointSize);
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }
        }

        unsafe static private bool DrawHistogramDX2D(int W, int H)
        {
            DrawSpectrumGridDX2D(W, H, false);

            updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = Int32.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int yRange = grid_max - grid_min;
            if (data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/W * sizeof(float));

                data_ready = false;
            }

            int sum = 0;

            for (int i = 0; i < W; i++)
            {
                float max = max = current_display_data[i];

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
                points[i].Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);

                sum += points[i].Y;
            }

            max_y = local_max_y;

            // get the average
            float avg = 0.0F;            
            avg = (float)((float)sum / W / 1.12);

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

                    m_bDX2_dhp.Opacity = alpha / 255f;
                    drawFillRectangleDX2D(m_bDX2_dhp, i, histogram_data[i], 1, height);
                }
                if (points[i].Y >= avg)		// value is below the average
                {
                    drawFillRectangleDX2D(m_bDX2_dhp1, points[i].X, points[i].Y, 1, H - points[i].Y);
                }
                else
                {
                    drawFillRectangleDX2D(m_bDX2_dhp1, points[i].X, (int)Math.Floor(avg), 1, H - (int)Math.Floor(avg));
                    drawFillRectangleDX2D(m_bDX2_dhp2, points[i].X, points[i].Y, 1, (int)Math.Floor(avg) - points[i].Y);
                }
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

    }
}
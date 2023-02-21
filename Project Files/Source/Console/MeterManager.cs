using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;

//directX
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace Thetis
{
    // this enum is similar to MeterRXMode & MeterTXMode
    public enum Reading
    {
        NONE = -1,
        //RX
        SIGNAL_STRENGTH = 0,
        AVG_SIGNAL_STRENGTH,
        ADC_PK,
        ADC_AV,
        AGC_PK,
        AGC_AV,
        AGC_GAIN,
        //TX
        MIC,
        PWR,
        ALC,
        EQ,
        LEVELER,
        COMP,
        CPDR,
        ALC_G,
        ALC_GROUP,
        LVL_G,
        MIC_PK,
        ALC_PK,
        EQ_PK,
        LEVELER_PK,
        COMP_PK,
        CPDR_PK,
        CFC_PK,
        CFC_G,
        //additional to MeterRXMode & MeterTXMode
        REVERSE_PWR,
        SWR,
        //pa
        DRIVE_FWD_ADC,
        FWD_ADC,
        REV_ADC,
        DRIVE_PWR,
        PA_FWD_PWR,
        PA_REV_PWR,
        CAL_FWD_PWR,
        REV_VOLT,
        FWD_VOLT,
        // volts/amps
        VOLTS,
        AMPS,
        ESTIMATED_PBSNR,
        EYE_PERCENT,
        LAST
    }

    public enum MeterType
    {
        NONE = 0,
        //rx
        SIGNAL_STRENGTH,
        AVG_SIGNAL_STRENGTH,
        ADC,
        AGC,
        AGC_GAIN,
        ESTIMATED_PBSNR,
        //tx
        MIC,
        EQ,
        LEVELER,
        LEVELER_GAIN,
        ALC,
        ALC_GAIN,
        ALC_GROUP,
        CFC,
        COMP,
        //--
        PWR,
        REVERSE_PWR,
        SWR,
        //CPDR, //not used
        //special
        MAGIC_EYE,
        KENWOOD,
        CROSS,
        LAST
    }    
    internal static class MeterManager
    {
        #region MeterManager
        // member variables
        private static Console _console;
        private static bool _delegatesAdded;
        private static Dictionary<int, clsReadings> _readings;
        private static Dictionary<string, clsMeter> _meters;
        private static Thread _meterThread;
        private static bool _meterThreadRunning;
        //private static object _readingsLock = new object();
        private static bool _power;
        private static bool _rx1VHForAbove;
        private static bool _rx2VHForAbove;
        private static HPSDRModel _currentHPSDRmodel;
        private static bool _alexPresent;
        private static bool _paPresent;
        private static bool _apolloPresent;
        private static int _transverterIndex;
        private static string _imagePath;

        private static Object _imageLock = new Object();

        private static Dictionary<string, DataStream> _pooledStreamData;
        private static Dictionary<string, System.Drawing.Bitmap> _pooledImages;        

        static MeterManager()
        {
            // static constructor
            _rx1VHForAbove = false;
            _rx2VHForAbove = false;
            _delegatesAdded = false;
            _console = null;
            _readings = new Dictionary<int, clsReadings>();
            _meters = new Dictionary<string, clsMeter>();

            _currentHPSDRmodel = HPSDRModel.HERMES;
            _alexPresent = false;
            _paPresent = false;
            _apolloPresent = false;
            _transverterIndex = -1; // no transverter
            _imagePath = "";

            _pooledImages = new Dictionary<string, System.Drawing.Bitmap>();
            _pooledStreamData = new Dictionary<string, DataStream>();

            // two sets of readings, for each trx
            _readings.Add(1, new clsReadings());
            _readings.Add(2, new clsReadings());

            _meterThreadRunning = false;
        }
        public static string MeterName(MeterType meter)
        {
            switch (meter)
            {
                case MeterType.SIGNAL_STRENGTH: return "Signal Strength";
                case MeterType.AVG_SIGNAL_STRENGTH: return "Average Signal Strength";
                case MeterType.ADC: return "ADC";
                case MeterType.AGC: return "AGC";
                case MeterType.AGC_GAIN: return "AGC Gain";
                case MeterType.MIC: return "Mic";
                case MeterType.PWR: return "ForwardPower";
                case MeterType.REVERSE_PWR: return "Reverse Power";
                case MeterType.ALC: return "ALC";
                case MeterType.EQ: return "EQ";
                case MeterType.LEVELER: return "Leveler";
                case MeterType.COMP: return "Compression";
                //case MeterType.CPDR: return "TODO Compander";
                case MeterType.ALC_GAIN: return "ALC Compression";
                case MeterType.ALC_GROUP: return "ALC Group";
                case MeterType.LEVELER_GAIN: return "Leveler Gain";
                case MeterType.CFC: return "CFC Compression";
                case MeterType.MAGIC_EYE: return "Magic Eye";
                case MeterType.ESTIMATED_PBSNR: return "Estimated Passband SNR";
                case MeterType.KENWOOD: return "Kenwood Meter";
                case MeterType.CROSS: return "Cross Meter";
            }

            return meter.ToString();
        }
        public static string ReadingName(Reading reading)
        {
            switch (reading)
            {
                case Reading.ADC_AV: return "ADC Average";
                case Reading.ADC_PK: return "ADC Peak";
                case Reading.AGC_AV: return "AGC Average";
                case Reading.AGC_PK: return "AGC Peak";
                case Reading.AGC_GAIN: return "AGC Gain";
                case Reading.EYE_PERCENT: return "Magic Eye";
                case Reading.ALC: return "ALC";
                case Reading.ALC_G: return "ALC Compression";
                case Reading.ALC_GROUP: return "ALC Group";
                case Reading.ESTIMATED_PBSNR: return "Estimated PBSNR";
                case Reading.ALC_PK: return "ALC";// Peak";
                case Reading.AMPS: return "Amps";
                case Reading.AVG_SIGNAL_STRENGTH: return "Signal Average";
                case Reading.CAL_FWD_PWR: return "Calibrated FWD Power";
                case Reading.CFC_G: return "CFC Compression";
                case Reading.CFC_PK: return "CFC Compression";// Peak";
                case Reading.COMP: return "Compression";
                case Reading.COMP_PK: return "Compression";// Peak";
                //case Reading.CPDR: return "TODO Compander";
                //case Reading.CPDR_PK: return "Compander Peak";
                case Reading.DRIVE_FWD_ADC: return "Drive Forward ADC";
                case Reading.DRIVE_PWR: return "Drive Power";
                case Reading.EQ: return "EQ";
                case Reading.EQ_PK: return "EQ";// Peak";
                case Reading.FWD_ADC: return "Forward ADC";
                case Reading.FWD_VOLT: return "Forward Volt";
                case Reading.LEVELER: return "Leveler";
                case Reading.LEVELER_PK: return "Leveler";// Peak";
                case Reading.LVL_G: return "Leveler Gain";
                case Reading.MIC: return "MIC";
                case Reading.MIC_PK: return "MIC";// Peak";
                case Reading.PA_FWD_PWR: return "PA Forward Power";
                case Reading.PA_REV_PWR: return "PA Reverse Power";
                case Reading.PWR: return "Power";
                case Reading.REVERSE_PWR: return "Reverse Power";
                case Reading.REV_ADC: return "Reverse ADC";
                case Reading.REV_VOLT: return "Reverse VOLT";
                case Reading.SIGNAL_STRENGTH: return "Signal";
                case Reading.SWR: return "SWR";
                case Reading.VOLTS: return "Volts";
            }

            return reading.ToString();
        }
        public static string ReadingUnits(Reading reading)
        {
            switch (reading)
            {
                case Reading.ADC_AV: return "dBFS";
                case Reading.ADC_PK: return "dBFS";
                case Reading.AGC_PK: return "dB";
                case Reading.AGC_AV: return "dB";
                case Reading.AGC_GAIN: return "dB";
                case Reading.EYE_PERCENT: return "?";
                case Reading.ALC: return "dB";
                case Reading.ALC_G: return "dB";
                case Reading.ALC_GROUP: return "dB";
                case Reading.ESTIMATED_PBSNR: return "dB";
                case Reading.ALC_PK: return "dB";
                case Reading.AMPS: return "A";
                case Reading.AVG_SIGNAL_STRENGTH: return "dBm";
                case Reading.CAL_FWD_PWR: return "W";
                case Reading.CFC_G: return "dB";
                case Reading.CFC_PK: return "dB";
                case Reading.COMP: return "dB";
                case Reading.COMP_PK: return "dB";
                //case Reading.CPDR: return "dB";
                //case Reading.CPDR_PK: return "dB";
                case Reading.DRIVE_FWD_ADC: return "?";
                case Reading.DRIVE_PWR: return "W";
                case Reading.EQ: return "dB";
                case Reading.EQ_PK: return "dB";
                case Reading.FWD_ADC: return "FWD ADC";
                case Reading.FWD_VOLT: return "?";
                case Reading.LEVELER: return "dB";
                case Reading.LEVELER_PK: return "dB";
                case Reading.LVL_G: return "dB";
                case Reading.MIC: return "dB";
                case Reading.MIC_PK: return "dB";
                case Reading.PA_FWD_PWR: return "W";
                case Reading.PA_REV_PWR: return "W";
                case Reading.PWR: return "W";
                case Reading.REVERSE_PWR: return "W";
                case Reading.REV_ADC: return "?";
                case Reading.REV_VOLT: return "V";
                case Reading.SIGNAL_STRENGTH: return "dBm";
                case Reading.SWR: return ":1";
                case Reading.VOLTS: return "V";
            }

            return reading.ToString();
        }

        private static void UpdateMeters()
        {
            List<Reading> readingsUsed = new List<Reading>();

            _meterThreadRunning = true;
            while (_meterThreadRunning)
            {
                if (_power)
                {
                    int nDelay = 5000;

                    // udpate any meter
                    foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                    {
                        readingsUsed.Clear();

                        clsMeter m = kvp.Value;

                        m.Update(ref readingsUsed);

                        int nTmp = m.DelayForUpdate();
                        if (nTmp < nDelay) nDelay = nTmp;

                        //lock (_readingsLock)
                        {
                            // use/invalidate any readings that have been used, so that they can be updated
                            foreach (Reading rt in readingsUsed)
                            {
                                _readings[m.RX].UseReading(rt);
                            }
                        }
                    }

                    Thread.Sleep(nDelay);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }              
        public static void AddImage(string sKey, System.Drawing.Bitmap image)
        {
            lock (_imageLock)
            {
                if (_pooledImages == null) return;

                _pooledImages.Add(sKey, image);
            }
        }
        public static System.Drawing.Bitmap GetImage(string sKey)
        {
            lock (_imageLock)
            {
                if (_pooledImages == null) return null;
                if (!_pooledImages.ContainsKey(sKey)) return null;

                return _pooledImages[sKey];
            }
        }
        public static bool ContainsBitmap(string sKey)
        {
            lock (_imageLock)
            {
                if (_pooledImages == null) return false;
                return _pooledImages.ContainsKey(sKey);
            }
        }
        public static void AddStreamData(string sId, DataStream tempStream)
        {
            lock (_imageLock)
            {
                if (_pooledStreamData == null) return;

                _pooledStreamData.Add(sId, tempStream);
            }
        }
        public static DataStream GetStreamData(string sKey)
        {
            lock (_imageLock)
            {
                if (_pooledStreamData == null) return null;
                if (!_pooledStreamData.ContainsKey(sKey)) return null;

                return _pooledStreamData[sKey];
            }
        }
        public static bool ContainsStreamData(string sKey)
        {
            lock (_imageLock)
            {
                if (_pooledStreamData == null) return false;
                return _pooledStreamData.ContainsKey(sKey);
            }
        }
        public static void HighlightContainer(string sId)
        {
            foreach(KeyValuePair<string, DXRenderer> kvp in _DXrenderers)
            {
                kvp.Value.HighlightEdge = false;
            }

            if (sId != "" && _DXrenderers.ContainsKey(sId))
            {
                _DXrenderers[sId].HighlightEdge = true;
            }
        }
        public static void DisposeImageData()
        {
            foreach (KeyValuePair<string, DataStream> kvp in _pooledStreamData)
            {
                DataStream tempStream = kvp.Value;
                Utilities.Dispose(ref tempStream);
                tempStream = null;
            }
            _pooledStreamData.Clear();

            foreach (KeyValuePair<string, System.Drawing.Bitmap> kvp in _pooledImages)
            {
                System.Drawing.Bitmap tempBmp = kvp.Value;
                tempBmp.Dispose();
            }
            _pooledImages.Clear();
        }
        public static void Init(Console c, string sImagePath = "")
        {
            _console = c;
            _power = _console.PowerOn;
            _rx1VHForAbove = _console.VFOAFreq >= 30;
            _rx2VHForAbove = _console.RX2Enabled && _console.VFOBFreq >= 30;
            _currentHPSDRmodel = _console.CurrentHPSDRModel;
            _apolloPresent = _console.ApolloPresent;
            _alexPresent = _console.AlexPresent;
            _paPresent = _console.PAPresent;
            _transverterIndex = _console.TXXVTRIndex;
            _imagePath = sImagePath;

            addDelegates();                       

            _meterThread = new Thread(new ThreadStart(UpdateMeters))
            {
                Name = "Multimeter Thread",
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            _meterThread.Start();
        }
        public static string ImagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }
        private static Dictionary<string, DXRenderer> _DXrenderers = new Dictionary<string, DXRenderer>();
        private static void addRenderer(string sId, int rx, PictureBox target, clsMeter meter)
        {
            DXRenderer renderer = new DXRenderer(sId, rx, target, _console, MeterManager.ImagePath, meter);            

            _DXrenderers.Add(sId, renderer);
        }
        private static void removeRenderer(string sId)
        {
            if (!_DXrenderers.ContainsKey(sId)) return;

            DXRenderer r = _DXrenderers[sId];
            r.ShutdownDX();

            _DXrenderers.Remove(sId);
        }
        public static void Shutdown()
        {
            removeDelegates();

            foreach (KeyValuePair<string, DXRenderer> kvp in _DXrenderers)
            {
                kvp.Value.ShutdownDX();
            }

            DisposeImageData();           
            
            _meterThreadRunning = false;
            if (_meterThread != null && _meterThread.IsAlive) _meterThread.Join();

            foreach (KeyValuePair<string, frmMeterDisplay> kvp in _lstMeterDisplayForms)
            {
                kvp.Value.Close();
            }
        }
        private static void addDelegates()
        {
            if (_delegatesAdded || _console == null) return;

            _console.MeterReadingsChangedHandlers += OnMeterReadings;
            _console.MoxChangeHandlers += OnMox;
            _console.PowerChangeHanders += OnPower;
            _console.VFOAFrequencyChangeHandlers += OnVFOA;
            _console.VFOBFrequencyChangeHandlers += OnVFOB;

            _console.AlexPresentChangedHandlers += OnAlexPresentChanged;
            _console.PAPresentChangedHandlers += OnPAPresentChanged;
            _console.ApolloPresentChangedHandlers += OnApolloPresentChanged;
            _console.CurrentModelChangedHandlers += OnCurrentModelChanged;
            _console.TransverterIndexChangedHandlers += OnTransverterIndexChanged;

            _console.RX2EnabledChangedHandlers += OnRX2EnabledChanged;
            _console.RX2EnabledPreChangedHandlers += OnRX2EnabledPreChanged;

            _delegatesAdded = true;
        }
        private static void removeDelegates()
        {
            if (!_delegatesAdded || _console == null) return;

            _console.MeterReadingsChangedHandlers -= OnMeterReadings;
            _console.MoxChangeHandlers -= OnMox;
            _console.PowerChangeHanders -= OnPower;
            _console.VFOAFrequencyChangeHandlers -= OnVFOA;
            _console.VFOBFrequencyChangeHandlers -= OnVFOB;

            _console.AlexPresentChangedHandlers -= OnAlexPresentChanged;
            _console.PAPresentChangedHandlers -= OnPAPresentChanged;
            _console.ApolloPresentChangedHandlers -= OnApolloPresentChanged;
            _console.CurrentModelChangedHandlers -= OnCurrentModelChanged;
            _console.TransverterIndexChangedHandlers -= OnTransverterIndexChanged;

            _console.RX2EnabledChangedHandlers -= OnRX2EnabledChanged;
            _console.RX2EnabledPreChangedHandlers -= OnRX2EnabledPreChanged;

            foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
            {
                kvp.Value.RemoveDelegates();
            }

            _delegatesAdded = false;
        }
        private static void OnTransverterIndexChanged(int oldIndex, int newIndex)
        {
            _transverterIndex = newIndex;
        }
        public static void OnAlexPresentChanged(bool oldSetting, bool newSetting)
        {
            _alexPresent = newSetting;
        }
        public static void OnPAPresentChanged(bool oldSetting, bool newSetting)
        {
            _paPresent = newSetting;
        }
        public static void OnApolloPresentChanged(bool oldSetting, bool newSetting)
        {
            _apolloPresent = newSetting;
        }
        public static void OnCurrentModelChanged(HPSDRModel oldModel, HPSDRModel newModel)
        {
            _currentHPSDRmodel = newModel;
        }
        private static void OnVFOA(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            _rx1VHForAbove = _console.VFOAFreq >= 30;
        }
        private static void OnVFOB(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            _rx2VHForAbove = _console.RX2Enabled && _console.VFOBFreq >= 30;
        }
        private static void OnPower(bool oldPower, bool newPower)
        {
            _power = newPower;
        }
        private static void OnMox(int rx, bool oldMox, bool newMox)
        {
            foreach (KeyValuePair<string, clsMeter> mkvp in _meters)
            {
                clsMeter m = mkvp.Value;
                m.MOX = rx == m.RX && newMox;
            }

            if(oldMox && !newMox)
            {
                // tx to rx
                // set tx values to min as now not getting any readings
                foreach(KeyValuePair<string, clsMeter> ms in _meters.Where(o => o.Value.RX == rx))
                {
                    clsMeter m = ms.Value;

                    m.ZeroOut(true);
                }
            }
            else if(!oldMox && newMox)
            {
                // rx to tx
                // set rx values to min as now not getting any readings
                foreach (KeyValuePair<string, clsMeter> ms in _meters.Where(o => o.Value.RX == rx))
                {
                    clsMeter m = ms.Value;

                    m.ZeroOut(false);
                }
            }
        }
        public static int CurrentPowerRating
        {
            get
            {
                // power choice based on console.getMeterPixelPosAndDrawScales
                // TODO: 8000mk2  !!!!!!!!!!!!!!!!!!!!

                int nWatts = 500;

                if (_alexPresent && 
                    ((_currentHPSDRmodel == HPSDRModel.ORIONMKII || _currentHPSDRmodel == HPSDRModel.ANAN8000D) 
                    && _transverterIndex < 0))
                {
                    nWatts = 200;
                }
                else if ((_alexPresent || _paPresent) &&
                                (_currentHPSDRmodel != HPSDRModel.ANAN10 &&
                                 _currentHPSDRmodel != HPSDRModel.ANAN10E &&
                                !_apolloPresent))
                {
                    nWatts = 100;
                }
                else if (_currentHPSDRmodel == HPSDRModel.ANAN10 ||
                                _currentHPSDRmodel == HPSDRModel.ANAN10E)
                {
                    nWatts = 30;
                }
                else if (_apolloPresent)
                {
                    nWatts = 15;
                }
                else
                {
                    nWatts = 1;
                }
                
                return nWatts;
            }
        }
        private static float dbmOffsetForAbove30(int rx)
        {
            if ((rx == 1 && _rx1VHForAbove) || (rx == 2 && _rx2VHForAbove))
                return 20f;
            else
                return 0f;
        }
        private static float normaliseTo100W()
        {
            // return a factor to apply to power values to bring them to 100w
            // this is needed because power meter scaling is based on 100w at full deflection
            switch(CurrentPowerRating)
            {
                case 500: return 1 / 5f;
                case 200: return 1 / 2f;
                case 100: return 1f;
                case 30: return 100 / 30f;
                case 15: return 100 / 15f;
                case 1: return 100f;
            }

            return 1f;
        }
        private static float getReading(int rx, Reading rt, bool bUseReading = false)
        {
            if (rt == Reading.NONE) return -200f;

            //lock (_readingsLock)
            {
                return _readings[rx].GetReading(rt, bUseReading);
            }
        }
        private static void setReading(int rx, Reading rt, ref Dictionary<Reading, float> readings)
        {
            // locked outside
            if (_readings[rx].RequiresUpdate(rt)) _readings[rx].SetReading(rt, readings[rt]);
        }
        private static void setReading(int rx, Reading rt, float reading)
        {
            //lock (_readingsLock)
            {
                if (_readings[rx].RequiresUpdate(rt)) _readings[rx].SetReading(rt, reading);
            }
        }
        private static void OnMeterReadings(int rx, bool mox, ref Dictionary<Reading, float> readings)
        {
            //lock (_readingsLock)
            {
                //
                if (!mox)
                {
                    setReading(rx, Reading.SIGNAL_STRENGTH, ref readings);
                    setReading(rx, Reading.AVG_SIGNAL_STRENGTH, ref readings);
                    setReading(rx, Reading.ADC_PK, ref readings);
                    setReading(rx, Reading.ADC_AV, ref readings);
                    setReading(rx, Reading.AGC_PK, ref readings);
                    setReading(rx, Reading.AGC_AV, ref readings);
                    setReading(rx, Reading.AGC_GAIN, ref readings);
                    setReading(rx, Reading.EYE_PERCENT, ref readings);

                    setReading(rx, Reading.ESTIMATED_PBSNR, ref readings);
                }
                else
                {
                    setReading(rx, Reading.MIC, ref readings);
                    setReading(rx, Reading.MIC_PK, ref readings);
                    setReading(rx, Reading.EQ_PK, ref readings);
                    setReading(rx, Reading.EQ, ref readings);
                    setReading(rx, Reading.LEVELER_PK, ref readings);
                    setReading(rx, Reading.LEVELER, ref readings);
                    setReading(rx, Reading.LVL_G, ref readings);
                    setReading(rx, Reading.CFC_PK, ref readings);
                    setReading(rx, Reading.CFC_G, ref readings);
                    //setReading(rx, Reading.CPDR_PK, ref readings);
                    //setReading(rx, Reading.CPDR, ref readings);

                    setReading(rx, Reading.ALC_PK, ref readings);
                    setReading(rx, Reading.ALC, ref readings);
                    setReading(rx, Reading.ALC_G, ref readings);

                    setReading(rx, Reading.COMP, ref readings);
                    setReading(rx, Reading.COMP_PK, ref readings);
                    setReading(rx, Reading.ALC_GROUP, ref readings);

                    setReading(rx, Reading.PWR, ref readings);
                    setReading(rx, Reading.REVERSE_PWR, ref readings);
                    setReading(rx, Reading.SWR, ref readings);

                    if (rx == 1)
                    { // only rx1 data
                        setReading(rx, Reading.DRIVE_FWD_ADC, ref readings);
                        setReading(rx, Reading.FWD_ADC, ref readings);
                        setReading(rx, Reading.REV_ADC, ref readings);
                        setReading(rx, Reading.DRIVE_PWR, ref readings);
                        setReading(rx, Reading.PA_FWD_PWR, ref readings);
                        setReading(rx, Reading.PA_REV_PWR, ref readings);
                        setReading(rx, Reading.CAL_FWD_PWR, ref readings);
                    }
                }
                setReading(rx, Reading.VOLTS, ref readings);
                setReading(rx, Reading.AMPS, ref readings);                
            }
        }
        public static bool RequiresUpdate(int rx, Reading rt)
        {
            //lock (_readingsLock)
            {
                return _readings[rx].RequiresUpdate(rt);
            }
        }

        // meters
        public static int QuickestUpdateInterval(int rx, bool mox)
        {
            int updateRate = 5000;

            foreach (KeyValuePair<string, clsMeter> kvp in _meters.Where(kvp => kvp.Value.RX == rx))
            {
                int ui = kvp.Value.QuickestUpdateInterval(mox);
                if (ui < updateRate) updateRate = ui;
            }

            return updateRate;
        }

        private static Dictionary<string, frmMeterDisplay> _lstMeterDisplayForms = new Dictionary<string, frmMeterDisplay>();
        private static Dictionary<string, ucMeter> _lstUCMeters = new Dictionary<string, ucMeter>();
        public static clsMeter MeterFromId(string sId)
        {
            if (_meters == null) return null;
            if (!_meters.ContainsKey(sId)) return null;

            return _meters[sId];
        }
        public static void AddMeterContainer(ucMeter ucM, bool mox)
        {
            if (_console == null) return;
            ucM.Console = _console;
            ucM.FloatingDockedClicked += ucMeter_FloatingDockedClicked;
            ucM.DockedMoved += ucMeter_FloatingDockedMoved;

            // float form
            frmMeterDisplay f = new frmMeterDisplay(_console, ucM.RX);
            f.ID = ucM.ID;            

            // meter items
            clsMeter meter = new clsMeter(ucM.RX, "", 1f, 1f, mox);
            meter.ID = ucM.ID;

            // a renderer
            addRenderer(ucM.ID, ucM.RX, ucM.DisplayContainer, meter);

            // store everything
            _lstMeterDisplayForms.Add(f.ID, f);
            _lstUCMeters.Add(ucM.ID, ucM);
            _meters.Add(meter.ID, meter);

            // setup
            if (ucM.Floating)
            {
                setMeterFloating(ucM, f);
            }
            else
                returnMeterFromFloating(ucM, f);
        }

        public static string AddMeterContainer(int nRx, bool bFloating, bool mox)
        {
            ucMeter ucM = new ucMeter();
            ucM.RX = nRx;
            ucM.Floating = bFloating;

            AddMeterContainer(ucM, mox);

            return ucM.ID;
        }
        public static int TotalMeterContainers
        {
            get { return _lstUCMeters.Count; }
        }
        private static void ucMeter_FloatingDockedClicked(object sender, EventArgs e)
        {
            ucMeter ucM = (ucMeter)sender;

            if (!_lstMeterDisplayForms.ContainsKey(ucM.ID)) return;

            if (ucM.Floating)
                returnMeterFromFloating(ucM, _lstMeterDisplayForms[ucM.ID]);
            else
                setMeterFloating(ucM, _lstMeterDisplayForms[ucM.ID]);
        }
        private static void ucMeter_FloatingDockedMoved(object sender, EventArgs e)
        {
            ucMeter ucM = (ucMeter)sender;

            ucM.Delta = new System.Drawing.Point(_console.HDelta, _console.VDelta);
        }
        public static void SetPositionOfDockedMeters()
        {
            foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
            {
                setPoisitionOfDockedMeter(kvp.Value);
            }
        }
        private static void setPoisitionOfDockedMeter(ucMeter m)
        {
            if (_console == null) return;

            int HDelta = _console.HDelta;
            int VDelta = _console.VDelta;

            if (!m.Floating)
            {
                System.Drawing.Point newLocation = new System.Drawing.Point();

                switch (m.AxisLock)
                {
                    case Axis.RIGHT:
                    case Axis.BOTTOMRIGHT:
                        //case Axis.NONE:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X - m.Delta.X + HDelta, m.DockedLocation.Y - m.Delta.Y + VDelta);
                        break;
                    case Axis.BOTTOMLEFT:
                    case Axis.LEFT:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X, m.DockedLocation.Y - m.Delta.Y + VDelta);
                        break;
                    case Axis.TOPLEFT:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X, m.DockedLocation.Y);
                        break;
                    case Axis.TOP:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X - m.Delta.X + HDelta, m.DockedLocation.Y);
                        break;
                    case Axis.BOTTOM:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X - m.Delta.X + HDelta, m.DockedLocation.Y - m.Delta.Y + VDelta);
                        break;
                    case Axis.TOPRIGHT:
                        newLocation = new System.Drawing.Point(m.DockedLocation.X - m.Delta.X + HDelta, m.DockedLocation.Y);
                        break;
                }

                //limit to client area
                if (newLocation.X + m.Width > _console.Width) newLocation.X = _console.Width - m.Width;
                if (newLocation.Y + m.Height > _console.Height) newLocation.Y = _console.Height - m.Height;
                if (newLocation.X < 0) newLocation.X = 0;
                if (newLocation.Y < 0) newLocation.Y = 0;

                m.Location = newLocation;
            }
        }
        private static void returnMeterFromFloating(ucMeter m, frmMeterDisplay frm)
        {
            if (_console == null) return;

            frm.Hide();
            m.Hide();
            m.Parent = _console;
            m.Anchor = AnchorStyles.None;// AnchorStyles.Right | AnchorStyles.Bottom;
            m.RestoreLocation();
            m.Floating = false;
            setPoisitionOfDockedMeter(m);
            m.BringToFront();

            if (m.RX == 2 && !_console.RX2Enabled) return;

            m.Show();
        }
        private static void setMeterFloating(ucMeter m, frmMeterDisplay frm)
        {
            if (_console == null) return;

            if (m.RX == 2 && !_console.RX2Enabled) return;

            m.Hide();
            frm.TakeOwner(m);
            m.Floating = true;

            if (m.RX == 2 && !_console.RX2Enabled) return;

            frm.Show();
        }
        private static void OnRX2EnabledChanged(bool enabled)
        {
            if (enabled)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters.Where(o => o.Value.RX == 2))
                {
                    ucMeter ucM = kvp.Value;

                    if (!_lstMeterDisplayForms.ContainsKey(ucM.ID)) return;

                    if (ucM.Floating)
                        setMeterFloating(ucM, _lstMeterDisplayForms[ucM.ID]);
                    else
                        returnMeterFromFloating(ucM, _lstMeterDisplayForms[ucM.ID]);
                }
            }
        }
        private static void OnRX2EnabledPreChanged(bool enabled)
        {
            if (!enabled)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters.Where(o => o.Value.RX == 2))
                {
                    ucMeter ucM = kvp.Value;

                    if (!_lstMeterDisplayForms.ContainsKey(ucM.ID)) return;

                    if (ucM.Floating)
                        _lstMeterDisplayForms[ucM.ID].Hide();
                    else
                        ucM.Hide();
                }
            }
        }
        public static void RestoreSettings(ref List<KeyValuePair<string, string>> settings)
        {
            foreach(KeyValuePair<string, string> kvp in settings.Where(o => o.Key.StartsWith("meterContData")))
            {
                ucMeter ucM = new ucMeter();
                bool bUcMeterOk = ucM.TryParse(kvp.Value);
                if (bUcMeterOk)
                {
                    AddMeterContainer(ucM, false);

                    clsMeter m = MeterFromId(ucM.ID);

                    if (m != null)
                    {
                        // now the meter
                        IEnumerable<KeyValuePair<string, string>> meterData = settings.Where(o => o.Key.StartsWith("meterData_" + m.ID));
                        if (meterData != null && meterData.Count() == 1)
                        {
                            KeyValuePair<string, string> md = meterData.First();

                            clsMeter tmpMeter = new clsMeter(1,""); // dummy init data, will get replaced by tryparse below
                            tmpMeter.TryParse(md.Value);

                            // copy to actual meter
                            // id will be the same
                            m.Name = tmpMeter.Name;
                            m.RX = tmpMeter.RX;
                            m.XRatio = tmpMeter.XRatio;
                            m.YRatio = tmpMeter.YRatio;
                            m.DisplayGroup = tmpMeter.DisplayGroup;
                            m.PadX = tmpMeter.PadX;
                            m.PadY = tmpMeter.PadY;
                            m.Height = tmpMeter.Height;
                        }

                        // finally the groups

                        IEnumerable<KeyValuePair<string, string>> meterIGData = settings.Where(o => o.Key.StartsWith("meterIGData_") && o.Value.Contains(m.ID)); // parent id, stored in value
                        foreach (KeyValuePair<string, string> igd in meterIGData)
                        {
                            clsItemGroup ig = new clsItemGroup();
                            ig.TryParse(igd.Value);

                            m.AddMeter(ig.MeterType, ig);
                        }

                        m.Rebuild();
                    }
                }
            }
        }
        public static void StoreSettings(ref ArrayList a)
        {
            if (a == null) return;

            // meter container data
            foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
            {
                a.Add("meterContData_" + kvp.Value.ID + "/" + kvp.Value.ToString());

                // then the meter itself which holds multiple meter items
                clsMeter m = MeterFromId(kvp.Value.ID);
                if (m != null)
                {           
                    a.Add("meterData_" + kvp.Value.ID + "/" + m.ToString());

                    Dictionary<string, clsItemGroup> groupItems = m.getMeterGroups();

                    // then each meter item which are groups in this case
                    foreach (KeyValuePair<string, clsItemGroup> ig in groupItems)
                    {
                        Dictionary<string, clsMeterItem> mis = m.itemsFromID(ig.Value.ID);
                        if (mis != null)
                        {
                            foreach (KeyValuePair<string, clsMeterItem> kvp2 in mis.Where(o => o.Value.StoreSettings == true))
                            {
                                clsMeterItem mi = kvp2.Value;
                                if (mi != null)
                                {
                                    a.Add("meterIGData_" + kvp2.Value.ID + "/" + mi.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
        public static Dictionary<string, ucMeter> MeterContainers
        {
            get { return _lstUCMeters; }
        }
        public static void RemoveMeterContainer(string sId)
        {
            if (!_lstUCMeters.ContainsKey(sId)) return;
            if (!_lstMeterDisplayForms.ContainsKey(sId)) return;

            frmMeterDisplay f = _lstMeterDisplayForms[sId];
            f.Hide();

            ucMeter uc = _lstUCMeters[sId];
            uc.Hide();

            removeRenderer(sId);

            f.Close();

            _lstMeterDisplayForms.Remove(sId);
            _lstUCMeters.Remove(sId);
            _meters.Remove(sId);
        }
        #endregion
        #region clsMeterItem
        public class clsMeterItem
        {
            public enum MeterItemType
            {
                BASE = 0,
                H_BAR,
                V_BAR,
                H_SCALE,
                V_SCALE,
                NEEDLE,
                TEXT,
                IMAGE,
                SOLID_COLOUR,
                CLICKBOX,
                MAGIC_EYE,
                ITEM_GROUP
            }

            private string _sId;
            private string _sParentId;

            private MeterItemType _ItemType;

            private bool _storeSettings;

            private PointF _topLeft; // 0-1,0-1
            private PointF _displayTopLeft; // 0-1,0-1
            private SizeF _size;  // 0-1,0-1
            private int _zOrder; // lower first
            private int _msUpdateInterval; //ms
            private float _attackRatio; // 0f - 1f
            private float _decayRatio; // 0f - 1f
            private float _value;
            private System.Drawing.Color _historyColour;
            private Reading _readingType;
            private Stopwatch _updateStopwatch;
            private Dictionary<float, PointF> _scaleCalibration;
            private int _displayGroup;
            private bool _bNormaliseTo100W;
            private bool _bOnlyWhenTx;
            private bool _bOnlyWhenRx;

            public clsMeterItem()
            {
                // constructor
                _sId = System.Guid.NewGuid().ToString();
                _sParentId = "";
                _ItemType = MeterItemType.BASE;
                _storeSettings = false;
                _readingType = Reading.NONE;
                _topLeft.X = 0;
                _topLeft.Y = 0;
                _displayTopLeft.X = _topLeft.X;
                _displayTopLeft.Y = _topLeft.Y;
                _zOrder = 0;
                _msUpdateInterval = 5000; //ms
                _attackRatio = 0.8f;
                _decayRatio = 0.2f;
                _value = -200f;
                _historyColour = System.Drawing.Color.FromArgb(128, 255, 255, 255);
                _scaleCalibration = new Dictionary<float, PointF>();
                _size.Width = 1f;
                _size.Height = 1f;
                _displayGroup = 0;
                _bNormaliseTo100W = false;
                _bOnlyWhenTx = false;
                _bOnlyWhenRx = false;                
                _updateStopwatch = new Stopwatch();
            }
            public string ID { 
                get { return _sId; } 
                set { _sId = value; }
            }
            public string ParentID
            {
                get { return _sParentId; }
                set { _sParentId = value; }
            }
            public bool StoreSettings
            {
                get { return _storeSettings; }
                set { _storeSettings = value; }
            }
            public PointF TopLeft {
                get { return _topLeft; }
                set { 
                    _topLeft = value;
                    _displayTopLeft = new PointF(_topLeft.X, _topLeft.Y);
                }
            }
            public PointF DisplayTopLeft
            {
                get { return _displayTopLeft; }
                set { _displayTopLeft = value; }
            }
            public SizeF Size
            {
                get { return _size; }
                set 
                { _size = value; }
            }
            public int DisplayGroup
            {
                get { return _displayGroup; }
                set { _displayGroup = value; }
            }
            public bool NormaliseTo100W
            {
                get { return _bNormaliseTo100W; }
                set { _bNormaliseTo100W = value; }
            }
            public int UpdateInterval
            {
                get { return _msUpdateInterval; }
                set
                {
                    _msUpdateInterval = value;
                    if (_msUpdateInterval < 1) _msUpdateInterval = 1;
                }
            }
            public float AttackRatio
            {
                get { return _attackRatio; }
                set {
                    _attackRatio = value;
                    if (_attackRatio < 0) _attackRatio = 0;
                    if (_attackRatio > 1f) _attackRatio = 1f;
                }
            }
            public float DecayRatio
            {
                get { return _decayRatio; }
                set
                {
                    _decayRatio = value;
                    if (_decayRatio < 0) _decayRatio = 0;
                    if (_decayRatio > 1f) _decayRatio = 1f;
                }
            }
            public MeterItemType ItemType
            {
                get { return _ItemType; }
                set { _ItemType = value; }
            }
            public Reading ReadingSource
            {
                get { return _readingType; }
                set { _readingType = value; }
            }

            public bool RequiresUpdate
            {
                get
                {
                    //if (!_updateStopwatch.IsRunning) _updateStopwatch.Restart();
                    //bool bNeedsUpdate = _updateStopwatch.ElapsedMilliseconds >= _msUpdateInterval;
                    //if(bNeedsUpdate) _updateStopwatch.Stop();

                    bool bNeedsUpdate = DelayUntilNextUpdate <= 0;

                    return bNeedsUpdate;
                }
            }
            public int DelayUntilNextUpdate
            {
                // returns the delay that is needed before we need an update in ms
                get
                {
                    if (!_updateStopwatch.IsRunning) _updateStopwatch.Restart();
                    long nDelay = _msUpdateInterval - _updateStopwatch.ElapsedMilliseconds;
                    if (nDelay <= 0)
                    {
                        _updateStopwatch.Stop();
                        nDelay = 0;
                    }

                    return (int)nDelay;
                }
            }
            public virtual void Update(int rx, ref List<Reading> readingsUsed)
            {
                // can be overriden by derived

                //// get latest reading
                //float reading = MeterManager.GetReading(rx, ReadingSource);

                //Value = reading;

                //// this reading has been used
                //if (!readingsUsed.Contains(ReadingSource))
                //    readingsUsed.Add(ReadingSource);
            }
            public virtual float Value
            {
                get { return _value; }
                set { _value = value; }
            }
            public int ZOrder
            {
                get { return _zOrder; }
                set { _zOrder = value; }
            }
            public virtual void History(out float minHistory, out float maxHistory)
            {
                minHistory = 0f;
                maxHistory = 0f;
            }
            public virtual float MinHistory
            {
                get
                {
                    return 0f;
                }
            }
            public virtual float MaxHistory
            {
                get
                {
                    return 0f;
                }
            }
            public virtual bool ShowHistory
            {
                get { return false; }
                set { }
            }
            public virtual System.Drawing.Color HistoryColour
            {
                get { return _historyColour; }
                set { _historyColour = value; }
            }
            public virtual Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }            
            public bool OnlyWhenTX
            {
                get { return _bOnlyWhenTx; }
                set { _bOnlyWhenTx = value; }
            }
            public bool OnlyWhenRX
            {
                get { return _bOnlyWhenRx; }
                set { _bOnlyWhenRx = value; }
            }
            public override string ToString()
            {
                return base.ToString();
            }
            public virtual bool TryParse(string val)
            {
                return false;
            }
        }
        //
        internal class clsItemGroup : clsMeterItem
        {
            private MeterType _meterType;
            private int _order; // 0 = topmost

            public clsItemGroup()
            {
                ItemType = MeterItemType.ITEM_GROUP;
                _meterType = MeterType.NONE;
                StoreSettings = true;
                ZOrder = 20;
            }
            public MeterType MeterType
            {
                get { return _meterType; }
                set { _meterType = value; }
            }
            public int Order
            {
                get { return _order; }
                set { _order = value; }
            }
            public override string ToString()
            {
                string sRet;

                sRet = ID + "|" +
                    ParentID + "|" +
                    ItemType.ToString() + "|" +
                    TopLeft.X.ToString("f4") + "|" +
                    TopLeft.Y.ToString("f4") + "|" +
                    Size.Width.ToString("f4") + "|" +
                    Size.Height.ToString("f4") + "|" +
                    MeterType.ToString() + "|" +
                    Order.ToString();
                    //ZOrder.ToString() + "|" +
                    //UpdateInterval.ToString() + "|" +
                    //AttackRatio.ToString("f4") + "|" +
                    //DecayRatio.ToString("f4") + "|" +
                    //HistoryColour.ToString() + "|" +
                    //ReadingSource.ToString();

                return sRet;
            }
            public override bool TryParse(string str)
            {
                bool bOk = false;
                float x = 0, y = 0, w = 0, h = 0;
                int order = 0;
                MeterType mt = MeterType.LAST;

                if (str != "")
                {
                    string[] tmp = str.Split('|');
                    if (tmp.Length == 9)
                    {
                        bOk = tmp[0] != "";
                        if (bOk) ID = tmp[0];
                        if (bOk) ParentID = tmp[1];
                        //if (bOk) bOk = Enum.TryParse<MeterItemType>(tmp[2], out it);  //ignore

                        if (bOk) bOk = float.TryParse(tmp[3], out x);
                        if (bOk) bOk = float.TryParse(tmp[4], out y);
                        if (bOk) bOk = float.TryParse(tmp[5], out w);
                        if (bOk) bOk = float.TryParse(tmp[6], out h);
                        if (bOk) bOk = Enum.TryParse<MeterType>(tmp[7], out mt);
                        if (bOk)
                            MeterType = mt;
                        if (bOk) bOk = int.TryParse(tmp[8], out order);
                        if (bOk) Order = order;
                    }
                }

                return bOk;
            }
        }
        internal class clsClickBox : clsMeterItem
        {
            private MouseButtons _button;
            private int _gotoGroup;
            public clsClickBox()
            {
                ItemType = MeterItemType.CLICKBOX;

                _button = MouseButtons.Left | MouseButtons.Right | MouseButtons.Middle;
                _gotoGroup = 0;
                StoreSettings = false;
            }
            public MouseButtons Button
            {
                get { return _button; }
                set { _button = value; }
            }
            public int GotoGroup 
            {
                get { return _gotoGroup; }
                set { _gotoGroup = value; }
            }
            //public override string ToString()
            //{
            //    string sRet;

            //    sRet = ID + "|" +
            //        ParentID + "|" +
            //        ItemType.ToString() + "|" +
            //        TopLeft.X.ToString("f4") + "|" +
            //        TopLeft.Y.ToString("f4") + "|" +
            //        Size.Width.ToString("f4") + "|" +
            //        Size.Height.ToString("f4") + "|" +
            //        Button.ToString() + "|" +
            //        GotoGroup.ToString("f4") + "|";

            //    return sRet;
            //}
        }
        internal class clsSolidColour : clsMeterItem
        {
            private System.Drawing.Color _colour;
            public clsSolidColour()
            {
                ItemType = MeterItemType.SOLID_COLOUR;
                _colour = System.Drawing.Color.Black;
                StoreSettings = false;
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set
                { 
                    _colour = value;
                }
            }
            //public override string ToString()
            //{
            //    string sRet;

            //    sRet = ID + "|" +
            //        ParentID + "|" +
            //        ItemType.ToString() + "|" +
            //        TopLeft.X.ToString("f4") + "|" +
            //        TopLeft.Y.ToString("f4") + "|" +
            //        Size.Width.ToString("f4") + "|" +
            //        Size.Height.ToString("f4") + "|" +
            //        //
            //        _colour.ToString();

            //    return sRet;
            //}
        }
        internal class clsImage : clsMeterItem
        {
            private string _name;
            private PointF _clipTopLeft;
            private SizeF _clipSize;
            private bool _clipped;
            public clsImage()
            {
                ItemType = MeterItemType.IMAGE;

                _name = "";
                _clipTopLeft = new PointF(0f, 0f);
                _clipSize = new SizeF(1f, 1f);
                _clipped = false;
                StoreSettings = false;
            }
            public string ImageName
            {
                get { return _name; }
                set
                {
                    _name = value;
                }
            }
            public PointF ClipTopLeft
            {
                get { return _clipTopLeft; }
                set { _clipTopLeft = value; }
            }
            public SizeF ClipSize
            {
                get { return _clipSize; }
                set { _clipSize = value; }
            }
            public bool Clipped
            {
                get { return _clipped; }
                set { _clipped = value; }
            }
            //public override string ToString()
            //{
            //    string sRet;

            //    sRet = ID + "|" +
            //        ParentID + "|" +
            //        ItemType.ToString() + "|" +
            //        TopLeft.X.ToString("f4") + "|" +
            //        TopLeft.Y.ToString("f4") + "|" +
            //        Size.Width.ToString("f4") + "|" +
            //        Size.Height.ToString("f4") + "|";

            //    return sRet;
            //}
        }
        internal class clsScaleItem : clsMeterItem
        {
            private System.Drawing.Color _lowColour;
            private System.Drawing.Color _highColour;
            private string _fontFamily;
            private FontStyle _fontStyle;
            private System.Drawing.Color _fontColorLow;
            private System.Drawing.Color _fontColorHigh;
            private System.Drawing.Color _fontColourType;
            private float _fontSize;
            private bool _showType;
            private bool _showMarkers;

            public clsScaleItem()
            {
                _lowColour = System.Drawing.Color.White;
                _highColour = System.Drawing.Color.Red;
                _fontColourType = System.Drawing.Color.DarkGray;

                _showType = false;
                _showMarkers = true;

                _fontFamily = "Trebuchet MS";
                _fontStyle = FontStyle.Regular;
                _fontColorLow = System.Drawing.Color.White;
                _fontColorHigh = System.Drawing.Color.Red;
                _fontColourType = System.Drawing.Color.DarkGray;
                _fontSize = 20f;

                ItemType = MeterItemType.H_SCALE;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
                StoreSettings = false;
            }
            public System.Drawing.Color LowColour
            {
                get { return _lowColour; }
                set { _lowColour = value; }
            }
            public System.Drawing.Color HighColour
            {
                get { return _highColour; }
                set { _highColour = value; }
            }
            public System.Drawing.Color FontColourType
            {
                get { return _fontColourType; }
                set { _fontColourType = value; }
            }
            public string FontFamily
            {
                get { return _fontFamily; }
                set { _fontFamily = value; }
            }
            public FontStyle FntStyle
            {
                get { return _fontStyle; }
                set { _fontStyle = value; }
            }
            public System.Drawing.Color FontColourLow
            {
                get { return _fontColorLow; }
                set { _fontColorLow = value; }
            }
            public System.Drawing.Color FontColourHigh
            {
                get { return _fontColorHigh; }
                set { _fontColorHigh = value; }
            }
            public float FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value; }
            }
            public bool ShowType
            {
                get { return _showType; }
                set { _showType = value; }
            }  
            public bool ShowMarkers
            {
                get { return _showMarkers; }
                set { _showMarkers = value; }
            }
            public override string ToString()
            {
                string sRet;

                sRet = ID + "|" +
                    ParentID + "|" +
                    ItemType.ToString() + "|" +
                    TopLeft.X.ToString("f4") + "|" +
                    TopLeft.Y.ToString("f4") + "|" +
                    Size.Width.ToString("f4") + "|" +
                    Size.Height.ToString("f4") + "|";
                    //


                return sRet;
            }
        }
        internal class clsMagicEyeItem : clsMeterItem
        {
            private List<float> _history;
            private int _msHistoryDuration; //ms
            private bool _showHistory;
            private bool _showValue;
            private object _historyLock = new object();
            private System.Drawing.Color _colour;

            private Dictionary<float, PointF> _scaleCalibration;

            public clsMagicEyeItem()
            {
                _history = new List<float>();
                _msHistoryDuration = 0;
                _showHistory = false;
                _showValue = true;

                _colour = System.Drawing.Color.Lime;

                _scaleCalibration = new Dictionary<float, PointF>();

                ItemType = MeterItemType.MAGIC_EYE;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
                StoreSettings = false;
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                lock (_historyLock)
                {
                    if (reading > Value)
                        Value = (reading * AttackRatio) + (Value * (1f - AttackRatio));
                    else
                        Value = (reading * DecayRatio) + (Value * (1f - DecayRatio));

                    // signal history
                    _history.Add(Value); // adds to end of the list
                    int numberToRemove = _history.Count - (_msHistoryDuration / UpdateInterval);
                    // the list is sized based on delay
                    if (numberToRemove > 0) _history.RemoveRange(0, numberToRemove); // remove the oldest, the head of the list
                }

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }
            public override bool ShowHistory
            {
                get { return _showHistory; }
                set { _showHistory = value; }
            }
            public bool ShowValue
            {
                get { return _showValue; }
                set { _showValue = value; }
            }
            public int HistoryDuration
            {
                get { return _msHistoryDuration; }
                set
                {
                    _msHistoryDuration = value;
                    if (_msHistoryDuration < UpdateInterval) _msHistoryDuration = UpdateInterval;
                }
            }
            public override float MinHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Min();
                    }
                }
            }
            public override float MaxHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Max();
                    }
                }
            }
            public override void History(out float minHistory, out float maxHistory)
            {
                if (_history.Count == 0)
                {
                    minHistory = Value;
                    maxHistory = Value;
                }
                else
                {
                    minHistory = _history.Min();
                    maxHistory = _history.Max();
                }
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set { _colour = value; }
            }
            public override Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }
        }

        internal class clsBarItem : clsMeterItem
        {
            public enum BarStyle
            {
                None = 0,
                Line,
                SolidFilled,
                GradientFilled,
                Segments,
            }
            private List<float> _history;
            private int _msHistoryDuration; //ms
            private bool _showHistory;
            private bool _showValue;
            private object _historyLock = new object();
            private BarStyle _style;
            private System.Drawing.Color _colour;
            private System.Drawing.Color _markerColour;
            private System.Drawing.Color _peakHoldMarkerColour;
            private float _strokeWidth;
            private bool _peakHold;
            private bool _showMarker;

            private string _fontFamily;
            private FontStyle _fontStyle;
            private System.Drawing.Color _fontColor;
            private float _fontSize;

            private Dictionary<float, PointF> _scaleCalibration;

            public clsBarItem()
            {
                _history = new List<float>();
                _msHistoryDuration = 2000;
                _showHistory = false;
                _showValue = true;

                _style = BarStyle.Line;
                _colour = System.Drawing.Color.Red;
                _markerColour = System.Drawing.Color.Yellow;
                _peakHoldMarkerColour = System.Drawing.Color.Red;
                _strokeWidth = 3f;
                _peakHold = false;
                _showMarker = true;

                _scaleCalibration = new Dictionary<float, PointF>();

                _fontFamily = "Trebuchet MS";
                _fontStyle = FontStyle.Regular;
                _fontColor = System.Drawing.Color.DarkGray;
                _fontSize = 20f;

                ItemType = MeterItemType.H_BAR;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
                StoreSettings = false;
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                lock (_historyLock)
                {
                    if (reading > Value)
                        Value = (reading * AttackRatio) + (Value * (1f - AttackRatio));
                    else
                        Value = (reading * DecayRatio) + (Value * (1f - DecayRatio));

                    // signal history
                    _history.Add(Value); // adds to end of the list
                    int numberToRemove = _history.Count - (_msHistoryDuration / UpdateInterval);
                    // the list is sized based on delay
                    if (numberToRemove > 0) _history.RemoveRange(0, numberToRemove); // remove the oldest, the head of the list
                }

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }

            public BarStyle Style
            {
                get { return _style; }
                set { _style = value; }
            }
            public override bool ShowHistory
            {
                get { return _showHistory; }
                set { _showHistory = value; }
            }
            public bool ShowValue
            {
                get { return _showValue; }
                set { _showValue = value; }
            }
            public int HistoryDuration
            {
                get { return _msHistoryDuration; }
                set
                {
                    _msHistoryDuration = value;
                    if (_msHistoryDuration < UpdateInterval) _msHistoryDuration = UpdateInterval;
                }
            }
            public override float MinHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Min();
                    }
                }
            }
            public override float MaxHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Max();
                    }
                }
            }
            public override void History(out float minHistory, out float maxHistory)
            {
                if (_history.Count == 0)
                {
                    minHistory = Value;
                    maxHistory = Value;
                }
                else
                {
                    minHistory = _history.Min();
                    maxHistory = _history.Max();
                }
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set { _colour = value; }
            }
            public System.Drawing.Color MarkerColour
            {
                get { return _markerColour; }
                set { _markerColour = value; }
            }
            public System.Drawing.Color PeakHoldMarkerColour
            {
                get { return _peakHoldMarkerColour; }
                set { _peakHoldMarkerColour = value; }
            }
            public bool PeakHold
            {
                get { return _peakHold; }
                set { _peakHold = value; }
            }
            public bool ShowMarker
            {
                get { return _showMarker; }
                set { _showMarker = value; }
            }
            public float StrokeWidth
            {
                get { return _strokeWidth; }
                set { _strokeWidth = value; }
            }
            public override Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }

            public string FontFamily
            {
                get { return _fontFamily; }
                set { _fontFamily = value; }
            }
            public FontStyle FntStyle
            {
                get { return _fontStyle; }
                set { _fontStyle = value; }
            }
            public System.Drawing.Color FontColour
            {
                get { return _fontColor; }
                set { _fontColor = value; }
            }
            public float FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value; }
            }
        }
        internal class clsNeedleItem : clsMeterItem
        {
            public enum NeedlePlacement
            {
                Bottom = 0,
                Top,
                Left,
                Right
            }
            public enum NeedleStyle
            {
                Line = 0,
                Arrow,
                Hollow
            }
            public enum NeedleDirection
            {
                Clockwise = 0,
                CounterClockwise
            }
            private List<float> _history;
            private int _msHistoryDuration; //ms
            private bool _showHistory;
            private object _historyLock = new object();
            private NeedleStyle _style;
            private PointF _needleOffset;
            private NeedlePlacement _placement;
            private System.Drawing.Color _colour;
            private float _strokeWidth;
            private bool _scaleStrokeWidth;
            private NeedleDirection _needleDirection;
            private float _lengthFactor;
            private bool _setup;
            private Dictionary<float, PointF> _scaleCalibration;
            private PointF _radiusRatio;

            public clsNeedleItem()
            {
                _history = new List<float>();
                _msHistoryDuration = 500;
                _showHistory = false;
                _placement = NeedlePlacement.Bottom;
                _needleOffset.X = 0f;
                _needleOffset.Y = 0.5f;
                _lengthFactor = 1f;
                _setup = false;
                _radiusRatio.X = 1f;
                _radiusRatio.Y = 1f;
                _style = NeedleStyle.Line;
                _colour = System.Drawing.Color.White;
                _strokeWidth = 3f;
                _scaleStrokeWidth = false;
                _needleDirection = NeedleDirection.Clockwise;
                _scaleCalibration = new Dictionary<float, PointF>();

                ItemType = MeterItemType.NEEDLE;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
                StoreSettings = false;
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                lock (_historyLock)
                {
                    if (reading > Value)
                       Value = (reading * AttackRatio) + (Value * (1f - AttackRatio));
                    else
                        Value = (reading * DecayRatio) + (Value * (1f - DecayRatio));

                    // signal history
                    _history.Add(Value); // adds to end of the list
                    int numberToRemove = _history.Count - (_msHistoryDuration / UpdateInterval);
                    // the list is sized based on delay
                    if (numberToRemove > 0) _history.RemoveRange(0, numberToRemove); // remove the oldest, the head of the list
                }

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }
            public float StrokeWidth
            {
                get { return _strokeWidth; }
                set { _strokeWidth = value; }
            }
            public bool ScaleStrokeWidth
            {
                get { return _scaleStrokeWidth; }
                set { _scaleStrokeWidth = value; }
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set
                { _colour = value; }
            }
            public bool Setup
            {
                get { return _setup; }
                set { _setup = value; }
            }
            public NeedlePlacement Placement
            {
                get { return _placement; }
                set { _placement = value; }
            }
            public NeedleDirection Direction
            {
                get { return _needleDirection; }
                set { _needleDirection = value; }
            }
            public PointF NeedleOffset
            {
                get { return _needleOffset; }
                set { _needleOffset = value; }
            }
            public NeedleStyle Style
            {
                get { return _style; }
                set { _style = value; }
            }
            public override bool ShowHistory
            {
                get { return _showHistory; }
                set { _showHistory = value; }
            }
            public int HistoryDuration
            {
                get { return _msHistoryDuration; }
                set
                {
                    _msHistoryDuration = value;
                    if (_msHistoryDuration < UpdateInterval) _msHistoryDuration = UpdateInterval;
                }
            }
            public override float MinHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if(_history.Count == 0) return Value;
                        return _history.Min();
                    }
                }
            }
            public override float MaxHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Max();
                    }
                }
            }
            public override void History(out float minHistory, out float maxHistory)
            {
                lock (_historyLock)
                {
                    if (_history.Count == 0)
                    {
                        minHistory = Value;
                        maxHistory = Value;
                    }
                    else
                    {
                        minHistory = _history.Min();
                        maxHistory = _history.Max();
                    }
                }
            }
            public float LengthFactor
            {
                get { return _lengthFactor; }
                set { _lengthFactor = value; }
            }
            public override Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }
            public PointF RadiusRatio
            {
                get { return _radiusRatio; }
                set { _radiusRatio = value; }
            }
        }
        internal class clsText : clsMeterItem
        {
            private string _sText;
            private string _fontFamily;
            private FontStyle _fontStyle;
            private System.Drawing.Color _color;
            private bool _fixedSize;
            private float _fontSize;
            public clsText()
            {
                _sText = "";
                _fontFamily = "Trebuchet MS";
                _fontStyle = FontStyle.Regular;
                _color = System.Drawing.Color.White;
                _fixedSize = false;
                _fontSize = 9f;
                _readingText = "";

                ItemType = MeterItemType.TEXT;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                StoreSettings = false;
            }
            private void updateReadintText(float reading)
            {
                switch (_sText)
                {
                    case "%valueWithType%":
                        {
                            _readingText = reading.ToString("0.0") + MeterManager.ReadingUnits(ReadingSource);
                        }
                        break;
                    case "%value%":
                        {
                            _readingText = reading.ToString("0.0");
                        }
                        break;
                    default:
                        _readingText = "";
                        break;
                }
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                updateReadintText(reading);

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }
            private string _readingText;
            public string Text
            {
                get
                {
                    string sText = _sText;
                    switch(_sText)
                    { 
                        case "%valueWithType%":
                        case "%value%":
                            sText = _readingText;
                        break; 
                    }

                    return sText; 
                }
                set { _sText = value; }
            }
            public string FontFamily
            {
                get { return _fontFamily; }
                set { _fontFamily = value; }
            }
            public FontStyle Style
            {
                get { return _fontStyle; }
                set { _fontStyle = value; }
            }
            public System.Drawing.Color Colour
            {
                get { return _color; }
                set { _color = value; }
            }
            public bool FixedSize
            {
                get { return _fixedSize; }
                set { _fixedSize = value; }
            }
            public float FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value; }
            }
            public override float Value
            {
                get { return base.Value; }
                set { 
                    base.Value = value;
                    updateReadintText(value);
                }
            }
        }
        //
        #endregion
        #region clsMeterItems
        public class clsMeter
        {
            // the single meter, that may contain many bars/labels/analog/images/etc
            private string _sId;
            private string _name;            
            private int _rx;

            private float _XRatio; // 0-1
            private float _YRatio; // 0-1

            private Dictionary<string, clsMeterItem> _meterItems;
            private Dictionary<string, clsMeterItem> _sortedMeterItemsForZOrder;
            private int _displayGroup;
            private List<string> _displayGroups;
            private bool _mox;
            private float _fPadX = 0.02f;
            private float _fPadY = 0.05f;
            private float _fHeight = 0.05f;

            private int _quickestRXUpdate;
            private int _quickestTXUpdate;
            private Object _objMeterItemLock = new Object();

            private void addMeterItem(clsMeterItem mi)
            {
                lock (_objMeterItemLock)
                {
                    _meterItems.Add(mi.ID, mi);
                }
            }
            public void AddMeter(MeterType meter, clsItemGroup restoreIg = null)
            {
                //restoreIg is passed in when used by restoreSettings, so that the item group can be ordered
                //correctly and the ID copied over
                float bBottom = 0;
                int nDelay = 50;

                switch (meter)
                {
                    case MeterType.SIGNAL_STRENGTH: AddSMeterBarSignal(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.AVG_SIGNAL_STRENGTH: AddSMeterBarSignalAvg(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.ADC: AddADCBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.AGC: AddAGCBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.AGC_GAIN: AddAGCGainBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.MIC: AddMicBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.PWR: AddPWRBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.REVERSE_PWR: AddREVPWRBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.ALC: AddALCBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.EQ: AddEQBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.LEVELER: AddLevelerBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.COMP: AddCompBar(nDelay, 0, out bBottom, restoreIg); break;
                    //case MeterType.CPDR: break;
                    case MeterType.ALC_GAIN: AddALCGainBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.ALC_GROUP: AddALCGroupBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.LEVELER_GAIN: AddLevelerGainBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.CFC: AddCFCBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.MAGIC_EYE: AddMagicEye(nDelay, 0, out bBottom, 0.2f, restoreIg); break;
                    case MeterType.ESTIMATED_PBSNR: AddPBSNRBar(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.KENWOOD: AddKenwood(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.CROSS: AddCrossNeedle(nDelay, 0, out bBottom, restoreIg); break;
                    case MeterType.SWR: AddSWRBar(nDelay, 0, out bBottom, restoreIg); break;
                }
            }

            #region meterDefs
            public string AddSMeterBarSignal(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg = null)
            { 
                return addSMeterBar(nMSupdate, Reading.SIGNAL_STRENGTH, fTop, out fBottom, restoreIg);
            }
            public string AddSMeterBarSignalAvg(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg = null)
            {
                return addSMeterBar(nMSupdate, Reading.AVG_SIGNAL_STRENGTH, fTop, out fBottom, restoreIg);
            }
            private string addSMeterBar(int nMSupdate, Reading reading, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = reading;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.2f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 4000;
                cb.ShowHistory = true;
                cb.ShowValue = true;
                cb.Colour = System.Drawing.Color.CadetBlue;
                cb.ShowMarker = true;
                cb.PeakHoldMarkerColour = System.Drawing.Color.Red;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-133, new PointF(0, 0)); // position for S0 or below  // -133 is the edge, as S0 (-127) is the first small tick
                cb.ScaleCalibration.Add(-73, new PointF(0.5f, 0)); // position for S9
                cb.ScaleCalibration.Add(-13, new PointF(0.99f, 0)); // position for S9+60dB or above
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 3;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc;
                sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                if (reading == Reading.SIGNAL_STRENGTH)
                    ig.MeterType = MeterType.SIGNAL_STRENGTH;
                else
                    ig.MeterType = MeterType.AVG_SIGNAL_STRENGTH;
                lock (_objMeterItemLock)
                {
                    ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;
                }
                addMeterItem(ig);

                return cb.ID;
            }
            public string AddADCBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.ADC_PK;
                cb.AttackRatio = 0.2f;
                cb.DecayRatio = 0.05f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 4000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Orange;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.CornflowerBlue);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-120, new PointF(0, 0));
                cb.ScaleCalibration.Add(-20, new PointF(0.8333f, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.99f, 0));
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 3;
                addMeterItem(cb);

                clsBarItem cb2;
                cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.ADC_AV;
                cb2.AttackRatio = 0.2f;
                cb2.DecayRatio = 0.05f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 0;
                cb2.ShowHistory = false;
                cb2.MarkerColour = System.Drawing.Color.DarkGray;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.CornflowerBlue);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-120, new PointF(0, 0));
                cb2.ScaleCalibration.Add(-20, new PointF(0.8333f, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.99f, 0));
                cb2.FontColour = System.Drawing.Color.Yellow;
                cb2.ShowValue = false;
                cb2.ZOrder = 2;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.ADC;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddPBSNRBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.ESTIMATED_PBSNR;
                cb.AttackRatio = 0.2f;
                cb.DecayRatio = 0.05f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 1000;
                cb.ShowHistory = false;
                cb.PeakHold = true;
                cb.Colour = System.Drawing.Color.DarkCyan;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Violet);
                cb.Style = clsBarItem.BarStyle.Segments;
                cb.ScaleCalibration.Add(0, new PointF(0, 0));
                cb.ScaleCalibration.Add(50, new PointF(0.8333f, 0));
                cb.ScaleCalibration.Add(60, new PointF(0.99f, 0));
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 3;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.ESTIMATED_PBSNR;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig); ;

                return cb.ID;
            }
            public string AddAGCGainBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.AGC_GAIN;
                cb.AttackRatio = 0.2f;
                cb.DecayRatio = 0.05f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 1000;
                cb.ShowHistory = true;
                cb.PeakHold = false;
                cb.Colour = System.Drawing.Color.DarkCyan;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Violet);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-50, new PointF(0, 0));
                cb.ScaleCalibration.Add(100, new PointF(0.857f, 0));
                cb.ScaleCalibration.Add(125, new PointF(0.99f, 0));
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 3;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.AGC_GAIN;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddAGCBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.AGC_PK;
                cb.AttackRatio = 0.2f;
                cb.DecayRatio = 0.05f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 1000;
                cb.ShowHistory = true;
                cb.PeakHold = false;
                cb.Colour = System.Drawing.Color.DarkCyan;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Violet);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-125, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.5f, 0));
                cb.ScaleCalibration.Add(125, new PointF(0.99f, 0));
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 3;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.AGC_AV;
                cb2.AttackRatio = 0.2f;
                cb2.DecayRatio = 0.05f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 0;
                cb2.ShowHistory = false;
                cb2.PeakHold = false;
                cb2.Colour = System.Drawing.Color.DarkCyan;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Violet);
                cb2.MarkerColour = System.Drawing.Color.DarkGray;
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-125, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.5f, 0));
                cb2.ScaleCalibration.Add(125, new PointF(0.99f, 0));
                cb2.FontColour = System.Drawing.Color.Yellow;
                cb2.ShowValue = false;
                cb2.ZOrder = 2;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.AGC;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddMagicEye(int nMSupdate, float fTop, out float fBottom, float fSize,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsMagicEyeItem me = new clsMagicEyeItem();
                me.ParentID = ig.ID;
                me.TopLeft = new PointF(0.5f - (fSize / 2f), fTop + _fPadY);
                me.Size = new SizeF(fSize, fSize);
                me.ZOrder = 2;
                me.AttackRatio = 0.2f;
                me.DecayRatio = 0.05f;
                me.UpdateInterval = nMSupdate;
                me.Colour = System.Drawing.Color.Lime;
                me.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                me.ScaleCalibration.Add(-127f, new PointF(0, 0));
                me.ScaleCalibration.Add(-73f, new PointF(0.85f, 0));
                me.ScaleCalibration.Add(-13f, new PointF(1f, 0));
                me.Value = -127f;
                addMeterItem(me);

                clsImage img = new clsImage();
                img.ParentID = ig.ID;
                img.TopLeft = me.TopLeft;
                img.Size = me.Size;
                img.ZOrder = 3;
                img.ImageName = "eye-bezel-glass";
                addMeterItem(img);

                fBottom = me.TopLeft.Y + me.Size.Height;

                ig.TopLeft = me.TopLeft;
                ig.Size = new SizeF(me.Size.Width, fBottom);
                ig.MeterType = MeterType.MAGIC_EYE;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return me.ID;
            }
            public string AddKenwood(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                //kenwood
                clsNeedleItem ni = new clsNeedleItem();
                ni.ParentID = ig.ID;
                ni.TopLeft = new PointF(0f, fTop + _fPadY - (_fHeight * 0.75f));
                ni.Size = new SizeF(1f, 0.441f); // image x to y ratio

                ni.OnlyWhenRX = true;
                ni.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                ni.AttackRatio = 0.8f;//0.1f;
                ni.DecayRatio = 0.2f;// 0.05f;
                ni.UpdateInterval = nMSupdate;
                ni.HistoryDuration = 4000;
                ni.ShowHistory = true;
                ni.Style = clsNeedleItem.NeedleStyle.Line;
                ni.Colour = System.Drawing.Color.FromArgb(255, 233, 51, 50);
                ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                ni.ZOrder = 4;
                ni.NeedleOffset = new PointF(0.004f, 0.736f);
                ni.RadiusRatio = new PointF(1f, 0.58f);
                ni.LengthFactor = 1.65f;
                ni.ScaleStrokeWidth = true;

                ni.ScaleCalibration.Add(-127f, new PointF(0.076f, 0.31f));
                ni.ScaleCalibration.Add(-121f, new PointF(0.131f, 0.272f));
                ni.ScaleCalibration.Add(-115f, new PointF(0.189f, 0.254f));
                ni.ScaleCalibration.Add(-109f, new PointF(0.233f, 0.211f));
                ni.ScaleCalibration.Add(-103f, new PointF(0.284f, 0.207f));
                ni.ScaleCalibration.Add(-97f, new PointF(0.326f, 0.177f));
                ni.ScaleCalibration.Add(-91f, new PointF(0.374f, 0.177f));
                ni.ScaleCalibration.Add(-85f, new PointF(0.414f, 0.151f));
                ni.ScaleCalibration.Add(-79f, new PointF(0.459f, 0.168f));
                ni.ScaleCalibration.Add(-73f, new PointF(0.501f, 0.142f));
                ni.ScaleCalibration.Add(-63f, new PointF(0.564f, 0.172f));
                ni.ScaleCalibration.Add(-53f, new PointF(0.63f, 0.164f));
                ni.ScaleCalibration.Add(-43f, new PointF(0.695f, 0.203f));
                ni.ScaleCalibration.Add(-33f, new PointF(0.769f, 0.211f));
                ni.ScaleCalibration.Add(-23f, new PointF(0.838f, 0.272f));
                ni.ScaleCalibration.Add(-13f, new PointF(0.926f, 0.31f));
                ni.Value = -127f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni);

                //volts
                clsNeedleItem ni2 = new clsNeedleItem(); ;
                ni2.ParentID = ig.ID;
                ni2.TopLeft = ni.TopLeft;
                ni2.Size = ni.Size;
                ni2.ReadingSource = Reading.VOLTS;
                ni2.AttackRatio = 0.2f;
                ni2.DecayRatio = 0.2f;
                ni2.UpdateInterval = nMSupdate;
                ni2.HistoryDuration = 500;
                ni2.ShowHistory = false;
                ni2.Style = clsNeedleItem.NeedleStyle.Line;
                ni2.Colour = System.Drawing.Color.Black;
                ni2.ZOrder = 3;
                ni2.NeedleOffset = new PointF(0.004f, 0.736f);
                ni2.RadiusRatio = new PointF(1f, 0.58f);
                ni2.LengthFactor = 0.75f;
                ni2.ScaleStrokeWidth = true;
                ni2.ScaleCalibration.Add(10f, new PointF(0.559f, 0.756f));
                ni2.ScaleCalibration.Add(12.5f, new PointF(0.605f, 0.772f));
                ni2.ScaleCalibration.Add(15f, new PointF(0.665f, 0.784f));
                ni2.Value = 10f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni2);

                //amps
                clsNeedleItem ni3 = new clsNeedleItem(); ;
                ni3.ParentID = ig.ID;
                ni3.TopLeft = ni.TopLeft;
                ni3.Size = ni.Size;
                ni3.OnlyWhenTX = true;
                ni3.ReadingSource = Reading.AMPS;
                ni3.AttackRatio = 0.2f;
                ni3.DecayRatio = 0.2f;
                ni3.UpdateInterval = nMSupdate;
                ni3.HistoryDuration = 500;
                ni3.ShowHistory = false;
                ni3.Style = clsNeedleItem.NeedleStyle.Line;
                ni3.Colour = System.Drawing.Color.Black;
                ni3.ZOrder = 3;
                ni3.NeedleOffset = new PointF(0.004f, 0.736f);
                ni3.RadiusRatio = new PointF(1f, 0.58f);
                ni3.LengthFactor = 1.15f;
                ni3.ScaleStrokeWidth = true;
                ni3.DisplayGroup = 4;
                ni3.ScaleCalibration.Add(0f, new PointF(0.199f, 0.576f));
                ni3.ScaleCalibration.Add(2f, new PointF(0.27f, 0.54f));
                ni3.ScaleCalibration.Add(4f, new PointF(0.333f, 0.516f));
                ni3.ScaleCalibration.Add(6f, new PointF(0.393f, 0.504f));
                ni3.ScaleCalibration.Add(8f, new PointF(0.448f, 0.492f));
                ni3.ScaleCalibration.Add(10f, new PointF(0.499f, 0.492f));
                ni3.ScaleCalibration.Add(12f, new PointF(0.554f, 0.488f));
                ni3.ScaleCalibration.Add(14f, new PointF(0.608f, 0.5f));
                ni3.ScaleCalibration.Add(16f, new PointF(0.667f, 0.516f));
                ni3.ScaleCalibration.Add(18f, new PointF(0.728f, 0.54f));
                ni3.ScaleCalibration.Add(20f, new PointF(0.799f, 0.576f));
                ni3.Value = 10f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni3);

                //
                clsClickBox clb = new clsClickBox();
                clb.ParentID = ig.ID;
                clb.TopLeft = new PointF(ni.TopLeft.X + 0.74f, ni.TopLeft.Y + 0.48f); // tl;
                clb.Size = new SizeF(0.2f, 0.05f);
                clb.OnlyWhenTX = true;
                clb.Button = MouseButtons.Left;
                addMeterItem(clb);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = clb.TopLeft;
                sc.Size = clb.Size;
                sc.OnlyWhenTX = true;
                sc.ZOrder = 5;
                sc.Colour = System.Drawing.Color.FromArgb(96, System.Drawing.Color.White);
                addMeterItem(sc);

                clsText tx = new clsText();
                tx.ParentID = ig.ID;
                tx.TopLeft = clb.TopLeft;
                tx.Size = clb.Size;
                tx.OnlyWhenTX = true;
                tx.Text = "%group%";
                tx.ZOrder = 10;
                tx.Style = FontStyle.Bold;
                tx.Colour = System.Drawing.Color.DarkGray;
                tx.FixedSize = true;
                tx.FontSize = 8f;
                addMeterItem(tx);

                clsNeedleItem ni4 = new clsNeedleItem();
                ni4.ParentID = ig.ID;
                ni4.TopLeft = ni.TopLeft;
                ni4.Size = ni.Size;
                ni4.OnlyWhenTX = true;
                ni4.ReadingSource = Reading.PWR;
                ni4.AttackRatio = 0.2f;//0.325f;
                ni4.DecayRatio = 0.1f;//0.5f;
                ni4.UpdateInterval = nMSupdate;
                ni4.HistoryDuration = 1000;
                ni4.ShowHistory = true;
                ni4.Style = clsNeedleItem.NeedleStyle.Line;
                ni4.Colour = System.Drawing.Color.FromArgb(255, 233, 51, 50);
                ni4.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                ni4.ZOrder = 2;
                ni4.NeedleOffset = new PointF(0.004f, 0.736f);
                ni4.RadiusRatio = new PointF(1f, 0.58f);
                ni4.LengthFactor = 1.55f;
                ni4.ScaleStrokeWidth = true;
                ni4.DisplayGroup = 1;
                //ni.Setup = true;
                ni4.ScaleCalibration.Add(0f, new PointF(0.099f, 0.352f));
                ni4.ScaleCalibration.Add(5f, new PointF(0.164f, 0.312f));
                ni4.ScaleCalibration.Add(10f, new PointF(0.224f, 0.28f));
                ni4.ScaleCalibration.Add(25f, new PointF(0.335f, 0.236f));
                ni4.ScaleCalibration.Add(30f, new PointF(0.367f, 0.228f));
                ni4.ScaleCalibration.Add(40f, new PointF(0.436f, 0.22f));
                ni4.ScaleCalibration.Add(50f, new PointF(0.499f, 0.212f));
                ni4.ScaleCalibration.Add(60f, new PointF(0.559f, 0.216f));
                ni4.ScaleCalibration.Add(100f, new PointF(0.751f, 0.272f));
                ni4.ScaleCalibration.Add(150f, new PointF(0.899f, 0.352f));
                ni4.Value = 0f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni4);

                clsNeedleItem ni5 = new clsNeedleItem();
                ni5.ParentID = ig.ID;
                ni5.TopLeft = ni.TopLeft;
                ni5.Size = ni.Size;
                ni5.OnlyWhenTX = true;
                ni5.ReadingSource = Reading.SWR;
                ni5.AttackRatio = 0.2f;//0.325f;
                ni5.DecayRatio = 0.1f;//0.5f;
                ni5.UpdateInterval = nMSupdate;
                ni5.HistoryDuration = 1000;
                ni5.ShowHistory = true;
                ni5.Style = clsNeedleItem.NeedleStyle.Line;
                ni5.Colour = System.Drawing.Color.Black;
                ni5.HistoryColour = System.Drawing.Color.FromArgb(64, System.Drawing.Color.CornflowerBlue);
                ni5.ZOrder = 3;
                ni5.NeedleOffset = new PointF(0.004f, 0.736f);
                ni5.RadiusRatio = new PointF(1f, 0.58f);
                ni5.LengthFactor = 1.36f;
                ni5.ScaleStrokeWidth = true;
                ni5.DisplayGroup = 1;
                ni5.ScaleCalibration.Add(1f, new PointF(0.152f, 0.468f));
                ni5.ScaleCalibration.Add(1.5f, new PointF(0.28f, 0.404f));
                ni5.ScaleCalibration.Add(2f, new PointF(0.393f, 0.372f));
                ni5.ScaleCalibration.Add(2.5f, new PointF(0.448f, 0.36f));
                ni5.ScaleCalibration.Add(3f, new PointF(0.499f, 0.36f));
                ni5.ScaleCalibration.Add(10f, new PointF(0.847f, 0.476f));
                ni5.Value = 1f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni5);

                clsNeedleItem ni6 = new clsNeedleItem();
                ni6.ParentID = ig.ID;
                ni6.TopLeft = ni.TopLeft;
                ni6.Size = ni.Size;
                ni6.OnlyWhenTX = true;
                ni6.ReadingSource = Reading.ALC_G; // alc_comp
                ni6.AttackRatio = 0.2f;//0.325f;
                ni6.DecayRatio = 0.1f;//0.5f;
                ni6.UpdateInterval = nMSupdate;
                ni6.HistoryDuration = 1000;
                ni6.ShowHistory = false;
                ni6.Style = clsNeedleItem.NeedleStyle.Line;
                ni6.Colour = System.Drawing.Color.Black;
                ni6.ZOrder = 3;
                ni6.NeedleOffset = new PointF(0.004f, 0.736f);
                ni6.RadiusRatio = new PointF(1f, 0.58f);
                ni6.LengthFactor = 0.96f;
                ni6.ScaleStrokeWidth = true;
                ni6.DisplayGroup = 2;
                ni6.ScaleCalibration.Add(0f, new PointF(0.249f, 0.68f));
                ni6.ScaleCalibration.Add(5f, new PointF(0.342f, 0.64f));
                ni6.ScaleCalibration.Add(10f, new PointF(0.425f, 0.624f));
                ni6.ScaleCalibration.Add(15f, new PointF(0.499f, 0.62f));
                ni6.ScaleCalibration.Add(20f, new PointF(0.571f, 0.628f));
                ni6.ScaleCalibration.Add(25f, new PointF(0.656f, 0.64f));
                ni6.ScaleCalibration.Add(30f, new PointF(0.751f, 0.688f));
                ni6.Value = 0f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni6);

                clsNeedleItem ni7 = new clsNeedleItem();
                ni7.ParentID = ig.ID;
                ni7.TopLeft = ni.TopLeft;
                ni7.Size = ni.Size;
                ni7.OnlyWhenTX = true;
                ni7.ReadingSource = Reading.ALC_GROUP;
                ni7.AttackRatio = 0.2f;//0.325f;
                ni7.DecayRatio = 0.1f;//0.5f;
                ni7.UpdateInterval = nMSupdate;
                ni7.HistoryDuration = 1000;
                ni7.ShowHistory = false;
                ni7.Style = clsNeedleItem.NeedleStyle.Line;
                ni7.Colour = System.Drawing.Color.Red;
                //ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                ni7.ZOrder = 3;
                ni7.NeedleOffset = new PointF(0.004f, 0.736f);
                ni7.RadiusRatio = new PointF(1f, 0.58f);
                ni7.LengthFactor = 0.75f;
                ni7.ScaleStrokeWidth = true;
                ni7.DisplayGroup = 3;
                //ni.Setup = true;
                ni7.ScaleCalibration.Add(-30f, new PointF(0.295f, 0.804f));
                ni7.ScaleCalibration.Add(0f, new PointF(0.332f, 0.784f));
                ni7.ScaleCalibration.Add(25f, new PointF(0.499f, 0.756f));
                ni7.Value = -30f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni7);
                //

                clsImage img = new clsImage();
                img.ParentID = ig.ID;
                img.TopLeft = ni.TopLeft;
                img.Size = ni.Size;
                img.ZOrder = 1;
                img.ImageName = "kenwood-s-meter";
                addMeterItem(img);

                img = new clsImage();
                img.ParentID = ig.ID;
                img.TopLeft = new PointF(ni.TopLeft.X, ni.TopLeft.Y + ni.Size.Height);
                img.Size = new SizeF(1f, 0.101f); // image x to y ratio
                img.ZOrder = 5;
                img.ImageName = "kenwood-s-meter-bg";
                addMeterItem(img);

                fBottom = img.TopLeft.Y + img.Size.Height;

                ig.TopLeft = ni.TopLeft;
                ig.Size = new SizeF(ni.Size.Width, fBottom);
                ig.MeterType = MeterType.KENWOOD;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                AddDisplayGroup("ALL"); //0
                AddDisplayGroup("PWR/SWR"); //1
                AddDisplayGroup("Comp"); //2
                AddDisplayGroup("ALC"); //3
                AddDisplayGroup("Volts/Amps"); //4
                DisplayGroup = 1;

                return ni.ID;
            }
            public string AddCrossNeedle(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsNeedleItem ni = new clsNeedleItem();
                ni.ParentID = ig.ID;
                ni.TopLeft = new PointF(0f, fTop + _fPadY - (_fHeight * 0.75f));
                ni.Size = new SizeF(1f, 0.782f); // image x to y ratio
                ni.ReadingSource = Reading.PWR;
                ni.AttackRatio = 0.2f;
                ni.DecayRatio = 0.1f;
                ni.UpdateInterval = nMSupdate;
                ni.HistoryDuration = 1000;
                ni.ShowHistory = true;
                ni.StrokeWidth = 2.5f;
                ni.Style = clsNeedleItem.NeedleStyle.Line;
                ni.Colour = System.Drawing.Color.Black;
                ni.HistoryColour = System.Drawing.Color.FromArgb(96, System.Drawing.Color.Red);
                ni.ZOrder = 4;
                ni.NeedleOffset = new PointF(0.322f, 0.611f);
                ni.LengthFactor = 1.62f;
                ni.ScaleStrokeWidth = true;
                ni.ScaleCalibration.Add(0f, new PointF(0.052f, 0.732f));
                ni.ScaleCalibration.Add(5f, new PointF(0.146f, 0.528f));
                ni.ScaleCalibration.Add(10f, new PointF(0.188f, 0.434f));
                ni.ScaleCalibration.Add(15f, new PointF(0.235f, 0.387f));
                ni.ScaleCalibration.Add(20f, new PointF(0.258f, 0.338f));
                ni.ScaleCalibration.Add(25f, new PointF(0.303f, 0.313f));
                ni.ScaleCalibration.Add(30f, new PointF(0.321f, 0.272f));
                ni.ScaleCalibration.Add(35f, new PointF(0.361f, 0.257f));
                ni.ScaleCalibration.Add(40f, new PointF(0.381f, 0.223f));
                ni.ScaleCalibration.Add(50f, new PointF(0.438f, 0.181f));
                ni.ScaleCalibration.Add(60f, new PointF(0.483f, 0.155f));
                ni.ScaleCalibration.Add(70f, new PointF(0.532f, 0.13f));
                ni.ScaleCalibration.Add(80f, new PointF(0.577f, 0.111f));
                ni.ScaleCalibration.Add(90f, new PointF(0.619f, 0.098f));
                ni.ScaleCalibration.Add(100f, new PointF(0.662f, 0.083f));
                ni.NormaliseTo100W = true;
                ni.Value = 0f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni);

                clsNeedleItem ni2 = new clsNeedleItem();
                ni2.ParentID = ig.ID;
                ni2.TopLeft = ni.TopLeft;
                ni2.Size = ni.Size;
                ni2.ReadingSource = Reading.REVERSE_PWR;
                ni2.AttackRatio = 0.2f;//0.325f;
                ni2.DecayRatio = 0.1f;//0.5f;
                ni2.UpdateInterval = nMSupdate;
                ni2.HistoryDuration = 1000;
                ni2.ShowHistory = true;
                ni2.StrokeWidth = 2.5f;
                ni2.Direction = clsNeedleItem.NeedleDirection.CounterClockwise;
                ni2.Style = clsNeedleItem.NeedleStyle.Line;
                ni2.Colour = System.Drawing.Color.Black;
                ni2.HistoryColour = System.Drawing.Color.FromArgb(96, System.Drawing.Color.CornflowerBlue);
                ni2.ZOrder = 3;
                ni2.NeedleOffset = new PointF(-0.322f, 0.611f);
                ni2.LengthFactor = 1.62f;
                ni2.ScaleStrokeWidth = true;
                ni2.ScaleCalibration.Add(0f, new PointF(0.948f, 0.74f));
                ni2.ScaleCalibration.Add(0.25f, new PointF(0.913f, 0.7f));
                ni2.ScaleCalibration.Add(0.5f, new PointF(0.899f, 0.638f));
                ni2.ScaleCalibration.Add(0.75f, new PointF(0.875f, 0.594f));
                ni2.ScaleCalibration.Add(1f, new PointF(0.854f, 0.538f));
                ni2.ScaleCalibration.Add(2f, new PointF(0.814f, 0.443f));
                ni2.ScaleCalibration.Add(3f, new PointF(0.769f, 0.4f));
                ni2.ScaleCalibration.Add(4f, new PointF(0.744f, 0.351f));
                ni2.ScaleCalibration.Add(5f, new PointF(0.702f, 0.321f));
                ni2.ScaleCalibration.Add(6f, new PointF(0.682f, 0.285f));
                ni2.ScaleCalibration.Add(7f, new PointF(0.646f, 0.268f));
                ni2.ScaleCalibration.Add(8f, new PointF(0.626f, 0.234f));
                ni2.ScaleCalibration.Add(9f, new PointF(0.596f, 0.228f));
                ni2.ScaleCalibration.Add(10f, new PointF(0.569f, 0.196f));
                ni2.ScaleCalibration.Add(12f, new PointF(0.524f, 0.166f));
                ni2.ScaleCalibration.Add(14f, new PointF(0.476f, 0.14f));
                ni2.ScaleCalibration.Add(16f, new PointF(0.431f, 0.121f));
                ni2.ScaleCalibration.Add(18f, new PointF(0.393f, 0.109f));
                ni2.ScaleCalibration.Add(20f, new PointF(0.349f, 0.098f));
                ni2.NormaliseTo100W = true;
                ni2.Value = 0f;
                //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                addMeterItem(ni2);

                clsImage img = new clsImage();
                img.ParentID = ig.ID;
                img.TopLeft = ni.TopLeft;
                img.Size = ni.Size;
                img.ZOrder = 1;
                img.ImageName = "cross-needle";
                addMeterItem(img);

                clsImage img2 = new clsImage();
                img2.ParentID = ig.ID;
                img2.TopLeft = new PointF(ni.TopLeft.X, ni.TopLeft.Y + ni.Size.Height - 0.180f); // it goes too high if we move it up by 0.217f
                img2.Size = new SizeF(1f, 0.217f);
                img2.ZOrder = 5;
                img2.ImageName = "cross-needle-bg";
                addMeterItem(img2);

                fBottom = img2.TopLeft.Y + img2.Size.Height;

                ig.TopLeft = ni.TopLeft;
                ig.Size = new SizeF(ni.Size.Width, fBottom);
                ig.MeterType = MeterType.CROSS;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return ni.ID;
            }
            public string AddMicBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.MIC;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = false;
                cb.MarkerColour = System.Drawing.Color.DarkGray;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ShowValue = false;
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.MIC_PK;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = true;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb2.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb2.ZOrder = 3;
                cb2.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.MIC;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddEQBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.EQ;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = false;
                cb.MarkerColour = System.Drawing.Color.DarkGray;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.CornflowerBlue);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.ShowValue = false;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.EQ_PK;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = true;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.CornflowerBlue);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb2.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb2.ZOrder = 3;
                cb2.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.ParentID = ig.ID;
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.EQ;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddLevelerBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.LEVELER;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = false;
                cb.MarkerColour = System.Drawing.Color.DarkGray;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Purple);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.ShowValue = false;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.LEVELER_PK;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = true;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Purple);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb2.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb2.ZOrder = 3;
                cb2.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.LEVELER;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddLevelerGainBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.LVL_G;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Purple);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(0, new PointF(0, 0));
                cb.ScaleCalibration.Add(20, new PointF(0.8f, 0));
                cb.ScaleCalibration.Add(25, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.LEVELER_GAIN;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddALCBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.ALC;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = false;
                cb.MarkerColour = System.Drawing.Color.DarkGray;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.LemonChiffon);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.ShowValue = false;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.ALC_PK;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = true;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.LemonChiffon);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb2.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb2.ZOrder = 3;
                cb2.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.ALC;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddALCGainBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.ALC_G;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.LemonChiffon);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(0, new PointF(0, 0));
                cb.ScaleCalibration.Add(20, new PointF(0.8f, 0));
                cb.ScaleCalibration.Add(25, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.ALC_GAIN;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddALCGroupBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.ALC_GROUP;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.LemonChiffon);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.5f, 0));
                cb.ScaleCalibration.Add(25, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.ALC_GROUP;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddCFCBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.CFC_PK;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.PaleTurquoise);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.CFC_G;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = false;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.PaleTurquoise);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(0, new PointF(0, 0));
                cb2.ScaleCalibration.Add(20, new PointF(0.8f, 0));
                cb2.ScaleCalibration.Add(25, new PointF(0.99f, 0));
                cb2.ZOrder = 2;
                cb2.FontColour = System.Drawing.Color.Yellow;
                cb2.ShowValue = false;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.CFC;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddCompBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.COMP;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = false;
                cb.MarkerColour = System.Drawing.Color.DarkGray;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.PeachPuff);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb.ZOrder = 2;
                cb.ShowValue = false;
                addMeterItem(cb);

                clsBarItem cb2 = new clsBarItem();
                cb2.ParentID = ig.ID;
                cb2.TopLeft = cb.TopLeft;
                cb2.Size = cb.Size;
                cb2.ReadingSource = Reading.COMP_PK;
                cb2.AttackRatio = 0.8f;
                cb2.DecayRatio = 0.1f;
                cb2.UpdateInterval = nMSupdate;
                cb2.HistoryDuration = 2000;
                cb2.ShowHistory = true;
                cb2.MarkerColour = System.Drawing.Color.Yellow;
                cb2.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.PeachPuff);
                cb2.Style = clsBarItem.BarStyle.Line;
                cb2.ScaleCalibration.Add(-30, new PointF(0, 0));
                cb2.ScaleCalibration.Add(0, new PointF(0.665f, 0));
                cb2.ScaleCalibration.Add(12, new PointF(0.99f, 0));
                cb2.ZOrder = 3;
                cb2.FontColour = System.Drawing.Color.Yellow;
                addMeterItem(cb2);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.COMP;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddPWRBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                return AddPWRBar(nMSupdate, fTop, out fBottom, Reading.PWR, restoreIg);
            }
            public string AddREVPWRBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                return AddPWRBar(nMSupdate, fTop, out fBottom, Reading.REVERSE_PWR, restoreIg);
            }
            public string AddPWRBar(int nMSupdate, float fTop, out float fBottom, Reading reading,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = reading;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                cb.Style = clsBarItem.BarStyle.Line;

                switch (CurrentPowerRating)
                {
                    case 500:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(50, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(100, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(250, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(500, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(600, new PointF(0.99f, 0));
                        }
                        break;
                    case 200:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(10, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(20, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(100, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(200, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(240, new PointF(0.99f, 0));
                        }
                        break;
                    case 100:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(5, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(10, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(50, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(100, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(120, new PointF(0.99f, 0));
                        }
                        break;
                    case 30:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(5, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(10, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(20, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(30, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(50, new PointF(0.99f, 0));
                        }
                        break;
                    case 15:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(1, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(5, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(10, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(15, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(25, new PointF(0.99f, 0));
                        }
                        break;
                    case 1:
                        {
                            cb.ScaleCalibration.Add(0, new PointF(0, 0));
                            cb.ScaleCalibration.Add(0.1f, new PointF(0.1875f, 0));
                            cb.ScaleCalibration.Add(0.25f, new PointF(0.375f, 0));
                            cb.ScaleCalibration.Add(0.5f, new PointF(0.5625f, 0));
                            cb.ScaleCalibration.Add(0.8f, new PointF(0.75f, 0));
                            cb.ScaleCalibration.Add(1f, new PointF(0.99f, 0));

                        }
                        break;
                }

                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                if (reading == Reading.PWR)
                    ig.MeterType = MeterType.PWR;
                else
                    ig.MeterType = MeterType.REVERSE_PWR;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            public string AddSWRBar(int nMSupdate, float fTop, out float fBottom,  clsItemGroup restoreIg = null)
            {
                clsItemGroup ig = new clsItemGroup();
                if(restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsBarItem cb = new clsBarItem();
                cb.ParentID = ig.ID;
                cb.TopLeft = new PointF(_fPadX, fTop + _fPadY);
                cb.Size = new SizeF(1f - _fPadX * 2f, _fHeight);
                cb.ReadingSource = Reading.SWR;
                cb.AttackRatio = 0.8f;
                cb.DecayRatio = 0.1f;
                cb.UpdateInterval = nMSupdate;
                cb.HistoryDuration = 2000;
                cb.ShowHistory = true;
                cb.MarkerColour = System.Drawing.Color.Yellow;
                cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Orange);
                cb.Style = clsBarItem.BarStyle.Line;
                cb.ScaleCalibration.Add(1, new PointF(0, 0));
                cb.ScaleCalibration.Add(1.5f, new PointF(0.25f, 0));
                cb.ScaleCalibration.Add(2, new PointF(0.5f, 0));
                cb.ScaleCalibration.Add(3, new PointF(0.75f, 0));
                cb.ScaleCalibration.Add(5, new PointF(0.99f, 0));
                cb.FontColour = System.Drawing.Color.Yellow;
                cb.ZOrder = 2;
                addMeterItem(cb);

                clsScaleItem cs = new clsScaleItem();
                cs.ParentID = ig.ID;
                cs.TopLeft = cb.TopLeft;
                cs.Size = cb.Size;
                cs.ReadingSource = cb.ReadingSource;
                cs.ZOrder = 4;
                cs.ShowType = true;
                addMeterItem(cs);

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(cb.TopLeft.X, cb.TopLeft.Y - _fHeight * 0.75f);
                sc.Size = new SizeF(cb.Size.Width, _fHeight + _fHeight * 0.75f);
                sc.Colour = System.Drawing.Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                fBottom = cb.TopLeft.Y + cb.Size.Height;

                ig.TopLeft = cb.TopLeft;
                ig.Size = new SizeF(cb.Size.Width, fBottom);
                ig.MeterType = MeterType.SWR;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;

                addMeterItem(ig);

                return cb.ID;
            }
            #endregion
            public clsMeter(int rx, string sName, float XRatio = 1f, float YRatio = 1f, bool mox = false)
            {
                // constructor
                _sId = Guid.NewGuid().ToString();
                _rx = rx;
                _name = sName;
                _mox = mox;
                _quickestRXUpdate = 500;
                _quickestTXUpdate = 500;

                _fPadX = 0.02f;
                _fPadY = 0.05f;
                _fHeight = 0.05f;

                _XRatio = XRatio;
                _YRatio = YRatio;

                _meterItems = new Dictionary<string, clsMeterItem>();
                _sortedMeterItemsForZOrder = null; // only after setupSortedZOrder is called
                _displayGroups = new List<string>();
                _displayGroup = 0;
            }
            public void ZeroOut(bool zeroTxReadings)
            {
                lock (_objMeterItemLock)
                {
                    foreach (KeyValuePair<string, clsMeterItem> mis in _meterItems)
                    {
                        clsMeterItem mi = mis.Value;

                        if (!zeroTxReadings && (
                            mi.ReadingSource == Reading.SIGNAL_STRENGTH ||
                            mi.ReadingSource == Reading.AVG_SIGNAL_STRENGTH ||
                            mi.ReadingSource == Reading.ADC_PK ||
                            mi.ReadingSource == Reading.ADC_AV ||
                            mi.ReadingSource == Reading.AGC_PK ||
                            mi.ReadingSource == Reading.AGC_AV ||
                            mi.ReadingSource == Reading.AGC_GAIN ||
                            mi.ReadingSource == Reading.ESTIMATED_PBSNR
                            ))
                        {
                            float lowValue = -200f;
                            if (mi.ScaleCalibration.Count > 0)
                                lowValue = mi.ScaleCalibration.First().Key;

                            setReading(RX, mi.ReadingSource, lowValue);
                        }
                        else if (zeroTxReadings && (
                            mi.ReadingSource == Reading.MIC ||
                            mi.ReadingSource == Reading.MIC_PK ||
                            mi.ReadingSource == Reading.EQ ||
                            mi.ReadingSource == Reading.EQ_PK ||
                            mi.ReadingSource == Reading.LEVELER ||
                            mi.ReadingSource == Reading.LEVELER_PK ||
                            mi.ReadingSource == Reading.LVL_G ||
                            mi.ReadingSource == Reading.CFC_G ||
                            mi.ReadingSource == Reading.CFC_PK ||
                            //mi.ReadingSource == Reading.CPDR ||
                            //mi.ReadingSource == Reading.CPDR_PK ||
                            mi.ReadingSource == Reading.COMP ||
                            mi.ReadingSource == Reading.COMP_PK ||
                            mi.ReadingSource == Reading.ALC ||
                            mi.ReadingSource == Reading.ALC_PK ||
                            mi.ReadingSource == Reading.ALC_G ||
                            mi.ReadingSource == Reading.ALC_GROUP ||
                            mi.ReadingSource == Reading.PWR ||
                            mi.ReadingSource == Reading.REVERSE_PWR ||
                            mi.ReadingSource == Reading.SWR
                            ))
                        {
                            float lowValue = -200f;
                            if (mi.ScaleCalibration.Count > 0)
                                lowValue = mi.ScaleCalibration.First().Key;

                            setReading(RX, mi.ReadingSource, lowValue);
                        }                        
                    }
                }
            }
            public bool TryParse(string str)
            {
                bool bOk = false;
                int rx=0, displaygroup=0;
                float xRatio=0, yRatio=0, fPadX=0, fPadY=0, fHeight=0;

                if (str != "")
                {
                    string[] tmp = str.Split('|');
                    if (tmp.Length == 9)
                    {
                        bOk = tmp[0] != "";
                        if (bOk) ID = tmp[0];
                        if (bOk) _name = tmp[1];
                        if (bOk) int.TryParse(tmp[2], out rx);
                        if (bOk) RX = rx;
                        if (bOk) bOk = float.TryParse(tmp[3], out xRatio);
                        if (bOk) bOk = float.TryParse(tmp[4], out yRatio);
                        if (bOk)
                        {
                            _XRatio = xRatio;
                            _YRatio = yRatio;
                        }
                        if (bOk) int.TryParse(tmp[5], out displaygroup);
                        if (bOk) _displayGroup = displaygroup;
                        if (bOk) bOk = float.TryParse(tmp[6], out fPadX);
                        if (bOk) bOk = float.TryParse(tmp[7], out fPadY);
                        if (bOk) bOk = float.TryParse(tmp[8], out fHeight);
                        if (bOk)
                        {
                            _fPadX = fPadX;
                            _fPadY = fPadY;
                            _fHeight = fHeight;
                        }

                    }
                }

                return bOk;
            }
            private int numberOfMeterGroups()
            {
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return 0;

                    Dictionary<string, clsMeterItem> items = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);

                    return items.Count;
                }
            }
            private void removeMeterItem(string sId, bool bRebuild = false)
            {
                lock (_objMeterItemLock)
                {
                    // remove this item, plus all items that are children

                    Dictionary<string, clsMeterItem> items = itemsFromID(sId);
                    if (items == null || items.Count == 0) return;

                    foreach (KeyValuePair<string, clsMeterItem> kvp in items)
                    {
                        _meterItems.Remove(kvp.Value.ID);
                    }

                    if (bRebuild) Rebuild();
                }
            }
            public void RemoveMeterType(MeterType mt, bool bRebuild = false)
            {
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return;

                    //special for kenwood
                    if(mt == MeterType.KENWOOD)
                    {
                        RemoveDisplayGroup("ALL");
                        RemoveDisplayGroup("PWR/SWR");
                        RemoveDisplayGroup("Comp");
                        RemoveDisplayGroup("ALC"); 
                        RemoveDisplayGroup("Volts/Amps");
                    }
                    //
                    Dictionary<string, clsMeterItem> items = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                    foreach(KeyValuePair<string, clsMeterItem> kvp in items)
                    {
                        int nOrder = -1;

                        clsItemGroup ig = kvp.Value as clsItemGroup;
                        if (ig != null && ig.MeterType == mt) {
                            nOrder = ig.Order;
                            removeMeterItem(ig.ID, false);
                        }

                        if(nOrder >= 0)
                        {
                            Dictionary<string, clsMeterItem> tmpItems = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                            foreach(KeyValuePair<string, clsMeterItem> tmpKvp in tmpItems){
                                clsItemGroup tmpIg = tmpKvp.Value as clsItemGroup;
                                if (tmpIg != null && tmpIg.Order > nOrder)
                                    tmpIg.Order--;
                            }
                        }
                    }

                    if (bRebuild) Rebuild();
                }
            }
            public bool HasMeterType(MeterType mt)
            {
                lock (_objMeterItemLock)
                { 
                    if (_meterItems == null) return false;

                    //IEnumerable<KeyValuePair<string, clsMeterItem>> items = _meterItems.Where(o => o.Value.ItemType == MeterItemType.ITEM_GROUP);
                    Dictionary<string, clsMeterItem> items = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                    foreach (KeyValuePair<string, clsMeterItem> kvp in items)
                    {
                        clsItemGroup ig = kvp.Value as clsItemGroup;
                        if (ig != null && ig.MeterType == mt) return true;
                    }

                    return false;
                }
            }
            public int GetOrderForMeterType(MeterType mt)
            {
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return -1;

                    Dictionary<string, clsMeterItem> items = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                    foreach (KeyValuePair<string, clsMeterItem> kvp in items)
                    {
                        clsItemGroup ig = kvp.Value as clsItemGroup;
                        if (ig != null && ig.MeterType == mt) return ig.Order;
                    }

                    return -1;
                }
            }
            public void SetOrderForMeterType(MeterType mt, int nOrder, bool bRebuild, bool bUp)
            {
                // only works for up/down 1 place
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return;

                    if (nOrder < 0) nOrder = 0;
                    int nTmp = numberOfMeterGroups() - 1;
                    if (nOrder > nTmp) nOrder = nTmp;

                    Dictionary<string, clsMeterItem> items = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                    foreach (KeyValuePair<string, clsMeterItem> kvp in items)
                    {
                        clsItemGroup ig = kvp.Value as clsItemGroup;
                        if (ig != null)
                        {
                            if (ig.MeterType == mt)
                            {
                                ig.Order = nOrder;
                            }
                            else if (ig.Order == nOrder)
                            {
                                if (bUp)
                                    ig.Order++;
                                else
                                    ig.Order--;
                            }
                        }
                    }
                }
                if (bRebuild) Rebuild();
            }
            private clsMeterItem itemFromID(string sId)
            {
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return null;
                    if (!_meterItems.ContainsKey(sId)) return null;

                    return _meterItems[sId];
                }
            }
            internal Dictionary<string, clsMeterItem> itemsFromID(string sId, bool bIncludeParent = true)
            {
                // obtains all items that have given ID and also the parent
                lock (_objMeterItemLock)
                {
                    if (_meterItems == null) return null;
                    if (!_meterItems.ContainsKey(sId)) return null;

                    Dictionary<string, clsMeterItem> lst = new Dictionary<string, clsMeterItem>();

                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems.Where(o => (o.Value.ParentID == sId) || (bIncludeParent && o.Value.ID == sId)))
                        lst.Add(kvp.Key, kvp.Value);

                    return lst;
                }
            }
            private bool hasReading(Reading reading)
            {
                bool bRet = false;
                foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems)
                {
                    if(kvp.Value.ReadingSource == reading)
                    {
                        bRet = true;
                        break;
                    }
                }
                return bRet;
            }
            public float GetBottom()
            {
                lock (_objMeterItemLock)
                {
                    float fBottom = 0;

                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems)
                    {
                        float fTmp = kvp.Value.TopLeft.Y + kvp.Value.Size.Height;
                        if (fTmp > fBottom) fBottom = fTmp;
                    }

                    return fBottom;
                }
            }
            public int QuickestRXUpdate
            {
                get { return _quickestRXUpdate; }
            }
            public int QuickestTXUpdate
            {
                get { return _quickestTXUpdate; }
            }
            internal Dictionary<string, clsItemGroup> getMeterGroups()
            {
                Dictionary<string, clsMeterItem> meterItems = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                Dictionary<string, clsItemGroup> groupItems = new Dictionary<string, clsItemGroup>();

                foreach (KeyValuePair<string, clsMeterItem> kvp in meterItems)
                {
                    groupItems.Add(kvp.Value.ID, (clsItemGroup)kvp.Value);
                }

                return groupItems;
            }
            public void Rebuild()
            {
                _quickestRXUpdate = QuickestUpdateInterval(false);
                _quickestTXUpdate = QuickestUpdateInterval(true);

                setupSortedZOrder();

                lock (_objMeterItemLock)
                {
                    // shift top
                    Dictionary<string, clsMeterItem> meterItems = _meterItems.Where(o => o.Value.ItemType == clsMeterItem.MeterItemType.ITEM_GROUP).ToDictionary(x => x.Key, x => x.Value);
                    Dictionary<string, clsItemGroup> groupItems = getMeterGroups();

                    float fTop = 0;

                    foreach (KeyValuePair<string, clsItemGroup> kvp in groupItems.OrderBy(o => o.Value.Order))
                    {
                        clsItemGroup ig = kvp.Value;

                        if (ig != null)
                        {
                            ig.TopLeft = new PointF(ig.TopLeft.X, fTop);

                            Dictionary<string, clsMeterItem> itemsInGroup = itemsFromID(ig.ID);

                            foreach (KeyValuePair<string, clsMeterItem> kvp2 in itemsInGroup)
                            {
                                clsMeterItem mi = kvp2.Value;
                                mi.DisplayTopLeft = new PointF(mi.TopLeft.X, mi.TopLeft.Y + fTop);
                            }

                            fTop += ig.Size.Height;
                        }
                    }
                }
            }
            private void setupSortedZOrder()
            {
                lock (_objMeterItemLock)
                {
                    _sortedMeterItemsForZOrder = _meterItems.OrderBy(o => o.Value.ZOrder).ToDictionary(x => x.Key, x => x.Value);
                }
            }
            internal void MouseUp(System.Windows.Forms.MouseEventArgs e, clsMeter m, clsClickBox cb)
            {
                if (cb.GotoGroup > 0)
                {
                    m.DisplayGroup = cb.GotoGroup;
                }
                else
                {
                    if (e.Button == MouseButtons.Left)
                        IncrementDisplayGroup();
                    else if (e.Button == MouseButtons.Right)
                        DecrementDisplayGroup();
                }
            }
            public bool MOX
            {
                get { return _mox; }
                set { _mox = value; }
            }
            public float PadX { get { return _fPadX; } set { _fPadX = value; } }
            public float PadY { get { return _fPadY; } set { _fPadY = value; } }
            public float Height { get { return _fHeight; } set { _fHeight = value; } }

            public float XRatio { get { return _XRatio; } set { _XRatio = value; } }
            public float YRatio { get { return _YRatio; } set { _YRatio = value; } }
            public string Name { get { return _name; } set { _name = value; } }            
            public string ID
            { 
                get { return _sId; } 
                set { _sId = value; }
            }
            public int RX { get { return _rx; } set { _rx = value; } }
            public int QuickestUpdateInterval(bool mox)
            {
                lock (_objMeterItemLock)
                {
                    int updateInterval = int.MaxValue;
                    foreach (var kvp in _meterItems.Where(o => ((mox && o.Value.OnlyWhenTX) || (!mox && o.Value.OnlyWhenRX)) || (!o.Value.OnlyWhenTX && !o.Value.OnlyWhenRX)))
                    {
                        clsMeterItem mi = kvp.Value;
                        if (mi.UpdateInterval < updateInterval) updateInterval = mi.UpdateInterval;
                    }
                    return updateInterval;
                }
            }
            public void Update(ref List<Reading> readingsUsed)
            {
                lock (_objMeterItemLock)
                {
                    // this is called for each meter from the meter manager thread
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems)
                    {
                        // now we need to update each item in the meter
                        clsMeterItem mi = kvp.Value;
                        if (mi.RequiresUpdate) mi.Update(_rx, ref readingsUsed);
                    }
                }
            }
            public int DelayForUpdate()
            {
                lock (_objMeterItemLock)
                {
                    // this is called for each meter from the meter manager thread
                    int nDelay = 5000;
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems)
                    {
                        clsMeterItem mi = kvp.Value;
                        int nTmp = mi.DelayUntilNextUpdate;
                        if (nTmp < nDelay) nDelay = nTmp;
                    }
                    return nDelay;
                }
            }
            public Dictionary<string, clsMeterItem> SortedMeterItemsForZOrder
            {
                get {
                    lock (_objMeterItemLock)
                    {
                        return _sortedMeterItemsForZOrder;
                    }
                }
            }
            public void IncrementDisplayGroup()
            {
                _displayGroup++;
                if (_displayGroup > _displayGroups.Count - 1) _displayGroup = 0;
            }
            public void DecrementDisplayGroup()
            {
                _displayGroup--;
                if (_displayGroup < 0) _displayGroup = _displayGroups.Count - 1;
            }
            public int DisplayGroups
            {
                get { return _displayGroups.Count; }
            }
            public int DisplayGroup
            {
                get { return _displayGroup; }
                set
                {
                    _displayGroup = value;
                    if (_displayGroup < 0) _displayGroup = 0;
                    if (_displayGroup > _displayGroups.Count - 1) _displayGroup = _displayGroups.Count - 1;
                }
            }
            public void AddDisplayGroup(string sName)
            {
                if (_displayGroups.Contains(sName)) return;

                _displayGroups.Add(sName);
                _displayGroup = 0; // 0 is all
            }
            public void RemoveDisplayGroup(string sName)
            {
                if (!_displayGroups.Contains(sName)) return;

                _displayGroups.Remove(sName);
                _displayGroup = 0; // 0 is all
            }
            public string DisplayGroupText
            {
                get
                {
                    if (_displayGroup < 0) return "";
                    if (_displayGroup > _displayGroups.Count - 1) return "";

                    return _displayGroups[_displayGroup];
                }
            }
            public override string ToString()
            {
                string sRet;

                sRet = _sId + "|" +
                    _name + "|" +
                    _rx.ToString() + "|" +
                    _XRatio.ToString("f4") + "|" +
                    _YRatio.ToString("f4") + "|" +
                    _displayGroup.ToString() + "|" +
                    _fPadX.ToString("f4") + "|" +
                    _fPadY.ToString("f4") + "|" +
                    _fHeight.ToString("f4");

                return sRet;
            }
        }
        #endregion
        #region clsReading
        private class clsReading
        {
            public Reading Type;
            public float Reading;
            public bool Updated;
        }
        private class clsReadings
        {
            private Dictionary<Reading, clsReading> _latestReading;
            public clsReadings()
            {
                _latestReading = new Dictionary<Reading, clsReading>();

                for (int n = 0; n < (int)Reading.LAST; n++)
                {
                    clsReading cr = new clsReading() { Type = (Reading)n, Reading = -200f, Updated = false };
                    _latestReading.Add((Reading)n, cr);
                }
            }
            public float GetReading(Reading rt, bool useReading = false)
            {
                if (rt == Reading.NONE) return -200f;
                if (useReading) UseReading(rt);
                return _latestReading[rt].Reading;
            }
            public void SetReading(Reading rt, float value)
            {
                if (rt == Reading.NONE) return;
                _latestReading[rt].Reading = value;
                _latestReading[rt].Updated = true;
            }
            public bool RequiresUpdate(Reading rt)
            {
                if (rt == Reading.NONE) return false;
                return !_latestReading[rt].Updated;
            }
            public void UseReading(Reading rt)
            {
                if (rt == Reading.NONE) return;
                _latestReading[rt].Updated = false;
            }
        }
        #endregion       
        #region DirectX
        //based on display.cs
        private class DXRenderer
        {
            private const SharpDX.Direct2D1.AlphaMode _ALPHA_MODE = SharpDX.Direct2D1.AlphaMode.Premultiplied;
            private bool _bDXSetup;
            private bool _displayRunning;
            private bool _dxDisplayThreadRunning;
            private Thread _dxRenderThread;
            private PresentFlags _NoVSYNCpresentFlag;
            private SharpDX.DXGI.Factory1 _factory1;
            private SharpDX.Direct3D11.Device _device;
            private int _nBufferCount;
            private Surface _surface;
            private SwapChain _swapChain;
            private SwapChain1 _swapChain1;
            private RenderTarget _renderTarget;
            private SharpDX.Direct2D1.Factory _factory;
            private bool _bAntiAlias;
            private Vector2 _pixelShift;
            private int _nVBlanks;
            private PictureBox _displayTarget;
            private object _DXlock = new object();
            //
            private Dictionary<System.Drawing.Color, SharpDX.Direct2D1.Brush> _DXBrushes;
            private Color4 _backColour;
            private Dictionary<string, SharpDX.Direct2D1.Bitmap> _images;
            //
            private Dictionary<string, SharpDX.DirectWrite.TextFormat> _textFormats; // fonts
            private SharpDX.DirectWrite.Factory _fontFactory;
            private Dictionary<string, SizeF> _stringMeasure;
            //

            private int _rx;
            private Console _console;

            //
            //private int _updateInterval = 100;
            private int _nFps = 0;
            private int _nFrameCount = 0;
            private HiPerfTimer _objFrameStartTimer = new HiPerfTimer();
            private double _fLastTime;
            private double _dElapsedFrameStart;
            private string _sId;
            private clsMeter _meter;
            private bool _highlightEdge;

            public DXRenderer(string sId, int rx, PictureBox target, Console c, string sImagePath, clsMeter meter)
            {
                _sId = sId;
                _rx = rx;
                _console = c;
                _meter = meter;
                _highlightEdge = false;

                //dx
                _DXBrushes = new Dictionary<System.Drawing.Color, SharpDX.Direct2D1.Brush>();
                _textFormats = new Dictionary<string, SharpDX.DirectWrite.TextFormat>();
                _stringMeasure = new Dictionary<string, SizeF>();

                _displayRunning = false;
                _bAntiAlias = true;
                _bDXSetup = false;
                _dxDisplayThreadRunning = false;
                _NoVSYNCpresentFlag = PresentFlags.None;
                _pixelShift = new Vector2(0.5f, 0.5f);
                _nVBlanks = 0;
                _displayTarget = target;
                _displayTarget.Tag = sId; // use the tag to hold sId, we can then use this in mouse event OnMouseUp
                _backColour = convertColour(System.Drawing.Color.Black);
                _images = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();

                _fLastTime = _objFrameStartTimer.ElapsedMsec;
                _dElapsedFrameStart = _objFrameStartTimer.ElapsedMsec;

                _displayTarget.Resize += target_Resize;
                _displayTarget.MouseUp += OnMouseUp;

                init();

                loadImages(sImagePath);

                _dxDisplayThreadRunning = false;
                _dxRenderThread = new Thread(new ThreadStart(dxRender))
                {
                    Name = "Multimeter DX Render Thread",
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };
                _dxRenderThread.Start();

                _displayRunning = true;
            }
            public string ID
            {
                get { return _sId; }
            }
            public bool HighlightEdge
            {
                get { return _highlightEdge; }
                set { _highlightEdge = value; }
            }
            //public int UpdateInterval
            //{
            //    get { return _updateInterval; }
            //    set { _updateInterval = value; }
            //}

            //DIRECTX
            private void loadImages(string sImagePath)
            {
                if (!sImagePath.EndsWith("\\")) sImagePath += "\\";

                if (System.IO.Directory.Exists(sImagePath))
                {
                    string[] sFiles = System.IO.Directory.GetFiles(sImagePath);
                    foreach (string sFile in sFiles)
                    {                        
                        loadImage(sFile);
                    }
                }
            }
            private void loadImage(string sFilePath)
            {
                string sID = System.IO.Path.GetFileNameWithoutExtension(sFilePath);

                if (!_images.ContainsKey(sID) && System.IO.File.Exists(sFilePath)) // check contains incase of filename dupe somehow
                {
                    try
                    {
                        if (!MeterManager.ContainsBitmap(sID))
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromFile(sFilePath);
                            System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(image);
                            MeterManager.AddImage(sID, bmp2);                                                            
                        }

                        System.Drawing.Bitmap bmp = MeterManager.GetImage(sID);

                        if (bmp != null)
                        {
                            SharpDX.Direct2D1.Bitmap img = bitmapFromSysBitmap2(_renderTarget, bmp, sID);
                            _images.Add(sID, img);
                        }
                    }
                    catch { }
                }
            }
            private int getMaxSamples()
            {
                //D3D11_MAX_MULTISAMPLE_SAMPLE_COUNT 32u32
                //NOTE: can not be used with FlipDiscard atm unfortunately
                for (int n = 32; n>0; n /= 2)
                {
                    if(_device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, n) > 0)
                    {
                        return n;
                    }
                }
                return 1;
            }
            private void init(DriverType driverType = DriverType.Hardware)
            {
                // code based on display.cs
                if (_bDXSetup) return;

                try
                {
                    DeviceCreationFlags debug = DeviceCreationFlags.None;

                    // to get this to work, need to target the os
                    // https://www.prugg.at/2019/09/09/properly-detect-windows-version-in-c-net-even-windows-10/
                    // you need to enable operating system support in the app1.manifest file, otherwise majVers will not report 10+
                    // note: windows 10, 11, server 2016, server 2019, server 2022 all use the windows 10 os id in the manifest file at this current time
                    int majVers = Environment.OSVersion.Version.Major;
                    int minVers = Environment.OSVersion.Version.Minor;

                    SharpDX.Direct3D.FeatureLevel[] featureLevels;

                    if (majVers >= 10) // win10 + 11
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_12_1,
                            SharpDX.Direct3D.FeatureLevel.Level_12_0,
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers >= 2) // windows 8, windows 8.1
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers < 2) // windows 7, 2008 R2, 2008, vista
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }
                    else
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }

                    _factory1 = new SharpDX.DXGI.Factory1();

                    _device = new SharpDX.Direct3D11.Device(driverType, debug | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded, featureLevels);

                    SharpDX.DXGI.Device1 device1 = _device.QueryInterfaceOrNull<SharpDX.DXGI.Device1>();
                    if (device1 != null)
                    {
                        device1.MaximumFrameLatency = 1;
                        Utilities.Dispose(ref device1);
                        device1 = null;
                    }

                    //this code should ideally be used to prevent use of flip if vsync is 0
                    //but is not used at this time
                    //SharpDX.DXGI.Factory5 f5 = factory.QueryInterfaceOrNull<SharpDX.DXGI.Factory5>();
                    //bool bAllowTearing = false;
                    //if(f5 != null)
                    //{
                    //    int size = Marshal.SizeOf(typeof(bool));
                    //    IntPtr pBool = Marshal.AllocHGlobal(size);

                    //    f5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, pBool, size);

                    //    bAllowTearing = Marshal.ReadInt32(pBool) == 1;

                    //    Marshal.FreeHGlobal(pBool);
                    //}
                    //

                    // check if the device has a factory4 interface
                    // if not, then we need to use old bitplit swapeffect
                    SwapEffect swapEffect;

                    SharpDX.DXGI.Factory4 factory4 = _factory1.QueryInterfaceOrNull<SharpDX.DXGI.Factory4>();
                    bool bFlipPresent = false;
                    if (factory4 != null)
                    {
                        /*if (!_bUseLegacyBuffers)*/ bFlipPresent = true;
                        Utilities.Dispose(ref factory4);
                        factory4 = null;
                    }

                    //https://walbourn.github.io/care-and-feeding-of-modern-swapchains/
                    swapEffect = bFlipPresent ? SwapEffect.FlipDiscard : SwapEffect.Discard; //NOTE: FlipSequential should work, but is mostly used for storeapps
                    _nBufferCount = bFlipPresent ? 2 : 1;

                    //int multiSample = 2; // eg 2 = MSAA_2, 2 times multisampling
                    //int maxQuality = _device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, multiSample) - 1;
                    //maxQuality = Math.Max(0, maxQuality);

                    ModeDescription md = new ModeDescription(_displayTarget.Width, _displayTarget.Height,
                                                               new Rational(60, 1)/*console.DisplayFPS, 1)*/, Format.B8G8R8A8_UNorm);
                    md.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
                    md.Scaling = DisplayModeScaling.Centered;
                    
                    SwapChainDescription desc = new SwapChainDescription()
                    {
                        BufferCount = _nBufferCount,
                        ModeDescription = md,
                        IsWindowed = true,
                        OutputHandle = _displayTarget.Handle,
                        //SampleDescription = new SampleDescription(multiSample, maxQuality),
                        SampleDescription = new SampleDescription(1, 0), // no multi sampling (1 sample), no antialiasing
                        SwapEffect = swapEffect,
                        Usage = Usage.RenderTargetOutput,// | Usage.BackBuffer,  // dont need usage.backbuffer as it is implied
                        Flags = SwapChainFlags.None,
                    };

                    _factory1.MakeWindowAssociation(_displayTarget.Handle, WindowAssociationFlags.IgnoreAll);
                    _swapChain = new SwapChain(_factory1, _device, desc);
                    _swapChain1 = _swapChain.QueryInterface<SwapChain1>();

                    _factory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded, DebugLevel.None);
                    
                    _surface = _swapChain1.GetBackBuffer<Surface>(0);

                    RenderTargetProperties rtp = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));                    
                    _renderTarget = new RenderTarget(_factory, _surface, rtp);

                    if (debug == DeviceCreationFlags.Debug)
                    {
                        _device.DebugName = "MeterDeviceDB_" + _rx.ToString();
                        _swapChain.DebugName = "MeterSwapChainDB_" + _rx.ToString();
                        _swapChain1.DebugName = "MeterSwapChain1DB_" + _rx.ToString();
                        _surface.DebugName = "MeterSurfaceDB_" + _rx.ToString();
                    }
                    else
                    {
                        _device.DebugName = ""; // used in shutdown
                    }

                    _bDXSetup = true;

                    setupAliasing();

                    //buildDXResources();
                    buildDXFonts();
                }
                catch (Exception e)
                {
                    // issue setting up dx
                    ShutdownDX();
                    MessageBox.Show("Problem initialising Meter DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            public void ShutdownDX(bool bFromRenderThread = false)
            {
                if (_displayTarget != null)
                {
                    _displayTarget.Resize -= target_Resize;
                    _displayTarget.MouseUp -= OnMouseUp;
                }

                if (!_bDXSetup) return;

                _dxDisplayThreadRunning = false;
                if (_dxRenderThread != null && _dxRenderThread.IsAlive && !bFromRenderThread) _dxRenderThread.Join();

                try
                {
                    if (_device != null && _device.ImmediateContext != null)
                    {
                        _device.ImmediateContext.ClearState();
                        _device.ImmediateContext.Flush();
                    }

                    foreach (KeyValuePair<string, SharpDX.Direct2D1.Bitmap> kvp in _images)
                    {
                        SharpDX.Direct2D1.Bitmap b = kvp.Value;
                        Utilities.Dispose(ref b);
                    }
                    _images.Clear();

                    releaseDXFonts();
                    releaseDXResources();

                    Utilities.Dispose(ref _renderTarget);
                    Utilities.Dispose(ref _swapChain1);
                    Utilities.Dispose(ref _swapChain);
                    Utilities.Dispose(ref _surface);
                    Utilities.Dispose(ref _factory);
                    Utilities.Dispose(ref _factory1);

                    _renderTarget = null;
                    _swapChain1 = null;
                    _swapChain = null;
                    _surface = null;
                    _factory = null;
                    _factory1 = null;

                    if (_device != null && _device.ImmediateContext != null)
                    {
                        SharpDX.Direct3D11.DeviceContext dc = _device.ImmediateContext;
                        Utilities.Dispose(ref dc);
                        dc = null;
                    }

                    SharpDX.Direct3D11.DeviceDebug ddb = null;
                    if (_device != null && _device.DebugName != "")
                    {
                        ddb = new SharpDX.Direct3D11.DeviceDebug(_device);
                        ddb.ReportLiveDeviceObjects(ReportingLevel.Detail);
                    }

                    if (ddb != null)
                    {
                        Utilities.Dispose(ref ddb);
                        ddb = null;
                    }
                    Utilities.Dispose(ref _device);
                    _device = null;

                    _bDXSetup = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Problem Shutting Down Meter DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            private void calcFps()
            {
                _nFrameCount++;
                if (_dElapsedFrameStart >= _fLastTime + 1000)
                {
                    double late = _dElapsedFrameStart - (_fLastTime + 1000);
                    if (late > 2000 || late < 0) late = 0; // ignore if too late
                    _nFps = _nFrameCount;
                    _nFrameCount = 0;
                    _fLastTime = _dElapsedFrameStart - late;
                }
            }
            private void dxRender()
            {
                if (!_bDXSetup) return;

                try
                {
                    _dxDisplayThreadRunning = true;
                    while (_dxDisplayThreadRunning)
                    {
                        int nSleepTime = 100;

                        if (_displayRunning && _displayTarget.Visible)
                        {
                            _dElapsedFrameStart = _objFrameStartTimer.ElapsedMsec;

                            lock (_DXlock)
                            {
                                _renderTarget.BeginDraw();

                                // middle pixel align shift
                                Matrix3x2 t = _renderTarget.Transform;
                                t.TranslationVector = _pixelShift;
                                _renderTarget.Transform = t;

                                // background for entire form/area?
                                _renderTarget.Clear(_backColour);

                                nSleepTime = drawMeters();

                                if (_highlightEdge)
                                {
                                    SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, targetWidth - 1f, targetHeight - 1f);
                                    rect.Inflate(-8, -8);
                                    _renderTarget.DrawRectangle(rect, getDXBrushForColour(System.Drawing.Color.FromArgb(192, System.Drawing.Color.YellowGreen)), 16f);
                                }

                                calcFps();

                                // undo the translate
                                _renderTarget.Transform = Matrix3x2.Identity;

                                _renderTarget.EndDraw();

                                // render
                                // note: the only way to have Present non block when using vsync number of blanks 0 , is to use DoNotWait
                                // however the gpu will error if it is busy doing something and the data can not be queued
                                // It will error and just ignore everything, we try present and ignore the 0x887A000A error
                                PresentFlags pf = _nVBlanks == 0 ? _NoVSYNCpresentFlag : PresentFlags.None;
                                Result r = _swapChain1.TryPresent(_nVBlanks, pf);

                                if (r != Result.Ok && r != 0x887A000A)
                                {
                                    string sMsg = "";
                                    if (r == 0x887A0001) sMsg = "Present Device Invalid Call" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";    //DXGI_ERROR_INVALID_CALL
                                    if (r == 0x887A0007) sMsg = "Present Device Reset" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";           //DXGI_ERROR_DEVICE_RESET
                                    if (r == 0x887A0005) sMsg = "Present Device Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";         //DXGI_ERROR_DEVICE_REMOVED
                                    if (r == 0x88760870) sMsg = "Present Device DD3DDI Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";  //D3DDDIERR_DEVICEREMOVED
                                                                                                                                                                                //if (r == 0x087A0001) sMsg = "Present Device Occluded" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";      //DXGI_STATUS_OCCLUDED
                                                                                                                                                                                //(ignored in the preceding if statement) if (r == 0x887A000A) sMsg = "Present Device Still Drawping" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]"; //DXGI_ERROR_WAS_STILL_DRAWING

                                    if (sMsg != "") throw (new Exception(sMsg));
                                }
                            }
                        }
                        if(_displayTarget.Visible)
                            Thread.Sleep(nSleepTime);
                        else
                            Thread.Sleep(500); // if not visible, sleep for half second
                    }
                }
                catch (Exception e)
                {
                    ShutdownDX(true);
                    MessageBox.Show("Problem in DirectX Meter Renderer !" + System.Environment.NewLine + System.Environment.NewLine + "[ " + e.ToString() + " ]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            private void setupAliasing()
            {
                if (!_bDXSetup) return;

                if (_bAntiAlias)
                    _renderTarget.AntialiasMode = AntialiasMode.PerPrimitive; // this will antialias even if multisampling is off
                else
                    _renderTarget.AntialiasMode = AntialiasMode.Aliased; // this will result in non antialiased lines only if multisampling = 1

                _renderTarget.TextAntialiasMode = TextAntialiasMode.Default;
            }
            public int VerticalBlanks
            {
                get { return _nVBlanks; }
                set
                {
                    int v = value;
                    if (v < 0) v = 0;
                    if (v > 4) v = 4;
                    _nVBlanks = v;
                }
            }
            public PictureBox Target
            {
                get { return _displayTarget; }
                set
                {
                    _displayTarget = value;
                }
            }
            private int targetWidth
            {
                get { return _displayTarget.Width; }
            }
            private int targetHeight
            {
                get { return _displayTarget.Height; }
            }
            //private void buildDXResources()
            //{
            //    if (!_bDXSetup) return;

            //    releaseDXResources();
            //}
            private void releaseDXResources()
            {
                if (!_bDXSetup) return;

                clearAllDynamicBrushes();
                clearAllDynamicTextFormats();
            }
            private void buildDXFonts()
            {
                if (!_bDXSetup) return;

                _fontFactory = new SharpDX.DirectWrite.Factory();
            }
            private void releaseDXFonts()
            {
                if (!_bDXSetup) return;

                if (_fontFactory != null) Utilities.Dispose(ref _fontFactory);

                _fontFactory = null;
            }
            private void clearAllDynamicBrushes()
            {
                if (!_bDXSetup || _DXBrushes == null) return;

                foreach (KeyValuePair<System.Drawing.Color, SharpDX.Direct2D1.Brush> kvp in _DXBrushes)
                {
                    SharpDX.Direct2D1.Brush b = kvp.Value;
                    Utilities.Dispose(ref b);
                    b = null;
                }

                _DXBrushes.Clear();
                _DXBrushes = null;
            }
            private void clearAllDynamicTextFormats()
            {
                if (!_bDXSetup || _textFormats == null) return;

                foreach (KeyValuePair<string, SharpDX.DirectWrite.TextFormat> kvp in _textFormats)
                {
                    SharpDX.DirectWrite.TextFormat tf = kvp.Value;
                    Utilities.Dispose(ref tf);
                    tf = null;
                }

                _textFormats.Clear();
                _textFormats = null;

                if (_fontFactory != null) Utilities.Dispose(ref _fontFactory);
                _fontFactory = null;
            }
            private SharpDX.Direct2D1.Brush getDXBrushForColour(System.Drawing.Color c, int replaceAlpha = -1)
            {
                if (!_bDXSetup) return null;

                //if (_DXBrushes == null) _DXBrushes = new Dictionary<System.Drawing.Color, SharpDX.Direct2D1.Brush>();

                System.Drawing.Color newC;
                if (replaceAlpha >= 0 && replaceAlpha <= 255)
                    newC = System.Drawing.Color.FromArgb(replaceAlpha, c.R, c.G, c.B); // override the alpha
                else
                    newC = c;

                if (_DXBrushes.ContainsKey(newC)) return _DXBrushes[newC];

                SolidBrush sb = new SolidBrush(newC);
                SharpDX.Direct2D1.Brush b = convertBrush(sb);
                sb.Dispose();

                _DXBrushes.Add(newC, b);

                return b;
            }
            private SharpDX.Direct2D1.SolidColorBrush convertBrush(SolidBrush b)
            {
                return new SharpDX.Direct2D1.SolidColorBrush(_renderTarget, convertColour(b.Color));
            }
            private SharpDX.Color4 convertColour(System.Drawing.Color c)
            {
                return new SharpDX.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            private SharpDX.DirectWrite.TextFormat getDXTextFormatForFont(string sFontFamily, float emSize, FontStyle style, bool bAlignRight = false)
            {
                if (!_bDXSetup) return null;

                //if (_textFormats == null) _textFormats = new Dictionary<string, SharpDX.DirectWrite.TextFormat>();

                string sKey = sFontFamily + emSize.ToString("0.0") + style.ToString();

                if (_textFormats.ContainsKey(sKey)) return _textFormats[sKey];

                try
                {
                    SharpDX.DirectWrite.FontWeight fontWeight = SharpDX.DirectWrite.FontWeight.Regular;
                    SharpDX.DirectWrite.FontStyle fontStyle = SharpDX.DirectWrite.FontStyle.Normal;
                    if (((int)style & (int)FontStyle.Bold) == (int)FontStyle.Bold) fontWeight = SharpDX.DirectWrite.FontWeight.Bold;
                    if (((int)style & (int)FontStyle.Italic) == (int)FontStyle.Italic) fontStyle = SharpDX.DirectWrite.FontStyle.Italic;

                    SharpDX.DirectWrite.TextFormat tf = new SharpDX.DirectWrite.TextFormat(_fontFactory, sFontFamily, fontWeight, fontStyle, (emSize / 72) * _renderTarget.DotsPerInch.Width);
                    tf.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
                    if(bAlignRight)
                        tf.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;

                    _textFormats.Add(sKey, tf);

                    return tf;                    
                }
                catch { }

                return null;
            }
            private void resizeDX()
            {
                lock (_DXlock)
                {
                    if (!_bDXSetup) return;

                    try
                    {
                        Utilities.Dispose(ref _renderTarget);
                        Utilities.Dispose(ref _surface);

                        _renderTarget = null;
                        _surface = null;

                        _device.ImmediateContext.ClearState();
                        _device.ImmediateContext.Flush();

                        _swapChain1.ResizeBuffers(_nBufferCount, targetWidth, targetHeight, _swapChain.Description.ModeDescription.Format, SwapChainFlags.None);

                        _surface = _swapChain1.GetBackBuffer<Surface>(0);

                        RenderTargetProperties rtp = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));
                        _renderTarget = new RenderTarget(_factory, _surface, rtp);

                        setupAliasing();
                    }
                    catch (Exception e)
                    {
                        ShutdownDX();
                        MessageBox.Show("DirectX resizeDX() Meter failure\n" + e.Message, "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            //
            private void target_Resize(object sender, System.EventArgs e)
            {
                resizeDX();
            }
            private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                PictureBox pb = sender as PictureBox;
                if (pb == null) return;
                string sId = pb.Tag.ToString();
                if (!_meters.ContainsKey(sId)) return;

                clsMeter m = _meters[sId];
                if (m.SortedMeterItemsForZOrder == null) return;

                float tw = targetWidth - 1f;// 0.5f;
                float rw = m.XRatio;
                float rh = m.YRatio;

                SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, tw * rw, tw * rh);

                foreach (KeyValuePair<string, clsMeterItem> mikvp in m.SortedMeterItemsForZOrder)
                {
                    clsMeterItem mi = mikvp.Value;
                    if(mi.ItemType == clsMeterItem.MeterItemType.CLICKBOX)
                    {
                        clsClickBox cb = (clsClickBox)mi;

                        float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                        float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                        float w = rect.Width * (mi.Size.Width / m.XRatio);
                        float h = rect.Height * (mi.Size.Height / m.YRatio);

                        SharpDX.RectangleF clickRect = new SharpDX.RectangleF(x, y, w, h);

                        if (clickRect.Contains(new SharpDX.Point(e.X, e.Y)))
                        {
                            m.MouseUp(e, m, cb);
                        }
                    }
                }
            }
            //
            private int drawMeters()
            {
                int nRedrawDelay = 500;
                clsMeter m = _meter;

                if (m.RX == _rx && m.SortedMeterItemsForZOrder != null)
                {
                    int nTmp = m.MOX ? m.QuickestTXUpdate : m.QuickestRXUpdate;
                    if (nTmp < nRedrawDelay) nRedrawDelay = nTmp;

                    foreach (KeyValuePair<string, clsMeterItem> mikvp in m.SortedMeterItemsForZOrder)
                    {
                        clsMeterItem mi = mikvp.Value;

                        bool bRender = ((m.MOX && mi.OnlyWhenTX) || (!m.MOX && mi.OnlyWhenRX)) || (!mi.OnlyWhenTX && !mi.OnlyWhenRX);

                        if (bRender && ((m.DisplayGroup == 0 || mi.DisplayGroup == 0) || (mi.DisplayGroup == m.DisplayGroup)))
                        {
                            float tw = targetWidth - 1f;
                            float rw = m.XRatio;
                            float rh = m.YRatio;

                            SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, tw * rw, tw * rh);

                            switch (mi.ItemType)
                            {
                                //case clsMeterItem.MeterItemType.V_BAR:
                                //    renderVBar(rect, mi, m);
                                //    break;
                                case clsMeterItem.MeterItemType.H_BAR:
                                    renderHBar(rect, mi, m);
                                    break;
                                case clsMeterItem.MeterItemType.SOLID_COLOUR:
                                    renderSolidColour(rect, mi, m);
                                    break;
                                case clsMeterItem.MeterItemType.NEEDLE:
                                    renderNeedle(rect, mi, m);
                                    break;
                                case clsMeterItem.MeterItemType.IMAGE:
                                    renderImage(rect, mi, m);
                                    break;
                                case clsMeterItem.MeterItemType.TEXT:
                                    renderText(rect, mi, m);                                            
                                    break;
                                case clsMeterItem.MeterItemType.H_SCALE:
                                case clsMeterItem.MeterItemType.V_SCALE:
                                    renderScale(rect, mi, m);
                                    break;
                                case clsMeterItem.MeterItemType.MAGIC_EYE:
                                    renderEye(rect, mi, m);
                                    break;
                                //case clsMeterItem.MeterItemType.ITEM_GROUP:
                                //    renderGroup(rect, mi, m);
                                //    break;
                            }
                        }
                    }

                }

                return nRedrawDelay;
            }

            //private float limit(float value, float min, float max)
            //{
            //    if(value < min) value = min;
            //    if(value > max) value = max;
            //    return value;
            //}
            private SizeF measureString(string sText, string sFontFamily, FontStyle style, float emSize)
            {
                if (!_bDXSetup) return SizeF.Empty;

                //if (_stringMeasure == null) _stringMeasure = new Dictionary<string, SizeF>();

                emSize = (float)Math.Round(emSize, 1);

                string sKey = sText + emSize.ToString("0.0");

                if (_stringMeasure.ContainsKey(sKey)) return _stringMeasure[sKey];

                SharpDX.DirectWrite.FontWeight fontWeight = SharpDX.DirectWrite.FontWeight.Regular;
                SharpDX.DirectWrite.FontStyle fontStyle = SharpDX.DirectWrite.FontStyle.Normal;
                if (((int)style & (int)FontStyle.Bold) == (int)FontStyle.Bold) fontWeight = SharpDX.DirectWrite.FontWeight.Bold;
                if (((int)style & (int)FontStyle.Italic) == (int)FontStyle.Italic) fontStyle = SharpDX.DirectWrite.FontStyle.Italic;

                // calculate how big the string would be @ emSize pt
                SharpDX.DirectWrite.TextFormat tf = new SharpDX.DirectWrite.TextFormat(_fontFactory, sFontFamily, fontWeight, fontStyle, emSize);
                tf.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
                
                SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(_fontFactory, sText, tf, float.MaxValue, float.MaxValue);
                float width = layout.Metrics.Width;
                float height = layout.Metrics.Height;
                Utilities.Dispose(ref layout);
                layout = null;
                Utilities.Dispose(ref tf);
                tf = null;

                SizeF size = new SizeF(width, height);

                //why these fudge factors? not sure, is it a font proportion issue?
                size.Width *= 1.3f;
                size.Height *= 1.1f;

                _stringMeasure.Add(sKey, size);

                return size;
            }
            private void renderScale(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsScaleItem scale = (clsScaleItem)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.CornflowerBlue));

                RawVector2 startPoint = new RawVector2();
                RawVector2 endPoint = new RawVector2();

                if (scale.ItemType == clsMeterItem.MeterItemType.H_SCALE)
                {
                    float fontSizeEmScaled = (scale.FontSize / 16f) * (rect.Width / 52f);

                    SizeF szTextSize;

                    if (scale.ShowType)
                    {                        
                        string sText = MeterManager.ReadingName(scale.ReadingSource);
                        szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, fontSizeEmScaled);
                        SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x + (w * 0.5f) - (szTextSize.Width / 2f), y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                        _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, fontSizeEmScaled, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourType));
                    }

                    szTextSize = measureString("0", scale.FontFamily, scale.FntStyle, fontSizeEmScaled);

                    float fLineBaseY = y + (h * 0.85f);

                    switch (scale.ReadingSource)
                    {
                        case Reading.AGC_GAIN:
                            {
                                generalScale(x, y, w, h, scale, 7, 1, -50, 125, 25, 25, fLineBaseY, fontSizeEmScaled);
                            }
                            break;
                        case Reading.AGC_AV:
                        case Reading.AGC_PK:
                            {
                                generalScale(x, y, w, h, scale, 6, 5, -125, 125, 25, 25, fLineBaseY, fontSizeEmScaled);
                            }
                            break;
                        case Reading.SIGNAL_STRENGTH:
                        case Reading.AVG_SIGNAL_STRENGTH:
                            {
                                generalScale(x,y,w,h, scale, 6, 3, -1, 60, 2, 20, fLineBaseY, fontSizeEmScaled, 0.5f, true, true);
                            }
                            break;
                        case Reading.ADC_PK:
                        case Reading.ADC_AV:
                            {
                                generalScale(x, y, w, h, scale, 6, 1, -120, 0, 20, 20, fLineBaseY, fontSizeEmScaled);
                            }
                            break;
                        case Reading.ESTIMATED_PBSNR:
                            {
                                generalScale(x, y, w, h, scale, 6, 1, 0, 60, 10, 10, fLineBaseY, fontSizeEmScaled);
                            }
                            break;
                        // -30 to 12
                        case Reading.MIC:
                        case Reading.MIC_PK:
                        case Reading.EQ:
                        case Reading.EQ_PK:
                        case Reading.LEVELER:
                        case Reading.LEVELER_PK:
                        case Reading.CFC_PK:
                        case Reading.COMP:
                        case Reading.COMP_PK:
                        case Reading.ALC:
                        case Reading.ALC_PK:
                            {
                                generalScale(x, y, w, h, scale, 4, 3, -30, 12, 10, 4, fLineBaseY, fontSizeEmScaled, 0.665f);
                            }
                            break;
                        case Reading.ALC_GROUP:
                            {
                                generalScale(x, y, w, h, scale, 4, 5, -30, 25, 10, 5, fLineBaseY, fontSizeEmScaled, 0.5f, true);
                            }
                            break;
                        case Reading.PWR:
                        case Reading.REVERSE_PWR:
                            {
                                string[] list500 = { "50", "100", "250", "500", "600+" };
                                string[] list200 = { "10", "20", "100", "200", "240+" };
                                string[] list100 = { "5", "10", "50", "100", "120+" };
                                string[] list30 = { "5", "10", "20", "30", "50+" };
                                string[] list15 = { "1", "5", "10", "15", "25+" };
                                string[] list1 = { "100", "250", "500", "800", "1000+" };

                                int nPower = MeterManager.CurrentPowerRating;
                                string[] powerList;
                                switch (nPower)
                                {
                                    case 500:
                                        powerList = list500;
                                        break;
                                    case 200:
                                        powerList = list200;
                                        break;
                                    case 100:
                                        powerList = list100;
                                        break;
                                    case 30:
                                        powerList = list30;
                                        break;
                                    case 15:
                                        powerList = list15;
                                        break;
                                    case 1:
                                        powerList = list1;
                                        break;
                                    default:
                                        powerList = list500;
                                        break;
                                }

                                float spacing = (w * 0.75f) / 4.0f;

                                // horiz line
                                startPoint.X = x;
                                startPoint.Y = fLineBaseY;
                                endPoint.X = x + (w * 0.75f);
                                endPoint.Y = startPoint.Y;
                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                startPoint.X = endPoint.X;
                                endPoint.X = x + (w * 0.99f);
                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                //low markers
                                for (int i = 1; i < 5; i++)
                                {
                                    // short ticks
                                    startPoint.X = x + (i * spacing) - (spacing * 0.5f); // - half a space to shift left between longer ticks
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.15f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                    // long ticks
                                    startPoint.X = x + (i * spacing);
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.3f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                    // text
                                    string sText = powerList[i - 1];
                                    szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, fontSizeEmScaled);
                                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(startPoint.X - szTextSize.Width, endPoint.Y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, fontSizeEmScaled, scale.FntStyle), txtrect, getDXBrushForColour(scale.LowColour));
                                }

                                //high markers
                                spacing = ((w * 0.99f) - (w * 0.75f));

                                // short ticks
                                startPoint.X = x + (w * 0.75f) + (1 * spacing) - (spacing * 0.5f); // - half a space to shift left between longer ticks
                                endPoint.X = startPoint.X;
                                endPoint.Y = fLineBaseY - (h * 0.15f);

                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                // long ticks
                                startPoint.X = x + (w * 0.75f) + (1 * spacing);
                                endPoint.X = startPoint.X;
                                endPoint.Y = fLineBaseY - (h * 0.3f);

                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                // text
                                string sText2 = powerList[4];
                                szTextSize = measureString(sText2, scale.FontFamily, scale.FntStyle, fontSizeEmScaled);
                                SharpDX.RectangleF txtrect2 = new SharpDX.RectangleF(startPoint.X - szTextSize.Width, endPoint.Y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                                _renderTarget.DrawText(sText2, getDXTextFormatForFont(scale.FontFamily, fontSizeEmScaled, scale.FntStyle), txtrect2, getDXBrushForColour(scale.HighColour));
                            }
                            break;
                        //case MeterTXMode.SWR_POWER:
                        //    {

                        //    }
                        //    break;
                        case Reading.SWR:
                            {
                                float spacing = (w * 0.75f) / 15.0f;
                                string[] swr_list = { "1.5", "2" };
                                string[] swr_hi_list = { "3", "4", "5" };

                                //g.FillRectangle(low_brush, 0, H - 4, (int)(W * 0.75), 2);
                                //g.FillRectangle(high_brush, (int)(W * 0.75), H - 4, (int)(W * 0.25) - 4, 2);

                                // horiz line
                                startPoint.X = x;
                                startPoint.Y = fLineBaseY;
                                endPoint.X = x + (w * 0.75f);
                                endPoint.Y = startPoint.Y;
                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                startPoint.X = endPoint.X;
                                endPoint.X = x + (w * 0.99f);
                                _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                //low markers
                                for (int i = 1; i < 15; i++)
                                {
                                    // short ticks
                                    startPoint.X = x + (i * spacing);
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.15f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);
                                }

                                //low markers
                                for (int i = 1; i < 15; i++)
                                {
                                    // short ticks
                                    startPoint.X = x + (i * spacing);
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.15f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);
                                }
                                for (int i = 1; i < 3; i++)
                                {
                                    spacing = (w * 0.5f) / 2.0f;

                                    // long ticks 
                                    startPoint.X = x + (i * spacing);
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.3f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                    // text
                                    string sText = swr_list[i - 1];
                                    szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, fontSizeEmScaled);
                                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(startPoint.X - (szTextSize.Width / 2f), endPoint.Y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, fontSizeEmScaled, scale.FntStyle), txtrect, getDXBrushForColour(scale.LowColour));
                                }

                                //high markers
                                spacing = ((w * 0.99f) - (w * 0.75f)) / 2f;

                                for (int i = 1; i < 4; i++)
                                {
                                    if (i < 3) // as small tick will go off then end when i=3
                                    {
                                        // short ticks
                                        startPoint.X = x + (w * 0.75f) + (i * spacing) - (spacing * 0.5f); // - half a space to shift left between longer ticks
                                        endPoint.X = startPoint.X;
                                        endPoint.Y = fLineBaseY - (h * 0.15f);

                                        _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);
                                    }

                                    // long ticks 
                                    startPoint.X = x + (w * 0.75f) + (i * spacing) - spacing; // - one full space to the left
                                    endPoint.X = startPoint.X;
                                    endPoint.Y = fLineBaseY - (h * 0.3f);

                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                    // text
                                    string sText = swr_hi_list[i - 1];
                                    szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, fontSizeEmScaled);
                                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(startPoint.X - (szTextSize.Width / 2f), endPoint.Y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, fontSizeEmScaled, scale.FntStyle), txtrect, getDXBrushForColour(scale.HighColour));
                                }
                            }
                            break;
                        case Reading.ALC_G:
                        case Reading.LVL_G:
                        case Reading.CFC_G:
                            {
                                generalScale(x, y, w, h, scale, 5, 1, 0, 25, 5, 5, fLineBaseY, fontSizeEmScaled, -1, true);                                
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (scale.ItemType == clsMeterItem.MeterItemType.V_SCALE)
                {
                    float fontSize = 8f;
                    SizeF adjustedFontSize = measureString("0", scale.FontFamily, scale.FntStyle, fontSize);
                    float ratio = w / adjustedFontSize.Width;
                    float newSize = (float)Math.Round((fontSize * ratio) * (fontSize / _renderTarget.DotsPerInch.Width), 1);

                    if (scale.ShowType)
                    {
                        string sText = MeterManager.ReadingName(scale.ReadingSource);
                        adjustedFontSize = measureString(sText, scale.FontFamily, scale.FntStyle, newSize);
                        SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y - (adjustedFontSize.Height * 1.1f), adjustedFontSize.Width, adjustedFontSize.Height);
                        _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, newSize, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourType));
                    }

                    fontSize = 10f;
                    adjustedFontSize = measureString("0", scale.FontFamily, scale.FntStyle, fontSize);
                    ratio = w / adjustedFontSize.Width;
                    newSize = (float)Math.Round((fontSize * ratio) * (fontSize / _renderTarget.DotsPerInch.Width), 1);

                    float fBottomY = y + h;
                    float xCentreLine = x + (w * 0.5f);                    

                    switch (scale.ReadingSource)
                    {
                        case Reading.SIGNAL_STRENGTH:
                        case Reading.AVG_SIGNAL_STRENGTH:
                            {
                                float spacing = (h * 0.5f) / 5.0f;
                                if (scale.ShowMarkers)
                                {
                                    // vert line
                                    startPoint.X = xCentreLine;
                                    startPoint.Y = fBottomY;
                                    endPoint.X = xCentreLine;
                                    endPoint.Y = fBottomY - (h * 0.5f);
                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                    startPoint.Y = endPoint.Y;
                                    endPoint.Y = fBottomY - (h * 0.99f);
                                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);
                                }

                                // markers low                                
                                for (int i = 1; i < 6; i++)
                                {
                                    if (scale.ShowMarkers)
                                    {
                                        // short ticks
                                        startPoint.X = xCentreLine - (w * 0.1f);
                                        startPoint.Y = fBottomY - ((i * spacing) - (spacing * 0.5f)); // - half a space to shift left between longer ticks
                                        endPoint.X = xCentreLine + (w * 0.1f);
                                        endPoint.Y = startPoint.Y;

                                        _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);
                                    }

                                    // long ticks
                                    startPoint.X = xCentreLine - (w * 0.3f);
                                    startPoint.Y = fBottomY - (i * spacing);
                                    endPoint.X = xCentreLine + (w * 0.3f);
                                    endPoint.Y = startPoint.Y;

                                    if (scale.ShowMarkers) _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                                    // text
                                    string sText = (-1 + i * 2).ToString();
                                    adjustedFontSize = measureString(sText, scale.FontFamily, scale.FntStyle, newSize);
                                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(endPoint.X + (w * 0.08f), endPoint.Y - (adjustedFontSize.Height / 2f), adjustedFontSize.Width, adjustedFontSize.Height);
                                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, newSize, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourLow));
                                }

                                // markers high
                                spacing = ((h * 0.5f) - (h * 0.01f)) / 3.0f; // - w*0.01f as we only draw the line up to w*0.99f
                                for (int i = 1; i < 4; i++)
                                {

                                    //short ticks
                                    if (scale.ShowMarkers)
                                    {
                                        startPoint.X = xCentreLine - (w * 0.1f);
                                        startPoint.Y = fBottomY -  ((h * 0.5f) + (i * spacing) - (spacing * 0.5f)); // - half a space to shift left between longer ticks
                                        endPoint.X = xCentreLine + (w * 0.1f);
                                        endPoint.Y = startPoint.Y;

                                        _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);
                                    }

                                    // long ticks
                                    startPoint.X = xCentreLine - (w * 0.3f);
                                    startPoint.Y = fBottomY - ((h * 0.5f) + (i * spacing));
                                    endPoint.X = xCentreLine + (w * 0.3f);
                                    endPoint.Y = startPoint.Y;

                                    if (scale.ShowMarkers) _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                                    // text
                                    string sText = "+" + (i * 20).ToString();
                                    adjustedFontSize = measureString(sText, scale.FontFamily, scale.FntStyle, newSize);
                                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(endPoint.X - (w * 0.2f), endPoint.Y - (adjustedFontSize.Height / 2f) + (h * 0.01f), adjustedFontSize.Width, adjustedFontSize.Height);
                                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, newSize, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourHigh));
                                }
                            }
                            break;
                    }
                }
            }
            private void generalScale(float x,float y,float w,float h,clsScaleItem scale, int lowLongTicks, int highLongTicks, int lowStartNumner, int highEndNumber, int lowIncrement, int highIngrement, float fLineBaseY, float newSize, float centrePerc = -1, bool addTrailingPlus = false, bool addAllTrailingPlus = false)
            {
                float lowToHighPoint = (float)(lowLongTicks - 1) / (float)(lowLongTicks + highLongTicks - 1);

                if (centrePerc >= 0) lowToHighPoint = centrePerc;

                float spacing = (w * lowToHighPoint) / (float)(lowLongTicks - 1);//(w * 0.6666f) / 5.0f;
                RawVector2 startPoint = new RawVector2();
                RawVector2 endPoint = new RawVector2();

                if (scale.ShowMarkers)
                {
                    // horiz line
                    startPoint.X = x;
                    startPoint.Y = fLineBaseY;
                    endPoint.X = x + (spacing * (float)(lowLongTicks - 1));//(w * 0.6666f);
                    endPoint.Y = startPoint.Y;
                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                    startPoint.X = endPoint.X;
                    endPoint.X = x + (w * 0.99f);
                    _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);
                }

                // markers low                                
                for (int i = 1; i < lowLongTicks; i++)
                {
                    if (scale.ShowMarkers)
                    {
                        // short ticks
                        startPoint.X = x + (i * spacing) - (spacing * 0.5f); // - half a space to shift left between longer ticks
                        endPoint.X = startPoint.X;
                        endPoint.Y = fLineBaseY - (h * 0.15f);

                        _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);
                    }

                    // long ticks
                    startPoint.X = x + (i * spacing);
                    endPoint.X = startPoint.X;
                    endPoint.Y = fLineBaseY - (h * 0.3f);

                    if (scale.ShowMarkers) _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.LowColour), 2f);

                    // text
                    string sText = (lowStartNumner + i * lowIncrement).ToString();
                    SizeF szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, newSize);
                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(startPoint.X - (szTextSize.Width / 2f), endPoint.Y - szTextSize.Height, szTextSize.Width, szTextSize.Height);
                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, newSize, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourLow));
                }
                
                // markers high
                float lowSpacing = spacing;
                spacing = (/*(w * 0.3333f)*/(w - (lowSpacing * (float)(lowLongTicks - 1))) - (w * 0.01f)) / (float)highLongTicks; // - w*0.01f as we only draw the line up to w*0.99f
                for (int i = 1; i < highLongTicks + 1; i++)
                {

                    //short ticks
                    if (scale.ShowMarkers)
                    {
                        startPoint.X = x + /*(w * 0.6666f)*/(lowSpacing * (float)(lowLongTicks-1)) + (i * spacing) - (spacing * 0.5f); // - half a space to shift left between longer ticks
                        endPoint.X = startPoint.X;
                        endPoint.Y = fLineBaseY - (h * 0.15f);

                        _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);
                    }

                    // long ticks
                    startPoint.X = x + (/*w * 0.6666f*/(lowSpacing * (float)(lowLongTicks - 1))) + (i * spacing);
                    endPoint.X = startPoint.X;
                    endPoint.Y = fLineBaseY - (h * 0.3f);

                    if (scale.ShowMarkers) _renderTarget.DrawLine(startPoint, endPoint, getDXBrushForColour(scale.HighColour), 2f);

                    // text
                    string sText = /*"+" +*/ ((highEndNumber - (highLongTicks * highIngrement)) + i * highIngrement).ToString();
                    if (addAllTrailingPlus || (i == highLongTicks && addTrailingPlus)) sText += "+";
                    SizeF szTextSize = measureString(sText, scale.FontFamily, scale.FntStyle, newSize);
                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(i == highLongTicks ? x + w - szTextSize.Width : startPoint.X - szTextSize.Width / 2f, endPoint.Y - szTextSize.Height, szTextSize.Width, szTextSize.Height);
                    _renderTarget.DrawText(sText, getDXTextFormatForFont(scale.FontFamily, newSize, scale.FntStyle), txtrect, getDXBrushForColour(scale.FontColourHigh));
                }
            }
            private void renderGroup(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsItemGroup ig = (clsItemGroup)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                float newSize = 16f;

                string sText = ig.Order.ToString();
                SizeF szTextSize = measureString(sText, "Trebuchet MS", FontStyle.Regular, newSize);
                SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y, szTextSize.Width, szTextSize.Height);
                _renderTarget.DrawText(sText, getDXTextFormatForFont("Trebuchet MS", newSize, FontStyle.Regular), txtrect, getDXBrushForColour(System.Drawing.Color.White));
            }
            private void renderEye(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsMagicEyeItem magicEye = (clsMagicEyeItem)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                //SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.Green));

                Vector2 centre = new Vector2(x + w / 2f, y + h / 2f);

                Ellipse e = new Ellipse(centre, w / 2f, h / 2f);

                System.Drawing.Color c = magicEye.Colour;
                SharpDX.Direct2D1.Brush br = getDXBrushForColour(System.Drawing.Color.FromArgb(255, (int)(c.R * 0.35f), (int)(c.G * 0.35f), (int)(c.B * 0.35f)));

                _renderTarget.FillEllipse(e, getDXBrushForColour(magicEye.Colour));

                PointF min, max;
                float percX, percY;
                getPerc(magicEye, magicEye.Value, out percX, out percY, out min, out max);

                if (percX <= 0.01f)
                {
                    _renderTarget.FillEllipse(e, br);
                }
                else if (percX >= 0.99f)
                {
                    _renderTarget.FillEllipse(e, getDXBrushForColour(magicEye.Colour));
                }
                else
                {
                    float fDeg = (360f - (int)(360 * percX)) / 2f;
                    float fRad = (float)degToRad(fDeg);

                    float radiusX = w / 2f;
                    float radiusY = h / 2f;
                    float endMaxX = centre.X + (float)Math.Sin(-fRad) * radiusX;
                    float endMaxY = centre.Y + (float)Math.Cos(-fRad) * radiusY;
                    float endMinX = centre.X + (float)Math.Sin(fRad) * radiusX;
                    float endMinY = centre.Y + (float)Math.Cos(fRad) * radiusY;

                    PathGeometry sharpGeometry = new PathGeometry(_renderTarget.Factory);

                    GeometrySink geo = sharpGeometry.Open();
                    geo.BeginFigure(new SharpDX.Vector2(centre.X, centre.Y), FigureBegin.Filled);

                    geo.AddLine(new SharpDX.Vector2(endMinX, endMinY));

                    ArcSegment arcSegment = new ArcSegment();
                    arcSegment.Point = new SharpDX.Vector2(endMaxX, endMaxY);
                    arcSegment.SweepDirection = SweepDirection.Clockwise;
                    arcSegment.ArcSize = fDeg <= 90f ? ArcSize.Small : ArcSize.Large;
                    arcSegment.Size = new Size2F(radiusX, radiusY);
                    geo.AddArc(arcSegment);

                    geo.EndFigure(FigureEnd.Closed); // adds the closing line
                    geo.Close();

                    _renderTarget.FillGeometry(sharpGeometry, br);

                    Utilities.Dispose(ref geo);
                    geo = null;
                    Utilities.Dispose(ref sharpGeometry);
                    sharpGeometry = null;
                }

                e.RadiusX = w / 6f;
                e.RadiusY = w / 6f;
                _renderTarget.FillEllipse(e, getDXBrushForColour(System.Drawing.Color.FromArgb(32, 32, 32)));
            }
            private void renderText(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsText txt = (clsText)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                string sText;
                switch (txt.Text.ToLower())
                {
                    case "%group%":
                        sText = m.DisplayGroupText;
                        break;
                    case "%fps%":
                        sText = _nFps.ToString();
                        break;
                    default:
                        sText = txt.Text;
                        break;
                }

                float fontSize;
                SizeF size;
                if (txt.FixedSize)
                {
                    fontSize = txt.FontSize;
                    size = measureString("0", txt.FontFamily, txt.Style, fontSize);
                }
                else
                {
                    fontSize = 72f;
                    size = measureString(sText, txt.FontFamily, txt.Style, fontSize);
                }

                float ratio = w / size.Width;
                float newSize = (float)Math.Round((fontSize * ratio) * (fontSize / _renderTarget.DotsPerInch.Width), 1);

                SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y, w, h);
                _renderTarget.DrawText(sText, getDXTextFormatForFont(txt.FontFamily, newSize, txt.Style), txtrect, getDXBrushForColour(txt.Colour));
            }
            private void renderHBar(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsBarItem cbi = (clsBarItem)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                //SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.Red));

                PointF min, max;
                float percX, percY;

                getPerc(cbi, cbi.Value, out percX, out percY, out min, out max);

                float xPos = x + (min.X * w) + (percX * ((max.X - min.X) * w));

                float minHistory_x = x;
                float maxHistory_x = x;
                if (cbi.ShowHistory)
                {
                    getPerc(cbi, cbi.MinHistory, out percX, out percY, out min, out max);
                    minHistory_x = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                    getPerc(cbi, cbi.MaxHistory, out percX, out percY, out min, out max);
                    maxHistory_x = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                }
                else if (cbi.PeakHold) // max is used for peak hold
                {
                    getPerc(cbi, cbi.MaxHistory, out percX, out percY, out min, out max);
                    maxHistory_x = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                }

                int segmentBlockSize = (int)(w * 0.02f);
                if (segmentBlockSize < 7) segmentBlockSize = 7;
                int segmentGapSize = (int)(segmentBlockSize * 0.2f);
                if (segmentBlockSize < 2) segmentBlockSize = 2;
                int segmentStep = segmentBlockSize + segmentGapSize;

                if (cbi.ShowHistory)
                {                   
                    switch (cbi.Style)
                    {
                        case clsBarItem.BarStyle.None:
                        case clsBarItem.BarStyle.SolidFilled:
                        case clsBarItem.BarStyle.Line:
                            {
                                SharpDX.RectangleF history = new SharpDX.RectangleF(minHistory_x, y, maxHistory_x - minHistory_x, h);
                                _renderTarget.FillRectangle(history, getDXBrushForColour(cbi.HistoryColour));
                            }
                            break;
                        case clsBarItem.BarStyle.Segments:
                            {
                                int numValueBlocks = (int)((xPos - x) / (float)segmentStep);

                                float i;
                                float startX = (numValueBlocks * segmentStep) + x;
                                SharpDX.RectangleF barrect;
                                for (i = startX; i < maxHistory_x - segmentStep; i += segmentStep)
                                {
                                    barrect = new SharpDX.RectangleF(i, y, segmentBlockSize, h);
                                    _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.HistoryColour));
                                }

                                // complete the sliver
                                if (i < maxHistory_x)
                                {
                                    barrect = new SharpDX.RectangleF(i, y, maxHistory_x - i, h);
                                    _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.HistoryColour));
                                }
                            }
                            break;
                    }
                }

                switch (cbi.Style)
                {
                    case clsBarItem.BarStyle.SolidFilled:
                        {
                            SharpDX.RectangleF barrect = new SharpDX.RectangleF(x, y, xPos - x, h);

                            if (cbi.PeakHold)
                                _renderTarget.DrawLine(new SharpDX.Vector2(maxHistory_x, y), new SharpDX.Vector2(maxHistory_x, y + h), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                        }
                        break;
                    case clsBarItem.BarStyle.Line:
                        {
                            if (cbi.PeakHold)
                                _renderTarget.DrawLine(new SharpDX.Vector2(maxHistory_x, y), new SharpDX.Vector2(maxHistory_x, y + h), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                            _renderTarget.DrawLine(new SharpDX.Vector2(xPos, y), new SharpDX.Vector2(xPos, y + h), getDXBrushForColour(cbi.MarkerColour), cbi.StrokeWidth);
                        }
                        break;
                    case clsBarItem.BarStyle.Segments:
                        {
                            float i;
                            SharpDX.RectangleF barrect;
                            for (i = x; i < xPos - segmentStep; i += segmentStep)
                            {
                                barrect = new SharpDX.RectangleF(i, y, segmentBlockSize, h);
                                _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                            }

                            // complete the sliver
                            if (i < xPos)
                            {
                                barrect = new SharpDX.RectangleF(i, y, xPos - i, h);
                                _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                            }

                            if (cbi.PeakHold)
                                _renderTarget.DrawLine(new SharpDX.Vector2(maxHistory_x, y), new SharpDX.Vector2(maxHistory_x, y + h), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                            if (cbi.ShowMarker)
                                _renderTarget.DrawLine(new SharpDX.Vector2(xPos, y), new SharpDX.Vector2(xPos, y + h), getDXBrushForColour(cbi.MarkerColour), cbi.StrokeWidth);
                        }
                        break;
                }

                if (cbi.ShowValue)
                {
                    float fontSizeEmScaled = (cbi.FontSize / 16f) * (rect.Width / 52f);

                    string sText = cbi.Value.ToString("0.0") + MeterManager.ReadingUnits(cbi.ReadingSource);
                    SizeF szTextSize = measureString(sText, cbi.FontFamily, cbi.FntStyle, fontSizeEmScaled);
                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                    _renderTarget.DrawText(sText, getDXTextFormatForFont(cbi.FontFamily, fontSizeEmScaled, cbi.FntStyle), txtrect, getDXBrushForColour(cbi.MarkerColour));

                    if (cbi.PeakHold || cbi.ShowHistory)
                    {
                        sText = cbi.MaxHistory.ToString("0.0") + MeterManager.ReadingUnits(cbi.ReadingSource);
                        szTextSize = measureString(sText, cbi.FontFamily, cbi.FntStyle, fontSizeEmScaled);
                        txtrect = new SharpDX.RectangleF(x + w - szTextSize.Width, y - szTextSize.Height - (h * 0.1f), szTextSize.Width, szTextSize.Height);
                        _renderTarget.DrawText(sText, getDXTextFormatForFont(cbi.FontFamily, fontSizeEmScaled, cbi.FntStyle, true), txtrect, getDXBrushForColour(cbi.PeakHoldMarkerColour));
                    }
                }
            }
            private void renderVBar(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsBarItem cbi = (clsBarItem)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                //SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.Green));

                PointF min, max;
                float percX, percY;

                getPerc(cbi, cbi.Value, out percX, out percY, out min, out max);

                //top left is 0,0
                float yBottom = y + h;
                //float xPos = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                float yPos = yBottom - ((min.Y * h) + (percY * ((max.Y - min.Y) * h)));

                float minHistory_y = y;
                float maxHistory_y = y;
                if (cbi.ShowHistory)
                {
                    getPerc(cbi, cbi.MinHistory, out percX, out percY, out min, out max);
                    minHistory_y = yBottom - ((min.Y * h) + (percY * ((max.Y - min.Y) * h)));
                    getPerc(cbi, cbi.MaxHistory, out percX, out percY, out min, out max);
                    maxHistory_y = yBottom - ((min.Y * h) + (percY * ((max.Y - min.Y) * h)));
                }
                else if (cbi.PeakHold) // max is used for peak hold
                {
                    getPerc(cbi, cbi.MaxHistory, out percX, out percY, out min, out max);
                    maxHistory_y = yBottom - ((min.Y * h) + (percY * ((max.Y - min.Y) * h)));
                }

                int segmentBlockSize = (int)(w * 0.02f);
                if (segmentBlockSize < 7) segmentBlockSize = 7;
                int segmentGapSize = (int)(segmentBlockSize * 0.2f);
                if (segmentBlockSize < 2) segmentBlockSize = 2;
                int segmentStep = segmentBlockSize + segmentGapSize;

                if (cbi.ShowHistory)
                {
                    switch (cbi.Style)
                    {
                        case clsBarItem.BarStyle.None:
                        case clsBarItem.BarStyle.SolidFilled:
                        case clsBarItem.BarStyle.Line:
                            {
                                SharpDX.RectangleF history = new SharpDX.RectangleF(x, maxHistory_y, w, minHistory_y - maxHistory_y); // drawn TL to BR
                                _renderTarget.FillRectangle(history, getDXBrushForColour(cbi.HistoryColour));
                            }
                            break;
                        //case clsBarItem.BarStyle.Segments:
                        //    {
                        //        int numValueBlocks = (int)((xPos - x) / (float)segmentStep);

                        //        float i;
                        //        float startX = (numValueBlocks * segmentStep) + x;
                        //        SharpDX.RectangleF barrect;
                        //        for (i = startX; i < maxHistory_x - segmentStep; i += segmentStep)
                        //        {
                        //            barrect = new SharpDX.RectangleF(i, y, segmentBlockSize, h);
                        //            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.HistoryColour));
                        //        }

                        //        // complete the sliver
                        //        if (i < maxHistory_x)
                        //        {
                        //            barrect = new SharpDX.RectangleF(i, y, maxHistory_x - i, h);
                        //            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.HistoryColour));
                        //        }
                        //    }
                        //    break;
                    }
                }

                switch (cbi.Style)
                {
                    case clsBarItem.BarStyle.SolidFilled:
                        {
                            SharpDX.RectangleF barrect = new SharpDX.RectangleF(x, yPos, w, yBottom - yPos);

                            if (cbi.PeakHold)
                                _renderTarget.DrawLine(new SharpDX.Vector2(x, maxHistory_y), new SharpDX.Vector2(x + w, maxHistory_y), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                        }
                        break;
                    case clsBarItem.BarStyle.Line:
                        {
                            if (cbi.PeakHold)
                                _renderTarget.DrawLine(new SharpDX.Vector2(x, maxHistory_y), new SharpDX.Vector2(x + w, maxHistory_y), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                            _renderTarget.DrawLine(new SharpDX.Vector2(x, yPos), new SharpDX.Vector2(x + w, yPos), getDXBrushForColour(cbi.MarkerColour), cbi.StrokeWidth);
                        }
                        break;
                    //case clsBarItem.BarStyle.Segments:
                    //    {
                    //        float i;
                    //        SharpDX.RectangleF barrect;
                    //        for (i = x; i < xPos - segmentStep; i += segmentStep)
                    //        {
                    //            barrect = new SharpDX.RectangleF(i, y, segmentBlockSize, h);
                    //            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                    //        }

                    //        // complete the sliver
                    //        if (i < xPos)
                    //        {
                    //            barrect = new SharpDX.RectangleF(i, y, xPos - i, h);
                    //            _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                    //        }

                    //        if (cbi.PeakHold)
                    //            _renderTarget.DrawLine(new SharpDX.Vector2(maxHistory_x, y), new SharpDX.Vector2(maxHistory_x, y + h), getDXBrushForColour(cbi.PeakHoldMarkerColour), cbi.StrokeWidth);

                    //        if (cbi.ShowMarker)
                    //            _renderTarget.DrawLine(new SharpDX.Vector2(xPos, y), new SharpDX.Vector2(xPos, y + h), getDXBrushForColour(cbi.MarkerColour), cbi.StrokeWidth);
                    //    }
                    //    break;
                }

                if (cbi.ShowValue)
                {
                    string sText = cbi.Value.ToString("0.0") + MeterManager.ReadingUnits(cbi.ReadingSource);

                    float fontSize = 38f;//cbi.FontSize; //38f;
                    SizeF adjustedFontSize = measureString("0", cbi.FontFamily, cbi.FntStyle, fontSize);
                    float ratio = h / adjustedFontSize.Height;
                    float newSize = (float)Math.Round((fontSize * ratio) * (fontSize / _renderTarget.DotsPerInch.Width), 1);

                    SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y + (h * 0.2f), w, h);
                    _renderTarget.DrawText(sText, getDXTextFormatForFont(cbi.FontFamily, newSize, cbi.FntStyle), txtrect, getDXBrushForColour(cbi.FontColour));
                }
            }
            private void renderSolidColour(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsSolidColour sc = (clsSolidColour)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF rectSC = new SharpDX.RectangleF(x, y, w, h);
                _renderTarget.FillRectangle(rectSC, getDXBrushForColour(sc.Colour));
            }
            private void renderImage(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsImage ipg = (clsImage)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                string sImage = ipg.ImageName;

                string sKey = sImage + "-" + MeterManager.CurrentPowerRating.ToString();
                if (/*_images.ContainsKey*/MeterManager.ContainsBitmap(sKey)) sImage = sKey;

                sKey = sImage + "-small";
                if ((w <= 200 || h <= 200) && /*_images.ContainsKey*/MeterManager.ContainsBitmap(sKey)) sImage = sKey; // use small version of the image if available

                if (/*_images.ContainsKey*/MeterManager.ContainsBitmap(sImage))
                {
                    SharpDX.RectangleF imgRect = new SharpDX.RectangleF(x, y, w, h);

                    if (!ipg.Clipped)
                    {
                        _renderTarget.PushAxisAlignedClip(imgRect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle, no need to cut the image
                    }
                    else
                    {
                        float cx = (ipg.ClipTopLeft.X / m.XRatio) * rect.Width;
                        float cy = (ipg.ClipTopLeft.Y / m.YRatio) * rect.Height;
                        float cw = rect.Width * (ipg.ClipSize.Width / m.XRatio);
                        float ch = rect.Height * (ipg.ClipSize.Height / m.YRatio);

                        SharpDX.RectangleF clipRect = new SharpDX.RectangleF(cx, cy, cw, ch);
                        _renderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle, no nee to cut the image
                    }

                    SharpDX.Direct2D1.Bitmap b = _images[sImage];

                    // maintain aspect ratio, the clip removes anything outside the rect
                    float im_w = b.Size.Width;
                    float im_h = b.Size.Height;

                    if (w > h) 
                        imgRect.Height = imgRect.Width * (im_h / im_w);
                    else
                        imgRect.Width = imgRect.Height * (im_w / im_h);

                    _renderTarget.DrawBitmap(b, imgRect, 1f, BitmapInterpolationMode.Linear);//, sourceRect);

                    _renderTarget.PopAxisAlignedClip();
                }
            }
            //private Vector _p;
            //private Vector _p2;
            //private Vector _q;
            //private Vector _q2;

            //private bool _bGotPWR = false;
            //private bool _bGotRefPWR = false;

            //private Dictionary<string, SharpDX.Vector2> _fanSWR = new Dictionary<string, Vector2>();

            private void renderNeedle(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsNeedleItem ni = (clsNeedleItem)mi;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF nirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(nirect, getDXBrushForColour(System.Drawing.Color.Red));

                _renderTarget.PushAxisAlignedClip(nirect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle

                //float startX = x + (w * ni.NeedleOffset.X);
                //float startY = y + (h * ni.NeedleOffset.Y);

                // needle offset from centre
                float cX = x + (w / 2);
                float cY = y + (h / 2);
                float startX = cX + (w * ni.NeedleOffset.X);
                float startY = cY + (h * ni.NeedleOffset.Y);

                float rotation = 180f;

                float radiusX = (w / 2) * (ni.LengthFactor * ni.RadiusRatio.X);
                float radiusY = (w / 2) * (ni.LengthFactor * ni.RadiusRatio.Y);

                //todo
                switch (ni.Placement)
                {
                    case clsNeedleItem.NeedlePlacement.Bottom:
                        rotation = 180f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Left:
                        //rotation = 90f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Top:
                        //rotation = 0f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Right:
                        //rotation = 270f;
                        break;
                }

                float eX, eY, dX, dY;

                if (ni.ShowHistory || ni.Setup)
                {
                    float valueMin;
                    float valueMax;

                    if (ni.Setup) 
                    {
                        KeyValuePair<float, PointF> kvp;
                        kvp = ni.ScaleCalibration.OrderBy(p => p.Key).First();
                        valueMin = kvp.Key;
                        kvp = ni.ScaleCalibration.OrderByDescending(p => p.Key).First();
                        valueMax = kvp.Key;
                    }
                    else
                    {
                        valueMin = ni.MinHistory;
                        valueMax = ni.MaxHistory;
                    }

                    getPerc(ni, valueMin, out float minPercX, out float minPercY, out PointF minHistoryMin, out PointF minHistoryMax);
                    getPerc(ni, valueMax, out float maxPercX, out float maxPercY, out PointF maxHistoryMin, out PointF maxHistoryMax);

                    //if (ni.Direction == clsNeedleItem.NeedleDirection.CounterClockwise)
                    //{
                    //    minPercX = 1f - minPercX;
                    //    maxPercX = 1f - maxPercX;
                    //    minPercY = 1f - minPercY;
                    //    maxPercY = 1f - maxPercY;
                    //}

                    // map the meter scales to pixels
                    float eXmin = x + (minHistoryMin.X * w) + (minPercX * ((minHistoryMax.X - minHistoryMin.X) * w));
                    float eYmin = y + (minHistoryMin.Y * h) + (minPercY * ((minHistoryMax.Y - minHistoryMin.Y) * h));
                    float eXmax = x + (maxHistoryMin.X * w) + (maxPercX * ((maxHistoryMax.X - maxHistoryMin.X) * w));
                    float eYmax = y + (maxHistoryMin.Y * h) + (maxPercY * ((maxHistoryMax.Y - maxHistoryMin.Y) * h));

                    // calc angle required
                    float dXmin = startX - eXmin;
                    float dYmin = startY - eYmin;
                    float dXmax = startX - eXmax;
                    float dYmax = startY - eYmax;
                    // expand for ellipse
                    dXmin /= ni.RadiusRatio.X;
                    dYmin /= ni.RadiusRatio.Y;
                    dXmax /= ni.RadiusRatio.X;
                    dYmax /= ni.RadiusRatio.Y;
                    //
                    float angMin = (float)Math.Atan2(dYmin, dXmin);
                    float angMax = (float)Math.Atan2(dYmax, dXmax);

                    float endMinX = startX + (float)(Math.Cos(angMin + degToRad(rotation)) * radiusX);
                    float endMinY = startY + (float)(Math.Sin(angMin + degToRad(rotation)) * radiusY);
                    float endMaxX = startX + (float)(Math.Cos(angMax + degToRad(rotation)) * radiusX);
                    float endMaxY = startY + (float)(Math.Sin(angMax + degToRad(rotation)) * radiusY);

                    PathGeometry sharpGeometry = new PathGeometry(_renderTarget.Factory);

                    GeometrySink geo = sharpGeometry.Open();
                    geo.BeginFigure(new SharpDX.Vector2(startX, startY), FigureBegin.Filled);

                    geo.AddLine(new SharpDX.Vector2(endMinX, endMinY));

                    ArcSegment arcSegment = new ArcSegment();
                    arcSegment.Point = new SharpDX.Vector2(endMaxX, endMaxY);
                    arcSegment.SweepDirection = ni.Direction == clsNeedleItem.NeedleDirection.Clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
                    arcSegment.ArcSize = Math.Abs(radToDeg(angMax) - radToDeg(angMin)) <= 180f ? ArcSize.Small : ArcSize.Large;
                    arcSegment.Size = new Size2F(radiusX, radiusY);
                    geo.AddArc(arcSegment);

                    geo.EndFigure(FigureEnd.Closed); // adds the closing line
                    geo.Close();

                    _renderTarget.FillGeometry(sharpGeometry, getDXBrushForColour(ni.HistoryColour));

                    Utilities.Dispose(ref geo);
                    geo = null;
                    Utilities.Dispose(ref sharpGeometry);
                    sharpGeometry = null;
                }

                getPerc(ni, ni.Value, out float percX, out float percY, out PointF min, out PointF max);
                //if (ni.Direction == clsNeedleItem.NeedleDirection.CounterClockwise)
                //{
                //    percX = 1f - percX;
                //    percY = 1f - percY;
                //}

                // map the meter scales to pixels
                eX = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                eY = y + (min.Y * h) + (percY * ((max.Y - min.Y) * h));

                // calc angle required
                dX = startX - eX;
                dY = startY - eY;
                // expand
                dX /= ni.RadiusRatio.X;
                dY /= ni.RadiusRatio.Y;
                float ang = (float)Math.Atan2(dY, dX);

                float endX = startX + (float)(Math.Cos(ang + degToRad(rotation)) * radiusX);
                float endY = startY + (float)(Math.Sin(ang + degToRad(rotation)) * radiusY);

                float fScale = 1f;
                if (ni.ScaleStrokeWidth)
                {
                    //float fSmallestDimension = w < h ? w : h;
                    //fScale = fSmallestDimension / 200f;
                    float fDiag = (float)Math.Sqrt((w*w) + (h*h));
                    fScale = fDiag / 450f;
                }
                _renderTarget.DrawLine(new SharpDX.Vector2(startX, startY), new SharpDX.Vector2(endX, endY), getDXBrushForColour(ni.Colour), ni.StrokeWidth * fScale);

                if (ni.Setup)
                {
                    foreach (KeyValuePair<float, PointF> kvp in ni.ScaleCalibration)
                    {
                        PointF p = kvp.Value;
                        _renderTarget.FillEllipse(new Ellipse(new SharpDX.Vector2(x + (p.X * w), y + (p.Y * h)), 2f, 2f), getDXBrushForColour(System.Drawing.Color.Red));
                    }
                }
                //if (ni.ReadingSource == Reading.PWR)
                //{
                //    _p = new Vector(startX, startY);
                //    _p2 = new Vector(endX, endY);
                //    _bGotPWR = true;
                //}
                //if(ni.ReadingSource == Reading.REVERSE_PWR)
                //{
                //    _q = new Vector(startX, startY);
                //    _q2 = new Vector(endX, endY);
                //    _bGotRefPWR = true;
                    
                //}
                //if(_bGotPWR && _bGotRefPWR)
                //{
                //    bool b = lineSegementsIntersect(_p, _p2, _q, _q2, out Vector intersect);

                //    if (b)
                //    {
                //        float xx = (float)(_p.X + (_q.X - _p.X)/2f);
                //        float yy = (float)(_p.Y + (_q.Y - _p.Y)/2f);

                //        string sKey = MeterManager.getReading(_rx, Reading.REVERSE_PWR).ToString("0.000");
                //        if (!_fanSWR.ContainsKey(sKey))
                //            _fanSWR.Add(sKey, new SharpDX.Vector2((float)intersect.X, (float)intersect.Y));

                //        foreach(KeyValuePair<string, SharpDX.Vector2> pair in _fanSWR)
                //        {
                //            _renderTarget.DrawLine(new SharpDX.Vector2(xx, yy), new SharpDX.Vector2(pair.Value.X, pair.Value.Y), getDXBrushForColour(System.Drawing.Color.Red), ni.StrokeWidth);
                //        }
                //    }

                //    _bGotPWR = false;
                //    _bGotRefPWR = false;
                //}

                _renderTarget.PopAxisAlignedClip();
            }
            private double degToRad(float deg)
            {
                return (deg * Math.PI) / 180f;
            }
            private double radToDeg(float rad)
            {
                return rad * (180f / Math.PI);
            }
            private void getPerc(clsMeterItem mi, float value, out float percX, out float percY, out PointF min, out PointF max)
            {
                percX = 0;
                percY = 0;
                min = new PointF(0f, 0f);
                max = new PointF(0f, 0f);

                value = (float)Math.Round(value, 3);

                // adjust for >= 30mhz
                if (mi.ReadingSource == Reading.SIGNAL_STRENGTH || mi.ReadingSource == Reading.AVG_SIGNAL_STRENGTH)
                    value += MeterManager.dbmOffsetForAbove30(_rx);

                // normalise to 100w for needles?
                else if (mi.NormaliseTo100W)
                    value *= MeterManager.normaliseTo100W();


                if (mi.ScaleCalibration.Count > 0)
                {
                    // todo, only implemented for clockwise needles atm
                    PointF p_low = PointF.Empty;
                    PointF p_high = PointF.Empty;
                    float value_low = -200f;
                    float value_high = -200f;
                    bool bEqual = false;

                    if (mi.ScaleCalibration.ContainsKey(value))
                    {
                        p_low = mi.ScaleCalibration[value];
                        p_high = mi.ScaleCalibration[value];
                        value_low = value;
                        value_high = value;
                        bEqual = true;
                    }
                    else if (value < mi.ScaleCalibration.OrderBy(p => p.Key).First().Key) // below lowest
                    {
                        p_low = p_high = mi.ScaleCalibration.OrderBy(p => p.Key).First().Value;
                        value_low = value_high = mi.ScaleCalibration.OrderBy(p => p.Key).First().Key;
                        bEqual = true;
                    }
                    else if (value > mi.ScaleCalibration.OrderBy(p => p.Key).Last().Key) // above highest
                    {
                        p_low = p_high = mi.ScaleCalibration.OrderBy(p => p.Key).Last().Value;
                        value_low = value_high = mi.ScaleCalibration.OrderBy(p => p.Key).Last().Key;
                        bEqual = true;
                    }
                    else
                    {
                        //low side
                        foreach (KeyValuePair<float, PointF> kvp in mi.ScaleCalibration.OrderByDescending(p => p.Key))
                        {
                            if (kvp.Key <= value)
                            {
                                p_low = kvp.Value;
                                value_low = kvp.Key;
                                break;
                            }
                        }

                        // high
                        foreach (KeyValuePair<float, PointF> kvp in mi.ScaleCalibration.OrderBy(p => p.Key))
                        {
                            if (kvp.Key >= value)
                            {
                                p_high = kvp.Value;
                                value_high = kvp.Key;
                                break;
                            }
                        }
                    }
                    //get mins/maxs
                    float minX = (float)Math.Round(mi.ScaleCalibration.OrderBy(p => p.Value.X).First().Value.X, 3);
                    float maxX = (float)Math.Round(mi.ScaleCalibration.OrderByDescending(p => p.Value.X).First().Value.X, 3);

                    float minY = (float)Math.Round(mi.ScaleCalibration.OrderBy(p => p.Value.Y).First().Value.Y, 3);
                    float maxY = (float)Math.Round(mi.ScaleCalibration.OrderByDescending(p => p.Value.Y).First().Value.Y, 3);

                    float rangeX = maxX - minX;
                    float rangeY = maxY - minY;

                    if (bEqual)
                    {
                        percX = (p_low.X - minX) / rangeX;
                        percY = (p_low.Y - minY) / rangeY;
                    }
                    else
                    {
                        float value_range = value_high - value_low;
                        float ratio = (value - value_low) / value_range;
                        float newRangeX = p_high.X - p_low.X;
                        float newRangeY = p_high.Y - p_low.Y;

                        percX = ((p_low.X - minX) + (newRangeX * ratio)) / rangeX;
                        percY = ((p_low.Y - minY) + (newRangeY * ratio)) / rangeY;
                    }

                    percX = (float)Math.Round(percX, 3);
                    percY = (float)Math.Round(percY, 3);
                    min = new PointF(minX, minY);
                    max = new PointF(maxX, maxY);
                }
            }
            private SharpDX.Direct2D1.Bitmap bitmapFromSysBitmap(RenderTarget rt, System.Drawing.Bitmap bitmap)
            {
                System.Drawing.Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapProperties bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)); //was R8G8B8A8_UNorm  //MW0LGE_21k9
                Size2 size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                DataStream tempStream = new DataStream(bitmap.Height * stride, true, true);

                // Lock System.Drawing.Bitmap
                System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

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
                        //int rgba = R | (G << 8) | (B << 16) | (A << 24); //MW0LGE_21k9
                        //tempStream.Write(rgba);
                        int bgra = B | (G << 8) | (R << 16) | (A << 24);
                        tempStream.Write(bgra);
                    }

                }
                bitmap.UnlockBits(bitmapData);

                tempStream.Position = 0;

                SharpDX.Direct2D1.Bitmap dxBitmap = new SharpDX.Direct2D1.Bitmap(rt, size, tempStream, stride, bitmapProperties);

                Utilities.Dispose(ref tempStream);
                tempStream = null;

                return dxBitmap;
            }
            private SharpDX.Direct2D1.Bitmap bitmapFromSysBitmap2(RenderTarget rt, System.Drawing.Bitmap bitmap, string sId)
            {
                SharpDX.Direct2D1.Bitmap dxBitmap;
                Size2 size = new Size2(bitmap.Width, bitmap.Height);
                int stride = bitmap.Width * sizeof(int);
                BitmapProperties bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)); //was R8G8B8A8_UNorm  //MW0LGE_21k9

                if (MeterManager.ContainsStreamData(sId))
                {
                    DataStream tempStream = MeterManager.GetStreamData(sId);
                    tempStream.Position = 0;

                    dxBitmap = new SharpDX.Direct2D1.Bitmap(rt, size, tempStream, stride, bitmapProperties);
                }
                else
                {
                    System.Drawing.Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    // Transform pixels from BGRA to RGBA
                    DataStream tempStream = new DataStream(bitmap.Height * stride, true, true);

                    // Lock System.Drawing.Bitmap
                    System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

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
                            //int rgba = R | (G << 8) | (B << 16) | (A << 24); //MW0LGE_21k9
                            //tempStream.Write(rgba);
                            int bgra = B | (G << 8) | (R << 16) | (A << 24);
                            tempStream.Write(bgra);
                        }

                    }
                    bitmap.UnlockBits(bitmapData);

                    tempStream.Position = 0;

                    dxBitmap = new SharpDX.Direct2D1.Bitmap(rt, size, tempStream, stride, bitmapProperties);

                    //Utilities.Dispose(ref tempStream);
                    //tempStream = null;

                    MeterManager.AddStreamData(sId, tempStream);
                }
                return dxBitmap;
            }
            //private class Vector
            //{
            //    public double X;
            //    public double Y;

            //    // Constructors.
            //    public Vector(double x, double y) { X = x; Y = y; }
            //    public Vector() : this(double.NaN, double.NaN) { }

            //    public static Vector operator -(Vector v, Vector w)
            //    {
            //        return new Vector(v.X - w.X, v.Y - w.Y);
            //    }

            //    public static Vector operator +(Vector v, Vector w)
            //    {
            //        return new Vector(v.X + w.X, v.Y + w.Y);
            //    }

            //    public static double operator *(Vector v, Vector w)
            //    {
            //        return v.X * w.X + v.Y * w.Y;
            //    }

            //    public static Vector operator *(Vector v, double mult)
            //    {
            //        return new Vector(v.X * mult, v.Y * mult);
            //    }

            //    public static Vector operator *(double mult, Vector v)
            //    {
            //        return new Vector(v.X * mult, v.Y * mult);
            //    }

            //    public double Cross(Vector v)
            //    {
            //        return X * v.Y - Y * v.X;
            //    }

            //    public override bool Equals(object obj)
            //    {
            //        var v = (Vector)obj;
            //        return (X - v.X).IsZero() && (Y - v.Y).IsZero();
            //    }
            //}
            ////https://www.codeproject.com/Tips/862988/Find-the-Intersection-Point-of-Two-Line-Segments
            //private bool lineSegementsIntersect(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
            //{
            //    intersection = new Vector();

            //    Vector r = p2 - p;
            //    Vector s = q2 - q;
            //    double rxs = r.Cross(s);
            //    double qpxr = (q - p).Cross(r);

            //    // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            //    if (rxs.IsZero() && qpxr.IsZero())
            //    {
            //        // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
            //        // then the two lines are overlapping,
            //        if (considerCollinearOverlapAsIntersect)
            //            if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
            //                return true;

            //        // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
            //        // then the two lines are collinear but disjoint.
            //        // No need to implement this expression, as it follows from the expression above.
            //        return false;
            //    }

            //    // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            //    if (rxs.IsZero() && !qpxr.IsZero())
            //        return false;

            //    // t = (q - p) x s / (r x s)
            //    double t = (q - p).Cross(s) / rxs;

            //    // u = (q - p) x r / (r x s)

            //    double u = (q - p).Cross(r) / rxs;

            //    // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            //    // the two line segments meet at the point p + t r = q + u s.
            //    if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            //    {
            //        // We can calculate the intersection point using either t or u.
            //        intersection = p + t * r;

            //        // An intersection was found.
            //        return true;
            //    }

            //    // 5. Otherwise, the two line segments are not parallel but do not intersect.
            //    return false;
            //}
        }        
        #endregion
    }
}

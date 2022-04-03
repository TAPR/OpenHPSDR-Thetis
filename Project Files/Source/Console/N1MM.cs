//=================================================================
// N1MM.cs - MW0LGE 2021
// N1MM+ spectral UDP output
//=================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Thetis
{
    public static class N1MM
    {
        const double KEEP_TIME = 0.1;

        private struct ReceiverStoredData
        {
            public float[] storedData;
            public double LowFreq;
            public double HighFreq;
            public bool Enabled;
            public int Width;
            public float Scale;
            public bool DataReady;
        }

        private static ReceiverStoredData[] RXstoredData;
        private static int m_nDestinationPort = 13064;
        private static string m_sDestinationIP = "255.255.255.255";

        static N1MM()
        {
            setMaxRXs(2);
        }
     
        public static bool IsEnabled(int rx)
        {
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return false;

            return RXstoredData[rx - 1].Enabled;
        }
        public static void SetEnabled(int rx, bool enable)
        {
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return;

            RXstoredData[rx - 1].Enabled = enable;
        }
        private static void setLowFrequencyMHz(int rx, double freq)
        {
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return;

            RXstoredData[rx-1].LowFreq = freq;
        }
        private static void setHighFrequencyMHz(int rx, double freq)
        {
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return;

            RXstoredData[rx-1].HighFreq = freq;
        }

        private static int m_nSendRate = 8;
        public static int SendRate {
            get { return m_nSendRate; }
            set { m_nSendRate = value; }
        }
        private static bool m_bStarted = false;
        private static Task udp_send_task;
        private static CancellationTokenSource cancelTokenSource;
        public static void Stop()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
                cancelTokenSource = null;
            }

            m_bStarted = false;
        }
        private static void setMaxRXs(int rxNumber)
        {
            RXstoredData = new ReceiverStoredData[rxNumber];
            for(int i = 0; i < RXstoredData.Length; i++)
            {
                RXstoredData[i].DataReady = false;
                RXstoredData[i].Enabled = false;
            }
        }

        public static void Resize(int rx)
        {
            if (!Display.IsDX2DSetup) return;

            if (rx < 1 || rx > 2) return;
            // init the arrays

            int width = Display.Target.Width / Display.Decimation;

            if (RXstoredData[rx - 1].storedData == null || RXstoredData[rx - 1].Width < width)
            {
                RXstoredData[rx - 1].storedData = new float[width];
            }
            RXstoredData[rx - 1].Width = width;

            if (IsEnabled(rx))
                {
                    // use the display versions of everything, as this is all that matters to N1MM as we are based on the picDisplay pixels
                    double bandWidth = rx == 1 ? Display.RXDisplayHigh - Display.RXDisplayLow : Display.RX2DisplayHigh - Display.RX2DisplayLow;
                    double centreFrequency = rx == 1 ? Display.CentreFreqRX1 : Display.CentreFreqRX2;

                    double dL = Math.Round(centreFrequency - ((bandWidth / 2) * 1e-6), 6);
                    double dH = Math.Round(centreFrequency + ((bandWidth / 2) * 1e-6), 6);

                    setLowFrequencyMHz(rx, dL);
                    setHighFrequencyMHz(rx, dH);
                }
        }

        private static readonly object m_objLock = new Object();

        public static void CopyData(int rx, float[] newData, bool bUse)
        {
            if (!m_bStarted) return;
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return;
            if (!RXstoredData[rx - 1].Enabled) return;

            if (RXstoredData[rx-1].DataReady) return;

            lock (m_objLock)
            {
                RXstoredData[rx-1].DataReady = false;

                if (bUse)
                {
                    unsafe
                    {
                        fixed (void* originalptr = &newData[0])
                        fixed (void* newptr = &RXstoredData[rx - 1].storedData[0])
                            Win32.memcpy(newptr, originalptr, RXstoredData[rx - 1].Width * sizeof(float));
                    }

                    RXstoredData[rx-1].DataReady = true;
                }
            }
        }
        public static void Start()
        {
            if (cancelTokenSource != null) return;

            cancelTokenSource = new CancellationTokenSource();

            m_bStarted = true;

            udp_send_task = Task.Factory.StartNew(() =>
            {
                while (cancelTokenSource != null && !cancelTokenSource.IsCancellationRequested)
                {
                    lock (m_objLock)
                    {
                        try
                        {
                            sendUDPData();
                        }
                        catch { }
                    }
                    Thread.Sleep(1000 / m_nSendRate);
                }

            }, cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public static bool IsStarted {
            get { return m_bStarted; }
        }
        public static string DestinationIP {
            get { return m_sDestinationIP; }
            set {
                string[] parts = value.Split(':');
                string sTmp = "";

                if(parts.Length==1) sTmp = value;
                else if (parts.Length > 1)
                {
                    sTmp = parts[0];
                    bool b = int.TryParse(parts[1], out int port);
                    if (b) DestinationPort = port;
                }

                //check ip is valid
                bool bOK = IPAddress.TryParse(sTmp, out IPAddress address);
                if (bOK)
                {
                    m_sDestinationIP = address.ToString();
                }
            }
        }
        public static int DestinationPort {
            get { return m_nDestinationPort; }
            set { m_nDestinationPort = value; }
        }
        public static void SetScale(int rx, float fScale)
        {
            if (RXstoredData == null || rx > RXstoredData.Length || rx < 1) return;

            RXstoredData[rx - 1].Scale = fScale;
        }
        private static void sendUDPData()
        {
            if (RXstoredData == null) return;

            for (int nn = 0; nn < RXstoredData.Length; nn++)
            {
                ReceiverStoredData rsd = RXstoredData[nn];

                if (rsd.storedData != null && rsd.Enabled && rsd.DataReady)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = false;

                    // Create an XML declaration.
                    XmlDeclaration xmldecl;
                    xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
                    xmldecl.Encoding = "UTF-8";
                    xmldecl.Standalone = "yes";

                    doc.AppendChild(xmldecl);

                    XmlElement spec = doc.CreateElement("Spectrum");
                    doc.AppendChild(spec);

                    XmlElement app = doc.CreateElement("app");
                    app.InnerText = "WaterfallBandmap";

                    XmlElement name = doc.CreateElement("Name");
                    name.InnerText = "Thetis_" + (nn + 1).ToString();

                    XmlElement low = doc.CreateElement("LowScopeFrequency");
                    //low.InnerText = ((int)Math.Round(rsd.LowFreq * 1e3, 3)).ToString();  // MW0LGE_21k9c to fix issue where there was an offset due to lack of accuracy
                    low.InnerText = (rsd.LowFreq * 1e3).ToString("0.000");

                    XmlElement high = doc.CreateElement("HighScopeFrequency");
                    //high.InnerText = ((int)Math.Round(rsd.HighFreq * 1e3, 3)).ToString();  // MW0LGE_21k9c to fix issue where there was an offset due to lack of accuracy
                    high.InnerText = (rsd.HighFreq * 1e3).ToString("0.000");

                    XmlElement sf = doc.CreateElement("ScalingFactor");
                    sf.InnerText = rsd.Scale.ToString("0.00");

                    XmlElement dataCount = doc.CreateElement("DataCount");
                    dataCount.InnerText = rsd.Width.ToString();

                    XmlElement spectrumData = doc.CreateElement("SpectrumData");
                    string specData = "";

                    double min = double.MaxValue;
                    for(int i = 0; i < rsd.Width; i++)
                    {
                        if (rsd.storedData[i] < min) min = rsd.storedData[i];
                    }
                    min = Math.Abs(min);

                    for (int n = 0; n < rsd.Width; n++)
                    {
                        int v = (int)rsd.storedData[n] + (int)min;
                        specData += v.ToString() + ",";
                    }
                    if (specData != "") specData = specData.Substring(0, specData.Length - 1); // drop ,

                    spectrumData.InnerText = specData;

                    spec.AppendChild(app);
                    spec.AppendChild(name);
                    spec.AppendChild(low);
                    spec.AppendChild(high);
                    spec.AppendChild(sf);
                    spec.AppendChild(dataCount);
                    spec.AppendChild(spectrumData);

                    byte[] finalData = Encoding.UTF8.GetBytes(doc.OuterXml);

                    using (UdpClient udpClient = new UdpClient())
                    {
                        udpClient.Send(finalData, finalData.Length, m_sDestinationIP, m_nDestinationPort);
                        udpClient.Close();
                    }

                    //clear down, ready for more
                    RXstoredData[nn].DataReady = false;
                }
            }
        }
    }
}

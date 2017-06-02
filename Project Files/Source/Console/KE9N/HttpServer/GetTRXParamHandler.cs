using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HttpServer
{
    public class GetTRXParamHandler : IHandler
    {
        public GetTRXParamHandler(string body, TcpClient tcpClient) :
            base(body, tcpClient)
        {
        }

        public const string THETIS = "Thetis";
        public const string VFOAFreq = "VFOAFreq";
        public const string VFOBFreq = "VFOBFreq";
        public const string BAND = "Band";
        public const string MODE = "Mode";
        public const string VFOA_FIL = "VFOA_FIL";

        public override void handle()
        {
            string CodeStr = "200 " + ((HttpStatusCode)200).ToString();

            string xml = getXml();
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nAccess-Control-Allow-Origin: *\nContent-Length:" + xml.Length.ToString() + "\n\n" + xml;
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            sendAnswer(Buffer);
        }

        string getXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode root = doc.CreateElement(THETIS);
            doc.AppendChild(root);

            XmlNode vFOAFreq = doc.CreateElement(VFOAFreq);
            vFOAFreq.InnerText = m_console.getVFOAFreqString();
            root.AppendChild(vFOAFreq);

            XmlNode vFOBFreq = doc.CreateElement(VFOBFreq);
            vFOBFreq.InnerText = m_console.getVFOBFreqString();
            root.AppendChild(vFOBFreq);

            XmlElement mode = doc.CreateElement(MODE);
            mode.InnerText = m_console.RX1DSPMode.ToString();
            root.AppendChild(mode);

            XmlNode vfoa_fil = doc.CreateElement(VFOA_FIL);
            vfoa_fil.InnerText = m_console.RX1Filter.ToString();
            root.AppendChild(vfoa_fil);

            XmlNode band = doc.CreateElement(BAND);
            band.InnerText = m_console.RX1Band.ToString();
            root.AppendChild(band);

            return doc.OuterXml;
        }
    }
}

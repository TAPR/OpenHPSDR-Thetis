using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServer
{
    class UnknownRequestHandler : IHandler
    {
        public UnknownRequestHandler(String body, TcpClient tcpClient):
            base(body, tcpClient)
        {
        }

        public override void handle()
        {
            if (m_tcpClient == null) return;

            string CodeStr = "404 " + ((HttpStatusCode)404).ToString();
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            m_tcpClient.GetStream().Write(Buffer, 0, Buffer.Length);
            m_tcpClient.Close();
        }
    }
}
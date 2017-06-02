using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace HttpServer
{
    enum RequestType
    {
        GET_IMAGE,
        GET_HTML_INDEX_PAGE,
        SET_PARAM,
        GET_TRX_PARAM,
        UNKNOWN,
        ERROR
    }

    public abstract class IHandler
    {
        public IHandler(string body, TcpClient tcpClient)
        {
            m_tcpClient = tcpClient;
            m_body = body;
        }

        public static void setConsole(Thetis.Console console)
        {
            m_console = console;
        }

        public abstract void handle();

        protected void sendAnswer(byte[] data)
        {
            if(m_tcpClient == null) return;

            NetworkStream stream = m_tcpClient.GetStream();
            stream.ReadTimeout = 10;
            try
            {
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);                  
                }
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            stream.Close();
            m_tcpClient.Close();
        }

        protected string m_body;
        protected TcpClient m_tcpClient;
        protected static Thetis.Console m_console;



    }
}
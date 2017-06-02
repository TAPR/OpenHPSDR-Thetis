using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace HttpServer
{
    public class ImageRequestHandler : IHandler
    {

        public ImageRequestHandler(String body, TcpClient tcpClient):
            base(body, tcpClient)
        {
        }

        public override void handle()
        {
            if (m_tcpClient == null) return;

            byte[] imageArray = getImage();

            if (imageArray == null)
            {
                string CodeStr = "200 " + ((System.Net.HttpStatusCode)200).ToString();

                string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";

                string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

                byte[] Buffer = Encoding.ASCII.GetBytes(Str);

                sendAnswer(Buffer);
            }

            //  "<meta http-equiv= \"refresh\" content= \"500\" > \r\n" +

            string responseHeaders = "HTTP/1.1 200 The file is coming right up!\r\n" +
                                     "Server: MyOwnServer\r\n" +
                                    "Content-Length: " + imageArray.Length + "\r\n" +
                                    "Content-Type: image/jpeg\r\n" +
                                    "Content-Disposition: inline;filename=\"picDisplay.jpg;\"\r\n" +
                                    "\r\n";



            byte[] headerArray = Encoding.ASCII.GetBytes(responseHeaders);

            byte[] data = new byte[headerArray.Length + imageArray.Length];

            Array.Copy(headerArray, 0, data, 0, headerArray.Length);
            Array.Copy(imageArray, 0, data, headerArray.Length, imageArray.Length);

            sendAnswer(data);
        }


        private byte[]  getImage()
        {
            Bitmap bitmap;
            byte[] picDisplayOutput;
            MemoryStream memstream;

            bitmap = new Bitmap(m_console.picDisplay.Width, m_console.picDisplay.Height); // ke9ns set bitmap size to size of picDisplay since it gets resized with your screen

            m_console.picDisplay.DrawToBitmap(bitmap, m_console.picDisplay.ClientRectangle); // ke9ns grab picDisplay and convert to bitmap

            using (memstream = new MemoryStream())
            {
                bitmap.Save(memstream, ImageFormat.Jpeg);
                picDisplayOutput = memstream.ToArray();
            }

            return picDisplayOutput;
        }
    }
}
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var listener = new UdpClient(4567);

            Console.WriteLine("Server hazırdır. Gözləyir...");

            while (true)
            {
                var result = await listener.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer).Trim().ToLower();
                Console.WriteLine($"{result.RemoteEndPoint} => {msg}");

                if (msg == "screenshot")
                {
                    int width = 640;
                    int height = 360;

                    using var bitmap = new Bitmap(width, height);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    }

                    using var ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    byte[] imgBytes = ms.ToArray();

                    if (imgBytes.Length < 60000)
                    {
                        await listener.SendAsync(imgBytes, imgBytes.Length, result.RemoteEndPoint);
                    }
                }
            }
        }
    }
}
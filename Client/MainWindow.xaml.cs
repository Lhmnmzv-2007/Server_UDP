using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cts;
        private readonly IPEndPoint _remoteEndPoint =
            new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4567);

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = false;
            Stop.IsEnabled = true;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            using var client = new UdpClient();
            byte[] request = Encoding.UTF8.GetBytes("screenshot");

            while (!token.IsCancellationRequested)
            {
                await client.SendAsync(request, request.Length, _remoteEndPoint);

                var result = await client.ReceiveAsync();

                using var ms = new MemoryStream(result.Buffer);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                screenshotImage.Source = bitmap;

                await Task.Delay(250, token);
            }

            Start.IsEnabled = true;
            Stop.IsEnabled = false;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            Stop.IsEnabled = false;
        }
    }
}
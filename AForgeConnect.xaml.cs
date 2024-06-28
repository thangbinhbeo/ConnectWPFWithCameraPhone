using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AForgeDemoOnWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            currentFrame?.Dispose();
            currentFrame = (Bitmap)eventArgs.Frame.Clone();

            Application.Current.Dispatcher.Invoke(() =>
            {
                videoImage.Source = BitmapToBitmapImage(currentFrame);
            });
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= VideoSource_NewFrame;
            }
            base.OnClosing(e);
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            var isIVCamRunning = Process.GetProcessesByName("iVCam").Any();

            if (!isIVCamRunning)
            {
                MessageBox.Show("iVCam is not running. Please start the iVCam app on your phone and computer.");
                return;
            }

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No video devices found");
                return;
            }

            bool iVCamFound = false;
            foreach (FilterInfo device in videoDevices)
            {
                if (device.Name.Contains("e2eSoft iVCam"))
                {
                    videoSource = new VideoCaptureDevice(device.MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                    btnStart.IsEnabled = false;
                    iVCamFound = true;
                    break;
                }
            }

            if (!iVCamFound)
            {
                MessageBox.Show("e2eSoft iVCam not found.");
            }
        }

        private void btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            if (currentFrame != null)
            {
                string filePath = $"Capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                currentFrame.Save(filePath, ImageFormat.Png);
                MessageBox.Show($"Image captured and saved to {filePath}");

                Process.Start("explorer.exe", filePath);
            }
            else
            {
                MessageBox.Show("No frame available to capture.");
            }
        }

        
    }
}
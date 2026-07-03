using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SCOI.WPF.Commands;
using SCOI.WPF.Utils;



namespace SCOI.WPF.ViewModels
{
    public class LayerVM : INotifyPropertyChanged
    {
        private string path;
        private string name;
        private Method overlayMethod;
        private bool r, g, b;
        private bool resize;
        private int offsetx, offsety;
        private int width, height;
        private double dpix, dpiy;
        private byte[] data;
        private byte[] resized;
        private BitmapSource bitmap;
        private double multiplier;
        private byte alpha;

        public string Path { get => path; set { path = value; } }
        public string Name { get => name; set { name = value; } }
        public Method OverlayMethod { get => overlayMethod; set { overlayMethod = value; OnPropertyChanged(nameof(Method)); } }
        public bool R { get => r; set { r = value; OnPropertyChanged(nameof(R)); } }
        public bool G { get => g; set { g = value; OnPropertyChanged(nameof(G)); } }
        public bool B { get => b; set { b = value; OnPropertyChanged(nameof(B)); } }
        public bool Resize { get => resize; set { resize = value; OnPropertyChanged(nameof(Resize)); } }
        public int OffsetX { get => offsetx; set { offsetx = value; OnPropertyChanged(nameof(OffsetX)); } }
        public int OffsetY { get => offsety; set { offsety = value; OnPropertyChanged(nameof(OffsetY)); } }
        public int Width { get => width; set { width = value; } }
        public int Height { get => height; set { height = value; } }
        public double DpiX { get => dpix; set { dpix = value; } }
        public double DpiY { get => dpiy; set { dpiy = value; } }
        public byte[] Data { get => data; set { data = value; OnPropertyChanged(nameof(Data)); } }
        public byte[] ResizedData { get => resized; set { data = value; OnPropertyChanged(nameof(ResizedData)); } }
        public double Multiplier { get => multiplier; set { multiplier = value; OnPropertyChanged(nameof(Multiplier)); } }
        public byte Alpha { get => alpha; set { alpha = value; OnPropertyChanged(nameof(Alpha)); } }
        public RelayCommand UpCommand { get; set; }
        public RelayCommand DownCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public BitmapSource Bitmap
        {
            get => bitmap;
            set
            {
                FormatConvertedBitmap converted = new FormatConvertedBitmap();
                converted.BeginInit();
                converted.Source = value;
                converted.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
                converted.EndInit();
                int stride = (int)converted.PixelWidth * (converted.Format.BitsPerPixel / 8);
                byte[] b = new byte[converted.PixelHeight * stride];
                converted.CopyPixels(b, stride, 0);
                bitmap = new BitmapImage();
                data = b;
                bitmap = BitmapSource.Create(converted.PixelWidth,
                                             converted.PixelHeight,
                                             converted.DpiX,
                                             converted.DpiY,
                                             converted.Format,
                                             converted.Palette,
                                             b,
                                             stride);
                Width = bitmap.PixelWidth;
                Height = bitmap.PixelHeight;
                DpiX = converted.DpiX;
                DpiY = converted.DpiY;
                OnPropertyChanged(nameof(Bitmap));

            }
        }
        public void CalculateResizedData(int width, int height)
        {
            var pic = BitmapHelper.BitmapFromSource(BitmapHelper.ByteToBitmap(data, this.width, this.height));
            var resized = BitmapHelper.Resize(pic, width, height);
            var source = BitmapHelper.GetSourceFromBitmap(resized);
            source.Freeze();
            FormatConvertedBitmap converted = new FormatConvertedBitmap();
            converted.BeginInit();
            converted.Source = source;
            converted.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
            converted.EndInit();
            int stride = (int)converted.PixelWidth * (converted.Format.BitsPerPixel / 8);
            byte[] b = new byte[converted.PixelHeight * stride];
            converted.CopyPixels(b, stride, 0);
            this.resized = b;


        }
        public Action UpdateAction { get; set; }

        public LayerVM(string path, Action update)
        {
            UpdateAction = update;
            Path = path;
            OverlayMethod = Method.MethodList[0];
            r = true;
            g = true;
            b = true;
            resize = false;
            offsetx = 0; offsety = 0;
            multiplier = 1;
            alpha = 255;

            BitmapImage img = new BitmapImage(new Uri(path));
            Bitmap = img;
            /*
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(ms);

                    Data = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            */


        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                UpdateAction();
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}

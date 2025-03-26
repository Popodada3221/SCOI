using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SCOI.WPF.ViewModels
{
    public class HistogramVM : INotifyPropertyChanged
    {
        private byte[] data;
        private BitmapSource bitmap;
        public BitmapSource Bitmap { get => bitmap; set { bitmap = value; OnPropertyChanged(); } }
        public byte[] Data { get => data; set { data = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prpos = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prpos));
            }
        }
    }
}

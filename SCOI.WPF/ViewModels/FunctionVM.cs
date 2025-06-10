using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using SCOI.WPF.Utils;

namespace SCOI.WPF.ViewModels
{
    public class FunctionVM : INotifyPropertyChanged
    {
        private byte[] data;
        private BitmapSource source;
        private ObservableCollection<Vec2D> vectors;
        private List<Vec2D> sortedVectors;
        private byte[] function;
        public byte[] Function { get => function; set { function = value; OnPropertyChanged(nameof(Function)); } }
        public ObservableCollection<Vec2D> Vectors { get => vectors; set { vectors = value; OnPropertyChanged(nameof(Vectors)); } }
        public byte[] Data { get => data; set { data = value; OnPropertyChanged(nameof(Data)); } }
        public BitmapSource Source { get => source; set { source = value; OnPropertyChanged(nameof(Source)); } }
        public event PropertyChangedEventHandler PropertyChanged;

        public Action UpdateAction { get; set; }
        public void OnPropertyChanged([CallerMemberName] string prpos = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prpos));
            }
        }
        public FunctionVM(Action action)
        {
            UpdateAction = action;
            data = new byte[256 * 256 * 4];
            for (int i = 0; i < 256 * 256; i++)
            {
                data[i * 4] = 200;
                data[i * 4 + 1] = 200;
                data[i * 4 + 2] = 200;
                data[i * 4 + 3] = 255;
            }
            Source = BitmapSource.Create(256, 256, 95, 95, System.Windows.Media.PixelFormats.Bgra32, null, data, 256 * 4);
            vectors = new ObservableCollection<Vec2D>();
            vectors.Add(new Vec2D(0, 0));
            vectors.Add(new Vec2D(255, 255));
            function = new byte[256];
            Update();
        }
        public void AddPoint(double x, double y)
        {
            var vec = new Vec2D(x, y);

            var v = vectors.FirstOrDefault(x => (x - vec).GetLength() < 16);

            if (v != null && !(v.X == 0 && v.Y == 0) && !(v.X == 255 && v.Y == 255))
            {
                vectors[vectors.IndexOf(v)] = vec;
            }
            else
            {
                vectors.Add(vec);
            }

            Update();
        }
        public void DeletePoint(double x, double y)
        {
            var vec = new Vec2D(x, y);
            var v = vectors.FirstOrDefault(x => (x - vec).GetLength() < 16);
            if (v != null && v.X != 0 && v.X != 255)
            {
                vectors.Remove(v);
            }

            Update();

        }
        public void ChangePoint(double x, double y)
        {
            var vec = new Vec2D(x, y);

            var v = vectors.FirstOrDefault(x => (x - vec).GetLength() < 16);
            if (v != null && v.X != 0 && v.X != 255)
            {
                vectors[vectors.IndexOf(v)] = vec;
            }
            Update();
        }
        public void Update()
        {
            double[] xs1 = new double[vectors.Count];
            double[] ys1 = new double[vectors.Count];
            Parallel.For(0, 255 * 255, (i, state) =>
            {
                data[i * 4] = 200;
                data[i * 4 + 1] = 200;
                data[i * 4 + 2] = 200;
                data[i * 4 + 3] = 255;
            });

            double[] xs2 = new double[256];
            double[] ys2 = new double[256];
            sortedVectors = vectors.OrderBy(x => x.X).ToList();
            int k = 0;
            foreach (var vec in sortedVectors)
            {
                xs1[k] = vec.X;
                ys1[k] = vec.Y;
                k++;
            }
            (xs2, ys2) = Cubic.InterpolateXY(xs1, ys1, 256);
            Parallel.For(0, 256, (i, state) =>

            //for (int i = 255; i >= 0; i--)
            {
                function[Method.CapByte(xs2[i])] = Method.CapByte(ys2[i]);
                data[4 * (Method.CapByte(xs2[i]) + 256 * Method.CapByte(ys2[i])) + 0] = 0;
                data[4 * (Method.CapByte(xs2[i]) + 256 * Method.CapByte(ys2[i])) + 1] = 0;
                data[4 * (Method.CapByte(xs2[i]) + 256 * Method.CapByte(ys2[i])) + 2] = 0;
            });
            foreach (var vec in vectors)
            {



                for (int i = -5; i <= 5; i++)
                {
                    for (int j = -5; j <= 5; j++)
                    {
                        if (((int)vec.X + 256 * ((int)vec.Y + i) + j) * 4 + 2 > 0 && ((int)vec.X + 256 * ((int)vec.Y + i) + j) * 4 + 2 < 256 * 256 * 4)
                        {

                            data[((int)vec.X + 256 * ((int)vec.Y + i) + j) * 4 + 2] = 255;
                            data[((int)vec.X + 256 * ((int)vec.Y + i) + j) * 4 + 1] = 0;
                            data[((int)vec.X + 256 * ((int)vec.Y + i) + j) * 4 + 0] = 0;
                        }
                    }

                }
            }

            Source = BitmapSource.Create(256, 256, 95, 95, System.Windows.Media.PixelFormats.Bgra32, null, data, 256 * 4);
        }
        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            //var position = e.GetPosition((System.Windows.IInputElement)sender);
        }
    }
}


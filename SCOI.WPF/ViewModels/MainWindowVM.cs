using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SCOI.WPF.Commands;

namespace SCOI.WPF.ViewModels
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private byte[] output;
        private BitmapSource outputBitmap;
        private ObservableCollection<LayerVM> layers;
        private HistogramVM histogram;
        private FunctionVM function;
        public FunctionVM Function { get => function; set { function = value; OnPropertyChanged(nameof(Function)); } }
        public HistogramVM Histogram { get => histogram; set { histogram = value; OnPropertyChanged(nameof(Histogram)); } }
        public byte[] OutputBytes { get => output; set => output = value; }
        public BitmapSource OutputBitmap { get => outputBitmap; set { outputBitmap = value; OnPropertyChanged(nameof(OutputBitmap)); } }

        public ObservableCollection<LayerVM> Layers { get => layers; set { layers = value; OnPropertyChanged(nameof(Layers)); } }
        public LayerVM CurrentLayer { get; set; }

        public BitmapSource CurrentImage { get; set; }

        public List<Method> Methods { get; set; }
        private RelayCommand addCommand;
        private RelayCommand upCommand;
        private RelayCommand downCommand;
        private RelayCommand saveCommand;
        private RelayCommand deleteCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand((obj =>
                {

                    using (var filestream = new FileStream("saved.png", FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(OutputBitmap));
                        encoder.Save(filestream);
                    }
                })));
            }
        }


        public MainWindowVM()
        {
            Layers = new ObservableCollection<LayerVM>();
            Methods = Method.MethodList;
            Histogram = new HistogramVM();
            Function = new FunctionVM(Update);


        }
        public RelayCommand UpCommand
        {
            get
            {
                return upCommand ?? (upCommand = new RelayCommand((obj =>
                {
                    var param = obj as LayerVM;
                    if (param != null)
                    {
                        if (Layers[Layers.IndexOf(param)] != null)
                        {
                            if (Layers.IndexOf(param) > 0)
                            {
                                Layers.Move(Layers.IndexOf(param), Layers.IndexOf(param) - 1);
                            }
                        }
                    }
                    Update();
                })));

            }
        }
        public RelayCommand DownCommand
        {
            get
            {
                return downCommand ?? (downCommand = new RelayCommand((obj =>
                {
                    var param = obj as LayerVM;
                    if (param != null)
                    {
                        if (Layers[Layers.IndexOf(param)] != null)
                        {
                            if (Layers.IndexOf(param) < Layers.Count - 1)
                            {
                                Layers.Move(Layers.IndexOf(param), Layers.IndexOf(param) + 1);
                            }
                        }
                    }
                    Update();
                })));

            }
        }
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(obj =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "Файлы рисунков (*.png)|*.png;";
                    bool? success = ofd.ShowDialog();
                    if (success == true)
                    {
                        string path = ofd.FileName;
                        Layers.Add(new LayerVM(path, Update)
                        {
                            UpCommand = new RelayCommand(obj =>
                            {
                                var param = obj as LayerVM;
                                if (Layers[Layers.IndexOf(param)] != null)
                                {
                                    if (Layers.IndexOf(param) > 0)
                                    {
                                        Layers.Move(Layers.IndexOf(param), Layers.IndexOf(param) - 1);

                                    }
                                }
                            })
                        });
                        Update();
                    }
                }));
            }
        }
        public RelayCommand DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new RelayCommand(obj =>
                {
                    var param = obj as LayerVM;
                    Layers.Remove(param);
                    Update();
                    OnPropertyChanged(nameof(Layers));
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public void UpdateHisto()
        {
            byte[] histo = new byte[256 * 2 * 256 * 4];
            int[] func = new int[256 * 4];
            if (output != null && output.Length > 0)
            {
                for (int i = 0; i < output.Length / 4; i++)
                {
                    func[4 * output[i * 4]]++;
                    func[4 * output[i * 4] + 3] = 255;

                    func[4 * output[i * 4 + 1] + 1]++;
                    func[4 * output[i * 4 + 1] + 3] = 255;

                    func[4 * output[i * 4 + 2] + 2]++;
                    func[4 * output[i * 4 + 2] + 3] = 255;

                }
                int max = func.Max();
                for (int i = 0; i < func.Length / 4; i++)
                {
                    func[i * 4] = func[i * 4] * 255 / max;
                    func[i * 4 + 1] = func[i * 4 + 1] * 255 / max;
                    func[i * 4 + 2] = func[i * 4 + 2] * 255 / max;
                    func[i * 4 + 3] = 255;
                }

                for (int x = 0; x < 256 * 4; x++)
                {
                    for (int y = 0; y <= func[x]; y++)
                    {
                        histo[(x + (y * 256 * 4))] = 255;
                    }
                    for (int y = func[x] + 1; y < 256; y++)
                    {
                        histo[x + y * 256 * 4] = 0;
                    }
                }
            }
            Histogram.Data = histo;
            Histogram.Bitmap = BitmapSource.Create(256, 256, 95, 95, System.Windows.Media.PixelFormats.Bgra32, null, Histogram.Data, 256 * 8);

        }

        private void Update()
        {
            if (layers != null && layers.Count > 0)
            {
                int max_width = layers.Max(layer => layer.Width + layer.OffsetX);
                int max_height = layers.Max(layer => layer.Height + layer.OffsetY);
                double min_dpix = layers.Min(layer => layer.DpiX);
                double min_dpiy = layers.Min(layer => layer.DpiY);
                output = new byte[max_width * max_height * 4];
                foreach (var layer in layers)
                {
                    if (layer.Resize == false)
                    {
                        for (int y = 0; y < layer.Height; y++)
                        {
                            for (int x = 0; x < layer.Width; x++)
                            {
                                int currentOutputPixel = 4 * (max_width * (y + layer.OffsetY) + x + layer.OffsetX);
                                int currentInputPixel = 4 * (layer.Width * y + x);
                                output[currentOutputPixel] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.Data[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B));
                                output[currentOutputPixel + 1] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.Data[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G));
                                output[currentOutputPixel + 2] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.Data[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R));
                                output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);
                                //output[currentOutputPixel] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.Data[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B))];
                                //output[currentOutputPixel + 1] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.Data[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G))];
                                //output[currentOutputPixel + 2] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.Data[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R))];
                                //output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);

                            }
                        }
                    }
                    else
                    {
                        layer.CalculateResizedData(max_width, max_height);
                        for (int y = 0; y < max_height; y++)
                        {
                            for (int x = 0; x < max_width; x++)
                            {
                                int currentOutputPixel = 4 * (max_width * y + x);
                                int currentInputPixel = 4 * (max_width * y + x);
                                output[currentOutputPixel] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.ResizedData[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B));
                                output[currentOutputPixel + 1] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.ResizedData[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G));
                                output[currentOutputPixel + 2] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.ResizedData[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R));
                                output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);

                                //output[currentOutputPixel] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.ResizedData[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B))];
                                //output[currentOutputPixel + 1] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.ResizedData[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G))];
                                //output[currentOutputPixel + 2] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.ResizedData[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R))];
                                //output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);
                            }
                        }
                    }


                }
                if (function.Vectors.Count > 2)
                    for (int i = 0; i < output.Length / 4; i++)
                    {
                        output[i * 4] = function.Function[output[i * 4]];
                        output[i * 4 + 1] = function.Function[output[i * 4 + 1]];
                        output[i * 4 + 2] = function.Function[output[i * 4 + 2]];
                    }
                using (MemoryStream ms = new MemoryStream(output))
                {
                    OutputBitmap = BitmapSource.Create(max_width, max_height, min_dpix, min_dpiy, System.Windows.Media.PixelFormats.Bgra32, null, output, max_width * 4);
                }
            }
            UpdateHisto();
        }
    }
}

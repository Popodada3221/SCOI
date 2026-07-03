using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
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
        private IBinarizationMethod binarization;
        private bool bin;
        private int time;
        private bool filter;
        private uint filterx, filtery;
        private uint oldfilterx, oldfiltery;
        private ObservableCollection<DoFilter> filterdata;
        private double gaussSigma;
        private bool linearFilter;
        private bool spaceFilter;
        private bool isFourier;
        private byte[] fourierBytes;
        private BitmapSource fourierBitmap;
        private int width;
        private int height;
        private Complex fourierData;
        private ObservableCollection<FourierFilterVM> fourierFilters;
        public ObservableCollection<FourierFilterVM> FourierFilters { get => fourierFilters; set { fourierFilters = value; OnPropertyChanged(nameof(FourierFilters)); } }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public BitmapSource FourierBitmap { get => fourierBitmap; set { fourierBitmap = value; OnPropertyChanged(nameof(FourierBitmap)); } }
        public byte[] FourierBytes { get => fourierBytes; set { fourierBytes = value; OnPropertyChanged(nameof(FourierBytes)); } }
        public bool IsFourier { get => isFourier; set { isFourier = value; OnPropertyChanged(nameof(IsFourier)); Update(); } }
        public bool SpaceFilter { get => spaceFilter; set { spaceFilter = value; OnPropertyChanged(nameof(spaceFilter)); } }
        public bool LinearFilter { get => linearFilter; set { linearFilter = value; OnPropertyChanged(nameof(linearFilter)); } }
        public double GaussSigma { get => gaussSigma; set { gaussSigma = value; OnPropertyChanged(nameof(gaussSigma)); } }
        public ObservableCollection<DoFilter> FilterData { get => filterdata; set { filterdata = value; OnPropertyChanged(nameof(FilterData)); } }
        public uint FilterX { get => filterx; set { filterx = value; OnPropertyChanged(nameof(FilterX)); } }
        public uint FilterY { get => filtery; set { filtery = value; OnPropertyChanged(nameof(FilterY)); } }
        public bool Filter { get => filter; set { filter = value; OnPropertyChanged(nameof(Filter)); } }
        public int Time { get => time; set { time = value; OnPropertyChanged(nameof(Time)); } }
        public bool Bin { get => bin; set { bin = value; OnPropertyChanged(nameof(Bin)); Update(); } }
        public IBinarizationMethod Binarization { get => binarization; set { binarization = value; OnPropertyChanged(nameof(Binarization)); Update(); } }
        public FunctionVM Function { get => function; set { function = value; OnPropertyChanged(nameof(Function)); } }
        public HistogramVM Histogram { get => histogram; set { histogram = value; OnPropertyChanged(nameof(Histogram)); } }
        public byte[] OutputBytes { get => output; set => output = value; }
        public BitmapSource OutputBitmap { get => outputBitmap; set { outputBitmap = value; OnPropertyChanged(nameof(OutputBitmap)); } }

        public ObservableCollection<LayerVM> Layers { get => layers; set { layers = value; OnPropertyChanged(nameof(Layers)); } }
        public LayerVM CurrentLayer { get; set; }

        public BitmapSource CurrentImage { get; set; }

        public List<Method> Methods { get; set; }
        public List<IBinarizationMethod> BinarizationMethods { get; set; }
        public static MainWindowVM Instance { get; private set; }
        private RelayCommand addCommand;
        private RelayCommand upCommand;
        private RelayCommand downCommand;
        private RelayCommand saveCommand;
        private RelayCommand deleteCommand;
        private RelayCommand addFilterCommand;
        private RelayCommand removeFilterCommand;
        private RelayCommand updateFourierCommand;
        public RelayCommand UpdateFourierCommand
        {
            get
            {
                return updateFourierCommand ?? (updateFourierCommand = new RelayCommand(obj =>
                {
                    Update();

                    if (FourierFilters != null && FourierFilters.Count != 0)
                    {




                        var filterdata = new byte[width * height * 4];
                        Parallel.For(0, filterdata.Length, (i, state) => filterdata[i] = 255);
                        foreach (var item in FourierFilters)
                        {
                            if (item != null && item.Filter != null)
                                Parallel.For(0, width * height * 4, (i, state) =>
                                {
                                    filterdata[i] = Math.Min(filterdata[i], item.Data[i]);
                                });

                        }

                        var fourierDataBlue = Fourier.GetFourierData(OutputBytes, width, height, 0);
                        var fourierDataGreen = Fourier.GetFourierData(OutputBytes, width, height, 1);
                        var fourierDataRed = Fourier.GetFourierData(OutputBytes, width, height, 2);

                        Parallel.For(0, width * height, (i, state) =>
                            {
                                int x = i % width;
                                int y = i / width;
                                fourierDataBlue[y, x] = fourierDataBlue[y, x] * ((double)filterdata[i * 4] / 255);
                                fourierDataGreen[y, x] = fourierDataGreen[y, x] * ((double)filterdata[i * 4 + 1] / 255);
                                fourierDataRed[y, x] = fourierDataRed[y, x] * ((double)filterdata[i * 4 + 2] / 255);
                            });

                        byte[] blue = Fourier.ReconstructFromFourier(fourierDataBlue, width, height);
                        byte[] green = Fourier.ReconstructFromFourier(fourierDataGreen, width, height);
                        byte[] red = Fourier.ReconstructFromFourier(fourierDataRed, width, height);
                        Parallel.For(0, width * height, (i, state) =>
                        {
                            OutputBytes[i * 4 + 0] = blue[i * 4];
                            OutputBytes[i * 4 + 1] = green[i * 4];
                            OutputBytes[i * 4 + 2] = red[i * 4];
                            OutputBytes[i * 4 + 3] = 255;
                        });
                        using (MemoryStream ms = new MemoryStream(OutputBytes))
                        {
                            OutputBitmap = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, OutputBytes, width * 4);
                        }
                    }
                }));
            }
        }
        public RelayCommand RemoveFilterCommand
        {
            get
            {
                return removeFilterCommand ?? (removeFilterCommand = new RelayCommand(obj =>
                {
                    var o = obj as FourierFilterVM;
                    FourierFilters.Remove(o);
                    OnPropertyChanged(nameof(FourierFilters));
                }));
            }
        }
        public RelayCommand AddFilterCommand
        {
            get
            {
                return addFilterCommand ?? (addFilterCommand = new RelayCommand(obj =>
                {
                    if (FourierFilters == null) FourierFilters = new ObservableCollection<FourierFilterVM>();
                    FourierFilters.Add(new FourierFilterVM()

                        );



                }));
            }
        }
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

            Instance = this;
            Layers = new ObservableCollection<LayerVM>();
            Methods = Method.MethodList;
            BinarizationMethods = BinarizationVM.BinarizationList;
            Binarization = BinarizationMethods[0];
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
        private RelayCommand manualUpdateCommand;
        public RelayCommand ManualUpdateCommand
        {
            get
            {
                return manualUpdateCommand ?? (manualUpdateCommand = new RelayCommand((obj =>
                {
                    DoFilter.Instance = FilterData;
                    Update();
                })));
            }
        }
        private RelayCommand calculateGaussCommand;
        public RelayCommand CalculateGaussCommand
        {

            get
            {
                return calculateGaussCommand ?? (calculateGaussCommand = new RelayCommand((obj) =>
                {
                    DoFilter.CalculateGauss(gaussSigma);
                    OnPropertyChanged(nameof(FilterData));
                }));
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
            if (filterx > 0 && filtery > 0 && (oldfilterx != FilterX || oldfiltery != FilterY))
            {
                filterdata = new ObservableCollection<DoFilter>();
                for (int i = 0; i < filterx; i++)
                {
                    filterdata.Add(new DoFilter(filtery));
                }
                oldfilterx = FilterX;
                oldfiltery = FilterY;
                DoFilter.Instance = FilterData;
                OnPropertyChanged(nameof(FilterData));
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }

        }

        public void UpdateHisto()
        {
            byte[] histo = new byte[256 * 2 * 256 * 4];
            int[] func = new int[256 * 4];
            if (output != null && output.Length > 0)
            {

                Parallel.For(0, output.Length / 4, (i, state) =>
                    {
                        func[4 * output[i * 4]]++;
                        func[4 * output[i * 4] + 3] = 255;

                        func[4 * output[i * 4 + 1] + 1]++;
                        func[4 * output[i * 4 + 1] + 3] = 255;

                        func[4 * output[i * 4 + 2] + 2]++;
                        func[4 * output[i * 4 + 2] + 3] = 255;

                    });
                int max = func.Max();

                Parallel.For(0, func.Length / 4, (i, state) =>
                {
                    func[i * 4] = func[i * 4] * 255 / max;
                    func[i * 4 + 1] = func[i * 4 + 1] * 255 / max;
                    func[i * 4 + 2] = func[i * 4 + 2] * 255 / max;
                    func[i * 4 + 3] = 255;
                });

                //for (int x = 0; x < 256 * 4; x++)
                //{
                //    for (int y = 0; y <= func[x]; y++)
                //    {
                //        histo[(x + (y * 256 * 4))] = 255;
                //    }
                //    for (int y = func[x] + 1; y < 256; y++)
                //    {
                //        histo[x + y * 256 * 4] = 0;
                //    }
                //}
                Parallel.For(0, 256 * 256 * 4, (i, state) =>
                {
                    if (i / (256 * 4) <= func[i % (256 * 4)])
                    {
                        histo[i] = 255;
                    }
                    else histo[i] = 0;
                });
            }
            Histogram.Data = histo;
            Histogram.Bitmap = BitmapSource.Create(256, 256, 95, 95, System.Windows.Media.PixelFormats.Bgra32, null, Histogram.Data, 256 * 8);
        }

        public void Update()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (layers != null && layers.Count > 0)
            {
                int max_width = layers.Max(layer => layer.Width + layer.OffsetX);
                int max_height = layers.Max(layer => layer.Height + layer.OffsetY);
                if (isFourier)
                {
                    int k = 2;
                    while (k < max_height || k < max_width)
                    {
                        k *= 2;
                    }
                    max_height = k;
                    max_width = k;
                }
                width = max_width;
                height = max_height;
                double min_dpix = layers.Min(layer => layer.DpiX);
                double min_dpiy = layers.Min(layer => layer.DpiY);
                output = new byte[max_width * max_height * 4];
                foreach (var layer in layers)
                {
                    if (layer.Resize == false)
                    {
                        Parallel.For(0, layer.Height * layer.Width, (i, state) =>
                        {
                            int currentInputPixel = 4 * i;
                            int currentOutputPixel = 4 * ((i / layer.Width + layer.OffsetY) * max_width + i % layer.Width + layer.OffsetX);
                            output[currentOutputPixel] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.Data[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B));
                            output[currentOutputPixel + 1] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.Data[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G));
                            output[currentOutputPixel + 2] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.Data[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R));
                            output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);
                        });
                        //for (int y = 0; y < layer.Height; y++)
                        //{
                        //    for (int x = 0; x < layer.Width; x++)
                        //    {
                        //        int currentOutputPixel = 4 * (max_width * (y + layer.OffsetY) + x + layer.OffsetX);
                        //        int currentInputPixel = 4 * (layer.Width * y + x);
                        //        output[currentOutputPixel] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.Data[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B));
                        //        output[currentOutputPixel + 1] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.Data[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G));
                        //        output[currentOutputPixel + 2] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.Data[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R));
                        //        output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);
                        //        //output[currentOutputPixel] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.Data[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B))];
                        //        //output[currentOutputPixel + 1] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.Data[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G))];
                        //        //output[currentOutputPixel + 2] = function.Function[(byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.Data[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R))];
                        //        //output[currentOutputPixel + 3] = Math.Max(output[currentOutputPixel + 3], layer.Alpha);

                        //    }
                        //}
                    }
                    else
                    {
                        layer.CalculateResizedData(max_width, max_height);
                        Parallel.For(0, max_height * max_width, (i, state) =>
                        {
                            int currentInputPixel = 4 * i;
                            int currentOutputPixel = 4 * i;
                            output[currentOutputPixel] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel], layer.ResizedData[currentInputPixel], layer.Multiplier) * System.Convert.ToByte(layer.B));
                            output[currentOutputPixel + 1] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 1], layer.ResizedData[currentInputPixel + 1], layer.Multiplier) * System.Convert.ToByte(layer.G));
                            output[currentOutputPixel + 2] = (byte)(layer.OverlayMethod.Operation(output[currentOutputPixel + 2], layer.ResizedData[currentInputPixel + 2], layer.Multiplier) * System.Convert.ToByte(layer.R));
                            output[currentOutputPixel + 3] = 255;
                        });

                    }


                }

                if (function.Vectors.Count > 2)
                    //for (int i = 0; i < output.Length / 4; i++)
                    Parallel.For(0, output.Length / 4, (i, state) =>
                    {
                        output[i * 4] = function.Function[output[i * 4]];
                        output[i * 4 + 1] = function.Function[output[i * 4 + 1]];
                        output[i * 4 + 2] = function.Function[output[i * 4 + 2]];
                    });
                if (Bin)
                {
                    OutputBytes = Binarization.Calculate(output);
                }
                if (Filter)
                {
                    if (linearFilter)
                    {
                        OutputBytes = DoFilter.CalculateLinear(output, max_width);
                    }
                    else if (spaceFilter)
                    {
                        OutputBytes = DoFilter.CalculateMedian(output, max_width);
                    }
                }

                using (MemoryStream ms = new MemoryStream(output))
                {
                    OutputBitmap = BitmapSource.Create(max_width, max_height, min_dpix, min_dpiy, System.Windows.Media.PixelFormats.Bgra32, null, output, max_width * 4);
                }
                UpdateHisto();
                if (isFourier)
                {

                    FourierBytes = Fourier.ApplyFourierToBgra32(output, max_width, max_height);



                    using (MemoryStream ms = new MemoryStream(fourierBytes))
                    {
                        FourierBitmap = BitmapSource.Create(max_width, max_height, min_dpix, min_dpiy, System.Windows.Media.PixelFormats.Bgra32, null, fourierBytes, max_width * 4);
                    }

                }
            }
            sw.Stop();
            Time = (int)sw.ElapsedMilliseconds;
        }
    }
}

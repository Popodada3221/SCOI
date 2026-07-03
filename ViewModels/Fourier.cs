using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SCOI.WPF.ViewModels
{
    public delegate double FilterDelegate(double insideVal, double outsideVal, int centerX, int centerY, int posx, int posy, double sigma, int x, int y, int radius, int radius2 = 0);
    /*
    public class Fourier
    {
        // Применяет FFT к каждому каналу и возвращает byte[] (BGRA32)
        public static byte[] ApplyFourierToBgra32(byte[] bgraData, int width, int height)
        {
            int pixelCount = width * height;
            byte[] output = new byte[bgraData.Length];

            // Обрабатываем каждый канал (B, G, R, A)
            Parallel.For(0, 3, (channel, state) =>



            //for (int channel = 0; channel < 3; channel++)
            {
                // Извлекаем канал в double[,]
                double[,] channelData = new double[height, width];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4 + channel;
                        channelData[y, x] = bgraData[index];
                    }
                }

                // Применяем FFT
                Complex[,] fft = FFT2D(channelData);
                Complex[,] fftShifted = ShiftFrequencies(fft);

                // Получаем амплитуду и нормализуем
                double[,] magnitude = CalculateMagnitude(fftShifted);
                Normalize(magnitude, 0, 255);

                // Записываем обратно в byte[]
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4 + channel;
                        output[index] = (byte)magnitude[y, x];
                    }
                }
            });
            //Parallel.For(0, output.Length / 4, (i, state) =>
            //{
            //    output[i * 4 + 3] = 255;
            //});
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4 + 3;
                    output[index] = 255;
                }
            }

            return output;
        }

        // 2D FFT (аналогично предыдущему примеру)
        public static Complex[,] FFT2D(double[,] input)
        {
            int height = input.GetLength(0);
            int width = input.GetLength(1);
            Complex[,] output = new Complex[height, width];

            // FFT по строкам
            for (int y = 0; y < height; y++)
            {
                Complex[] row = new Complex[width];
                for (int x = 0; x < width; x++)
                    row[x] = new Complex(input[y, x], 0);

                Complex[] rowTransformed = FFT(row);
                for (int x = 0; x < width; x++)
                    output[y, x] = rowTransformed[x];
            }

            // FFT по столбцам
            for (int x = 0; x < width; x++)
            {
                Complex[] column = new Complex[height];
                for (int y = 0; y < height; y++)
                    column[y] = output[y, x];

                Complex[] columnTransformed = FFT(column);
                for (int y = 0; y < height; y++)
                    output[y, x] = columnTransformed[y];
            }

            return output;
        }

        // 1D FFT (рекурсивный алгоритм Кули-Тьюки)
        private static Complex[] FFT(Complex[] input)
        {
            int n = input.Length;
            if (n == 1)
                return new Complex[] { input[0] };

            Complex[] even = new Complex[n / 2];
            Complex[] odd = new Complex[n / 2];
            for (int i = 0; i < n / 2; i++)
            {
                even[i] = input[2 * i];
                odd[i] = input[2 * i + 1];
            }

            Complex[] evenTransformed = FFT(even);
            Complex[] oddTransformed = FFT(odd);

            Complex[] output = new Complex[n];
            for (int k = 0; k < n / 2; k++)
            {
                double angle = -2 * Math.PI * k / n;
                Complex exp = Complex.FromPolarCoordinates(1, angle) * oddTransformed[k];
                output[k] = evenTransformed[k] + exp;
                output[k + n / 2] = evenTransformed[k] - exp;
            }

            return output;
        }

        // Сдвиг частот (центрирование)
        private static Complex[,] ShiftFrequencies(Complex[,] fft)
        {
            int height = fft.GetLength(0);
            int width = fft.GetLength(1);
            Complex[,] shifted = new Complex[height, width];

            int halfH = height / 2;
            int halfW = width / 2;

            for (int y = 0; y < halfH; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    shifted[y + halfH, x + halfW] = fft[y, x];
                    shifted[y, x] = fft[y + halfH, x + halfW];
                    shifted[y, x + halfW] = fft[y + halfH, x];
                    shifted[y + halfH, x] = fft[y, x + halfW];
                }
            }

            return shifted;
        }

        // Вычисление амплитуды (логарифмированное)
        private static double[,] CalculateMagnitude(Complex[,] fft)
        {
            int height = fft.GetLength(0);
            int width = fft.GetLength(1);
            double[,] magnitude = new double[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    magnitude[y, x] = Math.Log(1 + Complex.Abs(fft[y, x]));
            //magnitude[y, x] = Complex.Abs(fft[y, x]);

            return magnitude;
        }

        // Нормализация в [min, max]
        private static void Normalize(double[,] data, double min, double max)
        {
            double currentMin = double.MaxValue;
            double currentMax = double.MinValue;

            // Находим текущий диапазон
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (data[y, x] < currentMin) currentMin = data[y, x];
                    if (data[y, x] > currentMax) currentMax = data[y, x];
                }
            }

            // Масштабируем
            double range = currentMax - currentMin;
            double targetRange = max - min;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    data[y, x] = min + (data[y, x] - currentMin) * targetRange / range;
                }
            }
        }

    }
*/
    public class Fourier
    {
        // Применяет FFT к каждому каналу и возвращает byte[] (BGRA32)
        public static byte[] ApplyFourierToBgra32(byte[] bgraData, int width, int height)
        {
            int pixelCount = width * height;
            byte[] output = new byte[bgraData.Length];

            // Обрабатываем каждый канал (B, G, R)
            Parallel.For(0, 3, (channel) =>
            {
                // Извлекаем канал в double[,]
                double[,] channelData = new double[height, width];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4 + channel;
                        channelData[y, x] = bgraData[index];
                    }
                }

                // Применяем FFT
                Complex[,] fft = FFT2D(channelData);
                Complex[,] fftShifted = ShiftFrequencies(fft);

                // Получаем амплитуду и нормализуем
                double[,] magnitude = CalculateMagnitude(fftShifted);
                Normalize(magnitude, 0, 255);

                // Записываем обратно в byte[]
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4 + channel;
                        output[index] = (byte)magnitude[y, x];
                    }
                }
            });

            // Устанавливаем альфа-канал в 255
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4 + 3;
                    output[index] = 255;
                }
            }

            return output;
        }

        // Восстанавливает изображение из Фурье-образа (Complex[,])
        public static byte[] ReconstructFromFourier(Complex[,] fourierData, int width, int height)
        {
            byte[] output = new byte[width * height * 4];

            // Обратный сдвиг частот
            Complex[,] shiftedBack = InverseShiftFrequencies(fourierData);

            // Применяем обратное БПФ (IFFT)
            Complex[,] ifftResult = IFFT2D(shiftedBack);

            // Нормализуем и преобразуем в byte[]
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Берем действительную часть (мнимая должна быть ~0)
                    double realValue = ifftResult[y, x].Real;

                    // Обрезаем значения до [0, 255]
                    realValue = Math.Max(0, Math.Min(255, realValue));

                    // Записываем в B, G, R каналы (одинаковые значения для grayscale)
                    int index = (y * width + x) * 4;
                    output[index] = (byte)realValue;       // B
                    output[index + 1] = (byte)realValue;   // G
                    output[index + 2] = (byte)realValue;   // R
                    output[index + 3] = 255;               // A
                }
            }

            return output;
        }

        // 2D FFT
        public static Complex[,] FFT2D(double[,] input)
        {
            int height = input.GetLength(0);
            int width = input.GetLength(1);
            Complex[,] output = new Complex[height, width];

            // FFT по строкам
            for (int y = 0; y < height; y++)
            {
                Complex[] row = new Complex[width];
                for (int x = 0; x < width; x++)
                    row[x] = new Complex(input[y, x], 0);

                Complex[] rowTransformed = FFT(row);
                for (int x = 0; x < width; x++)
                    output[y, x] = rowTransformed[x];
            }

            // FFT по столбцам
            for (int x = 0; x < width; x++)
            {
                Complex[] column = new Complex[height];
                for (int y = 0; y < height; y++)
                    column[y] = output[y, x];

                Complex[] columnTransformed = FFT(column);
                for (int y = 0; y < height; y++)
                    output[y, x] = columnTransformed[y];
            }

            return output;
        }

        // 2D IFFT (обратное преобразование)
        public static Complex[,] IFFT2D(Complex[,] input)
        {
            int height = input.GetLength(0);
            int width = input.GetLength(1);
            Complex[,] output = new Complex[height, width];

            // IFFT по строкам
            for (int y = 0; y < height; y++)
            {
                Complex[] row = new Complex[width];
                for (int x = 0; x < width; x++)
                    row[x] = input[y, x];

                Complex[] rowTransformed = IFFT(row);
                for (int x = 0; x < width; x++)
                    output[y, x] = rowTransformed[x];
            }

            // IFFT по столбцам
            for (int x = 0; x < width; x++)
            {
                Complex[] column = new Complex[height];
                for (int y = 0; y < height; y++)
                    column[y] = output[y, x];

                Complex[] columnTransformed = IFFT(column);
                for (int y = 0; y < height; y++)
                    output[y, x] = columnTransformed[y];
            }

            return output;
        }

        // 1D FFT (Кули-Тьюки)
        private static Complex[] FFT(Complex[] input)
        {
            int n = input.Length;
            if (n == 1)
                return new Complex[] { input[0] };

            Complex[] even = new Complex[n / 2];
            Complex[] odd = new Complex[n / 2];
            for (int i = 0; i < n / 2; i++)
            {
                even[i] = input[2 * i];
                odd[i] = input[2 * i + 1];
            }

            Complex[] evenTransformed = FFT(even);
            Complex[] oddTransformed = FFT(odd);

            Complex[] output = new Complex[n];
            for (int k = 0; k < n / 2; k++)
            {
                double angle = -2 * Math.PI * k / n;
                Complex exp = Complex.FromPolarCoordinates(1, angle) * oddTransformed[k];
                output[k] = evenTransformed[k] + exp;
                output[k + n / 2] = evenTransformed[k] - exp;
            }

            return output;
        }

        // 1D IFFT (обратное преобразование)
        private static Complex[] IFFT(Complex[] input)
        {
            int n = input.Length;
            Complex[] output = new Complex[n];

            // Комплексное сопряжение входных данных
            for (int i = 0; i < n; i++)
                input[i] = Complex.Conjugate(input[i]);

            // Применяем обычный FFT
            output = FFT(input);

            // Комплексное сопряжение и деление на N
            for (int i = 0; i < n; i++)
                output[i] = Complex.Conjugate(output[i]) / n;

            return output;
        }

        // Сдвиг частот (центрирование)
        private static Complex[,] ShiftFrequencies(Complex[,] fft)
        {
            int height = fft.GetLength(0);
            int width = fft.GetLength(1);
            Complex[,] shifted = new Complex[height, width];

            int halfH = height / 2;
            int halfW = width / 2;

            for (int y = 0; y < halfH; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    shifted[y + halfH, x + halfW] = fft[y, x];
                    shifted[y, x] = fft[y + halfH, x + halfW];
                    shifted[y, x + halfW] = fft[y + halfH, x];
                    shifted[y + halfH, x] = fft[y, x + halfW];
                }
            }

            return shifted;
        }

        // Обратный сдвиг частот (децентрирование)
        private static Complex[,] InverseShiftFrequencies(Complex[,] fftShifted)
        {
            // Тот же код, что и ShiftFrequencies (сдвиг симметричен)
            return ShiftFrequencies(fftShifted);
        }

        // Вычисление амплитуды (логарифмированное)
        private static double[,] CalculateMagnitude(Complex[,] fft)
        {
            int height = fft.GetLength(0);
            int width = fft.GetLength(1);
            double[,] magnitude = new double[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    magnitude[y, x] = Math.Log(1 + Complex.Abs(fft[y, x]));

            return magnitude;
        }

        // Нормализация в [min, max]
        private static void Normalize(double[,] data, double min, double max)
        {
            double currentMin = double.MaxValue;
            double currentMax = double.MinValue;

            // Находим текущий диапазон
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (data[y, x] < currentMin) currentMin = data[y, x];
                    if (data[y, x] > currentMax) currentMax = data[y, x];
                }
            }

            // Масштабируем
            double range = currentMax - currentMin;
            double targetRange = max - min;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    data[y, x] = min + (data[y, x] - currentMin) * targetRange / range;
                }
            }
        }
        public static Complex[,] GetFourierData(byte[] bgraData, int width, int height, int channel)
        {
            // Проверка на корректность канала
            if (channel < 0 || channel > 2)
                throw new ArgumentException("Channel must be 0 (B), 1 (G), or 2 (R).");

            // Извлекаем канал в double[,]
            double[,] channelData = new double[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4 + channel;
                    channelData[y, x] = bgraData[index];
                }
            }

            // Применяем FFT и сдвигаем частоты
            Complex[,] fft = FFT2D(channelData);
            Complex[,] fftShifted = ShiftFrequencies(fft);

            return fftShifted;
        }
    }
    public class FourierFilter
    {
        public string Name { get; set; }
        public FilterDelegate FilterAction { get; set; }

        public static List<FourierFilter> fourierFilters = new List<FourierFilter>()
        {
            new FourierFilter()
            {
                Name = "Cut",
                FilterAction = (insideVal, outsideVal, centerX, centerY,posx ,posy,sigma, x, y, radius, radius2) =>
                {
                    if((x-posx)*(x-posx)+(y-posy)*(y-posy) < radius*radius || (x-(centerX*2-posx))*(x-(centerX*2-posx))+(y-(centerY*2-posy))*(y-(centerY*2-posy))<radius*radius)
                        return insideVal;
                    else return outsideVal;
                }
            },
            new FourierFilter()
            {
                Name="Gauss",
                FilterAction=(insideVal, outsideVal, centerX, centerY, posx, posy,sigma,x,y,radius,radius2)=>
                {
                    var a = Math.Sqrt((x-posx)*(x-posx)+(y-posy)*(y-posy));
                    var b = Math.Sqrt((x-(centerX*2-posx))*(x-(centerX*2-posx))+(y-(centerY*2-posy))*(y-(centerY*2-posy)));
                    var val = gauss(Math.Min(a,b)-radius,sigma);
                    var volume = insideVal - outsideVal;
                    if(a < radius || b<radius)
                        if(a<radius2 || b<radius2)
                        {
                            val = gauss(Math.Min(a,b)-radius2,sigma);
                            volume = insideVal - outsideVal;
                            return val*volume+outsideVal;
                        }
                            else
                        return insideVal;
                    else
                    return val*volume+outsideVal;
                }
            }
        };
        private static double gauss(double x, double radius)
        {
            return Math.Exp(-x * x / (2 * radius * radius));
        }
    }
    public class FourierFilterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
                if (prop != nameof(Source))
                {
                    Update(MainWindowVM.Instance.Width, MainWindowVM.Instance.Height);
                }
            }
        }
        public byte[] Data { get; set; }
        private BitmapSource source;
        private double insideVal, outsideVal, sigma;
        private int centerX, centerY, radius, innerRadius;
        public double Sigma { get => sigma; set { sigma = value; OnPropertyChanged(nameof(Sigma)); } }
        public double InsideVal { get => insideVal; set { insideVal = value; OnPropertyChanged(nameof(InsideVal)); } }
        public double OutsideVal { get => outsideVal; set { outsideVal = value; OnPropertyChanged(nameof(outsideVal)); } }
        public int CenterX { get => centerX; set { centerX = value; OnPropertyChanged(nameof(CenterX)); } }
        public int CenterY { get => centerY; set { centerY = value; OnPropertyChanged(nameof(CenterY)); } }
        public int Radius { get => radius; set { radius = value; OnPropertyChanged(nameof(Radius)); } }
        public int InnerRadius { get => innerRadius; set { innerRadius = value; OnPropertyChanged(nameof(InnerRadius)); } }
        public BitmapSource Source { get => source; set { source = value; OnPropertyChanged(nameof(Source)); } }

        private FourierFilter filter;
        public FourierFilter Filter { get => filter; set { filter = value; OnPropertyChanged(nameof(Filter)); } }
        public List<FourierFilter> FourierFilters { get => FourierFilter.fourierFilters; }
        public void Update(int width, int height)
        {
            if (filter != null && filter.FilterAction != null)
            {

                Data = new byte[width * height * 4];
                Parallel.For(0, width * height, (i, state) =>
                {
                    int x = i % width;
                    int y = i / width;
                    byte pix = Method.CapByte(255 * filter.FilterAction(InsideVal, OutsideVal, MainWindowVM.Instance.Width / 2, MainWindowVM.Instance.Height / 2, CenterX, CenterY, sigma, x, y, Radius, InnerRadius));
                    Data[i * 4] = pix;
                    Data[i * 4 + 1] = pix;
                    Data[i * 4 + 2] = pix;
                    Data[i * 4 + 3] = 255;
                });
                using (MemoryStream ms = new MemoryStream(Data))
                {
                    Source = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, Data, width * 4);
                }
            }
        }
    }
}


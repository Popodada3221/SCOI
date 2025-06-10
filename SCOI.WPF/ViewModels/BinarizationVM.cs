using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Converters;
using Accessibility;

namespace SCOI.WPF.ViewModels
{
    public class BinarizationVM
    {
        public delegate byte BinarizationOp(byte[] data);
        public static List<IBinarizationMethod> BinarizationList { get; set; } = new List<IBinarizationMethod>
        {
            new Gavr(),
            new Otsu(),
            new Niblack(),
            new Sauvola(),
            new Christian(),
            new Bradley(),
        };
    }
    public interface IBinarizationMethod
    {
        public string Name { get; }
        public byte[] Data { get; set; }
        public byte[] Calculate(byte[] data);
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Hidden; }

    }
    public class Gavr : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Hidden; }

        public string Name { get => "Gavr"; }
        public byte[] Data { get; set; }
        public byte[] Calculate(byte[] data)
        {
            Data = new byte[data.Length];
            int sumall = 0;
            for (int i = 0; i < data.Length / 4; i++)
            {
                sumall += (data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2]);
            }
            double median = sumall / (data.Length / 4 * 3);
            Parallel.For(0, data.Length / 4, (i, state) =>
            {
                int sum = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                if (sum / 3 > median)
                {
                    Data[i * 4] = 255;
                    Data[i * 4 + 1] = 255;
                    Data[i * 4 + 2] = 255;
                    Data[i * 4 + 3] = 255;
                }
                else
                {
                    Data[i * 4] = 0;
                    Data[i * 4 + 1] = 0;
                    Data[i * 4 + 2] = 0;
                    Data[i * 4 + 3] = 255;
                }

            });
            return Data;
        }

    }
    public class Otsu : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Hidden; }

        public string Name { get => "Otsu"; }
        public byte[] Data { get; set; }
        public byte[] Calculate(byte[] data)
        {
            Data = new byte[data.Length];
            byte[] histo = new byte[256];
            int min = 255;
            int max = 0;

            int m = 0; // m - сумма высот всех бинов, домноженных на положение их середины

            int n = 0; // n - сумма высот всех бинов

            Parallel.For(0, data.Length / 4, (i, state) =>
            {
                int med = (data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2]) / 3;
                histo[med]++;
                if (med > max) max = med;
                if (med < min) min = med;

            });
            for (int i = 0; i < 256; i++)
            {
                m += i * histo[i];
                n += histo[i];
            }
            float maxSigma = -1;
            int threshold = 0;
            int alpha1 = 0;
            int beta1 = 0;
            for (int t = 0; t < 256; t++)
            {
                alpha1 += t * histo[t];
                beta1 += histo[t];
                float w1 = (float)beta1 / n;
                float a = (float)alpha1 / beta1 - (float)(m - alpha1) / (n - beta1);
                float sigma = w1 * (1 - w1) * a * a;
                if (sigma > maxSigma)
                {
                    maxSigma = sigma;
                    threshold = t;
                }
            }
            Parallel.For(0, data.Length / 4, (i, state) =>
            {
                int sum = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                if (sum / 3 > threshold)
                {
                    Data[i * 4] = 255;
                    Data[i * 4 + 1] = 255;
                    Data[i * 4 + 2] = 255;
                    Data[i * 4 + 3] = 255;
                }
                else
                {
                    Data[i * 4] = 0;
                    Data[i * 4 + 1] = 0;
                    Data[i * 4 + 2] = 0;
                    Data[i * 4 + 3] = 255;
                }
            });

            return Data;
        }
    }
    public class Niblack : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Hidden; }

        public string Name { get => "Niblack"; }
        public byte[] Data { get; set; }
        private int size = 1;
        public int Size { get => size; set { size = value; MainWindowVM.Instance.Update(); } }
        private double sens = -0.2;
        public double Sens { get => sens; set { sens = value; MainWindowVM.Instance.Update(); } }

        private int width;
        public byte[] Calculate(byte[] data)
        {
            width = (int)MainWindowVM.Instance.OutputBitmap.Width;
            Data = new byte[data.Length];
            long[] Integr = Calc.CalcIntegrMatrix(data, width, x => x);
            long[] Integr2 = Calc.CalcIntegrMatrix(data, width, x => x * x);
            //for (int i = 0; i < Data.Length / 4; i++)
            Parallel.For(width * (size) + size, data.Length / 4 - (width * size + size), (i, state) =>
        {
            int mx = 0;
            int mx2 = 0;
            mx = Calc.CalcSum(Integr, size, width, i);
            mx2 = Calc.CalcSum(Integr2, size, width, i);
            mx = mx / ((Size + Size + 1) * (Size + Size + 1));
            mx2 = mx2 / ((Size + Size + 1) * (Size + Size + 1));
            double sigma = Math.Sqrt(mx2 - (mx * mx));
            int threshold = (int)(mx + Sens * sigma);
            int sum = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
            if (sum / 3 > threshold)
            {
                Data[i * 4] = 255;
                Data[i * 4 + 1] = 255;
                Data[i * 4 + 2] = 255;
                Data[i * 4 + 3] = 255;
            }
            else
            {
                Data[i * 4] = 0;
                Data[i * 4 + 1] = 0;
                Data[i * 4 + 2] = 0;
                Data[i * 4 + 3] = 255;
            }

        }
            );

            return Data;
        }
    }
    public class Sauvola : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Hidden; }


        public string Name { get => "Sauvola"; }

        public byte[] Data { get; set; }
        private int size = 1;
        public int Size { get => size; set { size = value; MainWindowVM.Instance.Update(); } }
        private double sens = -0.2;
        public double Sens { get => sens; set { sens = value; MainWindowVM.Instance.Update(); } }
        private int width;
        public byte[] Calculate(byte[] data)
        {
            width = (int)MainWindowVM.Instance.OutputBitmap.Width;

            Data = new byte[data.Length];

            long[] Integr = Calc.CalcIntegrMatrix(data, width, x => x);
            long[] Integr2 = Calc.CalcIntegrMatrix(data, width, x => x * x);


            Parallel.For(width * (size) + size, data.Length / 4 - (width * size + size), (i, state) =>
            //Parallel.For(0, data.Length / 4, (i, state) =>
            //for (int i = 0; i < Data.Length / 4; i++)
            {
                int mx = 0;
                int mx2 = 0;

                mx = Calc.CalcSum(Integr, size, width, i);
                mx2 = Calc.CalcSum(Integr2, size, width, i);
                mx = mx / ((Size + Size + 1) * (Size + Size + 1));
                mx2 = mx2 / ((Size + Size + 1) * (Size + Size + 1));
                double sigma = Math.Sqrt(mx2 - (mx * mx));
                int threshold = (int)(mx * (1 + sens * (sigma / 128 - 1)));
                int sum = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                if (sum / 3 > threshold)
                {
                    Data[i * 4] = 255;
                    Data[i * 4 + 1] = 255;
                    Data[i * 4 + 2] = 255;
                    Data[i * 4 + 3] = 255;
                }
                else
                {
                    Data[i * 4] = 0;
                    Data[i * 4 + 1] = 0;
                    Data[i * 4 + 2] = 0;
                    Data[i * 4 + 3] = 255;
                }

            });

            return Data;
        }
    }
    public class Christian : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Visible; }


        public string Name { get => "Christian"; }
        public byte[] Data { get; set; }
        private int size = 1;
        public int Size { get => size; set { size = value; MainWindowVM.Instance.Update(); } }
        private double strength = 0.5;
        public double Strength { get => strength; set { strength = value; MainWindowVM.Instance.Update(); } }
        private int width;
        public byte[] Calculate(byte[] data)
        {
            width = (int)MainWindowVM.Instance.OutputBitmap.Width;
            Data = new byte[data.Length];
            int min = 255 * 3;
            double sigmaMax = 0;

            long[] Integr = Calc.CalcIntegrMatrix(data, width, x => x);
            long[] Integr2 = Calc.CalcIntegrMatrix(data, width, x => x * x);

            Parallel.For(width * (size) + size, data.Length / 4 - (width * size + size), (i, state) =>
            //Parallel.For(0, data.Length / 4, (i, state) =>
            {
                if (data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2] < min)
                {
                    min = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                }
                int mx = 0;
                int mx2 = 0;
                mx = Calc.CalcSum(Integr, size, width, i);
                mx2 = Calc.CalcSum(Integr2, size, width, i);
                mx = mx / ((Size + Size + 1) * (Size + Size + 1));
                mx2 = mx2 / ((Size + Size + 1) * (Size + Size + 1));
                if (Math.Sqrt(mx2 - (mx * mx)) > sigmaMax)
                    sigmaMax = Math.Sqrt(mx2 - (mx * mx));

            });            //Parallel.For(size + size * width, data.Length / 4 - size * width - size, (i, state) =>
            Parallel.For(width * (size) + size, data.Length / 4 - (width * size + size), (i, state) =>
            //for (int i = width * size + size; i < Data.Length / 4 - width * size - size; i++)
            {
                int mx = 0;
                int mx2 = 0;
                /*
                for (int y = -Size; y <= Size; y++)
                //Parallel.For(-Size, Size, (y, state) =>
                {
                    for (int x = -Size; x <= Size; x++)
                    //Parallel.For(-Size, Size, (x, state) =>
                    {
                        int pixCoord = i + y * width + x;
                        if (pixCoord > 0 && pixCoord < data.Length / 4)
                        {
                            int pixVal = (data[pixCoord * 4] + data[pixCoord * 4 + 1] + data[pixCoord * 4 + 2]) / 3;
                            mx += pixVal;
                            mx2 += pixVal * pixVal;
                        }
                    }//);
                }
                //);
                */
                mx = Calc.CalcSum(Integr, size, width, i);
                mx2 = Calc.CalcSum(Integr2, size, width, i);
                mx = mx / ((Size + Size + 1) * (Size + Size + 1));
                mx2 = mx2 / ((Size + Size + 1) * (Size + Size + 1));
                double sigma = Math.Sqrt(mx2 - (mx * mx));
                int threshold = (int)((1 - Strength) * mx + Strength * min + Strength * sigma / sigmaMax * (mx - min));
                int sum = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                if (sum / 3 > threshold)
                {
                    Data[i * 4] = 255;
                    Data[i * 4 + 1] = 255;
                    Data[i * 4 + 2] = 255;
                    Data[i * 4 + 3] = 255;
                }
                else
                {
                    Data[i * 4] = 0;
                    Data[i * 4 + 1] = 0;
                    Data[i * 4 + 2] = 0;
                    Data[i * 4 + 3] = 255;
                }

            }
            );

            return Data;
        }
    }
    public class Bradley : IBinarizationMethod
    {
        public System.Windows.Visibility SensVisibility { get => System.Windows.Visibility.Hidden; }
        public System.Windows.Visibility SizeVisibility { get => System.Windows.Visibility.Visible; }
        public System.Windows.Visibility StrengthVisibility { get => System.Windows.Visibility.Visible; }


        public string Name { get => "Bradley"; }
        public byte[] Data { get; set; }
        private int size = 1;
        public int Size { get => size; set { size = value; MainWindowVM.Instance.Update(); } }
        private double strength = 0.5;
        public double Strength { get => strength; set { strength = value; MainWindowVM.Instance.Update(); } }
        private int width;

        public byte[] Calculate(byte[] data)
        {
            width = (int)MainWindowVM.Instance.OutputBitmap.Width;
            long[] Integr = new long[data.Length / 4];
            Data = new byte[data.Length];
            /*
                        for (int i = 0; i < data.Length / 4; i++)
                        {
                            long upper = 0, left = 0, leftupper = 0;
                            if (i / width >= 1 && i % width > 0)
                            {
                                upper = Integr[i - width];
                                left = Integr[i - 1];
                                leftupper = Integr[i - width - 1];
                            }
                            else if (i / width >= 1 && i % width == 0)
                            {
                                upper = Integr[i - width];
                                left = 0;
                                leftupper = 0;
                            }
                            else if (i < width && i % width > 0)
                            {
                                upper = 0;
                                left = Integr[i - 1];
                                leftupper = 0;
                            }
                            else if (i == 0)
                            {
                                upper = 0;
                                leftupper = 0;
                                left = 0;
                            }

                            Integr[i] = (data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2]) / 3 + left + upper - leftupper;

                        }
            */
            Integr = Calc.CalcIntegrMatrix(data, width, x => x);
            Parallel.For(width * (size) + size, data.Length / 4 - (width * size + size), (i, state) =>
            //for (int i = width * (size + 1) + size + 1; i < data.Length / 4 - (width * size + size); i++)
            {
                int median = data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2];
                median /= 3;
                /*
                int upperindex = i - width * (size + 1) + size;
                
                int leftindex = i + size * width - size - 1;
                int leftupperindex = i - width * (size + 1) - (size + 1);

                int sum;
                if (leftupperindex == 0)
                    sum = (int)(Integr[i + size * width + size]);
                else if (leftupperindex < width)
                    sum = (int)(Integr[i + size * width + size] - Integr[leftindex]);
                else if (leftupperindex % width == 0)
                    sum = (int)(Integr[i + size * width + size] - Integr[upperindex]);
                else
                    sum = (int)(Integr[i + size * width + size] + Integr[leftupperindex] - Integr[leftindex] - Integr[upperindex]);
                */
                int sum = Calc.CalcSum(Integr, size, width, i);
                if (median * (size * 2 + 1) * (size * 2 + 1) < sum * (1 - strength))
                {
                    Data[i * 4 + 0] = 0;
                    Data[i * 4 + 1] = 0;
                    Data[i * 4 + 2] = 0;
                    Data[i * 4 + 3] = 255;
                }
                else
                {
                    Data[i * 4 + 0] = 255;
                    Data[i * 4 + 1] = 255;
                    Data[i * 4 + 2] = 255;
                    Data[i * 4 + 3] = 255;
                }
            }
            );
            return Data;
        }
    }

    internal class Calc
    {

        public static long[] CalcIntegrMatrix(byte[] data, int width, Func<long, long> func)
        {


            long[] Integr = new long[data.Length / 4];
            for (int i = 0; i < data.Length / 4; i++)
            {
                long upper = 0, left = 0, leftupper = 0;
                if (i / width >= 1 && i % width > 0)
                {
                    upper = Integr[i - width];
                    left = Integr[i - 1];
                    leftupper = Integr[i - width - 1];
                }
                else if (i / width >= 1 && i % width == 0)
                {
                    upper = Integr[i - width];
                    left = 0;
                    leftupper = 0;
                }
                else if (i < width && i % width > 0)
                {
                    upper = 0;
                    left = Integr[i - 1];
                    leftupper = 0;
                }
                else if (i == 0)
                {
                    upper = 0;
                    leftupper = 0;
                    left = 0;
                }

                Integr[i] = func((data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2]) / 3) + left + upper - leftupper;

            }
            return Integr;
        }
        public static int CalcSum(long[] Integr, int size, int width, int i)
        {

            int upperindex = i - width * (size + 1) + size;
            int leftindex = i + size * width - size - 1;
            int leftupperindex = i - width * (size + 1) - (size + 1);
            int sum;
            if (leftupperindex + size + size * width == 0)
                sum = (int)(Integr[i + size * width + size]);
            else if (leftupperindex < width)
                sum = (int)(Integr[i + size * width + size] - Integr[leftindex]);
            else if (leftupperindex % width == 0)
                sum = (int)(Integr[i + size * width + size] - Integr[upperindex]);
            else
                sum = (int)(Integr[i + size * width + size] + Integr[leftupperindex] - Integr[leftindex] - Integr[upperindex]);
            return sum;
        }
    }

}

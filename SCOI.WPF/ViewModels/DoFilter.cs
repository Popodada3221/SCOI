using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Accessibility;

namespace SCOI.WPF.ViewModels
{
    public class RowData : INotifyPropertyChanged
    {
        private float data;
        public float Data { get => data; set { data = value; OnPropertyChanged(nameof(Data)); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
    public class DoFilter
    {
        public static ObservableCollection<DoFilter> Instance { get; set; }
        public DoFilter(uint columns)
        {
            Row = new ObservableCollection<RowData>();
            for (int i = 0; i < columns; i++)
            {
                Row.Add(new RowData());
            }
        }
        public ObservableCollection<RowData> Row { get; set; }
        public static byte[] CalculateLinear(byte[] data, int width)
        {
            byte[] result = new byte[data.Length];
            Parallel.For(0, data.Length / 4, (i, state) =>
            //for (int i = 0; i < data.Length / 4; i++)
            {
                int x = i % width;
                int y = i / width;
                int[] pixel = new int[4];
                for (int j = 0; j < Instance.Count; j++)
                {
                    for (int k = 0; k < Instance[j].Row.Count; k++)
                    {
                        if (y + j - Instance.Count / 2 >= 0 && y + j - Instance.Count / 2 < data.Length / width / 4)
                        {
                            if (x + k - Instance[j].Row.Count / 2 >= 0 && x + k - Instance[j].Row.Count / 2 < width)
                            {
                                //result[i * 4]
                                pixel[0] += (int)(data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4] * Instance[j].Row[k].Data);
                                //result[i * 4 + 1]
                                pixel[1] += (int)(data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4 + 1] * Instance[j].Row[k].Data);
                                //result[i * 4 + 2]
                                pixel[2] += (int)(data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4 + 2] * Instance[j].Row[k].Data);
                                //result[i * 4 + 3] = 255;

                            }

                        }
                    }

                }
                pixel[3] = 255;
                result[i * 4] = Method.CapByte(pixel[0]);
                result[i * 4 + 1] = Method.CapByte(pixel[1]);
                result[i * 4 + 2] = Method.CapByte(pixel[2]);
                result[i * 4 + 3] = 255;

            }
            );
            return result;
        }
        public static byte[] CalculateMedian(byte[] data, int width)
        {
            byte[] result = new byte[data.Length];

            if (Instance.Count > 0)
            {
                int medianIndex = Instance.Count * Instance[0].Row.Count / 2;
                //for (int i = 0; i < data.Length / 4; i++)
                Parallel.For(0, data.Length / 4, (i, state) =>
            {
                int x = i % width;
                int y = i / width;
                byte[] blues = new byte[Instance.Count * Instance[0].Row.Count];
                byte[] greens = new byte[Instance.Count * Instance[0].Row.Count];
                byte[] reds = new byte[Instance.Count * Instance[0].Row.Count];
                for (int j = 0; j < Instance.Count; j++)
                {
                    for (int k = 0; k < Instance[j].Row.Count; k++)
                    {
                        if (y + j - Instance.Count / 2 >= 0 && y + j - Instance.Count / 2 < data.Length / width / 4)
                        {
                            if (x + k - Instance[j].Row.Count / 2 >= 0 && x + k - Instance[j].Row.Count / 2 < width)
                            {


                                blues[j * Instance[0].Row.Count + k] = data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4];
                                greens[j * Instance[0].Row.Count + k] = data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4 + 1];
                                reds[j * Instance[0].Row.Count + k] = data[((y + j - Instance.Count / 2) * width + x + k - Instance[j].Row.Count / 2) * 4 + 2];
                            }
                        }
                    }
                }
                ////var ordered = blues.Order();
                ////result[i * 4] = ordered.ElementAt(medianIndex);
                ////ordered = greens.Order();
                ////result[i * 4 + 1] = ordered.ElementAt(medianIndex);
                ////ordered = reds.Order();
                ////result[i * 4 + 2] = ordered.ElementAt(medianIndex);
                result[i * 4] = (Select(blues, Instance.Count * Instance[0].Row.Count / 2))[Instance.Count * Instance[0].Row.Count / 2];
                result[i * 4 + 1] = (Select(greens, Instance.Count * Instance[0].Row.Count / 2))[Instance.Count * Instance[0].Row.Count / 2];
                result[i * 4 + 2] = (Select(reds, Instance.Count * Instance[0].Row.Count / 2))[Instance.Count * Instance[0].Row.Count / 2];
                result[i * 4 + 3] = 255;
            });
            }
            else result = data;
            return result;
        }
        private static byte[] Select(byte[] input, int n)
        {
            //keep original array
            byte[] partiallySortedArray = (byte[])input.Clone();
            int startIndex = 0;
            var endIndex = input.Length - 1;
            var pivotIndex = n;
            Random r = new Random();
            while (endIndex > startIndex)
            {
                pivotIndex = QuickSelectPartition(partiallySortedArray, startIndex, endIndex, pivotIndex);
                if (pivotIndex == n)
                {
                    break;
                }
                if (pivotIndex > n)
                {
                    endIndex = pivotIndex - 1;
                }
                else
                {
                    startIndex = pivotIndex + 1;
                }

                pivotIndex = r.Next(startIndex, endIndex);
            }

            return partiallySortedArray;
        }

        private static int QuickSelectPartition(byte[] array, int startIndex, int endIndex, int pivotIndex)
        {
            byte pivotValue = array[pivotIndex];
            Swap(ref array[pivotIndex], ref array[endIndex]);
            for (int i = startIndex; i < endIndex; i++)
            {
                if (array[i].CompareTo(pivotValue) > 0)
                {
                    continue;
                }
                Swap(ref array[i], ref array[startIndex]);
                startIndex++;
            }
            Swap(ref array[endIndex], ref array[startIndex]);
            return startIndex;
        }

        private static void Swap(ref byte i, ref byte i1)
        {
            byte temp = i;
            i = i1;
            i1 = temp;
        }
        public static void CalculateGauss(double sigma)
        {
            double sig_sqr = 2.0 * sigma * sigma;
            double pi_siq_sqr = sig_sqr * 3.14;
            double sum = 0;
            for (int i = 0; i < Instance.Count; i++)
            {
                for (int j = 0; j < Instance[i].Row.Count; j++)
                {
                    double y = i - Instance.Count / 2;
                    double x = j - Instance.Count / 2;
                    Instance[i].Row[j].Data = (float)(1.0 / pi_siq_sqr * Math.Exp(-1.0 * (x * x + y * y) / (sig_sqr)));
                    sum += Instance[i].Row[j].Data;
                }
            }
            for (int i = 0; i < Instance.Count; i++)
            {
                for (int j = 0; j < Instance[i].Row.Count; j++)
                {
                    Instance[i].Row[j].Data = (float)(Instance[i].Row[j].Data / sum);
                }
            }
        }
    }
}

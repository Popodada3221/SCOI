using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SCOI.WPF.Utils
{
    public class BitmapHelper
    {
        public static BitmapSource ByteToBitmap(byte[] byteArray, int width, int height)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                var img = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, byteArray, width * 4);
                return img;
            }
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static Bitmap Resize(Bitmap bitmap, int width, int height)
        {
            var newImage = new Bitmap(width, height);
            using (var gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bitmap, new Rectangle(0, 0, width, height));
            }

            return newImage;
        }


        public static BitmapSource GetSourceFromBitmap(Bitmap source)
        {
            Contract.Requires(source != null);

            var ip = source.GetHbitmap();
            BitmapSource bs;
            int result;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                result = NativeMethods.DeleteObject(ip);
            }
            if (result == 0)
                throw new InvalidOperationException("NativeMethods.DeleteObject returns 0 (operation failed)");

            return bs;
        }

        internal static class NativeMethods
        {
            [DllImport("gdi32")]
            static internal extern int DeleteObject(IntPtr o);
        }
    }
}

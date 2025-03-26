using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCOI.WPF.Utils
{
    public class Vec2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vec2D(double x, double y)
        {
            X = x; Y = y;
        }
        public double GetLength()
        {
            return Math.Sqrt(X * X + Y * Y);
        }
        public static Vec2D operator -(Vec2D left, Vec2D right)
        {
            return new Vec2D(left.X - right.X, left.Y - right.Y);
        }
    }
}

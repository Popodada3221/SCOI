using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCOI.WPF.ViewModels
{
    public class Method
    {
        public string Name { get; set; }
        public delegate byte ByteOperation(byte a, byte b, double opacity = 1);
        public ByteOperation Operation { get; set; }
        public static List<Method> MethodList = new List<Method>
        {
            new Method()
            {
                Name = "Normal",
                Operation = (a, b, o) => CapByte((int)(b * o))
            },
            new Method
            {
                Name = "Add",
                Operation = (a, b, o) => CapByte((int)((a+b)*o))
            },
            new Method
            {
                Name = "Substract",
                Operation = (a, b, o) => CapByte((int)((a-b)*o))
            },
            new Method
            {
                Name = "Multiply",
                Operation = (b, a, o) => CapByte((int)(a*b/255*o))
            },
            new Method
            {
                Name = "Max",
                Operation = (a, b, o) => CapByte((int)(Math.Max(a, b)*o))
            },
            new Method
            {
                Name = "Min",
                Operation = (a, b, o) => CapByte((int)(Math.Min(a, b)*o))
            },
            new Method
            {
                Name = "Screen",
                Operation = (a, b, o) => CapByte((int)((255-(255-a)*(255-b)/255)*o))
            },
            new Method
            {
                Name = "Color Burn",
                Operation = (a, b, o) => CapByte((int)(255-((255-(double)a)/(double)b*255)))


            },
            new Method
            {
                Name = "Color Dodge",
                Operation = (a, b, o) =>
                {
                    return CapByte((int)(255 - (((double)a / (255 - (double)b)) * 255)));
                }
            },
            new Method
            {
                Name = "Overlay",
                Operation = (a, b, o) =>
                {
                    if(a<128)
                    {
                        return CapByte((int)((a*b)/255*o));
                    }
                    else return CapByte((int)((255-(255-a)*(255-b)/255)*o));

                }
            },
            new Method
            {
                Name = "Median",
                Operation = (a, b, o) => CapByte((a+b)/2)
            }


        };

        private static byte CapByte(int a)
        {
            if (a < 0)
                return 0;
            else if (a > 255)
                return 255;
            else return (byte)a;
        }
        public static byte CapByte(double a)
        {
            if (a < 0)
                return 0;
            else if (a > 255)
                return 255;
            else return (byte)a;
        }
        public static List<Method> GetMethodList()
        {
            return MethodList;
        }
    }
}

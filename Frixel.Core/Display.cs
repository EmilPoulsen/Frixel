using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Display
{
    public class DisplayProperties
    {

    }
    public class Color
    {
        public double A;
        public int R;
        public int G;
        public int B;

        public Color(int a, int r, int g, int b)
        {
            this.A = a;
            this.B = b;
            this.G = g;
            this.R = r;
        }
    }
}

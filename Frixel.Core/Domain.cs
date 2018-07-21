using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core
{
    public class Domain
    {
        public double Min;
        public double Max;

        public Domain(double min, double max)
        {
            this.Min = min;
            this.Max = max;
        }
    }

    public class Domain2d
    {
        public Domain X;
        public Domain Y;

        public Domain2d(Domain x, Domain y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}

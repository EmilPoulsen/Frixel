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

        public double Size
        {
            get
            {
                return Math.Abs(this.Max - this.Min);
            }
        }

        public bool IsLargerThan(Domain b)
        {
            return this.Size > b.Size;
        }

        public void ScaleMid(double factor)
        {
            var fullMovementDist = this.Size - (this.Size * factor);
            this.Min += fullMovementDist / 2;
            this.Max -= fullMovementDist / 2;
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

        public double AspectRatioX
        {
            get
            {
                return X.Size / Y.Size;
            }
        }
        public double AspectRatioY
        {
            get
            {
                return Y.Size / X.Size;
            }
        }
    }
}

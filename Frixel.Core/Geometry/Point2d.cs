using Frixel.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Geometry
{
    public class Point2d
    {
        public bool IsLocked;
        public bool IsInside;
        public bool IsPixeled = false;
        public double X;
        public double Y;

        public Point2d(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point2d Map(Domain2d from, Domain2d to)
        {
            return new Point2d(this.X.Map(from.X, to.X),
                            this.Y.Map(from.Y, to.Y));
        }
    }
}

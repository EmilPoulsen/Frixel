using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Extensions;

namespace Frixel.Core.Geometry
{
    public class Line2d
    {
        public Point2d Start;
        public Point2d End;

        public Line2d(Point2d start, Point2d end)
        {
            this.Start = start;
            this.End = end;
        }

        public Point2d MidPoint()
        {
            return new Point2d(
                (this.Start.X + this.End.X) / 2,
                (this.Start.Y + this.End.Y) / 2
             );
        }

        public Line2d Map(Domain2d from, Domain2d to)
        {
            var start = new Point2d(this.Start.X.Map(from.X, to.X),
                            this.Start.Y.Map(from.Y, to.Y));
            var end = new Point2d(this.End.X.Map(from.X, to.X),
                            this.End.Y.Map(from.Y, to.Y));

            var line = new Line2d(start, end);
            return line;
        }
    }
}

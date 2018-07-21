using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Map(Domain2d from, Domain2d to)
        {
            // TODO TODO
        }
    }
}

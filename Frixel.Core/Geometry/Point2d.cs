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
        public static Point2d Origin = new Point2d(0, 0);

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

        public Point2d FindClosest(List<Point2d> cloud)
        {
            // Create a matching list with distance
            var pByDist = cloud.Select(p => new Tuple<double, Point2d>(p.DistanceTo(this), p)).OrderBy(p => p.Item1);
            return pByDist.First().Item2;
        }

        public double DistanceTo(Point2d b)
        {
            return Math.Sqrt(Math.Pow((this.X - b.X), 2) + Math.Pow((this.Y - b.Y), 2));
        }

        public bool IsWithinDomain(List<Point2d> cloud, int margin = 0)
        {
            // Check if location is within the domain of the points
            if (this.X < cloud.Select(p => p.X).Min() - margin
             | this.Y < cloud.Select(p => p.Y).Min() - margin
             | this.X > cloud.Select(p => p.X).Max() + margin
             | this.Y > cloud.Select(p => p.Y).Max() + margin
             ) { return false; }
            return true;
        }

        public Point2d Copy()
        {
            return new Point2d(this.X, this.Y);
        }

        public Point2d Move(Line2d translation, bool copy = false)
        {
            Point2d pointToMove;
            if (copy) { pointToMove = this.Copy(); } else { pointToMove = this; }

            pointToMove.X += (translation.End.X - translation.Start.X);
            pointToMove.Y += (translation.End.Y - translation.Start.Y);

            return pointToMove;
        }
    }
}

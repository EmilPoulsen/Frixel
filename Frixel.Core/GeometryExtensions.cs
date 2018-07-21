using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Geometry;
using System.Linq;

namespace Frixel.Core
{
    public static class GeometryExtensions
    {
        public static Domain2d GetBoundingBox(this List<Point2d> points)
        {
            var xCol = points.Select(p => p.X);
            var yCol = points.Select(p => p.Y);
            return new Domain2d(
                new Domain(xCol.Min(), xCol.Max()),
                new Domain(yCol.Min(), yCol.Max())
            );
        }
    }
}

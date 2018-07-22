using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Geometry;

namespace Frixel.Rhinoceros.Extensions
{
    public static class Extensions
    {
        public static Point2d ToFrixelPoint(this Rhino.Geometry.Point3d point, bool isInside = false)
        {
            var p = new Point2d(point.X, point.Y) { IsInside = isInside };
            return p;
        }

        public static Rhino.Geometry.Point3d ToRhinoPoint(this Point2d point, double z = 0)
        {
            return new Rhino.Geometry.Point3d(point.X, point.Y, z);
        }
    }
}

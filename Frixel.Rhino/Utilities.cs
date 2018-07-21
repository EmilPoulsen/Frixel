using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core;
using Rhino.Geometry;

namespace Frixel.Rhinoceros
{
    public static class Utilities
    {
        //public List<Frixel.Core.Geometry.Line2d> ConvertRhinoCurveToFrixelCurve(Curve c)
        //{

        //}

        // 1 - Bounding rect of rhino
        // 2 - Cut it into "pixels" (or rather, just place an array of points and check for inclusion). Then jump around the field determining if 4 pixels or present. 
        // 3 - Determine of the pixel is inside or outside
        // Should also send over the rhino curve as another asset that gets drawn
        // 4 - If its inside add the pixel to our thing
        //

        public static bool PointIsInsideOrOnCurve(Curve curve, Point3d point3d, double tolerance)
        {
            var result = PointInCurve(curve, point3d, tolerance);
            switch (result)
            {
                case PointContainment.Unset:
                    return false;
                case PointContainment.Inside:
                    return true;
                case PointContainment.Outside:
                    return false;
                case PointContainment.Coincident:
                    return true;
                default:
                    return false;
            }
        }

        public static PointContainment PointInCurve(Curve curve, Point3d point3d, double tolerance)
        {
            BoundingBox boundingBox = curve.GetBoundingBox(false);
            Plane worldXY = Plane.WorldXY;
            if (!curve.TryGetPlane(out worldXY, boundingBox.Min.DistanceTo(boundingBox.Max)))
            {
                worldXY = Plane.WorldXY;
            }
            point3d = worldXY.ClosestPoint(point3d);
            var result = curve.Contains(point3d, worldXY, tolerance);
            return result;
        }

        public static bool AtLeastXInside(bool a, bool b, bool c, bool d, int x)
        {
            int howMany  = 0;
            if (a) howMany++;
            if (b) howMany++;
            if (c) howMany++;
            if (d) howMany++;
            if(howMany >= x) { return true; }
            return false;
        }

        public static double Distance(Point3f a, Point3f b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2) + Math.Pow((a.Z - b.Z), 2));
        }

        public static double Distance(Point3d a, Point3d b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2) + Math.Pow((a.Z - b.Z), 2));
        }

        public static double Distance(double[] a, int[] b)
        {
            return Math.Sqrt(Math.Pow((a[0] + b[0]), 2) + Math.Pow((a[1] + b[1]), 2) + Math.Pow((a[2] + b[2]), 2));
        }
    }
}

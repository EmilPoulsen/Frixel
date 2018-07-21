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
    }
}

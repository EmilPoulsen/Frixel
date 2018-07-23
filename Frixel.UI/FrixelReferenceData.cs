using Frixel.Core;
using Frixel.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.UI
{
    /// <summary>
    /// Passed from client to window on our Reference
    /// </summary>
    public class FrixelReferenceData
    {
        public readonly PixelStructure Structure;
        public readonly MassingStructure MassingStructure;
        public readonly List<Line2d> ActualShape;
        public readonly Domain2d BoundingBox;
        public readonly double ActualXSize;
        public readonly double ActuveYSize;

        public FrixelReferenceData(PixelStructure pixelStructure, MassingStructure massingStructure)
        {
            this.Structure = pixelStructure;
            this.MassingStructure = massingStructure;
            this.ActualShape = massingStructure.Outline;
            this.BoundingBox = massingStructure.BoundingBox;
            this.ActualXSize = massingStructure.xSpacing;
            this.ActuveYSize = massingStructure.ySpacing;
        } 
    }
}

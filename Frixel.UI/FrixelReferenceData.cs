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
        public PixelStructure Structure;
        public List<Line2d> ActualShape;
        public Domain2d BoundingBox;

        public FrixelReferenceData(PixelStructure structure, List<Line2d> actualShape, Domain2d boundingBox)
        {
            this.Structure = structure;
            this.ActualShape = actualShape;
            this.BoundingBox = boundingBox;
        } 
    }
}

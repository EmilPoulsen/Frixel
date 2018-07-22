using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core
{
    public class MassingStructure
    {
        public Dictionary<Tuple<int, int>, Core.Geometry.Point2d> Nodes;
        public List<Core.Geometry.Line2d> Outline;
        public Core.Domain2d BoundingBox;
        public double xSpacing;
        public double ySpacing;
        public int xBayCount;
        public int yBayCount;

        public MassingStructure() { }
    }
}

using Frixel.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core {
    public class ModelCreator {

        public static PixelStructure CreatePixelStructure() {

            PixelStructure structure = new PixelStructure();

            structure.Nodes.AddRange(new List<Point2d>() {
                //add bottom nodes
                new Point2d(0, 0),
                new Point2d(3, 0),
                new Point2d(6, 0),
                new Point2d(9, 0),

                //add mid nodes
                new Point2d(0, 3),
                new Point2d(3, 3),
                new Point2d(6, 3),
                new Point2d(9, 3),

                //add top nodes
                new Point2d(0, 6),
                new Point2d(3, 6),
                new Point2d(6, 6),
                new Point2d(9, 6),
            });

            //add supports
            for (int i = 0; i < 4; i++) {
                structure.Nodes[i].IsLocked = true;
            }

            //add bracing elements
            structure.Edges.AddRange(new List<Edge>() {
                //horizontal elements
                new Edge(0, 1), new Edge(1, 2), new Edge(2, 3),
                new Edge(4, 5), new Edge(5, 6), new Edge(6, 7),
                new Edge(8, 9), new Edge(9, 10), new Edge(10, 11),

                new Edge(0, 4), new Edge(4, 8),
                new Edge(1, 5), new Edge(5, 9),
                new Edge(2, 6), new Edge(6, 10),
                new Edge(3, 7), new Edge(7, 11),
            });

            //add the bracing
            structure.Pixels.AddRange(new List<Pixel>() {
                new Pixel(5, 6, 1, 2, PixelState.Moment),
                new Pixel(9, 10, 5, 6, PixelState.Moment)
            });

            //add load
            structure.GravityLoad.Activated = true;
            structure.GravityLoad.Amplification = 1;

            structure.WindLoad.Activated = true;
            structure.WindLoad.Direction = new Point2d(70000000, 0);

            structure.WindLoad.NodeIndices.AddRange(new List<int>() {
                0, 4, 8
            });

            return structure;
        }
    }
}

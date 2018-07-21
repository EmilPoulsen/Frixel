using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frixel.Core;
using Frixel.Core.Geometry;
using System.Collections.Generic;

namespace Frixel.Test {
    [TestClass]
    public class FrixelAnalysisTest {
        [TestMethod]
        public void AnalyzeFrixelFrame() {
        }




        public PixelStructure CreatePixelStructure() {

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

            return structure;




        }
    }
}

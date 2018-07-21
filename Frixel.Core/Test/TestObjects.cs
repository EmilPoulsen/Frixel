using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Test
{
    public class TestObjects
    {
        /// <summary>
        /// Test pixel structure looks like tetris piece (diagram)
        /// [X]
        /// [X][ ][ ]
        /// </summary>
        public static PixelStructure TestStructure = new PixelStructure(
            new List<Geometry.Point2d>()
            {
                // P1
                new Geometry.Point2d(0,0), // 0
                new Geometry.Point2d(0,1), // 1
                new Geometry.Point2d(1,1), // 2
                new Geometry.Point2d(1,0), // 3
                // P2
                new Geometry.Point2d(2,1), // 4
                new Geometry.Point2d(2,0), // 5
                // P3
                new Geometry.Point2d(3,1), // 6
                new Geometry.Point2d(3,0), // 7
                // P4
                new Geometry.Point2d(0,2), // 8
                new Geometry.Point2d(1,2), // 9
            },
            new List<Pixel>()
            {
                new Pixel(1,2,0,3,PixelState.Moment),
                new Pixel(2,4,3,5,PixelState.None),
                new Pixel(4,6,5,7,PixelState.None),
                new Pixel(8,9,0,1,PixelState.Moment)
            }
       );
    }
}

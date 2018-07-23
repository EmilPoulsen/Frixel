using Frixel.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Loading
{
    public abstract class Load
    {

        //bla bla common properties and shit

    }

    public class GravityLoad : Load
    {
        public bool Activated { get; set; }

        public double Amplification { get; set; }

    }

    public class WindLoad : Load
    {

        public WindLoad()
        {

            this.NodeIndices = new List<int>();
            this.Direction = new Point2d(0, 1);
        }

        public bool Activated { get; set; }

        public List<int> NodeIndices { get; set; }

        public Point2d Direction { get; set; }
    }
}

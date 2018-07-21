using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Geometry;

namespace Frixel.Core {
    public class Pixel {
        public readonly PixelState State;
        public readonly int TopLeft;
        public readonly int TopRight;
        public readonly int BottomRight;
        public readonly int BottomLeft;

        public Pixel(int topLeft, int topRight, int botLeft, int botRight, PixelState state) {
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomLeft = botLeft;
            this.BottomRight = botRight;
            this.State = state;
        }

        public List<Edge> GetEdges() {
            return new List<Edge>()
            {
                new Edge(this.TopLeft, this.TopRight),
                new Edge(this.TopRight, this.BottomRight),
                new Edge(this.BottomRight, this.BottomLeft),
                new Edge(this.BottomLeft, this.TopLeft)
            };
        }

        public List<Edge> GetBracing() {
            switch (State) {
                case PixelState.Moment:
                    var x1 = new Edge(this.TopLeft, this.BottomRight);
                    var x2 = new Edge(this.TopRight, this.BottomLeft);
                    return new List<Edge>() { x1, x2 };
                case PixelState.None:
                    return new List<Edge>();
            };
            return new List<Edge>();
        }

        public List<Edge> GetAllEdges() {
            var edgeList = new List<Edge>();
            edgeList.AddRange(GetEdges());
            edgeList.AddRange(GetBracing());
            return edgeList;
        }

    }

    public class Edge {
        public int Start;
        public int End;

        public Edge(int s, int e) {
            this.Start = s;
            this.End = e;
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(Edge)) return false;
            var objEdge = obj as Edge;
            if (this.Start == objEdge.Start && this.End == objEdge.End) return true;
            else return false;
        }

    }

    public class PixelStructure {
        public List<Point2d> Nodes;
        public List<Edge> Edges;
        public List<Pixel> Pixels;
        public WindLoad WindLoad;
        public GravityLoad GravityLoad;

        public PixelStructure() { }

        /// <summary>
        /// Constructor for testing.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="pixels"></param>
        public PixelStructure(List<Point2d> nodes, List<Pixel> pixels) {
            this.Nodes = nodes;
            this.Pixels = pixels;

            // Generates edges from pixels
            var allEdges = pixels.SelectMany(p => p.GetEdges());

            Edges = allEdges.Distinct().ToList();
        }

        public List<Line2d> GetLines() {
            return Edges.Select(e => {
                return new Line2d(Nodes[e.Start], Nodes[e.End]);
            }).ToList();
        }

        public List<Line2d> GetAllLInes()
        {
            return Pixels.SelectMany(p =>
            {
                return p.GetAllEdges();
            }).Select(e =>
           {
               return new Line2d(Nodes[e.Start], Nodes[e.End]);
           }).ToList();
        }
    }

    public abstract class Load {
        
        //bla bla common properties and shit

    }

    public class GravityLoad : Load {
        public bool Activated { get; set; }
        
        public double Amplification { get; set; }

    }

    public class WindLoad : Load {
        
        public WindLoad() {

            this.NodeIndices = new List<int>();
            this.Direction = new Point2d(0, 1);
        }

        public bool Activated { get; set; }

        public List<int> NodeIndices { get; set; }

        public Point2d Direction { get; set; }
    }

    public class NodeResult{

        public double DispX { get; set; }
        public double DispY { get; set; }
    }

    public class ElementResult {
        double AxialForce { get; set; }
    }

    public class PixelResult {
        double LeftRight { get; set; }
        double RightLeft { get; set; }
        
    }

    public class AnalysisResults {

        public AnalysisResults() {
            this.NodeResults = new Dictionary<int, NodeResult>();
        }

        public Dictionary<int, NodeResult> NodeResults;
        public Dictionary<int, ElementResult> ElementResults;
        public Dictionary<int, PixelResult> PixelResults;
        
    }

    public enum PixelState
    {
        None,
        Moment
    }
}

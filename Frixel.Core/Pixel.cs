using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Geometry;

namespace Frixel.Core {
    public class Pixel {
        public PixelState State { get; private set; }
        public int TopLeft { get; private set; }
        public int TopRight { get; private set; }
        public int BottomRight { get; private set; }
        public int BottomLeft { get; private set; }

        public Pixel(int topLeft, int topRight, int botLeft, int botRight, PixelState state) {
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomLeft = botLeft;
            this.BottomRight = botRight;
            this.State = state;
        }

        public void UpdateTopology(int topleft, int topright, int botleft, int botright)
        {
            this.TopLeft = topleft;
            this.TopRight = topright;
            this.BottomLeft = botleft;
            this.BottomRight = botright;
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

        public bool ContainsNode(int nodeIndex, bool oneSideOnly = false)
        {
            if (oneSideOnly)
            {
                if (this.BottomLeft == nodeIndex
                  | this.TopLeft == nodeIndex)
                {
                    return true;
                }
                return false;
            }
            if (this.BottomLeft == nodeIndex
              | this.BottomRight == nodeIndex
              | this.TopLeft == nodeIndex
              | this.TopRight == nodeIndex)
            {
                return true;
            }
            return false;
        }

        public void ChangeStateTo(PixelState state)
        {
            this.State = state;
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
        public List<Point2d> DispNodes;
        public List<Edge> Edges;
        public List<Pixel> Pixels;
        public WindLoad WindLoad;
        public GravityLoad GravityLoad;

        public PixelStructure() {
            this.Nodes = new List<Point2d>();
            this.DispNodes = new List<Point2d>();
            this.Edges = new List<Edge>();
            this.Pixels = new List<Pixel>();
            this.WindLoad = new WindLoad();
            this.GravityLoad = new GravityLoad();
        }

        public PixelStructure(List<Point2d> nodes, List<Pixel> pixels) {
            this.Nodes = nodes;
            this.Pixels = pixels;
            this.DispNodes = nodes.Select(n =>
            {
                return new Point2d(n.X, n.Y)
                {
                    IsLocked = n.IsLocked,
                    IsInside = n.IsInside,
                    IsPixeled = n.IsPixeled,
                };
            }).ToList();

            // Generates edges from pixels
            var allEdges = pixels.SelectMany(p => p.GetEdges());

            Edges = allEdges.Distinct().ToList();
            this.WindLoad = new WindLoad();
            this.GravityLoad = new GravityLoad();
        }

        public List<Line2d> GetLines() {
            return Edges.Select(e => {
                return new Line2d(Nodes[e.Start], Nodes[e.End]);
            }).ToList();
        }

        public List<Line2d> GetAllLInes(bool disp)
        {
            var pix = Pixels.SelectMany(p =>
            {
                return p.GetAllEdges();
            });
            if(disp) return pix.Select(e => new Line2d(DispNodes[e.Start], DispNodes[e.End])).ToList();
            else return pix.Select(e => new Line2d(Nodes[e.Start], Nodes[e.End])).ToList();
        }

        public List<Edge> GetAllEdges()
        {
            return Pixels.SelectMany(p =>
            {
                return p.GetAllEdges();
            }).ToList();
        }

        public void ResetDisp()
        {
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                this.DispNodes[i].X = this.Nodes[i].X;
                this.DispNodes[i].Y = this.Nodes[i].Y;
            }
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

    public class _analysisResults {

        public _analysisResults() {
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

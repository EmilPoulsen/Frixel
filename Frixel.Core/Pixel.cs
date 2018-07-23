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
        public bool LockedBrace { get; set; } = false;

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

        public void FlipState()
        {
            if (this.State != PixelState.None)
            {
                this.ClearState();
            }
            else
            {
                this.ChangeStateTo(PixelState.Moment);
            }
        }

        public void ClearState()
        {
            this.ChangeStateTo(PixelState.None);
            this.LockedBrace = false;
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

        public bool IsBraced { get; set; }

        double LeftRight { get; set; }
        double RightLeft { get; set; }
        


    }

    public class AnalysisResults {

        public AnalysisResults() {
            this.NodeResults = new Dictionary<int, NodeResult>();
            this.PixelResults = new Dictionary<int, PixelResult>();
        }

        public Dictionary<int, NodeResult> NodeResults;
        public Dictionary<int, ElementResult> ElementResults;
        public Dictionary<int, PixelResult> PixelResults;
    }

    public class AnalysisSummary
    {
        public double MinDisplacement;
        public double MaxDisplacement;
        public double NetLength;
        public int Elements = 0;
        public int Connections = 0;
        public int Supports = 0;

        public AnalysisSummary(AnalysisResults results, PixelStructure structure)
        {
            foreach (var node in structure.Nodes)
            {
                if (node.IsLocked) { Supports++; }
                if (node.IsPixeled) { Connections++; }
            }
            Elements = structure.Edges.Count;
            NetLength = structure.Edges.Select(e => structure.Nodes[e.Start].DistanceTo(structure.Nodes[e.End])).Sum();
            IEnumerable<double> displacements = results.NodeResults.Values.Select(n => Math.Sqrt(Math.Pow(n.DispX, 2) + Math.Pow(n.DispY, 2)));
            MaxDisplacement = displacements.Max();
            MinDisplacement = displacements.Min();
        }
    }

    public enum PixelState
    {
        None,
        Moment
    }
}

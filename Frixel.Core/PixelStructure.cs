using Frixel.Core.Display;
using Frixel.Core.Geometry;
using Frixel.Core.Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core
{
    public class PixelStructure
    {
        public List<Point2d> Nodes;
        public List<Point2d> DispNodes;
        public List<Edge> Edges;
        public List<Pixel> Pixels;
        public WindLoad WindLoad;
        public GravityLoad GravityLoad;
        public List<Color> AllEdgeColors;

        public PixelStructure()
        {
            this.Nodes = new List<Point2d>();
            this.DispNodes = new List<Point2d>();
            this.Edges = new List<Edge>();
            this.AllEdgeColors = new List<Color>();
            this.Pixels = new List<Pixel>();
            this.WindLoad = new WindLoad();
            this.GravityLoad = new GravityLoad();
        }

        public PixelStructure(List<Point2d> nodes, List<Pixel> pixels)
        {
            this.Nodes = nodes;
            this.Pixels = pixels;
            this.DispNodes = new List<Point2d>();

            // Generates edges from pixels
            var allEdges = pixels.SelectMany(p => p.GetEdges());

            Edges = allEdges.Distinct().ToList();
            this.AllEdgeColors = new List<Color>();
            this.WindLoad = new WindLoad();
            this.GravityLoad = new GravityLoad();
        }

        /// <summary>
        /// Returns true if this structure has analysis values
        /// </summary>
        /// <returns></returns>
        public bool HasAnalysisValues()
        {
            return this.DispNodes.Count != 0;
        }
        /// <summary>
        /// Returns true if there is a matching list of edge colors for displaced nodes 
        /// </summary>
        /// <returns></returns>
        public bool HasEdgeColors()
        {
            return this.AllEdgeColors.Count != 0 && this.AllEdgeColors.Count == this.GetAllEdges().Count;
        }

        /// <summary>
        /// Clears the collection of edge colors
        /// </summary>
        public void ClearAllEdgeColors()
        {
            this.AllEdgeColors.Clear();
        }

        /// <summary>
        /// Returns non-bracing pixel frame lines
        /// </summary>
        /// <returns></returns>
        public List<Line2d> GetLines()
        {
            return Edges.Select(e => {
                return new Line2d(Nodes[e.Start], Nodes[e.End]);
            }).ToList();
        }

        /// <summary>
        /// Includes bracing lines!
        /// </summary>
        /// <param name="disp">If true, construct with displaced nodes</param>
        /// <returns></returns>
        public List<Line2d> GetAllLines(bool disp)
        {
            var pix = Pixels.SelectMany(p =>
            {
                return p.GetAllEdges();
            });
            if (disp) return pix.Select(e => new Line2d(DispNodes[e.Start], DispNodes[e.End])).ToList();
            else return pix.Select(e => new Line2d(Nodes[e.Start], Nodes[e.End])).ToList();
        }

        /// <summary>
        /// Includes bracing edges!
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetAllEdges()
        {
            return Pixels.SelectMany(p =>
            {
                return p.GetAllEdges();
            }).ToList();
        }

        /// <summary>
        /// Resets the position of the displaced nodes to their designed position
        /// </summary>
        public void ResetDisp()
        {
            if(this.DispNodes.Count == 0)
            {
                this.DispNodes = this.Nodes.Select(n =>
                {
                    return new Point2d(n.X, n.Y)
                    {
                        IsLocked = n.IsLocked,
                        IsInside = n.IsInside,
                        IsPixeled = n.IsPixeled,
                    };
                }).ToList();
            }
            else
            {
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    this.DispNodes[i].X = this.Nodes[i].X;
                    this.DispNodes[i].Y = this.Nodes[i].Y;
                }
            }

        }

        private List<Point2d> GetCurrentlyDisplayedNodes(DisplayState displayState)
        {
            if (displayState == DisplayState.Analytical) return this.DispNodes;
            else return this.Nodes;
        }

        /// <summary>
        /// Returns true if successful, implying a redraw
        /// </summary>
        /// <param name="location"></param>
        /// <param name="displayState"></param>
        /// <returns></returns>
        public bool ChangeSupportAtLocation(Point2d location, DisplayState displayState)
        {
            // Get currently displayed nodes
            List<Point2d> MyNodes = GetCurrentlyDisplayedNodes(displayState);

            // Check if point is within domain
            if (!location.IsWithinDomain(MyNodes, 2)) { return false; }

            // Find closest node to point
            var closestNode = location.FindClosest(MyNodes);

            // Flip status of support
            closestNode.IsLocked = !closestNode.IsLocked;

            return true;
        }

        /// <summary>
        /// Returns true if successful, implying a redraw
        /// </summary>
        /// <returns></returns>
        public bool ChangeBracingAtLocation(Point2d location, DisplayState displayState)
        {
            // Get currently displayed nodes
            List<Point2d> MyNodes = GetCurrentlyDisplayedNodes(displayState);

            // Check if point is within domain
            if (!location.IsWithinDomain(MyNodes)) { return false; }

            // Find closest node to point
            var closestNode = location.FindClosest(MyNodes);
            var closestNodeIndex = MyNodes.IndexOf(closestNode);

            // See what side the point is relative to the node
            bool above = location.Y > closestNode.Y;
            bool left = location.X < closestNode.X;

            // Find the relevant pixel  
            try
            {
                // Bottom Right
                if (above && left)
                {
                    this.Pixels.Where(p => p.BottomRight == closestNodeIndex).First().FlipState();
                }
                // Bottom Left
                else if (above && !left)
                {
                    this.Pixels.Where(p => p.BottomLeft == closestNodeIndex).First().FlipState();
                }
                // Top Left
                else if (!above && !left)
                {
                    this.Pixels.Where(p => p.TopLeft == closestNodeIndex).First().FlipState();
                }
                // Top right
                else
                {
                    this.Pixels.Where(p => p.TopRight == closestNodeIndex).First().FlipState();
                }
            } catch { return false; }

            return true;
        }
    }
}

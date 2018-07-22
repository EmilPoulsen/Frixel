﻿using Frixel.Core.Display;
using Frixel.Core.Geometry;
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
            return this.AllEdgeColors.Count != 0 && this.DispNodes.Count != 0;
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
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                this.DispNodes[i].X = this.Nodes[i].X;
                this.DispNodes[i].Y = this.Nodes[i].Y;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Analysis
{
    public class AnalysisResults
    {

        public AnalysisResults()
        {
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
            IEnumerable<double> displacements = results.NodeResults.Values.Select(n => Math.Sqrt(Math.Pow(n.DispX, 2) + Math.Pow(n.DispY, 2))).Where(x => x != 0);
            MaxDisplacement = displacements.Max();
            MinDisplacement = displacements.Min();
        }
    }

    public class NodeResult
    {

        public double DispX { get; set; }
        public double DispY { get; set; }
    }

    public class ElementResult
    {
        double AxialForce { get; set; }
    }

    public class PixelResult
    {

        public bool IsBraced { get; set; }

        double LeftRight { get; set; }
        double RightLeft { get; set; }
    }
}

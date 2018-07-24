using Frixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFE;
using SharpFE.Geometry;
using Frixel.Core.Analysis;

namespace Frixel.Optimizer
{
    public class FrixelAnalyzer
    {
        
        public FrixelAnalyzer() {
            
        }
        
        public AnalysisResults Analyze(PixelStructure pixelStructure) {

            var model = BuildModel(pixelStructure);

            var results = AnalyzeModel(model);

            return results;
        }

        public AnalysisResults AnalyzeModel(FiniteElementModel model) {

            IFiniteElementSolver solver = new MatrixInversionLinearSolver(model);
            FiniteElementResults results = solver.Solve();

            AnalysisResults pixResults = new AnalysisResults();

            int i = 0;
            foreach (var node in model.Nodes) {

                DisplacementVector disp = null;
                if (results.DisplacementVectorExists(node)) {
                    disp = results.GetDisplacement(node);
                }
                else {
                    disp = new DisplacementVector(node, 0, 0);
                }
                
                pixResults.NodeResults.Add(i, new NodeResult() {
                    DispX = disp.X,
                    DispY = disp.Z
                });
                i++;
            }

            return pixResults;
            foreach (var elem in model.Elements) {

                if(elem is LinearTruss) {
                    var truss = elem as LinearTruss;
                    var startLoc = truss.StartNode.Location;
                    var endLoc = truss.EndNode.Location;

                    double initialDist = Distance(startLoc, endLoc);



                }

                


            }

            
        }

        public double Distance(CartesianPoint p1, CartesianPoint p2) {
            double lengthX = p2.X - p1.X;
            double lengthY = p2.Y - p1.Y;
            double lengthZ = p2.Z - p1.Z;
            return Math.Sqrt((lengthX * lengthX) + (lengthY * lengthY) + (lengthZ * lengthZ));
        }


        public SharpFE.FiniteElementModel BuildModel(PixelStructure structure) {

            FiniteElementModel model = new FiniteElementModel(ModelType.Truss2D);
            IMaterial material = new GenericElasticMaterial(7700, 210.0e9, 0.3, 210.0e9 / 2.69);

            ICrossSection section = new SolidRectangle(0.03, 0.01);

            foreach (var node in structure.Nodes) {
                var feNode = model.NodeFactory.CreateFor2DTruss(node.X, node.Y);

                //lock the supports
                if (node.IsLocked) {
                    model.ConstrainNode(feNode, DegreeOfFreedom.X);
                    model.ConstrainNode(feNode, DegreeOfFreedom.Z);
                }

            }

            foreach (var edge in structure.Edges) {
                int s = edge.Start;
                int e = edge.End;

                var sNode = model.Nodes.ElementAt(s);
                var eNode = model.Nodes.ElementAt(e);

                model.ElementFactory.CreateLinearTruss(sNode, eNode, material, section);
            }

            foreach (var pix in structure.Pixels) {
                var bracing = pix.GetBracing();
                foreach (var brace in bracing) {
                    int s = brace.Start;
                    int e = brace.End;

                    var sNode = model.Nodes.ElementAt(s);
                    var eNode = model.Nodes.ElementAt(e);
                    
                    model.ElementFactory.CreateLinearTruss(sNode, eNode, material, section);
                }
            }

            var wind = AddWindLoad(structure, model);
            var grav = AddGravityLoad(structure, model);

            var combined = CombineDict(wind, grav);

            foreach (var pair in combined) {

                ForceVector force100Z;
                if (!model.Forces.Contains(pair.Value)) {
                    force100Z = model.ForceFactory.CreateForTruss(pair.Value.X, pair.Value.Z);
                }
                else {
                    force100Z = pair.Value;
                }

                model.ApplyForceToNode(force100Z, pair.Key);
            }

            return model;

        }


        private static Dictionary<IFiniteElementNode, ForceVector> CombineDict(
            Dictionary<IFiniteElementNode, ForceVector> gravity,
            Dictionary<IFiniteElementNode, ForceVector> wind) {

            //Dictionary<IFiniteElementNode, ForceVector> combined = new Dictionary<IFiniteElementNode, ForceVector>();

            foreach (var pair in gravity) {

                if (!wind.ContainsKey(pair.Key)) {
                    wind.Add(pair.Key, pair.Value);
                }
                else {
                    wind[pair.Key] = new ForceVector(
                        wind[pair.Key].X + pair.Value.X,
                        wind[pair.Key].Y + pair.Value.Y,
                        wind[pair.Key].Z + pair.Value.Z
                        );
                }
            }
            return wind;

        }

        private static Dictionary<IFiniteElementNode, ForceVector> AddWindLoad(PixelStructure structure, FiniteElementModel model) {
            Dictionary<IFiniteElementNode, ForceVector> map = new Dictionary<IFiniteElementNode, ForceVector>();


            if(structure.WindLoad != null) {
                if (structure.WindLoad.Activated) {

                    double forceX = structure.WindLoad.Direction.X;
                    double forceY = structure.WindLoad.Direction.Y;
                    foreach (var i in structure.WindLoad.NodeIndices) {

                        var node = model.Nodes.ElementAt(i);

                        if (!map.ContainsKey(node)) {
                            map.Add(node, new ForceVector(0,0,0));
                        }

                        map[node] = new ForceVector(
                            map[node].X + forceX,
                            0,
                            map[node].Z + forceY
                         );
                    }
                }
            }
            return map;
        }

        private static Dictionary<IFiniteElementNode, ForceVector> AddGravityLoad(PixelStructure structure, FiniteElementModel model) {
            Dictionary<IFiniteElementNode, ForceVector> map = new Dictionary<IFiniteElementNode, ForceVector>();

            if (structure.GravityLoad != null) {
                if (structure.GravityLoad.Activated) {
                    double amp = structure.GravityLoad.Amplification;
                    map = model.AddGravityLoad(amp);
                }
            }
            return map;

        }
    }
}

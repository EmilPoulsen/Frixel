using Frixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFE;

namespace Frixel.Optimizer
{
    public class FrixelAnalyzer
    {
        private PixelStructure _pixelStructure;

        public FrixelAnalyzer(PixelStructure pixelStructure) {
            _pixelStructure = pixelStructure;
        }
        
        public PixelStructure Analyze() {

            var model = BuildModel(_pixelStructure);



            return null;
        }


        public SharpFE.FiniteElementModel BuildModel(PixelStructure structure) {

            FiniteElementModel model = new FiniteElementModel(ModelType.Truss2D);
            IMaterial material = new GenericElasticMaterial(0, 70000000, 0, 0);
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



            return model;

        }

         
        
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpFE;

namespace Frixel.Test {
    [TestClass]
    public class SharpFeTest {
        [TestMethod]
        public void AnalyzeSimple2DFrame() {

            FiniteElementModel model = new FiniteElementModel(ModelType.Truss2D);

            //build geometric model and constraints

            FiniteElementNode node1 = model.NodeFactory.CreateFor2DTruss(0, 0);
            model.ConstrainNode(node1, DegreeOfFreedom.X);
            model.ConstrainNode(node1, DegreeOfFreedom.Z);

            FiniteElementNode node2 = model.NodeFactory.CreateFor2DTruss(0, 3);

            FiniteElementNode node3 = model.NodeFactory.CreateFor2DTruss(3, 0);

            FiniteElementNode node4 = model.NodeFactory.CreateFor2DTruss(3, 3);

            FiniteElementNode node5 = model.NodeFactory.CreateFor2DTruss(6, 0);
            model.ConstrainNode(node5, DegreeOfFreedom.Z);

            FiniteElementNode node6 = model.NodeFactory.CreateFor2DTruss(6, 3);

            IMaterial material = new GenericElasticMaterial(0, 70000000, 0, 0);
            ICrossSection section = new SolidRectangle(0.03, 0.01);

            LinearTruss truss1 = model.ElementFactory.CreateLinearTruss(node1, node2, material, section);
            LinearTruss truss2 = model.ElementFactory.CreateLinearTruss(node1, node3, material, section);
            LinearTruss truss3 = model.ElementFactory.CreateLinearTruss(node2, node3, material, section);
            LinearTruss truss4 = model.ElementFactory.CreateLinearTruss(node2, node4, material, section);
            LinearTruss truss5 = model.ElementFactory.CreateLinearTruss(node1, node4, material, section);
            LinearTruss truss6 = model.ElementFactory.CreateLinearTruss(node3, node4, material, section);
            LinearTruss truss7 = model.ElementFactory.CreateLinearTruss(node3, node6, material, section);
            LinearTruss truss8 = model.ElementFactory.CreateLinearTruss(node4, node5, material, section);
            LinearTruss truss9 = model.ElementFactory.CreateLinearTruss(node4, node6, material, section);
            LinearTruss truss10 = model.ElementFactory.CreateLinearTruss(node3, node5, material, section);
            LinearTruss truss11 = model.ElementFactory.CreateLinearTruss(node5, node6, material, section);

            //apply forces

            ForceVector force50Z = model.ForceFactory.CreateForTruss(0, -50000);
            model.ApplyForceToNode(force50Z, node2);
            model.ApplyForceToNode(force50Z, node6);

            ForceVector force100Z = model.ForceFactory.CreateForTruss(0, -100000);
            model.ApplyForceToNode(force100Z, node4);

            //solve model
            IFiniteElementSolver solver = new MatrixInversionLinearSolver(model);
            FiniteElementResults results = solver.Solve();

            //assert results

            ReactionVector reactionAtNode1 = results.GetReaction(node1);
            Assert.AreEqual(0, reactionAtNode1.X, 1);
            Assert.AreEqual(100000, reactionAtNode1.Z, 1);

            ReactionVector reactionAtNode5 = results.GetReaction(node1);
            Assert.AreEqual(0, reactionAtNode5.X, 1);
            Assert.AreEqual(100000, reactionAtNode5.Z, 1);

            DisplacementVector displacementAtNode2 = results.GetDisplacement(node2);
            Assert.AreEqual(7.1429, displacementAtNode2.X, 0.001);
            Assert.AreEqual(-9.0386, displacementAtNode2.Z, 0.001);

            DisplacementVector displacementAtNode3 = results.GetDisplacement(node3);
            Assert.AreEqual(5.2471, displacementAtNode3.X, 0.001);
            Assert.AreEqual(-16.2965, displacementAtNode3.Z, 0.001);

            DisplacementVector displacementAtNode4 = results.GetDisplacement(node4);
            Assert.AreEqual(5.2471, displacementAtNode4.X, 0.001);
            Assert.AreEqual(-20.0881, displacementAtNode4.Z, 0.001);

            DisplacementVector displacementAtNode5 = results.GetDisplacement(node5);
            Assert.AreEqual(10.4942, displacementAtNode5.X, 0.001);
            Assert.AreEqual(0, displacementAtNode5.Z, 0.001);

            DisplacementVector displacementAtNode6 = results.GetDisplacement(node6);
            Assert.AreEqual(3.3513, displacementAtNode6.X, 0.001);
            Assert.AreEqual(-9.0386, displacementAtNode6.Z, 0.001);




        }
    }
}

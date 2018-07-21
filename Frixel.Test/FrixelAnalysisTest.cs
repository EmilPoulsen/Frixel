using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frixel.Core;
using Frixel.Core.Geometry;
using System.Collections.Generic;
using Frixel.Optimizer;

namespace Frixel.Test {
    [TestClass]
    public class FrixelAnalysisTest {
        [TestMethod]
        public void AnalyzeFrixelFrame() {

            var pixelStructure = ModelCreator.CreatePixelStructure();
            
            FrixelAnalyzer analyzer = new FrixelAnalyzer();

            AnalysisResults results = analyzer.Analyze(pixelStructure);
            
        }

        [TestMethod]
        public void OptimizePixStruct() {

            var pixelStructure = ModelCreator.CreatePixelStructure();

            FrixelOptimizer optimizer = new FrixelOptimizer();

            optimizer.Optimize(pixelStructure);
        }

    }
}

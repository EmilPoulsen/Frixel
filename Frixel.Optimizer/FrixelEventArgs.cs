using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Analysis;

namespace Frixel.Optimizer {
    public class FrixelEventArgs : EventArgs {

        public AnalysisResults AnalysisResults { get; set; }

        public double Fitness { get; set; }

    }
}

using Frixel.Core.Analysis;
using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Optimizer {
    public class StructuralChromosome : ChromosomeBase {
        // Change the argument value passed to base construtor to change the length 
        // of your chromosome.
        int _numPixels;


        public AnalysisResults Results { get; set; }


        public StructuralChromosome(int numPixels)
            : base(numPixels) {

            _numPixels = numPixels;

            //int s = RandomizationProvider.Current.GetInt(0, 1);
            Random rand = new Random();


            for (int i = 0; i < numPixels; i++) {
                PixSwitch piswi = new PixSwitch();


                bool s = rand.Next(0, 2) == 0;

                piswi.Switch = s;
                ReplaceGene(i, new Gene(piswi));
            }

        }

        public override Gene GenerateGene(int geneIndex) {

            Random rand = new Random();


            PixSwitch piswi = new PixSwitch();
            bool s = rand.Next(0, 2) == 0;

            piswi.Switch = s;//s == 0 ? false : true;

            return new Gene(piswi);

        }

        public override IChromosome CreateNew() {
            return new StructuralChromosome(_numPixels);
        }
    }
}

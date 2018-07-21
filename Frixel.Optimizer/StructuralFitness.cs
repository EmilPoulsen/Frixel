using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFE;
using Frixel.Core;
using GeneticSharp.Domain.Randomizations;

namespace Frixel.Optimizer {


    public class PixSwitch {
        public PixSwitch() {

        }
        public bool Switch { get; set; }
        
        public int PixIndex { get; set; }
    }


    public class MyProblemChromosome : ChromosomeBase {
        // Change the argument value passed to base construtor to change the length 
        // of your chromosome.
        int _numPixels;

        public MyProblemChromosome(int numPixels) : base(numPixels) {

            _numPixels = numPixels;

            for (int i = 0; i < numPixels; i++) {
                PixSwitch piswi = new PixSwitch();

                int s = RandomizationProvider.Current.GetInt(0, 1);

                piswi.Switch = s == 0 ? false : true;
                ReplaceGene(i, new Gene(piswi));
            }

        }

        public override Gene GenerateGene(int geneIndex) {
            // Generate a gene base on my problem chromosome representation.
            //return new Gene(RandomizationProvider.Current.GetInt(0, _numPixels));

            PixSwitch piswi = new PixSwitch();
            int s = RandomizationProvider.Current.GetInt(0, 1);
            piswi.Switch = s == 0 ? false : true;

            return new Gene(piswi);

        }

        public override IChromosome CreateNew() {
            return new MyProblemChromosome(_numPixels);
        }
    }

    public class StructuralFitness : IFitness {
        FiniteElementModel _model;
        PixelStructure _structure;


        public StructuralFitness(FiniteElementModel model, PixelStructure structure) {
            _model = model;
            _structure = structure;
        }

        public double Evaluate(IChromosome chromosome) {
            

            var genes = chromosome.GetGenes();

            int i = 0;
            foreach (var g in genes) {
                var pixswi = g.Value as PixSwitch;

                var pixel = _structure.Pixels[i];


                if (pixel.LockedBrace) {

                }
                

                //int index = pixswi.PixIndex;
                
                //Convert.ToInt32(g.Value, CultureInfo.InvariantCulture);
                //distanceSum += CalcDistanceTwoCities(Cities[currentCityIndex], Cities[lastCityIndex]);
                //lastCityIndex = currentCityIndex;

                //citiesIndexes.Add(lastCityIndex);
            }


            return -1;
        }
    }
}

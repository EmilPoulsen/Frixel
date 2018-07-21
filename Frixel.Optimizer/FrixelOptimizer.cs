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


    public class FrixelOptimizer{
        
        public FrixelOptimizer() {

        }


        public void Optimize(PixelStructure structure) {

            var chromosome = new MyProblemChromosome(structure.Pixels.Count);

            var population = new Population(50, 100, chromosome);

            var fitness = new StructuralFitness(structure);

            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            var mutation = new FlipBitMutation();
            var termination = new FitnessStagnationTermination(100);

            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            ga.Termination = termination;

            ga.GenerationRan += GenerationRan;

            ga.Start();
            
        }

        private void GenerationRan(object sender, EventArgs e) {

            string s = "";
            
            //throw new NotImplementedException();
        }
    }


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
        //FiniteElementModel _model;
        PixelStructure _structure;


        public StructuralFitness(PixelStructure structure) {
            //_model = model;
            _structure = structure;
        }

        IMaterial _material = new GenericElasticMaterial(7700, 210.0e9, 0.3, 210.0e9 / 2.69);
        ICrossSection _section = new SolidRectangle(0.03, 0.01);


        public double Evaluate(IChromosome chromosome) {
            FrixelAnalyzer frixAnalyzer = new FrixelAnalyzer();

            var model = frixAnalyzer.BuildModel(_structure);
            
            var genes = chromosome.GetGenes();

            int i = 0;
            foreach (var g in genes) {
                var pixswi = g.Value as PixSwitch;

                var pixel = _structure.Pixels[i];
                
                if (!pixel.LockedBrace) {

                    if (pixswi.Switch) {

                        var bracing = pixel.GetBracing();
                        foreach (var brace in bracing) {
                            int s = brace.Start;
                            int e = brace.End;

                            var sNode = model.Nodes.ElementAt(s);
                            var eNode = model.Nodes.ElementAt(e);

                            model.ElementFactory.CreateLinearTruss(sNode, eNode, _material, _section);
                        }
                    }
                }
            }
            var results = frixAnalyzer.AnalyzeModel(model);

            double max = double.MinValue;

            foreach (var pair in results.NodeResults) {
                var res = pair.Value;

                double dist = Math.Sqrt(Math.Pow(res.DispX, 2) + Math.Pow(res.DispY, 2));

                if(dist > max) {
                    max = dist;
                }
            }

            //double weight = CalcWeight(model);
            return max;
        }
        
        public double CalcWeight(FiniteElementModel model) {
            double tot = 0;

            foreach (var elem in model.Elements) {
                tot += elem.Weight;
            }

            return tot;
        }
    }
}

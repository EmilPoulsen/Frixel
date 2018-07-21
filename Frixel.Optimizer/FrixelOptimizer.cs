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


    public class FrixelEventArgs : EventArgs {

        public AnalysisResults AnalysisResults { get; set; }

        public double Fitness { get; set; }

    }

    public class FrixelOptimizer {

        public event EventHandler RanIteration;

        protected virtual void OnRanIteration(EventArgs e) {
            if (RanIteration != null)
                RanIteration(this, e);
        }

        public FrixelOptimizer() {

        }


        public AnalysisResults Optimize(PixelStructure structure) {

            var chromosome = new MyProblemChromosome(structure.Pixels.Count);

            var population = new Population(5, 10, chromosome);

            var fitness = new StructuralFitness(structure);

            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            var mutation = new UniformMutation(); //FlipBitMutation();
            var termination = new FitnessStagnationTermination(10);

            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            ga.Termination = termination;

            ga.GenerationRan += GenerationRan;

            ga.Start();

            var structRes = ga.BestChromosome as MyProblemChromosome;

            return structRes.Results; 

        }

        private void GenerationRan(object sender, EventArgs e) {

            GeneticAlgorithm ga = sender as GeneticAlgorithm;

            var structFitness = ga.Fitness as StructuralFitness;

            
            var results = structFitness.LatestResults;
            
            FrixelEventArgs args = new FrixelEventArgs();
            args.AnalysisResults = results;
            args.Fitness = structFitness.CurrentFitness;

            OnRanIteration(args);

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


        public AnalysisResults Results { get; set; }


        public MyProblemChromosome(int numPixels) : base(numPixels) {

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
            return new MyProblemChromosome(_numPixels);
        }
        
        //public void FlipGene(int index) {
        //    //throw new NotImplementedException();
        //}
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

                //// HACK //////
                Random rand = new Random();
                bool lol = rand.Next(0, 2) == 0;
                pixswi.Switch = lol;
                ///// HACK //////

                if (!pixel.LockedBrace) {

                    if (pixswi.Switch) {
                        
                        var x1 = new Edge(pixel.TopLeft, pixel.BottomRight);
                        var x2 = new Edge(pixel.TopRight, pixel.BottomLeft);
                        var bracing = new List<Edge>() { x1, x2 };
                        
                        foreach (var brace in bracing) {
                            int s = brace.Start;
                            int e = brace.End;

                            var sNode = model.Nodes.ElementAt(s);
                            var eNode = model.Nodes.ElementAt(e);

                            model.ElementFactory.CreateLinearTruss(sNode, eNode, _material, _section);
                        }
                    }
                }
                i++;
            }
            var results = frixAnalyzer.AnalyzeModel(model);

            i = 0;
            foreach (var g in genes) {
                var pixswi = g.Value as PixSwitch;
                var pixel = _structure.Pixels[i];
                PixelResult pixResult = new PixelResult();



                if (pixel.LockedBrace) {
                    pixResult.IsBraced = true;
                }
                else {
                    pixResult.IsBraced = pixswi.Switch;
                }

                results.PixelResults.Add(i, pixResult);
                i++;
            }
            
            this.LatestResults = results;
            double max = double.MinValue;

            (chromosome as MyProblemChromosome).Results = results;

            foreach (var pair in results.NodeResults) {
                var res = pair.Value;

                double dist = Math.Sqrt(Math.Pow(res.DispX, 2) + Math.Pow(res.DispY, 2));

                if (dist > max) {
                    max = dist;
                }
            }
            this.CurrentFitness = max;

            //double weight = CalcWeight(model);
            return max;
        }

        public AnalysisResults LatestResults { get; set; }
        public double CurrentFitness { get; private set; }

        public double CalcWeight(FiniteElementModel model) {
            double tot = 0;

            foreach (var elem in model.Elements) {
                tot += elem.Weight;
            }

            return tot;
        }
    }
}

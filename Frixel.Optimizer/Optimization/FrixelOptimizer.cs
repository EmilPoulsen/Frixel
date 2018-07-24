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
using Frixel.Core.Analysis;
using Frixel.Core.Loading;

namespace Frixel.Optimizer {


    public class FrixelOptimizer {

        public event EventHandler RanIteration;

        protected virtual void OnRanIteration(EventArgs e) {
            if (RanIteration != null)
                RanIteration(this, e);
        }

        public FrixelOptimizer() {

        }


        public AnalysisResults Optimize(PixelStructure structure) {

            var chromosome = new StructuralChromosome(structure.Pixels.Count);

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

            var structRes = ga.BestChromosome as StructuralChromosome;

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
}

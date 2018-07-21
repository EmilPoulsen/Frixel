using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Terminations;

namespace Frixel.Test {
    [TestClass]
    public class GeneticSharpTests {
        [TestMethod]
        public void TestMethod1() {

            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new MyProblemFitness();
            var chromosome = new MyProblemChromosome();
            var population = new Population(50, 70, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            
            ga.Termination = new GenerationNumberTermination(100);

            Console.WriteLine("GA running...");
            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
        }

        public class MyProblemChromosome : ChromosomeBase {
            // Change the argument value passed to base construtor to change the length 
            // of your chromosome.
            public MyProblemChromosome() : base(10) {
                CreateGenes();
            }

            public override Gene GenerateGene(int geneIndex) {
                // Generate a gene base on my problem chromosome representation.
                
                return default(Gene);
            }

            public override IChromosome CreateNew() {
                return new MyProblemChromosome();
            }
        }

        public class MyProblemFitness : IFitness {
            public double Evaluate(IChromosome chromosome) {
                // Evaluate the fitness of chromosome.

                return -1;
            }
        }


    }
}

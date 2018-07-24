using Frixel.Core;
using Frixel.Core.Analysis;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using SharpFE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Optimizer {
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
            Random rand = new Random();
            int i = 0;
            foreach (var g in genes) {
                var pixswi = g.Value as PixSwitch;

                var pixel = _structure.Pixels[i];

                //// HACK //////
                //bool lol = rand.Next(0, 2) == 0;
                //pixswi.Switch = lol;
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
                } else {
                    pixResult.IsBraced = pixswi.Switch;
                }

                results.PixelResults.Add(i, pixResult);
                i++;
            }

            this.LatestResults = results;
            (chromosome as StructuralChromosome).Results = results;


            //double max = double.MinValue;
            //foreach (var pair in results.NodeResults) {
            //    var res = pair.Value;

            //    double dist = Math.Sqrt(Math.Pow(res.DispX, 2) + Math.Pow(res.DispY, 2));

            //    if (dist > max) {
            //        max = dist;
            //    }
            //}

            double energy = results.ElasticEnergy;
            double fitness = 0;
            if (energy > 0) {
                fitness = 1 / energy;                
            }

            this.CurrentFitness = fitness;

            //double weight = CalcWeight(model);
            return fitness;
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

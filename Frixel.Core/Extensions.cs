﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Extensions
{
    public static class ExtensionMethods
    {
        public static double Map(this double number, Domain from, Domain to)
        {
            return (number - from.Min) / (from.Max - from.Min) * (to.Max - to.Min) + to.Min;
        }

        public static double MapRound(this double number, Domain from, Domain to)
        {
            return Math.Round((number - from.Min) / (from.Max - from.Min) * (to.Max - to.Min) + to.Min);
        }  

        public static bool IsCloseTo(this double number, double to)
        {
            return number - to < 0.01;
        }

        public static double RoundTo(this double number, int decPlaces)
        {
            double multiplier = Math.Pow(10, decPlaces);
            return Math.Round(number * decPlaces) / decPlaces;
        }
    }
}

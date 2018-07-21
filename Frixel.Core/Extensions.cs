using System;
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

        public static double MapRound(this double value, Domain from, Domain to)
        {
            return Math.Round((value - from.Min) / (to.Min - from.Min) * (to.Max - from.Max) + from.Max);
        }
    }
}

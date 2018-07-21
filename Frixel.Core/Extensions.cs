using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frixel.Core.Extensions
{
    public static class ExtensionMethods
    {
        public static double Map(this double value, Domain from, Domain to)
        {
            return (value - from.Min) / (to.Min - from.Min) * (to.Max - from.Max) + from.Max;
        }
    }
}

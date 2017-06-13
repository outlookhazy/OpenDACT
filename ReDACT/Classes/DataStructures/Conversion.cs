using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ReDACT.Classes.DataStructures
{
    class Conversion
    {
        public static double scale(double value, double frommin, double frommax, double tomin, double tomax)
        {
            double fromrange = frommax - frommin;
            double torange = tomax - tomin;

            return (((value - frommin) * torange) / fromrange) + tomin;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    class Validation
    {
        public static float CheckZero(float value)
        {
            if(Math.Abs(value) < 0.001F)            
                return 0;            
            else            
                return value;            
        }
    }
}

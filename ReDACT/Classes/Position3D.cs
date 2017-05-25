using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files
{
    public class Position3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Position3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return String.Format("[X{0}, Y{1}, Z{2}]", X, Y, Z);
        }
    }
}

using OpenDACT.Class_Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes
{
    public class HeightMap : List<Position3D>
    {
        public HeightMap()
        {

        }
        public HeightMap(double[,] fromArray2D)
        {
            for (int i = 0; i < fromArray2D.GetLength(0); i++)
            {
                this.Add(new Position3D(fromArray2D[i, 0], fromArray2D[i, 1], fromArray2D[i, 2]));
            }
            int points = fromArray2D.GetLength(0);
        }

        public static HeightMap FromArray2D(double[,] fromArray2D)
        {
            return new HeightMap(fromArray2D);
        }

        public void Normalize()
        {
            for(int i=0; i< this.Count -1; i++)
            {
                this[i].Z = this[Count - 1].Z - this[i].Z;
            }            
        }

        public double[,] ToArray2D()
        {
            double[,] outArray = new double[this.Count, 3];
            for(int i=0; i< this.Count; i++)
            {
                outArray[i, 0] = this.ElementAt(i).X;
                outArray[i, 1] = this.ElementAt(i).Y;
                outArray[i, 2] = this.ElementAt(i).Z;
            }
            return outArray;
        }
    }
}

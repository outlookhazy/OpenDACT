using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReDACT.Escher;

namespace ReDACT.Classes
{
    public class TestData
    {
        public int NumPoints { get; private set; }
        public NumFactors NumFactors { get; private set; }
        public double[,] TestPoints { get; private set; }
        public bool Normalize { get; private set; }

        public double BedRadius { get; private set; }

        public TestData(int numpoints, NumFactors numfactors, double bedRadius, double[,] testpoints, bool normalize)
        {
            if ((int)numfactors > numpoints)
                throw new Exception("Error: need at least as many points as factors you want to calibrate");
            this.NumPoints = numpoints;
            this.NumFactors = numfactors;
            this.BedRadius = bedRadius;
            this.TestPoints = testpoints;
            this.Normalize = normalize;
        }

        public static double[,] CalcProbePoints(int points, double radius)
        {
            int numPoints = points;
            double bedRadius = radius;
            double[,] testPoints = new double[numPoints, 2];

            if (numPoints == 4)
            {
                for (var i = 0; i < 3; ++i)
                {
                    testPoints[i, 0] = (bedRadius * Math.Sin((2 * Math.PI * i) / 3)); //X
                    testPoints[i, 1] = (bedRadius * Math.Cos((2 * Math.PI * i) / 3)); //Y
                }
                testPoints[3, 0] = 0.0;//Center X
                testPoints[3, 1] = 0.0;//Center Y
            }
            else
            {
                if (numPoints >= 7)
                {
                    for (var i = 0; i < 6; ++i)
                    {
                        testPoints[i, 0] = (bedRadius * Math.Sin((2 * Math.PI * i) / 6)); //X
                        testPoints[i, 1] = (bedRadius * Math.Cos((2 * Math.PI * i) / 6)); //Y                
                    }
                }
                if (numPoints >= 10)
                {
                    for (var i = 6; i < 9; ++i)
                    {
                        testPoints[i, 0] = (bedRadius / 2 * Math.Sin((2 * Math.PI * (i - 6)) / 3)); //X
                        testPoints[i, 1] = (bedRadius / 2 * Math.Cos((2 * Math.PI * (i - 6)) / 3)); //Y                 
                    }
                    testPoints[9, 0] = 0.0;//Center X
                    testPoints[9, 1] = 0.0;//Center Y
                }
                else
                {
                    testPoints[6, 0] = 0.0;//Center X
                    testPoints[6, 1] = 0.0;//Center Y
                }
            }
            return testPoints;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReDACT.Classes.Escher.DParameters;

namespace ReDACT.Classes.Escher
{
    public class TParameters
    {
        internal int numFactors;
        internal int numPoints;
        internal double[] xBedProbePoints;
        internal double[] yBedProbePoints;
        internal double[] zBedProbePoints;
        internal bool normalise;
        internal Firmware firmware;
        internal static bool overfitPrevention = true;
        internal static Random randomSource = new Random(Guid.NewGuid().GetHashCode());

        internal double probeHeight;

        public TParameters(Firmware firmware, int numpoints, int numfactors, bool normal)
        {
            this.firmware = firmware;
            this.numPoints = numpoints;
            this.numFactors = numfactors;
            this.normalise = normal;
        }

        public void calcProbePoints(double bedRadius)
        {
            xBedProbePoints = new double[numPoints];
            yBedProbePoints = new double[numPoints];
            zBedProbePoints = new double[numPoints];
            double outsideRadius = bedRadius*.8;
            double middleRadius = (2 / 3.0) * outsideRadius;
            double insideRadius = (1 / 3.0) * outsideRadius;
            int numRingPoints = numPoints - 1;

            bool inside = false;
            bool middle = false;            

            if (numRingPoints > 6)
            {
                middle = true;
                if (numRingPoints > 12)
                    inside = true;                
            }

            int insidePoints = 0;
            int middlePoints = 0;

            if (middle)
            {
                if (inside)
                {
                    insidePoints = (int)((1 / 6.0) * numRingPoints);
                    middlePoints = (int)((1 / 3.0) * numRingPoints);
                }
                else
                {
                    middlePoints = (int)(numRingPoints/12 + (numRingPoints % 6));
                }
            }


            int index = 0;

            int outsidePoints = numRingPoints - insidePoints - middlePoints;
            double[,] outerRing = calcPoints(outsideRadius, outsidePoints);
            for (int i = 0; i < outsidePoints; i++)
            {
                xBedProbePoints[index] = outerRing[i, 0];
                yBedProbePoints[index] = outerRing[i, 1];
                zBedProbePoints[index] = 0;
                index++;
            }

            if (middle)
            {
                double[,] middleRing = calcPoints(middleRadius, middlePoints, 45);
                for (int i = 0; i < middlePoints; i++)
                {
                    xBedProbePoints[index] = middleRing[i, 0];
                    yBedProbePoints[index] = middleRing[i, 1];
                    zBedProbePoints[index] = 0;
                    index++;
                }
            }

            if (inside)
            {
                double[,] innerRing = calcPoints(insideRadius, insidePoints);
                for (int i = 0; i < insidePoints; i++)
                {
                    xBedProbePoints[index] = innerRing[i, 0];
                    yBedProbePoints[index] = innerRing[i, 1];
                    zBedProbePoints[index] = 0;
                    index++;
                }
            }

            //center
            xBedProbePoints[index] = 0;
            yBedProbePoints[index] = 0;
            zBedProbePoints[index] = 0;
        }

        public static double[,] calcPoints(double radius, int number, double offsetDegrees = 0)
        {
            double[,] points = new double[number,2];
            double degreesPerPoint = 360 / number;

            if (overfitPrevention)
            {
                offsetDegrees += randomSource.NextDouble() * 360.0;
            }

            for (int i = 0; i < number; i++)
            {
                double degrees = (i * degreesPerPoint) + offsetDegrees;
                points[i,0] = PointOnCircleX(radius, degrees);
                points[i,1] = PointOnCircleY(radius, degrees);
            }
            return points;
        }

        public static double PointOnCircleX(double radius, double angleInDegrees)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            double x = (double)(radius * Math.Cos(angleInDegrees * Math.PI / 180F));
            return x;
        }

        public static double PointOnCircleY(double radius, double angleInDegrees)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            double y = (double)(radius * Math.Sin(angleInDegrees * Math.PI / 180F));
            return y;
        }

        public void calcProbePointsOriginal(double bedRadius)
        {
            xBedProbePoints = new double[numPoints];
            yBedProbePoints = new double[numPoints];
            zBedProbePoints = new double[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                zBedProbePoints[i] = 0.0;
            }

            if (numPoints == 4)
            {
                for (var i = 0; i < 3; ++i)
                {
                    xBedProbePoints[i] = (bedRadius * Math.Sin((2 * Math.PI * i) / 3));
                    yBedProbePoints[i] = (bedRadius * Math.Cos((2 * Math.PI * i) / 3));
                }
                xBedProbePoints[3] = 0.0;
                yBedProbePoints[3] = 0.0;
            }
            else
            {
                if (numPoints >= 7)
                {
                    for (var i = 0; i < 6; ++i)
                    {
                        xBedProbePoints[i] = (bedRadius * Math.Sin((2 * Math.PI * i) / 6));
                        yBedProbePoints[i] = (bedRadius * Math.Cos((2 * Math.PI * i) / 6));
                    }
                }
                if (numPoints >= 10)
                {
                    for (var i = 6; i < 9; ++i)
                    {
                        xBedProbePoints[i] = (bedRadius / 2 * Math.Sin((2 * Math.PI * (i - 6)) / 3));
                        yBedProbePoints[i] = (bedRadius / 2 * Math.Cos((2 * Math.PI * (i - 6)) / 3));
                    }
                    xBedProbePoints[9] = 0.0;
                    yBedProbePoints[9] = 0.0;
                }
                else
                {
                    xBedProbePoints[6] = 0.0;
                    yBedProbePoints[6] = 0.0;
                }
            }
        }

        internal void NormalizeProbePoints(double? withValue = null)
        {
            int count = this.zBedProbePoints.Length;

            double zeroHeight = (withValue == null) ? this.zBedProbePoints[count - 1] : (double)withValue;

            for(int i=0; i<count; i++)
            {
                this.zBedProbePoints[i] = zeroHeight - this.zBedProbePoints[i];
            }
        }
    }
}

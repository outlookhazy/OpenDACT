using System;

namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    public class Parameters
    {
        internal int NumFactors;
        internal int NumPoints;
        internal double[] XBedProbePoints;
        internal double[] YBedProbePoints;
        internal double[] ZBedProbePoints;
        internal bool Normalise;
        internal DParameters.FirmwareType Firmware;
        internal static bool OverfitPrevention = false;
        internal static Random RandomSource = new Random(Guid.NewGuid().GetHashCode());

        public Parameters(DParameters.FirmwareType firmware, int numpoints, int numfactors, bool normal)
        {
            Firmware = firmware;
            NumPoints = numpoints;
            NumFactors = numfactors;
            Normalise = normal;
        }

        public void CalcProbePoints(double bedRadius)
        {
            XBedProbePoints = new double[NumPoints];
            YBedProbePoints = new double[NumPoints];
            ZBedProbePoints = new double[NumPoints];
            double outsideRadius = bedRadius*.8;
            double middleRadius = 2 / 3.0 * outsideRadius;
            double insideRadius = 1 / 3.0 * outsideRadius;
            int numRingPoints = NumPoints - 1;

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
                    insidePoints = (int)(1 / 6.0 * numRingPoints);
                    middlePoints = (int)(1 / 3.0 * numRingPoints);
                }
                else
                {
                    middlePoints = numRingPoints/12 + numRingPoints % 6;
                }
            }


            int index = 0;

            int outsidePoints = numRingPoints - insidePoints - middlePoints;
            double[,] outerRing = CalcPoints(outsideRadius, outsidePoints);
            for (int i = 0; i < outsidePoints; i++)
            {
                XBedProbePoints[index] = outerRing[i, 0];
                YBedProbePoints[index] = outerRing[i, 1];
                ZBedProbePoints[index] = 0;
                index++;
            }

            if (middle)
            {
                double[,] middleRing = CalcPoints(middleRadius, middlePoints, 45);
                for (int i = 0; i < middlePoints; i++)
                {
                    XBedProbePoints[index] = middleRing[i, 0];
                    YBedProbePoints[index] = middleRing[i, 1];
                    ZBedProbePoints[index] = 0;
                    index++;
                }
            }

            if (inside)
            {
                double[,] innerRing = CalcPoints(insideRadius, insidePoints);
                for (int i = 0; i < insidePoints; i++)
                {
                    XBedProbePoints[index] = innerRing[i, 0];
                    YBedProbePoints[index] = innerRing[i, 1];
                    ZBedProbePoints[index] = 0;
                    index++;
                }
            }

            //center
            XBedProbePoints[index] = 0;
            YBedProbePoints[index] = 0;
            ZBedProbePoints[index] = 0;
        }

        public static double[,] CalcPoints(double radius, int number, double offsetDegrees = 0)
        {
            double[,] points = new double[number,2];
            double degreesPerPoint = 360.0 / number;

            if (OverfitPrevention)
            {
                offsetDegrees += RandomSource.NextDouble() * 360.0;
            }

            for (int i = 0; i < number; i++)
            {
                double degrees = i * degreesPerPoint + offsetDegrees;
                points[i,0] = PointOnCircleX(radius, degrees);
                points[i,1] = PointOnCircleY(radius, degrees);
            }
            return points;
        }

        public static double PointOnCircleX(double radius, double angleInDegrees)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            double x = radius * Math.Cos(angleInDegrees * Math.PI / 180F);
            return x;
        }

        public static double PointOnCircleY(double radius, double angleInDegrees)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            double y = radius * Math.Sin(angleInDegrees * Math.PI / 180F);
            return y;
        }

        public void CalcProbePointsOriginal(double bedRadius)
        {
            XBedProbePoints = new double[NumPoints];
            YBedProbePoints = new double[NumPoints];
            ZBedProbePoints = new double[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                ZBedProbePoints[i] = 0.0;
            }

            if (NumPoints == 4)
            {
                for (int i = 0; i < 3; ++i)
                {
                    XBedProbePoints[i] = bedRadius * Math.Sin(2 * Math.PI * i / 3);
                    YBedProbePoints[i] = bedRadius * Math.Cos(2 * Math.PI * i / 3);
                }
                XBedProbePoints[3] = 0.0;
                YBedProbePoints[3] = 0.0;
            }
            else
            {
                if (NumPoints >= 7)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        XBedProbePoints[i] = bedRadius * Math.Sin(2 * Math.PI * i / 6);
                        YBedProbePoints[i] = bedRadius * Math.Cos(2 * Math.PI * i / 6);
                    }
                }
                if (NumPoints >= 10)
                {
                    for (int i = 6; i < 9; ++i)
                    {
                        XBedProbePoints[i] = bedRadius / 2 * Math.Sin(2 * Math.PI * (i - 6) / 3);
                        YBedProbePoints[i] = bedRadius / 2 * Math.Cos(2 * Math.PI * (i - 6) / 3);
                    }
                    XBedProbePoints[9] = 0.0;
                    YBedProbePoints[9] = 0.0;
                }
                else
                {
                    XBedProbePoints[6] = 0.0;
                    YBedProbePoints[6] = 0.0;
                }
            }
        }

        internal void NormalizeProbePoints(double? withValue = null)
        {
            int count = ZBedProbePoints.Length;

            double zeroHeight = withValue == null ? ZBedProbePoints[count - 1] : (double)withValue;

            for(int i=0; i<count; i++)
            {
                ZBedProbePoints[i] = zeroHeight - ZBedProbePoints[i];
            }
        }
    }
}

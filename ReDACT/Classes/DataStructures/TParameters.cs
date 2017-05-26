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

            for (int i = 0; i < numPoints - 1; i++)
            {
                if (i % 2 == 0)//even
                {
                    xBedProbePoints[i] = PointOnCircleX(bedRadius, i * (360.0 / (numPoints - 1)) - 180);
                    yBedProbePoints[i] = PointOnCircleY(bedRadius, i * (360.0 / (numPoints - 1)) - 180);
                } else//odd
                {
                    xBedProbePoints[i] = PointOnCircleX(bedRadius/2, i * (360.0 / (numPoints - 1)) - 180);
                    yBedProbePoints[i] = PointOnCircleY(bedRadius/2, i * (360.0 / (numPoints - 1)) - 180);
                }
                zBedProbePoints[i] = 0;
            }            

            xBedProbePoints[numPoints - 1] = 0;
            yBedProbePoints[numPoints - 1] = 0;
            zBedProbePoints[numPoints - 1] = 0;

            Debug.WriteLine("Done");
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

        public void calcProbePointsX(double bedRadius)
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

        internal void NormalizeProbePoints()
        {
            int count = this.zBedProbePoints.Length;

            double zeroHeight = this.zBedProbePoints[count - 1];

            for(int i=0; i<count; i++)
            {
                this.zBedProbePoints[i] = zeroHeight - this.zBedProbePoints[i];
            }
        }
    }
}

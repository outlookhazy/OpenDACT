using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes.Data
{
    static class Stats
    {
        public static double StdDev(double[] array) {
            return Math.Sqrt(
                Mean(
                    Square(
                        Delta(array, Mean(array)))));
        }

        public static double[] Delta(double[] array, double value)
        {
            double[] delta = new double[array.Length];

            for (int i = 0; i < array.Length; i++)
                delta[i] = array[i] - value;

            return delta;
        }

        public static double Mean(double[] array)
        {            
            return Sum(array) / array.Length;
        }

        public static double RMS(double[] array)
        {
            return Math.Sqrt(Mean(Square(array)));
        }

        public static double Sum(double[] array)
        {
            double sum = 0;
            foreach (double element in array)
                sum += element;
            return sum;
        }

        public static double[] Multiply(double[] array1, double[] array2)
        {
            double[] result = new double[array1.Length];

            for (int i = 0; i < array1.Length; i++)
                result[i] = array1[i] * array2[i];

            return result;
        }

        public static double[] Square(double[] array)
        {
            return Multiply(array, array);
        }

        public static LineDefinition SimpleLinearRegression(double[] x, double[] y)
        {
            int N = x.Length;

            double sumX = Sum(x);
            double sumY = Sum(y);

            double sumXY = Sum(Multiply(x, y));

            double sumXsq = Sum(Multiply(x, x));

            double slope = (((N * sumXY) - (sumX * sumY)) / (N * sumXsq - Math.Pow(sumX,2)));

            double intercept = (sumY - (slope * sumX)) / N;

            return new LineDefinition(slope, intercept);
        }

        public class LineDefinition
        {
            public double Slope { get; private set; }
            public double Intercept { get; private set; }

            public LineDefinition(double slope, double intercept)
            {
                this.Slope = slope;
                this.Intercept = intercept;
            }

            public double ValueAt(double X)
            {
                return ((Slope * X) + Intercept);
            }
        }
    }
}

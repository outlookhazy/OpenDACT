using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes.Escher
{
    public class CalibrationResult
    {
        public bool Success { get; private set; }
        public int Factors { get; private set; }
        public int Points { get; private set; }
        public double DeviationBefore { get; private set; }
        public double DeviationAfter { get; private set; }

        public CalibrationResult(int factors, int points, double initialSumOfSquares, double expectedRmsError)
        {
            this.Success = !(double.IsNaN(initialSumOfSquares) || double.IsNaN(expectedRmsError));
            this.Factors = factors;
            this.Points = points;
            this.DeviationBefore = Math.Sqrt(initialSumOfSquares / Convert.ToDouble(Points));
            this.DeviationAfter = expectedRmsError;

        }

        public override string ToString()
        {
            return String.Format("Calibrated {0} factors using {1} points, deviation before {2} after {3}", Factors, Points, DeviationBefore, DeviationAfter);
        }
    }
}

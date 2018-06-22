using System;

namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    public class CalibrationResult
    {
        public bool Success { get; }
        public int Factors { get; }
        public int Points { get; }
        public double DeviationBefore { get; }
        public double DeviationAfter { get; }

        public CalibrationResult(int factors, int points, double initialSumOfSquares, double expectedRmsError)
        {
            Success = !(double.IsNaN(initialSumOfSquares) || double.IsNaN(expectedRmsError));
            Factors = factors;
            Points = points;
            DeviationBefore = Math.Sqrt(initialSumOfSquares / Convert.ToDouble(Points));
            DeviationAfter = expectedRmsError;

        }

        public override string ToString()
        {
            return
                $"Calibrated {Factors} factors using {Points} points, deviation before {DeviationBefore} after {DeviationAfter}";
        }
    }
}

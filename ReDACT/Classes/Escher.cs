using ReDACT.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReDACT.Classes.DeltaParameters;

namespace ReDACT
{
    public static class Escher
    {
        // Delta calibration script

        public static double degreesToRadians = Math.PI / 180.0;

        public static DeltaParameters Calc(DeltaParameters delta, TestData data)
        {
            DeltaParameters incoming = DeltaParameters.ConvertIncomingEndstops(delta);

            DeltaParameters rslt = DoDeltaCalibration(incoming, data);

            DeltaParameters outconverted = DeltaParameters.ConvertOutgoingEndstops(rslt);
            
            DeltaParameters newparams = DeltaParameters.NewParameters(outconverted);

            return newparams;
        }

        public static DeltaParameters DoDeltaCalibration(DeltaParameters delta, TestData data)
        {

            // Transform the probing points to motor endpoints and store them in a matrix, so that we can do multiple iterations using the same data
            var probeMotorPositions = new Matrix(data.numpoints, 3);
            var corrections = new double[data.numpoints];
            var initialSumOfSquares = 0.0;
            for (var i = 0; i < data.numpoints; ++i)
            {
                corrections[i] = 0.0;
                double[] machinePos = new double[3];
                var xp = data.testpoints[i, 0];
                var yp = data.testpoints[i, 1];
                machinePos[0] = (xp);
                machinePos[1] = (yp);
                machinePos[2] = (0.0);

                probeMotorPositions.matrixData[i, 0] = DeltaParameters.Transform(delta, machinePos, 0);
                probeMotorPositions.matrixData[i, 1] = DeltaParameters.Transform(delta, machinePos, 1);
                probeMotorPositions.matrixData[i, 2] = DeltaParameters.Transform(delta, machinePos, 2);

                initialSumOfSquares += Math.Pow(data.testpoints[i, 2], 2);
            }

            // Do 1 or more Newton-Raphson iterations
            var iteration = 0;
            double expectedRmsError;

            DeltaParameters adjusted = delta;
            for (;;)
            {
                // Build a Nx7 matrix of derivatives with respect to xa, xb, yc, za, zb, zc, diagonal.
                var derivativeMatrix = new Matrix(data.numpoints, (int)data.numfactors);
                for (var i = 0; i < data.numpoints; ++i)
                {
                    for (var j = 0; j < (int)data.numfactors; ++j)
                    {
                        derivativeMatrix.matrixData[i, j] =
                            DeltaParameters.ComputeDerivative(adjusted, j, probeMotorPositions.matrixData[i, 0], probeMotorPositions.matrixData[i, 1], probeMotorPositions.matrixData[i, 2]);
                    }
                }


                // Now build the normal equations for least squares fitting
                var normalMatrix = new Matrix((int)data.numfactors, (int)data.numfactors + 1);
                for (var i = 0; i < (int)data.numfactors; ++i)
                {
                    for (var j = 0; j < (int)data.numfactors; ++j)
                    {
                        var tmp = derivativeMatrix.matrixData[0, i] * derivativeMatrix.matrixData[0, j];
                        for (var k = 1; k < data.numpoints; ++k)
                        {
                            tmp += derivativeMatrix.matrixData[k, i] * derivativeMatrix.matrixData[k, j];
                        }
                        normalMatrix.matrixData[i, j] = tmp;
                    }
                    var temp = derivativeMatrix.matrixData[0, i] * -(data.testpoints[0, 2] + corrections[0]);
                    for (var k = 1; k < data.numpoints; ++k)
                    {
                        temp += derivativeMatrix.matrixData[k, i] * -(data.testpoints[k, 2] + corrections[k]);
                    }
                    normalMatrix.matrixData[i, (int)data.numfactors] = temp;
                }

                double[] solution = new double[(int)data.numfactors];
                normalMatrix.GaussJordan(ref solution, (int)data.numfactors);

                /*
                for (var i = 0; i < numFactors; ++i)
                {
                    if (isNaN(solution[i]))
                    {
                        throw "Unable to calculate corrections. Please make sure the bed probe points are all distinct.";
                    }
                }
                */



                adjusted = DeltaParameters.Adjust(adjusted, (int)data.numfactors, solution, data.normalize);

                // Calculate the expected probe heights using the new parameters

                var expectedResiduals = new double[data.numpoints];
                var sumOfSquares = 0.0;
                for (var i = 0; i < data.numpoints; ++i)
                {
                    for (var axis = 0; axis < 3; ++axis)
                    {
                        probeMotorPositions.matrixData[i, axis] += solution[axis];
                    }
                    var newZ = DeltaParameters.InverseTransform(adjusted, probeMotorPositions.matrixData[i, 0], probeMotorPositions.matrixData[i, 1], probeMotorPositions.matrixData[i, 2]);
                    corrections[i] = newZ;
                    expectedResiduals[i] = data.testpoints[i, 2] + newZ;
                    sumOfSquares += Math.Pow(expectedResiduals[i], 2);
                }

                expectedRmsError = Math.Sqrt(sumOfSquares / data.numpoints);
                //DebugPrint(PrintVector("Expected probe error", expectedResiduals));


                // Decide whether to do another iteration Two is slightly better than one, but three doesn't improve things.
                // Alternatively, we could stop when the expected RMS error is only slightly worse than the RMS of the residuals.
                ++iteration;
                if (iteration == 2) { break; }
            }


            Console.WriteLine( "Calibrated " + data.numfactors + " factors using " + data.numpoints + " points, deviation before " + Math.Sqrt(initialSumOfSquares / data.numpoints)
                    + " after " + expectedRmsError);

            return adjusted;
        }

        public static string generateCommands(DeltaParameters delta, TestData test)
        {
            Debug.WriteLine("Generate Commands");
            var m665 = "M665 R" + Math.Round(delta.radius, 2) + " L" + Math.Round(delta.diagonal, 2);
            var m666 = "M666 X" + Math.Round(delta.xstop, 2) + " Y" + Math.Round(delta.ystop, 2) + " Z" + Math.Round(delta.zstop, 2);
            switch (delta.firmware)
            {
                case Firmware.RRF:
                    m665 += " H" + Math.Round(delta.homedHeight, 2) + " B" + Math.Round(test.bedRadius, 2)
                            + " X" + Math.Round(delta.xadj, 2) + " Y" + Math.Round(delta.yadj, 2) + " Z" + Math.Round(delta.zadj, 2);
                    break;
                case Firmware.Marlin:
                    break;
                case Firmware.MarlinRC:
                    m666 += " R" + Math.Round(delta.radius, 2) + " D" + Math.Round(delta.diagonal, 2) + " H" + Math.Round(delta.homedHeight, 2)
                            + " A" + Math.Round(delta.xadj, 2) + " B" + Math.Round(delta.yadj, 2) + " C" + Math.Round(delta.zadj, 2);
                    break;
                case Firmware.Repetier:
                    break;
                case Firmware.Smoothieware:
                    m665 += " D" + Math.Round(delta.xadj, 2) + " E" + Math.Round(delta.yadj, 2) + " H" + Math.Round(delta.zadj, 2) + " Z" + Math.Round(delta.homedHeight, 2);
                    break;
            }
            string commands = "";
            if (delta.firmware != Firmware.MarlinRC)
            {
                commands += m665 + "\n";
            }
            commands += m666;
            if (delta.firmware == Firmware.Marlin)
            {
                commands += "\n; Set homed height " + Math.Round(delta.homedHeight, 2) + "mm in config.h";
            }
            return commands;
        }

        public enum NumFactors
        {
            THREE = 3,
            FOUR = 4,
            SIX = 6,
            SEVEN = 7
        }
    }
}
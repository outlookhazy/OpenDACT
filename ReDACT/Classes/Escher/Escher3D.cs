using ReDACT.Classes.Escher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReDACT.Classes.Escher.DParameters;

namespace ReDACT.Classes
{
    public static class Escher3D
    {
        static bool debug = false;
        static string _debug;
        static string EscherDebug { get { return _debug; } set { Escher3D._debug = value; Debug.WriteLine(value); } }
        public const double degreesToRadians = Math.PI / 180.0;

        public static double fsquare(double value)
        {
            return Math.Pow(value, 2.0);
        }    
        
        public static double Transform(double[] machinePos, int axis, ref DParameters machine)
        {
            return machinePos[2] + Math.Sqrt(machine.D2 - fsquare(machinePos[0] - machine.towerX[axis]) - fsquare(machinePos[1] - machine.towerY[axis]));
        }

        public static double InverseTransform(double Ha, double Hb, double Hc, DParameters machine)
        {
            double Fa = machine.coreFa + fsquare(Ha);
            double Fb = machine.coreFb + fsquare(Hb);
            double Fc = machine.coreFc + fsquare(Hc);

            // Setup PQRSU such that x = -(S - uz)/P, y = (P - Rz)/Q
            double P = (machine.Xbc * Fa) + (machine.Xca * Fb) + (machine.Xab * Fc);
            double S = (machine.Ybc * Fa) + (machine.Yca * Fb) + (machine.Yab * Fc);

            double R = 2.0 * ((machine.Xbc * Ha) + (machine.Xca * Hb) + (machine.Xab * Hc));
            double U = 2.0 * ((machine.Ybc * Ha) + (machine.Yca * Hb) + (machine.Yab * Hc));

            double R2 = fsquare(R);
            double U2 = fsquare(U);

            double A = U2 + R2 + machine.Q2;
            double minusHalfB = S * U + P * R + Ha * machine.Q2 + machine.towerX[0] * U * machine.Q - machine.towerY[0] * R * machine.Q;
            double C = fsquare(S + machine.towerX[0] * machine.Q) + fsquare(P - machine.towerY[0] * machine.Q) + (fsquare(Ha) - machine.D2) * machine.Q2;

            double rslt = (minusHalfB - Math.Sqrt(fsquare(minusHalfB) - A * C)) / A;
            if (Double.IsNaN(rslt))
            {
                throw new Exception("At least one probe point is not reachable. Please correct your delta radius, diagonal rod length, or probe coordniates.");
            }
            return rslt;
        }

        public static double ComputeDerivative(int deriv, double ha, double hb, double hc, DParameters machine)
        {
            double perturb = 0.001;          // perturbation amount in mm or degrees
            DParameters hiParams = new DParameters(machine.diagonal, machine.radius, machine.homedHeight, machine.xstop, machine.ystop, machine.zstop, machine.xadj, machine.yadj, machine.zadj);
            DParameters loParams = new DParameters(machine.diagonal, machine.radius, machine.homedHeight, machine.xstop, machine.ystop, machine.zstop, machine.xadj, machine.yadj, machine.zadj);
            switch (deriv)
            {
                case 0:
                case 1:
                case 2:
                    break;

                case 3:
                    hiParams.radius += perturb;
                    loParams.radius -= perturb;
                    break;

                case 4:
                    hiParams.xadj += perturb;
                    loParams.xadj -= perturb;
                    break;

                case 5:
                    hiParams.yadj += perturb;
                    loParams.yadj -= perturb;
                    break;

                case 6:
                    hiParams.diagonal += perturb;
                    loParams.diagonal -= perturb;
                    break;
            }

            hiParams.Recalc();
            loParams.Recalc();

            double zHi = Escher3D.InverseTransform((deriv == 0) ? ha + perturb : ha, (deriv == 1) ? hb + perturb : hb, (deriv == 2) ? hc + perturb : hc, hiParams);
            double zLo = Escher3D.InverseTransform((deriv == 0) ? ha - perturb : ha, (deriv == 1) ? hb - perturb : hb, (deriv == 2) ? hc - perturb : hc, loParams);

            return (zHi - zLo) / (2.0 * perturb);
        }

        

        public static void NormaliseEndstopAdjustments(DParameters machine)
        {
            double eav = (machine.firmware == Firmware.MARLIN || machine.firmware == Firmware.MARLINRC || machine.firmware == Firmware.REPETIER) ? Math.Min(machine.xstop, Math.Min(machine.ystop, machine.zstop))
                : (machine.xstop + machine.ystop + machine.zstop) / 3.0;
            machine.xstop -= eav;
            machine.ystop -= eav;
            machine.zstop -= eav;
            machine.homedHeight += eav;
            machine.homedCarriageHeight += eav;				// no need for a full recalc, this is sufficient
        }

        public static void Adjust(int numFactors, double[] v, bool norm, DParameters machine)
        {
            double oldCarriageHeightA = machine.homedCarriageHeight + machine.xstop; // save for later

            // Update endstop adjustments
            machine.xstop += v[0];
            machine.ystop += v[1];
            machine.zstop += v[2];
            if (norm)
            {
                NormaliseEndstopAdjustments(machine);
            }

            if (numFactors >= 4)
            {
                machine.radius += v[3];

                if (numFactors >= 6)
                {
                    machine.xadj += v[4];
                    machine.yadj += v[5];

                    if (numFactors == 7)
                    {
                        machine.diagonal += v[6];
                    }
                }

                machine.Recalc();
            }

            // Adjusting the diagonal and the tower positions affects the homed carriage height.
            // We need to adjust homedHeight to allow for this, to get the change that was requested in the endstop corrections.
            double heightError = machine.homedCarriageHeight + machine.xstop - oldCarriageHeightA - v[0];
            machine.homedHeight -= heightError;
            machine.homedCarriageHeight -= heightError;
        }

        public static void ClearDebug()
        {
            Escher3D.EscherDebug = "";
        }

        public static void DebugPrint(string s)
        {
            Escher3D.EscherDebug = s;
        }

        public static string PrintVector(string label, double[] v)
        {
            string rslt = label + ": {";
            for (int i = 0; i < v.Length; ++i)
            {
                rslt += v[i].ToString("F4");
                if (i + 1 != v.Length)
                {
                    rslt += ", ";
                }
            }
            rslt += "}";
            return rslt;
        }

        public static CalibrationResult calc(ref DParameters machine, ref TParameters data)
        {
            convertIncomingEndstops(machine);
            try
            {
                CalibrationResult rslt = DoDeltaCalibration(ref machine, ref data);
                convertOutgoingEndstops(machine);
                Console.WriteLine(String.Format("Success! {0}", rslt));
                return rslt;
            } catch (Exception e)
            {
                Console.WriteLine(String.Format("Fail!\n" + e.StackTrace));
                return new CalibrationResult(data.numFactors, data.numPoints, double.NaN, double.NaN);
            }           
        }

        private static CalibrationResult DoDeltaCalibration(ref DParameters machine, ref TParameters data)
        {
            if (data.numFactors != 3 && data.numFactors != 4 && data.numFactors != 6 && data.numFactors != 7)
            {
                
                throw new Exception("Error: " + data.numFactors + " factors requested but only 3, 4, 6 and 7 supported");
            }
            if (data.numFactors > data.numPoints)
            {
                throw new Exception("Error: need at least as many points as factors you want to calibrate");
            }

            if (debug)
                ClearDebug();

            // Transform the probing points to motor endpoints and store them in a matrix, so that we can do multiple iterations using the same data
            ReMatrix probeMotorPositions = new ReMatrix(data.numPoints, 3);
            double[] corrections = new double[data.numPoints];
            double initialSumOfSquares = 0.0;
            for (int i = 0; i < data.numPoints; ++i)
            {
                corrections[i] = 0.0;
                double[] machinePos = new double[3];
                double xp = data.xBedProbePoints[i];
                double yp = data.yBedProbePoints[i];
                machinePos[0] = xp;
                machinePos[1] = yp;
                machinePos[2] = 0.0;

                probeMotorPositions.data[i][0] = Escher3D.Transform(machinePos, 0, ref machine);
                probeMotorPositions.data[i][1] = Escher3D.Transform(machinePos, 1, ref machine);
                probeMotorPositions.data[i][2] = Escher3D.Transform(machinePos, 2, ref machine);

                initialSumOfSquares += fsquare(data.zBedProbePoints[i]);
            }

            if (debug)
                DebugPrint(probeMotorPositions.Print("Motor positions:"));

            // Do 1 or more Newton-Raphson iterations
            double expectedRmsError = 0.0;
            for (int iteration = 0; iteration < 3; iteration++)
            {
                // Build a Nx7 matrix of derivatives with respect to xa, xb, yc, za, zb, zc, diagonal.
                ReMatrix derivativeMatrix = new ReMatrix(data.numPoints, data.numFactors);
                for (int i = 0; i < data.numPoints; ++i)
                {
                    for (int j = 0; j < data.numFactors; ++j)
                    {
                        derivativeMatrix.data[i][j] =
                            Escher3D.ComputeDerivative(j, probeMotorPositions.data[i][0], probeMotorPositions.data[i][1], probeMotorPositions.data[i][2],machine);
                    }
                }

                if (debug)
                    DebugPrint(derivativeMatrix.Print("Derivative matrix:"));

                // Now build the normal equations for least squares fitting
                ReMatrix normalMatrix = new ReMatrix(data.numFactors, data.numFactors + 1);
                for (int i = 0; i < data.numFactors; ++i)
                {
                    for (int j = 0; j < data.numFactors; ++j)
                    {
                        double temp2 = derivativeMatrix.data[0][i] * derivativeMatrix.data[0][j];
                        for (int k = 1; k < data.numPoints; ++k)
                        {
                            temp2 += derivativeMatrix.data[k][i] * derivativeMatrix.data[k][j];
                        }
                        normalMatrix.data[i][j] = temp2;
                    }
                    double temp = derivativeMatrix.data[0][i] * -(data.zBedProbePoints[0] + corrections[0]);
                    for (int k = 1; k < data.numPoints; ++k)
                    {
                        temp += derivativeMatrix.data[k][i] * -(data.zBedProbePoints[k] + corrections[k]);
                    }
                    normalMatrix.data[i][data.numFactors] = temp;
                }

                if (debug)
                    DebugPrint(normalMatrix.Print("Normal matrix:"));

                normalMatrix.GaussJordan(out double[] solution, data.numFactors);

                for (int i = 0; i < data.numFactors; ++i)
                {
                    if (double.IsNaN(solution[i]))
                    {
                        throw new Exception("Unable to calculate corrections. Please make sure the bed probe points are all distinct.");
                    }
                }

                if(debug)
                    DebugPrint(normalMatrix.Print("Solved matrix:"));

                if (debug)
                {
                    DebugPrint(PrintVector("Solution", solution));

                    // Calculate and display the residuals
                    var residuals = new double[data.numPoints];
                    for (int i = 0; i < data.numPoints; ++i)
                    {
                        var r = data.zBedProbePoints[i];
                        for (int j = 0; j < data.numFactors; ++j)
                        {
                            r += solution[j] * derivativeMatrix.data[i][j];
                        }
                        residuals[i] = r;
                    }
                    DebugPrint(PrintVector("Residuals", residuals));
                }

                Escher3D.Adjust(data.numFactors, solution, data.normalise, machine);

                // Calculate the expected probe heights using the new parameters
                {
                    double[] expectedResiduals = new double[data.numPoints];
                    double sumOfSquares = 0.0;
                    for (int i = 0; i < data.numPoints; ++i)
                    {
                        for (var axis = 0; axis < 3; ++axis)
                        {
                            probeMotorPositions.data[i][axis] += solution[axis];
                        }
                        double newZ = Escher3D.InverseTransform(probeMotorPositions.data[i][0], probeMotorPositions.data[i][1], probeMotorPositions.data[i][2],machine);
                        corrections[i] = newZ;
                        expectedResiduals[i] = data.zBedProbePoints[i] + newZ;
                        sumOfSquares += fsquare(expectedResiduals[i]);
                    }

                    expectedRmsError = Math.Sqrt(sumOfSquares / data.numPoints);

                    if(debug)
                        Debug.WriteLine(String.Format("Iteration {0} RMS:\t{1}", iteration, expectedRmsError));

                    if (debug)
                        DebugPrint(PrintVector("Expected probe error", expectedResiduals));
                }

                // Decide whether to do another iteration Two is slightly better than one, but three doesn't improve things.
                // Alternatively, we could stop when the expected RMS error is only slightly worse than the RMS of the residuals.
            }
            CalibrationResult result = new CalibrationResult(data.numFactors, data.numPoints, initialSumOfSquares, expectedRmsError);
            return result;
        }

        public static void convertIncomingEndstops(DParameters machine)
        {
            double endstopFactor = (machine.firmware == Firmware.RRF) ? 1.0
                        : (machine.firmware == Firmware.REPETIER) ? 1.0 / machine.stepspermm
                            : -1.0;
            machine.xstop *= endstopFactor;
            machine.ystop *= endstopFactor;
            machine.zstop *= endstopFactor;
        }

        public static void convertOutgoingEndstops(DParameters machine)
        {
            double endstopFactor = (machine.firmware == Firmware.RRF) ? 1.0
                        : (machine.firmware == Firmware.REPETIER) ? machine.stepspermm
                            : -1.0;
            machine.xstop *= endstopFactor;
            machine.ystop *= endstopFactor;
            machine.zstop *= endstopFactor;
        }

        public enum NumFactors
        {
            THREE=3,
            FOUR=4,
            SIX=6,
            SEVEN=7
        }
    }    
}

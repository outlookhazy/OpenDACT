using System;
using Klipper_Calibration_Tool.Classes.DataStructures;

namespace Klipper_Calibration_Tool.Classes.Escher
{
    public static class Escher3D
    {
        private static string _debug;
        public static string EscherDebug {
            get => _debug;
            set { _debug = value; System.Diagnostics.Debug.WriteLine(value); }
        }
        public const double DegreesToRadians = Math.PI / 180.0;

        public static double Fsquare(double value)
        {
            return Math.Pow(value, 2.0);
        }

        public static double Transform(double[] machinePos, int axis, ref DParameters machine)
        {
            return machinePos[2] + Math.Sqrt(machine.D2 - Fsquare(machinePos[0] - machine.TowerX[axis]) - Fsquare(machinePos[1] - machine.TowerY[axis]));
        }

        public static double InverseTransform(double ha, double hb, double hc, DParameters machine)
        {
            double fa = machine.CoreFa + Fsquare(ha);
            double fb = machine.CoreFb + Fsquare(hb);
            double fc = machine.CoreFc + Fsquare(hc);

            // Setup PQRSU such that x = -(S - uz)/P, y = (P - Rz)/Q
            double p = machine.Xbc * fa + machine.Xca * fb + machine.Xab * fc;
            double s = machine.Ybc * fa + machine.Yca * fb + machine.Yab * fc;

            double r = 2.0 * (machine.Xbc * ha + machine.Xca * hb + machine.Xab * hc);
            double u = 2.0 * (machine.Ybc * ha + machine.Yca * hb + machine.Yab * hc);

            double r2 = Fsquare(r);
            double u2 = Fsquare(u);

            double a = u2 + r2 + machine.Q2;
            double minusHalfB = s * u + p * r + ha * machine.Q2 + machine.TowerX[0] * u * machine.Q - machine.TowerY[0] * r * machine.Q;
            double c = Fsquare(s + machine.TowerX[0] * machine.Q) + Fsquare(p - machine.TowerY[0] * machine.Q) + (Fsquare(ha) - machine.D2) * machine.Q2;

            double rslt = (minusHalfB - Math.Sqrt(Fsquare(minusHalfB) - a * c)) / a;
            if (double.IsNaN(rslt))
            {
                throw new Exception("At least one probe point is not reachable. Please correct your delta radius, diagonal rod length, or probe coordniates.");
            }
            return rslt;
        }

        public static double ComputeDerivative(int deriv, double ha, double hb, double hc, DParameters machine)
        {
            const double perturb = 0.001; // perturbation amount in mm or degrees
            DParameters hiParams = new DParameters(machine.Diagonal, machine.Radius, machine.HomedHeight, machine.Xstop, machine.Ystop, machine.Zstop, machine.Xadj, machine.Yadj, machine.Zadj);
            DParameters loParams = new DParameters(machine.Diagonal, machine.Radius, machine.HomedHeight, machine.Xstop, machine.Ystop, machine.Zstop, machine.Xadj, machine.Yadj, machine.Zadj);
            switch (deriv)
            {
                case 0:
                case 1:
                case 2:
                    break;

                case 3:
                    hiParams.Radius += perturb;
                    loParams.Radius -= perturb;
                    break;

                case 4:
                    hiParams.Xadj += perturb;
                    loParams.Xadj -= perturb;
                    break;

                case 5:
                    hiParams.Yadj += perturb;
                    loParams.Yadj -= perturb;
                    break;

                case 6:
                    hiParams.Diagonal += perturb;
                    loParams.Diagonal -= perturb;
                    break;
            }

            hiParams.Recalc();
            loParams.Recalc();

            double zHi = InverseTransform(deriv == 0 ? ha + perturb : ha, deriv == 1 ? hb + perturb : hb, deriv == 2 ? hc + perturb : hc, hiParams);
            double zLo = InverseTransform(deriv == 0 ? ha - perturb : ha, deriv == 1 ? hb - perturb : hb, deriv == 2 ? hc - perturb : hc, loParams);

            return (zHi - zLo) / (2.0 * perturb);
        }



        public static void NormaliseEndstopAdjustments(DParameters machine)
        {
            double eav = machine.Firmware == DParameters.FirmwareType.Marlin || machine.Firmware == DParameters.FirmwareType.Marlinrc || machine.Firmware == DParameters.FirmwareType.Repetier || machine.Firmware == DParameters.FirmwareType.Klipper ? Math.Min(machine.Xstop, Math.Min(machine.Ystop, machine.Zstop))
                : (machine.Xstop + machine.Ystop + machine.Zstop) / 3.0;
            machine.Xstop -= eav;
            machine.Ystop -= eav;
            machine.Zstop -= eav;
            machine.HomedHeight += eav;
            machine.HomedCarriageHeight += eav;				// no need for a full recalc, this is sufficient
        }

        public static void Adjust(int numFactors, double[] v, bool norm, DParameters machine)
        {
            double oldCarriageHeightA = machine.HomedCarriageHeight + machine.Xstop; // save for later

            // Update endstop adjustments
            machine.Xstop += v[0];
            machine.Ystop += v[1];
            machine.Zstop += v[2];
            if (norm)
            {
                NormaliseEndstopAdjustments(machine);
            }

            if (numFactors >= 4)
            {
                machine.Radius += v[3];

                if (numFactors >= 6)
                {
                    machine.Xadj += v[4];
                    machine.Yadj += v[5];

                    if (numFactors == 7)
                    {
                        machine.Diagonal += v[6];
                    }
                }

                machine.Recalc();
            }

            // Adjusting the diagonal and the tower positions affects the homed carriage height.
            // We need to adjust homedHeight to allow for this, to get the change that was requested in the endstop corrections.
            double heightError = machine.HomedCarriageHeight + machine.Xstop - oldCarriageHeightA - v[0];
            machine.HomedHeight -= heightError;
            machine.HomedCarriageHeight -= heightError;
        }

        public static void ClearDebug()
        {
            EscherDebug = "";
        }

        public static void DebugPrint(string s)
        {
            EscherDebug = s;
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

        public static CalibrationResult Calc(ref DParameters machine, ref Parameters data)
        {
            ConvertIncomingEndstops(machine);
            try
            {
                CalibrationResult rslt = DoDeltaCalibration(ref machine, ref data);
                ConvertOutgoingEndstops(machine);
                Console.WriteLine("Success! {0}", rslt);
                return rslt;
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Fail!\n" + e.StackTrace));
                return new CalibrationResult(data.NumFactors, data.NumPoints, double.NaN, double.NaN);
            }
        }

        private static CalibrationResult DoDeltaCalibration(ref DParameters machine, ref Parameters data)
        {
            if (data.NumFactors != 3 && data.NumFactors != 4 && data.NumFactors != 6 && data.NumFactors != 7)
            {

                throw new Exception("Error: " + data.NumFactors + " factors requested but only 3, 4, 6 and 7 supported");
            }
            if (data.NumFactors > data.NumPoints)
            {
                throw new Exception("Error: need at least as many points as factors you want to calibrate");
            }

            ClearDebug();

            // Transform the probing points to motor endpoints and store them in a matrix, so that we can do multiple iterations using the same data
            ReMatrix probeMotorPositions = new ReMatrix(data.NumPoints, 3);
            double[] corrections = new double[data.NumPoints];
            double initialSumOfSquares = 0.0;
            for (int i = 0; i < data.NumPoints; ++i)
            {
                corrections[i] = 0.0;
                double[] machinePos = new double[3];
                double xp = data.XBedProbePoints[i];
                double yp = data.YBedProbePoints[i];
                machinePos[0] = xp;
                machinePos[1] = yp;
                machinePos[2] = 0.0;

                probeMotorPositions.Data[i][0] = Transform(machinePos, 0, ref machine);
                probeMotorPositions.Data[i][1] = Transform(machinePos, 1, ref machine);
                probeMotorPositions.Data[i][2] = Transform(machinePos, 2, ref machine);

                initialSumOfSquares += Fsquare(data.ZBedProbePoints[i]);
            }

            DebugPrint(probeMotorPositions.Print("Motor positions:"));

            // Do 1 or more Newton-Raphson iterations
            double expectedRmsError = 0.0;
            for (int iteration = 0; iteration < 3; iteration++)
            {
                // Build a Nx7 matrix of derivatives with respect to xa, xb, yc, za, zb, zc, diagonal.
                ReMatrix derivativeMatrix = new ReMatrix(data.NumPoints, data.NumFactors);
                for (int i = 0; i < data.NumPoints; ++i)
                {
                    for (int j = 0; j < data.NumFactors; ++j)
                    {
                        derivativeMatrix.Data[i][j] =
                            ComputeDerivative(j, probeMotorPositions.Data[i][0], probeMotorPositions.Data[i][1], probeMotorPositions.Data[i][2], machine);
                    }
                }


                DebugPrint(derivativeMatrix.Print("Derivative matrix:"));

                // Now build the normal equations for least squares fitting
                ReMatrix normalMatrix = new ReMatrix(data.NumFactors, data.NumFactors + 1);
                for (int i = 0; i < data.NumFactors; ++i)
                {
                    for (int j = 0; j < data.NumFactors; ++j)
                    {
                        double temp2 = derivativeMatrix.Data[0][i] * derivativeMatrix.Data[0][j];
                        for (int k = 1; k < data.NumPoints; ++k)
                        {
                            temp2 += derivativeMatrix.Data[k][i] * derivativeMatrix.Data[k][j];
                        }
                        normalMatrix.Data[i][j] = temp2;
                    }
                    double temp = derivativeMatrix.Data[0][i] * -(data.ZBedProbePoints[0] + corrections[0]);
                    for (int k = 1; k < data.NumPoints; ++k)
                    {
                        temp += derivativeMatrix.Data[k][i] * -(data.ZBedProbePoints[k] + corrections[k]);
                    }
                    normalMatrix.Data[i][data.NumFactors] = temp;
                }


                DebugPrint(normalMatrix.Print("Normal matrix:"));

                normalMatrix.GaussJordan(out double[] solution, data.NumFactors);

                for (int i = 0; i < data.NumFactors; ++i)
                {
                    if (double.IsNaN(solution[i]))
                    {
                        throw new Exception("Unable to calculate corrections. Please make sure the bed probe points are all distinct.");
                    }
                }


                DebugPrint(normalMatrix.Print("Solved matrix:"));


                DebugPrint(PrintVector("Solution", solution));

                // Calculate and display the residuals
                double[] residuals = new double[data.NumPoints];
                for (int i = 0; i < data.NumPoints; ++i)
                {
                    double r = data.ZBedProbePoints[i];
                    for (int j = 0; j < data.NumFactors; ++j)
                    {
                        r += solution[j] * derivativeMatrix.Data[i][j];
                    }
                    residuals[i] = r;
                }
                DebugPrint(PrintVector("Residuals", residuals));


                Adjust(data.NumFactors, solution, data.Normalise, machine);

                // Calculate the expected probe heights using the new parameters
                {
                    double[] expectedResiduals = new double[data.NumPoints];
                    double sumOfSquares = 0.0;
                    for (int i = 0; i < data.NumPoints; ++i)
                    {
                        for (int axis = 0; axis < 3; ++axis)
                        {
                            probeMotorPositions.Data[i][axis] += solution[axis];
                        }
                        double newZ = InverseTransform(probeMotorPositions.Data[i][0], probeMotorPositions.Data[i][1], probeMotorPositions.Data[i][2], machine);
                        corrections[i] = newZ;
                        expectedResiduals[i] = data.ZBedProbePoints[i] + newZ;
                        sumOfSquares += Fsquare(expectedResiduals[i]);
                    }

                    expectedRmsError = Math.Sqrt(sumOfSquares / data.NumPoints);


                    System.Diagnostics.Debug.WriteLine("Iteration {0} RMS:\t{1}", iteration, expectedRmsError);


                    DebugPrint(PrintVector("Expected probe error", expectedResiduals));
                }

                // Decide whether to do another iteration Two is slightly better than one, but three doesn't improve things.
                // Alternatively, we could stop when the expected RMS error is only slightly worse than the RMS of the residuals.
            }
            CalibrationResult result = new CalibrationResult(data.NumFactors, data.NumPoints, initialSumOfSquares, expectedRmsError);
            return result;
        }

        public static void ConvertIncomingEndstops(DParameters machine)
        {
            double endstopFactor = machine.Firmware == DParameters.FirmwareType.Rrf || machine.Firmware == DParameters.FirmwareType.Klipper ? 1.0
                        : machine.Firmware == DParameters.FirmwareType.Repetier ? 1.0 / machine.Stepspermm
                            : -1.0;
            machine.Xstop *= endstopFactor;
            machine.Ystop *= endstopFactor;
            machine.Zstop *= endstopFactor;
        }

        public static void ConvertOutgoingEndstops(DParameters machine)
        {
            double endstopFactor = machine.Firmware == DParameters.FirmwareType.Rrf || machine.Firmware == DParameters.FirmwareType.Klipper ? 1.0
                        : machine.Firmware == DParameters.FirmwareType.Repetier ? machine.Stepspermm
                            : -1.0;
            machine.Xstop *= endstopFactor;
            machine.Ystop *= endstopFactor;
            machine.Zstop *= endstopFactor;
        }

        public enum NumFactors
        {
            Three = 3,
            Four = 4,
            Six = 6,
            Seven = 7
        }
    }
}

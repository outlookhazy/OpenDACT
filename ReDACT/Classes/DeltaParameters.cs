using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes
{
    public class DeltaParameters
    {
        double[] towerX;
        double[] towerY;
        double D2;
        double coreFa;
        double coreFb;
        double coreFc;
        double Xbc;
        double Xca;
        double Xab;
        double Ybc;
        double Yca;
        double Yab;
        double Q2;
        double Q;
        public double radius;
        public double xadj;
        public double yadj;
        public double zadj;
        public double diagonal;
        double homedCarriageHeight;
        public double homedHeight;
        public double xstop;
        public double ystop;
        public double zstop;
        public double stepspermm;

        public Firmware firmware;

        public DeltaParameters(Firmware firmware, double stepspermm, double diagonal, double radius, double homedHeight, double xstop, double ystop, double zstop, double xadj, double yadj, double zadj)
        {
            this.firmware = firmware;
            this.stepspermm = stepspermm;
            this.diagonal = diagonal;
            this.radius = radius;
            this.homedHeight = homedHeight;
            this.xstop = xstop;
            this.ystop = ystop;
            this.zstop = zstop;
            this.xadj = xadj;
            this.yadj = yadj;
            this.zadj = zadj;

            this.Recalc();
        }

        public void Recalc()
        {
            this.towerX = new double[3];
            this.towerY = new double[3];
            this.towerX[0] = (-(this.radius * Math.Cos((30 + this.xadj) * Escher.degreesToRadians)));
            this.towerY[0] = (-(this.radius * Math.Sin((30 + this.xadj) * Escher.degreesToRadians)));
            this.towerX[1] = (+(this.radius * Math.Cos((30 - this.yadj) * Escher.degreesToRadians)));
            this.towerY[1] = (-(this.radius * Math.Sin((30 - this.yadj) * Escher.degreesToRadians)));
            this.towerX[2] = (-(this.radius * Math.Sin(this.zadj * Escher.degreesToRadians)));
            this.towerY[2] = (+(this.radius * Math.Cos(this.zadj * Escher.degreesToRadians)));

            this.Xbc = this.towerX[2] - this.towerX[1];
            this.Xca = this.towerX[0] - this.towerX[2];
            this.Xab = this.towerX[1] - this.towerX[0];
            this.Ybc = this.towerY[2] - this.towerY[1];
            this.Yca = this.towerY[0] - this.towerY[2];
            this.Yab = this.towerY[1] - this.towerY[0];
            this.coreFa = Math.Pow(this.towerX[0], 2) + Math.Pow(this.towerY[0], 2);
            this.coreFb = Math.Pow(this.towerX[1], 2) + Math.Pow(this.towerY[1], 2);
            this.coreFc = Math.Pow(this.towerX[2], 2) + Math.Pow(this.towerY[2], 2);
            this.Q = 2 * (this.Xca * this.Yab - this.Xab * this.Yca);
            this.Q2 = Math.Pow(this.Q, 2);
            this.D2 = Math.Pow(this.diagonal, 2);

            // Calculate the base carriage height when the printer is homed.
            var tempHeight = this.diagonal;     // any sensible height will do here, probably even zero
            this.homedCarriageHeight = this.homedHeight + tempHeight - DeltaParameters.InverseTransform(this, tempHeight, tempHeight, tempHeight);
        }


        #region internal-data-safe functions

        public static double Transform(DeltaParameters delta, double[] machinePos, int axis)
        {
            return machinePos[2] + Math.Sqrt(delta.D2 - Math.Pow((machinePos[0] - delta.towerX[axis]), 2) - Math.Pow((machinePos[1] - delta.towerY[axis]), 2));
        }

        // Inverse transform method, We only need the Z component of the result.
        public static double InverseTransform(DeltaParameters target, double Ha, double Hb, double Hc)
        {
            var Fa = target.coreFa + Math.Pow(Ha, 2);
            var Fb = target.coreFb + Math.Pow(Hb, 2);
            var Fc = target.coreFc + Math.Pow(Hc, 2);

            // Setup PQRSU such that x = -(S - uz)/P, y = (P - Rz)/Q
            var P = (target.Xbc * Fa) + (target.Xca * Fb) + (target.Xab * Fc);
            var S = (target.Ybc * Fa) + (target.Yca * Fb) + (target.Yab * Fc);

            var R = 2 * ((target.Xbc * Ha) + (target.Xca * Hb) + (target.Xab * Hc));
            var U = 2 * ((target.Ybc * Ha) + (target.Yca * Hb) + (target.Yab * Hc));

            var R2 = Math.Pow(R, 2);
            var U2 = Math.Pow(U, 2);

            var A = U2 + R2 + target.Q2;
            var minusHalfB = S * U + P * R + Ha * target.Q2 + target.towerX[0] * U * target.Q - target.towerY[0] * R * target.Q;
            var C = Math.Pow((S + target.towerX[0] * target.Q), 2) + Math.Pow((P - target.towerY[0] * target.Q), 2) + (Math.Pow(Ha, 2) - target.D2) * target.Q2;

            var rslt = (minusHalfB - Math.Sqrt(Math.Pow(minusHalfB, 2) - A * C)) / A;
            /*
            if (isNaN(rslt))
            {
                throw "At least one probe point is not reachable. Please correct your delta radius, diagonal rod length, or probe coordniates."
                    }
                    */
            return rslt;
        }

        public static double ComputeDerivative(DeltaParameters target, int deriv, double ha, double hb, double hc)
        {
            var perturb = 0.2;          // perturbation amount in mm or degrees *****ORIGINALLY .2 -cg
            var hiParams = new DeltaParameters(target.firmware, target.stepspermm, target.diagonal, target.radius, target.homedHeight, target.xstop, target.ystop, target.zstop, target.xadj, target.yadj, target.zadj);
            var loParams = new DeltaParameters(target.firmware, target.stepspermm, target.diagonal, target.radius, target.homedHeight, target.xstop, target.ystop, target.zstop, target.xadj, target.yadj, target.zadj);
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

            var zHi = DeltaParameters.InverseTransform(hiParams, (deriv == 0) ? ha + perturb : ha, (deriv == 1) ? hb + perturb : hb, (deriv == 2) ? hc + perturb : hc);
            var zLo = DeltaParameters.InverseTransform(loParams, (deriv == 0) ? ha - perturb : ha, (deriv == 1) ? hb - perturb : hb, (deriv == 2) ? hc - perturb : hc);

            return (zHi - zLo) / (2 * perturb);
        }

        // Make the average of the endstop adjustments zero, or make all emndstop corrections negative, without changing the individual homed carriage heights
        public static DeltaParameters NormaliseEndstopAdjustments(DeltaParameters preNormalized)
        {
            var eav = (preNormalized.firmware == Firmware.Marlin || preNormalized.firmware == Firmware.MarlinRC || preNormalized.firmware == Firmware.Repetier) ? Math.Min(preNormalized.xstop, Math.Min(preNormalized.ystop, preNormalized.zstop))
            : (preNormalized.xstop + preNormalized.ystop + preNormalized.zstop) / 3.0;

            var newxstop = preNormalized.xstop - eav;
            var newystop = preNormalized.ystop - eav;
            var newzstop = preNormalized.zstop - eav;
            var newhomedHeight = preNormalized.homedHeight + eav;
            //var newhomedCarriageHeight = preNormalized.homedCarriageHeight + eav;                // no need for a full recalc, this is sufficient

            return new DeltaParameters(preNormalized.firmware,
                preNormalized.stepspermm,
                preNormalized.diagonal,
                preNormalized.radius,
                newhomedHeight,
                newxstop,
                newystop,
                newzstop,
                preNormalized.xadj,
                preNormalized.yadj,
                preNormalized.zadj);
        }

        // Perform 3, 4, 6 or 7-factor adjustment.
        // The input vector contains the following parameters in this order:
        //  X, Y and Z endstop adjustments
        //  If we are doing 4-factor adjustment, the next argument is the delta radius. Otherwise:
        //  X tower X position adjustment
        //  Y tower X position adjustment
        //  Z tower Y position adjustment
        //  Diagonal rod length adjustment
        public static DeltaParameters Adjust(DeltaParameters target, int numFactors, double[] v, bool norm)
        {
            var oldCarriageHeightA = target.homedCarriageHeight + target.xstop; // save for later

            // Update endstop adjustments
            var newxstop = target.xstop + v[0];
            var newystop = target.ystop + v[1];
            var newzstop = target.zstop + v[2];

            DeltaParameters stopsAdjusted = new DeltaParameters(
                target.firmware,
                target.stepspermm,
                target.diagonal,
                target.radius,
                target.homedHeight,
                newxstop,
                newystop,
                newzstop,
                target.xadj,
                target.yadj,
                target.zadj);

            if (norm)
                stopsAdjusted = DeltaParameters.NormaliseEndstopAdjustments(stopsAdjusted);


            if (numFactors >= 4)
            {
                stopsAdjusted.radius += v[3];

                if (numFactors >= 6)
                {
                    stopsAdjusted.xadj += v[4];
                    stopsAdjusted.yadj += v[5];

                    if (numFactors == 7)
                    {
                        stopsAdjusted.diagonal += v[6];
                    }
                }

                stopsAdjusted.Recalc();
            }

            // Adjusting the diagonal and the tower positions affects the homed carriage height.
            // We need to adjust homedHeight to allow for this, to get the change that was requested in the endstop corrections.
            var heightError = stopsAdjusted.homedCarriageHeight + stopsAdjusted.xstop - oldCarriageHeightA - v[0];
            stopsAdjusted.homedHeight -= heightError;
            stopsAdjusted.homedCarriageHeight -= heightError;

            return stopsAdjusted;
        }

        public static DeltaParameters NewParameters(DeltaParameters startingParameters)
        {
            var endstopPlaces = (startingParameters.firmware == Firmware.Repetier) ? 0 : 2;

            var newparamsxstop = Math.Round(startingParameters.xstop, endstopPlaces);
            var newparamsystop = Math.Round(startingParameters.ystop, endstopPlaces);
            var newparamszstop = Math.Round(startingParameters.zstop, endstopPlaces);

            var newparamsdiagonal = Math.Round(startingParameters.diagonal, 2);

            var newparamsradius = Math.Round(startingParameters.radius, 2);

            var newparamshomedHeight = Math.Round(startingParameters.homedHeight, 2);

            var newparamsxadj = Math.Round(startingParameters.xadj, 2);
            var newparamsyadj = Math.Round(startingParameters.yadj, 2);
            var newparamszadj = Math.Round(startingParameters.zadj, 2);

            return new DeltaParameters(
                startingParameters.firmware, 
                startingParameters.stepspermm, 
                newparamsdiagonal, 
                newparamsradius, 
                newparamshomedHeight, 
                newparamsxstop, 
                newparamsystop, 
                newparamszstop, 
                newparamsxadj, 
                newparamsyadj, 
                newparamszadj);
        }

        public static DeltaParameters ConvertIncomingEndstops(DeltaParameters incoming)
        {
            var endstopFactor = (incoming.firmware == Firmware.RRF) ? 1.0
                                : (incoming.firmware == Firmware.Repetier) ? 1.0 / incoming.stepspermm
                                    : -1.0;
            var newxstop = incoming.xstop * endstopFactor;
            var newystop = incoming.ystop * endstopFactor;
            var newzstop = incoming.zstop * endstopFactor;

            return new DeltaParameters(incoming.firmware,
                incoming.stepspermm,
                incoming.diagonal,
                incoming.radius,
                incoming.homedHeight,
                newxstop,
                newystop,
                newzstop,
                incoming.xadj,
                incoming.yadj,
                incoming.zadj);
        }

        public static DeltaParameters ConvertOutgoingEndstops(DeltaParameters outgoing)
        {
            var endstopFactor = (outgoing.firmware == Firmware.RRF) ? 1.0
                                : (outgoing.firmware == Firmware.Repetier) ? (outgoing.stepspermm)
                                    : -1.0;
            var newxstop = outgoing.xstop * endstopFactor;
            var newystop = outgoing.ystop * endstopFactor;
            var newzstop = outgoing.zstop * endstopFactor;

            return new DeltaParameters(outgoing.firmware,
                outgoing.stepspermm,
                outgoing.diagonal,
                outgoing.radius,
                outgoing.homedHeight,
                newxstop,
                newystop,
                newzstop,
                outgoing.xadj,
                outgoing.yadj,
                outgoing.zadj);
        }

        #endregion


        public enum Firmware
        {
            Marlin,
            MarlinRC,
            Repetier,
            RRF,
            Smoothieware
        }

    }

}

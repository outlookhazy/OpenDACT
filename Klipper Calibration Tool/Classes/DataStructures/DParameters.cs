using System;
using Klipper_Calibration_Tool.Classes.Escher;

namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    public class DParameters
    {
        internal double D2;
        internal double[] TowerX;
        internal double[] TowerY;
        internal double CoreFa;
        internal double CoreFb;
        internal double CoreFc;
        internal double Xbc;
        internal double Ybc;
        internal double Xca;
        internal double Xab;
        internal double Yab;
        internal double Yca;
        internal double Q2;
        internal double Q;
        internal double Radius;
        internal double Xadj;
        internal double Yadj;
        internal double Zadj;
        internal double Diagonal;
        internal double HomedCarriageHeight;
        internal double HomedHeight;
        internal double Xstop;
        internal double Ystop;
        internal double Zstop;
        internal FirmwareType Firmware;
        internal double Stepspermm;

        public DParameters() { }

        public DParameters(double diagonal, double radius, double homedHeight, double xstop, double ystop, double zstop, double xadj, double yadj, double zadj)
        {
            Diagonal = diagonal;
            Radius = radius;
            HomedHeight = homedHeight;
            Xstop = xstop;
            Ystop = ystop;
            Zstop = zstop;
            Xadj = xadj;
            Yadj = yadj;
            Zadj = zadj;
            Recalc();
        }

        public void Recalc()
        {
            TowerX = new double[3];
            TowerY = new double[3];
            TowerX[0] = -(Radius * Math.Cos((30 + Xadj) * Escher3D.DegreesToRadians));
            TowerY[0] = -(Radius * Math.Sin((30 + Xadj) * Escher3D.DegreesToRadians));
            TowerX[1] = +(Radius * Math.Cos((30 - Yadj) * Escher3D.DegreesToRadians));
            TowerY[1] = -(Radius * Math.Sin((30 - Yadj) * Escher3D.DegreesToRadians));
            TowerX[2] = -(Radius * Math.Sin(Zadj * Escher3D.DegreesToRadians));
            TowerY[2] = +(Radius * Math.Cos(Zadj * Escher3D.DegreesToRadians));

            Xbc = TowerX[2] - TowerX[1];
            Xca = TowerX[0] - TowerX[2];
            Xab = TowerX[1] - TowerX[0];
            Ybc = TowerY[2] - TowerY[1];
            Yca = TowerY[0] - TowerY[2];
            Yab = TowerY[1] - TowerY[0];
            CoreFa = Escher3D.Fsquare(TowerX[0]) + Escher3D.Fsquare(TowerY[0]);
            CoreFb = Escher3D.Fsquare(TowerX[1]) + Escher3D.Fsquare(TowerY[1]);
            CoreFc = Escher3D.Fsquare(TowerX[2]) + Escher3D.Fsquare(TowerY[2]);
            Q = 2 * (Xca * Yab - Xab * Yca);
            Q2 = Escher3D.Fsquare(Q);
            D2 = Escher3D.Fsquare(Diagonal);

            // Calculate the base carriage height when the printer is homed.
            double tempHeight = Diagonal;     // any sensible height will do here, probably even zero
            HomedCarriageHeight = HomedHeight + tempHeight - Escher3D.InverseTransform(tempHeight, tempHeight, tempHeight, this);
        }

        public enum FirmwareType
        {
            Klipper,
            Marlin,
            Marlinrc,
            Repetier,
            Rrf
        }
    }
}

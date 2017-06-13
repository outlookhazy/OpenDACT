using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes.Escher
{
    public class DParameters
    {
        internal double D2;
        internal double[] towerX;
        internal double[] towerY;
        internal double coreFa;
        internal double coreFb;
        internal double coreFc;
        internal double Xbc;
        internal double Ybc;
        internal double Xca;
        internal double Xab;
        internal double Yab;
        internal double Yca;
        internal double Q2;
        internal double Q;
        internal double radius;
        internal double xadj;
        internal double yadj;
        internal double zadj;
        internal double diagonal;
        internal double homedCarriageHeight;
        internal double homedHeight;
        internal double xstop;
        internal double ystop;
        internal double zstop;
        internal Firmware firmware;
        internal double stepspermm;

        public DParameters() { }

        public DParameters(double diagonal, double radius, double homedHeight, double xstop, double ystop, double zstop, double xadj, double yadj, double zadj)
        {
            this.diagonal = diagonal;
            this.radius = radius;
            this.homedHeight = homedHeight;
            this.xstop = xstop;
            this.ystop = ystop;
            this.zstop = zstop;
            this.xadj = xadj;
            this.yadj = yadj;
            this.zadj = zadj;
            Recalc();
        }

        public void Recalc()
        {
            this.towerX = new double[3];
            this.towerY = new double[3];
            this.towerX[0] = (-(this.radius * Math.Cos((30 + this.xadj) * Escher3D.degreesToRadians)));
            this.towerY[0] = (-(this.radius * Math.Sin((30 + this.xadj) * Escher3D.degreesToRadians)));
            this.towerX[1] = (+(this.radius * Math.Cos((30 - this.yadj) * Escher3D.degreesToRadians)));
            this.towerY[1] = (-(this.radius * Math.Sin((30 - this.yadj) * Escher3D.degreesToRadians)));
            this.towerX[2] = (-(this.radius * Math.Sin(this.zadj * Escher3D.degreesToRadians)));
            this.towerY[2] = (+(this.radius * Math.Cos(this.zadj * Escher3D.degreesToRadians)));

            this.Xbc = this.towerX[2] - this.towerX[1];
            this.Xca = this.towerX[0] - this.towerX[2];
            this.Xab = this.towerX[1] - this.towerX[0];
            this.Ybc = this.towerY[2] - this.towerY[1];
            this.Yca = this.towerY[0] - this.towerY[2];
            this.Yab = this.towerY[1] - this.towerY[0];
            this.coreFa = Escher3D.fsquare(this.towerX[0]) + Escher3D.fsquare(this.towerY[0]);
            this.coreFb = Escher3D.fsquare(this.towerX[1]) + Escher3D.fsquare(this.towerY[1]);
            this.coreFc = Escher3D.fsquare(this.towerX[2]) + Escher3D.fsquare(this.towerY[2]);
            this.Q = 2 * (this.Xca * this.Yab - this.Xab * this.Yca);
            this.Q2 = Escher3D.fsquare(this.Q);
            this.D2 = Escher3D.fsquare(this.diagonal);

            // Calculate the base carriage height when the printer is homed.
            var tempHeight = this.diagonal;     // any sensible height will do here, probably even zero
            this.homedCarriageHeight = this.homedHeight + tempHeight - Escher3D.InverseTransform(tempHeight, tempHeight, tempHeight, this);
        }

        public enum Firmware
        {
            KLIPPER,
            MARLIN,
            MARLINRC,
            REPETIER,
            RRF
        }
    }
}

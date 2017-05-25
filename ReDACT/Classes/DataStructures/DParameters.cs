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
            Escher3D.Recalc(this);
        }

        public enum Firmware
        {
            MARLIN,
            MARLINRC,
            REPETIER,
            RRF
        }
    }
}

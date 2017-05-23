using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    public static class Printer {
        public enum ProbeType {            
            FSR,
            ZProbe
        }
    }

    public class HeightMap : Dictionary<Position,Position3D>
    {
        public HeightMap()
        {
            foreach(Position p in typeof(Position).GetEnumValues())
            {
                this[p] = new Position3D();
            }
        }

        public float AverageAccuracy()
        {
            return (Convert.ToSingle(
            (Math.Abs(this[Position.X].Z) +
            Math.Abs(this[Position.XOPP].Z) +
            Math.Abs(this[Position.Y].Z) +
            Math.Abs(this[Position.YOPP].Z) +
            Math.Abs(this[Position.Z].Z) +
            Math.Abs(this[Position.ZOPP].Z)) / 6.0));
        }

        public bool PrecisionReached(float targetAccuracy)
        {
            return (Math.Abs(this[Position.X].Z) <= targetAccuracy &&
                Math.Abs(this[Position.XOPP].Z) <= targetAccuracy &&
                Math.Abs(this[Position.Y].Z) <= targetAccuracy &&
                Math.Abs(this[Position.YOPP].Z) <= targetAccuracy &&
                Math.Abs(this[Position.Z].Z) <= targetAccuracy &&
                Math.Abs(this[Position.ZOPP].Z) <= targetAccuracy
                );
        }

        public void PrintHeights()
        {
            UserInterface.consoleLog.Log("Center:" + this[Position.CENTER] + " X:" + this[Position.X] + " XOpp:" + this[Position.XOPP] + " Y:" + this[Position.Y] + " YOpp:" + this[Position.YOPP] + " Z:" + this[Position.Z] + " ZOpp:" + this[Position.ZOPP]);
        }
    }

    public enum Position
    {
        CENTER,
        X,
        XOPP,
        Y,
        YOPP,
        Z,
        ZOPP,
        TEX,
        TEXOPP,
        TEY,
        TEYOPP,
        TEZ,
        TEZOPP
    }
}

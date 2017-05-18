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
        public static bool isCalibrating = true;
    }

    public static class Heights
    {
        //store every set of heights
        public static float center;
        public static float X;
        public static float XOpp;
        public static float Y;
        public static float YOpp;
        public static float Z;
        public static float ZOpp;
        public static float teX;
        public static float teXOpp;
        public static float teY;
        public static float teYOpp;
        public static float teZ;
        public static float teZOpp;
        public static bool firstHeights = true;

        public static bool heightsSet = false;
        public static bool checkHeightsOnly = false;

        public static void PrintHeights()
        {
            UserInterface.consoleLog.Log("Center:" + Heights.center + " X:" + Heights.X + " XOpp:" + Heights.XOpp + " Y:" + Heights.Y + " YOpp:" + Heights.YOpp + " Z:" + Heights.Z + " ZOpp:" + Heights.ZOpp);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using static OpenDACT.Class_Files.Printer;

namespace OpenDACT.Class_Files
{
    public static class UserVariables
    {
        public static float HRadRatio;
        public static float DRadRatio;
        public static float accuracy;
        public static float calculationAccuracy;
        public static float probingHeight;

        public static float offsetCorrection;
        public static float mainOppPerc;
        public static float towPerc;
        public static float oppPerc;

        public static float dRadCorrection;
        public static float dRadMainOppPerc;
        public static float dRadTowPerc;
        public static float dRadOppPerc;

        public static float alphaRotationPercentage;
        public static float deltaTower;
        public static float deltaOpp;
        public static float plateDiameter;
        public static float diagonalRodLength;
        public static float FSROffset;
        public static float probingSpeed;
        public static float xySpeed;//feedrate in gcode

        public static ProbeType probeChoice;

        public static bool advancedCalibration = false;
        
        public static int pauseTimeSet;
        public static int advancedCalCount;
        public static int maxIterations;
        public static int l;
        
        public static List<float> known_yDR = new List<float>();
        public static List<float> known_xDR = new List<float>();

        public static bool isInitiated = false;
        public static int stepsCalcNumber = 0;
    }


    public static class UserInterface
    {
        public static LogConsole consoleLog;
        public static LogConsole printerLog;

        public static void Init() {
            UserInterface.consoleLog = new LogConsole(Program.mainFormTest.consoleMain);
            UserInterface.printerLog = new LogConsole(Program.mainFormTest.consolePrinter, LogConsole.LogLevel.DEBUG);
        }
    }
}

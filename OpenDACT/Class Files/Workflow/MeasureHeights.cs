using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow
{
    static class MeasureHeights
    {
        private static int currentPosition = 0;
        private static int iteration = 0;
        internal static bool heightsMeasured = false;
        internal static bool measuringHeights = false;

        public static void NextCommand()
        {
            float probingHeight = UserVariables.probingHeight;
            float plateDiameter = UserVariables.plateDiameter;
            int pauseTimeSet = UserVariables.pauseTimeSet;
            float valueZ = 0.482F * plateDiameter;
            float valueXYLarge = 0.417F * plateDiameter;
            float valueXYSmall = 0.241F * plateDiameter;

            GCode.TrySend("G0 F" + UserVariables.xySpeed * 60);//converts mm/s to mm/min
            Debug.WriteLine(String.Format("Position: {0}\tIteration: {1}", currentPosition,iteration));
            switch (currentPosition)
            {
                case 0:
                    switch (iteration)
                    {
                        case 0:
                            GCode.TrySend(Command.HOME);
                            iteration++;
                            break;
                        case 1:
                            MoveToPosition(0, 0, probingHeight);
                            iteration++;
                            break;
                        case 2:
                            GCode.TrySend(Command.PROBE);                            
                            iteration = 0;
                            break;
                    }
                    break;
                case 1:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(-valueXYLarge, -valueXYSmall, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration = 0;
                            break;                       
                    }

                    break;
                case 2:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(valueXYLarge, valueXYSmall, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration = 0;
                            break;                        
                    }
                    break;
                case 3:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(valueXYLarge, -valueXYSmall, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration = 0;
                            break;                     
                    }

                    break;
                case 4:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(-valueXYLarge, valueXYSmall, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration = 0;
                            break;                        
                    }
                    break;
                case 5:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(0, valueZ, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration = 0;
                            break;                       
                    }

                    break;
                case 6:
                    switch (iteration)
                    {
                        case 0:
                            MoveToPosition(0, -valueZ, probingHeight);
                            iteration++;
                            break;
                        case 1:
                            GCode.TrySend(Command.PROBE);
                            iteration++;
                            break;                        
                        case 3:
                            MoveToPosition(0, 0, Convert.ToInt32(EEPROM.zMaxLength.Value / 3));
                            currentPosition = 0;
                            iteration = 0;
                            break;                            
                    }
                    break;
            }//end switch
        }

        public static void RecordHeight(float probePosition)
        {
            float zMaxLength = EEPROM.zMaxLength.Value;
            float probingHeight = UserVariables.probingHeight;

            switch (currentPosition)
            {
                case 0:
                    probePosition = zMaxLength - probingHeight + probePosition;
                    Heights.center = probePosition;
                    UserInterface.consoleLog.Log("Measured Center");
                    currentPosition++;
                    break;
                case 1:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.X = probePosition;
                    UserInterface.consoleLog.Log("Measured X");
                    currentPosition++;
                    break;
                case 2:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.XOpp = probePosition;
                    UserInterface.consoleLog.Log("Measured XOpp");
                    currentPosition++;
                    break;
                case 3:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.Y = probePosition;
                    UserInterface.consoleLog.Log("Measured Y");
                    currentPosition++;
                    break;
                case 4:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.YOpp = probePosition;
                    UserInterface.consoleLog.Log("Measured YOpp");
                    currentPosition++;
                    break;
                case 5:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.Z = probePosition;
                    UserInterface.consoleLog.Log("Measured Z");
                    currentPosition++;
                    break;
                case 6:
                    probePosition = Heights.center - (zMaxLength - probingHeight + probePosition);
                    probePosition = -probePosition;
                    Heights.ZOpp = probePosition;
                    UserInterface.consoleLog.Log("Measured ZOpp");
                    currentPosition++;

                    EEPROM.zMaxLength.Value = Heights.center;

                    heightsMeasured = true;
                    break;
            }
        }
    }
}

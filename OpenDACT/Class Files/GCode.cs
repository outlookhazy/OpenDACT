﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenDACT.Class_Files.Workflow;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    static class GCode
    {
        public static int currentPosition = 0;
        public static int iteration = 0;
        public static bool checkHeights = false;
        public static bool wasZProbeHeightSet = false;
        public static bool isHeuristicComplete = false;

        public static bool MoveToPosition(float X, float Y, float Z) {
            return TrySend("G1 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static bool RapidToPosition(float X, float Y, float Z) {
            return TrySend("G0 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static bool SendEEPROMVariable(EEPROM_Variable variable) {
            return TrySend(String.Format("M206 T{0} P{1} S{2}", variable.Type, variable.Position, variable.Value.ToString("F3")));
        }

        public static bool TrySend(String serialCommand) {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected) {
                if (UserInterface.printerLog.ConsoleLogLevel == LogConsole.LogLevel.DEBUG)
                    UserInterface.printerLog.Log(String.Format("Sending: {0}", serialCommand));
                return Connection.serialManager.WriteLine(serialCommand);
            }
            else {
                UserInterface.consoleLog.Log("Not Connected");
                return false;
            }
        }

        /*
        public static void PauseTimeRadius() {
            Thread.Sleep(Convert.ToInt32(((UserVariables.plateDiameter / 2) / UserVariables.xySpeed) * 1000));//1000 s to ms x 1.25 for multiplier
        }

        public static void PauseTimeProbe() {
            Thread.Sleep(Convert.ToInt32(((UserVariables.probingHeight * 2) / EEPROM.zProbeSpeed.Value) * 1125));
        }

        public static void PauseTimeZMax() {
            Thread.Sleep(Convert.ToInt32((EEPROM.zMaxLength.Value / UserVariables.xySpeed) * 1025));
        }

        public static void PauseTimeZMaxThird() {
            Thread.Sleep(Convert.ToInt32(((EEPROM.zMaxLength.Value / 3) / UserVariables.xySpeed) * 1000));
        }
        */


        

        public static void HeuristicLearning()
        {
            //find base heights
            //find heights with each value increased by 1 - HRad, tower offset 1-3, diagonal rod

            if (UserVariables.advancedCalCount == 0)
            {//start
                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    EEPROM.stepsPerMM.Value += 1;
                    UserInterface.consoleLog.Log("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 1)
            {//get diagonal rod percentages

                UserVariables.deltaTower = ((Heights.teX - Heights.X) + (Heights.teY - Heights.Y) + (Heights.teZ - Heights.Z)) / 3;
                UserVariables.deltaOpp = ((Heights.teXOpp - Heights.XOpp) + (Heights.teYOpp - Heights.YOpp) + (Heights.teZOpp - Heights.ZOpp)) / 3;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    EEPROM.stepsPerMM.Value -= 1;
                    UserInterface.consoleLog.Log("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());

                    //set Hrad +1
                    EEPROM.HRadius.Value += 1;
                    UserInterface.consoleLog.Log("Setting horizontal radius to: " + (EEPROM.HRadius.Value).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 2)
            {//get HRad percentages
                UserVariables.HRadRatio = -(Math.Abs((Heights.X - Heights.teX) + (Heights.Y - Heights.teY) + (Heights.Z - Heights.teZ) + (Heights.XOpp - Heights.teXOpp) + (Heights.YOpp - Heights.teYOpp) + (Heights.ZOpp - Heights.teZOpp))) / 6;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //reset horizontal radius
                    EEPROM.HRadius.Value -= 1;
                    UserInterface.consoleLog.Log("Setting horizontal radius to: " + (EEPROM.HRadius).ToString());

                    //set X offset
                    EEPROM.offsetX.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset X to: " + (EEPROM.offsetX).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 3)
            {//get X offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.X - Heights.teX));
                UserVariables.mainOppPerc += Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.X - Heights.teX));
                UserVariables.towPerc += (Math.Abs((Heights.Y - Heights.teY) / (Heights.X - Heights.teX)) + Math.Abs((Heights.Z - Heights.teZ) / (Heights.X - Heights.teX))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.X - Heights.teX)) + Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.X - Heights.teX))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //reset X offset
                    EEPROM.offsetX.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset X to: " + (EEPROM.offsetX).ToString());

                    //set Y offset
                    EEPROM.offsetY.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset Y to: " + (EEPROM.offsetY).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 4)
            {//get Y offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.Y - Heights.teY));
                UserVariables.mainOppPerc += Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.Y - Heights.teY));
                UserVariables.towPerc += (Math.Abs((Heights.X - Heights.teX) / (Heights.Y - Heights.teY)) + Math.Abs((Heights.Z - Heights.teZ) / (Heights.Y - Heights.teY))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.Y - Heights.teY)) + Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.Y - Heights.teY))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //reset Y offset
                    EEPROM.offsetY.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset Y to: " + (EEPROM.offsetY).ToString());

                    //set Z offset
                    EEPROM.offsetZ.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset Z to: " + (EEPROM.offsetZ).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 5)
            {//get Z offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.Z - Heights.teZ));
                UserVariables.mainOppPerc += Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.Z - Heights.teZ));
                UserVariables.towPerc += (Math.Abs((Heights.X - Heights.teX) / (Heights.Z - Heights.teZ)) + Math.Abs((Heights.Y - Heights.teY) / (Heights.Z - Heights.teZ))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.Z - Heights.teZ)) + Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.Z - Heights.teZ))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //set Z offset
                    EEPROM.offsetZ.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset Z to: " + (EEPROM.offsetZ).ToString());

                    //set alpha rotation offset perc X
                    EEPROM.A.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha A to: " + (EEPROM.A).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;

            }
            else if (UserVariables.advancedCalCount == 6)//6
            {
                //get A alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.YOpp - Heights.ZOpp) - (Heights.teYOpp - Heights.teZOpp)));

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //set alpha rotation offset perc X
                    EEPROM.A.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha A to: " + (EEPROM.A).ToString());

                    //set alpha rotation offset perc Y
                    EEPROM.B.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha B to: " + (EEPROM.B).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 7)//7
            {//get B alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.ZOpp - Heights.XOpp) - (Heights.teXOpp - Heights.teXOpp)));

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //set alpha rotation offset perc Y
                    EEPROM.B.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha B to: " + (EEPROM.B).ToString());

                    //set alpha rotation offset perc Z
                    EEPROM.C.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha C to: " + (EEPROM.C).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 8)//8
            {//get C alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.XOpp - Heights.YOpp) - (Heights.teXOpp - Heights.teYOpp)));
                UserVariables.alphaRotationPercentage /= 3;

                if (Connection.serialManager.CurrentState == ConnectionState.Connected)
                {
                    //set alpha rotation offset perc Z
                    EEPROM.C.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha C to: " + (EEPROM.C).ToString());

                }

                UserInterface.consoleLog.Log("Alpha offset percentage: " + UserVariables.alphaRotationPercentage);

                UserVariables.advancedCalibration = false;
                Program.mainFormTest.SetButtonValues();
                UserVariables.advancedCalCount = 0;
                isHeuristicComplete = true;

                //check heights

            }

            GCode.checkHeights = true;
        }

        public static float ParseZProbe(string value)
        {
            if (value.Contains("Z-probe:"))
            {
                //Z-probe: 10.66 zCorr: 0

                string[] parseInData = value.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string[] parseFirstLine = parseInData[0].Split(new char[] { ':', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                //: 10.66 zCorr: 0
                string[] parseZProbe = value.Split(new string[] { "Z-probe", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] parseZProbeSpace = parseZProbe[0].Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                float zProbeParse;

                //check if there is a space between
                if (parseZProbeSpace[0] == ":")
                {
                    //Space
                    zProbeParse = float.Parse(parseZProbeSpace[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    //No space
                    zProbeParse = float.Parse(parseZProbeSpace[0].Substring(1), CultureInfo.InvariantCulture);
                }

                return float.Parse(parseFirstLine[1], CultureInfo.InvariantCulture);
            }
            else
            {
                return 1000;
            }
        }

        public static class Command {
            public static String HOME { get { return "G28"; } }
            public static String PROBE { get { return "G30"; } }
            public static String RESET { get { return "M112"; } }
            public static String READ_EEPROM { get { return "M205"; } }
        }
    }
}

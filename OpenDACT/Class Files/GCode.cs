using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OpenDACT.Class_Files
{
    static class GCode
    {
        public static int currentPosition = 0;
        public static int iteration = 0;
        public static bool checkHeights = false;
        public static bool wasSet = false;
        public static bool wasZProbeHeightSet = false;
        public static bool isHeuristicComplete = false;

        public static void MoveToPosition(float X, float Y, float Z)
        {
            TrySend("G1 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static void RapidToPosition(float X, float Y, float Z) 
        {
            TrySend("G0 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static void SendEEPROMVariable(int type, int position, float value)
        {
            switch (type) {
                case 1:
                    TrySend("M206 T1 P" + position + " S" + value.ToString("F3"));
                    break;
                case 3:
                    TrySend("M206 T3 P" + position + " X" + value.ToString("F3"));
                    break;
                default:
                    UserInterface.LogConsole("Invalid EEPROM Variable.");
                    break;
            }          
        }

        public static bool TrySend(String serialCommand) {
            if (Connection._serialPort.IsOpen) {
                Connection._serialPort.WriteLine(serialCommand);
                return true;
            }
            else {
                UserInterface.LogConsole("Not Connected");
                return false;
            }
        }

        public static void PauseTimeRadius()
        {
            Thread.Sleep(Convert.ToInt32(((UserVariables.plateDiameter / 2) / UserVariables.xySpeed) * 1000));//1000 s to ms x 1.25 for multiplier
        }

        public static void PauseTimeProbe()
        {
            Thread.Sleep(Convert.ToInt32(((UserVariables.probingHeight * 2) / EEPROM.zProbeSpeed) * 1125));
        }

        public static void PauseTimeZMax()
        {
            Thread.Sleep(Convert.ToInt32((EEPROM.zMaxLength / UserVariables.xySpeed) * 1025));
        }

        public static void PauseTimeZMaxThird()
        {
            Thread.Sleep(Convert.ToInt32(((EEPROM.zMaxLength / 3) / UserVariables.xySpeed) * 1000));
        }


        public static void PositionFlow()
        {
            float probingHeight = UserVariables.probingHeight;
            float plateDiameter = UserVariables.plateDiameter;
            int pauseTimeSet = UserVariables.pauseTimeSet;
            float valueZ = 0.482F * plateDiameter;
            float valueXYLarge = 0.417F * plateDiameter;
            float valueXYSmall = 0.241F * plateDiameter;

            if (UserVariables.probeChoice == Printer.ProbeType.ZProbe && wasSet == false)
            {
                switch (iteration)
                {
                    case 0:
                        EEPROM.zProbeHeight = 0;
                        TrySend(Command.HOME);
                        iteration++;
                        break;
                    case 1:
                        MoveToPosition(0, 0, Convert.ToSingle(Math.Round(EEPROM.zMaxLength / 6)));
                        iteration++;
                        break;
                    case 2:
                        TrySend(Command.PROBE);

                        wasSet = true;
                        Program.mainFormTest.SetEEPROMGUIList();
                        EEPROMFunctions.SendEEPROM();
                        iteration = 0;
                        break;
                }
                /*
                pauseTimeZMax();
                pauseTimeZMax();
                pauseTimeProbe();
                */
            }
            else
            {
                TrySend("G0 F" + UserVariables.xySpeed * 60);//converts mm/s to mm/min

                switch (currentPosition)
                {
                    case 0:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.HOME);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(0, 0, probingHeight);
                                iteration++;
                                break;
                            case 2:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 3:
                                MoveToPosition(-valueXYLarge, -valueXYSmall, probingHeight);
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeZMaxThird();
                        pauseTimeZMaxThird();
                        pauseTimeZMax();
                        pauseTimeProbe();
                        pauseTimeRadius();
                        */
                        break;
                    case 1:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(-valueXYLarge, -valueXYSmall, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, 0, probingHeight);
                                iteration++;
                                break;
                            case 3:
                                MoveToPosition(valueXYLarge, valueXYSmall, probingHeight);                                
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        currentPosition++;
                        */
                        break;
                    case 2:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(valueXYLarge, valueXYSmall, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, 0, probingHeight);                                
                                iteration++;
                                break;
                            case 3:
                                MoveToPosition(valueXYLarge, -valueXYSmall, probingHeight);                                
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        currentPosition++;
                        */
                        break;
                    case 3:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(valueXYLarge, -valueXYSmall, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, 0, probingHeight);                                
                                iteration++;
                                break;
                            case 3:
                                MoveToPosition(-valueXYLarge, valueXYSmall, probingHeight);                                
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        currentPosition++;
                        */
                        break;
                    case 4:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(-valueXYLarge, valueXYSmall, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, 0, probingHeight);
                                iteration++;
                                break;
                            case 3:
                                MoveToPosition(0, valueZ, probingHeight);                                
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        currentPosition++;
                        */
                        break;
                    case 5:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(0, valueZ, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, -valueZ, probingHeight);
                                currentPosition++;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        currentPosition++;
                        */
                        break;
                    case 6:
                        switch (iteration)
                        {
                            case 0:
                                TrySend(Command.PROBE);
                                iteration++;
                                break;
                            case 1:
                                MoveToPosition(0, -valueZ, probingHeight);                                
                                iteration++;
                                break;
                            case 2:
                                MoveToPosition(0, 0, probingHeight);                                
                                iteration++;
                                if (Calibration.calibrateInProgress == false){ iteration++; }
                                break;
                            case 3:
                                MoveToPosition(0, 0, Convert.ToInt32(EEPROM.zMaxLength / 3));                                
                                iteration++;
                                break;
                            case 4:
                                currentPosition = 0;
                                checkHeights = false;
                                iteration = 0;
                                break;
                        }
                        /*
                        pauseTimeProbe();
                        pauseTimeRadius();
                        pauseTimeRadius();
                        pauseTimeZMaxThird();
                        */
                        break;
                }//end switch
            }//end else
        }



        public static void HeuristicLearning()
        {
            //find base heights
            //find heights with each value increased by 1 - HRad, tower offset 1-3, diagonal rod

            if (UserVariables.advancedCalCount == 0)
            {//start
                if (Connection._serialPort.IsOpen)
                {
                    EEPROM.stepsPerMM += 1;
                    UserInterface.LogConsole("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 1)
            {//get diagonal rod percentages

                UserVariables.deltaTower = ((Heights.teX - Heights.X) + (Heights.teY - Heights.Y) + (Heights.teZ - Heights.Z)) / 3;
                UserVariables.deltaOpp = ((Heights.teXOpp - Heights.XOpp) + (Heights.teYOpp - Heights.YOpp) + (Heights.teZOpp - Heights.ZOpp)) / 3;

                if (Connection._serialPort.IsOpen)
                {
                    EEPROM.stepsPerMM -= 1;
                    UserInterface.LogConsole("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());

                    //set Hrad +1
                    EEPROM.HRadius += 1;
                    UserInterface.LogConsole("Setting horizontal radius to: " + (EEPROM.HRadius).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 2)
            {//get HRad percentages
                UserVariables.HRadRatio = -(Math.Abs((Heights.X - Heights.teX) + (Heights.Y - Heights.teY) + (Heights.Z - Heights.teZ) + (Heights.XOpp - Heights.teXOpp) + (Heights.YOpp - Heights.teYOpp) + (Heights.ZOpp - Heights.teZOpp))) / 6;

                if (Connection._serialPort.IsOpen)
                {
                    //reset horizontal radius
                    EEPROM.HRadius -= 1;
                    UserInterface.LogConsole("Setting horizontal radius to: " + (EEPROM.HRadius).ToString());

                    //set X offset
                    EEPROM.offsetX += 80;
                    UserInterface.LogConsole("Setting offset X to: " + (EEPROM.offsetX).ToString());
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

                if (Connection._serialPort.IsOpen)
                {
                    //reset X offset
                    EEPROM.offsetX -= 80;
                    UserInterface.LogConsole("Setting offset X to: " + (EEPROM.offsetX).ToString());

                    //set Y offset
                    EEPROM.offsetY += 80;
                    UserInterface.LogConsole("Setting offset Y to: " + (EEPROM.offsetY).ToString());
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

                if (Connection._serialPort.IsOpen)
                {
                    //reset Y offset
                    EEPROM.offsetY -= 80;
                    UserInterface.LogConsole("Setting offset Y to: " + (EEPROM.offsetY).ToString());

                    //set Z offset
                    EEPROM.offsetZ += 80;
                    UserInterface.LogConsole("Setting offset Z to: " + (EEPROM.offsetZ).ToString());
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

                if (Connection._serialPort.IsOpen)
                {
                    //set Z offset
                    EEPROM.offsetZ -= 80;
                    UserInterface.LogConsole("Setting offset Z to: " + (EEPROM.offsetZ).ToString());

                    //set alpha rotation offset perc X
                    EEPROM.A += 1;
                    UserInterface.LogConsole("Setting Alpha A to: " + (EEPROM.A).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;

            }
            else if (UserVariables.advancedCalCount == 6)//6
            {
                //get A alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.YOpp - Heights.ZOpp) - (Heights.teYOpp - Heights.teZOpp)));

                if (Connection._serialPort.IsOpen)
                {
                    //set alpha rotation offset perc X
                    EEPROM.A -= 1;
                    UserInterface.LogConsole("Setting Alpha A to: " + (EEPROM.A).ToString());

                    //set alpha rotation offset perc Y
                    EEPROM.B += 1;
                    UserInterface.LogConsole("Setting Alpha B to: " + (EEPROM.B).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 7)//7
            {//get B alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.ZOpp - Heights.XOpp) - (Heights.teXOpp - Heights.teXOpp)));

                if (Connection._serialPort.IsOpen)
                {
                    //set alpha rotation offset perc Y
                    EEPROM.B -= 1;
                    UserInterface.LogConsole("Setting Alpha B to: " + (EEPROM.B).ToString());

                    //set alpha rotation offset perc Z
                    EEPROM.C += 1;
                    UserInterface.LogConsole("Setting Alpha C to: " + (EEPROM.C).ToString());
                }

                //check heights

                UserVariables.advancedCalCount++;
            }
            else if (UserVariables.advancedCalCount == 8)//8
            {//get C alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.XOpp - Heights.YOpp) - (Heights.teXOpp - Heights.teYOpp)));
                UserVariables.alphaRotationPercentage /= 3;

                if (Connection._serialPort.IsOpen)
                {
                    //set alpha rotation offset perc Z
                    EEPROM.C -= 1;
                    UserInterface.LogConsole("Setting Alpha C to: " + (EEPROM.C).ToString());

                }

                UserInterface.LogConsole("Alpha offset percentage: " + UserVariables.alphaRotationPercentage);

                UserVariables.advancedCalibration = false;
                Program.mainFormTest.SetButtonValues();
                UserVariables.advancedCalCount = 0;
                isHeuristicComplete = true;

                //check heights

            }

            GCode.checkHeights = true;
        }

        public static class Command {
            public static String HOME { get { return "G28"; } }
            public static String PROBE { get { return "G30"; } }
            public static String RESET { get { return "M112"; } }
            public static String READ_EEPROM { get { return "M205"; } }
        }
    }
}

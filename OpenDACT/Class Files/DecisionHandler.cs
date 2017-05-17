using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files
{
    static class DecisionHandler
    {
        public static void HandleInput(string message)
        {
            bool canMove = message.Contains("wait");

            Program.mainFormTest.SetUserVariables();

            if (EEPROMFunctions.tempEEPROMSet == false)
            {
                EEPROMFunctions.ParseEEPROM(message, out int parsedInt, out float parsedFloat);
                if(parsedInt != 0)
                    EEPROMFunctions.SetEEPROM((EEPROM_Position)parsedInt, parsedFloat);
            }
            else if (EEPROMFunctions.tempEEPROMSet == true && EEPROMFunctions.EEPROMReadOnly == true && EEPROMFunctions.EEPROMReadCount < 1)
            {
                //rm
            }
            else if (GCode.checkHeights == true && EEPROMFunctions.tempEEPROMSet == true && Calibration.calibrateInProgress == false && EEPROMFunctions.EEPROMReadOnly == false)
            {
                if (UserVariables.probeChoice == Printer.ProbeType.ZProbe && GCode.wasZProbeHeightSet == false && GCode.wasSet == true)
                {
                    if (HeightFunctions.ParseZProbe(message) != 1000)
                    {
                        EEPROM.zProbeHeight.Value = Convert.ToSingle(Math.Round(EEPROM.zMaxLength.Value / 6) - HeightFunctions.ParseZProbe(message));

                        GCode.wasZProbeHeightSet = true;
                        Program.mainFormTest.SetEEPROMGUIList();
                        EEPROMFunctions.SendEEPROM();
                    }
                }
                else if (canMove == true)
                {
                    //UserInterface.consoleLog.Log("position flow");
                    GCode.PositionFlow();
                }
                else if (HeightFunctions.ParseZProbe(message) != 1000 && HeightFunctions.heightsSet == false)
                {
                    HeightFunctions.SetHeights(HeightFunctions.ParseZProbe(message));
                }
            }
            else if (Calibration.calibrationState == true && Calibration.calibrateInProgress == false && GCode.checkHeights == false && EEPROMFunctions.tempEEPROMSet == true && EEPROMFunctions.EEPROMReadOnly == false && HeightFunctions.heightsSet == true)
            {
                Program.mainFormTest.SetHeightsInvoke();

                if (Calibration.calibrationState == true && HeightFunctions.checkHeightsOnly == false)
                {
                    Calibration.calibrateInProgress = true;

                    /*
                    if (EEPROMFunctions.EEPROMRequestSent == false)
                    {
                        EEPROMFunctions.readEEPROM();
                        EEPROMFunctions.EEPROMRequestSent = true;
                    }
                    */

                    if (UserVariables.advancedCalibration == false || GCode.isHeuristicComplete == true)
                    {
                        UserInterface.consoleLog.Log("Calibration Iteration Number: " + Calibration.iterationNum);
                        Calibration.Calibrate();

                        Program.mainFormTest.SetEEPROMGUIList();
                        EEPROMFunctions.SendEEPROM();

                        if (Calibration.calibrationState == false)
                        {
                            GCode.TrySend(GCode.Command.HOME);
                            Calibration.calibrationComplete = true;
                            UserInterface.consoleLog.Log("Calibration Complete");
                            //end calibration
                        }
                    }
                    else
                    {
                        UserInterface.consoleLog.Log("Heuristic Step: " + UserVariables.advancedCalCount);
                        GCode.HeuristicLearning();

                        Program.mainFormTest.SetEEPROMGUIList();
                        EEPROMFunctions.SendEEPROM();
                    }

                    Calibration.calibrateInProgress = false;
                }
                else
                {
                    if (UserVariables.probeChoice == Printer.ProbeType.FSR)
                    {
                        EEPROM.zMaxLength.Value -= UserVariables.FSROffset;
                        UserInterface.consoleLog.Log("Setting Z Max Length with adjustment for FSR");
                    }

                    GCode.TrySend(GCode.Command.HOME);

                    UserInterface.consoleLog.Log("Heights checked");
                }

                HeightFunctions.heightsSet = false;
            }
            /*
            else
            {
                UserInterface.consoleLog.Log("0: " + Calibration.calibrateInProgress + GCode.checkHeights + EEPROMFunctions.tempEEPROMSet + EEPROMFunctions.EEPROMReadOnly);
            }
            */

        }
    }
}

using OpenDACT.Class_Files.Workflow;
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

            switch (EEPROMFunctions.tempEEPROMSet)
            {
                case false:
                    EEPROMFunctions.ParseEEPROM(message, out int parsedInt, out float parsedFloat);
                    if (parsedInt != 0)
                        EEPROMFunctions.SetEEPROM((EEPROM_Position)parsedInt, parsedFloat);
                    break;
                case true:
                    if (!Calibration.calibrateInProgress && !EEPROMFunctions.EEPROMReadOnly)
                    {
                        if (GCode.checkHeights == true)
                        {
                            if (UserVariables.probeChoice == Printer.ProbeType.ZProbe)
                            {
                                if(!MeasureZProbe.zProbeMeasuringComplete)
                                    MeasureZProbe.DoNextStep();

                                //done calibrating Z-Probe
                                if (MeasureZProbe.zProbeMeasuringComplete && !MeasureZProbe.zProbeHeightSet)
                                    MeasureZProbe.SetHeight(GCode.ParseZProbe(message));
                            }
                            else if (canMove == true)
                            {
                                //UserInterface.consoleLog.Log("position flow");
                                MeasureHeights.NextCommand();
                            }
                            else if (GCode.ParseZProbe(message) != 1000 && HeightFunctions.heightsSet == false)
                            {
                                MeasureHeights.RecordHeight(GCode.ParseZProbe(message));
                            }
                        }
                        else if (Calibration.calibrationState == true && HeightFunctions.heightsSet == true)
                        {
                            Program.mainFormTest.SetHeightsInvoke();

                            if (HeightFunctions.checkHeightsOnly == false)
                            {
                                Calibration.calibrateInProgress = true;

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
                    }
                    break;
            }

        }
    }
}

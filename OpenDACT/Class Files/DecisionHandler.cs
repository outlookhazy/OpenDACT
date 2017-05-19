using OpenDACT.Class_Files.Workflow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files
{
    static class DecisionHandler
    {
        internal static LinkedList<DecisionTree> DecisionLogic = new LinkedList<DecisionTree>();
        internal static LinkedListNode<DecisionTree> CurrentLogic;

        public static void NextLogic()
        {
            CurrentLogic = CurrentLogic.Next;
            DecisionLogic.RemoveFirst();
        }

        public static void HandleInput(string message)
        {
            bool canMove = message.Contains("wait");
            Program.mainFormTest.SetUserVariables();

            if (CurrentLogic != null)
            {
                switch (CurrentLogic.Value)
                {
                    case DecisionTree.READ_EEPROM:
                        if (!EEPROM.Reading)
                        {
                            EEPROMFunctions.ReadEEPROM();
                            EEPROM.Reading = true;
                        }

                        EEPROMFunctions.ParseEEPROM(message, out int parsedInt, out float parsedFloat);
                        if (parsedInt != 0)
                            EEPROMFunctions.SetEEPROM((EEPROM_Position)parsedInt, parsedFloat);

                        if (EEPROM.ReadComplete())
                        {
                            Debug.WriteLine("Read Complete");
                            EEPROM.Reading = false;
                            Program.mainFormTest.SetEEPROMGUIList();
                            NextLogic();
                        }
                        break;
                    case DecisionTree.MEASURE_PROBE:
                        if (UserVariables.probeChoice == Printer.ProbeType.ZProbe)
                        {
                            if (!MeasureZProbe.zProbeMeasuringActive)
                            {
                                MeasureZProbe.zProbeMeasuringActive = true;
                                MeasureZProbe.zProbeMeasuringComplete = false;
                            }

                            if (!MeasureZProbe.zProbeMeasuringComplete)
                                MeasureZProbe.DoNextStep();

                            //done calibrating Z-Probe
                            if (MeasureZProbe.zProbeMeasuringComplete)
                            {
                                MeasureZProbe.SetHeight(GCode.ParseZProbe(message));
                                Program.mainFormTest.SetEEPROMGUIList();
                                EEPROMFunctions.SendEEPROM();
                                GCode.TrySend(GCode.Command.HOME);
                                UserInterface.consoleLog.Log("Z-Probe Measured");
                                MeasureZProbe.zProbeMeasuringActive = false;
                                NextLogic();
                            }
                        } else
                        {
                            EEPROM.zMaxLength.Value -= UserVariables.FSROffset;
                            UserInterface.consoleLog.Log("Setting Z Max Length with adjustment for FSR");
                            GCode.TrySend(GCode.Command.HOME);
                            NextLogic();
                        }
                        break;
                    case DecisionTree.MEASURE_HEIGHTS:
                        if (!MeasureHeights.measuringHeights)
                        {
                            MeasureHeights.heightsMeasured = false;
                            MeasureHeights.measuringHeights = true;
                        }
                        if (!MeasureHeights.heightsMeasured)
                        {
                            if (canMove == true)
                            {
                                MeasureHeights.NextCommand();
                            }
                            else if (GCode.ParseZProbe(message) != 1000)
                            {
                                MeasureHeights.RecordHeight(GCode.ParseZProbe(message));
                            }
                        }
                        else
                        {
                            MeasureHeights.measuringHeights = false;
                            NextLogic();
                        }
                        break;
                    case DecisionTree.CALIBRATE:
                        if (Calibration.calibrateInProgress)
                        {
                            Program.mainFormTest.SetHeightsInvoke();
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
                                        UserInterface.consoleLog.Log("Calibration Complete");
                                        //end calibration
                                    }
                                }
                                else
                                {

                                    HeuristicLearning.NextAction();

                                    Program.mainFormTest.SetEEPROMGUIList();
                                    EEPROMFunctions.SendEEPROM();
                                }

                                Calibration.calibrateInProgress = false;
                            }                        
                        else NextLogic();
                        break;
                }
            } else { 
                if (DecisionLogic.First != null)
                    CurrentLogic = DecisionLogic.First;
            }

            

            

            

            


                }
            
        

        internal enum DecisionTree
        {
            READ_EEPROM,
            MEASURE_HEIGHTS,
            CALIBRATE,
            MEASURE_PROBE

        }
    }
}

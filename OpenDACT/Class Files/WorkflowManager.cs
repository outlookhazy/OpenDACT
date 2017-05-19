using OpenDACT.Class_Files.Workflow_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files
{
    static class WorkflowManager
    {
        static Workflow currentWorkflow;

        public static bool ActivateWorkflow(Workflow workflow)
        {            
            if (currentWorkflow == null || currentWorkflow.Status == WorkflowState.FINISHED)
            {
                currentWorkflow = workflow;
                workflow.Start();
                return true;
            }
            else
                return false;
        }

        public static void DelegateInput(string message)
        {            
            Program.mainFormTest.SetUserVariables();

            if (currentWorkflow != null)
                if (currentWorkflow.Status == WorkflowState.STARTED)
                    currentWorkflow.RouteMessage(message);

            
            /*
                case WorkflowType.CALIBRATE:
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
            */

        }
    }
}

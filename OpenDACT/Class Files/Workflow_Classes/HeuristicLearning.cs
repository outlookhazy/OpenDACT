using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    static class HeuristicLearning
    {
        static internal bool isHeuristicComplete = false;
        static internal int heuristicStep = 0;

        public static void NextAction()
        {
            UserInterface.consoleLog.Log("Heuristic Step: " + heuristicStep);

            //find base heights
            //find heights with each value increased by 1 - HRad, tower offset 1-3, diagonal rod

            if (heuristicStep == 0)
            {//start
                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    EEPROM.stepsPerMM.Value += 1;
                    UserInterface.consoleLog.Log("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 1)
            {//get diagonal rod percentages

                UserVariables.deltaTower = ((Heights.teX - Heights.X) + (Heights.teY - Heights.Y) + (Heights.teZ - Heights.Z)) / 3;
                UserVariables.deltaOpp = ((Heights.teXOpp - Heights.XOpp) + (Heights.teYOpp - Heights.YOpp) + (Heights.teZOpp - Heights.ZOpp)) / 3;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    EEPROM.stepsPerMM.Value -= 1;
                    UserInterface.consoleLog.Log("Setting steps per millimeter to: " + (EEPROM.stepsPerMM).ToString());

                    //set Hrad +1
                    EEPROM.HRadius.Value += 1;
                    UserInterface.consoleLog.Log("Setting horizontal radius to: " + (EEPROM.HRadius.Value).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 2)
            {//get HRad percentages
                UserVariables.HRadRatio = -(Math.Abs((Heights.X - Heights.teX) + (Heights.Y - Heights.teY) + (Heights.Z - Heights.teZ) + (Heights.XOpp - Heights.teXOpp) + (Heights.YOpp - Heights.teYOpp) + (Heights.ZOpp - Heights.teZOpp))) / 6;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //reset horizontal radius
                    EEPROM.HRadius.Value -= 1;
                    UserInterface.consoleLog.Log("Setting horizontal radius to: " + (EEPROM.HRadius).ToString());

                    //set X offset
                    EEPROM.offsetX.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset X to: " + (EEPROM.offsetX).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 3)
            {//get X offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.X - Heights.teX));
                UserVariables.mainOppPerc += Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.X - Heights.teX));
                UserVariables.towPerc += (Math.Abs((Heights.Y - Heights.teY) / (Heights.X - Heights.teX)) + Math.Abs((Heights.Z - Heights.teZ) / (Heights.X - Heights.teX))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.X - Heights.teX)) + Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.X - Heights.teX))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //reset X offset
                    EEPROM.offsetX.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset X to: " + (EEPROM.offsetX).ToString());

                    //set Y offset
                    EEPROM.offsetY.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset Y to: " + (EEPROM.offsetY).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 4)
            {//get Y offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.Y - Heights.teY));
                UserVariables.mainOppPerc += Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.Y - Heights.teY));
                UserVariables.towPerc += (Math.Abs((Heights.X - Heights.teX) / (Heights.Y - Heights.teY)) + Math.Abs((Heights.Z - Heights.teZ) / (Heights.Y - Heights.teY))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.Y - Heights.teY)) + Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.Y - Heights.teY))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //reset Y offset
                    EEPROM.offsetY.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset Y to: " + (EEPROM.offsetY).ToString());

                    //set Z offset
                    EEPROM.offsetZ.Value += 80;
                    UserInterface.consoleLog.Log("Setting offset Z to: " + (EEPROM.offsetZ).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 5)
            {//get Z offset percentages

                UserVariables.offsetCorrection += Math.Abs(1 / (Heights.Z - Heights.teZ));
                UserVariables.mainOppPerc += Math.Abs((Heights.ZOpp - Heights.teZOpp) / (Heights.Z - Heights.teZ));
                UserVariables.towPerc += (Math.Abs((Heights.X - Heights.teX) / (Heights.Z - Heights.teZ)) + Math.Abs((Heights.Y - Heights.teY) / (Heights.Z - Heights.teZ))) / 2;
                UserVariables.oppPerc += (Math.Abs((Heights.XOpp - Heights.teXOpp) / (Heights.Z - Heights.teZ)) + Math.Abs((Heights.YOpp - Heights.teYOpp) / (Heights.Z - Heights.teZ))) / 2;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //set Z offset
                    EEPROM.offsetZ.Value -= 80;
                    UserInterface.consoleLog.Log("Setting offset Z to: " + (EEPROM.offsetZ).ToString());

                    //set alpha rotation offset perc X
                    EEPROM.A.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha A to: " + (EEPROM.A).ToString());
                }

                //check heights

                heuristicStep++;

            }
            else if (heuristicStep == 6)//6
            {
                //get A alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.YOpp - Heights.ZOpp) - (Heights.teYOpp - Heights.teZOpp)));

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //set alpha rotation offset perc X
                    EEPROM.A.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha A to: " + (EEPROM.A).ToString());

                    //set alpha rotation offset perc Y
                    EEPROM.B.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha B to: " + (EEPROM.B).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 7)//7
            {//get B alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.ZOpp - Heights.XOpp) - (Heights.teXOpp - Heights.teXOpp)));

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //set alpha rotation offset perc Y
                    EEPROM.B.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha B to: " + (EEPROM.B).ToString());

                    //set alpha rotation offset perc Z
                    EEPROM.C.Value += 1;
                    UserInterface.consoleLog.Log("Setting Alpha C to: " + (EEPROM.C).ToString());
                }

                //check heights

                heuristicStep++;
            }
            else if (heuristicStep == 8)//8
            {//get C alpha rotation

                UserVariables.alphaRotationPercentage += (2 / Math.Abs((Heights.XOpp - Heights.YOpp) - (Heights.teXOpp - Heights.teYOpp)));
                UserVariables.alphaRotationPercentage /= 3;

                if (Connection.serialManager.CurrentState == ConnectionState.CONNECTED)
                {
                    //set alpha rotation offset perc Z
                    EEPROM.C.Value -= 1;
                    UserInterface.consoleLog.Log("Setting Alpha C to: " + (EEPROM.C).ToString());

                }

                UserInterface.consoleLog.Log("Alpha offset percentage: " + UserVariables.alphaRotationPercentage);

                UserVariables.advancedCalibration = false;
                Program.mainFormTest.SetButtonValues();
                heuristicStep = 0;
                isHeuristicComplete = true;

                //check heights

            }

            //WorkflowManager.WorkflowQueue.AddLast(new MeasureHeightsWF());
        }

    }
}

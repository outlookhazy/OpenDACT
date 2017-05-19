using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenDACT.Class_Files.Workflow_Classes;

namespace OpenDACT.Class_Files
{
    static class Calibration
    {
        public static bool calibrateInProgress = false;
        public static bool calibrationState = false;
        public static CalibrationType calibrationSelection = CalibrationType.NORMAL;
        public static int iterationNum = 0;
        private static float tempAccuracy;

        public static void Calibrate()
        {
            switch (Calibration.calibrationSelection)
            {
                case CalibrationType.NORMAL:
                    BasicCalibration();
                    break;
                case CalibrationType.QUICK:
                    FastCalibration();
                    break;
            }            
            iterationNum++;
        }
        
        public static void FastCalibration()
        {
            //check if eeprom object remains after this function is called for the second time

            if (iterationNum == 0)
            {
                if (UserVariables.diagonalRodLength.ToString() == "")
                {
                    UserVariables.diagonalRodLength = EEPROM.diagonalRod.Value;
                    UserInterface.consoleLog.Log("Using default diagonal rod length from EEPROM");
                }
            }

            tempAccuracy = (Math.Abs(Heights.X) + Math.Abs(Heights.XOpp) + Math.Abs(Heights.Y) + Math.Abs(Heights.YOpp) + Math.Abs(Heights.Z) + Math.Abs(Heights.ZOpp)) / 6;
            Program.mainFormTest.SetAccuracyPoint(iterationNum, tempAccuracy);
            CheckAccuracy(Heights.X, Heights.XOpp, Heights.Y, Heights.YOpp, Heights.Z, Heights.ZOpp);

            if (calibrationState == true)
            {
                TowerOffsets(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                AlphaRotation(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                StepsPMM(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                HRad(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
            }            
        }

        public static void BasicCalibration()
        {
            //check if eeprom object remains after this function is called for the second time
            if (iterationNum == 0)
            {
                if (UserVariables.diagonalRodLength.ToString() == "")
                {
                    UserVariables.diagonalRodLength = EEPROM.diagonalRod.Value;
                    UserInterface.consoleLog.Log("Using default diagonal rod length from EEPROM");
                }
            }

            tempAccuracy = (Math.Abs(Heights.X) + Math.Abs(Heights.XOpp) + Math.Abs(Heights.Y) + Math.Abs(Heights.YOpp) + Math.Abs(Heights.Z) + Math.Abs(Heights.ZOpp)) / 6;
            Program.mainFormTest.SetAccuracyPoint(iterationNum, tempAccuracy);
            CheckAccuracy(Heights.X, Heights.XOpp, Heights.Y, Heights.YOpp, Heights.Z, Heights.ZOpp);

            if (calibrationState == true)
            {
                bool spm = (Heights.X + Heights.Y + Heights.Z) / 3 > (Heights.XOpp + Heights.YOpp + Heights.ZOpp) / 3 + UserVariables.accuracy || (Heights.X + Heights.Y + Heights.Z) / 3 < (Heights.XOpp + Heights.YOpp + Heights.ZOpp) / 3 - UserVariables.accuracy;//returns false if drad does not need corrected
                
                bool tower = Math.Abs(Math.Max(Heights.X, Math.Max(Heights.Y, Heights.Z)) - Math.Min(Heights.X, Math.Min(Heights.Y, Heights.Z))) > UserVariables.accuracy;

                bool alpha = Heights.XOpp > Heights.YOpp + UserVariables.accuracy || Heights.XOpp < Heights.YOpp - UserVariables.accuracy || Heights.XOpp > Heights.ZOpp + UserVariables.accuracy || Heights.XOpp < Heights.ZOpp - UserVariables.accuracy ||
                             Heights.YOpp > Heights.XOpp + UserVariables.accuracy || Heights.YOpp < Heights.XOpp - UserVariables.accuracy || Heights.YOpp > Heights.ZOpp + UserVariables.accuracy || Heights.YOpp < Heights.ZOpp - UserVariables.accuracy ||
                             Heights.ZOpp > Heights.YOpp + UserVariables.accuracy || Heights.ZOpp < Heights.YOpp - UserVariables.accuracy || Heights.ZOpp > Heights.XOpp + UserVariables.accuracy || Heights.ZOpp < Heights.XOpp - UserVariables.accuracy;//returns false if tower offsets do not need corrected

                bool hrad =  Heights.X < Heights.Y + UserVariables.accuracy && Heights.X > Heights.Y - UserVariables.accuracy &&
                             Heights.X < Heights.Z + UserVariables.accuracy && Heights.X > Heights.Z - UserVariables.accuracy &&
                             Heights.X < Heights.YOpp + UserVariables.accuracy && Heights.X > Heights.YOpp - UserVariables.accuracy &&
                             Heights.X < Heights.ZOpp + UserVariables.accuracy && Heights.X > Heights.ZOpp - UserVariables.accuracy &&
                             Heights.X < Heights.XOpp + UserVariables.accuracy && Heights.X > Heights.XOpp - UserVariables.accuracy &&
                             Heights.XOpp < Heights.X + UserVariables.accuracy && Heights.XOpp > Heights.X - UserVariables.accuracy &&
                             Heights.XOpp < Heights.Y + UserVariables.accuracy && Heights.XOpp > Heights.Y - UserVariables.accuracy &&
                             Heights.XOpp < Heights.Z + UserVariables.accuracy && Heights.XOpp > Heights.Z - UserVariables.accuracy &&
                             Heights.XOpp < Heights.YOpp + UserVariables.accuracy && Heights.XOpp > Heights.YOpp - UserVariables.accuracy &&
                             Heights.XOpp < Heights.ZOpp + UserVariables.accuracy && Heights.XOpp > Heights.ZOpp - UserVariables.accuracy &&
                             Heights.Y < Heights.X + UserVariables.accuracy && Heights.Y > Heights.X - UserVariables.accuracy &&
                             Heights.Y < Heights.Z + UserVariables.accuracy && Heights.Y > Heights.Z - UserVariables.accuracy &&
                             Heights.Y < Heights.XOpp + UserVariables.accuracy && Heights.Y > Heights.XOpp - UserVariables.accuracy &&
                             Heights.Y < Heights.YOpp + UserVariables.accuracy && Heights.Y > Heights.YOpp - UserVariables.accuracy &&
                             Heights.Y < Heights.ZOpp + UserVariables.accuracy && Heights.Y > Heights.ZOpp - UserVariables.accuracy &&
                             Heights.YOpp < Heights.X + UserVariables.accuracy && Heights.YOpp > Heights.X - UserVariables.accuracy &&
                             Heights.YOpp < Heights.Y + UserVariables.accuracy && Heights.YOpp > Heights.Y - UserVariables.accuracy &&
                             Heights.YOpp < Heights.Z + UserVariables.accuracy && Heights.YOpp > Heights.Z - UserVariables.accuracy &&
                             Heights.YOpp < Heights.XOpp + UserVariables.accuracy && Heights.YOpp > Heights.XOpp - UserVariables.accuracy &&
                             Heights.YOpp < Heights.ZOpp + UserVariables.accuracy && Heights.YOpp > Heights.ZOpp - UserVariables.accuracy &&
                             Heights.Z < Heights.X + UserVariables.accuracy && Heights.Z > Heights.X - UserVariables.accuracy &&
                             Heights.Z < Heights.Y + UserVariables.accuracy && Heights.Z > Heights.Y - UserVariables.accuracy &&
                             Heights.Z < Heights.XOpp + UserVariables.accuracy && Heights.Z > Heights.XOpp - UserVariables.accuracy &&
                             Heights.Z < Heights.YOpp + UserVariables.accuracy && Heights.Z > Heights.YOpp - UserVariables.accuracy &&
                             Heights.Z < Heights.ZOpp + UserVariables.accuracy && Heights.Z > Heights.ZOpp - UserVariables.accuracy &&
                             Heights.ZOpp < Heights.X + UserVariables.accuracy && Heights.ZOpp > Heights.X - UserVariables.accuracy &&
                             Heights.ZOpp < Heights.Y + UserVariables.accuracy && Heights.ZOpp > Heights.Y - UserVariables.accuracy &&
                             Heights.ZOpp < Heights.Z + UserVariables.accuracy && Heights.ZOpp > Heights.Z - UserVariables.accuracy &&
                             Heights.ZOpp < Heights.XOpp + UserVariables.accuracy && Heights.ZOpp > Heights.XOpp - UserVariables.accuracy &&
                             Heights.ZOpp < Heights.YOpp + UserVariables.accuracy && Heights.ZOpp > Heights.YOpp - UserVariables.accuracy;

                UserInterface.consoleLog.Log("Tower:" + tower + " SPM:" + spm + " Alpha:" + alpha + " HRad:" + hrad);

                if (tower)
                {
                    TowerOffsets(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                }
                else if (alpha)
                {
                    AlphaRotation(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                }
                else if (spm)
                {
                    StepsPMM(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                }
                else if (hrad)
                {
                    HRad(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                }
                
            }            
        }
        

        private static void CheckAccuracy(float offset_X, float offset_XOpp, float offset_Y, float offset_YOpp, float offset_Z, float offset_ZOpp)
        {
            float accuracy = UserVariables.accuracy;

            if (Math.Abs(offset_X) <= accuracy && 
                Math.Abs(offset_XOpp) <= accuracy && 
                Math.Abs(offset_Y) <= accuracy &&
                Math.Abs(offset_YOpp) <= accuracy &&
                Math.Abs(offset_Z) <= accuracy &&
                Math.Abs(offset_ZOpp) <= accuracy
                ){
                if (UserVariables.probeChoice == Printer.ProbeType.FSR)
                {
                    EEPROM.zMaxLength.Value -= UserVariables.FSROffset;
                    UserInterface.consoleLog.Log("Setting Z Max Length with adjustment for FSR");
                }
                UserInterface.consoleLog.Log("Calibration Meets Accuracy");
                calibrationState = false;
            }
            else
            {
                //WorkflowManager.WorkflowQueue.AddLast(new MeasureHeightsWF());
                UserInterface.consoleLog.Log("Continuing Calibration");
            }
        }

        private static void HRad(ref float X, ref float XOpp, ref float Y, ref float YOpp, ref float Z, ref float ZOpp)
        {
            float HRadSA = ((X + XOpp + Y + YOpp + Z + ZOpp) / 6);
            float HRadRatio = UserVariables.HRadRatio;

            EEPROM.HRadius.Value += (HRadSA / HRadRatio);

            X -= HRadSA;
            Y -= HRadSA;
            Z -= HRadSA;
            XOpp -= HRadSA;
            YOpp -= HRadSA;
            ZOpp -= HRadSA;

            UserInterface.consoleLog.Log("HRad:" + EEPROM.HRadius.ToString());
        }

        private static void TowerOffsets(ref float X, ref float XOpp, ref float Y, ref float YOpp, ref float Z, ref float ZOpp)
        {
            int j = 0;
            float accuracy = UserVariables.calculationAccuracy;
            float tempX2 = X;
            float tempXOpp2 = XOpp;
            float tempY2 = Y;
            float tempYOpp2 = YOpp;
            float tempZ2 = Z;
            float tempZOpp2 = ZOpp;
            float offsetX = EEPROM.offsetX.Value;
            float offsetY = EEPROM.offsetY.Value;
            float offsetZ = EEPROM.offsetZ.Value;
            float stepsPerMM = EEPROM.stepsPerMM.Value;

            float towMain = UserVariables.offsetCorrection;//0.6
            float oppMain = UserVariables.mainOppPerc;//0.5
            float towSub = UserVariables.towPerc;//0.3
            float oppSub = UserVariables.oppPerc;//-0.25

            while (j < 100)
            {
                if (Math.Abs(tempX2) > UserVariables.accuracy || Math.Abs(tempY2) > UserVariables.accuracy || Math.Abs(tempZ2) > UserVariables.accuracy)
                {
                    offsetX -= tempX2 * stepsPerMM * (1 / towMain);

                    tempXOpp2 -= tempX2 * (oppMain / towMain);
                    tempY2 -= tempX2 * (towSub / towMain);
                    tempYOpp2 -= tempX2 * (-oppSub / towMain);
                    tempZ2 -= tempX2 * (towSub / towMain);
                    tempZOpp2 -= tempX2 * (-oppSub / towMain);
                    tempX2 -= tempX2 / 1;

                    offsetY -= tempY2 * stepsPerMM * (1 / towMain);

                    tempYOpp2 -= tempY2 * (oppMain / towMain);
                    tempX2 -= tempY2 * (towSub / towMain);
                    tempXOpp2 -= tempY2 * (-oppSub / towMain);
                    tempZ2 -= tempY2 * (towSub / towMain);
                    tempZOpp2 -= tempY2 * (-oppSub / towMain);
                    tempY2 -= tempY2 / 1;

                    offsetZ -= tempZ2 * stepsPerMM * (1 / towMain);

                    tempZOpp2 -= tempZ2 * (oppMain / towMain);
                    tempX2 -= tempZ2 * (towSub / towMain);
                    tempXOpp2 += tempZ2 * (-oppSub / towMain);
                    tempY2 -= tempZ2 * (towSub / towMain);
                    tempYOpp2 -= tempZ2 * (-oppSub / towMain);
                    tempZ2 -= tempZ2 / 1;

                    tempX2 = Validation.CheckZero(tempX2);
                    tempY2 = Validation.CheckZero(tempY2);
                    tempZ2 = Validation.CheckZero(tempZ2);
                    tempXOpp2 = Validation.CheckZero(tempXOpp2);
                    tempYOpp2 = Validation.CheckZero(tempYOpp2);
                    tempZOpp2 = Validation.CheckZero(tempZOpp2);

                    if (Math.Abs(tempX2) <= UserVariables.accuracy && Math.Abs(tempY2) <= UserVariables.accuracy && Math.Abs(tempZ2) <= UserVariables.accuracy)
                    {
                        UserInterface.consoleLog.Log("VHeights :" + tempX2 + " " + tempXOpp2 + " " + tempY2 + " " + tempYOpp2 + " " + tempZ2 + " " + tempZOpp2);
                        UserInterface.consoleLog.Log("Offs :" + offsetX + " " + offsetY + " " + offsetZ);
                        UserInterface.consoleLog.Log("No Hrad correction");

                        float smallest = Math.Min(offsetX, Math.Min(offsetY, offsetZ));

                        offsetX -= smallest;
                        offsetY -= smallest;
                        offsetZ -= smallest;

                        UserInterface.consoleLog.Log("Offs :" + offsetX + " " + offsetY + " " + offsetZ);

                        X = tempX2;
                        XOpp = tempXOpp2;
                        Y = tempY2;
                        YOpp = tempYOpp2;
                        Z = tempZ2;
                        ZOpp = tempZOpp2;

                        //round to the nearest whole number
                        EEPROM.offsetX.Value = Convert.ToInt32(offsetX);
                        EEPROM.offsetY.Value = Convert.ToInt32(offsetY);
                        EEPROM.offsetZ.Value = Convert.ToInt32(offsetZ);

                        j = 100;
                    }
                    else if (j == 99)
                    {
                        UserInterface.consoleLog.Log("VHeights :" + tempX2 + " " + tempXOpp2 + " " + tempY2 + " " + tempYOpp2 + " " + tempZ2 + " " + tempZOpp2);
                        UserInterface.consoleLog.Log("Offs :" + offsetX + " " + offsetY + " " + offsetZ);
                        float dradCorr = tempX2 * -1.25F;
                        float HRadRatio = UserVariables.HRadRatio;

                        EEPROM.HRadius.Value += dradCorr;

                        EEPROM.offsetX.Value = 0;
                        EEPROM.offsetY.Value = 0;
                        EEPROM.offsetZ.Value = 0;

                        //hradsa = dradcorr
                        //solve inversely from previous method
                        float HRadOffset = HRadRatio * dradCorr;

                        tempX2 -= HRadOffset;
                        tempY2 -= HRadOffset;
                        tempZ2 -= HRadOffset;
                        tempXOpp2 -= HRadOffset;
                        tempYOpp2 -= HRadOffset;
                        tempZOpp2 -= HRadOffset;

                        UserInterface.consoleLog.Log("Hrad correction: " + dradCorr);
                        UserInterface.consoleLog.Log("HRad: " + EEPROM.HRadius.ToString());

                        j = 0;
                    }
                    else
                    {
                        j++;
                    }

                    //UserInterface.consoleLog.Log("Offs :" + offsetX + " " + offsetY + " " + offsetZ);
                    //UserInterface.consoleLog.Log("VHeights :" + tempX2 + " " + tempXOpp2 + " " + tempY2 + " " + tempYOpp2 + " " + tempZ2 + " " + tempZOpp2);
                }
                else
                {
                    j = 100;

                    UserInterface.consoleLog.Log("Tower Offsets and Delta Radii Calibrated");
                }
            }

            if (EEPROM.offsetX.Value > 1000 || EEPROM.offsetY.Value > 1000 || EEPROM.offsetZ.Value > 1000)
            {
                UserInterface.consoleLog.Log("Tower offset calibration error, setting default values.");
                UserInterface.consoleLog.Log("Tower offsets before damage prevention: X" + offsetX + " Y" + offsetY + " Z" + offsetZ);
                offsetX = 0;
                offsetY = 0;
                offsetZ = 0;
            }
        }

        private static void AlphaRotation(ref float X, ref float XOpp, ref float Y, ref float YOpp, ref float Z, ref float ZOpp)
        {
            float offsetX = EEPROM.offsetX.Value;
            float offsetY = EEPROM.offsetY.Value;
            float offsetZ = EEPROM.offsetZ.Value;
            float accuracy = UserVariables.accuracy;

            //change to object
            float alphaRotationPercentage = UserVariables.alphaRotationPercentage;

            int k = 0;
            while (k < 100)
            {
                //X Alpha Rotation
                if (YOpp > ZOpp)
                {
                    float ZYOppAvg = (YOpp - ZOpp) / 2;
                    EEPROM.A.Value = EEPROM.A.Value + (ZYOppAvg * alphaRotationPercentage); // (0.5/((diff y0 and z0 at X + 0.5)-(diff y0 and z0 at X = 0))) * 2 = 1.75
                    YOpp = YOpp - ZYOppAvg;
                    ZOpp = ZOpp + ZYOppAvg;
                }
                else if (YOpp < ZOpp)
                {
                    float ZYOppAvg = (ZOpp - YOpp) / 2;

                    EEPROM.A.Value = EEPROM.A.Value - (ZYOppAvg * alphaRotationPercentage);
                    YOpp = YOpp + ZYOppAvg;
                    ZOpp = ZOpp - ZYOppAvg;
                }

                //Y Alpha Rotation
                if (ZOpp > XOpp)
                {
                    float XZOppAvg = (ZOpp - XOpp) / 2;
                    EEPROM.B.Value = EEPROM.B.Value + (XZOppAvg * alphaRotationPercentage);
                    ZOpp = ZOpp - XZOppAvg;
                    XOpp = XOpp + XZOppAvg;
                }
                else if (ZOpp < XOpp)
                {
                    float XZOppAvg = (XOpp - ZOpp) / 2;

                    EEPROM.B.Value = EEPROM.B.Value - (XZOppAvg * alphaRotationPercentage);
                    ZOpp = ZOpp + XZOppAvg;
                    XOpp = XOpp - XZOppAvg;
                }
                //Z Alpha Rotation
                if (XOpp > YOpp)
                {
                    float YXOppAvg = (XOpp - YOpp) / 2;
                    EEPROM.C.Value = EEPROM.C.Value + (YXOppAvg * alphaRotationPercentage);
                    XOpp = XOpp - YXOppAvg;
                    YOpp = YOpp + YXOppAvg;
                }
                else if (XOpp < YOpp)
                {
                    float YXOppAvg = (YOpp - XOpp) / 2;

                    EEPROM.C.Value = EEPROM.C.Value - (YXOppAvg * alphaRotationPercentage);
                    XOpp = XOpp + YXOppAvg;
                    YOpp = YOpp - YXOppAvg;
                }

                //determine if value is close enough
                float hTow = Math.Max(Math.Max(XOpp, YOpp), ZOpp);
                float lTow = Math.Min(Math.Min(XOpp, YOpp), ZOpp);
                float towDiff = hTow - lTow;

                if (towDiff < UserVariables.calculationAccuracy && towDiff > -UserVariables.calculationAccuracy)
                {
                    k = 100;

                    //log
                    UserInterface.consoleLog.Log("ABC:" + EEPROM.A + " " + EEPROM.B + " " + EEPROM.C);
                }
                else
                {
                    k++;
                }
            }
        }
/// <summary>
/// /////////////////////////
/// </summary>
/// <param name="X"></param>
/// <param name="XOpp"></param>
/// <param name="Y"></param>
/// <param name="YOpp"></param>
/// <param name="Z"></param>
/// <param name="ZOpp"></param>
        private static void StepsPMM(ref float X, ref float XOpp, ref float Y, ref float YOpp, ref float Z, ref float ZOpp)
        {
            /*
            float diagChange = 1 / UserVariables.deltaOpp;
            float towChange = 1 / UserVariables.deltaTower;
            */

            float diagChange = 1 / UserVariables.deltaOpp;
            float towChange = 1 / UserVariables.deltaTower;

            float XYZ = (X + Y + Z) / 3;
            float XYZOpp = (XOpp + YOpp + ZOpp) / 3;

            EEPROM.stepsPerMM.Value -= (XYZ - XYZOpp) * ((diagChange + towChange) / 2);

            //XYZ is increased by the offset

            X += (XYZ - XYZOpp) * diagChange * 1;
            Y += (XYZ - XYZOpp) * diagChange * 1;
            Z += (XYZ - XYZOpp) * diagChange * 1;
            XOpp += (XYZ - XYZOpp) * towChange * 0.75f;
            YOpp += (XYZ - XYZOpp) * towChange * 0.75f;
            ZOpp += (XYZ - XYZOpp) * towChange * 0.75f;


            UserInterface.consoleLog.Log("Steps per Millimeter: " + EEPROM.stepsPerMM.ToString());
        }

        /*
        private static void linearRegression(float[] xVals, float[] yVals, int inclusiveStart, int exclusiveEnd, out float rsquared, out float yintercept, out float slope)
        {
            float sumOfX = 0;
            float sumOfY = 0;
            float sumOfXSq = 0;
            float sumOfYSq = 0;
            float ssX = 0;
            float ssY = 0;
            float sumCodeviates = 0;
            float sCo = 0;
            float count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                float x = xVals[ctr];
                float y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            float RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            float RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            float meanX = sumOfX / count;
            float meanY = sumOfY / count;
            float dblR = RNumerator / Convert.ToSingle(Math.Sqrt(RDenom));
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
        */

        public enum CalibrationType
        {
            NORMAL,
            QUICK            
        }
    }
}

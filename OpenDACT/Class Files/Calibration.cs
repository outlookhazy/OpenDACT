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
        
        
        /*
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

            Program.mainFormTest.SetAccuracyPoint(iterationNum, Calibration.AverageAccuracy());
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
        }*/

        public static EEPROM HRad(HeightMap Heights, EEPROM sourceEEPROM)
        {
            EEPROM EEPROM = sourceEEPROM.Copy();
            float HRadSA = ((Heights[HeightMap.Position.X].Z + Heights[HeightMap.Position.XOPP].Z + Heights[HeightMap.Position.Y].Z + Heights[HeightMap.Position.YOPP].Z + Heights[HeightMap.Position.Z].Z + Heights[HeightMap.Position.ZOPP].Z) / 6);
            float HRadRatio = UserVariables.HRadRatio;

            EEPROM[EEPROM_POSITION.HRadius].Value += (HRadSA / HRadRatio);

            Heights[HeightMap.Position.X].Z -= HRadSA;
            Heights[HeightMap.Position.Y].Z -= HRadSA;
            Heights[HeightMap.Position.Z].Z -= HRadSA;
            Heights[HeightMap.Position.XOPP].Z -= HRadSA;
            Heights[HeightMap.Position.YOPP].Z -= HRadSA;
            Heights[HeightMap.Position.ZOPP].Z -= HRadSA;

            UserInterface.consoleLog.Log("HRad:" + EEPROM[EEPROM_POSITION.HRadius].Value.ToString());
            return EEPROM;
        }

        public static EEPROM TowerOffsets(HeightMap Heights, EEPROM sourceEEPROM)
        {
            EEPROM EEPROM = sourceEEPROM.Copy();
            int j = 0;
            float accuracy = UserVariables.calculationAccuracy;
            float tempX2 = Heights[HeightMap.Position.X].Z;
            float tempXOpp2 = Heights[HeightMap.Position.XOPP].Z;
            float tempY2 = Heights[HeightMap.Position.Y].Z;
            float tempYOpp2 = Heights[HeightMap.Position.YOPP].Z;
            float tempZ2 = Heights[HeightMap.Position.Z].Z;
            float tempZOpp2 = Heights[HeightMap.Position.ZOPP].Z;
            float offsetX = EEPROM[EEPROM_POSITION.offsetX].Value;
            float offsetY = EEPROM[EEPROM_POSITION.offsetY].Value;
            float offsetZ = EEPROM[EEPROM_POSITION.offsetZ].Value;
            float stepsPerMM = EEPROM[EEPROM_POSITION.StepsPerMM].Value;

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

                    tempX2 = Util.CheckZero(tempX2);
                    tempY2 = Util.CheckZero(tempY2);
                    tempZ2 = Util.CheckZero(tempZ2);
                    tempXOpp2 = Util.CheckZero(tempXOpp2);
                    tempYOpp2 = Util.CheckZero(tempYOpp2);
                    tempZOpp2 = Util.CheckZero(tempZOpp2);

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

                        Heights[HeightMap.Position.X].Z = tempX2;
                        Heights[HeightMap.Position.XOPP].Z = tempXOpp2;
                        Heights[HeightMap.Position.Y].Z = tempY2;
                        Heights[HeightMap.Position.YOPP].Z = tempYOpp2;
                        Heights[HeightMap.Position.Z].Z = tempZ2;
                        Heights[HeightMap.Position.ZOPP].Z = tempZOpp2;

                        //round to the nearest whole number
                        EEPROM[EEPROM_POSITION.offsetX].Value = Convert.ToInt32(offsetX);
                        EEPROM[EEPROM_POSITION.offsetY].Value = Convert.ToInt32(offsetY);
                        EEPROM[EEPROM_POSITION.offsetZ].Value = Convert.ToInt32(offsetZ);

                        j = 100;
                    }
                    else if (j == 99)
                    {
                        UserInterface.consoleLog.Log("VHeights :" + tempX2 + " " + tempXOpp2 + " " + tempY2 + " " + tempYOpp2 + " " + tempZ2 + " " + tempZOpp2);
                        UserInterface.consoleLog.Log("Offs :" + offsetX + " " + offsetY + " " + offsetZ);
                        float dradCorr = tempX2 * -1.25F;
                        float HRadRatio = UserVariables.HRadRatio;

                        EEPROM[EEPROM_POSITION.HRadius].Value += dradCorr;

                        EEPROM[EEPROM_POSITION.offsetX].Value = 0;
                        EEPROM[EEPROM_POSITION.offsetY].Value = 0;
                        EEPROM[EEPROM_POSITION.offsetZ].Value = 0;

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
                        UserInterface.consoleLog.Log("HRad: " + EEPROM[EEPROM_POSITION.HRadius].Value.ToString());

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

            if (EEPROM[EEPROM_POSITION.offsetX].Value > 1000 || EEPROM[EEPROM_POSITION.offsetY].Value > 1000 || EEPROM[EEPROM_POSITION.offsetZ].Value > 1000)
            {
                UserInterface.consoleLog.Log("Tower offset calibration error, setting default values.");
                UserInterface.consoleLog.Log("Tower offsets before damage prevention: X" + offsetX + " Y" + offsetY + " Z" + offsetZ);
                offsetX = 0;
                offsetY = 0;
                offsetZ = 0;
            }
            return EEPROM;
        }

        public static EEPROM AlphaRotation(HeightMap Heights, EEPROM sourceEEPROM)
        {
            EEPROM EEPROM = sourceEEPROM.Copy();
            float offsetX = EEPROM[EEPROM_POSITION.offsetX].Value;
            float offsetY = EEPROM[EEPROM_POSITION.offsetY].Value;
            float offsetZ = EEPROM[EEPROM_POSITION.offsetZ].Value;
            float accuracy = UserVariables.accuracy;

            //change to object
            float alphaRotationPercentage = UserVariables.alphaRotationPercentage;

            int k = 0;
            while (k < 100)
            {                
                //X Alpha Rotation
                if (Heights[HeightMap.Position.YOPP].Z > Heights[HeightMap.Position.ZOPP].Z)
                {
                    float ZYOppAvg = (Heights[HeightMap.Position.YOPP].Z - Heights[HeightMap.Position.ZOPP].Z) / 2;
                    EEPROM[EEPROM_POSITION.A].Value = EEPROM[EEPROM_POSITION.A].Value + (ZYOppAvg * alphaRotationPercentage); // (0.5/((diff y0 and z0 at X + 0.5)-(diff y0 and z0 at X = 0))) * 2 = 1.75
                    Heights[HeightMap.Position.YOPP].Z = Heights[HeightMap.Position.YOPP].Z - ZYOppAvg;
                    Heights[HeightMap.Position.ZOPP].Z = Heights[HeightMap.Position.ZOPP].Z + ZYOppAvg;
                }
                else if (Heights[HeightMap.Position.YOPP].Z < Heights[HeightMap.Position.ZOPP].Z)
                {
                    float ZYOppAvg = (Heights[HeightMap.Position.ZOPP].Z - Heights[HeightMap.Position.YOPP].Z) / 2;

                    EEPROM[EEPROM_POSITION.A].Value = EEPROM[EEPROM_POSITION.A].Value - (ZYOppAvg * alphaRotationPercentage);
                    Heights[HeightMap.Position.YOPP].Z = Heights[HeightMap.Position.YOPP].Z + ZYOppAvg;
                    Heights[HeightMap.Position.ZOPP].Z = Heights[HeightMap.Position.ZOPP].Z - ZYOppAvg;
                }

                //Y Alpha Rotation
                if (Heights[HeightMap.Position.ZOPP].Z > Heights[HeightMap.Position.XOPP].Z)
                {
                    float XZOppAvg = (Heights[HeightMap.Position.ZOPP].Z - Heights[HeightMap.Position.XOPP].Z) / 2;
                    EEPROM[EEPROM_POSITION.B].Value = EEPROM[EEPROM_POSITION.B].Value + (XZOppAvg * alphaRotationPercentage);
                    Heights[HeightMap.Position.ZOPP].Z = Heights[HeightMap.Position.ZOPP].Z - XZOppAvg;
                    Heights[HeightMap.Position.XOPP].Z = Heights[HeightMap.Position.XOPP].Z + XZOppAvg;
                }
                else if (Heights[HeightMap.Position.ZOPP].Z < Heights[HeightMap.Position.XOPP].Z)
                {
                    float XZOppAvg = (Heights[HeightMap.Position.XOPP].Z - Heights[HeightMap.Position.ZOPP].Z) / 2;

                    EEPROM[EEPROM_POSITION.B].Value = EEPROM[EEPROM_POSITION.B].Value - (XZOppAvg * alphaRotationPercentage);
                    Heights[HeightMap.Position.ZOPP].Z = Heights[HeightMap.Position.ZOPP].Z + XZOppAvg;
                    Heights[HeightMap.Position.XOPP].Z = Heights[HeightMap.Position.XOPP].Z - XZOppAvg;
                }
                //Z Alpha Rotation
                if (Heights[HeightMap.Position.XOPP].Z > Heights[HeightMap.Position.YOPP].Z)
                {
                    float YXOppAvg = (Heights[HeightMap.Position.XOPP].Z - Heights[HeightMap.Position.YOPP].Z) / 2;
                    EEPROM[EEPROM_POSITION.C].Value = EEPROM[EEPROM_POSITION.C].Value + (YXOppAvg * alphaRotationPercentage);
                    Heights[HeightMap.Position.XOPP].Z = Heights[HeightMap.Position.XOPP].Z - YXOppAvg;
                    Heights[HeightMap.Position.YOPP].Z = Heights[HeightMap.Position.YOPP].Z + YXOppAvg;
                }
                else if (Heights[HeightMap.Position.XOPP].Z < Heights[HeightMap.Position.YOPP].Z)
                {
                    float YXOppAvg = (Heights[HeightMap.Position.YOPP].Z - Heights[HeightMap.Position.XOPP].Z) / 2;

                    EEPROM[EEPROM_POSITION.C].Value = EEPROM[EEPROM_POSITION.C].Value - (YXOppAvg * alphaRotationPercentage);
                    Heights[HeightMap.Position.XOPP].Z = Heights[HeightMap.Position.XOPP].Z + YXOppAvg;
                    Heights[HeightMap.Position.YOPP].Z = Heights[HeightMap.Position.YOPP].Z - YXOppAvg;
                }

                //determine if value is close enough
                float hTow = Math.Max(Math.Max(Heights[HeightMap.Position.XOPP].Z, Heights[HeightMap.Position.YOPP].Z), Heights[HeightMap.Position.ZOPP].Z);
                float lTow = Math.Min(Math.Min(Heights[HeightMap.Position.XOPP].Z, Heights[HeightMap.Position.YOPP].Z), Heights[HeightMap.Position.ZOPP].Z);
                float towDiff = hTow - lTow;

                if (towDiff < UserVariables.calculationAccuracy && towDiff > -UserVariables.calculationAccuracy)
                {
                    k = 100;

                    //log
                    UserInterface.consoleLog.Log("ABC:" + EEPROM[EEPROM_POSITION.A].Value + " " + EEPROM[EEPROM_POSITION.B].Value + " " + EEPROM[EEPROM_POSITION.C].Value);
                }
                else
                {
                    k++;
                }
            }
            return EEPROM;
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
        public static EEPROM StepsPMM(HeightMap Heights, EEPROM sourceEEPROM)
        {
            /*
            float diagChange = 1 / UserVariables.deltaOpp;
            float towChange = 1 / UserVariables.deltaTower;
            */

            EEPROM EEPROM = sourceEEPROM.Copy();

            float diagChange = 1 / UserVariables.deltaOpp;
            float towChange = 1 / UserVariables.deltaTower;

            float XYZ = (Heights[HeightMap.Position.X].Z + Heights[HeightMap.Position.Y].Z + Heights[HeightMap.Position.Z].Z) / 3;
            float XYZOpp = (Heights[HeightMap.Position.XOPP].Z + Heights[HeightMap.Position.YOPP].Z + Heights[HeightMap.Position.ZOPP].Z) / 3;

            EEPROM[EEPROM_POSITION.StepsPerMM].Value -= (XYZ - XYZOpp) * ((diagChange + towChange) / 2);

            //XYZ is increased by the offset

            Heights[HeightMap.Position.X].Z += (XYZ - XYZOpp) * diagChange * 1;
            Heights[HeightMap.Position.Y].Z += (XYZ - XYZOpp) * diagChange * 1;
            Heights[HeightMap.Position.Z].Z += (XYZ - XYZOpp) * diagChange * 1;
            Heights[HeightMap.Position.XOPP].Z += (XYZ - XYZOpp) * towChange * 0.75f;
            Heights[HeightMap.Position.YOPP].Z += (XYZ - XYZOpp) * towChange * 0.75f;
            Heights[HeightMap.Position.ZOPP].Z += (XYZ - XYZOpp) * towChange * 0.75f;


            UserInterface.consoleLog.Log("Steps per Millimeter: " + EEPROM[EEPROM_POSITION.StepsPerMM].Value.ToString());
            return EEPROM;
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
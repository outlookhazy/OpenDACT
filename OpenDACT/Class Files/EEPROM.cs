using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using OpenDACT.Class_Files.Workflow_Classes;

namespace OpenDACT.Class_Files
{
    public static class EEPROM
    {
        public static bool Reading { get; set; }

        public static EEPROM_Variable stepsPerMM = new EEPROM_Variable("stepsPerMM",3, EEPROM_Position.StepsPerMM);
        public static float tempSPM;
        public static EEPROM_Variable zMaxLength = new EEPROM_Variable("zMaxLength",3, EEPROM_Position.zMaxLength);
        public static EEPROM_Variable zProbeHeight = new EEPROM_Variable("zProbeHeight",3, EEPROM_Position.zProbeHeight);
        public static EEPROM_Variable zProbeSpeed = new EEPROM_Variable("zProbeSpeed",3, EEPROM_Position.zProbeSpeed);
        public static EEPROM_Variable diagonalRod = new EEPROM_Variable("diagonalRod",3, EEPROM_Position.diagonalRod);
        public static EEPROM_Variable HRadius = new EEPROM_Variable("HRadius",3, EEPROM_Position.HRadius);
        public static EEPROM_Variable offsetX = new EEPROM_Variable("offsetX",1, EEPROM_Position.offsetX);
        public static EEPROM_Variable offsetY = new EEPROM_Variable("offsetY",1, EEPROM_Position.offsetY);
        public static EEPROM_Variable offsetZ = new EEPROM_Variable("offsetZ",1, EEPROM_Position.offsetZ);
        public static EEPROM_Variable A = new EEPROM_Variable("A",3, EEPROM_Position.A);
        public static EEPROM_Variable B = new EEPROM_Variable("B",3, EEPROM_Position.B);
        public static EEPROM_Variable C = new EEPROM_Variable("C",3, EEPROM_Position.C);
        public static EEPROM_Variable DA = new EEPROM_Variable("DA",3, EEPROM_Position.DA);
        public static EEPROM_Variable DB = new EEPROM_Variable("DB",3, EEPROM_Position.DB);
        public static EEPROM_Variable DC = new EEPROM_Variable("DC",3, EEPROM_Position.DC);

        public static List<EEPROM_Variable> GetVarList()
        {
            return new List<EEPROM_Variable>
            {
                stepsPerMM,
                zMaxLength,
                zProbeHeight,
                zProbeSpeed,
                diagonalRod,
                HRadius,
                offsetX,
                offsetY,
                offsetZ,
                A,
                B,
                C,
                DA,
                DB,
                DC
            };
        }

        public static bool ReadComplete()
        {
            foreach(EEPROM_Variable v in EEPROM.GetVarList())
            {
                if (v.Pending)
                    return false;
            }
            return true;
        }

        public static void SetPending()
        {
            foreach (EEPROM_Variable v in EEPROM.GetVarList())
            {
                v.Pending = true;
            }
        }
    }

    public enum EEPROM_Position:int {
        StepsPerMM = 11,
        zMaxLength = 153,
        zProbeHeight = 808,
        zProbeSpeed = 812,
        diagonalRod = 881,
        HRadius = 885,
        offsetX = 893,
        offsetY = 895,
        offsetZ = 897,
        A = 901,
        B = 905,
        C = 909,
        DA = 913,
        DB = 917,
        DC = 921
    }

    public class EEPROM_Variable {
        public string Name { get; private set; }
        public int Type { get; private set; }
        public int Position { get; private set; }

        private float varValue;
        public float Value { get { return varValue; } set { varValue = value; this.Pending = false; } }
        public bool Pending { get; set; }

        public EEPROM_Variable(string Name, int Type, EEPROM_Position Position) {
            this.Name = Name;
            this.Type = Type;
            this.Position = (int)Position;
            this.Pending = true;
        }

        public override string ToString() {
            return this.Value.ToString();
        }
    }

    static class EEPROMFunctions
    {
        public static void ReadEEPROM()
        {
            EEPROM.SetPending();            
            GCode.TrySend(GCode.Command.READ_EEPROM);
        }

        public static void ParseEEPROM(string value, out int intParse, out float floatParse2)
        {
            //parse EEProm
            if (value.Contains("EPR"))
            {
                string[] parseEPR = value.Split(new string[] { "EPR", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] parseEPRSpace;

                if (parseEPR.Length > 1)
                {
                    parseEPRSpace = parseEPR[1].Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    parseEPRSpace = parseEPR[0].Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                
                //check if there is a space between
                if (parseEPRSpace[0] == ":")
                {
                    //Space
                    intParse = int.Parse(parseEPRSpace[2], CultureInfo.InvariantCulture);
                    floatParse2 = float.Parse(parseEPRSpace[3], CultureInfo.InvariantCulture);
                }
                else if (value.Contains("EEPROM") || value.Contains("updated"))
                {
                    intParse = 1000;
                    floatParse2 = 0F;
                }
                else
                {
                    //No space
                    intParse = int.Parse(parseEPRSpace[1], CultureInfo.InvariantCulture);
                    floatParse2 = float.Parse(parseEPRSpace[2], CultureInfo.InvariantCulture);
                }
            }//end EEProm capture
            else
            {
                //No space
                intParse = 0;
                floatParse2 = 0;
            }
        }

        public static void SetEEPROM(EEPROM_Position settingPosition, float value)
        {
            switch (settingPosition)
            {
                case EEPROM_Position.StepsPerMM:
                    EEPROM.stepsPerMM.Value = value;
                    EEPROM.tempSPM = value;
                    break;
                case EEPROM_Position.zMaxLength:
                    EEPROM.zMaxLength.Value = value;
                    break;
                case EEPROM_Position.zProbeHeight:
                    EEPROM.zProbeHeight.Value = value;
                    break;
                case EEPROM_Position.zProbeSpeed:
                    EEPROM.zProbeSpeed.Value = value;          
                    break;
                case EEPROM_Position.diagonalRod:
                    EEPROM.diagonalRod.Value = value;
                    break;
                case EEPROM_Position.HRadius:
                    EEPROM.HRadius.Value = value;
                    break;
                case EEPROM_Position.offsetX:
                    EEPROM.offsetX.Value = value;
                    break;
                case EEPROM_Position.offsetY:
                    EEPROM.offsetY.Value = value;
                    break;
                case EEPROM_Position.offsetZ:
                    EEPROM.offsetZ.Value = value;
                    break;
                case EEPROM_Position.A:
                    EEPROM.A.Value = value;
                    break;
                case EEPROM_Position.B:
                    EEPROM.B.Value = value;
                    break;
                case EEPROM_Position.C:
                    EEPROM.C.Value = value;
                    break;
                case EEPROM_Position.DA:
                    EEPROM.DA.Value = value;
                    break;
                case EEPROM_Position.DB:
                    EEPROM.DB.Value = value;
                    break;
                case EEPROM_Position.DC:
                    EEPROM.DC.Value = value;
                    break;
            }
        }

        public static void SendEEPROM()
        {
            //manually set all eeprom values
            UserInterface.consoleLog.Log("Sending EEPROM.");
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.stepsPerMM);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.zMaxLength);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.zProbeHeight);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.zProbeSpeed);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.diagonalRod);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.HRadius);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.offsetX);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.offsetY);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.offsetZ);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.A);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.B);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.C);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.DA);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.DB);
            Thread.Sleep(50);
            GCode.SendEEPROMVariable(EEPROM.DC);
            Thread.Sleep(50);
        }
    }
}

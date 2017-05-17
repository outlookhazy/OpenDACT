using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    public static class EEPROM
    {
        public static EEPROM_Variable stepsPerMM = new EEPROM_Variable(3, 11);
        public static float tempSPM;
        public static EEPROM_Variable zMaxLength = new EEPROM_Variable(3, 153);
        public static EEPROM_Variable zProbeHeight = new EEPROM_Variable(3, 808);
        public static EEPROM_Variable zProbeSpeed = new EEPROM_Variable(3, 812);
        public static EEPROM_Variable diagonalRod = new EEPROM_Variable(3, 881);
        public static EEPROM_Variable HRadius = new EEPROM_Variable(3, 885);
        public static EEPROM_Variable offsetX = new EEPROM_Variable(1, 893);
        public static EEPROM_Variable offsetY = new EEPROM_Variable(1, 895);
        public static EEPROM_Variable offsetZ = new EEPROM_Variable(1, 897);
        public static EEPROM_Variable A = new EEPROM_Variable(3, 901);
        public static EEPROM_Variable B = new EEPROM_Variable(3, 905);
        public static EEPROM_Variable C = new EEPROM_Variable(3, 909);
        public static EEPROM_Variable DA = new EEPROM_Variable(3, 913);
        public static EEPROM_Variable DB = new EEPROM_Variable(3, 917);
        public static EEPROM_Variable DC = new EEPROM_Variable(3, 921);
    }

    public class EEPROM_Variable {
        public int Type { get; private set; }
        public int Position { get; private set; }
        public float Value;

        public EEPROM_Variable(int Type, int Position) {
            this.Type = Type;
            this.Position = Position;
        }

        public override string ToString() {
            return this.Value.ToString();
        }
    }

    static class EEPROMFunctions
    {
        public static bool tempEEPROMSet = false;
        public static bool EEPROMRequestSent = false;
        public static bool EEPROMReadOnly = false;
        public static int EEPROMReadCount = 0;

        public static void ReadEEPROM()
        {
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

        public static void SetEEPROM(int intParse, float floatParse2)
        {
            switch (intParse)
            {
                case 11:
                    UserInterface.LogConsole("EEPROM capture initiated");

                    EEPROM.stepsPerMM.Value = floatParse2;
                    EEPROM.tempSPM = floatParse2;
                    break;
                case 153:
                    EEPROM.zMaxLength.Value = floatParse2;
                    break;
                case 808:
                    EEPROM.zProbeHeight.Value = floatParse2;
                    break;
                case 812:
                    EEPROM.zProbeSpeed.Value = floatParse2;
                    tempEEPROMSet = true;
                    GCode.checkHeights = true;
                    EEPROMReadCount++;
                    Program.mainFormTest.SetEEPROMGUIList();

                    break;
                case 881:
                    EEPROM.diagonalRod.Value = floatParse2;
                    break;
                case 885:
                    EEPROM.HRadius.Value = floatParse2;
                    break;
                case 893:
                    EEPROM.offsetX.Value = floatParse2;
                    break;
                case 895:
                    EEPROM.offsetY.Value = floatParse2;
                    break;
                case 897:
                    EEPROM.offsetZ.Value = floatParse2;
                    break;
                case 901:
                    EEPROM.A.Value = floatParse2;
                    break;
                case 905:
                    EEPROM.B.Value = floatParse2;
                    break;
                case 909:
                    EEPROM.C.Value = floatParse2;
                    break;
                case 913:
                    EEPROM.DA.Value = floatParse2;
                    break;
                case 917:
                    EEPROM.DB.Value = floatParse2;
                    break;
                case 921:
                    EEPROM.DC.Value = floatParse2;
                    break;
            }
        }

        public static void SendEEPROM()
        {
            //manually set all eeprom values
            UserInterface.LogConsole("Setting EEPROM.");
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

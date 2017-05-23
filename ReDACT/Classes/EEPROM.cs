using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using OpenDACT.Class_Files.Workflow_Classes;
using System.Diagnostics;

namespace OpenDACT.Class_Files
{
    public class EEPROM : Dictionary<EEPROM_Position,EEPROM_Variable>
    {
        public float tempSPM;
        public static bool Reading { get; set; }

        public EEPROM()
        {
            this[EEPROM_Position.StepsPerMM] = new EEPROM_Variable(EEPROM_Position.StepsPerMM,"stepsPerMM",3);
            this[EEPROM_Position.zMaxLength] = new EEPROM_Variable(EEPROM_Position.zMaxLength, "zMaxLength", 3);
            this[EEPROM_Position.zProbeHeight] = new EEPROM_Variable(EEPROM_Position.zProbeHeight, "zProbeHeight", 3);
            this[EEPROM_Position.zProbeSpeed] = new EEPROM_Variable(EEPROM_Position.zProbeSpeed, "zProbeSpeed", 3);

            this[EEPROM_Position.diagonalRod] = new EEPROM_Variable(EEPROM_Position.diagonalRod, "diagonalRod", 3);
            this[EEPROM_Position.HRadius] = new EEPROM_Variable(EEPROM_Position.HRadius, "HRadius", 3);

            this[EEPROM_Position.offsetX] = new EEPROM_Variable(EEPROM_Position.offsetX, "offsetX", 1);
            this[EEPROM_Position.offsetY] = new EEPROM_Variable(EEPROM_Position.offsetY, "offsetY", 1);
            this[EEPROM_Position.offsetZ] = new EEPROM_Variable(EEPROM_Position.offsetZ, "offsetZ", 1);

            this[EEPROM_Position.A] = new EEPROM_Variable(EEPROM_Position.A, "A", 3);
            this[EEPROM_Position.B] = new EEPROM_Variable(EEPROM_Position.B, "B", 3);
            this[EEPROM_Position.C] = new EEPROM_Variable(EEPROM_Position.C, "C", 3);

            this[EEPROM_Position.DA] = new EEPROM_Variable(EEPROM_Position.DA, "DA", 3);
            this[EEPROM_Position.DB] = new EEPROM_Variable(EEPROM_Position.DB, "DB", 3);
            this[EEPROM_Position.DC] = new EEPROM_Variable(EEPROM_Position.DC, "DC", 3);

        }      

        

        public bool ReadComplete()
        {
            foreach(EEPROM_Position v in this.Keys)
            {
                if (this[v].Pending)
                    return false;
            }
            return true;
        }

        public void SetPending()
        {
            foreach (EEPROM_Position v in this.Keys)
            {
                this[v].Pending = true;
            }
        }

        public static EEPROM_Position toPosition(int position)
        {
            foreach(EEPROM_Position pos in typeof(EEPROM_Position).GetEnumValues())
            {
                if ((int)pos == position)
                    return pos;
            }
            return EEPROM_Position.INVALID;
        }

        public static EEPROM_Value Parse(string value)
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
                    return new EEPROM_Value(EEPROM.toPosition(int.Parse(parseEPRSpace[2], CultureInfo.InvariantCulture)),
                        float.Parse(parseEPRSpace[3], CultureInfo.InvariantCulture));
                }
                else if (value.Contains("EEPROM") || value.Contains("updated"))
                {
                    return new EEPROM_Value(EEPROM_Position.INVALID);
                }
                else
                {
                    //No space
                    return new EEPROM_Value(EEPROM.toPosition(int.Parse(parseEPRSpace[1], CultureInfo.InvariantCulture)),
                        float.Parse(parseEPRSpace[2], CultureInfo.InvariantCulture));
                }
            }//end EEProm capture
            else
            {
                //No space
                return new EEPROM_Value(EEPROM_Position.INVALID);
            }
        }

        public void Set(EEPROM_Position settingPosition, float value)
        {
            if(settingPosition == EEPROM_Position.StepsPerMM)
                this.tempSPM = value;

            this[settingPosition].Value = value;            
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
        DC = 921,
        INVALID = 0
    }

    public class EEPROM_Variable {
        public string Name { get; private set; }
        public int Type { get; private set; }
        public int Position { get; private set; }

        private float varValue;
        public float Value { get { return varValue; } set { varValue = value; this.Pending = false; } }
        public bool Pending { get; set; }

        public EEPROM_Variable(EEPROM_Position Position, string Name, int Type, float Value = 0, bool Pending = true) {
            this.Name = Name;
            this.Type = Type;
            this.Position = (int)Position;
            this.Value = Value;
            this.Pending = Pending;
        }

        public override string ToString() {
            return this.Value.ToString();
        }
    }

    public class EEPROM_Value
    {
        public EEPROM_Position Type { get; private set; }
        public float Value { get; private set; }
        public EEPROM_Value (EEPROM_Position Type = EEPROM_Position.INVALID, float Value = 0)
        {
            this.Type = Type;
            this.Value = Value;
        }
    }

    static class EEPROMFunctions
    {
        

        

        
    }
}

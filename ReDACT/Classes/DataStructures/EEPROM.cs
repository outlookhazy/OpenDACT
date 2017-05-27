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
    public class EEPROM : Dictionary<EEPROM_POSITION,EEPROM_Variable>
    {
        public float tempSPM;
        public static bool Reading { get; set; }

        public EEPROM()
        {
            this[EEPROM_POSITION.StepsPerMM_Z] = new EEPROM_Variable(EEPROM_POSITION.StepsPerMM_Z,"stepsPerMM_Z", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.maxFeedrate] = new EEPROM_Variable(EEPROM_POSITION.maxFeedrate, "maxFeedrate", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.xMaxLength] = new EEPROM_Variable(EEPROM_POSITION.xMaxLength, "xMaxLength", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.yMaxLength] = new EEPROM_Variable(EEPROM_POSITION.yMaxLength, "yMaxLength", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.zMaxLength] = new EEPROM_Variable(EEPROM_POSITION.zMaxLength, "zMaxLength", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.zProbeHeight] = new EEPROM_Variable(EEPROM_POSITION.zProbeHeight, "zProbeHeight", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.zProbeBedDistance] = new EEPROM_Variable(EEPROM_POSITION.zProbeBedDistance, "zProbeBedDistance", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.zProbeSpeed] = new EEPROM_Variable(EEPROM_POSITION.zProbeSpeed, "zProbeSpeed", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.diagonalRod] = new EEPROM_Variable(EEPROM_POSITION.diagonalRod, "diagonalRod", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.HRadius] = new EEPROM_Variable(EEPROM_POSITION.HRadius, "HRadius", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.offsetX] = new EEPROM_Variable(EEPROM_POSITION.offsetX, "offsetX", EEPROM_TYPE.INTEGER);
            this[EEPROM_POSITION.offsetY] = new EEPROM_Variable(EEPROM_POSITION.offsetY, "offsetY", EEPROM_TYPE.INTEGER);
            this[EEPROM_POSITION.offsetZ] = new EEPROM_Variable(EEPROM_POSITION.offsetZ, "offsetZ", EEPROM_TYPE.INTEGER);

            this[EEPROM_POSITION.A] = new EEPROM_Variable(EEPROM_POSITION.A, "A", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.B] = new EEPROM_Variable(EEPROM_POSITION.B, "B", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.C] = new EEPROM_Variable(EEPROM_POSITION.C, "C", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.DA] = new EEPROM_Variable(EEPROM_POSITION.DA, "DA", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.DB] = new EEPROM_Variable(EEPROM_POSITION.DB, "DB", EEPROM_TYPE.FLOAT);
            this[EEPROM_POSITION.DC] = new EEPROM_Variable(EEPROM_POSITION.DC, "DC", EEPROM_TYPE.FLOAT);

            this[EEPROM_POSITION.printableRadius] = new EEPROM_Variable(EEPROM_POSITION.printableRadius, "Max Printable Radius", EEPROM_TYPE.FLOAT);

        }      

        

        public bool ReadComplete()
        {
            foreach(EEPROM_POSITION v in this.Keys)
            {
                if (this[v].Pending)
                {
                    return false;
                }
            }
            return true;
        }

        public void SetPending()
        {
            foreach (EEPROM_POSITION v in this.Keys)
            {
                this[v].Pending = true;
            }
        }

        public static EEPROM_POSITION toPosition(int position)
        {
            foreach(EEPROM_POSITION pos in typeof(EEPROM_POSITION).GetEnumValues())
            {
                if ((int)pos == position)
                    return pos;
            }
            return EEPROM_POSITION.INVALID;
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
                    return new EEPROM_Value(EEPROM_POSITION.INVALID);
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
                return new EEPROM_Value(EEPROM_POSITION.INVALID);
            }
        }

        public EEPROM Copy()
        {
            EEPROM copy = new EEPROM();
            foreach (EEPROM_POSITION pos in copy.Keys)
            {
                copy[pos].Value = this[pos].Value;
            }
            return copy;
        }

        public void Set(EEPROM_POSITION settingPosition, float value)
        {
            if(settingPosition == EEPROM_POSITION.StepsPerMM_Z)
                this.tempSPM = value;

            this[settingPosition].Value = value;            
        }        
    }

    public enum EEPROM_TYPE:int
    {
        INTEGER=1,
        FLOAT=3
    }

    public enum EEPROM_POSITION:int {
        StepsPerMM_X = 3,
        StepsPerMM_Y = 7,
        StepsPerMM_Z = 11,
        maxFeedrate = 23,
        xMaxLength = 145,
        yMaxLength = 149,
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
        printableRadius = 925,
        zProbeBedDistance = 929,
        INVALID = 0
    }

    public class EEPROM_Variable {
        public string Name { get; private set; }
        public EEPROM_TYPE Type { get; private set; }
        public int Position { get; private set; }

        private float varValue;
        public float Value { get { return varValue; } set { varValue = value; this.Pending = false; } }
        public bool Pending { get; set; }

        public EEPROM_Variable(EEPROM_POSITION Position, string Name, EEPROM_TYPE Type, float Value = 0, bool Pending = true) {
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
        public EEPROM_POSITION Type { get; private set; }
        public float Value { get; private set; }
        public EEPROM_Value (EEPROM_POSITION Type = EEPROM_POSITION.INVALID, float Value = 0)
        {
            this.Type = Type;
            this.Value = Value;
        }
    }
}

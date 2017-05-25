using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenDACT.Class_Files.Workflow_Classes;
using System.Globalization;
using System.Diagnostics;

namespace OpenDACT.Class_Files
{
    static class GCode
    {
        public static float ParseZProbe(string value)
        {
            if (value.Contains("Z-probe:"))
            {
                //Z-probe: 10.66 zCorr: 0

                string[] parseInData = value.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string[] parseFirstLine = parseInData[0].Split(new char[] { ':', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                //: 10.66 zCorr: 0
                string[] parseZProbe = value.Split(new string[] { "Z-probe", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] parseZProbeSpace = parseZProbe[0].Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                float zProbeParse;

                //check if there is a space between
                if (parseZProbeSpace[0] == ":")
                {
                    //Space
                    zProbeParse = float.Parse(parseZProbeSpace[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    //No space
                    zProbeParse = float.Parse(parseZProbeSpace[0].Substring(1), CultureInfo.InvariantCulture);
                }

                return float.Parse(parseFirstLine[1], CultureInfo.InvariantCulture);
            }
            else
            {
                return 1000;
            }
        }

        public static class Command {
            public static String RAPID(float? X = null, float? Y = null, float? Z = null)
            {
                string cmdX = X == null ? "" : String.Format(" X{0}", X);
                string cmdY = Y == null ? "" : String.Format(" Y{0}", Y);
                string cmdZ = Z == null ? "" : String.Format(" Z{0}", Z);

                return String.Format("G0{0}{1}{2}", cmdX, cmdY, cmdZ);
            }
            public static String MOVE(float? X = null, float? Y = null, float? Z = null)
            {
                string cmdX = X == null ? "" : String.Format(" X{0}", X);
                string cmdY = Y == null ? "" : String.Format(" Y{0}", Y);
                string cmdZ = Z == null ? "" : String.Format(" Z{0}", Z);

                return String.Format("G1{0}{1}{2} F12000", cmdX, cmdY, cmdZ);
            }

            public static String MOVE(double? X = null, double? Y = null, double? Z = null)
            {
                return MOVE(Convert.ToSingle(X), Convert.ToSingle(Y), Convert.ToSingle(Z));
            }

            public static String DWELL_SECONDS(int duration) { return String.Format("G4 S{0}", duration); }
            public static String DWELL_MILLISECONDS(int duration) { return String.Format("G4 P{0}", duration); }
            public static String HOME { get { return "G28"; } }
            public static String PROBE { get { return "G30"; } }
            public static String POSITIONING_ABSOLUTE { get { return "G90"; } }
            public static String POSITIONING_RELATIVE { get { return "G91"; } }          
            public static String ATX_ON { get { return "M80"; } }
            public static String ATX_OFF { get { return "M81"; } }
            public static String SET_INACTIVITY_TIMER(int seconds) { return String.Format("M85 S{0}", seconds); }
            public static String ESTOP { get { return "M112"; } }
            public static String GET_POSITION { get { return "M114"; } }
            public static String GET_FIRMWARE { get { return "M115"; } }
            public static String DISPLAY_MESSAGE(string message) { return String.Format("M117 {0}", message); }
            public static String QUERY_ENDSTOPS { get { return "M119"; } }
            public static String READ_EEPROM { get { return "M205"; } }
            public static String SEND_EEPROM_VARIABLE(EEPROM_Variable variable)
            {
                switch (variable.Type)
                {
                    case EEPROM_TYPE.FLOAT:
                        return (String.Format("M206 T{0} P{1} X{3}", (int)variable.Type, variable.Position, variable.Value.ToString("F3")));
                    default:
                        return (String.Format("M206 T{0} P{1} S{3}", (int)variable.Type, variable.Position, ((int)variable.Value).ToString()));
                }                
            }
            public static String MEASURE_HOME_STEPS { get { return ("M251"); } }
            public static String BEEP(int frequency, int duration) { return String.Format("M300 S{0} P{1}", frequency,duration); }
            public static String FIRMWARE_DETAILS { get { return "M360"; } } 
            public static String WAIT_MOVES_COMPLETE { get { return "M400"; } }
            public static String STORE_PARAMETERS_EEPROM { get { return "M500"; } }
            public static String LOAD_PARAMETERS_EEPROM { get { return "M501"; } }
            public static String REVERT_FACTORY_PARAMETERS_EEPROM { get { return "M502"; } }
            public static String SET_PRINT_NAME(string message) { return String.Format("M531 {0}", message); }
            public static String SET_PRINT_progress(float percent, int layer = 0) { return String.Format("M532 X{0} L{1}", percent, layer); }
            //M666: Set delta endstop adjustment
        }
    }
}

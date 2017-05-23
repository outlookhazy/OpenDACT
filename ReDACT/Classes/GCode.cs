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

        public static string MoveToPosition(float X, float Y, float Z) {
            return ("G90 : G1 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static string RapidToPosition(float X, float Y, float Z) {
            return ("G90 : G0 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static string SendEEPROMVariable(EEPROM_Variable variable) {
            char typeletter = variable.Type == 3 ? 'X' : 'S';
            return (String.Format("M206 T{0} P{1} {2}{3}", variable.Type, variable.Position, typeletter, variable.Value.ToString("F3")));
        }        
     
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
            public static String HOME { get { return "G28"; } }
            public static String PROBE { get { return "G30"; } }
            public static String RESET { get { return "M112"; } }
            public static String READ_EEPROM { get { return "M205"; } }
        }
    }
}

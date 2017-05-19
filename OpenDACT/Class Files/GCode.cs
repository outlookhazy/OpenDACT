using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenDACT.Class_Files.Workflow_Classes;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    static class GCode
    {
        public static bool isHeuristicComplete = false;

        public static bool MoveToPosition(float X, float Y, float Z) {
            return TrySend("G1 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static bool RapidToPosition(float X, float Y, float Z) {
            return TrySend("G0 Z" + Z.ToString() + " X" + X.ToString() + " Y" + Y.ToString());
        }

        public static bool SendEEPROMVariable(EEPROM_Variable variable) {
            return TrySend(String.Format("M206 T{0} P{1} S{2}", variable.Type, variable.Position, variable.Value.ToString("F3")));
        }

        public static bool TrySend(String serialCommand) {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected) {
                if (UserInterface.printerLog.ConsoleLogLevel == LogConsole.LogLevel.DEBUG)
                    UserInterface.printerLog.Log(String.Format("Sending: {0}", serialCommand));
                return Connection.serialManager.WriteLine(serialCommand);
            }
            else {
                UserInterface.consoleLog.Log("Not Connected");
                return false;
            }
        }

        /*
        public static void PauseTimeRadius() {
            Thread.Sleep(Convert.ToInt32(((UserVariables.plateDiameter / 2) / UserVariables.xySpeed) * 1000));//1000 s to ms x 1.25 for multiplier
        }

        public static void PauseTimeProbe() {
            Thread.Sleep(Convert.ToInt32(((UserVariables.probingHeight * 2) / EEPROM.zProbeSpeed.Value) * 1125));
        }

        public static void PauseTimeZMax() {
            Thread.Sleep(Convert.ToInt32((EEPROM.zMaxLength.Value / UserVariables.xySpeed) * 1025));
        }

        public static void PauseTimeZMaxThird() {
            Thread.Sleep(Convert.ToInt32(((EEPROM.zMaxLength.Value / 3) / UserVariables.xySpeed) * 1000));
        }
        */
        

        
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

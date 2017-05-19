using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    static class MeasureZProbe
    {
        internal static bool zProbeMeasuringComplete = false;
        internal static bool zProbeMeasuringActive = false;
        private static int iteration = 0;
        public static void DoNextStep()
        {
            switch (MeasureZProbe.iteration)
            {
                case 0:
                    EEPROM.zProbeHeight.Value = 0;
                    GCode.TrySend(Command.HOME);
                    iteration++;
                    break;
                case 1:
                    GCode.TrySend(Command.PROBE);

                    zProbeMeasuringComplete = true;
                    
                    iteration = 0;
                    break;
            }
        }

        public static void SetHeight(float value)
        {
            if (value != 1000)
            {
                EEPROM.zProbeHeight.Value = Convert.ToSingle(Math.Round(EEPROM.zMaxLength.Value / 6) - value);

                Program.mainFormTest.SetEEPROMGUIList();
                EEPROMFunctions.SendEEPROM();
            }
        }
    }
}

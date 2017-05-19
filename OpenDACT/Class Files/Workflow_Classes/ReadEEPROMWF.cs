using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ReadEEPROMWF : Workflow
    {
        protected override void OnStarted()
        {
            EEPROM.SetPending();
            GCode.TrySend(Command.READ_EEPROM);
        }

        protected override void OnMessage(string serialMessage)
        {
            EEPROMFunctions.ParseEEPROM(serialMessage, out int parsedInt, out float parsedFloat);
            if (parsedInt != 0)
                EEPROMFunctions.SetEEPROM((EEPROM_Position)parsedInt, parsedFloat);

            if (EEPROM.ReadComplete())
            {
                Program.mainFormTest.SetEEPROMGUIList();
                this.FinishOrAdvance();
            }
        }
    }
}

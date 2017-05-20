using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ReadEEPROMWF : Workflow
    {
        public override string ID { get { return "ReadEEPROMWF"; } set { return; } }

        protected override void OnStarted()
        {
            Debug.WriteLine("ReadEEPROMWF Started");
            EEPROM.SetPending();
            GCode.TrySend(Command.READ_EEPROM);
        }

        protected override void OnMessage(string serialMessage)
        {
            Debug.WriteLine("ReadEEPROMWF Active");
            EEPROMFunctions.ParseEEPROM(serialMessage, out int parsedInt, out float parsedFloat);
            if (parsedInt != 0)
                EEPROMFunctions.SetEEPROM((EEPROM_Position)parsedInt, parsedFloat);

            if (EEPROM.ReadComplete())
            {
                Program.mainFormTest.SetEEPROMGUIList();
                Debug.WriteLine("ReadEEPROMWF Done");
                this.FinishOrAdvance();
            }
        }
    }
}

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
        private EEPROM EEPROM;

        public ReadEEPROMWF(out EEPROM EEPROM)
        {
            this.ID = "ReadEEPROMWF";
            EEPROM = new EEPROM();
            this.EEPROM = EEPROM;
        }

        protected override void OnStarted()
        {     
            EEPROM.SetPending();
            SerialSource.WriteLine(GCode.Command.READ_EEPROM);
        }

        protected override void OnMessage(string serialMessage)
        {
            EEPROM_Value parsed = EEPROM.Parse(serialMessage);
            if (parsed.Type != EEPROM_POSITION.INVALID)
                this.EEPROM[parsed.Type].Value = parsed.Value;

            if (EEPROM.ReadComplete() && (serialMessage.Contains("wait") || serialMessage.Contains("ok")))
                this.FinishOrAdvance();
        }
    }
}

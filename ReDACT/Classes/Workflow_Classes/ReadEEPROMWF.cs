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

        public EEPROM EEPROM;
        private SerialManager serialSource;
        public ReadEEPROMWF(SerialManager serialSource)
        {
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            Debug.WriteLine("ReadEEPROMWF Started");
            EEPROM = new EEPROM();
            EEPROM.SetPending();
            serialSource.WriteLine(Command.READ_EEPROM);
        }

        protected override void OnMessage(string serialMessage)
        {
            Debug.WriteLine("ReadEEPROMWF Active");
            EEPROM_Value data = EEPROM.Parse(serialMessage);
            if (data.Type != EEPROM_Position.INVALID)
                this.EEPROM[data.Type].Value = data.Value;            

            if (EEPROM.ReadComplete())
                this.FinishOrAdvance();

        }
    }
}

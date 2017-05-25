﻿using System;
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
        private SerialManager serialSource;

        private EEPROM _eeprom;
        public EEPROM EEPROM { get { return _eeprom; } private set { _eeprom = value; } }

        public ReadEEPROMWF(SerialManager serialSource)
        {
            this.ID = "ReadEEPROMWF";
            this.serialSource = serialSource;
            this.EEPROM = new EEPROM();
        }

        protected override void OnStarted()
        {
            Debug.WriteLine("ReadEEPROMWF Started");            
            EEPROM.SetPending();
            serialSource.WriteLine(GCode.Command.READ_EEPROM);
        }

        protected override void OnMessage(string serialMessage)
        {
            EEPROM_Value parsed = EEPROM.Parse(serialMessage);
            if (parsed.Type != EEPROM_POSITION.INVALID)
                this.EEPROM[parsed.Type].Value = parsed.Value;

            if (EEPROM.ReadComplete())
                this.FinishOrAdvance();
        }
    }
}

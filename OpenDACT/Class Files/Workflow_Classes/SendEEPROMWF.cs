using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    class SendEEPROMWF : Workflow
    {
        public override string ID { get { return "SendEEPROMWF"; } set { return; } }

        protected override void OnStarted()
        {
            EEPROMFunctions.SendEEPROM();
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}

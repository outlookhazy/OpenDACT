using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class HomeWF : Workflow
    {
        private SerialManager serialSource;

        public HomeWF(SerialManager serialSource)
        {
            this.ID = "HomeWF";
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            serialSource.WriteLine(GCode.Command.HOME);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
            {
                this.FinishOrAdvance();
            }
        }
    }
}
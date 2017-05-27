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
        public HomeWF()
        {
            this.ID = "HomeWF";
        }

        protected override void OnStarted()
        {
            SerialSource.WriteLine(GCode.Command.HOME);
            SerialSource.WriteLine(GCode.Command.WAIT_MOVES_COMPLETE);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("ok"))
            {
                this.FinishOrAdvance();
            }
        }
    }
}
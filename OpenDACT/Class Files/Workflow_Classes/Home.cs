using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class Home : Workflow
    {
        protected override void OnStarted()
        {
            GCode.TrySend(Command.HOME);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (this.Status == WorkflowState.STARTED && serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}
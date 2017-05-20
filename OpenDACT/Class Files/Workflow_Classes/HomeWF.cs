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
        public override string ID { get { return "HomeWF"; } set { return; } }

        protected override void OnStarted()
        {
            Debug.WriteLine("Home Started");
            GCode.TrySend(Command.HOME);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
            {
                Debug.WriteLine("Home Done");
                this.FinishOrAdvance();
            }
        }
    }
}
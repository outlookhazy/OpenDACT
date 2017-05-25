using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeWF : Workflow
    {
        private double Result;

        public ProbeWF(ref double Result)
        {
            this.Result = Result;
        }

        protected override void OnStarted()
        {
            this.ID = "ProbeWF";
            SerialSource.WriteLine(GCode.Command.PROBE);
        }

        protected override void OnMessage(string serialMessage)
        {
            float value = GCode.ParseZProbe(serialMessage);
            if (value != 1000)
            {
                this.Result = value;
                this.FinishOrAdvance();
            } else
            {
                this.Abort();
            }
        }
    }
}
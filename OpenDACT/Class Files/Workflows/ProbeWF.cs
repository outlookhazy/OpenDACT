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
        public float Result { get; private set; }

        private SerialManager serialSource;

        public ProbeWF(SerialManager serialSource)
        {
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            this.ID = "ProbeWF";
            serialSource.WriteLine(GCode.Command.PROBE);
        }

        protected override void OnMessage(string serialMessage)
        {
            float value = GCode.ParseZProbe(serialMessage);
            if (value != 1000)
            {
                this.Result = value;
                this.FinishOrAdvance();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;
using static OpenDACT.Class_Files.Workflow_Classes.EscherWF.EscherMeasureHeightsWF;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeWF : Workflow
    {
        private onMeasureResult measureDone;
        private int IDProbe;

        public ProbeWF(onMeasureResult onDone, int ProbeID = 0)
        {
            this.measureDone = onDone;
            this.IDProbe = ProbeID;
        }

        protected override void OnStarted()
        {
            this.ID = String.Format("ProbeWF<{0}>",this.IDProbe);
            SerialSource.WriteLine(GCode.Command.PROBE);
            SerialSource.WriteLine(GCode.Command.DISPLAY_MESSAGE(String.Format("Probing point #{0}", this.IDProbe)));
        }

        protected override void OnMessage(string serialMessage)
        {
            float value = GCode.ParseZProbe(serialMessage);
            if (value != 1000)
            {
                this.measureDone(value, this.IDProbe);
                this.FinishOrAdvance();
            } 
        }
    }
}
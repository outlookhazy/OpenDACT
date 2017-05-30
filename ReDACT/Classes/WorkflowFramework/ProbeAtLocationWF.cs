using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.Workflow_Classes.EscherWF.EscherMeasureHeightsWF;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeAtLocationWF : Workflow
    {
        private double X;
        private double Y;
        private double Z;
        private double F;

        private onMeasureResult measureDone;
        private int IDProbe;

        public ProbeAtLocationWF(double X, double Y, double probeHeight, double moveSpeed, onMeasureResult measureDone, int IDProbe = 0)
        {
            this.IDProbe = IDProbe;
            this.ID = String.Format("ProbeAtLocationWF<{0}>", this.IDProbe);
            this.X = X;
            this.Y = Y;
            this.Z = probeHeight;
            this.F = moveSpeed;
            this.measureDone = measureDone;
        }

        protected override void OnStarted()
        {
            AddWorkflowItem(new MoveWF(this.X, this.Y, this.Z, this.F));
            AddWorkflowItem(new ProbeWF(measureDone, IDProbe));
        }
    }
}

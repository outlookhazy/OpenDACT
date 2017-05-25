using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeAtLocationWF : Workflow
    {
        private double X;
        private double Y;
        private double Z;
        private double ResultZ;

        public ProbeAtLocationWF(double X, double Y, double Z, ref double ResultZ)
        {
            this.ID = "ProbeAtLocationWF";
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.ResultZ = ResultZ;
        }

        protected override void OnStarted()
        {
            AddWorkflowItem(new MoveWF(this.X, this.Y, this.Z));
            AddWorkflowItem(new ProbeWF(ref this.ResultZ));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeAtLocation : Workflow
    {
        private Position3D probePoint;
        private Probe probeWorkflow;
        public float Result { get { return this.probeWorkflow.Result; }  }

        public ProbeAtLocation(Position3D location)
        {
            this.probePoint = location;
        }
        protected override void OnStarted()
        {
            AddWorkflowItem(new Move(probePoint));
            this.probeWorkflow = new Probe();
            AddWorkflowItem(this.probeWorkflow);
            this.FinishOrAdvance();
        }
    }
}

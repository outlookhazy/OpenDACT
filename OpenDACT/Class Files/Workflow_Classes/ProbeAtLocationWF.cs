using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeAtLocationWF : Workflow
    {
        public override string ID { get { return "ProbeAtLocationWF"; } set { return; } }

        private Position3D probePoint;
        private ProbeWF probeWorkflow;
        public float Result { get { return this.probeWorkflow.Result; }  }

        public ProbeAtLocationWF(Position3D location)
        {
            this.probePoint = location;
        }
        protected override void OnStarted()
        {
            AddWorkflowItem(new MoveWF(probePoint));
            this.probeWorkflow = new ProbeWF();
            AddWorkflowItem(this.probeWorkflow);
        }
    }
}

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

        private SerialManager serialSource;

        public ProbeAtLocationWF(SerialManager serialSource, Position3D location)
        {
            this.serialSource = serialSource;
            this.probePoint = location;
        }
        protected override void OnStarted()
        {
            AddWorkflowItem(new MoveWF(probePoint));
            this.probeWorkflow = new ProbeWF(this.serialSource);
            AddWorkflowItem(this.probeWorkflow);
        }
    }
}

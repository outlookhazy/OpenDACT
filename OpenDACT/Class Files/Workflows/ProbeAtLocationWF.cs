using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ProbeAtLocationWF : Workflow
    {
        public new string ID { get { return "ProbeAtLocationWF"; } set { return; } }

        private Position3D probePoint;
        private ProbeWF probeWorkflow;
        private SerialManager serialSource;
        public float Result { get { return this.probeWorkflow.Result; }  }

        public ProbeAtLocationWF(SerialManager serialSource, Position3D location)
        {
            this.serialSource = serialSource;
            this.probePoint = location;
        }
        protected override void OnStarted()
        {
            AddWorkflowItem(new MoveWF(serialSource,probePoint));
            this.probeWorkflow = new ProbeWF(serialSource);
            AddWorkflowItem(this.probeWorkflow);
        }
    }
}

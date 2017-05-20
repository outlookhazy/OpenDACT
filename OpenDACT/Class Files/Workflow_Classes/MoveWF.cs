using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MoveWF : Workflow
    {
        public override string ID { get { return "MoveWF"; } set { return; } }

        private Position3D targetLocation;
        public MoveWF(Position3D location)
        {
            this.targetLocation = location;
        }

        protected override void OnStarted()
        {
            GCode.MoveToPosition(this.targetLocation.X, this.targetLocation.Y, this.targetLocation.Z);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}

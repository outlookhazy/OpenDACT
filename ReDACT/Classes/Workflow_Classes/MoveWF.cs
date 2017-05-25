using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MoveWF : Workflow
    {

        private Position3D targetLocation;
        private SerialManager serialSource;
        public MoveWF(SerialManager serialSource, Position3D location)
        {
            this.ID = "MoveWF";
            this.serialSource = serialSource;
            this.targetLocation = location;
        }

        protected override void OnStarted()
        {
            string movecommand = GCode.Command.MOVE(targetLocation.X, targetLocation.Y, targetLocation.Z);
            serialSource.WriteLine(movecommand);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}

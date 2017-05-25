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
        private double X, Y, Z;
        public MoveWF(double X, double Y, double Z)
        {
            this.ID = "MoveWF";
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        protected override void OnStarted()
        {
            string movecommand = GCode.Command.MOVE(this.X, this.Y, this.Z);
            SerialSource.WriteLine(movecommand);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}

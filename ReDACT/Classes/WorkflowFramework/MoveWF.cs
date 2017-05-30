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
        private double X, Y, Z, F;
        public MoveWF(double X, double Y, double Z, double F)
        {
            this.ID = "MoveWF";
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.F = F;
        }

        protected override void OnStarted()
        {
            string movecommand = GCode.Command.MOVE(this.X, this.Y, this.Z, this.F);
            SerialSource.WriteLine(movecommand);
            SerialSource.WriteLine(GCode.Command.DISPLAY_MESSAGE(String.Format("Moving to [{0}, {1}, {2}]", this.X, this.Y, this.Z)));
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("ok") || serialMessage.Contains("wait"))
                this.FinishOrAdvance();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.GCode;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class HomeWF : Workflow
    {
        public new string ID { get { return "HomeWF"; } set { return; } }

        private SerialManager serialSource;

        public HomeWF(SerialManager serialSource)
        {
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            Debug.WriteLine("Home Started");
            serialSource.WriteLine(GCode.Command.HOME);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("wait"))
            {
                Debug.WriteLine("Home Done");
                this.FinishOrAdvance();
            }
        }
    }
}
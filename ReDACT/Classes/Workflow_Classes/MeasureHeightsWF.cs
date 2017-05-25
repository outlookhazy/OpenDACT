using ReDACT.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MeasureHeightsWF : Workflow
    {
        

        public HeightMap ResultHeights;
        public EEPROM ResultEEPROM;

        private SerialManager serialSource;
        private EEPROM sourceEEPROM;

        public MeasureHeightsWF(SerialManager serialSource, EEPROM sourceEEPROM)
        {
            this.ID = "MeasureHeightsWF";
            this.serialSource = serialSource;
            this.sourceEEPROM = sourceEEPROM;

        }

        protected override void OnStarted()
        {

            
        }

        protected override void OnChildrenFinished()
        {


         
        }
    }
}

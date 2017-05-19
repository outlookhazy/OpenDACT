using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ZProbeMeasure : Workflow
    {
        public Probe MeasurementProbe;
        protected override void OnStarted()
        {
            this.AddWorkflowItem(new ReadEEPROMWF());
            this.AddWorkflowItem(new Home());
            MeasurementProbe = new Probe();
            this.AddWorkflowItem(MeasurementProbe);
            this.FinishOrAdvance();
        }

        protected override void OnChildrenFinished()
        {
            EEPROM.zProbeHeight.Value = Convert.ToSingle(Math.Round(EEPROM.zMaxLength.Value / 6) - MeasurementProbe.Result);

            Program.mainFormTest.SetEEPROMGUIList();
            EEPROMFunctions.SendEEPROM();
        }
    }
}

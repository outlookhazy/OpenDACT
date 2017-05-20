using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ZProbeMeasureWF : Workflow
    {
        public override string ID { get { return "ZProbeMeasureWF"; } set { return; } }

        public ProbeWF MeasurementProbe;
        protected override void OnStarted()
        {
            this.AddWorkflowItem(new ReadEEPROMWF());
            this.AddWorkflowItem(new HomeWF());
            MeasurementProbe = new ProbeWF();
            this.AddWorkflowItem(MeasurementProbe);
        }

        protected override void OnChildrenFinished()
        {
            EEPROM.zProbeHeight.Value = Convert.ToSingle(Math.Round(EEPROM.zMaxLength.Value / 6) - MeasurementProbe.Result);

            Program.mainFormTest.SetEEPROMGUIList();
            EEPROMFunctions.SendEEPROM();
        }
    }
}

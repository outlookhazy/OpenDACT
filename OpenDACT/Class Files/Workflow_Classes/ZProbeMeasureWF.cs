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
            this.AddWorkflowItem(new CalculateZProbeWF(MeasurementProbe));
            this.AddWorkflowItem(new ApplySettingsWF());
        }

        class CalculateZProbeWF : Workflow
        {
            public override string ID { get { return "CalculateZProbeWF"; } set { return; } }

            ProbeWF probeSource;

            internal CalculateZProbeWF(ProbeWF dataSource)
            {
                this.probeSource = dataSource;
            }

            protected override void OnStarted()
            {
                EEPROM.zProbeHeight.Value = Convert.ToSingle(Math.Round(EEPROM.zMaxLength.Value / 6) - probeSource.Result);
                Program.mainFormTest.SetEEPROMGUIList();
            }
        }
    }
}

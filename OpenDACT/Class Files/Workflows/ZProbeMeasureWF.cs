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
        public new string ID { get { return "ZProbeMeasureWF"; } set { return; } }

        public ProbeWF MeasurementProbe;

        private SerialManager serialSource;

        public ZProbeMeasureWF(SerialManager serialSource)
        {
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            ReadEEPROMWF eepromSource = new ReadEEPROMWF(serialSource);
            this.AddWorkflowItem(eepromSource);
            this.AddWorkflowItem(new HomeWF(serialSource));
            MeasurementProbe = new ProbeWF(serialSource);
            this.AddWorkflowItem(MeasurementProbe);
            this.AddWorkflowItem(new CalculateZProbeWF(MeasurementProbe, eepromSource.EEPROM));
            this.AddWorkflowItem(new ApplySettingsWF(serialSource, eepromSource.EEPROM));
        }

        class CalculateZProbeWF : Workflow
        {

            ProbeWF probeSource;
            EEPROM eepromSource;

            internal CalculateZProbeWF(ProbeWF dataSource, EEPROM eepromSource)
            {
                this.ID = "CalculateZProbeWF";
                this.eepromSource = eepromSource;
                this.probeSource = dataSource;
            }

            protected override void OnStarted()
            {
                eepromSource[EEPROM_POSITION.zProbeHeight].Value = Convert.ToSingle(Math.Round(eepromSource[EEPROM_POSITION.zMaxLength].Value / 6) - probeSource.Result);
                Program.mainFormTest.SetEEPROMGUIList(eepromSource);
            }
        }
    }
}

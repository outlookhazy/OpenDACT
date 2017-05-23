using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ApplySettingsWF : Workflow
    {
        public override string ID { get { return "ApplySettingsWF"; } set { return; } }

        private EEPROM romSource;
        private SerialManager serialSource;

        public ApplySettingsWF(SerialManager serialSource, EEPROM romSource)
        {
            this.romSource = romSource;
            this.serialSource = serialSource;
        }

        protected override void OnStarted()
        {
            foreach(EEPROM_POSITION pos in typeof(EEPROM_POSITION).GetEnumValues())
            {
                this.AddWorkflowItem(new ApplySettingWF(this.serialSource, this.romSource, pos));
            }            
        }
    }
}

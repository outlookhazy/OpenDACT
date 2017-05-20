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

        protected override void OnStarted()
        {
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.stepsPerMM));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.zMaxLength));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.zProbeHeight));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.zProbeSpeed));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.diagonalRod));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.HRadius));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.offsetX));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.offsetY));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.offsetZ));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.A));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.B));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.C));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.DA));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.DB));
            this.AddWorkflowItem(new ApplySettingWF(EEPROM.DC));
        }
    }
}

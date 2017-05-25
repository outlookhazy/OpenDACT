using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ApplySettingsWF : Workflow
    {
        private EEPROM settings;

        public ApplySettingsWF(ref EEPROM targetSettings)
        {
            this.ID = "ApplySettingsWF";
            this.settings = targetSettings;
        }

        protected override void OnStarted()
        {
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.StepsPerMM]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.zMaxLength]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.zProbeHeight]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.zProbeSpeed]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.diagonalRod]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.HRadius]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.offsetX]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.offsetY]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.offsetZ]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.A]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.B]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.C]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.DA]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.DB]));
            this.AddWorkflowItem(new ApplySettingWF(settings[EEPROM_POSITION.DC]));
        }

        protected override void OnChildrenFinished()
        {
            Debug.WriteLine("Settings Applied");
        }
    }
}

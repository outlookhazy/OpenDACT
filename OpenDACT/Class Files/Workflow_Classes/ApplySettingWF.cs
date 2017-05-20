using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class ApplySettingWF : Workflow
    {
        private string settingID = "ApplySettingWF";
        public override string ID { get { return this.settingID; } set { return; } }

        private EEPROM_Variable targetSetting;

        public ApplySettingWF(EEPROM_Variable targetSetting)
        {
            this.targetSetting = targetSetting;
            this.settingID += String.Format("<{0}>",targetSetting.Name);
        }

        protected override void OnStarted()
        {
            GCode.SendEEPROMVariable(this.targetSetting);
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("ok"))
                this.FinishOrAdvance();
        }
    }
}

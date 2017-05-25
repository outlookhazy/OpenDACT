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

        private EEPROM_Variable targetSetting;

        public ApplySettingWF(EEPROM_Variable targetSetting)
        {
            this.targetSetting = targetSetting;
            this.settingID += String.Format("<{0}>",targetSetting.Name);
            this.ID = this.settingID;
        }

        protected override void OnStarted()
        {
            SerialSource.WriteLine(GCode.Command.SEND_EEPROM_VARIABLE(targetSetting));
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("ok"))
                this.FinishOrAdvance();
        }
    }
}

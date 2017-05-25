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

        private SerialManager serialSource;
        private EEPROM_Variable targetSetting;

        public ApplySettingWF(SerialManager serialSource, EEPROM_Variable targetSetting)
        {
            this.serialSource = serialSource;
            this.targetSetting = targetSetting;
            this.settingID += String.Format("<{0}>",targetSetting.Name);
            this.ID = this.settingID;
        }

        protected override void OnStarted()
        {
            serialSource.WriteLine(GCode.Command.SEND_EEPROM_VARIABLE(targetSetting));
        }

        protected override void OnMessage(string serialMessage)
        {
            if (serialMessage.Contains("ok"))
                this.FinishOrAdvance();
        }
    }
}

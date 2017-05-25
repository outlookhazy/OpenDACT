using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    class FastCalibrationWF : Workflow
    {
        private SerialManager serialSource;
        public FastCalibrationWF(SerialManager serialSource)
        {
            this.ID = "FastCalibrationWF";
            this.serialSource = serialSource;
        }
        protected override void OnStarted()
        {
            Program.mainFormTest.SetUserVariables();
            //this.AddWorkflowItem(new ZProbeMeasureWF());
            this.AddWorkflowItem(new HomeWF(serialSource));
            ReadEEPROMWF reader = new ReadEEPROMWF(serialSource);
            this.AddWorkflowItem(reader);
            MeasureHeightsWF heights = new MeasureHeightsWF(serialSource, reader.EEPROM);
            this.AddWorkflowItem(heights);
            this.AddWorkflowItem(new FastCalibrationEvaluateWF(serialSource, heights));
        }

        internal class FastCalibrationEvaluateWF : Workflow
        {
            private HeightMap Heights;
            private EEPROM adjustedEEPROM;
            private SerialManager serialSource;
            MeasureHeightsWF sourceHeightsWF;

            public FastCalibrationEvaluateWF(SerialManager serialSource, MeasureHeightsWF HeightsWF)
            {
                this.ID = "FastCalibrationEvaluateWF";
                this.serialSource = serialSource;
                this.sourceHeightsWF = HeightsWF;
            }
            protected override void OnStarted()
            {
                this.adjustedEEPROM = sourceHeightsWF.ResultEEPROM.Copy();
                this.Heights = sourceHeightsWF.ResultHeights;
                //Program.mainFormTest.SetAccuracyPoint(iterationNum, Calibration.AverageAccuracy());
                if (!(this.Heights.PrecisionReached(UserVariables.accuracy)))
                {
                    this.adjustedEEPROM = Calibration.TowerOffsets(this.Heights, this.adjustedEEPROM);
                    this.adjustedEEPROM = Calibration.AlphaRotation(this.Heights, this.adjustedEEPROM);
                    this.adjustedEEPROM = Calibration.StepsPMM(this.Heights, this.adjustedEEPROM);
                    this.adjustedEEPROM = Calibration.HRad(this.Heights, this.adjustedEEPROM);
                    this.adjustedEEPROM.AdjustZLength(UserVariables.probeChoice);
                    Program.mainFormTest.SetEEPROMGUIList(this.adjustedEEPROM);
                    this.AddWorkflowItem(new ApplySettingsWF(this.serialSource,this.adjustedEEPROM));
                }
            }
        }
    }
}

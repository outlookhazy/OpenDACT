using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    class FastCalibrationWF : Workflow
    {
        public new string ID { get { return "FastCalibrationWF"; } set { return; } }

        private SerialManager serialSource;
        public FastCalibrationWF(SerialManager serialSource)
        {
            this.serialSource = serialSource;
        }
        protected override void OnStarted()
        {
            //this.AddWorkflowItem(new ZProbeMeasureWF());
            this.AddWorkflowItem(new HomeWF(serialSource));
            ReadEEPROMWF reader = new ReadEEPROMWF(serialSource);
            this.AddWorkflowItem(reader);
            this.AddWorkflowItem(new MeasureHeightsWF(serialSource,reader.EEPROM));
            this.AddWorkflowItem(new FastCalibrationEvaluateWF());
        }

        internal class FastCalibrationEvaluateWF : Workflow
        {
            public new string ID { get { return "FastCalibrationEvaluateWF"; } set { return; } }

            private HeightMap Heights;
            private EEPROM adjustedEEPROM;
            private SerialManager serialSource;

            FastCalibrationEvaluateWF(SerialManager serialSource, EEPROM sourceEEPROM, HeightMap Heights)
            {
                this.serialSource = serialSource;
                this.adjustedEEPROM = sourceEEPROM.Copy();
                this.Heights = Heights;
            }
            protected override void OnStarted()
            {
                //Program.mainFormTest.SetAccuracyPoint(iterationNum, Calibration.AverageAccuracy());
                if (!(Calibration.PrecisionReached(this.Heights[Position.X].Z, this.Heights[Position.XOPP].Z, this.Heights[Position.Y].Z, this.Heights[Position.YOPP].Z, this.Heights[Position.Z].Z, this.Heights[Position.ZOPP].Z)))
                {
                    Calibration.TowerOffsets(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                    Calibration.AlphaRotation(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                    Calibration.StepsPMM(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                    Calibration.HRad(ref Heights.X, ref Heights.XOpp, ref Heights.Y, ref Heights.YOpp, ref Heights.Z, ref Heights.ZOpp);
                    Program.mainFormTest.SetEEPROMGUIList();
                    this.AddWorkflowItem(new ApplySettingsWF());
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    class FastCalibrationWF : Workflow
    {
        public override string ID { get { return "FastCalibrationWF"; } set { return; } }

        protected override void OnStarted()
        {
            //this.AddWorkflowItem(new ZProbeMeasureWF());
            this.AddWorkflowItem(new HomeWF());
            this.AddWorkflowItem(new ReadEEPROMWF());
            this.AddWorkflowItem(new MeasureHeightsWF());
            this.AddWorkflowItem(new FastCalibrationEvaluateWF());
        }

        internal class FastCalibrationEvaluateWF : Workflow
        {
            public override string ID { get { return "FastCalibrationEvaluateWF"; } set { return; } }

            protected override void OnStarted()
            {
                //Program.mainFormTest.SetAccuracyPoint(iterationNum, Calibration.AverageAccuracy());
                if (!(Calibration.PrecisionReached(Heights.X, Heights.XOpp, Heights.Y, Heights.YOpp, Heights.Z, Heights.ZOpp)))
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

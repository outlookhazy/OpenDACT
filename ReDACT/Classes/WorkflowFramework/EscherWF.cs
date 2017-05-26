using ReDACT;
using ReDACT.Classes;
using ReDACT.Classes.Escher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class EscherWF : Workflow
    {
        static TParameters testData;
        static DParameters deltaParameters;
        static EEPROM EEPROM;
        static CalibrationResult Result;

        public EscherWF(TParameters testData)
        {
            this.ID = "EscherWF";
            EscherWF.testData = testData;
        }

        protected override void OnStarted()
        {
            this.AddWorkflowItem(new HomeWF());

            this.AddWorkflowItem(new ReadEEPROMWF(out EscherWF.EEPROM));
            this.AddWorkflowItem(new EscherPrepareWF());

            this.AddWorkflowItem(new EscherMeasureHeightsWF());

            this.AddWorkflowItem(new EscherCalculateWF());

            this.AddWorkflowItem(new EscherUpdateEEPROMWF());
        }

        internal class EscherUpdateEEPROMWF : Workflow
        {
            public EscherUpdateEEPROMWF()
            {
                this.ID = "EscherUpdateEEPROMWF";
            }

            protected override void OnStarted()
            {
                EEPROM original = EscherWF.EEPROM.Copy();

                EscherWF.EEPROM[EEPROM_POSITION.StepsPerMM].Value = Convert.ToSingle(EscherWF.deltaParameters.stepspermm);
                EscherWF.EEPROM[EEPROM_POSITION.diagonalRod].Value = Convert.ToSingle(EscherWF.deltaParameters.diagonal);
                EscherWF.EEPROM[EEPROM_POSITION.HRadius].Value = Convert.ToSingle(EscherWF.deltaParameters.radius);
                EscherWF.EEPROM[EEPROM_POSITION.zMaxLength].Value = Convert.ToSingle(EscherWF.deltaParameters.homedHeight);
                EscherWF.EEPROM[EEPROM_POSITION.offsetX].Value = Convert.ToSingle(EscherWF.deltaParameters.xstop);
                EscherWF.EEPROM[EEPROM_POSITION.offsetY].Value = Convert.ToSingle(EscherWF.deltaParameters.ystop);
                EscherWF.EEPROM[EEPROM_POSITION.offsetZ].Value = Convert.ToSingle(EscherWF.deltaParameters.zstop);
                EscherWF.EEPROM[EEPROM_POSITION.A].Value = Convert.ToSingle(EscherWF.deltaParameters.xadj + 210);
                EscherWF.EEPROM[EEPROM_POSITION.B].Value = Convert.ToSingle(EscherWF.deltaParameters.yadj + 330);
                EscherWF.EEPROM[EEPROM_POSITION.C].Value = Convert.ToSingle(EscherWF.deltaParameters.zadj + 90);

                foreach (EEPROM_POSITION p in typeof(EEPROM_POSITION).GetEnumValues())
                {
                    if (p != EEPROM_POSITION.INVALID)
                    {
                        if (original[p].Value != EscherWF.EEPROM[p].Value)
                            Debug.WriteLine(String.Format("{0} changed from {1} to {2}", original[p].Name, original[p].Value, EscherWF.EEPROM[p].Value));
                        else
                            Debug.WriteLine(String.Format("\t{0} unchanged", original[p].Name));
                    }
                }

                if (EscherWF.Result.Success)
                    this.AddWorkflowItem(new ApplySettingsWF(ref EscherWF.EEPROM));
                else
                    Debug.WriteLine("Aborting EEPROM Update");
            }
        }

        internal class EscherCalculateWF : Workflow
        {
            public EscherCalculateWF()
            {
                this.ID = "EscherCalculateWF";
            }

            protected override void OnStarted()
            {
                EscherWF.Result = Escher3D.calc(ref EscherWF.deltaParameters, ref EscherWF.testData);
            }
        }

        public class EscherMeasureHeightsWF : Workflow
        {
            public EscherMeasureHeightsWF()
            {
                this.ID = "EscherMeasureHeightsWF";
            }

            protected override void OnStarted()
            {
                for (int i = 0; i < EscherWF.testData.numPoints; i++)
                {
                    this.AddWorkflowItem(new ProbeAtLocationWF(
                        EscherWF.testData.xBedProbePoints[i],
                        EscherWF.testData.yBedProbePoints[i],
                        EscherWF.testData.probeHeight,
                        measureDone,
                        i));
                }
            }

            public void measureDone(double Result, int ID)
            {
                EscherWF.testData.zBedProbePoints[ID] = Result;
            }

            public delegate void onMeasureResult(double Result, int ID = 0);

            protected override void OnChildrenFinished()
            {
                EscherWF.testData.NormalizeProbePoints();
            }
        }

        internal class EscherPrepareWF : Workflow
        {
            public EscherPrepareWF()
            {
                this.ID = "EscherPrepareWF";
            }

            protected override void OnStarted()
            {
                //(double diagonal, double radius, double homedHeight, double xstop, double ystop, double zstop, double xadj, double yadj, double zadj)
                EscherWF.testData.probeHeight = EEPROM[EEPROM_POSITION.zProbeHeight].Value + 11;
                EscherWF.testData.calcProbePoints(EEPROM[EEPROM_POSITION.printableRadius].Value);
                EscherWF.deltaParameters = new DParameters(
                    EEPROM[EEPROM_POSITION.diagonalRod].Value,
                    EEPROM[EEPROM_POSITION.HRadius].Value,
                    EEPROM[EEPROM_POSITION.zMaxLength].Value,
                    EEPROM[EEPROM_POSITION.offsetX].Value,
                    EEPROM[EEPROM_POSITION.offsetY].Value,
                    EEPROM[EEPROM_POSITION.offsetZ].Value,
                    EEPROM[EEPROM_POSITION.A].Value - 210,
                    EEPROM[EEPROM_POSITION.B].Value - 330,
                    EEPROM[EEPROM_POSITION.C].Value - 90)
                {
                    firmware = EscherWF.testData.firmware,
                    stepspermm = EEPROM[EEPROM_POSITION.StepsPerMM].Value
                };
                EscherWF.deltaParameters.Recalc(); ;
            }
        }

        internal class EscherEvalueateWF : Workflow
        {

            private SerialManager serialSource;
            MeasureHeightsWF sourceHeightsWF;

            public EscherEvalueateWF(SerialManager serialSource, MeasureHeightsWF HeightsWF)
            {
                this.ID = "EscherEvalueateWF";
                this.serialSource = serialSource;
                this.sourceHeightsWF = HeightsWF;
            }
            protected override void OnStarted()
            {

            }
        }
    }
}

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
    class EscherWF : Workflow
    {
        TParameters testData;
        DParameters deltaParameters;
        EEPROM EEPROM;
        CalibrationResult Result;

        public EscherWF(TParameters testData)
        {
            this.ID = "EscherWF";
            this.testData = testData;
        }

        protected override void OnStarted()
        {
            this.AddWorkflowItem(new HomeWF());

            this.AddWorkflowItem( new ReadEEPROMWF(out EEPROM));
            this.AddWorkflowItem( new EscherPrepareWF(ref EEPROM,ref testData, out deltaParameters));

            this.AddWorkflowItem( new EscherMeasureHeightsWF(ref testData));

            this.AddWorkflowItem( new EscherCalculateWF(ref this.deltaParameters, ref this.testData, ref this.Result));

            this.AddWorkflowItem(new EscherUpdateEEPROMWF(ref this.Result, ref EEPROM, ref deltaParameters));            
        }

        internal class EscherUpdateEEPROMWF : Workflow
        {
            DParameters targetParameters;
            EEPROM targetEEPROM;
            CalibrationResult Result;

            public EscherUpdateEEPROMWF(ref CalibrationResult Result, ref EEPROM sourceEEPROM, ref DParameters newParameters)
            {
                this.ID = "EscherUpdateEEPROMWF";
                this.Result = Result;
                this.targetEEPROM = sourceEEPROM;
                this.targetParameters = newParameters;
            }

            protected override void OnStarted()
            {
                EEPROM original = targetEEPROM.Copy();

                targetEEPROM[EEPROM_POSITION.StepsPerMM].Value = Convert.ToSingle(targetParameters.stepspermm);
                targetEEPROM[EEPROM_POSITION.diagonalRod].Value = Convert.ToSingle(targetParameters.diagonal);
                targetEEPROM[EEPROM_POSITION.HRadius].Value = Convert.ToSingle(targetParameters.radius);
                targetEEPROM[EEPROM_POSITION.zMaxLength].Value = Convert.ToSingle(targetParameters.homedHeight);
                targetEEPROM[EEPROM_POSITION.offsetX].Value = Convert.ToSingle(targetParameters.xstop);
                targetEEPROM[EEPROM_POSITION.offsetY].Value = Convert.ToSingle(targetParameters.ystop);
                targetEEPROM[EEPROM_POSITION.offsetZ].Value = Convert.ToSingle(targetParameters.zstop);
                targetEEPROM[EEPROM_POSITION.A].Value = Convert.ToSingle(targetParameters.xadj + 210);
                targetEEPROM[EEPROM_POSITION.B].Value = Convert.ToSingle(targetParameters.yadj + 330);
                targetEEPROM[EEPROM_POSITION.C].Value = Convert.ToSingle(targetParameters.zadj + 90);

                foreach(EEPROM_POSITION p in typeof(EEPROM_POSITION).GetEnumValues())
                {
                    if(p != EEPROM_POSITION.INVALID &&
                        original[p].Value != targetEEPROM[p].Value)
                    {
                        Debug.WriteLine(String.Format("{0} changed from {1} to {2}", original[p].Name, original[p].Value, targetEEPROM[p].Value));
                    }
                }
                if (Result.Success)
                    this.AddWorkflowItem(new ApplySettingsWF(ref targetEEPROM));
                else
                    Debug.WriteLine("Aborting EEPROM Update");                
            }
        }

        internal class EscherCalculateWF : Workflow
        {
            DParameters deltaSource;
            TParameters testData;
            CalibrationResult Result;

            public EscherCalculateWF(ref DParameters deltaSource, ref TParameters testData, ref CalibrationResult Result)
            {
                this.ID = "EscherCalculateWF";                
                this.deltaSource = deltaSource;
                this.testData = testData;
                this.Result = Result;
            }

            protected override void OnStarted()
            {
                this.Result = Escher3D.calc(deltaSource, testData);
            }
        }

        internal class EscherMeasureHeightsWF : Workflow
        {            
            private TParameters pointSource;
            public EscherMeasureHeightsWF(ref TParameters pointSource)
            {
                this.pointSource = pointSource;
            }

            protected override void OnStarted()
            {
                for(int i=0; i<pointSource.numPoints; i++)
                {
                    this.AddWorkflowItem(new ProbeAtLocationWF(
                        this.pointSource.xBedProbePoints[i],
                        this.pointSource.yBedProbePoints[i],
                        this.pointSource.probeHeight,
                        ref this.pointSource.zBedProbePoints[i]));
                }
            }

            protected override void OnChildrenFinished()
            {
                this.pointSource.NormalizeProbePoints();
            }
        }

        internal class EscherPrepareWF : Workflow
        {
            private EEPROM EEPROM;
            private TParameters targetData;
            private DParameters deltaParameters;

            public EscherPrepareWF(ref EEPROM EEPROM, ref TParameters targetData, out DParameters deltaParameters)
            {
                this.ID = "EscherPrepareWF";
                this.EEPROM = EEPROM;
                this.targetData = targetData;
                deltaParameters = new DParameters();
                this.deltaParameters = deltaParameters;
            }

            protected override void OnStarted()
            {
                //(double diagonal, double radius, double homedHeight, double xstop, double ystop, double zstop, double xadj, double yadj, double zadj)
                this.targetData.probeHeight = EEPROM[EEPROM_POSITION.zProbeHeight].Value + 11;
                this.targetData.calcProbePoints(EEPROM[EEPROM_POSITION.printableRadius].Value);
                
                this.deltaParameters.diagonal = EEPROM[EEPROM_POSITION.diagonalRod].Value;
                this.deltaParameters.radius = EEPROM[EEPROM_POSITION.HRadius].Value;
                this.deltaParameters.homedHeight = EEPROM[EEPROM_POSITION.zMaxLength].Value;
                this.deltaParameters.xstop = EEPROM[EEPROM_POSITION.offsetX].Value;
                this.deltaParameters.ystop = EEPROM[EEPROM_POSITION.offsetY].Value;
                this.deltaParameters.zstop = EEPROM[EEPROM_POSITION.offsetZ].Value;
                this.deltaParameters.xadj = EEPROM[EEPROM_POSITION.A].Value - 210;
                this.deltaParameters.yadj = EEPROM[EEPROM_POSITION.B].Value - 330;
                this.deltaParameters.zadj = EEPROM[EEPROM_POSITION.C].Value - 90;

                this.deltaParameters.firmware = this.targetData.firmware;
                this.deltaParameters.stepspermm = EEPROM[EEPROM_POSITION.StepsPerMM].Value;                
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

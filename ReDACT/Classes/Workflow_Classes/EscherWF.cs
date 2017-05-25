using ReDACT;
using ReDACT.Classes;
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
        private SerialManager serialSource;
        EscherPrepareWF preparation;
        TestData testData;

        public EscherWF(SerialManager serialSource, TestData testData)
        {
            this.ID = "EscherWF";
            this.serialSource = serialSource;
            this.testData = testData;
        }
        protected override void OnStarted()
        {
            this.AddWorkflowItem(new HomeWF(serialSource));

            ReadEEPROMWF eepReadResult = new ReadEEPROMWF(this.serialSource);
            this.preparation = new EscherPrepareWF(eepReadResult, this.testData);
            this.AddWorkflowItem(eepReadResult);
            this.AddWorkflowItem(preparation);
            this.AddWorkflowItem(new EscherMeasureHeightsWF(this.serialSource, this.testData));
            EscherCalculateWF parameterSource = new EscherCalculateWF(preparation, this.testData);
            this.AddWorkflowItem(parameterSource);
            this.AddWorkflowItem(new EscherUpdateEEPROMWF(serialSource, parameterSource, eepReadResult));
            
        }

        internal class EscherUpdateEEPROMWF : Workflow
        {
            SerialManager serialSource;
            EscherCalculateWF parameterSource;
            ReadEEPROMWF sourceEEPROM;
            EEPROM targetEEPROM;

            public EscherUpdateEEPROMWF(SerialManager serialSource, EscherCalculateWF parameterSource, ReadEEPROMWF sourceEEPROM)
            {
                this.serialSource = serialSource;
                this.parameterSource = parameterSource;
                this.sourceEEPROM = sourceEEPROM;
            }

            protected override void OnStarted()
            {
                EEPROM original = sourceEEPROM.EEPROM.Copy();

                targetEEPROM = sourceEEPROM.EEPROM;
                targetEEPROM[EEPROM_POSITION.StepsPerMM].Value = Convert.ToSingle(parameterSource.calculatedParameters.stepspermm);
                targetEEPROM[EEPROM_POSITION.diagonalRod].Value = Convert.ToSingle(parameterSource.calculatedParameters.diagonal);
                targetEEPROM[EEPROM_POSITION.HRadius].Value = Convert.ToSingle(parameterSource.calculatedParameters.radius);
                targetEEPROM[EEPROM_POSITION.zMaxLength].Value = Convert.ToSingle(parameterSource.calculatedParameters.homedHeight);
                targetEEPROM[EEPROM_POSITION.offsetX].Value = Convert.ToSingle(parameterSource.calculatedParameters.xstop);
                targetEEPROM[EEPROM_POSITION.offsetY].Value = Convert.ToSingle(parameterSource.calculatedParameters.ystop);
                targetEEPROM[EEPROM_POSITION.offsetZ].Value = Convert.ToSingle(parameterSource.calculatedParameters.zstop);
                targetEEPROM[EEPROM_POSITION.A].Value = Convert.ToSingle(parameterSource.calculatedParameters.xadj + 210);
                targetEEPROM[EEPROM_POSITION.B].Value = Convert.ToSingle(parameterSource.calculatedParameters.yadj + 330);
                targetEEPROM[EEPROM_POSITION.C].Value = Convert.ToSingle(parameterSource.calculatedParameters.zadj + 90);

                foreach(EEPROM_POSITION p in typeof(EEPROM_POSITION).GetEnumValues())
                {
                    if(p != EEPROM_POSITION.INVALID &&
                        original[p].Value != targetEEPROM[p].Value)
                    {
                        Debug.WriteLine(String.Format("{0} changed from {1} to {2}", original[p].Name, original[p].Value, targetEEPROM[p].Value));
                    }
                }

                this.AddWorkflowItem(new ApplySettingsWF(this.serialSource, targetEEPROM));
            }
        }

        internal class EscherCalculateWF : Workflow
        {
            EscherPrepareWF deltaParamsSource;
            TestData testDataSource;
            public DeltaParameters calculatedParameters;

            public EscherCalculateWF(EscherPrepareWF deltaParamsSource, TestData testDataSource)
            {
                this.deltaParamsSource = deltaParamsSource;
                this.testDataSource = testDataSource;
            }

            protected override void OnStarted()
            {
                this.calculatedParameters = Escher.Calc(this.deltaParamsSource.generatedParameters, this.testDataSource);
            }
        }

        internal class EscherMeasureHeightsWF : Workflow
        {
            SerialManager serialSource;
            TestData pointsSource;
            List<ProbeAtLocationWF> actionList;
            public EscherMeasureHeightsWF(SerialManager serialSource, TestData pointSource)
            {
                this.serialSource = serialSource;
                this.pointsSource = pointSource;
                this.actionList = new List<ProbeAtLocationWF>();
            }

            protected override void OnStarted()
            {
                foreach(Position3D pos in HeightMap.FromArray2D(pointsSource.TestPoints))
                {
                    ProbeAtLocationWF item = new ProbeAtLocationWF(serialSource, pos);
                    actionList.Add(item);
                    this.AddWorkflowItem(item);
                }
            }

            protected override void OnChildrenFinished()
            {
                HeightMap Results = new HeightMap();
                foreach(ProbeAtLocationWF wf in this.actionList)
                {
                    Position3D result = new Position3D(wf.probePoint.X, wf.probePoint.Y, wf.Result);
                    Results.Add(result);
                }
                this.pointsSource.TestPoints = Results.ToArray2D();
            }
        }

        internal class EscherPrepareWF : Workflow
        {
            ReadEEPROMWF eepromSource;
            public DeltaParameters generatedParameters;
            TestData targetData;
            public EscherPrepareWF(ReadEEPROMWF eepRomSource, TestData targetData)
            {
                this.ID = "EscherPrepareWF";
                this.eepromSource = eepRomSource;
                this.targetData = targetData;
            }

            protected override void OnStarted()
            {
                double bedRadius = eepromSource.EEPROM[EEPROM_POSITION.printableRadius].Value;
                double[,] testPoints = TestData.CalcProbePoints(targetData.NumPoints, bedRadius, eepromSource.EEPROM[EEPROM_POSITION.zProbeHeight].Value +5);
                targetData.ApplySettings(bedRadius, testPoints);
                generatedParameters = new DeltaParameters(
                    targetData.Firmware,
                    this.eepromSource.EEPROM[EEPROM_POSITION.StepsPerMM].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.diagonalRod].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.HRadius].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.zMaxLength].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.offsetX].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.offsetY].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.offsetZ].Value,
                    this.eepromSource.EEPROM[EEPROM_POSITION.A].Value - 210,
                    this.eepromSource.EEPROM[EEPROM_POSITION.B].Value - 330,
                    this.eepromSource.EEPROM[EEPROM_POSITION.C].Value - 90
                    );
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

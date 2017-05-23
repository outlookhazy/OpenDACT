using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MeasureHeightsWF : Workflow
    {
        public new string ID { get { return "MeasureHeightsWF"; } set { return; } }

        ProbeAtLocationWF X;
        ProbeAtLocationWF Xopp;
        ProbeAtLocationWF Y;
        ProbeAtLocationWF Yopp;
        ProbeAtLocationWF Z;
        ProbeAtLocationWF Zopp;
        ProbeAtLocationWF Center;

        public HeightMap ResultHeights;
        public EEPROM ResultEEPROM;

        private SerialManager serialSource;

        public MeasureHeightsWF(SerialManager serialSource, EEPROM sourceEEPROM)
        {
            this.serialSource = serialSource;
            this.ResultEEPROM = sourceEEPROM.Copy();
            this.ResultHeights = new HeightMap();
        }

        protected override void OnStarted()
        {
            float probingHeight = UserVariables.probingHeight;
            float plateDiameter = UserVariables.plateDiameter;

            float valueZ = 0.482F * plateDiameter;
            float valueXYLarge = 0.417F * plateDiameter;
            float valueXYSmall = 0.241F * plateDiameter;

            this.ResultHeights[Position.CENTER].X = 0;
            this.ResultHeights[Position.CENTER].Y = 0;

            this.ResultHeights[Position.X].X = -valueXYLarge;
            this.ResultHeights[Position.X].Y = -valueXYSmall;

            this.ResultHeights[Position.XOPP].X = valueXYLarge;
            this.ResultHeights[Position.XOPP].Y = valueXYSmall;

            this.ResultHeights[Position.Y].X = valueXYLarge;
            this.ResultHeights[Position.Y].Y = -valueXYSmall;

            this.ResultHeights[Position.YOPP].X = -valueXYLarge;
            this.ResultHeights[Position.YOPP].Y = valueXYSmall;

            this.ResultHeights[Position.Z].X = 0;
            this.ResultHeights[Position.Z].Y = valueZ;

            this.ResultHeights[Position.ZOPP].X = 0;
            this.ResultHeights[Position.ZOPP].Y = -valueZ;

            Center = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.CENTER].X,
                    this.ResultHeights[Position.CENTER].Y, 
                    probingHeight));

            X = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.X].X,
                    this.ResultHeights[Position.X].Y, 
                    probingHeight));

            Xopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.XOPP].X,
                    this.ResultHeights[Position.XOPP].Y, 
                    probingHeight));

            Y = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.Y].X,
                    this.ResultHeights[Position.Y].Y, 
                    probingHeight));

            Yopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.YOPP].X,
                    this.ResultHeights[Position.YOPP].Y, 
                    probingHeight));

            Z = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.Z].X,
                    this.ResultHeights[Position.Z].Y, 
                    probingHeight));

            Zopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[Position.ZOPP].X,
                    this.ResultHeights[Position.ZOPP].Y, 
                    probingHeight));

            MoveWF park = new MoveWF(serialSource, new Position3D(0, 0, Convert.ToInt32(ResultEEPROM[EEPROM_POSITION.zMaxLength].Value / 3)));// park

            this.AddWorkflowItem(new ReadEEPROMWF(serialSource)); //measure heights without read will crash due to no max z length
            this.AddWorkflowItem(Center);
            this.AddWorkflowItem(X);
            this.AddWorkflowItem(Xopp);
            this.AddWorkflowItem(Y);
            this.AddWorkflowItem(Yopp);
            this.AddWorkflowItem(Z);
            this.AddWorkflowItem(Zopp);
            this.AddWorkflowItem(park);
        }

        protected override void OnChildrenFinished()
        {
            float zMaxLength = ResultEEPROM[EEPROM_POSITION.zMaxLength].Value;
            float probingHeight = UserVariables.probingHeight;

            float probeOffset = zMaxLength - probingHeight;

            ResultHeights[Position.CENTER].Z = probeOffset + Center.Result;
            ResultHeights[Position.X].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + X.Result));
            ResultHeights[Position.XOPP].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + Xopp.Result));
            ResultHeights[Position.Y].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + Y.Result));
            ResultHeights[Position.YOPP].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + Yopp.Result));
            ResultHeights[Position.Z].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + Z.Result));
            ResultHeights[Position.ZOPP].Z = -(ResultHeights[Position.CENTER].Z - (probeOffset + Zopp.Result));

            ResultEEPROM[EEPROM_POSITION.zMaxLength].Value = ResultHeights[Position.CENTER].Z;            
        }
    }
}

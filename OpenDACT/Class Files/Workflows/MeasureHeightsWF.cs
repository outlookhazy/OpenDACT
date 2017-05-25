using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MeasureHeightsWF : Workflow
    {
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
        private EEPROM sourceEEPROM;

        public MeasureHeightsWF(SerialManager serialSource, EEPROM sourceEEPROM)
        {
            this.ID = "MeasureHeightsWF";
            this.serialSource = serialSource;
            this.sourceEEPROM = sourceEEPROM;
            this.ResultHeights = new HeightMap();
        }

        protected override void OnStarted()
        {
            this.ResultEEPROM = sourceEEPROM.Copy();
            float probingHeight = UserVariables.probingHeight;
            Debug.WriteLine("Using probing height {0}", probingHeight);
            float plateDiameter = UserVariables.plateDiameter;

            float valueZ = 0.482F * plateDiameter;
            float valueXYLarge = 0.417F * plateDiameter;
            float valueXYSmall = 0.241F * plateDiameter;

            this.ResultHeights[HeightMap.Position.CENTER].X = 0;
            this.ResultHeights[HeightMap.Position.CENTER].Y = 0;

            this.ResultHeights[HeightMap.Position.X].X = -valueXYLarge;
            this.ResultHeights[HeightMap.Position.X].Y = -valueXYSmall;

            this.ResultHeights[HeightMap.Position.XOPP].X = valueXYLarge;
            this.ResultHeights[HeightMap.Position.XOPP].Y = valueXYSmall;

            this.ResultHeights[HeightMap.Position.Y].X = valueXYLarge;
            this.ResultHeights[HeightMap.Position.Y].Y = -valueXYSmall;

            this.ResultHeights[HeightMap.Position.YOPP].X = -valueXYLarge;
            this.ResultHeights[HeightMap.Position.YOPP].Y = valueXYSmall;

            this.ResultHeights[HeightMap.Position.Z].X = 0;
            this.ResultHeights[HeightMap.Position.Z].Y = valueZ;

            this.ResultHeights[HeightMap.Position.ZOPP].X = 0;
            this.ResultHeights[HeightMap.Position.ZOPP].Y = -valueZ;

            Center = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.CENTER].X,
                    this.ResultHeights[HeightMap.Position.CENTER].Y, 
                    probingHeight));

            X = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.X].X,
                    this.ResultHeights[HeightMap.Position.X].Y, 
                    probingHeight));

            Xopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.XOPP].X,
                    this.ResultHeights[HeightMap.Position.XOPP].Y, 
                    probingHeight));

            Y = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.Y].X,
                    this.ResultHeights[HeightMap.Position.Y].Y, 
                    probingHeight));

            Yopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.YOPP].X,
                    this.ResultHeights[HeightMap.Position.YOPP].Y, 
                    probingHeight));

            Z = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.Z].X,
                    this.ResultHeights[HeightMap.Position.Z].Y, 
                    probingHeight));

            Zopp = new ProbeAtLocationWF(serialSource, 
                new Position3D(
                    this.ResultHeights[HeightMap.Position.ZOPP].X,
                    this.ResultHeights[HeightMap.Position.ZOPP].Y, 
                    probingHeight));

            MoveWF park = new MoveWF(serialSource, new Position3D(0, 0, Convert.ToInt32(ResultEEPROM[EEPROM_POSITION.zMaxLength].Value / 3)));// park

            this.AddWorkflowItem(new ReadEEPROMWF(serialSource)); //measure heights without read will crash due to no max z length
            
            this.AddWorkflowItem(Z);
            this.AddWorkflowItem(Xopp);
            this.AddWorkflowItem(Y);
            this.AddWorkflowItem(Zopp);
            this.AddWorkflowItem(X);
            this.AddWorkflowItem(Yopp);
            this.AddWorkflowItem(Center);
            this.AddWorkflowItem(park);
        }

        protected override void OnChildrenFinished()
        {
            float zMaxLength = ResultEEPROM[EEPROM_POSITION.zMaxLength].Value;
            float probingHeight = UserVariables.probingHeight;

            float probeOffset = zMaxLength - probingHeight;

            ResultHeights[HeightMap.Position.CENTER].Z = probeOffset + Center.Result;
            ResultHeights[HeightMap.Position.X].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + X.Result));
            ResultHeights[HeightMap.Position.XOPP].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + Xopp.Result));
            ResultHeights[HeightMap.Position.Y].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + Y.Result));
            ResultHeights[HeightMap.Position.YOPP].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + Yopp.Result));
            ResultHeights[HeightMap.Position.Z].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + Z.Result));
            ResultHeights[HeightMap.Position.ZOPP].Z = -(ResultHeights[HeightMap.Position.CENTER].Z - (probeOffset + Zopp.Result));

            ResultEEPROM[EEPROM_POSITION.zMaxLength].Value = ResultHeights[HeightMap.Position.CENTER].Z;            
        }
    }
}

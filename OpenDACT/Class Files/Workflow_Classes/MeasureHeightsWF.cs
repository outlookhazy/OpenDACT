﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files.Workflow_Classes
{
    public class MeasureHeightsWF : Workflow
    {
        public override string ID { get { return "MeasureHeightsWF"; } set { return; } }

        ProbeAtLocationWF X;
        ProbeAtLocationWF Xopp;
        ProbeAtLocationWF Y;
        ProbeAtLocationWF Yopp;
        ProbeAtLocationWF Z;
        ProbeAtLocationWF Zopp;
        ProbeAtLocationWF Center;

        protected override void OnStarted()
        {
            float probingHeight = UserVariables.probingHeight;
            float plateDiameter = UserVariables.plateDiameter;

            float valueZ = 0.482F * plateDiameter;
            float valueXYLarge = 0.417F * plateDiameter;
            float valueXYSmall = 0.241F * plateDiameter;

            Center = new ProbeAtLocationWF(new Position3D(0, 0, probingHeight));
            X = new ProbeAtLocationWF(new Position3D(-valueXYLarge, -valueXYSmall, probingHeight));
            Xopp = new ProbeAtLocationWF(new Position3D(valueXYLarge, valueXYSmall, probingHeight));
            Y = new ProbeAtLocationWF(new Position3D(valueXYLarge, -valueXYSmall, probingHeight));
            Yopp = new ProbeAtLocationWF(new Position3D(-valueXYLarge, valueXYSmall, probingHeight));
            Z = new ProbeAtLocationWF(new Position3D(0, valueZ, probingHeight));
            Zopp = new ProbeAtLocationWF(new Position3D(0, -valueZ, probingHeight));
            MoveWF park = new MoveWF(new Position3D(0, 0, Convert.ToInt32(EEPROM.zMaxLength.Value / 3)));// park

            this.AddWorkflowItem(new ReadEEPROMWF()); //measure heights without read will crash due to no max z length
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
            float zMaxLength = EEPROM.zMaxLength.Value;
            float probingHeight = UserVariables.probingHeight;

            float probeOffset = zMaxLength - probingHeight;

            Heights.center = probeOffset + Center.Result;
            Heights.X = -(Heights.center - (probeOffset + X.Result));
            Heights.XOpp = -(Heights.center - (probeOffset + Xopp.Result));
            Heights.Y = -(Heights.center - (probeOffset + Y.Result));
            Heights.YOpp = -(Heights.center - (probeOffset + Yopp.Result));
            Heights.Z = -(Heights.center - (probeOffset + Z.Result));
            Heights.ZOpp = -(Heights.center - (probeOffset + Zopp.Result));

            EEPROM.zMaxLength.Value = Heights.center;            
        }
    }
}

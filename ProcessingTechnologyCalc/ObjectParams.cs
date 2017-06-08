using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingTechnologyCalc
{
    public class ProcessOptions 
    {
        public int MaterialType;
        public int GreatSpeed;
        public int SmallSpeed;
        public int Frequency;
        public int ToolNo;
        public struct Tool
        {
            public double Diameter;
            public double Thickness;
        }
        public Tool[] Tools; 
        public int DepthAll;
        public int Depth;

        public ProcessOptions()
        {
            this.MaterialType = 0;
            this.GreatSpeed = 1500;
            this.SmallSpeed = 250;
            this.Frequency = 1800;
            this.DepthAll = 32;
            this.Depth = 8;
            this.ToolNo = 1;
            this.Tools = new ProcessOptions.Tool[3];
            this.Tools[0].Diameter = 100;
            this.Tools[0].Thickness = 10;
            this.Tools[1].Diameter = 200;
            this.Tools[1].Thickness = 20;
            this.Tools[2].Diameter = 300;
            this.Tools[2].Thickness = 30;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ProcessingTechnologyCalc
{
    public class ProcessAction
    {
        public string Command { get; set; }

        public Curve Toolpath { get; set; }

        public Point3d Point { get; set; }

        public double Angle { get; set; }

        public bool IsClockwise { get; set; }
    }
}

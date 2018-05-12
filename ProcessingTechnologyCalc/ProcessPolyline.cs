using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using System.ComponentModel;

namespace ProcessingTechnologyCalc
{
    //-------------------------------------------------------------------    полилиния   --------------------------
    class ProcessObjectPolyline : ProcessObject
    {
        private static int last_no;

        public ProcessObjectPolyline(Curve acadObject, ProcessOptions processOptions)
            : base(acadObject, processOptions)
        {
            ObjectName = "Полилиния" + (++last_no).ToString();
            TypeName = "Полилиния";
            ObjectType = ObjectType.Polyline;
        }

        public override double Length
        {
            get
            {
                return (ProcessCurve as Polyline).Length;
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Количество вершин"), DescriptionAttribute("Количество вершин")]
        public double Angle
        {
            get
            {
                return (ProcessCurve as Polyline).NumberOfVertices;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Длина"), DescriptionAttribute("Длина отрезка")]
        public double ToolpathLength
        {
            get
            {
                return 0; // ToolpathCurve != null ? ToolpathLine.Length : 0;
            }
        }
    }
}

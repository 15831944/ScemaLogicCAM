using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using System.Drawing;
using System.ComponentModel;

namespace ProcessingTechnologyCalc
{
    //---------------------------------------------------------------------     дуга        -----------------------
    public class ProcessObjectArc : ProcessObject
    {
        private static int last_no;
        private PointF centerPoint = new PointF();
        private PointF toolpathCenterPoint = new PointF();
        public double Compensation;
        public double BetaGrad;

        public ProcessObjectArc(Curve acadObject, ProcessOptions processOptions)
            : base(acadObject, processOptions)
        {
            RefreshProcessCurveData();
            ObjectName = "Дуга" + (++last_no).ToString();
            TypeName = "Дуга";
            ObjectType = ObjectType.Arc;
        } 

        public override void RefreshProcessCurveData()
        {
            base.RefreshProcessCurveData();
            ConvertToPointF(ProcessArc.Center, ref centerPoint);
        }

        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Точка центр"), DescriptionAttribute("Центр дуги")]
        public PointF Center
        {
            get
            {
                return centerPoint;
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Угол начало"), DescriptionAttribute("Начальный угол дуги в градусах")]
        public double StartAngle
        {
            get
            {
                return Math.Round(ProcessArc.StartAngle * 180 / Math.PI, 3);
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Угол конец"), DescriptionAttribute("Конечный угол дуги в градусах")]
        public double EndAngle
        {
            get
            {
                return Math.Round(ProcessArc.EndAngle * 180 / Math.PI, 3);
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Радиус"), DescriptionAttribute("Радиус дуги")]
        public double Radius
        {
            get
            {
                return ProcessArc.Radius;
            }
        }
        public override double Length
        {
            get
            {
                return ProcessArc.Length;
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Угол полный"), DescriptionAttribute("Угол сектора в градусах")]
        public double TotalAngle
        {
            get
            {
                return Math.Round(ProcessArc.TotalAngle * 180 / Math.PI, 3);
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Точка центр"), DescriptionAttribute("Центр дуги")]
        public PointF ToolpathCenter
        {
            get
            {
                if (ToolpathCurve != null)
                {
                    ConvertToPointF(ToolpathArc.Center, ref toolpathCenterPoint);
                }
                else
                {
                    ClearPointF(ref toolpathCenterPoint);
                }
                return toolpathCenterPoint;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Угол начало"), DescriptionAttribute("Начальный угол дуги в градусах")]
        public double ToolpathStartAngle
        {
            get
            {
                return ToolpathCurve != null ? Math.Round(ToolpathArc.StartAngle * 180 / Math.PI, 3) : 0;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Угол конец"), DescriptionAttribute("Конечный угол дуги в градусах")]
        public double ToolpathEndAngle
        {
            get
            {
                return ToolpathCurve != null ? Math.Round(ToolpathArc.EndAngle * 180 / Math.PI, 3) : 0;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Радиус"), DescriptionAttribute("Радиус дуги")]
        public double ToolpathRadius
        {
            get
            {
                return ToolpathCurve != null ? ToolpathArc.Radius : 0;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Длина"), DescriptionAttribute("Длина дуги")]
        public double ToolpathLength
        {
            get
            {
                return ToolpathCurve != null ? ToolpathArc.Length : 0;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Угол полный"), DescriptionAttribute("Угол сектора в градусах")]
        public double ToolpathTotalAngle
        {
            get
            {
                return ToolpathCurve != null ? Math.Round(ToolpathArc.TotalAngle * 180 / Math.PI, 3) : 0;
                //return BetaGrad;
            }
        }
    }
}

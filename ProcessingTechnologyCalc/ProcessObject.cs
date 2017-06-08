using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ProcessingTechnologyCalc
{
    //-------------------------------------------------------------------     базовый класс   ---------------------
    [DefaultPropertyAttribute("Название")]
    abstract public class ProcessObject
    {
        public ObjectType ObjectType;

        public readonly Curve ProcessCurve;
        public readonly Line ProcessLine;
        public readonly Arc ProcessArc;

        private Curve toolpathCurve;
        public  Line ToolpathLine;
        public  Arc ToolpathArc;

        public SideType Side = SideType.None;
        public ProcessObject[] ConnectObject = new ProcessObject[2];
        public VertexType[] ConnectVertex = new VertexType[2];
        //public Point3d[] ConnectPoint = new Point3d[2];

        public bool[] IsExactly = { false, false };

        private PointF startPoint = new PointF();
        private PointF endPoint = new PointF();
        private PointF toolpathStartPoint = new PointF();
        private PointF toolpathEndPoint = new PointF();

        private ProcessOptions processOptions;

        public ProcessObject(Curve acadObject, ProcessOptions processOptions)
        {
            ProcessCurve = acadObject;

            ProcessLine  = ProcessCurve as Line;
            ProcessArc   = ProcessCurve as Arc;

            RefreshProcessCurveData();

            this.processOptions = new ProcessOptions(processOptions);
        }
        protected void ConvertToPointF(Point3d point3d, ref PointF pointF)
        {
            pointF.X = (float)point3d.X;
            pointF.Y = (float)point3d.Y;
        }
        protected void ClearPointF(ref PointF pointF)
        {
            pointF.X = 0;
            pointF.Y = 0;
        }
        public virtual void RefreshProcessCurveData()
        {
            ConvertToPointF(ProcessCurve.StartPoint, ref startPoint);
            ConvertToPointF(ProcessCurve.EndPoint, ref endPoint);
        }
        public override string ToString()
        {
            return ObjectName;
        }

        [Browsable(false)]
        public Curve ToolpathCurve
        {
            get
            {
                return toolpathCurve;
            }
            set
            {
                toolpathCurve = value;
                ToolpathLine = toolpathCurve as Line;
                ToolpathArc = toolpathCurve as Arc;
            }
        }
        [CategoryAttribute("1. Общие"), DisplayName("Название"), DescriptionAttribute("Название объекта")]
        public string ObjectName { get; set; }

        [CategoryAttribute("1. Общие"), DisplayName("Тип"), DescriptionAttribute("Тип объекта")]
        public string TypeName { get; protected set; } // TODO из ObjectType

        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Точка начало"), DescriptionAttribute("Начальная вершина")]
        public PointF StartPoint
        {
            get
            {
                return startPoint;
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Точка конец"), DescriptionAttribute("Конечная вершина")]
        public PointF EndPoint
        {
            get
            {
                return endPoint;
            }
        }
        [CategoryAttribute("2. Геометрия объекта"), DisplayName("Длина"), DescriptionAttribute("Длина объекта")]
        public abstract double Length { get; }

        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Точка начало"), DescriptionAttribute("Начальная вершина")]
        public PointF ToolpathStartPoint
        {
            get
            {
                if (ToolpathCurve != null)
                {
                    ConvertToPointF(ToolpathCurve.StartPoint, ref toolpathStartPoint);
                }
                else
                {
                    ClearPointF(ref toolpathStartPoint);
                }
                return toolpathStartPoint;
            }
        }
        [CategoryAttribute("3. Геометрия траектории"), DisplayName("Точка конец"), DescriptionAttribute("Конечная вершина")]
        public PointF ToolpathEndPoint
        {
            get
            {
                if (ToolpathCurve != null)
                {
                    ConvertToPointF(ToolpathCurve.EndPoint, ref toolpathEndPoint);
                }
                else
                {
                    ClearPointF(ref toolpathEndPoint);
                }
                return toolpathEndPoint;
            }
        }

        [CategoryAttribute("4. Инструмент"), DisplayName("№ инструмента"), DescriptionAttribute("Номер используемого инструмента")]
        public int ToolNo 
        { 
            get { return processOptions.ToolNo; }
            set { processOptions.ToolNo = value; }
        }

        [CategoryAttribute("4. Инструмент"), DisplayName("Диаметр"), DescriptionAttribute("Диаметр используемого инструмента, мм")]
        public double Diameter
        {
            get { return ToolNo <= processOptions.ToolsList.Count ? processOptions.ToolsList[ToolNo - 1].Diameter : 0; }
        }

        [CategoryAttribute("4. Инструмент"), DisplayName("Толщина"), DescriptionAttribute("Толщина используемого инструмента, мм")]
        public double Thickness
        {
            get { return ToolNo <= processOptions.ToolsList.Count ? processOptions.ToolsList[ToolNo - 1].Thickness : 0; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Скорость большая"), DescriptionAttribute("Скорость реза большая, мм/мин")]
        public int GreatSpeed 
        {
            get { return processOptions.GreatSpeed; }
            set { processOptions.GreatSpeed = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Скорость малая"), DescriptionAttribute("Скорость реза малая, мм/мин")]
        public int SmallSpeed
        {
            get { return processOptions.SmallSpeed; }
            set { processOptions.SmallSpeed = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Шпиндель"), DescriptionAttribute("Скорость вращения шпинделя, об/мин")]
        public int Frequency
        {
            get { return processOptions.Frequency; }
            set { processOptions.Frequency = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Глубина реза"), DescriptionAttribute("Суммарная глубина реза, мм")]
        public int DepthAll
        {
            get { return processOptions.DepthAll; }
            set { processOptions.DepthAll = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Глубина построчно"), DescriptionAttribute("Глубина реза за один проход, мм")]
        public int Depth
        {
            get { return processOptions.Depth; }
            set { processOptions.Depth = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Точно начало"), DescriptionAttribute("Крайняя точка реза соответствует начальной точке объекта")]
        public bool IsBeginExactly
        {
            get { return IsExactly[VertexType.Start.Index()]; }
            set { IsExactly[VertexType.Start.Index()] = value; }
        }

        [CategoryAttribute("5. Параметры обработки"), DisplayName("Точно конец"), DescriptionAttribute("Крайняя точка реза соответствует конечной точке объекта")]
        public bool IsEndExactly
        {
            get { return IsExactly[VertexType.End.Index()]; }
            set { IsExactly[VertexType.End.Index()] = value; }
        }
    }
}

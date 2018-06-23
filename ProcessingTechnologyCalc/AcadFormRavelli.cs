using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcDb = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ProcessingTechnologyCalc
{
    public partial class AcadProcessesView
    {
        public void ConstructToolpathObjectRavelli(ProcessObject obj)
        {
            if (obj.Side == SideType.None)
            {
                return;
            }
            if (obj.VerticalAngleDeg == 90)
            {
                ConstructPlaneToolpath(obj);
                return;
            }
            double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll / Math.Cos(obj.VerticalAngle))) + ExactlyIncrease;
            if ((obj.IsBeginExactly || obj.IsEndExactly) && (obj.Length <= s) || (obj.IsBeginExactly && obj.IsEndExactly) && (obj.Length <= 2 * s))
            {
                Намечание(obj, s);
                return;
            }
            Curve toolpathCurve = GetOffsetCopy(obj.ProcessCurve, GetOffsetValueRavelli(obj));  // TODO расчет OffsetCurves + ModifiedEventHandler

            switch (obj.ObjectType)
            {
                case ObjectType.Line:

                    if (obj.IsBeginExactly)
                    {
                        toolpathCurve.StartPoint = toolpathCurve.GetPointAtDist(s);
                    }
                    if (obj.IsEndExactly)
                    {
                        toolpathCurve.EndPoint = toolpathCurve.GetPointAtDist((toolpathCurve as Line).Length - s);
                    }
                    break;

                case ObjectType.Arc:

                    if (obj.IsBeginExactly)
                    {
                        (toolpathCurve as Arc).StartAngle = obj.ProcessArc.StartAngle + s / obj.ProcessArc.Radius;
                    }
                    if (obj.IsEndExactly)
                    {
                        (toolpathCurve as Arc).EndAngle = obj.ProcessArc.EndAngle - s / obj.ProcessArc.Radius;
                    }
                    break;
            }
            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
                    if (obj.ProcessActions != null)
                        obj.ProcessActions.ForEach(p => trans.GetObject(p.Toolpath.ObjectId, OpenMode.ForWrite).Erase());

                    obj.ProcessActions = new List<ProcessAction>();
                    var layerId = GetProcessLayer(trans);
                    var tanAngle = Math.Tan(obj.VerticalAngle);

                    double angleStart = 0;
                    double angleEnd = 0;
                    if (obj.ObjectType == ObjectType.Arc)
                    {
                        angleStart = CalcAngle(obj, (toolpathCurve as Arc).StartAngle);
                        angleEnd = CalcAngle(obj, (toolpathCurve as Arc).EndAngle);
                    }
                    if (!obj.TopEdge)
                    {
                        s = (ProcessOptions.Thickness + ProcessOptions.ZSafety) * tanAngle;
                        var sign = obj.Side == SideType.Left ^ obj.ObjectType == ObjectType.Arc ? -1 : 1;
                        var tCurve0 = GetDisplacementCopy(GetOffsetCopy(toolpathCurve, sign * s), ProcessOptions.ZSafety);
                        var isStart = true;
                        var point0 = tCurve0.StartPoint;
                        Point3d point;
                        int d = 0;
                        if (obj.ObjectType == ObjectType.Arc)
                            d = -obj.Depth;
                        
                        do
                        {
                            d += obj.Depth;
                            if (d > obj.DepthAll)
                                d = obj.DepthAll;
                            s = (ProcessOptions.Thickness - d) * tanAngle;
                            var tCurve = GetDisplacementCopy(GetOffsetCopy(toolpathCurve, sign * s), -d);
                            point = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                            obj.ProcessActions.Add(new ProcessAction
                            {
                                Command = "Заглубление",
                                Toolpath = new Line(point0, point),
                                Point = point,
                                Angle = isStart ? angleStart : angleEnd,
                                IsClockwise = isStart
                            });
                            isStart = !isStart;
                            point0 = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                            obj.ProcessActions.Add(new ProcessAction
                            {
                                Command = "Рез",
                                Toolpath = tCurve,
                                Point = point0,
                                Angle = isStart ? angleStart : angleEnd,
                                IsClockwise = isStart
                            });
                        }
                        while (d < obj.DepthAll);
                        point = isStart ? tCurve0.StartPoint : tCurve0.EndPoint;
                        obj.ProcessActions.Add(new ProcessAction
                        {
                            Command = "Подъем",
                            Toolpath = new Line(point0, point),
                            Point = point,
                            Angle = isStart ? angleStart : angleEnd,
                            IsClockwise = isStart
                        });
                    }
                    else
                    {
                        s = ProcessOptions.ZSafety * tanAngle;
                        var sign = obj.Side == SideType.Left ^ obj.ObjectType == ObjectType.Arc ? -1 : 1;
                        var tCurve0 = GetDisplacementCopy(GetOffsetCopy(toolpathCurve, sign * (-s)), ProcessOptions.ZSafety);
                        var isStart = true;
                        var point0 = tCurve0.StartPoint;
                        Point3d point;
                        int d = 0;
                        if (obj.ObjectType == ObjectType.Arc)
                            d = -obj.Depth;
                        do
                        {
                            d += obj.Depth;
                            if (d > obj.DepthAll)
                                d = obj.DepthAll;
                            s = d * tanAngle;
                            var tCurve = GetDisplacementCopy(GetOffsetCopy(toolpathCurve, sign * s), -d);
                            point = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                            obj.ProcessActions.Add(new ProcessAction
                            {
                                Command = "Заглубление",
                                Toolpath = new Line(point0, point),
                                Point = point,
                                Angle = isStart ? angleStart : angleEnd,
                                IsClockwise = isStart
                            });
                            isStart = !isStart;
                            point0 = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                            obj.ProcessActions.Add(new ProcessAction
                            {
                                Command = "Рез",
                                Toolpath = tCurve,
                                Point = point0,
                                Angle = isStart ? angleStart : angleEnd,
                                IsClockwise = isStart
                            });
                        }
                        while (d < obj.DepthAll);
                        point = isStart ? tCurve0.StartPoint : tCurve0.EndPoint;
                        obj.ProcessActions.Add(new ProcessAction
                        {
                            Command = "Подъем",
                            Toolpath = new Line(point0, point),
                            Point = point,
                            Angle = isStart ? angleStart : angleEnd,
                            IsClockwise = isStart
                        });
                    }

                    obj.ProcessActions.ForEach(p =>
                    {
                        p.Toolpath.LayerId = layerId;
                        BlkTblRec.AppendEntity(p.Toolpath);
                        trans.AddNewlyCreatedDBObject(p.Toolpath, true);
                        p.Toolpath.Erased += new ObjectErasedEventHandler(ToolpathCurveErasedEventHandlerRavelli);
                    });
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        private void Temp()
        {
            double h = 190, R = 1100, D = 625, adeg = 11.479, step = 0.0001;
            var s = R - Math.Sqrt(R * R - h * (D - h));
            var alfa = Math.Atan2(s, h);
            var alfaDeg = alfa * 180 / Math.PI;
            var l1 = h / Math.Cos(alfa);
            var l2 = h;
            do
            {
                alfa += step;
                alfaDeg = alfa * 180 / Math.PI;
                l1 = h / Math.Cos(alfa);
                s = h * Math.Tan(alfa);
                var Dis = D * D - 4 * s * (2 * R - s);
                l2 = (D - Math.Sqrt(Dis)) / 2;
            }
            while (l2 < l1);
        }

        private void Намечание(ProcessObject obj, double s)
        {
            if (!(obj is ProcessObjectLine))
                return;
            double h;
            Point3d pointC;
            if (obj.IsBeginExactly && obj.IsEndExactly)
            {
                var l = obj.Length - 2 * ExactlyIncrease;
                h = (obj.Diameter - Math.Sqrt(obj.Diameter * obj.Diameter - l * l)) / 2;
                pointC = obj.ProcessCurve.GetPointAtParameter(obj.ProcessCurve.EndParam / 2);
            }
            else
            {
                h = obj.DepthAll;
                pointC = obj.ProcessCurve.StartPoint + obj.ProcessCurve.GetFirstDerivative(0).GetNormal() * (obj.IsBeginExactly ? s : obj.Length - s);
            }

            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
                    if (obj.ProcessActions != null)
                        obj.ProcessActions.ForEach(p => trans.GetObject(p.Toolpath.ObjectId, OpenMode.ForWrite).Erase());

                    obj.ProcessActions = new List<ProcessAction>();
                    var layerId = GetProcessLayer(trans);

                    var point0 = new Point3d(pointC.X, pointC.Y, ProcessOptions.ZSafety);
                    var point1 = new Point3d(pointC.X, pointC.Y, -h);

                    obj.ProcessActions.Add(new ProcessAction
                    {
                        Command = "Заглубление",
                        Toolpath = new Line(point0, point1),
                        Point = point1
                    });
                    obj.ProcessActions.Add(new ProcessAction
                    {
                        Command = "Подъем",
                        Toolpath = new Line(point1, point0),
                        Point = point0
                    });

                    obj.ProcessActions.ForEach(p =>
                    {
                        p.Toolpath.LayerId = layerId;
                        BlkTblRec.AppendEntity(p.Toolpath);
                        trans.AddNewlyCreatedDBObject(p.Toolpath, true);
                        p.Toolpath.Erased += new ObjectErasedEventHandler(ToolpathCurveErasedEventHandlerRavelli);
                    });
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        void ToolpathCurveErasedEventHandlerRavelli(object senderObj, EventArgs evtArgs)
        {
            ProcessObject obj = ObjectList.Find(p => p.ProcessActions.Any(t => t.Toolpath == ((Curve)senderObj)));
            if (obj != null)
                obj.ProcessActions.RemoveAll(t => t.Toolpath == ((Curve)senderObj));
        }

        double CalcAngle(ProcessObject obj, double angle)
        {
            var angleDeg = angle * 180 / Math.PI;
            return obj.TopEdge ^ obj.Side == SideType.Right ? (450 - angleDeg) % 360 : (630 - angleDeg) % 360;
        }

        double GetOffsetValueRavelli(ProcessObject obj)
        {
            bool hasOffset = obj.TopEdge;
            double d = 0;
            if (obj.ObjectType == ObjectType.Arc && obj.Side == SideType.Left)
            {
                obj.TopEdge = true;
                hasOffset = true;
                double R = obj.ProcessArc.Radius;
                d = R - Math.Sqrt(R * R - obj.DepthAll * (obj.Diameter - obj.DepthAll));
                (obj as ProcessObjectArc).Compensation = d;
                obj.VerticalAngleDeg = Math.Atan2(d, obj.DepthAll) * 180 / Math.PI;
            }
            double sign = obj.Side == SideType.Left ^ obj.ObjectType == ObjectType.Arc ? 1 : -1;

            return sign * ((hasOffset ? (obj.Thickness / Math.Cos(obj.VerticalAngle)) : 0) + d);
        }

        /// <summary>
        /// Получить копию кривой со смещением в плоскости XY
        /// </summary>
        /// <param name="curve">Копируемая кривая</param>
        /// <param name="offset">Смещение</param>
        /// <returns>Созданная копия</returns>
        public static Curve GetOffsetCopy(Curve curve, double offset)
        {
            var sign = 1; // (curve is Line) ? 1 : -1;
            return curve.GetOffsetCurves(offset * sign)[0] as Curve;
        }

        /// <summary>
        /// Получить копию кривой со смещением по Z
        /// </summary>
        /// <param name="curve">Копируемая кривая</param>
        /// <param name="displacement">Смещение</param>
        /// <returns>Созданная копия</returns>
        public static Curve GetDisplacementCopy(Curve curve, double displacement)
        {
            return curve.GetTransformedCopy(Matrix3d.Displacement(new Vector3d(0, 0, displacement))) as Curve;
        }

        private void ConstructPlaneToolpath(ProcessObject obj)
        {
            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
                    if (obj.ProcessActions != null)
                        obj.ProcessActions.ForEach(p => trans.GetObject(p.Toolpath.ObjectId, OpenMode.ForWrite).Erase());

                    obj.ProcessActions = new List<ProcessAction>();
                    var layerId = GetProcessLayer(trans);
                    var tanAngle = Math.Tan(obj.VerticalAngle);

                    double angleStart = 0;
                    double angleEnd = 0;
                    if (obj.ObjectType == ObjectType.Arc)
                    {
                        angleStart = CalcAngle(obj, (obj.ProcessCurve as Arc).StartAngle);
                        angleEnd = CalcAngle(obj, (obj.ProcessCurve as Arc).EndAngle);
                    }
                    var sign = obj.Side == SideType.Left ^ obj.ObjectType == ObjectType.Arc ? -1 : 1;
                    var tCurve0 = GetDisplacementCopy(obj.ProcessCurve, -ProcessOptions.Thickness);
                    var tCurve = GetOffsetCopy(tCurve0, -sign * obj.Depth);
                    var tCurve01 = tCurve;
                    var isStart = true;
                    var point = tCurve.StartPoint;
                    obj.ProcessActions.Add(new ProcessAction
                    {
                        Command = "Опускание",
                        Toolpath = new Line(new Point3d(point.X, point.Y, ProcessOptions.ZSafety), point),
                        Point = point,
                        Angle = isStart ? angleStart : angleEnd,
                    });
                    var point0 = point;
                    var d = 0;
                    do
                    {
                        d += obj.Depth;
                        if (d > obj.DepthAll)
                            d = obj.DepthAll;
                        tCurve = GetOffsetCopy(tCurve0, sign * d);
                        point = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                        obj.ProcessActions.Add(new ProcessAction
                        {
                            Command = "Заглубление",
                            Toolpath = new Line(point0, point),
                            Point = point,
                            Angle = isStart ? angleStart : angleEnd
                        });
                        isStart = !isStart;
                        point0 = isStart ? tCurve.StartPoint : tCurve.EndPoint;
                        obj.ProcessActions.Add(new ProcessAction
                        {
                            Command = "Рез",
                            Toolpath = tCurve,
                            Point = point0,
                            Angle = isStart ? angleStart : angleEnd,
                            IsClockwise = isStart
                        });
                    }
                    while (d < obj.DepthAll);
                    point = isStart ? tCurve01.StartPoint : tCurve01.EndPoint;
                    obj.ProcessActions.Add(new ProcessAction
                    {
                        Command = "Отвод",
                        Toolpath = new Line(point0, point),
                        Point = point,
                        Angle = isStart ? angleStart : angleEnd
                    });
                    obj.ProcessActions.Add(new ProcessAction
                    {
                        Command = "Подъем",
                        Toolpath = new Line(point, new Point3d(point.X, point.Y, ProcessOptions.ZSafety)),
                        Point = new Point3d(point.X, point.Y, ProcessOptions.ZSafety),
                        Angle = isStart ? angleStart : angleEnd
                    });

                    obj.ProcessActions.ForEach(p =>
                    {
                        p.Toolpath.LayerId = layerId;
                        BlkTblRec.AppendEntity(p.Toolpath);
                        trans.AddNewlyCreatedDBObject(p.Toolpath, true);
                        p.Toolpath.Erased += new ObjectErasedEventHandler(ToolpathCurveErasedEventHandlerRavelli);
                    });
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }
    }
}

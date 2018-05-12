using System;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
//using AcRx = Autodesk.AutoCAD.Runtime;
//using AcEd = Autodesk.AutoCAD.EditorInput;
using AcDb = Autodesk.AutoCAD.DatabaseServices;
//using AcAp = Autodesk.AutoCAD.ApplicationServices;
//using AcGe = Autodesk.AutoCAD.Geometry;
using AcCl = Autodesk.AutoCAD.Colors;

namespace ProcessingTechnologyCalc
{
    public partial class AcadProcessesView
    {
        //private AcAp.Application Acad = AcAp.Application.AcadApplication as AcAp.Application;  // TODO Application?
        private Database Database = HostApplicationServices.WorkingDatabase;
        private AcDb.TransactionManager TransactionManager = AcDb.HostApplicationServices.WorkingDatabase.TransactionManager;
        private Document Document = Application.DocumentManager.MdiActiveDocument;
        private Editor Editor = Application.DocumentManager.MdiActiveDocument.Editor;

        const double cPI = Math.PI;
        const double c2PI = Math.PI * 2;
        const double cPI2 = Math.PI / 2;
        const double ExactlyIncrease = 5;
        const double AngleTolerance = 0.000001;        

        ObjectId CurrentObjectID = ObjectId.Null;
        void CallbackSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            //string type = e.AddedObjects[0].ObjectId.Handle.ToString();
            //ACADProcessesView.getInstanse().m_SettingForm.textBox1.AppendText(String.Format("{0} cnt:{1} add:{2} sel:{3}\r\n", ++line, e.Selection.Count, e.AddedObjects.Count, type));

            if (e.AddedObjects.Count == 1)
            {
                if (e.AddedObjects.GetObjectIds()[0] != CurrentObjectID)
                {
                    CurrentObjectID = e.AddedObjects.GetObjectIds()[0];
                    ObjectForm.SelectObject(ObjectList.FindIndex(p => p.ProcessCurve.ObjectId == CurrentObjectID));
                }
            }
            else if (CurrentObjectID != ObjectId.Null)
            {
                ObjectForm.SelectObject(-1);
                CurrentObjectID = ObjectId.Null;
            }
        }

        public void SelectObject(ProcessObject obj)
        {
            Editor.SetImpliedSelection(new ObjectId[] { obj.ProcessCurve.ObjectId });
            Editor.UpdateScreen();
        }

        public void DeleteObject(ProcessObject obj)
        {
            // TODO удалить обработчики
            //obj.ProcessCurve.Modified -= ProcessCurveModifiedEventHandler;  
            //obj.ProcessCurve.Erased -= ProcessCurveErasedEventHandler;
            if (obj.ToolpathCurve != null)
            {
                using (DocumentLock doclock = Document.LockDocument())
                {
                    using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                    {
                        trans.GetObject(obj.ToolpathCurve.ObjectId, AcDb.OpenMode.ForWrite);
                        obj.ToolpathCurve.Erase();
                        trans.Commit();
                        Editor.UpdateScreen();
                    }
                }
            }
            RemoveConnect(obj, VertexType.Start);
            RemoveConnect(obj, VertexType.End);
            ObjectList.Remove(obj);
            ObjectForm.RefreshList();
        }

        public void DeleteAllObjects()
        {
            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    foreach( var obj in (ObjectList.FindAll(p => p.ToolpathCurve != null)))
                    {
                        trans.GetObject(obj.ToolpathCurve.ObjectId, AcDb.OpenMode.ForWrite);
                        obj.ToolpathCurve.Erase();
                    }
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
            ObjectList.Clear();
        }

        void AddSelectedObject(ProcessOptions processOptions)
        {
            List<ProcessObject> addList = new List<ProcessObject>();
            using (Transaction acTrans = TransactionManager.StartTransaction())
            {
                PromptSelectionResult sel = Editor.SelectPrevious(); //SelectImplied(); // 
                if (sel.Status == PromptStatus.OK)
                {
                    foreach (ObjectId objId in sel.Value.GetObjectIds())
                    {
                        if (!ObjectList.Exists(p => p.ProcessCurve.ObjectId == objId))
                        {
                            DBObject obj = acTrans.GetObject(objId, OpenMode.ForRead);
                            if ((((Entity)obj).Layer != "Обработка") && (((obj is Line) || (obj is Arc) || (obj is Polyline))))
                            {
                                obj.Modified += new EventHandler(ProcessCurveModifiedEventHandler);
                                obj.Erased += new ObjectErasedEventHandler(ProcessCurveErasedEventHandler);
                                ProcessObject processObject = null;
                                if (obj is Line)
                                {
                                    processObject = new ProcessObjectLine(obj as Curve, processOptions);
                                }
                                if (obj is Arc)
                                {
                                    processObject = new ProcessObjectArc(obj as Curve, processOptions);
                                }
                                if (obj is Polyline)
                                {
                                    processObject = new ProcessObjectPolyline(obj as Curve, processOptions);
                                }
                                ObjectList.Add(processObject);
                                addList.Add(processObject);
                                if (processObject.ConnectObject[VertexType.Start.Index()] == null)
                                {
                                    SetConnect(processObject, VertexType.Start);
                                }
                                if (processObject.ConnectObject[VertexType.End.Index()] == null)
                                {
                                    SetConnect(processObject, VertexType.End);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Editor.WriteMessage("Объекты для добавления не найдены");
                }
            }
            if (addList.Count > 0)
            {
                do
                {
                    var findObj = addList.Find(p => p.Side != SideType.None);
                    if (findObj != null)
                    {
                        StartCalcToolpath(findObj);
                    }
                    else
                    {
                        SetProcessSide(addList[0]);
                    }
                }
                while (addList.RemoveAll(p => p.ToolpathCurve != null) > 0 && addList.Count > 0);
            }
        }

        void ProcessCurveModifiedEventHandler(object senderObj, EventArgs evtArgs)  // TODO изменение для дуг
        {
            Curve curve = senderObj as Curve;
            ProcessObject obj = ObjectList.Find(p => p.ProcessCurve == curve);
            if (obj != null
                && (
                (float)curve.StartPoint.X != obj.StartPoint.X || (float)curve.StartPoint.Y != obj.StartPoint.Y ||
                (float)curve.EndPoint.X != obj.EndPoint.X || (float)curve.EndPoint.Y != obj.EndPoint.Y))
            {
                if ((float)curve.StartPoint.X != obj.StartPoint.X || (float)curve.StartPoint.Y != obj.StartPoint.Y)
                {
                    SetConnect(obj, VertexType.Start);
                }
                if ((float)curve.EndPoint.X != obj.EndPoint.X || (float)curve.EndPoint.Y != obj.EndPoint.Y)
                {
                    SetConnect(obj, VertexType.End);
                }
                obj.RefreshProcessCurveData();
                StartCalcToolpath(obj);
                ObjectForm.RefreshProperty();
                //Application.ShowAlertDialog("Modified");
            }
        }

        void ProcessCurveErasedEventHandler(object senderObj, EventArgs evtArgs)
        {
            //Application.ShowAlertDialog("Erased");
            ProcessObject obj = ObjectList.Find(p => p.ProcessCurve == senderObj as Curve);
            if (obj != null)
            {
                DeleteObject(obj);
            }
        }

        void ToolpathCurveErasedEventHandler(object senderObj, EventArgs evtArgs)
        {
            ProcessObject obj = ObjectList.Find(p => p.ToolpathCurve == senderObj as Curve);
            if (obj != null)
            {
                obj.ToolpathCurve = null;
                obj.Side = SideType.None;
            }
        }

        public void TurnProcessLayer()
        {
            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    ObjectId processLayerId = GetProcessLayer(trans);
                    LayerTableRecord layerTblRec = trans.GetObject(processLayerId, OpenMode.ForWrite) as LayerTableRecord;
                    layerTblRec.IsOff = !layerTblRec.IsOff;
                    trans.Commit();
                    Editor.UpdateScreen();
                    Editor.WriteMessage("\nСлой 'Обработка' " + (layerTblRec.IsOff ? "отключен\n" : "включен\n"));
                }
            }
        }

        ObjectId GetProcessLayer(Transaction trans)
        {
            ObjectId layerId;
            string layerName = "Обработка";

            LayerTable layerTbl = trans.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;

            if (layerTbl.Has(layerName))       // Проверяем нет ли еще слоя с таким именем в чертеже
            {
                layerId = layerTbl[layerName];
            }
            else   // создание нового слоя
            {
                layerTbl.UpgradeOpen();
                LayerTableRecord layer = new LayerTableRecord();
                layer.Name = layerName;
                layer.Color = AcCl.Color.FromColor(System.Drawing.Color.Red);

                LinetypeTable lineTypeTbl = ((LinetypeTable)(trans.GetObject(Database.LinetypeTableId, OpenMode.ForWrite)));
                ObjectId lineTypeID;
                string ltypeName = "Continuous";
                if (lineTypeTbl.Has(ltypeName))
                {
                    lineTypeID = lineTypeTbl[ltypeName];
                }
                else    // создания стиля линий
                {
                    LinetypeTableRecord lineType = new LinetypeTableRecord();
                    lineType.Name = ltypeName;
                    lineTypeTbl.Add(lineType);
                    trans.AddNewlyCreatedDBObject(lineType, true);
                    lineTypeID = lineType.ObjectId;
                }
                layer.LinetypeObjectId = lineTypeID;
                layer.IsPlottable = true;
                layerId = layerTbl.Add(layer);
                trans.AddNewlyCreatedDBObject(layer, true);
            }
            return layerId;
        }

        public void SetProcessSide(ProcessObject obj)
        {
            SideType side = InputProcessSide(obj);
            if (side != SideType.None && side != obj.Side)
            {
                obj.Side = side;
                StartCalcToolpath(obj);
            }
        }

        SideType InputProcessSide(ProcessObject obj)
        {
            // TODO попытка после нажатия кнопки переключитьсся на окно автокада - не получилось нихуйа

            //EditorUserInteraction edInt = ed.StartUserInteraction(Program.ps as System.Windows.Forms.Control); 
            //Program.ps.Visible = false;
            //Program.ed.UpdateScreen();
            //[System.Runtime.InteropServices.DllImport("user32.dll")]
            //static extern int SetActiveWindow(IntPtr hwnd);
            //SetActiveWindow(doc.Window.Handle);

            PromptAngleOptions promptOptions = new PromptAngleOptions("\nВыберите направление внешней нормали к объекту");
            //                AcEd.PromptPointOptions ptOpt = new AcEd.PromptPointOptions("\nВыберите направление внешней нормали к объекту");

            promptOptions.BasePoint = obj.ProcessCurve.GetPointAtDist(obj.Length / 2);
            promptOptions.UseBasePoint = true;
            promptOptions.UseDashedLine = true;

            PromptDoubleResult promptResult = Editor.GetAngle(promptOptions);
            //                AcEd.PromptPointResult resPt2 = ed.GetPoint(ptOpt);

            //Program.ps.Visible = true;
            //            edInt.End();
            if (promptResult.Status == PromptStatus.OK)
            {
                double angle = 0;
                switch (obj.ObjectType)
                {
                    case ObjectType.Line:
                        angle = obj.ProcessLine.Angle;
                        break;
                    case ObjectType.Arc:
                        angle = (obj.ProcessArc.StartAngle + obj.ProcessArc.TotalAngle / 2 + cPI2) % c2PI;
                        break;
                    case ObjectType.Polyline:
                        angle = obj.ProcessCurve.GetFirstDerivative(promptOptions.BasePoint).GetAngleTo(Vector3d.XAxis);
                        break;
                }
                return Math.Sin(promptResult.Value - angle) > 0 ? SideType.Left : SideType.Right;
            }
            else
            {
                return SideType.None;
            }
        }

        public void SetExactlyEnd(ProcessObject obj, VertexType vertex)
        {
            if (obj.Side != SideType.None)
            {
                ConstructToolpathObject(obj);

                if (vertex == VertexType.Start)
                {
                    CalcToolpath(obj.ConnectObject[VertexType.Start.Index()], obj.ConnectVertex[VertexType.Start.Index()]);
                }
                else
                {
                    CalcToolpath(obj.ConnectObject[VertexType.End.Index()], obj.ConnectVertex[VertexType.End.Index()]);
                }
            }
        }

        public void RecalcToolpath()
        {
            ObjectList.ForEach(StartCalcToolpath);
            Editor.WriteMessage("\nОбъекты траектории обработки перерисованы\n");
        }

        void StartCalcToolpath(ProcessObject obj)
        {
            if (obj.Side != SideType.None)
            {
                obj.IsExactly[VertexType.Start.Index()] = CalcExactlyEnd(obj, VertexType.Start);
                obj.IsExactly[VertexType.End.Index()]   = CalcExactlyEnd(obj, VertexType.End);

                ConstructToolpathObject(obj);

                CalcToolpath(obj.ConnectObject[VertexType.Start.Index()], obj.ConnectVertex[VertexType.Start.Index()]);
                CalcToolpath(obj.ConnectObject[VertexType.End.Index()], obj.ConnectVertex[VertexType.End.Index()]);
            }
        }

        void CalcToolpath(ProcessObject obj, VertexType vertex)
        {
            if (obj == null)
            {
                return;
            }
            ProcessObject srcObj = obj.ConnectObject[vertex.Index()];
            VertexType srcVertex = obj.ConnectVertex[vertex.Index()];
            SideType containedSide = vertex != srcVertex ? srcObj.Side : srcObj.Side.Opposite();

            if (obj.IsExactly[vertex.Index()] != srcObj.IsExactly[srcVertex.Index()] || obj.Side != containedSide || obj.ToolpathCurve == null)
            {
                obj.IsExactly[vertex.Index()] = srcObj.IsExactly[srcVertex.Index()];
                vertex = vertex.Opposite();

                if ((obj.Side != containedSide && obj.ConnectObject[vertex.Index()] != null) || obj.ToolpathCurve == null)
                {
                    obj.Side = containedSide;
                    obj.IsExactly[vertex.Index()] = CalcExactlyEnd(obj, vertex);
                }
                obj.Side = containedSide;
                ConstructToolpathObject(obj);

                CalcToolpath( obj.ConnectObject[vertex.Index()], obj.ConnectVertex[vertex.Index()]);
            }
        }

        bool CalcExactlyEnd(ProcessObject obj, VertexType vertex)
        {
            ProcessObject connectObject = obj.ConnectObject[vertex.Index()];
            VertexType connectVertex = obj.ConnectVertex[vertex.Index()];
            bool isLeftTurn, isLeftProcessSide, isNextStartPoint;
            double angle;
            bool isExactly = false;

            if (connectObject != null)
            {
                switch (obj.ObjectType)
                {
                    case ObjectType.Line:
                        switch (connectObject.ObjectType)
                        {
                            case ObjectType.Line:

                                angle = connectObject.ProcessLine.Angle - obj.ProcessLine.Angle;
                                if (Math.Abs(angle) > AngleTolerance)
                                {
                                    isLeftTurn = Math.Sin(angle) > 0;
                                    isLeftProcessSide = obj.Side == SideType.Left;
                                    isNextStartPoint = connectVertex == VertexType.Start;
                                    //bool isSameDirection = point == obj.ProcessEndPoint ? nextObj.ProcessStartPoint == point : nextObj.ProcessEndPoint == point;
                                    //bool isNextPosDir = point == obj.ProcessEndPoint;
                                    isExactly = isLeftTurn ^ isLeftProcessSide ^ isNextStartPoint;
                                }
                                break;

                            case ObjectType.Arc:

                                double angleTan = connectVertex == VertexType.Start ? connectObject.ProcessArc.StartAngle + cPI2 : connectObject.ProcessArc.EndAngle - cPI2;
                                angle = angleTan - obj.ProcessLine.Angle;
                                if (Math.Abs(angle) > AngleTolerance)
                                {
                                    isLeftTurn = Math.Sin(angle) > 0;
                                }
                                else
                                {
                                    isLeftTurn = connectVertex == VertexType.Start;
                                }
                                bool isRightProcessSide = obj.Side == SideType.Right;
                                isExactly = isLeftTurn ^ isRightProcessSide;
                                break;

                            case ObjectType.Polyline:   // TODO концы полилиний
                                break;
                        }
                        break;

                    case ObjectType.Arc:   // TODO концы дуг
                        if (connectObject.ObjectType == ObjectType.Line)
                        {
                            connectObject.Side = vertex != connectVertex ? obj.Side : obj.Side.Opposite();
                            isExactly = CalcExactlyEnd(connectObject, connectVertex);
                        }
                        break;

                    case ObjectType.Polyline:   // TODO концы полилиний
                        break;
                }
            }
            return isExactly;
        }

        void ConstructToolpathObject(ProcessObject obj)
        {
            if (obj.Side == SideType.None)
            {
                return;
            }
            if (obj.ObjectType == ObjectType.Arc && (
                (obj.ProcessArc.StartAngle < cPI2 && obj.ProcessArc.EndAngle > cPI2) ||
                (obj.ProcessArc.StartAngle < cPI + cPI2 && (obj.ProcessArc.EndAngle > cPI + cPI2 || obj.ProcessArc.EndAngle < obj.ProcessArc.StartAngle))))
            {
                Application.ShowAlertDialog($"Обработка дуги {obj} невозможна - дуга пересекает угол 90 или 270 градусов. Текущие углы: начальный {180 / cPI * obj.ProcessArc.StartAngle}, конечный {180 / cPI * obj.ProcessArc.EndAngle}");
                return;
            }
            double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll)) + ExactlyIncrease;
            if ((obj.IsBeginExactly || obj.IsEndExactly) && (obj.Length / 2 <= s))
            {
                Application.ShowAlertDialog("Обработка объекта " + obj.ToString() + " невозможна вследствие слишком малой длины");
                return;
            }
            Curve toolpathCurve = obj.ProcessCurve.GetOffsetCurves(GetOffsetValue(obj))[0] as Curve;  // TODO расчет OffsetCurves + ModifiedEventHandler

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
                    if (obj.ToolpathCurve == null)  // TODO удаление объекта
                    {
                        BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                        BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

                        toolpathCurve.LayerId = GetProcessLayer(trans);
                        BlkTblRec.AppendEntity(toolpathCurve);
                        trans.AddNewlyCreatedDBObject(toolpathCurve, true);
                        toolpathCurve.Erased += new ObjectErasedEventHandler(ToolpathCurveErasedEventHandler);
                        obj.ToolpathCurve = toolpathCurve;
                    }
                    else
                    {
                        trans.GetObject(obj.ToolpathCurve.ObjectId, AcDb.OpenMode.ForWrite);
                        switch (obj.ObjectType)
                        {                                   // TODO копия объекта
                            case ObjectType.Line:
                                obj.ToolpathCurve.StartPoint = toolpathCurve.StartPoint;
                                obj.ToolpathCurve.EndPoint = toolpathCurve.EndPoint;
                                break;
                            case ObjectType.Arc:
                                (obj.ToolpathCurve as Arc).Center = (toolpathCurve as Arc).Center;
                                (obj.ToolpathCurve as Arc).Radius = (toolpathCurve as Arc).Radius;
                                (obj.ToolpathCurve as Arc).StartAngle = (toolpathCurve as Arc).StartAngle;
                                (obj.ToolpathCurve as Arc).EndAngle = (toolpathCurve as Arc).EndAngle;
                                break;
                        }
                    }
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        double GetOffsetValue(ProcessObject obj)
        {
            bool hasOffset = false;
            double d = 0;
            switch (obj.ObjectType)
            {
                case ObjectType.Line:
                    hasOffset = ((obj as ProcessObjectLine).AngleRound > 0 && (obj as ProcessObjectLine).AngleRound <= cPI) ^ (obj.Side == SideType.Left);
                    break;

                case ObjectType.Arc:
                    hasOffset = (obj.ProcessArc.StartAngle >= cPI2 && obj.ProcessArc.StartAngle < cPI + cPI2) ^ (obj.Side == SideType.Right);
                    if (obj.Side == SideType.Left)
                    {
                        double R = obj.ProcessArc.Radius;
                        d = R - Math.Sqrt(R * R - obj.DepthAll * (obj.Diameter - obj.DepthAll));
                        (obj as ProcessObjectArc).Compensation = d;
                        (obj as ProcessObjectArc).BetaGrad = Math.Atan2(d, obj.DepthAll)*180/Math.PI;
                    }
                    break;

                case ObjectType.Polyline:
                    hasOffset = true;  // TODO GetOffsetValue
                    break;
            }
            double sign = obj.Side == SideType.Left  ^ obj.ObjectType == ObjectType.Arc ? 1 : -1;

            return sign * ((hasOffset ? obj.Thickness : 0) + d);
        }

        void SetConnect(ProcessObject obj, VertexType vertex)
        {
            RemoveConnect(obj, vertex);
            Point3d point = vertex == VertexType.Start ? obj.ProcessCurve.StartPoint : obj.ProcessCurve.EndPoint;
            List<ProcessObject> connectList = ObjectList.FindAll(p => p != obj && (p.ProcessCurve.StartPoint == point || p.ProcessCurve.EndPoint == point));
            if (connectList.Count == 1)  // TODO поиск первого
            {
                ProcessObject connectObject = connectList[0];
                VertexType connectVertex = point == connectObject.ProcessCurve.StartPoint ? VertexType.Start : VertexType.End;
                obj.ConnectObject[vertex.Index()] = connectObject;
                obj.ConnectVertex[vertex.Index()] = connectVertex;
                connectObject.ConnectObject[connectVertex.Index()] = obj;
                connectObject.ConnectVertex[connectVertex.Index()] = vertex;
                if (connectObject.Side != SideType.None)
                {
                    obj.Side = vertex != connectVertex ? connectObject.Side : connectObject.Side.Opposite();
                }
            }
        }

        void RemoveConnect(ProcessObject obj, VertexType vertex)
        {
            if (obj.ConnectObject[vertex.Index()] != null)
            {
                ProcessObject connectObject = obj.ConnectObject[vertex.Index()];
                VertexType connectVertex = obj.ConnectVertex[vertex.Index()];
                connectObject.ConnectObject[connectVertex.Index()] = null;
                connectObject.IsExactly[connectVertex.Index()] = false;
                ConstructToolpathObject(connectObject);
                obj.ConnectObject[vertex.Index()] = null;
            }
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("pline")]
        public void Polyline()
        {
            using (DocumentLock doclock = Document.LockDocument())
            {
                using (AcDb.Transaction trans = TransactionManager.StartTransaction())
                {
                    PromptSelectionResult sel = Editor.SelectPrevious(); //SelectImplied(); // 
                    Curve ProcessCurve = trans.GetObject(sel.Value[0].ObjectId, OpenMode.ForRead) as Curve;
                    Editor.WriteMessage(ProcessCurve.ToString());

                    Curve toolpathCurve = ProcessCurve.GetOffsetCurves(50)[0] as Curve;  // TODO расчет OffsetCurves + ModifiedEventHandler

                        BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                        BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

                        toolpathCurve.LayerId = GetProcessLayer(trans);
                        BlkTblRec.AppendEntity(toolpathCurve);
                        trans.AddNewlyCreatedDBObject(toolpathCurve, true);
                    trans.Commit();
                    Editor.UpdateScreen();
                }
            }
        }

        // TODO точка пересечения траекторий
        //private void GetIntersect(ProcessedObject curObj)
        //{
           // Line2d processCcurve = new Line2d(curObj.ProcessAcadObject.StartPoint, curObj.ProcessAcadObject.EndPoint);
           // CurveCurveIntersector2d( processCcurve, curve2);
        //}

        // Define Command "MoveEnt"
        //[CommandMethod("MoveEnt")]
        //static public void Move()
        //{
        //  AcDb.Database db = AcDb.HostApplicationServices.WorkingDatabase;
        //  AcDb.TransactionManager trm = db.TransactionManager;
        //  AcEd.Editor ed = AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
        //  AcEd.PromptSelectionResult resSel = ed.GetSelection();
        //  if (resSel.Status == AcEd.PromptStatus.OK) {
        //    AcEd.SelectionSet ids = resSel.Value;
        //    AcEd.PromptPointResult resPt1 = ed.GetPoint("\nPick from point: ");
        //    if (resPt1.Status == AcEd.PromptStatus.OK) {
        //      AcEd.PromptPointOptions ptOpt = new AcEd.PromptPointOptions("\nPick to point: ");
        //      ptOpt.UseBasePoint = true; ptOpt.BasePoint = resPt1.Value;
        //      AcEd.PromptPointResult resPt2 = ed.GetPoint(ptOpt);
        //      if (resPt2.Status == AcEd.PromptStatus.OK)
        //      {
        //          AcGe.Point3d from = resPt1.Value;
        //          AcGe.Point3d to = resPt2.Value;
        //          //AcGe.Point3d from = UcsToWcs(resPt1.Value);
        //          //AcGe.Point3d to = UcsToWcs(resPt2.Value);
        //          AcGe.Vector3d vec = to - from;
        //        AcGe.Matrix3d mat = AcGe.Matrix3d.Displacement(vec);
        //        using (AcDb.Transaction tr = trm.StartTransaction()) {
        //          foreach (AcDb.ObjectId id in ids.GetObjectIds()) {
        //            AcDb.Entity en = tr.GetObject(id,AcDb.OpenMode.ForWrite) as AcDb.Entity;
        //            if (en != null) {
        //              en.TransformBy(mat);
        //            }
        //          }
        //          tr.Commit();
        //        }
        //      }
        //    }
        //  }
        //}

        //public static bool IsPaperSpace(Database db)
        //{
        //  if (db.TileMode) return false;
        //  AcEd.Editor ed = AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
        //  if (db.PaperSpaceVportId == ed.CurrentViewportObjectId) return true;
        //  return false;
        //}
        //public static Matrix3d  GetUcsMatrix(Database db)
        //{
        //  AcGe.Point3d origin;
        //  AcGe.Vector3d xAxis, yAxis, zAxis;
        //  if (IsPaperSpace(db)) {
        //    origin = db.Pucsorg; xAxis  = db.Pucsxdir; yAxis  = db.Pucsydir;
        //  } else {
        //    origin = db.Ucsorg;  xAxis  = db.Ucsxdir;  yAxis  = db.Ucsydir;
        //  }
        //  zAxis = xAxis.CrossProduct(yAxis);
        //  return AcGe.Matrix3d.AlignCoordinateSystem(
        //    AcGe.Point3d.Origin, AcGe.Vector3d.XAxis, AcGe.Vector3d.YAxis, AcGe.Vector3d.ZAxis,
        //    origin, xAxis, yAxis, zAxis);
        //}
        //public static AcGe.Point3d UcsToWcs(AcGe.Point3d pt)
        //{
        //  AcGe.Matrix3d m = GetUcsMatrix(AcDb.HostApplicationServices.WorkingDatabase);
        //  return pt.TransformBy(m);
        //}

        // TODO edit the summary info
        // Uses builder to edit the summary info (file properties) of the drawing
        //DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(Database.SummaryInfo);
        //infoBuilder.CustomPropertyTable.Add("key", "value");
        //Database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        //Database.SummaryInfo.CustomProperties.Reset();
        //while (Database.SummaryInfo.CustomProperties.MoveNext())
        //{
        //    MessageBox.Show(Database.SummaryInfo.CustomProperties.Entry.ToString());
        //}
        /*
В меню "по умолчанию" получается добавить свои строки:
Application.AddDefaultContextMenuExtension(myMenu)
В меню "объекта" не получается:
Dim rxc as RXClass=Entity.GetClass(GetType(ENTITY)) 'ENTITY = Polyline, BlockReference .. etc
Application.AddObjectContextMenuExtension(rxc, myMenu)      
   
Для того, чтобы при загрузке сборки выполнился некий код, необходимо создать класс определяющий 
         * интерфейс IExtensionApplication. Метод Initialize() будет вызван при загрузке сборки. 
         * Пример смотри здесь: http://through-the-interface.typepad....tion_.html 
Только вот если твоя сборка автозагружается вместе с AutoCAD далеко не все можно делать в момент загрузки
         * , т.к. ядро AutoCAD еще загружено не полностью и не полностью создан/загружен документ (dwg-файл). 
         * Так что с этим нужно очень осторожно. Можно запустить таймер (на ObjectARX я такое делал, на .NET не пробовал)
         * , который через некоторый интервал времени запустит определенную в твоей сборке команду 
         * (через Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("CL",true,true,false)         
          
        */

    }
}

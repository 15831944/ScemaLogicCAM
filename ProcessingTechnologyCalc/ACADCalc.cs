using System;
using System.Collections.Generic;
//using System.Windows.Forms;
using System.Drawing;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcRx = Autodesk.AutoCAD.Runtime;
using AcEd = Autodesk.AutoCAD.EditorInput;
using AcDb = Autodesk.AutoCAD.DatabaseServices;
using AcAp = Autodesk.AutoCAD.ApplicationServices;
using AcGe = Autodesk.AutoCAD.Geometry;
using AcCl = Autodesk.AutoCAD.Colors;

namespace ProcessingTechnologyCalc
{
    public class AcadForm : IDrawForm
    {
        private IObjectForm ObjectForm;
        private List<ProcessObject> ObjectList;

        //private AcAp.Application Acad = AcAp.Application.AcadApplication as AcAp.Application;  // TODO Application?
        private AcDb.Database Database = AcDb.HostApplicationServices.WorkingDatabase;
        private AcDb.TransactionManager TransactionManager = AcDb.HostApplicationServices.WorkingDatabase.TransactionManager;
        private AcAp.Document Document = AcAp.Application.DocumentManager.MdiActiveDocument;
        private AcEd.Editor Editor = AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

        private const double cPI = Math.PI;
        private const double c2PI = Math.PI * 2;
        private const double cPI2 = Math.PI / 2;
        private const double ExactlyIncrease = 3;
        private const double AngleTolerance = 0.001;        

        public AcadForm(List<ProcessObject> list)
        {
            ObjectList = list;
            Editor.SelectionAdded += new SelectionAddedEventHandler(CallbackSelectionAdded);
        }

        public void SetObjectForm (IObjectForm objectForm)
        {
            ObjectForm = objectForm;
        }

        private ObjectId CurrentObjectID = ObjectId.Null;
        private void CallbackSelectionAdded(object sender, SelectionAddedEventArgs e)
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
            RemoveConnect(obj, VertexType.Start);
            RemoveConnect(obj, VertexType.End);
            ObjectList.Remove(obj);
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
                obj.ToolpathCurve = null;
            }
        }

        public void AddSelectedObject(ProcessOptions processOptions)
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

                            if ((((Entity)obj).Layer != "Обработка") && (((obj is Line) || (obj is Arc))))
                            {
                                obj.Modified += new EventHandler(ProcessCurveModifiedEventHandler);
                                obj.Erased += new ObjectErasedEventHandler(ProcessCurveErasedEventHandler);
                                ProcessObject processObject;
                                if (obj is Line)
                                {
                                    processObject = new ProcessObjectLine(obj as Curve, processOptions);
                                }
                                else
                                {
                                    processObject = new ProcessObjectArc(obj as Curve, processOptions);
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
            do
            {
                var findObj = addList.Find(p => p.Side != SideType.None);
                if (findObj != null)
                {
                    CalcToolpath(findObj);
                }
                else
                {
                    SetProcessSide(addList[0]);
                }
            }
            while (addList.RemoveAll(p => p.ToolpathCurve != null) > 0 && addList.Count > 0);
        }

        public void ProcessCurveModifiedEventHandler(object senderObj, EventArgs evtArgs)  // TODO изменение для дуг
        {
            Curve curve = senderObj as Curve;
            ProcessObject obj = ObjectList.Find(p => p.ProcessCurve == curve);
            if ((float)curve.StartPoint.X != obj.StartPoint.X || (float)curve.StartPoint.Y != obj.StartPoint.Y ||
                (float)curve.EndPoint.X != obj.EndPoint.X || (float)curve.EndPoint.Y != obj.EndPoint.Y)
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
                CalcToolpath(obj);        
                //Application.ShowAlertDialog(((Curve)senderObj).StartPoint.ToString());
            }
        }

        public void ProcessCurveErasedEventHandler(object senderObj, EventArgs evtArgs)
        {
            ProcessObject obj = ObjectList.Find(p => p.ProcessCurve == senderObj as Curve);
            DeleteObject(obj);
            ObjectForm.RefreshList();
        }

        public void ToolpathCurveErasedEventHandler(object senderObj, EventArgs evtArgs)
        {
            ProcessObject obj = ObjectList.Find(p => p.ToolpathCurve == senderObj as Curve);
            obj.ToolpathCurve = null;
            obj.Side = SideType.None;
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

        }

        private ObjectId GetProcessLayer(Transaction trans)
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
                CalcToolpath(obj);
            }
        }

        private SideType InputProcessSide(ProcessObject obj)
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
                }
                return Math.Sin(promptResult.Value - angle) > 0 ? SideType.Left : SideType.Right;
            }
            else
            {
                return SideType.None;
            }
        }

        //private void CalcToolpath()
        //{
        //    var list = ObjectList.FindAll(p => p.Side == SideType.None);

        //    while (list.Count > 0)
        //    {
        //        string message = String.Empty;

        //        foreach (var obj in list)
        //        {
        //            SideType sideStart = SideType.None;
        //            SideType sideEnd = SideType.None;

        //            var findList = ObjectList.FindAll(p => p.Side != SideType.None && (p.StartPoint == obj.StartPoint || p.EndPoint == obj.StartPoint));

        //            if (findList.Count > 1)
        //            {
        //                //message += "\nОбработка объекта \"" + obj.ToString() + "\" корректно не оперделена";
        //                continue;
        //            }
        //            if (findList.Count == 1)
        //            {
        //                sideStart = findList[0].EndPoint == obj.StartPoint ? findList[0].Side : findList[0].Side == SideType.Left ? SideType.Right : SideType.Left;
        //            }

        //            findList = ObjectList.FindAll(p => p.Side != SideType.None && (p.StartPoint == obj.EndPoint || p.EndPoint == obj.EndPoint));

        //            if (findList.Count > 1)
        //            {
        //                //message += "\nОбработка объекта \"" + obj.ToString() + "\" корректно не оперделена";
        //                continue;
        //            }
        //            if (findList.Count == 1)
        //            {
        //                sideEnd = findList[0].StartPoint == obj.EndPoint ? findList[0].Side : findList[0].Side == SideType.Left ? SideType.Right : SideType.Left;
        //            }
        //            if (sideStart == SideType.None && sideEnd == SideType.None)
        //            {
        //                //message += "\nОбработка объекта \"" + obj.ToString() + "\" не оперделена";
        //                continue;
        //            }
        //            if (sideStart != SideType.None && sideEnd != SideType.None && sideStart != sideEnd)
        //            {
        //                //message += "\nОбработка объекта \"" + obj.ToString() + "\" корректно не оперделена";
        //                continue;
        //            }

        //            obj.Side = sideStart != SideType.None ? sideStart : sideEnd;
        //            ConstructToolpathObject(obj);
        //        }
        //        if (list.RemoveAll(p => p.Side != SideType.None) == 0)
        //        {
        //            //Editor.WriteMessage(message);  
        //            // TODO комментарии к объекту
        //            break;
        //        }
        //    }
        //}

        public void SetExactlyEnd(ProcessObject obj, VertexType vertex)
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

        private void CalcToolpath(ProcessObject obj)
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

        private void CalcToolpath(ProcessObject obj, VertexType vertex)
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

        private void ConstructToolpathObject(ProcessObject obj)
        {
            if (obj.Side == SideType.None)
            {
                return;
            }
            bool hasOffset = false;
            switch (obj.ObjectType)
            {
                case ObjectType.Line:
                    hasOffset = (obj.ProcessLine.Angle > 0 && obj.ProcessLine.Angle <= cPI) ^ (obj.Side == SideType.Left);
                    break;
                case ObjectType.Arc:
                    hasOffset = (obj.ProcessArc.StartAngle >= cPI2 && obj.ProcessArc.StartAngle < cPI + cPI2) ^ (obj.Side == SideType.Right);
                    break;
            }
            double offset = obj.ToolpathOffsetSign * ((hasOffset ? obj.Thickness : 0) + CalcOffsetInnerSideArc(obj));
            Curve toolpathCurve = obj.ProcessCurve.GetOffsetCurves(offset)[0] as Curve;  // TODO расчет OffsetCurves
            double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll)) + ExactlyIncrease;

            switch (obj.ObjectType)
            {
                case ObjectType.Line:

                    //obj.IsBeginExactly = CalcExactlyEnd(obj, obj.StartPoint);
                    if (obj.IsBeginExactly)
                    {
                        if (obj.Length <= s)
                        {
                            Application.ShowAlertDialog("Обработка объекта " + obj.ToString() + " невозможна вследствие слишком малой длины");
                            DeleteObject(obj);
                            return;
                        }
                        toolpathCurve.StartPoint = toolpathCurve.GetPointAtDist(s);
                    }
                    //obj.IsEndExactly = CalcExactlyEnd(obj, obj.EndPoint);
                    if (obj.IsEndExactly)
                    {
                        if (obj.Length <= s)
                        {
                            Application.ShowAlertDialog("Обработка объекта " + obj.ToString() + " невозможна вследствие слишком малой длины");
                            DeleteObject(obj);
                            return;
                        }
                        toolpathCurve.EndPoint = toolpathCurve.GetPointAtDist((toolpathCurve as Line).Length - s);
                    }
                    if (obj.IsBeginExactly && obj.IsEndExactly && obj.Length / 2 <= s)
                    {
                        Application.ShowAlertDialog("Обработка объекта " + obj.ToString() + " невозможна вследствие слишком малой длины");
                        DeleteObject(obj);
                        return;
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
                        {
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

        private bool CalcExactlyEnd(ProcessObject obj, VertexType vertex)
        {
            ProcessObject connectObject = obj.ConnectObject[vertex.Index()];
            VertexType connectVertex = obj.ConnectVertex[vertex.Index()];
            bool isLeftTurn, isLeftProcessSide, isNextStartPoint;
            double angle;
            bool isExactly = false;

            if (connectObject != null)
            {
                if (obj.ObjectType == ObjectType.Arc)   // TODO концы дуг
                {
                    if (connectObject.ObjectType == ObjectType.Line)
                    {
                        connectObject.Side = vertex != connectVertex ? obj.Side : obj.Side.Opposite();
                        isExactly = CalcExactlyEnd(connectObject, connectVertex);
                    }
                }
                else
                {
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
                    }
                }
            }
            return isExactly;
        }

        private double CalcOffsetInnerSideArc(ProcessObject obj)
        {
            if (obj.ObjectType == ObjectType.Arc && obj.Side == SideType.Left)
            {
                double R = obj.ProcessArc.Radius;
                double s = R - Math.Sqrt(R * R - obj.DepthAll * (obj.Diameter - obj.DepthAll));
                //Editor.WriteMessage("\n" + obj.ToString() + " s=" + s + "\n");
                return s;
            }
            else
            {
                return 0;
            }
        }

        private void SetConnect(ProcessObject obj, VertexType vertex)
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

        private void RemoveConnect(ProcessObject obj, VertexType vertex)
        {
            if (obj.ConnectObject[vertex.Index()] != null)
            {
                ProcessObject connectObject = obj.ConnectObject[vertex.Index()];
                VertexType connectVertex = obj.ConnectVertex[vertex.Index()];
                connectObject.ConnectObject[connectVertex.Index()] = null;
                connectObject.IsExactly[connectVertex.Index()] = false;    // TODO обновить PropertyGrid
                ConstructToolpathObject(connectObject);
                obj.ConnectObject[vertex.Index()] = null;
            }
        }

        private bool SetConnect(ProcessObject obj, PointF point, out ProcessObject nextObj, out bool isSameDir)    // TODO автоматом след конец
        {
            bool isExectly = false;
            nextObj = null;
            isSameDir = false;

            List<ProcessObject> findList = ObjectList.FindAll(p => p != obj && (p.StartPoint == point || p.EndPoint == point));

            if (findList.Count == 1)  // TODO поиск первого
            {
                nextObj = findList[0];
                isSameDir = point == obj.EndPoint ? nextObj.StartPoint == point : nextObj.EndPoint == point;

                if (nextObj.Side != SideType.None)
                {
                    if (obj.Side == SideType.None)
                    {
                        obj.Side = isSameDir ? nextObj.Side : (nextObj.Side == SideType.Left ? SideType.Right : SideType.Left);
                    }
                    else
                    {
                        if (obj.Side != nextObj.Side)
                        {
                            Application.ShowAlertDialog("Ошибка! Смежные объекты " + obj.ToString() + " и " + nextObj.ToString() + 
                                " имеют разные стороны обработки!");
                        }
                    }
                }

                if (obj.Side != SideType.None && obj.ObjectType == ObjectType.Line) // TODO расчет концов для дуг
                {
                    bool isLeftTurn, isLeftProcessSide, isNextStartPoint;
                    double angle;

                    switch (nextObj.ObjectType)
                    {
                        case ObjectType.Line:

                            angle = nextObj.ProcessLine.Angle - obj.ProcessLine.Angle;
                            if (Math.Abs(angle) > AngleTolerance)
                            {
                                isLeftTurn = Math.Sin(angle) > 0;
                                isLeftProcessSide = obj.Side == SideType.Left;
                                isNextStartPoint = nextObj.StartPoint == point;
                                //bool isSameDirection = point == obj.ProcessEndPoint ? nextObj.ProcessStartPoint == point : nextObj.ProcessEndPoint == point;
                                //bool isNextPosDir = point == obj.ProcessEndPoint;
                                isExectly = isLeftTurn ^ isLeftProcessSide ^ isNextStartPoint;
                            }
                            break;

                        case ObjectType.Arc:

                            double angleTan = nextObj.StartPoint == point ? nextObj.ProcessArc.StartAngle + cPI2 : nextObj.ProcessArc.EndAngle - cPI2;
                            angle = angleTan - obj.ProcessLine.Angle;
                            if (Math.Abs(angle) > AngleTolerance)
                            {
                                isLeftTurn = Math.Sin(angle) > 0;
                            }
                            else
                            {
                                isLeftTurn = nextObj.StartPoint == point;
                            }
                            bool isRightProcessSide = obj.Side == SideType.Right;
                            isExectly = isLeftTurn ^ isRightProcessSide;
                            break;
                    }
                }
            }
            return isExectly;
        }

        // TODO точка пересечения траекторий
        //private void GetIntersect(ProcessedObject curObj)
        //{
           // Line2d processCcurve = new Line2d(curObj.ProcessAcadObject.StartPoint, curObj.ProcessAcadObject.EndPoint);
           // CurveCurveIntersector2d( processCcurve, curve2);
        //}

        //public void SetProcessSideObjects(ProcessObject obj)
        //{
        //    SideType side = InputProcessSide(obj);
        //    if (side != SideType.None)
        //    {
        //        bool isClosed = CalcProcessSideObjects(obj, obj.EndPoint, side);
        //        if (!isClosed)
        //        {
        //            CalcProcessSideObjects(obj, obj.StartPoint, side == SideType.Left ? SideType.Right : SideType.Left);
        //        }
        //        ToolpathCalc();
        //    }
        //}

        //private bool CalcProcessSideObjects(ProcessObject startObj, PointF curPoint, SideType side)
        //{
        //    ProcessObject curObj = startObj;
        //    List<ProcessObject> findList;
        //    do
        //    {
        //        if (curObj.ObjectType == ObjectType.Arc && (curObj.ProcessArc.TotalAngle > Math.PI ||
        //            Math.Sign(Math.Cos(curObj.ProcessArc.StartAngle)) != Math.Sign(Math.Cos(curObj.ProcessArc.EndAngle))))
        //        {
        //            //MessageBox.Show("Обработка объекта \"" + curObj.ToString() + "\" невозможна");
        //            curObj.Side = SideType.None;
        //        }
        //        curObj.HasProcessSide = false;  // TODO НАХ

        //        curObj.SetSide(side, curPoint != curObj.EndPoint);

        //        findList =
        //            ObjectList.FindAll(p => p != curObj && (p.StartPoint == curPoint || p.EndPoint == curPoint));

        //        if (findList.Exists(p => p == startObj))
        //        {
        //            return true;
        //        }
        //        if (findList.Count == 1)
        //        {
        //            curObj = findList[0];
        //            curPoint = curPoint == curObj.StartPoint ? curObj.EndPoint : curObj.StartPoint;
        //        }

        //    } while (findList.Count == 1);

        //    return false;
        //}

        //private void ToolpathCalc()
        //{
        //    using (DocumentLock doclock = Document.LockDocument())
        //    {
        //        using (AcDb.Transaction trans = TransactionManager.StartTransaction())
        //        {
        //            ObjectId processLayerId = GetProcessLayer(trans);

        //            BlockTable BlkTbl = trans.GetObject(Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
        //            BlockTableRecord BlkTblRec = trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
        //            Curve toolpathCurve;

        //            foreach (ProcessObject obj in ObjectList)
        //            {
        //                //ed.WriteMessage("\n" + obj.ToString() + ':' + obj.ProcessSide.ToString());
        //                if (!obj.HasProcessSide && obj.Side != SideType.None)
        //                {
        //                    if (obj.ToolpathCurve != null)
        //                    {
        //                        toolpathCurve = trans.GetObject(obj.ToolpathCurve.ObjectId, AcDb.OpenMode.ForWrite) as Curve;
        //                        toolpathCurve.Erase(true);
        //                    }
        //                    bool hasOffset;
        //                    if (obj.ProcessCurve is Line)
        //                    {
        //                        hasOffset = (obj.ProcessLine.Angle > 0 && obj.ProcessLine.Angle <= cPI) ^ (obj.Side == SideType.Left);
        //                    }
        //                    else
        //                    {
        //                        hasOffset = (obj.ProcessArc.StartAngle >= cPI2 && obj.ProcessArc.StartAngle < cPI + cPI2) ^ (obj.Side == SideType.Right);
        //                    }
        //                    double offset = obj.ToolpathOffsetSign * ((hasOffset ? obj.Thickness : 0) + CalcOffsetInnerSideArc(obj));
        //                    toolpathCurve = obj.ProcessCurve.GetOffsetCurves(offset)[0] as Curve;
        //                    toolpathCurve.LayerId = processLayerId;

        //                    if (obj.ProcessCurve is Line)
        //                    {
        //                        SetEndsNew(obj, toolpathCurve, obj.EndPoint);
        //                        SetEndsNew(obj, toolpathCurve, obj.StartPoint);
        //                    }
        //                    else
        //                    {
        //                        double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll));
        //                        if (obj.IsBeginExactly)
        //                        {
        //                            (toolpathCurve as Arc).StartAngle = obj.ProcessArc.StartAngle + s / obj.ProcessArc.Radius;
        //                        }
        //                        if (obj.IsEndExactly)
        //                        {
        //                            (toolpathCurve as Arc).EndAngle = obj.ProcessArc.EndAngle - s / obj.ProcessArc.Radius;
        //                        }
        //                    }
        //                    BlkTblRec.AppendEntity(toolpathCurve);
        //                    trans.AddNewlyCreatedDBObject(toolpathCurve, true);
        //                    obj.ToolpathCurve = toolpathCurve;
        //                    obj.HasProcessSide = true;
        //                }
        //            }
        //            trans.Commit();
        //            Editor.UpdateScreen();
        //        }
        //    }
        //}

        //private void SetEndsNew(ProcessObject obj, Curve toolpathCurve, PointF point)    // TODO автоматом след конец
        //{
        //    bool isExactly = false;
        //    List<ProcessObject> findList = ObjectList.FindAll(p => p != obj && (p.StartPoint == point || p.EndPoint == point));

        //    if (findList.Count == 1)  // TODO поиск первого
        //    {
        //        ProcessObject nextObj = findList[0];
        //        bool isLeftTurn, isLeftProcessSide, isNextStartPoint;
        //        double angle;

        //        switch (nextObj.ObjectType)
        //        {
        //            case ObjectType.Line:

        //                angle = nextObj.ProcessLine.Angle - obj.ProcessLine.Angle;
        //                if (Math.Abs(angle) > AngleTolerance)
        //                {
        //                    isLeftTurn = Math.Sin(angle) > 0;
        //                    isLeftProcessSide = obj.Side == SideType.Left;
        //                    isNextStartPoint = nextObj.StartPoint == point;
        //                    //bool isSameDirection = point == obj.ProcessEndPoint ? nextObj.ProcessStartPoint == point : nextObj.ProcessEndPoint == point;
        //                    //bool isNextPosDir = point == obj.ProcessEndPoint;
        //                    isExactly = isLeftTurn ^ isLeftProcessSide ^ isNextStartPoint;
        //                }
        //                break;

        //            case ObjectType.Arc:

        //                double angleTan = nextObj.StartPoint == point ? nextObj.ProcessArc.StartAngle + cPI2 : nextObj.ProcessArc.EndAngle - cPI2;
        //                angle = angleTan - obj.ProcessLine.Angle;
        //                if (Math.Abs(angle) > AngleTolerance)
        //                {
        //                    isLeftTurn = Math.Sin(angle) > 0;
        //                }
        //                else
        //                {
        //                    isLeftTurn = nextObj.StartPoint == point;
        //                }
        //                bool isRightProcessSide = obj.Side == SideType.Right;
        //                isExactly = isLeftTurn ^ isRightProcessSide;
        //                break;
        //        }
        //        if (isExactly)
        //        {
        //            double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll)) + ExactlyIncrease;
        //            if (point == obj.EndPoint)
        //            {
        //                toolpathCurve.EndPoint = toolpathCurve.GetPointAtDist((toolpathCurve as Line).Length - s);
        //            }
        //            else
        //            {
        //                toolpathCurve.StartPoint = toolpathCurve.GetPointAtDist(s);
        //            }
        //        }
        //    }
        //    if (point == obj.EndPoint)
        //    {
        //        obj.IsEndExactly = isExactly;
        //    }
        //    else
        //        {
        //        obj.IsBeginExactly = isExactly;
        //    }            
        //}

        /*        private void SetEnds(ProcessObject curObj, Curve toolpathCurve, Point3d curPoint)
                {
                    ProcessObject nextObj = null;
                    double nextAngle = 0;
                    int dir = curPoint == curObj.ProcessCurve.EndPoint ? 1 : -1;

                    if (curPoint == curObj.ProcessCurve.EndPoint)
                    {
                        curObj.IsEndExactly = false;
                    }
                    else
                    {
                        curObj.IsBeginExactly = false;
                    }
                    foreach (ProcessObject obj in ObjectList)
                    {
                        if (obj != curObj && obj.ProcessSide != 0 && (obj is ProcessObjectLine) &&
                            ((obj.ProcessCurve.StartPoint == curPoint) || (obj.ProcessCurve.EndPoint == curPoint)))
                        {
                            nextObj = obj;
                            break;
                        }
                    }
                    if (nextObj != null)
                    {
                        if (curObj.ProcessSide != nextObj.ProcessSide)
                        {
                            nextAngle = ((nextObj.ProcessCurve as Line).Angle + Math.PI) % (2 * Math.PI);
                        }
                        else
                        {
                            nextAngle = (nextObj.ProcessCurve as Line).Angle;
                        }

                        int nextSide = Math.Sign(Math.Sin(nextAngle - (curObj.ProcessCurve as Line).Angle));

                        //ed.WriteMessage("\n" + curObj.ToString() + " Знак: " + sign.ToString() + " dir: " + dir.ToString() + " ProcessSide: " + curObj.ProcessSide.ToString() +
                        //    ", " + ((nextObj.AcadObject as Line).Angle * 180 / Math.PI).ToString() + ", " + (nextAngle * 180 / Math.PI).ToString() + "\n");

                        if (dir * nextSide * curObj.ProcessSide == 1)
                        {
                            double s = Math.Sqrt(curObj.DepthAll * (curObj.Diameter - curObj.DepthAll));

                            if (curPoint == curObj.ProcessCurve.EndPoint)
                            {
                                toolpathCurve.EndPoint = toolpathCurve.GetPointAtDist((toolpathCurve as Line).Length - s);
                                curObj.IsEndExactly = true;
                            }
                            else
                            {
                                toolpathCurve.StartPoint = toolpathCurve.GetPointAtDist(s);
                                curObj.IsBeginExactly = true;
                            }
                        }
                    }
            
                    //throw new NotImplementedException();
                }
        */
        //public void SetEnd(ProcessObject obj, bool fStartCurve)
        //{
        //    ConstructToolpathObject(obj);
        //    return;

        //    if (obj.HasProcessSide)
        //    {
        //        using (DocumentLock doclock = Document.LockDocument())
        //        {
        //            using (AcDb.Transaction trans = TransactionManager.StartTransaction())
        //            {
        //                Curve procObj = trans.GetObject(obj.ToolpathCurve.ObjectId, AcDb.OpenMode.ForWrite) as Curve;
        //                double s = Math.Sqrt(obj.DepthAll * (obj.Diameter - obj.DepthAll));

        //                if (obj.HasProcessSide && obj is ProcessObjectLine)
        //                {
        //                    int offset;
        //                    if (obj.ProcessCurve is Line)
        //                    {
        //                        double angle = (obj.ProcessCurve as Line).Angle;
        //                        if (angle > 0 && angle <= Math.PI)
        //                        {
        //                            offset = obj.ToolpathOffsetSign == 1 ? 0 : 1;
        //                        }
        //                        else
        //                        {
        //                            offset = obj.ToolpathOffsetSign == 1 ? 1 : 0;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        double angle = (obj.ProcessCurve as Arc).StartAngle;
        //                        if (angle >= Math.PI * 0.5 && angle < Math.PI * 1.5)
        //                        {
        //                            offset = obj.ToolpathOffsetSign == 1 ? 0 : 1;
        //                        }
        //                        else
        //                        {
        //                            offset = obj.ToolpathOffsetSign == 1 ? 1 : 0;
        //                        }
        //                    }
        //                    DBObjectCollection path_coll = obj.ProcessCurve.GetOffsetCurves(obj.ToolpathOffsetSign * (offset * obj.Thickness + CalcOffsetInnerSideArc(obj)));
        //                    Curve tmpProcessCurve = path_coll[0] as Curve;

        //                    if (fStartCurve)
        //                    {
        //                        if (obj.IsBeginExactly)
        //                        {
        //                            procObj.StartPoint = tmpProcessCurve.GetPointAtDist(s);
        //                        }
        //                        else
        //                        {
        //                            procObj.StartPoint = tmpProcessCurve.StartPoint;
        //                        }
        //                    }
        //                    if (!fStartCurve)
        //                    {
        //                        if (obj.IsEndExactly)
        //                        {
        //                            double length = (tmpProcessCurve is Line) ? (tmpProcessCurve as Line).Length : (tmpProcessCurve as Arc).Length;
        //                            procObj.EndPoint = tmpProcessCurve.GetPointAtDist(length - s);
        //                        }
        //                        else
        //                        {
        //                            procObj.EndPoint = tmpProcessCurve.EndPoint;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var arc = obj.ProcessCurve as Arc;
        //                    if (fStartCurve)
        //                    {
        //                        if (obj.IsBeginExactly)
        //                        {
        //                            (procObj as Arc).StartAngle = arc.StartAngle + s / arc.Radius;
        //                        }
        //                        else
        //                        {
        //                            (procObj as Arc).StartAngle = arc.StartAngle;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (obj.IsEndExactly)
        //                        {
        //                            (procObj as Arc).EndAngle = arc.EndAngle - s / arc.Radius;
        //                        }
        //                        else
        //                        {
        //                            (procObj as Arc).EndAngle = arc.EndAngle;
        //                        }
        //                    }
        //                }
        //                trans.Commit();
        //                Editor.UpdateScreen();
        //            }
        //        }
        //    }
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
    }
}

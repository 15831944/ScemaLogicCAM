using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace ProcessingTechnologyCalc
{
    public partial class AcadProcessesView
    {
        public void ProgramGenerateRavelli()
        {
            ProgramLine.Reset();
            ProgramList.Clear();

            AddCommand("%300");
            AddCommand("RTCP=1");
            AddCommand("G600 X0 Y-2500 Z-370 U3800 V0 W0 N0");
            AddCommand("G601");
            AddCommand("G0 G53 Z0.0");
            AddCommand("G64");
            AddCommand("G154O10");
            AddCommand("T1");
            AddCommand("M6");
            AddCommand("G172 T1 H1 D1");
            AddCommand("M300");

            foreach (ProcessObject obj in ObjectList)
            {
                var line = obj as ProcessObjectLine;
                var arc = obj as ProcessObjectArc;
                //var angle = obj.TopEdge ^ obj.Side == SideType.Right ? (2 * Math.PI - line.ProcessLine.Angle) % (2 * Math.PI) : (3 * Math.PI - line.ProcessLine.Angle) % (2 * Math.PI);
                var point0 = obj.ProcessActions.First().Toolpath.StartPoint;

                //var r = obj.Diameter / 2;
                //var da = ProcessOptions.AxisDist + ProcessOptions.VertAxisDist;
                //var dz = da * Math.Sin(obj.VerticalAngle) - r * (1 - Math.Cos(obj.VerticalAngle));
                //var dr = da * (1 - Math.Cos(obj.VerticalAngle)) + r * Math.Sin(obj.VerticalAngle);
                //var dc = ProcessOptions.VertAxisDist - dr;
                //var angle = !obj.TopEdge ^ obj.Side == SideType.Right ? (2 * Math.PI - line.ProcessLine.Angle) % (2 * Math.PI) : (3 * Math.PI - line.ProcessLine.Angle) % (2 * Math.PI);
                //var dx = dc * Math.Sin(angle);
                //var dy = - dc * (1 - Math.Cos(angle));

                //AddCommand("' dx=" + Math.Round(dx, 2) + "; dy=" + Math.Round(dy, 2) + "; dz=" + Math.Round(dz, 2) + "; dc=" + Math.Round(dc, 2));

                var angle = line != null
                    ? line.ToolpathAngle
                    : obj.ProcessActions.First().Angle;
                AddCommand("G0" + ToGCode("X", point0.X)
                                + ToGCode("Y", point0.Y));
                AddCommand("G0" + ToGCode("Z", point0.Z)); //  ZToGCode(point0.Z));
                AddCommand("G1" + ToGCode("C", angle) + " F2000");
                AddCommand("G1" + ToGCode("A", obj.VerticalAngleDeg) + " F2000");

                if (obj == ObjectList.First())
                {
                    AddCommand("M3" + "S" + ProcessOptions.Frequency.ToString());
                    AddCommand("M7");
                }

                foreach (var action in obj.ProcessActions)
                {
                    if (action.Toolpath is Line)
                        AddCommand("G1" + ToGCode("X", action.Point.X )
                                        + ToGCode("Y", action.Point.Y )
                                        + ToGCode("Z", action.Point.Z ) // ZToGCode(action.Point.Z)
                                        + ((obj is ProcessObjectArc) ? ToGCode("C", action.Angle) : "")
                                        + ToGCode("F", action.Command == "Рез" || action.Command == "Подъем" ? obj.GreatSpeed : obj.SmallSpeed), action.Toolpath, action.Command);
                    else
                        AddCommand("G" + (action.IsClockwise ? 2 : 3)
                                       + ToGCode("X", action.Point.X)
                                       + ToGCode("Y", action.Point.Y)
                                       + ToGCode("I", ((Arc)action.Toolpath).Center.X)
                                       + ToGCode("J", ((Arc)action.Toolpath).Center.Y)
                                       + ToGCode("C", action.Angle)
                                       + ToGCode("F", obj.GreatSpeed), action.Toolpath, action.Command);
                }
            }
            AddCommand("M5");
            AddCommand("M9");
            AddCommand("G61");
            AddCommand("G153");
            AddCommand("G0 G53 Z0");
            AddCommand(";END OF PROGRAM");
            AddCommand("SETMSP=1");
            AddCommand("G0 G53 Z0");
            AddCommand("G0 G53 A0 C0");
            AddCommand("G0 G53 X0 Y0");
            AddCommand("M30");

            ProgramForm.RefreshGrid();
            PaletteSet.Activate(2);
        }

        //        Point3d[] point = { obj.ToolpathCurve.StartPoint, obj.ToolpathCurve.EndPoint };
        //        //int rest = (int)Math.Ceiling((double)obj.DepthAll / obj.Depth) % 2;
        //        bool hasRest = (int)Math.Ceiling((double)obj.DepthAll / obj.Depth) % 2 == 1;

        //        switch (obj.ObjectType)
        //        {
        //            case ObjectType.Line:
        //                //ind = (obj.ToolpathLine.Angle > 0 && obj.ToolpathLine.Angle <= Math.PI) ? 1 - rest : rest;
        //                var processObjectLine = obj as ProcessObjectLine;
        //                vertex = (processObjectLine.AngleRound > 0 && processObjectLine.AngleRound <= Math.PI) ^ hasRest ? VertexType.End : VertexType.Start;
        //                angle[0] = angle[1] = processObjectLine.ToolpathAngle;
        //                z = 0;
        //                break;
        //            case ObjectType.Arc:
        //                //ind = (obj.ToolpathArc.StartAngle >= Math.PI * 0.5 && obj.ToolpathArc.StartAngle < Math.PI * 1.5) ? 1 - rest : rest;
        //                bool isLeftArc = obj.ToolpathArc.StartAngle >= Math.PI * 0.5 && obj.ToolpathArc.StartAngle < Math.PI * 1.5;
        //                vertex = isLeftArc ^ hasRest ? VertexType.End : VertexType.Start;
        //                if (isLeftArc)
        //                {
        //                    angle[VertexType.Start.Index()] = (Math.PI * 1.5 - obj.ToolpathArc.StartAngle) * 180 / Math.PI;
        //                    angle[VertexType.End.Index()] = (Math.PI * 1.5 - obj.ToolpathArc.EndAngle) * 180 / Math.PI;
        //                }
        //                else
        //                {
        //                    angle[VertexType.Start.Index()] = ((Math.PI * 2.5 - obj.ToolpathArc.StartAngle) % (2 * Math.PI)) * 180 / Math.PI;
        //                    angle[VertexType.End.Index()] = ((Math.PI * 2.5 - obj.ToolpathArc.EndAngle) % (2 * Math.PI)) * 180 / Math.PI;
        //                }
        //                z = -obj.Depth;
        //                break;
        //        }
        //        if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //        {
        //            //                ProgramList.Add(new ProgramLine(0, 0, "XYC", 0, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()]));
        //            AddSetToolCommand("XYC", point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], obj.ToString());
        //            //                ProgramList.Add(new ProgramLine(0, 0, "XYZ", 0, point[vertex.Index()].X, point[vertex.Index()].Y, 20));
        //            AddSetToolCommand("XYZ", point[vertex.Index()].X, point[vertex.Index()].Y, ProcessOptions.ZSafety, obj.ToString());
        //        }
        //        else
        //        {
        //            AddCommand("G0" + ToGCode("X", point[vertex.Index()].X)
        //                            + ToGCode("Y", point[vertex.Index()].Y)
        //                            + CToGCode(angle[vertex.Index()])
        //                            + " B-90");
        //            AddCommand("G0" + ZToGCode(20));

        //        }
        //        if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //        {
        //            if (obj == ObjectList.First())
        //            {
        //                AddMCommand(97, 7);
        //                AddMCommand(97, 8);
        //                AddMCommand(97, 3, obj.Frequency);
        //            }
        //            //                ProgramList.Add(new ProgramLine(28, 0, "XYCZ"));
        //            AddGCommand(28, "XYCZ");
        //        }
        //        else
        //        {
        //            if (obj == ObjectList.First())
        //            {
        //                AddCommand("M3" + "S" + ProcessOptions.Frequency.ToString());
        //                AddCommand("M8");
        //                AddCommand("M7");
        //            }
        //        }

        //        do
        //        {
        //            z += obj.Depth;
        //            if (z > obj.DepthAll)
        //            {
        //                z = obj.DepthAll;
        //            }
        //            //                    ProgramList.Add(new ProgramLine(1, 0, "XYCZ", obj.SmallSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z));

        //            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //                AddProcessCommand(1, obj.SmallSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z, obj.ToString());
        //            else
        //                AddCommand("G1" + ToGCode("X", point[vertex.Index()].X)
        //                                + ToGCode("Y", point[vertex.Index()].Y)
        //                                + ZToGCode(-z)
        //                                + CToGCode(angle[vertex.Index()])
        //                                + ToGCode("F", obj.SmallSpeed));

        //            double xOld = point[vertex.Index()].X;
        //            double yOld = point[vertex.Index()].Y;

        //            vertex = vertex.Opposite();

        //            switch (obj.ObjectType)
        //            {
        //                case ObjectType.Line:
        //                    //                            ProgramList.Add(new ProgramLine(1, 0, "XYCZ", obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z));
        //                    if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //                        AddProcessCommand(1, obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z, obj.ToString());
        //                    else
        //                        AddCommand("G1" + ToGCode("X", point[vertex.Index()].X)
        //                                        + ToGCode("Y", point[vertex.Index()].Y)
        //                                        + ZToGCode(-z)
        //                                        + CToGCode(angle[vertex.Index()])
        //                                        + ToGCode("F", obj.GreatSpeed));
        //                    ;
        //                    break;
        //                case ObjectType.Arc:
        //                    //                            ProgramList.Add(new ProgramLine(type[vertex.Index()], 0, "XYCZ", obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, obj.ToolpathArc.Center.X, obj.ToolpathArc.Center.Y));
        //                    if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //                        AddProcessCommand(type[vertex.Index()], obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, obj.ToolpathArc.Center.X, obj.ToolpathArc.Center.Y, obj.ToString());
        //                    else
        //                        AddCommand("G" + type[vertex.Index()].ToString()
        //                                       + ToGCode("X", point[vertex.Index()].X)
        //                                       + ToGCode("Y", point[vertex.Index()].Y)
        //                                       + ToGCode("I", obj.ToolpathArc.Center.X - xOld)
        //                                       + ToGCode("J", obj.ToolpathArc.Center.Y - yOld)
        //                                       + CToGCode(angle[vertex.Index()])
        //                                       + ToGCode("F", obj.GreatSpeed));
        //                    ;
        //                    break;
        //            }
        //        }
        //        while (z < obj.DepthAll);

        //        //                ProgramList.Add(new ProgramLine(0, 0, "XYZ", 0, point[vertex.Index()].X, point[vertex.Index()].Y, 20));
        //        if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //            AddSetToolCommand("XYZ", point[vertex.Index()].X, point[vertex.Index()].Y, ProcessOptions.ZSafety, obj.ToString());
        //        else
        //            AddCommand("G0" + ToGCode("X", point[vertex.Index()].X)
        //                            + ToGCode("Y", point[vertex.Index()].Y)
        //                            + ZToGCode(20));
        //    }
        //    if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
        //    {

        //        AddMCommand(97, 9);
        //        AddMCommand(97, 10);
        //        AddMCommand(97, 5);
        //        AddMCommand(97, 30);
        //    }
        //    else
        //    {
        //        AddCommand("M5");
        //        AddCommand("M9");
        //        AddCommand("TRAFOOF");
        //        AddCommand("G0G90G153D0Z0");
        //        AddCommand("M30");
        //    }
        //    ProgramForm.RefreshGrid();
        //    PaletteSet.Activate(2);
        //}

        //private void AddProcessCommand(int type, int speed, double x, double y, double param1, double param2, string name)
        //{
        //    ProgramList.Add(new ProgramLine(type, null, "XYCZ", speed, x, y, param1, param2, name));
        //}

        //private void AddSetToolCommand(string axis, double x, double y, double param, string name)
        //{
        //    ProgramList.Add(new ProgramLine(0, null, axis, 0, x, y, param, null, name));
        //}

        //private void AddMCommand(int gCode, int mCode)
        //{
        //    ProgramList.Add(new ProgramLine(gCode, mCode, "", null, null, null, null, null, ""));
        //}

        //private void AddMCommand(int gCode, int mCode, int param)
        //{
        //    ProgramList.Add(new ProgramLine(gCode, mCode, "", param, null, null, null, null, ""));
        //}

        //private void AddGCommand(int gCode)
        //{
        //    ProgramList.Add(new ProgramLine(gCode, null, "", null, null, null, null, null, ""));
        //}

        //private void AddGCommand(int gCode, string axis)
        //{
        //    ProgramList.Add(new ProgramLine(gCode, null, axis, null, null, null, null, null, ""));
        //}

        //public void AddCommand(string line)
        //{
        //    ProgramList.Add(new ProgramLine(line));
        //}

        //public string ToGCode(string axis, double par)
        //{
        //    return (" " + axis + String.Format("{0:0.####}", par));
        //}

        //public string ZToGCode(double par)
        //{
        //    return (" Z" + String.Format("{0:0.####}", par + ProcessOptions.ToolsList[ProcessOptions.ToolNo - 1].Diameter / 2));
        //}

        //public string CToGCode(double par)
        //{
        //    return (" C" + String.Format("{0:0.####}", 270 - par));
        //}
    }
}
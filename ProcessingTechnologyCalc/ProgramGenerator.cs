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
        public void ProgramGenerate()
        {
            ProgramLine.Reset();
            ProgramList.Clear();

            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
            {
                AddGCommand(98);
                AddMCommand(97, 2, 1);
                AddGCommand(17, "XYCZ");
                AddGCommand(28, "XYCZ");
                AddMCommand(97, 6, ObjectList[0].ToolNo);
            }
            else
            {
                AddCommand("G54G17G90G642");
                AddCommand("G0G90G153D0Z0");

                AddCommand("T18");
                AddCommand("M6");
                AddCommand("D6");
                AddCommand("TRAORI");
                AddCommand("G54");
            }
            VertexType vertex = VertexType.Start;
            //int ind = 0;
            int z = 0;
            double[] angle = new double[2];
            int[] type = { 2, 3 };

            foreach (ProcessObject obj in ObjectList)
            {
                if (obj.ToolpathCurve == null)
                {
                    Application.ShowAlertDialog("Не указана сторона обработки объекта " + obj.ToString());
                    continue;
                }
                Point3d[] point = { obj.ToolpathCurve.StartPoint, obj.ToolpathCurve.EndPoint };
                //int rest = (int)Math.Ceiling((double)obj.DepthAll / obj.Depth) % 2;
                bool hasRest = (int)Math.Ceiling((double)obj.DepthAll / obj.Depth) % 2 == 1;

                switch (obj.ObjectType)
                {
                    case ObjectType.Line:
                        //ind = (obj.ToolpathLine.Angle > 0 && obj.ToolpathLine.Angle <= Math.PI) ? 1 - rest : rest;
                        vertex = (obj.ToolpathLine.Angle > 0 && obj.ToolpathLine.Angle <= Math.PI) ^ hasRest ? VertexType.End : VertexType.Start;
                        angle[0] = angle[1] = (obj as ProcessObjectLine).ToolpathAngle;
                        z = 0;
                        break;
                    case ObjectType.Arc:
                        //ind = (obj.ToolpathArc.StartAngle >= Math.PI * 0.5 && obj.ToolpathArc.StartAngle < Math.PI * 1.5) ? 1 - rest : rest;
                        bool isLeftArc = obj.ToolpathArc.StartAngle >= Math.PI * 0.5 && obj.ToolpathArc.StartAngle < Math.PI * 1.5;
                        vertex = isLeftArc ^ hasRest ? VertexType.End : VertexType.Start;
                        if (isLeftArc)
                        {
                            angle[VertexType.Start.Index()] = (Math.PI * 1.5 - obj.ToolpathArc.StartAngle) * 180 / Math.PI;
                            angle[VertexType.End.Index()] = (Math.PI * 1.5 - obj.ToolpathArc.EndAngle) * 180 / Math.PI;
                        }
                        else
                        {
                            angle[VertexType.Start.Index()] = ((Math.PI * 2.5 - obj.ToolpathArc.StartAngle) % (2 * Math.PI)) * 180 / Math.PI;
                            angle[VertexType.End.Index()] = ((Math.PI * 2.5 - obj.ToolpathArc.EndAngle) % (2 * Math.PI)) * 180 / Math.PI;
                        }
                        z = -obj.Depth;
                        break;
                }
                if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                {
                    //                ProgramList.Add(new ProgramLine(0, 0, "XYC", 0, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()]));
                    AddSetToolCommand("XYC", point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], obj.ToString());
                    //                ProgramList.Add(new ProgramLine(0, 0, "XYZ", 0, point[vertex.Index()].X, point[vertex.Index()].Y, 20));
                    AddSetToolCommand("XYZ", point[vertex.Index()].X, point[vertex.Index()].Y, 20, obj.ToString());
                }
                else
                {
                     AddCommand("G0" + ToGCode("X", point[vertex.Index()].X) 
                                     + ToGCode("Y", point[vertex.Index()].Y)
                                     + CToGCode(angle[vertex.Index()]) 
                                     + " B-90");
                    AddCommand("G0" + ZToGCode(20));

                }
                if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                {
                    if (obj == ObjectList.First())
                    {
                        AddMCommand(97, 7);
                        AddMCommand(97, 8);
                        AddMCommand(97, 3, obj.Frequency);
                    }
                    //                ProgramList.Add(new ProgramLine(28, 0, "XYCZ"));
                    AddGCommand(28, "XYCZ");
                }
                else
                {
                    if (obj == ObjectList.First())
                    {
                        AddCommand("M3" + "S" + ProcessOptions.Frequency.ToString());
                        AddCommand("M8");
                        AddCommand("M7");
                    }
                }

                do
                {
                    z += obj.Depth;
                    if (z > obj.DepthAll)
                    {
                        z = obj.DepthAll;
                    }
//                    ProgramList.Add(new ProgramLine(1, 0, "XYCZ", obj.SmallSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z));

                    if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                        AddProcessCommand(1, obj.SmallSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z, obj.ToString());
                    else
                        AddCommand("G1" + ToGCode("X", point[vertex.Index()].X)
                                        + ToGCode("Y", point[vertex.Index()].Y)
                                        + ZToGCode(-z)
                                        + CToGCode(angle[vertex.Index()])
                                        + ToGCode("F", obj.SmallSpeed));
                    
                    double xOld = point[vertex.Index()].X;
                    double yOld = point[vertex.Index()].Y;
                    
                    vertex = vertex.Opposite();

                    switch (obj.ObjectType)
                    {
	                    case ObjectType.Line:
//                            ProgramList.Add(new ProgramLine(1, 0, "XYCZ", obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z));
                            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                                AddProcessCommand(1, obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, angle[vertex.Index()], -z, obj.ToString());
                            else
                                AddCommand("G1" + ToGCode("X", point[vertex.Index()].X)
                                                + ToGCode("Y", point[vertex.Index()].Y)
                                                + ZToGCode(-z)
                                                + CToGCode(angle[vertex.Index()])
                                                + ToGCode("F", obj.GreatSpeed));
                            ;
                            break;
                        case ObjectType.Arc:
//                            ProgramList.Add(new ProgramLine(type[vertex.Index()], 0, "XYCZ", obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, obj.ToolpathArc.Center.X, obj.ToolpathArc.Center.Y));
                            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                                AddProcessCommand(type[vertex.Index()], obj.GreatSpeed, point[vertex.Index()].X, point[vertex.Index()].Y, obj.ToolpathArc.Center.X, obj.ToolpathArc.Center.Y, obj.ToString());
                            else
                                AddCommand("G" + type[vertex.Index()].ToString() 
                                               + ToGCode("X", point[vertex.Index()].X)
                                               + ToGCode("Y", point[vertex.Index()].Y)
                                               + ToGCode("I", obj.ToolpathArc.Center.X - xOld)
                                               + ToGCode("J", obj.ToolpathArc.Center.Y - yOld)
                                               + CToGCode(angle[vertex.Index()])
                                               + ToGCode("F", obj.GreatSpeed));
                            ;
                            break;
                    }
                }
                while (z < obj.DepthAll);

//                ProgramList.Add(new ProgramLine(0, 0, "XYZ", 0, point[vertex.Index()].X, point[vertex.Index()].Y, 20));
                if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                    AddSetToolCommand("XYZ", point[vertex.Index()].X, point[vertex.Index()].Y, 20, obj.ToString());
                else
                    AddCommand("G0" + ToGCode("X", point[vertex.Index()].X)
                                    + ToGCode("Y", point[vertex.Index()].Y)
                                    + ZToGCode(20));
            }
            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
            {

                AddMCommand(97, 9);
                AddMCommand(97, 10);
                AddMCommand(97, 5);
                AddMCommand(97, 30);
            }
            else
            {
                AddCommand("M5");
                AddCommand("M9");
                AddCommand("TRAFOOF");
                AddCommand("G0G90G153D0Z0");
                AddCommand("M30");
            }
            ProgramForm.RefreshGrid();
            PaletteSet.Activate(2);
        }

        private void AddProcessCommand(int type, int speed, double x, double y, double param1, double param2, string name)
        {
            ProgramList.Add(new ProgramLine(type, null, "XYCZ", speed, x, y, param1, param2, name));
        }

        private void AddSetToolCommand(string axis, double x, double y, double param, string name)
        {
            ProgramList.Add(new ProgramLine(0, null, axis, 0, x, y, param, null, name));
        }

        private void AddMCommand(int gCode, int mCode)
        {
            ProgramList.Add(new ProgramLine(gCode, mCode, "", null, null, null, null, null, ""));
        }

        private void AddMCommand(int gCode, int mCode, int param)
        {
            ProgramList.Add(new ProgramLine(gCode, mCode, "", param, null, null, null, null, ""));
        }

        private void AddGCommand(int gCode)
        {
            ProgramList.Add(new ProgramLine(gCode, null, "", null, null, null, null, null, ""));
        }

        private void AddGCommand(int gCode, string axis)
        {
            ProgramList.Add(new ProgramLine(gCode, null, axis, null, null, null, null, null, ""));
        }

        public void AddCommand(string line)
        {
            ProgramList.Add(new ProgramLine(line));
        }

        public string ToGCode(string axis, double par)
        {
            return (" " + axis + String.Format("{0:0.####}", par));
        }

        public string ZToGCode(double par)
        {
            return (" Z" + String.Format("{0:0.####}", par + ProcessOptions.ToolsList[ProcessOptions.ToolNo-1].Diameter/2));
        }

        public string CToGCode(double par)
        {
            return (" C" + String.Format("{0:0.####}", 270 - par));
        }
    }
}
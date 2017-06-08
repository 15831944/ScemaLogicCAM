using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingTechnologyCalc
{
    public class ProgramLine
    {
        static int Count;

        public string Line_no { get; set; }
        public string G_Code { get; set; }
        public string M_Code { get; set; }
        public string Axis { get; set; }
        public string Speed { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string C { get; set; }
        public string Z { get; set; }
        public string ObjectName { get; set; }

        //public ProgramLine(int g_code, int? m_code, string axis, int? speed, double? x, double? y, double? c, double? z)
        //{
        //    InitProgramLine(g_code, m_code, axis, speed, x, y, c, z);
        //}
        //public ProgramLine(int g_code, int m_code, string axis, int speed, double x, double y, double c)
        //{
        //    InitProgramLine(g_code, m_code, axis, speed, x, y, c, null);
        //}
        //public ProgramLine(int g_code, int m_code, string axis, int speed)
        //{
        //    InitProgramLine(g_code, m_code, axis, speed, null, null, null, null);
        //}
        //public ProgramLine(int g_code, int m_code, string axis)
        //{
        //    InitProgramLine(g_code, m_code, axis, null, null, null, null, null);
        //}
        //public ProgramLine(int g_code, int m_code)
        //{
        //    InitProgramLine(g_code, m_code, null, null, null, null, null, null);
        //}
        //public ProgramLine(int g_code)
        //{
        //    InitProgramLine(g_code, null, null, null, null, null, null, null);
        //}
        static public void Reset()
        {
            Count = 0;
        }
//        public void InitProgramLine(int g_code, int? m_code, string axis, int? speed, double? x, double? y, double? c, double? z)
        public ProgramLine(int g_code, int? m_code, string axis, int? speed, double? x, double? y, double? c, double? z, string objectName)
        {
            Line_no = (++Count).ToString();
            G_Code = g_code.ToString();
            if (m_code != null)
            {
                M_Code = m_code.ToString();
            }
            Axis = axis;
            if (speed != null)
            {
                Speed = speed.ToString();
            }
            if (x != null)
            {
                X = String.Format("{0:0.####}", x);
            }
            if (y != null)
            {
                Y = String.Format("{0:0.####}", y);
            }
            if (c != null)
            {
                C = String.Format("{0:0.####}", c);
            }
            if (z != null)
            {
                Z = String.Format("{0:0.####}", z);
            }
            ObjectName = objectName;
        }
        public ProgramLine(string line)
        {
            Line_no = "N"+ (++ Count).ToString();
            G_Code = line;
        }
    }
}

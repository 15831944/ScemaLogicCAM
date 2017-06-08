using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingTechnologyCalc
{
    public enum ObjectType { Line, Arc, Polyline };
    public enum SideType { None, Left, Right };
    public enum VertexType { Start, End };

    public static class EnumExtension
    {
        public static int Index(this VertexType vertex)
        {
            return (int)vertex;
        }
        public static VertexType Opposite(this VertexType vertex)
        {
            return vertex == VertexType.Start ? VertexType.End : VertexType.Start;
        }
        public static SideType Opposite(this SideType side)
        {
            switch (side)
            {
                case SideType.Left:
                    return SideType.Right;
                case SideType.Right:
                    return SideType.Left;
                default:
                    return SideType.None;
            }
        }
    }
}

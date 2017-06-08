using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingTechnologyCalc
{
    public interface IDrawForm
    {
        void SelectObject(ProcessObject obj);
        void DeleteObject(ProcessObject obj);
        void SetProcessSide(ProcessObject obj);
        void SetExactlyEnd(ProcessObject obj, VertexType vertex);
        void TurnProcessLayer();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingTechnologyCalc
{
    public interface IObjectForm
    {
        void SelectObject(int index);
        void RefreshList();
    }
}

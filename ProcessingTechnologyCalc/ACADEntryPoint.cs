namespace ProcessingTechnologyCalc
{
    public class AcadCommand
    {
        [Autodesk.AutoCAD.Runtime.CommandMethod("show")]
        public void Show()
        {
            AcadProcessesView.Instanse.PaletteSetShow();
        }
        [Autodesk.AutoCAD.Runtime.CommandMethod("add")] //, CommandFlags.UsePickSet)] // | CommandFlags.Redraw | CommandFlags.Modal)] //CommandFlags.Redraw SetImpliedSelection()SelectImplied
        public void Add()
        {
            AcadProcessesView.Instanse.AddObject();
        }
    }
}

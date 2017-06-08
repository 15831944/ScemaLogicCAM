using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace ProcessingTechnologyCalc
{
    public partial class AcadProcessesView
    {
        private PaletteSet PaletteSet;

        private ObjectForm ObjectForm;
        private SettingForm SettingForm;
        private ProgramForm ProgramForm;

        private List<ProcessObject> ObjectList = new List<ProcessObject>();
        private ProcessOptions ProcessOptions = new ProcessOptions();
        private List<ProgramLine> ProgramList = new List<ProgramLine>();

        public AcadProcessesView()
        {
            PaletteSet = new PaletteSet("Технология", new Guid("63B8DB5B-10E4-4924-B8A2-A9CF9158E4F6"));
            PaletteSet.Style = PaletteSetStyles.NameEditable | PaletteSetStyles.ShowPropertiesMenu | 
                PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
            PaletteSet.MinimumSize = new System.Drawing.Size(300, 200);

            ObjectForm = new ObjectForm(this, ObjectList);
            PaletteSet.Add("Объекты", ObjectForm);

            SettingForm = new SettingForm(ProcessOptions, this);  // TODO - загрузка ProcessOptions
            PaletteSet.Add("Настройка", SettingForm);

            ProgramForm = new ProgramForm(ProgramList, ProcessOptions);
            PaletteSet.Add("Программа", ProgramForm);

            PaletteSet.PaletteActivated += new PaletteActivatedEventHandler(PaletteSet_PaletteActivated);
            Application.DocumentManager.MdiActiveDocument.Editor.SelectionAdded += new SelectionAddedEventHandler(CallbackSelectionAdded);
        }

        void PaletteSet_PaletteActivated(object sender, PaletteActivatedEventArgs e)
        {
            if (e.Deactivated.Name == "Настройка")
            {
                SettingForm.UpdateControls();
            }
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("show")]
        public void PaletteSetShow()
        {
            PaletteSet.Visible = true;
            PaletteSet.Activate(0);
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("add")] //, CommandFlags.UsePickSet)] // | CommandFlags.Redraw | CommandFlags.Modal)] //CommandFlags.Redraw SetImpliedSelection()SelectImplied
        public void AddObject()
        {
            SettingForm.UpdateParams();
            AddSelectedObject(ProcessOptions);
            PaletteSetShow();
            ObjectForm.RefreshList();
        }
    }
}

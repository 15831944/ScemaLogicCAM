using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.DatabaseServices;

namespace ProcessingTechnologyCalc
{
    public class ACADProcessObjectHandler
    {
        private Autodesk.AutoCAD.Windows.PaletteSet ps;
        private ObjectForm fmObject;
        private SettingForm fmSetting;

        private Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;        
        private List<ProcessObject> m_processList = new List<ProcessObject>();
        private ObjectId CurrentObjectID;

        private static ACADProcessObjectHandler m_instanse = new ACADProcessObjectHandler();

        private ACADProcessObjectHandler()
        {
            ps = new Autodesk.AutoCAD.Windows.PaletteSet("Технология", new Guid("63B8DB5B-10E4-4924-B8A2-A9CF9158E4F6"));
            ps.Style = Autodesk.AutoCAD.Windows.PaletteSetStyles.NameEditable |
                Autodesk.AutoCAD.Windows.PaletteSetStyles.ShowPropertiesMenu |
                Autodesk.AutoCAD.Windows.PaletteSetStyles.ShowAutoHideButton |
                Autodesk.AutoCAD.Windows.PaletteSetStyles.ShowCloseButton;
            ps.MinimumSize = new System.Drawing.Size(300, 200);
            fmObject = new ObjectForm(this);
            ps.Add("Объекты", fmObject);
            fmSetting = new SettingForm(this);
            ps.Add("Настройка", fmSetting);
           
            ObjectParams param = new ObjectParams();
            param.MaterialType = 0;
            param.GreatSpeed   = 1500;
            param.SmallSpeed   = 250;
            param.Frequency    = 1800;
            param.DepthAll     = 32;
            param.Depth        = 32;
            param.ToolNo       = 1;
            param.Tools = new ObjectParams.Tool[3];
            param.Tools[0].Diameter = 100;
            param.Tools[0].Thickness = 1;
            param.Tools[1].Diameter = 200;
            param.Tools[1].Thickness = 2;
            param.Tools[2].Diameter = 300;
            param.Tools[2].Thickness = 3;
            fmSetting.Params = param;

            ed.SelectionAdded += new SelectionAddedEventHandler(callback_SelectionAdded);
            fmObject.SelectItemAction = delegate(int index) { return m_processList[index].SetSelected(); };
        }

        private  bool clearFlag = true;

        private void callback_SelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            //fmSetting.textBox1.AppendText(String.Format("SelectionAdded - {0} ssCount:{1} added:{2}\r\n", e.Flags, e.Selection.Count, e.AddedObjects.Count));
            if (e.AddedObjects != null && e.AddedObjects.Count > 0)
                if (e.AddedObjects.Count > 1)
                {
                    fmObject.ClearSelected();
                    clearFlag = true;
                }
                else
                    if (clearFlag || e.AddedObjects.GetObjectIds()[0] != CurrentObjectID)
                    {
                        clearFlag = false;
                        CurrentObjectID = e.AddedObjects.GetObjectIds()[0];
                        fmObject.SelectItem(m_processList.FindIndex(p => p.Object.ObjectId == CurrentObjectID));
                    }
        }

        public bool ProcessExists(ObjectId id)
        {
            return m_processList.Exists(p => p.Object.ObjectId == id);
        }

        public void Visible()
        {
            ps.Visible = true;
            ps.Activate(0);
        }

        public Editor getEditor()
        {
            return ed;
        }

        public ObjectForm getObjectForm()
        {
            return fmObject;
        }

        public SettingForm getSettingForm()
        {
            return fmSetting;
        }

        public static ACADProcessObjectHandler getInstanse()
        {
            return m_instanse;
        }

        public void addProcess(ProcessObject processObject)
        {
            m_processList.Add(processObject);
            fmObject.AddItem(processObject.ToString());
        }

        public List<ProcessObject> getProcessList()
        {
            return m_processList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessingTechnologyCalc
{
    public partial class ObjectForm : UserControl
    {
        private AcadProcessesView Owner;
        private CurrencyManager CurrencyManager;

        public ObjectForm( AcadProcessesView owner, IList<ProcessObject> list)
        {
            InitializeComponent();
            Owner = owner;
            listBox.DataSource = list;
            CurrencyManager = (CurrencyManager)BindingContext[listBox.DataSource];
            toolStrip.ImageList = imageList;
            toolStripButtonDraw.ImageIndex = 0;
        }

        public void SelectObject(int index)                 //  выбор объекта 
        {
            if (index != -1)
            {
                CurrencyManager.Position = index;
                if (listBox.SelectedIndex == -1)
                {
                    listBox.SelectedIndex = index;
                    propertyGrid.SelectedObject = CurrencyManager.Current;
                }
            }
            else
            {
                listBox.ClearSelected();
                propertyGrid.SelectedObject = null;
            }
        }

        public void RefreshList()
        {
            if (CurrencyManager.Count > listBox.Items.Count)                           // добавление объекта
            {
                CurrencyManager.Refresh();
                CurrencyManager.Position = CurrencyManager.Count - 1;
                propertyGrid.SelectedObject = CurrencyManager.Current;
            }
            else
            {
                CurrencyManager.Refresh();
                listBox.SelectedIndex = -1;
                propertyGrid.SelectedObject = null;
            }
            toolStrip.Enabled = CurrencyManager.Count > 0;
        }

        public void RefreshProperty()
        {
            propertyGrid.Refresh();
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                Owner.SelectObject(CurrencyManager.Current as ProcessObject);
                propertyGrid.SelectedObject = CurrencyManager.Current;
            }
        }

        private void toolStripButtonMove_Click(object sender, EventArgs e)
        {
            int newPos;
            if (sender == toolStripButtonUp)
            {
                newPos = CurrencyManager.Position - 1;
            }
            else
            {
                newPos = CurrencyManager.Position + 1;
            }
            if (newPos >= 0 && newPos < CurrencyManager.Count)
            {
                object obj = CurrencyManager.Current;
                CurrencyManager.List[CurrencyManager.Position] = CurrencyManager.List[newPos];
                CurrencyManager.List[newPos] = obj;
                CurrencyManager.Position = newPos;
                CurrencyManager.Refresh();
            }
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            Owner.DeleteObject(CurrencyManager.Current as ProcessObject);
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "№ инструмента")
            {
                propertyGrid.Refresh();
            }
            if (e.ChangedItem.Label == "Название")
            {
                CurrencyManager.Refresh();
            }
            if (e.ChangedItem.Label == "Точно начало")
            {
                Owner.SetExactlyEnd(CurrencyManager.Current as ProcessObject, VertexType.Start);
            }
            if (e.ChangedItem.Label == "Точно конец")
            {
                Owner.SetExactlyEnd(CurrencyManager.Current as ProcessObject, VertexType.End);
            }
        }

        private void toolStripButtonDraw_Click(object sender, EventArgs e)
        {
            Owner.TurnProcessLayer();
            toolStripButtonDraw.ImageIndex = 1 - toolStripButtonDraw.ImageIndex;
            toolStripButtonDraw.Text = toolStripButtonDraw.ImageIndex == 0 ? "Скрыть слой обработки" : "Показать слой обработки";
        }

        private void toolStripButtonGenerate_Click(object sender, EventArgs e)
        {
            if (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver)
                Owner.ProgramGenerate();
            else 
                Owner.ProgramGenerateRavelli();
        }

        private void toolStripButtonSetSide_Click(object sender, EventArgs e)
        {
            Owner.SetProcessSide(CurrencyManager.Current as ProcessObject);
            propertyGrid.Refresh();
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            Owner.AddObject(); // TODO Add_Click
        }

        private void toolStripButtonDeleteAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить все объекты?", "Вопрос", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Owner.DeleteAllObjects();
                RefreshList();
            }
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            Owner.RecalcToolpath();
        }
    }
}

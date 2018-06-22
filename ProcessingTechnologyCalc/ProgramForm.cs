using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ProcessingTechnologyCalc
{
    public partial class ProgramForm : UserControl
    {
        private List<ProgramLine> List;
        private string ProgramPath;
        private ProcessOptions Options;

        public ProgramForm(List<ProgramLine> list, ProcessOptions processOptions)
        {
            List = list;
            InitializeComponent();
            Options = processOptions;
            bindingSource1.DataSource = List;
        }
        public void RefreshGrid()
        {
            //dataGridView.DataSource = null;
            //dataGridView.DataSource = List;
            bindingSource1.ResetBindings(false);
            toolStripButtonSave.Enabled = true;
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver ?
                                    "Denver (*.csv)|*.csv" : "Donatoni (*.pgm)|*.pgm";
            saveFileDialog.FileName = ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver ?
                                    "*.csv" : "*.pgm";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ProgramPath = saveFileDialog.FileName;
                    using (StreamWriter streamwriter = new StreamWriter(ProgramPath))
                    {
                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            string line = "";
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                if (dataGridView.Columns[cell.ColumnIndex].HeaderText != "ObjectName" && dataGridView.Columns[cell.ColumnIndex].HeaderText != "Описание")
                                {
                                    line += cell.Value + (ProcessOptions.Machine == ProcessOptions.TTypeMachine.Denver ? ";" : " ");
                                }
                            }
                            streamwriter.WriteLine(line);
                        }
                    }
                    toolStripButtonSend.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при записи файла программы: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        private void toolStripButtonSend_Click(object sender, EventArgs e)
        {
            string path = @"\\192.168.137.59\ssd\Automatico\";
            //string path = @"C:\Documents and Settings\Mikov\Мои документы\1\";
            FileInfo file = new FileInfo(ProgramPath);

            if (file.Exists)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;                   
                    file.CopyTo(path + file.Name, true);
                    Cursor = Cursors.Default;
                    MessageBox.Show("Файл программы успешно загружен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Ошибка при копировании: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Файл \"" + ProgramPath + "\" не найден", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {
            var action = bindingSource1.Current as ProgramLine;
            if (action != null)
            {
                var list = new List<Autodesk.AutoCAD.DatabaseServices.ObjectId>();
                if (action.Curve != null)
                    list.Add(action.Curve.ObjectId);
                var editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                editor.SetImpliedSelection(list.ToArray());
                editor.UpdateScreen();
            }
        }
    }
}

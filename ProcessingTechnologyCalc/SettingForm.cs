using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace ProcessingTechnologyCalc
{
    public partial class SettingForm : UserControl
    {
        private ProcessOptions Options;
        private AcadProcessesView AcadForm;
        private string ToolPath = "Utensili.csv";//@"\Program Files\Common Files\Autodesk Shared\Catalina\Utensili.csv";

        public SettingForm(ProcessOptions processOptions, AcadProcessesView acadForm)
        {
            InitializeComponent();

            var pathName = Assembly.GetExecutingAssembly().Location;
            ToolPath = pathName.Replace("out.dll", ToolPath);

            Options = processOptions;
            AcadForm = acadForm;
            DenverBtn.Checked = Options.Machine == ProcessOptions.TTypeMachine.Denver;
            RavelliBtn.Checked = Options.Machine == ProcessOptions.TTypeMachine.Ravelli;
            
            LoadTools();

            //cbMaterialType.SelectedIndex = Options.MaterialType;
            //edMaterialThickness.Text = (Options.DepthAll + 2).ToString();
            //edGreatSpeed.Text = Options.GreatSpeed.ToString();
            //edSmallSpeed.Text = Options.SmallSpeed.ToString();
            //edFrequency.Text = Options.Frequency.ToString();
            //edDepthAll.Text = Options.DepthAll.ToString();
            //edDepth.Text = Options.Depth.ToString();
            //edToolNo.Value = Options.ToolNo;
            //edToolNo.Maximum = Options.ToolsList.Count;
            SetTool(Options.ToolNo);
        }
        public void UpdateParams()
        {
            try
            {
                //Options.MaterialType = cbMaterialType.SelectedIndex;
                Options.GreatSpeed = Convert.ToInt32(edGreatSpeed.Text);
                Options.SmallSpeed = Convert.ToInt32(edSmallSpeed.Text);
                Options.Frequency = Convert.ToInt32(edFrequency.Text);
                Options.DepthAll = Convert.ToInt32(edDepthAll.Text);
                Options.Depth = Convert.ToInt32(edDepth.Text);
                Options.ToolNo = Convert.ToInt32(edToolNo.Value);            
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка при преобразовании параметров настройки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void edToolNo_ValueChanged(object sender, EventArgs e)
        {
            SetTool(Decimal.ToInt32(edToolNo.Value));
        }

        private void SetTool(int toolNo)
        {
            edToolDiameter.Text = Options.ToolsList[toolNo - 1].Diameter.ToString();
            edToolThickness.Text = Options.ToolsList[toolNo - 1].Thickness.ToString();
        }

        private void LoadTools()
        {
            try
            {
                using (StreamReader sr = new StreamReader(ToolPath))
                {
                    string line;
                    string[] strArray;
                    double val3, val4;
                    Options.ToolsList.Clear();
                    while ((line = sr.ReadLine()) != null)
                    {
                        Tool tool = new Tool();
                        tool.Diameter = 0;
                        strArray = line.Split(';');
                        try
                        {
                            tool.Diameter = double.Parse(strArray[0]);
                            tool.Thickness = double.Parse(strArray[1]);
                            val3 = double.Parse(strArray[2]);
                            val4 = double.Parse(strArray[3]);
                            if (val3 == 0 && val4 == 1)
                            {
                                Options.ToolsList.Add(tool);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Ошибка при преобразовании строки \"" + line + "\"\n" + e.Message
                                , "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при открытии файла инструментов: \n" + e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void edMaterialThickness_Validated(object sender, EventArgs e)
        {
            if (errorProvider.GetError(edMaterialThickness) == "")
            {
                int thickness = Convert.ToInt32(edMaterialThickness.Text);
                edDepthAll.Text = (thickness + 2).ToString();
                edDepth.Text = Math.Ceiling((double)(thickness + 2) / 2).ToString();
                errorProvider.SetError(edDepthAll, "");
                errorProvider.SetError(edDepth, "");
                if (thickness > 30)
                {
                    edGreatSpeed.Text = "1000";
                    errorProvider.SetError(edGreatSpeed, "");
                }
            }
        }

        private void edit_Validating(object sender, CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Modified)
            {
                textBox.Modified = false;
                try
                {
                    int thickness = Convert.ToInt32(textBox.Text);
                    errorProvider.SetError(textBox, "");
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка при преобразовании строки к целому числу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    errorProvider.SetError(textBox, "Ошибка при преобразовании строки к целому числу");
                }
            }
        }

        private void bCopy_Click(object sender, EventArgs e)
        {
            string path = @"\\192.168.1.59\ssd\_CUST\Utensili.csv";
            //string path = @"\Program Files\Common Files\Autodesk Shared\Catalina\1\Utensili.csv";
            if (File.Exists(path))
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    File.Copy(path, ToolPath, true);
                    Cursor = Cursors.Default;
                    MessageBox.Show("Файл инструментов успешно загружен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTools();
                    edToolNo.Maximum = Options.ToolsList.Count;
                    SetTool(Decimal.ToInt32(edToolNo.Value));
                }
                catch (Exception ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Ошибка при копировании: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Файл \"" + path + "\" не найден", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void radioButton1_Click(object sender, EventArgs e)  // мрамор    
        {
            edFrequency.Text  = "2500";
            edGreatSpeed.Text = "1500";
            edSmallSpeed.Text = "250";
            errorProvider.SetError(edFrequency, "");
            errorProvider.SetError(edGreatSpeed, "");
            errorProvider.SetError(edSmallSpeed, "");
        }

        private void radioButton2_Click(object sender, EventArgs e)   // Гранит
        {
            edFrequency.Text  = "1800";
            edGreatSpeed.Text = "2000";
            edSmallSpeed.Text = "200";
            errorProvider.SetError(edFrequency, "");
            errorProvider.SetError(edGreatSpeed, "");
            errorProvider.SetError(edSmallSpeed, "");
        }

        private void DenverBtn_CheckedChanged(object sender, EventArgs e)
        {
            Options.Machine = ProcessOptions.TTypeMachine.Denver;
        }

        private void RavelliBtn_CheckedChanged(object sender, EventArgs e)
        {
            Options.Machine = ProcessOptions.TTypeMachine.Ravelli;
        }

        private void edToolDiameter_Validating(object sender, CancelEventArgs e)
        {
            //e.Cancel = !SetTool();
        }

        private void edToolThickness_Validating(object sender, CancelEventArgs e)
        {
            //e.Cancel = !SetTool();
        }

        private bool SetTool()
        {
            double diameter;
            double thickness;
            if (Double.TryParse(edToolDiameter.Text, out diameter) && Double.TryParse(edToolThickness.Text, out thickness))
            {
                int toolNo = Decimal.ToInt32(edToolNo.Value);
                Options.ToolsList[toolNo - 1].Diameter = diameter;
                Options.ToolsList[toolNo - 1].Thickness = thickness;
                AcadForm.RecalcToolpath();
                return true;
            }
            else
            {
                MessageBox.Show("Введите числовое значение");
                return false;
            }
        }

        public void UpdateControls()
        {
            ProcessOptions.ZSafety = Convert.ToInt32(edZSafety.Text);
            if (edToolDiameter.Modified || edToolThickness.Modified)
            {
                SetTool();
                edToolDiameter.Modified = false;
                edToolThickness.Modified = false;
            }
        }


        // TODO пропадает список материалов
    //Friend Shared m_ps As Autodesk.AutoCAD.Windows.PaletteSet 
    //Private Sub ComboBox1_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox1.DropDown 
    //    m_ps.KeepFocus = True 
    //End Sub 

    //Private Sub ComboBox1_DropDownClosed(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox1.DropDownClosed 
    //    m_ps.KeepFocus = False 
    //End Sub 
    }
}

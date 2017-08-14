namespace ProcessingTechnologyCalc
{
    partial class SettingForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.edGreatSpeed = new System.Windows.Forms.TextBox();
            this.edSmallSpeed = new System.Windows.Forms.TextBox();
            this.edFrequency = new System.Windows.Forms.TextBox();
            this.edToolDiameter = new System.Windows.Forms.TextBox();
            this.edToolThickness = new System.Windows.Forms.TextBox();
            this.edDepthAll = new System.Windows.Forms.TextBox();
            this.edDepth = new System.Windows.Forms.TextBox();
            this.edToolNo = new System.Windows.Forms.NumericUpDown();
            this.gbTool = new System.Windows.Forms.GroupBox();
            this.bCopy = new System.Windows.Forms.Button();
            this.gbMaterial = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.edMaterialThickness = new System.Windows.Forms.TextBox();
            this.lbMaterialThickness = new System.Windows.Forms.Label();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.DenverBtn = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RavelliBtn = new System.Windows.Forms.RadioButton();
            this.edZSafety = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.edToolNo)).BeginInit();
            this.gbTool.SuspendLayout();
            this.gbMaterial.SuspendLayout();
            this.gbOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Скорость б";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Скорость м";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Шпиндель";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Номер";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Диаметр";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Толщина";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 100);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Глубина реза";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 126);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Построчно";
            // 
            // edGreatSpeed
            // 
            this.edGreatSpeed.Location = new System.Drawing.Point(95, 19);
            this.edGreatSpeed.Name = "edGreatSpeed";
            this.edGreatSpeed.Size = new System.Drawing.Size(84, 20);
            this.edGreatSpeed.TabIndex = 0;
            this.edGreatSpeed.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            // 
            // edSmallSpeed
            // 
            this.edSmallSpeed.Location = new System.Drawing.Point(95, 45);
            this.edSmallSpeed.Name = "edSmallSpeed";
            this.edSmallSpeed.Size = new System.Drawing.Size(84, 20);
            this.edSmallSpeed.TabIndex = 1;
            this.edSmallSpeed.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            // 
            // edFrequency
            // 
            this.edFrequency.Location = new System.Drawing.Point(95, 71);
            this.edFrequency.Name = "edFrequency";
            this.edFrequency.Size = new System.Drawing.Size(84, 20);
            this.edFrequency.TabIndex = 2;
            this.edFrequency.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            // 
            // edToolDiameter
            // 
            this.edToolDiameter.Location = new System.Drawing.Point(95, 45);
            this.edToolDiameter.Name = "edToolDiameter";
            this.edToolDiameter.Size = new System.Drawing.Size(84, 20);
            this.edToolDiameter.TabIndex = 1;
            this.edToolDiameter.Validating += new System.ComponentModel.CancelEventHandler(this.edToolDiameter_Validating);
            // 
            // edToolThickness
            // 
            this.edToolThickness.Location = new System.Drawing.Point(95, 71);
            this.edToolThickness.Name = "edToolThickness";
            this.edToolThickness.Size = new System.Drawing.Size(84, 20);
            this.edToolThickness.TabIndex = 2;
            this.edToolThickness.Validating += new System.ComponentModel.CancelEventHandler(this.edToolThickness_Validating);
            // 
            // edDepthAll
            // 
            this.edDepthAll.Location = new System.Drawing.Point(95, 97);
            this.edDepthAll.Name = "edDepthAll";
            this.edDepthAll.Size = new System.Drawing.Size(84, 20);
            this.edDepthAll.TabIndex = 3;
            this.edDepthAll.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            // 
            // edDepth
            // 
            this.edDepth.Location = new System.Drawing.Point(95, 123);
            this.edDepth.Name = "edDepth";
            this.edDepth.Size = new System.Drawing.Size(84, 20);
            this.edDepth.TabIndex = 4;
            this.edDepth.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            // 
            // edToolNo
            // 
            this.edToolNo.Location = new System.Drawing.Point(95, 19);
            this.edToolNo.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.edToolNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.edToolNo.Name = "edToolNo";
            this.edToolNo.Size = new System.Drawing.Size(84, 20);
            this.edToolNo.TabIndex = 0;
            this.edToolNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.edToolNo.ValueChanged += new System.EventHandler(this.edToolNo_ValueChanged);
            // 
            // gbTool
            // 
            this.gbTool.Controls.Add(this.bCopy);
            this.gbTool.Controls.Add(this.edToolNo);
            this.gbTool.Controls.Add(this.edToolThickness);
            this.gbTool.Controls.Add(this.edToolDiameter);
            this.gbTool.Controls.Add(this.label6);
            this.gbTool.Controls.Add(this.label5);
            this.gbTool.Controls.Add(this.label4);
            this.gbTool.Location = new System.Drawing.Point(37, 310);
            this.gbTool.Name = "gbTool";
            this.gbTool.Size = new System.Drawing.Size(203, 135);
            this.gbTool.TabIndex = 2;
            this.gbTool.TabStop = false;
            this.gbTool.Text = "Инструмент";
            // 
            // bCopy
            // 
            this.bCopy.Location = new System.Drawing.Point(104, 101);
            this.bCopy.Name = "bCopy";
            this.bCopy.Size = new System.Drawing.Size(75, 23);
            this.bCopy.TabIndex = 6;
            this.bCopy.Text = "Загрузить";
            this.bCopy.UseVisualStyleBackColor = true;
            this.bCopy.Click += new System.EventHandler(this.bCopy_Click);
            // 
            // gbMaterial
            // 
            this.gbMaterial.Controls.Add(this.radioButton2);
            this.gbMaterial.Controls.Add(this.radioButton1);
            this.gbMaterial.Controls.Add(this.edMaterialThickness);
            this.gbMaterial.Controls.Add(this.lbMaterialThickness);
            this.gbMaterial.Location = new System.Drawing.Point(37, 20);
            this.gbMaterial.Name = "gbMaterial";
            this.gbMaterial.Size = new System.Drawing.Size(203, 97);
            this.gbMaterial.TabIndex = 0;
            this.gbMaterial.TabStop = false;
            this.gbMaterial.Text = "Материал";
            // 
            // radioButton2
            // 
            this.radioButton2.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(95, 19);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(52, 23);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Гранит";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Click += new System.EventHandler(this.radioButton2_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(22, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(58, 23);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Мрамор";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.radioButton1_Click);
            // 
            // edMaterialThickness
            // 
            this.edMaterialThickness.Location = new System.Drawing.Point(95, 62);
            this.edMaterialThickness.Name = "edMaterialThickness";
            this.edMaterialThickness.Size = new System.Drawing.Size(84, 20);
            this.edMaterialThickness.TabIndex = 2;
            this.edMaterialThickness.Validating += new System.ComponentModel.CancelEventHandler(this.edit_Validating);
            this.edMaterialThickness.Validated += new System.EventHandler(this.edMaterialThickness_Validated);
            // 
            // lbMaterialThickness
            // 
            this.lbMaterialThickness.AutoSize = true;
            this.lbMaterialThickness.Location = new System.Drawing.Point(19, 65);
            this.lbMaterialThickness.Name = "lbMaterialThickness";
            this.lbMaterialThickness.Size = new System.Drawing.Size(53, 13);
            this.lbMaterialThickness.TabIndex = 9;
            this.lbMaterialThickness.Text = "Толщина";
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.edZSafety);
            this.gbOptions.Controls.Add(this.label9);
            this.gbOptions.Controls.Add(this.edGreatSpeed);
            this.gbOptions.Controls.Add(this.label1);
            this.gbOptions.Controls.Add(this.label2);
            this.gbOptions.Controls.Add(this.edDepth);
            this.gbOptions.Controls.Add(this.edSmallSpeed);
            this.gbOptions.Controls.Add(this.edDepthAll);
            this.gbOptions.Controls.Add(this.edFrequency);
            this.gbOptions.Controls.Add(this.label3);
            this.gbOptions.Controls.Add(this.label8);
            this.gbOptions.Controls.Add(this.label7);
            this.gbOptions.Location = new System.Drawing.Point(37, 123);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(203, 181);
            this.gbOptions.TabIndex = 1;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Параметры обработки";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // DenverBtn
            // 
            this.DenverBtn.AutoSize = true;
            this.DenverBtn.Checked = true;
            this.DenverBtn.Location = new System.Drawing.Point(22, 28);
            this.DenverBtn.Name = "DenverBtn";
            this.DenverBtn.Size = new System.Drawing.Size(60, 17);
            this.DenverBtn.TabIndex = 3;
            this.DenverBtn.TabStop = true;
            this.DenverBtn.Text = "Denver";
            this.DenverBtn.UseVisualStyleBackColor = true;
            this.DenverBtn.CheckedChanged += new System.EventHandler(this.DenverBtn_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RavelliBtn);
            this.groupBox1.Controls.Add(this.DenverBtn);
            this.groupBox1.Location = new System.Drawing.Point(37, 451);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(203, 80);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Станок";
            this.groupBox1.Visible = false;
            // 
            // RavelliBtn
            // 
            this.RavelliBtn.AutoSize = true;
            this.RavelliBtn.Location = new System.Drawing.Point(22, 57);
            this.RavelliBtn.Name = "RavelliBtn";
            this.RavelliBtn.Size = new System.Drawing.Size(57, 17);
            this.RavelliBtn.TabIndex = 4;
            this.RavelliBtn.Text = "Ravelli";
            this.RavelliBtn.UseVisualStyleBackColor = true;
            this.RavelliBtn.CheckedChanged += new System.EventHandler(this.RavelliBtn_CheckedChanged);
            // 
            // edZSafety
            // 
            this.edZSafety.Location = new System.Drawing.Point(94, 149);
            this.edZSafety.Name = "edZSafety";
            this.edZSafety.Size = new System.Drawing.Size(84, 20);
            this.edZSafety.TabIndex = 8;
            this.edZSafety.Text = "20";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 152);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Z безопасн.";
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.gbMaterial);
            this.Controls.Add(this.gbTool);
            this.Name = "SettingForm";
            this.Size = new System.Drawing.Size(272, 602);
            ((System.ComponentModel.ISupportInitialize)(this.edToolNo)).EndInit();
            this.gbTool.ResumeLayout(false);
            this.gbTool.PerformLayout();
            this.gbMaterial.ResumeLayout(false);
            this.gbMaterial.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox edGreatSpeed;
        private System.Windows.Forms.TextBox edSmallSpeed;
        private System.Windows.Forms.TextBox edFrequency;
        private System.Windows.Forms.TextBox edToolDiameter;
        private System.Windows.Forms.TextBox edToolThickness;
        private System.Windows.Forms.TextBox edDepthAll;
        private System.Windows.Forms.TextBox edDepth;
        private System.Windows.Forms.NumericUpDown edToolNo;
        private System.Windows.Forms.GroupBox gbTool;
        private System.Windows.Forms.GroupBox gbMaterial;
        private System.Windows.Forms.TextBox edMaterialThickness;
        private System.Windows.Forms.Label lbMaterialThickness;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Button bCopy;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton DenverBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RavelliBtn;
        private System.Windows.Forms.TextBox edZSafety;
        private System.Windows.Forms.Label label9;
    }
}

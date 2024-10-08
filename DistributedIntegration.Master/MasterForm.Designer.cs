﻿namespace DistributedIntegration.Master
{
    partial class MasterForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnExecute = new Button();
            btnSelectDll = new Button();
            txtDllPath = new Label();
            lstClients = new ListBox();
            txtResult = new TextBox();
            btnSelectDataFile = new Button();
            txtDataFilePath = new Label();
            SuspendLayout();
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(36, 229);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(179, 29);
            btnExecute.TabIndex = 0;
            btnExecute.Text = "Запуск тестов";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // btnSelectDll
            // 
            btnSelectDll.Location = new Point(36, 16);
            btnSelectDll.Name = "btnSelectDll";
            btnSelectDll.Size = new Size(179, 29);
            btnSelectDll.TabIndex = 1;
            btnSelectDll.Text = "Select DLL";
            btnSelectDll.UseVisualStyleBackColor = true;
            btnSelectDll.Click += btnSelectDll_Click;
            // 
            // txtDllPath
            // 
            txtDllPath.AutoSize = true;
            txtDllPath.Location = new Point(268, 16);
            txtDllPath.Name = "txtDllPath";
            txtDllPath.Size = new Size(50, 20);
            txtDllPath.TabIndex = 2;
            txtDllPath.Text = "label1";
            // 
            // lstClients
            // 
            lstClients.FormattingEnabled = true;
            lstClients.Location = new Point(36, 104);
            lstClients.Name = "lstClients";
            lstClients.Size = new Size(738, 104);
            lstClients.TabIndex = 3;
            // 
            // txtResult
            // 
            txtResult.Location = new Point(36, 282);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ScrollBars = ScrollBars.Vertical;
            txtResult.Size = new Size(750, 312);
            txtResult.TabIndex = 4;
            // 
            // btnSelectDataFile
            // 
            btnSelectDataFile.Location = new Point(39, 59);
            btnSelectDataFile.Name = "btnSelectDataFile";
            btnSelectDataFile.Size = new Size(176, 29);
            btnSelectDataFile.TabIndex = 6;
            btnSelectDataFile.Text = "Select Data File";
            btnSelectDataFile.UseVisualStyleBackColor = true;
            btnSelectDataFile.Click += btnSelectDataFile_Click;
            // 
            // txtDataFilePath
            // 
            txtDataFilePath.AutoSize = true;
            txtDataFilePath.Location = new Point(268, 63);
            txtDataFilePath.Name = "txtDataFilePath";
            txtDataFilePath.Size = new Size(50, 20);
            txtDataFilePath.TabIndex = 7;
            txtDataFilePath.Text = "label1";
            // 
            // MasterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 606);
            Controls.Add(txtDataFilePath);
            Controls.Add(btnSelectDataFile);
            Controls.Add(txtResult);
            Controls.Add(lstClients);
            Controls.Add(txtDllPath);
            Controls.Add(btnSelectDll);
            Controls.Add(btnExecute);
            Name = "MasterForm";
            Text = "Form1";
            FormClosing += MasterForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnExecute;
        private Button btnSelectDll;
        private Label txtDllPath;
        private ListBox lstClients;
        private TextBox txtResult;
        private Button btnSelectDataFile;
        private Label txtDataFilePath;
    }
}

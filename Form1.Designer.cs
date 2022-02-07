using System;
using System.Drawing;

namespace Live_Funds_Data_Exporter
{
    partial class FundsDataExporterForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.prgBarFile = new System.Windows.Forms.ProgressBar();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSign = new System.Windows.Forms.Label();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.Lavender;
            this.pnlButtons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlButtons.Controls.Add(this.prgBarFile);
            this.pnlButtons.Controls.Add(this.btnExport);
            this.pnlButtons.Controls.Add(this.btnSelect);
            this.pnlButtons.Location = new System.Drawing.Point(12, 57);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(304, 306);
            this.pnlButtons.TabIndex = 0;
            // 
            // prgBarFile
            // 
            this.prgBarFile.AccessibleDescription = "";
            this.prgBarFile.AccessibleName = "";
            this.prgBarFile.BackColor = System.Drawing.Color.MintCream;
            this.prgBarFile.ForeColor = System.Drawing.Color.MediumAquamarine;
            this.prgBarFile.Location = new System.Drawing.Point(34, 138);
            this.prgBarFile.Name = "prgBarFile";
            this.prgBarFile.Size = new System.Drawing.Size(228, 23);
            this.prgBarFile.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgBarFile.TabIndex = 4;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.MintCream;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnExport.FlatAppearance.BorderSize = 3;
            this.btnExport.FlatAppearance.MouseDownBackColor = System.Drawing.Color.MediumAquamarine;
            this.btnExport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumAquamarine;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Sylfaen", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.btnExport.Location = new System.Drawing.Point(34, 178);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(228, 108);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export Data";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.MintCream;
            this.btnSelect.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSelect.FlatAppearance.BorderSize = 3;
            this.btnSelect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.MediumAquamarine;
            this.btnSelect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumAquamarine;
            this.btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelect.Font = new System.Drawing.Font("Sylfaen", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelect.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.btnSelect.Location = new System.Drawing.Point(34, 20);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(228, 101);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select File";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Sylfaen", 13.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "Live Funds Data Exporter";
            // 
            // lblSign
            // 
            this.lblSign.AutoSize = true;
            this.lblSign.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblSign.Location = new System.Drawing.Point(69, 366);
            this.lblSign.Name = "lblSign";
            this.lblSign.Size = new System.Drawing.Size(377, 17);
            this.lblSign.TabIndex = 2;
            this.lblSign.Text = "Created for Springpoint Finance Pty (Ltd) by J. Koekemoer";
            // 
            // FundsDataExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MediumAquamarine;
            this.ClientSize = new System.Drawing.Size(332, 387);
            this.Controls.Add(this.lblSign);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pnlButtons);
            this.Name = "FundsDataExporterForm";
            this.Text = "Funds Data Exporter";
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSign;
        private System.Windows.Forms.ProgressBar prgBarFile;
    }
}


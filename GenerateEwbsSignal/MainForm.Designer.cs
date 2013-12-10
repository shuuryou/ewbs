namespace GenerateEwbsSignal
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.fixedCodeComboBox = new System.Windows.Forms.ComboBox();
			this.fixedCodeLabel = new System.Windows.Forms.Label();
			this.preceedingCodeLabel = new System.Windows.Forms.Label();
			this.preceedingCodeComboBox = new System.Windows.Forms.ComboBox();
			this.locationCodeLabel = new System.Windows.Forms.Label();
			this.locationCodeComboBox = new System.Windows.Forms.ComboBox();
			this.dateTimeLabel = new System.Windows.Forms.Label();
			this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.generateButton = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.fixedCodeComboBox, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.fixedCodeLabel, 0, 1);
			this.tableLayoutPanel.Controls.Add(this.preceedingCodeLabel, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.preceedingCodeComboBox, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.locationCodeLabel, 0, 2);
			this.tableLayoutPanel.Controls.Add(this.locationCodeComboBox, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.dateTimeLabel, 0, 3);
			this.tableLayoutPanel.Controls.Add(this.dateTimePicker, 1, 3);
			this.tableLayoutPanel.Controls.Add(this.generateButton, 1, 5);
			this.tableLayoutPanel.Location = new System.Drawing.Point(8, 8);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 6;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(416, 136);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// fixedCodeComboBox
			// 
			this.fixedCodeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.fixedCodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.fixedCodeComboBox.FormattingEnabled = true;
			this.fixedCodeComboBox.Items.AddRange(new object[] {
            "Category I Start Signal/End Signal",
            "Category II Start Signal"});
			this.fixedCodeComboBox.Location = new System.Drawing.Point(100, 30);
			this.fixedCodeComboBox.Name = "fixedCodeComboBox";
			this.fixedCodeComboBox.Size = new System.Drawing.Size(313, 21);
			this.fixedCodeComboBox.TabIndex = 3;
			this.fixedCodeComboBox.SelectedIndexChanged += new System.EventHandler(this.fixedCodeComboBox_SelectedIndexChanged);
			// 
			// fixedCodeLabel
			// 
			this.fixedCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.fixedCodeLabel.AutoSize = true;
			this.fixedCodeLabel.Location = new System.Drawing.Point(3, 34);
			this.fixedCodeLabel.Name = "fixedCodeLabel";
			this.fixedCodeLabel.Size = new System.Drawing.Size(91, 13);
			this.fixedCodeLabel.TabIndex = 2;
			this.fixedCodeLabel.Text = "Fixed code:";
			// 
			// preceedingCodeLabel
			// 
			this.preceedingCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.preceedingCodeLabel.AutoSize = true;
			this.preceedingCodeLabel.Location = new System.Drawing.Point(3, 7);
			this.preceedingCodeLabel.Name = "preceedingCodeLabel";
			this.preceedingCodeLabel.Size = new System.Drawing.Size(91, 13);
			this.preceedingCodeLabel.TabIndex = 0;
			this.preceedingCodeLabel.Text = "Preceeding code:";
			// 
			// preceedingCodeComboBox
			// 
			this.preceedingCodeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.preceedingCodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.preceedingCodeComboBox.FormattingEnabled = true;
			this.preceedingCodeComboBox.Items.AddRange(new object[] {
            "Category I/II Start Signal",
            "End Signal"});
			this.preceedingCodeComboBox.Location = new System.Drawing.Point(100, 3);
			this.preceedingCodeComboBox.Name = "preceedingCodeComboBox";
			this.preceedingCodeComboBox.Size = new System.Drawing.Size(313, 21);
			this.preceedingCodeComboBox.TabIndex = 1;
			this.preceedingCodeComboBox.SelectedIndexChanged += new System.EventHandler(this.preceedingCodeComboBox_SelectedIndexChanged);
			// 
			// locationCodeLabel
			// 
			this.locationCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.locationCodeLabel.AutoSize = true;
			this.locationCodeLabel.Location = new System.Drawing.Point(3, 61);
			this.locationCodeLabel.Name = "locationCodeLabel";
			this.locationCodeLabel.Size = new System.Drawing.Size(91, 13);
			this.locationCodeLabel.TabIndex = 4;
			this.locationCodeLabel.Text = "Location code:";
			// 
			// locationCodeComboBox
			// 
			this.locationCodeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.locationCodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.locationCodeComboBox.FormattingEnabled = true;
			this.locationCodeComboBox.Location = new System.Drawing.Point(100, 57);
			this.locationCodeComboBox.Name = "locationCodeComboBox";
			this.locationCodeComboBox.Size = new System.Drawing.Size(313, 21);
			this.locationCodeComboBox.TabIndex = 5;
			// 
			// dateTimeLabel
			// 
			this.dateTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.dateTimeLabel.AutoSize = true;
			this.dateTimeLabel.Location = new System.Drawing.Point(3, 87);
			this.dateTimeLabel.Name = "dateTimeLabel";
			this.dateTimeLabel.Size = new System.Drawing.Size(91, 13);
			this.dateTimeLabel.TabIndex = 6;
			this.dateTimeLabel.Text = "Date and time:";
			// 
			// dateTimePicker
			// 
			this.dateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.dateTimePicker.CustomFormat = "yyyy/MM/dd HH:00";
			this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateTimePicker.ImeMode = System.Windows.Forms.ImeMode.Off;
			this.dateTimePicker.Location = new System.Drawing.Point(100, 84);
			this.dateTimePicker.Name = "dateTimePicker";
			this.dateTimePicker.Size = new System.Drawing.Size(313, 20);
			this.dateTimePicker.TabIndex = 7;
			// 
			// generateButton
			// 
			this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.generateButton.Location = new System.Drawing.Point(338, 110);
			this.generateButton.Name = "generateButton";
			this.generateButton.Size = new System.Drawing.Size(75, 23);
			this.generateButton.TabIndex = 8;
			this.generateButton.Text = "Generate";
			this.generateButton.UseVisualStyleBackColor = true;
			this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.Filter = "Wave files (*.wav)|*.wav";
			this.saveFileDialog.Title = "Save EWBS Signal";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 154);
			this.Controls.Add(this.tableLayoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EWBS Signal Generator";
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.Label preceedingCodeLabel;
		private System.Windows.Forms.ComboBox preceedingCodeComboBox;
		private System.Windows.Forms.ComboBox fixedCodeComboBox;
		private System.Windows.Forms.Label fixedCodeLabel;
		private System.Windows.Forms.Label locationCodeLabel;
		private System.Windows.Forms.ComboBox locationCodeComboBox;
		private System.Windows.Forms.Label dateTimeLabel;
		private System.Windows.Forms.DateTimePicker dateTimePicker;
		private System.Windows.Forms.Button generateButton;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
	}
}


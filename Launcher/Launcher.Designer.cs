namespace Launcher
{
    partial class Launcher
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
			this.label1 = new System.Windows.Forms.Label();
			this.listView1 = new System.Windows.Forms.ListView();
			this.fileNameColumn = new System.Windows.Forms.ColumnHeader();
			this.fileTypeColumn = new System.Windows.Forms.ColumnHeader();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(122, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select the file to Launch";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fileNameColumn,
            this.fileTypeColumn});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.listView1.Location = new System.Drawing.Point(0, 30);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(364, 141);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
			// 
			// fileNameColumn
			// 
			this.fileNameColumn.Text = "Filename";
			this.fileNameColumn.Width = 288;
			// 
			// fileTypeColumn
			// 
			this.fileTypeColumn.Text = "Type";
			this.fileTypeColumn.Width = 72;
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox1.Location = new System.Drawing.Point(272, 8);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(87, 17);
			this.checkBox1.TabIndex = 2;
			this.checkBox1.Text = "Always show";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// Launcher
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(364, 171);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.label1);
			this.Name = "Launcher";
			this.Text = "Launcher";
			this.Load += new System.EventHandler(this.Launcher_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Launcher_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader fileNameColumn;
		private System.Windows.Forms.ColumnHeader fileTypeColumn;
		private System.Windows.Forms.CheckBox checkBox1;
    }
}


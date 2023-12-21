namespace ShowUI
{
    partial class frmUnlockChange
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
            this.tb_CardID = new System.Windows.Forms.TextBox();
            this.lb_title = new System.Windows.Forms.Label();
            this.tb_Password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nbr_Space = new System.Windows.Forms.NumericUpDown();
            this.btn_Change = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_Cable = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nbr_Space)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Card ID";
            // 
            // tb_CardID
            // 
            this.tb_CardID.Location = new System.Drawing.Point(87, 55);
            this.tb_CardID.Name = "tb_CardID";
            this.tb_CardID.Size = new System.Drawing.Size(140, 20);
            this.tb_CardID.TabIndex = 1;
            this.tb_CardID.Text = "V";
            // 
            // lb_title
            // 
            this.lb_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_title.ForeColor = System.Drawing.Color.Red;
            this.lb_title.Location = new System.Drawing.Point(12, 9);
            this.lb_title.Name = "lb_title";
            this.lb_title.Size = new System.Drawing.Size(274, 25);
            this.lb_title.TabIndex = 2;
            this.lb_title.Text = "Change Cable";
            this.lb_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lb_title.DoubleClick += new System.EventHandler(this.lb_title_DoubleClick);
            // 
            // tb_Password
            // 
            this.tb_Password.Location = new System.Drawing.Point(87, 81);
            this.tb_Password.Name = "tb_Password";
            this.tb_Password.PasswordChar = '*';
            this.tb_Password.Size = new System.Drawing.Size(140, 20);
            this.tb_Password.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Space";
            // 
            // nbr_Space
            // 
            this.nbr_Space.Increment = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nbr_Space.Location = new System.Drawing.Point(87, 133);
            this.nbr_Space.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nbr_Space.Name = "nbr_Space";
            this.nbr_Space.Size = new System.Drawing.Size(139, 20);
            this.nbr_Space.TabIndex = 6;
            this.nbr_Space.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // btn_Change
            // 
            this.btn_Change.Location = new System.Drawing.Point(12, 164);
            this.btn_Change.Name = "btn_Change";
            this.btn_Change.Size = new System.Drawing.Size(274, 25);
            this.btn_Change.TabIndex = 7;
            this.btn_Change.Text = "Unlock";
            this.btn_Change.UseVisualStyleBackColor = true;
            this.btn_Change.Click += new System.EventHandler(this.btn_Change_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancel.Location = new System.Drawing.Point(121, 195);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(54, 24);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseCompatibleTextRendering = true;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Cable";
            // 
            // comboBox_Cable
            // 
            this.comboBox_Cable.FormattingEnabled = true;
            this.comboBox_Cable.Location = new System.Drawing.Point(86, 106);
            this.comboBox_Cable.Name = "comboBox_Cable";
            this.comboBox_Cable.Size = new System.Drawing.Size(139, 21);
            this.comboBox_Cable.TabIndex = 11;
            // 
            // frmUnlockChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(298, 231);
            this.Controls.Add(this.comboBox_Cable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btn_Change);
            this.Controls.Add(this.nbr_Space);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_Password);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lb_title);
            this.Controls.Add(this.tb_CardID);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ImeMode = System.Windows.Forms.ImeMode.On;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUnlockChange";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmUnlockChangeCable";
            ((System.ComponentModel.ISupportInitialize)(this.nbr_Space)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_CardID;
        private System.Windows.Forms.Label lb_title;
        private System.Windows.Forms.TextBox tb_Password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nbr_Space;
        private System.Windows.Forms.Button btn_Change;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_Cable;
    }
}
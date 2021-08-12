namespace ShowUI
{
    partial class frmLocking
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLocking));
            this.lbGeneralMsg = new System.Windows.Forms.Label();
            this.tbUnlock = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tbl_unlockData = new System.Windows.Forms.Button();
            this.lbBoom = new System.Windows.Forms.Label();
            this.lblClose = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxAction = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.DelayTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbGeneralMsg
            // 
            this.lbGeneralMsg.Font = new System.Drawing.Font("Times New Roman", 47.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbGeneralMsg.ForeColor = System.Drawing.Color.Yellow;
            this.lbGeneralMsg.Location = new System.Drawing.Point(31, 13);
            this.lbGeneralMsg.Name = "lbGeneralMsg";
            this.lbGeneralMsg.Size = new System.Drawing.Size(1033, 223);
            this.lbGeneralMsg.TabIndex = 7;
            this.lbGeneralMsg.Text = "Không Thể Test Sản Phẩm Do Lỗi Vượt Quá Tiêu Chuẩn";
            this.lbGeneralMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbUnlock
            // 
            this.tbUnlock.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbUnlock.ForeColor = System.Drawing.Color.Green;
            this.tbUnlock.Location = new System.Drawing.Point(882, 36);
            this.tbUnlock.Name = "tbUnlock";
            this.tbUnlock.Size = new System.Drawing.Size(82, 42);
            this.tbUnlock.TabIndex = 9;
            this.tbUnlock.Text = "Unlock";
            this.tbUnlock.UseVisualStyleBackColor = true;
            this.tbUnlock.Visible = false;
            this.tbUnlock.Click += new System.EventHandler(this.tbUnlock_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.tbl_unlockData);
            this.groupBox1.Controls.Add(this.lbBoom);
            this.groupBox1.Controls.Add(this.lblClose);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.lbGeneralMsg);
            this.groupBox1.ForeColor = System.Drawing.Color.Yellow;
            this.groupBox1.Location = new System.Drawing.Point(9, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox1.Size = new System.Drawing.Size(1095, 606);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(28, 239);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 200);
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // tbl_unlockData
            // 
            this.tbl_unlockData.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbl_unlockData.ForeColor = System.Drawing.Color.Green;
            this.tbl_unlockData.Location = new System.Drawing.Point(555, 540);
            this.tbl_unlockData.Name = "tbl_unlockData";
            this.tbl_unlockData.Size = new System.Drawing.Size(82, 33);
            this.tbl_unlockData.TabIndex = 13;
            this.tbl_unlockData.Text = "Unlock";
            this.tbl_unlockData.UseVisualStyleBackColor = true;
            this.tbl_unlockData.Click += new System.EventHandler(this.tbl_unlockData_Click);
            // 
            // lbBoom
            // 
            this.lbBoom.Font = new System.Drawing.Font("Times New Roman", 35.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbBoom.ForeColor = System.Drawing.Color.Yellow;
            this.lbBoom.Location = new System.Drawing.Point(234, 216);
            this.lbBoom.Name = "lbBoom";
            this.lbBoom.Size = new System.Drawing.Size(818, 155);
            this.lbBoom.TabIndex = 14;
            this.lbBoom.Text = "Error";
            this.lbBoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbBoom.Click += new System.EventHandler(this.lbBoom_Click);
            // 
            // lblClose
            // 
            this.lblClose.AutoSize = true;
            this.lblClose.BackColor = System.Drawing.Color.Orange;
            this.lblClose.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClose.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblClose.Location = new System.Drawing.Point(1070, 13);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(20, 19);
            this.lblClose.TabIndex = 13;
            this.lblClose.Text = "X";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Red;
            this.groupBox2.Controls.Add(this.tbUnlock);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.tbxAction);
            this.groupBox2.Location = new System.Drawing.Point(28, 443);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1041, 111);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 22);
            this.label1.TabIndex = 12;
            this.label1.Text = "Handling Action:";
            // 
            // tbxAction
            // 
            this.tbxAction.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxAction.Location = new System.Drawing.Point(183, 36);
            this.tbxAction.MinimumSize = new System.Drawing.Size(458, 30);
            this.tbxAction.Multiline = true;
            this.tbxAction.Name = "tbxAction";
            this.tbxAction.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxAction.Size = new System.Drawing.Size(693, 42);
            this.tbxAction.TabIndex = 11;
            this.tbxAction.Click += new System.EventHandler(this.tbxAction_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 30000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // DelayTimer
            // 
            this.DelayTimer.Interval = 30000;
            this.DelayTimer.Tick += new System.EventHandler(this.DelayTimer_Tick);
            // 
            // frmLocking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Red;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1113, 618);
            this.Controls.Add(this.groupBox1);
            this.ForeColor = System.Drawing.Color.Red;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Name = "frmLocking";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ShowUILock";
            this.Load += new System.EventHandler(this.frmLocking_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbGeneralMsg;
        private System.Windows.Forms.Button tbUnlock;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbxAction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.Timer DelayTimer;
        private System.Windows.Forms.Label lbBoom;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button tbl_unlockData;
    }
}
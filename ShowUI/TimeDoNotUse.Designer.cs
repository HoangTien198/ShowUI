namespace ShowUI
{
    partial class TimeDoNotUse
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
            this.lblTime = new System.Windows.Forms.Label();
            this.CountTime = new System.Windows.Forms.Timer(this.components);
            this.CheckTest = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Algerian", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.ForeColor = System.Drawing.Color.AliceBlue;
            this.lblTime.Location = new System.Drawing.Point(12, 35);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(139, 34);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "10:12:35";
            // 
            // CountTime
            // 
            this.CountTime.Interval = 1000;
            this.CountTime.Tick += new System.EventHandler(this.CountTime_Tick);
            // 
            // CheckTest
            // 
            this.CheckTest.Enabled = true;
            this.CheckTest.Interval = 1000;
            this.CheckTest.Tick += new System.EventHandler(this.CheckTest_Tick);
            // 
            // TimeDoNotUse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(355, 103);
            this.ControlBox = false;
            this.Controls.Add(this.lblTime);
            this.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TimeDoNotUse";
            this.Text = "TimeDoNotUse";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.TimeDoNotUse_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Timer CountTime;
        private System.Windows.Forms.Timer CheckTest;
    }
}
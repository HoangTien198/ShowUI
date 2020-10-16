namespace ShowUI
{
    partial class frmEmpAuthentication
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEmpAuthentication));
            this.lbEmpl = new System.Windows.Forms.Label();
            this.lbMsg = new System.Windows.Forms.Label();
            this.lbPassword = new System.Windows.Forms.Label();
            this.tbxEmp = new System.Windows.Forms.TextBox();
            this.tbxPw = new System.Windows.Forms.TextBox();
            this.btGo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbEmpl
            // 
            this.lbEmpl.AutoSize = true;
            this.lbEmpl.Location = new System.Drawing.Point(3, 20);
            this.lbEmpl.Name = "lbEmpl";
            this.lbEmpl.Size = new System.Drawing.Size(39, 13);
            this.lbEmpl.TabIndex = 0;
            this.lbEmpl.Text = "EmpID";
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.ForeColor = System.Drawing.Color.Red;
            this.lbMsg.Location = new System.Drawing.Point(39, 44);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(35, 13);
            this.lbMsg.TabIndex = 3;
            this.lbMsg.Text = "label1";
            this.lbMsg.Visible = false;
            // 
            // lbPassword
            // 
            this.lbPassword.AutoSize = true;
            this.lbPassword.Location = new System.Drawing.Point(145, 20);
            this.lbPassword.Name = "lbPassword";
            this.lbPassword.Size = new System.Drawing.Size(53, 13);
            this.lbPassword.TabIndex = 4;
            this.lbPassword.Text = "Password";
            this.lbPassword.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbPassword_MouseDoubleClick);
            // 
            // tbxEmp
            // 
            this.tbxEmp.Location = new System.Drawing.Point(42, 16);
            this.tbxEmp.Name = "tbxEmp";
            this.tbxEmp.Size = new System.Drawing.Size(97, 20);
            this.tbxEmp.TabIndex = 1;
            this.tbxEmp.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // tbxPw
            // 
            this.tbxPw.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tbxPw.Enabled = false;
            this.tbxPw.Location = new System.Drawing.Point(200, 17);
            this.tbxPw.Name = "tbxPw";
            this.tbxPw.PasswordChar = '|';
            this.tbxPw.Size = new System.Drawing.Size(97, 20);
            this.tbxPw.TabIndex = 5;
            this.tbxPw.Text = "@pw";
            this.tbxPw.TextChanged += new System.EventHandler(this.tbxPw_TextChanged);
            // 
            // btGo
            // 
            this.btGo.Location = new System.Drawing.Point(307, 17);
            this.btGo.Name = "btGo";
            this.btGo.Size = new System.Drawing.Size(58, 23);
            this.btGo.TabIndex = 6;
            this.btGo.Text = "Go";
            this.btGo.UseVisualStyleBackColor = true;
            this.btGo.Click += new System.EventHandler(this.btGo_Click);
            // 
            // frmEmpAuthentication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ClientSize = new System.Drawing.Size(373, 73);
            this.Controls.Add(this.btGo);
            this.Controls.Add(this.tbxPw);
            this.Controls.Add(this.lbPassword);
            this.Controls.Add(this.lbMsg);
            this.Controls.Add(this.tbxEmp);
            this.Controls.Add(this.lbEmpl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmEmpAuthentication";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Authentication";
            this.Load += new System.EventHandler(this.frmEmpAuthentication_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbEmpl;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Label lbPassword;
        private System.Windows.Forms.TextBox tbxEmp;
        private System.Windows.Forms.TextBox tbxPw;
        private System.Windows.Forms.Button btGo;
    }
}
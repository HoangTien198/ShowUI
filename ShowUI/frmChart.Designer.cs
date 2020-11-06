namespace ShowUIApp
{
    partial class frmChart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChart));
            this.RRChart = new ZedGraph.ZedGraphControl();
            this.YRChart = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // RRChart
            // 
            this.RRChart.BackColor = System.Drawing.SystemColors.Control;
            this.RRChart.Dock = System.Windows.Forms.DockStyle.Top;
            this.RRChart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RRChart.Location = new System.Drawing.Point(0, 0);
            this.RRChart.Name = "RRChart";
            this.RRChart.ScrollGrace = 0D;
            this.RRChart.ScrollMaxX = 0D;
            this.RRChart.ScrollMaxY = 0D;
            this.RRChart.ScrollMaxY2 = 0D;
            this.RRChart.ScrollMinX = 0D;
            this.RRChart.ScrollMinY = 0D;
            this.RRChart.ScrollMinY2 = 0D;
            this.RRChart.Size = new System.Drawing.Size(692, 230);
            this.RRChart.TabIndex = 0;
            this.RRChart.Load += new System.EventHandler(this.RRChart_Load);
            // 
            // YRChart
            // 
            this.YRChart.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.YRChart.Location = new System.Drawing.Point(0, 229);
            this.YRChart.Name = "YRChart";
            this.YRChart.ScrollGrace = 0D;
            this.YRChart.ScrollMaxX = 0D;
            this.YRChart.ScrollMaxY = 0D;
            this.YRChart.ScrollMaxY2 = 0D;
            this.YRChart.ScrollMinX = 0D;
            this.YRChart.ScrollMinY = 0D;
            this.YRChart.ScrollMinY2 = 0D;
            this.YRChart.Size = new System.Drawing.Size(692, 230);
            this.YRChart.TabIndex = 1;
            this.YRChart.Load += new System.EventHandler(this.YRChart_Load);
            // 
            // frmChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 459);
            this.Controls.Add(this.YRChart);
            this.Controls.Add(this.RRChart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmChart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chart";
            this.Load += new System.EventHandler(this.frmChart_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl RRChart;
        private ZedGraph.ZedGraphControl YRChart;
    }
}
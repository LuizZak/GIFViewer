namespace GIF_Viewer
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbl_keyframeReach = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_keyframeReach = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbl_keyframeMemory = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_keyframeMemory = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_bufferMemory = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_bufferMemory = new System.Windows.Forms.TrackBar();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb_keyframeReach)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_keyframeMemory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_bufferMemory)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.Location = new System.Drawing.Point(360, 250);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(83, 30);
            this.btn_ok.TabIndex = 0;
            this.btn_ok.Text = "&Ok";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(449, 250);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(83, 30);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "&Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lbl_keyframeReach);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.tb_keyframeReach);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lbl_keyframeMemory);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.tb_keyframeMemory);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lbl_bufferMemory);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tb_bufferMemory);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 232);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Memory";
            // 
            // lbl_keyframeReach
            // 
            this.lbl_keyframeReach.Location = new System.Drawing.Point(18, 187);
            this.lbl_keyframeReach.Name = "lbl_keyframeReach";
            this.lbl_keyframeReach.Size = new System.Drawing.Size(109, 13);
            this.lbl_keyframeReach.TabIndex = 14;
            this.lbl_keyframeReach.Text = "100";
            this.lbl_keyframeReach.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(489, 187);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(25, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "100";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(142, 187);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(13, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "0";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(18, 155);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(109, 32);
            this.label9.TabIndex = 11;
            this.label9.Text = "Maximum Keyframe Reach:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tb_keyframeReach
            // 
            this.tb_keyframeReach.LargeChange = 1;
            this.tb_keyframeReach.Location = new System.Drawing.Point(136, 155);
            this.tb_keyframeReach.Maximum = 100;
            this.tb_keyframeReach.Name = "tb_keyframeReach";
            this.tb_keyframeReach.Size = new System.Drawing.Size(378, 45);
            this.tb_keyframeReach.TabIndex = 10;
            this.tb_keyframeReach.Scroll += new System.EventHandler(this.tb_keyframeReach_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(483, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "1 GB";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(133, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "5 MB";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_keyframeMemory
            // 
            this.lbl_keyframeMemory.Location = new System.Drawing.Point(18, 116);
            this.lbl_keyframeMemory.Name = "lbl_keyframeMemory";
            this.lbl_keyframeMemory.Size = new System.Drawing.Size(109, 13);
            this.lbl_keyframeMemory.TabIndex = 7;
            this.lbl_keyframeMemory.Text = "100 MB";
            this.lbl_keyframeMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(18, 84);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 32);
            this.label8.TabIndex = 6;
            this.label8.Text = "Available Memory For\r\nKeyframes:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tb_keyframeMemory
            // 
            this.tb_keyframeMemory.LargeChange = 1;
            this.tb_keyframeMemory.Location = new System.Drawing.Point(136, 84);
            this.tb_keyframeMemory.Maximum = 100;
            this.tb_keyframeMemory.Name = "tb_keyframeMemory";
            this.tb_keyframeMemory.Size = new System.Drawing.Size(378, 45);
            this.tb_keyframeMemory.TabIndex = 5;
            this.tb_keyframeMemory.Scroll += new System.EventHandler(this.tb_keyframeMemory_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(483, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "1 GB";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(133, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "5 MB";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_bufferMemory
            // 
            this.lbl_bufferMemory.Location = new System.Drawing.Point(18, 51);
            this.lbl_bufferMemory.Name = "lbl_bufferMemory";
            this.lbl_bufferMemory.Size = new System.Drawing.Size(109, 13);
            this.lbl_bufferMemory.TabIndex = 2;
            this.lbl_bufferMemory.Text = "100 MB";
            this.lbl_bufferMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(18, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Available Buffer Memory:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tb_bufferMemory
            // 
            this.tb_bufferMemory.LargeChange = 1;
            this.tb_bufferMemory.Location = new System.Drawing.Point(136, 19);
            this.tb_bufferMemory.Maximum = 100;
            this.tb_bufferMemory.Name = "tb_bufferMemory";
            this.tb_bufferMemory.Size = new System.Drawing.Size(378, 45);
            this.tb_bufferMemory.TabIndex = 0;
            this.tb_bufferMemory.Scroll += new System.EventHandler(this.tb_bufferMemory_Scroll);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(544, 292);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tb_keyframeReach)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_keyframeMemory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_bufferMemory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_bufferMemory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar tb_bufferMemory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbl_keyframeMemory;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar tb_keyframeMemory;
        private System.Windows.Forms.Label lbl_keyframeReach;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar tb_keyframeReach;
    }
}
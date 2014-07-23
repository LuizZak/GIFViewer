namespace GIF_Viewer
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cb_useMinFrameInterval = new System.Windows.Forms.CheckBox();
            this.nud_minFrameInterval = new System.Windows.Forms.NumericUpDown();
            this.lblFrame = new System.Windows.Forms.Label();
            this.PlayBtn = new System.Windows.Forms.Button();
            this.cms_gifRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblLoading = new System.Windows.Forms.Label();
            this.tt_mainTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.btn_configuration = new System.Windows.Forms.Button();
            this.fileFormatWarningImage = new System.Windows.Forms.PictureBox();
            this.tlc_timeline = new GIF_Viewer.TimelineControl();
            this.pb_gif = new GIF_Viewer.CPictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_minFrameInterval)).BeginInit();
            this.cms_gifRightClick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileFormatWarningImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_gif)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_configuration);
            this.panel1.Controls.Add(this.tlc_timeline);
            this.panel1.Controls.Add(this.cb_useMinFrameInterval);
            this.panel1.Controls.Add(this.fileFormatWarningImage);
            this.panel1.Controls.Add(this.nud_minFrameInterval);
            this.panel1.Controls.Add(this.lblFrame);
            this.panel1.Controls.Add(this.PlayBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 412);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(484, 50);
            this.panel1.TabIndex = 1;
            // 
            // cb_useMinFrameInterval
            // 
            this.cb_useMinFrameInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_useMinFrameInterval.AutoSize = true;
            this.cb_useMinFrameInterval.Checked = true;
            this.cb_useMinFrameInterval.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_useMinFrameInterval.Location = new System.Drawing.Point(397, 5);
            this.cb_useMinFrameInterval.Name = "cb_useMinFrameInterval";
            this.cb_useMinFrameInterval.Size = new System.Drawing.Size(81, 17);
            this.cb_useMinFrameInterval.TabIndex = 5;
            this.cb_useMinFrameInterval.Text = "Use Min ms";
            this.cb_useMinFrameInterval.UseVisualStyleBackColor = true;
            this.cb_useMinFrameInterval.CheckedChanged += new System.EventHandler(this.cb_useMinFrameInterval_CheckedChanged);
            // 
            // nud_minFrameInterval
            // 
            this.nud_minFrameInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nud_minFrameInterval.Location = new System.Drawing.Point(397, 22);
            this.nud_minFrameInterval.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_minFrameInterval.Name = "nud_minFrameInterval";
            this.nud_minFrameInterval.Size = new System.Drawing.Size(50, 20);
            this.nud_minFrameInterval.TabIndex = 4;
            this.nud_minFrameInterval.ValueChanged += new System.EventHandler(this.nud_minFrameInterval_ValueChanged);
            this.nud_minFrameInterval.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nud_minFrameInterval_KeyUp);
            // 
            // lblFrame
            // 
            this.lblFrame.AutoSize = true;
            this.lblFrame.Location = new System.Drawing.Point(3, 29);
            this.lblFrame.Name = "lblFrame";
            this.lblFrame.Size = new System.Drawing.Size(42, 13);
            this.lblFrame.TabIndex = 2;
            this.lblFrame.Text = "Frame: ";
            // 
            // PlayBtn
            // 
            this.PlayBtn.Location = new System.Drawing.Point(3, 3);
            this.PlayBtn.Name = "PlayBtn";
            this.PlayBtn.Size = new System.Drawing.Size(66, 24);
            this.PlayBtn.TabIndex = 2;
            this.PlayBtn.Text = "&Stop";
            this.PlayBtn.UseVisualStyleBackColor = true;
            this.PlayBtn.Click += new System.EventHandler(this.PlayBtn_Click);
            // 
            // cms_gifRightClick
            // 
            this.cms_gifRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithToolStripMenuItem});
            this.cms_gifRightClick.Name = "contextMenuStrip1";
            this.cms_gifRightClick.Size = new System.Drawing.Size(141, 26);
            this.cms_gifRightClick.Opening += new System.ComponentModel.CancelEventHandler(this.cms_gifRightClick_Opening);
            this.cms_gifRightClick.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cms_gifRightClick_ItemClicked);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.openWithToolStripMenuItem.Text = "Open With...";
            // 
            // lblLoading
            // 
            this.lblLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLoading.Location = new System.Drawing.Point(0, 0);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(484, 412);
            this.lblLoading.TabIndex = 2;
            this.lblLoading.Text = "Loading...";
            this.lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_configuration
            // 
            this.btn_configuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_configuration.AutoSize = true;
            this.btn_configuration.Image = global::GIF_Viewer.Properties.Resources.applications_system;
            this.btn_configuration.Location = new System.Drawing.Point(453, 20);
            this.btn_configuration.Name = "btn_configuration";
            this.btn_configuration.Size = new System.Drawing.Size(23, 23);
            this.btn_configuration.TabIndex = 7;
            this.tt_mainTooltip.SetToolTip(this.btn_configuration, "Configurations");
            this.btn_configuration.UseVisualStyleBackColor = true;
            // 
            // fileFormatWarningImage
            // 
            this.fileFormatWarningImage.BackColor = System.Drawing.Color.Transparent;
            this.fileFormatWarningImage.Image = global::GIF_Viewer.Properties.Resources.emblem_important;
            this.fileFormatWarningImage.Location = new System.Drawing.Point(6, 7);
            this.fileFormatWarningImage.Name = "fileFormatWarningImage";
            this.fileFormatWarningImage.Size = new System.Drawing.Size(16, 16);
            this.fileFormatWarningImage.TabIndex = 3;
            this.fileFormatWarningImage.TabStop = false;
            this.tt_mainTooltip.SetToolTip(this.fileFormatWarningImage, "This image  is not in a .GIF file format!");
            this.fileFormatWarningImage.Visible = false;
            // 
            // tlc_timeline
            // 
            this.tlc_timeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlc_timeline.BehaviorType = GIF_Viewer.TimelineBehaviorType.Timeline;
            this.tlc_timeline.CurrentFrame = 1;
            this.tlc_timeline.DisplayFrameUnderMouse = true;
            this.tlc_timeline.FrameDisplayType = GIF_Viewer.TimelineFrameDisplayType.FrameNumber;
            this.tlc_timeline.Location = new System.Drawing.Point(75, 4);
            this.tlc_timeline.Maximum = 10;
            this.tlc_timeline.Minimum = 1;
            this.tlc_timeline.Name = "tlc_timeline";
            this.tlc_timeline.Range = new System.Drawing.Point(1, 9);
            this.tlc_timeline.ScrollScaleWidth = 1D;
            this.tlc_timeline.ScrollX = 0D;
            this.tlc_timeline.Size = new System.Drawing.Size(316, 38);
            this.tlc_timeline.TabIndex = 6;
            this.tlc_timeline.Text = "timelineControl1";
            this.tlc_timeline.TimelineHeight = 15;
            this.tlc_timeline.FrameChanged += new GIF_Viewer.TimelineControl.FrameChangedEventHandler(this.tlc_timeline_FrameChanged);
            // 
            // pb_gif
            // 
            this.pb_gif.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pb_gif.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pb_gif.Location = new System.Drawing.Point(0, 0);
            this.pb_gif.Name = "pb_gif";
            this.pb_gif.Size = new System.Drawing.Size(484, 409);
            this.pb_gif.TabIndex = 0;
            this.pb_gif.TabStop = false;
            this.pb_gif.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pb_gif_MouseDown);
            // 
            // FormMain
            // 
            this.AcceptButton = this.PlayBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 462);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pb_gif);
            this.Controls.Add(this.lblLoading);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(300, 95);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GIF Viewer";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_minFrameInterval)).EndInit();
            this.cms_gifRightClick.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileFormatWarningImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_gif)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CPictureBox pb_gif;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button PlayBtn;
        private System.Windows.Forms.Label lblFrame;
        private System.Windows.Forms.NumericUpDown nud_minFrameInterval;
        private System.Windows.Forms.CheckBox cb_useMinFrameInterval;
        private System.Windows.Forms.ContextMenuStrip cms_gifRightClick;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.PictureBox fileFormatWarningImage;
        private System.Windows.Forms.ToolTip tt_mainTooltip;
        private TimelineControl tlc_timeline;
        private System.Windows.Forms.Button btn_configuration;
    }
}


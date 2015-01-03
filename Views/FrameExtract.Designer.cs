namespace GIF_Viewer.Views
{
    partial class FrameExtract
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameExtract));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_close = new System.Windows.Forms.Button();
            this.btn_help = new System.Windows.Forms.Button();
            this.btn_extract = new System.Windows.Forms.Button();
            this.tlc_timeline = new GIF_Viewer.TimelineControl();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cpb_preview = new GIF_Viewer.CPictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cpb_preview)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btn_close);
            this.panel1.Controls.Add(this.btn_help);
            this.panel1.Controls.Add(this.btn_extract);
            this.panel1.Controls.Add(this.tlc_timeline);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 242);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(448, 65);
            this.panel1.TabIndex = 1;
            // 
            // btn_close
            // 
            this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_close.Image = global::GIF_Viewer.Properties.Resources.action_delete;
            this.btn_close.Location = new System.Drawing.Point(368, 36);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 24);
            this.btn_close.TabIndex = 2;
            this.btn_close.Text = "Close";
            this.btn_close.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_close.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // btn_help
            // 
            this.btn_help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_help.Image = global::GIF_Viewer.Properties.Resources.help_browser;
            this.btn_help.Location = new System.Drawing.Point(126, 36);
            this.btn_help.Name = "btn_help";
            this.btn_help.Size = new System.Drawing.Size(24, 24);
            this.btn_help.TabIndex = 2;
            this.btn_help.UseVisualStyleBackColor = true;
            this.btn_help.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btn_extract
            // 
            this.btn_extract.Image = global::GIF_Viewer.Properties.Resources.edit_copy;
            this.btn_extract.Location = new System.Drawing.Point(3, 36);
            this.btn_extract.Name = "btn_extract";
            this.btn_extract.Size = new System.Drawing.Size(120, 24);
            this.btn_extract.TabIndex = 1;
            this.btn_extract.Text = "Extract Frames...";
            this.btn_extract.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_extract.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_extract.UseVisualStyleBackColor = true;
            this.btn_extract.Click += new System.EventHandler(this.btn_extract_Click);
            // 
            // tlc_timeline
            // 
            this.tlc_timeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlc_timeline.BackColor = System.Drawing.SystemColors.Control;
            this.tlc_timeline.Location = new System.Drawing.Point(3, 3);
            this.tlc_timeline.Maximum = 1;
            this.tlc_timeline.Minimum = 0;
            this.tlc_timeline.Name = "tlc_timeline";
            this.tlc_timeline.Range = new System.Drawing.Point(0, 1);
            this.tlc_timeline.ScrollScaleWidth = 1F;
            this.tlc_timeline.ScrollX = 0F;
            this.tlc_timeline.Size = new System.Drawing.Size(440, 28);
            this.tlc_timeline.TabIndex = 0;
            this.tlc_timeline.Text = "timelineControl1";
            this.tlc_timeline.RangeChanged += new GIF_Viewer.TimelineControl.RangeChangedEventHandler(this.tlc_timeline_RangeChangedEvent);
            // 
            // cpb_preview
            // 
            this.cpb_preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cpb_preview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cpb_preview.Location = new System.Drawing.Point(0, 0);
            this.cpb_preview.Name = "cpb_preview";
            this.cpb_preview.Size = new System.Drawing.Size(448, 240);
            this.cpb_preview.TabIndex = 0;
            this.cpb_preview.TabStop = false;
            this.toolTip1.SetToolTip(this.cpb_preview, "Click to pause/unpause GIF playback\r\nRight click to toggle quality");
            this.cpb_preview.DoubleClick += new System.EventHandler(this.cpb_preview_DoubleClick);
            this.cpb_preview.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cpb_preview_MouseClick);
            // 
            // FrameExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 307);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cpb_preview);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrameExtract";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Frame Extract";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrameExtract_KeyDown);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cpb_preview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CPictureBox cpb_preview;
        private System.Windows.Forms.Panel panel1;
        private TimelineControl tlc_timeline;
        private System.Windows.Forms.Button btn_extract;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.Button btn_close;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GIF_Viewer.Utils;

namespace GIF_Viewer
{
    /// <summary>
    /// Represents a form where the user can change the settings of the program
    /// </summary>
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// Temporary buffer memory variable reference
        /// </summary>
        private int bufferMemory;
        /// <summary>
        /// Temporary keyframe memory variable reference
        /// </summary>
        private int keyframeMemory;
        /// <summary>
        /// Temporary keyframe reach variable reference
        /// </summary>
        private int keyframeReach;

        /// <summary>
        /// Initializes a new instance of the SettingsForm class
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();

            this.bufferMemory = Settings.Instance.MaxBufferMemory;
            this.keyframeMemory = Settings.Instance.MaxKeyframeMemory;
            this.keyframeReach = Settings.Instance.MaxKeyframeReach;

            SetTrackbarValueRelative(this.tb_bufferMemory, bufferMemory, 5, 512);
            SetTrackbarValueRelative(this.tb_keyframeMemory, keyframeMemory, 5, 512);
            SetTrackbarValueRelative(this.tb_keyframeReach, keyframeReach, 0, 100);

            this.lbl_bufferMemory.Text = Settings.Instance.MaxBufferMemory + " MB";
            this.lbl_keyframeMemory.Text = Settings.Instance.MaxKeyframeMemory + " MB";
            this.lbl_keyframeReach.Text = Settings.Instance.MaxKeyframeReach + "";
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            Settings.Instance.MaxBufferMemory = bufferMemory;
            Settings.Instance.MaxKeyframeMemory = keyframeMemory;
            Settings.Instance.MaxKeyframeReach = keyframeReach;
            Settings.Instance.SaveSettings();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 
        // Maximum Buffer Memory trackbar scroll
        // 
        private void tb_bufferMemory_Scroll(object sender, EventArgs e)
        {
            this.bufferMemory = GetTrackbarValueRelative(this.tb_bufferMemory, 5, 512);
            this.lbl_bufferMemory.Text = bufferMemory + " MB";
        }

        // 
        // Maximum Buffer Memory trackbar scroll
        // 
        private void tb_keyframeMemory_Scroll(object sender, EventArgs e)
        {
            this.keyframeMemory = GetTrackbarValueRelative(this.tb_keyframeMemory, 5, 512);
            this.lbl_keyframeMemory.Text = keyframeMemory + " MB";
        }

        // 
        // Maximum Buffer Memory trackbar scroll
        // 
        private void tb_keyframeReach_Scroll(object sender, EventArgs e)
        {
            this.keyframeReach = GetTrackbarValueRelative(this.tb_keyframeReach, 0, 100);
            this.lbl_keyframeReach.Text = keyframeReach + "";
        }

        /// <summary>
        /// Updates the value of the given trackbar utilizing the given range of values
        /// </summary>
        /// <param name="bar">The trackbar to update</param>
        /// <param name="value">The value to set on the trackbar</param>
        /// <param name="minValue">The minimum value to scale to</param>
        /// <param name="maxValue">The maximum value to scale to</param>
        private void SetTrackbarValueRelative(TrackBar bar, int value, int minValue, int maxValue)
        {
            float realValue = (float)(value - minValue) / (maxValue - minValue);

            bar.Value = (int)(bar.Minimum + (bar.Maximum - bar.Minimum) * realValue);
        }

        /// <summary>
        /// Gets a value that is the value of the given trackbar scaled to the given range
        /// </summary>
        /// <param name="bar">The trackbar to get the value from</param>
        /// <param name="minValue">The minimum value to scale to</param>
        /// <param name="maxValue">The maximum value to scale to</param>
        /// <returns>A value that ranges from minValue - maxValue based on the given trackbar's progress</returns>
        private int GetTrackbarValueRelative(TrackBar bar, int minValue, int maxValue)
        {
            float realValue = (float)(bar.Value - bar.Minimum) / (bar.Maximum - bar.Minimum);

            return (int)(minValue + (maxValue - minValue) * realValue);
        }
    }
}
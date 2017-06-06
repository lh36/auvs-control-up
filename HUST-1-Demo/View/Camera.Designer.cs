namespace HUST_1_Demo.View
{
    partial class Camera
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
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.OpnVideo = new System.Windows.Forms.Button();
            this.videoSourcePlayer = new AForge.Controls.VideoSourcePlayer();
            this.clsVideo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timer
            // 
            this.timer.Interval = 40;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // OpnVideo
            // 
            this.OpnVideo.Location = new System.Drawing.Point(26, 12);
            this.OpnVideo.Name = "OpnVideo";
            this.OpnVideo.Size = new System.Drawing.Size(95, 42);
            this.OpnVideo.TabIndex = 0;
            this.OpnVideo.Text = "Open video";
            this.OpnVideo.UseVisualStyleBackColor = true;
            this.OpnVideo.Click += new System.EventHandler(this.OpenVideo);
            // 
            // videoSourcePlayer
            // 
            this.videoSourcePlayer.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.videoSourcePlayer.Location = new System.Drawing.Point(26, 74);
            this.videoSourcePlayer.Name = "videoSourcePlayer";
            this.videoSourcePlayer.Size = new System.Drawing.Size(591, 448);
            this.videoSourcePlayer.TabIndex = 1;
            this.videoSourcePlayer.Text = "videoSourcePlayer1";
            this.videoSourcePlayer.VideoSource = null;
            // 
            // clsVideo
            // 
            this.clsVideo.Location = new System.Drawing.Point(148, 12);
            this.clsVideo.Name = "clsVideo";
            this.clsVideo.Size = new System.Drawing.Size(95, 42);
            this.clsVideo.TabIndex = 0;
            this.clsVideo.Text = "Close video";
            this.clsVideo.UseVisualStyleBackColor = true;
            this.clsVideo.Click += new System.EventHandler(this.CloseVideo);
            // 
            // Camera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 534);
            this.Controls.Add(this.videoSourcePlayer);
            this.Controls.Add(this.clsVideo);
            this.Controls.Add(this.OpnVideo);
            this.Name = "Camera";
            this.Text = "Camera";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button OpnVideo;
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private System.Windows.Forms.Button clsVideo;
    }
}
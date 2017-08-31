namespace HUST_1_Demo.View
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.label1 = new System.Windows.Forms.Label();
            this.PassWord = new System.Windows.Forms.TextBox();
            this.BLogin = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.UsrName = new System.Windows.Forms.TextBox();
            this.BQuit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "密码：";
            // 
            // PassWord
            // 
            this.PassWord.Location = new System.Drawing.Point(133, 156);
            this.PassWord.Name = "PassWord";
            this.PassWord.PasswordChar = '*';
            this.PassWord.Size = new System.Drawing.Size(131, 21);
            this.PassWord.TabIndex = 1;
            // 
            // BLogin
            // 
            this.BLogin.Location = new System.Drawing.Point(75, 213);
            this.BLogin.Name = "BLogin";
            this.BLogin.Size = new System.Drawing.Size(75, 34);
            this.BLogin.TabIndex = 2;
            this.BLogin.Text = "登录系统";
            this.BLogin.UseVisualStyleBackColor = true;
            this.BLogin.Click += new System.EventHandler(this.BLogin_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "用户名：";
            // 
            // UsrName
            // 
            this.UsrName.Location = new System.Drawing.Point(133, 115);
            this.UsrName.Name = "UsrName";
            this.UsrName.Size = new System.Drawing.Size(131, 21);
            this.UsrName.TabIndex = 1;
            // 
            // BQuit
            // 
            this.BQuit.Location = new System.Drawing.Point(189, 213);
            this.BQuit.Name = "BQuit";
            this.BQuit.Size = new System.Drawing.Size(75, 34);
            this.BQuit.TabIndex = 2;
            this.BQuit.Text = "退出系统";
            this.BQuit.UseVisualStyleBackColor = true;
            this.BQuit.Click += new System.EventHandler(this.BQuit_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(53, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(243, 28);
            this.label3.TabIndex = 3;
            this.label3.Text = "云端无人艇控制系统登录";
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MediumTurquoise;
            this.ClientSize = new System.Drawing.Size(352, 304);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BQuit);
            this.Controls.Add(this.BLogin);
            this.Controls.Add(this.UsrName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PassWord);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PassWord;
        private System.Windows.Forms.Button BLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox UsrName;
        private System.Windows.Forms.Button BQuit;
        private System.Windows.Forms.Label label3;
    }
}
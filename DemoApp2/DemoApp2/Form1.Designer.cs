namespace DemoApp2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnGenerateLog = new Button();
            SuspendLayout();
            // 
            // btnGenerateLog
            // 
            btnGenerateLog.Location = new Point(322, 180);
            btnGenerateLog.Name = "btnGenerateLog";
            btnGenerateLog.Size = new Size(135, 72);
            btnGenerateLog.TabIndex = 0;
            btnGenerateLog.Text = "GenerateLog";
            btnGenerateLog.UseVisualStyleBackColor = true;
            btnGenerateLog.Click += btnGenerateLog_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnGenerateLog);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private Button btnGenerateLog;
    }
}

namespace DemoApp
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
            btnStartApp = new Button();
            btnLoadFile = new Button();
            btnSaveFile = new Button();
            btnSimulateError = new Button();
            btnTraceOnly = new Button();
            SuspendLayout();
            // 
            // btnStartApp
            // 
            btnStartApp.Location = new Point(110, 140);
            btnStartApp.Name = "btnStartApp";
            btnStartApp.Size = new Size(167, 52);
            btnStartApp.TabIndex = 0;
            btnStartApp.Text = "▶️ Start App";
            btnStartApp.UseVisualStyleBackColor = true;
            btnStartApp.Click += btnStartApp_Click;
            // 
            // btnLoadFile
            // 
            btnLoadFile.Location = new Point(527, 140);
            btnLoadFile.Name = "btnLoadFile";
            btnLoadFile.Size = new Size(167, 52);
            btnLoadFile.TabIndex = 1;
            btnLoadFile.Text = "📁 Load File";
            btnLoadFile.UseVisualStyleBackColor = true;
            btnLoadFile.Click += btnLoadFile_Click;
            // 
            // btnSaveFile
            // 
            btnSaveFile.Location = new Point(318, 140);
            btnSaveFile.Name = "btnSaveFile";
            btnSaveFile.Size = new Size(167, 52);
            btnSaveFile.TabIndex = 2;
            btnSaveFile.Text = "💾 Save File";
            btnSaveFile.UseVisualStyleBackColor = true;
            btnSaveFile.Click += btnSaveFile_Click;
            // 
            // btnSimulateError
            // 
            btnSimulateError.Location = new Point(200, 241);
            btnSimulateError.Name = "btnSimulateError";
            btnSimulateError.Size = new Size(167, 52);
            btnSimulateError.TabIndex = 3;
            btnSimulateError.Text = "🚫 Simulate Error";
            btnSimulateError.UseVisualStyleBackColor = true;
            btnSimulateError.Click += btnSimulateError_Click;
            // 
            // btnTraceOnly
            // 
            btnTraceOnly.Location = new Point(457, 241);
            btnTraceOnly.Name = "btnTraceOnly";
            btnTraceOnly.Size = new Size(167, 52);
            btnTraceOnly.TabIndex = 4;
            btnTraceOnly.Text = "🕵️ Trace Only";
            btnTraceOnly.UseVisualStyleBackColor = true;
            btnTraceOnly.Click += btnTraceOnly_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnTraceOnly);
            Controls.Add(btnSimulateError);
            Controls.Add(btnSaveFile);
            Controls.Add(btnLoadFile);
            Controls.Add(btnStartApp);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private Button btnStartApp;
        private Button btnLoadFile;
        private Button btnSaveFile;
        private Button btnSimulateError;
        private Button btnTraceOnly;
    }
}

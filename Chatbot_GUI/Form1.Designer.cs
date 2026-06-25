namespace Chatbot_GUI
{
    partial class Form1
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
            this.ChatPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.UserInput = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.asciiPanel = new System.Windows.Forms.Panel();
            this.AsciiMini = new System.Windows.Forms.Label();
            this.AsciiLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.CloseButton = new System.Windows.Forms.Button();
            this.TestYourselfButton = new System.Windows.Forms.Button();
            this.asciiPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChatPanel
            // 
            this.ChatPanel.AutoScroll = true;
            this.ChatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChatPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ChatPanel.Location = new System.Drawing.Point(0, 80);
            this.ChatPanel.Name = "ChatPanel";
            this.ChatPanel.Size = new System.Drawing.Size(936, 378);
            this.ChatPanel.TabIndex = 0;
            this.ChatPanel.WrapContents = false;
            // 
            // UserInput
            // 
            this.UserInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserInput.Location = new System.Drawing.Point(100, 0);
            this.UserInput.Name = "UserInput";
            this.UserInput.Size = new System.Drawing.Size(616, 26);
            this.UserInput.TabIndex = 1;
            // 
            // SendButton
            // 
            this.SendButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.SendButton.Location = new System.Drawing.Point(716, 0);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(100, 40);
            this.SendButton.TabIndex = 2;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // asciiPanel
            // 
            this.asciiPanel.Controls.Add(this.AsciiMini);
            this.asciiPanel.Controls.Add(this.AsciiLabel);
            this.asciiPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.asciiPanel.Location = new System.Drawing.Point(0, 0);
            this.asciiPanel.Name = "asciiPanel";
            this.asciiPanel.Size = new System.Drawing.Size(936, 80);
            this.asciiPanel.TabIndex = 2;
            // 
            // AsciiMini
            // 
            this.AsciiMini.BackColor = System.Drawing.Color.Transparent;
            this.AsciiMini.Dock = System.Windows.Forms.DockStyle.Right;
            this.AsciiMini.Font = new System.Drawing.Font("Consolas", 8F);
            this.AsciiMini.ForeColor = System.Drawing.Color.LightGray;
            this.AsciiMini.Location = new System.Drawing.Point(776, 0);
            this.AsciiMini.Name = "AsciiMini";
            this.AsciiMini.Padding = new System.Windows.Forms.Padding(4);
            this.AsciiMini.Size = new System.Drawing.Size(160, 80);
            this.AsciiMini.TabIndex = 0;
            this.AsciiMini.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AsciiLabel
            // 
            this.AsciiLabel.AutoEllipsis = true;
            this.AsciiLabel.BackColor = System.Drawing.Color.Transparent;
            this.AsciiLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AsciiLabel.Font = new System.Drawing.Font("Consolas", 8F);
            this.AsciiLabel.ForeColor = System.Drawing.Color.LightGray;
            this.AsciiLabel.Location = new System.Drawing.Point(0, 0);
            this.AsciiLabel.Name = "AsciiLabel";
            this.AsciiLabel.Padding = new System.Windows.Forms.Padding(6);
            this.AsciiLabel.Size = new System.Drawing.Size(936, 80);
            this.AsciiLabel.TabIndex = 1;
            this.AsciiLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.UserInput);
            this.bottomPanel.Controls.Add(this.SendButton);
            this.bottomPanel.Controls.Add(this.CloseButton);
            this.bottomPanel.Controls.Add(this.TestYourselfButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 458);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(936, 40);
            this.bottomPanel.TabIndex = 1;
            // 
            // CloseButton
            // 
            this.CloseButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.CloseButton.Location = new System.Drawing.Point(0, 0);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(100, 40);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // TestYourselfButton
            // 
            this.TestYourselfButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.TestYourselfButton.Location = new System.Drawing.Point(816, 0);
            this.TestYourselfButton.Name = "TestYourselfButton";
            this.TestYourselfButton.Size = new System.Drawing.Size(120, 40);
            this.TestYourselfButton.TabIndex = 4;
            this.TestYourselfButton.Text = "Test yourself";
            this.TestYourselfButton.Click += new System.EventHandler(this.TestYourselfButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(936, 498);
            this.Controls.Add(this.ChatPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.asciiPanel);
            this.Name = "Form1";
            this.Text = "Cyber Avengers - Chatbot";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.asciiPanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.FlowLayoutPanel ChatPanel;
        private System.Windows.Forms.TextBox UserInput;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.Panel asciiPanel;
        private System.Windows.Forms.Label AsciiLabel;
        private System.Windows.Forms.Label AsciiMini;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button TestYourselfButton;


        #endregion
    }
}


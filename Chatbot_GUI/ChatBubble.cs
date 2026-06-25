using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Chatbot_GUI
{
    public class ChatBubble : UserControl
    {
        private Label lblText;

        private int cornerRadius = 12;

        public string Message
        {
            get => lblText.Text;
            set
            {
                lblText.Text = value ?? string.Empty;
                UpdateSize();
                Invalidate();
            }
        }

        private bool _isUser;
        public bool IsUser
        {
            get => _isUser;
            set
            {
                _isUser = value;
                this.BackColor = _isUser ? Color.LightGreen : Color.LightGray;
                lblText.BackColor = this.BackColor;
                Invalidate();
            }
        }

        public ChatBubble()
        {
            this.DoubleBuffered = true;
            this.Padding = new Padding(10);
            this.Margin = new Padding(6);

            lblText = new Label();
            lblText.AutoSize = false;
            lblText.MaximumSize = new Size(400, 0);
            lblText.ForeColor = Color.Black;
            lblText.BackColor = Color.LightGray;
            lblText.Font = new Font("Segoe UI", 9F);

            this.Controls.Add(lblText);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.MinimumSize = new Size(50, 24);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateSize()
        {
            // Compute max width based on parent if available
            int maxWidth = lblText.MaximumSize.Width;
            if (this.Parent is FlowLayoutPanel fp)
            {
                maxWidth = Math.Max(100, (int)(fp.ClientSize.Width * 0.65));
                lblText.MaximumSize = new Size(maxWidth, 0);
            }

            var measured = TextRenderer.MeasureText(lblText.Text, lblText.Font, new Size(lblText.MaximumSize.Width, int.MaxValue), TextFormatFlags.WordBreak);
            lblText.Size = new Size(measured.Width + 2, measured.Height + 4);

            this.Size = new Size(lblText.Width + this.Padding.Horizontal, lblText.Height + this.Padding.Vertical);
            lblText.Location = new Point(this.Padding.Left, this.Padding.Top);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            var rect = new Rectangle(0, 0, this.Width, this.Height);
            using (var path = RoundedRect(rect, cornerRadius))
            {
                this.Region?.Dispose();
                this.Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(this.BackColor))
            using (var path = RoundedRect(new Rectangle(0, 0, this.Width, this.Height), cornerRadius))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.StartFigure();
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

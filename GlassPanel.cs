using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public class GlassPanel : Panel
    {
        private int _cornerRadius = 15;
        private Color _glassColor = Color.FromArgb(30, 255, 255, 255);
        private Color _borderColor = Color.FromArgb(60, 255, 255, 255);
        private bool _isHovered = false;

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public GlassPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath path = GetRoundedRectPath(rect, _cornerRadius))
            {
                // Glass background
                Color bgColor = _isHovered
                    ? Color.FromArgb(50, 255, 255, 255)
                    : _glassColor;

                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Gradient overlay for depth
                using (LinearGradientBrush gradBrush = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(40, 255, 255, 255),
                    Color.FromArgb(10, 255, 255, 255),
                    90f))
                {
                    g.FillPath(gradBrush, path);
                }

                // Border
                Color borderColor = _isHovered
                    ? Color.FromArgb(100, 255, 255, 255)
                    : _borderColor;

                using (Pen borderPen = new Pen(borderColor, 1.5f))
                {
                    g.DrawPath(borderPen, path);
                }

                // Inner glow
                using (Pen glowPen = new Pen(Color.FromArgb(20, 255, 255, 255), 2))
                {
                    GraphicsPath innerPath = GetRoundedRectPath(
                        new Rectangle(2, 2, Width - 5, Height - 5),
                        _cornerRadius - 2);
                    g.DrawPath(glowPen, innerPath);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public class GlassButton : Button
    {
        private bool _isHovered = false;

        public GlassButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Background
                Color bgColor = _isHovered
                    ? Color.FromArgb(60, 255, 255, 255)
                    : Color.FromArgb(30, 255, 255, 255);

                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Text with shadow
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Shadow
            g.DrawString(Text, Font, new SolidBrush(Color.FromArgb(50, 0, 0, 0)),
                new RectangleF(1, 1, Width, Height), sf);

            // Text
            g.DrawString(Text, Font, new SolidBrush(ForeColor), ClientRectangle, sf);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

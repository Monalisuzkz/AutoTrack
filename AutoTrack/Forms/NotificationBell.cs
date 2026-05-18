using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AutoTrack.Helpers;

namespace AutoTrack.Forms
{
    public class NotificationBell : Control
    {
        private int _count = 0;
        private Timer _pollTimer;

        public int UnreadCount
        {
            get => _count;
            set
            {
                _count = value;
                this.Invalidate();
            }
        }

        public NotificationBell()
        {
            this.Size = new Size(36, 36);
            this.MinimumSize = new Size(36, 36);
            this.MaximumSize = new Size(36, 36);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            _pollTimer = new Timer { Interval = 30000 };
            _pollTimer.Tick += (s, e) => RefreshCount();
            _pollTimer.Start();
        }

        public void RefreshCount()
        {
            try
            {
                int uid = SessionManager.CurrentUser?.UserID ?? 0;
                if (uid == 0) return;
                UnreadCount = NotificationHelper.GetUnreadCount(uid);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to refresh notification count: " + ex);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(this.BackColor);

            int cx = this.Width / 2;
            int cy = this.Height / 2;

            // Filled bell icon in ORANGE
            using (SolidBrush orangeBrush = new SolidBrush(Color.FromArgb(255, 140, 0)))
            using (Pen orangePen = new Pen(Color.FromArgb(255, 140, 0), 2f))
            {
                // Bell body (filled)
                GraphicsPath bellPath = new GraphicsPath();
                Rectangle bellBody = new Rectangle(cx - 10, cy - 8, 20, 16);
                bellPath.AddArc(bellBody, 200, 140);
                bellPath.AddLine(cx - 10, cy + 8, cx + 10, cy + 8);
                bellPath.CloseFigure();

                // Fill bell body
                g.FillPath(orangeBrush, bellPath);

                // Bell sides
                g.DrawLine(orangePen, cx - 10, cy + 8, cx - 10, cy + 4);
                g.DrawLine(orangePen, cx + 10, cy + 8, cx + 10, cy + 4);

                // Bell bottom bar
                g.DrawLine(orangePen, cx - 11, cy + 8, cx + 11, cy + 8);

                // Bell top stem
                g.DrawLine(orangePen, cx, cy - 9, cx, cy - 12);
            }

            // Bell clapper (lighter orange)
            using (SolidBrush clapperBrush = new SolidBrush(Color.FromArgb(255, 180, 80)))
            {
                g.FillEllipse(clapperBrush, cx - 3, cy + 8, 6, 5);
            }

            // Draw red badge with count
            if (_count > 0)
            {
                string countText = _count > 99 ? "99+" : _count.ToString();

                using (Font badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(countText, badgeFont);

                    int badgeW = Math.Max((int)textSize.Width + 8, 18);
                    int badgeH = 16;
                    int badgeX = this.Width - badgeW - 2;
                    int badgeY = 0;

                    // Red circle
                    using (SolidBrush redBrush = new SolidBrush(Color.FromArgb(220, 38, 38)))
                    {
                        g.FillEllipse(redBrush, badgeX, badgeY, badgeW, badgeH);
                    }

                    // White border
                    using (Pen whitePen = new Pen(Color.White, 1.5f))
                    {
                        g.DrawEllipse(whitePen, badgeX, badgeY, badgeW, badgeH);
                    }

                    // Count text
                    using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                    {
                        StringFormat sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(countText, badgeFont, whiteBrush,
                            new RectangleF(badgeX, badgeY, badgeW, badgeH), sf);
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _pollTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}

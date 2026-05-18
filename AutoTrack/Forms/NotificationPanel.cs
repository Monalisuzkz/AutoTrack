using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class NotificationPanel : UserControl
    {
        private FlowLayoutPanel flowNotifications;
        private Panel headerPanel;
        private Button btnMarkAllRead;
        private Label lblTitle, lblCount;
        private Timer refreshTimer;
        private int _userId;

        public NotificationPanel(int userId)
        {
            _userId = userId;
            InitializeComponents();

            this.Load += (s, e) => LoadNotifications();

            refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) => RefreshNewNotifications();
            refreshTimer.Start();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White
            };

            headerPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            // Title
            lblTitle = new Label
            {
                Text = "Notifications",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(15, 14),
                AutoSize = true
            };

            // Count badge
            lblCount = new Label
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(224, 123, 36),
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Make count badge round
            lblCount.Paint += (s, e) =>
            {
                lblCount.BackColor = Color.Transparent;
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, lblCount.Width, lblCount.Height);
                (s as Label).Region = new Region(path);
            };

            // Mark All Read button
            btnMarkAllRead = new Button
            {
                Text = "✓ Mark All Read",
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnMarkAllRead.FlatAppearance.BorderSize = 0;
            btnMarkAllRead.Click += (s, e) => MarkAllRead();


            // Position buttons
            btnMarkAllRead.Location = new Point(headerPanel.Width - 240, 12);

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(btnMarkAllRead);
            headerPanel.Controls.Add(lblCount);

            // Flow layout for notifications
            flowNotifications = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            flowNotifications.Resize += (s, e) =>
            {
                // Only update if there are cards
                if (flowNotifications.Controls.Count > 0)
                {
                    int newCardWidth = flowNotifications.ClientSize.Width - 22;
                    if (newCardWidth > 0)
                    {
                        foreach (Control control in flowNotifications.Controls)
                        {
                            if (control is Panel card && !(control.Tag?.ToString() == "separator"))
                            {
                                card.Width = newCardWidth;

                                // Update child label widths
                                foreach (Control child in card.Controls)
                                {
                                    if (child is Label lbl && lbl.Location.X == 48)
                                    {
                                        lbl.Width = newCardWidth - 70;

                                        // Recalculate message height if it's the message label
                                        if (lbl.Font.Size == 8.5f && lbl.Text.Length > 50)
                                        {
                                            Size newSize = TextRenderer.MeasureText(lbl.Text, lbl.Font,
                                                new Size(lbl.Width, int.MaxValue), TextFormatFlags.WordBreak);
                                            lbl.Height = newSize.Height;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            headerPanel.Resize += (s, e) =>
            {
                btnMarkAllRead.Location = new Point(headerPanel.Width - 130, 12);
            };

            Controls.Add(flowNotifications);
            Controls.Add(headerPanel);
        }

        private void LoadNotifications()
        {
            try
            {
                // Suspend layout to prevent flicker
                flowNotifications.SuspendLayout();

                // Save scroll position
                int savedScrollPosition = 0;
                if (flowNotifications.Controls.Count > 0 && flowNotifications.AutoScrollPosition.Y < 0)
                {
                    savedScrollPosition = Math.Abs(flowNotifications.AutoScrollPosition.Y);
                }

                DataTable dt = NotificationHelper.GetAllNotifications(_userId);
                flowNotifications.Controls.Clear();

                int unreadCount = 0;

                if (dt.Rows.Count == 0)
                {
                    ShowEmptyState();
                    flowNotifications.ResumeLayout();
                    return;
                }

                // Get FIXED card width
                int cardWidth = flowNotifications.ClientSize.Width - 22;
                if (cardWidth <= 0) cardWidth = 500;

                // Group by date
                var grouped = dt.AsEnumerable()
                    .GroupBy(r => Convert.ToDateTime(r["CreatedAt"]).Date)
                    .OrderByDescending(g => g.Key);

                foreach (var group in grouped)
                {
                    string dateLabel = GetDateLabel(group.Key);
                    AddDateSeparator(dateLabel, cardWidth);

                    foreach (DataRow row in group)
                    {
                        int id = Convert.ToInt32(row["NotificationID"]);
                        string title = row["Title"].ToString();
                        string message = row["Message"].ToString();
                        string type = row["NotificationType"].ToString();
                        bool isRead = Convert.ToBoolean(row["IsRead"]);
                        DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

                        if (!isRead) unreadCount++;

                        Panel card = CreateNotificationCard(id, title, message, type, isRead, createdAt, cardWidth);
                        flowNotifications.Controls.Add(card);
                    }
                }

                // Restore scroll position
                if (savedScrollPosition > 0)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        flowNotifications.AutoScrollPosition = new Point(0, savedScrollPosition);
                    }));
                }

                flowNotifications.ResumeLayout();

                // Update badge
                if (unreadCount > 0)
                {
                    lblCount.Text = unreadCount.ToString();
                    lblCount.Visible = true;
                    lblCount.Location = new Point(lblTitle.Right + 10, 18);
                }
                else
                {
                    lblCount.Visible = false;
                }
            }
            catch (Exception ex)
            {
                flowNotifications.ResumeLayout();
                MessageBox.Show("Error loading notifications: " + ex.Message);
            }
        }

        private void AddDateSeparator(string dateLabel, int cardWidth)
        {
            Panel separatorPanel = new Panel
            {
                Width = cardWidth,
                Height = 30,
                Margin = new Padding(0, 5, 0, 5),
                BackColor = Color.Transparent,
                Tag = "Separator",
            };

            Label lblDate = new Label
            {
                Text = dateLabel,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(0, 8),
                AutoSize = true
            };

            Panel line = new Panel
            {
                Location = new Point(lblDate.Right + 10, 18),
                Size = new Size(cardWidth - lblDate.Right - 20, 1),
                BackColor = Color.FromArgb(200, 200, 200)
            };

            separatorPanel.Controls.Add(lblDate);
            separatorPanel.Controls.Add(line);
            flowNotifications.Controls.Add(separatorPanel);
        }

        private Panel CreateNotificationCard(int id, string title, string message, string type, bool isRead, DateTime createdAt, int cardWidth)
        {
            // Color bar
            Color accentColor = GetTypeColor(type);

            // Icon
            Label lblIcon = new Label
            {
                Text = GetTypeIcon(type),
                Font = new Font("Segoe UI", 14f),
                ForeColor = accentColor,
                Location = new Point(12, 12),
                AutoSize = true
            };

            // Title
            Label lblTitle2 = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10f, isRead ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(48, 10),
                AutoSize = false,
                Width = cardWidth - 70
            };

            // Message
            Label lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(48, 34),
                AutoSize = false,
                Width = cardWidth - 70
            };

            // Calculate message height
            Size msgSize = TextRenderer.MeasureText(
                message,
                lblMessage.Font,
                new Size(lblMessage.Width, int.MaxValue),
                TextFormatFlags.WordBreak);
            lblMessage.Height = msgSize.Height;

            // Time
            Label lblTime = new Label
            {
                Text = GetTimeAgo(createdAt),
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(48, 34 + lblMessage.Height + 6),
                AutoSize = true
            };

            // Card height
            int cardHeight = 34 + lblMessage.Height + 6 + 20 + 10;

            Panel card = new Panel
            {
                Width = cardWidth,
                Height = cardHeight,
                Margin = new Padding(0, 4, 0, 4),
                BackColor = isRead ? Color.White : Color.FromArgb(255, 252, 245),
                Cursor = Cursors.Hand
            };

            // Color bar
            Panel colorBar = new Panel
            {
                Width = 4,
                Height = cardHeight,
                BackColor = accentColor,
                Dock = DockStyle.Left
            };

            // Border
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
                    Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid);
            };

            // Click handlers
            card.Click += (s, e) => OpenNotification(id);
            lblTitle2.Click += (s, e) => OpenNotification(id);
            lblMessage.Click += (s, e) => OpenNotification(id);

            card.Controls.Add(colorBar);
            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle2);
            card.Controls.Add(lblMessage);
            card.Controls.Add(lblTime);

            return card;
        }

        private void RefreshNewNotifications()
        {
            try
            {
                // Get only unread notifications from last 5 minutes
                DataTable newDt = DatabaseHelper.ExecuteQuery(@"
                    SELECT * FROM Notifications 
                    WHERE UserID = @UserID 
                    AND IsRead = 0
                    AND CreatedAt > DATEADD(MINUTE, -5, GETDATE())
                    ORDER BY CreatedAt DESC",
                    new[] { new SqlParameter("@UserID", _userId) });

                if (newDt.Rows.Count > 0)
                {
                    // Reload all to keep order correct
                    LoadNotifications();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Refresh error: " + ex.Message);
            }
        }

        private void ShowEmptyState()
        {
            Panel emptyPanel = new Panel
            {
                Width = flowNotifications.ClientSize.Width - 22,
                Height = 300,
                Margin = new Padding(0, 50, 0, 0)
            };

            Label lblEmpty = new Label
            {
                Text = "🔔\n\nNo notifications yet",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(150, 150, 150),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            emptyPanel.Controls.Add(lblEmpty);
            flowNotifications.Controls.Add(emptyPanel);
        }

        private string GetDateLabel(DateTime date)
        {
            if (date == DateTime.Today) return "TODAY";
            if (date == DateTime.Today.AddDays(-1)) return "YESTERDAY";
            if (date >= DateTime.Today.AddDays(-7)) return date.ToString("dddd").ToUpper();
            return date.ToString("MMMM dd, yyyy").ToUpper();
        }

        private Color GetTypeColor(string type)
        {
            switch (type)
            {
                case "Alert": return Color.FromArgb(220, 53, 69);
                case "Warning": return Color.FromArgb(255, 193, 7);
                case "Success": return Color.FromArgb(40, 167, 69);
                default: return Color.FromArgb(0, 123, 255);
            }
        }

        private string GetTypeIcon(string type)
        {
            switch (type)
            {
                case "Alert": return "⚠️";
                case "Warning": return "⚠️";
                case "Success": return "✓";
                default: return "🔔";
            }
        }

        private string GetTimeAgo(DateTime date)
        {
            var diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hr ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} day{(diff.TotalDays >= 2 ? "s" : "")} ago";
            return date.ToString("MMM dd");
        }

        private void OpenNotification(int notificationId)
        {
            try
            {
                NotificationHelper.MarkAsRead(notificationId);
                LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void MarkAllRead()
        {
            if (MessageBox.Show("Mark all notifications as read?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                NotificationHelper.MarkAllAsRead(_userId);
                LoadNotifications();
            }
        }
    }
}
using System.Drawing.Drawing2D;
using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public partial class MainForm : BaseForm
    {
        private Button _activeButton = null;
        private Button notificationBell;
        private Timer notificationTimer;
        private int currentYPosition = 0;
        private bool isNotificationOpen = false;

        public MainForm()
        {
            InitializeComponent();
            LoadUserInfo();
            ApplyRoleVisibility();
            ShowPanel(panelDashboard, btnDashboard);
            LoadRoleSpecificDashboard();
            ShowLoginSuccessMessage();
        }

        // -------------------------------------------------------
        // Show login success message with user info
        // -------------------------------------------------------
        private void ShowLoginSuccessMessage()
        {
            string fullName = SessionManager.CurrentUser?.FullName ?? "User";
            string role = SessionManager.CurrentUser?.Role ?? "";

            MessageBox.Show(
                $"Welcome, {fullName}!\n\nYou are logged in as: {role}\n",
                "Login Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetupNotificationBell()
        {
            try
            {
                if (SessionManager.CurrentUser == null) return;

                foreach (Control ctrl in panelTopBar.Controls)
                {
                    if (ctrl is Button btn && btn.Text == "🔔")
                    {
                        notificationBell = btn;
                        // REMOVE this line if the click is already assigned in designer:
                        // notificationBell.Click += BtnNotifications_Click;
                        UpdateNotificationBadge();
                        break;
                    }
                }

                notificationTimer = new Timer();
                notificationTimer.Interval = 30000;
                notificationTimer.Tick += (s, e) => UpdateNotificationBadge();
                notificationTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }

        private void UpdateNotificationBadge()
        {
            try
            {
                if (SessionManager.CurrentUser == null || notificationBell == null) return;

                int unreadCount = NotificationHelper.GetUnreadCount(SessionManager.CurrentUser.UserID);

                if (notificationBell.InvokeRequired)
                {
                    notificationBell.Invoke(new Action(() => UpdateNotificationBadge()));
                    return;
                }

                if (unreadCount > 0)
                {
                    notificationBell.Text = $"🔔 {unreadCount}";
                    notificationBell.ForeColor = Color.FromArgb(255, 140, 0);
                    notificationBell.BackColor = Color.Transparent;
                    notificationBell.Size = new Size(60, 36);
                    notificationBell.TextAlign = ContentAlignment.MiddleCenter;
                }
                else
                {
                    notificationBell.Text = "🔔";
                    notificationBell.ForeColor = Color.FromArgb(255, 140, 0);
                    notificationBell.BackColor = Color.Transparent;
                    notificationBell.Size = new Size(36, 36);
                    notificationBell.TextAlign = ContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }

        private void BtnNotifications_Click(object sender, EventArgs e)
        {
            try
            {
                if (isNotificationOpen) return;
                if (SessionManager.CurrentUser == null) return;

                isNotificationOpen = true;

                var notificationPanel = new NotificationPanel(SessionManager.CurrentUser.UserID);
                var form = new Form
                {
                    Text = "Notifications",
                    Size = new Size(850, 500),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                notificationPanel.Dock = DockStyle.Fill;
                form.Controls.Add(notificationPanel);

                form.FormClosed += (s, args) =>
                {
                    // Small delay to prevent any race conditions
                    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                    timer.Interval = 100;
                    timer.Tick += (t, ev) =>
                    {
                        isNotificationOpen = false;
                        UpdateNotificationBadge();
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                };

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                isNotificationOpen = false;
            }
        }

        // -------------------------------------------------------
        // Apply role-based visibility for sidebar buttons
        // -------------------------------------------------------
        private void ApplyRoleVisibility()
        {
            string role = SessionManager.CurrentUser?.Role ?? "";

            HideAllSidebarButtons();
            btnDashboard.Visible = true;

            // Make case-insensitive comparison
            switch (role.ToLower())
            {
                case "superadmin":
                    ShowSuperAdminMenu();
                    break;
                case "admin":
                    ShowAdminMenu();
                    break;
                case "staff":
                    ShowStaffMenu();
                    break;
                case "technician":
                    ShowTechnicianMenu();
                    break;
                case "supplier":
                    ShowSupplierMenu();
                    break;
                default:
                    ShowDefaultMenu();
                    break;
            }

            RepositionSidebarButtons();
        }

        private void HideAllSidebarButtons()
        {
            btnDashboard.Visible = false;
            btnCustomers.Visible = false;
            btnVehicles.Visible = false;
            btnServices.Visible = false;
            btnTechnicians.Visible = false;
            btnInventory.Visible = false;
            btnSuppliers.Visible = false;
            btnPayments.Visible = false;
            btnReports.Visible = false;
            btnRestockRequests.Visible = false;
            btnUsers.Visible = false;
            btnSubscriptions.Visible = false;
            btnArchivedItems.Visible = false;
        }

        private void ShowSuperAdminMenu()
        {
            btnCustomers.Visible = true;
            btnVehicles.Visible = true;
            btnServices.Visible = true;
            btnTechnicians.Visible = true;
            btnInventory.Visible = true;
            btnSuppliers.Visible = true;
            btnPayments.Visible = true;
            btnReports.Visible = true;
            btnRestockRequests.Visible = true;
            btnUsers.Visible = true;
            btnSubscriptions.Visible = true;
            btnArchivedItems.Visible = true;
        }

        private void ShowAdminMenu()
        {
            btnCustomers.Visible = true;
            btnVehicles.Visible = true;
            btnServices.Visible = true;
            btnTechnicians.Visible = true;
            btnInventory.Visible = true;
            btnSuppliers.Visible = true;
            btnPayments.Visible = true;
            btnRestockRequests.Visible = true;
            btnSubscriptions.Visible = true;
            btnReports.Visible = true;
        }

        private void ShowStaffMenu()
        {
            btnCustomers.Visible = true;
            btnVehicles.Visible = true;
            btnServices.Visible = true;
            btnInventory.Visible = true;
            btnPayments.Visible = true;
        }

        private void ShowTechnicianMenu()
        {
            btnServices.Visible = true;
        }

        private void ShowSupplierMenu()
        {
            btnRestockRequests.Visible = true;
        }

        private void ShowDefaultMenu()
        {
            btnServices.Visible = true;
        }

        private void RepositionSidebarButtons()
        {
            panelSidebar.Controls.Clear();

            int currentY = 10;
            int labelHeight = 18;
            int buttonHeight = 36;
            int spacing = 2;
            int sectionSpacing = 8;

            // MENU SECTION
            lblNavMain.Text = "MENU";
            lblNavMain.Location = new Point(16, currentY);
            lblNavMain.Size = new Size(178, labelHeight);
            lblNavMain.Visible = true;
            panelSidebar.Controls.Add(lblNavMain);
            currentY += labelHeight + spacing;

            btnDashboard.Text = "⬛ Dashboard";
            btnDashboard.Location = new Point(0, currentY);
            btnDashboard.Size = new Size(210, buttonHeight);
            btnDashboard.Visible = true;
            panelSidebar.Controls.Add(btnDashboard);
            currentY += buttonHeight + sectionSpacing;

            // MANAGEMENT SECTION
            bool hasManagement = btnCustomers.Visible || btnVehicles.Visible || btnUsers.Visible;
            if (hasManagement)
            {
                lblNavManagement.Text = "MANAGEMENT";
                lblNavManagement.Location = new Point(16, currentY);
                lblNavManagement.Size = new Size(178, labelHeight);
                lblNavManagement.Visible = true;
                panelSidebar.Controls.Add(lblNavManagement);
                currentY += labelHeight + spacing;

                if (btnUsers.Visible) { btnUsers.Location = new Point(0, currentY); btnUsers.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnUsers); currentY += buttonHeight + spacing; }
                if (btnCustomers.Visible) { btnCustomers.Location = new Point(0, currentY); btnCustomers.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnCustomers); currentY += buttonHeight + spacing; }
                if (btnVehicles.Visible) { btnVehicles.Location = new Point(0, currentY); btnVehicles.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnVehicles); currentY += buttonHeight + sectionSpacing; }
            }
            else { lblNavManagement.Visible = false; }

            // SERVICE SECTION
            bool hasService = btnServices.Visible || btnTechnicians.Visible || btnPayments.Visible;
            if (hasService)
            {
                lblNavService.Text = "SERVICE";
                lblNavService.Location = new Point(16, currentY);
                lblNavService.Size = new Size(178, labelHeight);
                lblNavService.Visible = true;
                panelSidebar.Controls.Add(lblNavService);
                currentY += labelHeight + spacing;

                if (btnServices.Visible) { btnServices.Location = new Point(0, currentY); btnServices.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnServices); currentY += buttonHeight + spacing; }
                if (btnTechnicians.Visible) { btnTechnicians.Location = new Point(0, currentY); btnTechnicians.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnTechnicians); currentY += buttonHeight + spacing; }
                if (btnPayments.Visible) { btnPayments.Location = new Point(0, currentY); btnPayments.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnPayments); currentY += buttonHeight + sectionSpacing; }
            }
            else { lblNavService.Visible = false; }

            // INVENTORY SECTION
            bool hasInventory = btnInventory.Visible || btnArchivedItems.Visible || btnRestockRequests.Visible || btnSuppliers.Visible;
            if (hasInventory)
            {
                lblNavInventory.Text = "INVENTORY";
                lblNavInventory.Location = new Point(16, currentY);
                lblNavInventory.Size = new Size(178, labelHeight);
                lblNavInventory.Visible = true;
                panelSidebar.Controls.Add(lblNavInventory);
                currentY += labelHeight + spacing;

                if (btnInventory.Visible) { btnInventory.Location = new Point(0, currentY); btnInventory.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnInventory); currentY += buttonHeight + spacing; }
                if (btnArchivedItems.Visible) { btnArchivedItems.Location = new Point(0, currentY); btnArchivedItems.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnArchivedItems); currentY += buttonHeight + spacing; }
                if (btnRestockRequests.Visible) { btnRestockRequests.Location = new Point(0, currentY); btnRestockRequests.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnRestockRequests); currentY += buttonHeight + spacing; }
                if (btnSuppliers.Visible) { btnSuppliers.Location = new Point(0, currentY); btnSuppliers.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnSuppliers); currentY += buttonHeight + sectionSpacing; }
            }
            else { lblNavInventory.Visible = false; }

            // ANALYTICS SECTION
            bool hasAnalytics = btnReports.Visible || btnSubscriptions.Visible;
            if (hasAnalytics)
            {
                lblNavAnalytics.Text = "ANALYTICS";
                lblNavAnalytics.Location = new Point(16, currentY);
                lblNavAnalytics.Size = new Size(178, labelHeight);
                lblNavAnalytics.Visible = true;
                panelSidebar.Controls.Add(lblNavAnalytics);
                currentY += labelHeight + spacing;

                if (btnReports.Visible) { btnReports.Location = new Point(0, currentY); btnReports.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnReports); currentY += buttonHeight + spacing; }
                if (btnSubscriptions.Visible) { btnSubscriptions.Location = new Point(0, currentY); btnSubscriptions.Size = new Size(210, buttonHeight); panelSidebar.Controls.Add(btnSubscriptions); currentY += buttonHeight + sectionSpacing; }
            }
            else { lblNavAnalytics.Visible = false; }

            panelSidebar.Refresh();
        }

        // -------------------------------------------------------
        // Load logged-in user info into top bar
        // -------------------------------------------------------
        private void LoadUserInfo()
        {
            lblUserName.Text = SessionManager.CurrentUser?.FullName ?? "User";
            lblUserRole.Text = SessionManager.CurrentUser?.Role ?? "";
        }

        // -------------------------------------------------------
        // Sidebar navigation — switch panels
        // -------------------------------------------------------
        private void ShowPanel(Panel panel, Button btn)
        {
            panelDashboard.Visible = false;
            panelCustomers.Visible = false;
            panelVehicles.Visible = false;
            panelServices.Visible = false;
            panelTechnicians.Visible = false;
            panelInventory.Visible = false;
            panelSuppliers.Visible = false;
            panelPayments.Visible = false;
            panelReports.Visible = false;
            panelRestockRequests.Visible = false;
            panelUsers.Visible = false;
            panelSubscriptions.Visible = false;
            panelArchivedItems.Visible = false;

            panel.Visible = true;

            if (_activeButton != null)
            {
                _activeButton.BackColor = Color.FromArgb(30, 30, 30);
                _activeButton.ForeColor = Color.FromArgb(180, 180, 180);
            }

            btn.BackColor = Color.FromArgb(224, 123, 36);
            btn.ForeColor = Color.White;
            _activeButton = btn;
        }

        private void LoadRoleSpecificDashboard()
        {
            string role = SessionManager.CurrentUser?.Role ?? "";

            // For SuperAdmin and Admin - show the full dashboard from designer
            if (role == "SuperAdmin" || role == "Admin")
            {
                // Clear any role-specific dashboard if present
                foreach (Control ctrl in panelDashboard.Controls)
                {
                    if (ctrl is DashboardPanel)
                    {
                        panelDashboard.Controls.Remove(ctrl);
                        ctrl.Dispose();
                        break;
                    }
                }

                // Make sure designer controls are visible
                SetDesignerControlsVisibility(true);

                // Load data into existing controls
                LoadDashboardData();

                // Create or update charts
                CreateOrUpdateCharts();

                // Set height for SuperAdmin/Admin dashboard
                panelDashboard.AutoScrollMinSize = new Size(0, 1035);
            }
            else
            {
                // Hide designer controls
                SetDesignerControlsVisibility(false);

                // Clear any existing dashboard panel
                foreach (Control ctrl in panelDashboard.Controls)
                {
                    if (ctrl is DashboardPanel)
                    {
                        panelDashboard.Controls.Remove(ctrl);
                        ctrl.Dispose();
                        break;
                    }
                }

                // Show role-specific simplified dashboard
                DashboardPanel dashboard = null;

                switch (role)
                {
                    case "Staff":
                        dashboard = new StaffDashboard();
                        panelDashboard.AutoScrollMinSize = new Size(0, 750);  // Staff height
                        break;
                    case "Technician":
                        dashboard = new TechnicianDashboard();
                        panelDashboard.AutoScrollMinSize = new Size(0, 700);  // Technician height
                        break;
                    case "Supplier":
                        dashboard = new SupplierDashboard();
                        panelDashboard.AutoScrollMinSize = new Size(0, 750);  // Supplier height
                        break;
                    default:
                        SetDesignerControlsVisibility(true);
                        LoadDashboardData();
                        panelDashboard.AutoScrollMinSize = new Size(0, 1100);
                        return;
                }

                if (dashboard != null)
                {
                    dashboard.Dock = DockStyle.Fill;
                    panelDashboard.Controls.Add(dashboard);
                    dashboard.LoadDashboard();
                }
            }
        }

        private void SetDesignerControlsVisibility(bool visible)
        {
            // Set visibility of all designer-added controls
            lblDashTitle.Visible = visible;
            btnRefreshDashboard.Visible = visible;
            pnlStatActive.Visible = visible;
            pnlStatCompleted.Visible = visible;
            pnlStatCustomers.Visible = visible;
            pnlStatVehicles.Visible = visible;
            lblRecentTitle.Visible = visible;
            dgvRecentServices.Visible = visible;
            lblPaymentsTitle.Visible = visible;
            dgvRecentPayments.Visible = visible;

            // Position refresh button
            if (visible)
            {
                btnRefreshDashboard.Location = new Point(panelDashboard.Width - btnRefreshDashboard.Width - 20, 4);
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                object todayRevenue = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TotalAmount), 0) FROM Payments WHERE CAST(PaymentDate AS DATE) = CAST(GETDATE() AS DATE)");
                lblStatActive.Text = $"₱{Convert.ToDecimal(todayRevenue):N0}";

                object monthlyRevenue = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TotalAmount), 0) FROM Payments WHERE MONTH(PaymentDate) = MONTH(GETDATE()) AND YEAR(PaymentDate) = YEAR(GETDATE())");
                lblStatCompleted.Text = $"₱{Convert.ToDecimal(monthlyRevenue):N0}";

                object activeCount = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM ServiceRecords WHERE Status IN ('Pending','InProgress')");
                lblStatCustomers.Text = activeCount?.ToString() ?? "0";

                object completedCount = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM ServiceRecords WHERE Status = 'Completed' AND CAST(DateCompleted AS DATE) = CAST(GETDATE() AS DATE)");
                lblStatVehicles.Text = completedCount?.ToString() ?? "0";

                DataTable dtServices = DatabaseHelper.ExecuteQuery(@"
            SELECT TOP 8
                sr.JobOrderNo AS [Job #],
                v.PlateNumber AS [Plate],
                v.Make + ' ' + v.Model AS [Vehicle],
                sr.ServiceType AS [Service],
                sr.Status AS [Status],
                CONVERT(VARCHAR, sr.DateIn, 107) AS [Date In],
                sr.FinalAmount AS [Amount]
            FROM ServiceRecords sr
            LEFT JOIN Vehicles v ON sr.VehicleID = v.VehicleID
            ORDER BY sr.CreatedAt DESC");

                dgvRecentServices.DataSource = null;
                dgvRecentServices.DataSource = dtServices;

                DataTable dtPayments = DatabaseHelper.ExecuteQuery(@"
            SELECT TOP 8
                p.ReceiptNo AS [Receipt],
                sr.JobOrderNo AS [Job #],
                c.FirstName + ' ' + c.LastName AS [Customer],
                v.PlateNumber AS [Plate],
                p.TotalAmount AS [Amount],
                CONVERT(VARCHAR, p.PaymentDate, 107) AS [Date]
            FROM Payments p
            JOIN ServiceRecords sr ON p.ServiceID = sr.ServiceID
            JOIN Vehicles v ON sr.VehicleID = v.VehicleID
            JOIN Customers c ON v.CustomerID = c.CustomerID
            ORDER BY p.PaymentDate DESC");

                dgvRecentPayments.DataSource = null;
                dgvRecentPayments.DataSource = dtPayments;

                if (dgvRecentServices.Columns.Contains("Amount"))
                {
                    dgvRecentServices.Columns["Amount"].DefaultCellStyle.Format = "₱#,##0.00";
                    dgvRecentServices.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                if (dgvRecentPayments.Columns.Contains("Amount"))
                {
                    dgvRecentPayments.Columns["Amount"].DefaultCellStyle.Format = "₱#,##0.00";
                    dgvRecentPayments.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard: " + ex.Message);
            }
        }

        private void CreateOrUpdateCharts()
        {
            // Check if chart panel already exists
            if (panelDashboard.Controls.ContainsKey("chartPanel"))
            {
                var chartPanel = panelDashboard.Controls["chartPanel"];
                // Update existing chart position
                chartPanel.Location = new Point(0, 160);
                chartPanel.Width = panelDashboard.Width - 40;
                chartPanel.BringToFront();

                int targetIndex = 0;
                for (int i = 0; i < panelDashboard.Controls.Count; i++)
                {
                    if (panelDashboard.Controls[i] == lblRecentTitle)
                    {
                        targetIndex = i;
                        break;
                    }
                }
                panelDashboard.Controls.SetChildIndex(chartPanel, targetIndex);
                chartPanel.Invalidate();
                foreach (Control ctrl in chartPanel.Controls)
                {
                    ctrl.Invalidate();
                }
            }
            else
            {
                CreateDashboardCharts();
            }
        }

        private void CreateDashboardCharts()
        {
            Panel chartPanel = new Panel
            {
                Name = "chartPanel",
                Height = 290,
                Width = panelDashboard.Width - 40,
                BackColor = Color.FromArgb(245, 245, 245),
                Location = new Point(0, 160)
            };

            Panel revenueChart = new Panel
            {
                Location = new Point(20, 10),
                Size = new Size(550, 260),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblRevenueTitle = new Label
            {
                Text = "📈 Weekly Revenue (Last 7 Days)",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(10, 8),
                AutoSize = true
            };
            revenueChart.Controls.Add(lblRevenueTitle);

            Panel serviceChart = new Panel
            {
                Location = new Point(590, 10),
                Size = new Size(350, 260),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblServiceTitle = new Label
            {
                Text = "📊 Service Status Distribution",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(10, 8),
                AutoSize = true
            };
            serviceChart.Controls.Add(lblServiceTitle);

            revenueChart.Paint += (s, e) => DrawRevenueChart(e.Graphics, revenueChart);
            serviceChart.Paint += (s, e) => DrawServicePieChart(e.Graphics, serviceChart);

            chartPanel.Controls.Add(revenueChart);
            chartPanel.Controls.Add(serviceChart);
            panelDashboard.Controls.Add(chartPanel);

            // Position chart panel before the Recent Services table
            int targetIndex = 0;
            for (int i = 0; i < panelDashboard.Controls.Count; i++)
            {
                if (panelDashboard.Controls[i] == lblRecentTitle)
                {
                    targetIndex = i;
                    break;
                }
            }
            panelDashboard.Controls.SetChildIndex(chartPanel, targetIndex);
            chartPanel.Invalidate();
        }

        private void DrawRevenueChart(Graphics g, Panel panel)
        {
            int[] weeklyRevenue = new int[7];
            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        DATEPART(dw, PaymentDate) as WeekDay,
                        ISNULL(SUM(TotalAmount), 0) as DailyRevenue
                    FROM Payments
                    WHERE PaymentDate >= DATEADD(day, -7, GETDATE())
                    GROUP BY DATEPART(dw, PaymentDate)");

                foreach (DataRow row in dt.Rows)
                {
                    int day = (Convert.ToInt32(row["WeekDay"]) + 5) % 7;
                    if (day >= 0 && day < 7)
                        weeklyRevenue[day] = Convert.ToInt32(row["DailyRevenue"]);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to load weekly revenue chart data: " + ex);
            }

            int maxRevenue = weeklyRevenue.Length > 0 ? Math.Max(1, weeklyRevenue.Max()) : 1;
            int barWidth = (panel.Width - 80) / 7 - 8;
            int chartBottom = panel.Height - 40;

            for (int i = 0; i < 7; i++)
            {
                int barHeight = (weeklyRevenue[i] * (panel.Height - 80)) / maxRevenue;
                int x = 35 + i * (barWidth + 10);
                int y = chartBottom - barHeight;
                if (barHeight < 0) barHeight = 0;

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(52, 152, 219)))
                {
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                {
                    g.DrawString(days[i], new Font("Segoe UI", 8f), brush, x + 5, chartBottom + 5);
                }

                if (weeklyRevenue[i] > 0)
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
                    {
                        g.DrawString($"₱{weeklyRevenue[i]:N0}", new Font("Segoe UI", 7f), brush, x + 2, y - 12);
                    }
                }
            }
        }

        private void DrawServicePieChart(Graphics g, Panel panel)
        {
            int pending = 0, inProgress = 0, completed = 0;

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as Pending,
                        SUM(CASE WHEN Status = 'InProgress' THEN 1 ELSE 0 END) as InProgress,
                        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as Completed
                    FROM ServiceRecords");

                if (dt.Rows.Count > 0)
                {
                    pending = Convert.ToInt32(dt.Rows[0]["Pending"]);
                    inProgress = Convert.ToInt32(dt.Rows[0]["InProgress"]);
                    completed = Convert.ToInt32(dt.Rows[0]["Completed"]);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to load service pie chart data: " + ex);
            }

            int total = pending + inProgress + completed;
            if (total == 0) total = 1;

            int centerX = panel.Width / 2;
            int centerY = panel.Height / 2 + 20;
            int radius = 80;

            float pendingAngle = 360f * pending / total;
            float inProgressAngle = 360f * inProgress / total;
            float completedAngle = 360f * completed / total;

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(241, 196, 15)))
            {
                g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, 0, pendingAngle);
            }

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, 126, 34)))
            {
                g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, pendingAngle, inProgressAngle);
            }

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(46, 204, 113)))
            {
                g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, pendingAngle + inProgressAngle, completedAngle);
            }

            int legendX = panel.Width - 130;
            int legendY = 40;

            DrawLegendItem(g, legendX, legendY, "Pending", Color.FromArgb(241, 196, 15), pending);
            DrawLegendItem(g, legendX, legendY + 25, "In Progress", Color.FromArgb(230, 126, 34), inProgress);
            DrawLegendItem(g, legendX, legendY + 50, "Completed", Color.FromArgb(46, 204, 113), completed);

            string totalText = total.ToString();
            SizeF textSize = g.MeasureString(totalText, new Font("Segoe UI", 14f, FontStyle.Bold));
            g.DrawString(totalText, new Font("Segoe UI", 14f, FontStyle.Bold), Brushes.White,
                centerX - textSize.Width / 2, centerY - 10);
            g.DrawString("Total", new Font("Segoe UI", 8f), Brushes.White,
                centerX - 15, centerY + 10);
        }

        private void DrawLegendItem(Graphics g, int x, int y, string label, Color color, int count)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, x, y, 12, 12);
            }

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            {
                g.DrawString($"{label}: {count}", new Font("Segoe UI", 8f), brush, x + 18, y);
            }
        }

        // -------------------------------------------------------
        // Button Click Events
        // -------------------------------------------------------
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ShowPanel(panelDashboard, btnDashboard);
            LoadRoleSpecificDashboard();
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Technician" || role == "Supplier")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Customers.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelCustomers, btnCustomers);
            try
            {
                panelCustomers.Controls.Clear();
                CustomerPanel cp = new CustomerPanel();
                cp.Dock = DockStyle.Fill;
                panelCustomers.Controls.Add(cp);
                cp.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading customers panel: " + ex.Message); }
        }

        private void btnVehicles_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Technician" || role == "Supplier")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Vehicles.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelVehicles, btnVehicles);
            try
            {
                panelVehicles.Controls.Clear();
                VehiclePanel vp = new VehiclePanel();
                vp.Dock = DockStyle.Fill;
                panelVehicles.Controls.Add(vp);
                vp.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading vehicles panel: " + ex.Message); }
        }

        private void btnServices_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Supplier")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Service Records.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelServices, btnServices);
            try
            {
                panelServices.Controls.Clear();
                var sp = new ServiceRecordsPanel();
                sp.Dock = DockStyle.Fill;
                sp.SetUserRole(role);
                sp.SetUserId(SessionManager.CurrentUser?.UserID ?? 0);
                panelServices.Controls.Add(sp);
                sp.BringToFront();
            }
            catch (Exception ex) { ShowError("services", ex); }
        }

        private void btnTechnicians_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role != "SuperAdmin" && role != "Admin")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Technicians.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelTechnicians, btnTechnicians);
            try
            {
                panelTechnicians.Controls.Clear();
                TechniciansPanel tp = new TechniciansPanel();
                tp.Dock = DockStyle.Fill;
                panelTechnicians.Controls.Add(tp);
                tp.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading technicians panel: " + ex.Message); }
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Technician")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Inventory.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelInventory, btnInventory);
            try
            {
                panelInventory.Controls.Clear();
                var ip = new InventoryPanel();
                ip.Dock = DockStyle.Fill;
                ip.SetUserRole(role);
                ip.SetUserId(SessionManager.CurrentUser?.UserID ?? 0);
                panelInventory.Controls.Add(ip);
                ip.BringToFront();
            }
            catch (Exception ex) { ShowError("inventory", ex); }
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Staff" || role == "Technician")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Suppliers.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelSuppliers, btnSuppliers);
            try
            {
                panelSuppliers.Controls.Clear();
                var sp = new SuppliersPanel();
                sp.Dock = DockStyle.Fill;
                if (role == "Supplier")
                {
                    sp.SetSupplierMode(true);
                    sp.SetUserId(SessionManager.CurrentUser?.UserID ?? 0);
                }
                panelSuppliers.Controls.Add(sp);
                sp.BringToFront();
            }
            catch (Exception ex) { ShowError("suppliers", ex); }
        }

        private void btnPayments_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Technician" || role == "Supplier")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Payments.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelPayments, btnPayments);
            try
            {
                panelPayments.Controls.Clear();
                PaymentsPanel pp = new PaymentsPanel();
                pp.Dock = DockStyle.Fill;
                panelPayments.Controls.Add(pp);
                pp.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading payments panel: " + ex.Message); }
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role != "SuperAdmin" && role != "Admin")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Reports.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelReports, btnReports);
            try
            {
                panelReports.Controls.Clear();
                ReportsPanel rp = new ReportsPanel();
                rp.Dock = DockStyle.Fill;
                panelReports.Controls.Add(rp);
                rp.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading reports panel: " + ex.Message); }
        }

        private void btnRestockRequests_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Staff" || role == "Technician")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Restock Requests.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelRestockRequests, btnRestockRequests);
            try
            {
                panelRestockRequests.Controls.Clear();
                var rp = new RestockRequestsPanel();
                rp.Dock = DockStyle.Fill;
                panelRestockRequests.Controls.Add(rp);
                rp.BringToFront();
            }
            catch (Exception ex) { ShowError("restock requests", ex); }
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role != "SuperAdmin")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access User Management.\n\nOnly SuperAdmin can manage users.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelUsers, btnUsers);
            try
            {
                panelUsers.Controls.Clear();
                UsersPanel up = new UsersPanel();
                up.Dock = DockStyle.Fill;
                panelUsers.Controls.Add(up);
                up.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Error loading users panel: " + ex.Message); }
        }

        private void btnSubscriptions_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role != "SuperAdmin" && role != "Admin")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Subscriptions.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelSubscriptions, btnSubscriptions);
            try
            {
                panelSubscriptions.Controls.Clear();
                var sp = new SubscriptionsPanel();
                sp.Dock = DockStyle.Fill;
                panelSubscriptions.Controls.Add(sp);
                sp.BringToFront();
            }
            catch (Exception ex) { ShowError("subscriptions", ex); }
        }

        private void btnArchivedItems_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            if (role == "Staff" || role == "Technician")
            {
                MessageBox.Show($"Access Denied!\n\nYour role '{role}' does not have permission to access Archived Items.\n\nOnly SuperAdmin, Admin, and Suppliers can access archived items.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPanel(panelArchivedItems, btnArchivedItems);
            panelArchivedItems.Controls.Clear();
            var archivedPanel = new ArchivedInventoryPanel();
            archivedPanel.Dock = DockStyle.Fill;
            panelArchivedItems.Controls.Add(archivedPanel);
        }

        private void btnRefreshDashboard_Click(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentUser?.Role ?? "";

            if (role == "SuperAdmin" || role == "Admin")
            {
                LoadDashboardData(); // Refresh the data only, don't recreate controls
                CreateOrUpdateCharts(); // Refresh charts
            }
            else
            {
                // Reload role-specific dashboard with correct height
                LoadRoleSpecificDashboard();
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                SessionManager.Logout();
                this.Hide();
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
            }
        }

        private void ShowError(string panel, Exception ex)
        {
            MessageBox.Show($"Error loading {panel} panel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadRoleSpecificDashboard();
            SetupNotificationBell();
            NotificationHelper.RunScheduledChecks();

            // Force sidebar reposition after form is fully loaded
            this.BeginInvoke(new Action(() =>
            {
                RepositionSidebarButtons();
            }));

            panelDashboard.Resize += (s, ev) =>
            {
                var chartPanel = panelDashboard.Controls["chartPanel"];
                if (chartPanel != null)
                {
                    chartPanel.Width = panelDashboard.Width - 40;
                    chartPanel.Invalidate();
                }
            };
        }
    }
    }

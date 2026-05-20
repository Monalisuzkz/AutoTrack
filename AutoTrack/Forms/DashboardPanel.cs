using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    // Base Dashboard Panel
    public class DashboardPanel : UserControl
    {
        protected Label lblWelcome;
        protected Label lblRoleInfo;

        public DashboardPanel()
        {
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.AutoScroll = false;
            this.Padding = new Padding(0);

            lblWelcome = new Label
            {
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0),
                AutoSize = true
            };

            lblRoleInfo = new Label
            {
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                Location = new Point(0, 35),
                AutoSize = true
            };

            this.Controls.Add(lblWelcome);
            this.Controls.Add(lblRoleInfo);
        }

        public virtual void LoadDashboard()
        {
            lblWelcome.Text = $"Welcome, {SessionManager.CurrentUser?.FullName}!";
            lblRoleInfo.Text = $"You are logged in as: {SessionManager.CurrentUser?.Role}";
        }
    }

    // ============================================================
    // STAFF DASHBOARD
    // ============================================================
    public class StaffDashboard : DashboardPanel
    {
        private Label lblRevenueValue, lblServicesValue, lblLowStockValue;
        private DataGridView dgvTodayServices;
        private DataGridView dgvPendingServices;
        private DataGridView dgvLowStockItems;

        public StaffDashboard()
        {
            this.MinimumSize = new Size(1020, 1400);  // Increased from 1000 to 1400
            this.Height = 1500;                       // Increased from 1200 to 1500
            InitializeStaffControls();
            this.Resize += StaffDashboard_Resize;
        }

        private void StaffDashboard_Resize(object sender, EventArgs e)
        {
            // Adjust table widths when form resizes
            if (dgvTodayServices != null)
            {
                dgvTodayServices.Width = this.Width - 40;
                dgvPendingServices.Width = this.Width - 40;
                dgvLowStockItems.Width = this.Width - 40;
            }
        }

        private void InitializeStaffControls()
        {
            // ========== STAT CARDS ==========
            int statY = 70;  // Reduced because padding adds 20px
            int cardW = 230;
            int cardH = 105;
            int gap = 18;

            var pnlRevenue = CreateStatCard("Today's Revenue", Color.FromArgb(46, 204, 113), 0, statY, cardW, cardH);
            lblRevenueValue = (Label)pnlRevenue.Controls["lblValue"];
            this.Controls.Add(pnlRevenue);

            var pnlServices = CreateStatCard("Today's Services", Color.FromArgb(52, 152, 219), cardW + gap, statY, cardW, cardH);
            lblServicesValue = (Label)pnlServices.Controls["lblValue"];
            this.Controls.Add(pnlServices);

            var pnlLowStock = CreateStatCard("Low Stock Items", Color.FromArgb(241, 196, 15), (cardW + gap) * 2, statY, cardW, cardH);
            lblLowStockValue = (Label)pnlLowStock.Controls["lblValue"];
            this.Controls.Add(pnlLowStock);

            // ========== TABLES ==========
            int tableY = statY + cardH + 40;

            var lblTodayTitle = new Label
            {
                Text = "🔧 Today's Service Schedule",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, tableY),
                AutoSize = true
            };
            this.Controls.Add(lblTodayTitle);

            dgvTodayServices = CreateDataGridView();
            dgvTodayServices.Location = new Point(0, tableY + 30);
            dgvTodayServices.Width = this.Width - this.Padding.Horizontal;
            dgvTodayServices.Height = 200;
            dgvTodayServices.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvTodayServices);

            int pendingY = tableY + 260;
            var lblPendingTitle = new Label
            {
                Text = "⏳ Pending Approvals",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, pendingY),
                AutoSize = true
            };
            this.Controls.Add(lblPendingTitle);

            dgvPendingServices = CreateDataGridView();
            dgvPendingServices.Location = new Point(0, pendingY + 30);
            dgvPendingServices.Width = this.Width - this.Padding.Horizontal;
            dgvPendingServices.Height = 200;
            dgvPendingServices.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvPendingServices);

            int lowStockY = pendingY + 260;
            var lblLowStockTitle = new Label
            {
                Text = "⚠️ Low Stock Alert - Need Restocking",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, lowStockY),
                AutoSize = true
            };
            this.Controls.Add(lblLowStockTitle);

            dgvLowStockItems = CreateDataGridView();
            dgvLowStockItems.Location = new Point(0, lowStockY + 30);
            dgvLowStockItems.Width = this.Width - this.Padding.Horizontal;
            dgvLowStockItems.Height = 200;
            dgvLowStockItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvLowStockItems);

            int totalHeight = lowStockY + 30 + dgvLowStockItems.Height + 40;
            this.AutoScrollMinSize = new Size(0, totalHeight);
            this.AutoScroll = true;
        }

        private Panel CreateStatCard(string title, Color accent, int x, int y, int width, int height)
        {
            var pnl = new Panel
            {
                Size = new Size(width, height),
                Location = new Point(x, y),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnl.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, pnl.Height);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Name = "lblValue",
                Text = "0",
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 45),
                AutoSize = true
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);

            return pnl;
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                GridColor = Color.FromArgb(230, 230, 230)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 38;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 123, 36);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgv.RowTemplate.Height = 32;

            return dgv;
        }

        public override void LoadDashboard()
        {
            base.LoadDashboard();

            try
            {
                // Load Today's Revenue
                object todayRevenue = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(FinalAmount), 0) FROM ServiceRecords WHERE CAST(DateIn AS DATE) = CAST(GETDATE() AS DATE) AND Status = 'Completed'");
                lblRevenueValue.Text = $"₱{Convert.ToDecimal(todayRevenue):N0}";

                // Load Today's Services Count
                object todayServices = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM ServiceRecords WHERE CAST(DateIn AS DATE) = CAST(GETDATE() AS DATE)");
                lblServicesValue.Text = todayServices?.ToString() ?? "0";

                // Load Low Stock Count
                object lowStock = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Inventory WHERE Quantity <= ReorderLevel AND (IsArchived = 0 OR IsArchived IS NULL)");
                lblLowStockValue.Text = lowStock?.ToString() ?? "0";

                // Load Today's Services
                DataTable todayServicesTable = DatabaseHelper.ExecuteQuery(@"
                    SELECT TOP 10 
                        sr.JobOrderNo AS [Job #],
                        v.PlateNumber AS [Plate],
                        v.Make + ' ' + v.Model AS [Vehicle],
                        sr.ServiceType AS [Service],
                        sr.Status AS [Status],
                        CONVERT(VARCHAR, sr.DateIn, 107) AS [Time In]
                    FROM ServiceRecords sr
                    JOIN Vehicles v ON sr.VehicleID = v.VehicleID
                    WHERE CAST(sr.DateIn AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY sr.CreatedAt DESC");
                dgvTodayServices.DataSource = todayServicesTable;

                // Load Pending Services (Restock Requests)
                DataTable pendingServices = DatabaseHelper.ExecuteQuery(@"
                    SELECT TOP 10 
                        p.PartName,
                        rr.QuantityRequested,
                        u.FullName AS RequestedBy,
                        CONVERT(VARCHAR, rr.RequestDate, 107) AS RequestDate,
                        rr.Status
                    FROM RestockRequests rr
                    JOIN Inventory p ON rr.PartID = p.PartID
                    JOIN Users u ON rr.RequestedBy = u.UserID
                    WHERE rr.Status = 'Pending'
                    ORDER BY rr.RequestDate DESC");
                dgvPendingServices.DataSource = pendingServices;

                // Load Low Stock Items
                DataTable lowStockItems = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        PartName,
                        Quantity,
                        ReorderLevel,
                        CASE 
                            WHEN Quantity <= 0 THEN 'CRITICAL'
                            WHEN Quantity <= ReorderLevel THEN 'LOW'
                            ELSE 'OK'
                        END AS [Status]
                    FROM Inventory
                    WHERE Quantity <= ReorderLevel AND (IsArchived = 0 OR IsArchived IS NULL)
                    ORDER BY Quantity ASC");
                dgvLowStockItems.DataSource = lowStockItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Staff Dashboard: " + ex.Message);
            }
        }
    }

    // ============================================================
    // TECHNICIAN DASHBOARD
    // ============================================================
    public class TechnicianDashboard : DashboardPanel
    {
        private DataGridView dgvMyTasks;
        private DataGridView dgvCompletedTasks;
        private Label lblPendingCount, lblInProgressCount, lblCompletedTodayCount;

        public TechnicianDashboard()
        {
            this.MinimumSize = new Size(1095, 1400);
            this.Height = 1500;
            InitializeTechnicianControls();
            this.Resize += TechnicianDashboard_Resize;
        }


        private void TechnicianDashboard_Resize(object sender, EventArgs e)
        {
            if (dgvMyTasks != null)
            {
                int margin = 20;
                dgvMyTasks.Width = this.Width - (margin * 2);
                dgvCompletedTasks.Width = this.Width - (margin * 2);
            }
        }
        private void InitializeTechnicianControls()
        {
            // ========== STAT CARDS ==========
            int statY = 70;
            int cardW = 230;
            int cardH = 105;
            int gap = 18;
            int margin = 0;  // Start from left edge

            // Pending Card
            var pnlPending = CreateStatCard("Pending Tasks", Color.FromArgb(241, 196, 15), margin, statY, cardW, cardH);
            lblPendingCount = (Label)pnlPending.Controls["lblValue"];
            this.Controls.Add(pnlPending);

            // In Progress Card
            var pnlInProgress = CreateStatCard("In Progress", Color.FromArgb(52, 152, 219), margin + cardW + gap, statY, cardW, cardH);
            lblInProgressCount = (Label)pnlInProgress.Controls["lblValue"];
            this.Controls.Add(pnlInProgress);

            // Completed Today Card
            var pnlCompleted = CreateStatCard("Completed Today", Color.FromArgb(46, 204, 113), margin + (cardW + gap) * 2, statY, cardW, cardH);
            lblCompletedTodayCount = (Label)pnlCompleted.Controls["lblValue"];
            this.Controls.Add(pnlCompleted);

            // ========== TABLES ==========
            int tableY = statY + cardH + 20;

            var lblTasksTitle = new Label
            {
                Text = "🔧 My Assigned Tasks",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(margin, tableY),
                AutoSize = true
            };
            this.Controls.Add(lblTasksTitle);

            dgvMyTasks = CreateDataGridView();
            dgvMyTasks.Location = new Point(margin, tableY + 30);
            dgvMyTasks.Width = this.Width;  // Full width
            dgvMyTasks.Height = 220;
            dgvMyTasks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvMyTasks);

            int completedY = tableY + 265;
            var lblCompletedTitle = new Label
            {
                Text = "✅ Recently Completed",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(margin, completedY),
                AutoSize = true
            };
            this.Controls.Add(lblCompletedTitle);

            dgvCompletedTasks = CreateDataGridView();
            dgvCompletedTasks.Location = new Point(margin, completedY + 30);
            dgvCompletedTasks.Width = this.Width;  // Full width
            dgvCompletedTasks.Height = 220;
            dgvCompletedTasks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvCompletedTasks);
        }

        private Panel CreateStatCard(string title, Color accent, int x, int y, int width, int height)
        {
            var pnl = new Panel
            {
                Size = new Size(width, height),
                Location = new Point(x, y),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnl.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, pnl.Height);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Name = "lblValue",
                Text = "0",
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 45),
                AutoSize = true
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);

            return pnl;
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                GridColor = Color.FromArgb(230, 230, 230)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 38;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 123, 36);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgv.RowTemplate.Height = 32;

            return dgv;
        }

        public override void LoadDashboard()
        {
            base.LoadDashboard();

            try
            {
                int userId = SessionManager.CurrentUser?.UserID ?? 0;

                // Load counts
                object pendingCount = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM ServiceRecords sr
                    JOIN Technicians t ON sr.TechnicianID = t.TechnicianID
                    WHERE t.UserID = @UserID AND sr.Status = 'Pending'",
                    new[] { new SqlParameter("@UserID", userId) });
                lblPendingCount.Text = pendingCount?.ToString() ?? "0";

                object inProgressCount = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM ServiceRecords sr
                    JOIN Technicians t ON sr.TechnicianID = t.TechnicianID
                    WHERE t.UserID = @UserID AND sr.Status = 'InProgress'",
                    new[] { new SqlParameter("@UserID", userId) });
                lblInProgressCount.Text = inProgressCount?.ToString() ?? "0";

                object completedTodayCount = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM ServiceRecords sr
                    JOIN Technicians t ON sr.TechnicianID = t.TechnicianID
                    WHERE t.UserID = @UserID 
                    AND sr.Status = 'Completed' 
                    AND CAST(sr.DateCompleted AS DATE) = CAST(GETDATE() AS DATE)",
                    new[] { new SqlParameter("@UserID", userId) });
                lblCompletedTodayCount.Text = completedTodayCount?.ToString() ?? "0";

                // Load My Tasks
                DataTable myTasks = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        sr.JobOrderNo AS [Job #],
                        v.PlateNumber AS [Plate],
                        v.Make + ' ' + v.Model AS [Vehicle],
                        sr.ServiceType AS [Service],
                        sr.Status AS [Status],
                        CONVERT(VARCHAR, sr.DateIn, 107) AS [Date In]
                    FROM ServiceRecords sr
                    JOIN Vehicles v ON sr.VehicleID = v.VehicleID
                    JOIN Technicians t ON sr.TechnicianID = t.TechnicianID
                    WHERE t.UserID = @UserID AND sr.Status IN ('Pending', 'InProgress')
                    ORDER BY sr.DateIn ASC",
                    new[] { new SqlParameter("@UserID", userId) });
                dgvMyTasks.DataSource = myTasks;

                // Load Recently Completed
                DataTable completedTasks = DatabaseHelper.ExecuteQuery(@"
                    SELECT TOP 10 
                        sr.JobOrderNo AS [Job #],
                        v.PlateNumber AS [Plate],
                        sr.ServiceType AS [Service],
                        CONVERT(VARCHAR, sr.DateCompleted, 107) AS [Completed On]
                    FROM ServiceRecords sr
                    JOIN Vehicles v ON sr.VehicleID = v.VehicleID
                    JOIN Technicians t ON sr.TechnicianID = t.TechnicianID
                    WHERE t.UserID = @UserID AND sr.Status = 'Completed'
                    ORDER BY sr.DateCompleted DESC",
                    new[] { new SqlParameter("@UserID", userId) });
                dgvCompletedTasks.DataSource = completedTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Technician Dashboard: " + ex.Message);
            }
        }
    }

    // ============================================================
    // SUPPLIER DASHBOARD - Simplified (Restock Requests only)
    // ============================================================
    public class SupplierDashboard : DashboardPanel
    {
        private DataGridView dgvPendingRequests;
        private DataGridView dgvCompletedRequests;
        private Label lblPendingCount, lblCompletedCount;

        public SupplierDashboard()
        {
            this.MinimumSize = new Size(1095, 1500);
            this.Height = 1700;
            InitializeSupplierControls();
            this.Resize += SupplierDashboard_Resize;
        }

        private void SupplierDashboard_Resize(object sender, EventArgs e)
        {
            if (dgvPendingRequests != null)
            {
                int margin = 20;
                dgvPendingRequests.Width = this.Width - (margin * 2);
                dgvCompletedRequests.Width = this.Width - (margin * 2);
            }
        }

        private void InitializeSupplierControls()
        {
            int sideMargin = 0; // Consistent with Staff/Technician

            // ========== STAT CARDS ==========
            int statY = 70;
            int cardW = 230;
            int cardH = 105;
            int gap = 18;
            int margin = 0;  // Start from left edge

            var pnlPending = CreateStatCard("Pending Requests", Color.FromArgb(241, 196, 15), sideMargin, statY, cardW, cardH);
            lblPendingCount = (Label)pnlPending.Controls["lblValue"];
            this.Controls.Add(pnlPending);

            var pnlCompleted = CreateStatCard("Completed Requests", Color.FromArgb(46, 204, 113), sideMargin + cardW + gap, statY, cardW, cardH);
            lblCompletedCount = (Label)pnlCompleted.Controls["lblValue"];
            this.Controls.Add(pnlCompleted);

            // ========== TABLE 1: Pending Restock Requests ==========
            int tableY = statY + cardH + 40;

            var lblPendingTitle = new Label
            {
                Text = "📋 Pending Restock Requests",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(sideMargin, tableY),
                AutoSize = true
            };
            this.Controls.Add(lblPendingTitle);

            dgvPendingRequests = CreateDataGridView();
            dgvPendingRequests.Location = new Point(sideMargin, tableY + 30);
            dgvPendingRequests.Width = this.Width - sideMargin * 2;
            dgvPendingRequests.Height = 220;
            dgvPendingRequests.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvPendingRequests);

            // ========== TABLE 2: Completed/Delivered Requests ==========
            int completedY = tableY + 280;

            var lblCompletedTitle = new Label
            {
                Text = "✅ Completed & Delivered Requests",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(sideMargin, completedY),
                AutoSize = true
            };
            this.Controls.Add(lblCompletedTitle);

            dgvCompletedRequests = CreateDataGridView();
            dgvCompletedRequests.Location = new Point(sideMargin, completedY + 30);
            dgvCompletedRequests.Width = this.Width - sideMargin * 2;
            dgvCompletedRequests.Height = 220;
            dgvCompletedRequests.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(dgvCompletedRequests);

            // Calculate total height for scrolling
            int totalHeight = completedY + 300 + 350;
            this.AutoScrollMinSize = new Size(0, totalHeight);
        }

        private Panel CreateStatCard(string title, Color accent, int x, int y, int width, int height)
        {
            var pnl = new Panel
            {
                Size = new Size(width, height),
                Location = new Point(x, y),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnl.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, pnl.Height);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Name = "lblValue",
                Text = "0",
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 45),
                AutoSize = true
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);

            return pnl;
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                GridColor = Color.FromArgb(230, 230, 230)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 38;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 123, 36);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgv.RowTemplate.Height = 32;

            return dgv;
        }

        public override void LoadDashboard()
        {
            base.LoadDashboard();

            try
            {
                int userId = SessionManager.CurrentUser?.UserID ?? 0;

                // Get supplier ID
                object supplierIdObj = DatabaseHelper.ExecuteScalar(@"
SELECT s.SupplierID 
FROM Suppliers s
JOIN Users u ON s.SupplierID = u.SupplierID
WHERE u.UserID = @UserID",
    new[] { new SqlParameter("@UserID", userId) });

                int supplierId = supplierIdObj != null ? Convert.ToInt32(supplierIdObj) : 0;

                if (supplierId > 0)
                {
                    // Load Pending Requests Count
                    object pendingCount = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM RestockRequests rr
                    JOIN Inventory i ON rr.PartID = i.PartID
                    WHERE i.SupplierID = @SupplierID AND rr.Status = 'Pending'",
                        new[] { new SqlParameter("@SupplierID", supplierId) });
                    lblPendingCount.Text = pendingCount?.ToString() ?? "0";

                    // Load Completed Count
                    object completedCount = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM RestockRequests rr
                    JOIN Inventory i ON rr.PartID = i.PartID
                    WHERE i.SupplierID = @SupplierID AND rr.Status IN ('Delivered')",
                        new[] { new SqlParameter("@SupplierID", supplierId) });
                    lblCompletedCount.Text = completedCount?.ToString() ?? "0";

                    // Load Pending Requests Table
                    DataTable pendingRequests = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        p.PartName,
                        rr.QuantityRequested,
                        u.FullName AS RequestedBy,
                        CONVERT(VARCHAR, rr.RequestDate, 107) AS [Request Date],
                        rr.Status
                    FROM RestockRequests rr
                    JOIN Inventory p ON rr.PartID = p.PartID
                    JOIN Users u ON rr.RequestedBy = u.UserID
                    WHERE p.SupplierID = @SupplierID AND rr.Status = 'Pending'
                    ORDER BY rr.RequestDate DESC",
                        new[] { new SqlParameter("@SupplierID", supplierId) });
                    dgvPendingRequests.DataSource = pendingRequests;

                    // Load Completed/Delivered Requests Table
                    DataTable completedRequests = DatabaseHelper.ExecuteQuery(@"
                    SELECT 
                        p.PartName,
                        rr.QuantityRequested,
                        u.FullName AS RequestedBy,
                        CONVERT(VARCHAR, rr.RequestDate, 107) AS [Request Date],
                        CONVERT(VARCHAR, rr.DeliveryDate, 107) AS [Delivery Date],
                        rr.Status
                    FROM RestockRequests rr
                    JOIN Inventory p ON rr.PartID = p.PartID
                    JOIN Users u ON rr.RequestedBy = u.UserID
                    WHERE p.SupplierID = @SupplierID AND rr.Status = 'Delivered'
                    ORDER BY rr.RequestDate DESC",
                        new[] { new SqlParameter("@SupplierID", supplierId) });
                    dgvCompletedRequests.DataSource = completedRequests;
                }
                else
                {
                    lblPendingCount.Text = "0";
                    lblCompletedCount.Text = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Supplier Dashboard: " + ex.Message);
            }
        }
    }
}

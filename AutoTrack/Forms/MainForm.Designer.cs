using AutoTrack.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTopBar = new Panel();
            this.panelSidebar = new Panel();
            this.panelContent = new Panel();
            this.lblAppTitle = new Label();
            this.lblUserName = new Label();
            this.lblUserRole = new Label();
            this.btnLogout = new Button();
            this.lblNavMain = new Label();
            this.btnDashboard = new Button();
            this.lblNavManagement = new Label();
            this.btnCustomers = new Button();
            this.btnVehicles = new Button();
            this.btnUsers = new Button();
            this.lblNavService = new Label();
            this.btnServices = new Button();
            this.btnTechnicians = new Button();
            this.btnPayments = new Button();
            this.lblNavInventory = new Label();
            this.btnInventory = new Button();
            this.btnSuppliers = new Button();
            this.lblNavAnalytics = new Label();
            this.btnReports = new Button();
            this.btnRestockRequests = new Button();
            this.btnSubscriptions = new Button();  // ← Only declare once
            this.btnArchivedItems = new Button();  // ← Only declare once
            this.panelDashboard = new Panel();
            this.panelCustomers = new Panel();
            this.panelVehicles = new Panel();
            this.panelServices = new Panel();
            this.panelTechnicians = new Panel();
            this.panelInventory = new Panel();
            this.panelSuppliers = new Panel();
            this.panelPayments = new Panel();
            this.panelReports = new Panel();
            this.panelUsers = new Panel();
            this.panelRestockRequests = new Panel();
            this.lblDashTitle = new Label();
            this.btnRefreshDashboard = new Button();
            this.panelSubscriptions = new Panel();
            this.panelArchivedItems = new Panel();

            // Stat Cards
            this.pnlStatActive = new Panel();
            this.pnlStatCompleted = new Panel();
            this.pnlStatCustomers = new Panel();
            this.pnlStatVehicles = new Panel();
            this.lblStatActive = new Label();
            this.lblStatCompleted = new Label();
            this.lblStatCustomers = new Label();
            this.lblStatVehicles = new Label();
            this.lblStatActiveL = new Label();
            this.lblStatCompletedL = new Label();
            this.lblStatCustomersL = new Label();
            this.lblStatVehiclesL = new Label();

            // Tables
            this.dgvRecentServices = new DataGridView();
            this.dgvRecentPayments = new DataGridView();
            this.lblRecentTitle = new Label();
            this.lblPaymentsTitle = new Label();

            this.SuspendLayout();

            // ════════════════════════════════════════════════════
            // FORM
            // ════════════════════════════════════════════════════
            this.Text = "AutoTrack — Vehicle Service & Maintenance Monitoring System";
            this.Size = new Size(1280, 850);
            this.MinimumSize = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 9f);
            this.Load += new System.EventHandler(this.MainForm_Load);
            // ════════════════════════════════════════════════════
            // TOP BAR
            // ════════════════════════════════════════════════════
            this.panelTopBar.Dock = DockStyle.Top;
            this.panelTopBar.Height = 52;
            this.panelTopBar.BackColor = Color.FromArgb(20, 20, 20);

            // AutoTrack Title
            this.lblAppTitle.Text = "⚙  AutoTrack";
            this.lblAppTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            this.lblAppTitle.ForeColor = Color.White;
            this.lblAppTitle.Location = new Point(16, 14);
            this.lblAppTitle.AutoSize = true;

            // User Name
            this.lblUserName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.lblUserName.ForeColor = Color.White;
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new Point(220, 10);
            this.lblUserName.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // User Role
            this.lblUserRole.Font = new Font("Segoe UI", 8f);
            this.lblUserRole.ForeColor = Color.FromArgb(224, 123, 36);
            this.lblUserRole.AutoSize = true;
            this.lblUserRole.Location = new Point(220, 28);
            this.lblUserRole.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // Notification Bell Button
            Button notificationBell = new Button
            {
                Text = "🔔",
                Font = new Font("Segoe UI", 14f),
                Size = new Size(36, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(255, 140, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = {
        BorderSize = 0,
        MouseOverBackColor = Color.Transparent,
        MouseDownBackColor = Color.Transparent
    },
                TabStop = false,
            };
            notificationBell.Click += (s, e) => BtnNotifications_Click(s, e);

            // Logout Button
            this.btnLogout.Text = "Logout";
            this.btnLogout.Font = new Font("Segoe UI", 9f);
            this.btnLogout.ForeColor = Color.White;
            this.btnLogout.BackColor = Color.FromArgb(180, 50, 50);
            this.btnLogout.FlatStyle = FlatStyle.Flat;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.Size = new Size(72, 30);
            this.btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnLogout.Cursor = Cursors.Hand;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            this.btnLogout.Location = new Point(1190, 11);  // Set initial fixed position

            // Add controls (ONCE)
            this.panelTopBar.Controls.AddRange(new Control[] {
    lblAppTitle, lblUserName, lblUserRole, notificationBell, btnLogout
});

            // Simple resize event (ONCE) - keep positions relative
            this.panelTopBar.Resize += (s, e) =>
            {
                // Keep buttons on the right side
                btnLogout.Location = new Point(panelTopBar.Width - 90, 11);
                notificationBell.Location = new Point(panelTopBar.Width - 160, 8);
            };
            // ════════════════════════════════════════════════════
            // SIDEBAR
            // ════════════════════════════════════════════════════
            this.panelSidebar.Dock = DockStyle.Left;
            this.panelSidebar.Width = 210;
            this.panelSidebar.BackColor = Color.FromArgb(30, 30, 30);

            void NavLabel(Label lbl, string text, int y)
            {
                lbl.Text = text;
                lbl.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                lbl.ForeColor = Color.FromArgb(100, 100, 100);
                lbl.Location = new Point(16, y);
                lbl.Size = new Size(178, 18);
            }

            void NavButton(Button btn, string text, int y, System.EventHandler handler)
            {
                btn.Text = text;
                btn.Font = new Font("Segoe UI", 10f);
                btn.ForeColor = Color.FromArgb(180, 180, 180);
                btn.BackColor = Color.FromArgb(30, 30, 30);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(14, 0, 0, 0);
                btn.Location = new Point(0, y);
                btn.Size = new Size(210, 36);
                btn.Cursor = Cursors.Hand;
                btn.Click += handler;
                btn.MouseEnter += (s, e) => {
                    if (btn.BackColor != Color.FromArgb(224, 123, 36))
                        btn.BackColor = Color.FromArgb(45, 45, 45);
                };
                btn.MouseLeave += (s, e) => {
                    if (btn.BackColor != Color.FromArgb(224, 123, 36))
                        btn.BackColor = Color.FromArgb(30, 30, 30);
                };
            }

            NavLabel(lblNavMain, "MENU", 10);
            NavButton(btnDashboard, "⬛ Dashboard", 30, btnDashboard_Click);
            NavLabel(lblNavManagement, "MANAGEMENT", 78);
            NavButton(btnUsers, "👤 Users", 96, btnUsers_Click);
            NavButton(btnCustomers, "👥 Customers", 132, btnCustomers_Click);
            NavButton(btnVehicles, "🚗 Vehicles", 168, btnVehicles_Click);
            NavLabel(lblNavService, "SERVICE", 214);
            NavButton(btnServices, "🔧 Service Records", 232, btnServices_Click);
            NavButton(btnTechnicians, "🪪 Technicians", 268, btnTechnicians_Click);
            NavButton(btnPayments, "💳 Payments", 304, btnPayments_Click);
            NavLabel(lblNavInventory, "INVENTORY", 350);
            NavButton(btnInventory, "📦 Inventory", 368, btnInventory_Click);
            NavButton(btnArchivedItems, "🗄️ Archived Items", 404, btnArchivedItems_Click);
            NavButton(btnRestockRequests, "📦 Restock Requests", 440, btnRestockRequests_Click);
            NavButton(btnSuppliers, "🚚 Suppliers", 476, btnSuppliers_Click);
            NavLabel(lblNavAnalytics, "ANALYTICS", 522);
            NavButton(btnReports, "📊 Reports", 540, btnReports_Click);
            NavButton(btnSubscriptions, "📅 Subscriptions", 576, btnSubscriptions_Click);

            this.panelSidebar.Controls.AddRange(new Control[] {
                lblNavMain, btnDashboard,
                lblNavManagement, btnUsers, btnCustomers, btnVehicles,
                lblNavService, btnServices, btnTechnicians, btnPayments,
                lblNavInventory, btnInventory, btnArchivedItems, btnRestockRequests, btnSuppliers,
                lblNavAnalytics, btnReports, btnSubscriptions
            });

            // ════════════════════════════════════════════════════
            // CONTENT AREA
            // ════════════════════════════════════════════════════
            this.panelContent.Dock = DockStyle.Fill;
            this.panelContent.BackColor = Color.FromArgb(245, 245, 245);
            this.panelContent.Padding = new Padding(20);

            // Configure panelDashboard
            // Configure panelDashboard - CHANGE THIS
            this.panelDashboard.AutoScroll = true;  // Change from true to false
            this.panelDashboard.AutoScrollMinSize = new Size(0, 1035);  // Reset this
            this.panelDashboard.BackColor = Color.FromArgb(245, 245, 245);
            this.panelDashboard.Visible = false;
            this.panelDashboard.Dock = DockStyle.Fill;  // Make sure this is set

            // Configure other panels
            foreach (Panel p in new Panel[] {
                panelCustomers, panelVehicles, panelServices, panelTechnicians,
                panelInventory, panelSuppliers, panelPayments, panelReports,
                panelUsers, panelRestockRequests, panelSubscriptions, panelArchivedItems
            })
            {
                p.Dock = DockStyle.Fill;
                p.BackColor = Color.FromArgb(245, 245, 245);
                p.Visible = false;
            }

            // Add all panels to content
            this.panelContent.Controls.AddRange(new Control[] {
                panelDashboard, panelCustomers, panelVehicles, panelServices,
                panelTechnicians, panelInventory, panelSuppliers, panelPayments,
                panelReports, panelUsers, panelRestockRequests, panelSubscriptions, panelArchivedItems
            });

            // ════════════════════════════════════════════════════
            // DASHBOARD PANEL CONTROLS
            // ════════════════════════════════════════════════════
            this.lblDashTitle.Text = $"Dashboard - Welcome back, {SessionManager.CurrentUser?.FullName ?? "User"}!";
            this.lblDashTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            this.lblDashTitle.ForeColor = Color.FromArgb(30, 30, 30);
            this.lblDashTitle.Location = new Point(0, 0);
            this.lblDashTitle.AutoSize = true;

            this.btnRefreshDashboard.Text = "↻ Refresh";
            this.btnRefreshDashboard.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.btnRefreshDashboard.BackColor = Color.FromArgb(22, 163, 74);
            this.btnRefreshDashboard.ForeColor = Color.White;
            this.btnRefreshDashboard.FlatStyle = FlatStyle.Flat;
            this.btnRefreshDashboard.Size = new Size(100, 32);
            this.btnRefreshDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnRefreshDashboard.Cursor = Cursors.Hand;
            this.btnRefreshDashboard.Click += this.btnRefreshDashboard_Click;

            // Stat Cards
            int statY = 45;
            int cardW = 230;
            int cardH = 105;
            int gap = 18;

            CreateStatCard(this.pnlStatActive, this.lblStatActive, this.lblStatActiveL,
                "Today's Revenue", 0, statY, cardW, cardH, Color.FromArgb(46, 204, 113));

            CreateStatCard(this.pnlStatCompleted, this.lblStatCompleted, this.lblStatCompletedL,
                "Monthly Revenue", cardW + gap, statY, cardW, cardH, Color.FromArgb(52, 152, 219));

            CreateStatCard(this.pnlStatCustomers, this.lblStatCustomers, this.lblStatCustomersL,
                "Active Services", (cardW + gap) * 2, statY, cardW, cardH, Color.FromArgb(241, 196, 15));

            CreateStatCard(this.pnlStatVehicles, this.lblStatVehicles, this.lblStatVehiclesL,
                "Completed Today", (cardW + gap) * 3, statY, cardW, cardH, Color.FromArgb(155, 89, 182));

            // Recent Services Table
            int servicesY = statY + cardH + 295;
            this.lblRecentTitle.Text = "Recent Service Records";
            this.lblRecentTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            this.lblRecentTitle.ForeColor = Color.FromArgb(30, 30, 30);
            this.lblRecentTitle.Location = new Point(0, servicesY);
            this.lblRecentTitle.AutoSize = true;

            SetupTableStyle(this.dgvRecentServices);
            this.dgvRecentServices.Location = new Point(0, servicesY + 30);
            this.dgvRecentServices.Size = new Size(980, 250);
            this.dgvRecentServices.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // Recent Payments Table
            int paymentsY = servicesY + 300;
            this.lblPaymentsTitle.Text = "Recent Payments";
            this.lblPaymentsTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            this.lblPaymentsTitle.ForeColor = Color.FromArgb(30, 30, 30);
            this.lblPaymentsTitle.Location = new Point(0, paymentsY);
            this.lblPaymentsTitle.AutoSize = true;

            SetupTableStyle(this.dgvRecentPayments);
            this.dgvRecentPayments.Location = new Point(0, paymentsY + 30);
            this.dgvRecentPayments.Size = new Size(980, 250);
            this.dgvRecentPayments.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // Add controls to dashboard
            this.panelDashboard.Controls.AddRange(new Control[] {
                lblDashTitle, btnRefreshDashboard,
                pnlStatActive, pnlStatCompleted, pnlStatCustomers, pnlStatVehicles,
                lblRecentTitle, dgvRecentServices,
                lblPaymentsTitle, dgvRecentPayments
            });

            // Position refresh button on resize
            this.panelDashboard.Resize += (s, e) =>
                btnRefreshDashboard.Location = new Point(panelDashboard.Width - btnRefreshDashboard.Width - 20, 4);

            // Placeholder labels for other panels
            void PlaceholderPanel(Panel pnl, string title)
            {
                var lbl = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(0, 0),
                    AutoSize = true
                };
                pnl.Controls.Add(lbl);
            }

            PlaceholderPanel(panelCustomers, "Customers");
            PlaceholderPanel(panelVehicles, "Vehicles");
            PlaceholderPanel(panelServices, "Service Records");
            PlaceholderPanel(panelTechnicians, "Technicians");
            PlaceholderPanel(panelInventory, "Inventory");
            PlaceholderPanel(panelSuppliers, "Suppliers");
            PlaceholderPanel(panelPayments, "Payments");
            PlaceholderPanel(panelReports, "Reports & Analytics");
            PlaceholderPanel(panelUsers, "Manage Users");
            PlaceholderPanel(panelRestockRequests, "Restock Requests");

            // ════════════════════════════════════════════════════
            // ASSEMBLE FORM
            // ════════════════════════════════════════════════════
            this.Controls.Add(panelContent);
            this.Controls.Add(panelSidebar);
            this.Controls.Add(panelTopBar);

            this.ResumeLayout(false);
        }

        // Helper method for stat cards (4 main cards)
        private void CreateStatCard(Panel pnl, Label valLbl, Label nameLbl,
            string name, int x, int y, int width, int height, Color accent)
        {
            pnl.Size = new Size(width, height);
            pnl.Location = new Point(x, y);
            pnl.BackColor = Color.White;

            pnl.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, pnl.Height);

            valLbl.Text = "0";
            valLbl.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
            valLbl.ForeColor = Color.FromArgb(30, 30, 30);
            valLbl.Location = new Point(16, 12);
            valLbl.AutoSize = true;

            nameLbl.Text = name;
            nameLbl.Font = new Font("Segoe UI", 10f);
            nameLbl.ForeColor = Color.Gray;
            nameLbl.Location = new Point(16, 58);
            nameLbl.AutoSize = true;

            pnl.Controls.Add(valLbl);
            pnl.Controls.Add(nameLbl);
        }

        // Helper method for table styling
        private void SetupTableStyle(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = Color.FromArgb(210, 210, 210);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToOrderColumns = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;
            dgv.ScrollBars = ScrollBars.Both;
            dgv.ColumnHeadersVisible = true;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 38;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 123, 36);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgv.RowTemplate.Height = 32;

            dgv.DataBindingComplete += (s, e) =>
            {
                dgv.ColumnHeadersVisible = true;
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    col.Resizable = DataGridViewTriState.False;
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            };
        }

        // ── Fields ──────────────────────────────────────────────
        private Panel panelTopBar;
        private Panel panelSidebar;
        private Panel panelContent;
        private Label lblAppTitle;
        private Label lblUserName;
        private Label lblUserRole;
        private Button btnLogout;
        private Label lblNavMain;
        private Label lblNavManagement;
        private Label lblNavService;
        private Label lblNavInventory;
        private Label lblNavAnalytics;
        private Button btnDashboard;
        private Button btnCustomers;
        private Button btnVehicles;
        private Button btnUsers;
        private Button btnServices;
        private Button btnTechnicians;
        private Button btnPayments;
        private Button btnInventory;
        private Button btnArchivedItems;
        private Button btnSuppliers;
        private Button btnReports;
        private Button btnRestockRequests;
        private Button btnSubscriptions;
        private Panel panelDashboard;
        private Panel panelCustomers;
        private Panel panelVehicles;
        private Panel panelServices;
        private Panel panelTechnicians;
        private Panel panelInventory;
        private Panel panelArchivedItems;
        private Panel panelSuppliers;
        private Panel panelPayments;
        private Panel panelReports;
        private Panel panelUsers;
        private Panel panelRestockRequests;
        private Panel panelSubscriptions;
        private Label lblDashTitle;
        private Button btnRefreshDashboard;

        // Stat Cards
        private Panel pnlStatActive;
        private Panel pnlStatCompleted;
        private Panel pnlStatCustomers;
        private Panel pnlStatVehicles;
        private Label lblStatActive;
        private Label lblStatCompleted;
        private Label lblStatCustomers;
        private Label lblStatVehicles;
        private Label lblStatActiveL;
        private Label lblStatCompletedL;
        private Label lblStatCustomersL;
        private Label lblStatVehiclesL;

        // Tables
        private Label lblRecentTitle;
        private Label lblPaymentsTitle;
        private DataGridView dgvRecentServices;
        private DataGridView dgvRecentPayments;
    }
}
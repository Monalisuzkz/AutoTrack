using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;
using AutoTrack.Helpers;

namespace AutoTrack.Forms
{
    public class SubscriptionsPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "SubscriptionID", "CustomerID", "PlanID" };

        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnRefresh, btnViewDetails;
        private ComboBox cboStatus;
        private Label lblCount;
        private bool isAdmin = false;
        private Button btnPrintContract;
        public SubscriptionsPanel()
        {
            isAdmin = SessionManager.CurrentUser?.Role == "SuperAdmin" ||
                      SessionManager.CurrentUser?.Role == "Admin";
            Init();
            LoadData();
        }

        private void Init()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by customer or plan...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Subscribe", Color.FromArgb(224, 123, 36), 100, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            btnViewDetails = MakeButton("View Plan Details", Color.FromArgb(52, 152, 219), 120, 34);
            lblCount = new Label();


            // Status filter
            cboStatus = new ComboBox
            {
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                FlatStyle = FlatStyle.Flat
            };
            cboStatus.Items.AddRange(new object[] { "All", "Active", "Expired", "Cancelled", "Suspended" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += (s, e) => LoadData();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += EditClick;
            btnRefresh.Click += (s, e) => LoadData();
            btnViewDetails.Click += BtnViewDetails_Click;
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            CreateGrid();
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            var toolbar = BuildCustomToolbar();

            Controls.Add(dgv);
            Controls.Add(toolbar);
        }

        private Panel BuildCustomToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(0, 0, 0, 8)
            };

            toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            // Title at top
            var lblTitle = new Label
            {
                Text = "Service Records",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 12),
                AutoSize = true
            };
            toolbar.Controls.Add(lblTitle);

            // Controls Y position (adjusted down)
            int yPos = 62;  // ← CHANGED FROM 56 TO 62
            int xPos = 16;

            // Search Box
            txtSearch.Location = new Point(xPos, yPos);
            txtSearch.Size = new Size(200, 30);
            txtSearch.Font = new Font("Segoe UI", 10f);
            toolbar.Controls.Add(txtSearch);
            xPos += txtSearch.Width + 5;

            // Search Button
            btnSearch.Location = new Point(xPos, yPos);
            btnSearch.Size = new Size(80, 30);
            btnSearch.BackColor = Color.FromArgb(60, 60, 60);
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnSearch);
            xPos += btnSearch.Width + 10;

            // Status Filter
            cboStatus.Location = new Point(xPos, yPos);
            cboStatus.Size = new Size(120, 30);
            cboStatus.Font = new Font("Segoe UI", 9f);
            toolbar.Controls.Add(cboStatus);
            xPos += cboStatus.Width + 10;

            // Separator
            var sep1 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep1);
            xPos += sep1.Width + 10;

            // Add Button
            btnAdd.Location = new Point(xPos, yPos);
            btnAdd.Size = new Size(100, 30);
            btnAdd.BackColor = Color.FromArgb(224, 123, 36);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnAdd);
            xPos += btnAdd.Width + 5;

            // Edit Button
            btnEdit.Location = new Point(xPos, yPos);
            btnEdit.Size = new Size(80, 30);
            btnEdit.BackColor = Color.FromArgb(29, 78, 216);
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnEdit);
            xPos += btnEdit.Width + 5;

            // View Details Button
            btnViewDetails.Location = new Point(xPos, yPos);
            btnViewDetails.Size = new Size(120, 30);
            btnViewDetails.BackColor = Color.FromArgb(52, 152, 219);
            btnViewDetails.ForeColor = Color.White;
            btnViewDetails.FlatStyle = FlatStyle.Flat;
            btnViewDetails.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnViewDetails);
            xPos += btnViewDetails.Width + 10;

            // Separator
            var sep2 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep2);
            xPos += sep2.Width + 10;

            // Refresh Button
            btnRefresh.Location = new Point(xPos, yPos);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.BackColor = Color.FromArgb(22, 163, 74);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnRefresh);
            xPos += btnRefresh.Width + 15;

            // Count Label
            lblCount.Location = new Point(xPos, yPos + 6);
            lblCount.Font = new Font("Segoe UI", 9f);
            lblCount.ForeColor = Color.Gray;
            lblCount.AutoSize = true;
            toolbar.Controls.Add(lblCount);

            return toolbar;
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int subscriptionId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["SubscriptionID"].Value);
                ShowSubscriptionDetails(subscriptionId);
            }
        }

        private void ShowSubscriptionDetails(int subscriptionId)
        {
            try
            {
                string query = @"
            SELECT 
                c.FirstName + ' ' + c.LastName AS Customer,
                sp.PlanName,
                sp.MonthlyFee,
                sp.ServiceLimit,
                sp.DiscountPercent,
                cs.StartDate,
                cs.EndDate,
                cs.Status,
                cs.AutoRenew,
                DATEDIFF(DAY, GETDATE(), cs.EndDate) AS DaysRemaining
            FROM CustomerSubscriptions cs
            JOIN Customers c ON cs.CustomerID = c.CustomerID
            JOIN SubscriptionPlans sp ON cs.PlanID = sp.PlanID
            WHERE cs.SubscriptionID = @ID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, new[] { new SqlParameter("@ID", subscriptionId) });

                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    int daysLeft = Convert.ToInt32(r["DaysRemaining"]);
                    string status = r["Status"].ToString();
                    decimal monthlyFee = Convert.ToDecimal(r["MonthlyFee"]);
                    int serviceLimit = Convert.ToInt32(r["ServiceLimit"]);
                    int discount = Convert.ToInt32(r["DiscountPercent"]);
                    DateTime startDate = Convert.ToDateTime(r["StartDate"]);
                    DateTime endDate = Convert.ToDateTime(r["EndDate"]);
                    bool autoRenew = Convert.ToBoolean(r["AutoRenew"]);

                    // Create centered popup form
                    Form detailsForm = new Form
                    {
                        Text = "Subscription Details",
                        Size = new Size(550, 480),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false,
                        BackColor = Color.White
                    };

                    TableLayoutPanel mainLayout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(25),
                        AutoScroll = false,
                        ColumnCount = 1,
                        RowCount = 10
                    };
                    mainLayout.RowStyles.Clear();
                    for (int i = 0; i < 10; i++)
                    {
                        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    }

                    // Customer name - CENTERED
                    Label lblCustomerTitle = new Label
                    {
                        Text = "CUSTOMER",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    Label lblCustomer = new Label
                    {
                        Text = r["Customer"].ToString(),
                        Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(30, 30, 30),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    // Plan name - CENTERED
                    Label lblPlanTitle = new Label
                    {
                        Text = "SUBSCRIPTION PLAN",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    Label lblPlan = new Label
                    {
                        Text = r["PlanName"].ToString(),
                        Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(224, 123, 36),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    // Separator - CENTERED
                    Label lblSeparator1 = new Label
                    {
                        Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
                        Font = new Font("Segoe UI", 9f),
                        ForeColor = Color.FromArgb(220, 220, 220),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    // Plan Benefits Box - CENTERED
                    Panel benefitsBox = new Panel
                    {
                        Size = new Size(460, 130),
                        BackColor = Color.FromArgb(240, 240, 240),
                        BorderStyle = BorderStyle.FixedSingle,
                        AutoSize = false
                    };

                    Label lblBenefitsTitle = new Label
                    {
                        Text = "PLAN BENEFITS",
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(224, 123, 36),
                        Location = new Point(10, 8),
                        AutoSize = true
                    };

                    Label lblBenefits = new Label
                    {
                        Text = $"• Monthly Fee: ₱{monthlyFee:N2}\n" +
                               $"• Service Limit: {(serviceLimit == 0 ? "Unlimited" : serviceLimit + " per month")}\n" +
                               $"• Parts Discount: {discount}% OFF\n\n" +
                               $"• Priority scheduling\n" +
                               $"• Free vehicle inspection\n" +
                               $"• Digital service history\n" +
                               $"• SMS/Email reminders\n" +
                               $"• 24/7 customer support",
                        Font = new Font("Segoe UI", 9f),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        Location = new Point(10, 35),
                        AutoSize = true
                    };

                    benefitsBox.Controls.AddRange(new Control[] { lblBenefitsTitle, lblBenefits });

                    // Center the benefitsBox
                    FlowLayoutPanel centerPanel1 = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        WrapContents = false
                    };
                    centerPanel1.Controls.Add(benefitsBox);
                    benefitsBox.Location = new Point(0, 0);

                    // Subscription Info - CENTERED
                    Label lblInfoTitle = new Label
                    {
                        Text = "SUBSCRIPTION INFORMATION",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    string daysText = daysLeft > 0 ? $"{daysLeft} days" : "EXPIRED";

                    Label lblInfo = new Label
                    {
                        Text = $"Start Date: {startDate:MMM dd, yyyy}\n" +
                               $"End Date: {endDate:MMM dd, yyyy}\n" +
                               $"Days Remaining: {daysText}\n" +
                               $"Auto-Renew: {(autoRenew ? "YES" : "NO")}\n" +
                               $"Status: {status}",
                        Font = new Font("Segoe UI", 10f),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };

                    // Close button - CENTERED
                    Button btnClose = new Button
                    {
                        Text = "Close",
                        Size = new Size(100, 38),
                        BackColor = Color.FromArgb(224, 123, 36),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold)
                    };
                    btnClose.FlatAppearance.BorderSize = 0;
                    btnClose.Click += (s, ev) => detailsForm.Close();

                    FlowLayoutPanel centerButtonPanel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        WrapContents = false
                    };
                    centerButtonPanel.Controls.Add(btnClose);
                    btnClose.Location = new Point(0, 0);

                    // Add all controls to layout with center alignment
                    mainLayout.Controls.Add(lblCustomerTitle, 0, 0);
                    mainLayout.Controls.Add(lblCustomer, 0, 1);
                    mainLayout.Controls.Add(lblPlanTitle, 0, 2);
                    mainLayout.Controls.Add(lblPlan, 0, 3);
                    mainLayout.Controls.Add(lblSeparator1, 0, 4);
                    mainLayout.Controls.Add(centerPanel1, 0, 5);
                    mainLayout.Controls.Add(lblInfoTitle, 0, 6);
                    mainLayout.Controls.Add(lblInfo, 0, 7);
                    mainLayout.Controls.Add(centerButtonPanel, 0, 8);

                    // Set center alignment for table layout panel
                    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    foreach (Control ctrl in mainLayout.Controls)
                    {
                        ctrl.Anchor = AnchorStyles.Top;
                    }

                    detailsForm.Controls.Add(mainLayout);
                    detailsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading details: " + ex.Message);
            }
        }


        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a subscription to view details.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SubscriptionID"].Value);
            ShowSubscriptionDetails(id);
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Active":
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);
                        break;
                    case "Expired":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        break;
                    case "Suspended":
                        e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                        e.CellStyle.ForeColor = Color.FromArgb(133, 77, 14);
                        break;
                }
            }

            if (dgv.Columns[e.ColumnIndex].Name == "DaysRemaining" && e.Value != null)
            {
                int days = Convert.ToInt32(e.Value);
                if (days < 0)
                {
                    e.Value = "Expired";
                    e.CellStyle.ForeColor = Color.Red;
                }
                else if (days <= 7)
                {
                    e.CellStyle.ForeColor = Color.Orange;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
            }
        }

        private void LoadData(string search = "")
        {
            try
            {
                string statusFilter = cboStatus.SelectedItem?.ToString() == "All" ? "" : cboStatus.SelectedItem?.ToString();

                string query = @"
                    SELECT 
                        cs.SubscriptionID,
                        cs.CustomerID,
                        c.FirstName + ' ' + c.LastName AS Customer,
                        sp.PlanName AS PlanName,
                        sp.MonthlyFee,
                        sp.ServiceLimit,
                        sp.DiscountPercent,
                        cs.StartDate,
                        cs.EndDate,
                        cs.Status,
                        cs.AutoRenew,
                        DATEDIFF(DAY, GETDATE(), cs.EndDate) AS DaysRemaining
                    FROM CustomerSubscriptions cs
                    JOIN Customers c ON cs.CustomerID = c.CustomerID
                    JOIN SubscriptionPlans sp ON cs.PlanID = sp.PlanID
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(statusFilter))
                    query += " AND cs.Status = @Status";
                if (!string.IsNullOrEmpty(search))
                    query += " AND (c.FirstName LIKE @S OR c.LastName LIKE @S OR sp.PlanName LIKE @S)";
                query += " ORDER BY cs.StartDate DESC";

                var paramList = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(statusFilter))
                    paramList.Add(new SqlParameter("@Status", statusFilter));
                if (!string.IsNullOrEmpty(search))
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));

                DataTable dt = DatabaseHelper.ExecuteQuery(query, paramList.ToArray());
                BindGrid(dt);
                lblCount.Text = $"{dgv.RowCount} subscription(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new SubscriptionForm();
            if (f.ShowDialog() == DialogResult.OK)
                LoadData();
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a subscription to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SubscriptionID"].Value);
            var f = new SubscriptionForm(id);
            if (f.ShowDialog() == DialogResult.OK)
                LoadData();
        }
    }

    // Subscription Form with LinkLabel for Plan Details - FULLY CENTERED
    // Subscription Form with Automatic End Date Calculation
    public class SubscriptionForm : Form
    {
        private int _id = 0;
        private bool _edit = false;
        private ComboBox cboCustomer;
        private ComboBox cboPlan;
        private ComboBox cboStatus;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private CheckBox chkAutoRenew;
        private TextBox txtNotes;
        private Label lblMonthlyFee, lblServiceLimit, lblDiscount, lblDuration;
        private Button btnSave, btnCancel;
        private LinkLabel lnkViewPlanBenefits;
        private Panel pnlPlanBenefits;

        public SubscriptionForm(int id = 0)
        {
            _id = id;
            _edit = id > 0;
            Init();
            LoadCustomers();
            LoadPlans();
            if (_edit) LoadSubscription();
        }

        private void Init()
        {
            Text = _edit ? "Edit Subscription" : "New Subscription";
            Size = new Size(520, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);
            Font = new Font("Segoe UI", 9f);

            // Title
            var lblTitle = new Label
            {
                Text = _edit ? "Edit Subscription" : "Create Subscription",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 16),
                AutoSize = true
            };

            // Customer
            var lblCustomer = new Label { Text = "Customer:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboCustomer = new ComboBox { Location = new Point(140, 57), Size = new Size(340, 30), DropDownStyle = ComboBoxStyle.DropDownList };

            // Plan
            var lblPlan = new Label { Text = "Subscription Plan:", Location = new Point(20, 100), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboPlan = new ComboBox { Location = new Point(140, 97), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboPlan.SelectedIndexChanged += PlanChanged;

            // View Plan Benefits LinkLabel
            lnkViewPlanBenefits = new LinkLabel
            {
                Text = "View Plan Benefits",
                Location = new Point(370, 110),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                LinkColor = Color.FromArgb(52, 152, 219)
            };
            lnkViewPlanBenefits.Click += LnkViewPlanBenefits_Click;

            // Plan Details Panel
            pnlPlanBenefits = new Panel
            {
                Location = new Point(20, 140),
                Size = new Size(460, 150),
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblBenefitsHeader = new Label
            {
                Text = "PLAN BENEFITS & WHAT'S INCLUDED",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(10, 8),
                AutoSize = true,
                ForeColor = Color.FromArgb(224, 123, 36)
            };

            lblMonthlyFee = new Label
            {
                Text = "Monthly Fee: ₱0",
                Location = new Point(10, 35),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30)
            };

            lblDuration = new Label
            {
                Text = "Duration: 1 month",
                Location = new Point(10, 58),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };

            lblServiceLimit = new Label
            {
                Text = "Service Limit: Unlimited",
                Location = new Point(10, 81),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };

            lblDiscount = new Label
            {
                Text = "Discount: 0% off parts",
                Location = new Point(10, 104),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };

            var lblExtraBenefits = new Label
            {
                Text = "✓ Priority scheduling  ✓ Free inspection  ✓ Digital history  ✓ 24/7 support",
                Location = new Point(10, 127),
                AutoSize = true,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            pnlPlanBenefits.Controls.AddRange(new Control[] {
            lblBenefitsHeader, lblMonthlyFee, lblDuration, lblServiceLimit, lblDiscount, lblExtraBenefits
        });

            // Start Date
            var lblStartDate = new Label { Text = "Start Date:", Location = new Point(20, 310), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            dtpStartDate = new DateTimePicker { Location = new Point(140, 307), Size = new Size(140, 28), Format = DateTimePickerFormat.Short };
            dtpStartDate.ValueChanged += (s, e) => UpdateEndDate();

            // End Date (auto-calculated, read-only)
            var lblEndDate = new Label { Text = "End Date:", Location = new Point(20, 350), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            dtpEndDate = new DateTimePicker { Location = new Point(140, 347), Size = new Size(140, 28), Format = DateTimePickerFormat.Short };
            dtpEndDate.Enabled = false;
            dtpEndDate.BackColor = Color.FromArgb(245, 245, 245);

            // Auto Renew
            chkAutoRenew = new CheckBox { Text = "Auto Renew (automatic billing every month)", Location = new Point(140, 385), AutoSize = true, Checked = true };

            // Status
            var lblStatus = new Label { Text = "Status:", Location = new Point(20, 420), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboStatus = new ComboBox { Location = new Point(140, 417), Size = new Size(140, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Active", "Expired", "Cancelled", "Suspended" });
            cboStatus.SelectedIndex = 0;

            // Notes
            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 465), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtNotes = new TextBox { Location = new Point(20, 485), Size = new Size(460, 60), Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            // Buttons
            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Create Subscription",
                Location = new Point(20, 570),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnSave.Click += Save;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(180, 570),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] {
            lblTitle, lblCustomer, cboCustomer, lblPlan, cboPlan, lnkViewPlanBenefits,
            pnlPlanBenefits, lblStartDate, dtpStartDate, lblEndDate, dtpEndDate,
            chkAutoRenew, lblStatus, cboStatus, lblNotes, txtNotes,
            btnSave, btnCancel
        });
        }

        private void UpdateEndDate()
        {
            if (cboPlan.SelectedValue != null)
            {
                DataRowView drv = cboPlan.SelectedItem as DataRowView;
                if (drv != null)
                {
                    int durationMonths = drv["DurationMonths"] != DBNull.Value ? Convert.ToInt32(drv["DurationMonths"]) : 1;

                    if (durationMonths >= 999)
                    {
                        dtpEndDate.Value = dtpStartDate.Value.AddYears(100);
                    }
                    else
                    {
                        dtpEndDate.Value = dtpStartDate.Value.AddMonths(durationMonths).AddDays(-1);
                    }
                }
            }
            else
            {
                dtpEndDate.Value = dtpStartDate.Value.AddMonths(1).AddDays(-1);
            }
        }

        private void PlanChanged(object sender, EventArgs e)
        {
            if (cboPlan.SelectedValue != null)
            {
                DataRowView drv = cboPlan.SelectedItem as DataRowView;
                if (drv != null)
                {
                    decimal fee = Convert.ToDecimal(drv["MonthlyFee"]);
                    int serviceLimit = drv["ServiceLimit"] == DBNull.Value ? 0 : Convert.ToInt32(drv["ServiceLimit"]);
                    int discount = Convert.ToInt32(drv["DiscountPercent"]);
                    int durationMonths = drv["DurationMonths"] != DBNull.Value ? Convert.ToInt32(drv["DurationMonths"]) : 1;

                    lblMonthlyFee.Text = $"Monthly Fee: ₱{fee:N2}";
                    lblDuration.Text = durationMonths >= 999 ? "Duration: Lifetime" : $"Duration: {durationMonths} month(s)";
                    lblServiceLimit.Text = serviceLimit == 0 ? "Service Limit: Unlimited" : $"Service Limit: {serviceLimit} per month";
                    lblDiscount.Text = $"Discount: {discount}% off parts";

                    UpdateEndDate();
                }
            }
        }

        private void LnkViewPlanBenefits_Click(object sender, EventArgs e)
        {
            if (cboPlan.SelectedItem == null)
            {
                MessageBox.Show("Please select a plan first to view its benefits.",
                    "No Plan Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRowView drv = cboPlan.SelectedItem as DataRowView;
            if (drv != null)
            {
                decimal fee = Convert.ToDecimal(drv["MonthlyFee"]);
                int serviceLimit = drv["ServiceLimit"] == DBNull.Value ? 0 : Convert.ToInt32(drv["ServiceLimit"]);
                int discount = Convert.ToInt32(drv["DiscountPercent"]);
                int durationMonths = drv["DurationMonths"] != DBNull.Value ? Convert.ToInt32(drv["DurationMonths"]) : 1;
                string planName = drv["PlanName"].ToString();
                string durationText = durationMonths >= 999 ? "LIFETIME" : $"{durationMonths} MONTHS";

                Form planDetailsForm = new Form
                {
                    Text = $"{planName} - Plan Benefits",
                    Size = new Size(520, 650),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                TableLayoutPanel mainLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(25),
                    ColumnCount = 1,
                    RowCount = 6
                };
                mainLayout.RowStyles.Clear();
                for (int i = 0; i < 6; i++)
                {
                    mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                Label lblPlanName = new Label
                {
                    Text = planName,
                    Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(224, 123, 36),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    AutoSize = true
                };

                Label lblSeparator = new Label
                {
                    Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    AutoSize = true
                };

                Panel investBox = new Panel
                {
                    Size = new Size(430, 50),
                    BackColor = Color.FromArgb(248, 248, 248),
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoSize = false
                };

                Label lblInvestment = new Label
                {
                    Text = $"MONTHLY INVESTMENT: ₱{fee:N2}",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(224, 123, 36),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                investBox.Controls.Add(lblInvestment);

                FlowLayoutPanel centerInvestPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false
                };
                centerInvestPanel.Controls.Add(investBox);
                investBox.Location = new Point(0, 0);

                Label lblDetails = new Label
                {
                    Text = $@"

COMMITMENT PERIOD: {durationText}
────────────────────────────────
• Service Appointments: {(serviceLimit == 0 ? "UNLIMITED" : serviceLimit + " per month")}
• Parts Discount: {discount}% OFF on all parts
• Priority Lane Access
• Free 20-Point Vehicle Inspection
• Digital Service History Record
• SMS & Email Reminders for Maintenance
• Free Car Wash with Every Service
• 24/7 Customer Support Hotline

BONUS PERKS:
────────────────────────────────
• Birthday month: 10% additional discount
• Refer a friend: 1 month FREE
• Loyalty points: 1 point = ₱10 for future services",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    AutoSize = true
                };

                Button btnClose = new Button
                {
                    Text = "Close",
                    Size = new Size(100, 38),
                    BackColor = Color.FromArgb(224, 123, 36),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, ev) => planDetailsForm.Close();

                FlowLayoutPanel centerButtonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false
                };
                centerButtonPanel.Controls.Add(btnClose);
                btnClose.Location = new Point(0, 0);

                mainLayout.Controls.Add(lblPlanName, 0, 0);
                mainLayout.Controls.Add(lblSeparator, 0, 1);
                mainLayout.Controls.Add(centerInvestPanel, 0, 2);
                mainLayout.Controls.Add(lblDetails, 0, 3);
                mainLayout.Controls.Add(centerButtonPanel, 0, 4);

                foreach (Control ctrl in mainLayout.Controls)
                {
                    ctrl.Anchor = AnchorStyles.Top;
                }

                planDetailsForm.Controls.Add(mainLayout);
                planDetailsForm.ShowDialog(this);
            }
        }

        private void LoadCustomers()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT CustomerID, FirstName + ' ' + LastName AS Name FROM Customers ORDER BY FirstName");
            cboCustomer.DataSource = dt;
            cboCustomer.DisplayMember = "Name";
            cboCustomer.ValueMember = "CustomerID";
        }

        private void LoadPlans()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT PlanID, PlanName, MonthlyFee, ServiceLimit, DiscountPercent, DurationMonths FROM SubscriptionPlans WHERE IsActive = 1");
            cboPlan.DataSource = dt;
            cboPlan.DisplayMember = "PlanName";
            cboPlan.ValueMember = "PlanID";
        }

        private void LoadSubscription()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(@"
            SELECT cs.*, sp.MonthlyFee, sp.ServiceLimit, sp.DiscountPercent, sp.DurationMonths
            FROM CustomerSubscriptions cs
            JOIN SubscriptionPlans sp ON cs.PlanID = sp.PlanID
            WHERE cs.SubscriptionID = @ID", new[] { new SqlParameter("@ID", _id) });

            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                cboCustomer.SelectedValue = Convert.ToInt32(r["CustomerID"]);
                cboPlan.SelectedValue = Convert.ToInt32(r["PlanID"]);
                dtpStartDate.Value = Convert.ToDateTime(r["StartDate"]);
                if (r["EndDate"] != DBNull.Value)
                    dtpEndDate.Value = Convert.ToDateTime(r["EndDate"]);
                chkAutoRenew.Checked = Convert.ToBoolean(r["AutoRenew"]);
                cboStatus.Text = r["Status"].ToString();
                txtNotes.Text = r["Notes"].ToString();
            }
        }

        private void Save(object sender, EventArgs e)
        {
            if (cboCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboPlan.SelectedValue == null)
            {
                MessageBox.Show("Please select a subscription plan.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_edit)
                {
                    DatabaseHelper.ExecuteNonQuery(@"
                    UPDATE CustomerSubscriptions 
                    SET PlanID = @PlanID, 
                        EndDate = @EndDate, 
                        Status = @Status, 
                        AutoRenew = @AutoRenew, 
                        Notes = @Notes,
                        UpdatedAt = GETDATE()
                    WHERE SubscriptionID = @ID",
                        new SqlParameter[]
                        {
                        new SqlParameter("@PlanID", cboPlan.SelectedValue),
                        new SqlParameter("@EndDate", dtpEndDate.Value),
                        new SqlParameter("@Status", cboStatus.Text),
                        new SqlParameter("@AutoRenew", chkAutoRenew.Checked ? 1 : 0),
                        new SqlParameter("@Notes", txtNotes.Text.Trim()),
                        new SqlParameter("@ID", _id)
                        });
                    MessageBox.Show("Subscription updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(@"
                    INSERT INTO CustomerSubscriptions (CustomerID, PlanID, StartDate, EndDate, AutoRenew, Status, Notes)
                    VALUES (@CustomerID, @PlanID, @StartDate, @EndDate, @AutoRenew, @Status, @Notes)",
                        new SqlParameter[]
                        {
                        new SqlParameter("@CustomerID", cboCustomer.SelectedValue),
                        new SqlParameter("@PlanID", cboPlan.SelectedValue),
                        new SqlParameter("@StartDate", dtpStartDate.Value),
                        new SqlParameter("@EndDate", dtpEndDate.Value),
                        new SqlParameter("@AutoRenew", chkAutoRenew.Checked ? 1 : 0),
                        new SqlParameter("@Status", "Active"),
                        new SqlParameter("@Notes", txtNotes.Text.Trim())
                        });
                    MessageBox.Show("Subscription created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
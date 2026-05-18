// ══════════════════════════════════════════════════════════════
// USERS PANEL
// ══════════════════════════════════════════════════════════════
using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class UsersPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "UserID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        public UsersPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill; BackColor = Color.FromArgb(245, 245, 245);
            txtSearch = MakeSearchBox("Search by name, username, or role...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new UserForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid(); dgv.DoubleClick += EditClick;
            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Manage Users", txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT UserID, FullName AS [Full Name], Username, Role,
                    CASE WHEN IsActive=1 THEN 'Active' ELSE 'Inactive' END AS [Status],
                    CONVERT(VARCHAR,CreatedAt,107) AS [Created] 
                    FROM Users 
                    WHERE Role IN ('SuperAdmin', 'Admin', 'Staff', 'Technician', 'Supplier')";  // ← Add this filter

                SqlParameter[] p = null;
                if (!string.IsNullOrEmpty(search) && !search.StartsWith("Search"))
                {
                    q += " AND (FullName LIKE @S OR Username LIKE @S OR Role LIKE @S)";
                    p = new[] { new SqlParameter("@S", "%" + search + "%") };
                }
                q += " ORDER BY CreatedAt DESC";
                BindGrid(DatabaseHelper.ExecuteQuery(q, p));
                lblCount.Text = $"{dgv.RowCount} record(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a user to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["UserID"].Value);
            var f = new UserForm(id); if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (dgv.SelectedRows[0].Cells["Role"].Value?.ToString() == "SuperAdmin") { MessageBox.Show("Cannot delete SuperAdmin.", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string name = dgv.SelectedRows[0].Cells["Full Name"].Value?.ToString();
            if (MessageBox.Show($"Delete '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["UserID"].Value);
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserID=@ID", new[] { new SqlParameter("@ID", id) });
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}

// ══════════════════════════════════════════════════════════════
// TECHNICIANS PANEL - CORRECTED WITH DELETE
// ══════════════════════════════════════════════════════════════
namespace AutoTrack.Forms
{
    public class TechniciansPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "TechnicianID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        public TechniciansPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by name or specialization...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);  // Changed from Archive to Delete
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new TechnicianForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;  // Changed from ArchiveClick to DeleteClick
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();
            dgv.DoubleClick += EditClick;
            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Technicians", txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT t.TechnicianID, u.FullName AS [Full Name], u.Username,
                    t.Specialization, t.Level,
                    CASE WHEN t.IsAvailable=1 THEN 'Available' ELSE 'Busy' END AS [Availability],
                    CASE WHEN u.IsActive=1 THEN 'Active' ELSE 'Inactive' END AS [Status],
                    (SELECT COUNT(*) FROM ServiceRecords sr WHERE sr.TechnicianID=t.TechnicianID AND sr.Status IN('Pending','InProgress')) AS [Active Jobs],
                    CONVERT(VARCHAR,t.CreatedAt,107) AS [Added]
                    FROM Technicians t JOIN Users u ON t.UserID=u.UserID";

                SqlParameter[] p = null;
                if (!string.IsNullOrEmpty(search) && !search.StartsWith("Search"))
                {
                    q += " WHERE u.FullName LIKE @S OR t.Specialization LIKE @S";
                    p = new[] { new SqlParameter("@S", "%" + search + "%") };
                }
                q += " ORDER BY u.FullName";

                BindGrid(DatabaseHelper.ExecuteQuery(q, p));
                lblCount.Text = $"{dgv.RowCount} technician(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a technician to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["TechnicianID"].Value);
            var f = new TechnicianForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a technician to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if technician has active jobs
            int activeJobs = Convert.ToInt32(dgv.SelectedRows[0].Cells["Active Jobs"].Value);
            if (activeJobs > 0)
            {
                MessageBox.Show($"Cannot delete this technician.\n\nThey have {activeJobs} active job(s) assigned.\n\nPlease reassign or complete those jobs first.",
                    "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgv.SelectedRows[0].Cells["Full Name"].Value?.ToString();

            if (MessageBox.Show($"Delete technician '{name}'?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["TechnicianID"].Value);

                    // First, unlink from service records (set TechnicianID to NULL)
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE ServiceRecords SET TechnicianID = NULL WHERE TechnicianID = @ID",
                        new[] { new SqlParameter("@ID", id) });

                    // Then delete the technician
                    DatabaseHelper.ExecuteNonQuery(
                        "DELETE FROM Technicians WHERE TechnicianID = @ID",
                        new[] { new SqlParameter("@ID", id) });

                    LoadData();
                    MessageBox.Show("Technician deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

// ══════════════════════════════════════════════════════════════
// SERVICE RECORDS PANEL - FIXED with filter next to search
// ══════════════════════════════════════════════════════════════
namespace AutoTrack.Forms
{
    public class ServiceRecordsPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "ServiceID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private ComboBox cboFilter;
        private Label lblCount;

        private string _userRole = "";
        private int _userId = 0;

        public void SetUserRole(string role) { _userRole = role; }
        public void SetUserId(int id) { _userId = id; }

        public ServiceRecordsPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by job#, plate, or service type...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            // Create filter dropdown
            cboFilter = new ComboBox
            {
                Size = new Size(130, 30),
                Font = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                FlatStyle = FlatStyle.Flat
            };
            cboFilter.Items.AddRange(new object[] { "All", "Pending", "InProgress", "Completed", "Cancelled" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => LoadData();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new ServiceForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            // Create grid using base class
            CreateGrid();
            dgv.DoubleClick += EditClick;

            // Create custom toolbar
            var toolbar = CreateCustomToolbar();

            Controls.Add(dgv);
            Controls.Add(toolbar);
        }

        private Panel CreateCustomToolbar()
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
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            // Title - at top
            var lblTitle = new Label
            {
                Text = "Service Records",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 12),
                AutoSize = true
            };
            toolbar.Controls.Add(lblTitle);

            // Controls Y position (centered vertically in the bottom area)
            int yPos = 56;
            int xPos = 16;

            // 1. Search Box
            txtSearch.Location = new Point(xPos, yPos);
            txtSearch.Size = new Size(200, 30);
            txtSearch.Font = new Font("Segoe UI", 10f);
            toolbar.Controls.Add(txtSearch);
            xPos += txtSearch.Width + 5;

            // 2. Search Button
            btnSearch.Location = new Point(xPos, yPos);
            btnSearch.Size = new Size(80, 30);
            btnSearch.BackColor = Color.FromArgb(60, 60, 60);
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnSearch);
            xPos += btnSearch.Width + 10;

            // 3. Status Filter
            cboFilter.Location = new Point(xPos, yPos);
            cboFilter.Size = new Size(120, 30);
            cboFilter.Font = new Font("Segoe UI", 9f);
            toolbar.Controls.Add(cboFilter);
            xPos += cboFilter.Width + 10;

            // 4. Separator
            var sep1 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep1);
            xPos += sep1.Width + 10;

            // 5. Add Button
            btnAdd.Location = new Point(xPos, yPos);
            btnAdd.Size = new Size(90, 30);
            btnAdd.BackColor = Color.FromArgb(224, 123, 36);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnAdd);
            xPos += btnAdd.Width + 5;

            // 6. Edit Button
            btnEdit.Location = new Point(xPos, yPos);
            btnEdit.Size = new Size(80, 30);
            btnEdit.BackColor = Color.FromArgb(29, 78, 216);
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnEdit);
            xPos += btnEdit.Width + 5;

            // 7. Delete Button
            btnDelete.Location = new Point(xPos, yPos);
            btnDelete.Size = new Size(80, 30);
            btnDelete.BackColor = Color.FromArgb(180, 50, 50);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnDelete);
            xPos += btnDelete.Width + 10;

            // 8. Separator
            var sep2 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep2);
            xPos += sep2.Width + 10;

            // 9. Refresh Button
            btnRefresh.Location = new Point(xPos, yPos);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.BackColor = Color.FromArgb(22, 163, 74);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnRefresh);
            xPos += btnRefresh.Width + 15;

            // 10. Count Label
            lblCount.Location = new Point(xPos, yPos + 6);
            lblCount.Font = new Font("Segoe UI", 9f);
            lblCount.ForeColor = Color.Gray;
            lblCount.AutoSize = true;
            toolbar.Controls.Add(lblCount);

            return toolbar;
        }


        private void LoadData(string search = "")
        {
            try
            {
                string filter = cboFilter?.SelectedItem?.ToString() ?? "All";
                string q = @"SELECT sr.ServiceID, sr.JobOrderNo AS [Job #],
                    v.PlateNumber AS [Plate], v.Make+' '+v.Model AS [Vehicle],
                    sr.ServiceType AS [Service Type], ISNULL(u.FullName, 'Unassigned') AS [Technician],
                    sr.Status, CONVERT(VARCHAR,sr.DateIn,107) AS [Date In],
                    CONVERT(VARCHAR,sr.EstimatedDate,107) AS [Est. Done],
                    sr.LaborCost AS [Labor Cost],
                    sr.PartsCost AS [Parts Cost],
                    sr.Discount,
                    sr.TotalCost AS [Total Cost],
                    sr.FinalAmount AS [Amount]
                    FROM ServiceRecords sr
                    LEFT JOIN Vehicles v ON sr.VehicleID=v.VehicleID
                    LEFT JOIN Technicians t ON sr.TechnicianID=t.TechnicianID
                    LEFT JOIN Users u ON t.UserID=u.UserID
                    WHERE 1=1";

                if (_userRole == "Technician")
                {
                    q += " AND t.UserID = @UserID";
                }

                if (filter != "All") q += $" AND sr.Status='{filter}'";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                if (_userRole == "Technician" && _userId > 0)
                {
                    paramList.Add(new SqlParameter("@UserID", _userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    q += " AND (sr.JobOrderNo LIKE @S OR v.PlateNumber LIKE @S OR sr.ServiceType LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }

                q += " ORDER BY sr.CreatedAt DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());
                BindGrid(dt);
                lblCount.Text = $"{dgv.RowCount} record(s) found";

                if (_userRole == "Technician")
                {
                    btnAdd.Visible = false;
                    btnDelete.Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a record to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ServiceID"].Value);
            var f = new ServiceForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a record to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string job = dgv.SelectedRows[0].Cells["Job #"].Value?.ToString();
            if (MessageBox.Show($"Delete '{job}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ServiceID"].Value);
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM ServiceRecords WHERE ServiceID=@ID",
                        new[] { new SqlParameter("@ID", id) });
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

// ══════════════════════════════════════════════════════════════
// INVENTORY PANEL - ACTIVE ITEMS ONLY
// ══════════════════════════════════════════════════════════════
namespace AutoTrack.Forms
{
    public class InventoryPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "PartID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnArchive, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        private string _userRole = "";
        private int _userId = 0;

        public void SetUserRole(string role) { _userRole = role; }
        public void SetUserId(int id) { _userId = id; }

        public InventoryPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill; BackColor = Color.FromArgb(245, 245, 245);
            txtSearch = MakeSearchBox("Search parts by name or category...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnArchive = MakeButton("Archive", Color.FromArgb(255, 140, 0), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new InventoryForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnArchive.Click += ArchiveClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();
            dgv.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    string st = row.Cells["Stock Status"]?.Value?.ToString();
                    if (st == "Out of Stock") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                    else if (st == "Low Stock") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 200);
                }
            };
            dgv.DoubleClick += EditClick;
            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Active Inventory", txtSearch, btnSearch, btnAdd, btnEdit, btnArchive, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT p.PartID, p.PartName AS [Part Name], p.Category, p.Unit,
                    p.Quantity AS [Qty], p.ReorderLevel AS [Reorder At],
                    p.UnitPrice AS [Unit Price (₱)], s.CompanyName AS [Supplier],
                    CASE WHEN p.Quantity<=0 THEN 'Out of Stock'
                         WHEN p.Quantity<=p.ReorderLevel THEN 'Low Stock'
                         ELSE 'In Stock' END AS [Stock Status]
                    FROM Inventory p 
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    WHERE (p.IsArchived = 0 OR p.IsArchived IS NULL)";  // Only show active items

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                if (_userRole == "Supplier" && _userId > 0)
                {
                    q += " AND p.SupplierID = (SELECT SupplierID FROM Suppliers WHERE ContactPerson = (SELECT FullName FROM Users WHERE UserID = @UserID))";
                    paramList.Add(new SqlParameter("@UserID", _userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    q += " AND (p.PartName LIKE @S OR p.Category LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }
                q += " ORDER BY p.PartName";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());

                if (dgv != null)
                {
                    dgv.DataSource = null;
                    dgv.DataSource = dt;
                    HideColumns();
                }

                lblCount.Text = $"{dgv.RowCount} active item(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ArchiveClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a part to archive.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgv.SelectedRows[0].Cells["Part Name"].Value?.ToString();
            int quantity = 0;

            if (dgv.SelectedRows[0].Cells["Qty"].Value != null && dgv.SelectedRows[0].Cells["Qty"].Value != DBNull.Value)
            {
                quantity = Convert.ToInt32(dgv.SelectedRows[0].Cells["Qty"].Value);
            }

            string message = quantity > 0
                ? $"Part '{name}' has {quantity} units in stock.\n\nArchiving will hide it from inventory but keep records.\n\nContinue?"
                : $"Archive '{name}'? This will hide it from active inventory.";

            if (MessageBox.Show(message, "Archive Part",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["PartID"].Value);
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Inventory SET IsArchived = 1, UpdatedAt = GETDATE() WHERE PartID = @ID",
                        new[] { new SqlParameter("@ID", id) });
                    LoadData();
                    MessageBox.Show("Part archived successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a part to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["PartID"].Value);
            var f = new InventoryForm(id); if (f.ShowDialog() == DialogResult.OK) LoadData();
        }
    }
}

// ══════════════════════════════════════════════════════════════
// SUPPLIERS PANEL
// ══════════════════════════════════════════════════════════════
namespace AutoTrack.Forms
{
    public class SuppliersPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "SupplierID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private Label lblCount;

        private bool _supplierMode = false;
        private int _userId = 0;

        public void SetSupplierMode(bool isSupplier) { _supplierMode = isSupplier; }
        public void SetUserId(int id) { _userId = id; }

        public SuppliersPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill; BackColor = Color.FromArgb(245, 245, 245);
            txtSearch = MakeSearchBox("Search suppliers...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new SupplierForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid(); dgv.DoubleClick += EditClick;
            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Suppliers", txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT SupplierID, CompanyName AS [Company], ContactPerson AS [Contact],
                    Phone, Email, PartsSupplied AS [Parts Supplied],
                    CONVERT(VARCHAR,CreatedAt,107) AS [Added] FROM Suppliers";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                // If supplier mode, only show their own info
                if (_supplierMode && _userId > 0)
                {
                    q += " WHERE ContactPerson = (SELECT FullName FROM Users WHERE UserID = @UserID)";
                    paramList.Add(new SqlParameter("@UserID", _userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    q += (q.Contains("WHERE") ? " AND" : " WHERE") + " (CompanyName LIKE @S OR ContactPerson LIKE @S OR PartsSupplied LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }
                q += " ORDER BY CompanyName";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());

                if (dgv != null)
                {
                    dgv.DataSource = null;
                    dgv.DataSource = dt;
                    HideColumns();
                }

                lblCount.Text = $"{dt.Rows.Count} record(s) found";

                // If supplier mode, restrict editing
                if (_supplierMode)
                {
                    btnAdd.Visible = false;
                    btnDelete.Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a supplier to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SupplierID"].Value);
            var f = new SupplierForm(id); if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a supplier to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string name = dgv.SelectedRows[0].Cells["Company"].Value?.ToString();
            if (MessageBox.Show($"Delete '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SupplierID"].Value);
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE SupplierID=@ID", new[] { new SqlParameter("@ID", id) });
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}

// ══════════════════════════════════════════════════════════════
// REPORTS PANEL
// ══════════════════════════════════════════════════════════════
namespace AutoTrack.Forms
{
    public class ReportsPanel : BaseGridPanel
    {
        private ComboBox cboType, cboPeriod;
        private Button btnGenerate;
        private DataGridView dgv;
        private Panel pnlStats;
        private Label lblV1, lblV2, lblV3, lblV4, lblS1, lblS2, lblS3, lblS4, lblReportTitle;

        public ReportsPanel() { Init(); Generate(); }

        private void Init()
        {
            Dock = DockStyle.Fill; BackColor = Color.FromArgb(245, 245, 245);

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 210, BackColor = Color.FromArgb(245, 245, 245) };

            var lblTitle = new Label
            {
                Text = "Reports & Analytics",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0),
                AutoSize = true
            };

            new Label { Text = "Report:" }.Let(l => { l.Font = new Font("Segoe UI", 9f, FontStyle.Bold); l.ForeColor = Color.FromArgb(60, 60, 60); l.Location = new Point(0, 44); l.AutoSize = true; toolbar.Controls.Add(l); });
            cboType = new ComboBox
            {
                Location = new Point(60, 40),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cboType.Items.AddRange(new object[] { "Service Summary", "Revenue Report", "Customer Report", "Inventory Status", "Technician Performance" });
            cboType.SelectedIndex = 0;

            new Label { Text = "Period:" }.Let(l => { l.Font = new Font("Segoe UI", 9f, FontStyle.Bold); l.ForeColor = Color.FromArgb(60, 60, 60); l.Location = new Point(276, 44); l.AutoSize = true; toolbar.Controls.Add(l); });
            cboPeriod = new ComboBox
            {
                Location = new Point(328, 40),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cboPeriod.Items.AddRange(new object[] { "This Month", "Last Month", "Last 3 Months", "This Year", "All Time" });
            cboPeriod.SelectedIndex = 0;

            btnGenerate = new Button
            {
                Text = "Generate",
                Location = new Point(490, 40),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) => Generate();

            // Stat cards
            pnlStats = new Panel { Location = new Point(0, 80), Size = new Size(880, 90), BackColor = Color.Transparent };
            lblV1 = new Label(); lblS1 = new Label();
            lblV2 = new Label(); lblS2 = new Label();
            lblV3 = new Label(); lblS3 = new Label();
            lblV4 = new Label(); lblS4 = new Label();

            void Card(Label vl, Label nl, string name, int x, Color accent)
            {
                var p = new Panel { Location = new Point(x, 0), Size = new Size(200, 90), BackColor = Color.White };
                p.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(accent), 0, 0, 5, p.Height);
                vl.Text = "0"; vl.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
                vl.ForeColor = Color.FromArgb(30, 30, 30); vl.Location = new Point(14, 10); vl.AutoSize = true;
                nl.Text = name; nl.Font = new Font("Segoe UI", 9f); nl.ForeColor = Color.Gray;
                nl.Location = new Point(14, 56); nl.AutoSize = true;
                p.Controls.Add(vl); p.Controls.Add(nl); pnlStats.Controls.Add(p);
            }
            Card(lblV1, lblS1, "Total Services", 0, Color.FromArgb(224, 123, 36));
            Card(lblV2, lblS2, "Completed", 210, Color.FromArgb(22, 163, 74));
            Card(lblV3, lblS3, "Total Revenue", 420, Color.FromArgb(29, 78, 216));
            Card(lblV4, lblS4, "Total Customers", 630, Color.FromArgb(126, 34, 206));

            lblReportTitle = new Label
            {
                Text = "Results",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 184),
                AutoSize = true
            };

            toolbar.Controls.AddRange(new Control[] { lblTitle, cboType, cboPeriod, btnGenerate, pnlStats, lblReportTitle });

            dgv = CreateGrid();
            Controls.Add(dgv);
            Controls.Add(toolbar);
        }

        private string DateFilter()
        {
            switch (cboPeriod.SelectedItem?.ToString())
            {
                case "This Month": return "AND MONTH(sr.CreatedAt)=MONTH(GETDATE()) AND YEAR(sr.CreatedAt)=YEAR(GETDATE())";
                case "Last Month": return "AND MONTH(sr.CreatedAt)=MONTH(DATEADD(MONTH,-1,GETDATE())) AND YEAR(sr.CreatedAt)=YEAR(DATEADD(MONTH,-1,GETDATE()))";
                case "Last 3 Months": return "AND sr.CreatedAt>=DATEADD(MONTH,-3,GETDATE())";
                case "This Year": return "AND YEAR(sr.CreatedAt)=YEAR(GETDATE())";
                default: return "";
            }
        }

        private void Generate()
        {
            try
            {
                string df = DateFilter();
                string rep = cboType.SelectedItem?.ToString() ?? "Service Summary";

                lblV1.Text = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM ServiceRecords sr WHERE 1=1 {df}")?.ToString() ?? "0";
                lblV2.Text = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM ServiceRecords sr WHERE sr.Status='Completed' {df}")?.ToString() ?? "0";
                lblV3.Text = "₱" + string.Format("{0:N0}", Convert.ToDecimal(DatabaseHelper.ExecuteScalar($"SELECT ISNULL(SUM(p.TotalAmount),0) FROM Payments p JOIN ServiceRecords sr ON p.ServiceID=sr.ServiceID WHERE 1=1 {df.Replace("sr.CreatedAt", "p.PaymentDate")}")));
                lblV4.Text = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Customers")?.ToString() ?? "0";
                lblReportTitle.Text = rep;

                DataTable dt;
                switch (rep)
                {
                    case "Revenue Report":
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT sr.JobOrderNo AS [Job#], v.PlateNumber AS [Plate],
                            c.FirstName+' '+c.LastName AS [Customer], sr.ServiceType AS [Service],
                            p.TotalAmount AS [Amount (₱)], p.PaymentMethod AS [Method],
                            CONVERT(VARCHAR,p.PaymentDate,107) AS [Date]
                            FROM Payments p JOIN ServiceRecords sr ON p.ServiceID=sr.ServiceID
                            JOIN Vehicles v ON sr.VehicleID=v.VehicleID
                            JOIN Customers c ON v.CustomerID=c.CustomerID
                            WHERE 1=1 {df.Replace("sr.CreatedAt", "p.PaymentDate")} ORDER BY p.PaymentDate DESC");
                        break;
                    case "Customer Report":
                        dt = DatabaseHelper.ExecuteQuery(@"SELECT c.FirstName+' '+c.LastName AS [Customer], c.Phone, c.Email,
                            COUNT(DISTINCT v.VehicleID) AS [Vehicles], COUNT(DISTINCT sr.ServiceID) AS [Services],
                            CONVERT(VARCHAR,c.CreatedAt,107) AS [Registered]
                            FROM Customers c LEFT JOIN Vehicles v ON c.CustomerID=v.CustomerID
                            LEFT JOIN ServiceRecords sr ON v.VehicleID=sr.VehicleID
                            GROUP BY c.CustomerID,c.FirstName,c.LastName,c.Phone,c.Email,c.CreatedAt
                            ORDER BY [Services] DESC");
                        break;
                    case "Inventory Status":
                        dt = DatabaseHelper.ExecuteQuery(@"SELECT PartName AS [Part], Category, Unit,
                            Quantity AS [Qty], ReorderLevel AS [Reorder At], UnitPrice AS [Unit Price (₱)],
                            CASE WHEN Quantity<=0 THEN 'Out of Stock' WHEN Quantity<=ReorderLevel THEN 'Low Stock' ELSE 'In Stock' END AS [Status]
                            FROM Inventory ORDER BY [Status], PartName");
                        break;
                    case "Technician Performance":
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT u.FullName AS [Technician],
                            COUNT(sr.ServiceID) AS [Total Jobs],
                            SUM(CASE WHEN sr.Status='Completed' THEN 1 ELSE 0 END) AS [Completed],
                            SUM(CASE WHEN sr.Status='InProgress' THEN 1 ELSE 0 END) AS [In Progress],
                            SUM(CASE WHEN sr.Status='Pending' THEN 1 ELSE 0 END) AS [Pending]
                            FROM Technicians t JOIN Users u ON t.UserID=u.UserID
                            LEFT JOIN ServiceRecords sr ON t.TechnicianID=sr.TechnicianID
                            WHERE 1=1 {df} GROUP BY u.FullName ORDER BY [Total Jobs] DESC");
                        break;
                    default:
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT sr.JobOrderNo AS [Job#],
                            v.PlateNumber AS [Plate], v.Make+' '+v.Model AS [Vehicle],
                            c.FirstName+' '+c.LastName AS [Customer],
                            sr.ServiceType AS [Service Type], u.FullName AS [Technician],
                            sr.Status, CONVERT(VARCHAR,sr.DateIn,107) AS [Date In],
                            CONVERT(VARCHAR,sr.DateCompleted,107) AS [Completed]
                            FROM ServiceRecords sr
                            LEFT JOIN Vehicles v ON sr.VehicleID=v.VehicleID
                            LEFT JOIN Customers c ON v.CustomerID=c.CustomerID
                            LEFT JOIN Technicians t ON sr.TechnicianID=t.TechnicianID
                            LEFT JOIN Users u ON t.UserID=u.UserID
                            WHERE 1=1 {df} ORDER BY sr.CreatedAt DESC");
                        break;
                }
                dgv.DataSource = null;
                dgv.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    // Extension helper
    internal static class LabelExt
    {
        public static Label Let(this Label l, Action<Label> action) { action(l); return l; }
    }
}
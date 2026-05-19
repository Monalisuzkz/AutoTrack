using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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
            btnAdd.Click += (s, e) =>
            {
                // Check role BEFORE creating the form
                string currentRole = SessionManager.CurrentUser?.Role ?? "";

                if (currentRole == "Technician")
                {
                    MessageBox.Show("Technicians cannot create new service records.\nPlease contact an administrator.",
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var f = new ServiceForm();
                if (f.ShowDialog() == DialogResult.OK)
                    LoadData();
            };
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

                // Get current user role from SessionManager
                string currentRole = SessionManager.CurrentUser?.Role ?? "";
                int currentUserId = SessionManager.CurrentUser?.UserID ?? 0;

                if (currentRole == "Technician")
                {
                    q += " AND t.UserID = @UserID";
                }

                if (filter != "All") q += $" AND sr.Status='{filter}'";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                if (currentRole == "Technician" && currentUserId > 0)
                {
                    paramList.Add(new SqlParameter("@UserID", currentUserId));
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

                // ===== ROLE-BASED BUTTON VISIBILITY =====
                if (currentRole == "Technician")
                {
                    // Technicians can only edit (view and update status)
                    btnAdd.Visible = false;
                    btnDelete.Visible = false;
                    btnEdit.Visible = true;
                    btnEdit.Text = "View/Update";
                    btnEdit.Width = 100;
                }
                else if (currentRole == "Staff")
                {
                    // Staff can add and edit, but not delete
                    btnAdd.Visible = true;
                    btnEdit.Visible = true;
                    btnDelete.Visible = false;
                }
                else // Admin or SuperAdmin
                {
                    // Full access
                    btnAdd.Visible = true;
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
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

            // ServiceForm will handle role-based restrictions internally
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

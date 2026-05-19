using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

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

            dgv = CreateGrid();
            dgv.DoubleClick += EditClick;
            Controls.Add(dgv);

            Controls.Add(BuildToolbar("Suppliers",
                txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                // Modified query: Only show suppliers that exist in Users table with role = 'Supplier'
                string q = @"SELECT DISTINCT 
                    s.SupplierID, 
                    s.CompanyName AS [Company], 
                    s.ContactPerson AS [Contact],
                    s.Phone, 
                    s.Email, 
                    s.PartsSupplied AS [Parts Supplied],
                    CONVERT(VARCHAR, s.CreatedAt, 107) AS [Added] 
                    FROM Suppliers s
                    INNER JOIN Users u ON (s.ContactPerson = u.FullName OR s.SupplierID = u.SupplierID)
                    WHERE u.Role = 'Supplier' AND u.IsActive = 1";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                // Supplier mode: only see their own info
                if (_supplierMode && _userId > 0)
                {
                    q += " AND u.UserID = @UserID";
                    paramList.Add(new SqlParameter("@UserID", _userId));
                }

                // Search box filter
                if (!string.IsNullOrEmpty(search))
                {
                    q += " AND (s.CompanyName LIKE @S OR s.ContactPerson LIKE @S OR s.PartsSupplied LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }
                q += " ORDER BY s.CompanyName";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());

                if (dgv != null)
                {
                    dgv.DataSource = null;
                    dgv.DataSource = dt;
                    HideColumns();
                    if (_supplierMode)
                    {
                        dgv.ReadOnly = true;
                        dgv.DoubleClick -= EditClick;
                    }
                }

                lblCount.Text = $"{dt.Rows.Count} record(s) found";

                if (_supplierMode)
                {
                    ApplySupplierUI();
                }
                else
                {
                    ApplyAdminUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Hide controls and disable edit for supplier self-service mode.
        /// </summary>
        private void ApplySupplierUI()
        {
            btnAdd.Visible = false;
            btnDelete.Visible = false;
            btnEdit.Visible = false;
            btnRefresh.Visible = false;
            btnSearch.Visible = false;
            txtSearch.Visible = false;
            lblCount.Visible = true;
            if (dgv != null)
            {
                dgv.ReadOnly = true;
                dgv.DoubleClick -= EditClick;
            }
        }

        /// <summary>
        /// Restore all controls for admin/staff.
        /// </summary>
        private void ApplyAdminUI()
        {
            btnAdd.Visible = true;
            btnDelete.Visible = true;
            btnEdit.Visible = true;
            btnRefresh.Visible = true;
            btnSearch.Visible = true;
            txtSearch.Visible = true;
            lblCount.Visible = true;
            if (dgv != null)
            {
                dgv.ReadOnly = false;
                dgv.DoubleClick -= EditClick; // Remove in case added multiple
                dgv.DoubleClick += EditClick;
            }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a supplier to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SupplierID"].Value);
            var f = new SupplierForm(id);
            if (f.ShowDialog() == DialogResult.OK)
                LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a supplier to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgv.SelectedRows[0].Cells["Company"].Value?.ToString();

            if (MessageBox.Show($"Delete '{name}'?\n\nThis will also remove the associated user account.", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["SupplierID"].Value);

                    // First, get the user associated with this supplier
                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT UserID FROM Users WHERE SupplierID = @SupplierID OR FullName = (SELECT ContactPerson FROM Suppliers WHERE SupplierID = @SupplierID)",
                        new[] { new SqlParameter("@SupplierID", id) });

                    // Delete the supplier
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE SupplierID=@ID", new[] { new SqlParameter("@ID", id) });

                    // Delete the associated user if exists
                    if (userDt.Rows.Count > 0)
                    {
                        int userId = Convert.ToInt32(userDt.Rows[0]["UserID"]);
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserID = @UserID", new[] { new SqlParameter("@UserID", userId) });
                    }

                    LoadData();
                    MessageBox.Show("Supplier and associated user deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
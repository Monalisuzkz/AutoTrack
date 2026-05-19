using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;
using AutoTrack.Helpers;

namespace AutoTrack.Forms
{
    public class InventoryPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "PartID", "SupplierID" };
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnArchive, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;
        private string _userRole = "";
        private int _userId = 0;

        public void SetUserRole(string role) { _userRole = role; }
        public void SetUserId(int id) { _userId = id; }

        public InventoryPanel()
        {
            _userRole = SessionManager.CurrentUser?.Role ?? "";
            _userId = SessionManager.CurrentUser?.UserID ?? 0;
            InitializeControls();
            ApplyRolePermissions();
            LoadData();
        }

        private void ApplyRolePermissions()
        {
            // For Staff role - disable Archive button
            if (_userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                // Hide Archive button completely
                if (btnArchive != null)
                {
                    btnArchive.Visible = false;
                }

                // Keep Add and Edit buttons enabled
                if (btnAdd != null) btnAdd.Enabled = true;
                if (btnEdit != null) btnEdit.Enabled = true;
            }
            else
            {
                // For other roles - enable Archive button
                if (btnArchive != null)
                {
                    btnArchive.Enabled = true;
                    btnArchive.BackColor = Color.FromArgb(180, 80, 80);
                    btnArchive.ForeColor = Color.White;
                    btnArchive.Text = "📦 Archive";
                }
            }
        }

        private void InitializeControls()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search inventory...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnArchive = MakeButton("📦 Archive", Color.FromArgb(180, 80, 80), 90, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) =>
            {
                var f = new InventoryForm();
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };
            btnEdit.Click += EditClick;
            btnArchive.Click += ArchiveClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();
            dgv.DoubleClick += EditClick;

            // Add CellFormatting event for highlighting
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.RowPrePaint += Dgv_RowPrePaint;

            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Inventory", txtSearch, btnSearch, btnAdd, btnEdit, btnArchive, btnRefresh, lblCount));
        }

        private void Dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // Alternative method to highlight entire row based on status
            if (e.RowIndex >= 0 && dgv.Rows[e.RowIndex].Cells["Status"].Value != null)
            {
                string status = dgv.Rows[e.RowIndex].Cells["Status"].Value.ToString();

                if (status == "Low Stock")
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200); // Light Red
                    dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    dgv.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (status == "Out of Stock")
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 150, 150); // Darker Red
                    dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    dgv.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else
                {
                    // Reset to default for normal rows
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = dgv.DefaultCellStyle.BackColor;
                    dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = dgv.DefaultCellStyle.ForeColor;
                    dgv.Rows[e.RowIndex].DefaultCellStyle.Font = dgv.DefaultCellStyle.Font;
                }
            }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Highlight specific cells (Qty and Status columns) based on stock level
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Qty" && e.Value != null)
            {
                int qty = Convert.ToInt32(e.Value);
                int reorderLevel = 0;

                // Get reorder level from the same row
                if (dgv.Rows[e.RowIndex].Cells["Reorder At"].Value != null)
                {
                    reorderLevel = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Reorder At"].Value);
                }

                if (qty <= 0)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 100, 100); // Bright Red
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (qty <= reorderLevel)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 200, 200); // Light Red
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
            }

            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();

                if (status == "Low Stock")
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (status == "Out of Stock")
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 100, 100);
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (status == "In Stock")
                {
                    e.CellStyle.BackColor = Color.FromArgb(200, 255, 200);
                    e.CellStyle.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT p.PartID, p.PartName AS [Part Name], p.Category, p.Unit,
            p.Quantity AS [Qty], p.ReorderLevel AS [Reorder At],
            p.UnitPrice AS [Unit Price (₱)], s.CompanyName AS [Supplier],
            p.SupplierID,
            CASE 
                WHEN p.Quantity <= 0 THEN 'Out of Stock'
                WHEN p.Quantity <= p.ReorderLevel THEN 'Low Stock'
                ELSE 'In Stock' 
            END AS [Status]
            FROM Inventory p 
            LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
            WHERE p.IsArchived = 0";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                // Supplier role: only show items from their own company
                if (_userRole.Equals("Supplier", StringComparison.OrdinalIgnoreCase) && _userId > 0)
                {
                    q += " AND s.SupplierID = (SELECT SupplierID FROM Suppliers WHERE ContactPerson = (SELECT FullName FROM Users WHERE UserID = @UserID))";
                    paramList.Add(new SqlParameter("@UserID", _userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    q += " AND (p.PartName LIKE @S OR p.Category LIKE @S OR s.CompanyName LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }

                // Fixed ORDER BY clause - no line breaks in the middle
                q += " ORDER BY CASE WHEN p.Quantity <= 0 THEN 1 WHEN p.Quantity <= p.ReorderLevel THEN 2 ELSE 3 END, p.PartName";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());

                if (dgv != null)
                {
                    dgv.DataSource = null;
                    dgv.DataSource = dt;
                    HideColumns();

                    // Set column colors
                    if (dgv.Columns["Status"] != null)
                    {
                        dgv.Columns["Status"].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                    }
                    if (dgv.Columns["Qty"] != null)
                    {
                        dgv.Columns["Qty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }

                lblCount.Text = $"{dt.Rows.Count} item(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["PartID"].Value);
            var f = new InventoryForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void ArchiveClick(object sender, EventArgs e)
        {
            // Show message if Staff tries to click (though button is disabled, this is extra safety)
            if (_userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Staff accounts cannot archive inventory items. This button is disabled for your role.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item to archive.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgv.SelectedRows[0].Cells["Part Name"].Value?.ToString();

            if (MessageBox.Show($"Archive '{name}'? This will move it to archived items.", "Confirm Archive",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["PartID"].Value);
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Inventory SET IsArchived = 1, UpdatedAt = GETDATE() WHERE PartID = @ID",
                        new[] { new SqlParameter("@ID", id) });
                    LoadData();
                    MessageBox.Show("Item archived successfully!", "Success",
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
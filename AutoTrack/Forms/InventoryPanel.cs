using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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

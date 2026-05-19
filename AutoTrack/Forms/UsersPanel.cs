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
                    WHERE Role IN ('SuperAdmin', 'Admin', 'Staff', 'Technician', 'Supplier')"; 

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

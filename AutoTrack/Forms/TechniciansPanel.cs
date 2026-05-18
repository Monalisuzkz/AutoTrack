using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

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

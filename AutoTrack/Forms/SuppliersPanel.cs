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

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;
using AutoTrack.Helpers;

namespace AutoTrack.Forms
{
    public class ArchivedInventoryPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "PartID" };
        private TextBox txtSearch;
        private Button btnSearch, btnUnarchive, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        public ArchivedInventoryPanel()
        {
            InitializeControls();
            LoadData();
        }

        public void RefreshData()
        {
            LoadData();
        }

        private void InitializeControls()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search archived parts...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnUnarchive = MakeButton("↺ Unarchive", Color.FromArgb(46, 204, 113), 100, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnUnarchive.Click += UnarchiveClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();

            dgv.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                }
            };

            dgv.DoubleClick += (s, e) => UnarchiveSelected();

            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Archived Items", txtSearch, btnSearch, btnUnarchive, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"SELECT p.PartID, p.PartName AS [Part Name], p.Category, p.Unit,
                    p.Quantity AS [Qty], p.ReorderLevel AS [Reorder At],
                    p.UnitPrice AS [Unit Price (₱)], s.CompanyName AS [Supplier],
                    CONVERT(VARCHAR, p.UpdatedAt, 107) AS [Archived Date],
                    CASE WHEN p.Quantity<=0 THEN 'Out of Stock'
                         WHEN p.Quantity<=p.ReorderLevel THEN 'Low Stock'
                         ELSE 'In Stock' END AS [Status]
                    FROM Inventory p 
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    WHERE p.IsArchived = 1";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrEmpty(search))
                {
                    q += " AND (p.PartName LIKE @S OR p.Category LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }
                q += " ORDER BY p.UpdatedAt DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(q, paramList.ToArray());

                if (dgv != null)
                {
                    dgv.DataSource = null;
                    dgv.DataSource = dt;
                    HideColumns();
                }

                lblCount.Text = $"{dgv.RowCount} archived item(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UnarchiveClick(object sender, EventArgs e)
        {
            UnarchiveSelected();
        }

        private void UnarchiveSelected()
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item to unarchive.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgv.SelectedRows[0].Cells["Part Name"].Value?.ToString();

            if (MessageBox.Show($"Unarchive '{name}'? This will restore it to active inventory.", "Confirm Unarchive",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["PartID"].Value);
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Inventory SET IsArchived = 0, UpdatedAt = GETDATE() WHERE PartID = @ID",
                        new[] { new SqlParameter("@ID", id) });
                    LoadData();
                    MessageBox.Show("Item unarchived successfully!", "Success",
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
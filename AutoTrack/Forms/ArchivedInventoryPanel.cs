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
        private Label lblReadOnlyMessage;
        private string _currentUserRole;

        public ArchivedInventoryPanel()
        {
            _currentUserRole = SessionManager.CurrentUser?.Role ?? "";
            InitializeControls();
            ApplyRolePermissions();
            LoadData();
        }

        public void RefreshData()
        {
            LoadData();
        }

        private void ApplyRolePermissions()
        {
            // For Staff role - hide unarchive button and make grid read-only
            if (_currentUserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                // Hide the unarchive button
                if (btnUnarchive != null)
                {
                    btnUnarchive.Visible = false;
                }

                // Add read-only message label
                if (lblReadOnlyMessage == null)
                {
                    lblReadOnlyMessage = new Label
                    {
                        Text = "🔒 View Only Mode - You cannot unarchive items",
                        ForeColor = Color.FromArgb(180, 80, 80),
                        Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                        AutoSize = true,
                        Location = new Point(350, 8),
                        BackColor = Color.Transparent
                    };
                    this.Controls.Add(lblReadOnlyMessage);

                    // Position it after the toolbar
                    lblReadOnlyMessage.BringToFront();
                }
                else
                {
                    lblReadOnlyMessage.Visible = true;
                }

                // Make grid read-only to prevent any editing
                if (dgv != null)
                {
                    dgv.ReadOnly = true;
                    dgv.AllowUserToAddRows = false;
                    dgv.AllowUserToDeleteRows = false;
                }
            }
            else
            {
                // For other roles, make sure unarchive button is visible
                if (btnUnarchive != null)
                {
                    btnUnarchive.Visible = true;
                }

                if (lblReadOnlyMessage != null)
                {
                    lblReadOnlyMessage.Visible = false;
                }

                if (dgv != null)
                {
                    dgv.ReadOnly = false;
                }
            }
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

            dgv.DoubleClick += (s, e) =>
            {
                // Prevent double-click unarchive for Staff
                if (_currentUserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Staff accounts cannot unarchive items. View only.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                UnarchiveSelected();
            };

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

                    // Re-apply read-only for Staff after data load
                    if (_currentUserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                    {
                        dgv.ReadOnly = true;
                    }
                }

                lblCount.Text = $"{dgv.RowCount} archived item(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UnarchiveClick(object sender, EventArgs e)
        {
            // Check role before allowing unarchive
            if (_currentUserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Staff accounts cannot unarchive items.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UnarchiveSelected();
        }

        private void UnarchiveSelected()
        {
            // Double-check role at execution time
            if (_currentUserRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Staff accounts cannot unarchive items.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
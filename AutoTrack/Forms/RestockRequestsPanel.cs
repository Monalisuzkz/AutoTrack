using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;
using AutoTrack.Helpers;

namespace AutoTrack.Forms
{
    public class RestockRequestsPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new string[0];

        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnRefresh;
        private ComboBox cboStatus;
        private Label lblCount;
        private bool isAdmin = false;

        public RestockRequestsPanel()
        {
            string role = SessionManager.CurrentUser?.Role ?? "";
            isAdmin = role == "SuperAdmin" || role == "Admin";
            bool canCreate = role == "SuperAdmin" || role == "Admin" || role == "Supplier";

            Init();

            // Show/hide Add button based on permissions
            btnAdd.Visible = canCreate;

            LoadData();
        }

        private void Init()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by part name or supplier...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Request Restock", Color.FromArgb(224, 123, 36), 140, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            cboStatus = new ComboBox
            {
                Size = new Size(130, 30),
                Font = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),  // CHANGE: from White
                FlatStyle = FlatStyle.Flat
            };
            cboStatus.Items.AddRange(new object[] { "All", "Pending", "Approved", "Delivered", "Cancelled" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += (s, e) => LoadData();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += BtnAdd_Click;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            CreateGrid();
            dgv.CellFormatting += Dgv_CellFormatting;

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
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Restock Requests",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 12),
                AutoSize = true
            };
            toolbar.Controls.Add(lblTitle);

            int yPos = 56;
            int xPos = 16;

            // Search Box
            txtSearch.Location = new Point(xPos, yPos);
            txtSearch.Size = new Size(200, 30);
            txtSearch.Font = new Font("Segoe UI", 10f);
            toolbar.Controls.Add(txtSearch);
            xPos += txtSearch.Width + 5;

            // Search Button
            btnSearch.Location = new Point(xPos, yPos);
            btnSearch.Size = new Size(80, 30);
            btnSearch.BackColor = Color.FromArgb(60, 60, 60);
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnSearch);
            xPos += btnSearch.Width + 10;

            // Status Filter
            cboStatus.Location = new Point(xPos, yPos);
            cboStatus.Size = new Size(120, 30);
            cboStatus.Font = new Font("Segoe UI", 9f);
            toolbar.Controls.Add(cboStatus);
            xPos += cboStatus.Width + 10;

            // Separator
            var sep1 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep1);
            xPos += sep1.Width + 10;

            // Add Button
            btnAdd.Location = new Point(xPos, yPos);
            btnAdd.Size = new Size(140, 30);
            btnAdd.BackColor = Color.FromArgb(224, 123, 36);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnAdd);
            xPos += btnAdd.Width + 10;

            // Separator
            var sep2 = new Panel
            {
                Location = new Point(xPos, yPos + 2),
                Size = new Size(1, 26),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            toolbar.Controls.Add(sep2);
            xPos += sep2.Width + 10;

            // Refresh Button
            btnRefresh.Location = new Point(xPos, yPos);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.BackColor = Color.FromArgb(22, 163, 74);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            toolbar.Controls.Add(btnRefresh);
            xPos += btnRefresh.Width + 15;

            // Count Label
            lblCount.Location = new Point(xPos, yPos + 6);
            lblCount.Font = new Font("Segoe UI", 9f);
            lblCount.ForeColor = Color.Gray;
            lblCount.AutoSize = true;
            toolbar.Controls.Add(lblCount);

            return toolbar;
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Pending":
                        e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                        e.CellStyle.ForeColor = Color.FromArgb(133, 100, 4);
                        break;
                    case "Approved":
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);
                        break;
                    case "Delivered":
                        e.CellStyle.BackColor = Color.FromArgb(219, 234, 254);
                        e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                        break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        break;
                }
            }
        }

        private void LoadData(string search = "")
        {
            try
            {
                string statusFilter = cboStatus.SelectedItem?.ToString() == "All" ? "" : cboStatus.SelectedItem?.ToString();
                string role = SessionManager.CurrentUser?.Role ?? "";
                int userId = SessionManager.CurrentUser?.UserID ?? 0;

                string query = @"
                    SELECT 
                        rr.RestockID,
                        rr.PartID,
                        p.PartName AS PartName,
                        ISNULL(s.CompanyName, 'N/A') AS Supplier,
                        u.FullName AS RequestedBy,
                        rr.QuantityRequested AS Quantity,
                        rr.Status,
                        CONVERT(VARCHAR, rr.RequestDate, 107) AS RequestDate,
                        CONVERT(VARCHAR, rr.DeliveryDate, 107) AS DeliveryDate,
                        ISNULL(rr.Notes, '') AS Notes
                    FROM RestockRequests rr
                    JOIN Inventory p ON rr.PartID = p.PartID
                    LEFT JOIN Suppliers s ON rr.SupplierID = s.SupplierID
                    LEFT JOIN Users u ON rr.RequestedBy = u.UserID
                    WHERE 1=1";

                var paramList = new System.Collections.Generic.List<SqlParameter>();

                if (role == "Supplier")
                {
                    query += " AND p.SupplierID = (SELECT SupplierID FROM Suppliers WHERE ContactPerson = (SELECT FullName FROM Users WHERE UserID = @UserID))";
                    paramList.Add(new SqlParameter("@UserID", userId));
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query += " AND rr.Status = @Status";
                    paramList.Add(new SqlParameter("@Status", statusFilter));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query += " AND (p.PartName LIKE @S OR s.CompanyName LIKE @S)";
                    paramList.Add(new SqlParameter("@S", "%" + search + "%"));
                }

                query += " ORDER BY rr.RequestDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, paramList.ToArray());
                BindGrid(dt);
                lblCount.Text = $"{dgv.RowCount} request(s) found";

                if (isAdmin)
                {
                    AddActionButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddActionButtons()
        {
            if (dgv.Columns.Contains("Actions"))
            {
                dgv.Columns.Remove("Actions");
            }

            DataGridViewButtonColumn actionCol = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "Action",
                Text = "Approve",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgv.Columns.Add(actionCol);

            dgv.CellClick -= Dgv_CellClick;
            dgv.CellClick += Dgv_CellClick;
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "Actions") return;

            try
            {
                DataGridViewRow row = dgv.Rows[e.RowIndex];

                if (row.Cells["RestockID"].Value == null)
                {
                    MessageBox.Show("Invalid row data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int restockId = Convert.ToInt32(row.Cells["RestockID"].Value);
                string currentStatus = row.Cells["Status"].Value.ToString();

                // PENDING -> APPROVED
                if (currentStatus == "Pending")
                {
                    var result = MessageBox.Show("Approve this restock request?\n\nThis will mark the request as approved.\n\nWhen goods arrive, click 'Deliver' to update inventory.",
                        "Approve Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Check if already approved (prevent duplicate)
                        DataTable checkDt = DatabaseHelper.ExecuteQuery(
                            "SELECT Status FROM RestockRequests WHERE RestockID = @ID",
                            new[] { new SqlParameter("@ID", restockId) });

                        if (checkDt.Rows.Count > 0 && checkDt.Rows[0]["Status"].ToString() == "Pending")
                        {
                            string updateQuery = "UPDATE RestockRequests SET Status = 'Approved', UpdatedAt = GETDATE() WHERE RestockID = @ID";
                            DatabaseHelper.ExecuteNonQuery(updateQuery, new[] { new SqlParameter("@ID", restockId) });

                            LoadData();
                            MessageBox.Show("Request approved! Click 'Deliver' when goods arrive to update inventory.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("This request has already been processed.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                // APPROVED -> DELIVERED (add to inventory)
                else if (currentStatus == "Approved")
                {
                    var result = MessageBox.Show("Mark this request as delivered?\n\nThis will add the quantity to inventory and mark as delivered.\n\nThis action cannot be undone.",
                        "Mark Delivered", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (row.Cells["PartID"].Value == null || row.Cells["Quantity"].Value == null)
                        {
                            MessageBox.Show("Missing part information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int partId = Convert.ToInt32(row.Cells["PartID"].Value);
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                        // Check if already delivered (prevent duplicate)
                        DataTable checkDt = DatabaseHelper.ExecuteQuery(
                            "SELECT Status FROM RestockRequests WHERE RestockID = @ID",
                            new[] { new SqlParameter("@ID", restockId) });

                        if (checkDt.Rows.Count > 0 && checkDt.Rows[0]["Status"].ToString() == "Approved")
                        {
                            // First, check current inventory quantity
                            DataTable inventoryDt = DatabaseHelper.ExecuteQuery(
                                "SELECT Quantity FROM Inventory WHERE PartID = @PID",
                                new[] { new SqlParameter("@PID", partId) });

                            int oldQuantity = 0;
                            if (inventoryDt.Rows.Count > 0)
                            {
                                oldQuantity = Convert.ToInt32(inventoryDt.Rows[0]["Quantity"]);
                            }

                            // Update inventory quantity
                            string inventoryQuery = "UPDATE Inventory SET Quantity = Quantity + @Qty, UpdatedAt = GETDATE() WHERE PartID = @PID";
                            DatabaseHelper.ExecuteNonQuery(inventoryQuery, new[]
                            {
                        new SqlParameter("@Qty", quantity),
                        new SqlParameter("@PID", partId)
                    });

                            // Update restock request status to Delivered
                            string updateQuery = "UPDATE RestockRequests SET Status = 'Delivered', DeliveryDate = GETDATE(), UpdatedAt = GETDATE() WHERE RestockID = @ID";
                            DatabaseHelper.ExecuteNonQuery(updateQuery, new[] { new SqlParameter("@ID", restockId) });

                            int newQuantity = oldQuantity + quantity;

                            LoadData();
                            MessageBox.Show($"Request marked as delivered!\n\nAdded {quantity} units to inventory.\n\nStock updated from {oldQuantity} to {newQuantity}.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("This request has already been delivered or is not approved.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else if (currentStatus == "Delivered")
                {
                    MessageBox.Show("This request has already been delivered.\n\nInventory has already been updated.",
                        "Already Processed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (currentStatus == "Cancelled")
                {
                    MessageBox.Show("This request has been cancelled and cannot be modified.",
                        "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new RestockRequestForm();
            if (f.ShowDialog() == DialogResult.OK)
                LoadData();
        }
    }

    // COMPLETELY FIXED Restock Request Form
    // COMPLETELY FIXED Restock Request Form with better supplier loading
    public class RestockRequestForm : Form
    {
        private ComboBox cboPart, cboSupplier;
        private NumericUpDown nudQuantity;
        private TextBox txtNotes;
        private Button btnSave, btnCancel;
        private DataTable partsTable;

        public RestockRequestForm()
        {
            Init();
            LoadParts();
        }

        private void Init()
        {
            Text = "Request Restock";
            Size = new Size(480, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);  // CHANGE: from White
            Font = new Font("Segoe UI", 9f);

            var lblTitle = new Label
            {
                Text = "Request Restock",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 16),
                AutoSize = true
            };

            var lblPart = new Label { Text = "Part to Restock:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboPart = new ComboBox
            {
                Location = new Point(20, 82),
                Size = new Size(420, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(245, 245, 245),  // Match background
                FlatStyle = FlatStyle.Flat
            };
            cboPart.SelectedIndexChanged += CboPart_SelectedIndexChanged;

            var lblSupplier = new Label { Text = "Supplier:", Location = new Point(20, 124), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cboSupplier = new ComboBox
            {
                Location = new Point(20, 146),
                Size = new Size(420, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(245, 245, 245),  // Match background
                FlatStyle = FlatStyle.Flat
            };

            var lblQuantity = new Label { Text = "Quantity:", Location = new Point(20, 188), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            nudQuantity = new NumericUpDown { Location = new Point(20, 210), Size = new Size(120, 30), Minimum = 1, Maximum = 10000, Value = 1, Font = new Font("Segoe UI", 10f) };

            var lblNotes = new Label { Text = "Notes (Optional):", Location = new Point(20, 252), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtNotes = new TextBox
            {
                Location = new Point(20, 274),
                Size = new Size(420, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)  // Match background
            };

            btnSave = new Button
            {
                Text = "Submit Request",
                Location = new Point(20, 360),
                Size = new Size(140, 38),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(170, 360),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] {
                lblTitle, lblPart, cboPart, lblSupplier, cboSupplier,
                lblQuantity, nudQuantity, lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private void LoadParts()
        {
            try
            {
                // Load parts that need restocking with their supplier info
                string query = @"
                SELECT DISTINCT 
                    p.PartID, 
                    p.PartName,
                    s.SupplierID,
                    s.CompanyName
                FROM Inventory p
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE (p.Quantity <= p.ReorderLevel OR p.Quantity = 0)
                ORDER BY p.PartName";

                partsTable = DatabaseHelper.ExecuteQuery(query);

                // Display part name with supplier info
                DataTable displayTable = new DataTable();
                displayTable.Columns.Add("PartID", typeof(int));
                displayTable.Columns.Add("DisplayName", typeof(string));

                foreach (DataRow row in partsTable.Rows)
                {
                    int partId = Convert.ToInt32(row["PartID"]);
                    string partName = row["PartName"].ToString();
                    string supplierName = row["CompanyName"] != DBNull.Value ? row["CompanyName"].ToString() : "No Supplier";

                    displayTable.Rows.Add(partId, $"{partName} (Supplier: {supplierName})");
                }

                cboPart.DataSource = displayTable;
                cboPart.DisplayMember = "DisplayName";
                cboPart.ValueMember = "PartID";

                if (displayTable.Rows.Count == 0)
                {
                    MessageBox.Show("No parts need restocking at this time.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnSave.Enabled = false;
                }
                else
                {
                    btnSave.Enabled = true;
                    if (cboPart.Items.Count > 0)
                        cboPart.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parts: " + ex.Message);
            }
        }

        private void CboPart_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSupplier.DataSource = null;
            cboSupplier.Items.Clear();

            if (cboPart.SelectedIndex < 0 || cboPart.SelectedItem == null)
                return;

            try
            {
                int partId = 0;

                // Get the PartID from the selected item
                DataRowView selectedItem = cboPart.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    partId = Convert.ToInt32(selectedItem["PartID"]);
                }
                else if (cboPart.SelectedValue != null)
                {
                    partId = Convert.ToInt32(cboPart.SelectedValue);
                }

                if (partId > 0)
                {
                    // Get the supplier from the partsTable
                    DataRow[] rows = partsTable.Select($"PartID = {partId}");

                    if (rows.Length > 0)
                    {
                        int supplierId = rows[0]["SupplierID"] != DBNull.Value ? Convert.ToInt32(rows[0]["SupplierID"]) : 0;
                        string supplierName = rows[0]["CompanyName"] != DBNull.Value ? rows[0]["CompanyName"].ToString() : "";

                        if (supplierId > 0 && !string.IsNullOrEmpty(supplierName))
                        {
                            // Create a single supplier entry
                            DataTable supplierTable = new DataTable();
                            supplierTable.Columns.Add("SupplierID", typeof(int));
                            supplierTable.Columns.Add("CompanyName", typeof(string));
                            supplierTable.Rows.Add(supplierId, supplierName);

                            cboSupplier.DataSource = supplierTable;
                            cboSupplier.DisplayMember = "CompanyName";
                            cboSupplier.ValueMember = "SupplierID";
                            cboSupplier.SelectedIndex = 0;
                        }
                        else
                        {
                            cboSupplier.Text = "No supplier assigned to this part";
                            cboSupplier.Enabled = false;
                        }
                    }
                    else
                    {
                        cboSupplier.Text = "No supplier found";
                        cboSupplier.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading supplier: " + ex.Message);
                cboSupplier.Text = "Error loading supplier";
            }
        }

        private void Save(object sender, EventArgs e)
        {
            // Validate part selection
            if (cboPart.SelectedIndex < 0 || cboPart.SelectedItem == null)
            {
                MessageBox.Show("Please select a part.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = SessionManager.CurrentUser?.UserID ?? 1;

            // Get the PartID
            int partId = 0;
            DataRowView selectedPart = cboPart.SelectedItem as DataRowView;
            if (selectedPart != null)
            {
                partId = Convert.ToInt32(selectedPart["PartID"]);
            }
            else if (cboPart.SelectedValue != null)
            {
                partId = Convert.ToInt32(cboPart.SelectedValue);
            }

            // ========== DUPLICATE CHECK ==========
            // Check if there's already a Pending or Approved request for this part
            DataTable checkDt = DatabaseHelper.ExecuteQuery(
                @"SELECT RestockID, Status FROM RestockRequests 
          WHERE PartID = @PID AND Status IN ('Pending', 'Approved')",
                new[] { new SqlParameter("@PID", partId) });

            if (checkDt.Rows.Count > 0)
            {
                string existingStatus = checkDt.Rows[0]["Status"].ToString();
                MessageBox.Show($"Cannot create new restock request.\n\nA {existingStatus} request for this part already exists.\n\nPlease wait for the current request to be completed before creating a new one.",
                    "Duplicate Request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // =====================================

            // Get SupplierID
            object supplierId = DBNull.Value;
            if (cboSupplier.SelectedIndex >= 0 && cboSupplier.SelectedItem != null)
            {
                DataRowView selectedSupplier = cboSupplier.SelectedItem as DataRowView;
                if (selectedSupplier != null && selectedSupplier["SupplierID"] != DBNull.Value)
                {
                    supplierId = Convert.ToInt32(selectedSupplier["SupplierID"]);
                }
                else if (cboSupplier.SelectedValue != null && cboSupplier.SelectedValue != DBNull.Value)
                {
                    supplierId = Convert.ToInt32(cboSupplier.SelectedValue);
                }
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    @"INSERT INTO RestockRequests (PartID, SupplierID, RequestedBy, QuantityRequested, Status, RequestDate, Notes)
              VALUES (@PID, @SID, @By, @Qty, 'Pending', GETDATE(), @Notes)",
                    new SqlParameter[]
                    {
                new SqlParameter("@PID", partId),
                new SqlParameter("@SID", supplierId),
                new SqlParameter("@By", userId),
                new SqlParameter("@Qty", nudQuantity.Value),
                new SqlParameter("@Notes", txtNotes.Text.Trim())
                    });

                MessageBox.Show("Restock request submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}

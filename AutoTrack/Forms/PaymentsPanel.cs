using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;

namespace AutoTrack.Forms
{
    public class PaymentsPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "PaymentID", "ServiceID" };

        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;


        public PaymentsPanel() { Init(); LoadData(); }

        private void Init()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by receipt#, customer, or method...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Process Payment", Color.FromArgb(224, 123, 36), 140, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += BtnAdd_Click;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            // Grid with scrollbar
            dgv = CreateGrid();

            Controls.Add(dgv);
            Controls.Add(BuildToolbar("Payments", txtSearch, btnSearch, btnAdd, btnRefresh, lblCount));
        }

        private void LoadData(string search = "")
        {
            try
            {
                string q = @"
            SELECT
                p.PaymentID,
                p.ReceiptNo                          AS [Receipt #],
                sr.JobOrderNo                        AS [Job #],
                c.FirstName + ' ' + c.LastName       AS [Customer],
                v.PlateNumber                        AS [Plate],
                p.TotalAmount                        AS [Amount (₱)],
                p.AmountTendered                     AS [Tendered (₱)],
                p.Change                             AS [Change (₱)],
                p.PaymentMethod                      AS [Method],
                u.FullName                           AS [Processed By],
                CONVERT(VARCHAR, p.PaymentDate, 107) AS [Date]
            FROM Payments p
            JOIN ServiceRecords sr ON p.ServiceID = sr.ServiceID
            JOIN Vehicles v ON sr.VehicleID = v.VehicleID
            JOIN Customers c ON v.CustomerID = c.CustomerID
            LEFT JOIN Users u ON p.ProcessedBy = u.UserID
            WHERE 1=1";


                SqlParameter[] prms = null;
                if (!string.IsNullOrEmpty(search) && !search.StartsWith("Search"))
                {
                    q += @" AND (p.ReceiptNo LIKE @S
                               OR c.FirstName  LIKE @S
                               OR c.LastName   LIKE @S
                               OR p.PaymentMethod LIKE @S
                               OR sr.JobOrderNo   LIKE @S)";
                    prms = new[] { new SqlParameter("@S", "%" + search + "%") };
                }
                q += " ORDER BY p.PaymentDate DESC";

                BindGrid(DatabaseHelper.ExecuteQuery(q, prms));
                lblCount.Text = $"{dgv.RowCount} payment(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new PaymentForm();
            if (f.ShowDialog() == DialogResult.OK)
                LoadData();
        }
    }

    /// ──────────────────────────────────────────────────────────
    // PAYMENT FORM - COMPLETE FIXED VERSION
    // ──────────────────────────────────────────────────────────
    public class PaymentForm : Form
    {
        private Label lblTitle, lblJob, lblAmount, lblTendered, lblMethod;
        private ComboBox cboJob, cboMethod;
        private TextBox txtAmount, txtTendered;
        private Label lblChange, lblChangeVal;
        private Button btnSave, btnCancel;
        private CheckBox chkManualAmount;

        public PaymentForm()
        {
            Init();
            LoadCompletedJobs();
        }

        private void Init()
        {
            Text = "Process Payment";
            Size = new Size(480, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = "Process Payment",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 16),
                AutoSize = true
            };

            Label Lbl(string t, int x, int y) => new Label
            {
                Text = t,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(x, y),
                AutoSize = true
            };

            lblJob = Lbl("Select Completed Job Order", 20, 56);
            cboJob = new ComboBox
            {
                Location = new Point(20, 76),
                Size = new Size(430, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                MaxDropDownItems = 15,
                IntegralHeight = false,
                DropDownHeight = 300
            };
            cboJob.SelectedIndexChanged += CboJob_SelectedIndexChanged;

            lblAmount = Lbl("Total Amount (₱)", 20, 116);
            txtAmount = new TextBox
            {
                Location = new Point(20, 136),
                Size = new Size(180, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245),
                ReadOnly = true
            };

            lblTendered = Lbl("Amount Tendered (₱)", 230, 116);
            txtTendered = new TextBox
            {
                Location = new Point(230, 136),
                Size = new Size(220, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            txtTendered.TextChanged += (s, e) => UpdateChange();

            lblChange = Lbl("Change (₱)", 20, 176);
            lblChangeVal = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                Location = new Point(20, 196),
                AutoSize = true
            };

            lblMethod = Lbl("Payment Method", 20, 236);
            cboMethod = new ComboBox
            {
                Location = new Point(20, 256),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            cboMethod.Items.AddRange(new object[] { "Cash", "GCash", "Card", "Bank Transfer" });
            cboMethod.SelectedIndex = 0;

            chkManualAmount = new CheckBox
            {
                Text = "Manual Amount Entry",
                Location = new Point(20, 296),
                AutoSize = true,
                Checked = false
            };
            chkManualAmount.CheckedChanged += (s, e) =>
            {
                txtAmount.ReadOnly = !chkManualAmount.Checked;
                if (chkManualAmount.Checked)
                {
                    txtAmount.BackColor = Color.FromArgb(255, 255, 200);
                    txtAmount.Text = "0.00";
                    txtAmount.Focus();
                }
                else
                {
                    txtAmount.BackColor = Color.FromArgb(245, 245, 245);
                    if (cboJob.SelectedIndex >= 0)
                        CboJob_SelectedIndexChanged(null, null);
                }
                UpdateChange();
            };

            btnSave = new Button
            {
                Text = "Confirm Payment",
                Location = new Point(20, 340),
                Size = new Size(150, 36),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(180, 340),
                Size = new Size(100, 36),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
            lblTitle, lblJob, cboJob,
            lblAmount, txtAmount, lblTendered, txtTendered,
            lblChange, lblChangeVal,
            lblMethod, cboMethod,
            chkManualAmount,
            btnSave, btnCancel
            });
        }

        private void LoadCompletedJobs()
        {
            try
            {
                string query = @"
                SELECT 
                    sr.ServiceID,
                    sr.JobOrderNo,
                    v.PlateNumber,
                    c.FirstName,
                    c.LastName,
                    ISNULL(sr.FinalAmount, 0) AS Amount
                FROM ServiceRecords sr
                JOIN Vehicles v ON sr.VehicleID = v.VehicleID
                JOIN Customers c ON v.CustomerID = c.CustomerID
                WHERE sr.Status = 'Completed'
                  AND sr.ServiceID NOT IN (SELECT ISNULL(ServiceID, 0) FROM Payments)
                ORDER BY sr.DateIn DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No completed unpaid job orders found.\n\n" +
                        "Make sure:\n" +
                        "1. Services have Status = 'Completed'\n" +
                        "2. Services are not already in Payments table\n" +
                        "3. FinalAmount is greater than 0",
                        "No Jobs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create display column
                dt.Columns.Add("Display", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string jobNo = row["JobOrderNo"].ToString();
                    string plate = row["PlateNumber"].ToString();
                    string firstName = row["FirstName"].ToString();
                    string lastName = row["LastName"].ToString();
                    decimal amount = Convert.ToDecimal(row["Amount"]);

                    row["Display"] = $"{jobNo} — {plate} ({firstName} {lastName}) - ₱{amount:N2}";
                }

                cboJob.DataSource = dt;
                cboJob.DisplayMember = "Display";
                cboJob.ValueMember = "ServiceID";
                cboJob.DropDownStyle = ComboBoxStyle.DropDownList;

                if (cboJob.Items.Count > 0)
                {
                    cboJob.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading jobs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboJob_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chkManualAmount != null && chkManualAmount.Checked) return;

            if (cboJob.SelectedItem == null)
            {
                txtAmount.Text = "0.00";
                return;
            }

            try
            {
                int serviceId = 0;

                // Get ServiceID from selected item
                if (cboJob.SelectedItem is DataRowView rowView)
                {
                    serviceId = Convert.ToInt32(rowView["ServiceID"]);
                }
                else if (cboJob.SelectedValue != null)
                {
                    serviceId = Convert.ToInt32(cboJob.SelectedValue);
                }

                if (serviceId == 0)
                {
                    txtAmount.Text = "0.00";
                    return;
                }

                // Get FinalAmount from ServiceRecords
                string query = @"
                SELECT ISNULL(FinalAmount, 0) AS TotalAmount
                FROM ServiceRecords 
                WHERE ServiceID = @ServiceID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@ServiceID", serviceId)
                };

                object total = DatabaseHelper.ExecuteScalar(query, parameters);
                decimal totalAmount = total != DBNull.Value ? Convert.ToDecimal(total) : 0;

                txtAmount.Text = totalAmount.ToString("F2");
                UpdateChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating amount: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAmount.Text = "0.00";
            }
        }

        private void UpdateChange()
        {
            if (decimal.TryParse(txtAmount.Text, out decimal amt)
             && decimal.TryParse(txtTendered.Text, out decimal ten))
            {
                decimal change = ten - amt;
                lblChangeVal.Text = $"₱{change:N2}";
                lblChangeVal.ForeColor = change >= 0
                    ? Color.FromArgb(22, 163, 74)
                    : Color.FromArgb(180, 50, 50);
            }
        }

        private void Save(object sender, EventArgs e)
        {
            if (cboJob.SelectedItem == null)
            {
                MessageBox.Show("Please select a job order.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int serviceId = 0;
            if (cboJob.SelectedItem is DataRowView rowView)
            {
                serviceId = Convert.ToInt32(rowView["ServiceID"]);
            }
            else if (cboJob.SelectedValue != null)
            {
                serviceId = Convert.ToInt32(cboJob.SelectedValue);
            }

            if (serviceId == 0)
            {
                MessageBox.Show("Invalid job order selected.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal total)
             || !decimal.TryParse(txtTendered.Text, out decimal tendered))
            {
                MessageBox.Show("Enter valid amounts.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (tendered < total)
            {
                MessageBox.Show("Amount tendered cannot be less than total.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int userId = AutoTrack.Helpers.SessionManager.CurrentUser?.UserID ?? 1;

                string query = @"
                INSERT INTO Payments (ServiceID, ProcessedBy, TotalAmount, AmountTendered, PaymentMethod, PaymentDate)
                VALUES (@SID, @By, @Total, @Tendered, @Method, @Date)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@SID", serviceId),
                new SqlParameter("@By", userId),
                new SqlParameter("@Total", total),
                new SqlParameter("@Tendered", tendered),
                new SqlParameter("@Method", cboMethod.Text),
                new SqlParameter("@Date", DateTime.Now)
                };

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    object idResult = DatabaseHelper.ExecuteScalar("SELECT SCOPE_IDENTITY()");
                    int paymentId = (idResult != null && idResult != DBNull.Value) ? Convert.ToInt32(idResult) : 0;
                    string receiptNo = paymentId > 0 ? $"RCP-{paymentId:D4}" : "Pending";

                    MessageBox.Show($"Payment recorded successfully!\nReceipt No: {receiptNo}\nChange: ₱{(tendered - total):N2}",
                        "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("No rows were inserted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class ServiceForm : Form
    {
        private int _id = 0;
        private bool _edit = false;

        // Controls
        private Label lblTitle;
        private ComboBox cboVehicle, cboTech, cboStatus, cboAssignedBy;
        private ComboBox cboServiceType;
        private TextBox txtNotes, txtDescription, txtJobOrderNo;
        private TextBox txtLaborCost, txtPartsCost, txtDiscount;
        private Label lblTotalAmount;
        private DateTimePicker dtpDateIn, dtpEstDate, dtpDateCompleted;
        private Button btnSave, btnCancel, btnPrint;

        public ServiceForm(int id = 0)
        {
            _id = id;
            _edit = id > 0;
            Init();
            LoadVehicles();
            LoadTechs();
            LoadServiceTypes();
            LoadAssignedBy();
            if (_edit) LoadService();
        }

        private void Init()
        {
            Text = _edit ? "Edit Service Record" : "New Service Record";
            Size = new Size(760, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            int labelX = 20;
            int fieldX = 130;
            int fieldWidth = 250;
            int rowHeight = 35;
            int currentY = 60;

            // Title
            lblTitle = new Label
            {
                Text = _edit ? "Edit Service Record" : "New Service Record",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 16),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            Label Lbl(string text, int x, int y) => new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(x, y),
                AutoSize = true
            };

            // Row 1: Vehicle
            var lblVehicle = Lbl("Vehicle:", labelX, currentY);
            cboVehicle = new ComboBox
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(480, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            Controls.Add(lblVehicle);
            Controls.Add(cboVehicle);
            currentY += rowHeight;

            // Row 2: Description
            var lblDesc = Lbl("Description:", labelX, currentY);
            txtDescription = new TextBox
            {
                Location = new Point(labelX, currentY + 20),
                Size = new Size(600, 50),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };
            Controls.Add(lblDesc);
            Controls.Add(txtDescription);
            currentY += 90;

            // Row 3: Service Type
            var lblType = Lbl("Service Type:", labelX, currentY);
            cboServiceType = new ComboBox
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(300, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            Controls.Add(lblType);
            Controls.Add(cboServiceType);
            currentY += rowHeight;

            // Row 4: Technician and Assigned By (side by side)
            var lblTech = Lbl("Technician:", labelX, currentY);
            cboTech = new ComboBox
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(220, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var lblAssigned = Lbl("Assigned By:", fieldX + 240, currentY);
            cboAssignedBy = new ComboBox
            {
                Location = new Point(fieldX + 330, currentY - 3),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            Controls.Add(lblTech);
            Controls.Add(cboTech);
            Controls.Add(lblAssigned);
            Controls.Add(cboAssignedBy);
            currentY += rowHeight;

            // Row 5: Status
            var lblStatus = Lbl("Status:", labelX, currentY);
            cboStatus = new ComboBox
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            cboStatus.Items.AddRange(new object[] { "Pending", "InProgress", "Completed", "Cancelled" });
            Controls.Add(lblStatus);
            Controls.Add(cboStatus);
            currentY += rowHeight;

            // Row 6: Dates (Date In and Est Date side by side)
            var lblDateIn = Lbl("Date In:", labelX, currentY);
            dtpDateIn = new DateTimePicker
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(130, 28),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            var lblEstDate = Lbl("Est. Date:", fieldX + 150, currentY);
            dtpEstDate = new DateTimePicker
            {
                Location = new Point(fieldX + 230, currentY - 3),
                Size = new Size(130, 28),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(3)
            };
            Controls.Add(lblDateIn);
            Controls.Add(dtpDateIn);
            Controls.Add(lblEstDate);
            Controls.Add(dtpEstDate);
            currentY += rowHeight;

            // Row 7: Date Completed (alone)
            var lblDateComp = Lbl("Date Completed:", labelX, currentY);
            dtpDateCompleted = new DateTimePicker
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(130, 28),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            Controls.Add(lblDateComp);
            Controls.Add(dtpDateCompleted);
            currentY += rowHeight;

            // Row 8: Price Fields
            var lblLabor = Lbl("Labor Cost (₱):", labelX, currentY);
            txtLaborCost = new TextBox
            {
                Location = new Point(fieldX, currentY - 3),
                Size = new Size(120, 28),
                Text = "0",
                Font = new Font("Segoe UI", 10f),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245),
                TextAlign = HorizontalAlignment.Right
            };

            var lblParts = Lbl("Parts Cost (₱):", fieldX + 140, currentY);
            txtPartsCost = new TextBox
            {
                Location = new Point(fieldX + 250, currentY - 3),
                Size = new Size(120, 28),
                Text = "0",
                Font = new Font("Segoe UI", 10f),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245),
                TextAlign = HorizontalAlignment.Right
            };

            var lblDiscount = Lbl("Discount (₱):", fieldX + 390, currentY);
            txtDiscount = new TextBox
            {
                Location = new Point(fieldX + 490, currentY - 3),
                Size = new Size(100, 28),
                Text = "0",
                Font = new Font("Segoe UI", 10f),
                TextAlign = HorizontalAlignment.Right
            };
            Controls.Add(lblLabor);
            Controls.Add(txtLaborCost);
            Controls.Add(lblParts);
            Controls.Add(txtPartsCost);
            Controls.Add(lblDiscount);
            Controls.Add(txtDiscount);
            currentY += rowHeight;

            // Row 9: Total Amount (centered)
            var lblTotal = Lbl("Total Amount:", labelX, currentY);
            lblTotalAmount = new Label
            {
                Text = "₱0.00",
                Location = new Point(fieldX, currentY - 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(224, 123, 36)
            };
            Controls.Add(lblTotal);
            Controls.Add(lblTotalAmount);
            currentY += rowHeight;

            // Row 10: Notes
            var lblNotes = Lbl("Notes:", labelX, currentY);
            txtNotes = new TextBox
            {
                Location = new Point(labelX, currentY + 20),
                Size = new Size(600, 50),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };
            Controls.Add(lblNotes);
            Controls.Add(txtNotes);
            currentY += 90;

            // Buttons
            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Create Record",
                Location = new Point(labelX, currentY),
                Size = new Size(140, 40),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            btnPrint = new Button
            {
                Text = "🖨️ Print Invoice",
                Location = new Point(labelX + 155, currentY),
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = _edit
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(labelX + 300, currentY),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(btnSave);
            Controls.Add(btnPrint);
            Controls.Add(btnCancel);

            // Calculation events
            txtLaborCost.TextChanged += CalculateTotal;
            txtPartsCost.TextChanged += CalculateTotal;
            txtDiscount.TextChanged += CalculateTotal;
        }


        private void LoadServiceTypes()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT ServiceTypeID, TypeName, DefaultLaborCost, DefaultPartsCost FROM ServiceTypes WHERE IsActive = 1 ORDER BY SortOrder, TypeName");

                cboServiceType.DataSource = dt;
                cboServiceType.DisplayMember = "TypeName";
                cboServiceType.ValueMember = "ServiceTypeID";
                cboServiceType.DropDownStyle = ComboBoxStyle.DropDownList;

                cboServiceType.SelectedIndex = -1;

                // Add event to update costs when service type changes
                cboServiceType.SelectedIndexChanged += ServiceTypeChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading service types: " + ex.Message);
            }
        }

        private void ServiceTypeChanged(object sender, EventArgs e)
        {
            if (cboServiceType.SelectedValue != null && cboServiceType.SelectedValue != DBNull.Value)
            {
                try
                {
                    DataTable dt = DatabaseHelper.ExecuteQuery(
                        "SELECT DefaultLaborCost, DefaultPartsCost FROM ServiceTypes WHERE ServiceTypeID = @TypeID",
                        new[] { new SqlParameter("@TypeID", cboServiceType.SelectedValue) });

                    if (dt.Rows.Count > 0 && !_edit) // Only auto-fill for new records
                    {
                        decimal laborCost = Convert.ToDecimal(dt.Rows[0]["DefaultLaborCost"]);
                        decimal partsCost = Convert.ToDecimal(dt.Rows[0]["DefaultPartsCost"]);

                        if (laborCost > 0) txtLaborCost.Text = laborCost.ToString("F2");
                        if (partsCost > 0) txtPartsCost.Text = partsCost.ToString("F2");

                        CalculateTotal(null, null);
                    }
                }
                catch (Exception ex)
                {
                    // Silent fail - user can still enter manually
                    System.Diagnostics.Debug.WriteLine($"Error loading default costs: {ex.Message}");
                }
            }
        }

        private void CalculateTotal(object sender, EventArgs e)
        {
            try
            {
                decimal labor = decimal.TryParse(txtLaborCost.Text, out decimal l) ? l : 0;
                decimal parts = decimal.TryParse(txtPartsCost.Text, out decimal p) ? p : 0;
                decimal discount = decimal.TryParse(txtDiscount.Text, out decimal d) ? d : 0;

                decimal total = labor + parts;
                decimal finalAmount = total - discount;

                if (finalAmount < 0) finalAmount = 0;

                lblTotalAmount.Text = $"₱{finalAmount:N2}";
            }
            catch (Exception)
            {
                lblTotalAmount.Text = "₱0.00";
            }
        }

        private void LoadVehicles()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"SELECT v.VehicleID,
            v.PlateNumber+' — '+v.Make+' '+v.Model+' ('+c.FirstName+' '+c.LastName+')' AS Display
            FROM Vehicles v LEFT JOIN Customers c ON v.CustomerID=c.CustomerID ORDER BY v.PlateNumber");

                cboVehicle.DataSource = dt;
                cboVehicle.DisplayMember = "Display";
                cboVehicle.ValueMember = "VehicleID";
                cboVehicle.DropDownStyle = ComboBoxStyle.DropDownList;

                // Clear any existing selection
                if (cboVehicle.Items.Count > 0)
                {
                    cboVehicle.SelectedIndex = -1;
                }
                cboVehicle.Text = "";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadTechs()
        {
            try
            {
                // Get only technicians (users with role 'Technician' who are in Technicians table)
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
            SELECT 
                t.TechnicianID, 
                u.FullName + ' (' + CASE WHEN t.IsAvailable = 1 THEN 'Available' ELSE 'Busy' END + ')' AS DisplayName,
                t.IsAvailable,
                u.FullName
            FROM Technicians t 
            INNER JOIN Users u ON t.UserID = u.UserID 
            WHERE u.IsActive = 1 
            AND u.Role = 'Technician'
            ORDER BY t.IsAvailable DESC, u.FullName");

                // Create a new DataTable WITHOUT Unassigned option
                DataTable result = new DataTable();
                result.Columns.Add("TechnicianID", typeof(object));
                result.Columns.Add("DisplayName", typeof(string));

                // Add all technicians (NO Unassigned option)
                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = result.NewRow();
                    newRow["TechnicianID"] = row["TechnicianID"];
                    newRow["DisplayName"] = row["DisplayName"];
                    result.Rows.Add(newRow);
                }

                cboTech.DataSource = null;
                cboTech.DataSource = result;
                cboTech.DisplayMember = "DisplayName";
                cboTech.ValueMember = "TechnicianID";
                cboTech.DropDownStyle = ComboBoxStyle.DropDownList;

                cboTech.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading technicians: " + ex.Message);
            }
        }

        private void LoadAssignedBy()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, FullName FROM Users WHERE IsActive = 1 AND Role IN ('Admin', 'SuperAdmin') ORDER BY FullName");
                cboAssignedBy.DataSource = dt;
                cboAssignedBy.DisplayMember = "FullName";
                cboAssignedBy.ValueMember = "UserID";

                // Set default selection if available
                if (dt.Rows.Count > 0)
                {
                    cboAssignedBy.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading users: " + ex.Message); }
        }

        private void LoadService()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM ServiceRecords WHERE ServiceID=@ID",
                    new[] { new SqlParameter("@ID", _id) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];

                    txtJobOrderNo.Text = r["JobOrderNo"].ToString();
                    cboVehicle.SelectedValue = Convert.ToInt32(r["VehicleID"]);

                    if (r["TechnicianID"] != DBNull.Value)
                        cboTech.SelectedValue = Convert.ToInt32(r["TechnicianID"]);

                    if (r["AssignedBy"] != DBNull.Value)
                        cboAssignedBy.SelectedValue = Convert.ToInt32(r["AssignedBy"]);

                    txtDescription.Text = r["Description"].ToString();
                    cboServiceType.Text = r["ServiceType"].ToString();
                    cboStatus.Text = r["Status"].ToString();
                    dtpDateIn.Value = Convert.ToDateTime(r["DateIn"]);

                    if (r["EstimatedDate"] != DBNull.Value)
                        dtpEstDate.Value = Convert.ToDateTime(r["EstimatedDate"]);

                    if (r["DateCompleted"] != DBNull.Value)
                    {
                        dtpDateCompleted.Value = Convert.ToDateTime(r["DateCompleted"]);
                        dtpDateCompleted.Checked = true;
                    }

                    txtNotes.Text = r["Notes"].ToString();
                    txtLaborCost.Text = r["LaborCost"] != DBNull.Value ? r["LaborCost"].ToString() : "0";
                    txtPartsCost.Text = r["PartsCost"] != DBNull.Value ? r["PartsCost"].ToString() : "0";
                    txtDiscount.Text = r["Discount"] != DBNull.Value ? r["Discount"].ToString() : "0";

                    CalculateTotal(null, null);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Get service data with ALL columns including prices
                DataTable serviceDt = DatabaseHelper.ExecuteQuery(
                    "SELECT ServiceID, JobOrderNo, ServiceType, Status, DateIn, Notes, " +
                    "LaborCost, PartsCost, Discount, TotalCost, FinalAmount " +
                    "FROM ServiceRecords WHERE ServiceID = @ID",
                    new[] { new SqlParameter("@ID", _id) });

                if (serviceDt.Rows.Count == 0)
                {
                    MessageBox.Show("Service record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get vehicle and customer data
                DataTable vehicleDt = DatabaseHelper.ExecuteQuery(
                    "SELECT v.Make, v.Model, v.PlateNumber, c.FirstName, c.LastName, c.Phone, c.Email " +
                    "FROM ServiceRecords s " +
                    "JOIN Vehicles v ON s.VehicleID = v.VehicleID " +
                    "JOIN Customers c ON v.CustomerID = c.CustomerID " +
                    "WHERE s.ServiceID = @ID",
                    new[] { new SqlParameter("@ID", _id) });

                if (vehicleDt.Rows.Count == 0)
                {
                    MessageBox.Show("Vehicle or customer information not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create payment row
                DataTable paymentDt = new DataTable();
                paymentDt.Columns.Add("PaymentMethod");
                paymentDt.Columns.Add("Amount");
                paymentDt.Columns.Add("PaymentDate");
                paymentDt.Columns.Add("Status");
                DataRow paymentRow = paymentDt.NewRow();
                paymentRow["PaymentMethod"] = "Cash";
                paymentRow["Amount"] = serviceDt.Rows[0]["FinalAmount"] != DBNull.Value ? serviceDt.Rows[0]["FinalAmount"] : 0;
                paymentRow["PaymentDate"] = DateTime.Now;
                paymentRow["Status"] = "Paid";
                paymentDt.Rows.Add(paymentRow);

                // Print the invoice with PREVIEW
                PrintHelper printHelper = new PrintHelper();
                printHelper.PrintServiceInvoice(
                    serviceDt.Rows[0],
                    vehicleDt.Rows[0],
                    vehicleDt.Rows[0],
                    paymentRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupNumericValidation()
        {
            // Labor Cost validation
            txtLaborCost.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    e.Handled = true;
                if (e.KeyChar == '.' && (s as TextBox).Text.Contains("."))
                    e.Handled = true;
            };
            txtLaborCost.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtLaborCost.Text))
                    txtLaborCost.Text = "0";
                if (decimal.TryParse(txtLaborCost.Text, out decimal value))
                    txtLaborCost.Text = value.ToString("F2");
                else
                    txtLaborCost.Text = "0";
                CalculateTotal(null, null);
            };

            // Parts Cost validation
            txtPartsCost.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    e.Handled = true;
                if (e.KeyChar == '.' && (s as TextBox).Text.Contains("."))
                    e.Handled = true;
            };
            txtPartsCost.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPartsCost.Text))
                    txtPartsCost.Text = "0";
                if (decimal.TryParse(txtPartsCost.Text, out decimal value))
                    txtPartsCost.Text = value.ToString("F2");
                else
                    txtPartsCost.Text = "0";
                CalculateTotal(null, null);
            };

            // Discount validation (only whole numbers or decimals)
            txtDiscount.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    e.Handled = true;
                if (e.KeyChar == '.' && (s as TextBox).Text.Contains("."))
                    e.Handled = true;
            };
            txtDiscount.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtDiscount.Text))
                    txtDiscount.Text = "0";
                if (decimal.TryParse(txtDiscount.Text, out decimal value))
                    txtDiscount.Text = value.ToString("F2");
                else
                    txtDiscount.Text = "0";
                CalculateTotal(null, null);
            };
        }
        private void Save(object sender, EventArgs e)
        {
            // ========== VALIDATION ==========

            // 1. Check Vehicle
            if (cboVehicle.SelectedIndex == -1 || cboVehicle.SelectedValue == null)
            {
                MessageBox.Show("Please select a vehicle.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboVehicle.DroppedDown = true;
                return;
            }

            // 2. Check Service Type
            if (cboServiceType.SelectedIndex == -1 || string.IsNullOrWhiteSpace(cboServiceType.Text))
            {
                MessageBox.Show("Please select a service type.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboServiceType.DroppedDown = true;
                return;
            }

            // 3. Check Status
            if (cboStatus.SelectedIndex == -1 || string.IsNullOrWhiteSpace(cboStatus.Text))
            {
                MessageBox.Show("Please select a status.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboStatus.DroppedDown = true;
                return;
            }

            // 4. Check Assigned By
            if (cboAssignedBy.SelectedIndex == -1 || cboAssignedBy.SelectedValue == null)
            {
                MessageBox.Show("Please select who assigned this service.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboAssignedBy.DroppedDown = true;
                return;
            }

            // 5. Check Date In
            if (dtpDateIn.Value == null)
            {
                MessageBox.Show("Please enter a valid Date In.",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDateIn.Focus();
                return;
            }

            // 6. Check Labor Cost
            if (!decimal.TryParse(txtLaborCost.Text, out decimal labor) || labor < 0)
            {
                MessageBox.Show("Labor Cost must be a valid number (0 or greater).",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLaborCost.Text = "0";
                txtLaborCost.Focus();
                return;
            }

            // 7. Check Parts Cost
            if (!decimal.TryParse(txtPartsCost.Text, out decimal parts) || parts < 0)
            {
                MessageBox.Show("Parts Cost must be a valid number (0 or greater).",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartsCost.Text = "0";
                txtPartsCost.Focus();
                return;
            }

            // 8. Check Discount
            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0)
            {
                MessageBox.Show("Discount must be a valid number (0 or greater).",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscount.Text = "0";
                txtDiscount.Focus();
                return;
            }

            // 9. Calculate totals
            decimal total = labor + parts;
            decimal finalAmount = total - discount;

            // 10. Check if discount exceeds total
            if (discount > total)
            {
                MessageBox.Show($"Discount cannot exceed total amount (₱{total:N2}).",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscount.Focus();
                return;
            }

            // 11. Check Date logic
            if (dtpEstDate.Value < dtpDateIn.Value)
            {
                DialogResult result = MessageBox.Show(
                    "Estimated completion date is before the start date. Do you want to continue?",
                    "Date Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    dtpEstDate.Focus();
                    return;
                }
            }

            if (dtpDateCompleted.Checked && dtpDateCompleted.Value < dtpDateIn.Value)
            {
                MessageBox.Show("Completion date cannot be before the start date.",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDateCompleted.Focus();
                return;
            }
            SetupNumericValidation();

            // ========== SAVE DATA ==========

            try
            {
                // Get old technician ID for update (to update availability)
                object oldTechID = null;
                if (_edit)
                {
                    DataTable oldData = DatabaseHelper.ExecuteQuery(
                        "SELECT TechnicianID FROM ServiceRecords WHERE ServiceID = @ID",
                        new[] { new SqlParameter("@ID", _id) });
                    if (oldData.Rows.Count > 0 && oldData.Rows[0]["TechnicianID"] != DBNull.Value)
                        oldTechID = oldData.Rows[0]["TechnicianID"];
                }

                // Get ServiceTypeID
                object serviceTypeId = DBNull.Value;
                if (cboServiceType.SelectedValue != null)
                {
                    serviceTypeId = cboServiceType.SelectedValue;
                }

                // Get other values
                object techID = DBNull.Value;
                if (cboTech.SelectedItem != null && cboTech.Text != "— Unassigned —")
                {
                    if (cboTech.SelectedValue != null && cboTech.SelectedValue != DBNull.Value)
                    {
                        techID = cboTech.SelectedValue;
                    }
                }

                object assignedBy = (cboAssignedBy.SelectedValue == null || cboAssignedBy.SelectedValue == DBNull.Value)
                    ? (object)DBNull.Value : cboAssignedBy.SelectedValue;

                object dateCompleted = dtpDateCompleted.Checked ? (object)dtpDateCompleted.Value.Date : DBNull.Value;

                if (_edit)
                {
                    SqlParameter[] updateParams = {
                new SqlParameter("@VID", cboVehicle.SelectedValue),
                new SqlParameter("@TID", techID),
                new SqlParameter("@AssignedBy", assignedBy),
                new SqlParameter("@Desc", txtDescription.Text.Trim()),
                new SqlParameter("@Type", cboServiceType.Text.Trim()),
                new SqlParameter("@ServiceTypeID", serviceTypeId),
                new SqlParameter("@Status", cboStatus.Text),
                new SqlParameter("@DIn", dtpDateIn.Value.Date),
                new SqlParameter("@DEst", dtpEstDate.Value.Date),
                new SqlParameter("@DCompleted", dateCompleted),
                new SqlParameter("@Notes", txtNotes.Text.Trim()),
                new SqlParameter("@LaborCost", labor),
                new SqlParameter("@PartsCost", parts),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@TotalCost", total),
                new SqlParameter("@FinalAmount", finalAmount),
                new SqlParameter("@ID", _id)
            };

                    DatabaseHelper.ExecuteNonQuery(@"
                UPDATE ServiceRecords 
                SET VehicleID=@VID, TechnicianID=@TID, AssignedBy=@AssignedBy,
                    Description=@Desc, ServiceType=@Type, ServiceTypeID=@ServiceTypeID,
                    Status=@Status, DateIn=@DIn, EstimatedDate=@DEst, DateCompleted=@DCompleted,
                    Notes=@Notes, LaborCost=@LaborCost, PartsCost=@PartsCost,
                    Discount=@Discount, TotalCost=@TotalCost, FinalAmount=@FinalAmount,
                    UpdatedAt=GETDATE() 
                WHERE ServiceID=@ID", updateParams);

                    // Update technician availability
                    string newStatus = cboStatus.Text;
                    if (newStatus == "Completed" || newStatus == "Cancelled")
                    {
                        if (techID != DBNull.Value)
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE Technicians SET IsAvailable = 1 WHERE TechnicianID = @TechID",
                                new[] { new SqlParameter("@TechID", techID) });
                        }
                        // Also free up the old technician if they were assigned
                        if (oldTechID != null && oldTechID != DBNull.Value && !oldTechID.Equals(techID))
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE Technicians SET IsAvailable = 1 WHERE TechnicianID = @TechID",
                                new[] { new SqlParameter("@TechID", oldTechID) });
                        }
                    }
                    else if (newStatus == "InProgress" || newStatus == "Pending")
                    {
                        if (oldTechID != null && oldTechID != DBNull.Value && !oldTechID.Equals(techID))
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE Technicians SET IsAvailable = 1 WHERE TechnicianID = @TechID",
                                new[] { new SqlParameter("@TechID", oldTechID) });
                        }
                        if (techID != DBNull.Value)
                        {
                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE Technicians SET IsAvailable = 0 WHERE TechnicianID = @TechID",
                                new[] { new SqlParameter("@TechID", techID) });
                        }
                    }

                    MessageBox.Show("Record updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string jobOrderNo = $"JO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                    SqlParameter[] insertParams = {
                new SqlParameter("@JobNo", jobOrderNo),
                new SqlParameter("@VID", cboVehicle.SelectedValue),
                new SqlParameter("@TID", techID),
                new SqlParameter("@AssignedBy", assignedBy),
                new SqlParameter("@Desc", txtDescription.Text.Trim()),
                new SqlParameter("@Type", cboServiceType.Text.Trim()),
                new SqlParameter("@ServiceTypeID", serviceTypeId),
                new SqlParameter("@Status", cboStatus.Text),
                new SqlParameter("@DIn", dtpDateIn.Value.Date),
                new SqlParameter("@DEst", dtpEstDate.Value.Date),
                new SqlParameter("@DCompleted", dateCompleted),
                new SqlParameter("@Notes", txtNotes.Text.Trim()),
                new SqlParameter("@LaborCost", labor),
                new SqlParameter("@PartsCost", parts),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@TotalCost", total),
                new SqlParameter("@FinalAmount", finalAmount)
            };

                    DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO ServiceRecords(
                    JobOrderNo, VehicleID, TechnicianID, AssignedBy, Description,
                    ServiceType, ServiceTypeID, Status, DateIn, EstimatedDate, DateCompleted,
                    Notes, LaborCost, PartsCost, Discount, TotalCost, FinalAmount, CreatedAt, UpdatedAt) 
                VALUES(
                    @JobNo, @VID, @TID, @AssignedBy, @Desc,
                    @Type, @ServiceTypeID, @Status, @DIn, @DEst, @DCompleted,
                    @Notes, @LaborCost, @PartsCost, @Discount, @TotalCost, @FinalAmount, GETDATE(), GETDATE())", insertParams);

                    string newStatus = cboStatus.Text;
                    if ((newStatus == "Pending" || newStatus == "InProgress") && techID != DBNull.Value)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Technicians SET IsAvailable = 0 WHERE TechnicianID = @TechID",
                            new[] { new SqlParameter("@TechID", techID) });
                    }

                    MessageBox.Show("Record created successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

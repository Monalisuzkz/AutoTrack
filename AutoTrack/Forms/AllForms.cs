using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;


namespace AutoTrack.Forms
{
    // ══════════════════════════════════════════════════════════
    // USER FORM
    // ══════════════════════════════════════════════════════════
    public class UserForm : Form
    {
        private int _id = 0; private bool _edit = false;
        private Label lblTitle, lblName, lblUser, lblPass, lblRole;
        private TextBox txtName, txtUser, txtPass;
        private ComboBox cboRole;
        private CheckBox chkActive;
        private Button btnSave, btnCancel;

        public UserForm(int id = 0) { _id = id; _edit = id > 0; Init(); if (_edit) LoadUser(); }

        private void Init()
        {
            Text = _edit ? "Edit User" : "Add User";
            Size = new Size(420, 360); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = _edit ? "Edit User Account" : "Add New User",
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
            TextBox Txt(int x, int y, int w = 360) => new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            lblName = Lbl("Full Name", 20, 56); txtName = Txt(20, 76);
            lblUser = Lbl("Username", 20, 116); txtUser = Txt(20, 136, 170);
            lblPass = Lbl(_edit ? "New Password (optional)" : "Password", 210, 116); txtPass = Txt(210, 136, 170);
            txtPass.PasswordChar = '●';

            lblRole = Lbl("Role", 20, 176);
            cboRole = new ComboBox
            {
                Location = new Point(20, 196),
                Size = new Size(170, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            cboRole.Items.AddRange(new object[] {"Admin", "Staff", "Technician", "Supplier" });
            cboRole.SelectedIndex = 1;

            chkActive = new CheckBox
            {
                Text = "Account is Active",
                Location = new Point(20, 236),
                Font = new Font("Segoe UI", 10f),
                Checked = true,
                AutoSize = true
            };

            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Add User",
                Location = new Point(20, 270),
                Size = new Size(130, 36),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0; btnSave.Click += Save;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(160, 270),
                Size = new Size(100, 36),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { lblTitle, lblName, txtName, lblUser, txtUser,
                lblPass, txtPass, lblRole, cboRole, chkActive, btnSave, btnCancel });
        }

        private void LoadUser()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Users WHERE UserID=@ID",
                    new[] { new SqlParameter("@ID", _id) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtName.Text = r["FullName"].ToString(); txtUser.Text = r["Username"].ToString();
                    txtPass.Text = string.Empty; cboRole.Text = r["Role"].ToString();
                    chkActive.Checked = Convert.ToBoolean(r["IsActive"]);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Full name and username are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            if (!_edit && string.IsNullOrWhiteSpace(txtPass.Text))
            {
                MessageBox.Show("Password is required for new users.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            try
            {
                if (_edit)
                {
                    bool hasNewPassword = !string.IsNullOrWhiteSpace(txtPass.Text);
                    string updateQuery = hasNewPassword
                        ? "UPDATE Users SET FullName=@Name,Username=@User,Password=@Pass,Role=@Role,IsActive=@Active,UpdatedAt=GETDATE() WHERE UserID=@ID"
                        : "UPDATE Users SET FullName=@Name,Username=@User,Role=@Role,IsActive=@Active,UpdatedAt=GETDATE() WHERE UserID=@ID";

                    var updateParams = hasNewPassword
                        ? new SqlParameter[]
                        {
                            new SqlParameter("@Name", txtName.Text.Trim()),
                            new SqlParameter("@User", txtUser.Text.Trim()),
                            new SqlParameter("@Pass", PasswordHelper.HashPassword(txtPass.Text)),
                            new SqlParameter("@Role", cboRole.Text),
                            new SqlParameter("@Active", chkActive.Checked ? 1 : 0),
                            new SqlParameter("@ID", _id)
                        }
                        : new SqlParameter[]
                        {
                            new SqlParameter("@Name", txtName.Text.Trim()),
                            new SqlParameter("@User", txtUser.Text.Trim()),
                            new SqlParameter("@Role", cboRole.Text),
                            new SqlParameter("@Active", chkActive.Checked ? 1 : 0),
                            new SqlParameter("@ID", _id)
                        };

                    DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);
                }
                else
                {
                    object exists = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Users WHERE Username=@User", new[] { new SqlParameter("@User", txtUser.Text.Trim()) });
                    if (Convert.ToInt32(exists) > 0) { MessageBox.Show("Username already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Users(FullName,Username,Password,Role,IsActive) VALUES(@Name,@User,@Pass,@Role,@Active)",
                        new[]
                        {
                            new SqlParameter("@Name", txtName.Text.Trim()),
                            new SqlParameter("@User", txtUser.Text.Trim()),
                            new SqlParameter("@Pass", PasswordHelper.HashPassword(txtPass.Text)),
                            new SqlParameter("@Role", cboRole.Text),
                            new SqlParameter("@Active", chkActive.Checked ? 1 : 0)
                        });
                }
                MessageBox.Show(_edit ? "User updated!" : "User added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    // ══════════════════════════════════════════════════════════
    // TECHNICIAN FORM WITH EMPTY DROPDOWNS
    // ══════════════════════════════════════════════════════════
    public class TechnicianForm : Form
    {
        private int _id = 0;
        private bool _edit = false;
        private Label lblTitle, lblUser, lblSpec, lblLevel;
        private ComboBox cboUser, cboLevel, cboSpecialization;
        private CheckBox chkAvail;
        private Button btnSave, btnCancel;

        public TechnicianForm(int id = 0)
        {
            _id = id;
            _edit = id > 0;
            Init();
            LoadUsers();
            LoadSpecializations();
            if (_edit) LoadTech();
        }

        private void Init()
        {
            Text = _edit ? "Edit Technician" : "Add Technician";
            Size = new Size(450, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = _edit ? "Edit Technician" : "Add New Technician",
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

            // Link to User Account
            lblUser = Lbl("Link to User Account:", 20, 56);
            cboUser = new ComboBox
            {
                Location = new Point(20, 76),
                Size = new Size(390, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            cboUser.SelectedIndex = -1;  // Empty by default

            // Specialization (Dropdown)
            lblSpec = Lbl("Specialization:", 20, 116);
            cboSpecialization = new ComboBox
            {
                Location = new Point(20, 136),
                Size = new Size(390, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Add common specializations
            cboSpecialization.Items.AddRange(new object[] {
            "General Repair",
            "Engine Specialist",
            "Transmission Specialist",
            "Electrical Systems",
            "Brakes & Suspension",
            "Air Conditioning",
            "Diagnostics",
            "Body & Paint",
            "Tire & Wheel",
            "Exhaust System",
            "Cooling System",
            "Fuel System"
        });
            cboSpecialization.SelectedIndex = -1;  // ← Empty, no default selection

            // Level
            lblLevel = Lbl("Level:", 20, 176);
            cboLevel = new ComboBox
            {
                Location = new Point(20, 196),
                Size = new Size(170, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            cboLevel.Items.AddRange(new object[] { "Junior", "Senior", "Master" });
            cboLevel.SelectedIndex = -1;  // ← Empty, no default selection

            // Available
            chkAvail = new CheckBox
            {
                Text = "Currently Available",
                Location = new Point(20, 240),
                Font = new Font("Segoe UI", 10f),
                Checked = true,
                AutoSize = true
            };

            // Buttons
            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Add Technician",
                Location = new Point(20, 290),
                Size = new Size(150, 38),
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
                Location = new Point(180, 290),
                Size = new Size(100, 38),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] {
            lblTitle, lblUser, cboUser,
            lblSpec, cboSpecialization,
            lblLevel, cboLevel,
            chkAvail,
            btnSave, btnCancel
        });
        }

        private void LoadSpecializations()
        {
            try
            {
                // Specializations already loaded in Init()
                // Keep SelectedIndex = -1
                cboSpecialization.SelectedIndex = -1;
            }
            catch (Exception ex) { MessageBox.Show("Error loading specializations: " + ex.Message); }
        }

        private void LoadUsers()
        {
            try
            {
                string q = _edit
                    ? "SELECT UserID, FullName FROM Users WHERE Role='Technician' AND IsActive=1 AND (UserID NOT IN(SELECT UserID FROM Technicians) OR UserID=(SELECT UserID FROM Technicians WHERE TechnicianID=@ID)) ORDER BY FullName"
                    : "SELECT UserID, FullName FROM Users WHERE Role='Technician' AND IsActive=1 AND UserID NOT IN(SELECT UserID FROM Technicians) ORDER BY FullName";

                SqlParameter[] p = _edit ? new[] { new SqlParameter("@ID", _id) } : null;
                DataTable dt = DatabaseHelper.ExecuteQuery(q, p);

                cboUser.DataSource = dt;
                cboUser.DisplayMember = "FullName";
                cboUser.ValueMember = "UserID";
                cboUser.SelectedIndex = -1;  // ← Empty, no default selection
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadTech()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Technicians WHERE TechnicianID=@ID",
                    new[] { new SqlParameter("@ID", _id) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    cboUser.SelectedValue = Convert.ToInt32(r["UserID"]);
                    cboSpecialization.Text = r["Specialization"].ToString();
                    cboLevel.Text = r["Level"].ToString();
                    chkAvail.Checked = Convert.ToBoolean(r["IsAvailable"]);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            // Validation
            if (cboUser.SelectedValue == null || cboUser.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a user account.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboUser.DroppedDown = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(cboSpecialization.Text) || cboSpecialization.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a specialization.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboSpecialization.DroppedDown = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(cboLevel.Text) || cboLevel.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a level.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLevel.DroppedDown = true;
                return;
            }

            try
            {
                SqlParameter[] p = {
                new SqlParameter("@UID",   cboUser.SelectedValue),
                new SqlParameter("@Spec",  cboSpecialization.Text.Trim()),
                new SqlParameter("@Level", cboLevel.Text),
                new SqlParameter("@Avail", chkAvail.Checked ? 1 : 0)
            };

                if (_edit)
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Technicians SET UserID=@UID, Specialization=@Spec, Level=@Level, IsAvailable=@Avail WHERE TechnicianID=@ID",
                        new SqlParameter[] { p[0], p[1], p[2], p[3], new SqlParameter("@ID", _id) });
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Technicians(UserID, Specialization, Level, IsAvailable) VALUES(@UID,@Spec,@Level,@Avail)",
                        p);

                MessageBox.Show(_edit ? "Technician updated!" : "Technician added!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    // ══════════════════════════════════════════════════════════
    // SERVICE FORM WITH PROPER ALIGNMENT
    // ══════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════
    // INVENTORY FORM
    // ══════════════════════════════════════════════════════════
public class InventoryForm : Form
        {
            private int _id = 0; private bool _edit = false;
            private Label lblTitle, lblName, lblCat, lblUnit, lblQty, lblReorder, lblPrice, lblSupplier;
            private TextBox txtName, txtQty, txtReorder, txtPrice;
            private ComboBox cboCat, cboUnit, cboSupplier;
            private Button btnSave, btnCancel;

            public InventoryForm(int id = 0)
            {
                _id = id;
                _edit = id > 0;
                Init();
                LoadSuppliers();
                if (_edit) LoadPart();
            }

            private void Init()
            {
                Text = _edit ? "Edit Part" : "Add Part";
                Size = new Size(440, 420);  // Increased height
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                BackColor = Color.White;
                Font = new Font("Segoe UI", 9f);

                lblTitle = new Label
                {
                    Text = _edit ? "Edit Inventory Item" : "Add Inventory Item",
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

                TextBox Txt(int x, int y, int w = 100) => new TextBox
                {
                    Location = new Point(x, y),
                    Size = new Size(w, 28),
                    Font = new Font("Segoe UI", 10f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                lblName = Lbl("Part Name", 20, 56);
                txtName = new TextBox
                {
                    Location = new Point(20, 76),
                    Size = new Size(380, 28),
                    Font = new Font("Segoe UI", 10f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                lblCat = Lbl("Category", 20, 116);
                cboCat = new ComboBox
                {
                    Location = new Point(20, 136),
                    Size = new Size(170, 28),
                    Font = new Font("Segoe UI", 10f),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(245, 245, 245)
                };
                cboCat.Items.AddRange(new object[] { "Lubricants", "Filters", "Brakes", "Ignition", "Tires", "Electrical", "Body Parts", "Other" });
                cboCat.SelectedIndex = 0;

                lblUnit = Lbl("Unit", 210, 116);
                cboUnit = new ComboBox
                {
                    Location = new Point(210, 136),
                    Size = new Size(170, 28),
                    Font = new Font("Segoe UI", 10f),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(245, 245, 245)
                };
                cboUnit.Items.AddRange(new object[] { "Piece", "Set", "Liter", "Box", "Bottle", "Roll" });
                cboUnit.SelectedIndex = 0;

                lblQty = Lbl("Quantity", 20, 176);
                txtQty = Txt(20, 196, 100);

                lblReorder = Lbl("Reorder Level", 140, 176);
                txtReorder = Txt(140, 196, 100);

                lblPrice = Lbl("Unit Price (₱)", 260, 176);
                txtPrice = Txt(260, 196, 130);

                lblSupplier = Lbl("Supplier", 20, 236);
                cboSupplier = new ComboBox
                {
                    Location = new Point(20, 256),
                    Size = new Size(380, 28),
                    Font = new Font("Segoe UI", 10f),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                btnSave = new Button
                {
                    Text = _edit ? "Save Changes" : "Add Part",
                    Location = new Point(20, 310),
                    Size = new Size(130, 36),
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
                    Location = new Point(160, 310),
                    Size = new Size(100, 36),
                    Font = new Font("Segoe UI", 10f),
                    BackColor = Color.FromArgb(220, 220, 220),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

                Controls.AddRange(new Control[] { lblTitle, lblName, txtName, lblCat, cboCat, lblUnit, cboUnit,
            lblQty, txtQty, lblReorder, txtReorder, lblPrice, txtPrice,
            lblSupplier, cboSupplier, btnSave, btnCancel });
            }

            private void LoadSuppliers()
            {
                try
                {
                    // Test if suppliers exist
                    DataTable testDt = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) AS Count FROM Suppliers");
                    int count = Convert.ToInt32(testDt.Rows[0]["Count"]);

                    if (count == 0)
                    {
                        MessageBox.Show("No suppliers found. Please add suppliers first.", "Information",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cboSupplier.Enabled = false;
                        cboSupplier.Text = "-- No Suppliers Available --";
                        return;
                    }

                    // Load suppliers
                    DataTable dt = DatabaseHelper.ExecuteQuery("SELECT SupplierID, CompanyName FROM Suppliers ORDER BY CompanyName");

                    // Create a new DataTable with a "None" option
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("SupplierID", typeof(object));
                    displayDt.Columns.Add("CompanyName", typeof(string));

                    // Add "None" option first
                    displayDt.Rows.Add(DBNull.Value, "-- None --");

                    // Add all suppliers
                    foreach (DataRow row in dt.Rows)
                    {
                        displayDt.Rows.Add(row["SupplierID"], row["CompanyName"]);
                    }

                    cboSupplier.DataSource = displayDt;
                    cboSupplier.DisplayMember = "CompanyName";
                    cboSupplier.ValueMember = "SupplierID";
                    cboSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
                    cboSupplier.SelectedIndex = 0;

                    // Debug
                    System.Diagnostics.Debug.WriteLine($"Loaded {displayDt.Rows.Count} suppliers (including None)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading suppliers: " + ex.Message, "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cboSupplier.Enabled = false;
                    cboSupplier.Text = "-- Error Loading Suppliers --";
                }
            }

            private void LoadPart()
            {
                try
                {
                    DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Inventory WHERE PartID=@ID",
                        new[] { new SqlParameter("@ID", _id) });
                    if (dt.Rows.Count > 0)
                    {
                        DataRow r = dt.Rows[0];
                        txtName.Text = r["PartName"].ToString();
                        cboCat.Text = r["Category"].ToString();
                        cboUnit.Text = r["Unit"].ToString();
                        txtQty.Text = r["Quantity"].ToString();
                        txtReorder.Text = r["ReorderLevel"].ToString();
                        txtPrice.Text = r["UnitPrice"].ToString();

                        if (r["SupplierID"] != DBNull.Value && r["SupplierID"] != null)
                        {
                            int supplierId = Convert.ToInt32(r["SupplierID"]);
                            // Find and select the supplier in the combo box
                            foreach (DataRowView item in cboSupplier.Items)
                            {
                                if (item["SupplierID"] != DBNull.Value && Convert.ToInt32(item["SupplierID"]) == supplierId)
                                {
                                    cboSupplier.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading part: " + ex.Message);
                }
            }

            private void Save(object sender, EventArgs e)
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Part name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                if (!int.TryParse(txtQty.Text, out int qty) || qty < 0)
                {
                    MessageBox.Show("Please enter a valid quantity (0 or greater).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQty.Focus();
                    return;
                }

                if (!int.TryParse(txtReorder.Text, out int reorder) || reorder < 0)
                {
                    MessageBox.Show("Please enter a valid reorder level.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtReorder.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid unit price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrice.Focus();
                    return;
                }

                try
                {
                    // Get supplier ID - can be NULL (if "None" is selected)
                    object supplierId = DBNull.Value;
                    if (cboSupplier.SelectedIndex > 0 && cboSupplier.SelectedValue != null && cboSupplier.SelectedValue != DBNull.Value)
                    {
                        supplierId = cboSupplier.SelectedValue;
                    }

                    SqlParameter[] p = {
                new SqlParameter("@Name",    txtName.Text.Trim()),
                new SqlParameter("@Cat",     cboCat.Text),
                new SqlParameter("@Unit",    cboUnit.Text),
                new SqlParameter("@Qty",     qty),
                new SqlParameter("@Reorder", reorder),
                new SqlParameter("@Price",   price),
                new SqlParameter("@SupID",   supplierId)
            };

                    if (_edit)
                    {
                        // Update existing part
                        var updateParams = new SqlParameter[p.Length + 1];
                        Array.Copy(p, updateParams, p.Length);
                        updateParams[p.Length] = new SqlParameter("@ID", _id);

                        DatabaseHelper.ExecuteNonQuery(@"
                    UPDATE Inventory 
                    SET PartName = @Name, 
                        Category = @Cat, 
                        Unit = @Unit, 
                        Quantity = @Qty, 
                        ReorderLevel = @Reorder, 
                        UnitPrice = @Price, 
                        SupplierID = @SupID, 
                        UpdatedAt = GETDATE() 
                    WHERE PartID = @ID", updateParams);

                        MessageBox.Show("Part updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Add new part
                        DatabaseHelper.ExecuteNonQuery(@"
                    INSERT INTO Inventory (PartName, Category, Unit, Quantity, ReorderLevel, UnitPrice, SupplierID, CreatedAt, UpdatedAt, IsArchived) 
                    VALUES (@Name, @Cat, @Unit, @Qty, @Reorder, @Price, @SupID, GETDATE(), GETDATE(), 0)", p);

                        MessageBox.Show("Part added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    

        // ══════════════════════════════════════════════════════════
        // SUPPLIER FORM
        // ══════════════════════════════════════════════════════════
        public class SupplierForm : Form
    {
        private int _id = 0; private bool _edit = false;
        private Label lblTitle, lblCompany, lblContact, lblPhone, lblEmail, lblAddress, lblParts;
        private TextBox txtCompany, txtContact, txtPhone, txtEmail, txtAddress, txtParts;
        private Button btnSave, btnCancel;

        public SupplierForm(int id = 0) { _id = id; _edit = id > 0; Init(); if (_edit) LoadSupplier(); }

        private void Init()
        {
            Text = _edit ? "Edit Supplier" : "Add Supplier";
            Size = new Size(440, 390); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = _edit ? "Edit Supplier" : "Add New Supplier",
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
            TextBox Txt(int x, int y, int w = 185) => new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            lblCompany = Lbl("Company Name", 20, 56); txtCompany = new TextBox { Location = new Point(20, 76), Size = new Size(380, 28), Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(245, 245, 245) };
            lblContact = Lbl("Contact Person", 20, 116); txtContact = Txt(20, 136);
            lblPhone = Lbl("Phone", 220, 116); txtPhone = Txt(220, 136);
            lblEmail = Lbl("Email", 20, 176); txtEmail = Txt(20, 196);
            lblAddress = Lbl("Address", 220, 176); txtAddress = Txt(220, 196);
            lblParts = Lbl("Parts Supplied", 20, 236);
            txtParts = new TextBox
            {
                Location = new Point(20, 256),
                Size = new Size(380, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Add Supplier",
                Location = new Point(20, 296),
                Size = new Size(140, 36),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0; btnSave.Click += Save;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(170, 296),
                Size = new Size(100, 36),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { lblTitle, lblCompany, txtCompany, lblContact, txtContact,
                lblPhone, txtPhone, lblEmail, txtEmail, lblAddress, txtAddress,
                lblParts, txtParts, btnSave, btnCancel });
        }

        private void LoadSupplier()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Suppliers WHERE SupplierID=@ID", new[] { new SqlParameter("@ID", _id) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtCompany.Text = r["CompanyName"].ToString(); txtContact.Text = r["ContactPerson"].ToString();
                    txtPhone.Text = r["Phone"].ToString(); txtEmail.Text = r["Email"].ToString();
                    txtAddress.Text = r["Address"].ToString(); txtParts.Text = r["PartsSupplied"].ToString();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompany.Text)) { MessageBox.Show("Company name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                SqlParameter[] p = {
                    new SqlParameter("@Co",    txtCompany.Text.Trim()),
                    new SqlParameter("@Con",   txtContact.Text.Trim()),
                    new SqlParameter("@Ph",    txtPhone.Text.Trim()),
                    new SqlParameter("@Em",    txtEmail.Text.Trim()),
                    new SqlParameter("@Addr",  txtAddress.Text.Trim()),
                    new SqlParameter("@Parts", txtParts.Text.Trim()) };
                if (_edit)
                    DatabaseHelper.ExecuteNonQuery("UPDATE Suppliers SET CompanyName=@Co,ContactPerson=@Con,Phone=@Ph,Email=@Em,Address=@Addr,PartsSupplied=@Parts,UpdatedAt=GETDATE() WHERE SupplierID=@ID",
                        new SqlParameter[] { p[0], p[1], p[2], p[3], p[4], p[5], new SqlParameter("@ID", _id) });
                else
                    DatabaseHelper.ExecuteNonQuery("INSERT INTO Suppliers(CompanyName,ContactPerson,Phone,Email,Address,PartsSupplied) VALUES(@Co,@Con,@Ph,@Em,@Addr,@Parts)", p);
                MessageBox.Show(_edit ? "Supplier updated!" : "Supplier added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}

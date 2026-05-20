using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class SupplierForm : Form
    {
        private int _id = 0;
        private bool _edit = false;
        private Label lblTitle, lblUser, lblCompany, lblContact, lblPhone, lblEmail, lblAddress, lblParts;
        private ComboBox cboUser;
        private TextBox txtCompany, txtContact, txtPhone, txtEmail, txtAddress;
        private NumericUpDown nudParts;  // Changed from TextBox to NumericUpDown
        private Button btnSave, btnCancel;

        public SupplierForm(int id = 0)
        {
            _id = id;
            _edit = id > 0;
            Init();
            LoadUsers();
            if (_edit)
                LoadSupplier();
        }

        private void Init()
        {
            Text = _edit ? "Edit Supplier" : "Add Supplier";
            Size = new Size(500, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

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

            // Link to User Account
            lblUser = Lbl("Link to User Account:", 20, 56);
            cboUser = new ComboBox
            {
                Location = new Point(20, 76),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 200,
                MaxDropDownItems = 10,
                IntegralHeight = true
            };
            cboUser.SelectedIndex = -1;

            // Company Name
            lblCompany = Lbl("Company Name:", 20, 116);
            txtCompany = new TextBox
            {
                Location = new Point(20, 136),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Contact Person
            lblContact = Lbl("Contact Person:", 20, 176);
            txtContact = new TextBox
            {
                Location = new Point(20, 196),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Phone
            lblPhone = Lbl("Phone:", 20, 236);
            txtPhone = new TextBox
            {
                Location = new Point(20, 256),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Email
            lblEmail = Lbl("Email:", 240, 236);
            txtEmail = new TextBox
            {
                Location = new Point(240, 256),
                Size = new Size(220, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Address
            lblAddress = Lbl("Address:", 20, 296);
            txtAddress = new TextBox
            {
                Location = new Point(20, 316),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Parts Supplied - Changed to NumericUpDown
            lblParts = Lbl("Number of Parts Supplied:", 20, 356);
            nudParts = new NumericUpDown
            {
                Location = new Point(20, 376),
                Size = new Size(120, 28),
                Minimum = 0,
                Maximum = 999,
                Value = 0,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(245, 245, 245),
                TextAlign = HorizontalAlignment.Right
            };

            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Add Supplier",
                Location = new Point(20, 430),
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
                Location = new Point(180, 430),
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
                lblCompany, txtCompany,
                lblContact, txtContact,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblAddress, txtAddress,
                lblParts, nudParts,
                btnSave, btnCancel
            });
        }

        private void LoadUsers()
        {
            try
            {
                // Get users with Role = 'Supplier' who are NOT yet linked to any supplier
                string q = @"SELECT u.UserID, u.FullName 
                   FROM Users u 
                   WHERE u.Role = 'Supplier' 
                   AND u.IsActive = 1 
                   AND (u.SupplierID IS NULL OR u.SupplierID = 0)
                   ORDER BY u.FullName";

                DataTable dt = DatabaseHelper.ExecuteQuery(q);

                if (dt.Rows.Count == 0)
                {
                    // Clear the DataSource first
                    cboUser.DataSource = null;
                    cboUser.Items.Clear();
                    cboUser.DropDownStyle = ComboBoxStyle.DropDown;
                    cboUser.Text = "No available users. Please add a user with Supplier role first.";
                    cboUser.Enabled = false;
                }
                else
                {
                    cboUser.DataSource = dt;
                    cboUser.DisplayMember = "FullName";
                    cboUser.ValueMember = "UserID";
                    cboUser.SelectedIndex = -1;
                    cboUser.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void LoadSupplier()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT * FROM Suppliers WHERE SupplierID = @ID",
                    new[] { new SqlParameter("@ID", _id) });

                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];

                    txtCompany.Text = r["CompanyName"].ToString();
                    txtContact.Text = r["ContactPerson"].ToString();
                    txtPhone.Text = r["Phone"].ToString();
                    txtEmail.Text = r["Email"].ToString();
                    txtAddress.Text = r["Address"].ToString();

                    if (r["PartsSupplied"] != DBNull.Value)
                    {
                        nudParts.Value = Convert.ToDecimal(r["PartsSupplied"]);
                    }

                    // Find the user linked to this supplier
                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT UserID FROM Users WHERE SupplierID = @SupplierID",
                        new[] { new SqlParameter("@SupplierID", _id) });

                    if (userDt.Rows.Count > 0)
                    {
                        int userId = Convert.ToInt32(userDt.Rows[0]["UserID"]);

                        // Temporarily disable the SelectedIndexChanged event if any
                        cboUser.SelectedValue = userId;
                    }
                    else
                    {
                        cboUser.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading supplier: " + ex.Message);
            }
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

            if (string.IsNullOrWhiteSpace(txtCompany.Text))
            {
                MessageBox.Show("Company name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompany.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContact.Text))
            {
                MessageBox.Show("Contact person is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContact.Focus();
                return;
            }

            try
            {
                int userId = Convert.ToInt32(cboUser.SelectedValue);

                if (_edit)
                {
                    // Update supplier
                    DatabaseHelper.ExecuteNonQuery(
                        @"UPDATE Suppliers 
                          SET CompanyName = @Company,
                              ContactPerson = @Contact, 
                              Phone = @Phone, 
                              Email = @Email, 
                              Address = @Address, 
                              PartsSupplied = @Parts,
                              UpdatedAt = GETDATE() 
                          WHERE SupplierID = @ID",
                        new SqlParameter[]
                        {
                            new SqlParameter("@Company", txtCompany.Text.Trim()),
                            new SqlParameter("@Contact", txtContact.Text.Trim()),
                            new SqlParameter("@Phone", txtPhone.Text.Trim()),
                            new SqlParameter("@Email", txtEmail.Text.Trim()),
                            new SqlParameter("@Address", txtAddress.Text.Trim()),
                            new SqlParameter("@Parts", nudParts.Value),  // Changed to decimal
                            new SqlParameter("@ID", _id)
                        });

                    MessageBox.Show("Supplier updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Check if user is already linked to a supplier
                    DataTable checkDt = DatabaseHelper.ExecuteQuery(
                        "SELECT SupplierID FROM Users WHERE UserID = @UID AND SupplierID IS NOT NULL",
                        new[] { new SqlParameter("@UID", userId) });

                    if (checkDt.Rows.Count > 0)
                    {
                        MessageBox.Show("This user is already linked to a supplier.", "Duplicate",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Insert new supplier
                    string insertQuery = @"
                        INSERT INTO Suppliers (CompanyName, ContactPerson, Phone, Email, Address, PartsSupplied, CreatedAt, UpdatedAt) 
                        VALUES (@Company, @Contact, @Phone, @Email, @Address, @Parts, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    object supplierIdObj = DatabaseHelper.ExecuteScalar(insertQuery, new SqlParameter[]
                    {
                        new SqlParameter("@Company", txtCompany.Text.Trim()),
                        new SqlParameter("@Contact", txtContact.Text.Trim()),
                        new SqlParameter("@Phone", txtPhone.Text.Trim()),
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@Address", txtAddress.Text.Trim()),
                        new SqlParameter("@Parts", nudParts.Value)  // Changed to decimal
                    });

                    int newSupplierId = Convert.ToInt32(supplierIdObj);

                    // Update the user with the new SupplierID
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Users SET SupplierID = @SupplierID WHERE UserID = @UserID",
                        new SqlParameter[]
                        {
                            new SqlParameter("@SupplierID", newSupplierId),
                            new SqlParameter("@UserID", userId)
                        });

                    MessageBox.Show("Supplier added successfully!", "Success",
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
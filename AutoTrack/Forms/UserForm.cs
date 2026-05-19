using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
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
            // FIXED: Show "New Password (optional)" when editing, "Password" when adding
            lblPass = Lbl(_edit ? "New Password (optional)" : "Password", 210, 116);
            txtPass = Txt(210, 136, 170);
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
            cboRole.Items.AddRange(new object[] { "Admin", "Staff", "Technician", "Supplier" });
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
                    txtName.Text = r["FullName"].ToString();
                    txtUser.Text = r["Username"].ToString();
                    // FIXED: Don't load password - clear it for security
                    txtPass.Text = string.Empty;
                    cboRole.Text = r["Role"].ToString();
                    chkActive.Checked = Convert.ToBoolean(r["IsActive"]);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            // FIXED: Validation logic
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Full name and username are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // For new users, password is required
            if (!_edit && string.IsNullOrWhiteSpace(txtPass.Text))
            {
                MessageBox.Show("Password is required for new users.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_edit)
                {
                    // Check if user entered a new password
                    bool hasNewPassword = !string.IsNullOrWhiteSpace(txtPass.Text);

                    if (hasNewPassword)
                    {
                        // Update including password
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Users SET FullName=@Name, Username=@User, Password=@Pass, Role=@Role, IsActive=@Active, UpdatedAt=GETDATE() WHERE UserID=@ID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@Name", txtName.Text.Trim()),
                                new SqlParameter("@User", txtUser.Text.Trim()),
                                new SqlParameter("@Pass", PasswordHelper.HashPassword(txtPass.Text)),
                                new SqlParameter("@Role", cboRole.Text),
                                new SqlParameter("@Active", chkActive.Checked ? 1 : 0),
                                new SqlParameter("@ID", _id)
                            });
                    }
                    else
                    {
                        // Update without changing password
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Users SET FullName=@Name, Username=@User, Role=@Role, IsActive=@Active, UpdatedAt=GETDATE() WHERE UserID=@ID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@Name", txtName.Text.Trim()),
                                new SqlParameter("@User", txtUser.Text.Trim()),
                                new SqlParameter("@Role", cboRole.Text),
                                new SqlParameter("@Active", chkActive.Checked ? 1 : 0),
                                new SqlParameter("@ID", _id)
                            });
                    }
                }
                else
                {
                    // Check for duplicate username
                    object exists = DatabaseHelper.ExecuteScalar(
                        "SELECT COUNT(*) FROM Users WHERE Username=@User",
                        new[] { new SqlParameter("@User", txtUser.Text.Trim()) });

                    if (Convert.ToInt32(exists) > 0)
                    {
                        MessageBox.Show("Username already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Insert new user with hashed password
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Users(FullName, Username, Password, Role, IsActive) VALUES(@Name, @User, @Pass, @Role, @Active)",
                        new SqlParameter[]
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
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
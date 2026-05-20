using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
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
            Size = new Size(450, 390);
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
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 200,
                MaxDropDownItems = 10,
                IntegralHeight = true
            };
            cboUser.SelectedIndex = -1;

            // Specialization (Dropdown)
            lblSpec = Lbl("Specialization:", 20, 116);
            cboSpecialization = new ComboBox
            {
                Location = new Point(20, 136),
                Size = new Size(390, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 200,
                MaxDropDownItems = 12,
                IntegralHeight = true
            };

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
            cboSpecialization.SelectedIndex = -1;

            // Level
            lblLevel = Lbl("Level:", 20, 176);
            cboLevel = new ComboBox
            {
                Location = new Point(20, 196),
                Size = new Size(170, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 100,
                MaxDropDownItems = 6,
                IntegralHeight = true
            };
            cboLevel.Items.AddRange(new object[] { "Junior", "Senior", "Master" });
            cboLevel.SelectedIndex = -1;

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
                cboSpecialization.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading specializations: " + ex.Message);
            }
        }

        private void LoadUsers()
        {
            try
            {
                string q = _edit
                    ? @"SELECT u.UserID, u.FullName 
                       FROM Users u 
                       WHERE u.Role = 'Technician' 
                       AND u.IsActive = 1 
                       AND (u.UserID NOT IN (SELECT UserID FROM Technicians WHERE UserID IS NOT NULL) 
                            OR u.UserID = (SELECT UserID FROM Technicians WHERE TechnicianID = @ID)) 
                       ORDER BY u.FullName"
                    : @"SELECT u.UserID, u.FullName 
                       FROM Users u 
                       WHERE u.Role = 'Technician' 
                       AND u.IsActive = 1 
                       AND u.UserID NOT IN (SELECT UserID FROM Technicians WHERE UserID IS NOT NULL)
                       ORDER BY u.FullName";

                SqlParameter[] p = _edit ? new[] { new SqlParameter("@ID", _id) } : null;
                DataTable dt = DatabaseHelper.ExecuteQuery(q, p);

                if (dt.Rows.Count == 0)
                {
                    // No available users - show message
                    cboUser.DataSource = null;
                    cboUser.Items.Clear();
                    cboUser.DropDownStyle = ComboBoxStyle.DropDown;
                    cboUser.Text = "No available users. Please add a user with Technician role first.";
                    cboUser.Enabled = false;
                    btnSave.Enabled = false;

                }
                else
                {
                    cboUser.DataSource = dt;
                    cboUser.DisplayMember = "FullName";
                    cboUser.ValueMember = "UserID";
                    cboUser.SelectedIndex = -1;
                    cboUser.Enabled = true;
                    btnSave.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void LoadTech()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT * FROM Technicians WHERE TechnicianID = @ID",
                    new[] { new SqlParameter("@ID", _id) });

                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];

                    // Find the user linked to this technician
                    int userId = Convert.ToInt32(r["UserID"]);
                    cboUser.SelectedValue = userId;

                    cboSpecialization.Text = r["Specialization"].ToString();
                    cboLevel.Text = r["Level"].ToString();
                    chkAvail.Checked = Convert.ToBoolean(r["IsAvailable"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading technician: " + ex.Message);
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
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Technicians SET UserID=@UID, Specialization=@Spec, Level=@Level, IsAvailable=@Avail WHERE TechnicianID=@ID",
                        new SqlParameter[] { p[0], p[1], p[2], p[3], new SqlParameter("@ID", _id) });

                    MessageBox.Show("Technician updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Technicians(UserID, Specialization, Level, IsAvailable) VALUES(@UID,@Spec,@Level,@Avail)",
                        p);

                    MessageBox.Show("Technician added successfully!", "Success",
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
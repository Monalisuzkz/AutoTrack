using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using AutoTrack.Database;
using AutoTrack.Helpers;
using AutoTrack.Models;

namespace AutoTrack.Forms
{
    public partial class LoginForm : BaseForm
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your username and password.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Use a single query - don't reuse parameters
                string query = @"
                    SELECT UserID, FullName, Username, Role, IsActive, Password
                    FROM Users 
                    WHERE Username = @Username
                    AND IsActive = 1";

                SqlParameter[] parameters = {
                    new SqlParameter("@Username", username)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string storedPassword = row["Password"]?.ToString() ?? string.Empty;
                    if (!PasswordHelper.VerifyPassword(password, storedPassword))
                    {
                        MessageBox.Show("Invalid username or password. Please try again.",
                            "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPassword.Clear();
                        txtPassword.Focus();
                        return;
                    }

                    if (PasswordHelper.NeedsRehash(storedPassword))
                    {
                        string upgradedHash = PasswordHelper.HashPassword(password);
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Users SET Password=@Password, UpdatedAt=GETDATE() WHERE UserID=@UserID",
                            new[]
                            {
                                new SqlParameter("@Password", upgradedHash),
                                new SqlParameter("@UserID", Convert.ToInt32(row["UserID"]))
                            });
                        Trace.TraceInformation("Password hash upgraded for user ID " + Convert.ToInt32(row["UserID"]));
                    }

                    SessionManager.CurrentUser = new User
                    {
                        UserID = Convert.ToInt32(row["UserID"]),
                        FullName = row["FullName"].ToString(),
                        Username = row["Username"].ToString(),
                        Role = row["Role"].ToString(),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };

                    this.Hide();
                    MainForm mainForm = new MainForm();
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                }

                else
                {
                    MessageBox.Show("Invalid username or password. Please try again.",
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(sender, e);
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPassword.Focus();
        }
    }
}

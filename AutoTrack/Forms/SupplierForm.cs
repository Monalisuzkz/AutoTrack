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

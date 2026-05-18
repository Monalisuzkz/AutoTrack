using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;

namespace AutoTrack.Forms
{
    public class CustomerPanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "CustomerID" };

        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        public CustomerPanel()
        {
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);

            txtSearch = MakeSearchBox("Search by name, phone, or email...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new CustomerForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();
            dgv.DoubleClick += EditClick;

            var toolbar = BuildToolbar("Customers", txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount);

            this.Controls.Add(dgv);        // Fill — goes first
            this.Controls.Add(toolbar);    // Top — docked on top
        }

        private void LoadData(string search = "")
        {
            try
            {
                string query = @"
                    SELECT CustomerID,
                        FirstName + ' ' + LastName AS [Full Name],
                        Phone, Email, Address,
                        CONVERT(VARCHAR, CreatedAt, 107) AS [Registered]
                    FROM Customers";

                SqlParameter[] p = null;
                if (!string.IsNullOrEmpty(search) && search != txtSearch.Tag?.ToString())
                {
                    query += @" WHERE FirstName+' '+LastName LIKE @S OR Phone LIKE @S OR Email LIKE @S";
                    p = new[] { new SqlParameter("@S", "%" + search + "%") };
                }
                query += " ORDER BY CreatedAt DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, p);
                BindGrid(dt);
                lblCount.Text = $"{dt.Rows.Count} record(s) found";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a customer to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["CustomerID"].Value);
            var f = new CustomerForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a customer to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string name = dgv.SelectedRows[0].Cells["Full Name"].Value?.ToString();
            if (MessageBox.Show($"Delete '{name}'?\nThis will also delete their vehicles and service records.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["CustomerID"].Value);
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@ID", new[] { new SqlParameter("@ID", id) });
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }

    // ── Customer Add/Edit Form (unchanged) ───────────────────
    public class CustomerForm : Form
    {
        private int _id = 0; private bool _edit = false;
        private Label lblTitle;
        private Label lblFirst, lblLast, lblPhone, lblEmail, lblAddress;
        private TextBox txtFirst, txtLast, txtPhone, txtEmail, txtAddress;
        private Button btnSave, btnCancel;

        public CustomerForm(int id = 0) { _id = id; _edit = id > 0; Init(); if (_edit) Load(); }

        private void Init()
        {
            Text = _edit ? "Edit Customer" : "Register Customer";
            Size = new Size(460, 340); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = _edit ? "Edit Customer" : "Register New Customer",
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
            TextBox Txt(int x, int y, int w = 190) => new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            lblFirst = Lbl("First Name", 20, 56); txtFirst = Txt(20, 76);
            lblLast = Lbl("Last Name", 230, 56); txtLast = Txt(230, 76);
            lblPhone = Lbl("Phone", 20, 116); txtPhone = Txt(20, 136);
            lblEmail = Lbl("Email", 230, 116); txtEmail = Txt(230, 136);
            lblAddress = Lbl("Address", 20, 176); txtAddress = Txt(20, 196, 400);

            btnSave = new Button
            {
                Text = _edit ? "Save Changes" : "Register",
                Location = new Point(20, 240),
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
                Location = new Point(160, 240),
                Size = new Size(100, 36),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { lblTitle,
                lblFirst, txtFirst, lblLast, txtLast,
                lblPhone, txtPhone, lblEmail, txtEmail,
                lblAddress, txtAddress, btnSave, btnCancel });
        }

        private void Load()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers WHERE CustomerID=@ID", new[] { new SqlParameter("@ID", _id) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtFirst.Text = r["FirstName"].ToString(); txtLast.Text = r["LastName"].ToString();
                    txtPhone.Text = r["Phone"].ToString(); txtEmail.Text = r["Email"].ToString();
                    txtAddress.Text = r["Address"].ToString();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
            {
                MessageBox.Show("First and last name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            try
            {
                SqlParameter[] p = {
                    new SqlParameter("@F", txtFirst.Text.Trim()), new SqlParameter("@L", txtLast.Text.Trim()),
                    new SqlParameter("@P", txtPhone.Text.Trim()), new SqlParameter("@E", txtEmail.Text.Trim()),
                    new SqlParameter("@A", txtAddress.Text.Trim()) };
                if (_edit)
                    DatabaseHelper.ExecuteNonQuery("UPDATE Customers SET FirstName=@F,LastName=@L,Phone=@P,Email=@E,Address=@A,UpdatedAt=GETDATE() WHERE CustomerID=@ID",
                        new SqlParameter[] { p[0], p[1], p[2], p[3], p[4], new SqlParameter("@ID", _id) });
                else
                    DatabaseHelper.ExecuteNonQuery("INSERT INTO Customers(FirstName,LastName,Phone,Email,Address) VALUES(@F,@L,@P,@E,@A)", p);
                MessageBox.Show(_edit ? "Customer updated!" : "Customer registered!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
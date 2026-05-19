using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoTrack.Database;

namespace AutoTrack.Forms
{
    public class VehiclePanel : BaseGridPanel
    {
        protected override string[] HiddenColumns => new[] { "VehicleID" };

        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;

        public VehiclePanel() { InitializeControls(); LoadData(); }

        private void InitializeControls()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Color.FromArgb(245, 245, 245);
            txtSearch = MakeSearchBox("Search by plate, make, model, or owner...");
            btnSearch = MakeButton("Search", Color.FromArgb(60, 60, 60), 80, 32);
            btnAdd = MakeButton("+ Add", Color.FromArgb(224, 123, 36), 90, 34);
            btnEdit = MakeButton("Edit", Color.FromArgb(29, 78, 216), 80, 34);
            btnDelete = MakeButton("Delete", Color.FromArgb(180, 50, 50), 80, 34);
            btnRefresh = MakeButton("Refresh", Color.FromArgb(22, 163, 74), 80, 34);
            lblCount = new Label();

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAdd.Click += (s, e) => { var f = new VehicleForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
            btnEdit.Click += EditClick;
            btnDelete.Click += DeleteClick;
            btnRefresh.Click += (s, e) => LoadData();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            dgv = CreateGrid();
            dgv.DoubleClick += EditClick;

            var toolbar = BuildToolbar("Vehicles", txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh, lblCount);
            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
        }

        private void LoadData(string search = "")
        {
            try
            {
                string query = @"
                    SELECT v.VehicleID, v.PlateNumber AS [Plate No.],
                        v.Make+' '+v.Model AS [Make / Model],
                        v.Year, v.Color,
                        c.FirstName+' '+c.LastName AS [Owner],
                        v.EngineType AS [Engine], v.Transmission,
                        CONVERT(VARCHAR,v.CreatedAt,107) AS [Registered]
                    FROM Vehicles v
                    LEFT JOIN Customers c ON v.CustomerID=c.CustomerID";

                SqlParameter[] p = null;
                if (!string.IsNullOrEmpty(search) && !search.StartsWith("Search"))
                {
                    query += @" WHERE v.PlateNumber LIKE @S OR v.Make LIKE @S OR v.Model LIKE @S OR c.FirstName+' '+c.LastName LIKE @S";
                    p = new[] { new SqlParameter("@S", "%" + search + "%") };
                }
                query += " ORDER BY v.CreatedAt DESC";

                BindGrid(DatabaseHelper.ExecuteQuery(query, p));
                lblCount.Text = $"{dgv.RowCount} record(s) found";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void EditClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a vehicle to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["VehicleID"].Value);
            var f = new VehicleForm(id); if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Select a vehicle to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string plate = dgv.SelectedRows[0].Cells["Plate No."].Value?.ToString();
            if (MessageBox.Show($"Delete vehicle '{plate}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["VehicleID"].Value);
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Vehicles WHERE VehicleID=@ID", new[] { new SqlParameter("@ID", id) });
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }

    public class VehicleForm : Form
    {
        private int _id = 0;
        private bool _edit = false;
        private Label lblTitle, lblOwner, lblPlate, lblMake, lblModel, lblYear, lblColor, lblEngine, lblTrans;
        private ComboBox cboOwner, cboTrans, cboMake, cboYear;
        private TextBox txtPlate, txtModel, txtColor, txtEngine;
        private Button btnSave, btnCancel;

        public VehicleForm(int id = 0)
        {
            _id = id;
            _edit = id > 0;
            Init();
            LoadOwners();
            LoadYears();
            LoadMakes();
            if (_edit) Load();
        }

        private void Init()
        {
            Text = _edit ? "Edit Vehicle" : "Register Vehicle";
            Size = new Size(510, 540);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            lblTitle = new Label
            {
                Text = _edit ? "Edit Vehicle" : "Register New Vehicle",
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

            TextBox Txt(int x, int y, int w = 200) => new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            int labelX = 25;
            int fieldX = 150;
            int yPos = 60;
            int rowGap = 45;

            // Owner
            lblOwner = Lbl("Owner (Customer):", labelX, yPos);
            cboOwner = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 100,  
                MaxDropDownItems = 10,  
                IntegralHeight = false
            };
            cboOwner.SelectedIndex = -1;
            yPos += rowGap;

            // Plate Number
            lblPlate = Lbl("Plate Number:", labelX, yPos);
            txtPlate = Txt(fieldX, yPos, 200);
            yPos += rowGap;

            // Make (Dropdown)
            lblMake = Lbl("Make (Brand):", labelX, yPos);
            cboMake = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 100,
                MaxDropDownItems = 12,
                IntegralHeight = false
            };
            cboMake.SelectedIndex = -1;
            yPos += rowGap;

            // Model
            lblModel = Lbl("Model:", labelX, yPos);
            txtModel = Txt(fieldX, yPos, 200);
            yPos += rowGap;

            // Year (Dropdown)
            lblYear = Lbl("Year:", labelX, yPos);
            cboYear = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Size = new Size(100, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 100,
                MaxDropDownItems = 10,
                IntegralHeight = false
            };
            cboYear.SelectedIndex = -1;
            yPos += rowGap;

            // Color
            lblColor = Lbl("Color:", labelX, yPos);
            txtColor = Txt(fieldX, yPos, 200);
            yPos += rowGap;

            // Engine Type
            lblEngine = Lbl("Engine Type:", labelX, yPos);
            txtEngine = Txt(fieldX, yPos, 200);
            yPos += rowGap;

            // Transmission
            lblTrans = Lbl("Transmission:", labelX, yPos);
            cboTrans = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245),
                DropDownHeight = 100,
                MaxDropDownItems = 3,
                IntegralHeight = true
            };
            cboTrans.Items.AddRange(new object[] { "Automatic", "Manual", "CVT" });
            cboTrans.SelectedIndex = -1;
            yPos += rowGap + 15;

            // Buttons
             btnSave = new Button
        {
            Text = _edit ? "Save Changes" : "Register Vehicle",
            Location = new Point(20, yPos),
            Size = new Size(150, 40),
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
            Location = new Point(180, yPos),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 10f),
            BackColor = Color.FromArgb(220, 220, 220),
            ForeColor = Color.FromArgb(60, 60, 60),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[] { 
            lblTitle, lblOwner, cboOwner,
            lblPlate, txtPlate, 
            lblMake, cboMake,
            lblModel, txtModel, 
            lblYear, cboYear,
            lblColor, txtColor, 
            lblEngine, txtEngine, 
            lblTrans, cboTrans, 
            btnSave, btnCancel 
        });
    }

        // ADD THIS METHOD - Load Makes from database
        private void LoadMakes()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT MakeName FROM MakeBrands WHERE IsActive = 1 ORDER BY SortOrder, MakeName");

                if (dt.Rows.Count > 0)
                {
                    cboMake.DataSource = dt;
                    cboMake.DisplayMember = "MakeName";
                    cboMake.ValueMember = "MakeName";
                }
                else
                {
                    // Fallback to hardcoded list
                    cboMake.Items.AddRange(new object[] {
                    "Toyota", "Honda", "Mitsubishi", "Ford", "Hyundai",
                    "Nissan", "Suzuki", "Kia", "Mazda", "Subaru",
                    "BMW", "Mercedes", "Audi", "Volkswagen", "Chevrolet",
                    "Isuzu", "Lexus", "Volvo", "Jaguar", "Porsche"
                });
                }
                cboMake.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // If table doesn't exist, use hardcoded list
                cboMake.Items.Clear();
                cboMake.Items.AddRange(new object[] {
                "Toyota", "Honda", "Mitsubishi", "Ford", "Hyundai",
                "Nissan", "Suzuki", "Kia", "Mazda", "Subaru",
                "BMW", "Mercedes", "Audi", "Volkswagen", "Chevrolet",
                "Isuzu", "Lexus", "Volvo", "Jaguar", "Porsche"
            });
                cboMake.SelectedIndex = -1;
            }
        }

        private void LoadYears()
        {
            try
            {
                cboYear.Items.Clear();
                int currentYear = DateTime.Now.Year;

                // Add years from NEWEST to OLDEST
                for (int year = currentYear + 1; year >= 1950; year--)
                {
                    cboYear.Items.Add(year.ToString());
                }

                cboYear.SelectedIndex = 0;

                // Critical: Set these properties
                cboYear.DropDownHeight = 150;
                cboYear.MaxDropDownItems = 10;
                cboYear.IntegralHeight = false;  // ← This prevents upward flipping
                cboYear.DropDownWidth = cboYear.Width;

                // Optional: Add tooltip
                ToolTip tt = new ToolTip();
                tt.SetToolTip(cboYear, "Select vehicle year");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading years: " + ex.Message);
            }
        }

        private void LoadOwners()
    {
        try
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT CustomerID, FirstName+' '+LastName AS FullName FROM Customers ORDER BY FirstName");
            cboOwner.DataSource = dt; 
            cboOwner.DisplayMember = "FullName"; 
            cboOwner.ValueMember = "CustomerID";
            cboOwner.SelectedIndex = -1;
        }
        catch (Exception ex) { MessageBox.Show("Error loading owners: " + ex.Message); }
    }

    private void Load()
    {
        try
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM Vehicles WHERE VehicleID=@ID", 
                new[] { new SqlParameter("@ID", _id) });
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                cboOwner.SelectedValue = Convert.ToInt32(r["CustomerID"]);
                txtPlate.Text = r["PlateNumber"].ToString();
                cboMake.Text = r["Make"].ToString();
                txtModel.Text = r["Model"].ToString();
                cboYear.Text = r["Year"].ToString();
                txtColor.Text = r["Color"].ToString();
                txtEngine.Text = r["EngineType"].ToString();
                cboTrans.Text = r["Transmission"].ToString();
            }
        }
        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
    }
        private void Save(object sender, EventArgs e)
        {
            // ========== VALIDATION ==========

            // 1. Check Owner
            if (cboOwner.SelectedValue == null || cboOwner.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an owner.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboOwner.DroppedDown = true;
                return;
            }

            // 2. Check Plate Number
            if (string.IsNullOrWhiteSpace(txtPlate.Text))
            {
                MessageBox.Show("Please enter plate number.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPlate.Focus();
                return;
            }

            if (txtPlate.Text.Length < 3)
            {
                MessageBox.Show("Please enter a valid plate number (minimum 3 characters).",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPlate.Focus();
                return;
            }

            // 3. Check Make
            if (cboMake.SelectedIndex == -1 || string.IsNullOrWhiteSpace(cboMake.Text))
            {
                MessageBox.Show("Please select vehicle make (brand).", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboMake.DroppedDown = true;
                return;
            }

            // 4. Check Model
            if (string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Please enter vehicle model.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtModel.Focus();
                return;
            }

            // 5. Check Year
            if (cboYear.SelectedIndex == -1 || string.IsNullOrWhiteSpace(cboYear.Text))
            {
                MessageBox.Show("Please select vehicle year.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboYear.DroppedDown = true;
                return;
            }

            int year = Convert.ToInt32(cboYear.Text);
            if (year < 1950 || year > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Please select a valid year (1950 - " + (DateTime.Now.Year + 1) + ").",
                    "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboYear.DroppedDown = true;
                return;
            }

            // 6. Check Transmission
            if (cboTrans.SelectedIndex == -1 || string.IsNullOrWhiteSpace(cboTrans.Text))
            {
                MessageBox.Show("Please select transmission type.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboTrans.DroppedDown = true;
                return;
            }

            // ========== SAVE ==========

            try
            {
                SqlParameter[] p = {
                new SqlParameter("@CID",   cboOwner.SelectedValue),
                new SqlParameter("@Plate", txtPlate.Text.Trim().ToUpper()),
                new SqlParameter("@Make",  cboMake.Text.Trim()),
                new SqlParameter("@Model", txtModel.Text.Trim()),
                new SqlParameter("@Year",  year),
                new SqlParameter("@Color", txtColor.Text.Trim()),
                new SqlParameter("@Eng",   txtEngine.Text.Trim()),
                new SqlParameter("@Trans", cboTrans.Text)
            };

                if (_edit)
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Vehicles SET CustomerID=@CID,PlateNumber=@Plate,Make=@Make,Model=@Model,Year=@Year,Color=@Color,EngineType=@Eng,Transmission=@Trans,UpdatedAt=GETDATE() WHERE VehicleID=@ID",
                        new SqlParameter[] { p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], new SqlParameter("@ID", _id) });
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Vehicles(CustomerID,PlateNumber,Make,Model,Year,Color,EngineType,Transmission) VALUES(@CID,@Plate,@Make,@Model,@Year,@Color,@Eng,@Trans)",
                        p);

                MessageBox.Show(_edit ? "Vehicle updated successfully!" : "Vehicle registered successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
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
}

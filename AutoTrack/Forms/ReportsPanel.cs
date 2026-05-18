using AutoTrack.Database;
using AutoTrack.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class ReportsPanel : BaseGridPanel
    {
        private ComboBox cboType, cboPeriod;
        private Button btnGenerate;
        private DataGridView dgv;
        private Panel pnlStats;
        private Label lblV1, lblV2, lblV3, lblV4, lblS1, lblS2, lblS3, lblS4, lblReportTitle;

        public ReportsPanel() { Init(); Generate(); }

        private void Init()
        {
            Dock = DockStyle.Fill; BackColor = Color.FromArgb(245, 245, 245);

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 210, BackColor = Color.FromArgb(245, 245, 245) };

            var lblTitle = new Label
            {
                Text = "Reports & Analytics",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0),
                AutoSize = true
            };

            new Label { Text = "Report:" }.Let(l => { l.Font = new Font("Segoe UI", 9f, FontStyle.Bold); l.ForeColor = Color.FromArgb(60, 60, 60); l.Location = new Point(0, 44); l.AutoSize = true; toolbar.Controls.Add(l); });
            cboType = new ComboBox
            {
                Location = new Point(60, 40),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cboType.Items.AddRange(new object[] { "Service Summary", "Revenue Report", "Customer Report", "Inventory Status", "Technician Performance" });
            cboType.SelectedIndex = 0;

            new Label { Text = "Period:" }.Let(l => { l.Font = new Font("Segoe UI", 9f, FontStyle.Bold); l.ForeColor = Color.FromArgb(60, 60, 60); l.Location = new Point(276, 44); l.AutoSize = true; toolbar.Controls.Add(l); });
            cboPeriod = new ComboBox
            {
                Location = new Point(328, 40),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cboPeriod.Items.AddRange(new object[] { "This Month", "Last Month", "Last 3 Months", "This Year", "All Time" });
            cboPeriod.SelectedIndex = 0;

            btnGenerate = new Button
            {
                Text = "Generate",
                Location = new Point(490, 40),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) => Generate();

            // Stat cards
            pnlStats = new Panel { Location = new Point(0, 80), Size = new Size(880, 90), BackColor = Color.Transparent };
            lblV1 = new Label(); lblS1 = new Label();
            lblV2 = new Label(); lblS2 = new Label();
            lblV3 = new Label(); lblS3 = new Label();
            lblV4 = new Label(); lblS4 = new Label();

            void Card(Label vl, Label nl, string name, int x, Color accent)
            {
                var p = new Panel { Location = new Point(x, 0), Size = new Size(200, 90), BackColor = Color.White };
                p.Paint += (s, e) => e.Graphics.FillRectangle(new System.Drawing.SolidBrush(accent), 0, 0, 5, p.Height);
                vl.Text = "0"; vl.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
                vl.ForeColor = Color.FromArgb(30, 30, 30); vl.Location = new Point(14, 10); vl.AutoSize = true;
                nl.Text = name; nl.Font = new Font("Segoe UI", 9f); nl.ForeColor = Color.Gray;
                nl.Location = new Point(14, 56); nl.AutoSize = true;
                p.Controls.Add(vl); p.Controls.Add(nl); pnlStats.Controls.Add(p);
            }
            Card(lblV1, lblS1, "Total Services", 0, Color.FromArgb(224, 123, 36));
            Card(lblV2, lblS2, "Completed", 210, Color.FromArgb(22, 163, 74));
            Card(lblV3, lblS3, "Total Revenue", 420, Color.FromArgb(29, 78, 216));
            Card(lblV4, lblS4, "Total Customers", 630, Color.FromArgb(126, 34, 206));

            lblReportTitle = new Label
            {
                Text = "Results",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 184),
                AutoSize = true
            };

            toolbar.Controls.AddRange(new Control[] { lblTitle, cboType, cboPeriod, btnGenerate, pnlStats, lblReportTitle });

            dgv = CreateGrid();
            Controls.Add(dgv);
            Controls.Add(toolbar);
        }

        private string DateFilter()
        {
            switch (cboPeriod.SelectedItem?.ToString())
            {
                case "This Month": return "AND MONTH(sr.CreatedAt)=MONTH(GETDATE()) AND YEAR(sr.CreatedAt)=YEAR(GETDATE())";
                case "Last Month": return "AND MONTH(sr.CreatedAt)=MONTH(DATEADD(MONTH,-1,GETDATE())) AND YEAR(sr.CreatedAt)=YEAR(DATEADD(MONTH,-1,GETDATE()))";
                case "Last 3 Months": return "AND sr.CreatedAt>=DATEADD(MONTH,-3,GETDATE())";
                case "This Year": return "AND YEAR(sr.CreatedAt)=YEAR(GETDATE())";
                default: return "";
            }
        }

        private void Generate()
        {
            try
            {
                string df = DateFilter();
                string rep = cboType.SelectedItem?.ToString() ?? "Service Summary";

                lblV1.Text = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM ServiceRecords sr WHERE 1=1 {df}")?.ToString() ?? "0";
                lblV2.Text = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM ServiceRecords sr WHERE sr.Status='Completed' {df}")?.ToString() ?? "0";
                lblV3.Text = "₱" + string.Format("{0:N0}", Convert.ToDecimal(DatabaseHelper.ExecuteScalar($"SELECT ISNULL(SUM(p.TotalAmount),0) FROM Payments p JOIN ServiceRecords sr ON p.ServiceID=sr.ServiceID WHERE 1=1 {df.Replace("sr.CreatedAt", "p.PaymentDate")}")));
                lblV4.Text = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Customers")?.ToString() ?? "0";
                lblReportTitle.Text = rep;

                DataTable dt;
                switch (rep)
                {
                    case "Revenue Report":
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT sr.JobOrderNo AS [Job#], v.PlateNumber AS [Plate],
                            c.FirstName+' '+c.LastName AS [Customer], sr.ServiceType AS [Service],
                            p.TotalAmount AS [Amount (₱)], p.PaymentMethod AS [Method],
                            CONVERT(VARCHAR,p.PaymentDate,107) AS [Date]
                            FROM Payments p JOIN ServiceRecords sr ON p.ServiceID=sr.ServiceID
                            JOIN Vehicles v ON sr.VehicleID=v.VehicleID
                            JOIN Customers c ON v.CustomerID=c.CustomerID
                            WHERE 1=1 {df.Replace("sr.CreatedAt", "p.PaymentDate")} ORDER BY p.PaymentDate DESC");
                        break;
                    case "Customer Report":
                        dt = DatabaseHelper.ExecuteQuery(@"SELECT c.FirstName+' '+c.LastName AS [Customer], c.Phone, c.Email,
                            COUNT(DISTINCT v.VehicleID) AS [Vehicles], COUNT(DISTINCT sr.ServiceID) AS [Services],
                            CONVERT(VARCHAR,c.CreatedAt,107) AS [Registered]
                            FROM Customers c LEFT JOIN Vehicles v ON c.CustomerID=v.CustomerID
                            LEFT JOIN ServiceRecords sr ON v.VehicleID=sr.VehicleID
                            GROUP BY c.CustomerID,c.FirstName,c.LastName,c.Phone,c.Email,c.CreatedAt
                            ORDER BY [Services] DESC");
                        break;
                    case "Inventory Status":
                        dt = DatabaseHelper.ExecuteQuery(@"SELECT PartName AS [Part], Category, Unit,
                            Quantity AS [Qty], ReorderLevel AS [Reorder At], UnitPrice AS [Unit Price (₱)],
                            CASE WHEN Quantity<=0 THEN 'Out of Stock' WHEN Quantity<=ReorderLevel THEN 'Low Stock' ELSE 'In Stock' END AS [Status]
                            FROM Inventory ORDER BY [Status], PartName");
                        break;
                    case "Technician Performance":
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT u.FullName AS [Technician],
                            COUNT(sr.ServiceID) AS [Total Jobs],
                            SUM(CASE WHEN sr.Status='Completed' THEN 1 ELSE 0 END) AS [Completed],
                            SUM(CASE WHEN sr.Status='InProgress' THEN 1 ELSE 0 END) AS [In Progress],
                            SUM(CASE WHEN sr.Status='Pending' THEN 1 ELSE 0 END) AS [Pending]
                            FROM Technicians t JOIN Users u ON t.UserID=u.UserID
                            LEFT JOIN ServiceRecords sr ON t.TechnicianID=sr.TechnicianID
                            WHERE 1=1 {df} GROUP BY u.FullName ORDER BY [Total Jobs] DESC");
                        break;
                    default:
                        dt = DatabaseHelper.ExecuteQuery($@"SELECT sr.JobOrderNo AS [Job#],
                            v.PlateNumber AS [Plate], v.Make+' '+v.Model AS [Vehicle],
                            c.FirstName+' '+c.LastName AS [Customer],
                            sr.ServiceType AS [Service Type], u.FullName AS [Technician],
                            sr.Status, CONVERT(VARCHAR,sr.DateIn,107) AS [Date In],
                            CONVERT(VARCHAR,sr.DateCompleted,107) AS [Completed]
                            FROM ServiceRecords sr
                            LEFT JOIN Vehicles v ON sr.VehicleID=v.VehicleID
                            LEFT JOIN Customers c ON v.CustomerID=c.CustomerID
                            LEFT JOIN Technicians t ON sr.TechnicianID=t.TechnicianID
                            LEFT JOIN Users u ON t.UserID=u.UserID
                            WHERE 1=1 {df} ORDER BY sr.CreatedAt DESC");
                        break;
                }
                dgv.DataSource = null;
                dgv.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}

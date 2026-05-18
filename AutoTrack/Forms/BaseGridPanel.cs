using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class BaseGridPanel : UserControl
    {
        // Store the DataGridView as a protected field
        protected DataGridView dgv;

        protected virtual string[] HiddenColumns => new string[0];

        // Add constructor to set background
        public BaseGridPanel()
        {
            this.BackColor = Color.FromArgb(245, 245, 245);  // Match form background
        }

        // ═══════════════════════════════════════════════════════
        // GRID CREATION
        // ═══════════════════════════════════════════════════════
        protected virtual DataGridView CreateGrid()
        {
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(245, 245, 245),  // CHANGE: from White to match form
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(210, 210, 210),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible = false,
                MultiSelect = false,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersVisible = true,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false
            };

            // Header styling
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dgv.ColumnHeadersHeight = 38;

            // Row styling
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);  // CHANGE: from White
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 123, 36);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);  // CHANGE: slightly darker for alternating
            dgv.RowTemplate.Height = 32;

            dgv.DataBindingComplete += OnBindingComplete;
            return dgv;
        }

        // ═══════════════════════════════════════════════════════
        // HIDE COLUMNS METHOD
        // ═══════════════════════════════════════════════════════
        protected void HideColumns()
        {
            if (dgv == null || dgv.Columns == null) return;

            foreach (string colName in HiddenColumns)
            {
                if (dgv.Columns.Contains(colName))
                {
                    dgv.Columns[colName].Visible = false;
                }
            }
        }

        private void OnBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgv == null) return;

            dgv.ColumnHeadersVisible = true;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.Resizable = DataGridViewTriState.False;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // Hide columns after binding
            HideColumns();
        }

        // ═══════════════════════════════════════════════════════
        // BIND GRID METHOD
        // ═══════════════════════════════════════════════════════
        protected void BindGrid(DataTable dt)
        {
            if (dgv == null) return;

            dgv.DataSource = null;
            dgv.DataSource = dt;
            dgv.ColumnHeadersVisible = true;
            HideColumns();
        }

        // ═══════════════════════════════════════════════════════
        // Override in child classes to set column widths
        // ═══════════════════════════════════════════════════════
        protected virtual void SetColumnWidths()
        {
            // Override in child classes
        }

        // ═══════════════════════════════════════════════════════
        // SEARCH BOX
        // ═══════════════════════════════════════════════════════
        protected TextBox MakeSearchBox(string placeholder)
        {
            var txt = new TextBox
            {
                Size = new Size(220, 32),
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245),  // CHANGE: from White
                ForeColor = Color.FromArgb(100, 100, 100),
                Text = placeholder
            };

            txt.GotFocus += (s, e) =>
            {
                if (txt.ForeColor == Color.FromArgb(100, 100, 100))
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(40, 40, 40);
                }
                txt.BackColor = Color.White;  // White when focused
            };

            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.FromArgb(100, 100, 100);
                    txt.BackColor = Color.FromArgb(245, 245, 245);
                }
            };

            return txt;
        }

        // ═══════════════════════════════════════════════════════
        // BUTTON HELPER
        // ═══════════════════════════════════════════════════════
        protected Button MakeButton(string text, Color backColor, int width, int height)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ═══════════════════════════════════════════════════════
        // TOOLBAR BUILDER - FIXED BACKGROUND
        // ═══════════════════════════════════════════════════════
        protected Panel BuildToolbar(string title, params Control[] controls)
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Color.FromArgb(245, 245, 245)  // CHANGE: from White to match form
            };

            toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))  // Lighter border
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            toolbar.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(16, 10),
                AutoSize = true
            });

            int x = 16;
            int btnY = 55;
            int groupGap = 10;
            Label countLbl = null;
            bool passedSearch = false;

            foreach (var ctrl in controls)
            {
                if (ctrl == null) continue;

                if (ctrl is Label lbl)
                {
                    lbl.Font = new Font("Segoe UI", 9f);
                    lbl.ForeColor = Color.FromArgb(100, 100, 100);
                    lbl.AutoSize = true;
                    countLbl = lbl;
                    continue;
                }

                if (ctrl is Button btn)
                {
                    string t = btn.Text.Trim();

                    if (!passedSearch && t.StartsWith("+"))
                    {
                        x += groupGap;
                        passedSearch = true;

                        var sep = new Panel
                        {
                            Location = new Point(x, btnY + 2),
                            Size = new Size(1, 28),
                            BackColor = Color.FromArgb(200, 200, 200)  // Lighter separator
                        };
                        toolbar.Controls.Add(sep);
                        x += sep.Width + groupGap;
                    }

                    if (t == "Refresh" || t.StartsWith("↻"))
                    {
                        x += groupGap;
                        var sep2 = new Panel
                        {
                            Location = new Point(x, btnY + 2),
                            Size = new Size(1, 28),
                            BackColor = Color.FromArgb(200, 200, 200)
                        };
                        toolbar.Controls.Add(sep2);
                        x += sep2.Width + groupGap;
                    }
                }

                ctrl.Location = new Point(x, btnY + (32 - Math.Min(ctrl.Height, 32)) / 2);
                toolbar.Controls.Add(ctrl);
                x += ctrl.Width + 5;
            }

            if (countLbl != null)
            {
                countLbl.Location = new Point(x + 8, btnY + 7);
                toolbar.Controls.Add(countLbl);
            }

            return toolbar;
        }

        // ═══════════════════════════════════════════════════════
        // FORM HELPERS (unchanged)
        // ═══════════════════════════════════════════════════════
        public static void ApplyFormStyle(Form frm, string title, int width = 480, int height = 440)
        {
            frm.Text = title;
            frm.Size = new Size(width, height);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.FormBorderStyle = FormBorderStyle.FixedDialog;
            frm.MaximizeBox = false;
            frm.MinimizeBox = false;
            frm.BackColor = Color.White;
            frm.Font = new Font("Segoe UI", 9.5f);
        }

        public static Label MakeFormTitle(string text)
            => new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(24, 20),
                AutoSize = true
            };

        public static Panel MakeTitleDivider(int formWidth)
            => new Panel
            {
                Location = new Point(0, 52),
                Size = new Size(formWidth, 1),
                BackColor = Color.FromArgb(230, 230, 230)
            };

        public static Label MakeFieldLabel(string text, int x, int y)
            => new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(x, y),
                AutoSize = true
            };

        public static TextBox MakeFieldBox(int x, int y, int width = 410, int height = 30)
            => new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

        public static ComboBox MakeFieldCombo(int x, int y, int width = 200)
            => new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(245, 245, 245)
            };

        public static Button MakeSaveButton(string text, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(140, 38),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 123, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        public static Button MakeCancelButton(string text, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 38),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(60, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
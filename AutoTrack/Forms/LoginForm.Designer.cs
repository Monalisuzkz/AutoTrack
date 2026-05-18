using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ── Controls ────────────────────────────────────────
            this.panelLeft        = new Panel();
            this.lblAppName       = new Label();
            this.lblTagline       = new Label();
            this.lblFeature1      = new Label();
            this.lblFeature2      = new Label();
            this.lblFeature3      = new Label();
            this.panelRight       = new Panel();
            this.lblWelcome       = new Label();
            this.lblSubtitle      = new Label();
            this.lblUsername      = new Label();
            this.txtUsername      = new TextBox();
            this.lblPassword      = new Label();
            this.txtPassword      = new TextBox();
            this.btnLogin         = new Button();
            this.lblFooter        = new Label();

            this.SuspendLayout();

            // ── Form ────────────────────────────────────────────
            this.Text            = "AutoTrack — Login";
            this.Size            = new Size(820, 500);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9f);

            // ── Left Panel (dark) ────────────────────────────────
            this.panelLeft.Size      = new Size(360, 500);
            this.panelLeft.Location  = new Point(0, 0);
            this.panelLeft.BackColor = Color.FromArgb(26, 26, 26);

            // App name
            this.lblAppName.Text      = "⚙  AutoTrack";
            this.lblAppName.ForeColor = Color.White;
            this.lblAppName.Font      = new Font("Segoe UI", 22f, FontStyle.Bold);
            this.lblAppName.Location  = new Point(36, 80);
            this.lblAppName.AutoSize  = true;

            // Tagline
            this.lblTagline.Text      = "Vehicle Service & Maintenance\nMonitoring System";
            this.lblTagline.ForeColor = Color.FromArgb(150, 150, 150);
            this.lblTagline.Font      = new Font("Segoe UI", 10f);
            this.lblTagline.Location  = new Point(36, 140);
            this.lblTagline.AutoSize  = true;

            // Features
            this.lblFeature1.Text      = "✔   Real-time service tracking";
            this.lblFeature1.ForeColor = Color.FromArgb(180, 180, 180);
            this.lblFeature1.Font      = new Font("Segoe UI", 10f);
            this.lblFeature1.Location  = new Point(36, 230);
            this.lblFeature1.AutoSize  = true;

            this.lblFeature2.Text      = "✔   Centralized vehicle records";
            this.lblFeature2.ForeColor = Color.FromArgb(180, 180, 180);
            this.lblFeature2.Font      = new Font("Segoe UI", 10f);
            this.lblFeature2.Location  = new Point(36, 265);
            this.lblFeature2.AutoSize  = true;

            this.lblFeature3.Text      = "✔   Reports & analytics";
            this.lblFeature3.ForeColor = Color.FromArgb(180, 180, 180);
            this.lblFeature3.Font      = new Font("Segoe UI", 10f);
            this.lblFeature3.Location  = new Point(36, 300);
            this.lblFeature3.AutoSize  = true;

            this.panelLeft.Controls.AddRange(new Control[] {
                lblAppName, lblTagline, lblFeature1, lblFeature2, lblFeature3
            });

            // ── Right Panel (white) ──────────────────────────────
            this.panelRight.Size      = new Size(460, 500);
            this.panelRight.Location  = new Point(360, 0);
            this.panelRight.BackColor = Color.White;

            // Welcome
            this.lblWelcome.Text      = "Welcome back";
            this.lblWelcome.Font      = new Font("Segoe UI", 18f, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.FromArgb(30, 30, 30);
            this.lblWelcome.Location  = new Point(50, 80);
            this.lblWelcome.AutoSize  = true;

            this.lblSubtitle.Text      = "Sign in to your AutoTrack account";
            this.lblSubtitle.Font      = new Font("Segoe UI", 9f);
            this.lblSubtitle.ForeColor = Color.Gray;
            this.lblSubtitle.Location  = new Point(50, 118);
            this.lblSubtitle.AutoSize  = true;

            // Username label
            this.lblUsername.Text      = "Username";
            this.lblUsername.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.lblUsername.ForeColor = Color.FromArgb(60, 60, 60);
            this.lblUsername.Location  = new Point(50, 168);
            this.lblUsername.AutoSize  = true;

            // Username textbox
            this.txtUsername.Location    = new Point(50, 188);
            this.txtUsername.Size        = new Size(340, 32);
            this.txtUsername.Font        = new Font("Segoe UI", 11f);
            this.txtUsername.BorderStyle = BorderStyle.FixedSingle;
            this.txtUsername.BackColor   = Color.FromArgb(245, 245, 245);
            this.txtUsername.KeyDown    += new KeyEventHandler(this.txtUsername_KeyDown);

            // Password label
            this.lblPassword.Text      = "Password";
            this.lblPassword.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.lblPassword.ForeColor = Color.FromArgb(60, 60, 60);
            this.lblPassword.Location  = new Point(50, 238);
            this.lblPassword.AutoSize  = true;

            // Password textbox
            this.txtPassword.Location     = new Point(50, 258);
            this.txtPassword.Size         = new Size(340, 32);
            this.txtPassword.Font         = new Font("Segoe UI", 11f);
            this.txtPassword.BorderStyle  = BorderStyle.FixedSingle;
            this.txtPassword.BackColor    = Color.FromArgb(245, 245, 245);
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.KeyDown     += new KeyEventHandler(this.txtPassword_KeyDown);

            // Login button
            this.btnLogin.Text      = "Sign In";
            this.btnLogin.Location  = new Point(50, 320);
            this.btnLogin.Size      = new Size(340, 44);
            this.btnLogin.Font      = new Font("Segoe UI", 11f, FontStyle.Bold);
            this.btnLogin.BackColor = Color.FromArgb(224, 123, 36);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Cursor    = Cursors.Hand;
            this.btnLogin.Click    += new System.EventHandler(this.btnLogin_Click);

            // Footer
            this.lblFooter.Text      = "AutoTrack  •  IT13/L IT Professional Track 4";
            this.lblFooter.Font      = new Font("Segoe UI", 8f);
            this.lblFooter.ForeColor = Color.LightGray;
            this.lblFooter.Location  = new Point(50, 440);
            this.lblFooter.AutoSize  = true;

            this.panelRight.Controls.AddRange(new Control[] {
                lblWelcome, lblSubtitle,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnLogin, lblFooter
            });

            // ── Add panels to form ───────────────────────────────
            this.Controls.AddRange(new Control[] { panelLeft, panelRight });

            this.ResumeLayout(false);
        }

        // ── Fields ──────────────────────────────────────────────
        private Panel   panelLeft;
        private Panel   panelRight;
        private Label   lblAppName;
        private Label   lblTagline;
        private Label   lblFeature1;
        private Label   lblFeature2;
        private Label   lblFeature3;
        private Label   lblWelcome;
        private Label   lblSubtitle;
        private Label   lblUsername;
        private TextBox txtUsername;
        private Label   lblPassword;
        private TextBox txtPassword;
        private Button  btnLogin;
        private Label   lblFooter;
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public class UpdateForm : Form
    {
        private Panel topBar;
        private Label lblTitle;
        private Panel contentPanel;
        private Label lblVersion;
        private Label lblChangelog;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Button btnUpdate;

        private UpdateInfo _updateInfo;
        private Point lastPoint;

        public UpdateForm(UpdateInfo updateInfo)
        {
            _updateInfo = updateInfo;
            InitializeComponent();
            DisplayUpdateInfo();
        }

        private void InitializeComponent()
        {
            this.Text = "Update Available";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;
            this.TopMost = true;

            CreateTopBar();
            CreateContentPanel();

            this.Load += UpdateForm_Load;
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            AcrylicHelper.EnableBlur(this.Handle, Color.FromArgb(120, 15, 15, 20));
        }

        private void CreateTopBar()
        {
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            topBar.Paint += TopBar_Paint;
            topBar.MouseDown += TopBar_MouseDown;
            topBar.MouseMove += TopBar_MouseMove;
            this.Controls.Add(topBar);

            lblTitle = new Label
            {
                Text = "🔄  UPDATE AVAILABLE",
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblTitle);
        }

        private void TopBar_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                topBar.ClientRectangle,
                Color.FromArgb(20, 255, 255, 255),
                Color.FromArgb(10, 255, 255, 255),
                90f))
            {
                e.Graphics.FillRectangle(brush, topBar.ClientRectangle);
            }
        }

        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(540, 320),
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            contentPanel.Paint += ContentPanel_Paint;
            this.Controls.Add(contentPanel);

            lblVersion = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblVersion);

            Label lblChangelogTitle = new Label
            {
                Text = "What's New:",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 240),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblChangelogTitle);

            lblChangelog = new Label
            {
                Location = new Point(20, 90),
                Size = new Size(500, 130),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 220),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblChangelog);

            progressBar = new ProgressBar
            {
                Location = new Point(20, 230),
                Size = new Size(500, 10),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            contentPanel.Controls.Add(progressBar);

            lblProgress = new Label
            {
                Text = "Ready to update",
                Location = new Point(20, 245),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 200, 255),
                BackColor = Color.Transparent,
                Visible = false
            };
            contentPanel.Controls.Add(lblProgress);

            btnUpdate = new Button
            {
                Text = "🚀  UPDATE NOW",
                Location = new Point(320, 260),
                Size = new Size(200, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 102, 204);
            btnUpdate.Click += BtnUpdate_Click;
            contentPanel.Controls.Add(btnUpdate);
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                contentPanel.ClientRectangle,
                Color.FromArgb(25, 255, 255, 255),
                Color.FromArgb(12, 255, 255, 255),
                90f))
            {
                e.Graphics.FillRectangle(brush, contentPanel.ClientRectangle);
            }

            using (Pen pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, contentPanel.Width - 1, contentPanel.Height - 1);
            }
        }

        private void DisplayUpdateInfo()
        {
            lblVersion.Text = $"Version {_updateInfo.Version} is available!";
            lblChangelog.Text = _updateInfo.Changelog;

            if (_updateInfo.ForceUpdate)
            {
                btnUpdate.Text = "⚠️  UPDATE REQUIRED";
                this.FormClosing += (s, e) =>
                {
                    if (this.DialogResult != DialogResult.OK)
                    {
                        e.Cancel = true;
                        MessageBox.Show("This update is required to continue using the application.",
                            "Update Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
            progressBar.Visible = true;
            lblProgress.Visible = true;

            var progress = new Progress<int>(percent =>
            {
                progressBar.Value = percent;
                lblProgress.Text = $"Downloading update... {percent}%";
            });

            lblProgress.Text = "Downloading update...";

            bool success = await AutoUpdater.DownloadAndInstallUpdateAsync(_updateInfo, progress);

            if (success)
            {
                this.DialogResult = DialogResult.OK;
                Application.Exit();
            }
            else
            {
                btnUpdate.Enabled = true;
                progressBar.Visible = false;
                lblProgress.Visible = false;
            }
        }
    }
}

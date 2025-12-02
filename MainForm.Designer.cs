using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    partial class MainForm
    {
        private Panel topBar;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;
        private Button btnMinimize;

        private Panel cardStory;
        private Panel cardOnline;
        private Button btnLaunchStory;
        private Button btnLaunchOnline;

        private Panel panelStats;
        private Label lblLastPlayed;
        private Label lblTotalLaunches;

        private ProgressBar progressBar;
        private Label lblStatus;

        private Button btnSettings;
        private Button btnSync;
        private Button btnBackup;
        private Button btnHistory;

        private Timer animTimer;

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.Text = "RDR2 Mode Switcher";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;

            CreateTopBar();
            CreateModeCards();
            CreateStatsPanel();
            CreateProgressBar();
            CreateActionButtons();

            animTimer = new Timer { Interval = 50 };
            animTimer.Tick += AnimTimer_Tick;
            animTimer.Start();

            this.Load += MainForm_Load;
            this.ResumeLayout(false);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Maximum transparency - ultra glass effect
            AcrylicHelper.EnableBlur(this.Handle, Color.FromArgb(120, 15, 15, 20));
        }

        private void CreateTopBar()
        {
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            topBar.Paint += TopBar_Paint;
            topBar.MouseDown += TopBar_MouseDown;
            topBar.MouseMove += TopBar_MouseMove;
            this.Controls.Add(topBar);

            lblTitle = new Label
            {
                Text = "🎮  RDR2 Mode Switcher",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Seamlessly switch between Story and Online modes",
                Location = new Point(22, 43),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(240, 240, 250),
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblSubtitle);

            btnClose = CreateHeaderButton("✕", 795, Color.FromArgb(232, 17, 35));
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            btnMinimize = CreateHeaderButton("─", 750, Color.FromArgb(80, 80, 80));
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            topBar.Controls.Add(btnMinimize);
        }

        private Button CreateHeaderButton(string text, int x, Color bgColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 15),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bgColor,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor, 0.1f);
            return btn;
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

        private void CreateModeCards()
        {
            // Story Mode Card
            cardStory = CreateCard(30, 100, 390, 160);
            this.Controls.Add(cardStory);

            Label iconStory = new Label
            {
                Text = "🎮",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 36),
                BackColor = Color.Transparent
            };
            cardStory.Controls.Add(iconStory);

            Label titleStory = new Label
            {
                Text = "STORY MODE",
                Location = new Point(20, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            cardStory.Controls.Add(titleStory);

            Label descStory = new Label
            {
                Text = "Play with mods • Vulkan API",
                Location = new Point(22, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 220),
                BackColor = Color.Transparent
            };
            cardStory.Controls.Add(descStory);

            btnLaunchStory = CreateLaunchButton("▶ LAUNCH", 240, 100);
            btnLaunchStory.Click += async (s, e) => await HandleStoryModeAsync();
            cardStory.Controls.Add(btnLaunchStory);

            // Online Mode Card
            cardOnline = CreateCard(430, 100, 390, 160);
            this.Controls.Add(cardOnline);

            Label iconOnline = new Label
            {
                Text = "🌐",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 36),
                BackColor = Color.Transparent
            };
            cardOnline.Controls.Add(iconOnline);

            Label titleOnline = new Label
            {
                Text = "ONLINE MODE",
                Location = new Point(20, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            cardOnline.Controls.Add(titleOnline);

            Label descOnline = new Label
            {
                Text = "No mods • DirectX 12 API",
                Location = new Point(22, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 220),
                BackColor = Color.Transparent
            };
            cardOnline.Controls.Add(descOnline);

            btnLaunchOnline = CreateLaunchButton("▶ LAUNCH", 240, 100);
            btnLaunchOnline.Click += async (s, e) => await HandleOnlineModeAsync();
            cardOnline.Controls.Add(btnLaunchOnline);
        }

        private Panel CreateCard(int x, int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            card.Paint += Card_Paint;
            return card;
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(25, 255, 255, 255),
                Color.FromArgb(12, 255, 255, 255),
                90f))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }

            using (Pen pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            }
        }

        private Button CreateLaunchButton(string text, int x, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(130, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 102, 204);
            return btn;
        }

        private void CreateStatsPanel()
        {
            panelStats = CreateCard(30, 280, 790, 80);
            this.Controls.Add(panelStats);

            Label statsTitle = new Label
            {
                Text = "📊  STATISTICS",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            panelStats.Controls.Add(statsTitle);

            lblLastPlayed = new Label
            {
                Location = new Point(20, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(220, 220, 240),
                BackColor = Color.Transparent
            };
            panelStats.Controls.Add(lblLastPlayed);

            lblTotalLaunches = new Label
            {
                Location = new Point(400, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(220, 220, 240),
                BackColor = Color.Transparent
            };
            panelStats.Controls.Add(lblTotalLaunches);
        }

        private void CreateProgressBar()
        {
            progressBar = new ProgressBar
            {
                Location = new Point(30, 380),
                Size = new Size(790, 8),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            this.Controls.Add(progressBar);

            lblStatus = new Label
            {
                Text = "✨ Ready to launch",
                Location = new Point(30, 395),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 255, 150),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblStatus);
        }

        private void CreateActionButtons()
        {
            int y = 430;
            int spacing = 15;
            int btnWidth = 185;

            btnSettings = CreateActionButton("⚙️  Settings", 30, y, btnWidth);
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            btnSync = CreateActionButton("🔄  Sync Mods", 30 + btnWidth + spacing, y, btnWidth);
            btnSync.Click += BtnSync_Click;
            this.Controls.Add(btnSync);

            btnBackup = CreateActionButton("💾  Backup", 30 + (btnWidth + spacing) * 2, y, btnWidth);
            btnBackup.Click += (s, e) => BackupManager.CreateBackup(SettingsXmlPath);
            this.Controls.Add(btnBackup);

            btnHistory = CreateActionButton("📊  History", 30 + (btnWidth + spacing) * 3, y, btnWidth);
            btnHistory.Click += BtnHistory_Click;
            this.Controls.Add(btnHistory);
        }

        private Button CreateActionButton(string text, int x, int y, int width)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(15, 255, 255, 255),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(40, 255, 255, 255);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            return btn;
        }
    }
}

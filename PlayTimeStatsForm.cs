using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public class PlayTimeStatsForm : Form
    {
        private Panel topBar;
        private Label lblTitle;
        private Button btnClose;

        private Panel contentPanel;
        private Label lblTotalTime;
        private Label lblTotalSessions;
        private Label lblAvgSession;
        private Label lblFirstPlayed;
        private Label lblLastPlayed;

        private Label lblTodayTime;
        private Label lblWeekTime;
        private Label lblMonthTime;

        private ListView listViewSessions;
        private Button btnResetStats;

        private Point lastPoint;
        private PlayTimeStats stats;

        public PlayTimeStatsForm()
        {
            stats = PlayTimeTracker.LoadStats();
            InitializeComponent();
            DisplayStats();
        }

        private void InitializeComponent()
        {
            this.Text = "Play Time Statistics";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;

            CreateTopBar();
            CreateContentPanel();

            this.Load += PlayTimeStatsForm_Load;
        }

        private void PlayTimeStatsForm_Load(object sender, EventArgs e)
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
                Text = "⏱️  PLAY TIME STATISTICS",
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblTitle);

            btnClose = new Button
            {
                Text = "✕",
                Location = new Point(795, 10),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 17, 35),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { this.Close(); };
            topBar.Controls.Add(btnClose);
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
                Size = new Size(790, 520),
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            contentPanel.Paint += ContentPanel_Paint;
            this.Controls.Add(contentPanel);

            // Summary Stats
            CreateStatBox("🎮 Total Play Time", ref lblTotalTime, 20, 20);
            CreateStatBox("📊 Total Sessions", ref lblTotalSessions, 200, 20);
            CreateStatBox("⏰ Avg Session", ref lblAvgSession, 380, 20);
            CreateStatBox("🎯 First Played", ref lblFirstPlayed, 560, 20);

            CreateStatBox("📅 Today", ref lblTodayTime, 20, 120);
            CreateStatBox("📆 This Week", ref lblWeekTime, 200, 120);
            CreateStatBox("📈 This Month", ref lblMonthTime, 380, 120);
            CreateStatBox("🕒 Last Played", ref lblLastPlayed, 560, 120);

            // Session History Title
            Label lblHistoryTitle = new Label
            {
                Text = "Recent Sessions",
                Location = new Point(20, 230),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblHistoryTitle);

            // Sessions ListView
            listViewSessions = new ListView
            {
                Location = new Point(20, 260),
                Size = new Size(750, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            listViewSessions.Columns.Add("Date", 150);
            listViewSessions.Columns.Add("Start Time", 120);
            listViewSessions.Columns.Add("End Time", 120);
            listViewSessions.Columns.Add("Duration", 120);
            listViewSessions.Columns.Add("Mode", 220);

            contentPanel.Controls.Add(listViewSessions);

            // Reset Button
            btnResetStats = new Button
            {
                Text = "🗑️  Reset All Stats",
                Location = new Point(620, 475),
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(15, 255, 255, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnResetStats.FlatAppearance.BorderColor = Color.FromArgb(232, 17, 35);
            btnResetStats.FlatAppearance.BorderSize = 1;
            btnResetStats.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnResetStats.Click += BtnResetStats_Click;
            contentPanel.Controls.Add(btnResetStats);
        }

        private void CreateStatBox(string title, ref Label valueLabel, int x, int y)
        {
            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(x, y),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 200, 255),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblTitle);

            valueLabel = new Label
            {
                Text = "0h 0m",
                Location = new Point(x, y + 25),
                Size = new Size(160, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(valueLabel);
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

        private void DisplayStats()
        {
            // Summary stats
            lblTotalTime.Text = FormatTimeSpan(stats.TotalPlayTime);
            lblTotalSessions.Text = stats.TotalSessions.ToString();
            lblAvgSession.Text = FormatTimeSpan(stats.AverageSessionTime);
            lblFirstPlayed.Text = stats.FirstPlayed == DateTime.MinValue ? "Never" : stats.FirstPlayed.ToString("MMM dd, yyyy");

            lblTodayTime.Text = FormatTimeSpan(stats.TodayPlayTime);
            lblWeekTime.Text = FormatTimeSpan(stats.ThisWeekPlayTime);
            lblMonthTime.Text = FormatTimeSpan(stats.ThisMonthPlayTime);
            lblLastPlayed.Text = stats.LastPlayed == DateTime.MinValue ? "Never" : stats.LastPlayed.ToString("MMM dd, yyyy");

            // Recent sessions (last 20)
            listViewSessions.Items.Clear();
            var recentSessions = stats.Sessions.OrderByDescending(s => s.StartTime).Take(20);

            foreach (var session in recentSessions)
            {
                var item = new ListViewItem(session.StartTime.ToString("MMM dd, yyyy"));
                item.SubItems.Add(session.StartTime.ToString("hh:mm tt"));
                item.SubItems.Add(session.EndTime.ToString("hh:mm tt"));
                item.SubItems.Add(FormatTimeSpan(session.Duration));
                item.SubItems.Add(session.Mode);
                listViewSessions.Items.Add(item);
            }

            if (stats.Sessions.Count == 0)
            {
                var item = new ListViewItem("No sessions recorded yet");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.ForeColor = Color.FromArgb(150, 150, 160);
                listViewSessions.Items.Add(item);
            }
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            int hours = (int)time.TotalHours;
            int minutes = time.Minutes;

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        private void BtnResetStats_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all play time statistics? This cannot be undone.",
                "Reset Statistics",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                PlayTimeTracker.ResetStats();
                stats = PlayTimeTracker.LoadStats();
                DisplayStats();
                MessageBox.Show("Statistics have been reset!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

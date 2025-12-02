using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public class HistoryForm : Form
    {
        private Panel topBar;
        private Label lblTitle;
        private Button btnClose;

        private Panel contentPanel;
        private ListView listView;
        private Button btnCloseBottom;
        private Button btnClearHistory;

        private Point lastPoint;

        public HistoryForm()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "Launch History";
            this.Size = new Size(750, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;

            CreateTopBar();
            CreateContentPanel();

            this.Load += HistoryForm_Load;
        }

        private void HistoryForm_Load(object sender, EventArgs e)
        {
            // Apply ultra-transparent glassmorphism
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
                Text = "📊  LAUNCH HISTORY",
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
                Location = new Point(695, 10),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 17, 35),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
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
                Size = new Size(690, 420),
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            contentPanel.Paint += ContentPanel_Paint;
            this.Controls.Add(contentPanel);

            // ListView
            listView = new ListView
            {
                Location = new Point(20, 20),
                Size = new Size(650, 320),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            listView.Columns.Add("Date & Time", 220);
            listView.Columns.Add("Mode", 200);
            listView.Columns.Add("Status", 200);

            contentPanel.Controls.Add(listView);

            // Clear History button
            btnClearHistory = new Button
            {
                Text = "🗑️  Clear History",
                Location = new Point(340, 360),
                Size = new Size(150, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(15, 255, 255, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClearHistory.FlatAppearance.BorderColor = Color.FromArgb(40, 255, 255, 255);
            btnClearHistory.FlatAppearance.BorderSize = 1;
            btnClearHistory.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            btnClearHistory.Click += BtnClearHistory_Click;
            contentPanel.Controls.Add(btnClearHistory);

            // Close button
            btnCloseBottom = new Button
            {
                Text = "✅  Close",
                Location = new Point(500, 360),
                Size = new Size(170, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnCloseBottom.FlatAppearance.BorderSize = 0;
            btnCloseBottom.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 102, 204);
            contentPanel.Controls.Add(btnCloseBottom);
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

        private void LoadHistory()
        {
            var history = LaunchHistoryManager.LoadHistory();

            listView.Items.Clear();

            foreach (var entry in history)
            {
                var item = new ListViewItem(entry.Timestamp.ToString("MMM dd, yyyy HH:mm:ss"));
                item.SubItems.Add(entry.Mode);
                item.SubItems.Add(entry.Status);
                listView.Items.Add(item);
            }

            if (history.Count == 0)
            {
                var item = new ListViewItem("No history available");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.ForeColor = Color.FromArgb(150, 150, 160);
                listView.Items.Add(item);
            }
        }

        private void BtnClearHistory_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all launch history?",
                "Clear History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                LaunchHistoryManager.ClearHistory();
                LoadHistory();
                MessageBox.Show("History cleared successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
